import { randomBytes } from 'node:crypto';
import { spawn } from 'node:child_process';
import { mkdirSync, openSync, closeSync, writeFileSync, readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { setTimeout as delay } from 'node:timers/promises';
import { evaluateZap } from './gate.mjs';

// Fixed targets: this script cannot be pointed at production or a third-party site.
const directory = resolve('artifacts/security');
mkdirSync(directory, { recursive: true });
const emptyEnv = resolve(directory, 'empty.env');
writeFileSync(emptyEnv, '');
const env = {
  ...process.env,
  DAST_PASSWORD: `Aa1!${randomBytes(24).toString('hex')}`,
  DAST_JWT_KEY: randomBytes(48).toString('hex'),
  DAST_ZAP_KEY: randomBytes(24).toString('hex'),
};
if (process.env.GITHUB_ACTIONS) {
  for (const name of ['DAST_PASSWORD', 'DAST_JWT_KEY', 'DAST_ZAP_KEY']) console.log(`::add-mask::${env[name]}`);
}
const composeArgs = ['compose', '--env-file', emptyEnv, '-p', 'horamais-dast', '-f', 'docker-compose.security.yml'];
const summary = { scanner: 'zap', blocked: true, stage: 'startup', authenticated: false };

async function compose(...args) {
  const log = openSync(resolve(directory, 'dast-private.log'), 'a');
  try {
    await new Promise((done, reject) => {
      const child = spawn('docker', [...composeArgs, ...args], { env, stdio: ['ignore', log, log] });
      child.on('error', reject);
      child.on('exit', code => code === 0 ? done() : reject(new Error('Docker Compose failed')));
    });
  } finally {
    closeSync(log);
  }
}

async function request(url, options = {}) {
  return fetch(url, { ...options, redirect: 'error', signal: AbortSignal.timeout(30_000) });
}

async function poll(check, timeoutMs, description) {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    if (await check()) return;
    await delay(2000);
  }
  throw new Error(`Timeout: ${description}`);
}

async function ready(url) {
  await poll(async () => {
    try { return (await request(url)).ok; } catch { return false; }
  }, 180_000, `application readiness at ${new URL(url).pathname}`);
}

async function zap(component, kind, operation, params = {}) {
  const url = new URL(`http://127.0.0.1:18080/JSON/${component}/${kind}/${operation}/`);
  for (const [key, value] of Object.entries({ ...params, apikey: env.DAST_ZAP_KEY })) url.searchParams.set(key, value);
  const response = await request(url);
  if (!response.ok) throw new Error('ZAP API request failed');
  const result = await response.json();
  if (result.code || result.error) throw new Error('ZAP API returned an error');
  return result;
}

async function authenticatedProbe() {
  const result = await zap('core', 'action', 'sendRequest', {
    request: 'GET http://backend:5000/api/Curso HTTP/1.1\r\nHost: backend:5000\r\n\r\n',
    followRedirects: 'false',
  });
  if (!result.sendRequest?.some(message => /^HTTP\/\d(?:\.\d)? 200\b/.test(message.responseHeader))) {
    throw new Error('Authenticated ZAP request did not return 200');
  }
}

async function alerts() {
  const all = [];
  for (let start = 0; ; start += 500) {
    const page = (await zap('core', 'view', 'alerts', { start, count: 500 })).alerts;
    if (!Array.isArray(page)) throw new Error('Invalid ZAP alerts response');
    all.push(...page);
    if (page.length < 500) return all;
  }
}

try {
  console.log('DAST: building and starting isolated application');
  await compose('up', '-d', '--build', 'db', 'storage', 'mail', 'backend', 'frontend', 'gateway');
  await ready('http://127.0.0.1:15000/swagger/v1/swagger.json');
  await ready('http://127.0.0.1:13000/');
  summary.stage = 'authentication';
  const anonymous = await request('http://127.0.0.1:15000/api/Curso');
  if (anonymous.status !== 401) throw new Error('Protected route must reject anonymous access');
  const login = await request('http://127.0.0.1:15000/api/Auth/login', {
    method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: 'admin@dast.invalid', senha: env.DAST_PASSWORD }),
  });
  if (!login.ok) throw new Error('DAST login failed');
  const { token } = await login.json();
  if (typeof token !== 'string' || !token) throw new Error('Missing DAST access token');
  if (process.env.GITHUB_ACTIONS) console.log(`::add-mask::${token}`);
  env.DAST_AUTH_HEADER = `Bearer ${token}`;
  await compose('--profile', 'scanner', 'up', '-d', 'zap');
  await poll(async () => {
    try { return Boolean((await zap('core', 'view', 'version')).version); } catch { return false; }
  }, 180_000, 'ZAP startup');
  await authenticatedProbe();
  summary.authenticated = true;
  summary.stage = 'scan';
  const { contextId } = await zap('context', 'action', 'newContext', { contextName: 'horamais' });
  if (!contextId) throw new Error('Missing scan context');
  await zap('context', 'action', 'setContextInScope', { contextName: 'horamais', booleanInScope: 'true' });
  await zap('context', 'action', 'includeInContext', {
    contextName: 'horamais', regex: 'http://(backend:5000|frontend:3000)(/.*)?',
  });
  // Avoid revoking the session under test or scanning external OAuth providers.
  for (const regex of ['http://backend:5000/api/Auth/.*', 'http://frontend:3000/api/auth/.*']) {
    await zap('context', 'action', 'excludeFromContext', { contextName: 'horamais', regex });
  }
  await zap('openapi', 'action', 'importUrl', {
    url: 'http://backend:5000/swagger/v1/swagger.json', hostOverride: 'http://backend:5000', contextId,
  });
  const spider = await zap('spider', 'action', 'scan', { url: 'http://frontend:3000', contextName: 'horamais' });
  if (!/^\d+$/.test(spider.scan)) throw new Error('Missing spider scan ID');
  await poll(async () => (await zap('spider', 'view', 'status', { scanId: spider.scan })).status === '100', 180_000, 'spider');
  for (const url of ['http://backend:5000', 'http://frontend:3000']) {
    console.log(`DAST: active scan of ${url}`);
    const scan = await zap('ascan', 'action', 'scan', { url, recurse: 'true', inScopeOnly: 'true', contextId });
    if (!/^\d+$/.test(scan.scan)) throw new Error('Missing active scan ID');
    await poll(async () => (await zap('ascan', 'view', 'status', { scanId: scan.scan })).status === '100', 900_000, 'active scan');
  }
  await poll(async () => (await zap('pscan', 'view', 'recordsToScan')).recordsToScan === '0', 180_000, 'passive scan');
  await authenticatedProbe();
  const { urls } = await zap('core', 'view', 'urls');
  if (!Array.isArray(urls) || !urls.some(url => url.startsWith('http://backend:5000/api/')) ||
      !urls.some(url => url.startsWith('http://frontend:3000/'))) throw new Error('Incomplete DAST coverage');
  Object.assign(summary, evaluateZap(await alerts()), { stage: 'complete', scannedUrls: urls.length });
} catch (error) {
  summary.blocked = true;
  summary.error = error instanceof Error ? error.message : 'DAST failed or incomplete';
  // Retain a sanitized partial report when the scanner is still available.
  try { summary.findings = evaluateZap(await alerts()).findings; } catch { /* No report available. */ }
} finally {
  console.log(`DAST: ${summary.blocked ? 'BLOCKED' : 'PASSED'} (${summary.stage})`);
  if (summary.blocked) {
    try { await compose('logs', '--no-color', '--tail', '60', 'backend', 'frontend'); } catch { /* Best effort diagnostics. */ }
  }
  try { await compose('down', '--volumes', '--remove-orphans'); } catch {
    summary.blocked = true;
    summary.cleanupFailed = true;
  }
  let summaryText = JSON.stringify(summary, null, 2);
  // This stack contains synthetic data only; redact generated credentials in diagnostics.
  let diagnostics = readFileSync(resolve(directory, 'dast-private.log'), 'utf8');
  for (const [key, value] of Object.entries(env)) {
    if (key.startsWith('DAST_') && value) {
      diagnostics = diagnostics.replaceAll(value, '[REDACTED]');
      summaryText = summaryText.replaceAll(value, '[REDACTED]');
    }
  }
  writeFileSync(resolve(directory, 'dast-summary.json'), summaryText);
  diagnostics = diagnostics.replace(/eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+/g, '[REDACTED JWT]');
  writeFileSync(resolve(directory, 'dast-diagnostics.log'), diagnostics.slice(-200_000));
  if (summary.blocked) console.error(diagnostics.slice(-4000));
  process.exitCode = summary.blocked ? 1 : 0;
}

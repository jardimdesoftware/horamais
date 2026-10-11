import { readFileSync, writeFileSync, mkdirSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';

const severities = new Set(['INFO', 'INVENTORY', 'LOW', 'WARNING', 'MEDIUM', 'ERROR', 'HIGH', 'CRITICAL']);

export function evaluateSemgrep(report) {
  if (!Array.isArray(report.results) || !Array.isArray(report.errors) || report.errors.length) {
    throw new Error('SAST report missing or scan errors present');
  }
  const scanned = report.paths?.scanned;
  if (!Array.isArray(scanned) || !scanned.some(p => p.endsWith('.cs')) || !scanned.some(p => /\.tsx?$/.test(p))) {
    throw new Error('SAST must scan both C# and TypeScript');
  }
  const findings = report.results.map(result => {
    const severity = result.extra?.severity?.toUpperCase();
    if (!severities.has(severity)) throw new Error('Unknown SAST severity');
    const impact = result.extra?.metadata?.impact?.toUpperCase();
    return {
      rule: result.check_id,
      path: result.path,
      line: result.start?.line,
      severity,
      blocking: ['ERROR', 'HIGH', 'CRITICAL'].includes(severity) || ['HIGH', 'CRITICAL'].includes(impact),
    };
  });
  return { scanner: 'semgrep', scannedFiles: scanned.length, findings, blocked: findings.some(f => f.blocking) };
}

export function evaluateZap(alerts) {
  if (!Array.isArray(alerts)) throw new Error('Missing ZAP alerts');
  const findings = alerts.map(alert => {
    const risk = String(alert.risk ?? alert.riskcode);
    const mapping = { Informational: 0, Low: 1, Medium: 2, High: 3, '0': 0, '1': 1, '2': 2, '3': 3 };
    if (!(risk in mapping)) throw new Error('Unknown ZAP severity');
    // Do not retain request bodies, evidence, query strings, cookies or tokens.
    return { rule: alert.pluginId ?? alert.pluginid, risk: mapping[risk], blocking: mapping[risk] >= 3 };
  });
  return { scanner: 'zap', findings, blocked: findings.some(f => f.blocking) };
}

if (process.argv[1] && import.meta.url === pathToFileURL(resolve(process.argv[1])).href) {
  const [scanner, input, output] = process.argv.slice(2);
  if (scanner !== 'semgrep' || !input || !output) throw new Error('Usage: gate.mjs semgrep input.json summary.json');
  let summary;
  try {
    summary = evaluateSemgrep(JSON.parse(readFileSync(input, 'utf8')));
  } catch {
    summary = { scanner, blocked: true, error: 'Missing, incomplete or invalid scanner report' };
  }
  mkdirSync(dirname(output), { recursive: true });
  writeFileSync(output, JSON.stringify(summary, null, 2));
  console.log(`${scanner}: ${summary.blocked ? 'BLOCKED' : 'PASSED'}`);
  process.exitCode = summary.blocked ? 1 : 0;
}

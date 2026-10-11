import { test } from 'node:test';
import assert from 'node:assert/strict';
import { evaluateSemgrep, evaluateZap } from './gate.mjs';
import { mkdtempSync, writeFileSync, readFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { spawnSync } from 'node:child_process';

const sast = (severity, impact) => ({
  paths: { scanned: ['back/Example.cs', 'front/example.tsx'] }, errors: [],
  results: severity ? [{ check_id: 'test-rule', path: 'front/example.tsx', start: { line: 1 },
    extra: { severity, metadata: { impact }, lines: 'private source', message: 'private evidence' } }] : [],
});

test('SAST permits clean/lower severity scans, blocks high/critical and high-impact rules', () => {
  assert.equal(evaluateSemgrep(sast()).blocked, false);
  assert.equal(evaluateSemgrep(sast('WARNING', 'MEDIUM')).blocked, false);
  for (const severity of ['ERROR', 'HIGH', 'CRITICAL']) assert.equal(evaluateSemgrep(sast(severity)).blocked, true);
  assert.equal(evaluateSemgrep(sast('WARNING', 'HIGH')).blocked, true);
});

test('SAST fails closed on incomplete scans and unknown severities', () => {
  for (const report of [{}, { ...sast(), errors: [{}] }, { ...sast(), paths: { scanned: ['front/a.ts'] } }, sast('UNKNOWN')]) {
    assert.throws(() => evaluateSemgrep(report));
  }
});

test('DAST blocks the highest ZAP risk level; medium/low remain visible', () => {
  assert.equal(evaluateZap([]).blocked, false);
  for (const risk of ['Low', 'Medium', '0', '1', '2']) assert.equal(evaluateZap([{ risk }]).blocked, false);
  for (const risk of ['High', '3']) assert.equal(evaluateZap([{ risk }]).blocked, true);
  assert.throws(() => evaluateZap([{ risk: 'unknown' }]));
  assert.throws(() => evaluateZap(null));
});

test('Published summaries omit source, evidence, headers and tokens', () => {
  assert.ok(!JSON.stringify(evaluateSemgrep(sast('HIGH'))).includes('private'));
  const result = evaluateZap([{ risk: 'High', evidence: 'secret', url: 'http://backend/?token=secret', requestHeader: 'secret' }]);
  assert.ok(!JSON.stringify(result).includes('secret'));
});

test('CLI returns failing status for high/critical findings and corrupt/missing reports', () => {
  const directory = mkdtempSync(join(tmpdir(), 'horamais-security-'));
  try {
    const input = join(directory, 'scan.json');
    const output = join(directory, 'summary.json');
    for (const [report, expected] of [[sast(), 0], [sast('HIGH'), 1], [sast('CRITICAL'), 1], [{}, 1]]) {
      writeFileSync(input, JSON.stringify(report));
      const result = spawnSync(process.execPath, ['scripts/security/gate.mjs', 'semgrep', input, output]);
      assert.equal(result.status, expected, result.stderr.toString());
      assert.equal(JSON.parse(readFileSync(output)).blocked, expected === 1);
    }
    rmSync(input);
    const missing = spawnSync(process.execPath, ['scripts/security/gate.mjs', 'semgrep', input, output]);
    assert.equal(missing.status, 1);
  } finally {
    rmSync(directory, { recursive: true });
  }
});

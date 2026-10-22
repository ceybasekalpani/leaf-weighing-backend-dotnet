import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';

const utilityPath = process.argv[2];
if (!utilityPath) throw new Error('Pass the path to frontend src/utils/weightUtils.js');
const source = await readFile(utilityPath, 'utf8');
const { calculateNetWeight, calculateAdjustedGross, calculateNetWeightFromAdjustedGross,
  getAdjustedGrossFromSummary, normalizeCollectionWeights } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);
const cases = JSON.parse(await readFile(new URL('./net-weight-cases.json', import.meta.url), 'utf8'));
const frontendRow = (row) => ({
  gross: row.Gross, bagWeight: row.BagWeight, water: row.Water,
  coarse: row.Coarse, rejected: row.Rejected, boiled: row.Boild,
  superDeduction: row.SuperDeduction, normalDeduction: row.NormalDeduction,
});
for (const test of cases) {
  const adjustedGross = Number(test.rows.reduce((sum, row) => sum + calculateAdjustedGross(frontendRow(row)), 0).toFixed(4));
  assert.equal(adjustedGross, test.expectedGross, `${test.name}: adjusted gross`);
  const actual = Number(test.rows.reduce((sum, row) => sum + calculateNetWeight(frontendRow(row)), 0).toFixed(4));
  assert.equal(actual, test.expected, test.name);
  const fromAdjustedGross = Number(test.rows.reduce((sum, row) => sum + calculateNetWeightFromAdjustedGross({
    ...frontendRow(row), gross: calculateAdjustedGross(frontendRow(row)),
  }), 0).toFixed(4));
  assert.equal(fromAdjustedGross, test.expected, `${test.name}: no repeated Super/Normal deduction`);
}
// Existing saved deductions plus unsaved inputs must each be subtracted once.
assert.equal(calculateNetWeight({ gross: '1000', bagWeight: '12.5', water: '3', coarse: '4',
  rejected: '5', boiled: '6.25', superDeduction: '7', normalDeduction: '8' }), 954.25);
assert.equal(calculateNetWeight({ gross: '0.3', bagWeight: '0.1', boiled: '0.2' }), 0);
if (process.argv[3]) {
  const page = await readFile(process.argv[3], 'utf8');
  const grossExpression = page.match(/const adjustedGross = ([\s\S]*?);/)?.[1];
  assert.ok(grossExpression, 'Find the actual Add Deduction Gross calculation');
  const displayGross = new Function('getAdjustedGrossFromSummary', 'data', `return ${grossExpression};`);
  assert.equal(displayGross(getAdjustedGrossFromSummary, {
    totalRawGross: 1000, totalGross: 870, totalSuperDeduction: 60, totalNormalDeduction: 70,
  }), 870, 'Displayed Gross subtracts both deductions from original gross');
  assert.equal(displayGross(getAdjustedGrossFromSummary, {
    TotalRawGross: '1000', TotalGross: '870', TotalSuperDeduction: '60', TotalNormalDeduction: '70',
  }), 870, 'PascalCase response and numeric strings');
  assert.equal(displayGross(getAdjustedGrossFromSummary, {
    totalGross: 180, totalSuperDeduction: 41, totalNormalDeduction: 0,
  }), 139, 'RegNo 3 running legacy API response must display 139');
  const expression = page.match(/const displayedNetWeight = ([\s\S]*?)\.toString\(\);/)?.[1];
  assert.ok(expression, 'Find the actual Add Deduction preview calculation');
  const preview = new Function('calculateNetWeightFromAdjustedGross', 'summaryTotals',
    'displayedBagWeight', 'displayedWater', 'displayedCoarce', 'displayedRejected', 'displayedBoiled',
    `return ${expression};`);
  assert.equal(preview(calculateNetWeightFromAdjustedGross,
    { gross: '870', superDeduction: '60', normalDeduction: '70' },
    '10', '20', '30', '40', '50'), 720, 'Actual page preview must use adjusted API gross once');
}
const legacyCollection = { regNo: 3, gross: '180', totalSuperDeduction: '41', totalNormalDeduction: '0' };
const updatedCollection = { ...legacyCollection, rawGross: '180', gross: '139' };
assert.equal(normalizeCollectionWeights(legacyCollection).gross, '139', 'View Collection legacy response');
assert.equal(normalizeCollectionWeights(updatedCollection).gross, '139', 'View Collection updated response');
assert.equal(normalizeCollectionWeights(normalizeCollectionWeights(legacyCollection)).gross, '139', 'Repeated normalization must preserve gross');
console.log(`Passed ${cases.length} shared frontend cases, adjusted gross, preview and decimal arithmetic.`);

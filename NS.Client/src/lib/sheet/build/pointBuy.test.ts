import { describe, expect, it } from 'vitest';
import { canDecrement, canIncrement, costOf, remaining, totalCost } from './pointBuy';
import type { AbilityScores } from '$lib/api/types';

const scores = (d: number, i: number, s: number, w: number): AbilityScores =>
  ({ dexterity: d, intelligence: i, strength: s, will: w });

describe('pointBuy', () => {
  it('costOf follows the table', () => {
    expect(costOf(8)).toBe(0);
    expect(costOf(13)).toBe(5);
    expect(costOf(15)).toBe(9);
  });

  it('totalCost sums all four', () => {
    expect(totalCost(scores(14, 13, 12, 8))).toBe(7 + 5 + 4 + 0);
  });

  it('remaining is budget minus spent', () => {
    expect(remaining(scores(8, 8, 8, 8))).toBe(27);
    expect(remaining(scores(15, 14, 13, 13))).toBe(27 - 26);
  });

  it('canIncrement is false at max or when unaffordable', () => {
    expect(canIncrement(scores(8, 8, 8, 8), 'strength')).toBe(true);
    expect(canIncrement(scores(15, 8, 8, 8), 'dexterity')).toBe(false); // at max
    // 15(9)+15(9)+8(0)+8(0)=18 spent, 9 left; raising will 8->9 costs 1 -> affordable
    expect(canIncrement(scores(15, 15, 8, 8), 'will')).toBe(true);
    // 15(9)+15(9)+14(7)=25 spent for d/i/s, 2 left; will 13->14 costs 7-5=2 -> affordable; ->15 costs 2 more, only after
    expect(canIncrement(scores(15, 15, 14, 13), 'will')).toBe(false); // 25+5=30 already... guard via remaining
  });

  it('canDecrement is false at min', () => {
    expect(canDecrement(scores(8, 10, 10, 10), 'dexterity')).toBe(false);
    expect(canDecrement(scores(9, 10, 10, 10), 'dexterity')).toBe(true);
  });
});

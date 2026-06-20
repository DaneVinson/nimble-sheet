import type { AbilityScores } from '$lib/api/types';

export const POINT_BUY_MIN = 8;
export const POINT_BUY_MAX = 15;
export const POINT_BUY_BUDGET = 27;

export type AbilityKey = 'dexterity' | 'intelligence' | 'strength' | 'will';

const COST: Record<number, number> = { 8: 0, 9: 1, 10: 2, 11: 3, 12: 4, 13: 5, 14: 7, 15: 9 };

export function costOf(score: number): number {
  return COST[score] ?? Number.POSITIVE_INFINITY;
}

export function totalCost(scores: AbilityScores): number {
  return costOf(scores.dexterity) + costOf(scores.intelligence) + costOf(scores.strength) + costOf(scores.will);
}

export function remaining(scores: AbilityScores): number {
  return POINT_BUY_BUDGET - totalCost(scores);
}

export function canIncrement(scores: AbilityScores, key: AbilityKey): boolean {
  const current = scores[key];
  if (current >= POINT_BUY_MAX) return false;
  const delta = costOf(current + 1) - costOf(current);
  return remaining(scores) >= delta;
}

export function canDecrement(scores: AbilityScores, key: AbilityKey): boolean {
  return scores[key] > POINT_BUY_MIN;
}

import type { DieType } from '../api/types';

/** Formats a skill/bonus modifier: positive values get a leading '+', zero and negatives are shown as-is. */
export function formatModifier(value: number): string {
  return value > 0 ? `+${value}` : `${value}`;
}

/** Formats a die type for display, e.g. 'D10' -> 'd10'. */
export function formatDie(die: DieType): string {
  return die.toLowerCase();
}

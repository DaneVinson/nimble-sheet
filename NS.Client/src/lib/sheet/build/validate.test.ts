import { describe, expect, it } from 'vitest';
import { validateBuild } from './validate';
import { blankBuildModel } from './model';

describe('validateBuild', () => {
  it('create requires name, ancestry, class', () => {
    const e = validateBuild(blankBuildModel(), { mode: 'create', level: 1 });
    expect(e.name).toBeDefined();
    expect(e.ancestryId).toBeDefined();
    expect(e.heroClass).toBeDefined();
  });

  it('create passes with valid inputs', () => {
    const m = { ...blankBuildModel(), name: 'Caldra', ancestryId: 'a1', heroClass: 'Oathsworn' as const };
    expect(validateBuild(m, { mode: 'create', level: 1 })).toEqual({});
  });

  it('create rejects over-budget scores', () => {
    const m = {
      ...blankBuildModel(), name: 'Caldra', ancestryId: 'a1', heroClass: 'Mage' as const,
      baseAbilityScores: { dexterity: 15, intelligence: 15, strength: 15, will: 9 }
    };
    expect(validateBuild(m, { mode: 'create', level: 1 }).baseAbilityScores).toBeDefined();
  });

  it('edit checks maxHp bounds', () => {
    const m = { ...blankBuildModel(), name: 'Caldra', ancestryId: 'a1', heroClass: 'Oathsworn' as const, maxHp: 100 };
    expect(validateBuild(m, { mode: 'edit', level: 1 }).maxHp).toBeDefined();
    expect(validateBuild({ ...m, maxHp: 17 }, { mode: 'edit', level: 1 }).maxHp).toBeUndefined();
  });
});

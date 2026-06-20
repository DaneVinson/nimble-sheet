import { describe, expect, it } from 'vitest';
import { blankBuildModel, defaultMaxHpForClass, heroToBuildModel, normalizeBuild } from './model';

describe('build model', () => {
  it('blank model has class unset and all scores at 8', () => {
    const m = blankBuildModel();
    expect(m.heroClass).toBe('');
    expect(m.baseAbilityScores).toEqual({ dexterity: 8, intelligence: 8, strength: 8, will: 8 });
  });

  it('defaultMaxHpForClass returns the class starting HP', () => {
    expect(defaultMaxHpForClass('Oathsworn')).toBe(17);
    expect(defaultMaxHpForClass('')).toBe(0);
  });

  it('normalizeBuild coerces NaN scores to 0', () => {
    const m = blankBuildModel();
    m.baseAbilityScores.strength = NaN as unknown as number;
    expect(normalizeBuild(m).baseAbilityScores.strength).toBe(0);
  });

  it('heroToBuildModel copies class, base scores and maxHp', () => {
    const hero = {
      name: 'Caldra', ancestryId: 'a1', backgroundId: null, class: 'Mage',
      baseAbilityScores: { dexterity: 10, intelligence: 14, strength: 8, will: 12 }, maxHp: 10
    } as unknown as Parameters<typeof heroToBuildModel>[0];
    const m = heroToBuildModel(hero);
    expect(m.heroClass).toBe('Mage');
    expect(m.baseAbilityScores.intelligence).toBe(14);
    expect(m.maxHp).toBe(10);
  });
});

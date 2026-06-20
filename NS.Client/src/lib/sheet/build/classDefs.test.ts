import { describe, expect, it } from 'vitest';
import { abilityModifier, finalScores, maxHpBounds, playableClasses, previewMaxMana, startingHp } from './classDefs';

describe('classDefs', () => {
  it('lists exactly the four playable classes', () => {
    expect([...playableClasses].sort()).toEqual(['Cheat', 'Hunter', 'Mage', 'Oathsworn']);
  });

  it('abilityModifier floors (score-10)/2', () => {
    expect(abilityModifier(8)).toBe(-1);
    expect(abilityModifier(14)).toBe(2);
    expect(abilityModifier(11)).toBe(0);
  });

  it('finalScores add ancestry bonuses', () => {
    expect(finalScores({ dexterity: 10, intelligence: 12, strength: 14, will: 8 }, { dexterity: 0, intelligence: 2, strength: 0, will: 1 }))
      .toEqual({ dexterity: 10, intelligence: 14, strength: 14, will: 9 });
  });

  it('previewMaxMana matches per-class rules', () => {
    const f = { dexterity: 10, intelligence: 14, strength: 10, will: 14 };
    expect(previewMaxMana('Mage', f, 1)).toBe(7);          // intMod 2 *3 +1
    expect(previewMaxMana('Oathsworn', f, 1)).toBeNull();  // caster from level 2
    expect(previewMaxMana('Oathsworn', f, 3)).toBe(5);     // wilMod 2 + 3
    expect(previewMaxMana('Hunter', f, 5)).toBeNull();
  });

  it('startingHp and maxHpBounds come from the class block', () => {
    expect(startingHp('Oathsworn')).toBe(17);
    expect(maxHpBounds('Oathsworn', 3)).toEqual({ min: 17, max: 17 + 10 * 2 });
  });
});

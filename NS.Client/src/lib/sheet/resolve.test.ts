import { describe, expect, it } from 'vitest';
import { caldra, referenceData } from '../fixtures/caldra';
import type { Hero } from '../api/types';
import { resolveSheet } from './resolve';

describe('resolveSheet', () => {
  const vm = resolveSheet(caldra, referenceData);

  it('resolves identity fields', () => {
    expect(vm.name).toBe('Caldra Brightward');
    expect(vm.className).toBe('Oathsworn');
    expect(vm.ancestryName).toBe('Human');
    expect(vm.level).toBe(1);
  });

  it('derives save markers from HeroSaves', () => {
    const str = vm.stats.find((s) => s.type === 'Strength');
    const dex = vm.stats.find((s) => s.type === 'Dexterity');
    const int = vm.stats.find((s) => s.type === 'Intelligence');
    expect(str?.save).toBe('advantage');
    expect(dex?.save).toBe('disadvantage');
    expect(int?.save).toBe(null);
  });

  it('formats skill modifiers with sign rules', () => {
    const influence = vm.skills.find((s) => s.name === 'Influence');
    const lore = vm.skills.find((s) => s.name === 'Lore');
    const finesse = vm.skills.find((s) => s.name === 'Finesse');
    expect(influence?.display).toBe('+4');
    expect(lore?.display).toBe('-1');
    expect(finesse?.display).toBe('0');
  });

  it('formats hit die in lowercase', () => {
    expect(vm.hitDice.die).toBe('d10');
  });

  it('joins weapons to reference data', () => {
    expect(vm.weapons).toHaveLength(1);
    expect(vm.weapons[0].name).toBe('Mace');
    expect(vm.weapons[0].damage).toBe('1d6+2');
    expect(vm.weapons[0].statLabel).toBe('STR');
  });

  it('builds class resources, skipping mana for non-casters', () => {
    const labels = vm.classResources.map((r) => r.label);
    expect(labels).not.toContain('Mana');
    expect(vm.classResources).toContainEqual({ label: 'Judgment Dice', value: '2d6' });
    expect(vm.classResources).toContainEqual({ label: 'Lay on Hands', value: '5' });
  });

  it('groups features by level', () => {
    expect(vm.features).toHaveLength(1);
    expect(vm.features[0].level).toBe(1);
    expect(vm.features[0].features.map((f) => f.name)).toEqual(['Lay on Hands', 'Radiant Judgment']);
  });

  it('produces empty collections for absent sections', () => {
    expect(vm.spellsByTier).toEqual([]);
    expect(vm.magicItems).toEqual([]);
    expect(vm.conditions).toEqual([]);
  });

  it('falls back gracefully when a referenced entity is missing', () => {
    const heroWithBadWeapon: Hero = {
      ...caldra,
      weapons: [{ heroId: caldra.id, isEquipped: true, notes: null, weaponId: 'missing-id' }]
    };
    const result = resolveSheet(heroWithBadWeapon, referenceData);
    expect(result.weapons[0].name).toBe('Unknown weapon');
  });
});

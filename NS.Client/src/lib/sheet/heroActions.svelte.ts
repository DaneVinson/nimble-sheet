import { invalidateAll } from '$app/navigation';
import {
	addArmor, addCondition, addFeature, addGearItem, addMagicItem, addSpell, addWeapon, applyHpIncrease, applyStatIncrease,
	finalizeSkillAllocation, gainWound, grantTempHp, heal, healWound, levelUp as levelUpRequest,
	recoverAll, removeArmor, removeCondition, removeFeature, removeGearItem, removeMagicItem, removeSpell, removeWeapon,
	setArmorEquipped, setMagicItemEquipped, setSubclass, setWeaponEquipped, spendHitDice, spendMana, takeDamage
} from '$lib/api/client';
import type { HeroSkills } from '$lib/api/types';
import { runAction } from './runAction';

/** Context key for the per-hero mutation actions. */
export const HERO_ACTIONS = Symbol('heroActions');

/** Reactive mutation actions for the displayed hero. `busy`/`error` are shared across all actions. */
export interface HeroActions {
	readonly busy: boolean;
	readonly error: string | null;
	takeDamage(amount: number): Promise<void>;
	heal(amount: number): Promise<void>;
	grantTempHp(amount: number): Promise<void>;
	gainWound(): Promise<void>;
	healWound(): Promise<void>;
	spendHitDice(count: number, healingAmount: number): Promise<void>;
	spendMana(amount: number): Promise<void>;
	recoverAll(): Promise<void>;
	addWeapon(weaponId: string, isEquipped: boolean, notes: string | null): Promise<void>;
	removeWeapon(weaponId: string): Promise<void>;
	setWeaponEquipped(weaponId: string, isEquipped: boolean): Promise<void>;
	addArmor(armorId: string, isEquipped: boolean): Promise<void>;
	removeArmor(armorId: string): Promise<void>;
	setArmorEquipped(armorId: string, isEquipped: boolean): Promise<void>;
	addMagicItem(magicItemId: string, isEquipped: boolean, chargesRemaining: number | null): Promise<void>;
	removeMagicItem(magicItemId: string): Promise<void>;
	setMagicItemEquipped(magicItemId: string, isEquipped: boolean): Promise<void>;
	addSpell(spellId: string, tierUnlocked: number, notes: string | null): Promise<void>;
	removeSpell(spellId: string): Promise<void>;
	addGearItem(name: string, quantity: number): Promise<void>;
	removeGearItem(name: string): Promise<void>;
	addCondition(conditionId: string, expiresAtEndOf: string | null): Promise<void>;
	removeCondition(conditionId: string): Promise<void>;
	addFeature(featureId: string, choices: string[], levelGained: number): Promise<void>;
	removeFeature(featureId: string): Promise<void>;
	levelUp(hpIncrease: number): Promise<void>;
	applyStatIncrease(stat: string): Promise<void>;
	finalizeSkillAllocation(skills: HeroSkills): Promise<void>;
	setSubclass(subclass: string): Promise<void>;
}

/** Create the actions bound to a (lazily-read) hero id. Each action POSTs then re-fetches. */
export function createHeroActions(getHeroId: () => string): HeroActions {
	let busy = $state(false);
	let error = $state<string | null>(null);
	const setBusy = (value: boolean) => (busy = value);
	const setError = (value: string | null) => (error = value);
	const run = (action: () => Promise<void>) => runAction(action, invalidateAll, setBusy, setError);

	return {
		get busy() {
			return busy;
		},
		get error() {
			return error;
		},
		takeDamage: (amount) => run(() => takeDamage(getHeroId(), amount)),
		heal: (amount) => run(() => heal(getHeroId(), amount)),
		grantTempHp: (amount) => run(() => grantTempHp(getHeroId(), amount)),
		gainWound: () => run(() => gainWound(getHeroId())),
		healWound: () => run(() => healWound(getHeroId())),
		spendHitDice: (count, healingAmount) => run(() => spendHitDice(getHeroId(), count, healingAmount)),
		spendMana: (amount) => run(() => spendMana(getHeroId(), amount)),
		recoverAll: () => run(() => recoverAll(getHeroId())),
		addWeapon: (weaponId, isEquipped, notes) => run(() => addWeapon(getHeroId(), weaponId, isEquipped, notes)),
		removeWeapon: (weaponId) => run(() => removeWeapon(getHeroId(), weaponId)),
		setWeaponEquipped: (weaponId, isEquipped) => run(() => setWeaponEquipped(getHeroId(), weaponId, isEquipped)),
		addArmor: (armorId, isEquipped) => run(() => addArmor(getHeroId(), armorId, isEquipped)),
		removeArmor: (armorId) => run(() => removeArmor(getHeroId(), armorId)),
		setArmorEquipped: (armorId, isEquipped) => run(() => setArmorEquipped(getHeroId(), armorId, isEquipped)),
		addMagicItem: (magicItemId, isEquipped, chargesRemaining) => run(() => addMagicItem(getHeroId(), magicItemId, isEquipped, chargesRemaining)),
		removeMagicItem: (magicItemId) => run(() => removeMagicItem(getHeroId(), magicItemId)),
		setMagicItemEquipped: (magicItemId, isEquipped) => run(() => setMagicItemEquipped(getHeroId(), magicItemId, isEquipped)),
		addSpell: (spellId, tierUnlocked, notes) => run(() => addSpell(getHeroId(), spellId, tierUnlocked, notes)),
		removeSpell: (spellId) => run(() => removeSpell(getHeroId(), spellId)),
		addGearItem: (name, quantity) => run(() => addGearItem(getHeroId(), name, quantity)),
		removeGearItem: (name) => run(() => removeGearItem(getHeroId(), name)),
		addCondition: (conditionId, expiresAtEndOf) => run(() => addCondition(getHeroId(), conditionId, expiresAtEndOf)),
		removeCondition: (conditionId) => run(() => removeCondition(getHeroId(), conditionId)),
		addFeature: (featureId, choices, levelGained) => run(() => addFeature(getHeroId(), featureId, choices, levelGained)),
		removeFeature: (featureId) => run(() => removeFeature(getHeroId(), featureId)),
		levelUp: (hpIncrease) =>
			run(async () => {
				if (hpIncrease > 0) {
					await applyHpIncrease(getHeroId(), hpIncrease);
				}
				await levelUpRequest(getHeroId());
			}),
		applyStatIncrease: (stat) => run(() => applyStatIncrease(getHeroId(), stat)),
		finalizeSkillAllocation: (skills) => run(() => finalizeSkillAllocation(getHeroId(), skills)),
		setSubclass: (subclass) => run(() => setSubclass(getHeroId(), subclass))
	};
}

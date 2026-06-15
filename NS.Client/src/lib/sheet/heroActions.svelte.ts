import { invalidateAll } from '$app/navigation';
import {
	addWeapon, gainWound, grantTempHp, heal, healWound, recoverAll, removeWeapon,
	setWeaponEquipped, spendHitDice, spendMana, takeDamage
} from '$lib/api/client';
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
		setWeaponEquipped: (weaponId, isEquipped) => run(() => setWeaponEquipped(getHeroId(), weaponId, isEquipped))
	};
}

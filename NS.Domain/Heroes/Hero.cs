namespace NS.Domain;

/// <summary>The primary entity representing a player character in Nimble.</summary>
public sealed class Hero
{
    private List<HeroArmor> _armor = [];
    private List<HeroCondition> _conditions = [];
    private List<HeroFeature> _features = [];
    private List<HeroGearItem> _gear = [];
    private List<HeroMagicItem> _magicItems = [];
    private List<string> _pendingFeatureChoices = [];
    private List<HeroSpell> _spells = [];
    private List<HeroWeapon> _weapons = [];

    /// <summary>Private parameterless constructor reserved for deserializers.</summary>
    private Hero()
    {
        CombatStats = null!;
        Name = null!;
        Resources = null!;
        Saves = null!;
        Skills = null!;
        Stats = null!;
    }

    /// <summary>Initializes a new level-1 hero.</summary>
    /// <param name="ancestryId">The identifier of the hero's ancestry.</param>
    /// <param name="backgroundId">The optional identifier of the hero's background.</param>
    /// <param name="combatStats">The hero's initial combat statistics.</param>
    /// <param name="heroClass">The hero's class.</param>
    /// <param name="maxHp">The hero's starting maximum hit points.</param>
    /// <param name="maxMana">The hero's starting maximum mana; <see langword="null"/> for non-casters.</param>
    /// <param name="name">The hero's name.</param>
    /// <param name="resources">The hero's class-specific resource pools.</param>
    /// <param name="saves">The hero's advantaged and disadvantaged saves.</param>
    /// <param name="skills">The hero's initial skill bonuses.</param>
    /// <param name="stats">The hero's base stats.</param>
    /// <param name="userId">The identifier of the <see cref="User"/> who owns this hero.</param>
    public Hero(
        Guid ancestryId,
        Guid? backgroundId,
        HeroCombatStats combatStats,
        HeroClass heroClass,
        int maxHp,
        int? maxMana,
        string name,
        ClassResources resources,
        HeroSaves saves,
        HeroSkills skills,
        HeroStats stats,
        Guid userId)
    {
        AncestryId = ancestryId;
        BackgroundId = backgroundId;
        Class = heroClass;
        CombatStats = combatStats;
        CurrentHp = maxHp;
        CurrentMana = maxMana;
        CurrentWounds = 0;
        HitDiceAvailable = 1;
        Id = Guid.CreateVersion7();
        Level = 1;
        MaxHitDice = 1;
        MaxHp = maxHp;
        MaxMana = maxMana;
        Name = name;
        PendingStatIncrease = false;
        Resources = resources;
        Saves = saves;
        Skills = skills;
        Stats = stats;
        TempHp = 0;
        UnspentSkillPoints = 0;
        UserId = userId;
    }

    /// <summary>The active conditions currently affecting the hero.</summary>
    public IReadOnlyList<HeroCondition> ActiveConditions { get => _conditions; init => _conditions = value is null ? [] : [.. value]; }

    /// <summary>The identifier of the hero's ancestry.</summary>
    public Guid AncestryId { get; private set; }

    /// <summary>The armor items the hero is carrying or wearing.</summary>
    public IReadOnlyList<HeroArmor> Armor { get => _armor; init => _armor = value is null ? [] : [.. value]; }

    /// <summary>The identifier of the hero's background; <see langword="null"/> if none selected.</summary>
    public Guid? BackgroundId { get; private set; }

    /// <summary>The hero's class.</summary>
    public HeroClass Class { get; private set; }

    /// <summary>The hero's combat statistics including armor value, hit die type, initiative bonus, and speed.</summary>
    public HeroCombatStats CombatStats { get; private set; }

    /// <summary>The hero's current hit points. Cannot go below zero.</summary>
    public int CurrentHp { get; private set; }

    /// <summary>The hero's current mana; <see langword="null"/> for non-casters.</summary>
    public int? CurrentMana { get; private set; }

    /// <summary>The number of wounds the hero has accumulated. Death occurs at 6.</summary>
    public int CurrentWounds { get; private set; }

    /// <summary>The class features the hero has unlocked.</summary>
    public IReadOnlyList<HeroFeature> Features { get => _features; init => _features = value is null ? [] : [.. value]; }

    /// <summary>The mundane gear items the hero is carrying.</summary>
    public IReadOnlyList<HeroGearItem> Gear { get => _gear; init => _gear = value is null ? [] : [.. value]; }

    /// <summary>The number of hit dice currently available to spend during a rest.</summary>
    public int HitDiceAvailable { get; private set; }

    /// <summary>The unique identifier of this hero.</summary>
    public Guid Id { get; private set; }

    /// <summary>Whether the hero is dead (6 or more wounds accumulated).</summary>
    public bool IsDead => CurrentWounds >= 6;

    /// <summary>Whether the hero is at 0 hit points and in the dying state.</summary>
    public bool IsDying => CurrentHp == 0;

    /// <summary>The spells this hero knows.</summary>
    public IReadOnlyList<HeroSpell> KnownSpells { get => _spells; init => _spells = value is null ? [] : [.. value]; }

    /// <summary>The hero's current level (1–20).</summary>
    public int Level { get; private set; }

    /// <summary>The magic items the hero is carrying or wearing.</summary>
    public IReadOnlyList<HeroMagicItem> MagicItems { get => _magicItems; init => _magicItems = value is null ? [] : [.. value]; }

    /// <summary>The maximum number of hit dice the hero can hold; equals the hero's level.</summary>
    public int MaxHitDice { get; private set; }

    /// <summary>The hero's maximum hit points.</summary>
    public int MaxHp { get; private set; }

    /// <summary>The hero's maximum mana; <see langword="null"/> for non-casters.</summary>
    public int? MaxMana { get; private set; }

    /// <summary>The hero's name.</summary>
    public string Name { get; private set; }

    /// <summary>Level-up feature selections still to be resolved, e.g. "Choose Subclass", "Choose Underhanded Ability".</summary>
    public IReadOnlyList<string> PendingFeatureChoices { get => _pendingFeatureChoices; init => _pendingFeatureChoices = value is null ? [] : [.. value]; }

    /// <summary>Whether the hero has a pending +1 stat increase from leveling up.</summary>
    public bool PendingStatIncrease { get; private set; }

    /// <summary>The hero's class-specific resource pools such as Judgment Dice, Lay on Hands, and Thrill of the Hunt charges.</summary>
    public ClassResources Resources { get; private set; }

    /// <summary>The hero's advantaged and disadvantaged save types.</summary>
    public HeroSaves Saves { get; private set; }

    /// <summary>The hero's current skill bonuses, each capped at +12.</summary>
    public HeroSkills Skills { get; private set; }

    /// <summary>The hero's base stats.</summary>
    public HeroStats Stats { get; private set; }

    /// <summary>The hero's chosen subclass; <see langword="null"/> until level 3.</summary>
    public string? Subclass { get; private set; }

    /// <summary>The hero's temporary hit points, which absorb damage before current hit points. Lost on a Safe Rest.</summary>
    public int TempHp { get; private set; }

    /// <summary>Skill points available to allocate from the most recent level-up.</summary>
    public int UnspentSkillPoints { get; private set; }

    /// <summary>The identifier of the <see cref="User"/> who owns this hero.</summary>
    public Guid UserId { get; private set; }

    /// <summary>The weapons the hero is carrying or wielding.</summary>
    public IReadOnlyList<HeroWeapon> Weapons { get => _weapons; init => _weapons = value is null ? [] : [.. value]; }

    /// <summary>Adds an armor item to the hero's equipment.</summary>
    public void AddArmor(HeroArmor armor) => _armor.Add(armor);

    /// <summary>Applies a condition to the hero.</summary>
    public void AddCondition(HeroCondition condition) => _conditions.Add(condition);

    /// <summary>Adds an unlocked class feature to the hero.</summary>
    public void AddFeature(HeroFeature feature) => _features.Add(feature);

    /// <summary>Adds a mundane gear item to the hero's inventory.</summary>
    public void AddGearItem(HeroGearItem item) => _gear.Add(item);

    /// <summary>Adds a magic item to the hero's inventory.</summary>
    public void AddMagicItem(HeroMagicItem item) => _magicItems.Add(item);

    /// <summary>Adds a spell to the hero's known spells.</summary>
    public void AddSpell(HeroSpell spell) => _spells.Add(spell);

    /// <summary>Adds a weapon to the hero's equipment.</summary>
    public void AddWeapon(HeroWeapon weapon) => _weapons.Add(weapon);

    /// <summary>Increases the hero's maximum and current hit points, typically applied from a level-up HP roll.</summary>
    public void ApplyHpIncrease(int amount)
    {
        MaxHp += amount;
        CurrentHp += amount;
    }

    /// <summary>Applies a +1 increase to the specified stat and clears the pending stat increase flag.</summary>
    public void ApplyStatIncrease(StatType stat)
    {
        Stats = stat switch
        {
            StatType.Dexterity => Stats with { Dexterity = Stats.Dexterity + 1 },
            StatType.Intelligence => Stats with { Intelligence = Stats.Intelligence + 1 },
            StatType.Strength => Stats with { Strength = Stats.Strength + 1 },
            StatType.Will => Stats with { Will = Stats.Will + 1 },
            _ => Stats
        };
        PendingStatIncrease = false;
    }

    /// <summary>Resolves a pending feature choice, removing it from the pending list and recording the completed feature.</summary>
    public void CompletePendingChoice(string choiceLabel, HeroFeature feature)
    {
        _pendingFeatureChoices.Remove(choiceLabel);
        _features.Add(feature);
    }

    /// <summary>Finalizes skill point allocation after a level-up, replacing the current skills and clearing unspent points.</summary>
    public void FinalizeSkillAllocation(HeroSkills updatedSkills)
    {
        Skills = updatedSkills;
        UnspentSkillPoints = 0;
    }

    /// <summary>Inflicts one wound on the hero. Death occurs at 6 wounds.</summary>
    public void GainWound() => CurrentWounds = Math.Min(CurrentWounds + 1, 6);

    /// <summary>Grants temporary hit points. Temp HP does not stack; the greater of the current and granted values is kept.</summary>
    public void GrantTempHp(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        TempHp = Math.Max(TempHp, amount);
    }

    /// <summary>Restores the specified amount of hit points, up to the hero's maximum.</summary>
    public void Heal(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        CurrentHp = Math.Min(CurrentHp + amount, MaxHp);
    }

    /// <summary>Heals one wound.</summary>
    public void HealWound() => CurrentWounds = Math.Max(CurrentWounds - 1, 0);

    /// <summary>Advances the hero to the next level, incrementing the hit dice maximum and setting pending level-up choices.</summary>
    public void LevelUp(IEnumerable<string> pendingChoices)
    {
        Level++;
        MaxHitDice = Level;
        PendingStatIncrease = true;
        UnspentSkillPoints++;
        _pendingFeatureChoices.AddRange(pendingChoices);
    }

    /// <summary>Restores all hit points, hit dice, mana, and class resources, and heals 1 wound. Called after a Safe Rest.</summary>
    public void RecoverAllResources()
    {
        CurrentHp = MaxHp;
        CurrentMana = MaxMana;
        HitDiceAvailable = MaxHitDice;
        TempHp = 0;
        HealWound();
        if (Resources.LayOnHandsPool.HasValue)
            Resources = Resources with { LayOnHandsPool = 5 * Level };
    }

    /// <summary>Removes an armor item from the hero's equipment by the referenced armor entity identifier.</summary>
    public void RemoveArmor(Guid armorId) =>
        _armor.RemoveAll(a => a.ArmorId == armorId);

    /// <summary>Removes a condition from the hero by the referenced condition entity identifier.</summary>
    public void RemoveCondition(Guid conditionId) =>
        _conditions.RemoveAll(c => c.ConditionId == conditionId);

    /// <summary>Removes a class feature by the referenced feature entity identifier.</summary>
    public void RemoveFeature(Guid featureId) =>
        _features.RemoveAll(f => f.FeatureId == featureId);

    /// <summary>Removes a mundane gear item by name.</summary>
    public void RemoveGearItem(string name) =>
        _gear.RemoveAll(g => g.Name == name);

    /// <summary>Removes a magic item from the hero's inventory by the referenced magic item entity identifier.</summary>
    public void RemoveMagicItem(Guid magicItemId) =>
        _magicItems.RemoveAll(m => m.MagicItemId == magicItemId);

    /// <summary>Removes a spell from the hero's known spells by the referenced spell entity identifier.</summary>
    public void RemoveSpell(Guid spellId) =>
        _spells.RemoveAll(s => s.SpellId == spellId);

    /// <summary>Removes a weapon from the hero's equipment by the referenced weapon entity identifier.</summary>
    public void RemoveWeapon(Guid weaponId) =>
        _weapons.RemoveAll(w => w.WeaponId == weaponId);

    /// <summary>Sets whether the referenced armor item is equipped; no-op if the hero does not have it.</summary>
    public void SetArmorEquipped(Guid armorId, bool isEquipped)
    {
        var index = _armor.FindIndex(a => a.ArmorId == armorId);
        if (index >= 0)
        {
            _armor[index] = _armor[index] with { IsEquipped = isEquipped };
        }
    }

    /// <summary>Sets whether the referenced magic item is equipped; no-op if the hero does not have it.</summary>
    public void SetMagicItemEquipped(Guid magicItemId, bool isEquipped)
    {
        var index = _magicItems.FindIndex(m => m.MagicItemId == magicItemId);
        if (index >= 0)
        {
            _magicItems[index] = _magicItems[index] with { IsEquipped = isEquipped };
        }
    }

    /// <summary>Sets the hero's subclass, chosen at level 3.</summary>
    public void SetSubclass(string subclass) => Subclass = subclass;

    /// <summary>Sets whether the referenced weapon is equipped; no-op if the hero does not have it.</summary>
    public void SetWeaponEquipped(Guid weaponId, bool isEquipped)
    {
        var index = _weapons.FindIndex(w => w.WeaponId == weaponId);
        if (index >= 0)
        {
            _weapons[index] = _weapons[index] with { IsEquipped = isEquipped };
        }
    }

    /// <summary>Spends the specified number of hit dice and heals the hero by the total rolled amount.</summary>
    public void SpendHitDice(int count, int healingAmount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfNegative(healingAmount);
        HitDiceAvailable = Math.Max(HitDiceAvailable - count, 0);
        Heal(healingAmount);
    }

    /// <summary>Spends the specified amount of mana. Has no effect for non-casters.</summary>
    public void SpendMana(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        if (CurrentMana.HasValue)
            CurrentMana = Math.Max(CurrentMana.Value - amount, 0);
    }

    /// <summary>Reduces the hero's hit points by the specified amount, flooring at zero. Temporary hit points absorb damage first. When reduced to zero the hero enters the dying state.</summary>
    public void TakeDamage(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        if (TempHp > 0)
        {
            var absorbed = Math.Min(TempHp, amount);
            TempHp -= absorbed;
            amount -= absorbed;
        }
        CurrentHp = Math.Max(CurrentHp - amount, 0);
    }

    /// <summary>Overwrites the hero's build attributes (those chosen during character creation), preserving level, subclass, play state, and all collections. Current hit points and mana are clamped to the new maximums.</summary>
    public void UpdateBuild(
        Guid ancestryId,
        Guid? backgroundId,
        HeroCombatStats combatStats,
        HeroClass heroClass,
        int maxHp,
        int? maxMana,
        string name,
        ClassResources resources,
        HeroSaves saves,
        HeroSkills skills,
        HeroStats stats)
    {
        AncestryId = ancestryId;
        BackgroundId = backgroundId;
        Class = heroClass;
        CombatStats = combatStats;
        MaxHp = maxHp;
        CurrentHp = Math.Min(CurrentHp, maxHp);
        MaxMana = maxMana;
        CurrentMana = maxMana.HasValue ? Math.Min(CurrentMana ?? maxMana.Value, maxMana.Value) : null;
        Name = name;
        Resources = resources;
        Saves = saves;
        Skills = skills;
        Stats = stats;
    }

    /// <summary>Updates the hero's combat statistics, for example after equipping or removing armor.</summary>
    public void UpdateCombatStats(HeroCombatStats combatStats) => CombatStats = combatStats;
}

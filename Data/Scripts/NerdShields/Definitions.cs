using System;
using System.Collections.Generic;
using VRageMath;
using VisualFunction = System.Func<float, float, float, VRage.MyTuple<float, string>>;
using VisualSetting = VRage.MyTuple<float, string>;

namespace NerdShields.Definitions
{
    public static class ShieldConstants
    {
        public const long MessageHandlerId = 3514216428;
        public enum ClassType : byte
        {
            Main,
            Upgrade,
        }
        /// <summary>
        /// Funky quartic function that has steep falloff everywhere but from 40% to 80% shield HP, hovering around 60% blocked, look at https://www.desmos.com/calculator/agor4exetc for more info
        /// </summary>
        public static Func<float, float> ShieldQuarticDamageCurve => (x) => Math.Min(1, (float)(
                    Math.Pow(x, 4) * 1.6758 +
                    Math.Pow(x, 2) * -3.5269 +
                    x * 2.85101));
        /// <summary>
        /// Starts out only blocking 60% of the damage but the falloff is shallow until 10% shield HP, look at https://www.desmos.com/calculator/agor4exetc for more info
        /// </summary>
        public static Func<float, float> ShieldAltDamageCurve => (x) =>
        {
            if (x > 0.1)
            {
                return x / 9 + 0.488889f;
            }
            else
            {
                return 5 * x;
            }
        };
        /// <summary>
        /// Blocks a percentage of damage equal to the percent of shield HP remaining
        /// </summary>
        public static Func<float, float> ShieldLinearDamageCurve => (x) =>
        {
            return x;
        };
        /// <summary>
        /// Blocks 100% of damage until the shield reaches 0%, then blocks none
        /// </summary>
        public static Func<float, float> ShieldFullBlockDamageCurve => (x) =>
        {
            if (x > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        };
        /// <summary>
        /// Blocks 50% of damage until the shield reaches 0%, then blocks none
        /// </summary>
        public static Func<float, float> ShieldHalfBlockDamageCurve => (x) =>
        {
            if (x > 0)
            {
                return 0.5f;
            }
            else
            {
                return 0;
            }
        };

        /// <summary>
        /// Blocks percentageBlockedAny until the shield reaches 0%, then blocks percentageBlockedAt0.
        /// </summary>
        /// <param name="percentageBlockedAny">When the shield HP is above zero, blocks this amount of damage</param>
        /// <param name="percentageBlockedAt0">When the shield HP is at zero, blocks this amount of damage</param>
        /// <returns></returns>
        public static Func<float, float> ShieldBlockCurve(float percentageBlockedAny, float percentageBlockedAt0)
        {
            return (float x) =>
            {
                if (x > 0)
                {
                    return percentageBlockedAny;
                }
                else
                {
                    return percentageBlockedAt0;
                }
            };
        }

        /// <summary>
        /// Radius has no effect on damage taken from explosions
        /// </summary>
        public static Func<float, float> NoRadiusMultiplier => (x) => 1;
        /// <summary>
        /// Radius has a linear effect (whatever multiplier * the radius) on damage
        /// </summary>
        public static Func<float, float> LinearRadiusMultiplier => (x) => x;
        /// <summary>
        /// Radius has a quadratic effect (whatever multiplier * the radius^2) on damage
        /// </summary>
        public static Func<float, float> AreaRadiusMultiplier => (x) => x * x;
        /// <summary>
        /// Radius has a cubic effect (whatever multiplier * the radius^3) on damage
        /// </summary>
        public static Func<float, float> VolumeRadiusMultiplier => (x) => x * x * x;
        
        /// <summary>
        /// Original particle effect. Increases size based on percentage lost in the hit, and is split into three.
        /// </summary>
        public static VisualFunction DefaultParticleEffect = ThreeWayParticleSplit(
                    minimumShieldHPRequired: 0.01f,
                    lowParticleId: "Shield_HitEffect_Red",
                    lowerAndMiddleSplit: 0.33f,
                    middleParticleId: "Shield_HitEffect_Yellow",
                    middleAndUpperSplit: 0.66f,
                    upperParticleId: "Shield_HitEffect",
                    minParticleSize: 1f,
                    particleSizeMultiplier: 150f,
                    maxParticleSize: 10f
                    );
        /// <summary>
        /// Generates settings for having three different particles, switching between the three based on the current shield HP Percent. The particle's size is based on the percentage lost and the size settings inputted.
        /// </summary>
        /// <param name="minimumShieldHPRequired">Minimum HP Percent required to show any particles.</param>
        /// <param name="lowParticleId">Subtype ID of the particle to use between minimumShieldHPRequired and lowerAndMiddleSplit</param>
        /// <param name="lowerAndMiddleSplit">Cuttoff value between the low and middle particle Id. If the shield HP Percent is less than this, shows the lower one, otherwise shows middle or upper particle effect.</param>
        /// <param name="middleParticleId">Subtype ID of the particle to use between lowerAndMiddleSplit and middleAndUpperSplit</param>
        /// <param name="middleAndUpperSplit">Cuttoff value between the middle and upper particle Id. If the shield HP Percent is less than this, shows the lower or middle one, otherwise shows the upper particle effect.</param>
        /// <param name="upperParticleId">Subtype ID of the particle to use between middleAndUpperSplit and 1 (100%)</param>
        /// <param name="minParticleSize">Minimum particle size.</param>
        /// <param name="particleSizeMultiplier">Amount to multiply the shield Percentage Lost by to get particle size.</param>
        /// <param name="maxParticleSize">Maximum particle size.</param>
        /// <returns>Settings</returns>
        public static VisualFunction ThreeWayParticleSplit(float minimumShieldHPRequired, string lowParticleId, float lowerAndMiddleSplit,
            string middleParticleId, float middleAndUpperSplit, string upperParticleId, float minParticleSize, float particleSizeMultiplier, float maxParticleSize)
        {
            return (float percentageHP, float percentageLost, float damageDone) =>
            {
                string particleId = upperParticleId;
                if (percentageHP <= minimumShieldHPRequired)
                {
                    return new VisualSetting(0, "");
                }
                else if (percentageHP <= lowerAndMiddleSplit)
                {
                    particleId = lowParticleId;
                }
                else if (percentageHP <= middleAndUpperSplit)
                {
                    particleId = middleParticleId;
                }

                return new VisualSetting(MathHelper.Clamp(percentageLost * particleSizeMultiplier, minParticleSize, maxParticleSize), particleId);
            };
        }
        /// <summary>
        /// Generates settings for having two different particles, switching between the two based on the current shield HP Percent. The particle's size is based on the percentage lost and the size settings inputted.
        /// </summary>
        /// <param name="minimumShieldHPRequired">Minimum HP Percent required to show any particles.</param>
        /// <param name="lowerParticleId">SubtypeID of the particle to use when shield HP is between minimumShieldHPRequired and cutoffValue.</param>
        /// <param name="upperParticleId">SubtypeID of the particle to use when shield HP is between cutoffValue and 1 (100%).</param>
        /// <param name="cutoffValue"></param>
        /// <param name="minParticleSize">Minimum particle size.</param>
        /// <param name="particleSizeMultiplier">Amount to multiply the shield Percentage Lost by to get particle size.</param>
        /// <param name="maxParticleSize">Maximum particle size.</param>
        /// <returns>Settings</returns>
        public static VisualFunction TwoWayParticleSplit(float minimumShieldHPRequired, string lowerParticleId, string upperParticleId, float cutoffValue, float minParticleSize, float particleSizeMultiplier, float maxParticleSize)
        {
            return (float percentageHP, float percentageLost, float damageDone) =>
            {
                string particleId = upperParticleId;
                if (percentageHP <= minimumShieldHPRequired)
                {
                    return new VisualSetting(0, "");
                }
                else if (percentageHP <= cutoffValue)
                {
                    particleId = lowerParticleId;
                }

                return new VisualSetting(MathHelper.Clamp(percentageLost * particleSizeMultiplier, minParticleSize, maxParticleSize), particleId);
            };
        }
        /// <summary>
        /// Generates settings for having one shield hit particle. The particle's size is based on the percentage lost and the size settings inputted.
        /// </summary>
        /// <param name="minimumShieldHPRequired">Minimum HP Percent required to show any particles.</param>
        /// <param name="subtypeId">SubtypeID of the particle to use.</param>
        /// <param name="minParticleSize">Minimum particle size.</param>
        /// <param name="particleSizeMultiplier">Amount to multiply the Shield Percentage Lost by to get particle size.</param>
        /// <param name="maxParticleSize">Maximum particle size.</param>
        /// <returns>Settings</returns>
        public static VisualFunction Particle(float minimumShieldHPRequired, string subtypeId, float minParticleSize, float particleSizeMultiplier, float maxParticleSize)
        {
            return (float percentageHP, float percentageLost, float damageDone) =>
            {
                if (percentageHP <= minimumShieldHPRequired)
                {
                    return new VisualSetting(0, "");
                }

                return new VisualSetting(MathHelper.Clamp(percentageLost * particleSizeMultiplier, minParticleSize, maxParticleSize), subtypeId);
            };
        }
        /// <summary>
        /// "Original Sound" (No sound).
        /// </summary>
        public static VisualFunction NoSound = Sound(
            minimumShieldHPRequired: 0.01f,
            subtypeId: "",
            minVolume: 1f,
            volumeMultiplier: 1f,
            maxVolume: 1f
            );
        /// <summary>
        /// Generates settings for having one shield hit sound. The sound's volume is based on the percentage lost and the size settings inputted.
        /// </summary>
        /// <param name="minimumShieldHPRequired">Minimum HP Percent required to show any particles.</param>
        /// <param name="subtypeId">SubtypeID of the sound to use.</param>
        /// <param name="minVolume">Minimum volume.</param>
        /// <param name="volumeMultiplier">Amount to multiply the Shield Percentage Lost by to get the sound volume.</param>
        /// <param name="maxVolume">Maximum volume.</param>
        /// <returns>Settings</returns>
        public static VisualFunction Sound(float minimumShieldHPRequired, string subtypeId, float minVolume, float volumeMultiplier, float maxVolume)
        {
            return (float percentageHP, float percentageLost, float damageDone) =>
            {
                if (percentageHP <= minimumShieldHPRequired)
                {
                    return new VisualSetting(0, "");
                }

                return new VisualSetting(MathHelper.Clamp(percentageLost * volumeMultiplier, minVolume, maxVolume), subtypeId);
            };
        }

        /// <summary>
        /// Generates settings for having one shield hit sound for small hits and one for large ones. The sound's volume is based on the percentage lost and the size settings inputted.
        /// </summary>
        /// <param name="minimumShieldHPRequired">Minimum HP Percent required to show any particles.</param>
        /// <param name="smallHitSubtypeId">SubtypeID of the sound to use for hits under largeDamageThreshold.</param>
        /// <param name="largeHitSubtypeId">SubtypeID of the sound to use for hits over largeDamageThreshold.</param>
        /// <param name="largeDamageThreshold">Damage threshold for switching between the two hit sonds.</param>
        /// <param name="minVolume">Minimum volume.</param>
        /// <param name="volumeMultiplier">Amount to multiple the Shield Percentage Lost by to get the particle size.</param>
        /// <param name="maxVolume">Maximum volume.</param>
        /// <returns>Settings</returns>
        public static VisualFunction LargeHitSound(float minimumShieldHPRequired, string smallHitSubtypeId, string largeHitSubtypeId, float largeDamageThreshold, float minVolume, float volumeMultiplier, float maxVolume)
        {
            return (float percentageHP, float percentageLost, float damageDone) =>
            {
                if (percentageHP <= minimumShieldHPRequired)
                {
                    return new VisualSetting(0, "");
                }

                return new VisualSetting(MathHelper.Clamp(percentageLost * volumeMultiplier, minVolume, maxVolume), damageDone > largeDamageThreshold ? largeHitSubtypeId : smallHitSubtypeId);
            };
        }
    }
    public abstract class ShieldBlockDefinition
    {
        /// <summary>
        /// Subtype ID of the block you want to turn into a shield. It must be a functional block, although sorters are recommended. Sorters will have their terminal controls removed.
        /// </summary>
        public string SubtypeId;
        /// <summary>
        /// If there are multiple definitions with the same subtype ID (like say someone is adjusting stats of another's shield mod), then the definition with the highest priority will be loaded.
        /// <para>For people making their own shield mod, its recommended to leave this at zero.</para>
        /// <para>For people MODIFYING other people's mod, its recommended to set this at anything greater than zero.</para>
        /// <para>This effectively allows mod adjuster-like behavior without relying on mod load order, although the entire definition must be copied for it to work properly. Those modifying shield stats can just have the shield definitions in their place w/o copying any models, sbc files, or sounds to the modified mod.</para>
        /// <para>
        /// Units: Unitless
        /// </para>
        /// <para>
        /// Requirements: <c>Value is an integer</c>
        /// </para>
        /// </summary>
        public int DefinitionPriority;

        /// <summary>
        /// If not zero, overrides the block's power requirement to this value in MW.
        /// <para>
        /// Units: MW
        /// </para>
        /// <para>
        /// Requirements: <c>Value is greater than or equal to 0</c>
        /// </para>
        /// </summary>
        public float PowerRequirementOverride;
        public abstract object[] ConvertToObjectArray();
    }
    /// <summary>
    /// Main Shield Block definition. Main shield blocks are limited to 1 per grid and are required for the grid to have any shields. Their stats are modifyable through upgrade blocks.
    /// </summary>
    public class MainShieldBlockDefinition : ShieldBlockDefinition
    {
        /// <summary>
        /// Shield HP the main shield block gives to the grid shield.
        /// <para>
        /// Units: Proprietary Shield Hitpoint™
        /// </para>
        /// <para>
        /// Requirements: <c>Value is greater than zero</c>
        /// </para>
        /// </summary>
        public float DefaultHP;
        /// <summary>
        /// Shield regen per second the main shield block gives
        /// <para>
        /// Units: Proprietary Shield Hitpoint™/s
        /// </para>
        /// <para>
        /// Requirements: <c>Value is greater than or equal to zero</c>
        /// </para>
        /// </summary>
        public float DefaultHPRegenPerSec;

        /// <summary>
        /// Ticks after recieving a hit will the shield start regenning (cooldown on regen efectively)
        /// <para>
        /// Units: Ticks
        /// </para>
        /// <para>
        /// Requirements: <c>Value is greater than or equal to zero, integer</c>
        /// </para>
        /// </summary>
        public int TicksUntilRegen;

        /// <summary>
        /// Default resistance value for unknown damage resistances. Shield will take Xx damage from whatever damage it blocks with the given damage type. This is a shield only damage multiplier. This will not affect how much damage is mitigated, just how much damage the shield takes off of the mitigated damage.
        /// <para>
        /// Units: Unitless
        /// </para>
        /// <para>
        /// Requirements: <c>Value is greater than or equal to zero</c>
        /// </para>
        /// </summary>
        public float DefaultDamageResistance = 1;
        /// <summary>
        /// Shield will take Xx damage from whatever damage it blocks with the given damage type. This is a shield only damage multiplier. This will not affect how much damage is mitigated, just how much damage the shield takes off of the mitigated damage.
        /// <para>
        /// Units: N/A
        /// </para>
        /// <para>
        /// Requirements: <c>Values are greater than or equal to zero, follows format ["&lt;resistance name&gt;"] = &lt;multiplier&gt;,</c>
        /// </para>
        /// </summary>
        public Dictionary<string, float> DamageResistances;
        /// <summary>
        /// Shield will let through Xx damage through the shield from the given damage type. 1f means it will let all damage through, 0 means none, 0.5f means 50% of damage will be blocked and 50% will pass through. Multiplicative with DefaultDamageCurve - if the damage curve will only block 50% and the damage type has a 50% pass through, the shield will let 75% through and take 25%
        /// <para>
        /// Units: N/A
        /// </para>
        /// <para>
        /// Requirements: <c>Values are greater than or equal to zero, follows format ["&lt;resistance name&gt;"] = &lt;multiplier&gt;,</c>
        /// </para>
        /// </summary>
        public Dictionary<string, float> DamagePassthroughs;

        /// <summary>
        /// Function that takes in shield percent (0-1) and returns percentage (0-1) of damage to block. Effectively allows any type of bleedthrough.
        /// Defaults:
        /// <list type="table">
        /// <item>desmos: https://www.desmos.com/calculator/agor4exetc</item>
        /// <item><c>ShieldConstants.ShieldLinearDamageCurve</c> - blocks a percentage of damage equal to the percent of shield HP remaining</item>
        /// <item><c>ShieldConstants.ShieldFullBlockDamageCurve</c> - blocks 100% of damage until the shield reaches 0%, then blocks none</item>
        /// <item><c>ShieldConstants.ShieldAltDamageCurve</c> - starts out only blocking 60% of the damage but the falloff is shallow until 10% shield HP, look at desmos for more info</item>
        /// <item><c>ShieldConstants.ShieldQuarticDamageCurve</c> - funky quartic function that has steep falloff everywhere but from 40% to 80% shield HP, hovering around 60% blocked, look at desmos for more info</item>
        /// </list>
        /// <para>
        /// Units: Shield Percent --> Passthrough Percent
        /// </para>
        /// <para>
        /// Requirements: <c>Use a default, or a custom C# Func&lt;float, float&gt;</c>
        /// </para>
        /// </summary>
        public Func<float, float> DefaultDamageCurve;

        /// <summary>
        /// Function that takes in an explosion's radius and returns a damage multiplier for the explosion's actual damage. Effectively allows any type of multiplier based on an explosion radius.
        /// Defaults:
        /// <list type="table">
        /// <item><c>ShieldConstants.NoRadiusMultiplier</c> - 1</item>
        /// <item><c>ShieldConstants.LinearRadiusMultiplier</c> - r</item>
        /// <item><c>ShieldConstants.ShieldLinearDamageCurve</c> - r^2</item>
        /// <item><c>ShieldConstants.ShieldFullBlockDamageCurve</c> - r^3</item>
        /// </list>
        /// <para>
        /// Units: Meters --> Unitless Multiplier
        /// </para>
        /// <para>
        /// Requirements: <c>Use a default, or a custom C# Func&lt;float, float&gt;</c>
        /// </para>
        /// </summary>
        public Func<float, float> ExplosionRadiusMultiplierCurve;

        /// <summary>
        /// Function that takes in shield percent (0-1), percent lost in the hit (0-1), and damage done and returns a particle's subtype as well as the particles size. Useful for having the hit particle change dynamically based on shield stats.
        /// Defaults:
        /// <list type="table">
        /// <item><c>ShieldConstants.DefaultParticleEffect</c> - Original particle effect.</item>
        /// </list>
        /// <para>
        /// Units: Shield Percent (%), Percent Lost (%), Damage Done (Proprietary Shield Hitpoint™) --> Particle Size (Keen Size Unit™) & Subtype ID (unitless)
        /// </para>
        /// <para>
        /// Requirements: <c>Use a default, function generator, or a custom C# Func&lt;float, float, float, &lt;float, string&gt;&gt;</c>
        /// </para>
        /// </summary>
        public VisualFunction CustomParticle;

        /// <summary>
        /// Function that takes in shield percent (0-1), percent lost in the hit (0-1), and damage done and returns a sound's subtype as well as the sound's volume. Useful for having the hit sound change dynamically based on shield stats.
        /// Defaults:
        /// <list type="table">
        /// <item><c>ShieldConstants.NoSound</c> - No sound.</item>
        /// </list>
        /// <para>
        /// Units: Shield Percent (%), Percent Lost (%), Damage Done (Proprietary Shield Hitpoint™) --> Volume (Keen Volume Unit™ (multiplier? not sure)) & Subtype ID (unitless)
        /// </para>
        /// <para>
        /// Requirements: <c>Use a default, function generator, or a custom C# Func&lt;float, float, float, &lt;float, string&gt;&gt;</c>
        /// </para>
        /// </summary>
        public VisualFunction CustomSoundEffect;

        public MainShieldBlockDefinition()
        {
        }

        public override object[] ConvertToObjectArray()
        {
            return new object[]
            {
                ShieldConstants.ClassType.Main,
                SubtypeId,
                DefaultHP,
                DefaultHPRegenPerSec,
                TicksUntilRegen,
                DefaultDamageResistance,
                DamageResistances,
                DamagePassthroughs,
                DefaultDamageCurve,
                ExplosionRadiusMultiplierCurve,
                DefinitionPriority,
                PowerRequirementOverride,
                CustomParticle,
                CustomSoundEffect,
            };
        }

        public static MainShieldBlockDefinition ConvertFromObjectArray(object[] array)
        {
            return new MainShieldBlockDefinition
            {
                SubtypeId = array.Length <= 1 ? "" : (string)array[1],
                DefaultHP = array.Length <= 2 ? 0 : (float)array[2],
                DefaultHPRegenPerSec = array.Length <= 3 ? 0 : (float)array[3],
                TicksUntilRegen = array.Length <= 4 ? 0 : (int)array[4],
                DefaultDamageResistance = array.Length <= 5 ? 1 : (float)array[5],
                DamageResistances = array.Length <= 6 ? null : (Dictionary<string, float>)array[6],
                DamagePassthroughs = array.Length <= 7 ? null : (Dictionary<string, float>)array[7],
                DefaultDamageCurve = array.Length <= 8 ? null : (Func<float, float>)array[8],
                ExplosionRadiusMultiplierCurve = array.Length <= 9 ? null : (Func<float, float>)array[9],
                DefinitionPriority = array.Length <= 10 ? 0 : (int)array[10],
                PowerRequirementOverride = array.Length <= 11 ? 0 : (float)array[11],
                CustomParticle = array.Length <= 12 ? null : (VisualFunction)array[12],
                CustomSoundEffect = array.Length <= 13 ? null : (VisualFunction)array[13],
            };
        }
    }
    /// <summary>
    /// Shield upgrade block definition. Upgrade shield blocks modify a main shield block on grid. They do NOT create shields on their own, and can be placed anywhere on the same grid.
    /// </summary>
    public class ShieldUpgradeBlockDefinition : ShieldBlockDefinition
    {
        /// <summary>
        /// Shield HP the upgrade block gives to the grid shield. Negative values subtract.
        /// <para>
        /// Units: Proprietary Shield Hitpoint™
        /// </para>
        /// <para>
        /// Requirements: <c>Value is a real number.</c>
        /// </para>
        /// </summary>
        public float AddedHP;
        /// <summary>
        /// Shield HP regen per second the upgrade block gives to the grid shield. Negative values subtract.
        /// <para>
        /// Units: Proprietary Shield Hitpoint™/s
        /// </para>
        /// <para>
        /// Requirements: <c>Value is a real number.</c>
        /// </para>
        /// </summary>
        public float AddedHPRegenPerSec;

        /// <summary>
        /// Additional ticks after recieving a hit will the shield start regenning (cooldown on regen efectively). Negative values subtract.
        /// <para>
        /// Units: Ticks
        /// </para>
        /// <para>
        /// Requirements: <c>Value is an integer.</c>
        /// </para>
        /// </summary>
        public int AddedTicksUntilRegen;

        /// <summary>
        /// Toggle on whether the effects of the shield block can be toggled by enabling and disabling the block. This will not mean the block is unable to be disabled, but that the effect persists when off.
        /// This only matters for the terminal on/off switch though, if the block is nonfunctional its effects don't perist
        /// <para>
        /// Units: N/A
        /// </para>
        /// <para>
        /// Requirements: <c>Value is true or false.</c>
        /// </para>
        /// </summary>
        public bool CanBeTurnedOff;

        /// <summary>
        /// Function that takes in shield percent (0-1) and returns percentage (0-1) of damage to block. Effectively allows any type of bleedthrough. Replaces the main shield block's damage curve.
        /// Defaults:
        /// <list type="table">
        /// <item>desmos: https://www.desmos.com/calculator/agor4exetc</item>
        /// <item><c>ShieldConstants.ShieldLinearDamageCurve</c> - blocks a percentage of damage equal to the percent of shield HP remaining</item>
        /// <item><c>ShieldConstants.ShieldFullBlockDamageCurve</c> - blocks 100% of damage until the shield reaches 0%, then blocks none</item>
        /// <item><c>ShieldConstants.ShieldAltDamageCurve</c> - starts out only blocking 60% of the damage but the falloff is shallow until 10% shield HP, look at desmos for more info</item>
        /// <item><c>ShieldConstants.ShieldQuarticDamageCurve</c> - funky quartic function that has steep falloff everywhere but from 40% to 80% shield HP, hovering around 60% blocked, look at desmos for more info</item>
        /// </list>
        /// <para>
        /// Units: Shield Percent --> Passthrough Percent
        /// </para>
        /// <para>
        /// Requirements: <c>Either null, a default, or a custom C# Func&lt;float, float&gt;</c>
        /// </para>
        /// </summary>
        public Func<float, float> ReplacementDamageCurve;

        public ShieldUpgradeBlockDefinition()
        {
        }

        public override object[] ConvertToObjectArray()
        {
            return new object[]
            {
                ShieldConstants.ClassType.Upgrade,
                SubtypeId,
                AddedHP,
                AddedHPRegenPerSec,
                AddedTicksUntilRegen,
                CanBeTurnedOff,
                ReplacementDamageCurve,
                DefinitionPriority,
                PowerRequirementOverride,
            };
        }

        public static ShieldUpgradeBlockDefinition ConvertFromObjectArray(object[] array)
        {
            return new ShieldUpgradeBlockDefinition
            {
                SubtypeId = array.Length <= 1 ? "" : (string)array[1],
                AddedHP = array.Length <= 1 ? 0 : (float)array[2],
                AddedHPRegenPerSec = array.Length <= 1 ? 0 : (float)array[3],
                AddedTicksUntilRegen = array.Length <= 1 ? 0 : (int)array[4],
                CanBeTurnedOff = array.Length <= 1 ? false : (bool)array[5],
                ReplacementDamageCurve = array.Length <= 1 ? null : (Func<float, float>)array[6],
                DefinitionPriority = array.Length <= 1 ? 0 : (int)array[7],
                PowerRequirementOverride = array.Length <= 1 ? 0 : (float)array[8],
            };
        }
    }
}

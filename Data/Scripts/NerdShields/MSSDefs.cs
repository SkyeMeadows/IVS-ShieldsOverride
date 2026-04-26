using NerdShields.Definitions;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;

namespace ShieldDefTemplate
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MSSDefs : MySessionComponentBase
    {
        // do not modify above here barring the class name (in this case, "MSSDefs"), recommend to make it the same as the file name
        List<ShieldBlockDefinition> Definitions => new List<ShieldBlockDefinition>()
        {
            // Main shield blocks are limited to 1 per grid
            new MainShieldBlockDefinition()
            {
                SubtypeId = "NerdMainShieldBlock", // subtype ID of the block you want to turn into a shield. It must be a functional block, although sorters are recommended. Sorters will have their terminal controls removed.
                
                // If there are multiple definitions with the same subtype ID (like say someone is adjusting stats of another's shield mod), then the definition with the highest priority will be loaded.
                // For people making their own shield mod, its recommended to leave this at zero.
                // For people MODIFYING other people's mod, its recommended to set this at anything greater than zero.
                // This effectively allows mod adjuster-like behavior without relying on mod load order, although the entire definition must be copied for it to work properly.
                //  - those modifying shield stats can just have the shield definitions in their place w/o copying any models, sbc files, or sounds to the modified mod.
                DefinitionPriority = 1,

                DefaultHP = 200000,  // shield HP the main shield block gives
                DefaultHPRegenPerSec = 2000, // shield regen per second the main shield block gives
                TicksUntilRegen = 30 * 60, // ticks after recieving a hit will the shield start regenning (cooldown on regen efectively)

                // function that takes in shield percent (0-1) and returns percentage (0-1) of damage to block
                // effectively allows any type of bleedthrough
                // comes with some defaults - visualized here https://www.desmos.com/calculator/agor4exetc : 
                //    ShieldConstants.ShieldLinearDamageCurve    - blocks a percentage of damage equal to the percent of shield HP remaining
                //    ShieldConstants.ShieldHalfBlockDamageCurve - blocks 50% of damage until the shield reaches 0%, then blocks none
                //    ShieldConstants.ShieldFullBlockDamageCurve - blocks 100% of damage until the shield reaches 0%, then blocks none
                //    ShieldConstants.ShieldAltDamageCurve       - starts out only blocking 60% of the damage but the falloff is shallow until 10% shield HP, look at desmos for more info
                //    ShieldConstants.ShieldQuarticDamageCurve   - funky quartic function that has steep falloff everywhere but from 40% to 80% shield HP, hovering around 60% blocked, look at desmos for more info
                //
                // for the people who know C#, the curve is a Func<float, float> that you can set to whatever lamda function you want
                DefaultDamageCurve = ShieldConstants.ShieldQuarticDamageCurve,
                DefaultDamageResistance = 1, // default resistance for unknown damage resistances
                // shield will take Xx damage from whatever damage it blocks with the given damage type. This is a shield only damage multiplier.
                // This will not affect how much damage is mitigated, just how much damage the shield takes off of the mitigated damage.
                // Multiplier of 0 = shield takes no damage
                DamageResistances = new Dictionary<string, float>()
                {
                    ["Bullet"] = 1f, // pen/gatling weapons
                    ["Explosion"] = 1f, // explosive weapons
                    ["WeaponLaser"] = 1f, // V+ default beam damage
                    ["Deformation"] = 0, // deformation damage, inherent to all damage types

                    ["Thruster"] = 1f, // Thruster damage

                    ["Kinetic"] = 1f, // Weaponcore Kinetic damage
                    ["Energy"] = 1f, // Weaponcore Energy damage
                },
                // shield will let through Xx damage through the shield from the given damage type.
                // 1f means it will let all damage through, 0 means none, 0.5f means 50% of damage will be blocked and 50% will pass through
                // multiplicative with DefaultDamageCurve - if the damage curve will only block 50% and the damage type has a 50% pass through, the shield will let 75% through and take 25%
                DamagePassthroughs = new Dictionary<string, float>()
                {
                    ["IgnoreShields"] = 1f, // special IgnoreShields passthrough
                    ["Grind"] = 1f, // grinder damage, recommend leaving this at 1f so people can modify their own grids
                },

                // function that takes in an explosion's radius and returns a damage multiplier for the explosion's actual damage.
                // effectively allows any type of multiplier based on an explosion radius
                // comes with some defaults:
                //    ShieldConstants.NoRadiusMultiplier         - radius has no effect on damage taken from explosions
                //    ShieldConstants.LinearRadiusMultiplier     - radius has a linear effect (whatever multiplier * the radius) on damage
                //    ShieldConstants.ShieldLinearDamageCurve    - radius has a quadratic effect (whatever multiplier * the radius^2) on damage
                //    ShieldConstants.ShieldFullBlockDamageCurve - radius has a cubic effect (whatever multiplier * the radius^3) on damage
                //
                // for the people who know C#, the curve is a Func<float, float> that you can set to whatever lamda function you want
                ExplosionRadiusMultiplierCurve = ShieldConstants.NoRadiusMultiplier,

                // If not zero, overrides the block's power requirement to this value. Units are MW
                PowerRequirementOverride = 100f,

                // Function that takes in shield percent (0-1), percent lost in the hit (0-1), and damage done and returns a particle's subtype as well as the particles size. Useful for having the hit particle change dynamically based on shield stats.
                // For those of you have zero idea what was just said, uncomment ONE of the examples below at a time which have some presets with settings, and mess around with it.

                // Original particle effect. Changes color in 3 parts (blue, yellow, red) based on the shield's HP and size based on how much damage was dealt in that hit.
                // CustomParticle = ShieldConstants.DefaultParticleEffect,

                // This is also the original particle effect, just with its settings exposed.
                
                CustomParticle = ShieldConstants.ThreeWayParticleSplit(
                    minimumShieldHPRequired: 0.01f, // Minimum Shield HP Percentage required to show any particle
                    lowParticleId: "Shield_HitEffect_Red", // Particle to use when the shield HP is low (between min and lowerAndMiddleSplit)
                    lowerAndMiddleSplit: 0.33f, // Shield HP Percentage to switch from the low to middle particle
                    middleParticleId: "Shield_HitEffect_Yellow", // Particle to use when the shield HP is in the middle (between lowerAndMiddleSplit and middleAndUpperSplit)
                    middleAndUpperSplit: 0.66f, // Shield HP Percentage to switch from the middle to upper particle
                    upperParticleId: "Shield_HitEffect", // Particle to use when the shield HP is high (between middleAndUpperSplit and 1 (100%))
                    minParticleSize: 1f, // Minimum particle size
                    particleSizeMultiplier: 100f, // Amount to multiply the shield Percentage Lost by to get particle size.
                    maxParticleSize: 50f // Maximum particle size
                    ),

                // This preset only has two particles to switch between

                /*delete this line to uncomment the below section
                CustomParticle = ShieldConstants.TwoWayParticleSplit(
                    minimumShieldHPRequired: 0.01f, // Minimum Shield HP Percentage required to show any particle
                    lowerParticleId: "Shield_HitEffect_Red", // Particle to use when the shield HP is low (between min and cutoffValue)
                    upperParticleId: "Shield_HitEffect",// Particle to use when the shield HP is high (between cutoffValue and 1 (100%))
                    cutoffValue: 0.5f, // Shield HP Percentatge to switch from lower and upper particles.
                    minParticleSize: 1f, // Minimum particle size
                    particleSizeMultiplier: 100f, // Amount to multiply the shield Percentage Lost by to get particle size.
                    maxParticleSize: 50f // Maximum particle size
                    ),
                delete this line to uncomment the above section*/

                // This preset only has one particle to use at any time

                /*delete this line to uncomment this section
                CustomParticle = ShieldConstants.OneParticle(
                    minimumShieldHPRequired: 0.01f, // Minimum Shield HP Percentage required to show any particle
                    subtypeId: "Shield_HitEffect_Red", // Particle to use when the shield is hit.
                    minParticleSize: 1f, // Minimum particle size
                    particleSizeMultiplier: 100f, // Amount to multiply the shield Percentage Lost by to get particle size.
                    maxParticleSize: 50f // Maximum particle size
                    ),
                delete this line to uncomment the above section*/



                // Function that takes in shield percent (0-1), percent lost in the hit (0-1), and damage done and returns a sound's subtype as well as the sound's volume. Useful for having the hit sound change dynamically based on shield stats.
                // For those of you have zero idea what was just said, uncomment ONE of the examples below at a time which have some presets with settings, and mess around with it.

                // Original particle effect - Plays no sound.
                CustomSoundEffect = ShieldConstants.NoSound,

                // Generates settings for having one shield hit sound. The sound's volume is based on the percentage lost and the size settings inputted.

                /*delete this line to uncomment this section
                CustomSoundEffect = ShieldConstants.Sound(
                    minimumShieldHPRequired: 0.01f, // Minimum Shield HP Percentage required to play any sound
                    subtypeId: "MusFun", // Subtype ID of the sound to play
                    minVolume: 0.1f, // minimum volume to play the sound at.
                    volumeMultiplier: 0.1f, // Amount to multiply the shield Percentage Lost by to get sound volume.
                    maxVolume: 0.1f  // Maximum volume to play the sound at.
                    ),
                delete this line to uncomment the above section*/

                // Generates settings for having one shield hit sound for small hits and one for large ones. The sound's volume is based on the percentage lost and the size settings inputted.

                 /*delete this line to uncomment this section
                CustomSoundEffect = ShieldConstants.LargeHitSound(
                    minimumShieldHPRequired: 0.01f, // Minimum Shield HP Percentage required to play any sound
                    smallHitSubtypeId: "MusCalm_05", // SubtypeID of the sound to use for hits under largeDamageThreshold
                    largeHitSubtypeId: "MusFun", // SubtypeID of the sound to use for hits over largeDamageThreshold
                    largeDamageThreshold: 5000f, // Damage threshold for switching between the two hit sonds
                    minVolume: 0.1f, // minimum volume to play the sound at.
                    volumeMultiplier: 0.1f, // Amount to multiply the shield Percentage Lost by to get sound volume.
                    maxVolume: 0.1f  // Maximum volume to play the sound at.
                    ),
                delete this line to uncomment the above section*/
            },
            new MainShieldBlockDefinition()
            {
                SubtypeId = "NerdMainShieldBlock_Admin",// subtype ID of the block you want to turn into a shield. It must be a functional block, although sorters are recommended. Sorters will have their terminal controls removed.
                
                // If there are multiple definitions with the same subtype ID (like say someone is adjusting stats of another's shield mod), then the definition with the highest priority will be loaded.
                // For people making their own shield mod, its recommended to leave this at zero.
                // For people MODIFYING other people's mod, its recommended to set this at anything greater than zero.
                // This effectively allows mod adjuster-like behavior without relying on mod load order, although the entire definition must be copied for it to work properly.
                //  - those modifying shield stats can just have the shield definitions in their place w/o copying any models, sbc files, or sounds to the modified mod.
                DefinitionPriority = 1,

                DefaultHP = 100000, // shield HP the main shield block gives
                DefaultHPRegenPerSec = 1000, // shield regen per second the main shield block gives
                TicksUntilRegen = 10 * 60,// ticks after recieving a hit will the shield start regenning (cooldown on regen efectively)

                // function that takes in shield percent (0-1) and returns percentage (0-1) of damage to block
                // effectively allows any type of bleedthrough
                // comes with some defaults - visualized here https://www.desmos.com/calculator/agor4exetc : 
                //    ShieldConstants.ShieldLinearDamageCurve    - blocks a percentage of damage equal to the percent of shield HP remaining
                //    ShieldConstants.ShieldHalfBlockDamageCurve - blocks 50% of damage until the shield reaches 0%, then blocks none
                //    ShieldConstants.ShieldFullBlockDamageCurve - blocks 100% of damage until the shield reaches 0%, then blocks none
                //    ShieldConstants.ShieldAltDamageCurve       - starts out only blocking 60% of the damage but the falloff is shallow until 10% shield HP, look at desmos for more info
                //    ShieldConstants.ShieldQuarticDamageCurve   - funky quartic function that has steep falloff everywhere but from 40% to 80% shield HP, hovering around 60% blocked, look at desmos for more info
                //
                // for the people who know C#, the curve is a Func<float, float> that you can set to whatever lamda function you want
                DefaultDamageCurve = ShieldConstants.ShieldFullBlockDamageCurve,
                DefaultDamageResistance = 0.1f, // default resistance for unknown damage resistances
                // shield will take Xx damage from whatever damage it blocks with the given damage type. This is a shield only damage multiplier.
                // This will not affect how much damage is mitigated, just how much damage the shield takes off of the mitigated damage.
                // Multiplier of 0 = shield takes no damage from what it blocks
                DamageResistances = new Dictionary<string, float>()
                {
                    ["Bullet"] = 0.40f / 10f, // pen/gatling weapons
                    ["Explosion"] = 2f / 10f, // explosive weapons
                    ["WeaponLaser"] = 0.4f / 10f, // V+ default beam damage
                    ["Deformation"] = 0 / 10f, // deformation damage, inherent to all damage types

                    ["Thruster"] = 1f / 10f, // Thruster damage

                    ["Kinetic"] = 1f / 10f, // Weaponcore Kinetic damage
                    ["Energy"] = 1f / 10f, // Weaponcore Energy damage
                },
                // shield will let through Xx damage through the shield from the given damage type.
                // 1f means it will let all damage through, 0 means none, 0.5f means 50% of damage will be blocked and 50% will pass through
                // multiplicative with DefaultDamageCurve - if the damage curve will only block 50% and the damage type has a 50% pass through, the shield will let 75% through and take 25%
                DamagePassthroughs = new Dictionary<string, float>()
                {
                    ["IgnoreShields"] = 1f, // special IgnoreShields passthrough
                    ["Grind"] = 1f, // grinder damage, recommend leaving this at 1f so people can modify their own grids
                },

                // function that takes in an explosion's radius and returns a damage multiplier for the explosion's actual damage.
                // effectively allows any type of multiplier based on an explosion radius
                // comes with some defaults:
                //    ShieldConstants.NoRadiusMultiplier         - radius has no effect on damage taken from explosions
                //    ShieldConstants.LinearRadiusMultiplier     - radius has a linear effect (whatever multiplier * the radius) on damage
                //    ShieldConstants.ShieldLinearDamageCurve    - radius has a quadratic effect (whatever multiplier * the radius^2) on damage
                //    ShieldConstants.ShieldFullBlockDamageCurve - radius has a cubic effect (whatever multiplier * the radius^3) on damage
                //
                // for the people who know C#, the curve is a Func<float, float> that you can set to whatever lamda function you want
                ExplosionRadiusMultiplierCurve = ShieldConstants.NoRadiusMultiplier,

                // If not zero, overrides the block's power requirement to this value. Units are MW
                PowerRequirementOverride = 0f,

                // Function that takes in shield percent (0-1), percent lost in the hit (0-1), and damage done and returns a particle's subtype as well as the particles size. Useful for having the hit particle change dynamically based on shield stats.
                // For those of you have zero idea what was just said, uncomment ONE of the examples below at a time which have some presets with settings, and mess around with it.

                // Original particle effect. Changes color in 3 parts (blue, yellow, red) based on the shield's HP and size based on how much damage was dealt in that hit.
                // CustomParticle = ShieldConstants.DefaultParticleEffect,

                // This is also the original particle effect, just with its settings exposed.
                
                CustomParticle = ShieldConstants.ThreeWayParticleSplit(
                    minimumShieldHPRequired: 0.01f, // Minimum Shield HP Percentage required to show any particle
                    lowParticleId: "Shield_HitEffect_Red", // Particle to use when the shield HP is low (between min and lowerAndMiddleSplit)
                    lowerAndMiddleSplit: 0.33f, // Shield HP Percentage to switch from the low to middle particle
                    middleParticleId: "Shield_HitEffect_Yellow", // Particle to use when the shield HP is in the middle (between lowerAndMiddleSplit and middleAndUpperSplit)
                    middleAndUpperSplit: 0.66f, // Shield HP Percentage to switch from the middle to upper particle
                    upperParticleId: "Shield_HitEffect", // Particle to use when the shield HP is high (between middleAndUpperSplit and 1 (100%))
                    minParticleSize: 1f, // Minimum particle size
                    particleSizeMultiplier: 100f, // Amount to multiply the shield Percentage Lost by to get particle size.
                    maxParticleSize: 50f // Maximum particle size
                    ),

                // This preset only has two particles to switch between

                /*delete this line to uncomment the below section
                CustomParticle = ShieldConstants.TwoWayParticleSplit(
                    minimumShieldHPRequired: 0.01f, // Minimum Shield HP Percentage required to show any particle
                    lowerParticleId: "Shield_HitEffect_Red", // Particle to use when the shield HP is low (between min and cutoffValue)
                    upperParticleId: "Shield_HitEffect",// Particle to use when the shield HP is high (between cutoffValue and 1 (100%))
                    cutoffValue: 0.5f, // Shield HP Percentatge to switch from lower and upper particles.
                    minParticleSize: 1f, // Minimum particle size
                    particleSizeMultiplier: 100f, // Amount to multiply the shield Percentage Lost by to get particle size.
                    maxParticleSize: 50f // Maximum particle size
                    ),
                delete this line to uncomment the above section*/

                // This preset only has one particle to use at any time

                /*delete this line to uncomment this section
                CustomParticle = ShieldConstants.OneParticle(
                    minimumShieldHPRequired: 0.01f, // Minimum Shield HP Percentage required to show any particle
                    subtypeId: "Shield_HitEffect_Red", // Particle to use when the shield is hit.
                    minParticleSize: 1f, // Minimum particle size
                    particleSizeMultiplier: 100f, // Amount to multiply the shield Percentage Lost by to get particle size.
                    maxParticleSize: 50f // Maximum particle size
                    ),
                delete this line to uncomment the above section*/



                // Function that takes in shield percent (0-1), percent lost in the hit (0-1), and damage done and returns a sound's subtype as well as the sound's volume. Useful for having the hit sound change dynamically based on shield stats.
                // For those of you have zero idea what was just said, uncomment ONE of the examples below at a time which have some presets with settings, and mess around with it.

                // Original particle effect - Plays no sound.
                CustomSoundEffect = ShieldConstants.NoSound,

                // Generates settings for having one shield hit sound. The sound's volume is based on the percentage lost and the size settings inputted.

                /*delete this line to uncomment this section
                CustomSoundEffect = ShieldConstants.Sound(
                    minimumShieldHPRequired: 0.01f, // Minimum Shield HP Percentage required to play any sound
                    subtypeId: "MusFun", // Subtype ID of the sound to play
                    minVolume: 0.1f, // minimum volume to play the sound at.
                    volumeMultiplier: 0.1f, // Amount to multiply the shield Percentage Lost by to get sound volume.
                    maxVolume: 0.1f  // Maximum volume to play the sound at.
                    ),
                delete this line to uncomment the above section*/

                // Generates settings for having one shield hit sound for small hits and one for large ones. The sound's volume is based on the percentage lost and the size settings inputted.

                /*delete this line to uncomment this section
                CustomSoundEffect = ShieldConstants.LargeHitSound(
                    minimumShieldHPRequired: 0.01f, // Minimum Shield HP Percentage required to play any sound
                    smallHitSubtypeId: "MusCalm_05", // SubtypeID of the sound to use for hits under largeDamageThreshold
                    largeHitSubtypeId: "MusFun", // SubtypeID of the sound to use for hits over largeDamageThreshold
                    largeDamageThreshold: 5000f, // Damage threshold for switching between the two hit sonds
                    minVolume: 0.1f, // minimum volume to play the sound at.
                    volumeMultiplier: 0.1f, // Amount to multiply the shield Percentage Lost by to get sound volume.
                    maxVolume: 0.1f  // Maximum volume to play the sound at.
                    ),
                delete this line to uncomment the above section*/
            },
            new MainShieldBlockDefinition()
            {
                SubtypeId = "NerdMainShieldBlock_SG", // subtype ID of the block you want to turn into a shield. It must be a functional block, although sorters are recommended. Sorters will have their terminal controls removed.
                
                // If there are multiple definitions with the same subtype ID (like say someone is adjusting stats of another's shield mod), then the definition with the highest priority will be loaded.
                // For people making their own shield mod, its recommended to leave this at zero.
                // For people MODIFYING other people's mod, its recommended to set this at anything greater than zero.
                // This effectively allows mod adjuster-like behavior without relying on mod load order, although the entire definition must be copied for it to work properly.
                //  - those modifying shield stats can just have the shield definitions in their place w/o copying any models, sbc files, or sounds to the modified mod.
                DefinitionPriority = 1,

                DefaultHP = 10000,  // shield HP the main shield block gives
                DefaultHPRegenPerSec = 100, // shield regen per second the main shield block gives
                TicksUntilRegen = 30 * 60, // ticks after recieving a hit will the shield start regenning (cooldown on regen efectively)

                // function that takes in shield percent (0-1) and returns percentage (0-1) of damage to block
                // effectively allows any type of bleedthrough
                // comes with some defaults - visualized here https://www.desmos.com/calculator/agor4exetc : 
                //    ShieldConstants.ShieldLinearDamageCurve    - blocks a percentage of damage equal to the percent of shield HP remaining
                //    ShieldConstants.ShieldHalfBlockDamageCurve - blocks 50% of damage until the shield reaches 0%, then blocks none
                //    ShieldConstants.ShieldFullBlockDamageCurve - blocks 100% of damage until the shield reaches 0%, then blocks none
                //    ShieldConstants.ShieldAltDamageCurve       - starts out only blocking 60% of the damage but the falloff is shallow until 10% shield HP, look at desmos for more info
                //    ShieldConstants.ShieldQuarticDamageCurve   - funky quartic function that has steep falloff everywhere but from 40% to 80% shield HP, hovering around 60% blocked, look at desmos for more info
                //
                // for the people who know C#, the curve is a Func<float, float> that you can set to whatever lamda function you want
                DefaultDamageCurve = ShieldConstants.ShieldQuarticDamageCurve,
                DefaultDamageResistance = 1, // default resistance for unknown damage resistances
                // shield will take Xx damage from whatever damage it blocks with the given damage type. This is a shield only damage multiplier.
                // This will not affect how much damage is mitigated, just how much damage the shield takes off of the mitigated damage.
                // Multiplier of 0 = shield takes no damage from what it blocks
                DamageResistances = new Dictionary<string, float>()
                {
                    ["Bullet"] = 1f, // pen/gatling weapons
                    ["Explosion"] = 1f, // explosive weapons
                    ["WeaponLaser"] = 1f, // V+ default beam damage
                    ["Deformation"] = 0, // deformation damage, inherent to all damage types

                    ["Thruster"] = 1f, // Thruster damage

                    ["Kinetic"] = 1f, // Weaponcore Kinetic damage
                    ["Energy"] = 1f, // Weaponcore Energy damage
                },
                // shield will let through Xx damage through the shield from the given damage type.
                // 1f means it will let all damage through, 0 means none, 0.5f means 50% of damage will be blocked and 50% will pass through
                // multiplicative with DefaultDamageCurve - if the damage curve will only block 50% and the damage type has a 50% pass through, the shield will let 75% through and take 25%
                DamagePassthroughs = new Dictionary<string, float>()
                {
                    ["IgnoreShields"] = 1f, // special IgnoreShields passthrough
                    ["Grind"] = 1f, // grinder damage, recommend leaving this at 1f so people can modify their own grids
                },

                // function that takes in an explosion's radius and returns a damage multiplier for the explosion's actual damage.
                // effectively allows any type of multiplier based on an explosion radius
                // comes with some defaults:
                //    ShieldConstants.NoRadiusMultiplier         - radius has no effect on damage taken from explosions
                //    ShieldConstants.LinearRadiusMultiplier     - radius has a linear effect (whatever multiplier * the radius) on damage
                //    ShieldConstants.ShieldLinearDamageCurve    - radius has a quadratic effect (whatever multiplier * the radius^2) on damage
                //    ShieldConstants.ShieldFullBlockDamageCurve - radius has a cubic effect (whatever multiplier * the radius^3) on damage
                //
                // for the people who know C#, the curve is a Func<float, float> that you can set to whatever lamda function you want
                ExplosionRadiusMultiplierCurve = ShieldConstants.NoRadiusMultiplier,

                // If not zero, overrides the block's power requirement to this value. Units are MW
                PowerRequirementOverride = 20f,


                // Function that takes in shield percent (0-1), percent lost in the hit (0-1), and damage done and returns a particle's subtype as well as the particles size. Useful for having the hit particle change dynamically based on shield stats.
                // For those of you have zero idea what was just said, uncomment ONE of the examples below at a time which have some presets with settings, and mess around with it.

                // Original particle effect. Changes color in 3 parts (blue, yellow, red) based on the shield's HP and size based on how much damage was dealt in that hit.
                // CustomParticle = ShieldConstants.DefaultParticleEffect,

                // This is also the original particle effect, just with its settings exposed.
                
                CustomParticle = ShieldConstants.ThreeWayParticleSplit(
                    minimumShieldHPRequired: 0.01f, // Minimum Shield HP Percentage required to show any particle
                    lowParticleId: "Shield_HitEffect_Red", // Particle to use when the shield HP is low (between min and lowerAndMiddleSplit)
                    lowerAndMiddleSplit: 0.33f, // Shield HP Percentage to switch from the low to middle particle
                    middleParticleId: "Shield_HitEffect_Yellow", // Particle to use when the shield HP is in the middle (between lowerAndMiddleSplit and middleAndUpperSplit)
                    middleAndUpperSplit: 0.66f, // Shield HP Percentage to switch from the middle to upper particle
                    upperParticleId: "Shield_HitEffect", // Particle to use when the shield HP is high (between middleAndUpperSplit and 1 (100%))
                    minParticleSize: 1f, // Minimum particle size
                    particleSizeMultiplier: 100f, // Amount to multiply the shield Percentage Lost by to get particle size.
                    maxParticleSize: 50f // Maximum particle size
                    ),

                // This preset only has two particles to switch between

                /*delete this line to uncomment the below section
                CustomParticle = ShieldConstants.TwoWayParticleSplit(
                    minimumShieldHPRequired: 0.01f, // Minimum Shield HP Percentage required to show any particle
                    lowerParticleId: "Shield_HitEffect_Red", // Particle to use when the shield HP is low (between min and cutoffValue)
                    upperParticleId: "Shield_HitEffect",// Particle to use when the shield HP is high (between cutoffValue and 1 (100%))
                    cutoffValue: 0.5f, // Shield HP Percentatge to switch from lower and upper particles.
                    minParticleSize: 1f, // Minimum particle size
                    particleSizeMultiplier: 100f, // Amount to multiply the shield Percentage Lost by to get particle size.
                    maxParticleSize: 50f // Maximum particle size
                    ),
                delete this line to uncomment the above section*/

                // This preset only has one particle to use at any time

                /*delete this line to uncomment this section
                CustomParticle = ShieldConstants.OneParticle(
                    minimumShieldHPRequired: 0.01f, // Minimum Shield HP Percentage required to show any particle
                    subtypeId: "Shield_HitEffect_Red", // Particle to use when the shield is hit.
                    minParticleSize: 1f, // Minimum particle size
                    particleSizeMultiplier: 100f, // Amount to multiply the shield Percentage Lost by to get particle size.
                    maxParticleSize: 50f // Maximum particle size
                    ),
                delete this line to uncomment the above section*/



                // Function that takes in shield percent (0-1), percent lost in the hit (0-1), and damage done and returns a sound's subtype as well as the sound's volume. Useful for having the hit sound change dynamically based on shield stats.
                // For those of you have zero idea what was just said, uncomment ONE of the examples below at a time which have some presets with settings, and mess around with it.

                // Original particle effect - Plays no sound.
                CustomSoundEffect = ShieldConstants.NoSound,

                // Generates settings for having one shield hit sound. The sound's volume is based on the percentage lost and the size settings inputted.

                /*delete this line to uncomment this section
                CustomSoundEffect = ShieldConstants.Sound(
                    minimumShieldHPRequired: 0.01f, // Minimum Shield HP Percentage required to play any sound
                    subtypeId: "MusFun", // Subtype ID of the sound to play
                    minVolume: 0.1f, // minimum volume to play the sound at.
                    volumeMultiplier: 0.1f, // Amount to multiply the shield Percentage Lost by to get sound volume.
                    maxVolume: 0.1f  // Maximum volume to play the sound at.
                    ),
                delete this line to uncomment the above section*/

                // Generates settings for having one shield hit sound for small hits and one for large ones. The sound's volume is based on the percentage lost and the size settings inputted.

                 /*delete this line to uncomment this section
                CustomSoundEffect = ShieldConstants.LargeHitSound(
                    minimumShieldHPRequired: 0.01f, // Minimum Shield HP Percentage required to play any sound
                    smallHitSubtypeId: "MusCalm_05", // SubtypeID of the sound to use for hits under largeDamageThreshold
                    largeHitSubtypeId: "MusFun", // SubtypeID of the sound to use for hits over largeDamageThreshold
                    largeDamageThreshold: 5000f, // Damage threshold for switching between the two hit sonds
                    minVolume: 0.1f, // minimum volume to play the sound at.
                    volumeMultiplier: 0.1f, // Amount to multiply the shield Percentage Lost by to get sound volume.
                    maxVolume: 0.1f  // Maximum volume to play the sound at.
                    ),
                delete this line to uncomment the above section*/
            },
            // Upgrade shield blocks modify a main shield block on grid. They do NOT create shields on their own, and can be placed anywhere on the same grid
            new ShieldUpgradeBlockDefinition()
            {
                SubtypeId = "NerdUpgradeShieldBlock_HP", // subtype ID of the block you want to turn into a shield. It must be a functional block, although sorters are recommended. Sorters will have their terminal controls removed.
                // If there are multiple definitions with the same subtype ID (like say someone is adjusting stats of another's shield mod), then the definition with the highest priority will be loaded.
                // For people making their own shield mod, I recommend leaving this at zero.
                // For people MODIFYING other people's mod, I recommend setting this at anything greater than zero.
                // This effectively allows mod adjuster-like behavior without relying on mod load order, although the entire definition must be copied for it to work properly.
                //  - those modifying shield stats can just have the shield definitions in their place w/o copying any models, sbc files, or sounds to the modified mod.
                DefinitionPriority = 1,

                AddedHP = 25000, // shield HP the upgrade block gives
                AddedHPRegenPerSec = 0, // shield HP regen the upgrade block gives
                AddedTicksUntilRegen = 0, // number of ticks to change the main shield block's TicksUntilRegen value by. Positive increases the time, negative decreases
                
                // if not null, replaces the main shield's DefaultDamageCurve
                // function that takes in shield percent (0-1) and returns percentage (0-1) of damage to block
                // effectively allows any type of bleedthrough
                // comes with some defaults - visualized here https://www.desmos.com/calculator/agor4exetc : 
                //    ShieldConstants.ShieldLinearDamageCurve    - blocks a percentage of damage equal to the percent of shield HP remaining
                //    ShieldConstants.ShieldFullBlockDamageCurve - blocks 100% of damage until the shield reaches 1%, then blocks none
                //    ShieldConstants.ShieldAltDamageCurve       - starts out only blocking 60% of the damage but the falloff is shallow until 10% shield HP, look at desmos for more info
                //    ShieldConstants.ShieldQuarticDamageCurve   - funky quartic function that has steep falloff everywhere but from 40% to 80% shield HP, hovering around 60% blocked, look at desmos for more info
                //
                // for the people who know C#, the curve is a Func<float, float> that you can set to whatever lamda function you want
                ReplacementDamageCurve = null,
                
                // toggle on whether the effects of the shield block can be toggled by enabling and disabling the block. This will not mean the block is unable to be disabled, but that the effect persists when off.
                // This only matters for the terminal on/off switch though, if the block is nonfunctional its effects don't perist
                CanBeTurnedOff = true,

                // If not zero, overrides the block's power requirement to this value. Units are MW
                PowerRequirementOverride = 10f,


                

            },
            new ShieldUpgradeBlockDefinition()
            {
                SubtypeId = "NerdUpgradeShieldBlock_HP_SG", // subtype ID of the block you want to turn into a shield. It must be a functional block, although sorters are recommended. Sorters will have their terminal controls removed.
                // If there are multiple definitions with the same subtype ID (like say someone is adjusting stats of another's shield mod), then the definition with the highest priority will be loaded.
                // For people making their own shield mod, I recommend leaving this at zero.
                // For people MODIFYING other people's mod, I recommend setting this at anything greater than zero.
                // This effectively allows mod adjuster-like behavior without relying on mod load order, although the entire definition must be copied for it to work properly.
                //  - those modifying shield stats can just have the shield definitions in their place w/o copying any models, sbc files, or sounds to the modified mod.
                DefinitionPriority = 1,

                AddedHP = 1250, // shield HP the upgrade block gives
                AddedHPRegenPerSec = 0, // shield HP regen the upgrade block gives
                AddedTicksUntilRegen = 0, // number of ticks to change the main shield block's TicksUntilRegen value by. Positive increases the time, negative decreases
                
                // if not null, replaces the main shield's DefaultDamageCurve
                // function that takes in shield percent (0-1) and returns percentage (0-1) of damage to block
                // effectively allows any type of bleedthrough
                // comes with some defaults - visualized here https://www.desmos.com/calculator/agor4exetc : 
                //    ShieldConstants.ShieldLinearDamageCurve    - blocks a percentage of damage equal to the percent of shield HP remaining
                //    ShieldConstants.ShieldFullBlockDamageCurve - blocks 100% of damage until the shield reaches 1%, then blocks none
                //    ShieldConstants.ShieldAltDamageCurve       - starts out only blocking 60% of the damage but the falloff is shallow until 10% shield HP, look at desmos for more info
                //    ShieldConstants.ShieldQuarticDamageCurve   - funky quartic function that has steep falloff everywhere but from 40% to 80% shield HP, hovering around 60% blocked, look at desmos for more info
                //
                // for the people who know C#, the curve is a Func<float, float> that you can set to whatever lamda function you want
                ReplacementDamageCurve = null, // Set to null to turn off
                
                // toggle on whether the effects of the shield block can be toggled by enabling and disabling the block. This will not mean the block is unable to be disabled, but that the effect persists when off.
                // This only matters for the terminal on/off switch though, if the block is nonfunctional its effects don't perist
                CanBeTurnedOff = true,

                // If not zero, overrides the block's power requirement to this value. Units are MW
                PowerRequirementOverride = 2f,

            },
            new ShieldUpgradeBlockDefinition()
            {
                SubtypeId = "NerdUpgradeShieldBlock_Regen", // subtype ID of the block you want to turn into a shield. It must be a functional block, although sorters are recommended. Sorters will have their terminal controls removed.
                // If there are multiple definitions with the same subtype ID (like say someone is adjusting stats of another's shield mod), then the definition with the highest priority will be loaded.
                // For people making their own shield mod, I recommend leaving this at zero.
                // For people MODIFYING other people's mod, I recommend setting this at anything greater than zero.
                // This effectively allows mod adjuster-like behavior without relying on mod load order, although the entire definition must be copied for it to work properly.
                //  - those modifying shield stats can just have the shield definitions in their place w/o copying any models, sbc files, or sounds to the modified mod.
                DefinitionPriority = 1,

                AddedHP = 0, // shield HP the upgrade block gives
                AddedHPRegenPerSec = 750, // shield HP regen the upgrade block gives
                AddedTicksUntilRegen = 0, // number of ticks to change the main shield block's TicksUntilRegen value by. Positive increases the time, negative decreases
                
                // if not null, replaces the main shield's DefaultDamageCurve
                // function that takes in shield percent (0-1) and returns percentage (0-1) of damage to block
                // effectively allows any type of bleedthrough
                // comes with some defaults - visualized here https://www.desmos.com/calculator/agor4exetc : 
                //    ShieldConstants.ShieldLinearDamageCurve    - blocks a percentage of damage equal to the percent of shield HP remaining
                //    ShieldConstants.ShieldHalfBlockDamageCurve - blocks 50% of damage until the shield reaches 0%, then blocks none
                //    ShieldConstants.ShieldFullBlockDamageCurve - blocks 100% of damage until the shield reaches 0%, then blocks none
                //    ShieldConstants.ShieldAltDamageCurve       - starts out only blocking 60% of the damage but the falloff is shallow until 10% shield HP, look at desmos for more info
                //    ShieldConstants.ShieldQuarticDamageCurve   - funky quartic function that has steep falloff everywhere but from 40% to 80% shield HP, hovering around 60% blocked, look at desmos for more info
                //
                // for the people who know C#, the curve is a Func<float, float> that you can set to whatever lamda function you want
                ReplacementDamageCurve = null, // Set to null to turn off
                
                // toggle on whether the effects of the shield block can be toggled by enabling and disabling the block. This will not mean the block is unable to be disabled, but that the effect persists when off.
                // This only matters for the terminal on/off switch though, if the block is nonfunctional its effects don't perist
                CanBeTurnedOff = true,

                // If not zero, overrides the block's power requirement to this value. Units are MW
                PowerRequirementOverride = 10f,

            },
            new ShieldUpgradeBlockDefinition()
            {
                SubtypeId = "NerdUpgradeShieldBlock_Regen_SG", // subtype ID of the block you want to turn into a shield. It must be a functional block, although sorters are recommended. Sorters will have their terminal controls removed.
                // If there are multiple definitions with the same subtype ID (like say someone is adjusting stats of another's shield mod), then the definition with the highest priority will be loaded.
                // For people making their own shield mod, I recommend leaving this at zero.
                // For people MODIFYING other people's mod, I recommend setting this at anything greater than zero.
                // This effectively allows mod adjuster-like behavior without relying on mod load order, although the entire definition must be copied for it to work properly.
                //  - those modifying shield stats can just have the shield definitions in their place w/o copying any models, sbc files, or sounds to the modified mod.
                DefinitionPriority = 1,

                AddedHP = 0, // shield HP the upgrade block gives
                AddedHPRegenPerSec = 75, // shield HP regen the upgrade block gives
                AddedTicksUntilRegen = 0, // number of ticks to change the main shield block's TicksUntilRegen value by. Positive increases the time, negative decreases
                
                // if not null, replaces the main shield's DefaultDamageCurve
                // function that takes in shield percent (0-1) and returns percentage (0-1) of damage to block
                // effectively allows any type of bleedthrough
                // comes with some defaults - visualized here https://www.desmos.com/calculator/agor4exetc : 
                //    ShieldConstants.ShieldLinearDamageCurve    - blocks a percentage of damage equal to the percent of shield HP remaining
                //    ShieldConstants.ShieldHalfBlockDamageCurve - blocks 50% of damage until the shield reaches 0%, then blocks none
                //    ShieldConstants.ShieldFullBlockDamageCurve - blocks 100% of damage until the shield reaches 0%, then blocks none
                //    ShieldConstants.ShieldAltDamageCurve       - starts out only blocking 60% of the damage but the falloff is shallow until 10% shield HP, look at desmos for more info
                //    ShieldConstants.ShieldQuarticDamageCurve   - funky quartic function that has steep falloff everywhere but from 40% to 80% shield HP, hovering around 60% blocked, look at desmos for more info
                //
                // for the people who know C#, the curve is a Func<float, float> that you can set to whatever lamda function you want
                ReplacementDamageCurve = null, // Set to null to turn off
                
                // toggle on whether the effects of the shield block can be toggled by enabling and disabling the block. This will not mean the block is unable to be disabled, but that the effect persists when off.
                // This only matters for the terminal on/off switch though, if the block is nonfunctional its effects don't perist
                CanBeTurnedOff = true,

                // If not zero, overrides the block's power requirement to this value. Units are MW
                PowerRequirementOverride = 2f,
            },
            new ShieldUpgradeBlockDefinition()
            {
                SubtypeId = "NerdUpgradeShieldBlock_AltDamageCurve", // subtype ID of the block you want to turn into a shield. It must be a functional block, although sorters are recommended. Sorters will have their terminal controls removed.
                // If there are multiple definitions with the same subtype ID (like say someone is adjusting stats of another's shield mod), then the definition with the highest priority will be loaded.
                // For people making their own shield mod, I recommend leaving this at zero.
                // For people MODIFYING other people's mod, I recommend setting this at anything greater than zero.
                // This effectively allows mod adjuster-like behavior without relying on mod load order, although the entire definition must be copied for it to work properly.
                //  - those modifying shield stats can just have the shield definitions in their place w/o copying any models, sbc files, or sounds to the modified mod.
                DefinitionPriority = 1,

                AddedHP = 0, // shield HP the upgrade block gives
                AddedHPRegenPerSec = 0, // shield HP regen the upgrade block gives
                AddedTicksUntilRegen = 0, // number of ticks to change the main shield block's TicksUntilRegen value by. Positive increases the time, negative decreases
                
                // if not null, replaces the main shield's DefaultDamageCurve
                // function that takes in shield percent (0-1) and returns percentage (0-1) of damage to block
                // effectively allows any type of bleedthrough
                // comes with some defaults - visualized here https://www.desmos.com/calculator/agor4exetc : 
                //    ShieldConstants.ShieldLinearDamageCurve    - blocks a percentage of damage equal to the percent of shield HP remaining
                //    ShieldConstants.ShieldHalfBlockDamageCurve - blocks 50% of damage until the shield reaches 0%, then blocks none
                //    ShieldConstants.ShieldFullBlockDamageCurve - blocks 100% of damage until the shield reaches 0%, then blocks none
                //    ShieldConstants.ShieldAltDamageCurve       - starts out only blocking 60% of the damage but the falloff is shallow until 10% shield HP, look at desmos for more info
                //    ShieldConstants.ShieldQuarticDamageCurve   - funky quartic function that has steep falloff everywhere but from 40% to 80% shield HP, hovering around 60% blocked, look at desmos for more info
                //
                // for the people who know C#, the curve is a Func<float, float> that you can set to whatever lamda function you want
                ReplacementDamageCurve = ShieldConstants.ShieldAltDamageCurve, // Set to null to turn off
                
                // toggle on whether the effects of the shield block can be toggled by enabling and disabling the block. This will not mean the block is unable to be disabled, but that the effect persists when off.
                // This only matters for the terminal on/off switch though, if the block is nonfunctional its effects don't perist
                CanBeTurnedOff = false,

                // If not zero, overrides the block's power requirement to this value. Units are MW
                PowerRequirementOverride = 0f,
            },
            new ShieldUpgradeBlockDefinition()
            {
                SubtypeId = "NerdUpgradeShieldBlock_AltDamageCurve_SG", // subtype ID of the block you want to turn into a shield. It must be a functional block, although sorters are recommended. Sorters will have their terminal controls removed.
                // If there are multiple definitions with the same subtype ID (like say someone is adjusting stats of another's shield mod), then the definition with the highest priority will be loaded.
                // For people making their own shield mod, I recommend leaving this at zero.
                // For people MODIFYING other people's mod, I recommend setting this at anything greater than zero.
                // This effectively allows mod adjuster-like behavior without relying on mod load order, although the entire definition must be copied for it to work properly.
                //  - those modifying shield stats can just have the shield definitions in their place w/o copying any models, sbc files, or sounds to the modified mod.
                DefinitionPriority = 1,

                AddedHP = 0, // shield HP the upgrade block gives
                AddedHPRegenPerSec = 0, // shield HP regen the upgrade block gives
                AddedTicksUntilRegen = 0, // number of ticks to change the main shield block's TicksUntilRegen value by. Positive increases the time, negative decreases
                
                // if not null, replaces the main shield's DefaultDamageCurve
                // function that takes in shield percent (0-1) and returns percentage (0-1) of damage to block
                // effectively allows any type of bleedthrough
                // comes with some defaults - visualized here https://www.desmos.com/calculator/agor4exetc : 
                //    ShieldConstants.ShieldLinearDamageCurve    - blocks a percentage of damage equal to the percent of shield HP remaining
                //    ShieldConstants.ShieldHalfBlockDamageCurve - blocks 50% of damage until the shield reaches 0%, then blocks none
                //    ShieldConstants.ShieldFullBlockDamageCurve - blocks 100% of damage until the shield reaches 0%, then blocks none
                //    ShieldConstants.ShieldAltDamageCurve       - starts out only blocking 60% of the damage but the falloff is shallow until 10% shield HP, look at desmos for more info
                //    ShieldConstants.ShieldQuarticDamageCurve   - funky quartic function that has steep falloff everywhere but from 40% to 80% shield HP, hovering around 60% blocked, look at desmos for more info
                //
                // for the people who know C#, the curve is a Func<float, float> that you can set to whatever lamda function you want
                ReplacementDamageCurve = ShieldConstants.ShieldAltDamageCurve, // Set to null to turn off
                
                // toggle on whether the effects of the shield block can be toggled by enabling and disabling the block. This will not mean the block is unable to be disabled, but that the effect persists when off.
                // This only matters for the terminal on/off switch though, if the block is nonfunctional its effects don't perists
                CanBeTurnedOff = false,

                // If not zero, overrides the block's power requirement to this value. Units are MW
                PowerRequirementOverride = 0f,
            },
        };
        // do not modify above here
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            foreach (var def in Definitions)
            {
                MyAPIGateway.Utilities.SendModMessage(ShieldConstants.MessageHandlerId, def.ConvertToObjectArray());
            }
        }
    }
}

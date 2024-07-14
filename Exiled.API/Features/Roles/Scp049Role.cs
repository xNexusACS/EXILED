// -----------------------------------------------------------------------
// <copyright file="Scp049Role.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Roles
{
    using System.Collections.Generic;
    using System.Linq;

    using CustomPlayerEffects;
    using Exiled.API.Features.Core;
    using Exiled.API.Features.Core.Attributes;
    using HarmonyLib;
    using PlayerRoles;
    using PlayerRoles.PlayableScps;
    using PlayerRoles.PlayableScps.HumeShield;
    using PlayerRoles.PlayableScps.Scp049;
    using PlayerRoles.Ragdolls;
    using PlayerRoles.Subroutines;
    using PlayerStatsSystem;
    using UnityEngine;

    using Scp049GameRole = PlayerRoles.PlayableScps.Scp049.Scp049Role;

    /// <summary>
    /// Defines a role that represents SCP-049.
    /// </summary>
    public class Scp049Role : FpcRole, ISubroutinedScpRole, IHumeShieldRole, ISpawnableScp
    {
        private readonly ConstProperty<double> callAbilityDuration = new(Scp049CallAbility.EffectDuration, new[] { typeof(Scp049CallAbility) });
        private readonly ConstProperty<double> callAbilityBaseCooldown = new(Scp049CallAbility.BaseCooldown, new[] { typeof(Scp049CallAbility) });
        private readonly ConstProperty<double> senseAbilityBaseCooldown = new(Scp049SenseAbility.BaseCooldown, new[] { typeof(Scp049SenseAbility) });
        private readonly ConstProperty<double> senseAbilityReducedCooldown = new(Scp049SenseAbility.ReducedCooldown, new[] { typeof(Scp049SenseAbility) });
        private readonly ConstProperty<double> senseAbilityDuration = new(Scp049SenseAbility.EffectDuration, new[] { typeof(Scp049SenseAbility) }, new[] { AccessTools.Method(typeof(Scp049SenseAbility), nameof(Scp049SenseAbility.ServerLoseTarget)) });
        private readonly ConstProperty<double> senseAbilityFailCooldown = new(Scp049SenseAbility.AttemptFailCooldown, new[] { typeof(Scp049SenseAbility) }, new[] { AccessTools.Method(typeof(Scp049SenseAbility), nameof(Scp049SenseAbility.ServerProcessCmd)) });

        /// <summary>
        /// Initializes a new instance of the <see cref="Scp049Role"/> class.
        /// </summary>
        /// <param name="baseRole">the base <see cref="Scp049GameRole"/>.</param>
        internal Scp049Role(Scp049GameRole baseRole)
            : base(baseRole)
        {
            Base = baseRole;
            SubroutineModule = baseRole.SubroutineModule;
            HumeShieldModule = baseRole.HumeShieldModule;

            if (!SubroutineModule.TryGetSubroutine(out Scp049ResurrectAbility scp049ResurrectAbility))
                Log.Error("Scp049ResurrectAbility subroutine not found in Scp049Role::ctor");

            ResurrectAbility = scp049ResurrectAbility;

            if (!SubroutineModule.TryGetSubroutine(out Scp049CallAbility scp049CallAbility))
                Log.Error("Scp049CallAbility subroutine not found in Scp049Role::ctor");

            CallAbility = scp049CallAbility;

            if (!SubroutineModule.TryGetSubroutine(out Scp049SenseAbility scp049SenseAbility))
                Log.Error("Scp049SenseAbility subroutine not found in Scp049Role::ctor");

            SenseAbility = scp049SenseAbility;

            if (!SubroutineModule.TryGetSubroutine(out Scp049AttackAbility scp049AttackAbility))
                Log.Error("Scp049AttackAbility subroutine not found in Scp049Role::ctor");

            AttackAbility = scp049AttackAbility;
        }

        /// <summary>
        /// Gets a list of players who are turned away from SCP-049 Sense Ability.
        /// </summary>
        public static HashSet<Player> TurnedPlayers { get; } = new(20);

        /// <inheritdoc/>
        [EProperty(readOnly: true, category: nameof(Role))]
        public override RoleTypeId Type { get; } = RoleTypeId.Scp049;

        /// <inheritdoc/>
        public SubroutineManagerModule SubroutineModule { get; }

        /// <inheritdoc/>
        public HumeShieldModuleBase HumeShieldModule { get; }

        /// <summary>
        /// Gets SCP-049's <see cref="Scp049ResurrectAbility"/>.
        /// </summary>
        public Scp049ResurrectAbility ResurrectAbility { get; }

        /// <summary>
        /// Gets SCP-049's <see cref="Scp049AttackAbility"/>.
        /// </summary>
        public Scp049AttackAbility AttackAbility { get; }

        /// <summary>
        /// Gets SCP-049's <see cref="Scp049CallAbility"/>.
        /// </summary>
        public Scp049CallAbility CallAbility { get; }

        /// <summary>
        /// Gets SCP-049's <see cref="Scp049SenseAbility"/>.
        /// </summary>
        public Scp049SenseAbility SenseAbility { get; }

        /// <summary>
        /// Gets a value indicating whether or not SCP-049 is currently reviving a player.
        /// </summary>
        [EProperty(readOnly: true, category: nameof(Scp049Role))]
        public bool IsRecalling => ResurrectAbility.IsInProgress;

        /// <summary>
        /// Gets a value indicating whether or not SCP-049's "Doctor's Call" ability is currently active.
        /// </summary>
        [EProperty(readOnly: true, category: nameof(Scp049Role))]
        public bool IsCallActive => CallAbility.IsMarkerShown;

        /// <summary>
        /// Gets the player that is currently being revived by SCP-049. Will be <see langword="null"/> if <see cref="IsRecalling"/> is <see langword="false"/>.
        /// </summary>
        public Player RecallingPlayer => ResurrectAbility.CurRagdoll == null ? null : Player.Get(ResurrectAbility.CurRagdoll.Info.OwnerHub);

        /// <summary>
        /// Gets the ragdoll that is currently being revived by SCP-049. Will be <see langword="null"/> if <see cref="IsRecalling"/> is <see langword="false"/>.
        /// </summary>
        public Ragdoll RecallingRagdoll => Ragdoll.Get(ResurrectAbility.CurRagdoll);

        /// <summary>
        /// Gets all the dead zombies.
        /// </summary>
        public IEnumerable<Player> DeadZombies => Scp049ResurrectAbility.DeadZombies.Select(x => Player.Get(x));

        /// <summary>
        /// Gets or sets how mush time the Call Ability will be effective.
        /// </summary>
        public double CallAbilityDuration
        {
            get => callAbilityDuration;
            set => callAbilityDuration.Value = value;
        }

        /// <summary>
        /// Gets or sets the Cooldown of the Call Ability.
        /// </summary>
        public double CallAbilityBaseCooldown
        {
            get => callAbilityBaseCooldown;
            set => callAbilityBaseCooldown.Value = value;
        }

        /// <summary>
        /// Gets or sets the Cooldown of the Sense Ability.
        /// </summary>
        public double SenseAbilityBaseCooldown
        {
            get => senseAbilityBaseCooldown;
            set => senseAbilityBaseCooldown.Value = value;
        }

        /// <summary>
        /// Gets or sets the Cooldown of the Sense Ability when you lost your target.
        /// </summary>
        public double SenseAbilityReducedCooldown
        {
            get => senseAbilityReducedCooldown;
            set => senseAbilityReducedCooldown.Value = value;
        }

        /// <summary>
        /// Gets or sets the Cooldown of the Sense Ability when it's failed.
        /// </summary>
        public double SenseAbilityDuration
        {
            get => senseAbilityDuration;
            set => senseAbilityDuration.Value = value;
        }

        /// <summary>
        /// Gets or sets how mush time the Sense Ability will be effective.
        /// </summary>
        public double SenseAbilityFailCooldown
        {
            get => senseAbilityFailCooldown;
            set => senseAbilityFailCooldown.Value = value;
        }

        /// <summary>
        /// Gets all the resurrected players.
        /// </summary>
        public Dictionary<Player, int> ResurrectedPlayers => Scp049ResurrectAbility.ResurrectedPlayers.ToDictionary(x => Player.Get(x.Key), x => x.Value);

        /// <summary>
        /// Gets or sets the amount of time before SCP-049 can use its Doctor's Call ability again.
        /// </summary>
        [EProperty(category: nameof(Scp049Role))]
        public float CallCooldown
        {
            get => CallAbility.Cooldown.Remaining;
            set
            {
                CallAbility.Cooldown.Remaining = value;
                CallAbility.ServerSendRpc(true);
            }
        }

        /// <summary>
        /// Gets or sets the amount of time before SCP-049 can use its Good Sense of the Doctor ability again.
        /// </summary>
        [EProperty(category: nameof(Scp049Role))]
        public float GoodSenseCooldown
        {
            get => SenseAbility.Cooldown.Remaining;
            set
            {
                SenseAbility.Cooldown.Remaining = value;
                SenseAbility.ServerSendRpc(true);
            }
        }

        /// <summary>
        /// Gets or sets the amount of time before SCP-049 can attack again.
        /// </summary>
        [EProperty(category: nameof(Scp049Role))]
        public float RemainingAttackCooldown
        {
            get => AttackAbility.Cooldown.Remaining;
            set
            {
                AttackAbility.Cooldown.Remaining = value;
                AttackAbility.ServerSendRpc(true);
            }
        }

        /// <summary>
        /// Gets or sets the duration of the <see cref="Scp049CallAbility"/>.
        /// </summary>
        [EProperty(category: nameof(Scp049Role))]
        public float RemainingCallDuration
        {
            get => CallAbility.Duration.Remaining;
            set
            {
                CallAbility.Duration.Remaining = value;
                CallAbility.ServerSendRpc(true);
            }
        }

        /// <summary>
        /// Gets or sets the duration of the <see cref="Scp049SenseAbility"/>.
        /// </summary>
        [EProperty(category: nameof(Scp049Role))]
        public float RemainingGoodSenseDuration
        {
            get => SenseAbility.Duration.Remaining;
            set
            {
                SenseAbility.Duration.Remaining = value;
                SenseAbility.ServerSendRpc(true);
            }
        }

        /// <summary>
        /// Gets or sets the distance of the Sense Ability.
        /// </summary>
        [EProperty(category: nameof(Scp049Role))]
        public float SenseDistance
        {
            get => SenseAbility._distanceThreshold;
            set => SenseAbility._distanceThreshold = value;
        }

        /// <summary>
        /// Gets the <see cref="Scp049GameRole"/> instance.
        /// </summary>
        public new Scp049GameRole Base { get; }

        /// <summary>
        /// Lose the current target of the Good Sense ability.
        /// </summary>
        public void LoseSenseTarget() => SenseAbility.ServerLoseTarget();

        /// <summary>
        /// Resurrects a <see cref="Player"/>.
        /// </summary>
        /// <param name="player">The <see cref="Player"/>to resurrect.</param>
        /// <returns>The Resurrected player.</returns>
        public bool Resurrect(Player player)
        {
            if (player is null)
                return false;
            player.ReferenceHub.transform.position = ResurrectAbility.CastRole.FpcModule.Position;

            HumeShieldModuleBase humeShield = ResurrectAbility.CastRole.HumeShieldModule;
            humeShield.HsCurrent = Mathf.Min(humeShield.HsCurrent + 100f, humeShield.HsMax);

            return Resurrect(Ragdoll.GetLast(player));
        }

        /// <summary>
        /// Resurrects a <see cref="Ragdoll"/> owner.
        /// </summary>
        /// <param name="ragdoll">The Ragdoll to resurrect.</param>
        /// <returns>The Resurrected Ragdoll.</returns>
        public bool Resurrect(Ragdoll ragdoll)
        {
            if (ragdoll is null)
                return false;

            ResurrectAbility.CurRagdoll = ragdoll.Base;
            ResurrectAbility.ServerComplete();

            return true;
        }

        /// <summary>
        /// Attacks a Player.
        /// </summary>
        /// <param name="player">The <see cref="Player"/>to attack.</param>
        public void Attack(Player player)
        {
            AttackAbility._target = player?.ReferenceHub;

            if (AttackAbility._target is null || !AttackAbility.IsTargetValid(AttackAbility._target))
                return;

            AttackAbility.Cooldown.Trigger(Scp049AttackAbility.CooldownTime);
            CardiacArrest cardiacArrest = AttackAbility._target.playerEffectsController.GetEffect<CardiacArrest>();

            if (cardiacArrest.IsEnabled)
            {
                AttackAbility._target.playerStats.DealDamage(new Scp049DamageHandler(AttackAbility.Owner, StandardDamageHandler.KillValue, Scp049DamageHandler.AttackType.Instakill));
            }
            else
            {
                cardiacArrest.SetAttacker(AttackAbility.Owner);
                cardiacArrest.Intensity = 1;
                cardiacArrest.ServerChangeDuration(AttackAbility._statusEffectDuration, false);
            }

            SenseAbility.OnServerHit(AttackAbility._target);

            AttackAbility.ServerSendRpc(true);
            Hitmarker.SendHitmarkerDirectly(AttackAbility.Owner, 1f);
        }

        /// <summary>
        /// Trigger the Sense Ability on the specified <see cref="Player"/>.
        /// </summary>
        /// <param name="player">The Player to sense.</param>
        public void Sense(Player player)
        {
            if (!SenseAbility.Cooldown.IsReady || !SenseAbility.Duration.IsReady)
                return;

            SenseAbility.HasTarget = false;
            SenseAbility.Target = player?.ReferenceHub;

            if (SenseAbility.Target is null)
            {
                SenseAbility.Cooldown.Trigger(SenseAbilityFailCooldown);
                SenseAbility.ServerSendRpc(true);
                return;
            }
            else
            {
                if (SenseAbility.Target.roleManager.CurrentRole is not PlayerRoles.HumanRole humanRole)
                    return;

                float radius = humanRole.FpcModule.CharController.radius;
                if (!VisionInformation.GetVisionInformation(SenseAbility.Owner, SenseAbility.Owner.PlayerCameraReference, humanRole.CameraPosition, radius, SenseAbility._distanceThreshold).IsLooking)
                    return;

                SenseAbility.Duration.Trigger(SenseAbilityDuration);
                SenseAbility.HasTarget = true;
                SenseAbility.ServerSendRpc(true);
            }
        }

        /// <summary>
        /// Refresh the <see cref="Scp049CallAbility"/> duration.
        /// </summary>
        public void RefreshCallDuration() => CallAbility.ServerRefreshDuration();

        /// <summary>
        /// Gets the amount of resurrections of a <see cref="Player"/>.
        /// </summary>
        /// <param name="player">The <see cref="Player"/>to check.</param>
        /// <returns>The amount of resurrections of the checked player.</returns>
        public int GetResurrectionCount(Player player) => player is not null ? Scp049ResurrectAbility.GetResurrectionsNumber(player.ReferenceHub) : 0;

        /// <summary>
        /// Returns a <see langword="bool"/> indicating whether or not the ragdoll can be resurrected by SCP-049.
        /// </summary>
        /// <param name="ragdoll">The ragdoll to check.</param>
        /// <returns><see langword="true"/> if the body can be revived; otherwise, <see langword="false"/>.</returns>
        public bool CanResurrect(BasicRagdoll ragdoll) => ragdoll != null && ResurrectAbility.CheckRagdoll(ragdoll);

        /// <summary>
        /// Returns a <see langword="bool"/> indicating whether or not the ragdoll can be resurrected by SCP-049.
        /// </summary>
        /// <param name="ragdoll">The ragdoll to check.</param>
        /// <returns><see langword="true"/> if the body can be revived; otherwise, <see langword="false"/>.</returns>
        public bool CanResurrect(Ragdoll ragdoll) => ragdoll is not null && ResurrectAbility.CheckRagdoll(ragdoll.Base);

        /// <summary>
        /// Returns a <see langword="bool"/> indicating whether or not SCP-049 is close enough to a ragdoll to revive it.
        /// </summary>
        /// <remarks>This method only returns whether or not SCP-049 is close enough to the body to revive it; the body may have expired. Make sure to check <see cref="CanResurrect(BasicRagdoll)"/> to ensure the body can be revived.</remarks>
        /// <param name="ragdoll">The ragdoll to check.</param>
        /// <returns><see langword="true"/> if close enough to revive the body; otherwise, <see langword="false"/>.</returns>
        public bool IsInRecallRange(BasicRagdoll ragdoll) => ragdoll != null && ResurrectAbility.IsCloseEnough(Owner.Position, ragdoll.transform.position);

        /// <summary>
        /// Returns a <see langword="bool"/> indicating whether or not SCP-049 is close enough to a ragdoll to revive it.
        /// </summary>
        /// <remarks>This method only returns whether or not SCP-049 is close enough to the body to revive it; the body may have expired. Make sure to check <see cref="CanResurrect(Ragdoll)"/> to ensure the body can be revived.</remarks>
        /// <param name="ragdoll">The ragdoll to check.</param>
        /// <returns><see langword="true"/> if close enough to revive the body; otherwise, <see langword="false"/>.</returns>
        public bool IsInRecallRange(Ragdoll ragdoll) => ragdoll is not null && IsInRecallRange(ragdoll.Base);

        /// <summary>
        /// Gets the Spawn Chance of SCP-049.
        /// </summary>
        /// <param name="alreadySpawned">The List of Roles already spawned.</param>
        /// <returns>The Spawn Chance.</returns>
        public float GetSpawnChance(List<RoleTypeId> alreadySpawned) => Base.GetSpawnChance(alreadySpawned);
    }
}

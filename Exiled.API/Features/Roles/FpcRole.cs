// -----------------------------------------------------------------------
// <copyright file="FpcRole.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Roles
{
    using System.Collections.Generic;
    using System.Reflection;

    using Exiled.API.Extensions;
    using Exiled.API.Features.Core.Attributes;
    using Exiled.API.Features.Core.Generic.Pools;
    using HarmonyLib;
    using PlayerRoles;
    using PlayerRoles.FirstPersonControl;
    using RelativePositioning;
    using UnityEngine;

    /// <summary>
    /// Defines a role that represents an fpc class.
    /// </summary>
    public abstract class FpcRole : Role
    {
        private static FieldInfo enableFallDamageField;

        private bool isUsingStamina = true;
        private RoleTypeId? fakeAppearance;

        /// <summary>
        /// Initializes a new instance of the <see cref="FpcRole"/> class.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject"/>.</param>
        protected internal FpcRole(GameObject gameObject)
            : base(gameObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FpcRole"/> class.
        /// </summary>
        /// <param name="baseRole">the base <see cref="PlayerRoleBase"/>.</param>
        protected FpcRole(FpcStandardRoleBase baseRole)
            : base(baseRole)
        {
            FirstPersonController = baseRole;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="FpcRole"/> class.
        /// </summary>
        ~FpcRole() => HashSetPool<Player>.Pool.Return(IsInvisibleFor);

        /// <summary>
        /// Gets the <see cref="FirstPersonController"/>.
        /// </summary>
        public FpcStandardRoleBase FirstPersonController { get; }

        /// <summary>
        /// Gets the <see cref="FirstPersonMovementModule"/>.
        /// </summary>
        public FirstPersonMovementModule MovementModule => FirstPersonController.FpcModule;

        /// <summary>
        /// Gets the <see cref="CharacterController"/>.
        /// </summary>
        public CharacterController CharacterController => FirstPersonController.FpcModule.CharController;

        /// <summary>
        /// Gets or sets the player's relative position.
        /// </summary>
        [EProperty(category: nameof(FpcRole))]
        public RelativePosition RelativePosition
        {
            get => FirstPersonController.FpcModule.Motor.ReceivedPosition;
            set => FirstPersonController.FpcModule.Motor.ReceivedPosition = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether if the player should get <see cref="Enums.DamageType.Falldown"/> damage.
        /// </summary>
        [EProperty(category: nameof(FpcRole))]
        public bool IsFallDamageEnable
        {
            get => FirstPersonController.FpcModule.Motor._enableFallDamage;
            set
            {
                enableFallDamageField ??= AccessTools.Field(typeof(FpcMotor), nameof(FpcMotor._enableFallDamage));
                enableFallDamageField.SetValue(FirstPersonController.FpcModule.Motor, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether if a rotation is detected on the player.
        /// </summary>
        [EProperty(category: nameof(FpcRole))]
        public bool RotationDetected
        {
            get => FirstPersonController.FpcModule.Motor.RotationDetected;
            set => FirstPersonController.FpcModule.Motor.RotationDetected = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="Role"/> walking speed.
        /// </summary>
        [EProperty(category: nameof(FpcRole))]
        public float WalkingSpeed
        {
            get => FirstPersonController.FpcModule.WalkSpeed;
            set => FirstPersonController.FpcModule.WalkSpeed = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="Role"/> sprinting speed.
        /// </summary>
        [EProperty(category: nameof(FpcRole))]
        public float SprintingSpeed
        {
            get => FirstPersonController.FpcModule.SprintSpeed;
            set => FirstPersonController.FpcModule.SprintSpeed = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="Role"/> jumping speed.
        /// </summary>
        [EProperty(category: nameof(FpcRole))]
        public float JumpingSpeed
        {
            get => FirstPersonController.FpcModule.JumpSpeed;
            set => FirstPersonController.FpcModule.JumpSpeed = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="Role"/> crouching speed.
        /// </summary>
        [EProperty(category: nameof(FpcRole))]
        public float CrouchingSpeed
        {
            get => FirstPersonController.FpcModule.CrouchSpeed;
            set => FirstPersonController.FpcModule.CrouchSpeed = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="Player"/> velocity.
        /// </summary>
        [EProperty(category: nameof(FpcRole))]
        public Vector3 Velocity
        {
            get => FirstPersonController.FpcModule.Motor.Velocity;
            set => FirstPersonController.FpcModule.Motor.Velocity = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether if a movement is detected on a <see cref="Player"/>.
        /// </summary>
        [EProperty(category: nameof(FpcRole))]
        public bool MovementDetected
        {
            get => FirstPersonController.FpcModule.Motor.MovementDetected;
            set => FirstPersonController.FpcModule.Motor.MovementDetected = value;
        }

        /// <summary>
        /// Gets a value indicating whether or not the player can send inputs.
        /// </summary>
        [EProperty(readOnly: true, category: nameof(FpcRole))]
        public bool CanSendInputs => FirstPersonController.FpcModule.LockMovement;

        /// <summary>
        /// Gets or sets a value indicating whether or not the player is invisible.
        /// </summary>
        [EProperty(category: nameof(FpcRole))]
        public bool IsInvisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the player should use stamina system. Resets on death.
        /// </summary>
        [EProperty(category: nameof(FpcRole))]
        public bool IsUsingStamina
        {
            get => isUsingStamina;
            set
            {
                if (!value)
                    Owner.ResetStamina();
                isUsingStamina = value;
            }
        }

        /// <summary>
        /// Gets or sets the stamina usage multiplier. Resets on death.
        /// </summary>
        [EProperty(category: nameof(FpcRole))]
        public float StaminaUsageMultiplier { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the stamina regen multiplier. Resets on death.
        /// </summary>
        [EProperty(category: nameof(FpcRole))]
        public float StaminaRegenMultiplier { get; set; } = 1f;

        /// <summary>
        /// Gets a list of players who can't see the player.
        /// </summary>
        public HashSet<Player> IsInvisibleFor { get; } = HashSetPool<Player>.Pool.Get();

        /// <summary>
        /// Gets or sets the player's current <see cref="PlayerMovementState"/>.
        /// </summary>
        [EProperty(category: nameof(FpcRole))]
        public PlayerMovementState MoveState
        {
            get => FirstPersonController.FpcModule.CurrentMovementState;
            set => FirstPersonController.FpcModule.CurrentMovementState = value;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Player"/> is crouching or not.
        /// </summary>
        [EProperty(readOnly: true, category: nameof(FpcRole))]
        public bool IsCrouching => FirstPersonController.FpcModule.StateProcessor.CrouchPercent > 0;

        /// <summary>
        /// Gets a value indicating whether or not the player is on the ground.
        /// </summary>
        [EProperty(readOnly: true, category: nameof(FpcRole))]
        public bool IsGrounded => FirstPersonController.FpcModule.IsGrounded;

        /// <summary>
        /// Gets the <see cref="Player"/>'s current movement speed.
        /// </summary>
        [EProperty(readOnly: true, category: nameof(FpcRole))]
        public virtual float MovementSpeed => FirstPersonController.FpcModule.VelocityForState(MoveState, IsCrouching);

        /// <summary>
        /// Gets a value indicating whether or not the <see cref="Player"/> is in darkness.
        /// </summary>
        [EProperty(readOnly: true, category: nameof(FpcRole))]
        public bool IsInDarkness => FirstPersonController.InDarkness;

        /// <summary>
        /// Gets the <see cref="Player"/>'s vertical rotation.
        /// </summary>
        [EProperty(readOnly: true, category: nameof(FpcRole))]
        public float VerticalRotation => FirstPersonController.VerticalRotation;

        /// <summary>
        /// Gets the <see cref="Player"/>'s horizontal rotation.
        /// </summary>
        [EProperty(readOnly: true, category: nameof(FpcRole))]
        public float HorizontalRotation => FirstPersonController.HorizontalRotation;

        /// <summary>
        /// Gets a value indicating whether or not the <see cref="Player"/> is AFK.
        /// </summary>
        [EProperty(readOnly: true, category: nameof(FpcRole))]
        public bool IsAfk => FirstPersonController.IsAFK;

        /// <summary>
        /// Gets a value indicating whether or not this role is protected by a hume shield.
        /// </summary>
        [EProperty(readOnly: true, category: nameof(FpcRole))]
        public bool IsHumeShieldedRole => this is IHumeShieldRole;

        /// <summary>
        /// Gets or sets a value indicating the fake appearance of the player.
        /// </summary>
        [EProperty(category: nameof(FpcRole))]
        public RoleTypeId? FakeAppearance
        {
            get => fakeAppearance;
            set
            {
                if (!value.HasValue)
                {
                    fakeAppearance = Owner.Role.Type;
                    Owner.ChangeAppearance(Owner.Role.Type, skipJump: true);
                    return;
                }

                fakeAppearance = value.Value;
                Owner.ChangeAppearance(value.Value);
            }
        }

        /// <summary>
        /// Resets the <see cref="Player"/>'s stamina.
        /// </summary>
        /// <param name="multipliers">Resets <see cref="StaminaUsageMultiplier"/> and <see cref="StaminaRegenMultiplier"/>.</param>
        public void ResetStamina(bool multipliers = false)
        {
            Owner.Stamina = Owner.StaminaStat.MaxValue;

            if (!multipliers)
                return;

            StaminaUsageMultiplier = 1f;
            StaminaRegenMultiplier = 1f;
        }
    }
}

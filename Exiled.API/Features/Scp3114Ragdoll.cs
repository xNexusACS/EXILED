// -----------------------------------------------------------------------
// <copyright file="Scp3114Ragdoll.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features
{
    using Exiled.API.Features.Core.Attributes;
    using Exiled.API.Interfaces;
    using PlayerRoles;

    using BaseScp3114Ragdoll = PlayerRoles.PlayableScps.Scp3114.Scp3114Ragdoll;

    /// <summary>
    /// A wrapper for SCP-3114 ragdolls.
    /// </summary>
    [EClass(category: nameof(Scp3114Ragdoll))]
    public class Scp3114Ragdoll : Ragdoll, IWrapper<BaseScp3114Ragdoll>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Scp3114Ragdoll"/> class.
        /// </summary>
        /// <param name="ragdoll">The base ragdoll to wrap. <inheritdoc cref="Base"/></param>
        internal Scp3114Ragdoll(BaseScp3114Ragdoll ragdoll)
            : base(ragdoll)
        {
            Base = ragdoll;
        }

        /// <inheritdoc/>
        public new BaseScp3114Ragdoll Base { get; }

        /// <summary>
        /// Gets or sets the role that the corpse is disguised as.
        /// </summary>
        [EProperty(category: nameof(Scp3114Ragdoll))]
        public RoleTypeId DisguiseRole
        {
            get => Base._disguiseRole;
            set => Base.Network_disguiseRole = value;
        }

        /// <summary>
        /// Gets or sets the delay between when SCP-3114 can disguise this corpse.
        /// </summary>
        [EProperty(category: nameof(Scp3114Ragdoll))]
        public float RevealDelay
        {
            get => Base._revealDelay;
            set => Base._revealDelay = value;
        }

        /// <summary>
        /// Gets or sets the time required to reveal this corpse.
        /// </summary>
        [EProperty(category: nameof(Scp3114Ragdoll))]
        public float RevealDuration
        {
            get => Base._revealDuration;
            set => Base._revealDuration = value;
        }

        /// <summary>
        /// Gets or sets the current time of revealing this corpse.
        /// </summary>
        [EProperty(category: nameof(Scp3114Ragdoll))]
        public float RevealElapsed
        {
            get => Base._revealElapsed;
            set => Base._revealElapsed = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not this corpse will trigger animation.
        /// </summary>
        [EProperty(category: nameof(Scp3114Ragdoll))]
        public bool IsPlayingAnimation
        {
            get => Base._playingAnimation;
            set => Base._playingAnimation = value;
        }
    }
}
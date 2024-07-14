// -----------------------------------------------------------------------
// <copyright file="DancingEventArgs.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Scp3114
{
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Roles;
    using Exiled.Events.EventArgs.Interfaces;
    using Exiled.Events.Patches.Events.Scp3114;

    /// <summary>
    /// Contains all information before SCP-3114 changes its dancing status.
    /// </summary>
    public class DancingEventArgs : IDeniableEvent, IScp3114Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DancingEventArgs"/> class.
        /// </summary>
        /// <param name="newState"><inheritdoc cref="DanceType"/></param>
        /// <param name="player"><inheritdoc cref="Player"/></param>
        /// <param name="isAllowed"><inheritdoc cref="IsAllowed"/></param>
        public DancingEventArgs(bool newState, Player player, bool isAllowed = true)
        {
            Player = player;
            Scp3114 = player.Role.As<Scp3114Role>();
            IsDancing = newState;
            DanceType = (DanceType)(newState ? UnityEngine.Random.Range(0, Scp3114.Dance._danceVariants) : byte.MaxValue);
            IsAllowed = isAllowed;
        }

        /// <inheritdoc/>
        public bool IsAllowed { get; set; }

        /// <summary>
        /// Gets a value indicating whether or not SCP-3114 is dancing.
        /// </summary>
        public bool IsDancing { get; }

        /// <summary>
        /// Gets or sets the bound dance.
        /// </summary>
        public DanceType DanceType
        {
            get => Scp3114.DanceType;
            set => Scp3114.DanceType = value;
        }

        /// <inheritdoc/>
        public Player Player { get; }

        /// <inheritdoc/>
        public Scp3114Role Scp3114 { get; }
    }
}
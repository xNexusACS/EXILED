﻿// -----------------------------------------------------------------------
// <copyright file="EventHandler.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Example
{
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Player;

    /// <summary>
    /// Example of an event handler.
    /// </summary>
    public class EventHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandler"/> class.
        /// </summary>
        public EventHandler()
        {
            Exiled.Events.Handlers.Player.Verified += OnVerified;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="EventHandler"/> class.
        /// </summary>
        ~EventHandler()
        {
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
        }

        private void OnVerified(VerifiedEventArgs ev) => Log.Info($"{ev.Player} has joined the server!");
    }
}
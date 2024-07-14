// -----------------------------------------------------------------------
// <copyright file="GrenadePickup.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Pickups
{
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features.Core;
    using Exiled.API.Features.Core.Attributes;
    using Exiled.API.Features.Pickups.Projectiles;
    using Exiled.API.Interfaces;
    using Footprinting;
    using InventorySystem.Items;
    using InventorySystem.Items.ThrowableProjectiles;

    /// <summary>
    /// A wrapper class for a high explosive grenade pickup.
    /// </summary>
    public class GrenadePickup : Pickup, IWrapper<TimedGrenadePickup>
    {
        private readonly ConstProperty<double> explosionRadius = new(0.4000000059604645, new[] { typeof(TimedGrenadePickup) });

        /// <summary>
        /// Initializes a new instance of the <see cref="GrenadePickup"/> class.
        /// </summary>
        /// <param name="pickupBase">The base <see cref="TimedGrenadePickup"/> class.</param>
        internal GrenadePickup(TimedGrenadePickup pickupBase)
            : base(pickupBase)
        {
            Base = pickupBase;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrenadePickup"/> class.
        /// </summary>
        /// <param name="type">The <see cref="ItemType"/> of the pickup.</param>
        internal GrenadePickup(ItemType type)
            : this((TimedGrenadePickup)type.GetItemBase().ServerDropItem())
        {
        }

        /// <summary>
        /// Gets or sets how long the fuse will last.
        /// </summary>
        [EProperty(category: nameof(GrenadePickup))]
        public float FuseTime { get; set; }

        /// <summary>
        /// Gets the <see cref="Enums.ProjectileType"/> of the item.
        /// </summary>
        public ProjectileType ProjectileType => Type.GetProjectileType();

        /// <summary>
        /// Gets the <see cref="TimedGrenadePickup"/> that this class is encapsulating.
        /// </summary>
        public new TimedGrenadePickup Base { get; }

        /// <summary>
        /// Gets or sets maximum distance between grenade and explosion to trigger it.
        /// </summary>
        public double ExplosionRadius
        {
            get => explosionRadius.Value;
            set => explosionRadius.Value = value;
        }

        /// <summary>
        /// Trigger the grenade to make it Explode.
        /// </summary>
        public void Explode() => Explode(Base.PreviousOwner);

        /// <summary>
        /// Trigger the grenade to make it Explode.
        /// </summary>
        /// <param name="attacker">The <see cref="Footprint"/> of the explosion.</param>
        public void Explode(Footprint attacker)
        {
            Base._replaceNextFrame = true;
            Base._attacker = attacker;
        }

        /// <summary>
        /// Helper method for saving data between projectiles and pickups.
        /// </summary>
        /// <param name="projectile"><see cref="Projectile"/>-related data to write to.</param>
        internal virtual void WriteProjectileInfo(Projectile projectile)
        {
            if (projectile is TimeGrenadeProjectile timeGrenadeProjectile)
            {
                timeGrenadeProjectile.FuseTime = FuseTime;
            }
        }

        /// <inheritdoc/>
        protected override void InitializeProperties(ItemBase itemBase)
        {
            base.InitializeProperties(itemBase);
            if (itemBase is ThrowableItem throwable && throwable.Projectile is TimeGrenade timeGrenade)
            {
                FuseTime = timeGrenade._fuseTime;
            }
        }
    }
}

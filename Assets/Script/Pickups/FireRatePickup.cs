using Game.Player;

using UnityEngine;

namespace Game.Pickup
{
    public sealed class FireRatePickup : Pickup
    {
#pragma warning disable CS0649
        [SerializeField, Min(0.1f), Tooltip("Duration of the fire rate in seconds.")]
        private float fireRateDuration;
#pragma warning restore CS0649

        protected override void Execute(PlayerBody player) => player.GetComponent<Weapon>().AddFireRate(fireRateDuration);
    }
}
using Game.Player;

using UnityEngine;

namespace Game.Pickup
{
    public sealed class MultiShootPickup : Pickup
    {
#pragma warning disable CS0649
        [SerializeField, Min(0.1f), Tooltip("Duration of the multi shoot in seconds.")]
        private float multiShootDuration;
#pragma warning restore CS0649

        protected override void Execute(PlayerBody player) => player.GetComponent<Weapon>().AddMultiShoot(multiShootDuration);
    }
}
using Game.Player;

using UnityEngine;

namespace Game.Pickup
{
    public sealed class ShieldPickup : Pickup
    {
#pragma warning disable CS0649
        [SerializeField, Min(0.1f), Tooltip("Duration of the shield in seconds.")]
        private float shieldDuration;
#pragma warning restore CS0649

        protected override void Execute(PlayerBody player) => player.AddShield(shieldDuration);
    }
}
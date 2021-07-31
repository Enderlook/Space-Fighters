using Game.Level;
using Game.Player;

using Photon.Pun;

using UnityEngine;

namespace Game.Pickup
{
    [DisallowMultipleComponent, RequireComponent(typeof(PhotonView))]
    public abstract class Pickup : MonoBehaviourPun
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!Server.IsServer)
                return;

            if (PlayerScore.HasFinalized)
                goto end;

            if (collision.TryGetComponent(out PlayerBody player))
                Execute(player);

            end:
            PhotonNetwork.Destroy(gameObject);
        }

        protected abstract void Execute(PlayerBody player);
    }
}
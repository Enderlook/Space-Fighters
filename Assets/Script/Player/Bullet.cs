using Game.Level;

using Photon.Pun;

using UnityEngine;

namespace Game.Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(PhotonView))]
    public sealed class Bullet : MonoBehaviourPun
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!Server.IsServer)
                return;

            if (PlayerScore.HasFinalized)
                goto end;

            if (collision.TryGetComponent(out PlayerBody player))
            {
                Photon.Realtime.Player owner = this.GetPlayerOwner();
                PlayerScore.ChangeCounter(owner, owner == player.GetPlayerOwner());
                player.Die();
            }

            end:
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
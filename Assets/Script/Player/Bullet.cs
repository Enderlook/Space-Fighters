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
            if (!photonView.IsMine)
                return;

            if (collision.TryGetComponent(out PlayerBody player))
            {
                PlayerScore.IncreaseCounter(photonView.Owner);
                player.Die();
            }

            PhotonNetwork.Destroy(gameObject);
        }
    }
}
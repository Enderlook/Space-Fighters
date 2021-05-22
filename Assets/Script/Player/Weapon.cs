using Enderlook.Unity.AudioManager;

using Photon.Pun;

using UnityEngine;

namespace Game.Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D), typeof(PhotonView))]
    public sealed class Weapon : MonoBehaviourPun
    {
#pragma warning disable CS0649
        [Header("Projectile")]
        [SerializeField, Tooltip("Prefab of the projectile to shoot.")]
        private string projectile;

        [SerializeField, Tooltip("Force applied to projectile.")]
        private float force;

        [SerializeField, Tooltip("Sound played when shooting.")]
        private AudioUnit shootSound;

        [Header("Setup")]
        [SerializeField, Tooltip("Cooldown between shoots.")]
        private float shootCooldown;

        [SerializeField, Tooltip("Key pressed to shoot.")]
        private KeyCode shootKey;

        [SerializeField, Tooltip("Point where bullets are spawn.")]
        private Transform shootPoint;
#pragma warning restore CS0649

#pragma warning disable CS0108
        private Rigidbody2D rigidbody;
#pragma warning restore CS0108

        private float nextShootAt;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake() => rigidbody = GetComponent<Rigidbody2D>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FixedUpdate()
        {
            if (!photonView.IsMine)
                return;

            if (Time.fixedTime >= nextShootAt && Input.GetKey(shootKey))
            {
                nextShootAt = Time.fixedTime + shootCooldown;
                GameObject instance = PhotonNetwork.Instantiate(projectile, shootPoint.position, shootPoint.rotation);
                Rigidbody2D instanceRigidbody = instance.GetComponent<Rigidbody2D>();
                instanceRigidbody.velocity = rigidbody.velocity;
                instanceRigidbody.AddForce(shootPoint.up * force, ForceMode2D.Impulse);
                photonView.RPC(nameof(PlayShootSound), RpcTarget.All);
            }
        }

        [PunRPC]
        private void PlayShootSound() => AudioController.PlayOneShoot(shootSound, rigidbody.position);
    }
}
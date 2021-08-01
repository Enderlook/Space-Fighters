using Enderlook.Unity.AudioManager;

using Game.Level;

using Photon.Pun;

using System;
using System.Linq;
using System.Linq.Expressions;

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

        [SerializeField, Tooltip("Sound player when fire rate power up expires.")]
        private AudioUnit fireRateExpires;

        [SerializeField, Tooltip("Sound player when fire rate power up recharges.")]
        private AudioUnit fireRateRecharges;

        [SerializeField, Tooltip("Sound player when multi shoot power up expires.")]
        private AudioUnit multiShootExpires;

        [SerializeField, Tooltip("Sound player when multi shoot power up recharges.")]
        private AudioUnit multiShootRecharges;
#pragma warning restore CS0649

#pragma warning disable CS0108
        private Rigidbody2D rigidbody;
#pragma warning restore CS0108

        private float nextShootAt;
        private float loseFireRateAt;
        private float loseMultiShootAt;
        private PlayerBody body;
        private Transform[] multishootPoints;

        private Expression<Action> shootExpression;
        private Expression<Action<float>> addFireRateExpression;
        private Expression<Action<float>> addMultiShootExpression;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            body = GetComponent<PlayerBody>();
            shootExpression = () => RPC_Shoot();
            addFireRateExpression = (fireRateDuration) => RPC_AddFireRate(fireRateDuration);
            addMultiShootExpression = (multiShootDuration) => RPC_AddMultiShoot(multiShootDuration);
            multishootPoints = Enumerable.Range(0, shootPoint.childCount).Select(e => shootPoint.GetChild(e)).ToArray();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FixedUpdate()
        {
            if (loseFireRateAt != 0 && loseFireRateAt <= Time.fixedTime)
            {
                loseFireRateAt = 0;
                AudioController.PlayOneShoot(fireRateExpires, rigidbody.position);
            }

            if (loseMultiShootAt != 0 && loseMultiShootAt <= Time.fixedTime)
            {
                loseMultiShootAt = 0;
                AudioController.PlayOneShoot(multiShootExpires, rigidbody.position);
            }

            if (!body.IsPlayerInputAllowed)
                return;

            if (Input.GetKey(shootKey))
                this.RPC_ToServer(shootExpression);
        }

        [PunRPC]
        private void RPC_Shoot()
        {
            if (Time.fixedTime >= nextShootAt)
            {
                float divider = Mathf.Clamp(loseFireRateAt - Time.fixedTime, 0, 2) * .6f + 1;
                nextShootAt = Time.fixedTime + (shootCooldown / divider);
                photonView.RPC(nameof(RPC_PlayShootSound), RpcTarget.All);
                Photon.Realtime.Player owner = this.GetPlayerOwner();

                Shoot(shootPoint);
                if (loseMultiShootAt > Time.fixedTime)
                {
                    foreach (Transform transform in multishootPoints)
                        Shoot(transform);
                }

                void Shoot(Transform shootPoint)
                {
                    GameObject instance = Server.InstantiatePrefab(projectile, owner, shootPoint.position, shootPoint.rotation);
                    Rigidbody2D instanceRigidbody = instance.GetComponent<Rigidbody2D>();
                    instanceRigidbody.velocity = rigidbody.velocity;
                    instanceRigidbody.AddForce(shootPoint.up * force, ForceMode2D.Impulse);
                }
            }
        }

        [PunRPC]
        private void RPC_PlayShootSound() => AudioController.PlayOneShoot(shootSound, rigidbody.position);

        public void AddFireRate(float fireRateDuration) => this.RPC_FromServer(addFireRateExpression, fireRateDuration);

        [PunRPC]
        private void RPC_AddFireRate(float fireRateDuration)
        {
            if (loseFireRateAt < Time.fixedTime)
                loseFireRateAt = Time.fixedTime + fireRateDuration;
            else
                loseFireRateAt += fireRateDuration;

            AudioController.PlayOneShoot(fireRateRecharges, rigidbody.position);
        }

        public void AddMultiShoot(float multiShootDuration) => this.RPC_FromServer(addMultiShootExpression, multiShootDuration);

        [PunRPC]
        private void RPC_AddMultiShoot(float multiShootDuration)
        {
            if (loseMultiShootAt < Time.fixedTime)
                loseMultiShootAt = Time.fixedTime + multiShootDuration;
            else
                loseMultiShootAt += multiShootDuration;

            AudioController.PlayOneShoot(multiShootRecharges, rigidbody.position);
        }
    }
}
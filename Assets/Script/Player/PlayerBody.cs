using Enderlook.Unity.AudioManager;

using Game.Level;
using Game.Pickup;

using Photon.Pun;

using System;
using System.Linq.Expressions;

using UnityEngine;

namespace Game.Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D), typeof(PhotonView)), DefaultExecutionOrder(2)]
    public sealed class PlayerBody : MonoBehaviourPun
    {
#pragma warning disable CS0649
        [Header("Invulnerability")]
        [SerializeField, Tooltip("Amount of seconds invulnerability last.")]
        private float invulnerabilityDuration = 2;

        [SerializeField, Range(0, 1), Tooltip("Color multiplier of ship when invulnerable.")]
        private float invulnerabilityTone = .8f;

        [Header("Die")]
        [SerializeField, Tooltip("Name of die animation trigger.")]
        private string dieTrigger;

        [SerializeField, Tooltip("Sound played when die.")]
        private AudioUnit dieSound;

        [Header("Shield")]
        [SerializeField, Tooltip("Gameobject which holds shield sprite renderer.")]
        private GameObject shield;

        [SerializeField, Tooltip("Sound played when shield collapses for damage.")]
        private AudioUnit shieldHurtSound;

        [SerializeField, Tooltip("Sound played when shield collapses for time.")]
        private AudioUnit shieldExpiresSound;

        [SerializeField, Tooltip("Sound played when shield recharges.")]
        private AudioUnit shieldRechargesSound;
#pragma warning restore CS0649

#pragma warning disable CS0108
        private Rigidbody2D rigidbody;
        private SpriteRenderer renderer;
        private SpriteRenderer shieldRenderer;
        private Collider2D collider;
#pragma warning restore CS0108
        private Animator animator;

        private float becomeVulnerableAt;
        private float loseShieldAt;

        private Expression<Action<bool>> hurtExpression;
        private Expression<Action> fromDieExpression;
        private Expression<Action<float>> addShieldExpression;

        private bool IsAlive => becomeVulnerableAt != -1;

        public bool IsPlayerInputAllowed => this.IsOwnerPlayer() && IsAlive && !PlayerScore.HasFinalized;

        private const float shieldFading = 2;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            renderer = GetComponent<SpriteRenderer>();
            shieldRenderer = shield.GetComponent<SpriteRenderer>();
            collider = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();
            hurtExpression = (hadShield) => RPC_Hurt(hadShield);
            fromDieExpression = () => RPC_FromDie();
            addShieldExpression = (shieldDuration) => RPC_AddShield(shieldDuration);
            shield.SetActive(false);
            Server.AfterServerIsConnected(() =>
            {
                SetPlayerColor();
                BecomeInvulnerable();
            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FixedUpdate()
        {
            if (becomeVulnerableAt < Time.fixedTime && becomeVulnerableAt != 0)
            {
                if (becomeVulnerableAt == -1)
                    return;

                becomeVulnerableAt = 0;
                collider.enabled = true;
                SetPlayerColor();
            }

            if (loseShieldAt != 0)
            {
                if (loseShieldAt < Time.fixedTime)
                {
                    loseShieldAt = 0;
                    shield.SetActive(false);
                    AudioController.PlayOneShoot(shieldExpiresSound, rigidbody.position);
                }
                else
                {
                    Color color = shieldRenderer.color;
                    color.a = Mathf.Min(loseShieldAt - Time.fixedTime, shieldFading) / shieldFading;
                    shieldRenderer.color = color;
                }
            }
        }

        public bool Hurt()
        {
            Debug.Assert(Server.IsServer);
            bool hadShield = loseShieldAt > 0;
            this.RPC_FromServer(hurtExpression, hadShield);
            if (!hadShield)
                PickupSpawner.Spawn(rigidbody);
            return !hadShield;
        }

        [PunRPC]
        private void RPC_Hurt(bool hadShield)
        {
            if (hadShield)
            {
                loseShieldAt -= 1.5f;
                if (loseShieldAt <= Time.fixedTime)
                    shield.SetActive(false);
                else
                {
                    Color color = shieldRenderer.color;
                    color.a = Mathf.Min(loseShieldAt - Time.fixedTime, shieldFading) / shieldFading;
                    shieldRenderer.color = color;
                }
                AudioController.PlayOneShoot(shieldHurtSound, rigidbody.position);
            }
            else
            {
                rigidbody.velocity = default;
                animator.SetTrigger(dieTrigger);
                collider.enabled = false;
                becomeVulnerableAt = -1;
                AudioController.PlayOneShoot(dieSound, rigidbody.position);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FromDie()
        {
            if (Server.IsServer)
                this.RPC_FromServer(fromDieExpression);
        }

        [PunRPC]
        private void RPC_FromDie()
        {
            BecomeInvulnerable();
            (Vector2 position, float rotation) tuple = Server.GetSpawnPosition(this.GetPlayerOwner());
            rigidbody.position = tuple.position;
            rigidbody.rotation = tuple.rotation;
        }

        private void BecomeInvulnerable()
        {
            becomeVulnerableAt = Time.fixedTime + invulnerabilityDuration;
            renderer.color *= invulnerabilityTone;
            shieldRenderer.color *= invulnerabilityTone;
        }

        private void SetPlayerColor()
        {
            Color color = PlayerScore.GetShipColor(this.GetPlayerOwner());
            renderer.color = color;
            shieldRenderer.color = color;
        }

        public void AddShield(float shieldDuration) => this.RPC_FromServer(addShieldExpression, shieldDuration);

        [PunRPC]
        private void RPC_AddShield(float shieldDuration)
        {
            if (loseShieldAt < Time.fixedTime)
                loseShieldAt = Time.fixedTime + shieldDuration;
            else
                loseShieldAt += shieldDuration;

            shield.SetActive(true);
            Color color = shieldRenderer.color;
            color.a = Mathf.Min(loseShieldAt, shieldFading) / shieldFading;
            shieldRenderer.color = color;

            AudioController.PlayOneShoot(shieldRechargesSound, rigidbody.position);
        }
    }
}
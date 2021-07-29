using Enderlook.Unity.AudioManager;

using Game.Level;

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
#pragma warning restore CS0649

#pragma warning disable CS0108
        private Rigidbody2D rigidbody;
        private SpriteRenderer renderer;
        private Collider2D collider;
#pragma warning restore CS0108
        private Animator animator;

        private float becomeVulnerableAt;

        private Expression<Action> dieExpression;
        private Expression<Action> fromDieExpression;

        private bool IsAlive => becomeVulnerableAt != -1;

        public bool IsPlayerInputAllowed => this.IsOwnerPlayer() && IsAlive && !PlayerScore.HasFinalized;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            renderer = GetComponent<SpriteRenderer>();
            collider = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();
            dieExpression = () => RPC_Die();
            fromDieExpression = () => RPC_FromDie();
            Server.AfterServerIsConnected(() =>
            {
                SetPlayerColor();
                BecomeInvulnerable();
            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update()
        {
            if (becomeVulnerableAt < Time.time && becomeVulnerableAt != 0)
            {
                if (becomeVulnerableAt == -1)
                    return;

                becomeVulnerableAt = 0;
                collider.enabled = true;
                SetPlayerColor();
            }
        }

        public void Die() => this.RPC_FromServer(dieExpression);

        [PunRPC]
        private void RPC_Die()
        {
            rigidbody.velocity = default;
            animator.SetTrigger(dieTrigger);
            collider.enabled = false;
            becomeVulnerableAt = -1;
            AudioController.PlayOneShoot(dieSound, rigidbody.position);
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
            becomeVulnerableAt = Time.time + invulnerabilityDuration;
            renderer.color *= invulnerabilityTone;
        }

        private void SetPlayerColor() => renderer.color = PlayerScore.GetShipColor(this.GetPlayerOwner());
    }
}
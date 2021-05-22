﻿using Photon.Pun;

using UnityEngine;

namespace Game.Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D), typeof(PhotonView))]
    public sealed class PlayerBody : MonoBehaviourPun
    {
#pragma warning disable CS0649
        [Header("Colors")]
        [SerializeField]
        private Color enemyColor = Color.red;

        [SerializeField]
        private Color playerColor = Color.white;

        [Header("Invulnerability")]
        [SerializeField]
        private float invulnerabilityDuration = 2;

        [SerializeField, Range(0, 1)]
        private float invulnerabilityTone = .8f;

        [Header("Animations")]
        [SerializeField]
        private string dieTrigger;
#pragma warning restore CS0649

#pragma warning disable CS0108
        private Rigidbody2D rigidbody;
        private SpriteRenderer renderer;
        private Collider2D collider;
#pragma warning restore CS0108
        private Animator animator;

        private float becomeVulnerableAt;

        public bool IsAlive => becomeVulnerableAt != -1;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            renderer = GetComponent<SpriteRenderer>();
            collider = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();

            renderer.color = photonView.IsMine ? playerColor : enemyColor;

            BecomeInvulnerable();
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
                renderer.color = photonView.IsMine ? playerColor : enemyColor;
            }
        }

        public void Die() => photonView.RPC(nameof(RPC_Die), RpcTarget.All);

        [PunRPC]
        private void RPC_Die()
        {
            rigidbody.velocity = default;
            animator.SetTrigger(dieTrigger);
            collider.enabled = false;
            becomeVulnerableAt = -1;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FromDie()
        {
            if (photonView.IsMine)
                photonView.RPC(nameof(RPC_FromDie), RpcTarget.All);
        }

        [PunRPC]
        private void RPC_FromDie()
        {
            BecomeInvulnerable();
            rigidbody.position = Vector2.zero;
            transform.position = Vector2.zero;
            rigidbody.rotation = 0;
        }

        private void BecomeInvulnerable()
        {
            becomeVulnerableAt = Time.time + invulnerabilityDuration;
            renderer.color *= invulnerabilityTone;
        }
    }
}
﻿using Photon.Pun;

using UnityEngine;

namespace Game.Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D), typeof(PlayerBody), typeof(PhotonView))]
    public sealed class PlayerController : MonoBehaviourPun
    {
#pragma warning disable CS0649
        [Header("Movement")]
        [SerializeField, Tooltip("Acceleration when moving.")]
        private float accelerationSpeed;

        [SerializeField, Tooltip("Maximum movement speed.")]
        private float maximumSpeed;

        [SerializeField, Tooltip("Desacceleration when braking.")]
        private float brakeStrength;

        [SerializeField, Tooltip("Speed when rotating.")]
        private float rotationSpeed;

        [Header("Animations")]
        [SerializeField, Tooltip("Name of idle animation trigger.")]
        private string idleTrigger;

        [SerializeField, Tooltip("Name of accelerate animation trigger.")]
        private string accelerateTrigger;

        [SerializeField, Tooltip("Name of brake animation trigger.")]
        private string brakeTrigger;
#pragma warning restore CS0649

#pragma warning disable CS0108
        private Rigidbody2D rigidbody;
#pragma warning restore CS0108
        private Animator animator;
        private PlayerBody body;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            body = GetComponent<PlayerBody>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FixedUpdate()
        {
            if (!body.IsAlive)
                return;

            if (photonView.IsMine)
            {
                Move();
                Rotation();
            }
        }

        private void Move()
        {
            float acceleration = Input.GetAxis("Vertical");
            if (acceleration > 0)
            {
                animator.SetTrigger(accelerateTrigger);
                rigidbody.AddRelativeForce(Vector2.up * acceleration * accelerationSpeed, ForceMode2D.Force);
                rigidbody.velocity = Vector2.ClampMagnitude(rigidbody.velocity, maximumSpeed);
            }
            else if (acceleration < 0)
            {
                animator.SetTrigger(brakeTrigger);
                float newMagnitude = Mathf.Max(rigidbody.velocity.magnitude - (brakeStrength * Time.fixedDeltaTime), 0);
                rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, newMagnitude);
            }
            else
                animator.SetTrigger(idleTrigger);
        }

        private void Rotation()
        {
            float rotation = Input.GetAxis("Horizontal");
            if (rotation != 0)
                rigidbody.SetRotation(rigidbody.rotation - (rotation * rotationSpeed));
        }
    }
}
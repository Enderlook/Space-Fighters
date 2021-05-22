using Photon.Pun;

using UnityEngine;

namespace Game.Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D), typeof(PlayerBody), typeof(PhotonView))]
    public sealed class PlayerController : MonoBehaviourPun
    {
#pragma warning disable CS0649
        [Header("Movement")]
        [SerializeField]
        private float accelerationSpeed;

        [SerializeField]
        private float maximumSpeed;

        [SerializeField]
        private float brakeStrength;

        [SerializeField]
        private float rotationSpeed;

        [Header("Animations")]
        [SerializeField]
        private string idleTrigger;

        [SerializeField]
        private string accelerateTrigger;

        [SerializeField]
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
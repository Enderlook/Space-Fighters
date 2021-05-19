using Photon.Pun;

using UnityEngine;

namespace Game.Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(PhotonView))]
    public sealed class PlayerController : MonoBehaviourPun
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

        [SerializeField]
        private string dieTrigger;
#pragma warning restore CS0649

#pragma warning disable CS0108
        private Rigidbody2D rigidbody;
        private Collider2D collider;
        private SpriteRenderer renderer;
#pragma warning restore CS0108
        private Animator animator;

        private float becomeVulnerableAt;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();
            renderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            renderer.color = photonView.IsMine ? playerColor : enemyColor;

            BecomeInvulnerable();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FixedUpdate()
        {
            if (becomeVulnerableAt < Time.deltaTime && becomeVulnerableAt != 0)
            {
                if (becomeVulnerableAt == -1)
                    return;

                becomeVulnerableAt = 0;
                collider.enabled = true;
                renderer.color = photonView.IsMine ? playerColor : enemyColor;
            }

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

        public void Die()
        {
            rigidbody.velocity = default;
            animator.SetTrigger(dieTrigger);
            collider.enabled = false;
            becomeVulnerableAt = -1;
        }

        public void FromDie()
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
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public float speed = 20f;
    public ParticleSystem explosionEffect;

    [Header("Knockback Settings")]
    public float baseKnockbackForce = 50f;
    public float maxKnockbackDistance = 15f; // Maximum distance where knockback is applied
    public float minKnockbackDistance = 1f; // Distance for maximum knockback

    [Header("Audio")]
    [SerializeField] private AudioClip flyingSound;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private float explosionVolume = 1.0f;

    private Rigidbody rb;
    private AudioSource audioSource;
    private bool hasExploded = false;

    void Start()
    {
        // Set up rigidbody
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;

        // Set up audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Start flying sound if available
        if (flyingSound != null)
        {
            audioSource.clip = flyingSound;
            audioSource.loop = true;
            audioSource.volume = 0.5f;
            audioSource.spatialBlend = 1.0f; // 3D sound
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 50f;
            audioSource.Play();
        }

        // Destroy after lifetime
        Destroy(gameObject, 8f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasExploded)
            Explode();
    }

    void Explode()
    {
        // Prevent multiple explosions
        hasExploded = true;

        // Stop the flying sound
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        // Play explosion sound
        if (explosionSound != null && audioSource != null)
        {
            audioSource.loop = false;
            audioSource.spatialBlend = 1.0f; // 3D sound
            audioSource.PlayOneShot(explosionSound, explosionVolume);
        }

        if (explosionEffect != null)
        {
            ParticleSystem explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            explosion.Play();

            // Need to destroy the explosion after it finishes playing
            float explosionDuration = explosion.main.duration + explosion.main.startLifetime.constantMax;
            Destroy(explosion.gameObject, explosionDuration);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            float distance = Vector3.Distance(player.transform.position, transform.position);

            // Only apply knockback if player is within maxKnockbackDistance
            if (distance <= maxKnockbackDistance)
            {
                FPSController controller = player.GetComponent<FPSController>();

                if (controller != null)
                {
                    Vector3 knockbackDir = (player.transform.position - transform.position).normalized;

                    // Calculate knockback force based on distance
                    // Closer = stronger, further = weaker
                    float distanceFactor = Mathf.Clamp01(1.0f - (distance - minKnockbackDistance) / (maxKnockbackDistance - minKnockbackDistance));
                    float knockbackForce = baseKnockbackForce * distanceFactor;

                    controller.ApplyKnockback(knockbackDir * knockbackForce);

                    Debug.Log($"Applied rocket knockback to player. Distance: {distance:F1}m, Force: {knockbackForce:F1}");
                }
            }
            else
            {
                Debug.Log($"Player too far for knockback ({distance:F1}m > {maxKnockbackDistance:F1}m)");
            }
        }

        // Make the rocket mesh invisible but keep the GameObject alive for the sound to finish
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.enabled = false;

        // Disable collider
        Collider rocketCollider = GetComponent<Collider>();
        if (rocketCollider != null)
            rocketCollider.enabled = false;

        // Disable rigidbody physics
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }

        // Destroy the rocket after the explosion sound finishes
        float soundDuration = explosionSound != null ? explosionSound.length : 0f;
        Destroy(gameObject, Mathf.Max(2f, soundDuration));
    }
}

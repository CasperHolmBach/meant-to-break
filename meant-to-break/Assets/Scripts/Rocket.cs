using UnityEngine;

public class Rocket : MonoBehaviour
{
    public float speed = 20f;
    public ParticleSystem explosionEffect;

    [Header("Knockback Settings")]
    public float baseKnockbackForce = 50f;
    public float maxKnockbackDistance = 15f; // Maximum distance where knockback is applied
    public float minKnockbackDistance = 1f; // Distance for maximum knockback
    
    [Header("Damage Settings")]
    public int maxDamage = 100; // Maximum damage at center of explosion
    public float damageRadius = 8f; // How far the damage extends - bigger than the rocket
    public float criticalDamageRadius = 3f; // Radius within which full damage is applied
    public bool showDamageRadius = false; // Debug visualization

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

        // Store explosion position for reference
        Vector3 explosionPos = transform.position;

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

        // Create explosion visual effect
        if (explosionEffect != null)
        {
            ParticleSystem explosion = Instantiate(explosionEffect, explosionPos, Quaternion.identity);
            explosion.Play();

            // Need to destroy the explosion after it finishes playing
            float explosionDuration = explosion.main.duration + explosion.main.startLifetime.constantMax;
            Destroy(explosion.gameObject, explosionDuration);
        }

        // === Handle player knockback ===
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(player.transform.position, explosionPos);

            // Only apply knockback if player is within maxKnockbackDistance
            if (distance <= maxKnockbackDistance)
            {
                FPSController controller = player.GetComponent<FPSController>();
                if (controller != null)
                {
                    Vector3 knockbackDir = (player.transform.position - explosionPos).normalized;

                    // Calculate knockback force based on distance
                    // Closer = stronger, further = weaker
                    float distanceFactor = Mathf.Clamp01(1.0f - (distance - minKnockbackDistance) / (maxKnockbackDistance - minKnockbackDistance));
                    float knockbackForce = baseKnockbackForce * distanceFactor;

                    controller.ApplyKnockback(knockbackDir * knockbackForce);
                    Debug.Log($"Applied rocket knockback to player. Distance: {distance:F1}m, Force: {knockbackForce:F1}");
                }
            }
        }

        // === Handle enemy damage ===
        ApplyExplosionDamage(explosionPos);

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
    
    private void ApplyExplosionDamage(Vector3 explosionPos)
    {
        try
        {
            // Find all colliders within the damage radius
            Collider[] colliders = Physics.OverlapSphere(explosionPos, damageRadius);
            
            // Track the total damage applied for debug purposes
            int totalDamageApplied = 0;
            int enemiesHit = 0;
            
            foreach (Collider hit in colliders)
            {
                // Skip objects without a ZombieController component
                // This approach avoids the tag check completely
                ZombieController zombie = hit.GetComponent<ZombieController>();
                if (zombie == null)
                    continue;
                
                // Calculate distance from explosion
                float distance = Vector3.Distance(hit.transform.position, explosionPos);
                
                // Calculate damage based on distance
                int damage = CalculateDamageAtDistance(distance);
                
                // Apply damage to zombie
                zombie.TakeDamage(damage);
                totalDamageApplied += damage;
                enemiesHit++;
                
                // Optional: Apply physics force to zombies if they have rigidbodies
                Rigidbody enemyRb = hit.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    Vector3 pushDir = (hit.transform.position - explosionPos).normalized;
                    float pushForce = 500.0f * (1.0f - Mathf.Clamp01(distance / damageRadius));
                    enemyRb.AddForce(pushDir * pushForce, ForceMode.Impulse);
                }
                
                // Debug log for hit enemies
                Debug.Log($"Rocket hit {hit.name} at distance {distance:F1}m for {damage} damage!");
            }
            
            // Debug summary for the explosion
            if (enemiesHit > 0)
            {
                Debug.Log($"Rocket explosion hit {enemiesHit} enemies for {totalDamageApplied} total damage!");
            }
        }
        catch (System.Exception ex)
        {
            // Safety mechanism to prevent crashes - log the error but don't crash
            Debug.LogError($"Error in ApplyExplosionDamage: {ex.Message}");
        }
    }
    
    private int CalculateDamageAtDistance(float distance)
    {
        // Full damage within critical radius
        if (distance <= criticalDamageRadius)
        {
            return maxDamage;
        }
        
        // Linear falloff from critical radius to damage radius
        if (distance <= damageRadius)
        {
            float damagePercent = 1.0f - (distance - criticalDamageRadius) / (damageRadius - criticalDamageRadius);
            return Mathf.RoundToInt(maxDamage * damagePercent);
        }
        
        // No damage beyond damage radius
        return 0;
    }
    
    // Visual debugging of damage radius in the editor
    void OnDrawGizmosSelected()
    {
        if (showDamageRadius)
        {
            // Draw the overall damage radius
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(transform.position, damageRadius);
            
            // Draw the critical damage radius
            Gizmos.color = new Color(1, 0, 0, 0.6f);
            Gizmos.DrawSphere(transform.position, criticalDamageRadius);
        }
    }
}

using UnityEngine;

public class Rocket : MonoBehaviour
{
    public float speed = 20f;
    public ParticleSystem explosionEffect;

    [Header("Knockback Settings")]
    public float baseKnockbackForce = 50f;
    public float maxKnockbackDistance = 15f; // Maximum distance where knockback is applied
    public float minKnockbackDistance = 1f; // Distance for maximum knockback

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    void Explode()
    {
        if (explosionEffect != null)
        {
            ParticleSystem explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            explosion.Play();
            Destroy(explosion.gameObject, explosion.main.duration);
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
        
        Destroy(gameObject);
    }
}

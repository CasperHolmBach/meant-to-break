using UnityEngine;

public class SMGBullet : MonoBehaviour
{
    [Header("Bullet Properties")]
    public float speed = 80f;
    public float damage = 12f;
    public float lifetime = 3f;
    public float destructionDelay = 0.1f;

    [Header("Effects")]
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private LayerMask damageMask;
    
    private Rigidbody rb;
    private bool hasCollided = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    private void Start()
    {
        // Set forward velocity
        rb.velocity = transform.forward * speed;
        
        // Destroy after lifetime seconds if no collision occurs
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided)
            return;
            
        hasCollided = true;
        
        // Apply damage if the hit object has a health component - PLACEHOLDER IMPLEMENTATION
        if (damageMask == (damageMask | (1 << collision.gameObject.layer)))
        {
            // Placeholder for Health component
            // This is a temporary solution until a Health class is created
            
            // For now, just log the damage that would have been applied
            Debug.Log($"SMG Bullet hit {collision.gameObject.name} for {damage} damage");
            
            // You can also use SendMessage as a temporary workaround
            collision.gameObject.SendMessage("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);
        }
        
        // Stop the bullet
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        
        // Disable collider
        Collider bulletCollider = GetComponent<Collider>();
        if (bulletCollider != null)
        {
            bulletCollider.enabled = false;
        }
        
        // Show impact effect
        CreateImpactEffect(collision);
        
        // Destroy the bullet after a short delay
        Destroy(gameObject, destructionDelay);
    }
    
    private void CreateImpactEffect(Collision collision)
    {
        // Get impact point and normal
        ContactPoint contact = collision.contacts[0];
        
        // Play impact sound if available
        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, contact.point);
        }
        
        // Instantiate impact effect if available
        if (impactEffect != null)
        {
            // Orient the effect to match the surface normal
            Quaternion rotation = Quaternion.LookRotation(contact.normal);
            GameObject impact = Instantiate(impactEffect, contact.point, rotation);
            
            // Destroy the impact effect after a short time
            Destroy(impact, 2f);
        }
    }
    
    // Method for the SMG to set damage after instantiation
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
}
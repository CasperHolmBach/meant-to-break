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
    
    // Add trajectory tracking to handle fast-moving bullets
    private Vector3 lastPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        
        // Set proper physics properties to prevent bouncing
        rb.mass = 0.1f;  // Lower mass
        rb.drag = 0;     // No drag
        rb.useGravity = false;  // No gravity
        
        // Store initial position for trajectory tracking
        lastPosition = transform.position;
    }

    private void Start()
    {
        // Set forward velocity
        rb.velocity = transform.forward * speed;
        
        // Destroy after lifetime seconds if no collision occurs
        Destroy(gameObject, lifetime);
    }
    
    private void Update()
    {
        // Use raycasting to detect fast-moving bullet collisions
        if (!hasCollided)
        {
            // Calculate the distance moved this frame
            Vector3 movementThisFrame = transform.position - lastPosition;
            float distanceThisFrame = movementThisFrame.magnitude;
            
            // Cast a ray along the bullet's path
            RaycastHit hit;
            if (Physics.Raycast(lastPosition, movementThisFrame.normalized, out hit, distanceThisFrame))
            {
                // Check if we hit a zombie
                ZombieController zombie = hit.collider.GetComponent<ZombieController>();
                if (zombie != null)
                {
                    // Apply damage
                    zombie.TakeDamage(Mathf.RoundToInt(damage));
                    Debug.Log($"SMG Bullet raycast hit {hit.collider.name} for {damage} damage");
                    
                    // Create impact effect at the hit point
                    CreateImpactEffectAtPoint(hit.point, hit.normal);
                    
                    // Mark as collided and stop the bullet
                    hasCollided = true;
                    rb.velocity = Vector3.zero;
                    rb.isKinematic = true;
                    
                    // Disable collider
                    Collider bulletCollider = GetComponent<Collider>();
                    if (bulletCollider != null)
                    {
                        bulletCollider.enabled = false;
                    }
                    
                    // Destroy the bullet
                    Destroy(gameObject, destructionDelay);
                }
            }
            
            // Update the last position for the next frame
            lastPosition = transform.position;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided)
            return;
            
        hasCollided = true;
        
        // Apply damage based on component rather than tag
        ZombieController zombie = collision.gameObject.GetComponent<ZombieController>();
        if (zombie != null)
        {
            zombie.TakeDamage(Mathf.RoundToInt(damage));
            Debug.Log($"SMG Bullet collision hit {collision.gameObject.name} for {damage} damage");
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Hit player! This is friendly fire.");
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
        CreateImpactEffectAtPoint(contact.point, contact.normal);
    }
    
    private void CreateImpactEffectAtPoint(Vector3 point, Vector3 normal)
    {
        // Play impact sound if available
        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, point);
        }
        
        // Instantiate impact effect if available
        if (impactEffect != null)
        {
            // Orient the effect to match the surface normal
            Quaternion rotation = Quaternion.LookRotation(normal);
            GameObject impact = Instantiate(impactEffect, point, rotation);
            
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
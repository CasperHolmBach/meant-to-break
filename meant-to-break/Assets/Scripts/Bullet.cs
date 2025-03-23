using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 20f;
    public float lifetime = 5f; // Maximum bullet lifetime
    
    private Vector3 initialPosition;
    private Vector3 lastPosition;
    private Rigidbody rb;
    private bool hasHit = false;
    
    void Awake()
    {
        // Get or add Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        
        // Configure Rigidbody for better bullet physics
        rb.mass = 0.1f;
        rb.drag = 0;
        rb.useGravity = false;
        
        // Initialize tracking variables
        initialPosition = transform.position;
        lastPosition = initialPosition;
    }
    
    void Start()
    {
        // Store initial position for distance calculation
        initialPosition = transform.position;
        
        // Set velocity directly for more consistent movement
        rb.velocity = transform.forward * speed;
        
        // Destroy after lifetime expires
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        if (!hasHit)
        {
            // Use raycasting to detect fast-moving bullet collisions
            Vector3 movementThisFrame = transform.position - lastPosition;
            float distanceThisFrame = movementThisFrame.magnitude;
            
            // Skip if there's no movement (might happen on the first frame)
            if (distanceThisFrame > 0.001f)
            {
                RaycastHit hit;
                if (Physics.Raycast(lastPosition, movementThisFrame.normalized, out hit, distanceThisFrame))
                {
                    // Check if we hit a zombie
                    ZombieController zombie = hit.collider.GetComponent<ZombieController>();
                    if (zombie != null)
                    {
                        // Apply damage
                        zombie.TakeDamage(Mathf.RoundToInt(damage));
                        Debug.Log($"Bullet raycast hit {hit.collider.name} for {damage} damage");
                        
                        // Create impact effect
                        CreateImpactEffect(hit.point, hit.normal);
                        
                        // Mark as hit and disable
                        hasHit = true;
                        DisableBullet();
                    }
                    else
                    {
                        // Hit something else
                        CreateImpactEffect(hit.point, hit.normal);
                        hasHit = true;
                        DisableBullet();
                    }
                }
            }
            
            // Update last position for next frame
            lastPosition = transform.position;
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (hasHit)
            return;
            
        hasHit = true;
        
        // Calculate distance traveled
        float distanceTraveled = Vector3.Distance(initialPosition, transform.position);
        
        // Apply damage based on component rather than tag
        ZombieController zombie = collision.gameObject.GetComponent<ZombieController>();
        if (zombie != null)
        {
            zombie.TakeDamage(Mathf.RoundToInt(damage));
            Debug.Log($"Hit enemy {collision.gameObject.name}! Damage: {damage}");
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            // Handle friendly fire if needed
            Debug.Log("Hit player! This is friendly fire.");
        }
        
        // Create impact effect
        if (collision.contacts.Length > 0)
        {
            CreateImpactEffect(collision.contacts[0].point, collision.contacts[0].normal);
        }
        else
        {
            CreateImpactEffect(transform.position, -transform.forward);
        }
        
        // Disable bullet
        DisableBullet();
    }
    
    private void DisableBullet()
    {
        // Stop physics
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // Disable collider
        Collider bulletCollider = GetComponent<Collider>();
        if (bulletCollider != null)
        {
            bulletCollider.enabled = false;
        }
        
        // Optional: Make invisible
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
        
        // Destroy with a small delay
        Destroy(gameObject, 0.1f);
    }
    
    private void CreateImpactEffect(Vector3 position, Vector3 normal)
    {
        // Visual feedback (you can replace with particle effects)
        Debug.DrawRay(position, normal * 0.5f, Color.yellow, 1.0f);
        
        // TODO: Add particle effects or sound here
        // Example:
        // if (impactEffectPrefab != null)
        //     Instantiate(impactEffectPrefab, position, Quaternion.LookRotation(normal));
    }
}
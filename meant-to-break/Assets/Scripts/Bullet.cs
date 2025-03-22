using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 20f;
    public float lifetime = 5f; // Maximum bullet lifetime
    
    private Vector3 initialPosition;
    
    void Start()
    {
        // Store initial position for distance calculation
        initialPosition = transform.position;
        
        // Destroy after lifetime expires
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        // Move bullet forward
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Calculate distance traveled
        float distanceTraveled = Vector3.Distance(initialPosition, transform.position);
        
        // Debug info about the impact
        Debug.Log($"Bullet hit: {collision.gameObject.name} | " +
                  $"Would deal {damage} damage | " +
                  $"Distance traveled: {distanceTraveled:F2}m");
        
        // PLACEHOLDER - Instead of applying damage to Health component
        // This would be replaced with actual Health.TakeDamage() when you implement it
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log($"Hit enemy {collision.gameObject.name}! Damage: {damage}");
            // Future: collision.gameObject.GetComponent<Health>().TakeDamage(damage);
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Hit player! This is friendly fire.");
            // Future: Handle friendly fire if needed
        }
        else
        {
            Debug.Log($"Bullet hit environment: {collision.gameObject.name}");
            // Future: Add bullet holes, decals or impact effects here
        }
        
        // Create optional small impact effect
        CreateImpactEffect(collision.contacts[0].point, collision.contacts[0].normal);
        
        // Destroy the bullet
        Destroy(gameObject);
    }
    
    private void CreateImpactEffect(Vector3 position, Vector3 normal)
    {
        // PLACEHOLDER - You can add particle effects later
        Debug.DrawRay(position, normal * 0.5f, Color.yellow, 1.0f);
    }
}
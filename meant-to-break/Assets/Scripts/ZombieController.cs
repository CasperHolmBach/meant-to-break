using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform destination;
    public BoxCollider attackCollider;
    
    [Header("Stats")]
    public int health = 100;
    public int damage = 10;
    public float attackCooldown = 1.0f;
    public float attackRange = 1.5f; // Detection radius for attack
    
    private bool canAttack = true;
    private HealthManager healthManager;
    private Transform playerTransform;
    
    void Start()
    {
        healthManager = GetComponent<HealthManager>();
        if (healthManager == null)
        {
            healthManager = gameObject.AddComponent<HealthManager>();
            healthManager.maxHealth = health;
        }
        
        // Find the player at start
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            destination = playerTransform;
        }
    }
    
    void Update()
    {
        // Update destination if we have one
        if (destination != null)
        {
            agent.SetDestination(destination.position);
        }
        else
        {
            // Try to find the player if destination is not set
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                destination = playerTransform;
            }
        }
        
        // Check for player in attack range and attack if possible
        CheckAttackRange();
        
        // Check if dead
        if (healthManager.GetCurrentHealth() <= 0)
        {
            Die();
        }
    }
    
    // New method to check for attacks every frame
    private void CheckAttackRange()
    {
        // Only proceed if we can attack and have a player
        if (!canAttack || playerTransform == null)
            return;
            
        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // If player is within attack range
        if (distanceToPlayer <= attackRange)
        {
            // Get the player's health manager
            HealthManager playerHealth = playerTransform.GetComponent<HealthManager>();
            if (playerHealth != null)
            {
                // Attack the player
                playerHealth.TakeDamage(damage);
                Debug.Log($"Zombie attacked player for {damage} damage! Distance: {distanceToPlayer:F2}");
                StartCoroutine(AttackCooldown());
                
                // Optional: Play attack animation here
                PlayAttackAnimation();
            }
        }
    }

    // Keep this for compatibility - will trigger for moving players
    void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player") && canAttack) 
        {
            // Get the player's health manager
            HealthManager playerHealth = other.GetComponent<HealthManager>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Zombie triggered attack on player for {damage} damage!");
                StartCoroutine(AttackCooldown());
            }
        }
    }
    
    // Method that bullets/weapons will call to damage the zombie
    public void TakeDamage(int damageAmount)
    {
        if (healthManager != null)
        {
            // Store health before damage for comparison
            int previousHealth = healthManager.GetCurrentHealth();
            
            // Apply damage
            healthManager.TakeDamage(damageAmount);
            
            // Find camera direction to position popup better
            Vector3 cameraDirection = Vector3.zero;
            if (Camera.main != null)
            {
                cameraDirection = (Camera.main.transform.position - transform.position).normalized;
                // Don't include vertical component
                cameraDirection.y = 0;
                cameraDirection.Normalize();
            }
            
            // Position popup higher and slightly toward camera for better visibility
            Vector3 popupPosition = transform.position + Vector3.up * 2.2f + (cameraDirection * 0.5f);
            
            // Create the popup
            DamagePopup.Create(popupPosition, damageAmount);
            
            Debug.Log($"Zombie took {damageAmount} damage! Health: {healthManager.GetCurrentHealth()}");
            
            // Check if zombie died
            if (healthManager.GetCurrentHealth() <= 0 && previousHealth > 0)
            {
                Die();
            }
        }
    }
    
    // Legacy method to support older scripts
    public void ApplyDamage(float damageAmount)
    {
        TakeDamage(Mathf.RoundToInt(damageAmount));
    }
    
    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    
    private void PlayAttackAnimation()
    {
        // Add your attack animation code here
        // Example:
        // animator.SetTrigger("Attack");
    }
    
    private void Die()
    {
        // Play death animation or effects here
        
        // Disable components
        if (agent != null) agent.enabled = false;
        if (attackCollider != null) attackCollider.enabled = false;
        
        // Disable all colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
        
        // Destroy the zombie object
        Destroy(gameObject);
    }
    
    // Optional: Visualize attack range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

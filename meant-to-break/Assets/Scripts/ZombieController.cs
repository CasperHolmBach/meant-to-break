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
            healthManager.TakeDamage(damageAmount);
            Debug.Log($"Zombie took {damageAmount} damage! Health: {healthManager.GetCurrentHealth()}");
            
            if (healthManager.GetCurrentHealth() <= 0)
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

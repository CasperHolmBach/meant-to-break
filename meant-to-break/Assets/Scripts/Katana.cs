using UnityEngine;
using System.Collections.Generic;

public class Katana : MonoBehaviour, IWeapon
{
    [Header("Rotation Settings")]
    [SerializeField] private float xRotationAmount = 3f; // New X rotation parameter
    [SerializeField] private float yRotationAmount = 5f;
    [SerializeField] private float zRotationAmount = -3f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashCooldown = 3f;
    [SerializeField] private Camera playerCamera; // Add reference to player camera
    [SerializeField] private FPSController playerController; // Add reference to player controller
    private float nextDashTime = 0f;
    
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Damage Settings")]
    [SerializeField] private int damage = 35;  // Base damage for each hit
    [SerializeField] private float attackRadius = 1.5f; // How far to check for enemies
    [SerializeField] private float attackAngle = 60f; // Attack arc in degrees
    [SerializeField] private LayerMask enemyLayers; // Set to layers containing enemies
    [SerializeField] private Transform attackOrigin; // Where the attack ray starts from
    [SerializeField] private bool showDebugRays = false; // Visual debug option
    private List<GameObject> hitEnemies = new List<GameObject>(); // Prevents hitting the same enemy multiple times
    
    [Header("Wind Effect")]
    [SerializeField] private GameObject windEffectPrefab;
    [SerializeField] private Transform[] edgePoints; // Empty GameObjects positioned along the blade edge
    [SerializeField] private float windEffectDuration = 0.5f;
    [SerializeField] private float windEffectSpeed = 10f;
    [SerializeField] private float windEffectThreshold = 0.3f; // How far into the swing to trigger effect
    
    [Header("Audio")]
    [SerializeField] private AudioClip[] slashSounds; // Array of different slash sounds for variety
    [SerializeField] private AudioClip[] dashSounds; // Array of different dash sounds
    [SerializeField] private AudioClip dashFailSound; // Sound when dash is on cooldown
    [SerializeField] private AudioClip hitEnemySound;
    [SerializeField] private float slashVolume = 0.7f;
    [SerializeField] private float dashVolume = 1.0f;
    [SerializeField] private float failVolume = 0.5f;
    [SerializeField] private float hitVolume = 0.8f;
    private AudioSource audioSource;
    
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private Quaternion initialRotation;
    private Quaternion targetRotation;
    private bool windEffectTriggered = false;
    private bool damageApplied = false;
    
    private void Start()
    {
        initialRotation = transform.localRotation;
        
        // Initialize references if not set in inspector
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        if (playerController == null)
            playerController = FindObjectOfType<FPSController>();
            
        if (attackOrigin == null)
            attackOrigin = transform; // Default to this object's transform
            
        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        // Set up enemy layers if not configured
        if (enemyLayers == 0)
            enemyLayers = LayerMask.GetMask("Enemies", "Enemy", "Default");
    }
    
    private void Update()
    {
        // Handle attack cooldown
        if (Input.GetMouseButtonDown(0)) // Left mouse button (Mouse1)
        {
            Mouse1();
        }
        
        if (Input.GetMouseButtonDown(1)) // Right mouse button (Mouse2)
        {
            Mouse2();
        }
        
        if (isAttacking)
        {
            attackTimer += Time.deltaTime;
            float normalizedTime = attackTimer / attackCooldown;
            float curveValue = rotationCurve.Evaluate(normalizedTime);
            
            transform.localRotation = Quaternion.Slerp(initialRotation, targetRotation, curveValue);
            
            // Apply damage at the peak of the swing
            if (normalizedTime > 0.3f && normalizedTime < 0.7f && !damageApplied)
            {
                ApplyDamage();
                damageApplied = true;
            }
            
            if (normalizedTime > windEffectThreshold && !windEffectTriggered)
            {
                CreateWindEffect();
                windEffectTriggered = true;
            }
            
            if (attackTimer >= attackCooldown)
            {
                // Reset after attack
                transform.localRotation = initialRotation;
                isAttacking = false;
                attackTimer = 0f;
                windEffectTriggered = false;
                damageApplied = false;
                hitEnemies.Clear(); // Clear the hit enemies list for the next attack
            }
        }
    }
    
    public void Mouse1()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            attackTimer = 0f;
            
            // Calculate new rotation angles
            Vector3 newRotation = initialRotation.eulerAngles;
            newRotation.x += xRotationAmount;
            newRotation.y += yRotationAmount;
            newRotation.z += zRotationAmount;
            
            // Set the target rotation
            targetRotation = Quaternion.Euler(newRotation);
            
            // Play slash sound
            PlayRandomSlashSound();
        }
    }

    public void Mouse2()
    {
        if (Time.time >= nextDashTime)
        {
            // Use cached references instead of finding them every time
            if (playerController != null && playerCamera != null)
            {
                // Get the forward direction from the camera
                Vector3 dashDirection = playerCamera.transform.forward;
                
                // Apply dash force
                playerController.ApplyKnockback(dashDirection * dashForce);
                
                // Set cooldown
                nextDashTime = Time.time + dashCooldown;
                Debug.Log("Katana dash activated!");
                
                // Play dash sound
                PlayRandomDashSound();
            }
            else
            {
                // Try to find references again if they're missing
                playerCamera = Camera.main;
                playerController = FindObjectOfType<FPSController>();
                
                if (playerController != null && playerCamera != null)
                {
                    // Retry with newly found references
                    Mouse2();
                }
                else
                {
                    Debug.LogWarning("Missing references for dash! Make sure there's an FPSController in the scene.");
                }
            }
        }
        else
        {
            float remainingTime = nextDashTime - Time.time;
            Debug.Log($"Katana dash on cooldown ({remainingTime:F1}s remaining)");
            
            // Play failure sound for cooldown
            PlayDashFailSound();
        }
    }
    
    // New method to apply damage to enemies in swing arc
    private void ApplyDamage()
    {
        // Get direction the player/camera is facing
        Vector3 forwardDirection = playerCamera.transform.forward;
        Vector3 playerPosition = playerCamera.transform.position;
        
        Collider[] hitColliders = Physics.OverlapSphere(attackOrigin.position, attackRadius, enemyLayers);
        
        bool hitAny = false;
        
        foreach (var hitCollider in hitColliders)
        {
            // Skip if we've already hit this enemy in the current swing
            if (hitEnemies.Contains(hitCollider.gameObject))
                continue;
                
            // Get direction to the potential target
            Vector3 directionToTarget = (hitCollider.transform.position - attackOrigin.position).normalized;
            
            // Calculate angle between forward direction and direction to target
            float angle = Vector3.Angle(forwardDirection, directionToTarget);
            
            if (angle <= attackAngle / 2)
            {
                // Check if we can see the enemy (no walls in between)
                RaycastHit hit;
                bool lineOfSight = !Physics.Raycast(
                    attackOrigin.position, 
                    directionToTarget, 
                    out hit, 
                    attackRadius,
                    ~enemyLayers // Ignore enemy layers
                ) || hit.collider == hitCollider;
                
                if (lineOfSight)
                {
                    if (showDebugRays)
                    {
                        Debug.DrawRay(attackOrigin.position, directionToTarget * attackRadius, 
                                    Color.red, 1.0f);
                    }
                    
                    // Try to get the enemy health component
                    ZombieController zombie = hitCollider.GetComponent<ZombieController>();
                    if (zombie != null)
                    {
                        // Apply damage to the zombie
                        zombie.TakeDamage(damage);
                        
                        // Add to hit enemies to prevent multiple hits in one swing
                        hitEnemies.Add(hitCollider.gameObject);
                        
                        // Play hit sound
                        PlayHitSound();
                        
                        Debug.Log($"Katana hit zombie for {damage} damage!");
                        hitAny = true;
                    }
                }
                else if (showDebugRays)
                {
                    // Show blocked rays
                    Debug.DrawRay(attackOrigin.position, directionToTarget * hit.distance, 
                                Color.yellow, 1.0f);
                    Debug.DrawRay(hit.point, directionToTarget * (attackRadius - hit.distance), 
                                Color.gray, 1.0f);
                }
            }
        }
        
        // If we didn't hit anything, just play the slash sound
        if (!hitAny)
        {
            // Already played at the start of Mouse1
        }
    }
    
    // Play a random slash sound for variety
    private void PlayRandomSlashSound()
    {
        if (audioSource != null && slashSounds != null && slashSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, slashSounds.Length);
            audioSource.PlayOneShot(slashSounds[randomIndex], slashVolume);
        }
    }
    
    // Play hit sound when we hit an enemy
    private void PlayHitSound()
    {
        if (audioSource != null && hitEnemySound != null)
        {
            audioSource.PlayOneShot(hitEnemySound, hitVolume);
        }
    }
    
    // Play a random dash sound
    private void PlayRandomDashSound()
    {
        if (audioSource != null && dashSounds != null && dashSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, dashSounds.Length);
            audioSource.PlayOneShot(dashSounds[randomIndex], dashVolume);
        }
    }
    
    // Play the dash cooldown/fail sound
    private void PlayDashFailSound()
    {
        if (audioSource != null && dashFailSound != null)
        {
            audioSource.PlayOneShot(dashFailSound, failVolume);
        }
    }
    
    private void CreateWindEffect()
    {
        if (windEffectPrefab == null || edgePoints.Length == 0)
            return;
            
        Vector3 swingDirection = transform.right; // Adjust based on your model orientation
        
        foreach (Transform edgePoint in edgePoints)
        {
            GameObject windInstance = Instantiate(windEffectPrefab, edgePoint.position, Quaternion.identity);
            WindEffect windEffect = windInstance.GetComponent<WindEffect>();
            
            if (windEffect != null)
            {
                windEffect.Initialize(swingDirection, windEffectSpeed, windEffectDuration);
            }
            else
            {
                // If no custom WindEffect script, just destroy after duration
                Destroy(windInstance, windEffectDuration);
            }
        }
    }

    public float GetRemainingDashCooldown()
    {
        if (nextDashTime > Time.time)
            return nextDashTime - Time.time;
        else
            return 0f;
    }

    public float GetDashCooldownTime()
    {
        return dashCooldown;
    }

    // Optional: Visualize the attack arc in the editor
    private void OnDrawGizmosSelected()
    {
        if (attackOrigin == null)
            return;
            
        // Draw attack radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin.position, attackRadius);
        
        // Draw attack angle
        if (playerCamera != null)
        {
            Vector3 forward = playerCamera.transform.forward;
            
            // Draw the forward direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(attackOrigin.position, forward * attackRadius);
            
            // Draw the attack angle boundaries
            Gizmos.color = Color.yellow;
            Vector3 rightBoundary = Quaternion.Euler(0, attackAngle / 2, 0) * forward;
            Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle / 2, 0) * forward;
            
            Gizmos.DrawRay(attackOrigin.position, rightBoundary * attackRadius);
            Gizmos.DrawRay(attackOrigin.position, leftBoundary * attackRadius);
        }
    }
}
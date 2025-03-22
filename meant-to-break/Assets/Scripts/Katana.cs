using UnityEngine;

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
    
    [Header("Wind Effect")]
    [SerializeField] private GameObject windEffectPrefab;
    [SerializeField] private Transform[] edgePoints; // Empty GameObjects positioned along the blade edge
    [SerializeField] private float windEffectDuration = 0.5f;
    [SerializeField] private float windEffectSpeed = 10f;
    [SerializeField] private float windEffectThreshold = 0.3f; // How far into the swing to trigger effect
    
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private Quaternion initialRotation;
    private Quaternion targetRotation;
    private bool windEffectTriggered = false;
    
    private void Start()
    {
        initialRotation = transform.localRotation;
        
        // Initialize references if not set in inspector
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        if (playerController == null)
            playerController = FindObjectOfType<FPSController>();
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
}
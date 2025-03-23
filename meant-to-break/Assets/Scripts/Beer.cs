using UnityEngine;

public class Beer : MonoBehaviour
{
    [Header("Healing Properties")]
    public int healAmount = 30;
    public float destroyDelay = 0.5f;
    public bool rotateItem = true;   
    public float rotationSpeed = 30f; 
    
    [Header("Audio")]
    public AudioClip drinkingSound;
    [SerializeField] private float volume = 1.0f;
    
    [Header("Visual Effects")]
    public GameObject collectEffectPrefab; 
    public bool bobUpAndDown = true;      
    public float bobHeight = 0.2f;         
    public float bobSpeed = 1f;          
    
    private Vector3 startPosition;
    private float bobTime;
    
    void Start()
    {
        // Store initial position for bobbing effect
        startPosition = transform.position;
        
        // Start at a random point in the bob cycle
        bobTime = Random.Range(0f, 2f * Mathf.PI);
    }
    
    void Update()
    {
        if (bobUpAndDown)
        {
            // Apply bobbing motion
            bobTime += Time.deltaTime * bobSpeed;
            float yOffset = Mathf.Sin(bobTime) * bobHeight;
            transform.position = new Vector3(
                startPosition.x, 
                startPosition.y + yOffset, 
                startPosition.z);
        }
        
        if (rotateItem)
        {
            // Rotate around the Y axis
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            // Try to get the player's health manager
            HealthManager playerHealth = other.GetComponent<HealthManager>();
            if (playerHealth != null)
            {
                // Apply healing
                playerHealth.Applyhealing(gameObject, healAmount);
                
                // Play drinking sound
                PlayDrinkingSound(other.transform.position);
                
                // Show collection effect
                SpawnCollectEffect();
                
                // Disable collider and renderer immediately
                DisableItemVisuals();
                
                // The Applyhealing method will handle destroying this object
            }
            else
            {
                Debug.LogWarning("Player doesn't have a HealthManager component!");
            }
        }
    }
    
    private void PlayDrinkingSound(Vector3 position)
    {
        if (drinkingSound != null)
        {
            // Play sound at the player's position
            AudioSource.PlayClipAtPoint(drinkingSound, position, volume);
        }
    }
    
    private void SpawnCollectEffect()
    {
        if (collectEffectPrefab != null)
        {
            // Create collect effect at the beer's position
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
        }
    }
    
    private void DisableItemVisuals()
    {
        // Disable collider so it can't be picked up again
        Collider itemCollider = GetComponent<Collider>();
        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }
        
        // Hide the model
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }
        
        // Keep the object around briefly for audio to play
        if (drinkingSound != null && drinkingSound.length > destroyDelay)
        {
            Destroy(gameObject, drinkingSound.length);
        }
        else
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}
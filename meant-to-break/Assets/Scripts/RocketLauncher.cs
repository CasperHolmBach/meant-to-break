using UnityEngine;
using System.Collections;

public class RocketLauncher : MonoBehaviour, IWeapon
{
    public GameObject rocketPrefab;
    [SerializeField] private Camera playerCamera;
    public float fireDistance = 100f;
    
    [Header("Cooldown Settings")]
    public float cooldownTime = 2f;
    private float nextFireTime = 0f;
    
    [Header("Visual Effects")]
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private float muzzleFlashDuration = 0.1f;
    [SerializeField] private Light muzzleLight;
    [SerializeField] private float muzzleLightIntensity = 3f;
    [SerializeField] private float muzzleLightDuration = 0.1f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptySound;
    [SerializeField] private float fireSoundVolume = 0.7f;
    private AudioSource audioSource;
    private GameObject activeMuzzleFlash;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        // Make sure muzzle light is off initially
        if (muzzleLight != null)
            muzzleLight.enabled = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Mouse1();
        }
    }

    public void Mouse1()
    {
        // Check if enough time has passed since last fire
        if (Time.time >= nextFireTime)
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            Vector3 targetPoint;

            if (Physics.Raycast(ray, out hit, fireDistance))
            {
                targetPoint = hit.point; 
            }
            else
            {
                targetPoint = ray.origin + ray.direction * fireDistance; 
            }

            // Play fire sound
            PlayFireSound();
            
            // Show muzzle effects
            CreateMuzzleEffects();
            
            // Create launch position 
            Vector3 spawnPosition;
            if (muzzlePoint != null)
                spawnPosition = muzzlePoint.position;
            else
                spawnPosition = playerCamera.transform.position + playerCamera.transform.forward * 1.5f;
            
            // Create rocket
            GameObject rocket = Instantiate(rocketPrefab, spawnPosition, Quaternion.identity);
            rocket.transform.LookAt(targetPoint);
            
            // Set the next time when firing is allowed
            nextFireTime = Time.time + cooldownTime;
            Debug.Log("Rocket fired! Next fire available in " + cooldownTime + " seconds");
            
            // Play reload sound with delay
            if (reloadSound != null)
                StartCoroutine(PlayDelayedReloadSound(cooldownTime * 0.5f));
        }
        else
        {
            // Play empty/failure sound
            if (emptySound != null)
                audioSource.PlayOneShot(emptySound, 0.5f);
                
            // Calculate and show remaining cooldown time
            float remainingTime = nextFireTime - Time.time;
            Debug.Log($"Rocket Launcher on cooldown ({remainingTime:F1}s remaining)");
        }
    }

    public void Mouse2()
    {
        Debug.Log("Mouse 2: Rocket Launcher");
    }
    
    // Method to get remaining cooldown time (for UI)
    public float GetRemainingCooldown()
    {
        if (nextFireTime > Time.time)
            return nextFireTime - Time.time;
        else
            return 0f;
    }
    
    private void PlayFireSound()
    {
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound, fireSoundVolume);
        }
    }
    
    private IEnumerator PlayDelayedReloadSound(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (reloadSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(reloadSound, 0.6f);
        }
    }
    
    private void CreateMuzzleEffects()
    {
        // Create muzzle flash
        if (muzzleFlashPrefab != null && muzzlePoint != null)
        {
            // Clean up existing flash if needed
            if (activeMuzzleFlash != null)
                Destroy(activeMuzzleFlash);
                
            activeMuzzleFlash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);
            Destroy(activeMuzzleFlash, muzzleFlashDuration);
        }
        
        // Create muzzle light effect
        if (muzzleLight != null)
        {
            StartCoroutine(FlashMuzzleLight());
        }
    }
    
    private IEnumerator FlashMuzzleLight()
    {
        float originalIntensity = muzzleLight.intensity;
        bool originalEnabled = muzzleLight.enabled;
        
        muzzleLight.intensity = muzzleLightIntensity;
        muzzleLight.enabled = true;
        
        yield return new WaitForSeconds(muzzleLightDuration);
        
        muzzleLight.intensity = originalIntensity;
        muzzleLight.enabled = originalEnabled;
    }
}

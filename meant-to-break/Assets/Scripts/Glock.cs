using System.Collections;
using UnityEngine;

public class Glock : MonoBehaviour, IWeapon
{
    [Header("Weapon Properties")]
    public GameObject bulletPrefab;
    [SerializeField] private Camera playerCamera;
    public float fireDistance = 100f;
    public float bulletSpeed = 50f;
    public float bulletDamage = 20f;
    
    [Header("Visual Effects")]
    [SerializeField] private Transform muzzlePoint; 
    [SerializeField] private GameObject muzzleFlashPrefab; 
    [SerializeField] private float muzzleFlashDuration = 0.05f;
    [SerializeField] private Light muzzleLight;
    [SerializeField] private float muzzleLightIntensity = 2f; 
    [SerializeField] private float muzzleLightDuration = 0.05f; 
    
    [Header("Ammo Settings")]
    [SerializeField] private int maxAmmo = 18;
    [SerializeField] private int currentAmmo = 18;
    [SerializeField] private float fireRate = 0.3f; 
    [SerializeField] private float reloadTime = 1.5f;
    
    private float nextFireTime = 0f;
    private bool isReloading = false;
    private float reloadFinishTime = 0f;
    private GameObject activeMuzzleFlash = null;
    
    // Track active coroutines
    private Coroutine reloadCoroutine;
    private Coroutine muzzleLightCoroutine;
    
    // Audio
    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptySound;
    [SerializeField] private float shootVolume = 0.25f;
    [SerializeField] private float reloadVolume = 1.0f;
    [SerializeField] private float emptyVolume = 1.0f;
    private AudioSource audioSource;
    
    // Events
    public delegate void AmmoChangeHandler(int current, int max);
    public event AmmoChangeHandler OnAmmoChanged;
    
    public delegate void ReloadHandler(bool isReloading, float timeRemaining);
    public event ReloadHandler OnReloadStateChanged;

    void Start()
    {
        // If camera reference is missing, try to get the main camera
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // Make sure muzzle light is off initially
        if (muzzleLight != null)
        {
            muzzleLight.enabled = false;
        }
            
        // Notify UI of initial ammo state
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
    }
    
    private void OnEnable()
    {
        // Reset weapon state when enabled
        nextFireTime = 0f; 
        
        // Cancel any existing reload
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }
        
        // Cancel any muzzle light effect
        if (muzzleLightCoroutine != null)
        {
            StopCoroutine(muzzleLightCoroutine);
            muzzleLightCoroutine = null;
        }
        
        // Make sure muzzle light is off
        if (muzzleLight != null)
        {
            muzzleLight.enabled = false;
        }
        
        // Clean up any existing muzzle flash
        if (activeMuzzleFlash != null)
        {
            Destroy(activeMuzzleFlash);
            activeMuzzleFlash = null;
        }
        
        // Reset reload state
        if (isReloading)
        {
            isReloading = false;
            OnReloadStateChanged?.Invoke(false, 0);
        }
        
        // Refresh UI
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
        
        Debug.Log("Glock enabled and reset");
    }
    
    private void OnDisable()
    {
        // Stop coroutines
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }
        
        if (muzzleLightCoroutine != null)
        {
            StopCoroutine(muzzleLightCoroutine);
            muzzleLightCoroutine = null;
        }
        
        // Clean up effects
        if (activeMuzzleFlash != null)
        {
            Destroy(activeMuzzleFlash);
            activeMuzzleFlash = null;
        }
        
        if (muzzleLight != null)
        {
            muzzleLight.enabled = false;
        }
        
        Debug.Log("Glock disabled and cleanup performed");
    }

    void Update()
    {
        // Only process if active
        if (!gameObject.activeInHierarchy) return;
        
        // Handle firing with left mouse button
        if (Input.GetMouseButton(0) && !isReloading)
        {
            Mouse1();
        }
        
        // Handle reloading with R key
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            StartReload();
        }
        
        // Update reload progress
        if (isReloading && OnReloadStateChanged != null)
        {
            float timeRemaining = reloadFinishTime - Time.time;
            OnReloadStateChanged(isReloading, timeRemaining);
        }
    }

    public void Mouse1()
    {
        // Check if enough time has passed and we have ammo
        if (Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                FireBullet();
            }
            else
            {
                // Out of ammo sound at normal volume
                if (emptySound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(emptySound, emptyVolume);
                }
                Debug.Log("Glock: Out of ammo! Press R to reload.");
                
                // Auto-reload when empty
                StartReload();
            }
        }
    }

    public void Mouse2()
    {
        // Optional secondary fire (perhaps aim down sights in the future)
        Debug.Log("Glock: Mouse2 action");
    }
    
    private void FireBullet()
    {
        // Calculate firing direction using raycast
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
        
        // Determine muzzle position
        Vector3 muzzlePosition;
        if (muzzlePoint != null)
        {
            // Use defined muzzle point if available
            muzzlePosition = muzzlePoint.position;
        }
        else
        {
            // Fallback to camera position + offset
            muzzlePosition = playerCamera.transform.position + playerCamera.transform.forward * 0.5f;
        }
        
        // Create muzzle flash effect
        CreateMuzzleFlash();
        
        // Instantiate bullet
        GameObject bullet = Instantiate(bulletPrefab, muzzlePosition, Quaternion.identity);
        
        // Set bullet direction
        bullet.transform.LookAt(targetPoint);
        
        // Add bullet script if not already on the prefab
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript == null)
        {
            bulletScript = bullet.AddComponent<Bullet>();
        }
        
        // Set bullet properties
        bulletScript.speed = bulletSpeed;
        bulletScript.damage = bulletDamage;
        
        // Play sound at 50% volume
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound, shootVolume);
        }
        
        // Reduce ammo
        currentAmmo--;
        
        // Update UI
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
        
        // Set next fire time
        nextFireTime = Time.time + fireRate;
    }
    
    private void CreateMuzzleFlash()
    {
        if (muzzleFlashPrefab != null && muzzlePoint != null)
        {
            // Clean up any existing muzzle flash
            if (activeMuzzleFlash != null)
            {
                Destroy(activeMuzzleFlash);
            }
            
            // Create new muzzle flash
            activeMuzzleFlash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);
            
            // Destroy after duration
            Destroy(activeMuzzleFlash, muzzleFlashDuration);
        }
        
        // Light flash effect
        if (muzzleLight != null)
        {
            // Cancel existing coroutine if any
            if (muzzleLightCoroutine != null)
            {
                StopCoroutine(muzzleLightCoroutine);
            }
            
            // Start new coroutine
            muzzleLightCoroutine = StartCoroutine(FlashMuzzleLight());
        }
    }
    
    private IEnumerator FlashMuzzleLight()
    {
        // Store initial light settings
        float originalIntensity = muzzleLight.intensity;
        bool originalEnabled = muzzleLight.enabled;
        
        // Turn on light with flash intensity
        muzzleLight.intensity = muzzleLightIntensity;
        muzzleLight.enabled = true;
        
        // Wait for duration
        yield return new WaitForSeconds(muzzleLightDuration);
        
        // Only turn light off if we're still active
        if (gameObject.activeInHierarchy)
        {
            // Restore original settings
            muzzleLight.intensity = originalIntensity;
            muzzleLight.enabled = originalEnabled;
        }
        
        muzzleLightCoroutine = null;
    }
    
    private void StartReload()
    {
        if (currentAmmo == maxAmmo) return;
        
        // Stop any existing reload coroutine
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
        }
        
        // Start new reload coroutine
        reloadCoroutine = StartCoroutine(ReloadRoutine());
    }
    
    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        reloadFinishTime = Time.time + reloadTime;
        
        // Play reload sound at normal volume
        if (reloadSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(reloadSound, reloadVolume);
        }
        
        Debug.Log("Glock: Reloading... (" + reloadTime + "s)");
        
        // Notify UI of reload start
        OnReloadStateChanged?.Invoke(true, reloadTime);
        
        yield return new WaitForSeconds(reloadTime);
        
        // Only complete reload if still active
        if (gameObject.activeInHierarchy)
        {
            FinishReload();
        }
        
        reloadCoroutine = null;
    }
    
    private void FinishReload()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
        
        Debug.Log("Glock: Reloaded! Ammo: " + currentAmmo + "/" + maxAmmo);
        
        // Update UI
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
        
        // Notify UI of reload end
        OnReloadStateChanged?.Invoke(false, 0);
    }
    
    // Public getters for UI
    public int GetCurrentAmmo() { return currentAmmo; }
    public int GetMaxAmmo() { return maxAmmo; }
    public bool IsReloading() { return isReloading; }
    public float GetReloadTimeRemaining() { return Mathf.Max(0, reloadFinishTime - Time.time); }
    public float GetReloadTotalTime() { return reloadTime; }
}
using System.Collections;
using UnityEngine;

public class SMG : MonoBehaviour, IWeapon
{
    [Header("Weapon Properties")]
    [SerializeField] private float damage = 12f;
    [SerializeField] private float fireRate = 0.1f; 
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private float reloadTime = 1.5f;

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private ParticleSystem muzzleFlash;
    
    [Header("Audio")]
    [SerializeField] private AudioClip gunShotClip; 
    [SerializeField] private AudioClip reloadClip;
    [SerializeField] private float gunShotVolume = 0.5f;
    [SerializeField] private float reloadVolume = 0.7f;
    
    private int currentAmmo;
    private bool canFire = true;
    private bool isReloading = false;
    private bool isFiring = false; // Track if we're currently in firing mode
    private float reloadFinishTime = 0f;
    
    // Track active coroutines to properly stop them when needed
    private Coroutine reloadCoroutine;
    private Coroutine cooldownCoroutine;
    
    // Events for UI updates
    public delegate void AmmoChangeHandler(int current, int max);
    public event AmmoChangeHandler OnAmmoChanged;
    
    public delegate void ReloadHandler(bool isReloading, float timeRemaining);
    public event ReloadHandler OnReloadStateChanged;

    private void Start()
    {
        currentAmmo = maxAmmo;
        
        // Notify UI of initial ammo state
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
    }

    private void OnEnable()
    {
        // Reset weapon state when enabled
        isReloading = false;
        canFire = true;
        isFiring = false;
        
        // Cancel any ongoing coroutines from previous usage
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }
        
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }
        
        // Notify UI of current ammo state
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
        OnReloadStateChanged?.Invoke(false, 0);
        
        Debug.Log("SMG enabled and reset");
    }

    private void OnDisable()
    {
        // Clean up on disable
        isReloading = false;
        isFiring = false;
        
        // Stop any active coroutines
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }
        
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }
        
        Debug.Log("SMG disabled and cleanup performed");
    }

    private void Update()
    {
        // Only check for input if we're active
        if (!gameObject.activeInHierarchy) return;
        
        if (Input.GetMouseButton(0) && !isReloading) 
        {
            if (canFire)
            {
                Mouse1();
            }
        }

        // Reload
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            StartReload();
        }
        
        // Update reload progress for UI
        if (isReloading && OnReloadStateChanged != null)
        {
            float timeRemaining = reloadFinishTime - Time.time;
            OnReloadStateChanged(isReloading, timeRemaining);
        }
    }

    public void Mouse2()
    {
        Debug.Log("Mouse2 SMG");
    }   

    public void Mouse1()
    {
        if (currentAmmo <= 0)
        {
            StartReload();
            return;
        }

        // Use ammo
        currentAmmo--;
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
        
        // Fire effects
        if (muzzleFlash != null) muzzleFlash.Play();
        
        // Play gunshot sound using AudioClip
        if (gunShotClip != null)
        {
            AudioSource.PlayClipAtPoint(gunShotClip, transform.position, gunShotVolume);
        }

        // Instantiate bullet
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            SMGBullet bulletScript = bullet.GetComponent<SMGBullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDamage(damage);
            }
        }
        
        // Set cooldown
        canFire = false;
        StartCooldown();
    }
    
    private void StartCooldown()
    {
        // Stop any existing cooldown
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
        }
        
        // Start new cooldown
        cooldownCoroutine = StartCoroutine(ResetFireCooldown());
    }
    
    private void StartReload()
    {
        // Stop any existing reload
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
        }
        
        // Start new reload
        reloadCoroutine = StartCoroutine(Reload());
    }
    
    private IEnumerator ResetFireCooldown()
    {
        yield return new WaitForSeconds(fireRate);
        canFire = true;
        cooldownCoroutine = null;
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        reloadFinishTime = Time.time + reloadTime;
        Debug.Log("Reloading SMG...");
        
        // Notify UI of reload start
        OnReloadStateChanged?.Invoke(true, reloadTime);

        // Play reload sound
        if (reloadClip != null)
        {
            AudioSource.PlayClipAtPoint(reloadClip, transform.position, reloadVolume);
        }

        yield return new WaitForSeconds(reloadTime);

        // Check if we're still active - the weapon might have been disabled during reload
        if (gameObject.activeInHierarchy)
        {
            currentAmmo = maxAmmo;
            isReloading = false;
            
            // Update UI
            OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
            OnReloadStateChanged?.Invoke(false, 0);
        }
        
        reloadCoroutine = null;
    }

    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetMaxAmmo()
    {
        return maxAmmo;
    }
    
    public bool IsReloading()
    {
        return isReloading;
    }
    
    public float GetReloadTimeRemaining()
    {
        return Mathf.Max(0, reloadFinishTime - Time.time);
    }
    
    public float GetReloadTotalTime()
    {
        return reloadTime;
    }
}
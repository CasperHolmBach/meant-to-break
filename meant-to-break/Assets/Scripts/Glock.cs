using UnityEngine;

public class Glock : MonoBehaviour, IWeapon
{
    [Header("Weapon Properties")]
    public GameObject bulletPrefab;
    [SerializeField] private Camera playerCamera;
    public float fireDistance = 100f;
    public float bulletSpeed = 50f;
    public float bulletDamage = 20f;
    
    [Header("Ammo Settings")]
    [SerializeField] private int maxAmmo = 18;
    [SerializeField] private int currentAmmo = 18;
    [SerializeField] private float fireRate = 0.3f; // Time between shots in seconds
    [SerializeField] private float reloadTime = 1.5f; // Time to reload in seconds
    
    private float nextFireTime = 0f;
    private bool isReloading = false;
    private float reloadFinishTime = 0f;
    
    // Audio
    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptySound;
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
            
        // Notify UI of initial ammo state
        if (OnAmmoChanged != null)
            OnAmmoChanged(currentAmmo, maxAmmo);
    }

    void Update()
    {
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
        
        // Check if reload is complete
        if (isReloading && Time.time >= reloadFinishTime)
        {
            FinishReload();
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
                // Out of ammo sound
                if (emptySound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(emptySound);
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
        
        // Instantiate bullet at gun muzzle position
        Vector3 muzzlePosition = playerCamera.transform.position + playerCamera.transform.forward * 0.5f;
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
        
        // Play sound
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        
        // Reduce ammo
        currentAmmo--;
        
        // Update UI
        if (OnAmmoChanged != null)
            OnAmmoChanged(currentAmmo, maxAmmo);
        
        // Set next fire time
        nextFireTime = Time.time + fireRate;
    }
    
    private void StartReload()
    {
        if (currentAmmo == maxAmmo) return;
        
        isReloading = true;
        reloadFinishTime = Time.time + reloadTime;
        
        // Play reload sound
        if (reloadSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }
        
        Debug.Log("Glock: Reloading... (" + reloadTime + "s)");
        
        // Notify UI of reload start
        if (OnReloadStateChanged != null)
            OnReloadStateChanged(true, reloadTime);
    }
    
    private void FinishReload()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
        
        Debug.Log("Glock: Reloaded! Ammo: " + currentAmmo + "/" + maxAmmo);
        
        // Update UI
        if (OnAmmoChanged != null)
            OnAmmoChanged(currentAmmo, maxAmmo);
            
        // Notify UI of reload end
        if (OnReloadStateChanged != null)
            OnReloadStateChanged(false, 0);
    }
    
    // Public getters for UI
    public int GetCurrentAmmo() { return currentAmmo; }
    public int GetMaxAmmo() { return maxAmmo; }
    public bool IsReloading() { return isReloading; }
    public float GetReloadTimeRemaining() { return Mathf.Max(0, reloadFinishTime - Time.time); }
    public float GetReloadTotalTime() { return reloadTime; }
}
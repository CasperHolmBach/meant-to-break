using UnityEngine;

public class RocketLauncher : MonoBehaviour, IWeapon
{
    public GameObject rocketPrefab;
    [SerializeField] private Camera playerCamera;
    public float fireDistance = 100f;
    
    [Header("Cooldown Settings")]
    public float cooldownTime = 2f;
    private float nextFireTime = 0f;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
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

            GameObject rocket = Instantiate(rocketPrefab, playerCamera.transform.position + playerCamera.transform.forward * 1.5f, Quaternion.identity);

            rocket.transform.LookAt(targetPoint);
            
            // Set the next time when firing is allowed
            nextFireTime = Time.time + cooldownTime;
            Debug.Log("Rocket fired! Next fire available in " + cooldownTime + " seconds");
        }
        else
        {
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
}

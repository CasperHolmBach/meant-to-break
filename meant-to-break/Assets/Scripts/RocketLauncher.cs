using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    public GameObject rocketPrefab;
    public Transform firePoint;
    public float fireRate = 1f;

    private float nextFireTime = 0f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void Shoot()
    {
        Instantiate(rocketPrefab, firePoint.position, firePoint.rotation);
    }
}

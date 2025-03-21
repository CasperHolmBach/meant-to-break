using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    public GameObject rocketPrefab;
    public Transform firePoint;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        Instantiate(rocketPrefab, firePoint.position, firePoint.rotation);
    }
}

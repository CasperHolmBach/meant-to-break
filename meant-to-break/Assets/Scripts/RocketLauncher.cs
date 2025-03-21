using UnityEngine;

public class RocketLauncher : MonoBehaviour, IWeapon
{
    public GameObject rocketPrefab;
    [SerializeField] private Camera playerCamera;
    public float fireDistance = 100f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Mouse1();
        }
    }

    public void Mouse1()
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
    }

    public void Mouse2()
    {
        Console.WriteLine("Mouse 2: Rocket Launcher");
    }
}

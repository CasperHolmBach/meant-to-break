using UnityEngine;
public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private int slotNumber = -1; // 0=Katana, 1=Glock, 2=SMG, 3=RocketLauncher
    [SerializeField] private string weaponType;
    
    private bool playerInRange = false;
    private Inventory playerInventory = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered weapon pickup trigger");
            playerInRange = true;
            playerInventory = other.GetComponent<Inventory>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited weapon pickup trigger");
            playerInRange = false;
            playerInventory = null;
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && playerInventory != null)
        {
            Debug.Log("Player picked up weapon: " + weaponType);
            playerInventory.UnlockWeapon(slotNumber);
            Destroy(gameObject);
        }
    }
}
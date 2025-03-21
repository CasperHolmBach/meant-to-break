public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private GameObject weaponPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Inventory inventory = other.GetComponent<Inventory>();
            if (inventory != null)
            {
                GameObject weaponObject = Instantiate(weaponPrefab, inventory.transform);
                IWeapon weapon = weaponObject.GetComponent<IWeapon>();
                
                weaponObject.SetActive(false);
                
                inventory.PickUpWeapon(weapon);
                
                Destroy(gameObject);
            }
        }
    }
}
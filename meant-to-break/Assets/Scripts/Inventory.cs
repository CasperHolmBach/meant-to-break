using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [SerializeField] private Transform weaponParent;
    [SerializeField] private GameObject inventoryUI;

    private List<IWeapon> weapons = new List<IWeapon>();
    private int currentWeaponIndex = -1;

    // Event for UI to subscribe to
    public delegate void OnWeaponChanged();
    public event OnInventoryChanged onInventoryChanged;

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (scroll > 0f)
        {
            SelectNextWeapon();
        }
        else if (scroll < 0f)
        {
            SelectPreviousWeapon();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
        }

        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && i < weapons.Count)
            {
                SelectWeapon(i);
                break;
            }
        }
    }
    public void AddWeapon(IWeapon weapon)
    {
        if (!weapons.Contains(weapon))
        {
            weapons.Add(weapon);
            
            // If this is our first weapon, select it
            if (weapons.Count == 1)
            {
                SelectWeapon(0);
            }
            
            UpdateUI();
            onInventoryChanged?.Invoke();
        }
    }

    public void RemoveWeapon(IWeapon weapon)
    {
        int index = weapons.IndexOf(weapon);
        if (index >= 0)
        {
            weapons.RemoveAt(index);
            
            if (index == currentWeaponIndex)
            {
                if (weapons.Count > 0)
                {
                    SelectWeapon(Mathf.Min(index, weapons.Count - 1));
                }
                else
                {
                    currentWeaponIndex = -1;
                }
            }
            else if (index < currentWeaponIndex)
            {
                currentWeaponIndex--;
            }
            
            UpdateUI();
            onInventoryChanged?.Invoke();
        }
    }

    public void AddWeapon(IWeapon weapon)
    {
        if (!weapons.Contains(weapon))
        {
            weapons.Add(weapon);
            
            if (weapons.Count == 1)
            {
                SelectWeapon(0);
            }
            
            UpdateUI();
            onInventoryChanged?.Invoke();
        }
    }

     public IWeapon GetCurrentWeapon()
    {
        if (currentWeaponIndex >= 0 && currentWeaponIndex < weapons.Count)
        {
            return weapons[currentWeaponIndex];
        }
        return null;
    }

    public void SelectWeapon(int index)
    {
        if (index >= 0 && index < weapons.Count && index != currentWeaponIndex)
        {
            if (currentWeaponIndex >= 0 && currentWeaponIndex < weapons.Count)
            {
                DisableWeapon(weapons[currentWeaponIndex]);
            }
            
            currentWeaponIndex = index;
            
            EnableWeapon(weapons[currentWeaponIndex]);
            
            UpdateUI();
        }
    }
    public void SelectNextWeapon()
    {
        if (weapons.Count > 0)
        {
            int nextIndex = (currentWeaponIndex + 1) % weapons.Count;
            SelectWeapon(nextIndex);
        }
    }
        public void SelectPreviousWeapon()
    {
        if (weapons.Count > 0)
        {
            int prevIndex = (currentWeaponIndex - 1 + weapons.Count) % weapons.Count;
            SelectWeapon(prevIndex);
        }
    }
}
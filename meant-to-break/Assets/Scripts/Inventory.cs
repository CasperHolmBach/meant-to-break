using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Weapon References")]
    [SerializeField] private GameObject katanaObject;  // Slot 0
    [SerializeField] private GameObject glockObject;   // Slot 1
    [SerializeField] private GameObject smgObject;     // Slot 2
    [SerializeField] private GameObject rocketLauncherObject; // Slot 3
    
    [SerializeField] private GameObject inventoryUI;
    
    private bool[] weaponUnlocked = new bool[4]; // Array to track unlocked weapons
    private int activeWeaponIndex = -1; // Currently active weapon (-1 means no weapon active)
    
    // Optional event for UI updates
    public event Action<int> OnWeaponChanged;

    private void Start()
    {
        // Make sure all weapons are disabled at start
        DisableAllWeapons();
    }

    private void Update()
    {
        // Check for number key presses to switch weapons
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToWeapon(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToWeapon(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToWeapon(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchToWeapon(3);
        
        // Optional: Toggle inventory UI
        if (Input.GetKeyDown(KeyCode.Tab))
            inventoryUI.SetActive(!inventoryUI.activeSelf);
    }

    // Called from WeaponPickup when a weapon is picked up
    public void UnlockWeapon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponUnlocked.Length)
        {
            Debug.LogError("Invalid weapon slot: " + slotIndex);
            return;
        }
        
        weaponUnlocked[slotIndex] = true;
        Debug.Log($"Weapon in slot {slotIndex} unlocked!");
        
        // If this is our first weapon, automatically equip it
        if (activeWeaponIndex == -1)
        {
            SwitchToWeapon(slotIndex);
        }
        
        // Notify any listeners (like UI elements)
        OnWeaponChanged?.Invoke(activeWeaponIndex);
    }
    
    public void SwitchToWeapon(int slotIndex)
    {
        // Check if the weapon is unlocked
        if (slotIndex >= 0 && slotIndex < weaponUnlocked.Length && weaponUnlocked[slotIndex])
        {
            // First disable all weapons
            DisableAllWeapons();
            
            // Then activate the requested weapon
            switch (slotIndex)
            {
                case 0: // Katana
                    if (katanaObject != null) katanaObject.SetActive(true);
                    break;
                case 1: // Glock
                    if (glockObject != null) glockObject.SetActive(true);
                    break;
                case 2: // SMG
                    if (smgObject != null) smgObject.SetActive(true);
                    break;
                case 3: // Rocket Launcher
                    if (rocketLauncherObject != null) rocketLauncherObject.SetActive(true);
                    break;
            }
            
            activeWeaponIndex = slotIndex;
            Debug.Log($"Switched to weapon {slotIndex}");
            
            // Notify any listeners
            OnWeaponChanged?.Invoke(activeWeaponIndex);
        }
        else
        {
            Debug.Log($"Cannot switch to weapon {slotIndex} - not unlocked or invalid index");
        }
    }
    
    private void DisableAllWeapons()
    {
        if (katanaObject != null) katanaObject.SetActive(false);
        if (glockObject != null) glockObject.SetActive(false);
        if (smgObject != null) smgObject.SetActive(false);
        if (rocketLauncherObject != null) rocketLauncherObject.SetActive(false);
    }
    
    // Helper methods for UI or other systems
    public bool IsWeaponUnlocked(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < weaponUnlocked.Length)
            return weaponUnlocked[slotIndex];
        return false;
    }
    
    public int GetActiveWeaponIndex()
    {
        return activeWeaponIndex;
    }
}
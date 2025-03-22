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
    
    private bool[] weaponUnlocked = new bool[4]; // Array to track unlocked weapons
    private int activeWeaponIndex = -1; // Currently active weapon (-1 means no weapon active)
    
    // Optional event for UI updates
    public event Action<int> OnWeaponChanged;

    private void Start()
    {
        // Make sure all weapons are disabled at start
        DisableAllWeapons();
        
        // Initialize first weapon as unlocked
        weaponUnlocked[0] = true;
        SwitchToWeapon(0);
    }

    private void Update()
    {
        // Check for number key presses to switch weapons
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToWeapon(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToWeapon(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToWeapon(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchToWeapon(3);
        
        // Tab key handling is now done in the InventoryUI script
    }
    
    private void DisableAllWeapons()
    {
        if (katanaObject != null) katanaObject.SetActive(false);
        if (glockObject != null) glockObject.SetActive(false);
        if (smgObject != null) smgObject.SetActive(false);
        if (rocketLauncherObject != null) rocketLauncherObject.SetActive(false);
    }
    
    public void SwitchToWeapon(int slotIndex)
    {
        // Check if weapon is unlocked
        if (slotIndex < 0 || slotIndex >= weaponUnlocked.Length || !weaponUnlocked[slotIndex])
            return;
            
        // If it's already active, don't switch
        if (activeWeaponIndex == slotIndex)
            return;
            
        // Disable currently active weapon
        DisableAllWeapons();
        
        // Enable new weapon
        switch(slotIndex)
        {
            case 0:
                if (katanaObject != null) katanaObject.SetActive(true);
                break;
            case 1:
                if (glockObject != null) glockObject.SetActive(true);
                break;
            case 2:
                if (smgObject != null) smgObject.SetActive(true);
                break;
            case 3:
                if (rocketLauncherObject != null) rocketLauncherObject.SetActive(true);
                break;
        }
        
        // Set new active weapon
        activeWeaponIndex = slotIndex;
        
        // Notify listeners
        OnWeaponChanged?.Invoke(activeWeaponIndex);
    }
    
    public void UnlockWeapon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponUnlocked.Length)
            return;
            
        weaponUnlocked[slotIndex] = true;
        
        // If no weapon is active, auto-switch to this one
        if (activeWeaponIndex == -1)
            SwitchToWeapon(slotIndex);
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
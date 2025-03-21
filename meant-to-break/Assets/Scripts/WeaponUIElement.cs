using UnityEngine;
using TMPro; 
using UnityEngine.UI;

public class WeaponUIElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI weaponNameText; // Changed to TextMeshProUGUI
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image selectionIndicator;

    private IWeapon weapon;
    
    public void SetWeapon(IWeapon weapon, bool isSelected)
    {
        this.weapon = weapon;
        
        MonoBehaviour weaponMono = weapon as MonoBehaviour;
        if (weaponMono != null && weaponNameText != null)
        {
            string name = weaponMono.gameObject.name.Replace("(Clone)", "");
            weaponNameText.text = name;
        }
        
        if (selectionIndicator != null)
        {
            selectionIndicator.enabled = isSelected;
        }
        
        if (backgroundImage != null && isSelected)
        {
            backgroundImage.color = new Color(0.8f, 0.8f, 0.8f);
        }
        else if (backgroundImage != null)
        {
            backgroundImage.color = new Color(0.5f, 0.5f, 0.5f);
        }
    }
    
    public void OnClick()
    {
        Inventory inventory = FindObjectOfType<Inventory>();
        if (inventory != null)
        {
            inventory.SelectWeaponByInstance(weapon);
        }
    }
}
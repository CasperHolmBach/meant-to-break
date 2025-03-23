using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ShopItem
{
    public string itemName = "Item";
    public string description = "Item description";
    public int price = 100;
    public Sprite icon;
    public GameObject itemPrefab;
    public int itemID;
}

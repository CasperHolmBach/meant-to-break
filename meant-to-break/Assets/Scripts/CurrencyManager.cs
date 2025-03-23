using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] private int currentMoney = 0;
    
    // Event that fires when money changes
    public delegate void MoneyChangedHandler(int newAmount);
    public event MoneyChangedHandler OnMoneyChanged;
    
    // Singleton instance
    public static CurrencyManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: persist between scenes
    }
    
    // Get the current money amount
    public int GetMoney()
    {
        return currentMoney;
    }
    
    // Add money to the player's balance
    public void AddMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Trying to add negative money. Use SpendMoney instead.");
            return;
        }
        
        currentMoney += amount;
        Debug.Log($"Added {amount} money. New balance: {currentMoney}");
        
        // Notify listeners
        OnMoneyChanged?.Invoke(currentMoney);
    }
    
    // Attempt to spend money, returns true if successful
    public bool SpendMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Trying to spend negative money. Use AddMoney instead.");
            return false;
        }
        
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            Debug.Log($"Spent {amount} money. Remaining balance: {currentMoney}");
            
            // Notify listeners
            OnMoneyChanged?.Invoke(currentMoney);
            return true;
        }
        else
        {
            Debug.Log($"Not enough money to spend {amount}. Current balance: {currentMoney}");
            return false;
        }
    }
}
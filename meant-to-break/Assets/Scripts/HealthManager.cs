using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public int maxHealth = 100;

    [SerializeField]
    private int currentHealth;


    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if(currentHealth <= 0)
        {
            if(gameObject.tag == "player")
            {
                Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }        
        }
        
        // Check if player interacts with a healing item        
    }

    public void Applyhealing(GameObject healingItem, int healing)
    {
        currentHealth = currentHealth + healing;

        Destroy(healingItem);
    }

    public void TakeDamage(int damage)
    {
        currentHealth = currentHealth - damage;
    }
}

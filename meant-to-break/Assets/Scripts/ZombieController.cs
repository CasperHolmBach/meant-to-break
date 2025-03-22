using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform destination;
    public BoxCollider attackCollider;
    
    public int health = 100;
    public int damage = 10;
    public int attackSpeed = 1;
    
    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(destination.position);
    }

    void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player")) 
        {
            other.gameObject.GetComponent<HealthManager>().TakeDamage(damage);
        }
    }
}

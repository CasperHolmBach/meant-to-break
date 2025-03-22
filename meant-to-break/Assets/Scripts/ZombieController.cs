using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    public NavMeshAgent agent;
    public Vector3 destination;
    public BoxCollider attackCollider;
    
    public int health = 100;
    public int damage = 10;
    
    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(destination);
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            Debug.Log("Damage Player");
        }
    }
}

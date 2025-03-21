using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Timers;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Timer = System.Timers.Timer;
using Vector3 = UnityEngine.Vector3;

public class Spawner : MonoBehaviour
{
    private Timer _timer;
    
    public Transform[] destinations;
    public GameObject zombiePrefab;
    public float interval;
    
    void Start()
    {
        InvokeRepeating("SpawnZombie", interval, interval);
    }

    void SpawnZombie()
    {
        if (!CheckClerance())
        {
            return;
        }
        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
        Quaternion spawnRotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
        
        Transform destination = destinations[Random.Range(0, destinations.Length)];
        GameObject zombie = Instantiate(zombiePrefab, spawnPosition, spawnRotation);
        zombie.GetComponent<ZombieController>().destination = destination.position;
    }

    bool CheckClerance()
    {
        Vector3 rayOrigin = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
        
        if (Physics.Raycast(rayOrigin, transform.TransformDirection(Vector3.up), 10))
        {
            return false;
        }
        return true;
    }
    
}

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
    private int _overlapCount;
    
    public Transform[] destinations;
    public GameObject zombiePrefab;
    public Collider clerance;
    public float interval;

    private bool IsOverlapping {
        get {
            return _overlapCount > 0;
        }
    }
    
    public void Start()
    {
        InvokeRepeating(nameof(SpawnZombie), interval, interval);
    }

    public void SpawnZombie()
    {
        if (IsOverlapping)
        {
            return;
        }
        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
        Quaternion spawnRotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
        
        Transform destination = destinations[Random.Range(0, destinations.Length)];
        GameObject zombie = Instantiate(zombiePrefab, spawnPosition, spawnRotation);
        zombie.GetComponent<ZombieController>().destination = destination;
    }

    public void OnTriggerEnter(Collider other) {
        _overlapCount++;
    }

    public void OnTriggerExit(Collider other) {
        _overlapCount--;
    }
    
}

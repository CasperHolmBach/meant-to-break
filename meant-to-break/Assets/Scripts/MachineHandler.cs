using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MachineHandler : MonoBehaviour {
    private bool _startable = false;
    private bool _interactable = true;
    private float _cooldown;
    
    public GameObject[] zombieSpawners;
    public List<GameObject> destructionList;
    public TextMeshProUGUI machineControlls;
    public float checkingInterval;
    
    public void Update() {
        if (Input.GetKeyDown(KeyCode.E) && _startable) {
            Interact();
            _interactable = false;
            _cooldown = checkingInterval;
            machineControlls.enabled = false;
        }
        else if (Input.GetKeyDown(KeyCode.K)) {
            DestroyCrystal();
        }

        if (!_interactable) {
            _cooldown -= Time.deltaTime;
        }

        if (_cooldown <= 0 && !_interactable) {
            GameObject enemy = GameObject.FindWithTag("Enemy");
            if (enemy == null) {
                _interactable = true;
                DestroyCrystal();
            }
            else {
                _cooldown = checkingInterval;
            }
        }
    }

    private void DestroyCrystal() {
        destructionList[0].SetActive(false);
        destructionList.RemoveAt(0);
    }
    
    public void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Player") && _interactable) {
            machineControlls.enabled = true;
            _startable = true;
        }
    }

    public void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Player") && _interactable) {
            machineControlls.enabled = false;
            _startable = false;
        }
    }
    
    public void Interact() {
        foreach (GameObject zombieSpawner in zombieSpawners) {
            zombieSpawner.SendMessage(nameof(Spawner.StartNextWave));
        }
    }
}

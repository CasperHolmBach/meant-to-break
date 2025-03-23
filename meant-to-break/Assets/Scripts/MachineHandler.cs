using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MachineHandler : MonoBehaviour {
    private bool _startable = false;
    private bool _interactable = true;
    private float _cooldown;
    private int _currentWave = 1;
    
    public GameObject[] zombieSpawners;
    public List<GameObject> destructionList;
    public TextMeshProUGUI machineControlls;
    public CurrencyManager currencyManager;
    public float checkingInterval;
    public int wavePayout;
    
    public void Update() {
        if (Input.GetKeyDown(KeyCode.E) && _startable) {
            Interact();
            _startable = false;
            _interactable = false;
            _cooldown = checkingInterval;
            machineControlls.enabled = false;
        }
        else if (Input.GetKeyDown(KeyCode.K)) {
            FinishWave();
            _currentWave++;
        }

        if (!_interactable) {
            _cooldown -= Time.deltaTime;
        }

        if (_cooldown <= 0 && !_interactable) {
            var enemy = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemy.Length == 0) {
                _interactable = true;
                FinishWave();
                _currentWave++;
            }
            else {
                _cooldown = checkingInterval;
            }
        }
    }

    private void FinishWave() {
        // Destroy crystal
        if (_currentWave % 2 == 0) {
            destructionList[0].SetActive(false);
            destructionList.RemoveAt(0);
        }
        
        // Check if all waves have been cleared
        if (destructionList.Count == 0) {
            _interactable = false;
            _startable = false;
            SceneManager.LoadScene("GameWon");
            return;
        }
        
        // Give player money
        currencyManager.AddMoney((int)(wavePayout * Math.Ceiling((float)_currentWave/2)));
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

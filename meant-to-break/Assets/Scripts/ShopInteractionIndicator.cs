using UnityEngine;
using TMPro;

public class ShopInteractionIndicator : MonoBehaviour
{
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string defaultPromptText = "Press E to interact";
    [SerializeField] private float interactionDistance = 3f;
    
    private Transform playerTransform;
    
    void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Hide prompt initially
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
            
        // Set default text
        if (promptText != null)
            promptText.text = defaultPromptText;
    }
    
    void Update()
    {
        if (playerTransform == null) return;
        
        // Check distance to player
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool isInRange = distance <= interactionDistance;
        
        // Show/hide prompt based on distance
        if (interactionPrompt != null)
            interactionPrompt.SetActive(isInRange);
    }
}
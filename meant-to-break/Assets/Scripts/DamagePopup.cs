using UnityEngine;
using TMPro;
using System.Collections;

public class DamagePopup : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private TextMeshPro damageText;
    [SerializeField] private float lifetime = 1.0f;
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private float fadeSpeed = 3.0f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.5f, 0.3f, 1.5f);
    
    [Header("Color Settings")]
    [SerializeField] private Color normalDamageColor = Color.white;
    [SerializeField] private Color criticalDamageColor = Color.red;
    [SerializeField] private int criticalThreshold = 30;
    
    private float timeAlive = 0f;
    private Vector3 initialScale;
    private Vector3 randomOffset;
    
    public static DamagePopup Create(Vector3 position, int damageAmount, bool isCritical = false)
    {
        // Find the prefab (ensure this is set up in Resources folder)
        GameObject prefab = Resources.Load<GameObject>("DamagePopup");
        
        if (prefab == null)
        {
            // Create a new prefab dynamically if none exists
            prefab = CreateDynamicPrefab();
        }
        
        // Random offset to prevent overlap
        Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 0.5f), Random.Range(-0.5f, 0.5f));
        
        // Instantiate the popup
        GameObject popupObj = Instantiate(prefab, position + offset, Quaternion.identity);
        DamagePopup popup = popupObj.GetComponent<DamagePopup>();
        
        // Init the popup with damage amount
        popup.Setup(damageAmount, isCritical || damageAmount >= popup.criticalThreshold);
        
        return popup;
    }
    
    // Modify the CreateDynamicPrefab method to make the text larger and more visible:
    private static GameObject CreateDynamicPrefab()
    {
        // Create a new GameObject for the popup
        GameObject prefab = new GameObject("DamagePopup");
        
        // Add TextMeshPro component
        TextMeshPro textMesh = prefab.AddComponent<TextMeshPro>();
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontSize = 8;  // Increased from 5
        textMesh.fontStyle = FontStyles.Bold;
        
        // Add outline to text for better visibility
        textMesh.outlineWidth = 0.25f;
        textMesh.outlineColor = Color.black;
        
        // Make it face the camera
        prefab.AddComponent<Billboard>();
        
        // Add DamagePopup script
        prefab.AddComponent<DamagePopup>().damageText = textMesh;
        
        return prefab;
    }
    
    private void Awake()
    {
        if (damageText == null)
            damageText = GetComponent<TextMeshPro>();
            
        initialScale = transform.localScale;
    }
    
    public void Setup(int damageAmount, bool isCritical)
    {
        // Set text
        damageText.SetText(damageAmount.ToString());
        
        // Set color based on damage type
        damageText.color = isCritical ? criticalDamageColor : normalDamageColor;
        
        // Set size based on damage amount
        float sizeMultiplier = Mathf.Clamp(1.0f + (damageAmount / 100f), 1.0f, 2.0f);
        transform.localScale = initialScale * sizeMultiplier;
        
        // Random movement direction
        randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 1f, Random.Range(-0.5f, 0.5f)).normalized;
        
        // Set alpha to full
        Color color = damageText.color;
        color.a = 1f;
        damageText.color = color;
    }
    
    // Also modify the Update method to make the text move more visibly:
    private void Update()
    {
        timeAlive += Time.deltaTime;
        
        // Move up faster and with more pronounced random movement
        transform.position += (Vector3.up * 1.5f + randomOffset) * moveSpeed * Time.deltaTime;
        
        // Scale animation with more dramatic effect
        float scaleProgress = timeAlive / lifetime;
        float scaleMultiplier = scaleCurve.Evaluate(scaleProgress);
        transform.localScale = initialScale * scaleMultiplier * 1.2f; // 20% larger overall
        
        // Fade out near the end of lifetime
        if (timeAlive > lifetime * 0.5f)
        {
            Color color = damageText.color;
            color.a = Mathf.Lerp(1f, 0f, (timeAlive - lifetime * 0.5f) / (lifetime * 0.5f));
            damageText.color = color;
        }
        
        // Destroy when lifetime is over
        if (timeAlive >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}

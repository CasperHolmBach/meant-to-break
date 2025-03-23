using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public float mouseSensitivity = 100f;
    
    [Header("Knockback Settings")]
    public float knockbackDecay = 5f; 

    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 knockbackVelocity;
    private bool isGrounded;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ApplyKnockback(Vector3 force)
    {
        knockbackVelocity = force;
    }

    void Update()
    {
        MovePlayer();
        RotateCamera();
    }

    void MovePlayer()
    {
        // Ground check
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Input - Use GetKey instead of GetAxis for keyboard-only movement
        float x = 0;
        float z = 0;
        
        // Horizontal movement (A/D)
        if (Input.GetKey(KeyCode.A)) x = -1;
        else if (Input.GetKey(KeyCode.D)) x = 1;
        
        // Vertical movement (W/S)
        if (Input.GetKey(KeyCode.W)) z = 1;
        else if (Input.GetKey(KeyCode.S)) z = -1;

        // Calculate movement vector
        Vector3 move = transform.right * x + transform.forward * z;
        
        // Normalize if moving diagonally to prevent faster diagonal movement
        if (move.magnitude > 1)
            move.Normalize();
            
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // Apply knockback and decay it over time
        if (knockbackVelocity.magnitude > 0.01f)
        {
            controller.Move(knockbackVelocity * Time.deltaTime);
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDecay * Time.deltaTime);
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed           = 5f;
    public float jumpHeight      = 1.5f;
    public float gravity         = -9.81f;
    public float jumpCooldown    = 0.5f;   // seconds between jumps

    [Header("Look Settings")]
    public float   mouseSensitivity = 100f;
    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3             velocity;
    private float               xRotation;
    private float               lastJumpTime = -999f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleLook();
        HandleMoveAndJump();
    }

    void HandleLook()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation = Mathf.Clamp(xRotation - my, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mx);
    }

    void HandleMoveAndJump()
    {
        // 1) Move horizontally
        Vector3 move = transform.right * Input.GetAxis("Horizontal")
                     + transform.forward * Input.GetAxis("Vertical");
        controller.Move(move * speed * Time.deltaTime);

        // 2) Reset downward velocity when grounded, so gravity doesn't accumulate endlessly
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // 3) Jump on cooldown
        if (Input.GetButtonDown("Jump") 
            && Time.time >= lastJumpTime + jumpCooldown)
        {
            // calculate initial velocity for desired jumpHeight
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            lastJumpTime = Time.time;
        }

        // 4) Apply gravity and move vertically
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}

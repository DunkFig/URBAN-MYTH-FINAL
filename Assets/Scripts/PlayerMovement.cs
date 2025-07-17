using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public float lookSensitivity = 2f;
    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation;

    private PlayerControls controls;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    private float lastJumpTime = -999f;
    public float jumpCooldown = 0.5f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        controls = new PlayerControls();
        moveAction = controls.Player.Move;
        lookAction = controls.Player.Look;
        jumpAction = controls.Player.Jump;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        HandleLook();
        HandleMoveAndJump();
    }

    void HandleLook()
    {
        Vector2 look = lookAction.ReadValue<Vector2>();

        float mx = look.x * lookSensitivity;
        float my = look.y * lookSensitivity;

        xRotation -= my;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mx);
    }

    void HandleMoveAndJump()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // Basic movement
        controller.Move(move * speed * Time.deltaTime);

        // Gravity application
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // ✅ Jump allowed anywhere, only cooldown matters
        if (jumpAction.WasPressedThisFrame() && Time.time >= lastJumpTime + jumpCooldown)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            lastJumpTime = Time.time;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}

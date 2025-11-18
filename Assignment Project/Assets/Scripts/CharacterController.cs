using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Character controller script to handle player input and movement states.
/// </summary>
[RequireComponent(typeof(UnityEngine.CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    #region Movement States
    public enum MovementState
    {
        Idle,
        Walking,
        Running,
        Jumping,
        Sliding,
        WallRunning
    }

    [Header("State")]
    [SerializeField] private MovementState currentState = MovementState.Idle;
    #endregion

    #region Movement Settings
    [Header("Walking")]
    [SerializeField] private float walkSpeed = 5.2f;
    [SerializeField] private float sprintSpeed = 7f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float graceTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    [Header("Sliding")]
    [SerializeField] private float slideSpeed = 10f;
    [SerializeField] private float slideDuration = 1f;
    [SerializeField] private float slideControlStrength = 0.3f;

    [Header("Wall Running")]
    [SerializeField] private float wallRunSpeed = 6f;
    [SerializeField] private float wallRunDuration = 2f;
    [SerializeField] private float wallJumpForce = 10f;
    [SerializeField] private float wallCheckDistance = 0.7f;
    [SerializeField] private float wallRunGravity = 2f;

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    #endregion

    #region Components
    private UnityEngine.CharacterController controller;
    private PlayerInput playerInput;
    public Camera myCamera;
    #endregion

    #region Movement Variables
    private Vector3 velocity;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool wasGrounded;
    private float graceTimeCounter;
    private float jumpBufferCounter;
    private float slideTimer;
    private Vector3 slideDirection;
    private float wallRunTimer;
    private Vector3 wallNormal;
    private bool isWallRight;
    private bool isWallLeft;
    private bool isSprinting;
    private bool isSliding;
    #endregion

    #region Initialization
    void Awake()
    {
        controller = GetComponent<UnityEngine.CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        if (controller == null)
            Debug.LogError("CharacterController component missing!");
        if (myCamera == null)
            myCamera = Camera.main;
    }

    void Update()
    {
        // Fallback to old Input system if Input Actions aren't set up
        if (playerInput == null || playerInput.actions == null)
        {
            HandleLegacyInput();
        }

        CheckGroundStatus();
        HandleTimers();
        CheckWallRun();
        HandleMovement();
        UpdateMovementState();
        ApplyMovement();
    }

    private void HandleLegacyInput()
    {
        // WASD / Arrow keys
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Space for jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        // Left Shift for sprint
        isSprinting = Input.GetKey(KeyCode.LeftShift);

        // Left Control for slide
        if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded && moveInput.magnitude > 0.1f)
        {
            StartSlide();
        }
    }
    #endregion

    #region Input Handling
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.performed || context.started;
    }

    public void OnSlide(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded && moveInput.magnitude > 0.1f)
        {
            StartSlide();
        }
    }
    #endregion

    #region Timers
    private void HandleTimers()
    {
        // Coyote time
        if (isGrounded)
            graceTimeCounter = graceTime;
        else
            graceTimeCounter -= Time.deltaTime;

        // Jump buffer
        if (jumpBufferCounter > 0)
            jumpBufferCounter -= Time.deltaTime;

        // Slide timer
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
                isSliding = false;
        }

        // Wall run timer
        if (currentState == MovementState.WallRunning)
        {
            wallRunTimer -= Time.deltaTime;
            if (wallRunTimer <= 0)
                currentState = MovementState.Jumping;
        }
    }
    #endregion

    #region Ground Check
    private void CheckGroundStatus()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundLayer);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Landing
        if (!wasGrounded && isGrounded)
        {
            isSliding = false;
        }
    }
    #endregion

    #region Wall Running
    private void CheckWallRun()
    {
        isWallRight = Physics.Raycast(transform.position, transform.right, out RaycastHit rightHit, wallCheckDistance, groundLayer);
        isWallLeft = Physics.Raycast(transform.position, -transform.right, out RaycastHit leftHit, wallCheckDistance, groundLayer);

        if (!isGrounded && (isWallRight || isWallLeft) && moveInput.magnitude > 0.1f)
        {
            if (currentState != MovementState.WallRunning)
            {
                wallRunTimer = wallRunDuration;
                currentState = MovementState.WallRunning;
            }

            wallNormal = isWallRight ? rightHit.normal : leftHit.normal;
        }
        else if (currentState == MovementState.WallRunning)
        {
            currentState = MovementState.Jumping;
        }
    }
    #endregion

    #region Movement
    private void HandleMovement()
    {
        switch (currentState)
        {
            case MovementState.Idle:
            case MovementState.Walking:
            case MovementState.Running:
                HandleGroundMovement();
                break;

            case MovementState.Jumping:
                HandleAirMovement();
                break;

            case MovementState.Sliding:
                HandleSlideMovement();
                break;

            case MovementState.WallRunning:
                HandleWallRunMovement();
                break;
        }

        HandleJump();
        ApplyGravity();
    }

    private void HandleGroundMovement()
    {
        Vector3 forward = myCamera.transform.forward;
        Vector3 right = myCamera.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        float speed = isSprinting ? sprintSpeed : walkSpeed;
        Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x) * speed;

        controller.Move(moveDirection * Time.deltaTime);
    }

    private void HandleAirMovement()
    {
        Vector3 forward = myCamera.transform.forward;
        Vector3 right = myCamera.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x) * walkSpeed * 0.8f;
        controller.Move(moveDirection * Time.deltaTime);
    }

    private void HandleSlideMovement()
    {
        Vector3 controlInput = new Vector3(moveInput.x, 0, moveInput.y) * slideControlStrength;
        Vector3 slideMove = (slideDirection + controlInput) * slideSpeed;
        controller.Move(slideMove * Time.deltaTime);
    }

    private void HandleWallRunMovement()
    {
        Vector3 forward = myCamera.transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);
        if (Vector3.Dot(wallForward, forward) < 0)
            wallForward = -wallForward;

        controller.Move(wallForward * wallRunSpeed * Time.deltaTime);

        // Apply reduced gravity during wall run
        velocity.y += -wallRunGravity * Time.deltaTime;
    }

    private void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
        currentState = MovementState.Sliding;

        Vector3 forward = myCamera.transform.forward;
        forward.y = 0;
        forward.Normalize();
        slideDirection = forward;
    }

    private void HandleJump()
    {
        // Jump with buffer and coyote time
        if (jumpBufferCounter > 0 && (graceTimeCounter > 0 || currentState == MovementState.WallRunning))
        {
            if (currentState == MovementState.WallRunning)
            {
                // Wall jump
                velocity.y = wallJumpForce;
                Vector3 jumpDir = wallNormal * 5f + Vector3.up;
                controller.Move(jumpDir * Time.deltaTime);
                currentState = MovementState.Jumping;
            }
            else
            {
                // Normal jump
                velocity.y = jumpForce;
                currentState = MovementState.Jumping;
            }

            jumpBufferCounter = 0;
            graceTimeCounter = 0;
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded && currentState != MovementState.WallRunning)
        {
            velocity.y -= gravity * Time.deltaTime;
        }
    }

    private void ApplyMovement()
    {
        controller.Move(velocity * Time.deltaTime);
    }
    #endregion

    #region State Management
    private void UpdateMovementState()
    {
        if (currentState == MovementState.Sliding ||
            currentState == MovementState.WallRunning ||
            currentState == MovementState.Jumping && !isGrounded)
            return;

        if (!isGrounded)
        {
            currentState = MovementState.Jumping;
        }
        else if (moveInput.magnitude < 0.1f)
        {
            currentState = MovementState.Idle;
        }
        else if (isSprinting)
        {
            currentState = MovementState.Running;
        }
        else
        {
            currentState = MovementState.Walking;
        }

        if (isSliding)
            currentState = MovementState.Sliding;
    }
    #endregion

    #region Debug
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckDistance);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right * wallCheckDistance);
        Gizmos.DrawRay(transform.position, -transform.right * wallCheckDistance);
    }

    // Add this to see debug info in the Inspector
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"State: {currentState}");
        GUI.Label(new Rect(10, 30, 300, 20), $"Move Input: {moveInput}");
        GUI.Label(new Rect(10, 50, 300, 20), $"Is Grounded: {isGrounded}");
        GUI.Label(new Rect(10, 70, 300, 20), $"Velocity: {velocity}");
        GUI.Label(new Rect(10, 90, 300, 20), $"Sprint: {isSprinting}");
    }
    #endregion
}
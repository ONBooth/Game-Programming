using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Character controller script to handle player input and movement states.
/// </summary>

public class CharacterController : MonoBehaviour
{
 #region Movement States
    public enum MovementState // enum defines every movement state //
    {
        Idle,
        Walking,
        Running,
        Jumping,
        wallRunning,
    }
[Header("State")]
[SerializeField] private MovementState currentState = MovementState.Walking;
#endregion

#region Movement Settings
[Header("Idle")]
// when nothing is pressing pressed or used //
[Header("Walking")]
[SerializeField] private float walkSpeed = 5.2f; // Walking speed 
[SerializeField] private float sprintSpeed = 7f; // running speed 
[SerializeField] private float crouchSpeed = 3f; // crouched speed 

[Header("Jumping")]
[SerializeField] private float jumpForce = 7f; // force of jump strength
[SerializeField] private float gravity = 5.2f; 
[SerializeField] private float graceTime = 0.2f; // period after leaving ground
[SerializeField] private float JumpTime = 0.2f; // period before landing

[Header("Sliding")]
[SerializeField] private float slideSpeed = 10f;
[SerializeField] private float slideDuration = 1f;
[SerializeField] private float slideControlStrength = 0.3f;

[Header("Wall Running")]
[SerializeField] private float wallRunSpeed = 6f;
[SerializeField] private float wallRunDuration = 2f;
[SerializeField] private float wallJumpForce = 10f;
[SerializeField] private float wallCheckDistance = 0.7f;

[Header("Ground Detection")]
[SerializeField] private float groundCheckDistance = 0.3f;
[SerializeField] private LayerMask groundLayer;
[SerializeField] private Transform groundCheck; // Empty child object at feet
#endregion

#region Component 

private CharacterController controller;
private PlayerInput playerInput;

#endregion

#region Movement Variables

private Vector3 velocity;
private Vector2 moveInput;
private bool isGrounded;
private bool wasGrounded;

 private float graceTimeCounter;

private float jumpTimeCounter;

private float slideTimeCounter;
private float slideTimer;
private Vector3 slideDirection;

private float wallRunTimeCounter;
private Vector3 wallNormal;
private bool isWallRight;
private bool isWallLeft;

#endregion

#region Initialization
public Camera MyCamera;

     void Awake()
    {

     if (Controller == null)
            Debug.LogError("CharacterController component missing!");
    }
     void Update()
    {
        HandleMovement();
        CheckGroundStatus();
        UpdateMovementState();
        //==============================//
        float x = input.getAxis("Horizontal");
        float z = input.getAxis("Vertical");
        //==============================//
        Vector3 movement = new Vector3(x, 0, z);
        //==============================//
        Vector3 rotatedMovement = MyCamera.transform.rotation * movement;
        //==============================//
        MyController.Move(movement * walkSpeed * Time.deltaTime);

    }
    #endregion

      private void HandleMovement()
    {
         switch (currentState)
{
            case MovementState.Walking:
                HandleWalking();
                break;
            case MovementState.Sprinting:
                HandleSprinting();
                break;
            case MovementState.Crouching:
                HandleCrouching();
                break;
            case MovementState.Sliding:
                HandleSliding();
                break;
            case MovementState.WallRunning:
                HandleWallRunning();
                break;
            default:
                HandleWalking(); // Fallback
                break;
}

    private void CheckGroundStatus()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundLayer);
        if (isGrounded && velocity.y < 0);
        {
            velocity.y = -2f; // Small negative value to keep grounded
        }
       
        
    }

     

}
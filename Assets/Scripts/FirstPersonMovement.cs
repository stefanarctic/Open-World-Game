using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement")]
    public CharacterController controller;

    public float speed = 12f;

    [Header("Jumping")]
    public float jumpHeight = 3f;
    public float gravity = -9.81f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundmask;

    [HideInInspector]
    public Vector3 move;
    Vector3 velocity;
    [SerializeField]
    private bool isGrounded;

    [Header("Crouching")]
    public KeyCode crouchKey = KeyCode.LeftShift;
    // public string crouchKey = "left shift";
    public bool isCrouching = false;
    public Transform ceilingCheck;
    public float ceilingDistance = 0.4f;

    [Header("Sprinting")]
    public KeyCode sprintKey = KeyCode.LeftControl;
    // public string sprintKey = "left control";
    public float speedAmplifier = 2f;
    public bool isSprinting = false;

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundmask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        if (Input.GetKeyDown(crouchKey))
        {
            Crouch();
        }

        if (Input.GetKeyDown(sprintKey))
            ActivateSprinting();

        if(Input.GetKeyUp(sprintKey))
            DisableSprinting();
    }

    void ActivateCrouching()
    {
        isCrouching = true;
        speed /= 2;
        transform.localScale = new Vector3(
                transform.localScale.x,
                transform.localScale.y / 2,
                transform.localScale.z
            );
    }

    void DisableCrouching()
    {
        isCrouching = false;
        speed *= 2;
        transform.localScale = new Vector3(
                transform.localScale.x,
                transform.localScale.y * 2,
                transform.localScale.z
            );
    }

    void Crouch()
    {
        if (isCrouching)
        {
            DisableCrouching();
            if (Physics.CheckSphere(ceilingCheck.position, ceilingDistance, groundmask))
            {
                ActivateCrouching();
            }
        }
        else
        {
            ActivateCrouching();
        }
    }

    void ActivateSprinting()
    {
        Debug.Log("Activated sprinting");
        isSprinting = true;
        speed *= speedAmplifier;
    }

    void DisableSprinting()
    {
        Debug.Log("Disabled sprinting");
        isSprinting = false;
        speed /= speedAmplifier;
    }

    void Sprint()
    {
        if (isSprinting == true)
            DisableSprinting();
        else
            ActivateSprinting();
    }
}
using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    CharacterController controller;

    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    public float slideSpeed = 3f;
    public float slideSpace = 1f;
    public float slideFriction = 20f;

    private Vector3 moveDirection = Vector3.zero;

    private float targetPosition = 0f;

    private bool turningLeft = false;
    private bool turningRight = false;

    private Vector3 newPos;

    private bool running = true;

    //start 

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (running)
        {
            /*
             * Receive jump commands
             */
            if (controller.isGrounded)
            {
                moveDirection = new Vector3(moveDirection.x, 0, 0);  //get keyboard input to move in the horizontal direction    
                if (Input.GetButton("Jump"))
                {          //play "Jump" animation if character is grounded and spacebar is pressed   
                    GetComponentInChildren<Animator>().Play("Slide up");
                    moveDirection.y = jumpSpeed;         //add the jump height to the character
                }
            }

            moveDirection.y -= gravity * Time.deltaTime;       //Apply gravity

            /*
             * Receive turning commands
             */
            if (!turningLeft && !turningRight)
            {
                moveDirection = new Vector3(0, moveDirection.y, 0);  //get keyboard input to move in the horizontal direction    

                if (Input.GetKeyUp(KeyCode.LeftArrow))
                {
                    targetPosition = transform.position.x - slideSpace;
                    if (targetPosition >= -6)
                    {
                        moveDirection.x = -slideSpeed;
                        turningLeft = true;
                        GetComponentInChildren<Animator>().Play("Slide left");
                    }
                }
                if (Input.GetKeyUp(KeyCode.RightArrow))
                {
                    targetPosition = transform.position.x + slideSpace;
                    if (targetPosition <= 1)
                    {
                        moveDirection.x = +slideSpeed;
                        turningRight = true;
                        GetComponentInChildren<Animator>().Play("Slide right");
                    }
                }
            }

            /*
             * Slide the character with slideSpace to left or right
             */
            if (turningLeft || turningRight)
            {
                float currentPos = transform.position.x;
                if (turningLeft)
                {
                    if (targetPosition - currentPos < 0)
                    {
                        moveDirection.x -= slideFriction * Time.deltaTime; // Apply friction when turning right
                    }
                    else
                    {
                        turningLeft = false;
                        moveDirection.x = 0f;
                        Vector3 newPos = new Vector3(targetPosition, transform.position.y, transform.position.z);
                        transform.position = newPos;
                    }
                }
                if (turningRight)
                {
                    if (targetPosition - currentPos > 0)
                    {
                        moveDirection.x += slideFriction * Time.deltaTime; // Apply friction when turning left
                    }
                    else
                    {
                        turningRight = false;
                        moveDirection.x = 0f;
                        Vector3 newPos = new Vector3(targetPosition, transform.position.y, transform.position.z);
                        transform.position = newPos;
                    }
                }
            }

            controller.Move(moveDirection * Time.deltaTime);      //Move the controller
        }
    }

    //check if the character collects the powerups or the snags
    void OnTriggerEnter(Collider other)
    {
        Destroy(other.gameObject);
    }

    public void stop(){
        running = false;
    }

    public void start()
    {
        running = true;
    }
}
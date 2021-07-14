using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.p4bloGames.Buggario
{
    public class BuggarioMovement : MonoBehaviour
    {
        [SerializeField] float speed = 300f;
        [SerializeField] float jumpForce = 5f;
        [SerializeField] Transform groundCheckPoint;
        [SerializeField] float groundCheckRadius;
        [SerializeField] LayerMask whatIsGround;

        const string HORIZONTAL = "Horizontal";
        const string JUMP = "Jump";
        const string IS_RUNNING = "IsRunning";

        Rigidbody2D rb;
        Animator animator;
        SpriteRenderer spriteRenderer;

        float movement;
        bool isGrounded;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            movement = Input.GetAxisRaw(HORIZONTAL);
            animator.SetBool(IS_RUNNING, Mathf.Abs(movement) > 0);

            isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, whatIsGround);

            if (Input.GetButtonDown(JUMP) && isGrounded)
            {
                rb.velocity = Vector2.up * jumpForce;

                animator.SetTrigger(JUMP);
            }
        }

        void FixedUpdate()
        {

            if (Mathf.Abs(movement) > 0)
            {
                spriteRenderer.flipX = movement < 0;

                float xMove = movement * speed * Time.deltaTime;
                rb.velocity = new Vector2(xMove, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}

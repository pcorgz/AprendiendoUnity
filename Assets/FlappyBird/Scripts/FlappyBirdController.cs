using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.p4bloGames.FlappyBird
{
    public class FlappyBirdController : MonoBehaviour
    {
        [SerializeField] float upForce;
        [SerializeField] AudioClip flapClip;
        [SerializeField] AudioClip crashClip;
        [SerializeField] AudioClip fallClip;
        [SerializeField] AudioClip pointClip;

        bool isInbounds;
        float gravityScale;

        Rigidbody2D rb;
        AudioSource audioSource;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            audioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            gravityScale = rb.gravityScale;
            rb.gravityScale = 0f;
        }

        void Update()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                switch (GameManager.Instance.CurrentGameState)
                {
                    //case GameState.WaitingInput:
                    //    StartGame();
                    //    break;
                    case GameState.GameRunning:
                        ManageJump();
                        break;
                    case GameState.GameOver:
                        GameManager.Instance.RestartGame();
                        break;
                }
            }
        }

        public void StartGame()
        {
            GameManager.Instance.StartGame();
            SetGravityScale();
            ManageJump();
        }

        private void ManageJump()
        {
            if (isInbounds == false) return;

            rb.velocity = Vector2.zero;
            rb.AddForce(Vector2.up * upForce);
            audioSource.PlayOneShot(flapClip);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (GameManager.Instance.CurrentGameState != GameState.GameRunning)
                return;

            if (collision.gameObject.CompareTag("Crash"))
            {
                GetComponent<Animator>().enabled = false;
                StartCoroutine(PlayCrashSounds());
                GameManager.Instance.ManageGameOver();
            }
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Boundaries"))
            {
                isInbounds = true;
            }

            if (collision.CompareTag("Point"))
            {
                GameManager.Instance.AddScore();
                audioSource.PlayOneShot(pointClip);
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Boundaries"))
            {
                isInbounds = false;
            }
        }

        public void SetGravityScale()
        {
            rb.gravityScale = gravityScale;
        }

        IEnumerator PlayCrashSounds()
        {
            audioSource.PlayOneShot(crashClip);
            yield return new WaitForSeconds(crashClip.length);

            audioSource.PlayOneShot(fallClip);
        }
    }
}
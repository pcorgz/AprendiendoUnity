using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.p4bloGames.FlappyBird
{
    public class EnvironmentController : MonoBehaviour
    {
        [SerializeField] float moveSpeed;
        [SerializeField] bool needsGameRunning = true;
        [SerializeField] bool stopWhenGameOver = false;

        void Update()
        {
            if (stopWhenGameOver
                    && (GameManager.Instance.CurrentGameState == GameState.GameEnding
                        || GameManager.Instance.CurrentGameState == GameState.GameOver))
            {
                return;
            }

            if (needsGameRunning == false
                    || GameManager.Instance.CurrentGameState == GameState.GameRunning)
                transform.position += Vector3.left * moveSpeed * Time.deltaTime;
        }
    }
}
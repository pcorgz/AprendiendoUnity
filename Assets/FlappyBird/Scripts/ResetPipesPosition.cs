using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.p4bloGames.FlappyBird
{
    public class ResetPipesPosition : MonoBehaviour
    {
        [SerializeField] float thresholdToResetX = 12f;
        [SerializeField] float minValueInY = -1.5f;
        [SerializeField] float maxValueInY = 1.5f;

        void Start()
        {
            transform.position = new Vector3(
                    transform.position.x, // wherever its placed at start
                    SetRandomPosY());     // random Y
        }

        private void Update()
        {
            if (transform.position.x <= -thresholdToResetX)
            {
                transform.position = new Vector3(
                        transform.position.x * -1, // return to start (to the right)
                        SetRandomPosY()); // random Y
            }
        }

        float SetRandomPosY()
        {
            return Random.Range(minValueInY, maxValueInY);
        }
    }
}
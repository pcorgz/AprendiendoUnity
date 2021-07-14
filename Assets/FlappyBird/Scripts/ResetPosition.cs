using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.p4bloGames.FlappyBird
{
    public class ResetPosition : MonoBehaviour
    {
        [SerializeField] int positionMultiplier = 3;

        float sizeTimesLocalScaleX;

        private void Start()
        {
            sizeTimesLocalScaleX = GetComponent<SpriteRenderer>().size.x * transform.localScale.x;
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("LeftCheck"))
            {
                transform.position += Vector3.right * sizeTimesLocalScaleX * positionMultiplier;
            }
        }
    }
}
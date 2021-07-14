using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.p4bloGames.LearningUnity
{
    public class MainMenu : MonoBehaviour
    {
        const string FLAPPY_BIRD = "FlappyBird";
        const string BEJEWELED = "Bejeweled";
        const string BUGGARIO = "Buggario";

        [SerializeField] float animDuration = 1.2f;
        [SerializeField] GameObject toFlappyBirdTrans;
        [SerializeField] GameObject toBejeweledTrans;
        [SerializeField] GameObject panelCredits;

        bool creditsOpen = false;

        void Awake()
        {
            panelCredits.transform.localScale = new Vector3(0f, 0f, 0f);
        }

        public void GoToGame(string sceneName)
        {
            switch (sceneName)
            {
                case FLAPPY_BIRD:
                    toFlappyBirdTrans.SetActive(true);
                    break;

                case BEJEWELED:
                    toBejeweledTrans.SetActive(true);
                    break;

                case BUGGARIO:
                    // TODO: toBuggarioTrans.SetActive(true);
                    break;
            }

            StartCoroutine(LoadSelectedScene(sceneName));
        }

        IEnumerator LoadSelectedScene(string sceneName)
        {
            yield return new WaitForSeconds(animDuration);

            SceneManager.LoadScene(sceneName);
        }

        public void ToggleCredits()
        {
            creditsOpen = !creditsOpen;
            //panelCredits.SetActive(creditsOpen);

            Vector3 toScale = creditsOpen
                    ? new Vector3(1f, 1f, 1f)
                    : new Vector3(0f, 0f, 0f);

            if (creditsOpen)
            {
                LeanTween.scale(panelCredits, toScale, 0.5f).setEaseOutBounce();
            }
            else
            {
                LeanTween.scale(panelCredits, toScale, 0.5f).setEaseInBack();
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace com.p4bloGames.Bejeweled
{
    public enum GameState
    {
        Starting, Running, Busy, RemovingNodes, GameOver
    }

    public class GameManager : MonoBehaviour
    {
        #region SerializeFields

        [SerializeField] NodesManager nodesManager;
        [SerializeField] TMP_Text scoreText;
        [SerializeField] List<GemScriptableObject> gemsAvaiableList;
        [SerializeField] GameObject transitionToMenu;
        [SerializeField] float transitionTime = 1.2f;
        [SerializeField] float maxPlayTime = 60f;
        [SerializeField] TMP_Text timerText;
        [SerializeField] Slider timerSlider;
        [SerializeField] GameObject gameOverPanel;
        [SerializeField] float secondsUntilReset = 2f;
        [SerializeField] TMP_Text yourScoreText;
        [SerializeField] TMP_Text highScoreText;
        [SerializeField] GameObject restartGameButton;
        [SerializeField] float gameOverAnimTime = 0.25f;

        #endregion

        #region Private fields

        int score;

        const string HIGH_SCORE_PREF = "BEJEWELED_HighScore";
        const string MAIN_MENU_SCENE = "MainMenu";

        float timeRemaining;

        #endregion

        #region Public fields

        public static GameManager Instance { get; private set; }
        public GemNode NodeSelected { get; internal set; }
        public GameState CurrentGameState { get; set; }
        public GemNode NodeDragging { get; internal set; }
        public bool IsTimerRunning { get; set; }

        #endregion

        #region MonoBehaviour methods

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        void Start()
        {
            timeRemaining = maxPlayTime;
            score = 0;
            scoreText.text = $"Score: 0";
            CurrentGameState = GameState.Starting;
            StartCoroutine(WaitForStartAnimation());
        }

        void Update()
        {
            timerText.text = FormatTimer();
            timerSlider.value = timeRemaining / maxPlayTime;

            if (IsTimerRunning)
            {
                timeRemaining -= Time.deltaTime;

                if (timeRemaining <= 0)
                {
                    timeRemaining = 0f;
                    IsTimerRunning = false;
                    StartCoroutine(EndGame());
                }
            }
        }

        #endregion

        #region Private methods

        string FormatTimer()
        {
            float minutes = Mathf.FloorToInt(timeRemaining / 60);
            float seconds = Mathf.FloorToInt(timeRemaining % 60);

            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        void SaveHighScore()
        {
            if (PlayerPrefs.HasKey(HIGH_SCORE_PREF) == false
                    || score > PlayerPrefs.GetInt(HIGH_SCORE_PREF))
            {
                PlayerPrefs.SetInt(HIGH_SCORE_PREF, score);
            }
        }

        #endregion

        #region Coroutines

        IEnumerator WaitForStartAnimation()
        {
            yield return new WaitUntil(() =>
            {
                return nodesManager.GetGemNodes().Any(g => g.IsBusy) == false;
            });

            // All nodes are done moving
            if (nodesManager.CheckMatches() == false)
            {
                CurrentGameState = GameState.Running;
            }
            else
            {
                nodesManager.RemoveMatches();
            }
        }

        IEnumerator TransitionToMenu()
        {
            yield return new WaitForSeconds(transitionTime);

            SceneManager.LoadScene(MAIN_MENU_SCENE);
        }

        IEnumerator EndGame()
        {
            yield return new WaitUntil(() =>
            {
                return nodesManager.GetGemNodes().Any(g => g.IsBusy) == false 
                        && CurrentGameState == GameState.Running;
            });

            CurrentGameState = GameState.GameOver;

            gameOverPanel.SetActive(true);
            LeanTween.scale(gameOverPanel, Vector3.zero, 0f);
            LeanTween.scale(gameOverPanel, Vector3.one, gameOverAnimTime);
            scoreText.enabled = false;

            SaveHighScore();

            StartCoroutine(GameOver());
        }

        IEnumerator GameOver()
        {
            yourScoreText.text = $"Tu puntuación: {score}";
            highScoreText.text = $"Puntuación más alta: {PlayerPrefs.GetInt(HIGH_SCORE_PREF)}";
            yield return new WaitForSeconds(secondsUntilReset);

            CurrentGameState = GameState.GameOver;
            restartGameButton.SetActive(true);
        }

        #endregion

        #region Public methods

        public List<GemScriptableObject> GetGemsAvaiableList()
        {
            return gemsAvaiableList;
        }

        public void AddToScore(int amount)
        {
            score += amount;
            scoreText.text = $"Score: {string.Format("{0:n0}", score)}";
        }

        public void BackToMenu()
        {
            transitionToMenu.SetActive(true);
            gameOverPanel.SetActive(false);
            StartCoroutine(TransitionToMenu());
        }

        public float GetTimeRemaining()
        {
            return timeRemaining;
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        #endregion
    }
}
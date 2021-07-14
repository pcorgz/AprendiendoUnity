using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.p4bloGames.FlappyBird
{
    public enum GameState
    {
        WaitingInput, GameRunning, GameEnding, GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public int Score { get; private set; }

        public GameState CurrentGameState { get; private set; }
        [SerializeField] GameObject startPanel;
        [SerializeField] GameObject gameOverPanel;
        [SerializeField] TMP_Text yourScoreText;
        [SerializeField] TMP_Text highScoreText;
        [SerializeField] TMP_Text restartGameText;
        [SerializeField] TMP_Text currentScoreText;
        [SerializeField] float secondsUntilReset = 2f;
        [SerializeField] float transitionDuration = 1.2f;
        [SerializeField] GameObject transitionOut;

        const string HIGH_SCORE_PREF = "HighScore";
        const string MAIN_MENU_SCENE = "MainMenu";

        private void Awake()
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
            CurrentGameState = GameState.WaitingInput;
            startPanel.SetActive(true);
            gameOverPanel.SetActive(false);
            restartGameText.enabled = false;
            currentScoreText.enabled = false;
        }

        public void StartGame()
        {
            // TODO: Start pipes generation
            CurrentGameState = GameState.GameRunning;
            startPanel.SetActive(false);
            Score = 0;
            currentScoreText.text = "0";
            currentScoreText.enabled = true;
        }

        public void AddScore()
        {
            Score++;
            currentScoreText.text = Score.ToString();
            yourScoreText.text = $"Your score: {Score}";
        }

        public void ManageGameOver()
        {
            CurrentGameState = GameState.GameEnding;
            gameOverPanel.SetActive(true);
            currentScoreText.enabled = false;

            SaveHighScore();

            StartCoroutine(GameOver());
        }

        void SaveHighScore()
        {
            if (PlayerPrefs.HasKey(HIGH_SCORE_PREF) == false
                    || Score > PlayerPrefs.GetInt(HIGH_SCORE_PREF))
            {
                PlayerPrefs.SetInt(HIGH_SCORE_PREF, Score);
            }
        }

        IEnumerator GameOver()
        {
            yourScoreText.text = $"Tu puntuación: {Score}";
            highScoreText.text = $"Puntuación más alta: {PlayerPrefs.GetInt(HIGH_SCORE_PREF)}";
            yield return new WaitForSeconds(secondsUntilReset);

            CurrentGameState = GameState.GameOver;
            restartGameText.enabled = true;
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ReturnToMenu()
        {
            StartCoroutine(TransitionToMenu());
            transitionOut.SetActive(true);
        }

        IEnumerator TransitionToMenu()
        {
            yield return new WaitForSeconds(transitionDuration);

            SceneManager.LoadScene(MAIN_MENU_SCENE);
        }
    }
}

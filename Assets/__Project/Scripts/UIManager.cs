using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EscapeFromLava
{
    public class UIManager : MonoBehaviour
    {
        [Header("HUD References")]
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private GameObject[] heartIcons;

        [Header("Game Over Panel References")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TMP_Text gameOverTitleText;
        [SerializeField] private Button restartButton;

        private void OnEnable()
        {
            // Subscribe to GameEvents for UI updates
            GameEventManager.OnGameStateChanged += HandleGameStateChanged;
            GameEventManager.OnScoreChanged += HandleScoreChanged;
            GameEventManager.OnLivesChanged += HandleLivesChanged;
            GameEventManager.OnTimerChanged += HandleTimerChanged;

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            }
        }

        private void OnDisable()
        {
            // Clean up subscriptions
            GameEventManager.OnGameStateChanged -= HandleGameStateChanged;
            GameEventManager.OnScoreChanged -= HandleScoreChanged;
            GameEventManager.OnLivesChanged -= HandleLivesChanged;
            GameEventManager.OnTimerChanged -= HandleTimerChanged;

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartButtonClicked);
            }
        }

        private void Start()
        {
            // Ensure Game Over UI starts hidden
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (gameOverPanel == null) return;

            switch (state)
            {
                case GameState.Playing:
                    gameOverPanel.SetActive(false);
                    break;

                case GameState.GameOverWon:
                    gameOverPanel.SetActive(true);
                    if (gameOverTitleText != null)
                    {
                        gameOverTitleText.text = "Victory!\nYou Escaped the Lava!";
                    }
                    break;

                case GameState.GameOverLost:
                    gameOverPanel.SetActive(true);
                    if (gameOverTitleText != null)
                    {
                        gameOverTitleText.text = "Defeat!\nConsumed by Lava!";
                    }
                    break;
            }
        }

        private void HandleScoreChanged(int current, int total)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Diamonds: {current} / {total}";
            }
        }

        private void HandleLivesChanged(int lives)
        {
            if (heartIcons == null) return;

            // Enable hearts representing active lives, disable the rest
            for (int i = 0; i < heartIcons.Length; i++)
            {
                if (heartIcons[i] != null)
                {
                    heartIcons[i].SetActive(i < lives);
                }
            }
        }

        private void HandleTimerChanged(float timeRemaining)
        {
            if (timerText != null)
            {
                timerText.text = $"Time: {Mathf.CeilToInt(timeRemaining)}s";
            }
        }

        private void OnRestartButtonClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartLevel();
            }
        }
    }
}

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
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private GameObject[] heartIcons;

        [Header("Victory (Game Complete) Panel References")]
        [SerializeField] private GameObject winPanel;
        [SerializeField] private Button winRestartButton;

        [Header("Defeat (Game Over) Panel References")]
        [SerializeField] private GameObject losePanel;
        [SerializeField] private Button loseRestartButton;

        [Header("Pause Panel References")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;

        [Header("Delay Settings")]
        [Tooltip("Seconds to wait after game over/complete before showing the panel overlay.")]
        [SerializeField] private float showPanelDelay = 2.0f;

        private Coroutine panelDelayCoroutine;

        private void OnEnable()
        {
            // Subscribe to GameEvents for UI updates
            GameEventManager.OnGameStateChanged += HandleGameStateChanged;
            GameEventManager.OnScoreChanged += HandleScoreChanged;
            GameEventManager.OnLivesChanged += HandleLivesChanged;
            GameEventManager.OnTimerChanged += HandleTimerChanged;

            if (winRestartButton != null)
            {
                winRestartButton.onClick.AddListener(OnRestartButtonClicked);
            }
            if (loseRestartButton != null)
            {
                loseRestartButton.onClick.AddListener(OnRestartButtonClicked);
            }
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseButtonClicked);
            }
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeButtonClicked);
            }
        }

        private void OnDisable()
        {
            // Clean up subscriptions
            GameEventManager.OnGameStateChanged -= HandleGameStateChanged;
            GameEventManager.OnScoreChanged -= HandleScoreChanged;
            GameEventManager.OnLivesChanged -= HandleLivesChanged;
            GameEventManager.OnTimerChanged -= HandleTimerChanged;

            if (winRestartButton != null)
            {
                winRestartButton.onClick.RemoveListener(OnRestartButtonClicked);
            }
            if (loseRestartButton != null)
            {
                loseRestartButton.onClick.RemoveListener(OnRestartButtonClicked);
            }
            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveListener(OnPauseButtonClicked);
            }
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveListener(OnResumeButtonClicked);
            }
        }

        private void Start()
        {
            // Ensure all panels start hidden
            if (winPanel != null)
            {
                winPanel.SetActive(false);
            }
            if (losePanel != null)
            {
                losePanel.SetActive(false);
            }
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            // Stop any active delayed panel display coroutine
            if (panelDelayCoroutine != null)
            {
                StopCoroutine(panelDelayCoroutine);
                panelDelayCoroutine = null;
            }

            switch (state)
            {
                case GameState.Playing:
                    if (winPanel != null) winPanel.SetActive(false);
                    if (losePanel != null) losePanel.SetActive(false);
                    if (pausePanel != null) pausePanel.SetActive(false);
                    break;

                case GameState.Paused:
                    if (winPanel != null) winPanel.SetActive(false);
                    if (losePanel != null) losePanel.SetActive(false);
                    if (pausePanel != null) pausePanel.SetActive(true);
                    break;

                case GameState.GameOverWon:
                case GameState.GameOverLost:
                    // Deactivate immediately on state change, then show after delay
                    if (winPanel != null) winPanel.SetActive(false);
                    if (losePanel != null) losePanel.SetActive(false);
                    if (pausePanel != null) pausePanel.SetActive(false);
                    
                    panelDelayCoroutine = StartCoroutine(ShowPanelDelayed(state));
                    break;
            }
        }

        private void OnPauseButtonClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }
        }

        private void OnResumeButtonClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
        }

        private System.Collections.IEnumerator ShowPanelDelayed(GameState state)
        {
            yield return new WaitForSeconds(showPanelDelay);

            if (state == GameState.GameOverWon)
            {
                if (winPanel != null) winPanel.SetActive(true);
                if (losePanel != null) losePanel.SetActive(false);
            }
            else if (state == GameState.GameOverLost)
            {
                if (winPanel != null) winPanel.SetActive(false);
                if (losePanel != null) losePanel.SetActive(true);
            }

            panelDelayCoroutine = null;
        }

        private void HandleScoreChanged(int current, int total)
        {
            if (scoreText != null)
            {
                scoreText.text = $"{current} / {total}";
            }
        }

        private void HandleLivesChanged(int lives)
        {
            if (livesText != null)
            {
                livesText.text = $"{lives}";
            }

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
                timerText.text = $"{Mathf.CeilToInt(timeRemaining)}s";
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

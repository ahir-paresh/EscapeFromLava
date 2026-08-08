using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeFromLava
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Loop Settings")]
        [SerializeField] private int startLives = 5;
        [SerializeField] private float timeLimit = 30f;
        [SerializeField] private Camera mainCamera;

        // Current game values
        private int currentLives;
        private float timeRemaining;
        private int collectedDiamonds;
        private int totalDiamonds;
        private GameState currentState = GameState.Ready;

        private TileController activeClickedTile;

        // Public getters for UI
        public int CurrentLives => currentLives;
        public float TimeRemaining => timeRemaining;
        public int CollectedDiamonds => collectedDiamonds;
        public int TotalDiamonds => totalDiamonds;
        public GameState CurrentState => currentState;

        private void Awake()
        {
            // Simple Singleton pattern
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void Start()
        {
            InitializeGame();
        }

        public void InitializeGame()
        {
            // Reset timescale to normal in case we were paused
            Time.timeScale = 1f;

            // Find and count all diamonds in the scene
            TileController[] tiles = FindObjectsOfType<TileController>();
            totalDiamonds = 0;
            foreach (var tile in tiles)
            {
                if (tile.Type == TileType.BlueDiamond)
                {
                    totalDiamonds++;
                }
            }

            currentLives = startLives;
            timeRemaining = timeLimit;
            collectedDiamonds = 0;
            
            SetGameState(GameState.Playing);

            // Trigger initial events to update UI
            GameEventManager.TriggerScoreChanged(collectedDiamonds, totalDiamonds);
            GameEventManager.TriggerLivesChanged(currentLives);
            GameEventManager.TriggerTimerChanged(timeRemaining);
        }

        private void Update()
        {
            if (currentState != GameState.Playing) return;

            // Update countdown timer
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = 0f;
                GameEventManager.TriggerTimerChanged(timeRemaining);
                LoseGame();
                return;
            }
            GameEventManager.TriggerTimerChanged(timeRemaining);

            // Handle Input Interaction
            HandleInput();
        }

        private void HandleInput()
        {
            // Mouse/Touch Down
            if (Input.GetMouseButtonDown(0))
            {
                activeClickedTile = RaycastTile(out Vector3 hitPoint);
                if (activeClickedTile != null)
                {
                    activeClickedTile.TriggerClickDown();
                    GameEventManager.TriggerTileClicked(activeClickedTile, hitPoint);
                    
                    ProcessTileInteraction(activeClickedTile);
                }
            }
            // Mouse/Touch Up
            else if (Input.GetMouseButtonUp(0))
            {
                if (activeClickedTile != null)
                {
                    activeClickedTile.TriggerClickUp();
                    activeClickedTile = null;
                }
            }
        }

        private void ProcessTileInteraction(TileController tile)
        {
            switch (tile.Type)
            {
                case TileType.BlueDiamond:
                    // Check if already collected
                    if (tile.IsCollected) break;

                    tile.IsCollected = true;
                    collectedDiamonds++;
                    GameEventManager.TriggerScoreChanged(collectedDiamonds, totalDiamonds);

                    // Check Win Condition
                    if (collectedDiamonds >= totalDiamonds)
                    {
                        WinGame();
                    }
                    break;

                case TileType.RedLava:
                    // Damage Player
                    currentLives--;
                    GameEventManager.TriggerLivesChanged(currentLives);

                    // Check Lose Condition
                    if (currentLives <= 0)
                    {
                        currentLives = 0;
                        LoseGame();
                    }
                    break;

                case TileType.GreenIsland:
                case TileType.DarkStone:
                    // Safe zone, nothing happens
                    break;
            }
        }

        private TileController RaycastTile(out Vector3 hitPoint)
        {
            hitPoint = Vector3.zero;
            if (mainCamera == null) return null;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            // 1. Try 3D Physics raycast
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                hitPoint = hit.point;
                return hit.collider.GetComponentInParent<TileController>();
            }

            // 2. Try 2D Physics raycast (intersection with 2D colliders)
            RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);
            if (hit2D.collider != null)
            {
                hitPoint = hit2D.point;
                return hit2D.collider.GetComponentInParent<TileController>();
            }

            return null;
        }

        private void SetGameState(GameState state)
        {
            currentState = state;
            GameEventManager.TriggerGameStateChanged(state);
        }

        private void WinGame()
        {
            SetGameState(GameState.GameOverWon);
            Debug.Log("EscapeFromLava: Level Completed! Player Won!");

            // Calculate and submit completion time to Google Play Services Leaderboard
            float completionTime = timeLimit - timeRemaining;
            if (GPGSLeaderboardManager.Instance != null)
            {
                GPGSLeaderboardManager.Instance.SubmitLevelCompletionTime(completionTime);
            }
        }

        private void LoseGame()
        {
            SetGameState(GameState.GameOverLost);
            Debug.Log("EscapeFromLava: Game Over! Player Lost!");
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }

        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;
            SetGameState(GameState.Paused);
            Time.timeScale = 0f;
            Debug.Log("EscapeFromLava: Game Paused.");
        }

        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;
            SetGameState(GameState.Playing);
            Time.timeScale = 1f;
            Debug.Log("EscapeFromLava: Game Resumed.");
        }

        /// <summary>
        /// Restarts the current active level scene.
        /// </summary>
        public void RestartLevel()
        {
            Time.timeScale = 1f; // Ensure time is restored on load
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}

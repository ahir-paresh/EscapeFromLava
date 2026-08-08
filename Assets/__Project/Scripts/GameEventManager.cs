using System;
using UnityEngine;

namespace EscapeFromLava
{
    public static class GameEventManager
    {
        public static event Action<GameState> OnGameStateChanged;
        public static event Action<int, int> OnScoreChanged; // (collectedCount, totalInLevel)
        public static event Action<int> OnLivesChanged;      // (currentLives)
        public static event Action<float> OnTimerChanged;    // (timeRemainingSeconds)
        public static event Action<TileController, Vector3> OnTileClicked; // (tileClicked, clickWorldPosition)

        public static void TriggerGameStateChanged(GameState state)
        {
            OnGameStateChanged?.Invoke(state);
        }

        public static void TriggerScoreChanged(int score, int total)
        {
            OnScoreChanged?.Invoke(score, total);
        }

        public static void TriggerLivesChanged(int lives)
        {
            OnLivesChanged?.Invoke(lives);
        }

        public static void TriggerTimerChanged(float timeRemaining)
        {
            OnTimerChanged?.Invoke(timeRemaining);
        }

        public static void TriggerTileClicked(TileController tile, Vector3 position)
        {
            OnTileClicked?.Invoke(tile, position);
        }
    }
}

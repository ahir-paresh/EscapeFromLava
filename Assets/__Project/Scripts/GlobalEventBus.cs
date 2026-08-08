using UnityEngine;
using UnityEngine.Events;

namespace EscapeFromLava
{
    public class GlobalEventBus : MonoBehaviour
    {
        [Header("Tile Interaction Events")]
        [SerializeField] private UnityEvent onGrassClicked;
        [SerializeField] private UnityEvent onLavaClicked;
        [SerializeField] private UnityEvent onDiamondClicked;

        [Header("Game State Events")]
        [SerializeField] private UnityEvent onGameWon;
        [SerializeField] private UnityEvent onGameLost;

        public UnityEvent OnGrassClicked => onGrassClicked;
        public UnityEvent OnLavaClicked => onLavaClicked;
        public UnityEvent OnDiamondClicked => onDiamondClicked;
        public UnityEvent OnGameWon => onGameWon;
        public UnityEvent OnGameLost => onGameLost;

        private void OnEnable()
        {
            GameEventManager.OnTileClicked += HandleTileClicked;
            GameEventManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            GameEventManager.OnTileClicked -= HandleTileClicked;
            GameEventManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleTileClicked(TileController tile, Vector3 position)
        {
            if (tile == null) return;

            switch (tile.Type)
            {
                case TileType.GreenIsland:
                case TileType.DarkStone:
                    onGrassClicked?.Invoke();
                    break;

                case TileType.RedLava:
                    onLavaClicked?.Invoke();
                    break;

                case TileType.BlueDiamond:
                    onDiamondClicked?.Invoke();
                    break;
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.GameOverWon:
                    onGameWon?.Invoke();
                    break;

                case GameState.GameOverLost:
                    onGameLost?.Invoke();
                    break;
            }
        }
    }
}

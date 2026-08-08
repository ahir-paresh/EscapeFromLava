using UnityEngine;

namespace EscapeFromLava
{
    public class EffectSpawner : MonoBehaviour
    {
        [Header("FX Prefabs")]
        [SerializeField] private GameObject scorePopupPrefab;
        [SerializeField] private GameObject damageSplashPrefab;

        [Header("FX Lifetime Settings")]
        [SerializeField] private float scorePopupLifetime = 1.5f;
        [SerializeField] private float damageSplashLifetime = 2.0f;

        [Header("Positioning Offset")]
        [SerializeField] private Vector3 scorePopupOffset = new Vector3(0f, 0.5f, 0f);
        [SerializeField] private Vector3 damageSplashOffset = new Vector3(0f, 0.2f, 0f);

        private void OnEnable()
        {
            GameEventManager.OnTileClicked += SpawnTileEffect;
        }

        private void OnDisable()
        {
            GameEventManager.OnTileClicked -= SpawnTileEffect;
        }

        private void SpawnTileEffect(TileController tile, Vector3 worldPosition)
        {
            if (tile == null) return;

            switch (tile.Type)
            {
                case TileType.BlueDiamond:
                    if (scorePopupPrefab != null)
                    {
                        GameObject popup = Instantiate(scorePopupPrefab, worldPosition + scorePopupOffset, Quaternion.identity);
                        Destroy(popup, scorePopupLifetime);
                    }
                    break;

                case TileType.RedLava:
                    if (damageSplashPrefab != null)
                    {
                        GameObject splash = Instantiate(damageSplashPrefab, worldPosition + damageSplashOffset, Quaternion.identity);
                        Destroy(splash, damageSplashLifetime);
                    }
                    break;
            }
        }
    }
}

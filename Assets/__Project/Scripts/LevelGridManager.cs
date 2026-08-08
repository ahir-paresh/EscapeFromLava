using UnityEngine;

namespace EscapeFromLava
{
    public enum GridLayoutType
    {
        Orthogonal,
        Isometric,
        AdvanceOrthographic
    }

    public enum GridPlaneType
    {
        XZ_3D, // Horizontal plane (X and Z)
        XY_2D  // Vertical plane (X and Y)
    }

    [ExecuteInEditMode]
    public class LevelGridManager : MonoBehaviour
    {
        [Header("Level Configuration")]
        [SerializeField] private LevelData activeLevel;
        [SerializeField] private GridLayoutType layoutType = GridLayoutType.Isometric;
        [SerializeField] private GridPlaneType planeType = GridPlaneType.XZ_3D;
        [SerializeField] private Vector2 cellSpacing = new Vector2(1f, 1f);
        [SerializeField] private int paddingX = 0;
        [SerializeField] private int paddingY = 0;

        [Header("Idle Wave Animation")]
        [SerializeField] private bool enableWaveAnimation = true;
        [SerializeField] private float waveAmplitude = 0.08f;
        [SerializeField] private float waveFrequency = 2.5f;
        [SerializeField] private float wavePhaseOffset = 0.25f;

        [Header("Tile Prefabs")]
        [SerializeField] private GameObject darkStonePrefab;
        [SerializeField] private GameObject greenIslandPrefab;
        [SerializeField] private GameObject redLavaPrefab;
        [SerializeField] private GameObject blueDiamondPrefab;

        [Header("References")]
        [SerializeField] private Transform gridRoot;

        public LevelData ActiveLevel
        {
            get => activeLevel;
            set => activeLevel = value;
        }

        public GridLayoutType LayoutType
        {
            get => layoutType;
            set => layoutType = value;
        }

        public GridPlaneType PlaneType
        {
            get => planeType;
            set => planeType = value;
        }

        public Vector2 CellSpacing
        {
            get => cellSpacing;
            set => cellSpacing = value;
        }

        public int PaddingX
        {
            get => paddingX;
            set => paddingX = Mathf.Max(0, value);
        }

        public int PaddingY
        {
            get => paddingY;
            set => paddingY = Mathf.Max(0, value);
        }

        public bool EnableWaveAnimation
        {
            get => enableWaveAnimation;
            set => enableWaveAnimation = value;
        }

        public float WaveAmplitude
        {
            get => waveAmplitude;
            set => waveAmplitude = Mathf.Max(0f, value);
        }

        public float WaveFrequency
        {
            get => waveFrequency;
            set => waveFrequency = Mathf.Max(0f, value);
        }

        public float WavePhaseOffset
        {
            get => wavePhaseOffset;
            set => wavePhaseOffset = value;
        }

        public GameObject GetPrefabForType(TileType type)
        {
            return type switch
            {
                TileType.DarkStone => darkStonePrefab,
                TileType.GreenIsland => greenIslandPrefab,
                TileType.RedLava => redLavaPrefab,
                TileType.BlueDiamond => blueDiamondPrefab,
                _ => null
            };
        }

        [ContextMenu("Generate Grid")]
        public void GenerateGrid()
        {
            ClearGrid();

            if (activeLevel == null)
            {
                Debug.LogWarning("EscapeFromLava: No active LevelData assigned to LevelGridManager.");
                return;
            }

            // Create or get root
            if (gridRoot == null)
            {
                GameObject rootGo = GameObject.Find("GridRoot");
                if (rootGo == null)
                {
                    rootGo = new GameObject("GridRoot");
                    rootGo.transform.SetParent(transform);
                    rootGo.transform.localPosition = Vector3.zero;
                    rootGo.transform.localRotation = Quaternion.identity;
                }
                gridRoot = rootGo.transform;
            }

            int cols = activeLevel.Columns;
            int rows = activeLevel.Rows;

            int minCol = -paddingX;
            int maxCol = cols + paddingX;
            int minRow = -paddingY;
            int maxRow = rows + paddingY;

            for (int r = minRow; r < maxRow; r++)
            {
                for (int c = minCol; c < maxCol; c++)
                {
                    TileType tileType = TileType.DarkStone;
                    if (c >= 0 && c < cols && r >= 0 && r < rows)
                    {
                        tileType = activeLevel.GetTile(c, r);
                    }

                    GameObject prefab = GetPrefabForType(tileType);
                    if (prefab == null) continue;

                    Vector3 position = CalculatePosition(c, r);
                    
                    GameObject instantiatedTile = InstantiateTile(prefab, gridRoot);
                    if (instantiatedTile != null)
                    {
                        instantiatedTile.name = $"Tile_{c}_{r}_{tileType}";
                        instantiatedTile.transform.localPosition = position;
                        // Match prefab rotation by default
                        instantiatedTile.transform.localRotation = prefab.transform.rotation;

                        // Mirror even rows for AdvanceOrthographic layout
                        Vector3 finalScale = prefab.transform.localScale;
                        if (layoutType == GridLayoutType.AdvanceOrthographic && r % 2 == 0)
                        {
                            finalScale.x = -finalScale.x;
                        }
                        instantiatedTile.transform.localScale = finalScale;

                        // Attach and initialize TileController component
                        TileController tileController = instantiatedTile.GetComponent<TileController>();
                        if (tileController == null)
                        {
                            tileController = instantiatedTile.AddComponent<TileController>();
                        }
                        tileController.Initialize(tileType, c, r);

                        // Apply Wave bobbing if enabled
                        if (enableWaveAnimation)
                        {
                            FloatingTile floater = instantiatedTile.GetComponent<FloatingTile>();
                            if (floater == null)
                            {
                                floater = instantiatedTile.AddComponent<FloatingTile>();
                            }
                            floater.Amplitude = waveAmplitude;
                            floater.Frequency = waveFrequency;
                            floater.PhaseOffset = wavePhaseOffset;
                            floater.FloatDirection = Vector3.up;
                            floater.Initialize(c, r, position);
                        }
                    }
                }
            }

            Debug.Log($"EscapeFromLava: Generated {cols}x{rows} grid under '{gridRoot.name}'.");
        }

        [ContextMenu("Clear Grid")]
        public void ClearGrid()
        {
            if (gridRoot != null)
            {
                DestroyChildren(gridRoot);
            }
            else
            {
                // Try finding GridRoot
                GameObject rootGo = GameObject.Find("GridRoot");
                if (rootGo != null)
                {
                    DestroyChildren(rootGo.transform);
                }
            }
        }

        public Vector3 CalculatePosition(int col, int row)
        {
            float posX, posY, posZ;

            if (layoutType == GridLayoutType.Orthogonal)
            {
                posX = col * cellSpacing.x;
                posZ = row * cellSpacing.y;
                posY = 0f;
            }
            else if (layoutType == GridLayoutType.AdvanceOrthographic)
            {
                // AdvanceOrthographic staggered layout: even rows shifted horizontally by half cellSpacing.x
                posX = col * cellSpacing.x;
                posZ = row * cellSpacing.y;
                posY = 0f;

                if (row % 2 == 0)
                {
                    posX -= cellSpacing.x * 0.5f;
                }
            }
            else // Isometric
            {
                // Isometric grid calculation
                posX = (col - row) * (cellSpacing.x / 2f);
                posZ = (col + row) * (cellSpacing.y / 2f);
                posY = 0f;
            }

            if (planeType == GridPlaneType.XY_2D)
            {
                // Map Z to Y for 2D plane rendering
                return new Vector3(posX, posZ, posY);
            }
            else
            {
                // Map to 3D horizontal plane (X and Z)
                return new Vector3(posX, posY, posZ);
            }
        }

        private GameObject InstantiateTile(GameObject prefab, Transform parent)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parent);
            }
#endif
            return Instantiate(prefab, parent);
        }

        private void DestroyChildren(Transform parent)
        {
            // Gather all children first to avoid collection modification issues during deletion
            int childCount = parent.childCount;
            GameObject[] children = new GameObject[childCount];
            for (int i = 0; i < childCount; i++)
            {
                children[i] = parent.GetChild(i).gameObject;
            }

            foreach (var child in children)
            {
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }
    }
}

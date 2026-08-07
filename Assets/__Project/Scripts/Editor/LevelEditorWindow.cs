using UnityEngine;
using UnityEditor;
using System.IO;

namespace EscapeFromLava
{
    public class LevelEditorWindow : EditorWindow
    {
        private LevelData currentLevelAsset;
        private TileType selectedBrush = TileType.GreenIsland;
        private LevelGridManager sceneGridManager;

        private Vector2 gridScrollPosition;
        
        // Temp variables for dimensions resizing
        private int tempCols = 16;
        private int tempRows = 8;

        [MenuItem("Window/Escape From Lava/Level Editor")]
        public static void OpenWindow()
        {
            LevelEditorWindow window = GetWindow<LevelEditorWindow>("Level Editor");
            window.minSize = new Vector2(580, 500);
            window.Show();
        }

        private void OnEnable()
        {
            // Auto-find GridManager in active scene
            FindGridManagerInScene();
            
            if (sceneGridManager != null && sceneGridManager.ActiveLevel != null)
            {
                currentLevelAsset = sceneGridManager.ActiveLevel;
            }

            if (currentLevelAsset != null)
            {
                tempCols = currentLevelAsset.Columns;
                tempRows = currentLevelAsset.Rows;
            }
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawAssetSelector();

            if (currentLevelAsset == null)
            {
                EditorGUILayout.HelpBox("Please select or create a LevelData asset to begin editing.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(5);
            DrawGridDimensions();
            
            EditorGUILayout.Space(5);
            DrawBrushSelector();

            EditorGUILayout.Space(5);
            DrawGridPainter();

            EditorGUILayout.Space(10);
            DrawSceneControls();
        }

        private void FindGridManagerInScene()
        {
            sceneGridManager = FindFirstObjectByType<LevelGridManager>();
        }

        private void DrawHeader()
        {
            GUILayout.Label("Escape The Lava - Level Creator", new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 10, 10)
            });
        }

        private void DrawAssetSelector()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Level Data Asset", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            LevelData selectedAsset = (LevelData)EditorGUILayout.ObjectField("Active Level:", currentLevelAsset, typeof(LevelData), false);
            if (selectedAsset != currentLevelAsset)
            {
                currentLevelAsset = selectedAsset;
                if (currentLevelAsset != null)
                {
                    tempCols = currentLevelAsset.Columns;
                    tempRows = currentLevelAsset.Rows;
                    if (sceneGridManager != null)
                    {
                        sceneGridManager.ActiveLevel = currentLevelAsset;
                    }
                }
            }

            if (GUILayout.Button("Create New", GUILayout.Width(100)))
            {
                CreateNewLevelAsset();
            }

            EditorGUILayout.EndHorizontal();

            if (currentLevelAsset != null)
            {
                if (GUILayout.Button("Save Asset File Changes", GUILayout.Height(25)))
                {
                    AssetDatabase.SaveAssets();
                    Debug.Log("EscapeFromLava: Saved LevelData changes to disk.");
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void CreateNewLevelAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Level Data",
                "NewLevelData",
                "asset",
                "Enter a name for the new level asset file."
            );

            if (string.IsNullOrEmpty(path)) return;

            LevelData newLevel = ScriptableObject.CreateInstance<LevelData>();
            
            // Set some initial data
            newLevel.Resize(16, 8);
            newLevel.Clear();

            AssetDatabase.CreateAsset(newLevel, path);
            AssetDatabase.SaveAssets();

            currentLevelAsset = newLevel;
            tempCols = 16;
            tempRows = 8;

            if (sceneGridManager != null)
            {
                sceneGridManager.ActiveLevel = currentLevelAsset;
            }

            Selection.activeObject = newLevel;
            Debug.Log($"EscapeFromLava: Created new level data at {path}");
        }

        private void DrawGridDimensions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Grid Dimensions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            
            tempCols = EditorGUILayout.IntField("Columns:", tempCols);
            tempRows = EditorGUILayout.IntField("Rows:", tempRows);

            tempCols = Mathf.Max(1, tempCols);
            tempRows = Mathf.Max(1, tempRows);

            bool sizeChanged = (tempCols != currentLevelAsset.Columns || tempRows != currentLevelAsset.Rows);
            
            GUI.enabled = sizeChanged;
            if (GUILayout.Button("Apply Resize", GUILayout.Width(100)))
            {
                if (EditorUtility.DisplayDialog("Resize Grid", 
                    $"Are you sure you want to resize grid to {tempCols}x{tempRows}? This will keep overlapping elements but truncate others.", 
                    "Resize", "Cancel"))
                {
                    currentLevelAsset.Resize(tempCols, tempRows);
                }
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawBrushSelector()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Paint Brush Selector", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            DrawBrushButton(TileType.DarkStone, "Dark Stone (Default)", new Color(0.2f, 0.2f, 0.2f));
            DrawBrushButton(TileType.GreenIsland, "Green Island (Safe)", new Color(0.15f, 0.6f, 0.15f));
            DrawBrushButton(TileType.RedLava, "Red Lava (Danger)", new Color(0.8f, 0.2f, 0.1f));
            DrawBrushButton(TileType.BlueDiamond, "Blue Diamond (Diamond)", new Color(0.1f, 0.5f, 0.8f));

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.HelpBox("Left-click and Drag to Paint. Right-click any cell to pick its tile type as the active brush.", MessageType.Info);
            
            EditorGUILayout.EndVertical();
        }

        private void DrawBrushButton(TileType type, string label, Color color)
        {
            Color originalBg = GUI.backgroundColor;
            bool isSelected = (selectedBrush == type);
            
            GUI.backgroundColor = isSelected ? color : color * 0.5f;
            
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal
            };

            if (GUILayout.Button(label, buttonStyle, GUILayout.Height(30)))
            {
                selectedBrush = type;
            }

            GUI.backgroundColor = originalBg;
        }

        private void DrawGridPainter()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Visual Grid Editor", EditorStyles.boldLabel);

            gridScrollPosition = EditorGUILayout.BeginScrollView(gridScrollPosition, GUILayout.MaxHeight(400));
            
            int cols = currentLevelAsset.Columns;
            int rows = currentLevelAsset.Rows;
            
            float cellSize = 32f;
            float spacing = 2f;
            
            // Allocate layout space for the grid manually to allow drag interaction coordinates
            float totalWidth = cols * (cellSize + spacing);
            float totalHeight = rows * (cellSize + spacing);
            
            Rect gridRect = GUILayoutUtility.GetRect(totalWidth, totalHeight);
            
            // Draw backgrounds and handle input events
            Event currentEvent = Event.current;
            bool isMouseAction = currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag;

            // Paint Row 0 at the bottom to match 3D Z orientation
            for (int r = rows - 1; r >= 0; r--)
            {
                float drawY = gridRect.y + (rows - 1 - r) * (cellSize + spacing);
                
                for (int c = 0; c < cols; c++)
                {
                    float drawX = gridRect.x + c * (cellSize + spacing);
                    Rect cellRect = new Rect(drawX, drawY, cellSize, cellSize);

                    TileType cellType = currentLevelAsset.GetTile(c, r);
                    Color cellColor = GetColorForTileType(cellType);
                    string cellLabel = GetAbbreviationForTileType(cellType);

                    // Draw the cell visually
                    Color originalBg = GUI.backgroundColor;
                    GUI.backgroundColor = cellColor;
                    
                    GUIStyle cellStyle = new GUIStyle(GUI.skin.button)
                    {
                        fontSize = 9,
                        normal = { textColor = Color.white },
                        alignment = TextAnchor.MiddleCenter
                    };
                    
                    GUI.Box(cellRect, cellLabel, cellStyle);
                    GUI.backgroundColor = originalBg;

                    // Handle Paint / Sample interaction
                    if (isMouseAction && cellRect.Contains(currentEvent.mousePosition))
                    {
                        if (currentEvent.button == 0) // Left click / Left Drag -> Paint
                        {
                            if (currentLevelAsset.GetTile(c, r) != selectedBrush)
                            {
                                Undo.RecordObject(currentLevelAsset, "Paint Tile");
                                currentLevelAsset.SetTile(c, r, selectedBrush);
                                Repaint();
                            }
                        }
                        else if (currentEvent.button == 1) // Right click -> Dropper/Pick type
                        {
                            if (selectedBrush != cellType)
                            {
                                selectedBrush = cellType;
                                Repaint();
                            }
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fill Entire Grid with Current Brush"))
            {
                if (EditorUtility.DisplayDialog("Fill Grid", 
                    $"Are you sure you want to replace all tiles with {selectedBrush}?", "Fill", "Cancel"))
                {
                    Undo.RecordObject(currentLevelAsset, "Fill Level Grid");
                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < cols; c++)
                        {
                            currentLevelAsset.SetTile(c, r, selectedBrush);
                        }
                    }
                }
            }
            if (GUILayout.Button("Clear Grid (Fill with Dark Stone)"))
            {
                if (EditorUtility.DisplayDialog("Clear Grid", 
                    "Are you sure you want to clear the entire grid? All tiles will reset to Dark Stone.", "Clear", "Cancel"))
                {
                    Undo.RecordObject(currentLevelAsset, "Clear Level Grid");
                    currentLevelAsset.Clear();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private Color GetColorForTileType(TileType type)
        {
            return type switch
            {
                TileType.DarkStone => new Color(0.25f, 0.25f, 0.25f),
                TileType.GreenIsland => new Color(0.2f, 0.7f, 0.2f),
                TileType.RedLava => new Color(0.9f, 0.25f, 0.15f),
                TileType.BlueDiamond => new Color(0.15f, 0.6f, 0.9f),
                _ => Color.grey
            };
        }

        private string GetAbbreviationForTileType(TileType type)
        {
            return type switch
            {
                TileType.DarkStone => "DS",
                TileType.GreenIsland => "GI",
                TileType.RedLava => "RL",
                TileType.BlueDiamond => "BD",
                _ => "?"
            };
        }

        private void DrawSceneControls()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Scene Generation Controls", EditorStyles.boldLabel);

            if (sceneGridManager == null)
            {
                FindGridManagerInScene();
            }

            sceneGridManager = (LevelGridManager)EditorGUILayout.ObjectField("Scene Manager:", sceneGridManager, typeof(LevelGridManager), true);

            if (sceneGridManager == null)
            {
                EditorGUILayout.HelpBox("No LevelGridManager component found in the scene. Please create one to instantiate the level layout in 3D.", MessageType.Warning);
                if (GUILayout.Button("Create LevelGridManager GameObject", GUILayout.Height(30)))
                {
                    GameObject go = new GameObject("LevelGridManager");
                    sceneGridManager = go.AddComponent<LevelGridManager>();
                    sceneGridManager.ActiveLevel = currentLevelAsset;
                    Undo.RegisterCreatedObjectUndo(go, "Create LevelGridManager");
                }
                EditorGUILayout.EndVertical();
                return;
            }

            // Draw direct configuration fields for the manager to keep it easy to test different setups
            EditorGUILayout.Space(2);
            EditorGUI.indentLevel++;
            sceneGridManager.LayoutType = (GridLayoutType)EditorGUILayout.EnumPopup("Layout Type:", sceneGridManager.LayoutType);
            sceneGridManager.PlaneType = (GridPlaneType)EditorGUILayout.EnumPopup("Grid Plane:", sceneGridManager.PlaneType);
            sceneGridManager.CellSpacing = EditorGUILayout.Vector2Field("Cell Spacing:", sceneGridManager.CellSpacing);
            sceneGridManager.PaddingX = EditorGUILayout.IntField("Padding X (Cols):", sceneGridManager.PaddingX);
            sceneGridManager.PaddingY = EditorGUILayout.IntField("Padding Y (Rows):", sceneGridManager.PaddingY);
            sceneGridManager.EnableWaveAnimation = EditorGUILayout.Toggle("Wave Animation:", sceneGridManager.EnableWaveAnimation);
            if (sceneGridManager.EnableWaveAnimation)
            {
                EditorGUI.indentLevel++;
                sceneGridManager.WaveAmplitude = EditorGUILayout.FloatField("Wave Amplitude:", sceneGridManager.WaveAmplitude);
                sceneGridManager.WaveFrequency = EditorGUILayout.FloatField("Wave Frequency:", sceneGridManager.WaveFrequency);
                sceneGridManager.WavePhaseOffset = EditorGUILayout.FloatField("Phase Offset:", sceneGridManager.WavePhaseOffset);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            
            Color origColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
            if (GUILayout.Button("Generate Grid in Scene", GUILayout.Height(35)))
            {
                // Assign level asset if not set or mismatched
                if (sceneGridManager.ActiveLevel != currentLevelAsset)
                {
                    sceneGridManager.ActiveLevel = currentLevelAsset;
                }
                
                Undo.RecordObject(sceneGridManager, "Generate Level Grid");
                sceneGridManager.GenerateGrid();
                
                // Force scene views to refresh immediately
                SceneView.RepaintAll();
            }

            GUI.backgroundColor = new Color(0.9f, 0.3f, 0.2f);
            if (GUILayout.Button("Clear Scene Grid", GUILayout.Height(35)))
            {
                Undo.RecordObject(sceneGridManager, "Clear Level Grid");
                sceneGridManager.ClearGrid();
                SceneView.RepaintAll();
            }
            GUI.backgroundColor = origColor;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }
}

using UnityEngine;

namespace EscapeFromLava
{
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Escape From Lava/Level Data", order = 1)]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private int columns = 16;
        [SerializeField] private int rows = 8;
        [SerializeField] private TileType[] cells;

        public int Columns => columns;
        public int Rows => rows;

        private void OnEnable()
        {
            // Initialize array if null or empty
            if (cells == null || cells.Length != columns * rows)
            {
                cells = new TileType[columns * rows];
            }
        }

        public TileType GetTile(int col, int row)
        {
            if (col < 0 || col >= columns || row < 0 || row >= rows)
            {
                return TileType.DarkStone;
            }
            return cells[col + row * columns];
        }

        public void SetTile(int col, int row, TileType type)
        {
            if (col < 0 || col >= columns || row < 0 || row >= rows) return;
            cells[col + row * columns] = type;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public void Resize(int newCols, int newRows)
        {
            if (newCols <= 0 || newRows <= 0) return;

            TileType[] newCells = new TileType[newCols * newRows];

            // Copy old cells to new cells
            if (cells != null)
            {
                for (int r = 0; r < Mathf.Min(rows, newRows); r++)
                {
                    for (int c = 0; c < Mathf.Min(columns, newCols); c++)
                    {
                        newCells[c + r * newCols] = cells[c + r * columns];
                    }
                }
            }

            columns = newCols;
            rows = newRows;
            cells = newCells;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public void Clear()
        {
            cells = new TileType[columns * rows];
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}

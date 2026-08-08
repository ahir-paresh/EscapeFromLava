using UnityEngine;

namespace EscapeFromLava
{
    public class TileController : MonoBehaviour
    {
        [Header("Tile Settings")]
        [SerializeField] private TileType tileType;
        [SerializeField] private int column;
        [SerializeField] private int row;

        [Header("References")]
        [SerializeField] private Animator animator;

        public TileType Type => tileType;
        public int Col => column;
        public int Row => row;
        public bool IsCollected { get; set; } = false;

        private void Awake()
        {
            // Cache animator if not assigned
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }

        /// <summary>
        /// Initializer method called when LevelGridManager instantiates this tile.
        /// </summary>
        public void Initialize(TileType type, int col, int r)
        {
            tileType = type;
            column = col;
            row = r;
            
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }

        /// <summary>
        /// Call when mouse click goes down on this tile.
        /// </summary>
        public void TriggerClickDown()
        {
            if (animator != null)
            {
                animator.SetTrigger("Click");
            }
        }

        /// <summary>
        /// Call when mouse click is released.
        /// </summary>
        public void TriggerClickUp()
        {
            if (animator != null)
            {
                animator.SetTrigger("ClickUp");
            }
        }
    }
}

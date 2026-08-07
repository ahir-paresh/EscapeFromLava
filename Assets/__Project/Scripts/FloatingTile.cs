using UnityEngine;

namespace EscapeFromLava
{
    public class FloatingTile : MonoBehaviour
    {
        [Header("Wave Settings")]
        [Tooltip("How far the tile floats up and down.")]
        [SerializeField] private float amplitude = 0.1f;
        
        [Tooltip("Speed of the wave animation.")]
        [SerializeField] private float frequency = 2f;
        
        [Tooltip("Phase offset between adjacent tiles to create the wave ripple effect.")]
        [SerializeField] private float phaseOffset = 0.3f;

        [Tooltip("Local axis to apply the floating animation to (normally Y).")]
        [SerializeField] private Vector3 floatDirection = Vector3.up;

        private Vector3 startPosition;
        private float waveOffset;
        private bool isInitialized = false;

        public float Amplitude
        {
            get => amplitude;
            set => amplitude = value;
        }

        public float Frequency
        {
            get => frequency;
            set => frequency = value;
        }

        public float PhaseOffset
        {
            get => phaseOffset;
            set => phaseOffset = value;
        }

        public Vector3 FloatDirection
        {
            get => floatDirection;
            set => floatDirection = value.normalized;
        }

        private void Start()
        {
            // Ensure start position is recorded if not initialized manually
            if (!isInitialized)
            {
                startPosition = transform.localPosition;
                isInitialized = true;
            }
        }

        public void Initialize(int gridX, int gridY, Vector3 startPos)
        {
            startPosition = startPos;
            waveOffset = (gridX + gridY) * phaseOffset;
            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized) return;

            // Calculate the bobbing offset using a sine wave
            float offset = Mathf.Sin(Time.time * frequency + waveOffset) * amplitude;
            
            // Apply translation along the float direction from the start position
            transform.localPosition = startPosition + (floatDirection * offset);
        }
    }
}

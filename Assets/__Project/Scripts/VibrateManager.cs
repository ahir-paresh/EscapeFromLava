using UnityEngine;
using Solo.MOST_IN_ONE;

namespace EscapeFromLava
{
    public class VibrateManager : MonoBehaviour
    {
        [Header("Tile Haptic Settings")]
        [SerializeField] private MOST_HapticFeedback.HapticTypes grassClickHaptic = MOST_HapticFeedback.HapticTypes.SoftImpact;
        [SerializeField] private MOST_HapticFeedback.HapticTypes lavaClickHaptic = MOST_HapticFeedback.HapticTypes.HeavyImpact;
        [SerializeField] private MOST_HapticFeedback.HapticTypes diamondClickHaptic = MOST_HapticFeedback.HapticTypes.MediumImpact;

        [Header("Game State Haptic Settings")]
        [SerializeField] private MOST_HapticFeedback.HapticTypes winHaptic = MOST_HapticFeedback.HapticTypes.Success;
        [SerializeField] private MOST_HapticFeedback.HapticTypes loseHaptic = MOST_HapticFeedback.HapticTypes.Failure;

        public void VibrateGrassClick()
        {
            TriggerHaptic(grassClickHaptic);
        }

        public void VibrateLavaClick()
        {
            TriggerHaptic(lavaClickHaptic);
        }

        public void VibrateDiamondClick()
        {
            TriggerHaptic(diamondClickHaptic);
        }

        public void VibrateWin()
        {
            TriggerHaptic(winHaptic);
        }

        public void VibrateLose()
        {
            TriggerHaptic(loseHaptic);
        }

        /// <summary>
        /// Generic method to trigger any MOST Haptic Feedback type.
        /// </summary>
        public void TriggerHaptic(MOST_HapticFeedback.HapticTypes type)
        {
            // Verify if haptics are supported and enabled in the plugin
            if (MOST_HapticFeedback.HapticsEnabled)
            {
                MOST_HapticFeedback.Generate(type);
            }
            else
            {
                // Fallback to Unity default Handheld Vibrate if haptics are disabled or unsupported
                #if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
                Handheld.Vibrate();
                #endif
            }
        }
    }
}

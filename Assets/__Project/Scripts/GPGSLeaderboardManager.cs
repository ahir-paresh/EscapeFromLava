using UnityEngine;

#if USE_GPGS
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

namespace EscapeFromLava
{
    public class GPGSLeaderboardManager : MonoBehaviour
    {
        public static GPGSLeaderboardManager Instance { get; private set; }

        [Header("Leaderboard Settings")]
        [Tooltip("The ID of the leaderboard created in Google Play Console (e.g. CgkI123456789_AIQBg)")]
        [SerializeField] private string level1TimeLeaderboardId = "";

        private bool isAuthenticated = false;

        public bool IsAuthenticated => isAuthenticated;

        private void Awake()
        {
            // Simple persistent singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            InitializeGPGS();
        }

        /// <summary>
        /// Initializes Google Play Games Platform and attempts silent authentication.
        /// </summary>
        public void InitializeGPGS()
        {
#if USE_GPGS
            // Configure Play Games platform parameters
            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().Build();
            PlayGamesPlatform.InitializeInstance(config);
            
            // Enable debugging logs
            PlayGamesPlatform.DebugLogEnabled = true;

            // Activate Google Play Games as the active Social Platform
            PlayGamesPlatform.Activate();

            // Try logging in the user automatically
            AuthenticateUser(true);
#else
            Debug.Log("GPGS: USE_GPGS is not defined. Google Play Games Service is inactive.");
#endif
        }

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <param name="silent">If true, attempts silent sign-in. If false, opens the interactive GPGS login UI overlay.</param>
        public void AuthenticateUser(bool silent = false)
        {
#if USE_GPGS
            if (PlayGamesPlatform.Instance.IsAuthenticated())
            {
                isAuthenticated = true;
                return;
            }

            PlayGamesPlatform.Instance.Authenticate(silent ? SignInInteractivity.CanNotShow : SignInInteractivity.CanShow, (result) =>
            {
                if (result == SignInStatus.Success)
                {
                    isAuthenticated = true;
                    Debug.Log("GPGS: Signed in successfully!");
                }
                else
                {
                    isAuthenticated = false;
                    Debug.LogWarning($"GPGS: Sign-in failed. Status: {result}");
                }
            });
#endif
        }

        /// <summary>
        /// Submits the level completion time (in seconds) to the Google Play Leaderboard.
        /// </summary>
        public void SubmitLevelCompletionTime(float timeInSeconds)
        {
            if (string.IsNullOrEmpty(level1TimeLeaderboardId))
            {
                Debug.LogWarning("GPGS: Leaderboard ID is blank. Cannot submit score.");
                return;
            }

#if USE_GPGS
            if (!PlayGamesPlatform.Instance.IsAuthenticated())
            {
                Debug.LogWarning("GPGS: Player not logged in. Trying to authenticate.");
                AuthenticateUser(false);
                return;
            }

            // Google Play Leaderboard time format typically expects milliseconds as a long value
            long timeInMilliseconds = (long)(timeInSeconds * 1000f);

            Social.ReportScore(timeInMilliseconds, level1TimeLeaderboardId, (success) =>
            {
                if (success)
                {
                    Debug.Log($"GPGS: Successfully posted {timeInMilliseconds}ms ({timeInSeconds:F2}s) to leaderboard: {level1TimeLeaderboardId}");
                }
                else
                {
                    Debug.LogWarning("GPGS: Failed to post score to Google Play Leaderboard.");
                }
            });
#else
            Debug.Log($"GPGS: (Simulated) Submitting level completion time: {timeInSeconds:F2}s to leaderboard: {level1TimeLeaderboardId}");
#endif
        }

        /// <summary>
        /// Opens the Google Play Games built-in Leaderboards UI overlay.
        /// </summary>
        public void ShowLeaderboardUI()
        {
#if USE_GPGS
            if (PlayGamesPlatform.Instance.IsAuthenticated())
            {
                Social.ShowLeaderboardUI();
            }
            else
            {
                Debug.LogWarning("GPGS: Player not logged in. Opening login screen.");
                AuthenticateUser(false);
            }
#else
            Debug.Log("GPGS: (Simulated) Showing Leaderboards Overlay UI.");
#endif
        }
    }
}

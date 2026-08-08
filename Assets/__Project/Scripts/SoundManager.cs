using UnityEngine;
using UnityEngine.Audio;

namespace EscapeFromLava
{
    public class SoundManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Audio Mixer Groups")]
        [SerializeField] private AudioMixerGroup sfxMixerGroup;
        [SerializeField] private AudioMixerGroup musicMixerGroup;

        [Header("Background Music Settings")]
        [SerializeField] private AudioClip backgroundMusicClip;
        [SerializeField] private bool playMusicOnStart = true;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip[] grassClickClips;
        [SerializeField] private AudioClip[] lavaClickClips;
        [SerializeField] private AudioClip[] diamondClickClips;
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip loseClip;

        private void Awake()
        {
            // Auto-setup sfx source if missing
            if (sfxSource == null)
            {
                sfxSource = GetComponent<AudioSource>();
                if (sfxSource == null)
                {
                    sfxSource = gameObject.AddComponent<AudioSource>();
                }
            }
            if (sfxMixerGroup != null)
            {
                sfxSource.outputAudioMixerGroup = sfxMixerGroup;
            }

            // Auto-setup music source if missing
            if (musicSource == null)
            {
                // Create a secondary AudioSource on the same GameObject for BGM
                musicSource = gameObject.AddComponent<AudioSource>();
            }
            if (musicMixerGroup != null)
            {
                musicSource.outputAudioMixerGroup = musicMixerGroup;
            }
        }

        private void Start()
        {
            if (playMusicOnStart && backgroundMusicClip != null)
            {
                PlayBackgroundMusic(backgroundMusicClip);
            }
        }

        public void PlayBackgroundMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null) return;
            
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        public void StopBackgroundMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        public void PlayGrassClick()
        {
            PlayRandomSFX(grassClickClips);
        }

        public void PlayLavaClick()
        {
            PlayRandomSFX(lavaClickClips);
        }

        public void PlayDiamondClick()
        {
            PlayRandomSFX(diamondClickClips);
        }

        public void PlayWinSound()
        {
            PlaySFX(winClip);
        }

        public void PlayLoseSound()
        {
            PlaySFX(loseClip);
        }

        private void PlayRandomSFX(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0) return;
            int randomIndex = Random.Range(0, clips.Length);
            PlaySFX(clips[randomIndex]);
        }

        private void PlaySFX(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }
    }
}

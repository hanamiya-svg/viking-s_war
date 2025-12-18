using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("Mixer")]
    public AudioMixer audioMixer;
    public AudioMixerSnapshot gameplaySnapshot;
    public AudioMixerSnapshot endMatchSnapshot;

    [Header("Music")]
    public AudioClip musicClip;

    [Header("SFX")]
    public AudioClip appearSFX;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        // Music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f; // 2D
        musicSource.volume = 1f;

        // SFX source
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.spatialBlend = 0f; // 2D
        sfxSource.volume = 1f;
    }

    private void Start()
    {
        PlayMusic();
        GoToDefaultSnapshot();
    }

    /* =======================
       MUSIC
       ======================= */

    public void PlayMusic()
    {
        if (musicClip == null)
        {
            Debug.LogError("AudioManager: Music clip not assigned");
            return;
        }

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
            musicSource.Stop();
    }

    /* =======================
       SNAPSHOTS
       ======================= */

    public void GoToDefaultSnapshot()
    {
        TransitionSnapshots(1f, 0f, 0.1f);
    }

    public void GoToEndMatchSnapshot()
    {
        TransitionSnapshots(0f, 1f, 1f);
    }

    private void TransitionSnapshots(float gameplayWeight, float endMatchWeight, float time)
    {
        if (audioMixer == null || gameplaySnapshot == null || endMatchSnapshot == null)
            return;

        AudioMixerSnapshot[] snaps =
        {
            gameplaySnapshot,
            endMatchSnapshot
        };

        float[] weights =
        {
            gameplayWeight,
            endMatchWeight
        };

        audioMixer.TransitionToSnapshots(snaps, weights, time);
    }

    /* =======================
       SFX
       ======================= */

    public void PlayAppearSFX(Vector3 location)
    {
        PlayOneShot(appearSFX);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null)
            return;

        sfxSource.PlayOneShot(clip, 1f);
    }
}

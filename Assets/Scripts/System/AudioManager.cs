using UnityEngine;

// Universal audio playback shared by every game and every UI screen: one persistent
// AudioSource for music (looping), one single-channel AudioSource for gameplay sfx
// (hits, launches, ...) and one single-channel AudioSource for UI sfx (score ticks,
// button clicks, ...) — each starting a new clip on its own channel stops whatever
// was already playing THERE, but the two sfx channels don't fight each other, so a
// UI sound and a gameplay sound can be heard at the same time.
// Volumes are read from and written to Settings, so they persist and apply immediately.
public class AudioManager : MonoBehaviour
{
    static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("AudioManager");
                instance = go.AddComponent<AudioManager>();
            }
            return instance;
        }
    }

    AudioSource musicSource;
    AudioSource sfxSource;
    AudioSource uiSfxSource;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = Settings.Data.musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = Settings.Data.sfxVolume;

        uiSfxSource = gameObject.AddComponent<AudioSource>();
        uiSfxSource.loop = false;
        uiSfxSource.playOnAwake = false;
        uiSfxSource.volume = Settings.Data.sfxVolume;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.Stop();
        sfxSource.clip = clip;
        sfxSource.Play();
    }

    // Separate channel from PlaySfx so UI feedback (score ticks, button clicks)
    // can play at the same time as a gameplay sound without cutting it off.
    public void PlayUiSfx(AudioClip clip)
    {
        if (clip == null) return;
        uiSfxSource.Stop();
        uiSfxSource.clip = clip;
        uiSfxSource.Play();
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
        Settings.Data.musicVolume = volume;
        Settings.Save();
    }

    public void SetSfxVolume(float volume)
    {
        sfxSource.volume = volume;
        uiSfxSource.volume = volume;
        Settings.Data.sfxVolume = volume;
        Settings.Save();
    }
}

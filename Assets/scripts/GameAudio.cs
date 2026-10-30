using UnityEngine;

// Every game sound. Sits on the GameManager object; other scripts call GameAudio.Play(...).
public class GameAudio : MonoBehaviour
{
    public enum Sound { Throw, Hit, Collect, Boss, Win, Lose }

    public AudioClip throwClip;
    public AudioClip hitClip;
    public AudioClip collectClip;
    public AudioClip bossClip;
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip music;
    [Range(0f, 1f)] public float throwVolume = 0.4f;
    [Range(0f, 1f)] public float musicVolume = 0.3f;

    static GameAudio instance;

    private AudioSource sfx;
    private AudioSource musicSource;

    void Awake()
    {
        instance = this;

        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.clip = music;

        if (music != null)
            musicSource.Play();
    }

    public static void Play(Sound sound)
    {
        if (instance == null)
            return;

        switch (sound)
        {
            case Sound.Throw: instance.PlayClip(instance.throwClip, instance.throwVolume); break;
            case Sound.Hit: instance.PlayClip(instance.hitClip, 1f); break;
            case Sound.Collect: instance.PlayClip(instance.collectClip, 0.8f); break;
            case Sound.Boss: instance.PlayClip(instance.bossClip, 1f); break;
            case Sound.Win: instance.PlayClip(instance.winClip, 1f); break;
            case Sound.Lose: instance.PlayClip(instance.loseClip, 1f); break;
        }
    }

    public static void StopMusic()
    {
        if (instance != null)
            instance.musicSource.Stop();
    }

    void PlayClip(AudioClip clip, float volume)
    {
        if (clip != null)
            sfx.PlayOneShot(clip, volume);
    }
}

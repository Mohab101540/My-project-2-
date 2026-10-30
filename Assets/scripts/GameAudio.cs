using UnityEngine;

// Every game sound. Each one is an AudioSource object in the Hierarchy under "Audio";
// this script sits on the GameManager object and other scripts call GameAudio.Play(...).
public class GameAudio : MonoBehaviour
{
    public enum Sound { Throw, Hit, Collect, Boss, Win, Lose }

    public AudioSource throwSound;
    public AudioSource hitSound;
    public AudioSource collectSound;
    public AudioSource bossSound;
    public AudioSource winSound;
    public AudioSource loseSound;
    public AudioSource music; // Plays and loops by itself, set on its AudioSource.

    static GameAudio instance;

    void Awake()
    {
        instance = this;
    }

    public static void Play(Sound sound)
    {
        if (instance == null)
            return;

        switch (sound)
        {
            case Sound.Throw: PlaySource(instance.throwSound); break;
            case Sound.Hit: PlaySource(instance.hitSound); break;
            case Sound.Collect: PlaySource(instance.collectSound); break;
            case Sound.Boss: PlaySource(instance.bossSound); break;
            case Sound.Win: PlaySource(instance.winSound); break;
            case Sound.Lose: PlaySource(instance.loseSound); break;
        }
    }

    public static void StopMusic()
    {
        if (instance != null && instance.music != null)
            instance.music.Stop();
    }

    // PlayOneShot, not Play: quick repeats like throws overlap instead of cutting each other off.
    static void PlaySource(AudioSource source)
    {
        if (source != null)
            source.PlayOneShot(source.clip);
    }
}

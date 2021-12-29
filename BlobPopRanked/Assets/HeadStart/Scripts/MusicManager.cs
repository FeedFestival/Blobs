#pragma warning disable 0414 // private field assigned but not used.
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Hellmade.Sound;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static readonly string _version = "1.0.1";
    private static MusicManager _instance;
    public static MusicManager _ { get { return _instance; } }

    void Awake()
    {
        DontDestroyOnLoad(this);
        _instance = this;
    }

    private Dictionary<string, MAudio> Sounds;
    public AudioClip MainMenuMusic;
    public AudioClip ClickSound;
    [Header("GameMusic")]
    public AudioClip GameMusic1;
    [Header("Gameplay Sounds")]
    public AudioClip BlobHit;
    public AudioClip PreExplosion;
    public AudioClip Explosion;
    public AudioClip MoneyTravel;
    public AudioClip MoneyPump;

    public void Init()
    {
        Sounds = new Dictionary<string, MAudio>()
        {
            { "MainMenuMusic", new MAudio() { AudioClip = MainMenuMusic } },
            { "Click", new MAudio() { AudioClip = ClickSound } },
            { "GameMusic1", new MAudio() { AudioClip = GameMusic1 } },
            { "BlobHit", new MAudio() { AudioClip = BlobHit } },
            { "Pre_Explosion", new MAudio() { AudioClip = PreExplosion } },
            { "Explosion", new MAudio() { AudioClip = Explosion } },
            { "MoneyTravel", new MAudio() { AudioClip = MoneyTravel } },
            { "MoneyPump", new MAudio() { AudioClip = MoneyPump } }
        };
    }

    private int? _backgroundMusicId;
    private string _backgroundMusic;
    private int? _ambientMusicId;
    private string _ambientMusic;

    private int? _soundId;
    private string _sound;

    public void PlayBackgroundMusic(MusicOpts opts)
    {
        Audio audio;
        if (_backgroundMusicId.HasValue)
        {
            audio = EazySoundManager.GetAudio(_backgroundMusicId.Value);
            if (audio == null)
            {
                Debug.LogError("audio has a problem");
                return;
            }
            audio.Stop();
        }
        _backgroundMusicId = EazySoundManager
            .PrepareMusic(Sounds[opts.MusicName].AudioClip, opts.Volume, opts.Loop, false, opts.FadeInSeconds, opts.FadeOutSeconds);
        _backgroundMusic = opts.MusicName;
        audio = EazySoundManager.GetAudio(_backgroundMusicId.Value);
        audio.Play();
        if (opts.Time.HasValue)
        {
            audio.AudioSource.time = opts.Time.Value;
        }
    }

    public void PlayAmbient(string musicName, bool loop = true)
    {
        ClearAmbientQueue();
        _ambientMusic = musicName;
        if (Sounds[_ambientMusic].AudioClip != null)
        {
            PlayAmbientAudio(Sounds[musicName], loop: loop);
        }
        else
        {
            StartCoroutine(PlayAllAmbientAudio(Sounds[musicName], loop));
        }
    }

    private void ClearAmbientQueue()
    {
        if (_ambientMusicId.HasValue)
        {
            Audio audio = null;
            audio = EazySoundManager.GetAudio(_ambientMusicId.Value);
            if (audio == null)
            {
                Debug.LogError("audio has a problem");
                return;
            }
            audio.Stop();
            audio = null;
        }
    }

    private Audio PlayAmbientAudio(MAudio mAudio, float volume = 0.2f, bool loop = false, bool loadMany = false)
    {
        ClearAmbientQueue();
        _ambientMusicId = EazySoundManager.PrepareMusic(
            loadMany ? mAudio.AudioClips[mAudio.GetRandomIndex()] : mAudio.AudioClip,
            volume,
            loop,
            false
            );
        Audio audio = EazySoundManager.GetAudio(_ambientMusicId.Value);
        audio.Play();
        return audio;
    }

    private IEnumerator PlayAllAmbientAudio(MAudio mAudio, bool loop)
    {
        var audio = PlayAmbientAudio(mAudio, loop: loop, loadMany: true);
        var time = audio.AudioSource.clip.length;

        yield return new WaitForSeconds(time);

        StartCoroutine(PlayAllAmbientAudio(mAudio, loop));
    }

    public void PlayRequiredBackgroundMusic(string musicName, bool loop = true, float? time = null)
    {
        if (_backgroundMusic == musicName)
        {
            return;
        }

        MusicOpts opts = new MusicOpts(musicName, loop: loop, time: time);
        PlayBackgroundMusic(opts);
    }

    public void PlayRequiredAmbient(string musicName, bool loop = false)
    {
        if (_ambientMusic == musicName)
        {
            return;
        }

        PlayAmbient(musicName, loop);
    }

    public void PlaySound(string soundName, bool loop = false)
    {
        _sound = soundName;
        if (Sounds[_sound].AudioClip != null)
        {
            PlayAmbientAudio(Sounds[soundName], loop: loop);
        }
    }

    public void PlaySound(MusicOpts opts)
    {
        _sound = opts.MusicName;
        if (Sounds[_sound].AudioClip != null)
        {
            PlayAmbientAudio(Sounds[_sound], volume: opts.Volume, loop: opts.Loop);
        }
    }

    public void PlaySFX(MusicOpts opts)
    {
        _sound = opts.MusicName;
        if (Sounds[_sound].AudioClip != null)
        {
            PlaySoundAudio(Sounds[_sound], volume: opts.Volume, loop: opts.Loop);
        }
    }

    private void PlaySoundAudio(MAudio mAudio, float volume = 0.2f, bool loop = false)
    {
        _soundId = EazySoundManager.PrepareMusic(
            mAudio.AudioClip,
            volume,
            loop,
            false
            );
        Audio audio = EazySoundManager.GetAudio(_soundId.Value);
        audio.Play();
    }
}

public class MAudio
{
    public AudioClip AudioClip;
    public AudioClip[] AudioClips;

    public int GetRandomIndex()
    {
        if (AudioClips != null && AudioClips.Length > 0)
        {
            return (int)Mathf.Ceil(Random.Range(0, AudioClips.Length));
        }
        return 0;
    }
}

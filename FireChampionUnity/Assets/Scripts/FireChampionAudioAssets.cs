using UnityEngine;

public enum FireChampionAudioCue
{
    Ultimate,
    Skill,
    Serve,
    NormalHit,
    SmashHit,
    ScoreLeft,
    ScoreRight
}

public sealed class FireChampionAudioAssets
{
    public static readonly string[] RequiredResourcePaths =
    {
        "FireChampion/Audio/sfx_ultimate",
        "FireChampion/Audio/sfx_skill",
        "FireChampion/Audio/sfx_serve",
        "FireChampion/Audio/sfx_hit_normal",
        "FireChampion/Audio/sfx_hit_smash",
        "FireChampion/Audio/sfx_score_left",
        "FireChampion/Audio/sfx_score_right"
    };

    private readonly AudioClip[] clips;

    private FireChampionAudioAssets(AudioClip[] clips)
    {
        this.clips = clips;
    }

    public static FireChampionAudioAssets Load()
    {
        AudioClip[] loaded = new AudioClip[RequiredResourcePaths.Length];
        for (int i = 0; i < RequiredResourcePaths.Length; i++)
        {
            loaded[i] = Resources.Load<AudioClip>(RequiredResourcePaths[i]);
        }

        return new FireChampionAudioAssets(loaded);
    }

    public bool Play(AudioSource source, FireChampionAudioCue cue, float volume)
    {
        if (source == null || volume <= 0.01f)
        {
            return false;
        }

        AudioClip clip = Clip(cue);
        if (clip == null)
        {
            return false;
        }

        source.PlayOneShot(clip, Mathf.Clamp01(volume));
        return true;
    }

    public AudioClip Clip(FireChampionAudioCue cue)
    {
        int index = (int)cue;
        if (clips == null || index < 0 || index >= clips.Length)
        {
            return null;
        }

        return clips[index];
    }
}

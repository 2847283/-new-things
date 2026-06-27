using System.Collections.Generic;
using UnityEngine;

public sealed class ToneAudioPlayer
{
    private const int SampleRate = 22050;
    private const float BaseAmplitude = 0.18f;

    private readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

    public void Play(AudioSource source, float frequency, float duration, float volume)
    {
        if (source == null || volume <= 0.01f)
        {
            return;
        }

        AudioClip clip = GetOrCreateClip(frequency, duration);
        source.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    private AudioClip GetOrCreateClip(float frequency, float duration)
    {
        int hz = Mathf.RoundToInt(frequency);
        int milliseconds = Mathf.RoundToInt(duration * 1000.0f);
        string key = hz + "hz_" + milliseconds + "ms";

        AudioClip clip;
        if (clipCache.TryGetValue(key, out clip) && clip != null)
        {
            return clip;
        }

        int count = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
        float[] data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float fade = 1.0f - i / (float)count;
            data[i] = Mathf.Sin(2.0f * Mathf.PI * frequency * i / SampleRate) * BaseAmplitude * fade;
        }

        clip = AudioClip.Create(key, count, 1, SampleRate, false);
        clip.SetData(data, 0);
        clipCache[key] = clip;
        return clip;
    }
}

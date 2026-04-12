using UnityEngine;

[DisallowMultipleComponent]
public class VoiceEffectNoisePlayer : MonoBehaviour
{
    [SerializeField] private AudioSource noiseSource;

    public void PlayNoise(AudioClip clip, float volume, bool loop, float pitch = 1f)
    {
        if (noiseSource == null || clip == null)
            return;

        if (noiseSource.clip != clip)
            noiseSource.clip = clip;

        noiseSource.loop = loop;
        noiseSource.volume = Mathf.Clamp01(volume);
        noiseSource.pitch = Mathf.Clamp(pitch, 0.5f, 2f);

        if (!noiseSource.isPlaying)
            noiseSource.Play();
    }

    public void StopNoise()
    {
        if (noiseSource == null)
            return;

        noiseSource.Stop();
        noiseSource.clip = null;
        noiseSource.pitch = 1f;
    }

    public void SetNoiseVolume(float volume)
    {
        if (noiseSource == null)
            return;

        noiseSource.volume = Mathf.Clamp01(volume);
    }

    public void SetNoisePitch(float pitch)
    {
        if (noiseSource == null)
            return;

        noiseSource.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
    }
}
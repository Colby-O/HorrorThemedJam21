using UnityEngine;
using System.Collections;

namespace HTJ21
{
    public static class AudioHelper
    {
        public static void FadeOut(AudioSource audioSource, float startVolume, float targetVolume, float t)
        {
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            if (targetVolume < 0.01f && t > 0.99f && audioSource.isPlaying) audioSource.Stop();
        }

        public static void FadeIn(AudioSource audioSource, float startVolume, float targetVolume, float t)
        {
            if (!audioSource.isPlaying) audioSource.Play();
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t); ;
        }
    }
}

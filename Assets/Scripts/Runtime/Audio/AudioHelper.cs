using UnityEngine;
using System.Collections;

namespace HTJ21
{
    public static class AudioHelper
    {
        public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
        {
            float startVolume = audioSource.volume;

            while (audioSource.volume > 0)
            {
                audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

                yield return null;
            }

            audioSource.Stop();
            audioSource.volume = startVolume;
        }

        public static IEnumerator FadeIn(AudioSource audioSource, float targetVolume, float FadeTime)
        {
            audioSource.Play();

            audioSource.volume = 0f;
            while (audioSource.volume < targetVolume)
            {
                audioSource.volume += targetVolume * Time.deltaTime / FadeTime;

                yield return null;
            }

            audioSource.volume = targetVolume;
        }
    }
}

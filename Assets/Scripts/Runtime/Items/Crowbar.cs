using PlazmaGames.Audio;
using PlazmaGames.Core;
using UnityEngine;
using AudioType = PlazmaGames.Audio.AudioType;

namespace HTJ21
{
    public class Crowbar : MonoBehaviour
    {
        public void Restart()
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Board"))
            {
                if (other.gameObject.TryGetComponent(out Rigidbody rig))
                {
                    if (rig.isKinematic)
                    {
                        rig.isKinematic = false;
                        GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(HTJ21GameManager.Preferences.BoardPopSound, AudioType.Sfx, false, true);
                    }
                }
            }
        }
    }
}

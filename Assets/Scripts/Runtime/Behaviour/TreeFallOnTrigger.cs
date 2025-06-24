using System;
using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    public class TreeFallOnTrigger : MonoBehaviour
    {
        
        private bool _triggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!(other.CompareTag("Player") || other.CompareTag("Car"))) return;
            if (_triggered) return;
            _triggered = true;
            transform.parent.GetComponent<TreeFall>().Fall();
            GameManager.GetMonoSystem<IWeatherMonoSystem>().SpawnLightingAt(transform.parent.position);
        }

    }
}

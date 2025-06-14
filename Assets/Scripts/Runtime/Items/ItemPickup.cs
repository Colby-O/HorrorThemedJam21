using PlazmaGames.Attribute;
using PlazmaGames.Core;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    public class ItemPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private PickupableItem _type;

        [Header("Outline")]
        [SerializeField] private MeshRenderer _outlineMR;

        [SerializeField, ReadOnly] private bool _hasOutline = false;

        public void AddOutline()
        {
            if (!_outlineMR) return;

            _hasOutline = true;
            Material[] mats = _outlineMR.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].SetInt("Boolean_8BBF99CD", 0);
            }
            _outlineMR.materials = mats;
        }


        public void EndInteraction()
        {

        }

        public string GetHint()
        {
            return "Click 'E' to Pickup Flashlight";
        }

        public bool Interact(Interactor interactor)
        {
            if (HTJ21GameManager.PickupManager)
            {
                HTJ21GameManager.PickupManager.Pickup(_type);
                if (_type == PickupableItem.FlashLight)
                {
                    HTJ21GameManager.PlayerTutorial.ShowTutorial(0);
                    GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(HTJ21GameManager.Preferences.PickupFlashlightDialogue);
                }
            }
            Destroy(gameObject);
            return true;
        }

        public bool IsInteractable()
        {

            return true;
        }

        public void OnPickup(Interactor interactor)
        {

        }

        public void RemoveOutline()
        {
            if (!_outlineMR) return;

            _hasOutline = false;
            Material[] mats = _outlineMR.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].SetInt("Boolean_8BBF99CD", 1);
            }
            _outlineMR.materials = mats;
        }
    }
}

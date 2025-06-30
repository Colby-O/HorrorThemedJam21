using PlazmaGames.Attribute;
using PlazmaGames.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HTJ21
{
    public class ItemPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private PickupableItem _type;

        [Header("Dialogue")]
        [SerializeField] private DialogueSO _onPickupDialogue;

        [Header("Outline")]
        [SerializeField] private MeshRenderer _outlineMR;
        [SerializeField, ReadOnly] private bool _hasOutline = false;

        public UnityEvent OnPickupCallback = new UnityEvent();

        public void Restart()
        {
            gameObject.SetActive(true);
        }

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

        public string GetName()
        {
            if (_type == PickupableItem.FlashLight) return "FlashLight";
            else if (_type == PickupableItem.BathroomKey) return "Bathroom Key";
            else if (_type == PickupableItem.BathroomSupplies) return "Bathroom Supplies";
            else return string.Empty;
        }

        public string GetHint()
        {
            return $"Click 'E' to Pickup {GetName()}";
        }

        public bool Interact(Interactor interactor)
        {
            if (_onPickupDialogue) GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_onPickupDialogue);

            if (HTJ21GameManager.PickupManager)
            {
                HTJ21GameManager.PickupManager.Pickup(_type);
                if (_type == PickupableItem.FlashLight)
                {
                    HTJ21GameManager.PlayerTutorial.ShowTutorial(0);
                }
            }
            OnPickupCallback?.Invoke();
            gameObject.SetActive(false);
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

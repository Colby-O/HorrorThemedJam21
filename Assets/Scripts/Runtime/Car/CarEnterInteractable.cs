using PlazmaGames.Attribute;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    public class CarEnterInteractable : MonoBehaviour, IInteractable
    {
        [Header("Outline")]
        [SerializeField] private MeshRenderer _outlineMR;
        [SerializeField, ReadOnly] private bool _hasOutline = false;

        public void AddOutline()
        {
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
            return "Click 'E' To Enter";
        }

        public bool Interact(Interactor interactor)
        {
            if (HTJ21GameManager.Player.IsInCar() || !HTJ21GameManager.Car || HTJ21GameManager.Car.IsLocked()) return true;
            HTJ21GameManager.Player.EnterCar();
            return true;
        }

        public bool IsInteractable()
        {
            return true;
        }

        public void RemoveOutline()
        {
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

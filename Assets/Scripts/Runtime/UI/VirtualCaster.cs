using PlazmaGames.Attribute;
using PlazmaGames.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HTJ21
{
    public class VirtualCaster : GraphicRaycaster
    {
        [SerializeField, ReadOnly] private Camera _screenCamera;

        [SerializeField, ReadOnly] private List<IClickable> _lastClickables = new List<IClickable>();

        public void CheckForHit(Vector3 pos)
        {
            Ray ray = _screenCamera.ScreenPointToRay(pos);
            RaycastHit hit;

            List<IClickable> found = new List<IClickable>();

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.transform != null)
                {
                    if (hit.collider.TryGetComponent<IClickable>(out IClickable clickable))
                    {
                        if (clickable.CanClick)
                        {
                            found.Add(clickable);
                            if (Mouse.current.leftButton.wasPressedThisFrame) clickable.OnClick?.Invoke();
                            else clickable.OnHoverEnter?.Invoke();
                        }
                    }
                }
            }

            for (int i = _lastClickables.Count - 1; i >= 0; i--) 
            { 
                if (!found.Contains(_lastClickables[i])) _lastClickables[i].OnHoverExit?.Invoke();
            }

            _lastClickables = found;
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            _screenCamera = HTJ21GameManager.GetActiveCamera();
            if (_screenCamera == null) return;
            Ray ray = eventCamera.ScreenPointToRay(eventData.position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.transform == transform)
                {
                    Vector3 virtualPos = new Vector3(hit.textureCoord.x, hit.textureCoord.y);
                    virtualPos.x *= _screenCamera.targetTexture.width;
                    virtualPos.y *= _screenCamera.targetTexture.height;

                    CheckForHit(virtualPos);
                }
            }
        }
    }
}

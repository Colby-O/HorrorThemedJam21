using PlazmaGames.Attribute;
using PlazmaGames.Core;
using PlazmaGames.Core.Utils;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace HTJ21
{
    [RequireComponent(typeof(PlayerController))]
    public class Inspector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _offset;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private Transform _head;

        [Header("settings")]
        [SerializeField] private float _rotationSpeed = 1f;
        [SerializeField] private float _moveRate = 0.1f;
        [SerializeField] private float _headMoveRate = 0.1f;

        [Header("Flags")]
        [SerializeField, ReadOnly] private bool _isInspecting;
        [SerializeField, ReadOnly] private bool _isMovingBack;
        [SerializeField, ReadOnly] private bool _staredInspectThisFrame;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private InspectType _currentInsectType;
        [SerializeField, ReadOnly] private Transform _inspectingTarget;
        [SerializeField, ReadOnly] private Transform _inspectingObject;
        [SerializeField, ReadOnly] private Transform _objectOffset;

        private Dictionary<Transform, Vector3> _origPositions;
        private Dictionary<Transform, Quaternion> _origRotations;

        public bool IsInspecting() => _isInspecting;

        private void DisablePlayer()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            _playerController.LockMovement = true;
        }

        private void EnablePlayer()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _playerController.LockMovement = false;
        }

        public void StartInspect(Transform obj, InspectType inspectType, Transform offset)
        {
            if (obj == null || _isInspecting || _isMovingBack) return;

            _isInspecting = true;
            _staredInspectThisFrame = true;
            _inspectingObject = obj;
            _inspectingTarget = (inspectType == InspectType.Goto) ? _head : obj;
            _objectOffset = offset;
            _currentInsectType = inspectType;

            if (_currentInsectType != InspectType.Moveable) DisablePlayer();

            _origPositions[_inspectingTarget] = _inspectingTarget.position;
            _origRotations[_inspectingTarget] = _inspectingTarget.rotation;

            if (_inspectingTarget.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = true;
            }

            _inspectingTarget.GetComponents<Collider>().ForEach(c => c.enabled = false);
        }

        public void EndInspect()
        {
            if (!_isInspecting) return;
            _isInspecting = false;
            _isMovingBack = true;
            if (_currentInsectType != InspectType.Goto) EnablePlayer();
        }

        private void Inspect()
        {
            if (!_inspectingTarget) return;

            if (_currentInsectType == InspectType.ComeTo)
            {
                _inspectingTarget.position = Vector3.Lerp(_inspectingTarget.position, _offset.transform.position, _moveRate);

                Vector2 deltaMouse = GameManager.GetMonoSystem<IInputMonoSystem>().RawLook;
                _inspectingTarget.Rotate(deltaMouse.x * _rotationSpeed * Vector3.up, Space.World);
                _inspectingTarget.Rotate(deltaMouse.y * _rotationSpeed * Vector3.left, Space.World);
            }
            else if (_currentInsectType == InspectType.Moveable)
            {
                float dst = Vector3.Distance(transform.position, _offset.transform.position);
                Vector3 dir = (_offset.transform.position - transform.position).normalized;
                RaycastHit[] hits = Physics.RaycastAll(transform.position, dir, dst);
                if (hits.Any(hit => hit.transform != _inspectingTarget && hit.transform != transform))
                {
                    RaycastHit hit = hits.First(hit => hit.transform != _inspectingTarget && hit.transform != transform);
                    _inspectingTarget.position = hit.point - dir * 0.1f;
                }
                else
                {
                    _inspectingTarget.position = _offset.transform.position;
                }
                
            }
            else if (_currentInsectType == InspectType.Goto)
            {
                _head.transform.rotation = Quaternion.LookRotation((_inspectingObject.transform.position - _objectOffset.transform.position).normalized);
                _head.transform.position = Vector3.Lerp(_head.transform.position, _objectOffset.transform.position, _moveRate);
            }
        }

        private void CancelInspect()
        {
            if (!_inspectingTarget) return;

            if (_currentInsectType == InspectType.Moveable)
            {
                if (_inspectingTarget.TryGetComponent(out Rigidbody rb))
                {
                    rb.isKinematic = false;
                }
                _inspectingTarget.GetComponents<Collider>().ForEach(c => c.enabled = true);
                _isMovingBack = false;
            }
            else
            {
                float moveRate = (_currentInsectType == InspectType.ComeTo) ? _moveRate : _headMoveRate;

                if (_origPositions.ContainsKey(_inspectingTarget)) _inspectingTarget.position = Vector3.Lerp(_inspectingTarget.position, _origPositions[_inspectingTarget], moveRate);
                if (_origRotations.ContainsKey(_inspectingTarget)) _inspectingTarget.rotation = Quaternion.Lerp(_inspectingTarget.rotation, _origRotations[_inspectingTarget], moveRate);

                if ((_inspectingTarget.position - _origPositions[_inspectingTarget]).magnitude < 0.01)
                {
                    _inspectingTarget.position = _origPositions[_inspectingTarget];
                    _inspectingTarget.rotation = _origRotations[_inspectingTarget];

                    if (_inspectingTarget.TryGetComponent(out Rigidbody rb))
                    {
                        rb.isKinematic = false;
                    }

                    _inspectingTarget.GetComponents<Collider>().ForEach(c => c.enabled = true);

                    if (_currentInsectType == InspectType.Goto) EnablePlayer();
                    _isMovingBack = false;
                }
            }
        }

        private void OnEnable()
        {
            GameManager.GetMonoSystem<IInputMonoSystem>().InteractionCallback.AddListener(EndInspect);
        }

        private void OnDisable()
        {
            GameManager.GetMonoSystem<IInputMonoSystem>().InteractionCallback.RemoveListener(EndInspect);
        }

        private void Awake()
        {
            if (!_playerController) _playerController = GetComponent<PlayerController>();
            _origPositions = new Dictionary<Transform, Vector3>();
            _origRotations = new Dictionary<Transform, Quaternion>();
        }

        private void OnDestroy()
        {
            GameManager.GetMonoSystem<IInputMonoSystem>().InteractionCallback.RemoveListener(EndInspect);
        }

        private void Update()
        {
            if (HTJ21GameManager.IsPaused) return;

            if(_isInspecting) Inspect();
            if (_isMovingBack && !_isInspecting) CancelInspect();
        }

        private void LateUpdate()
        {
            _staredInspectThisFrame = false;
        }
    }
}

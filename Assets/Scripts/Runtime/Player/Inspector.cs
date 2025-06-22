using PlazmaGames.Attribute;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.Core.Utils;
using PlazmaGames.UI;
using System.Collections.Generic;
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

        [Header("Settings")]
        [SerializeField] private float _rotationSpeed = 1f;
        [SerializeField] private float _moveRate = 0.1f;
        [SerializeField] private float _headMoveRate = 0.1f;

        [Header("Audio")]
        [SerializeField] private AudioClip _pickup;
        [SerializeField] private AudioClip _drop;

        [Header("Flags")]
        [SerializeField, ReadOnly] private bool _isInspecting;
        [SerializeField, ReadOnly] private bool _isMovingBack;
        [SerializeField, ReadOnly] private bool _staredInspectThisFrame;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private InspectType _currentInsectType;
        [SerializeField, ReadOnly] private bool _rotateWithPlayer;
        [SerializeField, ReadOnly] private Quaternion _startRotation;
        [SerializeField, ReadOnly] private Transform _inspectingTarget;
        [SerializeField, ReadOnly] private Transform _inspectingObject;
        [SerializeField, ReadOnly] private Transform _objectOffset;
        [SerializeField, ReadOnly] private string _currentText;
        [SerializeField, ReadOnly] private bool _isReading;
        [SerializeField, ReadOnly] private Transform _currentTargetOverride;

        [SerializeField, ReadOnly] private float _yaw = 0f;
        [SerializeField, ReadOnly] private float _pitch = 0f;

        [SerializeField, ReadOnly] private float _currentComeToOffsetOverride;

        private Dictionary<Transform, Vector3> _origPositions;
        private Dictionary<Transform, Quaternion> _origRotations;

        public bool IsInspecting() => _isInspecting || _isMovingBack;

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

        public void StartInspect(Transform obj, InspectType inspectType, bool rotateWithPlayer, Transform offset, Transform targetOverride = null, string text = "", float comeToOffsetOverride = 0f)
        {
            if (obj == null || _isInspecting || _isMovingBack) return;

            _isInspecting = true;
            _isReading = false;
            _staredInspectThisFrame = true;
            _inspectingObject = obj;
            _rotateWithPlayer = rotateWithPlayer;
            _startRotation = Quaternion.Inverse(transform.rotation) * obj.rotation;
            _inspectingTarget = (inspectType == InspectType.Goto || inspectType == InspectType.ReadableGoTo) ? _head : obj;
            _objectOffset = offset;
            _currentInsectType = inspectType;
            _currentText = text;
            _currentTargetOverride = targetOverride;
            _currentComeToOffsetOverride = comeToOffsetOverride;

            if (_currentInsectType != InspectType.Goto && _currentInsectType != InspectType.ReadableGoTo) GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_pickup, PlazmaGames.Audio.AudioType.Sfx, false, true);

            if (_currentInsectType != InspectType.Moveable) DisablePlayer();

            _origPositions[_inspectingTarget] = _inspectingTarget.position;
            _origRotations[_inspectingTarget] = _inspectingTarget.rotation;

            if (_inspectingTarget.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = true;
            }

            _inspectingTarget.GetComponents<Collider>().ForEach(c =>
            {
                if (!c.isTrigger) c.enabled = false;
            });
        }

        public void EndInspect()
        {
            if (!_isInspecting || _staredInspectThisFrame) return;
            if (_isReading) ToggleRead();
            if (_currentInsectType != InspectType.Goto && _currentInsectType != InspectType.ReadableGoTo) GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_drop, PlazmaGames.Audio.AudioType.Sfx, false, true);
            _isInspecting = false;
            _isMovingBack = true;
            _currentTargetOverride = null;
            _currentText = string.Empty;
            if (_currentInsectType != InspectType.Goto && _currentInsectType != InspectType.ReadableGoTo) EnablePlayer();
        }

        private void Inspect()
        {
            if (!_inspectingTarget) return;

            if (_currentInsectType == InspectType.ComeTo || _currentInsectType == InspectType.ReadableComeTo)
            {
                if (_isReading && _currentInsectType == InspectType.ReadableComeTo) return;

                Vector3 offsetPosLoc = _offset.transform.localPosition;
                offsetPosLoc.z += _currentComeToOffsetOverride;
                Vector3 offsetPos = _offset.transform.TransformPoint(offsetPosLoc);

                _inspectingTarget.position = Vector3.Lerp(_inspectingTarget.position, offsetPos, _moveRate * UnityEngine.Time.deltaTime);

                Vector2 deltaMouse = GameManager.GetMonoSystem<IInputMonoSystem>().RawLook;

                _yaw += deltaMouse.x * _rotationSpeed * UnityEngine.Time.deltaTime;
                _pitch -= deltaMouse.y * _rotationSpeed * UnityEngine.Time.deltaTime;

                Quaternion targetRotation = Quaternion.Euler(_pitch, _yaw, 0);
                _inspectingTarget.rotation = targetRotation;
            }
            else if (_currentInsectType == InspectType.Moveable)
            {
                if (_rotateWithPlayer)
                {
                    _inspectingObject.rotation = transform.rotation * _startRotation;
                }
                
                Vector3 offsetPosLoc = _offset.transform.localPosition;
                offsetPosLoc.z += _currentComeToOffsetOverride;
                Vector3 offsetPos = _offset.transform.TransformPoint(offsetPosLoc);

                float dst = Vector3.Distance(_head.position, offsetPos);
                Vector3 dir = (offsetPos - _head.position).normalized;
                RaycastHit[] hits = Physics.RaycastAll(_head.position, dir, dst);
                if (hits.Any(hit => hit.transform != _inspectingTarget && hit.transform != transform))
                {
                    RaycastHit hit = hits.First(hit => hit.transform != _inspectingTarget && hit.transform != transform);
                    _inspectingTarget.position = hit.point - dir * 0.1f;
                }
                else
                {
                    _inspectingTarget.position = offsetPos;
                }
            }
            else if (_currentInsectType == InspectType.Goto || _currentInsectType == InspectType.ReadableGoTo)
            {
                _head.transform.rotation = Quaternion.LookRotation(((_currentTargetOverride ? _currentTargetOverride.position : _inspectingObject.transform.position) - _objectOffset.transform.position).normalized);
                _head.transform.position = Vector3.Lerp(_head.transform.position, _objectOffset.transform.position, _moveRate * UnityEngine.Time.deltaTime);
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
                float moveRate = (_currentInsectType == InspectType.ComeTo || _currentInsectType == InspectType.ReadableComeTo) ? _moveRate : _headMoveRate;

                if (_origPositions.ContainsKey(_inspectingTarget)) _inspectingTarget.position = Vector3.Lerp(_inspectingTarget.position, _origPositions[_inspectingTarget], moveRate * UnityEngine.Time.deltaTime);
                if (_origRotations.ContainsKey(_inspectingTarget)) _inspectingTarget.rotation = Quaternion.Lerp(_inspectingTarget.rotation, _origRotations[_inspectingTarget], moveRate * UnityEngine.Time.deltaTime);

                if ((_inspectingTarget.position - _origPositions[_inspectingTarget]).magnitude < 0.01)
                {
                    _inspectingTarget.position = _origPositions[_inspectingTarget];
                    _inspectingTarget.rotation = _origRotations[_inspectingTarget];

                    if (_inspectingTarget.TryGetComponent(out Rigidbody rb))
                    {
                        rb.isKinematic = false;
                    }

                    _inspectingTarget.GetComponents<Collider>().ForEach(c => c.enabled = true);

                    if (_currentInsectType == InspectType.Goto || _currentInsectType == InspectType.ReadableGoTo) EnablePlayer();
                    _isMovingBack = false;
                }
            }
        }

        private void ToggleRead()
        {
            if (_isMovingBack || !_isInspecting || (_currentInsectType != InspectType.ReadableComeTo && _currentInsectType != InspectType.ReadableGoTo)) return;

            _isReading = !_isReading;

            if (_isReading)
            {
                GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().ShowText(_currentText);
            }
            else
            {
                GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().HideText();
            }
        }

        private void OnEnable()
        {
            GameManager.GetMonoSystem<IInputMonoSystem>().InteractionCallback.AddListener(EndInspect);
            GameManager.GetMonoSystem<IInputMonoSystem>().RCallback.AddListener(ToggleRead);
        }

        private void OnDisable()
        {
            GameManager.GetMonoSystem<IInputMonoSystem>().InteractionCallback.RemoveListener(EndInspect);
            GameManager.GetMonoSystem<IInputMonoSystem>().RCallback.RemoveListener(ToggleRead);
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
            GameManager.GetMonoSystem<IInputMonoSystem>().RCallback.RemoveListener(ToggleRead);

        }

        private void Update()
        {
            if (HTJ21GameManager.IsPaused) return;

            if(_isInspecting) Inspect();
            if (_isMovingBack && !_isInspecting) CancelInspect();
            if (_isInspecting && (_currentInsectType == InspectType.ReadableComeTo || _currentInsectType == InspectType.ReadableGoTo) && !_isReading && !_isMovingBack) GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().SetHint("Click 'R' to read");
        }

        private void LateUpdate()
        {
            _staredInspectThisFrame = false;
        }
    }
}

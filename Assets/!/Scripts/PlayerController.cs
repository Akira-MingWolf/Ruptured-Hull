using System;
using UnityEngine;
using UnityEngine.Events;

namespace rupturedHull
{
    public class PlayerController : MonoBehaviour
    {

        #region Parameter

        [Header("Movement Parameters")]
        [SerializeField] private float _maxSpeed => _sprintInput ? _sprintSpeed : _walkSpeed;
        [SerializeField] private float _acceleration = 20f;
        [SerializeField] private float _walkSpeed = 4.5f;
        [SerializeField] private float _sprintSpeed = 8f;
        public bool _sprinting
        {
            get
            {
                return _sprintInput && _currentSpeed > 0.1f;
            }
        }
        [Space(15)]
        [Tooltip("This control the jump height of player")]
        [SerializeField] private float _jumpHeight = 2f;
        [SerializeField] private int _jumpCount = 0;
        [SerializeField] private bool _canDoubleJump = true;

        [Header("Look Parameters")]
        public Vector2 _mouseSensetivity = new Vector2(0.1f, 0.1f);
        public float _pitchLimit = 85f;
        [SerializeField] private float _currentPitch = 0f;
        public float _CurrentPitch
        {
            get => _currentPitch;

            set
            {
                _currentPitch = Mathf.Clamp(value, -_pitchLimit, _pitchLimit);
            }
        }

        [Header("Camera Parameters")]
        [SerializeField] private float _cameraNormalFov = 60f;
        [SerializeField] private float _cameraSprintFov = 80f;
        [SerializeField] private float _cameraFovSmoothing = 3f;
        float _targetCameraFov
        {
            get
            {
                return _sprinting ? _cameraSprintFov : _cameraNormalFov;
            }
        }

        [Header("Physic Parameters")]
        [SerializeField] private float _gravityScale = 3f;
        public float _verticalVelocity = 0f;
        public Vector3 _currentVelocity { get; private set; }
        public float _currentSpeed { get; private set; }
        public bool _isGrounded => _characterController.isGrounded;
        [SerializeField] private bool _wasGrounded = false;

        [Header("Input")]
        public Vector2 _moveInput;
        public Vector2 _lookInput;
        public bool _sprintInput;

        [Header("Component")]
        [SerializeField] private Camera _camera;
        [SerializeField] private CharacterController _characterController;

        [Header("Events")]
        public UnityEvent _landed;

        #endregion
        #region Update

        private void OnValidate()
        {
            if (_camera == null)
            {
                _camera = GetComponentInChildren<Camera>();
            }
            if (_characterController == null)
            {
                _characterController = GetComponentInChildren<CharacterController>();
            }
        }

        private void Update()
        {
            MoveUpdate();
            LookUpdate();
            CameraUpdate();

            if (!_wasGrounded && _isGrounded)
            {
                _jumpCount = 0;
                _landed?.Invoke();
            }
            _wasGrounded = _isGrounded;
        }

        #endregion
        #region method

        private void MoveUpdate()
        {
            Vector3 motion = transform.forward * _moveInput.y + transform.right * _moveInput.x;
            motion.y = 0f;
            motion.Normalize();

            if (motion.sqrMagnitude >= 0.01f)
            {
                _currentVelocity = Vector3.MoveTowards(_currentVelocity, motion * _maxSpeed, _acceleration * Time.deltaTime);
            }
            else
            {
                _currentVelocity = Vector3.MoveTowards(_currentVelocity, Vector3.zero, _acceleration * Time.deltaTime);
            }

            if (_isGrounded && _verticalVelocity <= 0.01f)
            {
                _verticalVelocity = -3f;
            }
            else
            {
                _verticalVelocity += Physics.gravity.y * _gravityScale * Time.deltaTime;
            }


            Vector3 _fullVelocity = new Vector3(_currentVelocity.x, _verticalVelocity, _currentVelocity.z);

            CollisionFlags flags = _characterController.Move(_fullVelocity * Time.deltaTime);

            if ((flags & CollisionFlags.Above) != 0 && _verticalVelocity > 0.01f)
            {
                _verticalVelocity = 0f;
            }

            //updating speed
            _currentSpeed = _currentVelocity.magnitude;
        }

        private void LookUpdate()
        {
            Vector2 input = new Vector2(_lookInput.x * _mouseSensetivity.x, _lookInput.y * _mouseSensetivity.y);
            //look up and down
            _CurrentPitch -= input.y;
            _camera.transform.localRotation = Quaternion.Euler(_CurrentPitch, 0f, 0f);
            //look left and right
            transform.Rotate(Vector3.up * input.x);
        }

        private void CameraUpdate()
        {
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _targetCameraFov, _cameraFovSmoothing * Time.deltaTime);
        }

        public void TryJump()
        {
            if (!_isGrounded)
            {
                if (!_canDoubleJump || _jumpCount >= 2)
                {
                    return;
                }
            }
            _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * Physics.gravity.y * _gravityScale);
            _jumpCount++;
        }

        #endregion
    }
}

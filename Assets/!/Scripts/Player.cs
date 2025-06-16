using UnityEngine;
using UnityEngine.InputSystem;

namespace rupturedHull
{
    public class Player : MonoBehaviour
    {

        #region Parameter

        [Header("Component")]
        [SerializeField] private PlayerController PlayerController;

        #endregion
        #region Update

        private void OnValidate()
        {
            if (PlayerController == null)
            {
                PlayerController = GetComponentInChildren<PlayerController>();
            }
        }

        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        #endregion
        #region Method

        private void OnMove(InputValue _value)
        {
            PlayerController._moveInput = _value.Get<Vector2>();
        }

        private void OnLook(InputValue _value)
        {
            PlayerController._lookInput = _value.Get<Vector2>();
        }

        private void OnSprint(InputValue _value)
        {
            PlayerController._sprintInput = _value.isPressed;
        }

        private void OnJump(InputValue _value)
        {
            if (_value.isPressed)
            {
                PlayerController.TryJump();
            }
        }

        #endregion
    }
}

using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FocusRunner
{
    /// <summary>
    /// SRP: Sadece klavye ve dokunmatik girişinden sorumludur.
    /// Şerit değişim yönünü event ile bildirir, hareket mantığı PlayerController'da kalır.
    /// </summary>
    public class FocusRunnerInputHandler : MonoBehaviour
    {
        [Header("Dokunmatik Ayarları")]
        [SerializeField] private float minSwipeDistance = 50f;

        /// <summary>
        /// Şerit değişim isteği: -1 (sol), +1 (sağ)
        /// </summary>
        public event Action<int> OnLaneChangeRequested;

        private Vector2 _swipeStart;
        private bool _isSwiping;

        private void Update()
        {
            HandleKeyboardInput();
            HandleTouchInput();
        }

        private void HandleKeyboardInput()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
            {
                OnLaneChangeRequested?.Invoke(-1);
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
            {
                OnLaneChangeRequested?.Invoke(1);
            }
        }

        private void HandleTouchInput()
        {
            if (Touchscreen.current == null) return;

            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                _swipeStart = touch.position.ReadValue();
                _isSwiping = true;
            }
            else if (touch.press.wasReleasedThisFrame && _isSwiping)
            {
                Vector2 swipeEnd = touch.position.ReadValue();
                float dist = swipeEnd.x - _swipeStart.x;

                if (Mathf.Abs(dist) > minSwipeDistance)
                {
                    OnLaneChangeRequested?.Invoke(dist > 0 ? 1 : -1);
                }
                _isSwiping = false;
            }
        }
    }
}

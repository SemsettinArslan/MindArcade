using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SequenceCubes
{
    /// <summary>
    /// SRP: Sadece kullanıcı girişinden sorumludur.
    /// Mouse/Dokunma ile Raycast yaparak tıklanan küpü bulur ve bildirir.
    /// </summary>
    public class SequenceInputHandler : MonoBehaviour
    {
        public bool IsAcceptingInput { get; set; }

        public event Action<SequenceCube> OnCubeClicked;

        private void Update()
        {
            if (!IsAcceptingInput) return;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleMouseClick();
            }

            // Mobil dokunmatik desteği
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                {
                    HandleTouchAt(touch.position.ReadValue());
                }
            }
        }

        private void HandleMouseClick()
        {
            if (Camera.main == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            TryRaycastCube(mousePos);
        }

        private void HandleTouchAt(Vector2 screenPos)
        {
            TryRaycastCube(screenPos);
        }

        private void TryRaycastCube(Vector2 screenPosition)
        {
            if (Camera.main == null) return;

            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                SequenceCube clickedCube = hit.collider.GetComponent<SequenceCube>();
                if (clickedCube != null)
                {
                    clickedCube.OnUserClicked();
                    OnCubeClicked?.Invoke(clickedCube);
                }
            }
        }
    }
}

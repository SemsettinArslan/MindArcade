using UnityEngine;
using UnityEngine.InputSystem; 

namespace FocusRunner
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _laneChangeSpeed = 10f;
        [SerializeField] private int _currentLane = 1; // 0=L, 1=M, 2=R

        [Header("VFX")]
        [SerializeField] private ParticleSystem _correctParticle;
        [SerializeField] private ParticleSystem _wrongParticle;

        private Vector3 _targetPosition;
        private Vector2 _swipeStart;
        private float _minSwipeDistance = 50f;
        private bool _isSwiping = false;

        private FocusRunnerUI _ui;

        private void Start()
        {
            _ui = FindObjectOfType<FocusRunnerUI>();
            UpdateTargetPosition();
            transform.position = _targetPosition;
        }

        private void Update()
        {
            HandleInput();
            MoveTowardTarget();
        }

        private void HandleInput()
        {
            // --- KLAVYE KONTROLÜ ---
            if (Keyboard.current != null)
            {
                if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
                {
                    ChangeLane(-1);
                }
                else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
                {
                    ChangeLane(1);
                }
            }

            // --- MOBİL DOKUNMATİK / SWIPE ---
            if (Touchscreen.current != null)
            {
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

                    if (Mathf.Abs(dist) > _minSwipeDistance)
                    {
                        ChangeLane(dist > 0 ? 1 : -1);
                    }
                    _isSwiping = false;
                }
            }
        }

        private void ChangeLane(int direction)
        {
            int newLane = _currentLane + direction;
            if (newLane >= 0 && newLane < FocusRunnerConstants.TotalLanes)
            {
                _currentLane = newLane;
                UpdateTargetPosition();
            }
        }

        private void UpdateTargetPosition()
        {
            _targetPosition = new Vector3(FocusRunnerConstants.GetLaneX(_currentLane), transform.position.y, transform.position.z);
        }

        private void MoveTowardTarget()
        {
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _laneChangeSpeed);
        }

        public void OnCollect(StroopObject item)
        {
            bool isCorrect = false;

            if (item.IsObstacle)
            {
                isCorrect = false;
            }
            else
            {
                isCorrect = RuleManager.Instance.ValidateMatch(item.data, item.inkColor);
            }

            // UI'daki parlama efektini tetikle
            if (_ui != null) _ui.TriggerFlash(isCorrect);
            
            PlayFeedback(isCorrect);
            FocusRunnerManager.Instance.ReportMatchResult(isCorrect);
        }

        private void PlayFeedback(bool isCorrect)
        {
            if (isCorrect && _correctParticle != null) _correctParticle.Play();
            else if (!isCorrect && _wrongParticle != null) _wrongParticle.Play();
        }
    }
}

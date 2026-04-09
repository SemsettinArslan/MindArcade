using UnityEngine;

namespace FocusRunner
{
    /// <summary>
    /// Oyuncunun fiziksel hareketini ve toplama etkileşimini yönetir.
    /// Input sorumluluğu FocusRunnerInputHandler'a devredildi.
    /// UI bağımlılığı event-driven olarak kaldırıldı.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _laneChangeSpeed = 10f;
        [SerializeField] private int _currentLane = 1; // 0=L, 1=M, 2=R

        [Header("VFX")]
        [SerializeField] private ParticleSystem _correctParticle;
        [SerializeField] private ParticleSystem _wrongParticle;

        [Header("Bağımlılıklar (Inspector'dan Ata)")]
        [SerializeField] private FocusRunnerInputHandler _inputHandler;

        private Vector3 _targetPosition;

        private void OnEnable()
        {
            if (_inputHandler != null)
                _inputHandler.OnLaneChangeRequested += ChangeLane;
        }

        private void OnDisable()
        {
            if (_inputHandler != null)
                _inputHandler.OnLaneChangeRequested -= ChangeLane;
        }

        private void Start()
        {
            UpdateTargetPosition();
            transform.position = _targetPosition;
        }

        private void Update()
        {
            MoveTowardTarget();
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
            _targetPosition = new Vector3(
                FocusRunnerConstants.GetLaneX(_currentLane),
                transform.position.y,
                transform.position.z
            );
        }

        private void MoveTowardTarget()
        {
            transform.position = Vector3.Lerp(
                transform.position,
                _targetPosition,
                Time.deltaTime * _laneChangeSpeed
            );
        }

        /// <summary>
        /// StroopObject trigger'ından çağrılır.
        /// Doğrulama ve rapor işini yapar, UI'ı event ile bilgilendirir.
        /// </summary>
        public void OnCollect(StroopObject item)
        {
            bool isCorrect;

            if (item.IsObstacle)
            {
                isCorrect = false;
            }
            else
            {
                isCorrect = RuleManager.Instance.ValidateMatch(item.data, item.inkColor);
            }

            // Event-driven feedback (UI doğrudan bağımlılığı yok)
            MindArcade.Core.GameEvents.RaiseFlashFeedback(isCorrect);

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

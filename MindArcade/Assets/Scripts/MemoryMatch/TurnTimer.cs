using System;
using UnityEngine;

namespace MemoryMatch
{
    /// <summary>
    /// SRP: Sadece tur zamanlayıcısını yönetir.
    /// Başlatma, durdurma ve timeout event'i sağlar.
    /// </summary>
    public class TurnTimer : MonoBehaviour
    {
        [Tooltip("İlk kart seçildikten sonra ikinciyi bulmak için kaç saniye verilecek?")]
        [SerializeField] private float turnTimeLimit = 5.0f;

        public float RemainingTime { get; private set; }
        public bool IsRunning { get; private set; }

        public event Action OnTimeOut;
        public event Action<float> OnTimerUpdated; // Her frame kalan süreyi bildirir

        /// <summary>
        /// Zamanlayıcıyı başlatır.
        /// </summary>
        public void StartTimer()
        {
            RemainingTime = turnTimeLimit;
            IsRunning = true;
        }

        /// <summary>
        /// Zamanlayıcıyı durdurur (doğru eşleşme veya ikinci kart seçildiğinde).
        /// </summary>
        public void StopTimer()
        {
            IsRunning = false;
        }

        private void Update()
        {
            if (!IsRunning) return;

            RemainingTime -= Time.deltaTime;
            OnTimerUpdated?.Invoke(RemainingTime);

            if (RemainingTime <= 0)
            {
                IsRunning = false;
                OnTimeOut?.Invoke();
            }
        }
    }
}

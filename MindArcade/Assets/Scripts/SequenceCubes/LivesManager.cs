using System;
using UnityEngine;

namespace SequenceCubes
{
    /// <summary>
    /// SRP: Sadece can (lives) sistemini yönetir.
    /// Can azaltma, sıfırlama ve event bildirimi yapar.
    /// </summary>
    public class LivesManager : MonoBehaviour
    {
        [SerializeField] private int maxLives = 3;

        public int CurrentLives { get; private set; }
        public bool IsAlive => CurrentLives > 0;

        public event Action<int> OnLivesChanged;
        public event Action OnAllLivesLost;

        /// <summary>
        /// Canları sıfırlar (yeni level başlangıcı).
        /// </summary>
        public void ResetLives()
        {
            CurrentLives = maxLives;
            OnLivesChanged?.Invoke(CurrentLives);
        }

        /// <summary>
        /// Bir can azaltır. Tüm canlar biterse event fırlatır.
        /// </summary>
        public void LoseLife()
        {
            CurrentLives--;
            OnLivesChanged?.Invoke(CurrentLives);

            if (CurrentLives <= 0)
            {
                OnAllLivesLost?.Invoke();
            }
        }
    }
}

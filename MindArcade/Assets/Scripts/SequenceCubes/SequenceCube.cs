using UnityEngine;
using System;
using System.Collections;

namespace SequenceCubes
{
    /// <summary>
    /// SOLID Sorumluluğu: Bu sınıf sadece Küpün görsel animasyonunu (kalkıp inmesi)
    /// ve tıklanabilmesini üstlenir. Sıralamayı, eşleştirmeyi bilip takip etmez.
    /// </summary>
    [RequireComponent(typeof(Collider))] // Tıklanabilmesi için 3D Collider şart.
    public class SequenceCube : MonoBehaviour
    {
        [Header("Animasyon Ayarları")]
        [Tooltip("Küp yukarı kalkarken Y ekseninde ne kadar yükselecek?")]
        [SerializeField] private float moveHeight = 1.0f;
        [Tooltip("Yukarı çıkma veya inme işlemi kaç saniye sürecek?")]
        [SerializeField] private float moveDuration = 0.25f;
        
        public int Id { get; private set; }
        
        private Action<SequenceCube> _onClickedCallback;
        private bool _isInteractable = false;
        
        private Vector3 _originalLocalPosition;
        private Coroutine _animationCoroutine;

        private void Awake()
        {
            // Başlangıç pozisyonunu belleğe al (Parent değiştirme ihtimaline karşı localPosition)
            _originalLocalPosition = transform.localPosition;
        }

        public void Initialize(int id, Action<SequenceCube> onClickedCallback)
        {
            Id = id;
            _onClickedCallback = onClickedCallback;
        }

        public void SetInteractable(bool state)
        {
            _isInteractable = state;
        }

        /// <summary>
        /// Manager tarafından fare ile küpe tıklandığında (Raycast çarptığında) tetiklenir.
        /// </summary>
        public void OnUserClicked()
        {
            Debug.Log($"[{gameObject.name}] adlı küpe Raycast ile tıklandı! Tıklama izni (Interactable) durumu: {_isInteractable}");

            if (!_isInteractable) return;
            
            // Oyuncu kendi tıklarken her zaman normal olarak "Yukarı" zıplatsın
            PlayAnimation(false);
            
            // Tıkladığımın bilgisini Manager'a uçur.
            _onClickedCallback?.Invoke(this);
        }

        public void PlayAnimation(bool isDecoy, Action onComplete = null)
        {
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);
                
            _animationCoroutine = StartCoroutine(AnimateCube(isDecoy, onComplete));
        }

        /// <summary>
        /// Performanslı şekilde çalışan yukarı/aşağı 3D sekme animasyonu
        /// İsDecoy (Aşağı inen kandırma) ise aşağı, aksi halde yukarı yön yapar
        /// </summary>
        private IEnumerator AnimateCube(bool isDecoy, Action onComplete)
        {
            float elapsedTime = 0f;
            Vector3 direction = isDecoy ? Vector3.down : Vector3.up;
            Vector3 targetPosition = _originalLocalPosition + (direction * moveHeight);

            // 1. Aşama: Yukarı doğru çık
            while (elapsedTime < moveDuration)
            {
                transform.localPosition = Vector3.Lerp(_originalLocalPosition, targetPosition, elapsedTime / moveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = targetPosition;
            
            // Tepede hissedilebilir kısa bir süre (0.1sn) bekle (Hissiyatı (Game Feel) güçlendirir)
            yield return new WaitForSeconds(0.1f);

            // 2. Aşama: Aşağı doğru orijinal konumuna dön
            elapsedTime = 0f;
            while (elapsedTime < moveDuration)
            {
                transform.localPosition = Vector3.Lerp(targetPosition, _originalLocalPosition, elapsedTime / moveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = _originalLocalPosition;
            
            // Animasyon tamamen bittiğinde CallBack (Action) fırlat.
            onComplete?.Invoke();
        }
    }
}

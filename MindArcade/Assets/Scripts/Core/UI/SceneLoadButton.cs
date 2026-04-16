using UnityEngine;
using UnityEngine.UI;

namespace MindArcade.Core.UI
{
    /// <summary>
    /// Unity Inspector'daki OnClick event'leri Enum parametrelerini doğrudan desteklemediği için
    /// bu yardımcı sınıf sayesinde butonlara kolayca Enum (açılır menü) üzerinden olay atayabilirsiniz.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SceneLoadButton : MonoBehaviour
    {
        [Tooltip("Yüklenecek sahneyi listeden seçiniz.")]
        [SerializeField] private SceneType targetScene;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (SceneNavigator.Instance != null)
            {
                SceneNavigator.Instance.LoadScene(targetScene);
            }
            else
            {
                Debug.LogError("SceneNavigator sahnede bulunamadı! Lütfen bir obje ekleyin.");
            }
        }
        
        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
        }
    }
}

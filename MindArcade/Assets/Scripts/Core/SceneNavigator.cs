using UnityEngine;
using UnityEngine.SceneManagement;
using MindArcade.Core.UI;

namespace MindArcade.Core
{
    /// <summary>
    /// Projedeki sahnelerin isimlerini temsil eden Enum.
    /// Build Settings'deki isimlerle birebir uyuşmaları için ToString() kullanılır.
    /// Sahnelerde isim değişikliği olursa Enum değerini (veya ToString yapısını) güncellemeniz yeterlidir.
    /// </summary>
    public enum SceneType
    {
        MainMenu,
        MemoryMatch,
        FocusRunner,
        SequenceCubes
    }

    /// <summary>
    /// Enum parametresi alarak ScreenFader üzerinden sahne geçişlerini yöneten sınıf.
    /// </summary>
    public class SceneNavigator : MonoBehaviour
    {
        public static SceneNavigator Instance { get; private set; }

        private static bool _isCreatingPersistentInstance = false;

        private void Awake()
        {
            if (_isCreatingPersistentInstance)
            {
                // Bu, yeni oluşturulan kalıcı nesnenin Awake çağrısıdır, işlem yapma
                return;
            }

            if (Instance == null)
            {
                _isCreatingPersistentInstance = true;
                GameObject navGo = new GameObject("SceneNavigator");
                Instance = navGo.AddComponent<SceneNavigator>();
                _isCreatingPersistentInstance = false;

                DontDestroyOnLoad(navGo);
                
                // Orijinal GameObject üzerindeki bu scripti temizle (böylece panel hayatta kalır)
                Destroy(this);
            }
            else
            {
                // Zaten bir instance varsa, sadece bu scripti yok et (GameObject'i değil)
                Destroy(this);
            }
        }

        // --- CORE YÜKLEME METODU ---
        
        /// <summary>
        /// SceneLoadButton üzerinden Enum ile güvenli çağrımlar yapmak için.
        /// </summary>
        public void LoadScene(SceneType sceneType)
        {
            Time.timeScale = 1f; // Pause durumunu sıfırla

            string sceneName = sceneType.ToString();

            if (ScreenFader.Instance != null)
            {
                ScreenFader.Instance.TransitionToScene(sceneName);
            }
            else
            {
                Debug.LogWarning("ScreenFader bulunamadı! Direkt geçiş yapılıyor.");
                SceneManager.LoadScene(sceneName);
            }
        }
        public void QuitGame()
        {
            Debug.Log("Uygulamadan Çıkılıyor...");
            Application.Quit();
        }
    }
}

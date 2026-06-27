using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MindArcade.Core.Firebase;
using MindArcade.Core.Llm;
using MindArcade.Core.Utils;

namespace MindArcade.Core.UI
{
    public class DoctorPortalPanel : UIPanel
    {
        [Header("Hasta Tanımlama Paneli")]
        [SerializeField] private TMP_InputField newPatientIdInputField;
        [SerializeField] private TMP_InputField newPatientNameInputField;
        [SerializeField] private TMP_InputField newPatientPasswordInputField;
        [SerializeField] private Button createPatientButton;
        [SerializeField] private TMP_Text createPatientStatusText;

        [Header("Hasta Listesi Paneli")]
        [SerializeField] private RectTransform patientListContainer; // ScrollRect Content
        [SerializeField] private Button patientItemPrefab; // Hasta butonu prefab'ı
        [SerializeField] private Button refreshListButton;
        [SerializeField] private TMP_Text listStatusText;

        [Header("Hasta Detay Paneli")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private TMP_Text detailTitleText; // Adı ve ID'si
        [SerializeField] private TMP_Text detailStatsText; // Cumulative İstatistikler
        [SerializeField] private TMP_Text detailSessionsText; // Seans Geçmişi Listesi
        [SerializeField] private Button exportReportButton;

        [Header("Genel Butonlar")]
        [SerializeField] private Button logoutButton;
        [SerializeField] private GameObject loginCanvas; // Giriş ekranına dönmek için
        
        [Header("Ana Menü Entegrasyonu")]
        [SerializeField] private MainMenuController mainMenuController;

        // Seçili hasta verileri
        private string _selectedPatientId;
        private string _selectedPatientName;
        private PlayerProfile _selectedPatientProfile;
        private List<FirebaseSessionData> _selectedPatientSessions;

        private void Start()
        {
            // Listener'ları bağla
            if (createPatientButton != null)
                createPatientButton.onClick.AddListener(OnCreatePatientClicked);

            if (refreshListButton != null)
                refreshListButton.onClick.AddListener(LoadPatientList);

            if (exportReportButton != null)
                exportReportButton.onClick.AddListener(OnExportReportClicked);

            if (logoutButton != null)
                logoutButton.onClick.AddListener(OnLogoutClicked);

            if (detailPanel != null) detailPanel.SetActive(false);
            if (createPatientStatusText != null) createPatientStatusText.text = "";
            if (listStatusText != null) listStatusText.text = "";
        }

        public void OpenPortal()
        {
            LoadPatientList();
            if (detailPanel != null) detailPanel.SetActive(false);
            if (createPatientStatusText != null) createPatientStatusText.text = "";
        }

        private void LoadPatientList()
        {
            if (listStatusText != null) listStatusText.text = "Hastalar yukleniyor...";
            Debug.Log("[DoctorPortal] LoadPatientList() başlatıldı.");

            // Mevcut listeyi temizle
            if (patientListContainer != null)
            {
                foreach (Transform child in patientListContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            else
            {
                Debug.LogError("[DoctorPortal] HATA: patientListContainer (ScrollRect Content) Inspector'da ATANMAMIŞ!");
            }

            if (FirebaseClient.Instance == null)
            {
                if (listStatusText != null) listStatusText.text = "Hata: Veritabani baglantisi yok!";
                Debug.LogError("[DoctorPortal] HATA: FirebaseClient.Instance bulunamadı!");
                return;
            }

            if (patientItemPrefab == null)
            {
                Debug.LogError("[DoctorPortal] HATA: patientItemPrefab Inspector'da ATANMAMIŞ!");
            }

            FirebaseClient.Instance.GetAllPatients((patients) =>
            {
                Debug.Log($"[DoctorPortal] GetAllPatients yanıtı geldi. Gelen hasta sayısı: {(patients != null ? patients.Count.ToString() : "null")}");
                
                if (patients == null || patients.Count == 0)
                {
                    if (listStatusText != null) listStatusText.text = "Kayitli hasta bulunamadi.";
                    return;
                }

                if (listStatusText != null) listStatusText.text = "";

                // Listeyi oluştur
                foreach (var kvp in patients)
                {
                    string id = kvp.Key;
                    string name = kvp.Value;

                    if (patientItemPrefab != null && patientListContainer != null)
                    {
                        Button btn = Instantiate(patientItemPrefab, patientListContainer);
                        TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
                        if (txt != null)
                        {
                            txt.text = $"{name} (ID: {id})";
                        }
                        else
                        {
                            Debug.LogWarning("[DoctorPortal] patientItemPrefab içinde veya alt objelerinde TextMeshPro (TMP_Text) bileşeni bulunamadı!");
                        }

                        btn.onClick.AddListener(() => OnPatientSelected(id, name));
                        Debug.Log($"[DoctorPortal] Hasta listeye eklendi: {name} (ID: {id})");
                    }
                }
            });
        }

        private void OnCreatePatientClicked()
        {
            if (newPatientIdInputField == null || newPatientNameInputField == null) return;

            string id = newPatientIdInputField.text.Trim();
            string name = newPatientNameInputField.text.Trim();
            string password = newPatientPasswordInputField != null ? newPatientPasswordInputField.text.Trim() : "";

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
            {
                ShowCreateStatus("Lutfen ID, Isim ve Sifre alanlarini doldurun!", isError: true);
                return;
            }

            ShowCreateStatus("Olusturuluyor...", isError: false);
            if (createPatientButton != null) createPatientButton.interactable = false;

            FirebaseClient.Instance.CheckPatientIdExists(id, (exists, existingName) =>
            {
                if (exists)
                {
                    if (createPatientButton != null) createPatientButton.interactable = true;
                    ShowCreateStatus("Hata: Bu ID zaten kayitli!", isError: true);
                }
                else
                {
                    FirebaseClient.Instance.CreatePatient(id, name, password, (success) =>
                    {
                        if (createPatientButton != null) createPatientButton.interactable = true;

                        if (success)
                        {
                            ShowCreateStatus("Hasta basariyla olusturuldu.", isError: false);
                            newPatientIdInputField.text = "";
                            newPatientNameInputField.text = "";
                            if (newPatientPasswordInputField != null) newPatientPasswordInputField.text = "";
                            LoadPatientList();
                        }
                        else
                        {
                            ShowCreateStatus("Hata: Olusturma basarisiz!", isError: true);
                        }
                    });
                }
            });
        }

        private void OnPatientSelected(string id, string name)
        {
            _selectedPatientId = id;
            _selectedPatientName = name;

            if (detailPanel != null) detailPanel.SetActive(true);
            if (detailTitleText != null) detailTitleText.text = $"{name} (Hasta ID: {id})";
            if (detailStatsText != null) detailStatsText.text = "Detaylar cekiliyor...";
            if (detailSessionsText != null) detailSessionsText.text = "";

            if (FirebaseClient.Instance == null) return;

            FirebaseClient.Instance.GetPatientDetails(id, (json) =>
            {
                if (string.IsNullOrEmpty(json) || json == "null")
                {
                    if (detailStatsText != null) detailStatsText.text = "Hasta verileri bulunamadi.";
                    return;
                }

                // 1. Profil (PlayerProfile) JSON'ını ayıkla ve deserialize et
                string profileJson = ExtractJsonBlock(json, "profile");
                if (!string.IsNullOrEmpty(profileJson))
                {
                    try
                    {
                        _selectedPatientProfile = JsonUtility.FromJson<PlayerProfile>(profileJson);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[DoctorPortal] Profile parsing error: {ex.Message}");
                        _selectedPatientProfile = new PlayerProfile();
                    }
                }
                else
                {
                    _selectedPatientProfile = new PlayerProfile();
                }

                // Seans geçmişini ayrıştırmıyoruz (kaldırıldı)
                _selectedPatientSessions = new List<FirebaseSessionData>();

                // UI Güncelle
                UpdateDetailUI();
            });
        }

        private void UpdateDetailUI()
        {
            if (_selectedPatientProfile == null) return;

            // Kümülatif istatistikleri yaz
            System.Text.StringBuilder sbStats = new System.Text.StringBuilder();
            sbStats.AppendLine("<b>Kumulatif Performans Ozeti:</b>\n");
            
            AppendGameStatsText(sbStats, "MemoryMatch (Hafiza)", _selectedPatientProfile.memoryMatchStats);
            AppendGameStatsText(sbStats, "FocusRunner (Dikkat)", _selectedPatientProfile.focusRunnerStats);
            AppendGameStatsText(sbStats, "SequenceCubes (Ardisil)", _selectedPatientProfile.sequenceCubesStats);

            if (detailStatsText != null) detailStatsText.text = sbStats.ToString();

            // Seans geçmişini ve yapay zeka özetini arayüzden kaldırdık
            if (detailSessionsText != null) detailSessionsText.text = "";
        }

        private void AppendGameStatsText(System.Text.StringBuilder sb, string title, GameStats stats)
        {
            if (stats == null) stats = new GameStats();
            sb.AppendLine($"<b>{title}:</b> Oynama: {stats.totalGamesPlayed} | Seviye: {stats.highestLevel} | En Yuksek Skor: {stats.highestScore} | Dogruluk: %{Mathf.RoundToInt(stats.averageAccuracy * 100)}");
        }

        private void OnExportReportClicked()
        {
            if (string.IsNullOrEmpty(_selectedPatientId) || string.IsNullOrEmpty(_selectedPatientName)) return;

            if (createPatientStatusText != null) createPatientStatusText.text = "Rapor hazirlaniyor...";

            string path = ReportExporter.ExportReport(_selectedPatientId, _selectedPatientName, _selectedPatientProfile, _selectedPatientSessions);

            if (!string.IsNullOrEmpty(path))
            {
                if (createPatientStatusText != null)
                {
                    createPatientStatusText.color = Color.green;
                    createPatientStatusText.text = $"Rapor kaydedildi!\nDosya: {System.IO.Path.GetFileName(path)}";
                }
            }
            else
            {
                if (createPatientStatusText != null)
                {
                    createPatientStatusText.color = Color.red;
                    createPatientStatusText.text = "Rapor olusturma hatasi!";
                }
            }
        }

        private void OnLogoutClicked()
        {
            if (detailPanel != null) detailPanel.SetActive(false);

            if (mainMenuController != null)
            {
                mainMenuController.ShowMainMenu();
            }
            else
            {
                var parentMenu = GetComponentInParent<MainMenuController>();
                if (parentMenu != null)
                {
                    parentMenu.ShowMainMenu();
                }
                else
                {
                    Hide();
                    if (loginCanvas != null) loginCanvas.SetActive(true);
                }
            }
        }

        // --- JSON AYIKLAMA YARDIMCILARI ---

        private string ExtractJsonBlock(string json, string key)
        {
            int keyIndex = json.IndexOf($"\"{key}\":");
            if (keyIndex == -1) return null;

            int startIndex = json.IndexOf('{', keyIndex);
            if (startIndex == -1) return null;

            int braceCount = 1;
            int endIndex = startIndex + 1;
            while (braceCount > 0 && endIndex < json.Length)
            {
                if (json[endIndex] == '{') braceCount++;
                else if (json[endIndex] == '}') braceCount--;
                endIndex++;
            }

            if (braceCount == 0)
            {
                return json.Substring(startIndex, endIndex - startIndex);
            }
            return null;
        }

        private List<FirebaseSessionData> ParseSessions(string sessionsJson)
        {
            List<FirebaseSessionData> list = new List<FirebaseSessionData>();
            if (string.IsNullOrEmpty(sessionsJson)) return list;

            // Seans nesnelerini çıkar: { "accuracy":... }
            var matches = Regex.Matches(sessionsJson, @"\{[^\}]+\}");
            foreach (Match match in matches)
            {
                try
                {
                    FirebaseSessionData session = JsonUtility.FromJson<FirebaseSessionData>(match.Value);
                    if (session != null && !string.IsNullOrEmpty(session.gameName))
                    {
                        list.Add(session);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[DoctorPortal] Seans ayrıştırma hatası: {ex.Message}");
                }
            }

            // Yeniden eskiye sırala (en son seans en üstte)
            list.Sort((a, b) => string.Compare(b.timestamp, a.timestamp));
            return list;
        }

        private void ShowCreateStatus(string message, bool isError)
        {
            if (createPatientStatusText != null)
            {
                createPatientStatusText.color = isError ? Color.red : Color.green;
                createPatientStatusText.text = message;
            }
        }
    }
}

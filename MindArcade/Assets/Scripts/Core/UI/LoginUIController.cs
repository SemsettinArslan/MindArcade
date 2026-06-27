using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MindArcade.Core.Firebase;
using MindArcade.Core.Llm;

namespace MindArcade.Core.UI
{
    public class LoginUIController : MonoBehaviour
    {
        [Header("Alt Paneller")]
        [SerializeField] private GameObject loginCanvas;
        [SerializeField] private GameObject patientLoginSubPanel;
        [SerializeField] private GameObject doctorLoginSubPanel;
        [SerializeField] private DoctorPortalPanel doctorPortalPanel;

        [Header("Hasta Giriş Elemanları")]
        [SerializeField] private TMP_InputField patientIdInputField;
        [SerializeField] private TMP_InputField patientPasswordInputField;
        [SerializeField] private Button patientLoginButton;
        [SerializeField] private TMP_Text patientErrorText;

        [Header("Doktor Giriş Elemanları")]
        [SerializeField] private Button openDoctorLoginButton;
        [SerializeField] private Button closeDoctorLoginButton;
        [SerializeField] private TMP_InputField doctorUsernameField;
        [SerializeField] private TMP_InputField doctorPasswordField;
        [SerializeField] private Button doctorLoginButton;
        [SerializeField] private TMP_Text doctorErrorText;

        [Header("Ana Menü Entegrasyonu")]
        [SerializeField] private MainMenuController mainMenuController;

        private void Start()
        {
            // Başlangıç durumları
            if (loginCanvas != null) loginCanvas.SetActive(true);
            if (patientLoginSubPanel != null) patientLoginSubPanel.SetActive(true);
            if (doctorLoginSubPanel != null) doctorLoginSubPanel.SetActive(false);
            if (doctorPortalPanel != null) doctorPortalPanel.gameObject.SetActive(false);

            if (patientErrorText != null) patientErrorText.text = "";
            if (doctorErrorText != null) doctorErrorText.text = "";

            // Listener'ları bağla
            if (patientLoginButton != null)
                patientLoginButton.onClick.AddListener(OnPatientLoginClicked);

            if (openDoctorLoginButton != null)
                openDoctorLoginButton.onClick.AddListener(() => SwitchSubPanel(isDoctor: true));

            if (closeDoctorLoginButton != null)
                closeDoctorLoginButton.onClick.AddListener(() => SwitchSubPanel(isDoctor: false));

            if (doctorLoginButton != null)
                doctorLoginButton.onClick.AddListener(OnDoctorLoginClicked);
        }

        private void SwitchSubPanel(bool isDoctor)
        {
            if (mainMenuController != null)
            {
                if (isDoctor)
                {
                    mainMenuController.ShowDoctorLoginPanel();
                }
                else
                {
                    mainMenuController.ShowLoginPanel();
                }
            }
            else
            {
                if (patientLoginSubPanel != null) patientLoginSubPanel.SetActive(!isDoctor);
                if (doctorLoginSubPanel != null) doctorLoginSubPanel.SetActive(isDoctor);
            }

            if (doctorErrorText != null) doctorErrorText.text = "";
            if (patientErrorText != null) patientErrorText.text = "";
        }

        private void OnPatientLoginClicked()
        {
            if (patientIdInputField == null) return;

            string id = patientIdInputField.text.Trim();
            string password = patientPasswordInputField != null ? patientPasswordInputField.text : "";

            if (string.IsNullOrEmpty(id))
            {
                ShowPatientError("Lutfen bir Hasta ID giriniz!");
                return;
            }

            ShowPatientError("Dogrulaniyor...");
            if (patientLoginButton != null) patientLoginButton.interactable = false;

            // Firebase'de ID ve Şifre sorgula
            if (FirebaseClient.Instance != null)
            {
                FirebaseClient.Instance.ValidatePatientLogin(id, password, (success, name) =>
                {
                    if (patientLoginButton != null) patientLoginButton.interactable = true;

                    if (success)
                    {
                        // Başarılı giriş
                        PlayerProfileManager.ActivePatientId = id;
                        PlayerProfileManager.ActivePatientName = name;

                        Debug.Log($"[Login] Başarılı giriş: {name} (ID: {id})");
                        
                        if (mainMenuController != null)
                        {
                            mainMenuController.ShowPlayPanel();
                        }
                        else
                        {
                            // Giriş ekranını kapat ve Ana Menüyü aç/başlat
                            if (loginCanvas != null) loginCanvas.SetActive(false);
                        }
                    }
                    else
                    {
                        ShowPatientError("Hata: Gecersiz ID veya Sifre!");
                    }
                });
            }
            else
            {
                if (patientLoginButton != null) patientLoginButton.interactable = true;
                ShowPatientError("Veritabani baglantisi yok (Offline Devre Disi).");
            }
        }

        private void OnDoctorLoginClicked()
        {
            if (doctorUsernameField == null || doctorPasswordField == null) return;

            string username = doctorUsernameField.text.Trim();
            string password = doctorPasswordField.text;

            // Sabit değerler: username = "test", password = "test123"
            if (username == "test" && password == "test123")
            {
                Debug.Log("[Login] Doktor girişi başarılı.");
                
                if (mainMenuController != null)
                {
                    mainMenuController.ShowDoctorPortalPanel();
                }
                else
                {
                    if (doctorLoginSubPanel != null) doctorLoginSubPanel.SetActive(false);
                    if (loginCanvas != null) loginCanvas.SetActive(false);

                    if (doctorPortalPanel != null)
                    {
                        doctorPortalPanel.gameObject.SetActive(true);
                        doctorPortalPanel.OpenPortal();
                    }
                }
            }
            else
            {
                if (doctorErrorText != null)
                {
                    doctorErrorText.color = Color.red;
                    doctorErrorText.text = "Gecersiz kullanici adi veya sifre!";
                }
            }
        }

        private void ShowPatientError(string message)
        {
            if (patientErrorText != null)
            {
                patientErrorText.color = message.Contains("Hata") || message.Contains("Lütfen") ? Color.red : Color.yellow;
                patientErrorText.text = message;
            }
        }
    }
}

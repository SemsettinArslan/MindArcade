using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using MindArcade.Core.Llm;

namespace MindArcade.Core.Firebase
{
    public class FirebaseClient : MonoBehaviour
    {
        public static FirebaseClient Instance { get; private set; }

        [Header("Firebase Ayarları")]
        [Tooltip("Firebase Realtime Database URL (Sondaki '/' karakteri ile birlikte, örn: https://proje-id.firebaseio.com/)")]
        [SerializeField] private string databaseUrl = "https://mind-arcade-3aef2-default-rtdb.europe-west1.firebasedatabase.app/";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // --- PUBLIC API ---

        /// <summary>
        /// Belirtilen Hasta ID'sinin veritabanında var olup olmadığını kontrol eder.
        /// Varsa ismini geri döner.
        /// </summary>
        public void CheckPatientIdExists(string patientId, Action<bool, string> callback)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                callback?.Invoke(false, null);
                return;
            }

            StartCoroutine(GetRequest(databaseUrl + "patients/" + patientId + ".json", (json) =>
            {
                if (string.IsNullOrEmpty(json) || json == "null")
                {
                    callback?.Invoke(false, null);
                }
                else
                {
                    // İsmini çek
                    string name = ExtractNameFromJson(json);
                    callback?.Invoke(true, name);
                }
            }));
        }

        /// <summary>
        /// Yeni bir hasta oluşturur.
        /// </summary>
        public void CreatePatient(string patientId, string name, string password, Action<bool> callback)
        {
            if (string.IsNullOrEmpty(patientId) || string.IsNullOrEmpty(name))
            {
                callback?.Invoke(false);
                return;
            }

            if (password == null) password = "";

            string json = "{\"name\":\"" + name + "\",\"password\":\"" + password + "\"}";
            StartCoroutine(PutRequest(databaseUrl + "patients/" + patientId + ".json", json, (success) =>
            {
                callback?.Invoke(success);
            }));
        }

        /// <summary>
        /// Hem hasta ID'sini hem de girilen şifreyi doğrular.
        /// </summary>
        public void ValidatePatientLogin(string patientId, string password, Action<bool, string> callback)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                callback?.Invoke(false, null);
                return;
            }

            StartCoroutine(GetRequest(databaseUrl + "patients/" + patientId + ".json", (json) =>
            {
                if (string.IsNullOrEmpty(json) || json == "null")
                {
                    callback?.Invoke(false, null);
                }
                else
                {
                    string name = ExtractNameFromJson(json);
                    string dbPassword = ExtractPasswordFromJson(json);

                    // Geriye dönük uyumluluk: DB'deki şifre boşsa şifresiz girişe izin ver, aksi halde şifre kontrolü yap
                    if (string.IsNullOrEmpty(dbPassword) || dbPassword == password)
                    {
                        callback?.Invoke(true, name);
                    }
                    else
                    {
                        callback?.Invoke(false, null);
                    }
                }
            }));
        }

        private string ExtractPasswordFromJson(string json)
        {
            var match = Regex.Match(json, @"\""password\""\s*:\s*\""([^""]+)\""");
            if (match.Success && match.Groups.Count >= 2)
            {
                return match.Groups[1].Value;
            }
            return "";
        }

        /// <summary>
        /// Hastanın genel profil (GameStats) verilerini Firebase'e kaydeder.
        /// </summary>
        public void SavePatientProfile(string patientId, PlayerProfile profile, Action<bool> callback = null)
        {
            if (string.IsNullOrEmpty(patientId) || profile == null)
            {
                callback?.Invoke(false);
                return;
            }

            string json = JsonUtility.ToJson(profile);
            StartCoroutine(PutRequest(databaseUrl + "patients/" + patientId + "/profile.json", json, callback));
        }

        /// <summary>
        /// Hastanın oynadığı bir oyun seansını (skor, doğruluk, süre, LLM yorumu) Firebase'e kaydeder.
        /// </summary>
        public void SaveSessionData(string patientId, string gameName, int score, float accuracy, int level, float duration, int mistakes, string aiComment, Action<bool> callback = null)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                callback?.Invoke(false);
                return;
            }

            FirebaseSessionData session = new FirebaseSessionData
            {
                timestamp = DateTime.UtcNow.ToString("o"), // ISO 8601 format
                gameName = gameName,
                score = score,
                accuracy = accuracy,
                level = level,
                duration = duration,
                mistakes = mistakes,
                aiComment = aiComment
            };

            string json = JsonUtility.ToJson(session);
            StartCoroutine(PostRequest(databaseUrl + "patients/" + patientId + "/sessions.json", json, (success) =>
            {
                callback?.Invoke(success);
            }));
        }

        /// <summary>
        /// Doktor için tüm hastaları ve bilgilerini listeler.
        /// </summary>
        public void GetAllPatients(Action<Dictionary<string, string>> callback)
        {
            Debug.Log("[FirebaseClient] GetAllPatients başlatıldı.");
            StartCoroutine(GetRequest(databaseUrl + "patients.json", (json) =>
            {
                Debug.Log($"[FirebaseClient] GetAllPatients yanıt aldı: {json}");
                Dictionary<string, string> patients = new Dictionary<string, string>();
                
                try
                {
                    if (!string.IsNullOrEmpty(json) && json != "null")
                    {
                        json = json.Trim();
                        if (json.StartsWith("["))
                        {
                            // Array formatı: [null, {"name":"s","profile":...}, ...]
                            // Köşeli parantezleri kaldır
                            string arrayContent = json.Substring(1, json.Length - 2);
                            
                            int braceCount = 0;
                            int startIndex = -1;
                            int currentIndex = 0; // Array indeksi (Hasta ID'si)
                            
                            for (int i = 0; i < arrayContent.Length; i++)
                            {
                                char c = arrayContent[i];
                                if (c == '{')
                                {
                                    if (braceCount == 0)
                                    {
                                        startIndex = i;
                                    }
                                    braceCount++;
                                }
                                else if (c == '}')
                                {
                                    braceCount--;
                                    if (braceCount == 0 && startIndex != -1)
                                    {
                                        string element = arrayContent.Substring(startIndex, i - startIndex + 1);
                                        string name = ExtractNameFromJson(element);
                                        
                                        // ID olarak dizideki indeks değerini kullanıyoruz
                                        patients[currentIndex.ToString()] = name;
                                        startIndex = -1;
                                    }
                                }
                                else if (c == ',' && braceCount == 0)
                                {
                                    currentIndex++;
                                }
                            }
                        }
                        else
                        {
                            // Object formatı: {"1":{"name":"s"}, "hasta2":{"name":"ali"}}
                            // Regex ile ID ve İsimleri eşleştir: "ID":{"name":"Name"
                            var matches = Regex.Matches(json, @"\""([^""]+)\""\s*:\s*\{\s*\""name\""\s*:\s*\""([^""]+)\""");
                            foreach (Match match in matches)
                            {
                                if (match.Groups.Count >= 3)
                                {
                                    string id = match.Groups[1].Value;
                                    string name = match.Groups[2].Value;
                                    patients[id] = name;
                                }
                            }
                        }
                    }
                    Debug.Log($"[FirebaseClient] GetAllPatients ayrıştırma bitti. Toplam hasta: {patients.Count}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[FirebaseClient] GetAllPatients ayrıştırma hatası: {ex.Message}\n{ex.StackTrace}");
                }
                
                callback?.Invoke(patients);
            }));
        }

        /// <summary>
        /// Seçilen hastanın detaylı profil ve oyun seans geçmişi verilerini çeker.
        /// </summary>
        public void GetPatientDetails(string patientId, Action<string> callback)
        {
            StartCoroutine(GetRequest(databaseUrl + "patients/" + patientId + ".json", callback));
        }

        // --- HTTP YARDIMCILARI (REST CLIENT) ---

        private IEnumerator GetRequest(string url, Action<string> callback)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(webRequest.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"[FirebaseClient] GET Hatası ({url}): {webRequest.error}");
                    callback?.Invoke(null);
                }
            }
        }

        private IEnumerator PutRequest(string url, string json, Action<bool> callback)
        {
            using (UnityWebRequest webRequest = new UnityWebRequest(url, "PUT"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true);
                }
                else
                {
                    Debug.LogError($"[FirebaseClient] PUT Hatası ({url}): {webRequest.error}");
                    callback?.Invoke(false);
                }
            }
        }

        private IEnumerator PostRequest(string url, string json, Action<bool> callback)
        {
            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true);
                }
                else
                {
                    Debug.LogError($"[FirebaseClient] POST Hatası ({url}): {webRequest.error}");
                    callback?.Invoke(false);
                }
            }
        }

        private string ExtractNameFromJson(string json)
        {
            // JSON içindeki ilk "name":"değer" kısmını regex ile bulur
            var match = Regex.Match(json, @"\""name\""\s*:\s*\""([^""]+)\""");
            if (match.Success && match.Groups.Count >= 2)
            {
                return match.Groups[1].Value;
            }
            return "Bilinmeyen Hasta";
        }
    }

    [Serializable]
    public class FirebaseSessionData
    {
        public string timestamp;
        public string gameName;
        public int score;
        public float accuracy;
        public int level;
        public float duration;
        public int mistakes;
        public string aiComment;
    }
}

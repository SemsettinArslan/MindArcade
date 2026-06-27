using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MindArcade.Core.Llm
{
    /// <summary>
    /// Gemini API'sine (2.5 Flash) asenkron HTTP POST istekleri atan servis sınıfı.
    /// Yapılandırılmış JSON çıktı (Structured JSON Output) almayı garanti eder.
    /// </summary>
    public static class GeminiClient
    {
        private const string ApiUrlBase = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=";

        [Serializable]
        private class GeminiPart
        {
            public string text;
        }

        [Serializable]
        private class GeminiContent
        {
            public GeminiPart[] parts;
        }

        [Serializable]
        private class GeminiConfig
        {
            public string responseMimeType;
        }

        [Serializable]
        private class GeminiRequest
        {
            public GeminiContent[] contents;
            public GeminiContent systemInstruction;
            public GeminiConfig generationConfig;
        }

        // Response structures for parsing
        [Serializable]
        private class GeminiResponsePart
        {
            public string text;
        }

        [Serializable]
        private class GeminiResponseContent
        {
            public GeminiResponsePart[] parts;
        }

        [Serializable]
        private class GeminiCandidate
        {
            public GeminiResponseContent content;
        }

        [Serializable]
        private class GeminiResponse
        {
            public GeminiCandidate[] candidates;
        }

        /// <summary>
        /// Gemini API'ye istek atar ve JSON formatında zorluk parametrelerini döndürür.
        /// </summary>
        public static async Task<LlmDifficultyConfig> RequestDifficultyConfig(string apiKey, string promptText)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("Gemini API Anahtarı eksik veya boş!");
            }

            string url = ApiUrlBase + apiKey;

            // System Instruction ile LLM'in bir zorluk yöneticisi olduğunu ve JSON formatında yanıt vermesi gerektiğini kesinleştiriyoruz.
            string systemInstructionText = 
                "You are an expert game difficulty adjustment AI coordinator. " +
                "You must analyze the player's profile and current performance to decide the next difficulty parameters. " +
                "You must respond ONLY with a valid JSON object matching the following structure and DO NOT include markdown formatting (like ```json): " +
                "{" +
                "  \"suggestedLevel\": 1, // (int) The game level, usually 1 to 10" +
                "  \"speedMultiplier\": 1.0, // (float) Multiplier for movement/spawning speed, between 0.5 and 2.5" +
                "  \"decoyChance\": 0.2, // (float) Probability of decoy objects/obstacles, between 0.1 and 0.7" +
                "  \"timeLimit\": 15.0, // (float) Turn/game timer limit in seconds, between 5.0 and 30.0" +
                "  \"maxMistakesAllowed\": 5, // (int) Maximum mistakes allowed for FocusRunner before dropping level, usually between 3 and 7" +
                "  \"aiComment\": \"Message for player\" // (string) A short encouraging or challenging comment in Turkish" +
                "}";

            // İstek objesini hazırla
            var requestData = new GeminiRequest
            {
                contents = new[]
                {
                    new GeminiContent { parts = new[] { new GeminiPart { text = promptText } } }
                },
                systemInstruction = new GeminiContent
                {
                    parts = new[] { new GeminiPart { text = systemInstructionText } }
                },
                generationConfig = new GeminiConfig
                {
                    responseMimeType = "application/json"
                }
            };

            string jsonPayload = JsonUtility.ToJson(requestData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                // Asenkron olarak isteği gönder ve bekle
                var operation = webRequest.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"API Hatası: {webRequest.error} | Detay: {webRequest.downloadHandler?.text}");
                }

                string responseText = webRequest.downloadHandler.text;
                return ParseResponse(responseText);
            }
        }

        private static LlmDifficultyConfig ParseResponse(string rawJson)
        {
            try
            {
                // Gemini API yanıt yapısını çözüyoruz
                GeminiResponse geminiResponse = JsonUtility.FromJson<GeminiResponse>(rawJson);
                if (geminiResponse?.candidates != null && geminiResponse.candidates.Length > 0)
                {
                    string innerJson = geminiResponse.candidates[0].content.parts[0].text;
                    
                    // markdown bloklarını temizle (garantiye almak için)
                    innerJson = innerJson.Replace("```json", "").Replace("```", "").Trim();

                    // C# nesnesine dönüştür
                    LlmDifficultyConfig config = JsonUtility.FromJson<LlmDifficultyConfig>(innerJson);
                    if (config != null)
                    {
                        return config;
                    }
                }
                throw new Exception("Geçersiz API yanıt yapısı.");
            }
            catch (Exception e)
            {
                throw new Exception($"JSON ayrıştırılamadı. Hata: {e.Message} | Ham Yanıt: {rawJson}");
            }
        }
    }
}

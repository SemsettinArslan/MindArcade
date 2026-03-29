using UnityEngine;

namespace MemoryMatch
{
    [System.Serializable]
    public struct CardData
    {
        public int MatchId; // 8 farklı çiftten hangisine ait olduğu
        public bool HasText; // Kart yazılı mı, yazısız (renk bloku) mu?
        
        public string DisplayText; 
        public Color TextColor; 
        
        public Color BackgroundColor; // Arka plan (Yazısızsa ana renk olur)
    }
}

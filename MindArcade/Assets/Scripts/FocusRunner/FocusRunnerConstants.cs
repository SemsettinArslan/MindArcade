using UnityEngine;

namespace FocusRunner
{
    public enum GameRule
    {
        MatchColor,      // "RENGE GÖRE SEÇ" (Collect item where the Ink Color matches the target)
        MatchText,       // "YAZIYA GÖRE SEÇ" (Collect item where the Text matches the target)
        MatchInverse,    // "TERSİNİ SEÇ" (Collect item that DOES NOT match either text or color of the target)
        SpecificColor,   // "SADECE MAVİLERİ TOPLA" (Rule defines a specific color to look for)
        AvoidObstacles   // Default state / Distractor state
    }

    [System.Serializable]
    public struct StroopData
    {
        public string colorName;
        public Color colorValue;
    }

    public static class FocusRunnerConstants
    {
        public static readonly StroopData[] ColorPalette = new StroopData[]
        {
            new StroopData { colorName = "KIRMIZI", colorValue = Color.red },
            new StroopData { colorName = "MAVI", colorValue = Color.blue },
            new StroopData { colorName = "YESIL", colorValue = Color.green },
            new StroopData { colorName = "SARI", colorValue = Color.yellow },
            new StroopData { colorName = "MOR", colorValue = new Color(0.5f, 0, 0.5f) },
            new StroopData { colorName = "TURUNCU", colorValue = new Color(1f, 0.5f, 0) },
            new StroopData { colorName = "BEYAZ", colorValue = Color.white },
            new StroopData { colorName = "SIYAH", colorValue = Color.black }
        };

        public const float LaneWidth = 2.0f;
        public const int TotalLanes = 3;
        
        public static float GetLaneX(int laneIndex)
        {
            // laneIndex: 0 (Left), 1 (Middle), 2 (Right)
            return (laneIndex - 1) * LaneWidth;
        }

        public static StroopData GetRandomColor()
        {
            return ColorPalette[Random.Range(0, ColorPalette.Length)];
        }
    }
}

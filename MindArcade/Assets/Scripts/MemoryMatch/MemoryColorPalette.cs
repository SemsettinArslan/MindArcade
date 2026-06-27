using UnityEngine;

namespace MemoryMatch
{
    /// <summary>
    /// Renk havuzunu ScriptableObject olarak tutar.
    /// Inspector'dan düzenlenebilir, koddan bağımsız veri kaynağı.
    /// Unity'de Assets > Create > MindArcade > MemoryColorPalette ile oluşturulur.
    /// </summary>
    [CreateAssetMenu(fileName = "MemoryColorPalette", menuName = "MindArcade/Memory Color Palette")]
    public class MemoryColorPalette : ScriptableObject
    {
        [System.Serializable]
        public struct ColorEntry
        {
            public string colorName;
            public Color colorValue;
        }

        [SerializeField] private ColorEntry[] colors = new ColorEntry[]
        {
            new ColorEntry { colorName = "SIYAH",       colorValue = Color.black },
            new ColorEntry { colorName = "KOYU GRI",    colorValue = new Color(0.3f, 0.3f, 0.3f) },
            new ColorEntry { colorName = "ACIK GRI",    colorValue = new Color(0.7f, 0.7f, 0.7f) },
            new ColorEntry { colorName = "KIRMIZI",     colorValue = Color.red },
            new ColorEntry { colorName = "BORDO",       colorValue = new Color(0.5f, 0f, 0f) },
            new ColorEntry { colorName = "PEMBE",       colorValue = new Color(1f, 0.1f, 0.6f) },
            new ColorEntry { colorName = "ACIK PEMBE",  colorValue = new Color(1f, 0.7f, 0.8f) },
            new ColorEntry { colorName = "TURUNCU",     colorValue = new Color(1f, 0.5f, 0f) },
            new ColorEntry { colorName = "KAHVERENGI",  colorValue = new Color(0.4f, 0.2f, 0f) },
            new ColorEntry { colorName = "SARI",        colorValue = Color.yellow },
            new ColorEntry { colorName = "KREM",        colorValue = new Color(1f, 0.95f, 0.8f) },
            new ColorEntry { colorName = "YESIL",       colorValue = Color.green },
            new ColorEntry { colorName = "KOYU YESIL",  colorValue = new Color(0f, 0.4f, 0f) },
            new ColorEntry { colorName = "ACIK YESIL",  colorValue = new Color(0.6f, 1f, 0.2f) },
            new ColorEntry { colorName = "MAVI",        colorValue = Color.blue },
            new ColorEntry { colorName = "LACIVERT",    colorValue = new Color(0f, 0f, 0.4f) },
            new ColorEntry { colorName = "ACIK MAVI",   colorValue = new Color(0f, 0.8f, 1f) },
            new ColorEntry { colorName = "MOR",         colorValue = new Color(0.6f, 0f, 1f) },
            new ColorEntry { colorName = "KOYU MOR",    colorValue = new Color(0.3f, 0f, 0.3f) },
            new ColorEntry { colorName = "CAMGOBEGI",  colorValue = new Color(0f, 0.6f, 0.6f) },
            new ColorEntry { colorName = "TEN RENGI",   colorValue = new Color(1f, 0.8f, 0.6f) },
            new ColorEntry { colorName = "HAKI",        colorValue = new Color(0.5f, 0.5f, 0f) },
            new ColorEntry { colorName = "TURKUAZ",     colorValue = new Color(0.2f, 1f, 0.8f) },
            new ColorEntry { colorName = "BEJ",         colorValue = new Color(0.8f, 0.7f, 0.5f) }
        };

        public int Count => colors.Length;

        public string GetColorName(int index) => colors[index].colorName;
        public Color GetColorValue(int index) => colors[index].colorValue;
    }
}

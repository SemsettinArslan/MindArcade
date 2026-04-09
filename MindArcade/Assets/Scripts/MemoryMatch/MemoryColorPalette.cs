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
            new ColorEntry { colorName = "SİYAH",       colorValue = Color.black },
            new ColorEntry { colorName = "KOYU GRİ",    colorValue = new Color(0.3f, 0.3f, 0.3f) },
            new ColorEntry { colorName = "AÇIK GRİ",    colorValue = new Color(0.7f, 0.7f, 0.7f) },
            new ColorEntry { colorName = "KIRMIZI",     colorValue = Color.red },
            new ColorEntry { colorName = "BORDO",       colorValue = new Color(0.5f, 0f, 0f) },
            new ColorEntry { colorName = "PEMBE",       colorValue = new Color(1f, 0.1f, 0.6f) },
            new ColorEntry { colorName = "AÇIK PEMBE",  colorValue = new Color(1f, 0.7f, 0.8f) },
            new ColorEntry { colorName = "TURUNCU",     colorValue = new Color(1f, 0.5f, 0f) },
            new ColorEntry { colorName = "KAHVERENGİ",  colorValue = new Color(0.4f, 0.2f, 0f) },
            new ColorEntry { colorName = "SARI",        colorValue = Color.yellow },
            new ColorEntry { colorName = "KREM",        colorValue = new Color(1f, 0.95f, 0.8f) },
            new ColorEntry { colorName = "YEŞİL",       colorValue = Color.green },
            new ColorEntry { colorName = "KOYU YEŞİL",  colorValue = new Color(0f, 0.4f, 0f) },
            new ColorEntry { colorName = "AÇIK YEŞİL",  colorValue = new Color(0.6f, 1f, 0.2f) },
            new ColorEntry { colorName = "MAVİ",        colorValue = Color.blue },
            new ColorEntry { colorName = "LACİVERT",    colorValue = new Color(0f, 0f, 0.4f) },
            new ColorEntry { colorName = "AÇIK MAVİ",   colorValue = new Color(0f, 0.8f, 1f) },
            new ColorEntry { colorName = "MOR",         colorValue = new Color(0.6f, 0f, 1f) },
            new ColorEntry { colorName = "KOYU MOR",    colorValue = new Color(0.3f, 0f, 0.3f) },
            new ColorEntry { colorName = "CAMGÖBEĞİ",  colorValue = new Color(0f, 0.6f, 0.6f) },
            new ColorEntry { colorName = "TEN RENGİ",   colorValue = new Color(1f, 0.8f, 0.6f) },
            new ColorEntry { colorName = "HAKİ",        colorValue = new Color(0.5f, 0.5f, 0f) },
            new ColorEntry { colorName = "TURKUAZ",     colorValue = new Color(0.2f, 1f, 0.8f) },
            new ColorEntry { colorName = "BEJ",         colorValue = new Color(0.8f, 0.7f, 0.5f) }
        };

        public int Count => colors.Length;

        public string GetColorName(int index) => colors[index].colorName;
        public Color GetColorValue(int index) => colors[index].colorValue;
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace MemoryMatch
{
    /// <summary>
    /// SRP: Sadece deste (kart listesi) üretiminden sorumludur.
    /// Renk havuzundan rastgele seçim yapar ve oyun moduna göre 16 kart üretir.
    /// </summary>
    public class DeckGenerator
    {
        private readonly MemoryColorPalette _palette;

        public DeckGenerator(MemoryColorPalette palette)
        {
            _palette = palette;
        }

        /// <summary>
        /// Verilen oyun moduna göre belirtilen sayıda eşleşme içeren bir deste üretir.
        /// </summary>
        public List<CardData> GenerateDeck(StroopMatchMode mode, int matchCount)
        {
            // Palette boyutundan fazla eşleşme istenirse sınırla
            int safeMatchCount = Mathf.Min(matchCount, _palette.Count);
            List<int> selectedIndices = PickRandomIndices(safeMatchCount, _palette.Count);

            return mode == StroopMatchMode.TextVsColor
                ? GenerateTextVsColorDeck(selectedIndices)
                : GenerateTextVsTextDeck(selectedIndices);
        }

        /// <summary>
        /// Fisher-Yates shuffle algoritması ile desteyi karıştırır.
        /// </summary>
        public void ShuffleDeck(List<CardData> deck)
        {
            for (int i = 0; i < deck.Count; i++)
            {
                CardData temp = deck[i];
                int randomIndex = Random.Range(i, deck.Count);
                deck[i] = deck[randomIndex];
                deck[randomIndex] = temp;
            }
        }

        // ─── Private ────────────────────────────────────────────────

        private List<int> PickRandomIndices(int count, int maxRange)
        {
            List<int> indices = new List<int>();
            while (indices.Count < count)
            {
                int r = Random.Range(0, maxRange);
                if (!indices.Contains(r))
                    indices.Add(r);
            }
            return indices;
        }

        private List<CardData> GenerateTextVsColorDeck(List<int> selectedIndices)
        {
            int count = selectedIndices.Count;
            List<CardData> deck = new List<CardData>(count * 2);

            for (int i = 0; i < count; i++)
            {
                int colorIndex = selectedIndices[i];
                string colorName = _palette.GetColorName(colorIndex);
                Color actualColor = _palette.GetColorValue(colorIndex);

                // Sahte mürekkep rengi (Stroop etkisi için)
                int fakeIndex = RandomIndexExcluding(colorIndex, _palette.Count);
                Color fakeInkColor = _palette.GetColorValue(fakeIndex);

                Color safeBgColor = CalculateSafeBackground(fakeInkColor);

                // 1. KART → Metin kartı
                deck.Add(new CardData
                {
                    MatchId = colorIndex,
                    HasText = true,
                    DisplayText = colorName,
                    TextColor = fakeInkColor,
                    BackgroundColor = safeBgColor
                });

                // 2. KART → Düz renk bloku
                deck.Add(new CardData
                {
                    MatchId = colorIndex,
                    HasText = false,
                    DisplayText = "",
                    TextColor = Color.clear,
                    BackgroundColor = actualColor
                });
            }

            return deck;
        }

        private List<CardData> GenerateTextVsTextDeck(List<int> selectedIndices)
        {
            int count = selectedIndices.Count;
            List<CardData> deck = new List<CardData>(count * 2);

            for (int i = 0; i < count; i++)
            {
                int wordIndex = selectedIndices[i];
                string wordName = _palette.GetColorName(wordIndex);

                // İki kart için iki farklı sahte mürekkep rengi
                int fakeIndex1 = RandomIndexExcluding(wordIndex, _palette.Count);
                Color fakeInkColor1 = _palette.GetColorValue(fakeIndex1);

                int fakeIndex2 = RandomIndexExcluding(wordIndex, _palette.Count);
                Color fakeInkColor2 = _palette.GetColorValue(fakeIndex2);

                Color safeBg1 = CalculateSafeBackground(fakeInkColor1);
                Color safeBg2 = CalculateSafeBackground(fakeInkColor2);

                deck.Add(new CardData
                {
                    MatchId = wordIndex,
                    HasText = true,
                    DisplayText = wordName,
                    TextColor = fakeInkColor1,
                    BackgroundColor = safeBg1
                });

                deck.Add(new CardData
                {
                    MatchId = wordIndex,
                    HasText = true,
                    DisplayText = wordName,
                    TextColor = fakeInkColor2,
                    BackgroundColor = safeBg2
                });
            }

            return deck;
        }

        private int RandomIndexExcluding(int excludedIndex, int maxRange)
        {
            int index = Random.Range(0, maxRange);
            while (index == excludedIndex)
                index = Random.Range(0, maxRange);
            return index;
        }

        /// <summary>
        /// Mürekkep renginin parlaklığına göre okunabilir arka plan hesaplar.
        /// Açık metin → koyu arka plan, koyu metin → açık arka plan.
        /// </summary>
        private Color CalculateSafeBackground(Color inkColor)
        {
            float luminance = (0.299f * inkColor.r) + (0.587f * inkColor.g) + (0.114f * inkColor.b);
            return luminance > 0.55f
                ? new Color(0.15f, 0.15f, 0.15f)
                : new Color(0.98f, 0.98f, 0.98f);
        }
    }
}

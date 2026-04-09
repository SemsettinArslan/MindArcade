using System;
using System.Collections.Generic;
using UnityEngine;

namespace MemoryMatch
{
    /// <summary>
    /// SRP: Sadece tahta (board) yönetiminden sorumludur.
    /// Kartları oluşturur, temizler ve görsel olarak karıştırır.
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private Transform cardGridParent;
        [SerializeField] private Card cardPrefab;

        private readonly List<Card> _cards = new List<Card>(16);

        public IReadOnlyList<Card> Cards => _cards;

        /// <summary>
        /// Mevcut tahtayı temizler ve tüm kartları yok eder.
        /// </summary>
        public void ClearBoard()
        {
            foreach (Transform child in cardGridParent)
            {
                Destroy(child.gameObject);
            }
            _cards.Clear();
        }

        /// <summary>
        /// Verilen deste verisinden kartları instantiate ederek tahtayı kurar.
        /// </summary>
        public void SetupBoard(List<CardData> deck, Action<Card> onCardClicked)
        {
            foreach (var cardData in deck)
            {
                Card newCard = Instantiate(cardPrefab, cardGridParent);
                newCard.Initialize(cardData, onCardClicked);
                _cards.Add(newCard);
            }
        }

        /// <summary>
        /// Kartların ekrandaki pozisyonlarını karıştırır (ceza mekanizması).
        /// Unity UI Grid sistemi child index'lerine göre çalışır.
        /// </summary>
        public void ShuffleBoardVisuals()
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in cardGridParent)
            {
                children.Add(child);
            }

            for (int i = 0; i < children.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, children.Count);
                children[i].SetSiblingIndex(randomIndex);
            }
        }
    }
}

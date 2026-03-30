using UnityEngine;
using TMPro;

namespace FocusRunner
{
    public class StroopObject : MonoBehaviour
    {
        [SerializeField] private TMP_Text _textLabel;
        [SerializeField] private MeshRenderer _baseRenderer;
        
        public StroopData data;
        public Color inkColor;
        
        public bool IsObstacle; // If true, it's a "bad" object regardless of rule

        public void Initialize(StroopData data, Color inkColor, bool isObstacle = false)
        {
            this.data = data;
            this.inkColor = inkColor;
            this.IsObstacle = isObstacle;

            if (_textLabel != null)
            {
                _textLabel.text = data.colorName;
                _textLabel.color = inkColor;
            }

            if (_baseRenderer != null)
            {
                // Optionally change background color or material
                _baseRenderer.material.color = isObstacle ? Color.gray : Color.white;
            }
            
            gameObject.SetActive(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.OnCollect(this);
                }
                
                // Return to pool (to be implemented in LevelGenerator) or simple destroy for now
                gameObject.SetActive(false);
            }
        }
    }
}

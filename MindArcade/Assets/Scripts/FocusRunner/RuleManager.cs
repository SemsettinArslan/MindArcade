using UnityEngine;
using System;

namespace FocusRunner
{
    public class RuleManager : MonoBehaviour
    {
        public static RuleManager Instance { get; private set; }

        public event Action<GameRule, string> OnRuleChanged;

        [SerializeField] private GameRule _currentRule;
        [SerializeField] private StroopData _targetStroop; 

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void SetRandomRule()
        {
            Array values = Enum.GetValues(typeof(GameRule));
            // Sadece ilk 4 kuralı rastgele seç (SpecificColor dahil)
            _currentRule = (GameRule)values.GetValue(UnityEngine.Random.Range(0, 4));
            
            _targetStroop = FocusRunnerConstants.GetRandomColor();
            
            string ruleText = GetRuleDescription();
            OnRuleChanged?.Invoke(_currentRule, ruleText);
        }

        public string GetRuleDescription()
        {
            string hexColor = ColorUtility.ToHtmlStringRGB(_targetStroop.colorValue);
            switch (_currentRule)
            {
                case GameRule.MatchColor:
                    return $"DIKKAT :  <color=#{hexColor}>RENK</color> OLANLARI TOPLA";
                case GameRule.MatchText:
                    return $"DIKKAT :  <color=white>'{_targetStroop.colorName}'</color> YAZANLARI TOPLA";
                case GameRule.MatchInverse:
                    return "<color=red>DISINDAKILERI</color> TOPLA :  " + _targetStroop.colorName;
                case GameRule.SpecificColor:
                    return $"SADECE <color=#{hexColor}>{_targetStroop.colorName}</color> RENGI TOPLA";
                default:
                    return "ENGELLERDEN KACIN!";
            }
        }

        public bool ValidateMatch(StroopData collectedData, Color collectedInkColor)
        {
            switch (_currentRule)
            {
                case GameRule.MatchColor:
                    return collectedInkColor == _targetStroop.colorValue;
                
                case GameRule.MatchText:
                    return collectedData.colorName == _targetStroop.colorName;

                case GameRule.MatchInverse:
                    return collectedInkColor != _targetStroop.colorValue && collectedData.colorName != _targetStroop.colorName;

                case GameRule.SpecificColor:
                    return collectedInkColor == _targetStroop.colorValue;

                default:
                    return false;
            }
        }
    }
}

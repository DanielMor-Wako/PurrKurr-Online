using Code.Wakoz.PurrKurr.DataClasses.Enums;

namespace Code.Wakoz.PurrKurr.DataClasses.Characters
{

    public class DisplayedableStatData {

        public Definitions.CharacterDisplayableStat Type;
        public float Percentage;
        public string DisplayText;

        public DisplayedableStatData(Definitions.CharacterDisplayableStat type, float percentage, string displayText) {
            
            Type = type;
            Percentage = percentage;
            DisplayText = displayText;
        }
    }
}
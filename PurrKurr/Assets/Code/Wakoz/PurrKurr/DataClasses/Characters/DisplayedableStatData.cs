using Code.Wakoz.PurrKurr.DataClasses.Enums;

namespace Code.Wakoz.PurrKurr.DataClasses.Characters {
    public class DisplayedableStatData {

        public Definitions.CharacterDisplayableStat Type;
        public float Percentage;

        public DisplayedableStatData(Definitions.CharacterDisplayableStat type, float percentage) {
            
            Type = type;
            Percentage = percentage;
        }
    }
}
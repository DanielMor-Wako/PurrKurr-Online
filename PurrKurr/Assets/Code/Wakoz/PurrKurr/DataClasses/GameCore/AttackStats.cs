using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore {
    public class AttackStats {

        public int Damage;
        public Vector2 ForceDir;

        public AttackStats(int damage, Vector2 forceDirAction) {

            ForceDir = forceDirAction;
            Damage = damage;
        }
    }
}
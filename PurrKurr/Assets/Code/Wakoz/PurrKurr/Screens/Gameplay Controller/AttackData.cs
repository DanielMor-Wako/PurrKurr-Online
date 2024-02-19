using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Gameplay_Controller {
    public class AttackData {

        public int Damage;
        public Vector2 ForceDir;

        public AttackData(int damage, Vector2 forceDirAction) {

            ForceDir = forceDirAction;
            Damage = damage;
        }
    }
}
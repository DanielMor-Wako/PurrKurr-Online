using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Logic.GameFlow {

    public class GameplayLogic : SOData<GameplayLogicSO> {

        public GameplayLogic(string assetName) : base(assetName) { }

        protected override void Init() {
        }

        public LayerMask GetSolidSurfaces() => Data.GetSolidSurfaces();

        public LayerMask GetSurfaces() => Data.GetSurfaces();

        public LayerMask GetDamageables() => Data.GetDamageables();
        
    }

}
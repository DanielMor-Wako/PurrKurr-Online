using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Logic.GameFlow {
    public sealed class GameplayLogic : SOData<GameplayLogicSO> {

        public GameplayLogic(string assetName) : base(assetName) { }

        protected override void Init() {}


        public LayerMask GetSolidSurfaces() => Data.GetSolidSurfaces();

        public LayerMask GetSurfaces() => Data.GetSurfaces();

        public LayerMask GetDamageables() => Data.GetDamageables();
        
        
        public bool IsStateConsideredAsGrounded(Definitions.CharacterState specificState) =>
            Data.IsStateConsideredAsGrounded(specificState);

        public bool IsStateConsideredAsAerial(Definitions.CharacterState specificState) =>
            Data.IsStateConsideredAsAerial(specificState);

        public bool IsStateConsideredAsRunning(Definitions.CharacterState specificState, float magnitude) =>
            Data.IsStateConsideredAsRunning(specificState, magnitude);

        public bool IsVelocityConsideredAsRunning(float magnitude) => Data.IsVelocityConsideredAsRunning(magnitude);

    }

}
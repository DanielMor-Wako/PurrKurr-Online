using Code.Wakoz.PurrKurr.DataClasses.Effects;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using UnityEngine;
using static Code.Wakoz.PurrKurr.DataClasses.Enums.Definitions;

namespace Code.Wakoz.PurrKurr.Logic.GameFlow 
{

    public sealed class GameplayLogic : ScriptableAsset<GameplayLogicSO> 
    {
        public GameplayLogic(string assetName, string assetPrefix) : base(assetName, assetPrefix) { }

        protected override void Init() {}

        public LayerMask GetSurfaces() => Data.GetSurfaces();

        public LayerMask GetSolidSurfacesForProjectile() => Data.GetSolidSurfacesForProjectile();
        public LayerMask GetSolidSurfaces() => Data.GetSolidSurfaces();

        public LayerMask GetPlatformSurfaces() => Data.GetPlatformSurfaces();
        public LayerMask GetClingableSurfaces() => Data.GetClingableSurfaces();
        public LayerMask GetTraversableSurfaces() => Data.GetTraversableSurfaces();
        public LayerMask GetTraversableCrouchAreas() => Data.GetTraversableCrouch();

        public LayerMask GetDamageables() => Data.GetDamageables();
        
        public EffectData GetEffectData(Effect2DType effectType) => Data.GetEffectByType(effectType);

        public bool IsStateConsideredAsGrounded(Definitions.ObjectState specificState) =>
            Data.IsStateConsideredAsGrounded(specificState);

        public bool IsStateConsideredAsAerial(Definitions.ObjectState specificState) =>
            Data.IsStateConsideredAsAerial(specificState);

        public bool IsStateConsideredAsRunning(Definitions.ObjectState specificState, float magnitude) =>
            Data.IsStateConsideredAsRunning(specificState, magnitude);

        public bool IsVelocityConsideredAsRunning(float magnitude) => Data.IsVelocityConsideredAsRunning(magnitude);

    }

}
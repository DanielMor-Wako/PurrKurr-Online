using UnityEngine;
using static Code.Wakoz.PurrKurr.DataClasses.Enums.Definitions;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Anchors {

    [System.Serializable]
    public class EffectData {

        [Min(0)] public float DurationInSeconds;
        public Effect2DType EffectType;
        public ParticleSystem Effect;
        public bool TrackPosition;
    }

}
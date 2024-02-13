using Code.Wakoz.PurrKurr.DataClasses.Effects;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Effects {

    [Serializable]
    public class EffectsData {

        [SerializeField] private List<EffectData> _data;

        public EffectData GetDataByType(Definitions.Effect2DType effectType) {

            if (_data.Count < 1) {
                return null;
            }

            foreach (var data in _data) {
                if (data.EffectType == effectType) {
                    return data;
                }
            }

            return null;
        }

    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.Utils.GraphicUtils.Effects {
    public class ParticleObjectKiller : MonoBehaviour {
        [SerializeField] private List<ParticleSystem> _particles;

        private void OnEnable() {
            foreach (var particle in _particles) {
                var masterFkr = particle.main;
                masterFkr.stopAction = ParticleSystemStopAction.Callback;
            }
        }

        private void OnParticleSystemStopped() {
            gameObject.SetActive(false);
        }
    
    }
}
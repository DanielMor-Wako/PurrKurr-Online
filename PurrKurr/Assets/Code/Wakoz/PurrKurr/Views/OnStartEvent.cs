using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Code.Wakoz.PurrKurr.Views {
        
    public class OnStartEvent : View {
        
        public UnityEvent OnStart;

        [Tooltip("Delay before action, in Milliseconds")]
        [SerializeField][Range(0, 10000)]
        private float _preActionDelay;

        private Coroutine _countDownCoroutine;

        protected override void ModelChanged() {}

        private void Awake()
        {
            if (OnStart == null || OnStart.GetPersistentEventCount() == 0)
            {
                Debug.LogWarning($"OnStartEvent component exist but has not events {gameObject.name}");
                return;
            }

            if (_preActionDelay == 0) 
            {
                OnStart?.Invoke();
                return;
            }

            StartCountdown();
        }

        private void StartCountdown()
        {
            if (_countDownCoroutine != null)
            {
                StopCoroutine(_countDownCoroutine);
            }
            _countDownCoroutine = StartCoroutine(InvokeOnStartWithDelay());
        }

        private void StopCountdown()
        {
            if (_countDownCoroutine != null)
            {
                StopCoroutine(_countDownCoroutine);
                _countDownCoroutine = null;
            }
        }

        private IEnumerator InvokeOnStartWithDelay()
        {
            var elapsedTime = 0f;
            var seconds = _preActionDelay / 1000;

            while (true && elapsedTime <= seconds)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            OnStart?.Invoke();

            StopCountdown();
        }
    }

}
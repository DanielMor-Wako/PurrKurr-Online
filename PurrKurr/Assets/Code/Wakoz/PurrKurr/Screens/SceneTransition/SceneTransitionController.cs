using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Wakoz.PurrKurr.Screens.SceneTransition
{
    public class SceneTransitionController : SingleController
    {
        [SerializeField] private SceneTransitionView _view;

        private static int _previoudBuildIndex = -1;

        protected override void Clean()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        protected override Task Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            return Task.CompletedTask;
        }

        public void LoadSceneByIndex(int index, string newTitle = "", Action onEndAnimation = null) {
            if (index < 0 || index >= SceneManager.sceneCountInBuildSettings) {
                Debug.LogError("Invalid build index: " + index);
            }

            DoAnimAndLoadScene(index, onEndAnimation, $"{newTitle}");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var isFirstScene = _previoudBuildIndex == -1;

            Debug.Log($"Scene '{scene.name}' Loaded <-- index ({scene.buildIndex}) prev ({_previoudBuildIndex})");

            _previoudBuildIndex = scene.buildIndex;

            if (isFirstScene) return;

            _view.EndTransition();
        }

        private void DoAnimAndLoadScene(int index, Action onEndAnimation = null, string newTitle = "")
        {
            _view.StartTransition(() =>
            {
                SceneManager.LoadScene(index);
                onEndAnimation?.Invoke();
            },
            newTitle);
        }

        #region Inspector Test
        private void TestSceneTransition(float durationInSeconds)
        {
            Action onEndAnimation = () =>
            {
                _view.EndTransition();
            };

            _view.StartTransition(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(durationInSeconds));
                onEndAnimation?.Invoke();
            },
            $"{durationInSeconds} Sec Transition Test");
        }

        [ContextMenu("Test - Scene Transition (0 seconds)")]
        public void TestSceneTransition5Seconds() 
            => TestSceneTransition(UnityEngine.Random.Range(0, 10));

        public void PretendLoad(float durationInSeconds, string title = "", Action onEndAction = null)
        {
            Action onEndAnimation = () =>
            {
                _view.EndTransition();
                onEndAction?.Invoke();
            };

            _view.StartTransition(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(durationInSeconds));
                onEndAnimation?.Invoke();
            }, title);
        }
        #endregion
    }
}

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Wakoz.PurrKurr.Screens.SceneTransition
{
    public class SceneTransitionController : SingleController
    {
        [SerializeField] private SceneTransitionView _view;

        protected override void Clean()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        protected override Task Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            return Task.CompletedTask;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _view.SetTitle();
            _view.EndTransition();
        }

        public Task LoadSceneByIndex(int index, Action onEndAnimation = null)
        {
            if (index < 0 || index >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError("Invalid build index: " + index);
                return Task.CompletedTask;
            }

            _view.SetTitle("Loading");
            DoAnimAndLoadScene(index, onEndAnimation);
            return Task.CompletedTask;
        }

        private void DoAnimAndLoadScene(int index, Action onEndAnimation = null)
        {
            _view.StartTransition(() =>
            {
                SceneManager.LoadScene(index);
                onEndAnimation?.Invoke();
            });
        }

        [ContextMenu("Test - Scene Transition (5 seconds)")]
        public Task TestSceneTransition()
        {
            Action onEndAnimation = () => 
            {
                _view.SetTitle();
                _view.EndTransition();
            };

            _view.SetTitle("Loading");
            _view.StartTransition(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                onEndAnimation?.Invoke();
            });

            return Task.CompletedTask;
        }

    }
}

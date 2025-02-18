using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Objectives
{
    public class ObjectivesView : View<ObjectivesModel>
    {
        [SerializeField] private ObjectiveView _objectiveViewPrefab; 
        [SerializeField] private Transform _parentContainer; 
        [SerializeField] private List<ObjectiveView> _objectives = new();

        private List<ObjectiveModel> _models = new();
        private Queue<ObjectiveView> _pool = new(); // Pool for reusing ObjectiveView instances

        protected override void ModelReplaced()
        {
            _models.Clear();

            var total = Model.Objectives.Count;

            // Ensure the Objectives list matches the number of models
            while (_objectives.Count < total)
            {
                _objectives.Add(GetOrCreateObjectiveView());
            }

            for (var i = 0; i < total; i++)
            {
                var data = Model.Objectives[i];
                var objectiveView = _objectives[i];

                if (data == null)
                {
                    // Deactivate the ObjectiveView if no data is available
                    if (objectiveView != null)
                    {
                        ReturnToPool(objectiveView); // Return to pool for reuse
                        _objectives[i] = null; // Clear the reference
                    }
                    continue;
                }

                if (objectiveView == null)
                {
                    // Get a new or reused ObjectiveView from the pool
                    objectiveView = GetOrCreateObjectiveView();
                    _objectives[i] = objectiveView;
                }

                // Set up the ObjectiveView with the model
                var model = new ObjectiveModel(data.InterfaceData);
                objectiveView.SetModel(model);
                objectiveView.gameObject.SetActive(true);
                objectiveView.Fader.StartTransition(data.InterfaceData.IsComplete() ? 0.5f : 1);
                _models.Add(model);
            }

            // Deactivate any extra ObjectiveViews beyond the total count
            for (var i = total; i < _objectives.Count; i++)
            {
                if (_objectives[i] != null)
                {
                    _objectives[i].gameObject.SetActive(false);
                    ReturnToPool(_objectives[i]); // Return to pool for reuse
                    _objectives[i] = null; // Clear the reference
                }
            }
        }

        protected override void ModelChanged()
        {
            for (var i = 0; i < _objectives.Count; i++)
            {
                var viewItem = _objectives[i];
                if (viewItem == null)
                    continue;

                if (viewItem.transform.GetSiblingIndex() != i)
                {
                    viewItem.transform.SetSiblingIndex(i);
                }
            }

            for (var i = 0; i < _models.Count; i++)
            {
                _models[i].UpdateItem();
            }
        }

        private ObjectiveView GetOrCreateObjectiveView()
        {
            // Check if there's an available instance in the pool
            if (_pool.Count > 0)
            {
                var reusedView = _pool.Dequeue();
                SetInitialObjectiveState(reusedView);
                return reusedView;
            }

            // Instantiate a new ObjectiveView if the pool is empty
            var newView = Instantiate(_objectiveViewPrefab, _parentContainer);

            newView.transform.SetParent(_parentContainer, false);
            SetInitialObjectiveState(newView);

            return newView;
        }

        private void SetInitialObjectiveState(ObjectiveView objectiveView)
        {
            objectiveView.Fader.CanvasTarget.alpha = 0;
        }

        private void ReturnToPool(ObjectiveView view)
        {
            view.gameObject.SetActive(false); // Deactivate the view
            _pool.Enqueue(view); // Add it back to the pool
        }
    }
}
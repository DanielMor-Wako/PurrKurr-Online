using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Objectives
{
    public class ObjectivesView : View<ObjectivesModel>
    {
        [SerializeField] private ObjectiveView _objectiveViewPrefab; 
        [SerializeField] private Transform _parentContainer; 
        [SerializeField] private List<ObjectiveView> _objectives = new();

        private List<ObjectiveModel> _models = new();
        private Queue<ObjectiveView> _pool = new();

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
                        ReturnToPool(objectiveView);
                        _objectives[i] = null;
                    }
                    continue;
                }

                if (objectiveView == null)
                {
                    objectiveView = GetOrCreateObjectiveView();
                    _objectives[i] = objectiveView;
                }

                // Set up the ObjectiveView with the model
                var model = new ObjectiveModel(data.InterfaceData);
                objectiveView.SetModel(model);
                objectiveView.gameObject.SetActive(true);
                objectiveView.transform.SetSiblingIndex(i);
                objectiveView.Fader.CanvasTarget.alpha = 0;
                objectiveView.Fader.StartTransition(data.InterfaceData.IsComplete() ? 0.25f : 1);
                _models.Add(model);
            }

            // Deactivate any extra ObjectiveViews beyond the total count
            for (var i = total; i < _objectives.Count; i++)
            {
                if (_objectives[i] != null)
                {
                    _objectives[i].gameObject.SetActive(false);
                    ReturnToPool(_objectives[i]);
                    _objectives[i] = null;
                }
            }
        }

        protected override void ModelChanged()
        {
            for (var i = 0; i < Model.Objectives.Count; i++)
            {
                var objectiveModel = Model.Objectives[i];

                var model = _models.FirstOrDefault(o => o != null && o.InterfaceData.GetUniqueId() == objectiveModel.InterfaceData.GetUniqueId());
                if (model == null)
                    continue;

                // Find the corresponding viewItem in _objectives with the same UniqueId
                var viewItem = _objectives.FirstOrDefault(o => o != null && o.IsViewOf(model));
                if (viewItem == null)
                    continue;

                Action action = null;

                // Set the viewItem's sibling index to match the model's index
                var itemIndexInScene = viewItem.transform.GetSiblingIndex();
                if (itemIndexInScene != i)
                {
                    var newSiblingIndex = i;
                    action = () => {
                        //viewItem.transform.SetSiblingIndex(newSiblingIndex);
                        viewItem.Fader.StartTransition(objectiveModel.InterfaceData.IsComplete() ? 0.01f : 1f,
                            () =>
                            {
                                viewItem.transform.SetSiblingIndex(newSiblingIndex);
                                viewItem.Fader.StartTransition(objectiveModel.InterfaceData.IsComplete() ? 0.25f : 1);
                            });
                    };
                }

                // Update the model item
                viewItem.Fader.StartTransition(objectiveModel.InterfaceData.IsComplete() ? 0.25f : 1, action);
                model.UpdateItem();
            }

            /*for (var i = 0; i < _objectives.Count; i++)
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
            }*/
        }

        private ObjectiveView GetOrCreateObjectiveView()
        {
            if (_pool.Count > 0)
            {
                var reusedView = _pool.Dequeue();
                SetInitialObjectiveState(reusedView);
                return reusedView;
            }

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
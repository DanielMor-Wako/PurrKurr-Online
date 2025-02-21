using Code.Wakoz.PurrKurr.Views;
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
        [SerializeField] private CanvasGroupFaderView _titleFader;
        private List<ObjectiveView> _objectiveViews = new();

        private List<ObjectiveModel> _models = new();
        private Queue<ObjectiveView> _pool = new();

        protected override void ModelReplaced()
        {
            var totalObjectives = Model.Objectives.Count;
            SetTitleFader(totalObjectives);

            _models.Clear();

            // Ensure the Objectives list matches the number of models
            while (_objectiveViews.Count < totalObjectives)
            {
                _objectiveViews.Add(GetOrCreateObjectiveView());
            }

            for (var i = 0; i < totalObjectives; i++)
            {
                var data = Model.Objectives[i];
                var objectiveView = _objectiveViews[i];

                if (data == null)
                {
                    // Deactivate the ObjectiveView if no data is available
                    if (objectiveView != null)
                    {
                        ReturnToPool(objectiveView);
                        _objectiveViews[i] = null;
                    }
                    continue;
                }

                if (objectiveView == null)
                {
                    objectiveView = GetOrCreateObjectiveView();
                    _objectiveViews[i] = objectiveView;
                }

                // Set up the ObjectiveView with the model
                var model = new ObjectiveModel(data.InterfaceData);
                objectiveView.SetModel(model);
                objectiveView.gameObject.SetActive(true);
                objectiveView.transform.SetSiblingIndex(i);
                objectiveView.Fader.CanvasTarget.alpha = 0;
                objectiveView.Fader.StartTransition(data.InterfaceData.IsComplete() ? 0.25f : 1);
                objectiveView.ImageFiller.ImageTarget.fillAmount = (float)data.InterfaceData.GetCurrentQuantity() / data.InterfaceData.GetRequiredQuantity();
                objectiveView.ImageFiller.StartTransition(objectiveView.ImageFiller.ImageTarget.fillAmount);
                objectiveView.ImageScaler.EndTransition();
                _models.Add(model);
            }

            // Deactivate any extra ObjectiveViews beyond the total count
            for (var i = totalObjectives; i < _objectiveViews.Count; i++)
            {
                if (_objectiveViews[i] != null)
                {
                    _objectiveViews[i].gameObject.SetActive(false);
                    ReturnToPool(_objectiveViews[i]);
                    _objectiveViews[i] = null;
                }
            }
        }

        private void SetTitleFader(int totalObjectives)
        {
            _titleFader.CanvasTarget.alpha = 0;
            _titleFader.StartTransition(totalObjectives < 1 ? 0.25f : 1);
        }

        protected override void ModelChanged()
        {
            var totalObjectives = Model.Objectives.Count;

            for (var i = 0; i < totalObjectives; i++)
            {
                var objectiveModel = Model.Objectives[i];

                var model = _models.FirstOrDefault(o => o != null && o.InterfaceData.GetUniqueId() == objectiveModel.InterfaceData.GetUniqueId());
                if (model == null)
                    continue;

                // Find the corresponding viewItem in _objectives with the same UniqueId
                var viewItem = _objectiveViews.FirstOrDefault(o => o != null && o.IsViewOf(model));
                if (viewItem == null)
                    continue;

                Action siblingAction = null;

                var isCompleteAndImageFillIsFull = objectiveModel.InterfaceData.IsComplete() && viewItem.ImageFiller.ImageTarget.fillAmount == 1;

                Action fillAction = () =>
                {
                    if (isCompleteAndImageFillIsFull)
                    {
                        model.UpdateItem();
                        viewItem.Fader.StartTransition(0.25f);
                    }
                    else
                    {
                        viewItem.ImageScaler.StartTransition();
                        var fillPercent = Mathf.Clamp01((float)objectiveModel.InterfaceData.GetCurrentQuantity() / objectiveModel.InterfaceData.GetRequiredQuantity());
                        viewItem.ImageFiller.StartTransition(fillPercent, () =>
                        {
                            model.UpdateItem();
                            viewItem.ImageScaler.EndTransition();
                            viewItem.Fader.StartTransition(objectiveModel.InterfaceData.IsComplete() ? 0.25f : 1f, siblingAction);
                        });
                    }
                };

                // Set the viewItem's sibling index to match the model's index
                var itemIndexInScene = viewItem.transform.GetSiblingIndex();
                if (itemIndexInScene != i)
                {
                    var newSiblingIndex = i;
                    siblingAction = () => {
                        if (objectiveModel.InterfaceData.IsComplete())
                        {
                            viewItem.transform.SetSiblingIndex(newSiblingIndex);
                        }
                        viewItem.Fader.StartTransition(objectiveModel.InterfaceData.IsComplete() ? 0.25f : 1);
                    };
                }

                // Update the model item
                viewItem.Fader.StartTransition(isCompleteAndImageFillIsFull ? 0.25f : 1, fillAction);
            }

            /* Simply refreshes all items and do not set siblingIndex
            for (var i = 0; i < _models.Count; i++)
            {
                _models[i].UpdateItem();
            }
            */
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
using Code.Wakoz.PurrKurr.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Objectives
{
    public class ObjectivesView : View<ObjectivesModel>
    {
        public event Action OnClickedToggleState;

        [SerializeField] private ObjectiveView _objectiveViewPrefab;
        [SerializeField] private Transform _parentContainer;
        [SerializeField] private CanvasGroupFaderView _titleFader;
        [SerializeField] private ImageColorChangerView _titleColor;
        [SerializeField] private MultiStateView _arrowIconState;
        [SerializeField] private RectTransformScalerView _arrowIconScaler;

        private List<ObjectiveView> _objectiveViews = new();

        private List<ObjectiveModel> _models = new();
        private Queue<ObjectiveView> _pool = new();

        private const float FullAlpha = 1f;
        private const float CompletedAlpha = 0.75f;
        private const float HiddenAlpha = 0f;

        private bool _isHidingCompletedObjectives = true;

        /// <summary>
        /// Inspector exposed function for toggling hiding
        /// </summary>
        public void ClickedToggleState() 
            => OnClickedToggleState?.Invoke();

        /// <summary>
        /// Toggle hiding completed objectives
        /// </summary>
        public void ToggleState()
        {
            _isHidingCompletedObjectives = !_isHidingCompletedObjectives;

            ProcessItemsUpdate();
            SetArrowIconState();
            SetArrowIconScale();
            TitleColorTransition();
        }

        private void UpdateAllFaders()
        {
            for (var i = 0; i < _models.Count; i++)
            {
                var model = _models[i];

                if (model == null || !model.InterfaceData.IsComplete())
                    continue;

                var viewItem = _objectiveViews.FirstOrDefault(o => o != null && o.IsViewOf(model));

                if (viewItem == null)
                    continue;

                viewItem.Fader.StartTransition(model.InterfaceData.IsComplete() ? GetCompletedAlpha() : FullAlpha);
            }
        }

        private void SetArrowIconState()
        {
            if (_arrowIconState == null)
                return;

            var canDisplayArrowIcon = HasAnyCompleted();
            var newState = canDisplayArrowIcon ? Convert.ToInt32(!_isHidingCompletedObjectives) : 2;

            if (newState == _arrowIconState.CurrentState)
                return;

            _arrowIconState.ChangeState(newState);
        }

        private void SetArrowIconScale()
        {
            var newState = _arrowIconState != null ? _arrowIconState.CurrentState :
                HasAnyCompleted() ? Convert.ToInt32(!_isHidingCompletedObjectives) : 2;

            if (_arrowIconScaler != null && newState == 2)
                return;

            _arrowIconScaler.TargetRectTransform.localScale = _arrowIconScaler.MaxScale;
            _arrowIconScaler.EndTransition();
        }

        private bool HasAnyCompleted()
        {
            if (_models.Count == 0)
                return false;

            return _models.FirstOrDefault(o => o.InterfaceData.IsComplete()) != null;
        }

        /// <summary>
        /// Refreshes all models and updates alpha of completed items
        /// </summary>
        private void UpdateAll()
        {
            for (var i = 0; i < _models.Count; i++)
            {
                var model = _models[i];

                if (model == null) 
                    continue;

                model.UpdateItem();
            }
        }

        /// <summary>
        /// Ensure the Objectives list matches the number of models , then create or get an viewitem
        /// Deactivate the ObjectiveView if no data is available and return to pool
        /// Finally returning any extra ObjectiveViews beyond the total count, back to the pool
        /// </summary>
        protected override void ModelReplaced()
        {
            var totalObjectives = Model.Objectives.Count;
            SetTitleFader(totalObjectives);

            _models.Clear();

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

                var model = new ObjectiveModel(data.InterfaceData);
                SetObjective(objectiveView, model, i);
                _models.Add(model);
            }

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

        protected override void ModelChanged()
        {
            ProcessItemsUpdate();
            SetArrowIconState();
        }

        private void SetTitleFader(int totalObjectives)
        {
            if (_titleFader == null)
                return;

            _titleFader.CanvasTarget.alpha = 0;
            _titleFader.StartTransition(totalObjectives < 1 ? HiddenAlpha : FullAlpha);
        }

        private void TitleColorTransition()
        {
            if (_titleColor == null)
                return;

            _titleColor.TargetImage.color = Color.white;
            _titleColor.EndTransition();
        }
        
        private ObjectiveView GetOrCreateObjectiveView()
        {
            if (_pool.Count > 0)
            {
                var reusedView = _pool.Dequeue();
                reusedView.gameObject.SetActive(true);
                return reusedView;
            }

            var newView = Instantiate(_objectiveViewPrefab, _parentContainer);

            newView.transform.SetParent(_parentContainer, false);
            newView.gameObject.SetActive(true);

            return newView;
        }

        /// <summary>
        /// Sets view with model and updates view state by the model
        /// </summary>
        /// <param name="objectiveView"></param>
        /// <param name="model"></param>
        /// <param name="sinlingIndex"></param>
        private void SetObjective(ObjectiveView objectiveView, ObjectiveModel model, int sinlingIndex)
        {
            objectiveView.SetModel(model);

            objectiveView.transform.SetSiblingIndex(sinlingIndex);

            objectiveView.Fader.CanvasTarget.alpha = 0;
            objectiveView.Fader.StartTransition(model.InterfaceData.IsComplete() ? GetCompletedAlpha() : FullAlpha);

            objectiveView.ImageFiller.ImageTarget.fillAmount = (float)model.InterfaceData.GetCurrentQuantity() / model.InterfaceData.GetRequiredQuantity();
            objectiveView.ImageFiller.StartTransition(objectiveView.ImageFiller.ImageTarget.fillAmount);

            objectiveView.ImageScaler.EndTransition();
        }

        private void ReturnToPool(ObjectiveView view)
        {
            view.gameObject.SetActive(false); // Deactivate the view
            _pool.Enqueue(view); // Add it back to the pool
        }

        /// <summary>
        /// Updates each viewItem in _objectives with the same UniqueId by new items order
        /// </summary>
        private void ProcessItemsUpdate()
        {
            var totalObjectives = Model.Objectives.Count;

            for (var i = 0; i < totalObjectives; i++)
            {
                var objectiveModel = Model.Objectives[i];

                // todo: use dictionary for faster lookup table
                var model = _models.FirstOrDefault(o => o != null && o.InterfaceData.GetUniqueId() == objectiveModel.InterfaceData.GetUniqueId());
                if (model == null)
                {
                    continue;
                }

                var viewItem = _objectiveViews.FirstOrDefault(o => o != null && o.IsViewOf(model));
                if (viewItem == null)
                {
                    continue;
                }

                Action siblingAction = null;

                var fillAmount = viewItem.ImageFiller.ImageTarget.fillAmount;
                var isCompleteAndImageFillIsFull = objectiveModel.InterfaceData.IsComplete() && fillAmount == 1;
                var hasFillAmountDifference = fillAmount != Mathf.Clamp01((float)objectiveModel.InterfaceData.GetCurrentQuantity() / objectiveModel.InterfaceData.GetRequiredQuantity());

                Action fillAction = () =>
                {
                    if (!hasFillAmountDifference)
                    {
                        model.UpdateItem();
                        viewItem.Fader.StartTransition(objectiveModel.InterfaceData.IsComplete() ? GetCompletedAlpha() : FullAlpha);
                    }
                    else
                    {
                        // Set the viewItem's sibling index to match the model's index
                        var itemIndexInScene = viewItem.transform.GetSiblingIndex();
                        Debug.Log($"Item sibling {itemIndexInScene} with new order as {i}");
                        if (itemIndexInScene != i)
                        {
                            var newSiblingIndex = i;
                            siblingAction = () =>
                            {
                                viewItem.transform.SetSiblingIndex(newSiblingIndex);
                                viewItem.Fader.StartTransition(objectiveModel.InterfaceData.IsComplete() ? GetCompletedAlpha() : 1);
                            };
                        }

                        viewItem.ImageScaler.StartTransition();
                        var fillPercent = Mathf.Clamp01((float)objectiveModel.InterfaceData.GetCurrentQuantity() / objectiveModel.InterfaceData.GetRequiredQuantity());
                        viewItem.ImageFiller.StartTransition(fillPercent, () =>
                        {
                            model.UpdateItem();
                            viewItem.ImageScaler.EndTransition();
                            viewItem.Fader.StartTransition(objectiveModel.InterfaceData.IsComplete() ? GetCompletedAlpha() : 1f, siblingAction);
                        });
                    }
                };

                viewItem.Fader.StartTransition(isCompleteAndImageFillIsFull ? GetCompletedAlpha() : 1, fillAction);
            }
        }

        private float GetCompletedAlpha() 
            => _isHidingCompletedObjectives ? HiddenAlpha : CompletedAlpha;

    }
}
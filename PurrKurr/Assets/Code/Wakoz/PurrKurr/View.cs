using System;
using UnityEngine;

namespace Code.Wakoz.PurrKurr {
    public abstract class View : MonoBehaviour {

        protected IModel Model { get; private set; }
        private bool _initialized = false;
        private bool _destroyed = false;
        public void SetModel(IModel model) {
            try {
                if(model == Model) { return; }
                if(Model != null) {
                    Model.OnChange -= ModelHasChanged;
                }
                Model = model;
                if(!_initialized && !_destroyed) {
                    _initialized = true;
                    InternalInitialized();
                }
                ModelReplaced();
                if(Model != null) {
                    Model.OnChange += ModelHasChanged;
                    ModelHasChanged();
                }
            } catch(Exception e) {
                Debug.LogException(e);
            }
        }

        public void SetModel(View otherView) {
            SetModel(otherView.Model);
        }

        private void OnDestroy() {
            if(Model != null) {
                Model.OnChange -= ModelHasChanged;
            }
            if(_initialized) {
                InternalCleaned();
            }
            _destroyed = true;
        }
        
        private void ModelHasChanged() {
            try {
                ModelChanged();
            } catch(Exception e) {
                Debug.LogException(e);
            }
        }

        private void InternalInitialized() {
            // Debug.Log(GetType().Name + " initialized");
            Initialize();
        }
        private void InternalCleaned() {
            Clean();
        }
        /// <summary>
        /// Will be called when a model is internally changed, only when a model is available, so inside, Model is never null
        /// </summary>
        protected abstract void ModelChanged();
        /// <summary>
        /// Called when a model is set, but not if the new model is the same as the old model, Model can be null when the method is called
        /// </summary>
        protected virtual void ModelReplaced() { }
        /// <summary>
        /// Called once, upon setting a model for the first time
        /// </summary>
        protected virtual void Initialize() { }
        /// <summary>
        /// Called when the view is destroyed if the view was initialized
        /// </summary>
        protected virtual void Clean() { }
        protected T CastModel<T>() where T : IModel {
            return (T)Model;
        }
        public bool Has(Model model) {
            return Model == model;
        }
        public T FuckMe<T>() {
            return (T)Model;
        }
        public bool IsViewOf(Model model) {
            return Model == model;
        }
    }
    public abstract class View<T> : View where T : class, IModel {
        protected new T Model { get { return base.Model as T; } }
        [Obsolete("Calling SetModel on an incorrect type", true)]
        public new void SetModel(IModel model) {
            base.SetModel(model);
        }
        public void SetModel(T model) {
            base.SetModel(model);
        }
    }
}

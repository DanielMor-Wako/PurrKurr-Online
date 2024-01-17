using System;

namespace Code.Wakoz.PurrKurr {
    public abstract class Model : IModel {

        public event Action OnChange;

        protected void Changed() {
            OnChange?.Invoke();
        }
        
    }
}
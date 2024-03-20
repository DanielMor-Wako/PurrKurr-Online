using System;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Anchors {

    [DefaultExecutionOrder(14)]
    public class AnchorHandler : MonoBehaviour {

        public static event Action<AnchorHandler> OnAnchorAdded;

        public static event Action<AnchorHandler> OnAnchorRemoved;

        public event Action<AnchorHandler, Vector3, Transform> OnAnchorChanged;

        public void ModifyAnchor(Vector3 Pos, Transform NewParent) => OnAnchorChanged(this, Pos, NewParent);

        private void OnEnable() => OnAnchorAdded(this);

        private void OnDisable() => OnAnchorRemoved(this);
    }
}

using Code.Wakoz.PurrKurr.Screens.InteractableObjectsPool;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Anchors {

    [DefaultExecutionOrder(12)]
    public class AnchorsController : SingleController {

        [SerializeField] private DynamicAnchor AnchorPrefab;

        private Dictionary<AnchorHandler, DynamicAnchor> anchors = new Dictionary<AnchorHandler, DynamicAnchor>();

        private GenericObjectPool<DynamicAnchor> _pool;

        protected override void Clean()
        {
            AnchorHandler.OnAnchorAdded -= AddAnchor;
            AnchorHandler.OnAnchorRemoved -= RemoveAnchor;
        }

        protected override Task Initialize()
        {
            AnchorHandler.OnAnchorAdded += AddAnchor;
            AnchorHandler.OnAnchorRemoved += RemoveAnchor;

            _pool = new GenericObjectPool<DynamicAnchor>(AnchorPrefab, transform, 2, 100);

            return Task.CompletedTask;
        }

        public Rigidbody2D GetAnchorRigidbody(AnchorHandler opponentAnchor)
        {
            if (anchors.TryGetValue(opponentAnchor, out DynamicAnchor anchor))
            {
                return anchor.GetComponent<Rigidbody2D>();
            }

            // If no anchor exists, create a new one from the pool
            anchor = _pool.GetObjectFromPool();
            if (anchor != null)
            {
                anchors[opponentAnchor] = anchor;
            }
            return anchor?.GetComponent<Rigidbody2D>();

        }
    
        private void AddAnchor(AnchorHandler opponentAnchor) 
        {
            if (!anchors.ContainsKey(opponentAnchor)) {

                var anchor = _pool.GetObjectFromPool();
                anchor.name = "dynAnchor_" + opponentAnchor.name;
                anchors.Add(opponentAnchor, anchor);

                opponentAnchor.OnAnchorChanged += HandleAnchorChange;
            }
        }

        private void RemoveAnchor(AnchorHandler opponentAnchor)
        {
            if (anchors.TryGetValue(opponentAnchor, out DynamicAnchor anchor))
            {
                opponentAnchor.OnAnchorChanged -= HandleAnchorChange;

                _pool.ReleaseObjectToPool(anchor);
                anchors.Remove(opponentAnchor);
            }
        }

        private void HandleAnchorChange(AnchorHandler anchor, Vector3 position, Transform anchorParent)
        {
            if (!anchors.TryGetValue(anchor, out var dynamicAnchor))
                return;

            dynamicAnchor.transform.position = position;

            if (anchorParent != null) {

                AssignAnchor(anchor, anchorParent);

            } else {

                DissociateAnchor(anchor);
            }
            
        }

        private void AssignAnchor(AnchorHandler anchor, Transform anchorParent)
        {
            anchors[anchor].transform.parent = anchorParent;
        }
        
        private void DissociateAnchor(AnchorHandler anchor)
        {
            anchors[anchor].transform.parent = transform;
        }

    }
}

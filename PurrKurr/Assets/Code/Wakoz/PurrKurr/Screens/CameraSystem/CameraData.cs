using UnityEngine;
using System.Collections.Generic;

namespace Code.Wakoz.PurrKurr.Screens.CameraSystem
{
    [System.Serializable]
    public struct CameraData
    {
        public CameraOffsetData OffsetData;
        public CameraTransitionsData Transitions;
        public LifeCycleData Duration;
        public IEnumerable<Transform> Targets;
        public int TargetsCount;

        public CameraData(IEnumerable<Transform> targets, LifeCycleData Duration = null, CameraTransitionsData transitionsData = null, CameraOffsetData offsetData = null)
        {
            this.Duration = Duration ?? new LifeCycleData();
            Transitions = transitionsData ?? new CameraTransitionsData();
            OffsetData = offsetData ?? new CameraOffsetData();
            Targets = targets ?? new List<Transform>();
            TargetsCount = 0;
            CountTotalTargets();
        }

        private void CountTotalTargets()
        {
            var _enumerator = Targets.GetEnumerator();
            while (_enumerator.MoveNext())
            {
                TargetsCount++;
            }
        }
    }
}

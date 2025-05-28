using System;
using System.Collections.Generic;

namespace Code.Wakoz.PurrKurr.DataClasses.ServerData
{
    [Serializable]
    public class OngoingObjectives_SerializeableData
    {
        public Dictionary<string, int[]> objectives = new();
    }
}
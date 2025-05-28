using System;
using System.Collections.Generic;

namespace Code.Wakoz.PurrKurr.DataClasses.ServerData
{
    [Serializable]
    public class CompletedObjectives_SerializeableData
    {
        public HashSet<string> objectives = new();
    }
}
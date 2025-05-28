using System;
using System.Collections.Generic;

namespace Code.Wakoz.PurrKurr.DataClasses.ServerData
{
    [Serializable]
    public class GameState_SerializeableData
    {
        public int roomId;
        public List<ObjectRecord> objects = new();
    }
}
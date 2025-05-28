using System;

namespace Code.Wakoz.PurrKurr.DataClasses.ServerData
{
    [Serializable]
    public class MainCharacter_SerializeableData
    {
        public int id;

        public MainCharacter_SerializeableData(int id)
        {
            this.id = id;
        }
    }
}
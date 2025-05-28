using System;
using System.Collections.Generic;

namespace Code.Wakoz.PurrKurr.DataClasses.ServerData
{
    [Serializable]
    public class CharactersIDs_SerializeableData
    {
        public List<string> characters = new();

        public CharactersIDs_SerializeableData(List<string> characters)
        {
            this.characters = characters;
        }
    }
}
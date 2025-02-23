using System;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems
{

    [Serializable]
    public class CollectableItemData
    {
        [SerializeField] private string _itemId = "";

        [SerializeField][Min(1)] private int _quantity = 1;

        public string ItemId => _itemId;
        public int Quantity => _quantity;
    }
}
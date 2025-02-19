using System.Threading.Tasks;
using UnityEngine;
using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Xml;

namespace Code.Wakoz.PurrKurr.Screens.Objectives
{

    [DefaultExecutionOrder(12)]
    public sealed class ObjectivesController : SingleController
    {
        [SerializeField] private ObjectivesView _view;

        private ObjectivesModel _model;

        protected override Task Initialize()
        {
            return Task.CompletedTask;
        }

        protected override void Clean() {}

        public void HandleNewObjectives(List<IObjective> objectives)
        {
            var objectivesModel = new List<ObjectiveModel>();
            foreach (var objective in objectives)
            {
                objectivesModel.Add(new ObjectiveModel(objective));
            }

            _model = new ObjectivesModel(objectivesModel);
            _model.SortAction = OrderItemsByUniqueIds();

            _view.SetModel(_model);
        }

        public void HandleObjectivesChanged(List<IObjective> objectives)
        {
            var uniqueIds = new List<string>();
            for (var i = 0; i < objectives.Count; i++)
            {
                uniqueIds.Add(objectives[i].GetUniqueId());
            }
            _model.ReorderItemsInternal(uniqueIds);
            _model.UpdateItems(uniqueIds);
        }

        /// <summary>
        /// Create a dictionary to map unique IDs to their order
        /// Then Sort the Objectives list in place based on the order in uniqueIds
        /// </summary>
        /// <returns></returns>
        private Action<List<string>> OrderItemsByUniqueIds() 
            => (uniqueIds) =>
            {
                var idOrder = uniqueIds
                    .Select((id, index) => new { id, index })
                    .ToDictionary(x => x.id, x => x.index);

                _model.Objectives.Sort((o1, o2) =>
                {
                    if (o1 == null || o2 == null)
                        return 0;

                    var id1 = o1.InterfaceData.GetUniqueId();
                    var id2 = o2.InterfaceData.GetUniqueId();

                    var index1 = idOrder.ContainsKey(id1) ? idOrder[id1] : int.MaxValue;
                    var index2 = idOrder.ContainsKey(id2) ? idOrder[id2] : int.MaxValue;

                    return index1.CompareTo(index2);
                });
            };
    }
}
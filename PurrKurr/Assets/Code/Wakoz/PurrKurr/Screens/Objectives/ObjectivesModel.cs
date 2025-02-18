using System;
using System.Collections.Generic;
using System.Linq;

namespace Code.Wakoz.PurrKurr.Screens.Objectives
{
    public class ObjectivesModel : Model
    {

        public List<ObjectiveModel> Objectives;

        public Action<List<string>> SortAction { get; set; }

        public ObjectivesModel(List<ObjectiveModel> objectives)
        {
            Objectives = objectives;
        }

        public void UpdateItems()
        {
            foreach (var objective in Objectives)
            {
                objective.UpdateItem();
            }

            Changed();
        }

        public void UpdateItems(List<string> uniqueIds)
        {
            var objectiveWithId = Objectives.Where(o => o != null && uniqueIds.Contains(o.InterfaceData.GetUniqueId()));

            foreach (var objective in objectiveWithId)
            {
                objective.UpdateItem();
            }

            Changed();
        }


        /// <summary>
        /// Executes the sorting logic if a SortAction is provided.
        /// </summary>
        public void ReorderByUniqueIdsInternal(List<string> uniqueIds)
        {
            if (SortAction == null)
            {
                throw new InvalidOperationException("SortAction is not set");
            }

            SortAction.Invoke(uniqueIds);
        }

        public void ReorderItemsInternal(List<string> uniqueIds)
        {
            if (uniqueIds == null)
                throw new ArgumentNullException(nameof(uniqueIds));

            if (Objectives == null || Objectives.Count == 0)
                return;

            // Create a dictionary to map unique IDs to their order
            var idOrder = uniqueIds
                .Select((id, index) => new { id, index })
                .ToDictionary(x => x.id, x => x.index);

            // Sort the Objectives list in place based on the order in uniqueIds
            Objectives.Sort((o1, o2) =>
            {
                if (o1 == null || o2 == null)
                    return 0;

                var id1 = o1.InterfaceData.GetUniqueId();
                var id2 = o2.InterfaceData.GetUniqueId();

                var index1 = idOrder.ContainsKey(id1) ? idOrder[id1] : int.MaxValue;
                var index2 = idOrder.ContainsKey(id2) ? idOrder[id2] : int.MaxValue;

                return index1.CompareTo(index2);
            });
        }

    }
}
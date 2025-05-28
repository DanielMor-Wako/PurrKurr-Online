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
            foreach (var objective in Objectives) {

                objective.UpdateItem();
            }

            Changed();
        }

        public void UpdateItems(List<string> uniqueIds)
        {
            var objectiveWithId = Objectives.Where(o => o != null && uniqueIds.Contains(o.InterfaceData.GetUniqueId()));

            foreach (var objective in objectiveWithId) {

                objective.UpdateItem();
            }

            Changed();
        }


        /// <summary>
        /// Executes the sorting logic if a SortAction is provided.
        /// </summary>
        public void ReorderItemsInternal(List<string> uniqueIds)
        {
            if (SortAction == null) {

                throw new InvalidOperationException("SortAction is not set");
            }

            SortAction.Invoke(uniqueIds);
        }

    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives {

    public class ObjectivesManager {

        private List<IObjective> _objectives;
        private List<string> _completedObjectivesId;

        public List<string> GetCompletedObjectivesIds() => _completedObjectivesId;

        public void Initialize(List<IObjective> objectivesList, ref List<string> completedObjectivesList) {

            _objectives = objectivesList;
            _completedObjectivesId = completedObjectivesList;
        }

        public void UpdateObjectives() {

            foreach (var objective in _objectives) {

                if (objective.IsComplete() && !_completedObjectivesId.Contains(objective.GetUniqueId())) {

                    _completedObjectivesId.Add(objective.GetUniqueId());
                    Debug.Log($"Objective: {objective.GetObjectiveDescription()} completed");
                }

            }
        }

        public void UpdateObjectiveOfCollectableType(string collectableType, int amountToAdd) {

            foreach (var objective in _objectives) {

                if (objective.GetObjectType() != collectableType) {
                    continue;
                }

                if (!objective.IsComplete() || !_completedObjectivesId.Contains(objective.GetUniqueId())) {

                    Debug.Log($"Objective: {objective.GetObjectiveDescription()} updated by {amountToAdd}");
                    objective.UpdateProgress(amountToAdd);

                    break;
                }
            }
        }

        public void CompleteObjectiveOfType(Type type) {

            foreach (var objective in _objectives) {

                if (objective.GetType() != type) {
                    continue;
                }

                if (!objective.IsComplete() || !_completedObjectivesId.Contains(objective.GetUniqueId())) {

                    Debug.Log($"Objective: {objective.GetObjectiveDescription()} force completed");
                    objective.Finish();

                    _completedObjectivesId.Add(objective.GetUniqueId());
                }
            }
        }
    }
}
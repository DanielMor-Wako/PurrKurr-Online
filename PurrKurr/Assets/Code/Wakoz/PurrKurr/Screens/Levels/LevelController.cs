using System.Collections.Generic;
using System.Threading.Tasks;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using UnityEngine;
using Code.Wakoz.Utils.Attributes;

namespace Code.Wakoz.PurrKurr.Screens.Levels 
{

    public class LevelController : Controller 
    {

        [SerializeField] private List<ObjectiveDataSO> _objectivesData;

        public List<ObjectiveDataSO> GetObjectives()
            => _objectivesData;

        protected override void Clean() { }

        protected override Task Initialize()
        {
            return Task.CompletedTask;
        }

    }

}

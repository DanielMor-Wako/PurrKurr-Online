using Code.Wakoz.PurrKurr.DataClasses.Objectives;

namespace Code.Wakoz.PurrKurr.Screens.Objectives
{
    public class ObjectiveModel : Model
    {
        public IObjective InterfaceData { get; set;}

        public ObjectiveModel(IObjective interfaceData)
        {
            InterfaceData = interfaceData;
        }

        public void UpdateItem()
        {
            Changed();
        }
    }
}
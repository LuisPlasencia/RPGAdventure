using UnityEngine;

namespace RPG.Core
{

    public class ActionScheduler : MonoBehaviour 
    {
        IAction currentAction;

        public void StartAction(IAction action)
        {
            if(currentAction == action) return;
            if(currentAction != null)
            {
                currentAction.Cancel();  // cancelamos la anterior acción, antes de asignar la nueva ... este cancel llama al Cancel de Mover o de Fighter pero eso me da igual aquí
            }

            currentAction = action;
        }

        public void CancelCurrentAction()
        {
            StartAction(null);
        }

    }
}
using System.Collections;
using System.Collections.Generic;
using RPG.Control;
using RPG.Core;
using UnityEngine;
using UnityEngine.Playables;

namespace RPG.Cinematics
{
    public class CinematicControlRemover : MonoBehaviour
    {
        GameObject player;

        private void Awake() 
        {
            player = GameObject.FindWithTag("Player");   // asignamos las variables en awake y no en start porque si hay otras clases que hicieran uso de él en su start y dicho start se ejecuta antes que este, estarían accediendo a algo null
        }


        // buena práctica añadir notificaciones en enable y quitarlas en disable para no recibir callbacks cuando la clase está desactivada pero volvernos a suscribir cuando sí lo está
        private void OnEnable() 
        {
            GetComponent<PlayableDirector>().played += DisableControl;   // registramos métodos al evento, el cual llama a dichos "callbacks" cuando se triggerea dicho evento
            GetComponent<PlayableDirector>().stopped += EnableControl;
        }

        private void OnDisable() 
        {
            GetComponent<PlayableDirector>().played -= DisableControl;   
            GetComponent<PlayableDirector>().stopped -= EnableControl;
        }

        void DisableControl(PlayableDirector pd)
        {
            player.GetComponent<ActionScheduler>().CancelCurrentAction();
            player.GetComponent<PlayerController>().enabled = false;
        }

        void EnableControl(PlayableDirector pd)
        {
            player.GetComponent<PlayerController>().enabled = true;
        }
    }
}


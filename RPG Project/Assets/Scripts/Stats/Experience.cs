using UnityEngine;
using RPG.Saving;
using System;

namespace RPG.Stats
{
    
    public class Experience : MonoBehaviour, ISaveable
    {
        [SerializeField] float experiencePoints = 0;

        // public delegate void ExperienceGainedDelegate();
        // public event ExperienceGainedDelegate onExperienceGained;
        public event Action onExperienceGained;  // exactamente lo mismo que arriba (sin return y sin parámetros de entrada). Action se encuentra en using System.
        // keyword Action = a predefined delegate with no return value

        //Ejemplo de eventos y delegados (con ello podemos establecer el patrón observer para notificar de un level up, por ejemplo)
//        public delegate bool ExampleDelegate (float value);  // un delegate es una lista de punteros a métodos. Otras clases se pueden suscribir al delegado llamando a onDoneStuff += método con la estructura indicada (return bool y parámetro float)
        // keyword delegate = list of pointers to methods
//        public event ExampleDelegate onDoneStuff;         // si llamamos a este método o evento pasándole el float por parámetros, llamará a todos los métodos suscritos al evento (el return que se devuelve es el último suscrito a la lista)
        // el keyword event nos protege para que no podamos overwritear con un onDoneStuff = método, y cargarnos los métodos ya suscritos al sobreescribir todo por ese 
        // keyword event = a protection to prevent overwriting



        public void GainExperience(float experience)
        {
            experiencePoints += experience;
            onExperienceGained();   // llama a todos los métodos suscritos al evento 
        }

        public float GetPoints()
        {
            return experiencePoints;
        }

        public object CaptureState()
        {
            return experiencePoints;
        }

        public void RestoreState(object state)
        {
            experiencePoints = (float) state;
        }
    }
}
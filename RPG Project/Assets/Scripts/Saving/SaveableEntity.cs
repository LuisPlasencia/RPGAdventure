using System;
using System.Collections.Generic;
using RPG.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Saving
{
    // execute always = se ejecuta tanto en el juego como en el editor
    [ExecuteAlways]
    public class SaveableEntity : MonoBehaviour
    {
        [SerializeField] string uniqueIdentifier = "";
        static Dictionary<string, SaveableEntity> globalLookup = new Dictionary<string, SaveableEntity>();  // static = no cambia en todo el lifecycle de la aplicación

        public string GetUniqueIdentifier()
        {
            return uniqueIdentifier;
        }

        public object CaptureState()
        {
            Dictionary<string, object> state = new Dictionary<string, object>();
            foreach (ISaveable saveable in GetComponents<ISaveable>())
            {
                state[saveable.GetType().ToString()] = saveable.CaptureState();   // si sacamos datos de la clase Mover por ejemplo, la key tendría de nombre Mover
                // Compile-Time: GetType() = ISaveable
                // Runtime: GetType() =  Mover, Health, Mover, Fighter.... (la clase que implementa dicho ISaveable)
            }
            return state;
        }

        public void RestoreState(object state)
        {
            Dictionary<string, object> stateDict = (Dictionary<string, object>)state;
            foreach (ISaveable saveable in GetComponents<ISaveable>())
            {
                string typeString = saveable.GetType().ToString();
                if (stateDict.ContainsKey(typeString))
                {
                    saveable.RestoreState(stateDict[typeString]);
                }
            }
        }

#if UNITY_EDITOR    // para que podamos empaquetar el juego (build), tenemos que indicar que solo se ejecute este código en el editor, no en la versión build  (serializedobject solo se puede acceder desde el editor)
        private void Update() {    
            if (Application.IsPlaying(gameObject)) return;   // para que no se ejecute mientras jugamos
            if (string.IsNullOrEmpty(gameObject.scene.path)) return;   // para que no aplique UUID a los prefabs (los prefabs no tienen path) (y así no se propague). Que solo lo aplique a los scene files

            SerializedObject serializedObject = new SerializedObject(this);  // para cambiar los valores de scene files o prefabs y que sean reconocibles por Unity (que no solo se queden en memoria)
            SerializedProperty property = serializedObject.FindProperty("uniqueIdentifier");
            
            if (string.IsNullOrEmpty(property.stringValue) || !IsUnique(property.stringValue))    // IsUnique comprueba que no estamos duplicando el identificador al hacer copias del gameobject
            {
                property.stringValue = System.Guid.NewGuid().ToString();
                serializedObject.ApplyModifiedProperties();   //  tells unity we've made a change
            }

            globalLookup[property.stringValue] = this;  
        }
#endif

        private bool IsUnique(string candidate)   // findGameObjectsOfType consume muchos recursos, sobretodo si lo estamos llamando en cada entidad, por eso mejor esta forma
        {
            if (!globalLookup.ContainsKey(candidate)) return true;  // si la key no existe, no necesitamos generar otro UUID porque no hay otras entidades de este tipo ya creadas 

            if (globalLookup[candidate] == this) return true;  // si somos nosotros, ya somos únicos porque estamos registrados

            if (globalLookup[candidate] == null)    // si la entidad ha sido eliminada (al cambiar de scene) apuntará a null. La debemos eliminar del registro porque ya es única. Si no, estaría generando otro UUID al volver a la scene reseteando así la entidad, lo cual no nos interesa.
            {
                globalLookup.Remove(candidate);
                return true;
            }

            if (globalLookup[candidate].GetUniqueIdentifier() != candidate)  // si cambiamos de UUID en el editor manualmente, querremos eliminar la anterior (eliminamos la clave not up-to-date)
            {
                globalLookup.Remove(candidate);
                return true;
            }

            return false;
        }
    }
}
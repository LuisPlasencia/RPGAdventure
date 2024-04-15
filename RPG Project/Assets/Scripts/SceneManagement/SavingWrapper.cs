using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Saving;
using UnityEngine;

namespace RPG.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {

        const string defaultSaveFile =  "save";
        [SerializeField] float fadeInTime = 0.5f;


        private void Awake()
        {
            StartCoroutine(LoadLastScene());  // hemos tenido que cambiar esto de Start a Awake para que siempre recuperemos los datos antes de que se asigne el updateLevel al evento en Experience
            // ya que nuestro medidor de nivel solo se calcula cuando ganamos experiencia y si ya la hemos recuperado antes de suscribirnos al evento, no la volverá a calcular y nos mostrará que estamos al nivel 1
            // queremos que restore ocurra antes que start ya que el calculate level está en start
            
            // y es que el yield LoadSceneAsync al que está enlazado el yield LoadLastScene de esta corrutina termina cuando todo ha sido Awaken pero antes de que algún Start haya sido llamado (despues de awaken y antes de start)
        
        }

        IEnumerator LoadLastScene()   // hacer del Start un IEnumerator nos ahorra poner startCoroutine() 
        {
            yield return GetComponent<SavingSystem>().LoadLastScene(defaultSaveFile);
            Fader fader = FindObjectOfType<Fader>();
            fader.FadeOutImmediate();    // OJO!: es fundamental acceder a esta función después del yield porque si no la estaríamos llamando en awake y llamar funciones en awake es un NO-NO, pero el yield nos da seguridad porque significa que hay un tiempo de espera en el que ya se habrán iniciado todos los awake, entre ellos el awake de savingwrapper() que es el que instancia el fader en el scene
            yield return fader.FadeIn(fadeInTime);

        }
        
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.L))
            {
                Load();
            } 
            if(Input.GetKeyDown(KeyCode.S))
            {
                Save();
            }
            if(Input.GetKeyDown(KeyCode.Delete))
            {
                Delete();
            }
        }

        public void Save()
        {
            GetComponent<SavingSystem>().Save(defaultSaveFile);
        }

        public void Load()
        {
            GetComponent<SavingSystem>().Load(defaultSaveFile);
        }

        public void Delete()
        {
            GetComponent<SavingSystem>().Delete(defaultSaveFile);
        }
    }

}
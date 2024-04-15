using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RPG.Core
{
    public class PersistentObjectSpawner : MonoBehaviour
    {
        [SerializeField] GameObject persistentObjectPrefab;

        static bool hasSpawned = false;  // lives and dies with application not with the instance of a class - si cambiamos de scene los valores no se resetean aunque sea otra instancia de la clase
        // nos evitamos estar usando singletons pero si que estamos usando una variable global static y eso tampoco es bueno del todo pero es mejor que el singleton ya que el singleton hace que otras
        // clases dependan de su implementación y si cambia algo rompe todo, en cambio un objeto persistente puede actuar por cuenta propia sin depender de los demás

        private void Awake(){
            if (hasSpawned) return;

            SpawnPersistentObjects();   // es correcto poner esto en awake ya que no estamos accediendo a ningún método de dicha clase, solo la instanciamos

            hasSpawned = true;
        }

        private void SpawnPersistentObjects()
        {
            GameObject persistentObject = Instantiate(persistentObjectPrefab);
            DontDestroyOnLoad(persistentObject);
        }
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Control;
using RPG.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace RPF.SceneManagement
{
    public class Portal : MonoBehaviour
    {
        enum DestinationIdentifier
        {
            A, B, C, D, E
        }

        [SerializeField] int sceneToLoad = -1;
        [SerializeField] Transform spawnPoint;
        [SerializeField] DestinationIdentifier destination;
        [SerializeField] float fadeOutTime = 1f;
        [SerializeField] float fadeInTime = 2f;
        [SerializeField] float fadeWaitTime = 0.5f;

        private void OnTriggerEnter(Collider other) {
            if(other.tag == "Player")
            {
                StartCoroutine(Transition());
            }
        }

        private IEnumerator Transition()
        {

            if(sceneToLoad < 0)
            {
                Debug.LogError("Scene to load not set");
                yield break;
            }

            DontDestroyOnLoad(gameObject);


            PlayerController playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            playerController.enabled = false;     // Remove Control

            Fader fader = FindObjectOfType<Fader>();
            yield return fader.FadeOut(fadeOutTime);   // antes de cargar el siguiente nivel nos aseguramos de que estamos completely faded out
            
            SavingWrapper wrapper = FindObjectOfType<SavingWrapper>();
            wrapper.Save();  // queremos que el nivel actual se guarde 

            yield return SceneManager.LoadSceneAsync(sceneToLoad);  // lo que va después de esta línea se ejecuta después de que todos los awake de la nueva scene se hayan llamado. De ahí la necesidad del yield


            PlayerController newPlayerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            newPlayerController.enabled = false;     // Remove Control from new scene player

            wrapper.Load();  // queremos que el nivel al que vamos esté como estaba

            // aqui, si el fader no fuera persistente, la referencia que tenemos de él no apuntaría a nada porque se habría eliminado su instancia
            // sin embargo, el fader que hemos creado sí que es persistente entre niveles y por eso podemos hacer esto
            // no es realmente un singleton porque no utiliza el patrón, sino que es un objeto persistente. Nos permite tener pequeñas clases que hacen cosas concretas
            // deventaja del singleton = hace que la aplicación sea difícil de debugear al tener todo en uno, pudiendo romper muchas cosas entre niveles por su camino

            Portal otherPortal = GetOtherPortal();
            UpdatePlayer(otherPortal);

            wrapper.Save();  // cada portal tiene su destino. Necesitamos guardar el estado del jugador en el portal al que le corresponde estar

            yield return new WaitForSeconds(fadeWaitTime);
            fader.FadeIn(fadeInTime);  // como podemos ver, al devolver Coroutine en vez de IEnumerator, podemos yieldear o no. Si yieldeamos paramos la ejecución hasta que termine el fade y si no yieldeamos, dejamos que termine de fadearse en segundo plano y así le podemos devolver el control al jugador antes de tiempo.

            newPlayerController.enabled = true;  // Restore Control
            Destroy(gameObject);
        }

        private Portal GetOtherPortal()
        {
            foreach (Portal portal in FindObjectsOfType<Portal>())
            {
                if(portal == this) continue;
                if(portal.destination != destination) continue;
                return portal;
            }
            return null;
        }

        private void UpdatePlayer(Portal otherPortal)
        {
            GameObject player = GameObject.FindWithTag("Player");
            player.GetComponent<NavMeshAgent>().Warp(otherPortal.spawnPoint.position);
            // player.transform.position = otherPortal.spawnPoint.position;
            player.transform.rotation = otherPortal.spawnPoint.rotation;
        }
    }
}


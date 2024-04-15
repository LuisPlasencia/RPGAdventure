using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Attributes;
using RPG.Core;
using RPG.Saving;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Movement
{
    public class Mover : MonoBehaviour, IAction, ISaveable  // una clase no puede heredar de más de una clase pero puede hacerlo de todas las interfaces que quiera (MonoBehaviour es una clase, lo otro interfaces)
    {   
        [SerializeField] Transform target; 
        [SerializeField] float maxSpeed = 6f;
        
        [SerializeField] float maxNavPathLength = 40f;
        NavMeshAgent navMeshAgent;
        Health health;

        private void Awake() 
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
        }

        void Update()
        {
            navMeshAgent.enabled = !health.IsDead();    // para no tropezarnos con el enemigo cuando muera (elimina la colisión)

            UpdateAnimator();
        }

        public void StartMoveAction(Vector3 destination, float speedFraction)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            MoveTo(destination, speedFraction);
        }

        public bool CanMoveTo(Vector3 destination)
        {
            NavMeshPath path = new NavMeshPath();  // al no usarse exactamente como un out parameter debemos inicializarla al no poder ser null un construct (el CalculatePath puede ponerla como null y algo así no se puede devolver si no es out al no ser referencia)
            bool hasPath = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);  // le pasamos un objeto path para que lo pueda modificar el CalculatePath
            if (!hasPath) return false;
            if (path.status != NavMeshPathStatus.PathComplete) return false;  // no queremos que el jugador pueda intentar moverse a sitios donde no pueda llegar
            if (GetPathLength(path) > maxNavPathLength) return false;  // no queremos uqe el jugador clickee en sitios muy lejos y que el navmesh le lleve automaticamente, queremos que encuentre el camino por su cuenta
            return true;
        }

        public void MoveTo(Vector3 destination, float speedFraction)
        {
            navMeshAgent.destination = destination;
            navMeshAgent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
            navMeshAgent.isStopped = false;
        }

        public void Cancel()
        {
            navMeshAgent.isStopped = true;
        }


        private void UpdateAnimator()
        {
            Vector3 velocity = navMeshAgent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float speed = localVelocity.z;
            GetComponent<Animator>().SetFloat("forwardSpeed", speed);
        }


        private float GetPathLength(NavMeshPath path)
        {
            float total = 0;
            if (path.corners.Length < 2) return total;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                total += Vector3.Distance(path.corners[i], path.corners[i+1]);  // sumamos las distancias del camino o path hasta el destino
            }

            return total;
        }

        public object CaptureState()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["position"] = new SerializableVector3(transform.position);   // los elementos que querramos guardar han de ser serializables
            data["rotation"] = new SerializableVector3(transform.eulerAngles);
            return data;

            // return new SerializableVector3(transform.position)   // si solo nos interesa guardar la posicion
        }

        // debemos asegurarnos de llamar al restore siempre después de awake pero antes de start para evitar race conditions
        public void RestoreState(object state)
        {
            
            Dictionary<string, object> data = (Dictionary<string, object>) state;
            GetComponent<NavMeshAgent>().enabled = false;  // OJO .. en vez de hacer enable y disable es mejor hacer NavMeshAgent().Move(position)
            transform.position = ((SerializableVector3) data["position"]).ToVector();
            transform.eulerAngles = ((SerializableVector3) data["rotation"]).ToVector();
            GetComponent<NavMeshAgent>().enabled = true;  // para que el navmeshagente no interfiera con nuestra posición
        }



// OTRA FORMA DE GUARDAR -> A TRAVÉS DE STRUCTS EN VEZ DE DICCIONARIOS Y NOS AHORRAMOS CASTS:
// C:\Users\Usuario\AppData\LocalLow\DefaultCompany\RPG Project    borrar save file para probar este método

        // [System.Serializable]
        // struct MoverSaveData
        // {
        //     public SerializableVector3 position;
        //     public SerializableVector3 rotation;
        // }

        // public object CaptureState()
        // {
        //     MoverSaveData data = new MoverSaveData();
        //     data.position = new SerializableVector3(transform.position); 
        //     data.rotation = new SerializableVector3(transform.eulerAngles);
        //     return data;
        // }

        // public void RestoreState(object state)
        // {
        //     MoverSaveData data = (MoverSaveData) state;
        //     GetComponent<NavMeshAgent>().enabled = false;
        //     transform.position = data.position.ToVector();
        //     transform.eulerAngles = data.rotation.ToVector();
        //     GetComponent<NavMeshAgent>().enabled = true;  // para que el navmeshagente no interfiera con nuestra posición
        // }       
    }
}

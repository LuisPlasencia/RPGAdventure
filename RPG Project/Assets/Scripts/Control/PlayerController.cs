using System;
using RPG.Attributes;
using RPG.Combat;
using RPG.Movement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace RPG.Control
{
    public class PlayerController : MonoBehaviour {

        Health health;
        
        [System.Serializable]
        struct CursorMapping   // Use Structure if all member fields are of value type. Use Class if any one member is of reference type.
        {
            public CursorType type;
            public Texture2D texture;
            public Vector2 hotspot;

        }
        
        [SerializeField] CursorMapping[] cursorMappings = null;
        [SerializeField] float maxNavMeshProjectionDistance = 1f;
        [SerializeField] float raycastRadius = 1f;


        private void Awake() {
            health = GetComponent<Health>(); // asignamos las variables en awake y no en start porque si hay otras clases que hicieran uso de él en su start y dicho start se ejecuta antes que este, estarían accediendo a algo null
        }


        void Update()
        {
            if(InteractWithUI()) return;

            if(health.IsDead())
            {
                SetCursor(CursorType.None);
                return;
            }

            if(InteractWithComponent()) return;  // raycastables (enemigos, pickups etc, elementos del escenario sobre los que queremos clickear (interactuar) incluidos enemigos)
            
            // if(InteractWithCombat()) return;  // ahora esto lo lleva interactwithcomponent también ya que el handleraycast lo puede llamar tanto combattarget como el weaponpickup etc

            if(InteractWithMovement()) return;   // aquí raycasteamos hacia el navmesh, el cual no tiene componentes a los que hacer raycastable por eso no lo podemos poner también en interactwithcomponent 

            SetCursor(CursorType.None);
        }

        private bool InteractWithComponent()
        {
            RaycastHit[] hits = RaycastAllSorted();  
            foreach(RaycastHit hit in hits)
            {
                IRayCastable[] raycastables = hit.transform.GetComponents<IRayCastable>();  // si el gameobject actual tiene algún script (hijos del gameobject = componentes) que implemente IRayCastable como interfaz, este será añadido a esta lista
                foreach(IRayCastable raycastable in raycastables)
                {
                    if(raycastable.HandleRaycast(this))  
                    {
                        SetCursor(raycastable.GetCursorType());
                        return true;
                    }
                }
            }
            return false;
        }

        RaycastHit[] RaycastAllSorted()
        {
            // spherecast es mejor para pickups y enemigos en vez de raycastAll porque asi no tenemos que apuntar tanto
            RaycastHit[] hits = Physics.SphereCastAll(GetMouseRay(), raycastRadius);  // todos los gameobjects que atraviese el rayo se añadirán aquí
            // el orden de los raycastables es random, no es el primero que alcanza el rayo y el segundo etc así que los vamos a ordenar por distancia (así si clickeamos en dos raycastables uno delante de otro, tomamos la interacción del que está delante y no del que está detrás)
            float[] distances = new float[hits.Length];
            for (int i = 0; i < hits.Length; i++)
            {
                distances[i] = hits[i].distance;
            }
            Array.Sort(distances, hits);  // ordena el segundo en base al primero
            return hits;
        }

        private bool InteractWithUI()
        {
            if(EventSystem.current.IsPointerOverGameObject())   // se refiere a UI game objects - es decir, es esto un game object que pertenece al UI? OJO : cuidado con el fader que ocupa toda la pantalla y es parte del UI (es un Canvas UI), hemos tenido que setear el Canvas Group no interactable y no block raycasts en el editor
            {
                SetCursor(CursorType.UI);
                return true;
            }
            return false;
        }



        private bool InteractWithMovement()
        {
            Vector3 target;
            bool hasHit = RaycastNavMesh(out target);
            if (hasHit)
            {
                if(!GetComponent<Mover>().CanMoveTo(target)) return false;   // comprobamos que podamos llegar o si estamos muy lejos no nos deja

                if (Input.GetMouseButton(0))
                {
                    GetComponent<Mover>().StartMoveAction(target, 1f);
                }
                SetCursor(CursorType.Movement);
                return true;
            }
            return false;
            // Debug.DrawRay(lastRay.origin, lastRay.direction * 100);
        }

        private bool RaycastNavMesh(out Vector3 target)
        {
            target = new Vector3();

            RaycastHit hit;
            bool hasHit = Physics.Raycast(GetMouseRay(), out hit);  // out = out parameter
            if (!hasHit) return false;

            // Find nearest navmesh point
            NavMeshHit navMeshHit;
            bool hasCastToNavMesh = NavMesh.SamplePosition(
                hit.point, out navMeshHit, maxNavMeshProjectionDistance, NavMesh.AllAreas);
            if(!hasCastToNavMesh) return false;

            target = navMeshHit.position;

            return true;
        }


        private void SetCursor(CursorType type)
        {
            CursorMapping mapping = GetCursorMapping(type); 
            Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
        }

        private CursorMapping GetCursorMapping(CursorType type)
        {
            foreach (CursorMapping mapping in cursorMappings)
            {
                if(mapping.type == type)
                {
                    return mapping;
                }
            }
            return cursorMappings[0];
        }

        private static Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }


        
    // así interactuabamos con el enemigo antes de implementar handleraycast:
        // private bool InteractWithCombat()
        // {
        //     RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());
        //     foreach(RaycastHit hit in hits)
        //     {
        //         CombatTarget target = hit.transform.GetComponent<CombatTarget>();  // get component en un component busca los componentes en los hijos de dicha entidad (en este caso de Enemy)
        //         if(target == null) continue;

        //         if(!GetComponent<Fighter>().CanAttack(target.gameObject)) continue;   // salir de este loop e ir al siguiente

        //         if(Input.GetMouseButton(0))
        //         {
        //             GetComponent<Fighter>().Attack(target.gameObject);
        //         }
        //         SetCursor(CursorType.Combat);
        //         return true;
        //     }
        //     return false;
        // }

    }
}
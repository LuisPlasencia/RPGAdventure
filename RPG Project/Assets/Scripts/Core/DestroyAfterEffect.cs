using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RPG.Core
{
    public class DestroyAfterEffect : MonoBehaviour
    {

        [SerializeField] GameObject targetToDestroy = null;  // por si el efecto está dentro de un padre y queremos eliminar al padre señalándolo en esta variable (al borrar al padre también se borran los hijos y con ellos el efecto)

        void Update()
        {
            if(!GetComponent<ParticleSystem>().IsAlive())
            {
                if(targetToDestroy != null)
                {
                    Destroy(targetToDestroy);
                }
                else 
                {
                    Destroy(gameObject);
                }
            }
        }

    }
}


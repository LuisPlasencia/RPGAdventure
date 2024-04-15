using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class CameraFacing : MonoBehaviour
    {
        void LateUpdate()
        {
            transform.forward = Camera.main.transform.forward;  // queremos que el texto mire a cámara (numeros con el daño)

            // Por qué usamos LateUpdate, en vez de Update?
            // si el update del enemigo entra antes del de la camara. Sabemos que el enemigo es el padre de la cámara y su rotación influye también en la rotación de la barra de vida.
            // esto puede provocar jittering en la barra de vida ya que un update estaría interaccionando con el otro y a veces se puede quedar torcida
            // nos tenemos uqe asegurar de que este update sea el último en ejecutarse y así la rotación del enemigo no influya para nada en la rotación de la barra de vida
            // LateUpdate es un update que entra después del Update convencional según el diagrama de ejecución. 
        }
    }
}


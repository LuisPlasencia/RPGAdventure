using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Attributes
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] Health healthComponent = null;
        [SerializeField] RectTransform foreground = null;
        [SerializeField] Canvas rootCanvas = null;

        void Update()
        {
            if(Mathf.Approximately(healthComponent.GetFraction(), 0) 
            || Mathf.Approximately(healthComponent.GetFraction(), 1))  // para floats siempre mejor usar approximately (porque el resultado de operaciones de floats puede ser impreciso y depender de los ciclos de computación del procesador aparte de poder ser decimales (aproximadamente)). No comparar floats con == como hacemos con los integers
            {
               rootCanvas.enabled = false;
               return;
            }
            rootCanvas.enabled = true;
            foreground.localScale = new Vector3(healthComponent.GetFraction(), 1, 1);
 
        }
    }  
}

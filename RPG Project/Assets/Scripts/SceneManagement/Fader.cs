using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RPG.SceneManagement
{
    public class Fader : MonoBehaviour
    {
        CanvasGroup canvasGroup;
        Coroutine currentActiveFade = null;

        private void Awake() {
            canvasGroup = GetComponent<CanvasGroup>();   // necesario en awake en vez de start porque puede que no tengamos un fader creado para cuando en el savingwrapper querramos hacer el fadeoutimmediately en start si ponemos esto en start
            
            // StartCoroutine(FadeOutIn());     // ejemplo de llamada de corrutinas dentro de otra corrutina
        }

        // IEnumerator FadeOutIn()
        // {
        //     yield return FadeOut(3f);
        //     print ("Faded out");
        //     yield return FadeIn(1f);
        //     print("Faded in");
        // }

        public void FadeOutImmediate()
        {
            canvasGroup.alpha = 1;
        }

        // OJO por qué hemos cambiado algunas funciones para que retornen Coroutine en vez de IEnumerator? 
        // Porque esto me permite que podamos continuar la ejecución independientemente del yield, es decir, retornar antes de que se complete la corutina y así podernos mover en el fade in y retomar el control. Es basicamente convertir la corrutina en un proceso en segundo plano en vez impedirnos continuar con las líneas de código en Portal.Transition();
        // de hecho, esto no nos limita la posibilidad de hacer yield. Si desde fuera hacemos el yield esperará como debe ser (fadeOut) pero si no nos interesa yieldear y que corra en segundo plano pues no yieldeamos, esa es la ventaja.

        public Coroutine FadeOut(float time)  // invisible a visible
        {
            return Fade(1, time);
        }

        public Coroutine FadeIn(float time)  // visible a invisible
        {
            return Fade(0, time);
        }

        public Coroutine Fade(float target, float time)
        {
            if(currentActiveFade != null)   // para que no haya un caso en el que las dos corrutinas se activen a la vez al ir y volver rápido de un nivel
            {
                StopCoroutine(currentActiveFade);
            }
            currentActiveFade = StartCoroutine(FadeRoutine(target, time));
            return currentActiveFade;
        }

        private IEnumerator FadeRoutine(float target, float time)
        {
            while(!Mathf.Approximately(canvasGroup.alpha, target))
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, Time.deltaTime/time);  // suma o resta dependiendo del target. Así me ahorro repetición de código
                yield return null;  // run on the next frame = execute every frame
            }
        }


    }
}


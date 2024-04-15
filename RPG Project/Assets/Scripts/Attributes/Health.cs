using System;
using GameDevTV.Utils;
using RPG.Core;
using RPG.Saving;
using RPG.Stats;
using UnityEngine;
using UnityEngine.Events;

namespace RPG.Attributes
{
    public class Health : MonoBehaviour, ISaveable
    {
        [SerializeField] float regenerationPercentage = 70;
        [SerializeField] TakeDamageEvent takeDamage;  // UnityEvent me permite enlazarle métodos o funciones desde el editor - es básicamente muy parecido al delegado que usamos en Health (delegate y event o Action) - nos da flexibilidad, sobretodo al diseñador al permitirnos hacer el enlace en el editor (si tiene <tipo> es decir que necesita un parámetro (serializable es incompatible con class generics) (LazyValue<float>) no nos saldrá en el editor porque no es serializable pero si lo usamos sin eso si sale) por eso la solución es implementarlo como una subclase en la línea de abajo
        [SerializeField] UnityEvent onDie;  // no pasamos nada en este caso así que lo podemos poner directamente

        [System.Serializable]  // para que me salga en el editor
        public class TakeDamageEvent : UnityEvent<float>  // implementamos el unity event con generics (chevrons) (<tipo>) como subclase para que sea serializable y nos salga en el editor (importnate elegir el método como dynamics flow y no como parámetros estáticos así pilla el parámetro pasado a través del Invoke(float) que está abajo con el daño real y no lo estamos hardcodeando en el editor)
        {

        }

        LazyValue<float> healthPoints;   // queremos cambiar el valor en awake pero no podemos porque necesitamos acceder a una función de otra clase (getStat) y si lo ponemos en start es mala práctica porque a lo mejor hay otros starts que accedan a esto.
        // así que lo cambiamos de float a LazyValueFloat, una clase que nos asegura que se inicializa antes de su uso gracias a un delegado (GetInitialHealth()). 

        bool isDead = false;

        private void Awake() 
        {
            healthPoints = new LazyValue<float>(GetInitialHealth);  // inicializamos el lazyValue pero no estamos llamando a GetInitialHealth, solo se lo pasamos como delegado y ya lo usará más adelante cuando necesite usarlo (healthPoints.value)
        }
        
        private float GetInitialHealth()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);
        }

        private void Start() 
        {
            healthPoints.ForceInit();  // si nada ha accedido a healthPoints.value antes de esto, inicializalo como está ahora en este punto de la ejecución del programa porque puede que valores cambien más adelante y los necesitemos como están (más importante en otras implementaciones pero recomendable). 


            // if(healthPoints.value < 0)   // no queremos que start sobrescriba los puntos de vida recuperados del fichero en captureState en el caso de que se ejecute después
            // {
            //     healthPoints.value = GetComponent<BaseStats>().GetStat(Stat.Health);
            // }   
        }

        // buena práctica añadir notificaciones en enable y quitarlas en disable para no recibir callbacks cuando la clase está desactivada pero volvernos a suscribir cuando sí lo está
        private void OnEnable() 
        {
            GetComponent<BaseStats>().onLevelUp += RegenerateHealth;
        }

        private void OnDisable() 
        {
            GetComponent<BaseStats>().onLevelUp -= RegenerateHealth;
        }


        public bool IsDead()
        {
            return isDead;
        }


        public void TakeDamage(GameObject instigator, float damage)
        {
            healthPoints.value = Mathf.Max(healthPoints.value - damage, 0);

            if(healthPoints.value == 0)
            {
                onDie.Invoke();
                Die();
                AwardExperience(instigator);
            }
            else 
            {
                takeDamage.Invoke(damage);  // triggerea todas las funciones o métodos enlazados desde el editor
            }
        }

        public void Heal(float healthToRestore)
        {
            healthPoints.value = Mathf.Min(healthPoints.value + healthToRestore, GetMaxHealthPoints());   // no queremos tener más vida de la máxima obviamente
        }

        public float GetHealthPoints()
        {
            return healthPoints.value;
        }

        public float GetPercentage()
        {
            return 100 * GetFraction();
        }

        public float GetFraction()
        {
            return healthPoints.value / GetComponent<BaseStats>().GetStat(Stat.Health);
        }

        public float GetMaxHealthPoints()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);
        }

        private void Die()
        {
            if(isDead) return;
            isDead = true;
            GetComponentInParent<Animator>().SetTrigger("die");
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        
        private void AwardExperience(GameObject instigator)
        {
            Experience experience = instigator.GetComponent<Experience>();
            if(experience == null) return;

            experience.GainExperience(GetComponent<BaseStats>().GetStat(Stat.ExperienceReward));
        }

        private void RegenerateHealth()
        {
            float regenHealthPoints = GetComponent<BaseStats>().GetStat(Stat.Health) * (regenerationPercentage / 100);  // regeneramos un porcentaje de la vida base del nuevo nivel al subir de nivel si tenemos menos vida que eso
            healthPoints.value = Mathf.Max(healthPoints.value, regenHealthPoints);  // si tenemos más vida que eso, no regeneramos nada
        }

        public object CaptureState()
        {
            return healthPoints.value;  // las variables primitivas son todas serializables
        }


        public void RestoreState(object state)
        {
            healthPoints.value = (float) state;

            if(healthPoints.value == 0)
            {
                Die();
            }
        }
    }
}

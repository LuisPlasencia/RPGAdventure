using System;
using System.Collections.Generic;
using GameDevTV.Utils;
using RPG.Attributes;
using RPG.Core;
using RPG.Movement;
using RPG.Saving;
using RPG.Stats;
using UnityEngine;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction, ISaveable, IModifierProvider
    {

        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] Transform rightHandTransform = null;
        [SerializeField] Transform leftHandTransform = null;
        [SerializeField] WeaponConfig defaultWeapon = null;  
        // esta manera de cargar el arma no funcionaría si cambiamos de nivel porque es un link directo, es decir, si cambiamos de nivel Unity habrá perdido el link al scriptable object, y esq no lo habrá empaquetado entre niveles
        // el arma que cargaríamos en un Scene1 en runtime, no podemos accederla en un Scene2 al no ser que incluyamos el recurso en una carpeta de Resources

        Health target;
       //  LazyValue<WeaponConfig> currentWeaponConfig;   // ahora separamos weaponconfig del weapon así que el lazy value lo hacemos solo en el weapon en sí el cual hace llamada a spawn
        WeaponConfig currentWeaponConfig;
        LazyValue<Weapon> currentWeapon;   // sin embargo para la currentWeapon sí que llamamos al Spawn así que aquí sí lo necesitamos
        float timeSinceLastAttack = Mathf.Infinity;

        private void Awake() 
        {
            currentWeaponConfig = defaultWeapon;
            currentWeapon = new LazyValue<Weapon>(SetupDefaultWeapon);

            // currentWeaponConfig = new LazyValue<WeaponConfig>(SetupDefaultWeapon);  // inicializamos el lazyValue pero no estamos llamando a GetInitialHealth, solo se lo pasamos como delegado y ya lo usará más adelante cuando necesite usarlo (healthPoints.value)
        }

        private Weapon SetupDefaultWeapon()
        {
            return AttachWeapon(defaultWeapon);  // en el caso de que cambiemos de arma o carguemos otra del fichero de guardado, se llamará después de esto así que se cambiará insta el arma a la que corresponda
        }

        private void Start() {    
            currentWeapon.ForceInit();

            // Implementación con Lazy Value cuando teníamos que implicar al animator en attackweapon:
            // esto no va en awake porque llama al animator y llamar métodos de otras clases en awake es caca. Además, en las clases que lo usemos públicas lo inicializamos tambiénp orque puede haber otro start que las llame en otra clase y que se ejecute antes que este.
            // currentWeaponConfig.ForceInit();  // importante esto si no, no tendríamos un arma visible al iniciar el juego así que forzamos su llamada
        }

        private void Update() 
        {
            timeSinceLastAttack += Time.deltaTime;

            if (target == null) return; 
            if (target.IsDead()) return;

            if(!GetIsInRange(target.transform))
            {
                GetComponent<Mover>().MoveTo(target.transform.position, 1f);
            }
            else
            {
                GetComponent<Mover>().Cancel();
                AttackBehaviour();
            }
        }

        public void EquipWeapon(WeaponConfig weapon)
        {
            currentWeaponConfig = weapon;
            currentWeapon.value = AttachWeapon(weapon);
        }

        private Weapon AttachWeapon(WeaponConfig weapon)
        {
            Animator animator = GetComponent<Animator>();
            return weapon.Spawn(rightHandTransform, leftHandTransform, animator);
        }

        public Health GetTarget()
        {
            return target;
        }

        private void AttackBehaviour()
        {
            transform.LookAt(target.transform);
            if(timeSinceLastAttack > timeBetweenAttacks)
            {
                // This will trigger the Hit() event
                TriggerAttack();
                timeSinceLastAttack = 0;
            }

        }

        private void TriggerAttack()
        {
            GetComponent<Animator>().ResetTrigger("stopAttack");
            GetComponent<Animator>().SetTrigger("attack");
        }

        // Animation Event
        void Hit()
        {
            if(target == null) return;

            float damage = GetComponent<BaseStats>().GetStat(Stat.Damage);

            if(currentWeapon.value != null)
            {
                currentWeapon.value.OnHit();
            }

            if(currentWeaponConfig.HasProjectile())
            {
                currentWeaponConfig.LaunchProjectile(rightHandTransform, leftHandTransform, target, gameObject, damage);
            }
            else 
            { 
                target.TakeDamage(gameObject, damage);
            }

        }

        // bow animation event
        void Shoot()
        {
            Hit();
        }

        private bool GetIsInRange(Transform targetTransform)
        {
            return Vector3.Distance(transform.position, targetTransform.position) < currentWeaponConfig.GetRange();
        }

        public bool CanAttack(GameObject combatTarget)
        {
            if(combatTarget == null) return false;
            if(!GetComponent<Mover>().CanMoveTo(combatTarget.transform.position) && 
                !GetIsInRange(combatTarget.transform))  // el range para atacar a npcs que no estén en un lugar alcanzable por ejemplo con el arco
            {
                return false; 
            }

            Health targetToTest = combatTarget.GetComponent<Health>();
            return targetToTest != null && !targetToTest.IsDead();
        }

        public void Attack(GameObject combatTarget)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            target = combatTarget.GetComponent<Health>();
        }

        public void Cancel()
        {
            StopAttack();
            target = null;
            GetComponent<Mover>().Cancel();
        }

        private void StopAttack()
        {
            GetComponent<Animator>().ResetTrigger("attack");
            GetComponent<Animator>().SetTrigger("stopAttack");
        }

        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            if(stat == Stat.Damage)
            {
                yield return currentWeaponConfig.GetDamage();  // este IEnumerator es una lista vacía. Ventaja con una lista normal esq podemos devolver unas cosas y no otras (no hay otro return) o también podemos devolver más de una cosa, es decir, retornar más de una vez
            }
        }

        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
            if(stat == Stat.Damage)
            {
                yield return currentWeaponConfig.GetPercentageBonus();
            }
        }

        public object CaptureState()
        {
            return currentWeaponConfig.name;
            // Esto es necesario para poder acceder en tiempo de ejecución (runtime) a objetos no referenciados en el scene pero sí incluidos en la carpeta Resources 
            // una alternativa a usar strings es usar UUID sobre los elementos de la carpeta de resources. Es más complicado pero más seguro.
        }

        public void RestoreState(object state)
        {
            string weaponName = (string) state;
            WeaponConfig weapon = Resources.Load<WeaponConfig>(weaponName);   // tambien UnityEngine.Resources...
            EquipWeapon(weapon);
            // va a buscar en la carpeta resources por un arma llamada así. (esta carpeta se mantiene entre niveles así que lo que contiene dentro incluidos los links de los objetos se mantienen en el build)
            // en el build, unity descarta todo lo que no necesite, por eso necesitamos poner todas las cosas a las que querramos acceder dinámicamente, que no estén en el scene, en dicha carpeta de recursos
            // esta carpeta de recursos se puede poner en múltiples lugares y si queremos acceder a subcarpetas dentro de recursos lo haríamos así subcarpeta/nombreFichero
            // si tenemos múltiples carpetas de Resources, es importante que no se repitan los nombres de los elementos
        }
    }
}
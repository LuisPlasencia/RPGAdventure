using System;
using System.Collections;
using System.Collections.Generic;
using GameDevTV.Utils;
using RPG.Attributes;
using UnityEngine;

namespace RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [Range(1, 99)]
        [SerializeField] int startingLevel = 1;
        [SerializeField] CharacterClass characterClass;
        [SerializeField] Progression progression = null;
        [SerializeField] GameObject levelUpParticleEffect = null;  // gameobject ya que el efecto está contenido dentro del padre en este caso (ver prefab Level Up)
        [SerializeField] bool shouldUseModifiers = false;

        public event Action onLevelUp;
        LazyValue<int> currentLevel;

        Experience experience;
        
        private void Awake() {
            experience = GetComponent<Experience>();
            currentLevel = new LazyValue<int>(CalculateLevel);
        }

        private void Start() 
        {
            currentLevel.ForceInit();    
            
            // currentLevel = CalculateLevel(); // esto puede ser causa de un race condition, en el que otro start de otra clase se ejecute antes y llame a algo que necesite current level, por eso GetLevel() de esta clase lo llamamos también 
            // no lo ponemos en awake porque dentro hay funciones que acceden a otras clases y sabemos que acceder a funciones de otras clases en awake es un NO-NO  -> solucion = LazyValue para que se inicialice en la llamada o en start por default
        }

        private void OnEnable()  // despues de awake y antes de start - lo estamos usando para registrar un callback no para llamar a una función externa así que es perfecto para esta labor
        {  
            if(experience != null)
            {
                experience.onExperienceGained += UpdateLevel;  // no añadimos paréntesis a UpdateLevel porque no lo queremos llamar sino añadirlo a la lista de métodos del evento
            }
        }

        private void OnDisable()   // si algo desactiva esta clase, es buena práctica quitar las suscripciones al evento en OnDisable. (Si otra clase la activa en otro momento se registran otra vez en el OnEnable())
        {
                if(experience != null)
            {
                experience.onExperienceGained -= UpdateLevel;  // para que no recibamos callbacks cuando la clase no está activa la desuscribimos
            }
        }

        private void UpdateLevel() {
            int newLevel  = CalculateLevel();
            if(newLevel > currentLevel.value)
            {
                currentLevel.value = newLevel;
                LevelUpEffect();
                onLevelUp();
            }
        }

        private void LevelUpEffect()
        {
            Instantiate(levelUpParticleEffect, transform);  // en este caso indicamos el transform de segundo parámetro y no el vector3 ya que estamos señalando al padre al que lo queremos asociar y no la posición en el mundo
        }

        public float GetStat(Stat stat)
        {
            return (GetBaseStat(stat) + GetAdditiveModifier(stat)) * (1 + GetPercentageModifier(stat) / 100);
        }

        private float GetBaseStat(Stat stat)
        {
            return progression.GetStat(stat, characterClass, GetLevel());
        }

        private float GetAdditiveModifier(Stat stat)
        {
            if(!shouldUseModifiers) return 0;  // solo vamos a aplicar modifiers al player, no a los npc

            float total = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())  // necesitamos todos aquellos componentes que implementen modificadores (todos los componentes que tengan un IModifierProvider como interfaz)
            {
                foreach (float modifiers in provider.GetAdditiveModifiers(stat))  // iteramos por todos los métodos implementados GetAdditiveModifier de los componentes (este foreach no podemos hacerlo con un IEnumerator, por eso usamos un IEnumerable, todo lo demás es igual)
                {
                    total += modifiers;  // si hubiera mas de un yield return en el modifier suma todos. Si no hay yield return es una lista vacía así que no suma nada ni hace nada
                }
            }
            return total;
        }

        private float GetPercentageModifier(Stat stat)
        {
            if(!shouldUseModifiers) return 0;

            float total = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())  
            {
                foreach (float modifiers in provider.GetPercentageModifiers(stat)) 
                {
                    total += modifiers; 
                }
            }
            return total;
        }

        public int GetLevel()
        {
            return currentLevel.value;


            // if(currentLevel.value < 1)  // prevenir race condition (se llama en el start() de Health.cs y se seteaba en este Start() y no sabemos cual se ejecuta primero)
            // {
            //     currentLevel.value = CalculateLevel();
            // }   // todo esto nos lo hace ya el lazyValue
        }

        public int CalculateLevel()
        {
            Experience experience = GetComponent<Experience>();

            if(experience == null) return startingLevel;

            float currentXP = experience.GetPoints();
            int penultimateLevel = progression.GetLevels(Stat.ExperienceToLevelUp, characterClass);

            for ( int level = 1 ; level <= penultimateLevel; level++)
            {
                float XPToLevelUp = progression.GetStat(Stat.ExperienceToLevelUp, characterClass, level);
                if (XPToLevelUp > currentXP)
                {
                    return level;
                }
            }
            return penultimateLevel + 1;
        }
 
        
    }
}

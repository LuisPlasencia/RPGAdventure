using UnityEngine;
using System.Collections.Generic;
using System;

namespace RPG.Stats
{
[CreateAssetMenu(fileName = "Progression", menuName = "Stats/New Progression", order = 0)]
    public class Progression : ScriptableObject {
        [SerializeField] ProgressionCharacterClass[] characterClasses = null;

        // no podemos usar diccionarios en el editor (al buildear nuestro scriptableObject) al no ser un tipo serializable para Unity. Por ello lo vamos a construir en código con el método BuildLookup().
        Dictionary<CharacterClass, Dictionary<Stat, float[]>> lookupTable = null;

        public float GetStat(Stat stat, CharacterClass characterClass, int level)
        {
            BuildLookup();

            float[] levels = lookupTable[characterClass][stat];   // un diccionario accede a otro diccionario que me devuelve un array

            if (levels.Length < level)
            {
                return 0;
            }

            return levels[level - 1];


            // esta otra forma es muy costosa (expensive) y mala de hacer esto ya que GetStat es algo que usamos constantemente y dos for loops pueden joder mucho el rendimiento si aumentamos las variables -> usar diccionario mejor así solo necesitamos hacer los foreach una vez para construirlo 
            
            // foreach(ProgressionCharacterClass progressionClass in characterClasses)
            // {
            //     if(progressionClass.characterClass != characterClass) continue;
                
            //     foreach (ProgressionStat progressionStat in progressionClass.stats)
            //     {
            //         if (progressionStat.stat != stat) continue;

            //         if (progressionStat.levels.Length < level) continue;

            //         return progressionStat.levels[level - 1];
            //     }
            // }
            // return 0;
        }

        public int GetLevels(Stat stat, CharacterClass characterClass)
        {
            BuildLookup();
            float[] levels = lookupTable[characterClass][stat];
            return levels.Length;
        }

        private void BuildLookup()
        {
            if (lookupTable != null) return;  // si ya hemos construido el diccionario no hace falta volver a construirlo

            lookupTable = new Dictionary<CharacterClass, Dictionary<Stat, float[]>>();

            foreach (ProgressionCharacterClass progressionClass in characterClasses)
            {
                var statLookupTable = new Dictionary<Stat, float[]>();  // esto es lo mismo que poner Dictionary<Stat, float[]> statLookupTable = new Dictionary<Stat, float[]>();
                
                foreach (ProgressionStat progressionStat in progressionClass.stats)
                {
                    statLookupTable[progressionStat.stat] = progressionStat.levels;
                }
                
                lookupTable[progressionClass.characterClass] = statLookupTable; 
            }
        }

        [System.Serializable]
        class ProgressionCharacterClass
        {
            public CharacterClass characterClass;
            public ProgressionStat[] stats;
        }


        [System.Serializable]
        class ProgressionStat
        {
            public Stat stat;
            public float[] levels;
        }
    }    
}
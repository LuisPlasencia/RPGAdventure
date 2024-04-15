using System.Collections.Generic;

namespace RPG.Stats
{
    public interface IModifierProvider 
    {
        IEnumerable<float> GetAdditiveModifiers(Stat stat);  // más eficiente que una lista (al IEnumerable lo podemos recorrer haciendo un foreach, cosa que no podemos con un IEnumerator)
        IEnumerable<float> GetPercentageModifiers(Stat stat);
    }
}
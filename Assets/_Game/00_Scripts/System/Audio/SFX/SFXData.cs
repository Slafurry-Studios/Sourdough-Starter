using UnityEngine;

namespace Slafurry.System.Audio
{

    
    [CreateAssetMenu(fileName = "SFX Data", menuName = "Game/Audio/SFX Data")]
    public class SFXData : ScriptableObject
    {
        public SFXCategory[] categories;

        public SFXCategory GetCategory(string categoryName)
        {
            foreach (var cat in categories)
            {
                if (cat.categoryName == categoryName)
                    return cat;
            }
            return null;
        }

        public SFXEffect GetSFXEffect(string categoryName, string effectName)
        {
            var category = GetCategory(categoryName);
            if (category?.effects == null) return null;

            foreach (var effect in category.effects)
            {
                if (effect.groupID == effectName)
                    return effect;
            }
            return null;
        }
    }
}
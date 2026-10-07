using UnityEngine;

namespace Game.Systems.MainMenu
{
    public abstract class MenuConditionBase : ScriptableObject
    {
        public abstract bool IsMet();
    }
}

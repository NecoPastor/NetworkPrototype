using UnityEngine;

namespace Game.Systems.MainMenu
{
    public abstract class MenuActionBase : ScriptableObject
    {
        public abstract void Execute();
    }
}
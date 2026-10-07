using System;
using UnityEngine;

namespace Game.Systems.MainMenu
{
    [CreateAssetMenu(fileName = "StartNewGameAction", menuName = "MenuSystem/StartNewGame Action")]
    [Serializable]
    public class StartNewGameAction : MenuActionBase
    {
        public Action OnExecute;

        public override void Execute()
        {
            OnExecute?.Invoke();
        }
    }
}

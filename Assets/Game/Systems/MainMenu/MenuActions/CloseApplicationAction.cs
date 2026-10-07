using System;
using UnityEngine;

namespace Game.Systems.MainMenu
{
    [CreateAssetMenu(fileName = "CloseApplicationAction", menuName = "MenuSystem/CloseApplication Action")]
    [Serializable]
    public class CloseApplicationAction : MenuActionBase
    {
        public override void Execute() => Application.Quit();
    }
}

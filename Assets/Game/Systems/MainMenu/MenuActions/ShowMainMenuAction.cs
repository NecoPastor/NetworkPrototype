using Game.Systems.MainMenu.Runtime;
using System;
using UnityEngine;

namespace Game.Systems.MainMenu
{
    [CreateAssetMenu(fileName = "ShowMainMenuAction", menuName = "MenuSystem/ShowMainMenu Action")]
    [Serializable]
    public class ShowMainMenuAction : MenuActionBase
    {
        [SerializeField] private MenuConfig menuConfig;

        public override void Execute()
        {
            var builder = FindAnyObjectByType<MenuBuilder>();
            builder?.BuildMenu(menuConfig);
        }

    }
}

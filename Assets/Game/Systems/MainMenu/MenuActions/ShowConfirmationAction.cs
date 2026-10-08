using Game.Systems.MainMenu.Runtime;
using System;
using UnityEngine;

namespace Game.Systems.MainMenu
{
    [CreateAssetMenu(fileName = "ShowConfirmationAction", menuName = "MenuSystem/ShowConfirmation Action")]
    [Serializable]
    public class ShowConfirmationAction : MenuActionBase
    {
        [SerializeField] private MenuConfig confirmConfig;

        public override void Execute()
        {
            var builder = FindAnyObjectByType<MenuBuilder>();
            builder?.BuildMenu(confirmConfig);
        }
    }
}
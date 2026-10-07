using UnityEngine;

namespace Game.Systems.MainMenu
{
    [CreateAssetMenu(fileName = "MenuItemConfig", menuName = "MenuSystem/Menu Item Config")]
    public class MenuItemConfig : ScriptableObject
    {
        [Header("Visuals")]
        public string Title;
        public Sprite Icon;

        [Header("Behavior")]
        [SerializeReference] public MenuActionBase Action;
        [SerializeReference] public MenuConditionBase Condition;

    }
}

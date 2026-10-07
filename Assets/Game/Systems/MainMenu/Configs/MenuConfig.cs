using System.Collections.Generic;
using UnityEngine;

namespace Game.Systems.MainMenu
{
    [CreateAssetMenu()]
    public class MenuConfig : ScriptableObject
    {
        [Header("Header")]
        public string Title;

        [Header("Items")]
        public List<MenuItemConfig> Items = new();

        [Header("Layout")]
        public float ItemSpacing = 10f;
        public bool SortByCondition = false;
    }
}

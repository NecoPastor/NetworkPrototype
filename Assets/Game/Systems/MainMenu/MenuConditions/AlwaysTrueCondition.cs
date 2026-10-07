
using System;
using UnityEngine;

namespace Game.Systems.MainMenu
{
    [CreateAssetMenu(fileName = "AlwaysTrueCondition", menuName = "MenuSystem/AlwaysTrue Condition")]
    [Serializable]
    public class AlwaysTrueCondition : MenuConditionBase
    {
        public override bool IsMet() => true;
    }
}

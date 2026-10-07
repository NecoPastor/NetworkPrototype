using Game.Systems.GameStateMachine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameStateAttribute))]
public class GameStateDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Получаем все типы наследников ItemConfig
        var types = Assembly.GetAssembly(typeof(GameState))
                            .GetTypes()
                            .Where(t => t.IsSubclassOf(typeof(GameState)) && !t.IsAbstract)
                            .ToList();

        // Список имен для выпадающего списка
        var typeNames = new List<string> { "None" };
        typeNames.AddRange(types.Select(t => t.Name));

        // Текущий индекс выбранного типа
        int currentIndex = Mathf.Max(0, typeNames.IndexOf(property.stringValue));

        // Отображение выпадающего списка
        currentIndex = EditorGUI.Popup(position, label.text, currentIndex, typeNames.ToArray());

        // Обновление значения свойства
        property.stringValue = typeNames[currentIndex];
    }
}


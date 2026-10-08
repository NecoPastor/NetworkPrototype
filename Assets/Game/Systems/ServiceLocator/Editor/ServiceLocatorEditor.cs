using Game.Systems.Service;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ServiceLocator))]
public class ServiceLocatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ServiceLocator locator = (ServiceLocator)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Зарегистрированные сервисы", EditorStyles.boldLabel);

        var servicesField = typeof(ServiceLocator)
            .GetField("services", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (servicesField != null)
        {
            var services = servicesField.GetValue(locator) as Dictionary<System.Type, object>;
            if (services != null && services.Count > 0)
            {
                foreach (var kvp in services)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(kvp.Key.Name, GUILayout.Width(200));

                    if (kvp.Value is UnityEngine.Object unityObject)
                    {
                        EditorGUILayout.ObjectField(unityObject, typeof(UnityEngine.Object), true);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("(не Unity объект)");
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Список сервисов пуст.", MessageType.Info);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Не удалось получить доступ к словарю сервисов.", MessageType.Warning);
        }
    }
}
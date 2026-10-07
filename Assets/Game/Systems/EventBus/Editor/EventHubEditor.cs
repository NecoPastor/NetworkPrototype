using Game.Systems;
using System;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(EventHub), true)]
public class EventHubEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EventHub hub = (EventHub)target;

        if (hub.EventBus == null)
        {
            EditorGUILayout.HelpBox("EventBus is not initialized.", MessageType.Warning);
            return;
        }

        Dictionary<Type, List<Delegate>> subscribers = hub.EventBus.GetAllSubscribers();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("📋 EventBus Subscribers", EditorStyles.boldLabel);

        if (subscribers.Count == 0)
        {
            EditorGUILayout.LabelField("No subscribers registered.");
        }
        else
        {
            foreach (var kvp in subscribers)
            {
                string eventTypeName = kvp.Key.Name;
                int count = kvp.Value.Count;

                EditorGUILayout.LabelField($"• {eventTypeName}: {count} subscriber(s)");

                foreach (var del in kvp.Value)
                {
                    string methodName = del.Method.Name;
                    string targetName = del.Target?.ToString() ?? "null";
                    EditorGUILayout.LabelField($"   ↳ {methodName} ({targetName})", EditorStyles.miniLabel);
                }
            }
        }
    }
}

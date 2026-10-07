using Game.Systems.GameStateMachine;
using UnityEditor;

[CustomEditor(typeof(StateMachine))]
public class GameStateViewerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var stateMachine = (StateMachine)target;

        DrawDefaultInspector();

        string stateName = stateMachine.CurrentState?.GetType().Name ?? "None";
        EditorGUILayout.LabelField("Текущее состояние", stateName);
    }
}

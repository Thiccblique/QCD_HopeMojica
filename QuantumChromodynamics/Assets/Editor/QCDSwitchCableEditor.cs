using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QCDSwitchCableBuilder))]
public class QCDSwitchCableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        QCDSwitchCableBuilder builder = (QCDSwitchCableBuilder)target;

        GUILayout.Space(15);
        EditorGUILayout.LabelField("Cable Builder Controls", EditorStyles.boldLabel);
        GUILayout.Space(5);

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Add Straight Segment"))
        {
            builder.AddStraightSegment();
        }

        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Rotate 90°"))
        {
            builder.RotateDirection90();
        }

        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("Undo Last Segment"))
        {
            builder.UndoLastSegment();
        }

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Clear Cable"))
        {
            builder.ClearCable();
        }

        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Current Direction", EditorStyles.boldLabel);
        EditorGUILayout.Vector2Field("", builder.currentDirection);
        EditorGUILayout.TextField(builder.directionString);
    }
}
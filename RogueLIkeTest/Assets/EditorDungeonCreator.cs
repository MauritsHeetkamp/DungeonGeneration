using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonCreator))]
public class EditorDungeonCreator : Editor
{
    public override void OnInspectorGUI()
    {
        DungeonCreator thisCreator = (DungeonCreator)target;
        if (GUILayout.Button("Generate Dungeon"))
        {
            thisCreator.GenerateDungeon();
        }
        if (GUILayout.Button("Remove Dungeon"))
        {
            thisCreator.ClearDungeon();
        }
        base.OnInspectorGUI();
    }
}

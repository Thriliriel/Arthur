//used to do things in Editor time, like prepare the scenario
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MainController))]
public class EditorController : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var rect = GUILayoutUtility.GetRect(500, 40);

        rect = GUILayoutUtility.GetRect(500, 40);
        if (GUI.Button(rect, "Clear Memory"))
        {
            ClearMemory();
        }
    }

    public void ClearMemory()
    {
        (target as MainController).ClearMemoryFile();
    }
}
#endif
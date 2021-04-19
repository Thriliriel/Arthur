//used to do things in Editor time, like prepare the scenario
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof (MainController))]
public class EditorController : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var rect = GUILayoutUtility.GetRect(500, 40);

        //pre process for set all cells and obstacles
        /*if (GUI.Button(rect, "Pre-Process"))
        {
            PreProccess();
        }

        rect = GUILayoutUtility.GetRect(500, 40);
        if (GUI.Button(rect, "Save Config File"))
        {
            SaveConfigFile();
        }*/

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class terrainShaderGUI : ShaderGUI
{

    MaterialEditor editor;
  
    public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        this.editor = materialEditor;

        base.OnGUI (editor, properties);
        doAdvanced();
    }


    void doAdvanced(){
        GUILayout.Label("Advanced Options", EditorStyles.boldLabel);

		editor.EnableInstancingField();
    }
}

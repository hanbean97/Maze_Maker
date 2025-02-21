using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class inspectEdit : Editor
{
    GameManager gameManager;

    private void OnEnable()
    {
        gameManager = target as GameManager;
    }

}

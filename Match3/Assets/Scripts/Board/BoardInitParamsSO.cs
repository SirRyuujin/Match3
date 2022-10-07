using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Board Init Params", menuName = "Board/Parameters/Board Init Params")]
public class BoardInitParamsSO : ScriptableObject
{
    public FloatVariable rows;
    public FloatVariable columns;
}

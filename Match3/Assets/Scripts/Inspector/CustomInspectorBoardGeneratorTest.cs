using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(BoardGenerator))]
public class CustomInspectorBoardGeneratorTest : Editor
{
    public IntVariable RowsS;
    public IntVariable ColumnsS;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BoardGenerator generator = (BoardGenerator)target;
        if (GUILayout.Button("Generate Board"))
        {
            generator.GenerateBoard();
        }
    }
}
#endif
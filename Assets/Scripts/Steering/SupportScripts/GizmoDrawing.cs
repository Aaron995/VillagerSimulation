using UnityEngine;

public class GizmoDrawing
{
    public static void DrawRayWithDisc(Vector3 position, Vector3 direction, Color color)
    {
        //Draw a ray with a solid disc at the end
        if (direction.sqrMagnitude > 0.001f)
        {
            Debug.DrawRay(position, direction, color);
            UnityEditor.Handles.color = color;
            DrawDot(position + direction, color);
        }
    }

    public static void DrawLabel(Vector3 position, string label, Color color)
    {
        //Draw a label at a vertain position with provided colour
        UnityEditor.Handles.BeginGUI();
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.Label(position, label);
        UnityEditor.Handles.EndGUI();
    }

    public static void DrawDot(Vector3 position, Color color)
    {
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.DrawSolidDisc(position,Vector3.up, 0.25f);
    }

    public static void DrawCircle(Vector3 position, float size, Color color)
    {
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.DrawWireDisc(position, Vector3.up, size);
    }

    public static void DrawLine(Vector3 startPos, Vector3 endPos, Color color)
    {
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.DrawLine(startPos, endPos);
    }
}

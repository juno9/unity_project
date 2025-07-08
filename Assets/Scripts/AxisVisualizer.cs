using UnityEngine;

public class AxisVisualizer : MonoBehaviour
{
    public float axisLength = 5f;
    
    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // X축 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * axisLength);
        
        // Y축 (초록색)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * axisLength);
        
        // Z축 (파란색)
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.forward * axisLength);
        
        // 각 축의 끝에 레이블 표시
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.right * axisLength, "X");
        UnityEditor.Handles.Label(transform.position + Vector3.up * axisLength, "Y");
        UnityEditor.Handles.Label(transform.position + Vector3.forward * axisLength, "Z");
    }
#endif
} 
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UnitCubePrefab : MonoBehaviour
{
    // Awake 메서드 제거 - 초기화는 UnitPlacer에서 처리

#if UNITY_EDITOR
    [MenuItem("GameObject/Create HexTile Prefab", false, 10)]
    static void CreateHexTilePrefab()
    {
        GameObject hexTile = new GameObject("HexTile");
        var mf = hexTile.AddComponent<MeshFilter>();
        var mr = hexTile.AddComponent<MeshRenderer>();
        mf.mesh = CreateHexMesh();
        
        // Try URP shader first, fall back to standard shader if not available
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }
        
        if (shader != null)
        {
            mr.material = new Material(shader);
            mr.material.color = Color.green;
        }
        
        PrefabUtility.SaveAsPrefabAsset(hexTile, "Assets/Prefabs/HexTile.prefab");
        DestroyImmediate(hexTile);
        Debug.Log("HexTile.prefab has been created in Assets/Prefabs");
    }

    static Mesh CreateHexMesh()
    {
        Mesh mesh = new Mesh();
        float radius = 0.5f;
        Vector3[] vertices = new Vector3[7];
        vertices[0] = Vector3.zero;
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Deg2Rad * (60 * i);
            vertices[i + 1] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
        }
        int[] triangles = new int[18];
        for (int i = 0; i < 6; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i == 5 ? 1 : i + 2;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
#endif
} 
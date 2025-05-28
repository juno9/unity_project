using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UnitCubePrefab : MonoBehaviour
{
    void Awake()
    {
        var mr = gameObject.AddComponent<MeshRenderer>();
        var mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = CreateCubeMesh();
        mr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mr.material.color = Color.blue;
        gameObject.transform.localScale = Vector3.one * 0.7f;
    }

    Mesh CreateCubeMesh()
    {
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
        Destroy(temp);
        return mesh;
    }

#if UNITY_EDITOR
    [MenuItem("GameObject/Create HexTile Prefab", false, 10)]
    static void CreateHexTilePrefab()
    {
        GameObject hexTile = new GameObject("HexTile");
        var mf = hexTile.AddComponent<MeshFilter>();
        var mr = hexTile.AddComponent<MeshRenderer>();
        mf.mesh = CreateHexMesh();
        mr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mr.material.color = Color.green;
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
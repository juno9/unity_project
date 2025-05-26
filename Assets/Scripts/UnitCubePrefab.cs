using UnityEngine;

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
} 
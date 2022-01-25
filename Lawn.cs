using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Lawn : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        ShrinkCells sc = gameObject.AddComponent<ShrinkCells>();
        sc.shrinkAmtLerp = 0.1f;

        Realign();
    }
     void Realign()
    {

        //makes the transform position the centre of the mesh and moves the mesh vertices so the stay the same in world space
        Mesh mesh = GetComponent<MeshFilter>().mesh;


        transform.position = mesh.vertices[0];

        Vector3[] verts = mesh.vertices;
        List<Vector3> vertsList = new List<Vector3>();

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 point = verts[i] - transform.position;
            //    point.y = 0;
            vertsList.Add(point);
        }
        mesh.vertices = vertsList.ToArray();

        mesh.RecalculateBounds();
    }
    
	
	
}

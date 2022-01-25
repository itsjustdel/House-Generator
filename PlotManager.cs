using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlotManager : MonoBehaviour
{

    void Start()
    {
        gameObject.AddComponent<ReAlign>();

        HousePosition();
    }

    void HousePosition()
    {
        List<Vector3> edgePoints = GetComponent<FindEdges>().pointsOnEdge;
        
        Vector3 centroid = FindCentralPointByAverage(edgePoints.ToArray());

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = centroid;
    }
    public Vector3 FindCentralPointByAverage(Vector3[] mVertices)
    {
        float x = 0f;
        float y = 0f;
        float z = 0f;

        for (int i = 0; i < mVertices.Length; i++)
        {
            x += mVertices[i].x;
            y += mVertices[i].y;
            z += mVertices[i].z;
        }

        x = x / mVertices.Length;
        y = y / mVertices.Length;
        z = z / mVertices.Length;

        Vector3 centre = new Vector3(x, y, z);

        return centre;
    }
}

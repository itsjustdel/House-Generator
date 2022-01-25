using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GardenBorders : MonoBehaviour {

    public float width = 1f;
    public List<Vector3> pointsOnEdge;

	// Use this for initialization
	void Start ()
    {
        //findEdges script works out outline
        pointsOnEdge = GetComponent<FindEdges>().pointsOnEdge;

        //  BorderCubes(Borders(pointsOnEdge));

        //get inside border points
        List<Vector3> intersectionPoints = IntersectionPoints();

        InnerMeshFromPoints(pointsOnEdge, intersectionPoints);
       // CreateBezierFromPoints(intersectionPoints,10);
        
	}


    List<Vector3> IntersectionPoints()
    {
        List<Vector3> points = new List<Vector3>();
        List<Vector3> directions = new List<Vector3>();
        //create vectors inside polygon
        for (int i = 0; i < pointsOnEdge.Count; i++)
        {
            Vector3 p0 = pointsOnEdge[i];
            //points on edge is [1] here to create a vector to intersect with from the last point to the second point
            //this gives us the final intersect point at the first vector
            Vector3 p1 = pointsOnEdge[1];

            if (i != pointsOnEdge.Count - 1)
                p1 = pointsOnEdge[i + 1];


            Vector3 dir = p1 - p0;

            //always builds clockwise so we only need to rotate right(to the inside of the polygon)
            Vector3 normal = (Quaternion.Euler(0f, 90f, 0f) * dir);
            normal.Normalize();
            normal *= width;

            //move points inside
            p0 += normal;
            p1 += normal;
          
            points.Add(p0);
          
            directions.Add(dir);
            
        }
      
        //get intersection points
        
        List<Vector3> intersectionPoints = new List<Vector3>();
        //check for intersections and add to list
        for (int i = 0; i < points.Count; i++)
        {
            //miss the last point
            if (i < points.Count - 1)
            {
                Vector3 closestP1;
                Vector3 closestP2;
                BushesForCell.ClosestPointsOnTwoLines(out closestP1, out closestP2, points[i], directions[i], points[i + 1], directions[i + 1]);

                //we only need to use one
                intersectionPoints.Add(closestP1);
            }
        }

        //re order this list to make it easier to mesh. basically pushing the points round one in the polyong
        intersectionPoints.Insert(0, intersectionPoints[intersectionPoints.Count - 1]);
        //    intersectionPoints.RemoveAt(intersectionPoints.Count - 1);    //if we keep this we ahve full loop
        pointsOnEdge.Add(pointsOnEdge[0]);

        for (int i = 0; i < pointsOnEdge.Count; i++)
        {
            GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube1.transform.position = pointsOnEdge[i] + transform.position;
            cube1.name = "outer" + i.ToString();
        }
        for (int i = 0; i < intersectionPoints.Count; i++)
        {
            GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube1.transform.position = intersectionPoints[i] + transform.position;
            cube1.name = "inter" + i.ToString();
        }
        
        return intersectionPoints;
    }
    
    void InnerMeshFromPoints(List<Vector3> pointsOnEdge, List<Vector3> interSectionPoints)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for(int i = 0; i < interSectionPoints.Count-1; i++)
        {
            vertices.Add(pointsOnEdge[i]);
            vertices.Add(pointsOnEdge[i + 1]);
            vertices.Add(interSectionPoints[i+1]);
            vertices.Add(interSectionPoints[i]);
            
        }


        for (int i = 0; i < vertices.Count-2; i += 4)
        {
            triangles.Add(i);
            triangles.Add(i+1);
            triangles.Add(i+2);

            triangles.Add(i);
            triangles.Add(i + 2);
            triangles.Add(i + 3);
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles,0);
        

        GameObject border = new GameObject();
        border.transform.parent = transform;
        border.transform.position = transform.position;

        MeshFilter meshFilter =  border.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = border.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Resources.Load("Brown") as Material;



    }


    public static Vector2 LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num/*,offset*/;
        float x1lo, x1hi, y1lo, y1hi;

        Ax = p2.x - p1.x;
        Bx = p3.x - p4.x;

        Vector2 intersection = new Vector2(0, 0);
        // X bound box test/
        if (Ax < 0)
        {
            x1lo = p2.x; x1hi = p1.x;
        }
        else {
            x1hi = p2.x; x1lo = p1.x;
        }

        if (Bx > 0)
        {
            //if (x1hi < p4.x || p3.x < x1lo) return intersection;
        }
        else {
            //if (x1hi < p3.x || p4.x < x1lo) return intersection;
        }

        Ay = p2.y - p1.y;
        By = p3.y - p4.y;

        // Y bound box test//
        if (Ay < 0)
        {
            y1lo = p2.y; y1hi = p1.y;
        }
        else {
            y1hi = p2.y; y1lo = p1.y;
        }

        if (By > 0)
        {
            //if (y1hi < p4.y || p3.y < y1lo) return intersection;
        }
        else {
            //if (y1hi < p3.y || p4.y < y1lo) return intersection;
        }

        Cx = p1.x - p3.x;
        Cy = p1.y - p3.y;
        d = By * Cx - Bx * Cy;  // alpha numerator//
        f = Ay * Bx - Ax * By;  // both denominator//

        // alpha tests//
        if (f > 0)
        {
            //if (d < 0 || d > f) return intersection;
        }
        else {
            //if (d > 0 || d < f) return intersection;
        }

        e = Ax * Cy - Ay * Cx;  // beta numerator//

        // beta tests //
        if (f > 0)
        {
            //if (e < 0 || e > f) return intersection;
        }
        else {
            //if (e > 0 || e < f) return intersection;
        }

        // check if they are parallel
        if (f == 0) return intersection;
        // compute intersection coordinates //
        num = d * Ax; // numerator //
                      //    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;   // round direction //
                      //    intersection.x = p1.x + (num+offset) / f;
        intersection.x = p1.x + num / f;
        num = d * Ay;
        //    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;
        //    intersection.y = p1.y + (num+offset) / f;
        intersection.y = p1.y + num / f;

        return intersection;

    }

    void CreateBezierFromPoints(List<Vector3> points, float frequency)
    {
        BezierSpline bezierSpline = gameObject.AddComponent<BezierSpline>();
        List<Vector3> pointsForBezier = new List<Vector3>();
        List<BezierControlPointMode> splineControlPointList = new List<BezierControlPointMode>();
        for (int i = 0; i < points.Count; i++)
        {
            pointsForBezier.Add(points[i]);
            splineControlPointList.Add(BezierControlPointMode.Free);
            //don't do on last one
          //  if (i < points.Count - 1)
          //  {
                Vector3 nextPoint = points[i];
            if (i != points.Count - 1)
                nextPoint = points[i + 1];

            Vector3 controlPointPosition1 = Vector3.Lerp(points[i], nextPoint, 0.25f);
                Vector3 controlPointPosition2 = Vector3.Lerp(points[i], nextPoint, 0.75f);

                pointsForBezier.Add(controlPointPosition1);
                splineControlPointList.Add(BezierControlPointMode.Free);
                pointsForBezier.Add(controlPointPosition2);
                splineControlPointList.Add(BezierControlPointMode.Free);
         //   }
          //  else
          //  {
                //pointsForBezier.Add(points[i]);
                //splineControlPointList.Add(BezierControlPointMode.Free);
           // }
        }

        bezierSpline.points = pointsForBezier.ToArray();
        bezierSpline.modes = splineControlPointList.ToArray();

       
        float stepSize = frequency;
        stepSize = 1f / (stepSize);

        for (int i = 0; i < frequency; i++)
        {
            Vector3 pos = bezierSpline.GetPoint(i * stepSize);

           // GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
          //  cube.transform.position = pos;
          //  cube.name = "CurveCube";
          //  cube.transform.parent = transform;
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPlanner : MonoBehaviour {


    public static List<int> WallsWithNoDoors(Vector3[] vertices,List<GameObject> doors)
    {
        List<int> walls = new List<int>();

        //create loop with vertices
        List<Vector3> verticesList = new List<Vector3>();

        foreach (Vector3 v3 in vertices)
            verticesList.Add(v3);
        verticesList.Add(vertices[0]);        

        //check if do is in line with wall
        for (int i = 0; i < verticesList.Count - 1; i++)
        {
            Vector3 directionToNextCorner = (verticesList[i + 1] - verticesList[i]).normalized;
            for (int j = 0; j < doors.Count; j++)
            {
                Vector3 directionToDoor = (doors[j].transform.position - verticesList[i]).normalized;

                if(directionToDoor == directionToNextCorner)
                {
                    Debug.Log("Door in line");

                }
            }
            
        }

        return walls;
    }

    public static int FurthestVerticesFromDoors(List<GameObject> doors,Vector3[] vertices,List<int> toSkip)
    {
        //take an average of these doors
        Vector3 average = Vector3.zero;
        foreach (GameObject door in doors)
        {
            average += door.transform.position;
        }
        //divide by how many doors
        average /= doors.Count;

        //find furthest corner
        
        float distance = 0;
        int furthest = 0;
        for (int i = 0; i < vertices.Length; i++)
        {
            float temp = Vector3.Distance(average, vertices[i]);
            if (temp > distance)
            {   
                if (!toSkip.Contains(i))
                {
                    furthest = i;
                    distance = temp;
                }
            }
        }

        return furthest;
    }

    public static List<GameObject> NameUpstairsRooms(List<GameObject> quads)
    {
        //quads already in size order from another function
        for (int i = 0; i < quads.Count; i++)
        {
            //smallest upstairs room is bathroom    
            if (i == 0)
            {
                quads[i].name = "Bathroom";

                quads[i].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
            }
            //all the rest are bedrooms
            else
            {
                quads[i].name = "Bedroom";
                quads[i].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Green") as Material;
            }
        }

        return quads;
    }

    public static void FirstFloorLayoutCentreStairs(GameObject gameObject, GameObject firstFloor,GameObject plot,float storeyHeight,GameObject stairCollider)
    {
       // Debug.Break();
        //find if stairs is aligned with longest edge
        Vector3[] plotVertices = plot.GetComponent<MeshFilter>().mesh.vertices;
        int[] longestEdge = Divide.LongestEdge(plotVertices);
        int[] shortestEdge = Divide.ShortestEdge(plotVertices);
        float shortestDistance = Vector3.Distance(plotVertices[shortestEdge[0]], plotVertices[shortestEdge[1]]);
        bool stairsFacingLong = false;
        if((plotVertices[longestEdge[1]] - plotVertices[longestEdge[0]]).normalized == stairCollider.transform.forward || (plotVertices[longestEdge[0]] - plotVertices[longestEdge[1]]).normalized == stairCollider.transform.forward)
        {
            Debug.Log("Stairs facing long");
            stairsFacingLong = true;
        }

            //centre of stairs
        Vector3 centreOfStairs = stairCollider.transform.position;
        //move to "hall width" each side*0.5f (make one side landing *1f), hall length for z axis + half a length forward, storeyheight for y pos
        float hallLength = stairCollider.transform.localScale.z;
        float hallWidth = stairCollider.transform.localScale.x;

        Vector3 hallWidthDir = stairCollider.transform.right * hallWidth*0.5f;
        bool widenHall = true; //when ? when more than 2 rooms?
        if (stairsFacingLong)
            widenHall = false;

        Vector3 widenHallDir = Vector3.zero;
        if (widenHall)
            widenHallDir = stairCollider.transform.right*hallWidth;
        bool changeHallDir = false; //when? , when facing away from centre of plot?
        Vector3 plotCentre = plot.GetComponent<MeshRenderer>().bounds.center;
        if (Vector3.Distance(plot.GetComponent<MeshRenderer>().bounds.center, stairCollider.transform.position + stairCollider.transform.right) < Vector3.Distance(plotCentre, stairCollider.transform.position - stairCollider.transform.right))
        {
            //Debug.Log("right is less");
        }
        else
            changeHallDir = true;

        //move everythin to the other side of stairs if needed 

        Vector3 p1 = centreOfStairs + (hallWidthDir + widenHallDir) - (stairCollider.transform.forward * hallLength * .5f);
        Vector3 p2 = centreOfStairs - hallWidthDir - (stairCollider.transform.forward * hallLength * .5f);
        Vector3 p3 = centreOfStairs + (hallWidthDir + widenHallDir) + (stairCollider.transform.forward * hallLength * .5f);
        Vector3 p4 = centreOfStairs - hallWidthDir + (stairCollider.transform.forward * hallLength * .5f);
        Vector3[] pointsForStairSpace = new Vector3[4] { p1, p2, p3, p4 };

        

        // we need to check the landing we have tried to put is inside plot limits
        //we can do this quickly by adding a box collider to the plot game object
        BoxCollider bc = plot.AddComponent<BoxCollider>();
        plot.SetActive(true);
        bc.size = new Vector3(bc.size.x + .1f, storeyHeight*3, bc.size.z + .1f); //adding .1 for innacuracy(tolerance)
        

        

        //note, this can mean width can be out of bounds - does this ever happen?
        if (changeHallDir && widenHall)
        {
            for (int i = 0; i < pointsForStairSpace.Length; i++)
            {
                pointsForStairSpace[i] -= stairCollider.transform.right * hallWidth;// * 2;
            }
        }

        foreach(Vector3 v3 in pointsForStairSpace)
        {
          //  GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
          ///  c.transform.position = v3;
         //   c.name = "Stair";
         //   c.transform.parent = gameObject.transform;
             
        }

        GameObject stairsSpace = Divide.Quad(gameObject, pointsForStairSpace);
        //move quad up storey level
        stairsSpace.transform.position += storeyHeight * Vector3.up;
        stairsSpace.name = "StairSpace";

        p1 = centreOfStairs + (hallWidthDir +  widenHallDir) - (stairCollider.transform.forward * hallLength * .5f);
        p2 = centreOfStairs - hallWidthDir - (stairCollider.transform.forward * hallLength * .5f);
        p3 = centreOfStairs + (hallWidthDir + widenHallDir) - (stairCollider.transform.forward * hallLength * 1f);
        p4 = centreOfStairs - hallWidthDir - (stairCollider.transform.forward * hallLength * 1f);

        Vector3[] pointsForLanding = new Vector3[4] { p1, p2, p3, p4 };
       
        if (changeHallDir && widenHall)
        {
            for (int i = 0; i < pointsForLanding.Length; i++)
            {
                pointsForLanding[i] -= stairCollider.transform.right * hallWidth;// * 2;
            }
        }

        bool allInside = true;
        bool closeToCorner = false;
        float distanceToCorner = Mathf.Infinity;
        for (int i = 0; i < pointsForLanding.Length; i++)
        {
            if (!bc.bounds.Contains(pointsForLanding[i]))
                allInside = false;

            //check if any point on the landing is too close to a corner of the house, if it is, extend to the edge of the house. No point in having a left over slither
            
            for (int j = 0; j < plotVertices.Length; j++)
            {
                float t = Vector3.Distance(pointsForLanding[i], plotVertices[j]);
                if (t < 5)
                {
                    

                    closeToCorner = true;

                    if (t < distanceToCorner)
                        distanceToCorner = t;
                }
            }
           // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
           // c.transform.position = pointsForLanding[i];
           // c.name = i.ToString();
           // c.transform.parent = gameObject.transform;
        }

        if(closeToCorner) //and on the outside
        {
            Debug.Log("Close to corner - If points nt on outside, distance to corner is wrong?");
                
            pointsForLanding[2] -= stairCollider.transform.forward * distanceToCorner;
            pointsForLanding[3] -= stairCollider.transform.forward * distanceToCorner;

            //Debug.Break();
        }

        Vector3[] pointsForLanding2 = new Vector3[0];
        if (!allInside)
        {
            pointsForLanding = new Vector3[4] { pointsForStairSpace[2], pointsForStairSpace[3], pointsForStairSpace[2] + stairCollider.transform.forward * hallWidth, pointsForStairSpace[3] + stairCollider.transform.forward * hallWidth };

            //landing 2 - could extend landin for a big hall or create small quad -could be cupbaords or a bathroom?
            pointsForLanding2 = new Vector3[4] { pointsForStairSpace[0] + stairCollider.transform.forward*shortestDistance , pointsForStairSpace[1] + stairCollider.transform.forward * shortestDistance, pointsForStairSpace[2] + stairCollider.transform.forward * hallWidth, pointsForStairSpace[3] + stairCollider.transform.forward * hallWidth };

            GameObject landing2 = Divide.Quad(gameObject, pointsForLanding2);
            //move quad up storey level
            landing2.transform.position += storeyHeight * Vector3.up;
            landing2.name = "Landing2";

            Debug.Log("!inside");
            
            //if(gameObject.GetComponent<Divide>().adjacentRooms.Count == 1)
            //    Debug.Break();

        }

        GameObject landing = Divide.Quad(gameObject, pointsForLanding);
        //move quad up storey level
        landing.transform.position += storeyHeight * Vector3.up;
        landing.name = "Landing";

        
        
        //make quads at each side of the stairs_lengthways to the end of the plot- this will split up the plot in to to 3 parallel sections
        //find intersection points on edge of plot mesh
        Mesh plotMesh = plot.GetComponent<MeshFilter>().mesh;
        List<Vector3> plotLoop = new List<Vector3>();
        foreach (Vector3 v3 in plotMesh.vertices)
            plotLoop.Add(v3);
        plotLoop.Add(plotLoop[0]);

        
        //save what we build
        List<GameObject> quadsBuilt = new List<GameObject>();

        List<Vector3> intersectionPoints = new List<Vector3>();
        Vector3[] points = new Vector3[] { pointsForLanding[2], pointsForLanding[3], pointsForStairSpace[2], pointsForStairSpace[3] };
        if (stairsFacingLong)//split this in to two, long ways facing stairs and short way facing stairs, perhaps one solution could handle both in the end
        {

            if (closeToCorner && !allInside)
            {
               // Debug.Break();
                Debug.Log("Broke from here");//shouldnt get here now - IT CAN
            }

            //Debug.Break();
            //Debug.Log("Broke from here");

            //find which points are on the outside
            Vector3[] pointsForQuad = new Vector3[] { pointsForLanding[1], pointsForLanding[3], pointsForStairSpace[1], pointsForStairSpace[3] };
            //change points
            //points = new Vector3[] { pointsForLanding[1], pointsForLanding[3], pointsForStairSpace[1], pointsForStairSpace[3] };
            if (Vector3.Distance(pointsForLanding[0], plotCentre) < Vector3.Distance(pointsForLanding[1], plotCentre))
            {


                pointsForQuad = new Vector3[] { pointsForLanding[0], pointsForLanding[2], pointsForStairSpace[0], pointsForStairSpace[2] };
                //Debug.Break();
            }



            //Findintersection point with other side - use shortest distance - hall width 8 across direction

            for (int i = 0; i < pointsForQuad.Length; i++)
            {
                //GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //c.transform.position = points[i];
                //c.name = "POINTS";
               // c.transform.parent = gameObject.transform;
            }


            //now get dir across and add it to these points
            Vector3 dirAcross = (pointsForLanding[0] - pointsForLanding[1]).normalized;
            
            if (Vector3.Distance(pointsForLanding[1] + dirAcross, plotCentre) > Vector3.Distance(pointsForLanding[1] - dirAcross, plotCentre))
                dirAcross = -dirAcross;
          
            //GameObject d = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //d.transform.position = pointsForQuad[0] + dirAcross;            
            //d.transform.parent = gameObject.transform;
            //d.name = " Direction";

            p1 = pointsForQuad[1];
            p2 = pointsForQuad[3];

            
            //intersect points with plot vertices
            List<Vector3[]> intersectionPairs = new List<Vector3[]>();
            Vector3 intersect = Vector3.zero;
            Vector3 intersect2 = Vector3.zero;
            for (int i = 0; i < plotLoop.Count-1; i++)
            {
                
                if (Divide.LineLineIntersection(out intersect, plotLoop[i], (plotLoop[i + 1] - plotLoop[i]).normalized, p1, dirAcross))
                {
                    
                    //d = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //d.transform.position = intersect;
                    //d.transform.parent = gameObject.transform;
                    //d.name = "intersect";

                
                    if (Divide.LineLineIntersection(out intersect2, plotLoop[i], (plotLoop[i + 1] - plotLoop[i]).normalized, p2, dirAcross))
                    {
                       
                        //d = GameObject.CreatePrimitive(PrimitiveType.Cube);
                      //  d.transform.position = intersect2;
                      //  d.transform.parent = gameObject.transform;
                      //  d.name = "intersect2";

                        Vector3[] pair = new Vector3[] { intersect, intersect2 };
                        intersectionPairs.Add(pair);


                        
                    }
                }
            }

        

            //now find which pair corresponds to which two points on the landing - the two closest
            Vector3 landingMp1 = Vector3.Lerp(pointsForQuad[0], pointsForQuad[2], 0.5f);
            Vector3 landingMp2 = Vector3.Lerp(pointsForQuad[1], pointsForQuad[3], 0.5f);

           // Vector3 pair1Mp1 = Vector3.Lerp(intersectionPairs[0][0], intersectionPairs[0][1], 0.5f);
           // Vector3 pair1Mp2 = Vector3.Lerp(intersectionPairs[1][0], intersectionPairs[1][1], 0.5f);
           
            for (int i = 0; i < intersectionPairs.Count; i++)
            {
                Vector3[] pair = intersectionPairs[i];
                //mid point of this pair
                Vector3 midpoint1 = Vector3.Lerp(pair[0], pair[1], 0.5f);

                //measure to mp of a landing pairs
                float distanceToLanding1 = Vector3.Distance(midpoint1, landingMp1);
                float distanceToLanding2 = Vector3.Distance(midpoint1, landingMp2);

                //if closer then,

                p3 = pointsForQuad[0];
                p4 = pointsForQuad[2];
                if (distanceToLanding1 > distanceToLanding2)
                {
                    p3 = pointsForQuad[1];
                    p4 = pointsForQuad[3];
                }

                //make quad from this intersection pair and the closest pair from stairspace/landing (points)
                //double check we aren't building on top of landing
                Vector3[] quadPoints = new Vector3[] { pair[0], pair[1], p3, p4 };
                int shared = 0;
                for (int a = 0; a < quadPoints.Length; a++)
                {
                    for (int b = 0; b < points.Length; b++)
                    {

                        if (quadPoints[a] == points[b])
                            shared++;
                    }
                }

                
                if (shared==2)
                {

                    for (int j = 0; j < pointsForQuad.Length; j++)
                    {
                      //  GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                      //  c.transform.position = quadPoints[j];
                      //  c.name = "QUAD POINTS " + i.ToString();
                      //  c.transform.parent = gameObject.transform;
                    }

                    GameObject quad = Divide.Quad(gameObject, quadPoints);
                    quad.transform.position += Vector3.up * storeyHeight;

                    quadsBuilt.Add(quad);

                    //intersectionPoints.Add(pair[0]);
                   // intersectionPoints.Add(pair[1]);

                    //now fill other half

                    //find  plot vertices which hasn't been buitl by either last quad or landing or stairspace
                    List<Vector3> freeVertices = new List<Vector3>();
                    //make listof all the points we ahve sued so far
                    List<Vector3> firstHalfPoints = new List<Vector3>();
                    foreach (Vector3 v3 in quadPoints)
                        firstHalfPoints.Add(v3);
                    foreach (Vector3 v3 in points)
                    {
                        if(!firstHalfPoints.Contains(v3))
                            firstHalfPoints.Add(v3);    
                    }

                    for (int j = 0; j < firstHalfPoints.Count; j++)
                    {
                       // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                      //  c.transform.position = firstHalfPoints[j];
                      //  c.transform.parent = gameObject.transform;
                      //  c.name = "first half";
                    }

                    List<Vector3> secondHalfPoints = new List<Vector3>();

                    for (int j = 0; j < plotLoop.Count-1; j++)
                    {
                        bool free = true;
                        for (int k = 0; k < firstHalfPoints.Count; k++)
                        {
                            if (plotLoop[j] == firstHalfPoints[k])
                                free = false;
                        }
                        if(free)
                        {
                           // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                          //  c.transform.position = plotLoop[j];
                         //   c.transform.parent = gameObject.transform;
                          //  c.name = "FREE";

                            secondHalfPoints.Add(plotLoop[j]);
                            //find the closest intersct
                            float distance = Mathf.Infinity;
                            Vector3 closest = Vector3.zero;
                            for (int k = 0; k < firstHalfPoints.Count; k++)
                            {
                                float temp = Vector3.Distance(plotLoop[j], firstHalfPoints[k]);
                                if(temp < distance)
                                {
                                    distance = temp;
                                    closest = firstHalfPoints[k];
                                }
                            }
                            secondHalfPoints.Add(closest);
                         //   c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        //    c.transform.position = closest;
                        //    c.transform.parent = gameObject.transform;
                        //    c.name = "CLOSEST" + j.ToString();

                            free = false;

                        }
                    }

                    quad = Divide.Quad(gameObject, secondHalfPoints.ToArray());
                    quad.transform.position += Vector3.up * storeyHeight;

                    quadsBuilt.Add(quad);
                }
            }

            Debug.Log("quads built = " + quadsBuilt.Count);
            
            

        }
       
        else
        
        {
            //Debug.Break();
           
            for (int i = 0; i < plotLoop.Count - 1; i++)
            {
                Vector3 plotVector = plotLoop[i] - plotLoop[i + 1];
                
                //to p2
                Vector3 stair1 = points[2] - points[0];
                //to p3
                Vector3 stair2 = points[3] - points[1];

                Vector3 intersectPoint = new Vector3();
                Vector3 closest1 = points[0];
                bool intersect = Divide.LineLineIntersection(out intersectPoint, points[0], stair1, plotLoop[i], plotVector);
                if (intersect)
                    if (Vector3.Distance(intersectPoint, points[0]) > Vector3.Distance(intersectPoint, points[2]))
                        closest1 = points[2];

                Vector3 intersectPoint2 = new Vector3();
                Vector3 closest2 = points[1];
                bool intersect2 = Divide.LineLineIntersection(out intersectPoint2, points[1], stair2, plotLoop[i], plotVector);
                if (intersect2)
                    if (Vector3.Distance(intersectPoint2, points[1]) > Vector3.Distance(intersectPoint2, points[3]))
                        closest2 = points[3];

                if (intersect && intersect2)
                {
                    if (allInside)
                    {
                        Vector3[] quadPoints = new Vector3[] { closest1, intersectPoint, closest2, intersectPoint2 };

                        allInside = true;
                        for (int j = 0; j < quadPoints.Length; j++)
                        {
                            if (!bc.bounds.Contains(quadPoints[j]))
                            {
                                allInside = false;

                               // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                               // c.transform.position = points[j];
                               // c.transform.parent = gameObject.transform;
                            }
                        }
                        bool allUnique = true;
                        for (int a = 0; a < quadPoints.Length; a++)
                        {
                            for (int b = 0; b < quadPoints.Length; b++)
                            {
                                if (a == b)
                                    continue;

                                if (quadPoints[a] == quadPoints[b])
                                    allUnique = false;
                            }
                        }
                        if (allUnique)
                        {
                            GameObject hallExt = Divide.Quad(gameObject, quadPoints);
                            hallExt.name = i.ToString();

                            hallExt.transform.position += Vector3.up * storeyHeight;
                        }
                        else
                        {
                            //Debug.Break();
                            Debug.Log("Successfully stopped quad building cos it is squashed");
                        }

                        //hallExt.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;
                    }

                    intersectionPoints.Add(intersectPoint);
                    intersectionPoints.Add(intersectPoint2);

                }
            }

            //create quads at each side using points in intersection Points worked out above

            for (int i = 0; i < plotLoop.Count - 1; i ++)//+= 2 makes it just find points from each corner
            {
                Vector3 closestIntersectionPoint = intersectionPoints[0];
                float distance = Mathf.Infinity;

                for (int j = 0; j < intersectionPoints.Count; j++)
                {

                    float temp = Vector3.Distance(plotLoop[i], intersectionPoints[j]);
                    if (temp < distance)
                    {
                        closestIntersectionPoint = intersectionPoints[j];
                        distance = temp;
                    }

                }

               // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //c.transform.position = plotLoop[i];
                //c.name = "plot " + i.ToString();
                //c.transform.parent = gameObject.transform;
                //c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //c.transform.position = closestIntersectionPoint;
                //c.name = "intersect " + i.ToString();
                //c.transform.parent = gameObject.transform;

                Vector3 plotPoint2 = Vector3.zero;
                bool plot2Found = false;
                //find 3rd point - in line  with 90 degree spin
                for (int j = 0; j < plotLoop.Count - 1; j++)
                {
                    if (i == j)
                        continue;

                    Vector3 dir = plotLoop[i] - closestIntersectionPoint;
                    dir = Quaternion.Euler(0, -90, 0) * dir;
                    if (Divide.PointsInLine(plotLoop[i], plotLoop[j], plotLoop[i] + dir.normalized))
                    {
                         //c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                         //c.transform.position = plotLoop[j];
                         //c.name = "other plot point " + i.ToString();
                        //c.transform.parent = gameObject.transform;

                        plotPoint2 = plotLoop[j];

                        plot2Found = true;
                    }
                    else
                    {
                        dir = closestIntersectionPoint - plotLoop[i];
                        dir = Quaternion.Euler(0, 90, 0) * dir;
                        if (Divide.PointsInLine(plotLoop[i], plotLoop[j], plotLoop[i] + dir.normalized))
                        {
                          //   c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            // c.transform.position = plotLoop[j];
                             //c.name = "other plot point - -() " + i.ToString();
                            //c.transform.parent = gameObject.transform;

                            plotPoint2 = plotLoop[j];
                            plot2Found = true;
                        }
                    }
                }

                if (!plot2Found)
                {
                    Debug.Log("plot 2 not found");
                    //Debug.Break();
                }
                else
                {
                    distance = Mathf.Infinity;
                    Vector3 lastPoint = Vector3.zero;
                    //now find closest intersection point to this 3 point - it will make a rectangle
                    for (int j = 0; j < intersectionPoints.Count; j++)
                    {
                        float temp = Vector3.Distance(plotPoint2, intersectionPoints[j]);
                        if (temp < distance)
                        {
                            lastPoint = intersectionPoints[j];
                            distance = temp;
                        }
                    }

                    //c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //c.transform.position = lastPoint;
                    //c.transform.name = "Last Point " + i.ToString();
                    //c.transform.parent = gameObject.transform;

                    points = new Vector3[] { plotLoop[i], closestIntersectionPoint, plotPoint2, lastPoint };
                    //check they are all unique
                    bool allUnique = true;
                    for (int a = 0; a < points.Length; a++)
                    {
                        for (int b = 0; b < points.Length; b++)
                        {
                            if (a == b)
                                continue;

                            if (points[a] == points[b])
                                allUnique = false;
                        }
                    }
                    if (allUnique)
                    {
                        GameObject quad = Divide.Quad(gameObject, points);
                        quad.transform.position += Vector3.up * storeyHeight;

                        quadsBuilt.Add(quad);
                    }
                }
            }
           

           
        }

        //check if quads have access to landing
        List<GameObject> sharedWithLanding = new List<GameObject>();
        List<GameObject> noAccessToLanding = new List<GameObject>();
        for (int i = 0; i < quadsBuilt.Count; i++)
        {
            Vector3[] quadVertices = quadsBuilt[i].GetComponent<MeshFilter>().mesh.vertices;

            for (int j = 0; j < quadVertices.Length; j++)
            {
                for (int k = 0; k < pointsForLanding.Length; k++)
                {
                    if (quadVertices[j] == pointsForLanding[k])
                    {
                        if (!sharedWithLanding.Contains(quadsBuilt[i]))
                            sharedWithLanding.Add(quadsBuilt[i]);


                        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        c.transform.position = quadVertices[j];
                        c.transform.parent = quadsBuilt[i].transform;
                    }
                }

                //check 2nd landing too
                for (int k = 0; k < pointsForLanding2.Length; k++)
                {
                    if (quadVertices[j] == pointsForLanding2[k])
                    {
                        if (!sharedWithLanding.Contains(quadsBuilt[i]))
                            sharedWithLanding.Add(quadsBuilt[i]);


                        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        c.transform.position = quadVertices[j];
                        c.transform.parent = quadsBuilt[i].transform;
                    }
                }
            }

            //if it never added this quad[i] to shared by now, it means there is no access
            if (!sharedWithLanding.Contains(quadsBuilt[i]))
                noAccessToLanding.Add(quadsBuilt[i]);


           // Debug.Break();

            Debug.Log("Shared with Landing " + sharedWithLanding.Count);
            //create a hall by spliotting the quad with a shared vertice
            for (int j = 0; j < sharedWithLanding.Count; j++)
            {
                if (j == 0)
                {
                    //pretty sure i can slim this down and create a recurring function, it is ok just now because i'm only dividing a few times- if i was doing a larger area, perhaps it would make sense to do it

                    //save before we destroy gO
                    Vector3 spinPoint = sharedWithLanding[j].GetComponent<MeshRenderer>().bounds.center;

                   // bool lengthwaysHall = true;
                   // if (stairsFacingLong)
                   //     lengthwaysHall = false;
                    List<GameObject> splits = Divide.Split(gameObject, sharedWithLanding[j], 0.3f, false, stairsFacingLong);
                    //hall can be on other side, check and swap if needed
                    
                    SwitchSmallerRoomNearPoint(splits, centreOfStairs, spinPoint, false);


                    foreach (GameObject s in splits)
                        s.transform.position += Vector3.up * storeyHeight;

                    return;
                    
                    //split larger room? - will need to make a hallway
                    GameObject larger = splits[0];
                    GameObject smaller = splits[1];
                    if (larger.GetComponent<MeshRenderer>().bounds.size.sqrMagnitude < splits[1].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude)
                    {
                        larger = splits[1];
                        smaller = splits[0];
                    }

                    splits = Divide.Split(gameObject, larger, 0.3f, false, false);
                    foreach (GameObject s in splits)
                        s.transform.position += Vector3.up * storeyHeight;


                    //other room we are trying to give access to
                    Vector3 centreOfOtherRoom = larger.GetComponent<MeshRenderer>().bounds.center;
                    SwitchSmallerRoomNearPoint(splits, centreOfStairs, centreOfOtherRoom, false);

                    //now split main room
                    splits = Divide.Split(gameObject, noAccessToLanding[0], 0.0f, false, false);
                    foreach (GameObject s in splits)
                        s.transform.position += Vector3.up * storeyHeight;


                    GameObject needsAccess = null;

                    //it's possible one of these rooms created doesn't have access to a hallway - check - if it doesnt have access it shares two outer plot points
                    for (int k = 0; k < splits.Count; k++)
                    {
                        int count = 0;
                        Vector3[] splitVertices = splits[k].GetComponent<MeshFilter>().mesh.vertices;
                        for (int l = 0; l < splitVertices.Length; l++)
                        {
                            for (int m = 0; m < plotVertices.Length; m++)
                            {
                                if (splitVertices[l] == plotVertices[m])
                                    count++;
                            }
                        }

                        if (count == 2)
                            needsAccess = splits[k];
                    }

                    GameObject toSplit = splits[0];
                    if (needsAccess != null)
                        if (needsAccess == splits[0])
                            toSplit = splits[1];

                    if (needsAccess != null)
                    {
                        //split larger room? - will need to make a hallway
                        larger = splits[0];
                        smaller = splits[1];
                        if (larger.GetComponent<MeshRenderer>().bounds.size.sqrMagnitude < splits[1].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude)
                        {
                            larger = splits[1];
                            smaller = splits[0];
                        }

                        spinPoint = toSplit.GetComponent<MeshRenderer>().bounds.center;

                        splits = Divide.Split(gameObject, toSplit, 0.4f, false, false);
                        foreach (GameObject s in splits)
                            s.transform.position += Vector3.up * storeyHeight;

                        Vector3 centreOfLanding = landing.GetComponent<MeshRenderer>().bounds.center;

                        SwitchSmallerRoomNearPoint(splits, centreOfLanding, spinPoint, true);

                    }
                   
                    
                }

                //other room we are trying to give access to


            }


        }


        plot.SetActive(false);

    }

    public static List<GameObject> FirstFloorLayoutV2(out List<GameObject> returnedHalls, GameObject gameObject, GameObject firstFloor, GameObject plot, float storeyHeight, GameObject stairCollider)
    {


        List<GameObject> quadsBuilt = new List<GameObject>();
        //keep a track of any halls built
        List<GameObject> halls = new List<GameObject>();

        //First, figure out, whre the stairs are and in what orientatin relative to the house's long and shirt sides/ This will govern what type of floor plan we attempt
        Vector3 cornerPoint = Vector3.zero;
        bool stairsInCorner = ColliderInCorner(out cornerPoint, stairCollider, plot.GetComponent<MeshFilter>().mesh);
        if(stairsInCorner)
        {
           // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
           // c.transform.position = cornerPoint;
           // c.transform.parent = gameObject.transform;
          //  c.name = "Corner point";
        }
        //find out what way stairs are facing
        bool stairsFacingLong = StairsFacingLong(stairCollider, plot);

        //get direction to centre
        Vector3 landingDir = -stairCollider.transform.right;
        if (Vector3.Distance(plot.GetComponent<MeshRenderer>().bounds.center, stairCollider.transform.position + stairCollider.transform.right) < Vector3.Distance(plot.GetComponent<MeshRenderer>().bounds.center, stairCollider.transform.position - stairCollider.transform.right))
        {
            landingDir = stairCollider.transform.right;
        }


        //if (!stairsFacingLong && !stairsInCorner)
        //    Debug.Break();

        //if(!stairsFacingLong && !stairsInCorner || !stairsFacingLong && stairsInCorner)
        {
            //split floor plot up in to sections
            //Debug.Break();

            float widthOfStair = stairCollider.transform.localScale.x * 0.5f;
            float lengthOfStair = stairCollider.transform.localScale.z * 0.5f;

            //to make fat hall
            if (stairsFacingLong && !stairsInCorner)
            {
                /*
                stairCollider.transform.position += landingDir * widthOfStair*0.5f;
                stairCollider.transform.localScale += Vector3.right * widthOfStair;
                //re asign
                widthOfStair = stairCollider.transform.localScale.x * 0.5f;
                lengthOfStair = stairCollider.transform.localScale.z * 0.5f;
                //Debug.Break();
                */
            }
            Vector3 centreOfStairs = stairCollider.GetComponent<MeshRenderer>().bounds.center;


            //find intersction points
            Vector3 topRight = (stairCollider.transform.localScale.x * 0.5f * stairCollider.transform.right) + (stairCollider.transform.forward * lengthOfStair);
            Vector3 topLeft = -(stairCollider.transform.localScale.x * 0.5f * stairCollider.transform.right) + (stairCollider.transform.forward * lengthOfStair);
            Vector3 backRight = (stairCollider.transform.localScale.x * 0.5f * stairCollider.transform.right) - (stairCollider.transform.forward * lengthOfStair);
            Vector3 backLeft = -(stairCollider.transform.localScale.x * 0.5f * stairCollider.transform.right) - (stairCollider.transform.forward * lengthOfStair);

            //make clockwise loop
            Vector3[] stairPoints = new Vector3[] { topLeft, topRight, backRight, backLeft, topLeft };
            //rotate
            for (int i = 0; i < stairPoints.Length; i++)
            {
                //stairPoints[i] = stairCollider.transform.rotation* stairPoints[i];

                //and add stairpositoin
                stairPoints[i] += stairCollider.transform.position;

                // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                // c.transform.position = stairPoints[i];
                // c.transform.parent = gameObject.transform;
            }
            //draw lines between these points and see where they intersect the plot exterior

            Vector3[] plotVertices = plot.GetComponent<MeshFilter>().mesh.vertices;
            //making loop
            List<Vector3> plotLoop = new List<Vector3>();
            foreach (Vector3 v3 in plotVertices)
                plotLoop.Add(v3);
            plotLoop.Add(plotVertices[0]);



            //get intersect points around plot mesh
            List<Vector3> stairIntersectPoints = FindIntersects(plotLoop, stairPoints);

            // do a check to see how far away the opposite wall from the stair is - affects floor plan options
            float distanceToOppositeWall = 0f;
            bool closeToOppositeWall = CloseToOppositeWall(out distanceToOppositeWall, plotLoop, stairPoints, stairCollider);
            //Debug.Log("distance to wall = " + distanceToOppositeWall);
            
            if (closeToOppositeWall)
            {
                Debug.Log("CLOSE");
                //Debug.Break();
            }

            //now add a landing and find intersects - we only need the x intersects, the z wil be the same as the z from the stairs
            Vector3 landing1 = centreOfStairs + (stairCollider.transform.forward * (lengthOfStair + widthOfStair * 2)) - (stairCollider.transform.right * widthOfStair);
            Vector3 landing2 = centreOfStairs + (stairCollider.transform.forward * (lengthOfStair + widthOfStair * 2)) + (stairCollider.transform.right * widthOfStair);
            Vector3[] landingPoints = new Vector3[] { landing1, landing2 };


            List<Vector3> landingIntersectPoints = FindIntersects(plotLoop, landingPoints);

            //combine lists 
            List<Vector3> intersectPoints = new List<Vector3>();
            intersectPoints.AddRange(stairIntersectPoints);

            if (!closeToOppositeWall)
                intersectPoints.AddRange(landingIntersectPoints);




            foreach (Vector3 v3 in stairPoints)
            {
                //GameObject c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //c.transform.position = v3;
                //c.transform.parent = gameObject.transform;
            }


            //now we ahve all out intersect points, let's create quads with them
            //find the nearsest intersect point in each direction from each plot corner
            //alter plot loop a bit - entering the "last point" so we can loop from front too - remmeber it is -2 because we already add an extra one on the ned
            plotLoop.Insert(0, plotLoop[plotLoop.Count - 2]);

            //save initial quad points
            List<Vector3[]> listOfQuadPoints = new List<Vector3[]>();
            bool swap = false;
            if (Random.Range(0, 2) == 0)
                swap = true;

            //////OVERRIDE backwards loop not working at findgin interects
            swap = false;///////

            //Debug.Log("swap is " + swap);
            for (int i = 1; i < plotLoop.Count - 1; i++)
            {
                //for (int i = plotLoop.Count-2; i > 0; i--) // below statement turns it in to reverse loop to swap direction of room layout
                int index = i;
                if (swap)
                {
                    index = plotLoop.Count - 1 - i;
                }


                //find intersect points in line
                List<Vector3> pointsInLineBack = new List<Vector3>();
                List<Vector3> pointsInLineForward = new List<Vector3>();
                for (int j = 0; j < intersectPoints.Count; j++)
                {

                    if (Divide.PointsInLine(plotLoop[index], plotLoop[index - 1], intersectPoints[j]))
                    {
                        pointsInLineBack.Add(intersectPoints[j]);

                    }

                    if (Divide.PointsInLine(plotLoop[index], plotLoop[index + 1], intersectPoints[j]))
                    {
                        pointsInLineForward.Add(intersectPoints[j]);

                    }
                }

                //sort by distance
                //sort freeVertices by distance from doors
                pointsInLineBack.Sort(delegate (Vector3 v1, Vector3 v2)
                {
                    return Vector3.Distance(plotLoop[index], v1).CompareTo
                                ((Vector3.Distance(plotLoop[index], v2)));
                });
                pointsInLineForward.Sort(delegate (Vector3 v1, Vector3 v2)
                {
                    return Vector3.Distance(plotLoop[index], v1).CompareTo
                                ((Vector3.Distance(plotLoop[index], v2)));
                });


                //this happens if stair point was on the edge of the house
                Vector3 closestIntersectBackwards = pointsInLineBack[0];
                if (closestIntersectBackwards == plotLoop[index])
                    closestIntersectBackwards = pointsInLineBack[1];
                //this happens if stair point was on the edge of the house
                Vector3 closestIntersectForward = pointsInLineForward[0];
                if (closestIntersectForward == plotLoop[index])
                    closestIntersectForward = pointsInLineForward[1];

                //great, we have the corner point and, plus the closest points now, get the far away point from the corner
                //dont create a quad in the corner if the stairs are there
                if (plotLoop[i] != cornerPoint || !stairsInCorner)
                {

                    /*
                    GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = plotLoop[i];
                    c.transform.parent = gameObject.transform;
                    c.name = "I";
                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = closestIntersectBackwards;
                    c.transform.parent = gameObject.transform;
                    c.name = "closest intersect BWD";

                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = closestIntersectForward;
                    c.transform.parent = gameObject.transform;
                    c.name = "closest intersect FWD";
                    */
                    //Vector3 midHypotenuse = Vector3.Lerp(closestIntersectBackwards, closestIntersectForward, 0.5f);
                    Vector3 dirTo1 = closestIntersectForward - plotLoop[index];
                    Vector3 dirTo2 = closestIntersectBackwards - plotLoop[index];
                    Vector3 farCorner = plotLoop[index] + dirTo1 + dirTo2;

                    //c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //c.transform.position = farCorner;
                    //c.transform.parent = gameObject.transform;
                    //c.name = "far point";

                    Vector3[] points = new Vector3[] { plotLoop[index], closestIntersectBackwards, closestIntersectForward, farCorner };


                    listOfQuadPoints.Add(points);

                }

            }
            //make loop --why asm i doing this?
            listOfQuadPoints.Insert(0, listOfQuadPoints[listOfQuadPoints.Count - 1]);
            listOfQuadPoints.Add(listOfQuadPoints[1]);

            //we will save a new list that we can alter wihout affecting our calculations while we are iterating through this loop
            List<Vector3[]> stretchedRooms = new List<Vector3[]>();
            for (int i = 0; i < listOfQuadPoints.Count; i++)
            {
                Vector3[] quadPoints = listOfQuadPoints[i];

                Vector3[] t = new Vector3[] { quadPoints[0], quadPoints[1], quadPoints[2], quadPoints[3] };

                stretchedRooms.Add(t);
            }

            //stretch rooms
            
            if (!stairsInCorner)
            {
                if (!closeToOppositeWall)
                {
                    if (!stairsFacingLong)// || stairsFacingLong) //doing both - belowe special case to investigate
                    {
                        for (int i = 1; i < listOfQuadPoints.Count - 1; i++)
                        {
                            //check if quad is next to stairs
                            Vector3[] quadPoints = listOfQuadPoints[i];

                            for (int j = 0; j < quadPoints.Length; j++)
                            {
                                for (int k = 0; k < stairPoints.Length - 1; k++)//-1 we made this a loop
                                {
                                    if (quadPoints[j] == stairPoints[k])
                                    {
                                        // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                        //  c.transform.position = quadPoints[j];
                                        //  c.transform.parent = gameObject.transform;
                                        //  c.name = "far point";

                                    }
                                }
                            }


                            //we can stretch to the next room // 
                            Vector3 avg = Vector3.zero; //centre of this room
                            foreach (Vector3 v3 in listOfQuadPoints[i])
                                avg += v3;
                            avg /= quadPoints.Length;

                            //find the two closest points to the next room
                            List<Vector3> pointsByDistance = new List<Vector3>();
                            foreach (Vector3 v3 in listOfQuadPoints[i + 1])
                                pointsByDistance.Add(v3);

                            pointsByDistance.Sort(delegate (Vector3 v1, Vector3 v2)
                            {
                                return Vector3.Distance(avg, v1).CompareTo
                                        ((Vector3.Distance(avg, v2)));
                            });

                            //which two original quad points to stretch to this new point? closest?

                            for (int j = 0; j < 2; j++)
                            {
                                float d = Mathf.Infinity;
                                int closestIndex = 0;

                                for (int k = 0; k < quadPoints.Length; k++)//remember i made it a loop
                                {
                                    float t = Vector3.Distance(pointsByDistance[j], quadPoints[k]);
                                    if (t < d)
                                    {
                                        d = t;
                                        closestIndex = k;

                                    }
                                }

                                //alter and save in seperate list

                                stretchedRooms[i][closestIndex] = pointsByDistance[j];

                                //Debug.Break();
                                //GameObject a = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                //// a.transform.position = quadPoints[closestIndex];
                                // a.transform.parent = gameObject.transform;
                                // a.name = "closest " + closestIndex.ToString();
                            }
                            /*
                            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            c.transform.position = pointsByDistance[0];
                            c.transform.parent = gameObject.transform;
                            c.name = "next room";

                            c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            c.transform.position = pointsByDistance[1];
                            c.transform.parent = gameObject.transform;
                            c.name = "next room";
                            */

                        }

                    }
                    if (stairsFacingLong)
                    {
                        //to test make if statement above only check for !stairsfacinglong
                        //special case //stair long and floating half way
                        //above covers at the moemnt, but could create a unique layout for this scenario in future
                        // Debug.Break();

                        //in this scenario we need to add another quad in the centre of the house



                        Vector3 l1 = centreOfStairs + (stairCollider.transform.forward * lengthOfStair) + landingDir * (widthOfStair);

                        //add landing
                        l1 += widthOfStair * 2 * stairCollider.transform.forward;

                        Vector3 l2 = centreOfStairs - (stairCollider.transform.forward * lengthOfStair) + landingDir * (widthOfStair);

                        //find intersect point across from this that isn't a stair point (there will be two)
                        Vector3 endOfStair = centreOfStairs + stairCollider.transform.forward * lengthOfStair;
                        //add landing
                        endOfStair += widthOfStair * 2 * stairCollider.transform.forward;
                        Vector3 startOfStair = centreOfStairs - stairCollider.transform.forward * lengthOfStair;
                        //add landing
                        //startOfStair += widthOfStair * 2 * stairCollider.transform.forward;

                        List<Vector3> quadPoints = new List<Vector3>();
                        for (int i = 0; i < intersectPoints.Count; i++)
                        {
                            bool stairPoint = false;
                            //check if stair point
                            for (int j = 0; j < stairPoints.Length; j++)
                            {
                                if (intersectPoints[i] == stairPoints[j])
                                    stairPoint = true;
                            }
                            for (int j = 0; j < landingPoints.Length; j++)
                            {
                                if (intersectPoints[i] == landingPoints[j])
                                    stairPoint = true;
                            }

                            if (!stairPoint)
                            {

                                if (Divide.PointsInLine(intersectPoints[i], startOfStair, l2))
                                    quadPoints.Add(intersectPoints[i]);

                                if (Divide.PointsInLine(intersectPoints[i], endOfStair, l1))
                                    quadPoints.Add(intersectPoints[i]);
                            }
                        }

                        //now add side of stairs
                        endOfStair += landingDir * stairCollider.transform.localScale.x * 0.5f;
                        startOfStair += landingDir * stairCollider.transform.localScale.x * 0.5f;

                        quadPoints.Add(endOfStair);
                        quadPoints.Add(startOfStair);

                        GameObject quad = Divide.Quad(gameObject, quadPoints.ToArray());
                        quad.transform.position = +storeyHeight * Vector3.up;
                        quad.transform.parent = gameObject.transform;
                        quadsBuilt.Add(quad);

                        /*
                        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        c.transform.position = l1;
                        c.transform.parent = gameObject.transform;
                        c.name = "Landing1";

                        c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        c.transform.position = l2;
                        c.transform.parent = gameObject.transform;
                        c.name = "Landing2";

                        c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        c.transform.position = startOfStair;
                        c.transform.parent = gameObject.transform;
                        c.name = "start of stair";

                        c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        c.transform.position = endOfStair;
                        c.transform.parent = gameObject.transform;
                        c.name = "End of stair";




                        foreach (Vector3 v3 in quadPoints)
                        {
                            c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            c.transform.position = v3;
                            c.transform.parent = gameObject.transform;
                            c.name = "Quad Point";
                        }
                        */
                        List<Vector3> lps = FindIntersects(plotLoop, landingPoints);

                    }
                }
            }

            else if (stairsInCorner)
            {
                if (!stairsFacingLong || stairsFacingLong) //does both - magic
                {
                    //Debug.Break();
                    //only stretch one quad - Quad to stretch is quad with no shared vertices
                    for (int i = 1; i < listOfQuadPoints.Count - 1; i++)
                    {

                        if (listOfQuadPoints[i][0] == cornerPoint)
                        {
                            Debug.Log("FKD");
                            continue;

                        }


                        bool shared = false;
                        for (int j = 0; j < listOfQuadPoints[i].Length; j++)
                        {


                            for (int a = 1; a < listOfQuadPoints.Count - 1; a++)
                            {
                                //dont check own quad
                                if (i == a)
                                    continue;

                                for (int b = 0; b < listOfQuadPoints[a].Length; b++)
                                {
                                    if (listOfQuadPoints[i][j] == listOfQuadPoints[a][b])
                                    {
                                        shared = true;


                                    }
                                }
                            }


                        }
                        if (!shared)
                        {
                            foreach (Vector3 v3 in listOfQuadPoints[i])
                            {

                            }

                            //we have found our lonesome quad - stretch towards the quad adjacent
                            //quad adjacent is just the closer one - do by average
                            List<Vector3> averages = new List<Vector3>();
                            for (int j = 1; j < listOfQuadPoints.Count - 1; j++)
                            {
                                if (i == j)
                                    continue;

                                Vector3 avg = Vector3.zero;
                                for (int k = 0; k < listOfQuadPoints[j].Length; k++)
                                    avg += listOfQuadPoints[j][k];

                                avg /= 4;

                                averages.Add(avg);

                            }

                            //average for our lonesome quad
                            Vector3 lonesomeAvg = Vector3.zero;
                            for (int j = 0; j < listOfQuadPoints[i].Length; j++)
                                lonesomeAvg += listOfQuadPoints[i][j];

                            lonesomeAvg /= 4;

                            //love this method :) V. Lazy
                            averages.Sort(delegate (Vector3 v1, Vector3 v2)
                            {
                                return Vector3.Distance(lonesomeAvg, v1).CompareTo
                                        ((Vector3.Distance(lonesomeAvg, v2)));
                            });

                            //get two closest points from lonesome quad to nearest avg
                            List<Vector3> sortedList = new List<Vector3>(listOfQuadPoints[i]);
                            sortedList.Sort(delegate (Vector3 v1, Vector3 v2)
                            {
                                return Vector3.Distance(averages[0], v1).CompareTo
                                        ((Vector3.Distance(averages[0], v2)));
                            });
                            /*
                            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            c.transform.position = sortedList[0];
                            c.transform.parent = gameObject.transform;
                            c.name = "closest";
                            c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            c.transform.position = sortedList[1];
                            c.transform.parent = gameObject.transform;
                            c.name = "closest";
                            */
                            //move these two points over
                            sortedList[0] += stairCollider.transform.forward * widthOfStair * 2;
                            sortedList[1] += stairCollider.transform.forward * widthOfStair * 2;
                            //make a new array

                            // GameObject quad = Divide.Quad(gameObject, sortedList.ToArray());
                            //quad.transform.position += Vector3.up * storeyHeight;
                            // quad.transform.parent = gameObject.transform;

                            stretchedRooms[i] = sortedList.ToArray();

                        }
                    }
                }
            }
           
           
            //now build the landing
            if (!closeToOppositeWall)
            {
                landingPoints = new Vector3[] { landing1, landing2, stairPoints[0], stairPoints[1] };

                GameObject landing = Divide.Quad(gameObject, landingPoints);
                landing.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;
                landing.transform.position += Vector3.up * storeyHeight;
                landing.transform.parent = gameObject.transform;
                landing.name = "Landing";
                halls.Add(landing);

                

            }
            else if(closeToOppositeWall)
            {
                landingPoints = new Vector3[] { stairPoints[0]+stairCollider.transform.forward*(distanceToOppositeWall), stairPoints[1] + stairCollider.transform.forward * (distanceToOppositeWall), stairPoints[0], stairPoints[1] };

                GameObject landing = Divide.Quad(gameObject, landingPoints);
                landing.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;
                landing.transform.position += Vector3.up * storeyHeight;
                landing.transform.parent = gameObject.transform;
                landing.name = "LandingClose"; //will be caught later with this nake to be split in to squares
                halls.Add(landing);

                Debug.Log("HERE 1");
            }

            //build
            
            List<GameObject> toRemove = new List<GameObject>();
            for (int i = 1; i < stretchedRooms.Count - 1; i++)
            {
                //check we ahvent already built a landing quad
                int count = 0;
                for (int j = 0; j < landingPoints.Length; j++)
                {
                    for (int k = 0; k < stretchedRooms[i].Length; k++)
                    {
                        if (landingPoints[j] == stretchedRooms[i][k])
                            count++;
                    }
                }

                if (count == 4)
                {
                    Debug.Log("COunt = 4");
                    
                    continue;
                }

                GameObject quad = Divide.Quad(gameObject, stretchedRooms[i]);
                quadsBuilt.Add(quad);
                quad.name = "this";
                // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                // c.transform.position = quad.GetComponent<MeshRenderer>().bounds.center + Vector3.up * storeyHeight;
                // c.transform.parent = gameObject.transform;
                //quad.transform.position += Vector3.up * storeyHeight;

                //check if stair is inside quad 

                BoxCollider bc = quad.AddComponent<BoxCollider>();
                bc.size += Vector3.up * 10;
                if (Divide.PointInOABB(stairCollider.transform.position, bc))
                {
                    // c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //  c.transform.position = bc.bounds.center + Vector3.up * storeyHeight;
                    //  c.transform.parent = gameObject.transform;

                    //we need to move the stretch back

                    List<Vector3> pointsByDistance = new List<Vector3>();
                    foreach (Vector3 v3 in stretchedRooms[i])
                        pointsByDistance.Add(v3);

                    pointsByDistance.Sort(delegate (Vector3 v1, Vector3 v2)
                    {
                        return Vector3.Distance(stairCollider.transform.position, v1).CompareTo
                                ((Vector3.Distance(stairCollider.transform.position, v2)));
                    });

                    //direction to move is centre of room minus centre of stair
                    Vector3 dir = (bc.bounds.center - stairCollider.transform.position).normalized;
                    dir *= widthOfStair * 2;



                    for (int j = 0; j < 2; j++)
                    {
                        pointsByDistance[j] += dir;
                    }

                    GameObject smaller = Divide.Quad(gameObject, pointsByDistance.ToArray());
                    smaller.transform.position += Vector3.up * storeyHeight;
                    smaller.name = "Smaller";
                    quadsBuilt.Add(smaller);

                    stretchedRooms[i] = pointsByDistance.ToArray();
                    Destroy(quad);
                    quadsBuilt.Remove(quad);
                }

                if (quad != null)
                    quad.transform.position += Vector3.up * storeyHeight;
            }

            
            //merge if needed

            //if stairs in corner, quads build ok
            if (closeToOppositeWall && !stairsInCorner)//needs work close to side wall false positive // doesnt always merge all rooms
            {
                //merge adjacent rooms
                List<int> roomsAlreadyBuilt = new List<int>();
                
                //temp list while we work through old lists
                List<GameObject> tempQuads = new List<GameObject>();
                for (int i = 0; i < quadsBuilt.Count; i++)
                {
                    Vector3 c0 = quadsBuilt[i].GetComponent<MeshRenderer>().bounds.center;

                    //GameObject c = null;//debug

                   
                    Vector3[] vertices = quadsBuilt[i].GetComponent<MeshFilter>().mesh.vertices;
                    for (int a = 0; a < quadsBuilt.Count; a++)
                    {
                        if (i == a)
                            continue;

                        if (roomsAlreadyBuilt.Contains(a))
                            continue;

                        List<Vector3> shared = new List<Vector3>();
                        int sharedRoom = 0;
                        Vector3[] otherVertices = quadsBuilt[a].GetComponent<MeshFilter>().mesh.vertices;
                        for (int b = 0; b < vertices.Length; b++)
                        {
                            for (int j = 0; j < otherVertices.Length; j++)
                            {
                                if (Vector3.Distance(vertices[b], otherVertices[j]) < 0.1f)
                                //if (vertices[b] == otherVertices[j])
                                {
                                    shared.Add(vertices[b]);
                                    sharedRoom = a;


                                    //c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                   // c.transform.position = vertices[b];
                                   // c.transform.parent = gameObject.transform;
                                   // c.name = "merge";
                                   // c.name = i.ToString() + " shared with " + a.ToString() + " shared count is at this point " + shared.Count;
                                }
                            }
                        }

                        if(shared.Count == 2)
                        {
                           // c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                           // c.transform.position = c0;
                          //  c.transform.parent = gameObject.transform;
                          //  c.name = "pair 1";
                            Vector3 c1 = quadsBuilt[a].GetComponent<MeshRenderer>().bounds.center;

                          //  c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                          //  c.transform.position = c1;
                          //  c.transform.parent = gameObject.transform;

                          //  c.name = "pair 2";

                            //now create a new quad removinf the doublers/adjacent
                            List<Vector3> allPoints = new List<Vector3>();
                            allPoints.AddRange(vertices);
                            allPoints.AddRange(otherVertices);

                            //was using .Contains but was getting inconsistent results, removing with distance check now
                            List<Vector3> quadPoints = new List<Vector3>(allPoints);
                            for (int k = 0; k < allPoints.Count; k++)
                            {
                                for (int l = 0; l < shared.Count; l++)
                                {
                                    if (Vector3.Distance(allPoints[k], shared[l]) < 0.01f)
                                    {
                                        quadPoints.Remove(allPoints[k]);
                                        //c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                        // c.transform.position = allPoints[k];
                                        // c.transform.parent = gameObject.transform;
                                    }
                                }
                            }

                            GameObject q = Divide.Quad(gameObject, quadPoints.ToArray());
                            q.transform.position += Vector3.up * storeyHeight;
                            q.transform.parent = gameObject.transform;
                            q.name = "MERGED QUAD here aa";

                            roomsAlreadyBuilt.Add(i);
                            roomsAlreadyBuilt.Add(a);
                            //toAdd.Add(q);
                            //force loop closed

                            toRemove.Add(quadsBuilt[i]);
                            //toRemove.Add(quadsBuilt[j]);

                            //Destroy(quadsBuilt[i]);
                            //Destroy(quadsBuilt[j]);
                            tempQuads.Add(q);
                        }
                    }
                }
                //replace list with new rooms
                foreach (GameObject go in quadsBuilt)
                    Destroy(go);

                quadsBuilt = new List<GameObject>(tempQuads);

                //Debug.Break();

                //and exit? if we exit here, we have two rooms - but need to check for adjacents for no access
                if (!stairsFacingLong)
                {
                    Debug.Log("Returned from here");
                    returnedHalls = new List<GameObject>();
                    foreach (GameObject h in halls)
                        returnedHalls.Add(h);


                  
                 //   Debug.Log("halls.Count " + halls.Count);
                    
                    return quadsBuilt;
                }
                else
                {
                    Debug.Log("broke here");
                    //Debug.Break();
                }

                //remove quads that were replaced from list
                foreach (GameObject go in toRemove)
                    quadsBuilt.Remove(go);
            }


            List<GameObject> toAdd = new List<GameObject>();
            //we might need this
            List<GameObject> mergedQuads = new List<GameObject>();
            if (1 == 1)
            {
                //Debug.Break();
                //find if any quad has two adjacent with the landing

                
                toRemove = new List<GameObject>();
                for (int i = 0; i < quadsBuilt.Count; i++)
                {
                    Vector3[] quadVertices = quadsBuilt[i].GetComponent<MeshFilter>().mesh.vertices;
                    int adjacent = 0;
                    for (int j = 0; j < quadVertices.Length; j++)
                    {
                        for (int k = 0; k < landingPoints.Length; k++)
                        {
                            if (quadVertices[j] == landingPoints[k])
                                adjacent++;

                        }
                    }

                    if (adjacent == 2)
                    {
                        
                        //find what other quad this quad has edges to
                        for (int j = 0; j < quadsBuilt.Count; j++)
                        {
                            List<Vector3> otherAdjacents = new List<Vector3>();
                            Vector3[] otherVertices = quadsBuilt[j].GetComponent<MeshFilter>().mesh.vertices;
                            for (int k = 0; k < otherVertices.Length; k++)
                            {
                                for (int a = 0; a < quadVertices.Length; a++)
                                {
                                    if (otherVertices[k] == quadVertices[a])
                                    {
                                        otherAdjacents.Add(otherVertices[k]);
                                    }
                                }
                            }

                            if (otherAdjacents.Count == 2)
                            {
                                /*
                                GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                c.transform.position = otherAdjacents[0];
                                c.transform.parent = gameObject.transform;
                                c.name = "Adjacent";

                                c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                c.transform.position = otherAdjacents[1];
                                c.transform.parent = gameObject.transform;
                                c.name = "Adjacent";
                                */

                                //now create a new quad removinf the doublers/adjacent
                                List<Vector3> allPoints = new List<Vector3>();
                                allPoints.AddRange(quadVertices);
                                allPoints.AddRange(otherVertices);

                                //was using .Contains but was getting inconsistent results, removing with distance check now
                                List<Vector3> quadPoints = new List<Vector3>(allPoints);
                                for (int k = 0; k < allPoints.Count; k++)
                                {
                                    for (int l = 0; l < otherAdjacents.Count; l++)
                                    {
                                        if (Vector3.Distance(allPoints[k], otherAdjacents[l]) < 0.01f)
                                        {
                                            quadPoints.Remove(allPoints[k]);
                                            //c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                            // c.transform.position = allPoints[k];
                                            // c.transform.parent = gameObject.transform;
                                        }
                                    }
                                }


                                GameObject q = Divide.Quad(gameObject, quadPoints.ToArray());
                                q.transform.position += Vector3.up*storeyHeight;
                                q.transform.parent = gameObject.transform;
                                q.name = "MERGED QUAD here";
                               
                                toAdd.Add(q);
                                //force loop closed

                                toRemove.Add(quadsBuilt[i]);
                                toRemove.Add(quadsBuilt[j]);

                                //Destroy(quadsBuilt[i]);
                                //Destroy(quadsBuilt[j]);
                                mergedQuads.Add(q);
                                //i = 1000;
                            }
                        }

                        
                        //
                    }                  
                }

                foreach (GameObject go in toAdd)
                    //if(!quadsBuilt.Contains(go))//duplicate happenig somewhere..too tired - :)
                        quadsBuilt.Add(go);

                foreach (GameObject go in toRemove)
                {
                    quadsBuilt.Remove(go);
                    Destroy(go);
                }

                
                //now check if any rooms have two adjacent vertices to each other - - creates larger rooms
                //only needed in this special case
                if (stairsFacingLong &&!stairsInCorner)
                {
                    //use this list to stop duplicates
                    List<int> sharedRooms = new List<int>();
                    int sharedRoom = 0;
                    for (int i = 0; i < quadsBuilt.Count; i++)
                    {
                        Vector3[] quadVertices = quadsBuilt[i].GetComponent<MeshFilter>().mesh.vertices;

                        for (int a = 0; a < quadsBuilt.Count; a++)
                        {
                            if (i == a)
                                continue;
                            List<Vector3> adjacents = new List<Vector3>();

                            for (int j = 0; j < quadVertices.Length; j++)
                            {
                                if (adjacents.Count >= 2)
                                    continue;

                                Vector3[] otherVertices = quadsBuilt[a].GetComponent<MeshFilter>().mesh.vertices;
                                for (int b = 0; b < otherVertices.Length; b++)
                                {
                                    if (quadVertices[j] == otherVertices[b])
                                    {
                                        adjacents.Add(quadVertices[j]);

                                        sharedRoom = a;
                                        //GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                       // c.transform.position = quadVertices[j];
                                       // c.transform.parent = gameObject.transform;
                                       // c.name = a.ToString();
                                    }
                                }

                                if (adjacents.Count == 2)
                                {
                                    if (!sharedRooms.Contains(sharedRoom))
                                    {
                                        List<Vector3> allPoints = new List<Vector3>();
                                        allPoints.AddRange(quadVertices);
                                        allPoints.AddRange(otherVertices);

                                        //was using .Contains but was getting inconsistent results, removing with distance check now
                                        List<Vector3> quadPoints = new List<Vector3>(allPoints);
                                        for (int k = 0; k < allPoints.Count; k++)
                                        {
                                            for (int l = 0; l < adjacents.Count; l++)
                                            {
                                                if (Vector3.Distance(allPoints[k], adjacents[l]) < 0.01f)
                                                {
                                                    quadPoints.Remove(allPoints[k]);

                                                }
                                            }
                                        }

                                        //Debug.Break();

                                        GameObject q = Divide.Quad(gameObject, quadPoints.ToArray());
                                        q.transform.position += Vector3.up*storeyHeight;
                                        q.transform.parent = gameObject.transform;
                                        q.name = "MERGED QUAD 2";

                                        sharedRooms.Add(i);

                                        //create a list of these merge quads - we don't need all of them but will select what we need in next section
                                        mergedQuads.Add(q);
                                    }

                                }
                            }
                        }
                    }

                    //
                    if (closeToOppositeWall)
                    {
                        //foreach (GameObject go in quadsBuilt)
                        //    Destroy(go);

                        //quadsBuilt = new List<GameObject>(mergedQuads);
                    }
                }
            }

            


            // 

            List<GameObject> splits = null;
            if (stairsFacingLong && !stairsInCorner)
            {
                    //special case handled above
            }
            else
            {
               
                //split quads to make rooms //dont always do this?

                //find largest quad - or the one which is the most square
                //most square is the least abs(bounds.x - bounds.z)
                List<GameObject> quadsBySize = new List<GameObject>(quadsBuilt);
                quadsBySize.Sort(delegate (GameObject g1, GameObject g2)
                {
                    return Mathf.Abs(g1.GetComponent<MeshRenderer>().bounds.size.x - g1.GetComponent<MeshRenderer>().bounds.size.z).CompareTo
                    (Mathf.Abs(g2.GetComponent<MeshRenderer>().bounds.size.x - g2.GetComponent<MeshRenderer>().bounds.size.z));
                });

                GameObject longestQuad = quadsBySize[quadsBySize.Count - 1];
                //split along longest edge/or shortest? when to decide what?
                bool longSplit = true;
                if (Random.Range(0, 2) == 0)
                    longSplit = false;

                longSplit = false;

              //  Debug.Log("long split = " + longSplit);
                //List<GameObject> splits =  Divide.Split(gameObject, longestQuad, widthOfStair*2, true, longSplit);
                quadsBuilt.Remove(longestQuad);
                splits = Divide.Split(gameObject, longestQuad, 0f, false, longSplit);
                quadsBuilt.Add(splits[0]);
                quadsBuilt.Add(splits[1]);
                foreach (GameObject go in splits)
                {
                    go.transform.position += storeyHeight * Vector3.up;
                    go.name = "Room";
                }

                //not doing?? why was it here?

                //quadsBuilt = HallAccessV2(out returnedHalls, gameObject, halls, quadsBuilt, splits.ToArray(), landingPoints, widthOfStair, storeyHeight, landing1, firstFloor);

                
            }
            //end of not always do ?

            


            //find furthest split from landing

            //List<GameObject> splits = null;
            if (stairsInCorner || !stairsInCorner && !stairsFacingLong)
            {
                //splitsFromLanding = new List<GameObject>(splits);

                splits.Sort(delegate (GameObject g1, GameObject g2)
                {
                    return Vector3.Distance(g1.GetComponent<MeshRenderer>().bounds.center, landing1).CompareTo
                    (Vector3.Distance(g2.GetComponent<MeshRenderer>().bounds.center, landing1));
                });
            }
            else
            {
                //this is a layout where stairs are half way up the ong wall, we need to make a fat hall
                //splt the quad which lies beside the stairs and hall

                //we have found a special layour se we need to re jig the quads a bit - this is a bit confusing
                //take from merged quads what we need
                
                toAdd = new List<GameObject>();

                GameObject centralQuad = quadsBuilt[0];

                Destroy(mergedQuads[1]);
                Destroy(quadsBuilt[1]);
                Destroy(quadsBuilt[2]);

                Vector3 spinPoint = centralQuad.GetComponent<MeshRenderer>().bounds.center;
                //make sure direction we plsit is the same as the way the stair is facing
                //what way is long way
                Vector3[] vs = centralQuad.GetComponent<MeshFilter>().mesh.vertices;
                int[] longestEdge = Divide.LongestEdge(vs);
                int[] shortestEdge = Divide.ShortestEdge(vs);
               // bool splitDir = false;
                {
                    if (Vector3.Distance(vs[longestEdge[0]], vs[longestEdge[1]]) != Vector3.Distance(vs[shortestEdge[0]], vs[shortestEdge[1]]))
                    {
                        Vector3 edgeDir = (vs[longestEdge[0]] - vs[longestEdge[1]]).normalized;                        

                        //if (stairCollider.transform.forward == edgeDir || stairCollider.transform.forward == -edgeDir)
                        //    splitDir = true;
                    }
                }
                

                //use split with direction to run hall alongside stair -- thi is all a bit particular for one scenario..
                //was using wplit with point but included point in SplitWith
                //splits = Divide.SplitWithDirection(gameObject, centralQuad, widthOfStair*2,stairCollider.transform.forward,
                    //stairCollider.transform.position);//NOT WORKING JUST USING POINT
                splits = Divide.SplitFromPoint(gameObject, centralQuad, widthOfStair * 2,
                    stairCollider.transform.position);
                //split and addd to halls

                GameObject[] smallerFirst = SpinSplitsToHall(splits, storeyHeight, landing1, spinPoint, firstFloor);
                smallerFirst[0].name = "Quad";
                halls.Add(smallerFirst[0]);


              //  Debug.Log("merged wuads count = " + mergedQuads.Count);
                quadsBuilt = new List<GameObject>();
                if (mergedQuads.Count == 3)
                {
                    //quadsBuilt.Add(splits[0]); //hall list now
                    quadsBuilt.Add(smallerFirst[1]);
                    quadsBuilt.Add(mergedQuads[0]);
                    quadsBuilt.Add(mergedQuads[2]);
                }

                if (mergedQuads.Count == 2)
                {
                    //quadsBuilt.Add(splits[0]);//hall
                    quadsBuilt.Add(smallerFirst[1]);
                    quadsBuilt.Add(mergedQuads[0]);
                    //quadsBuilt.Add(mergedQuads[2]);
                }

                for (int j = 0; j < halls.Count; j++)
                {
                   // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                   // c.transform.position = halls[j].GetComponent<MeshRenderer>().bounds.center;
                   // c.transform.parent = gameObject.transform;
                }
                Debug.Log("Special case, returned from here - will break on symmetrical plot");
                
                //  if(mergedQuads.Count ==2)

                returnedHalls = new List<GameObject>();
                quadsBuilt = HallAccessV2(out returnedHalls, gameObject, halls, quadsBuilt, smallerFirst, landingPoints, widthOfStair, storeyHeight, landing1, firstFloor,plot);

                //Debug.Break();

                return quadsBuilt;
            }

            


            List<GameObject> quadsWithOneShared = new List<GameObject>();
            List<GameObject> quadsWithTwoShared = new List<GameObject>();
            //find a room wher it only shares 1 point with splits from landing
            Vector3[] thisVertices = splits[1].GetComponent<MeshFilter>().mesh.vertices;
            //reset this list
            toAdd = new List<GameObject>();
            toRemove = new List<GameObject>();
            for (int i = 0; i < quadsBuilt.Count; i++)
            {
                int shared = 0;
                Vector3[] otherVertices = quadsBuilt[i].GetComponent<MeshFilter>().mesh.vertices;
                for (int j = 0; j < otherVertices.Length; j++)
                {
                    for (int k = 0; k < thisVertices.Length; k++)
                    {
                        if (Vector3.Distance( otherVertices[j],thisVertices[k]) < 0.1f)
                        {
                            shared++;
                        }
                                
                    }
                }

                if(shared == 1)
                {
                    quadsWithOneShared.Add(quadsBuilt[i]);
                }
                if(shared == 2)
                {
                    quadsWithTwoShared.Add(quadsBuilt[i]);
                }

                    
            }

            //FIRST HALL
            //we need to save this quad and add it to quads built later - I forgot to add in the list when iw as writing the method and if i add it now, it can create skewed layouts..sorry!
            GameObject partnerOfHall = null;
            //remember hall which we will build below
            GameObject hall = null;
            
            for (int i = 0; i < quadsWithOneShared.Count; i++)//shared to landing
            {
                Vector3 spinPoint = quadsWithOneShared[i].GetComponent<MeshRenderer>().bounds.center;

                //if long edge is perpendicular to stair direction- make true to split long edge

                //else if long edge matchs stair collider direction - make false -so split short edges
                Vector3[] vertices = quadsWithOneShared[i].GetComponent<MeshFilter>().mesh.vertices;
                int[] longestEdge = Divide.LongestEdge(vertices);
                int[] shortestEdge = Divide.ShortestEdge(vertices);
                if (Vector3.Distance(vertices[longestEdge[0]], vertices[longestEdge[1]]) == Vector3.Distance(vertices[shortestEdge[0]], vertices[shortestEdge[1]]))
                {
                    Debug.Log("EDGES SAME DISTANCE");
                    // Debug.Break();
                }

                Vector3 edgeDir = (vertices[longestEdge[0]] - vertices[longestEdge[1]]).normalized;
                bool perpendicular = true;
                if (stairCollider.transform.transform.forward == edgeDir || stairCollider.transform.forward == -edgeDir)
                    perpendicular = false;


                //quadsBuilt.Remove(quadsWithOneShared[i]);
                splits = Divide.Split(gameObject, quadsWithOneShared[i], widthOfStair * 2, true, perpendicular);

                GameObject[] hallAndPartner = SpinSplitsToHall(splits, storeyHeight, landing1, spinPoint, firstFloor);
                hall = hallAndPartner[0];


                //test
               
                spinPoint = hall.GetComponent<MeshRenderer>().bounds.center;
                splits = Divide.Split(gameObject, hall, widthOfStair * 2, true, false);
                GameObject[] hallAndHall = SpinSplitsToHall(splits, storeyHeight, landing1, spinPoint, firstFloor);
                //normally we only add [0] which is the hall, but both of these are halls
                halls.Add(hallAndHall[0]);
                halls.Add(hallAndHall[1]);
                hallAndHall[1].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;
               

                //quadsBuilt.Add(hallAndPartner[1]);cant add now, bug, save and add at end
                partnerOfHall = hallAndPartner[1];
                partnerOfHall.name = "partner";

                toAdd.Add(partnerOfHall);
                toRemove.Add(quadsWithOneShared[i]);

                // Debug.Break();

                foreach (GameObject go in toAdd)
                  //  if (!quadsBuilt.Contains(go))
                        quadsBuilt.Add(go);

                foreach (GameObject go in toRemove)
                    quadsBuilt.Remove(go);


                returnedHalls = new List<GameObject>();
                quadsBuilt = HallAccessV2(out returnedHalls, gameObject, halls, quadsBuilt, hallAndPartner, landingPoints, widthOfStair, storeyHeight, landing1, firstFloor,plot);

               // quadsBuilt = HallAccess(out halls, gameObject, quadsBuilt, halls, landingPoints, hall, storeyHeight, landing1, widthOfStair, firstFloor, stairsFacingLong);

                return quadsBuilt;
                
            }
            
            for (int i = 0; i < quadsWithTwoShared.Count; i++)//shared to landing
            {

                //doing this in function below ( i think!)

                  
            }

            foreach (GameObject go in toAdd)
               // if (!quadsBuilt.Contains(go))
                    quadsBuilt.Add(go);

            foreach (GameObject go in toRemove)
                quadsBuilt.Remove(go);

            //give access to a room with no hall

            //  quadsBuilt = HallAccess(out halls, gameObject, quadsBuilt, halls, landingPoints, hall, storeyHeight, landing1, widthOfStair, firstFloor,stairsFacingLong);



            //
            quadsBuilt = HallAccessV2(out returnedHalls, gameObject, halls, quadsBuilt, splits.ToArray(), landingPoints, widthOfStair, storeyHeight, landing1, firstFloor,plot);


            for (int i = 0; i < halls.Count; i++)
            {
               // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
               // c.transform.position = halls[i].GetComponent<MeshRenderer>().bounds.center;
               // c.transform.parent = gameObject.transform;
            }
            //Debug.Log(toAdd.Count + " to add count");

            //quadsBuilt.Add(toAdd[0]);

            //if (hall == null)
            //     Debug.Break();


            //meh, bug thingy, add here - if added before.. madness.
            //quadsBuilt.Add(partnerOfHall);
        }


        returnedHalls = new List<GameObject>(halls);
        Debug.Log("RETURN 1");
        return quadsBuilt;
    }

    public static List<GameObject> RoomsBySize(List<GameObject> quads)
    {
        //use sqr magnitude to order list
        quads.Sort(delegate (GameObject g1, GameObject g2)
        {
            return g1.GetComponent<MeshRenderer>().bounds.size.sqrMagnitude.CompareTo
                        (g2.GetComponent<MeshRenderer>().bounds.size.sqrMagnitude);
        });

        return quads;
    }

    public static void DoorsOnly(List<GameObject> quads,List<GameObject> halls,float storeyHeight,float doorHeight,float doorWidth,Divide divide)
    {
        foreach (GameObject go in halls)
        {
             // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
              //c.transform.position = go.GetComponent<MeshRenderer>().bounds.center;
             //  c.name = "p2";
             // c.transform.parent = go.transform;
        }

        //list of built doors
        List<Divide.WallWithDoor> wallsWithDoors = new List<Divide.WallWithDoor>();
        //doors
        List<GameObject> doorsBuilt = new List<GameObject>();
        //room info
        List<Divide.RoomAndEdge> roomsAndEdges = new List<Divide.RoomAndEdge>();

        //for each room, work out and store how many shared points it has with each other room
        List<List<Divide.TargetAndSharedPoints>> listOfRoomsAndSharedPoints = new List<List<Divide.TargetAndSharedPoints>>();
        for (int i = 0; i < quads.Count; i++)
        {
            List<Divide.TargetAndSharedPoints> targetAndSharedPoints = new List<Divide.TargetAndSharedPoints>();

            for (int j = 0; j < halls.Count; j++)
            {
                GameObject thisRoom = quads[i];
                GameObject targetRoom = halls[j];
                List<Vector3> sharedPoints = HouseBuilder.SharedPointsWithTargetRoom(thisRoom, targetRoom);

                Divide.TargetAndSharedPoints tasp = new Divide.TargetAndSharedPoints();
                tasp.room = quads[i];
                tasp.target = halls[j];
                tasp.sharedPoints = sharedPoints;

                targetAndSharedPoints.Add(tasp);
                //Debug.Log(thisRoom.name  + " Shared Points = " + sharedPoints.Count + ". Target room = " + targetRoom.name);
            }
            listOfRoomsAndSharedPoints.Add(targetAndSharedPoints);
        }

        for (int i = 0; i < quads.Count; i++)
        {
            //smallest room is bathroom
            GameObject thisRoom = quads[i];

             //GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // c.transform.position = quads[i].GetComponent<MeshRenderer>().bounds.center;
            // c.transform.parent = quads[i].transform;
            // c.name = i.ToString() + " I";
            //rooms to check //start with largest and work down the way - aim here is to create natural flow through house
            for (int r = 0; r < halls.Count; r++)
            {
              //  GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
              ///  c.transform.position = halls[r].GetComponent<MeshRenderer>().bounds.center;
              //  c.transform.parent = halls[r].transform;
             //   c.name = r.ToString() + " R";

                if (quads[i] == halls[r])
                {
                    continue;
                }

              


                GameObject targetRoom = halls[r];

                //find shared Points - vertices that match the same positions as other room's vertices
                //List<Vector3> sharedPoints = SharedPointsWithTargetRoom(thisRoom, targetRoom); ---old way, building lists before we enter this loop now
                List<Vector3> sharedPoints = listOfRoomsAndSharedPoints[i][r].sharedPoints;

                //check for symmetrical layout - quite rare, but when it happens, a room can try and build diagonnally across the building in to the largest room - if caught, just move target on to next room
                //if we share 1 point with the target room and two points with any other two rooms, it means we are diagonally across from out target room. We can't put a door here
                int targetRoomSharedPoints = sharedPoints.Count;
                //Debug.Log(thisRoom.name + " Target = " + targetRoom.name + " Shared Points Count = " + targetRoomSharedPoints);
                int countOfOtherRoomsWith2SharedPoints = 0;
                for (int a = 0; a < listOfRoomsAndSharedPoints.Count; a++)
                {
                    if (listOfRoomsAndSharedPoints[a][r].sharedPoints.Count == 2)
                    {
                        countOfOtherRoomsWith2SharedPoints++;
                        //Debug.Log(thisRoom.name + " Target = " + targetRoom.name + " Shared Points Count = " + targetRoomSharedPoints);
                    }
                }

                //if we have 2 shared points, we have a simple wall with door to create in to target room
                if (sharedPoints.Count == 2)
                {
                    float distance = Vector3.Distance(sharedPoints[0], sharedPoints[1]);
                    //create list fof door point options along available wall
                    List<Vector3> doorOptions = new List<Vector3>();
                    Vector3 dir = (sharedPoints[1] - sharedPoints[0]).normalized;
                    //leaving a gap of 1 at each side // room size shouldnt push this too much - system isn't great but i tihnk it catches all problems
                    float gap = 1f;//door width here? // clamping on room creation too
                    for (float d = gap; d <= distance - gap; d += 0.1f)
                    {
                        Vector3 p = sharedPoints[0] + (dir * d);
                       // if (!stairCollider.GetComponent<BoxCollider>().bounds.Contains(p))
                        {
                            //GameObject ex = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            //ex.transform.position = p;
                            //ex.transform.parent = roomsAndSizes[i].room.transform;
                            //ex.transform.localScale *= 0.5f;
                            doorOptions.Add(p);
                        }

                    }

                    //Debug.Log(doorOptions.Count + "door options " + "thisRoom = " + thisRoom.name + ", target room = " + targetRoom.name);
                    //randomly choose from this list

                    Vector3 doorPoint = Vector3.Lerp(sharedPoints[0], sharedPoints[1], 0.5f);

                    if (doorOptions.Count != 0)
                        doorPoint = doorOptions[Random.Range(0, doorOptions.Count)];
                    else
                    {
                        Debug.Log("door options 0");
                        // Debug.Break();
                        continue;
                    }

                    Vector3 p1 = sharedPoints[0];
                    Vector3 p2 = sharedPoints[1];
                    GameObject room = quads[i];
                    Vector3 globalP1 = room.transform.position + (room.transform.rotation * p1);
                    Vector3 globalP2 = room.transform.position + (room.transform.rotation * p2);
                    Vector3 globalDoorPoint = room.transform.position + (room.transform.rotation * doorPoint);

                    //build door using house builder class
                    Vector3 centre = room.GetComponent<MeshRenderer>().bounds.center;

                    //create points each side of the line - use "world" coordinates, because "centre" is a world co-ordinate
                    Vector3 lookDir1 = Quaternion.Euler(0, 90, 0) * (globalP2 - globalP1).normalized;
                    Vector3 lookDir2 = Quaternion.Euler(0, -90, 0) * (globalP2 - globalP1).normalized;
                    Vector3 lookDir = Vector3.zero;
                    //check which is closest - use that rotation to build wall
                    if (Vector3.Distance(globalDoorPoint + lookDir1, centre) > Vector3.Distance(globalDoorPoint + lookDir2, centre))
                        lookDir = Quaternion.Euler(0, 90, 0) * (p2 - p1).normalized;    //save as local direction
                    else
                        lookDir = Quaternion.Euler(0, -90, 0) * (p2 - p1).normalized; //save as local direction
                    GameObject door = HouseBuilder.DoorAtPosition(doorPoint, lookDir, quads[i], storeyHeight, doorHeight, doorWidth,divide);
                    door.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                    //build door using house builder class
                    //List<List<GameObject>> doorThenWalls = HouseBuilder.DoorWithWall(thisRoom, sharedPoints[0], sharedPoints[1], doorPoint, false, false);//dont miss door
                    //List<GameObject> doors = doorThenWalls[0];
                    //doors[0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;

                    //add to a list, we will use the door positions when planning room interiors
                    //List<GameObject> doorList = doorThenWalls[0];
                    //doorsBuilt.Add(doorList[0]);

                    //remember which edge we have already built
                    Divide.RoomAndEdge rae = new Divide.RoomAndEdge()
                    {
                        room = thisRoom,
                        edge = new Vector3[2] { sharedPoints[0], sharedPoints[1] }
                    };

                    roomsAndEdges.Add(rae);

                    //lremember where we ahve built a wall
                    Divide.WallWithDoor wwd = new Divide.WallWithDoor()
                    {
                        wallPoint1 = sharedPoints[0],
                        wallPoint2 = sharedPoints[1],
                        doorPoint = doorPoint,
                        parent = thisRoom,
                        target = targetRoom
                    };


                    wallsWithDoors.Add(wwd);
                    //force skip, we have found our door
                    r = 1000;


                }
                //else if we only have shared point with  
                else if (sharedPoints.Count == 1)
                {

                    //Debug.Log(thisRoom.name + " shared points 2");
                    //we need to discover the vertice of the other room, this lets us know where to stop the wall
                    //find closest point to the shared point we are trying to attach to(target Room)    

                    Vector3 closestPointFromThisRoom = Divide.ClosestVerticeOnThisRoomToCentreOfTargetRoom(quads[i], targetRoom, sharedPoints[0]);//normal
                    Vector3 centreOfTarget = targetRoom.GetComponent<MeshRenderer>().bounds.center;
                    Vector3 closestPointFromTargetRoom = Divide.ClosestVerticeOnThisRoomToCentreOfTargetRoom(targetRoom, quads[i], sharedPoints[0]);

                    //run through target room vertices and find which was in a straight line with shared point [0] and the closest point from this room

                    Vector3[] othervertices = targetRoom.GetComponent<MeshFilter>().mesh.vertices;

                    Vector3 p1 = Vector3.zero;
                    Vector3 p2 = Vector3.zero;
                    for (int v = 0; v < othervertices.Length; v++)
                    {
                        if (othervertices[v] == sharedPoints[0])
                            continue;

                        if (Divide.PointsInLine(sharedPoints[0], closestPointFromThisRoom, othervertices[v]))
                        {
                            //Debug.Log("Using 1st option" + thisRoom.name);
                            p1 = closestPointFromThisRoom;
                            p2 = othervertices[v];
                        }

                        if (Divide.PointsInLine(sharedPoints[0], othervertices[v], closestPointFromThisRoom))
                        {
                            //  Debug.Log("Using 2nd option" + thisRoom.name);
                            p1 = othervertices[v];
                            p2 = closestPointFromThisRoom;
                        }
                    }
                    bool buildExtraWall = false; //not using, this may be a soluition instad of raycasting in MissingWalls, not investigating atm
                    if (p1 == Vector3.zero)
                    {
                        buildExtraWall = true;
                        //Debug.Break();
                        Vector3[] thisvertices = thisRoom.GetComponent<MeshFilter>().mesh.vertices;
                        //we didnt find a suitable wall, using the target room's vertices, let's try this room's instead - note we are using the closest point form target room now
                        Debug.Log("points in line first count was zero, switchin to other room, this room is " + thisRoom.name + " Is living room at the end? no wal between kitchen?");
                        for (int v = 0; v < thisvertices.Length; v++)
                        {
                            if (thisvertices[v] == sharedPoints[0])
                            {

                                Debug.Log("Continued form here");
                                continue;
                            }

                            if (Divide.PointsInLine(sharedPoints[0], closestPointFromTargetRoom, thisvertices[v]))
                            {
                                p1 = closestPointFromTargetRoom;
                                p2 = othervertices[v];
                            }

                            if (Divide.PointsInLine(sharedPoints[0], thisvertices[v], closestPointFromTargetRoom))
                            {
                                p1 = thisvertices[v];
                                p2 = closestPointFromTargetRoom;
                            }
                        }
                    }
                    if (p1 == Vector3.zero)
                    {
                        Debug.Log("still zero????");
                        continue;

                    }


                    Vector3 closestInLine = p1;
                    Vector3 furthestInLine = p2;
                    if (Vector3.Distance(sharedPoints[0], p1) > Vector3.Distance(sharedPoints[0], p2))
                    {
                        closestInLine = p2;
                        furthestInLine = p1;
                    }
                    //////////
                    Vector3 target = closestInLine;

                    if (target == sharedPoints[0])
                    {
                        Debug.Log("Break on " + thisRoom.name);
                        // Debug.Break();

                    }

                    //Vector3 endPointForDoor = closestInLine;//testing
                    Vector3 endPointForDoor = closestInLine;

                    //debug

                    #region cubes

                    /*
                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = sharedPoints[0];
                    c.name = "shared points [0] " + targetRoom.name;
                    c.transform.parent = thisRoom.transform;

                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = closestPointFromTargetRoom;
                    c.name = "closestPointOnTargetRoom";
                    c.transform.parent = thisRoom.transform;

                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = closestPointFromThisRoom;
                    c.name = "closestPointOnThisRoom";
                    c.transform.parent = thisRoom.transform;

                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = target;
                    c.name = "target";
                    c.transform.parent = thisRoom.transform;

                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = endPointForDoor;
                    c.name = "endPointForDoor";
                    c.transform.parent = thisRoom.transform;

                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = closestInLine;
                    c.name = "closest in line";
                    c.transform.parent = thisRoom.transform;
                    
                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = furthestInLine;
                    c.name = "furthest in line";
                    c.transform.parent = thisRoom.transform;
                    */

                    #endregion

                    if (buildExtraWall)
                    {
                        //GameObject ex = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        //ex.transform.position = Vector3.Lerp(furthestInLine, closestInLine, 0.5f);
                        //ex.transform.parent = roomsAndSizes[i].room.transform;

                    }

                    //check distance, //door must be positioned so at least one side has minimum object width - using 1f atm         
                    float distance = Vector3.Distance(sharedPoints[0], endPointForDoor);

                    //create list fof door point options along available wall
                    List<Vector3> doorOptions = new List<Vector3>();
                    Vector3 dir = (endPointForDoor - sharedPoints[0]).normalized;
                    //leaving a gap of 1 at each side // room size shouldnt push this too much - system isn't great but i tihnk it catches all problems
                    float gap = 0.5f;//door width here? // clamping on room creation too
                    for (float d = gap; d <= distance - gap; d += 0.1f)
                    {
                        Vector3 p = sharedPoints[0] + (dir * d);
                        // GameObject ex = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        //ex.transform.position = p;
                        //ex.transform.parent = roomsAndSizes[i].room.transform;
                        //ex.transform.localScale *= 0.5f;

                        //add collider stair check? onlyneed if shared points = 2 like above?

                       // if (!stairCollider.GetComponent<BoxCollider>().bounds.Contains(p))
                        {
                            doorOptions.Add(p);
                        }

                    }

                    //Debug.Log(doorOptions.Count + "door options " + "thisRoom = " + thisRoom.name + ", target room = " + targetRoom.name);
                    //randomly choose from this list

                    Vector3 doorPoint = Vector3.Lerp(sharedPoints[0], endPointForDoor, 0.5f);

                    if (doorOptions.Count != 0)
                        doorPoint = doorOptions[Random.Range(0, doorOptions.Count)];
                    else
                    {
                        // Debug.Break(); Debug.Log("NO DOOR POINTS, shared points = 1");
                        continue;
                    }
                    GameObject room = quads[i];
                    Vector3 globalP1 = room.transform.position + (room.transform.rotation * p1);
                    Vector3 globalP2 = room.transform.position + (room.transform.rotation * p2);
                    Vector3 globalDoorPoint = room.transform.position + (room.transform.rotation * doorPoint);

                    //build door using house builder class
                    Vector3 centre = room.GetComponent<MeshRenderer>().bounds.center;

                    //create points each side of the line - use "world" coordinates, because "centre" is a world co-ordinate
                    Vector3 lookDir1 = Quaternion.Euler(0, 90, 0) * (globalP2 - globalP1).normalized;
                    Vector3 lookDir2 = Quaternion.Euler(0, -90, 0) * (globalP2 - globalP1).normalized;
                    Vector3 lookDir = Vector3.zero;
                    //check which is closest - use that rotation to build wall
                    if (Vector3.Distance(globalDoorPoint + lookDir1, centre) > Vector3.Distance(globalDoorPoint + lookDir2, centre))
                        lookDir = Quaternion.Euler(0, 90, 0) * (p2 - p1).normalized;    //save as local direction
                    else
                        lookDir = Quaternion.Euler(0, -90, 0) * (p2 - p1).normalized; //save as local direction
                    GameObject door = HouseBuilder.DoorAtPosition(doorPoint, lookDir, quads[i],storeyHeight, doorHeight, doorWidth,divide);
                    door.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                    /*
                    //build the wall
                    List<List<GameObject>> doorThenWalls = Divide.DoorWithWall(thisRoom, sharedPoints[0], target, doorPoint, false, false);//never skip door, need for room items - must delete later - save these to list?
                    List<GameObject> doors = doorThenWalls[0];
                    doors[0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                    //add for interiors
                    List<GameObject> doorList = doorThenWalls[0];
                    doorsBuilt.Add(doorList[0]);
                    //remember edge
                    */


                    //determine which way to buiid wall
                    //Vector3 centre = quads[i].GetComponent<MeshRenderer>().bounds.center;
                    //create world positions


                    //edgesBuilt.Add(new Vector3[2] { closestPointToTarget, sharedPoints[0] });
                    Divide.RoomAndEdge rae = new Divide.RoomAndEdge();
                    rae.room = thisRoom;
                    rae.edge = new Vector3[2] { sharedPoints[0], target };
                    roomsAndEdges.Add(rae);

                    rae = new Divide.RoomAndEdge();
                    rae.room = targetRoom;
                    rae.edge = new Vector3[2] { sharedPoints[0], target };
                    roomsAndEdges.Add(rae);

                    //let the target room know we have placed a door here. It needs to know so it can also leave a gap for the door
                    Divide.WallWithDoor wwd = new Divide.WallWithDoor();
                    wwd.wallPoint1 = sharedPoints[0];
                    wwd.wallPoint2 = target;
                    wwd.doorPoint = doorPoint;
                    wwd.parent = thisRoom;
                    wwd.target = targetRoom;

                    wallsWithDoors.Add(wwd);

                    //force skip, we have found our door
                    r = 1000;
                }
                //else if we have no shared points with target room, try next room(smaller)
                else if (sharedPoints.Count == 0)
                {
                    Debug.Log("MISSING WALL");
                    continue;
                }
            }
        }
    }

    public static void DoorsForSegmentedHall(List<GameObject> quads, List<GameObject> halls, float storeyHeight,float doorHeight,float doorWidth,Divide divide)
    {
        //find which quads have shared points between corners of room
        for (int i = 0; i < quads.Count; i++)
        {
            List<Vector3> loop = new List<Vector3>(quads[i].GetComponent<MeshFilter>().mesh.vertices);
            loop.Add(loop[0]);

            List<Vector3[]> pairs = new List<Vector3[]>();

            for (int j = 0; j < loop.Count - 1; j++)
            {
                //search for hall quads that have two points between these corner points
                for (int k = 0; k < halls.Count; k++)
                {
                    List<Vector3> shared = new List<Vector3>();
                    Vector3[] hallVertices = halls[k].GetComponent<MeshFilter>().mesh.vertices;
                    for (int a = 0; a < hallVertices.Length; a++)
                    {
                       /* 
                        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        c.transform.position = loop[j];
                        c.transform.parent = quads[i].transform;
                        c.transform.localScale *= 0.5f;
                        c.name = j.ToString() + " j";
                        c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        c.transform.position = loop [j+1];
                        c.transform.parent = quads[i].transform;
                        c.transform.localScale *= 0.5f;
                        c.name = j.ToString() + " j";
                        c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        c.transform.position = hallVertices[a];
                        c.transform.parent = quads[i].transform;
                        c.transform.localScale *= 0.5f;
                        c.name = k.ToString() + " k";
                        */
                        if (Divide.PointsInLine(loop[j],loop[j+1],hallVertices[a]))
                        {


                            shared.Add(hallVertices[a]);// + Vector3.up * storeyHeight);
                        }
                    }

                    if(shared.Count == 2)
                    {
                        /*
                        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        c.transform.position = shared[0];
                        c.transform.parent = quads[i].transform;
                        c.transform.localScale *= 0.5f;
                        c.name = k.ToString();
                        c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        c.transform.position = shared[1];
                        c.transform.parent = quads[i].transform;
                        c.transform.localScale *= 0.5f;
                        c.name = k.ToString();
                        */
                        //add this pair to a list of possible places for the door ( if wide enough)
                        //float doorWidth = 1f; //??? -- needs var
                        if (Vector3.Distance(shared[0], shared[1]) > doorWidth)
                        {
                            Debug.Log("Door width too small?");
                            //Debug.Break();
                        }
                            if (Vector3.Distance(shared[0], shared[1]) > doorWidth/2)
                        {
                            //Debug.Log(Vector3.Distance(shared[0], shared[1]));
                            Vector3[] pair = new Vector3[2] { shared[0], shared[1] };
                            pairs.Add(pair);
                        }
                    }
                }
            }
           // Debug.Log("halss count = " + halls.Count);
           // Debug.Log("pairs count = " + pairs.Count);
            if (pairs.Count == 0)
            {
                //terminal failure!!
                Debug.Log("no door place could be found for room");
                Debug.Break();
                //continue;
            }
            //else
            {
                //randomly choose a pair
                int random = Random.Range(0, pairs.Count);

                //put a door in this pair
                Vector3 doorPoint = Vector3.Lerp(pairs[random][0], pairs[random][1], 0.5f);
               // List<List<GameObject>> doorsThenWalls = HouseBuilder.DoorWithWall(quads[i], pairs[random][0], pairs[random][1], doorPoint, false, false,storeyHeight,doorHeight,doorWidth);

               // doorsThenWalls[0][0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;

                GameObject door = HouseBuilder.OnlyDoor(quads[i], pairs[random][0], pairs[random][1], doorPoint,false, storeyHeight, doorHeight, doorWidth,divide);
                if (quads[i].name == "Room" || quads[i].name == "partner")//yup
                {
                    door.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("White") as Material;
                }
               
                door.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
            }
        }
    }

    public static void WallsForRoom(List<GameObject> quads,GameObject plot, int floor,float storeyHeight,float doorHeight,float doorWidth,float windowHeight,float windowWidth,float skirtingHeight,float skirtingDepth,Divide divide)
    {
        //create loop of plot mesh to check for exterior wall
        List<Vector3> plotLoop = new List<Vector3>(plot.GetComponent<MeshFilter>().mesh.vertices);
        plotLoop.Add(plotLoop[0]);

        for (int i = 0; i < quads.Count; i++)
        {

            //GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //c.transform.position = quads[i].GetComponent<MeshRenderer>().bounds.center;
            //c.transform.parent = quads[i].transform;

            Vector3[] quadVertices = quads[i].GetComponent<MeshFilter>().mesh.vertices;
            //make a loop with start and end
            List<Vector3> quadLoop = new List<Vector3>(quadVertices);
            quadLoop.Add(quadVertices[0]);
            //quadLoop.Insert(0, quadVertices[quadVertices.Length - 1]);

             
           


            //add floor
            for (int j = 0; j < quadLoop.Count - 1; j++)
            {
                

                
                
            }
       

            List<Vector3> doors = new List<Vector3>();
            for (int j = 0; j < quads[i].transform.childCount; j++)
            {
                if (quads[i].transform.GetChild(j).name == "Door")
                    doors.Add(quads[i].transform.GetChild(j).transform.position);
            }

            for (int j = 0; j < doors.Count; j++)
            {
                doors[j] -= Vector3.up * storeyHeight * floor;
            }

            for (int j = 0; j < quadLoop.Count-1; j++)
            {
                //check if this wall is an exterior wall -
                bool exterior = false;
                for (int k = 0; k < plotLoop.Count - 1; k++)                
                    //if both points are on outside edge - it's an exterior wall
                    if (Divide.PointsInLine(plotLoop[k], plotLoop[k + 1], quadLoop[j]))                   
                        if (Divide.PointsInLine(plotLoop[k], plotLoop[k + 1], quadLoop[j + 1]))                        
                            exterior = true;                    
                
                if (!exterior)
                {
                    for (int k = 0; k < doors.Count; k++)
                    {
                        if (Divide.PointsInLine(quadLoop[j], quadLoop[j + 1], doors[k]))//found door
                        {
                            List<List<GameObject>> walls = HouseBuilder.DoorWithWall(quads[i], quadLoop[j], quadLoop[j + 1], doors[k], true, false, storeyHeight * (floor), doorHeight, doorWidth, divide);

                            

                        }
                        else //no door
                        {
                            GameObject room = quads[i];
                            Vector3 globalP1 = room.transform.position + (room.transform.rotation * quadLoop[j]);
                            Vector3 globalP2 = room.transform.position + (room.transform.rotation * quadLoop[j + 1]);

                            /*
                            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            c.transform.position = globalP1;
                            c.transform.parent = quads[i].transform;

                            c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            c.transform.position = globalP2;
                            c.transform.parent = quads[i].transform;

                            c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            c.transform.position = doors[k];
                            c.transform.parent = quads[i].transform;
                            */
                            //build door using house builder class
                            Vector3 centre = room.GetComponent<MeshRenderer>().bounds.center;

                            //create points each side of the line - use "world" coordinates, because "centre" is a world co-ordinate
                            Vector3 lookDir1 = Quaternion.Euler(0, 90, 0) * (globalP2 - globalP1).normalized;
                            Vector3 lookDir2 = Quaternion.Euler(0, -90, 0) * (globalP2 - globalP1).normalized;
                            Vector3 lookDir = Vector3.zero;
                            //check which is closest - use that rotation to build wall
                            Vector3 mid = Vector3.Lerp(quadLoop[j], quadLoop[j + 1], 0.5f);
                            if (Vector3.Distance(mid + lookDir1, centre) > Vector3.Distance(mid + lookDir2, centre))
                                lookDir = Quaternion.Euler(0, 90, 0) * (quadLoop[j + 1] - quadLoop[j]).normalized;    //save as local direction
                            else
                                lookDir = Quaternion.Euler(0, -90, 0) * (quadLoop[j + 1] - quadLoop[j]).normalized; //save as local direction


                            float length = Vector3.Distance(quadLoop[j], quadLoop[j + 1]);


                            GameObject wall = HouseBuilder.WallWithLookDirection(quads[i], globalP1, globalP2, storeyHeight, doorHeight, doorWidth, floor,false, divide);
                            

                            if (exterior)
                                wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                        }
                    }
                }
                else
                {
                    //exterior goes here - lovely

                    //only build windows on longer edges?
                    Vector3[] vertices = quads[i].GetComponent<MeshFilter>().mesh.vertices;
                    int[] longestEdge = Divide.LongestEdge(vertices);
                    Vector3 longEdgeDir = (vertices[longestEdge[0]] - vertices[longestEdge[1]]).normalized;
                    Vector3 thisEdgeDir = (plotLoop[j] - plotLoop[j+1]).normalized;

                    //determine which way to buiid wall
                    Vector3 centre = quads[i].GetComponent<MeshRenderer>().bounds.center;
                    //create world positions
                    Vector3 p1 = quadLoop[j];
                    Vector3 p2 = quadLoop[j + 1];
                    Vector3 midPoint = Vector3.Lerp(p1, p2, 0.5f);

                    //create points each side of the line
                    Vector3 lookDir1 = Quaternion.Euler(0, 90, 0) * (p1 - p2).normalized;
                    Vector3 lookDir2 = Quaternion.Euler(0, -90, 0) * (p1 - p2).normalized;
                    Vector3 lookDir = Vector3.zero;
                    //check which is closest - use that rotation to build door
                    if (Vector3.Distance(midPoint + lookDir1, centre) < Vector3.Distance(midPoint + lookDir2, centre))
                        lookDir = Quaternion.Euler(0, 90, 0) * (p2 - p1).normalized;     //feed local coords to static -- static always applies rotations from room
                    else
                        lookDir = Quaternion.Euler(0, -90, 0) * (p2 - p1).normalized;
                    if (longEdgeDir == thisEdgeDir || longEdgeDir == -thisEdgeDir)
                    {
                        //interior
                        HouseBuilder.WallAroundWindowWithOffset(midPoint, p1, p2, Vector3.Distance(p1, p2), lookDir, false, quads[i], storeyHeight*floor,doorHeight, windowHeight, windowWidth,divide,floor);
                        //outside
                  
                        List<GameObject> walls = HouseBuilder.WallAroundWindowWithOffset(midPoint, p1, p2, Vector3.Distance(p1, p2), lookDir, true, quads[i], storeyHeight*floor,doorHeight, windowHeight, windowWidth,divide,floor);
                        foreach (GameObject wall in walls)
                            wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;

                        //build window using older function -
                        GameObject window = HouseBuilder.WindowAtPosition(midPoint,doorHeight ,windowHeight, windowWidth, lookDir, quads[i],storeyHeight*floor,divide);
                        window.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Glass") as Material;
                    }
                    else
                    {
                        //wall no window
                        //GameObject wall = HouseBuilder.Wall(midPoint, Vector3.Distance(p1, p2), lookDir, false, quads[i], storeyHeight*floor);
                        GameObject wall = HouseBuilder.WallWithLookDirection(quads[i], p1, p2, storeyHeight, doorHeight, doorWidth, floor, false, divide);
                        
                        //meh, function slightly fucked for upstairs
                        wall.transform.position += Vector3.up * storeyHeight*floor;//..yeah
                        wall = HouseBuilder.Wall(midPoint, Vector3.Distance(p1, p2), lookDir, true, quads[i], storeyHeight * floor);
                        wall.transform.position += Vector3.up * storeyHeight * floor;//..yeah
                        wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                    }
                }
            }
        }
    }

    public static GameObject CeilingsForRooms(List<GameObject> quads,float storeyHeight,GameObject stairCollider,int floor, int floors,float floorDepth,Divide divide)
    {

        //save this for later
        GameObject ForAboveCeiling = null;

        List<GameObject> ceilings = new List<GameObject>();
        for (int i = 0; i < quads.Count; i++)
        {
            //build ceiling - //will need to build ceiling for each room to crate different ceiling colours
            GameObject ceiling = new GameObject();
            MeshRenderer mr = ceiling.AddComponent<MeshRenderer>();
            mr.sharedMaterial = Resources.Load("White") as Material;
            MeshFilter mf = ceiling.AddComponent<MeshFilter>(); 
            ceiling.transform.position += Vector3.up*(storeyHeight*floor) ;
            ceiling.transform.parent = quads[i].transform;
            //ceiling.transform.rotation *= Quaternion.Euler(180, 0, 180);
            ceiling.name = "Ceiling";

            //add to LOD list
            ceilings.Add(ceiling);

            //copy and reverse normals so it points downwards
            
            List<Mesh> meshList = new List<Mesh>();

            
            List<GameObject> allSplits = new List<GameObject>();
            if (floors == 2 && quads[i].name == "Hall")
            {
                //we need to cut a hole for the stairs
                List<GameObject> splits = Divide.SplitWithDirection(quads[i], quads[i], stairCollider.transform.localScale.x, stairCollider.transform.forward, stairCollider.transform.position);

                //function sets quad passed inactive by default, undo this
                quads[i].SetActive(true);
                quads[i].GetComponent<MeshRenderer>().enabled = true;

                for (int j = 0; j < splits.Count; j++)
                {
                    //split these the other way now
                    List<GameObject> splits2 = Divide.SplitWithDirection(quads[i], splits[j], stairCollider.transform.localScale.z, stairCollider.transform.right, stairCollider.transform.position);
                    //function sets quad passed inactive by default, undo this

                    foreach (GameObject go in splits2)
                    {
                        go.transform.position += Vector3.up * storeyHeight*floor;

                        meshList.Add(go.GetComponent<MeshFilter>().mesh);
                        //save gameobject too -could use a class but just using two seperate lists  - the indexes will match
                        allSplits.Add(go);
                    }
                }

                quads[i].SetActive(true);
                quads[i].GetComponent<MeshRenderer>().enabled = true;
            }
            else           
                meshList.Add(quads[i].GetComponent<MeshFilter>().mesh);

            //Debug.Log("MESHLIST COUNT = " + meshList.Count);

            for (int j = 0; j < meshList.Count; j++)
            {
                Mesh mesh = new Mesh();

                mesh.vertices = meshList[j].vertices;
                mesh.normals = meshList[j].normals;
                mesh.triangles = meshList[j].triangles;


                Vector3[] normals = mesh.normals;
                for (int x = 0; x < normals.Length; x++)
                    normals[x] = -normals[x];
                mesh.normals = normals;

                for (int m = 0; m < mesh.subMeshCount; m++)
                {
                    int[] triangles = mesh.GetTriangles(m);
                    for (int x = 0; x < triangles.Length; x += 3)
                    {
                        int temp = triangles[x + 0];
                        triangles[x + 0] = triangles[x + 1];
                        triangles[x + 1] = temp;
                    }
                    mesh.SetTriangles(triangles, m);
                }
                if(allSplits.Count == 0)
                    mf.mesh = mesh;
                if (allSplits.Count == 4)                
                    allSplits[j].GetComponent<MeshFilter>().mesh = mesh;
                    
            }

            if (allSplits.Count > 0)
            {
                //now we need to find the quad which the stairs run through if this is the hall still and remove it
                float distance = Mathf.Infinity;
                int closest = 0;
                Vector3 startOfStairs = stairCollider.transform.position - stairCollider.transform.forward * stairCollider.transform.localScale.z * 0.5f;
                for (int j = 0; j < allSplits.Count; j++)
                {

                    float temp = Vector3.Distance(startOfStairs, allSplits[j].GetComponent<MeshRenderer>().bounds.center);
                    if (temp < distance)
                    {
                        distance = temp;
                        closest = j;
                    }

                }
                
                //allSplits[closest].SetActive(false);
                allSplits[closest].name = "ForAboveCeiling";
                allSplits[closest].transform.parent = quads[i].transform;
                allSplits[closest].transform.position += (storeyHeight+0.4f) * Vector3.up;
                ForAboveCeiling = allSplits[closest];

                //there is a small gap left between the floors beside the stairs-let's fill it in
                //we can use all splits closest quad to do this - 2 of it's vertices are in the correct postion already - one of the longer edges which is closest to centre of the hall

                //find both edges , forward and sidways
                List<int[]> longEdges = new List<int[]>();
                List<int[]> shortEdges = new List<int[]>();
                Vector3[] vertices = allSplits[closest].GetComponent<MeshFilter>().mesh.vertices;
                List<Vector3> loop = new List<Vector3>(vertices);
                loop.Add(loop[0]);

                int[] shortEdge = Divide.ShortestEdge(vertices);
                int[] longEdge = Divide.LongestEdge(vertices);

                Vector3 shortEdgeDir = (vertices[shortEdge[0]] - vertices[shortEdge[1]]).normalized;
                Vector3 longEdgeDir = (vertices[longEdge[0]] - vertices[longEdge[1]]).normalized;
                //organising edges in to short and long lists
                for (int j = 0; j < loop.Count - 1; j++)
                {
                    Vector3 edgeDir = (loop[j] - loop[j + 1]).normalized;


                    if (edgeDir == shortEdgeDir || edgeDir == -shortEdgeDir)
                        shortEdges.Add(new int[2] { j, j + 1 });

                    if (edgeDir == longEdgeDir|| edgeDir == -longEdgeDir)
                        longEdges.Add(new int[2] { j, j + 1 });
                }

                //order edges by distance from centre of room
                Vector3 centreOfHall = quads[i].GetComponent<MeshRenderer>().bounds.center;
               // Debug.Log("Long edges count " + longEdges.Count);
                longEdges.Sort(delegate(int[] edge1,int[] edge2)
                {
                    return (Vector3.Distance(Vector3.Lerp(loop[edge1[0]], loop[edge1[1]], 0.5f), centreOfHall).CompareTo(
                        Vector3.Distance(Vector3.Lerp(loop[edge2[0]], loop[edge2[1]], 0.5f), centreOfHall)));
                });

                
                Vector3 centrePoint = Vector3.Lerp(loop[longEdges[0][0]], loop[longEdges[0][1]], 0.5f);

                //creating cube - optimisation would be to just make two quads - Also, probably needs welded with ceiling - normals etc
                GameObject filler = GameObject.CreatePrimitive(PrimitiveType.Cube);
                filler.transform.parent = quads[i].transform;
                filler.transform.position = centrePoint + Vector3.up * (storeyHeight + floorDepth*0.5f);
                Vector3 towardsStair = (stairCollider.transform.position - filler.transform.position);
                towardsStair.y = 0;
                //normalize after we flatten the vector - the stair position is below cos we already moved it up a line aboe 
                filler.transform.position += towardsStair.normalized * 0.05f;
                filler.name = "Filler";
                filler.transform.localScale = new Vector3( 0.1f,floorDepth,stairCollider.transform.localScale.z);
                filler.transform.rotation = stairCollider.transform.rotation;

                ceilings.Add(filler);

                //space at othere side too could loop this .meh

                centrePoint = Vector3.Lerp(loop[longEdges[1][0]], loop[longEdges[1][1]], 0.5f);

                //creating cube - optimisation would be to just make two quads - Also, probably needs welded with ceiling - normals etc
                filler = GameObject.CreatePrimitive(PrimitiveType.Cube);

                filler.transform.position = centrePoint + Vector3.up * (storeyHeight + floorDepth*0.5f);
                towardsStair = (stairCollider.transform.position - filler.transform.position);
                towardsStair.y = 0;
                //normalize after we flatten the vector - the stair position is below cos we already moved it up a line aboe 
                filler.transform.position += towardsStair.normalized * 0.05f;
                filler.name = "Filler";
                filler.transform.localScale = new Vector3(0.1f, floorDepth, stairCollider.transform.localScale.z);
                filler.transform.rotation = stairCollider.transform.rotation;
                filler.transform.parent = quads[i].transform;
                ceilings.Add(filler);


                //gap at top of stairs
                shortEdges.Sort(delegate (int[] edge1, int[] edge2)
                {
                    return (Vector3.Distance(Vector3.Lerp(loop[edge1[0]], loop[edge1[1]], 0.5f), centreOfHall).CompareTo(
                        Vector3.Distance(Vector3.Lerp(loop[edge2[0]], loop[edge2[1]], 0.5f), centreOfHall)));
                });

                
                Vector3[] points = new Vector3[] { new Vector3(-stairCollider.transform.localScale.x*0.5f, -floorDepth*0.5f, 0.0f), new Vector3(stairCollider.transform.localScale.x * 0.5f, -floorDepth * 0.5f, 0.0f), new Vector3(-stairCollider.transform.localScale.x * 0.5f, floorDepth * 0.5f, 0.0f), new Vector3(stairCollider.transform.localScale.x * 0.5f, floorDepth * 0.5f, 0.0f) };
                GameObject top = Divide.Quad(quads[i],points);
                centrePoint = Vector3.Lerp(loop[shortEdges[0][0]], loop[shortEdges[0][1]], 0.5f);
                top.transform.position = centrePoint + (storeyHeight + floorDepth*0.5f) * Vector3.up;
                
                top.transform.rotation = stairCollider.transform.rotation;
                top.transform.rotation *= Quaternion.Euler(0, 180, 0);
                //top.transform.localScale = new Vector3( stairCollider.transform.localScale.x,0.1f,0.1f);               
                top.name = "Filler";
                top.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;
                ceilings.Add(top);

                //needed at rear too
                GameObject rear = Instantiate(top, top.transform.position,top.transform.rotation);
                rear.name = "Filler";
                rear.transform.rotation *= Quaternion.Euler(0, 180, 0);
                rear.transform.parent = quads[i].transform;
                rear.transform.position -= stairCollider.transform.forward * (stairCollider.transform.localScale.z - 0.1f);

                ceilings.Add(rear);
            }
        }

        //add to assetts list so we can split in to LODs
        divide.interiorAssetsByRoom.Add(ceilings);

        return ForAboveCeiling;
    }

    public static GameObject HallWalls(List<GameObject> quads, List<GameObject> halls,GameObject plot, float storeyHeight,int floor,GameObject gameObject,GameObject stairCollider,float doorHeight,float doorWidth,GameObject parent,float skirtingHeight,float skirtingDepth,Divide divide)
    {

        //first of all make a list of midpoints between each quad (where you would walk along) - if they are adjacent
        List<Vector3> wallMidPoints = new List<Vector3>();
        foreach(GameObject go in halls)
        {   
            Vector3[] vs = go.GetComponent<MeshFilter>().mesh.vertices;
            
            foreach (GameObject go2 in halls)
            {
                List<Vector3> shared = new List<Vector3>();

                if (go == go2)
                    continue;

                
                Vector3[] vs2 = go2.GetComponent<MeshFilter>().mesh.vertices;
                foreach (Vector3 v1 in vs)
                    foreach (Vector3 v2 in vs2)
                        if (v1 == v2)
                            shared.Add(v1);

                if (shared.Count == 2)
                {
                    Vector3 mp = Vector3.Lerp(shared[0], shared[1], 0.5f) + storeyHeight*Vector3.up;
                    if(!wallMidPoints.Contains(mp))
                        wallMidPoints.Add(mp);

                    continue;
                }
            }
        }
        //we will need the plotmesh in a loop to check for exterior walls
        //loop of plot mesh
        List<Vector3> plotLoop = new List<Vector3>();
        Vector3[] plotVertices = plot.GetComponent<MeshFilter>().mesh.vertices;
        foreach (Vector3 v3 in plotVertices)
        {
            //add floor and storey height to plot 
            Vector3 temp = v3 + Vector3.up * (storeyHeight * floor);
            plotLoop.Add(temp);
        }
        plotLoop.Add(plotVertices[0] + Vector3.up * (storeyHeight * floor));

        //we do not want to build a wall at the point where the stairs are, we can add the stair midpoint here
        Vector3 frontRight = stairCollider.transform.position + stairCollider.transform.forward * stairCollider.transform.localScale.z * 0.5f + stairCollider.transform.right * stairCollider.transform.localScale.x * 0.5f + Vector3.up * storeyHeight;

        Vector3 frontLeft = stairCollider.transform.position + stairCollider.transform.forward * stairCollider.transform.localScale.z * 0.5f - stairCollider.transform.right * stairCollider.transform.localScale.x * 0.5f + Vector3.up* storeyHeight;         

        Vector3 stairMidpoint = Vector3.Lerp(frontLeft, frontRight, 0.5f);

        wallMidPoints.Add(stairMidpoint);

        //remember which quads are at the end of the hall (they can be not square
        List<Vector3> endPoints = new List<Vector3>();

        foreach (GameObject go in halls)
        {
            if (go.name == "Hall2")
            {
                Vector3[] vs = go.GetComponent<MeshFilter>().mesh.vertices;
                foreach (Vector3 v in vs)
                    endPoints.Add(v + storeyHeight * Vector3.up);
            }
        }
        /*
        foreach (Vector3 v3 in wallMidPoints)
        {            
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = v3;
            c.transform.localScale *= 0.5f;
            c.transform.parent = gameObject.transform;
            c.name = "mid";
        }
        */
        //weld mesh together in to new mesh

        List<MeshFilter> filterList = new List<MeshFilter>();
        foreach (GameObject hall in halls)
            filterList.Add(hall.GetComponent<MeshFilter>());

        MeshFilter[] meshFilters = filterList.ToArray();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            //meshFilters[i].gameObject.active = false;
            i++;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        Mesh weldedMesh = Divide.AutoWeldFunction(combinedMesh, 0.01f, 100);

        GameObject weldo = new GameObject();
        MeshFilter mf = weldo.AddComponent<MeshFilter>();
        mf.mesh = weldedMesh;
        //weldo.AddComponent<MeshRenderer>();
        weldo.transform.parent = gameObject.transform;
        weldo.name = "weldo";
        //use thi smesh to find edges

        Vector3[] vertices = weldedMesh.vertices;
        int[] triangles = weldedMesh.triangles;
        
        //use edge helper class to get outside edges
        List<EdgeHelpers.Edge> boundaryPath = EdgeHelpers.GetEdges(weldedMesh.triangles).FindBoundary().SortEdges();

        //get the doors 
        List<Vector3> doors = new List<Vector3>();
        for (int a = 0; a < quads.Count; a++)
        {
            for (int j = 0; j < quads[a].transform.childCount; j++)
            {
                if (quads[a].transform.GetChild(j).name == "Door")
                    doors.Add(quads[a].transform.GetChild(j).transform.position);
            }
        }
        List<Vector3> wallsPos = new List<Vector3>();
        
        for (int j = 0; j < boundaryPath.Count; j++)
        {
            //if (skip == true)
            //    continue;

            


            Vector3 v1 = vertices[boundaryPath[j].v1];
            Vector3 v2 = vertices[boundaryPath[j].v2];

            //checking if we moved any points -skip if we did - cutting out a jutty wall
            if (v1 == v2)
                continue;
            foreach (Vector3 v3 in endPoints)
            {
                if (Vector3.Distance(v3,v1) > 0.1f && Vector3.Distance(v3, v2) > 0.1f)
                {
                    if (Divide.PointsInLine(v1, v2, v3))
                    {
                        //Debug.Break();
                        j++;
                        v2 = v3;
                    

                        /*
                        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        c.transform.position = vertices[boundaryPath[j].v1];
                        c.transform.parent = gameObject.transform;
                        c.name = "v1";

                        c.transform.localScale *= 0.25f;

                        c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        c.transform.position = vertices[boundaryPath[j].v2];
                        c.transform.parent = gameObject.transform;
                        c.name = "v2";
                        c.transform.localScale *= 0.25f;
                        */
                        //catching a bug with find edges, sometimes it want to loop back round and build here- let's add the midpoint to the wall mid point list so it thinks it has already built - #work around
                        Vector3 mp = Vector3.Lerp(vertices[boundaryPath[j].v1], vertices[boundaryPath[j].v2], 0.5f);
                        wallMidPoints.Add(mp);
                        /*
                        c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        c.transform.position = mp;
                        c.transform.parent = gameObject.transform;
                        c.name = "mid";
                        c.transform.localScale *= 0.5f;
                        */
                       

                    }
                    
                }
            }


            //check if there is a door 
            bool doorBuilt = false;
            
            for(int k = 0; k < doors.Count;k++)
            {
                if(Divide.PointsInLine(v1,v2,doors[k]))
                {
                    doorBuilt = true;
                    Vector3 mid = Vector3.Lerp(v1, v2, 0.5f);
                    if (!wallsPos.Contains(mid))
                    {
                        bool tooClose = false;
                        foreach (Vector3 v3 in wallMidPoints)
                        {
                            if (Vector3.Distance(v3, mid) < 0.1f)
                                tooClose = true;
                        }

                        if (!tooClose)
                        {

                            //find closest quad to point this towards- usig wleded mesh can give off results since it's centre can be anywhere
                            float d = Mathf.Infinity;
                            int closest = 0;
                            for (int a = 0; a < meshFilters.Length; a++)
                            {

                                float temp = Vector3.Distance(meshFilters[a].mesh.bounds.center, mid);
                                if(temp < d)
                                {
                                    d = temp;
                                    closest = a;
                                }
                            }
                            GameObject closestQuad = meshFilters[closest].gameObject;
                            List<List<GameObject>> wallsAndDoor = HouseBuilder.DoorWithWall(closestQuad, v1, v2, doors[k], true, false,storeyHeight,doorHeight,doorWidth, divide);
                            
                            //need to move -maybe because meshfilter was used? I'm not sure why sometimes i need to move things up or downa storey atm
                            for (int a = 0; a < wallsAndDoor.Count; a++)
                            {
                                for (int b = 0; b < wallsAndDoor[a].Count; b++)
                                {
                                    wallsAndDoor[a][b].transform.position -= Vector3.up * storeyHeight;
                                    wallsAndDoor[a][b].transform.parent = parent.transform;
                                }
                            }
                            wallsPos.Add(mid);
                        }
                    }
                }
            }
            if (!doorBuilt)
            {
                Vector3 mid = Vector3.Lerp(v1, v2, 0.5f);
                if (!wallsPos.Contains(mid))
                {
                    //if (!wallMidPoints.Contains(mid))
                    bool tooClose = false;
                    foreach(Vector3 v3 in wallMidPoints)
                    {
                        if (Vector3.Distance(v3, mid) < 0.1f)
                            tooClose = true;
                    }

                    if(!tooClose)
                    {
                        //find closest quad to point this towards- usig wleded mesh can give off results since it's centre can be anywhere
                        float d = Mathf.Infinity;
                        int closest = 0;
                        for (int a = 0; a < meshFilters.Length; a++)
                        {

                            float temp = Vector3.Distance(meshFilters[a].mesh.bounds.center, mid);
                            if (temp < d)
                            {
                                d = temp;
                                closest = a;
                            }
                        }
                        GameObject closestQuad = meshFilters[closest].gameObject;

                        //check if exterior wall

                        //check if this wall is an exterior wall -
                        bool exterior = false;
                        for (int k = 0; k < plotLoop.Count - 1; k++)
                            //if both points are on outside edge - it's an exterior wall
                            if (Divide.PointsInLine(plotLoop[k], plotLoop[k + 1], v1))
                                if (Divide.PointsInLine(plotLoop[k], plotLoop[k + 1], v2))
                                    exterior = true;

                        if (!exterior)
                        {
                            GameObject wall = HouseBuilder.WallWithLookDirection(closestQuad, v1, v2, storeyHeight, doorHeight, doorWidth, floor, false, divide);
                            wallsPos.Add(mid);
                            wall.name = j.ToString();
                            wall.transform.parent = parent.transform;

                        }
                        else if(exterior)
                        {
                            //make window? //always?
                            bool makeWindow = true;
                            if (Random.Range(0, 2) == 0)
                                makeWindow = false;

;                            if (makeWindow)
                            {
                                //determine which way to buiid wall
                                Vector3 centre1 = closestQuad.GetComponent<MeshRenderer>().bounds.center;

                                Vector3 midPoint1 = Vector3.Lerp(v1, v2, 0.5f);

                                //create points each side of the line
                                Vector3 lookDir1a = Quaternion.Euler(0, 90, 0) * (v1 - v2).normalized;
                                Vector3 lookDir2a = Quaternion.Euler(0, -90, 0) * (v1 - v2).normalized;
                                Vector3 lookDira = Vector3.zero;
                                //check which is closest - use that rotation to build door
                                if (Vector3.Distance(midPoint1 + lookDir1a, centre1) < Vector3.Distance(midPoint1 + lookDir2a, centre1))
                                    lookDira = Quaternion.Euler(0, 90, 0) * (v2- v1).normalized;     //feed local coords to static -- static always applies rotations from room
                                else
                                    lookDira = Quaternion.Euler(0, -90, 0) * (v2 - v1).normalized;

                                GameObject window = HouseBuilder.WindowAtPosition(midPoint1, doorHeight, gameObject.GetComponent<Divide>().windowHeight,1f, lookDira,closestQuad, storeyHeight, divide);

                                //..window too high
                                window.transform.position -= Vector3.up * storeyHeight;
                                window.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Glass") as Material;

                                List<GameObject> walls = HouseBuilder.WallAroundWindowWithOffset(window.transform.position, v1, v2, Vector3.Distance(v1, v2), lookDir1a, false, closestQuad, storeyHeight, gameObject.GetComponent<Divide>().doorHeight, gameObject.GetComponent<Divide>().windowHeight, 1, divide,floor);
                                foreach (GameObject wall in walls)
                                {
                                    wall.transform.position -= Vector3.up * storeyHeight;
                                 
                                }
                                //move skirting board down - this is a messsss - too scared to chagne it!
                                closestQuad.transform.GetChild(5).transform.position -= Vector3.up * storeyHeight;


                                walls = HouseBuilder.WallAroundWindowWithOffset(window.transform.position, v1, v2, Vector3.Distance(v1, v2), lookDir1a, true, closestQuad, storeyHeight, gameObject.GetComponent<Divide>().doorHeight, gameObject.GetComponent<Divide>().windowHeight, 1, divide,floor);
                                //too high as well
                                foreach (GameObject wall in walls)
                                {
                                    wall.transform.position -= Vector3.up * storeyHeight;
                                    wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                                }

                            }
                            else
                            {
                                GameObject wall = HouseBuilder.WallWithLookDirection(closestQuad, v1, v2, storeyHeight, doorHeight, doorWidth, floor, false, divide);
                                wallsPos.Add(mid);
                                wall.name = j.ToString();
                                wall.transform.parent = parent.transform;

                                wall = HouseBuilder.WallWithLookDirection(closestQuad, v1, v2, storeyHeight, doorHeight, doorWidth, floor, true,divide);
                                wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                            }

                        }
                        //if (yup)
                        //    wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;

                    }                
                }
            }
        }
        return weldo;
    }

    public static List<GameObject> GroundFloorExteriorWalls(GameObject gameObject, List<GameObject> quads, GameObject plot, float storeyHeight, float doorHeight, float doorWidth,float windowHeight,float windowWidth,GameObject stairCollider,int floors,int rooms,float skirtingHeight,float skirtingDepth,Divide divide)
    {

        //return list
        List<GameObject> wallsReturned = new List<GameObject>();
        

        int floor = 1;//this is wrong..should be 0
        //make plot loop
        List<Vector3> plotLoop = new List<Vector3>();
        Vector3[] plotVertices = plot.GetComponent<MeshFilter>().mesh.vertices;
        foreach (Vector3 v3 in plotVertices)
            plotLoop.Add(v3);
        plotLoop.Add(plotVertices[0]);// + Vector3.up * (storeyHeight * floor));

        //go through quads and look for exterior edges
        for (int i = 0; i < quads.Count; i++)
        {
            List<Vector3[]> exteriorEdges = new List<Vector3[]>();
            List<GameObject> windows = new List<GameObject>();
            List<GameObject> doors = new List<GameObject>();

            List<Vector3> verticesLoop = new List<Vector3>();
            Vector3[] vs = quads[i].GetComponent<MeshFilter>().mesh.vertices;
            foreach (Vector3 v3 in vs)
                verticesLoop.Add(v3);
            verticesLoop.Add(vs[0]);

            Vector3 p1 = new Vector3();
            Vector3 p2 = new Vector3();

            for (int j = 0; j < verticesLoop.Count - 1; j++)
            {
                bool exterior = false;
                for (int k = 0; k < plotLoop.Count - 1; k++)
                    //if both points are on outside edge - it's an exterior wall
                    if (Divide.PointsInLine(plotLoop[k], plotLoop[k + 1], verticesLoop[j]))
                        if (Divide.PointsInLine(plotLoop[k], plotLoop[k + 1], verticesLoop[j + 1]))
                            exterior = true;

                //build walls and windows if exterior
                if (exterior)
                {
                    Vector3[] edge = new Vector3[] { verticesLoop[j], verticesLoop[j+1] };
                    exteriorEdges.Add(edge);
                }
            }


            //hall left some stuff to finsih off..should be in anoher function perha[s
            if (quads[i].name == "Hall")
            {
                
                if (floors == 2)
                {
                    
                    exteriorEdges.Sort(delegate (Vector3[] edge1, Vector3[] edge2)
                    {
                        Vector3 m1 = Vector3.Lerp(edge1[0], edge1[1], 0.5f);
                        Vector3 m2 = Vector3.Lerp(edge2[0], edge2[1], 0.5f);
                        Vector3 s = stairCollider.transform.position - stairCollider.transform.localScale.z * stairCollider.transform.forward;
                        return Vector3.Distance(m1, s).CompareTo((Vector3.Distance(m2, s)));

                    });

                    for (int j = 0; j < exteriorEdges.Count; j++)
                    {

                        p1 = exteriorEdges[j][0];
                        p2 = exteriorEdges[j][1];

                        //determine which way to buiid wall
                        Vector3 centre1 = quads[i].GetComponent<MeshRenderer>().bounds.center;

                        Vector3 midPoint1 = Vector3.Lerp(p1, p2, 0.5f);

                        //create points each side of the line
                        Vector3 lookDir1a = Quaternion.Euler(0, 90, 0) * (p1 - p2).normalized;
                        Vector3 lookDir2a = Quaternion.Euler(0, -90, 0) * (p1 - p2).normalized;
                        Vector3 lookDira = Vector3.zero;
                        //check which is closest - use that rotation to build door
                        if (Vector3.Distance(midPoint1 + lookDir1a, centre1) < Vector3.Distance(midPoint1 + lookDir2a, centre1))
                            lookDira = Quaternion.Euler(0, 90, 0) * (p2 - p1).normalized;     //feed local coords to static -- static always applies rotations from room
                        else
                            lookDira = Quaternion.Euler(0, -90, 0) * (p2 - p1).normalized;

                        Vector3 mid = Vector3.Lerp(p1, p2, 0.5f);
                        Vector3 furthest = p1;
                        if (Vector3.Distance(p2, stairCollider.transform.position) > Vector3.Distance(p1, stairCollider.transform.position))
                        {
                            furthest = p2;
                        }
                        Vector3 doorPos = Vector3.Lerp(mid, furthest, 0.5f);
                        //build door on closer edge
                        if (j == 0)
                        {
                            List<List<GameObject>> doorThenWalls = HouseBuilder.DoorWithWall(quads[i], p1, p2, doorPos, true, true, storeyHeight, doorHeight, doorWidth, divide);
                            for (int k = 0; k < doorThenWalls[0].Count; k++)
                            {
                                doorThenWalls[0][k].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                            }

                            //door
                            wallsReturned.Add(doorThenWalls[0][0]);
                            //walls
                            foreach (GameObject w in doorThenWalls[0])
                                wallsReturned.Add(w);

                            doorThenWalls = HouseBuilder.DoorWithWall(quads[i], p1, p2, doorPos, false, false, storeyHeight, doorHeight, doorWidth, divide);
                            doorThenWalls[0][0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;

                            //door
                            wallsReturned.Add(doorThenWalls[0][0]);
                            //walls
                            foreach (GameObject w in doorThenWalls[1])
                                wallsReturned.Add(w);
                            

                            //HouseBuilder.WallAroundDoorWithOffset(doorPos, p1, p2, Vector3.Distance(p1, p2), lookDira, false, quads[i], storeyHeight, doorHeight, doorWidth);
                        }
                        if (j == 1)
                        {
                            
                            GameObject wall1 = HouseBuilder.WallWithLookDirection(quads[i], p1, p2, storeyHeight, doorHeight, doorWidth, 1, false, divide);
                            wallsReturned.Add(wall1);
                            GameObject wall2 = HouseBuilder.WallWithLookDirection(quads[i], p1, p2, storeyHeight, doorHeight, doorWidth, 1, true, divide);
                            wall2.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                            wallsReturned.Add(wall2);
                        }
                        if (j == 2)
                        {
                            //could maybe put a window in? why not
                            Vector3 windowPos = Vector3.Lerp(p1, p2, 0.5f);
                            float hallWindowWidth = 1f;
                            GameObject window = HouseBuilder.WindowAtPosition(windowPos, doorHeight, windowHeight, hallWindowWidth, lookDira, quads[i], storeyHeight, divide);
                            window.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Glass") as Material;

                            //adding window to exterior, possibly could be moved to interior
                            wallsReturned.Add(window);
                            List<GameObject> walls = HouseBuilder.WallAroundWindowWithOffset(windowPos, p1, p2, Vector3.Distance(p1, p2), lookDira, false, quads[i], storeyHeight, doorHeight, windowHeight, windowWidth, divide, floor);
                            walls = HouseBuilder.WallAroundWindowWithOffset(windowPos, p1, p2, Vector3.Distance(p1, p2), lookDira, true, quads[i], storeyHeight, doorHeight, windowHeight, windowWidth, divide, floor);
                            foreach (GameObject wall in walls)
                            {
                                wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                                wallsReturned.Add(wall);
                            }
                        }
                    }
                }

                if (floors == 1)
                {
                    
                    //Debug.Log("count of ex edges = " + exteriorEdges.Count);
                    for (int j = 0; j < exteriorEdges.Count; j++)
                    {

                        p1 = exteriorEdges[j][0];
                        p2 = exteriorEdges[j][1];


                        Vector3 doorPos = Vector3.Lerp(p1, p2, 0.5f);
                        //build door on closer edge
                        if (j == 0)
                        {
                            List<List<GameObject>> doorThenWalls = HouseBuilder.DoorWithWall(quads[i], p1, p2, doorPos, false, true, storeyHeight, doorHeight, doorWidth, divide);
                            for (int k = 0; k < doorThenWalls[1].Count; k++)
                            {
                                doorThenWalls[1][k].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                            }

                            doorThenWalls = HouseBuilder.DoorWithWall(quads[i], p1, p2, doorPos, false, false, storeyHeight, doorHeight, doorWidth, divide);
                            doorThenWalls[0][0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;
                            //door
                            wallsReturned.Add(doorThenWalls[0][0]);
                            //walls
                            foreach (GameObject w in doorThenWalls[1])
                                wallsReturned.Add(w);
                           
                        }
                        if (j == 1)
                        {
                            GameObject wall1 = HouseBuilder.WallWithLookDirection(quads[i], p1, p2, storeyHeight, doorHeight, doorWidth, 1, false, divide);
                            GameObject wall2 = HouseBuilder.WallWithLookDirection(quads[i], p1, p2, storeyHeight, doorHeight, doorWidth, 1, true, divide);
                            wall2.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;

                            wallsReturned.Add(wall1);
                            wallsReturned.Add(wall2);
                        }
                    }
                }
            }
            
            //can be changed by belowe function if only one sink is found - is this working?
            float widthOfKitchenWindow = windowWidth;

            for (int j = 0; j < exteriorEdges.Count; j++)
            {
                //determine which way to buiid wall
                Vector3 centre = quads[i].GetComponent<MeshRenderer>().bounds.center;

                //get look direction just now
                p1 = exteriorEdges[j][0];
                p2 = exteriorEdges[j][1];
                Vector3 midPoint = Vector3.Lerp(p1, p2, 0.5f);

                //create points each side of the line
                Vector3 lookDir1 = Quaternion.Euler(0, 90, 0) * (p1 - p2).normalized;
                Vector3 lookDir2 = Quaternion.Euler(0, -90, 0) * (p1 - p2).normalized;
                Vector3 lookDir = Vector3.zero;
                //check which is closest - use that rotation to build door
                if (Vector3.Distance(midPoint + lookDir1, centre) < Vector3.Distance(midPoint + lookDir2, centre))
                    lookDir = Quaternion.Euler(0, 90, 0) * (p2 - p1).normalized;     //feed local coords to static -- static always applies rotations from room
                else
                    lookDir = Quaternion.Euler(0, -90, 0) * (p2 - p1).normalized;

                if (quads[i].name == "Hall")
                {
                    //already sorted above - should move it down here
                    continue;
                }
                //if kitchen
                else if (quads[i].name == "Kitchen")
                {
                    //build sink window first
                    p1 = exteriorEdges[j][0];
                    p2 = exteriorEdges[j][1];
                    //find the sink/sinks, we will place a window above it
                    List<GameObject> sinks = new List<GameObject>();
                    for (int x = 0; x < quads[i].transform.childCount; x++)
                    {
                        if (quads[i].transform.GetChild(x).name == "Sink")
                            sinks.Add(quads[i].transform.GetChild(x).gameObject);
                    }
                    if (sinks.Count == 0)
                    {
                        Debug.Log("No sinks!");
                        continue;
                    }
                    //get sink centre - can be a double, get avg
                    Vector3 avg = Vector3.zero;
                    foreach (GameObject sink in sinks)
                        avg += sink.transform.position;
                    avg /= sinks.Count;
                    avg += sinks[0].transform.forward * -0.6f - Vector3.up * 0.5f;

                 

                    //is this point on this edge - build sink window
                    if (Divide.PointsInLine(p1, p2, avg)) //0.5f is half of the 1 metre sink height - maybe a var is needed
                    {

                        //width is how many sinks there are
                        float sinksWidth = sinks[0].transform.localScale.x * sinks.Count;

                        //save for below function/loop
                        widthOfKitchenWindow = sinksWidth;
                        //save for window frame - defno the the best way to do this-could measure mesh verts?
                        if(sinksWidth ==1f)
                            divide.smallKitchenWindow = true;


                        GameObject window = HouseBuilder.WindowAtPosition(avg,doorHeight, windowHeight, windowWidth, lookDir, quads[i], storeyHeight, divide);
                        window.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Glass") as Material;

                        windows.Add(window);

                        wallsReturned.Add(window);

                    }
                }
                //bathroom and bedroom
               //else if(quads[i].name == "LivingRoom")///***removing, will build windows on edges now
                //{

                //}
                else if(quads[i].name == "Bedroom" || quads[i].name == "Bathroom")
                {

                    float tempWidth = windowWidth;
                    if (quads[i].name == "Bathroom")
                        tempWidth = 1f;
                    //only build windows on longer edges of house?-
                    Vector3[] vertices = quads[i].GetComponent<MeshFilter>().mesh.vertices;
                    
                    int[] longestEdge = Divide.LongestEdge(plotVertices);
                    Vector3 longEdgeDir = (plotVertices[longestEdge[0]] - plotVertices[longestEdge[1]]).normalized;
                    Vector3 thisEdgeDir = (plotLoop[j] - plotLoop[j + 1]).normalized;

                    //living room can have 2 windows, bedrooms too I guess?
                    bool buildWindow = false;
                    if (quads[i].name == "LivingRoom" && windows.Count < 2)
                        buildWindow = true;
                    else if (longEdgeDir == thisEdgeDir || longEdgeDir == -thisEdgeDir)
                        if(windows.Count < 2)
                            buildWindow = true;
                    
                    
                    if(buildWindow)
                    {
                        //interior
                        List<GameObject> wallsAroundWindow = HouseBuilder.WallAroundWindowWithOffset(midPoint, p1, p2, Vector3.Distance(p1, p2), lookDir, false, quads[i], storeyHeight * floor, doorHeight, windowHeight, tempWidth, divide,floor);
                        //LODS=s
                        foreach (GameObject w in wallsAroundWindow)
                            wallsReturned.Add(w);
                        //outside

                        List<GameObject> walls = HouseBuilder.WallAroundWindowWithOffset(midPoint, p1, p2, Vector3.Distance(p1, p2), lookDir, true, quads[i], storeyHeight * floor, doorHeight, windowHeight, tempWidth, divide,floor);
                        foreach (GameObject wall in walls)
                        {
                            wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                            wallsReturned.Add(wall);
                        }

                        //build window
                        GameObject window = HouseBuilder.WindowAtPosition(midPoint, doorHeight, windowHeight, tempWidth, lookDir, quads[i], storeyHeight * floor, divide);
                        window.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Glass") as Material;

                        wallsReturned.Add(window);
                    }
                    else
                    {
                        //wall no window
                        //GameObject wall = HouseBuilder.Wall(midPoint, Vector3.Distance(p1, p2), lookDir, false, quads[i], storeyHeight * floor);
                        GameObject wall = HouseBuilder.WallWithLookDirection(quads[i], p1, p2, storeyHeight, doorHeight, doorWidth, floor, false, divide);
                        wallsReturned.Add(wall);
                        //meh, function slightly fucked for upstairs
                        // wall.transform.position += Vector3.up * storeyHeight * floor;//..yeah
                        wall = HouseBuilder.Wall(midPoint, Vector3.Distance(p1, p2), lookDir, true, quads[i], storeyHeight * floor);
                        // wall.transform.position += Vector3.up * storeyHeight * floor;//..yeah
                        wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;

                        wallsReturned.Add(wall);
                    }
                }

                else if (quads[i].name == "LivingRoom")
                {

                    float tempWidth = windowWidth;
                   
                    //only build windows on longer edges of house?-
                    Vector3[] vertices = quads[i].GetComponent<MeshFilter>().mesh.vertices;

                    int[] longestEdge = Divide.LongestEdge(plotVertices);
                    Vector3 longEdgeDir = (plotVertices[longestEdge[0]] - plotVertices[longestEdge[1]]).normalized;
                    Vector3 thisEdgeDir = (plotLoop[j] - plotLoop[j + 1]).normalized;

                    //living room can have 2 windows, bedrooms too I guess?
                    bool buildWindow = false;
                    if (quads[i].name == "LivingRoom" && windows.Count < 2)
                        buildWindow = true;
                    else if (longEdgeDir == thisEdgeDir || longEdgeDir == -thisEdgeDir)
                        if (windows.Count < 2)
                            buildWindow = true;


                    if (buildWindow)
                    {
                        //interior
                        //HouseBuilder.WallAroundWindowWithOffset(midPoint, p1, p2, Vector3.Distance(p1, p2), lookDir, false, quads[i], storeyHeight * floor, doorHeight, windowHeight, tempWidth, divide, floor);
                        //outside

                        //List<GameObject> walls = HouseBuilder.WallAroundWindowWithOffset(midPoint, p1, p2, Vector3.Distance(p1, p2), lookDir, true, quads[i], storeyHeight * floor, doorHeight, windowHeight, tempWidth, divide, floor);
                       // foreach (GameObject wall in walls)
                       //     wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;

                        //build window
                        GameObject window = HouseBuilder.WindowAtPosition(midPoint, doorHeight, windowHeight, tempWidth, lookDir, quads[i], storeyHeight * floor, divide);
                        window.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Glass") as Material;

                        windows.Add(window);

                        wallsReturned.Add(window);
                    }
                    else
                    {
                        //wall no window
                        //GameObject wall = HouseBuilder.Wall(midPoint, Vector3.Distance(p1, p2), lookDir, false, quads[i], storeyHeight * floor);


                        //**Build below?

                        /*
                        GameObject wall = HouseBuilder.WallWithLookDirection(quads[i], p1, p2, storeyHeight, doorHeight, doorWidth, floor, false, divide);
                        //meh, function slightly fucked for upstairs
                        // wall.transform.position += Vector3.up * storeyHeight * floor;//..yeah
                        wall = HouseBuilder.Wall(midPoint, Vector3.Distance(p1, p2), lookDir, true, quads[i], storeyHeight * floor);
                        // wall.transform.position += Vector3.up * storeyHeight * floor;//..yeah
                        wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;

                        */
                    }
                }
            }

            //we need to make sure there has been window placed in kitchen - above loop tries to find exterior wall with sink- but sink can be on interior
            
            //Options for the door were dropped when kitchen layout was building
            List<GameObject> doorOptions = new List<GameObject>();
            for (int k = 0; k < quads[i].transform.childCount; k++)
            {
                if (quads[i].transform.GetChild(k).name == "DoorOption")
                    doorOptions.Add(quads[i].transform.GetChild(k).gameObject);
            }

            if (quads[i].name == "Kitchen" && windows.Count == 0)
            {
               // Debug.Break();

                Vector3[] edgeToUse = FindOppositeEdge(quads[i], exteriorEdges);
               

                p1 = edgeToUse[0];
                p2 = edgeToUse[1];

                //determine which way to buiid wall
                Vector3 centre1 = quads[i].GetComponent<MeshRenderer>().bounds.center;

                Vector3 midPoint1 = Vector3.Lerp(p1, p2, 0.5f);

                //create points each side of the line
                Vector3 lookDir1a = Quaternion.Euler(0, 90, 0) * (p1 - p2).normalized;
                Vector3 lookDir2a = Quaternion.Euler(0, -90, 0) * (p1 - p2).normalized;
                Vector3 lookDira = Vector3.zero;
                //check which is closest - use that rotation to build door
                if (Vector3.Distance(midPoint1 + lookDir1a, centre1) < Vector3.Distance(midPoint1 + lookDir2a, centre1))
                    lookDira = Quaternion.Euler(0, 90, 0) * (p2 - p1).normalized;     //feed local coords to static -- static always applies rotations from room
                else
                    lookDira = Quaternion.Euler(0, -90, 0) * (p2 - p1).normalized;

                Vector3 mid = Vector3.Lerp(p1, p2, 0.5f);

                //before we build window, check if this room will need a door
                if (doorOptions.Count == 0)
                {
                    //build door if possible and correct layour
                    Vector3 doorPos = new Vector3();
                    bool doorBuilt = false;
                    if (rooms == 3 && floors == 1 && quads[i].name == "Kitchen" || rooms == 4 && floors == 1 && quads[i].name == "LivingRoom")
                    {
                        doorPos = p2 + (p1 - p2).normalized * 2f;

                        GameObject door = HouseBuilder.DoorAtPosition(doorPos, lookDira, quads[i], storeyHeight, doorHeight, doorWidth,divide);
                        door.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                        doors.Add(door);

                        wallsReturned.Add(door);

                        //check what length we need for window - can overlap door
                        float distance = (Vector3.Distance(doorPos, p1) * 0.5f);
                        //clamp? --if we want cool long windows, dont clamp
                        distance = Mathf.Clamp(distance, distance, windowWidth);
                        widthOfKitchenWindow = distance;

                        doorBuilt = true;
                    }

                    Vector3 windowPos = Vector3.Lerp(p1, p2,0.5f);

                    if (doorBuilt)
                    {
                        //place half way between dor and wall with a bump for door width
                        Vector3 awayFromDoor = (p1 - p2).normalized * (doorWidth * 0.5f) * 0.5f;
                        windowPos = Vector3.Lerp(doorPos, p1, 0.5f) + awayFromDoor;
                    }

                    GameObject window = HouseBuilder.WindowAtPosition(windowPos, doorHeight, windowHeight, widthOfKitchenWindow, lookDira, quads[i], storeyHeight, divide);
                    window.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Glass") as Material;

                    windows.Add(window);

                    wallsReturned.Add(window);
                }
                else
                {
                    //builds window on a wall with no cupboards
                    Vector3 windowPos = Vector3.Lerp(p1, p2, 0.5f);
                    GameObject window = HouseBuilder.WindowAtPosition(windowPos, doorHeight, windowHeight, windowWidth, lookDira, quads[i], storeyHeight, divide);

                    window.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Glass") as Material;
                    windows.Add(window);
                }
            }

            
            //now build doors for each exterior edge
            if (quads[i].name == "Kitchen" || quads[i].name == "LivingRoom")
            {
                
                //build door if possible and correct layour
                if (rooms == 3 && floors == 1 && quads[i].name == "Kitchen" || rooms == 4 && floors ==1 && quads[i].name == "LivingRoom")
                {
                    if (doorOptions.Count > 0  && doors.Count == 0)
                    {
                        //place door 
                        int random = Random.Range(0, doorOptions.Count);
                        GameObject door = HouseBuilder.DoorAtPosition(doorOptions[random].transform.position, -doorOptions[random].transform.forward, quads[i], storeyHeight, doorHeight, doorWidth,divide);
                        door.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                        doors.Add(door);

                        wallsReturned.Add(door);

                        List<GameObject> windowsToRemove = new List<GameObject>();
                        // Debug.Log("windows list count = " + windows.Count);
                        foreach(GameObject window in windows)
                        {
                            if(Vector3.Distance(door.transform.position,window.transform.position) < 2)
                            {
                                windowsToRemove.Add(window);
                               // Debug.Log("addin to remove list");
                            }
                        }
                        foreach(GameObject window in windowsToRemove)
                        {   
                            windows.Remove(window);
                            Destroy(window);
                            wallsReturned.Remove(window);
                        }
                    }
                    else if (doors.Count == 0)
                    {
                        
                        //we must place a door on an exterior wall with no units on it

                        Vector3[] oppositeEdge = FindOppositeEdge(quads[i], exteriorEdges);
                        Vector3 edgePoint1 = oppositeEdge[0];
                        Vector3 edgePoint2 = oppositeEdge[1];
                        //use this edge to build a door
                        //Vector3 doorPos = Vector3.Lerp(edgeToUse[0], edgeToUse[1], 0.5f);
                        //put door near the units/cupboards - leave a gap of 2 (doubelthe cupboard width
                        Vector3 doorPos = edgePoint2 + (edgePoint1 - edgePoint2).normalized * 2f;

                        //determine which way to buiid wall
                        Vector3 centre = quads[i].GetComponent<MeshRenderer>().bounds.center;

                        //get look direction just now
                        p1 = edgePoint1;
                        p2 = edgePoint2;
                        Vector3 midPoint = Vector3.Lerp(p1, p2, 0.5f);

                        //create points each side of the line
                        Vector3 lookDir1 = Quaternion.Euler(0, 90, 0) * (p1 - p2).normalized;
                        Vector3 lookDir2 = Quaternion.Euler(0, -90, 0) * (p1 - p2).normalized;
                        Vector3 lookDir = Vector3.zero;
                        //check which is closest - use that rotation to build door
                        if (Vector3.Distance(midPoint + lookDir1, centre) < Vector3.Distance(midPoint + lookDir2, centre))
                            lookDir = Quaternion.Euler(0, 90, 0) * (p2 - p1).normalized;     //feed local coords to static -- static always applies rotations from room
                        else
                            lookDir = Quaternion.Euler(0, -90, 0) * (p2 - p1).normalized;

                        GameObject door = HouseBuilder.DoorAtPosition(doorPos, lookDir, quads[i], storeyHeight, doorHeight, doorWidth,divide);
                        door.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                        doors.Add(door);

                        wallsReturned.Add(door);


                        //remove and windows that are close to the door
                        List<GameObject> windowsToRemove = new List<GameObject>();
                        //Debug.Log("windows list count = " + windows.Count);
                        foreach(GameObject window in windows)
                        {
                            if(Vector3.Distance(door.transform.position,window.transform.position) < 2)
                            {
                                windowsToRemove.Add(window);
                          //      Debug.Log("addin to remove list");
                            }
                        }
                        foreach(GameObject window in windowsToRemove)
                        {   
                            windows.Remove(window);
                            Destroy(window);
                            wallsReturned.Remove(window);
                        }
                    }
                }
                
                for (int j = 0; j < exteriorEdges.Count; j++)
                {
                    Vector3 v1 = exteriorEdges[j][0];
                    Vector3 v2 = exteriorEdges[j][1];

                    //check for door or window
                    List<GameObject> windowsInLine = new List<GameObject>();
                    List<Vector3> doorPositions = new List<Vector3>();//only ever one?

                    //check for any windows or doors on each edge
                    foreach (GameObject window in windows)
                    {
                        if (Divide.PointsInLine(exteriorEdges[j][0], exteriorEdges[j][1], window.transform.position))
                            windowsInLine.Add(window);

                    }
                    foreach (GameObject door in doors)
                    {
                        if (Divide.PointsInLine(exteriorEdges[j][0], exteriorEdges[j][1], door.transform.position))
                            doorPositions.Add(door.transform.position);
                    }

                    if (doorPositions.Count == 0 && windowsInLine.Count == 0)
                    {
                       // if (quads[i].name != "LivingRoom")//i know, it's getting mental
                        {
                            GameObject w = HouseBuilder.WallWithLookDirection(quads[i], v1, v2, storeyHeight, doorHeight, doorWidth, 1, false, divide);
                            wallsReturned.Add(w);                           
                            GameObject wall = HouseBuilder.WallWithLookDirection(quads[i], v1, v2, storeyHeight, doorHeight, doorWidth, 1, true, divide);
                            wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                            wallsReturned.Add(wall);
                        }
                    }

                    if (doorPositions.Count == 0 && windowsInLine.Count == 1)
                    {  
                        //inside
                        List<GameObject> walls1 = HouseBuilder.WallAroundWindowWithOffset(windowsInLine[0].transform.position, v1, v2, Vector3.Distance(v1, v2), windowsInLine[0].transform.forward, false, quads[i], storeyHeight,doorHeight, windowHeight, widthOfKitchenWindow, divide,floor);
                        foreach (GameObject wall in walls1)
                            wallsReturned.Add(wall);

                        //outside
                        List<GameObject> walls2 = HouseBuilder.WallAroundWindowWithOffset(windowsInLine[0].transform.position, v1, v2, Vector3.Distance(v1, v2), windowsInLine[0].transform.forward, true, quads[i], storeyHeight,doorHeight, windowHeight, widthOfKitchenWindow, divide,floor);
                        foreach (GameObject wall in walls2)
                        {
                            wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                            wallsReturned.Add(wall);
                        }

                    }
                    if (doorPositions.Count == 1 && windowsInLine.Count == 0)
                    {

                        List<List<GameObject>> doorThenWalls = HouseBuilder.DoorWithWall(quads[i], v1, v2, doors[0].transform.position, true, false, storeyHeight, doorHeight, doorWidth, divide);
                        //door
                        wallsReturned.Add(doorThenWalls[0][0]);
                        //walls
                        foreach (GameObject w in doorThenWalls[0])
                            wallsReturned.Add(w);

                        //exterior
                        List<GameObject> walls = HouseBuilder.WallAroundDoorWithOffset(doors[0].transform.position, v1, v2, Vector3.Distance(v1, v2), doors[0].transform.forward, true, quads[i], storeyHeight, doorHeight, doorWidth);
                        foreach (GameObject wall in walls)
                        {
                            wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                            wallsReturned.Add(wall);
                        }

                      
                    }

                    if (doorPositions.Count == 1 && windowsInLine.Count == 1)
                    {
                        

                        Vector3 dirToWindowFromDoor = (windowsInLine[0].transform.position - doorPositions[0]).normalized;
                        //ensure the side wall of the door doesnt overlap window
                        Vector3 midPoint = doorPositions[0] + dirToWindowFromDoor * doorWidth * 0.75f;
                        //find which point the door is closer to
                        Vector3 doorEnd = v1;
                        if (Vector3.Distance(v1, doorPositions[0]) > Vector3.Distance(v2, doorPositions[0]))
                            doorEnd = v2;
                        //door wall

                        //if we have more than one door on the exterior (or window) this may need revised - doors list and door positions won't match******

                        List<List<GameObject>> doorThenWalls = HouseBuilder.DoorWithWall(quads[i], midPoint, doorEnd, doors[0].transform.position, true, false, storeyHeight, doorHeight, doorWidth, divide);
                        wallsReturned.Add(doorThenWalls[0][0]);
                        //walls
                        foreach (GameObject w in doorThenWalls[0])
                            wallsReturned.Add(w);

                        //exterior
                        doorThenWalls = HouseBuilder.DoorWithWall(quads[i], midPoint, doorEnd, doors[0].transform.position, true, true, storeyHeight, doorHeight, doorWidth, divide);
                        //door
                        wallsReturned.Add(doorThenWalls[0][0]);
                        //walls
                        foreach (GameObject wall in doorThenWalls[0])
                        {
                            wallsReturned.Add(wall);
                            wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                        }
                        
                        //window wall
                        Vector3 windowEnd = v1;
                        if (doorEnd == v1)
                            windowEnd = v2;
                        //inside
                        List<GameObject> walls1 = HouseBuilder.WallAroundWindowWithOffset(windowsInLine[0].transform.position, windowEnd, midPoint, Vector3.Distance(windowEnd, midPoint), windows[0].transform.forward, false, quads[i], storeyHeight,doorHeight, windowHeight, widthOfKitchenWindow, divide,floor);
                        foreach (GameObject wall in walls1)
                            wallsReturned.Add(wall);
                        //outside
                        List<GameObject> walls2 = HouseBuilder.WallAroundWindowWithOffset(windowsInLine[0].transform.position, windowEnd, midPoint, Vector3.Distance(windowEnd, midPoint), windows[0].transform.forward, true, quads[i], storeyHeight,doorHeight, windowHeight, widthOfKitchenWindow, divide,floor);
                        foreach (GameObject wall in walls2)
                        {
                            wallsReturned.Add(wall);
                            wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                        }
                    }
                }
            }
        }

        //also need the "seatbelt" around all walls, we need this because when we build the scond storey, it gets moved up by a little, leaving a gap
        if (floors > 1)
        {
            for (int i = 0; i < plotLoop.Count - 1; i++)
            {
                Vector3 mid = Vector3.Lerp(plotLoop[i], plotLoop[i + 1], 0.5f);
                float length = Vector3.Distance(plotLoop[i], plotLoop[i + 1]);
                //add a brick size to each side
                //length += 0.2f;


                Mesh mesh = new Mesh();

                Vector3 p0 = new Vector3(length * 0.5f, 0.1f, 0.1f);
                Vector3 p1 = new Vector3(length * 0.5f, -0.1f, 0.1f);
                Vector3 p2 = new Vector3(-length * 0.5f, 0.1f, 0.1f);
                Vector3 p3 = new Vector3(-length * 0.5f, -0.1f, 0.1f);
                Vector3[] points = new Vector3[] { p0, p1, p2, p3 };
                GameObject belt = Divide.Quad(gameObject, points);
                belt.transform.position = mid;
                belt.transform.LookAt(plot.GetComponent<MeshRenderer>().bounds.center);
                belt.transform.position -= belt.transform.forward * 0.2f;//0.1f is default brick size
                belt.transform.position += Vector3.up * (storeyHeight + 0.1f);

                belt.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                belt.GetComponent<MeshFilter>().mesh.RecalculateNormals();

                wallsReturned.Add(belt);
            }
        }

        return wallsReturned;
    }

    public static List<GameObject> PrepareHalls(GameObject gameObject, List<GameObject> halls,float hallWidth,float storeyHeight,int floor)
    {
        List<GameObject> tempHalls = new List<GameObject>();

        //split all halls which arent in square shapes in to square shapes

        //first we need the landing
        GameObject landing = null;
        for (int i = 0; i < halls.Count; i++)
        {
            if (halls[i].name == "Landing")
                landing = halls[i];

            if (halls[i].name == "LandingClose")
            {
                landing = halls[i];
                //Debug.Break();
            }

        }
        
        Vector3 landingCentre = landing.GetComponent<MeshRenderer>().bounds.center;
        //now we need the quads that aren't square - we named the square ones "hall" when building them, filter these out
        for (int i = 0; i < halls.Count; i++)
        {
            if (halls[i].name == "Quad" || halls[i].name == "LandingClose")
            {
                //get longest edge
                Vector3[] vertices = halls[i].GetComponent<MeshFilter>().mesh.vertices;
                int[] longestEdge = Divide.LongestEdge(vertices);
                int[] shortestEdge = Divide.ShortestEdge(vertices);

                //a square can slip through, double check we need to split it up
                if(Vector3.Distance(vertices[shortestEdge[0]],vertices[shortestEdge[1]]) == Vector3.Distance(vertices[longestEdge[0]],vertices[longestEdge[1]]))
                {
                    Debug.Log("Skip");
                    tempHalls.Add(halls[i]);
                    continue;
                }

                //find which point is furthest away from landing
                float distance0 = Vector3.Distance(vertices[longestEdge[0]], landingCentre);
                float distance1 = Vector3.Distance(vertices[longestEdge[1]], landingCentre);

                

                Vector3 closestToLanding = vertices[longestEdge[0]];
                Vector3 furthestFromLanding = vertices[longestEdge[1]];
                if (halls[i].name == "Quad")
                {
                    if (distance1 < distance0)
                    {
                        closestToLanding = vertices[longestEdge[1]];
                        furthestFromLanding = vertices[longestEdge[0]];
                    }
                }
                else if(halls[i].name == "LandingClose")
                {
                    
                    if (distance1 > distance0)
                    {
                        
                        closestToLanding = vertices[longestEdge[1]];
                        furthestFromLanding = vertices[longestEdge[0]];
                    }
                }


                //find which vertice is closest to "closest to landing" -  this will give us our first pair of points from which we can step up and create a path with

                float distance = Mathf.Infinity;
                int secondPoint = 0;
                for (int j = 0; j < vertices.Length; j++)
                {
                    if (vertices[j] == closestToLanding)
                        continue;

                    float temp = Vector3.Distance(vertices[j], closestToLanding);
                    if (temp < distance)
                    {
                        distance = temp;
                        secondPoint = j;
                    }
                }

                Vector3 directionFromLanding = (furthestFromLanding - closestToLanding).normalized;

                float length = Vector3.Distance(closestToLanding, furthestFromLanding);


                //Debug.Log("LENGTH = " + length);

                Vector3 last0 = Vector3.zero;
                Vector3 last1 = Vector3.zero;

                if (length < hallWidth)
                {
                    
                    tempHalls.Add(halls[i]);
                    //Debug.Break();
                    Debug.Log("LENGTH < Hall");
                    continue;
                    
                }

                for (float j = 0; j < length - hallWidth; j += hallWidth)
                {
                    Vector3 p0 = closestToLanding + directionFromLanding * j;
                    Vector3 p1 = vertices[secondPoint] + directionFromLanding * j;

                    Vector3 p2 = closestToLanding + directionFromLanding * (j + hallWidth);
                    Vector3 p3 = vertices[secondPoint] + directionFromLanding * (j + hallWidth);

                    Vector3[] points = new Vector3[4] { p0, p1, p2, p3 };
                    //check for duplicates
                    bool doop = false;
                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            if (x == y)
                                continue;

                            if(points[x] == points[y])
                                doop = true;
                        }
                    }

                    if (doop)
                        continue;

                    //save for end point 
                    last0 = p2;
                    last1 = p3;
                    
                    /*
                    GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = p0;
                    c.name = "p0";
                    c.transform.parent = gameObject.transform;

                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = p1;
                    c.name = "p1";
                    c.transform.parent = gameObject.transform;


                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = p2;
                    c.name = "p2";
                    c.transform.parent = gameObject.transform;

                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = p3;
                    c.name = "p3";
                    c.transform.parent = gameObject.transform;
                    */
                    GameObject segment = Divide.Quad(gameObject, points);
                    segment.transform.position += Vector3.up * storeyHeight * floor;
                    segment.name = "Hall1";
                    segment.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;
                   
                    //parent.transform.parent = segment.transform;

                    //we have replaced it
                    Destroy(halls[i]);

                    tempHalls.Add(segment);
                }

                //now do last non uniform segment
                Vector3 last2 = closestToLanding + directionFromLanding * length;
                Vector3 last3 = vertices[secondPoint] + directionFromLanding * length;
                //some times don't need this extra segment because the loop finished it off perfectly
                if (last0 == last2 || last1 == last3 || last0 == Vector3.zero || last1 == Vector3.zero)
                {
                    //Debug.Log("DID IT?");
                    //tempHalls.Add(halls[i]);
                }
                else
                {
                    Vector3[] lastPoints = new Vector3[4] { last0, last1, last2, last3 };
                    GameObject lastSegment = Divide.Quad(gameObject, lastPoints);
                    lastSegment.transform.position += Vector3.up * storeyHeight * floor;
                    lastSegment.name = "Hall2";
                    lastSegment.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;
                    tempHalls.Add(lastSegment);
                }
            }
            else
            {
                //already squar, dont need to work it
                if (halls[i].name == "Landing" || halls[i].name == "Hall")
                {
                    
                    tempHalls.Add(halls[i]);//moved up - "did it?" 4413
                }
            }

        }

        foreach(GameObject go in tempHalls)
        {
           // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
           // c.transform.position = go.GetComponent<MeshRenderer>().bounds.center;
           // c.name = "temp hall";
           /// c.transform.parent = gameObject.transform;
           // c.transform.localScale *= .5f;
        }

        return tempHalls;

    }

    public static void DoorsByRoomSize(out List<GameObject> doorsOut, List<GameObject> quads,List<GameObject> halls,GameObject stairCollider,float storeyHeight,float doorHeight,float doorWidth,Divide divide)
    {

        //rooms are ordered by small to large in roomsAndsizes list

        //list of built doors
        List<Divide.WallWithDoor> wallsWithDoors = new List<Divide.WallWithDoor>();
        //doors
        List<GameObject> doorsBuilt = new List<GameObject>();
        //room info
        List<Divide.RoomAndEdge> roomsAndEdges = new List<Divide.RoomAndEdge>();

        //for each room, work out and store how many shared points it has with each other room
        List<List<Divide.TargetAndSharedPoints>> listOfRoomsAndSharedPoints = new List<List<Divide.TargetAndSharedPoints>>();
        for (int i = 0; i < quads.Count; i++)
        {
            List<Divide.TargetAndSharedPoints> targetAndSharedPoints = new List<Divide.TargetAndSharedPoints>();

            for (int j = 0; j < halls.Count; j++)
            {
                GameObject thisRoom = quads[i];
                GameObject targetRoom = halls[j];
                List<Vector3> sharedPoints = HouseBuilder.SharedPointsWithTargetRoom(thisRoom, targetRoom);

                Divide.TargetAndSharedPoints tasp = new Divide.TargetAndSharedPoints();
                tasp.room = quads[i];
                tasp.target = halls[j];
                tasp.sharedPoints = sharedPoints;

                targetAndSharedPoints.Add(tasp);
                //Debug.Log(thisRoom.name  + " Shared Points = " + sharedPoints.Count + ". Target room = " + targetRoom.name);
            }
            listOfRoomsAndSharedPoints.Add(targetAndSharedPoints);
        }


        for (int i = 0; i < quads.Count;  i++)
        {
            //smallest room is bathroom
            GameObject thisRoom = quads[i];

           // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
           /// c.transform.position = quads[i].GetComponent<MeshRenderer>().bounds.center;
           // c.transform.parent = quads[i].transform;
           // c.name = i.ToString() + " I";
            //rooms to check //start with largest and work down the way - aim here is to create natural flow through house
            for (int r = 0; r < halls.Count; r++)
            {


                if (quads[i] == halls[r])
                {
                    continue;
                }

                //c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //c.transform.position = halls[r].GetComponent<MeshRenderer>().bounds.center;
                ///c.transform.parent = halls[r].transform;
                //c.name = r.ToString() + " R";


                GameObject targetRoom = halls[r];

                //find shared Points - vertices that match the same positions as other room's vertices
                //List<Vector3> sharedPoints = SharedPointsWithTargetRoom(thisRoom, targetRoom); ---old way, building lists before we enter this loop now
                List<Vector3> sharedPoints = listOfRoomsAndSharedPoints[i][r].sharedPoints;

                //check for symmetrical layout - quite rare, but when it happens, a room can try and build diagonnally across the building in to the largest room - if caught, just move target on to next room
                //if we share 1 point with the target room and two points with any other two rooms, it means we are diagonally across from out target room. We can't put a door here
                int targetRoomSharedPoints = sharedPoints.Count;
                //Debug.Log(thisRoom.name + " Target = " + targetRoom.name + " Shared Points Count = " + targetRoomSharedPoints);
                int countOfOtherRoomsWith2SharedPoints = 0;
                for (int a = 0; a < listOfRoomsAndSharedPoints.Count; a++)
                {
                    if (listOfRoomsAndSharedPoints[a][r].sharedPoints.Count == 2)
                    {
                        countOfOtherRoomsWith2SharedPoints++;
                        //Debug.Log(thisRoom.name + " Target = " + targetRoom.name + " Shared Points Count = " + targetRoomSharedPoints);
                    }
                }

                //if we have 2 shared points, we have a simple wall with door to create in to target room
                if (sharedPoints.Count == 2)
                {
                    float distance = Vector3.Distance(sharedPoints[0], sharedPoints[1]);
                    //create list fof door point options along available wall
                    List<Vector3> doorOptions = new List<Vector3>();
                    Vector3 dir = (sharedPoints[1] - sharedPoints[0]).normalized;
                    //leaving a gap of 1 at each side // room size shouldnt push this too much - system isn't great but i tihnk it catches all problems
                    float gap = 1f;//door width here? // clamping on room creation too
                    for (float d = gap; d <= distance - gap; d += 0.1f)
                    {
                        Vector3 p = sharedPoints[0] + (dir * d);
                        if (!stairCollider.GetComponent<BoxCollider>().bounds.Contains(p))
                        {
                            //GameObject ex = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            //ex.transform.position = p;
                            //ex.transform.parent = roomsAndSizes[i].room.transform;
                            //ex.transform.localScale *= 0.5f;
                            doorOptions.Add(p);
                        }

                    }

                    //Debug.Log(doorOptions.Count + "door options " + "thisRoom = " + thisRoom.name + ", target room = " + targetRoom.name);
                    //randomly choose from this list

                    Vector3 doorPoint = Vector3.Lerp(sharedPoints[0], sharedPoints[1], 0.5f);

                    if (doorOptions.Count != 0)
                        doorPoint = doorOptions[Random.Range(0, doorOptions.Count)];
                    else
                    {
                        Debug.Log("door options 0");
                       // Debug.Break();
                        continue;
                    }
                    //build door using house builder class
                    List<List<GameObject>> doorThenWalls = HouseBuilder.DoorWithWall(thisRoom, sharedPoints[0], sharedPoints[1], doorPoint, false, false,storeyHeight,doorHeight,doorWidth, divide);//dont miss door
                    List<GameObject> doors = doorThenWalls[0];
                    doors[0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;

                    //add to a list, we will use the door positions when planning room interiors
                    List<GameObject> doorList = doorThenWalls[0];
                    doorsBuilt.Add(doorList[0]);

                    //remember which edge we have already built
                    Divide.RoomAndEdge rae = new Divide.RoomAndEdge()
                    {
                        room = thisRoom,
                        edge = new Vector3[2] { sharedPoints[0], sharedPoints[1] }
                    };

                    roomsAndEdges.Add(rae);

                    //lremember where we ahve built a wall
                    Divide.WallWithDoor wwd = new Divide.WallWithDoor()
                    {
                        wallPoint1 = sharedPoints[0],
                        wallPoint2 = sharedPoints[1],
                        doorPoint = doorPoint,
                        parent = thisRoom,
                        target = targetRoom
                    };


                    wallsWithDoors.Add(wwd);
                    //force skip, we have found our door
                    r = 1000;


                }
                //else if we only have shared point with  
                else if (sharedPoints.Count == 1)
                {

                    //Debug.Log(thisRoom.name + " shared points 2");
                    //we need to discover the vertice of the other room, this lets us know where to stop the wall
                    //find closest point to the shared point we are trying to attach to(target Room)    

                    Vector3 closestPointFromThisRoom = Divide.ClosestVerticeOnThisRoomToCentreOfTargetRoom(quads[i], targetRoom, sharedPoints[0]);//normal
                    Vector3 centreOfTarget = targetRoom.GetComponent<MeshRenderer>().bounds.center;
                    Vector3 closestPointFromTargetRoom = Divide.ClosestVerticeOnThisRoomToCentreOfTargetRoom(targetRoom, quads[i], sharedPoints[0]);

                    //run through target room vertices and find which was in a straight line with shared point [0] and the closest point from this room

                    Vector3[] othervertices = targetRoom.GetComponent<MeshFilter>().mesh.vertices;

                    Vector3 p1 = Vector3.zero;
                    Vector3 p2 = Vector3.zero;
                    for (int v = 0; v < othervertices.Length; v++)
                    {
                        if (othervertices[v] == sharedPoints[0])
                            continue;

                        if (Divide.PointsInLine(sharedPoints[0], closestPointFromThisRoom, othervertices[v]))
                        {
                            //Debug.Log("Using 1st option" + thisRoom.name);
                            p1 = closestPointFromThisRoom;
                            p2 = othervertices[v];
                        }

                        if (Divide.PointsInLine(sharedPoints[0], othervertices[v], closestPointFromThisRoom))
                        {
                            //  Debug.Log("Using 2nd option" + thisRoom.name);
                            p1 = othervertices[v];
                            p2 = closestPointFromThisRoom;
                        }
                    }
                    bool buildExtraWall = false; //not using, this may be a soluition instad of raycasting in MissingWalls, not investigating atm
                    if (p1 == Vector3.zero)
                    {
                        buildExtraWall = true;
                        //Debug.Break();
                        Vector3[] thisvertices = thisRoom.GetComponent<MeshFilter>().mesh.vertices;
                        //we didnt find a suitable wall, using the target room's vertices, let's try this room's instead - note we are using the closest point form target room now
                        Debug.Log("points in line first count was zero, switchin to other room, this room is " + thisRoom.name + " Is living room at the end? no wal between kitchen?");
                        for (int v = 0; v < thisvertices.Length; v++)
                        {
                            if (thisvertices[v] == sharedPoints[0])
                            {

                                Debug.Log("Continued form here");
                                continue;
                            }

                            if (Divide.PointsInLine(sharedPoints[0], closestPointFromTargetRoom, thisvertices[v]))
                            {
                                p1 = closestPointFromTargetRoom;
                                p2 = othervertices[v];
                            }

                            if (Divide.PointsInLine(sharedPoints[0], thisvertices[v], closestPointFromTargetRoom))
                            {
                                p1 = thisvertices[v];
                                p2 = closestPointFromTargetRoom;
                            }
                        }
                    }
                    if (p1 == Vector3.zero)
                    {
                        Debug.Log("still zero????");
                        continue;
                        
                    }
                        

                    Vector3 closestInLine = p1;
                    Vector3 furthestInLine = p2;
                    if (Vector3.Distance(sharedPoints[0], p1) > Vector3.Distance(sharedPoints[0], p2))
                    {
                        closestInLine = p2;
                        furthestInLine = p1;
                    }
                    //////////
                    Vector3 target = closestInLine;

                    if (target == sharedPoints[0])
                    {
                        Debug.Log("Break on " + thisRoom.name);
                       // Debug.Break();

                    }

                    //Vector3 endPointForDoor = closestInLine;//testing
                    Vector3 endPointForDoor = closestInLine;

                    //debug

                    #region cubes

                    /*
                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = sharedPoints[0];
                    c.name = "shared points [0] " + targetRoom.name;
                    c.transform.parent = thisRoom.transform;

                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = closestPointFromTargetRoom;
                    c.name = "closestPointOnTargetRoom";
                    c.transform.parent = thisRoom.transform;

                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = closestPointFromThisRoom;
                    c.name = "closestPointOnThisRoom";
                    c.transform.parent = thisRoom.transform;

                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = target;
                    c.name = "target";
                    c.transform.parent = thisRoom.transform;

                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = endPointForDoor;
                    c.name = "endPointForDoor";
                    c.transform.parent = thisRoom.transform;

                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = closestInLine;
                    c.name = "closest in line";
                    c.transform.parent = thisRoom.transform;
                    
                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = furthestInLine;
                    c.name = "furthest in line";
                    c.transform.parent = thisRoom.transform;
                    */

                    #endregion

                    if (buildExtraWall)
                    {
                        //GameObject ex = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        //ex.transform.position = Vector3.Lerp(furthestInLine, closestInLine, 0.5f);
                        //ex.transform.parent = roomsAndSizes[i].room.transform;

                    }

                    //check distance, //door must be positioned so at least one side has minimum object width - using 1f atm         
                    float distance = Vector3.Distance(sharedPoints[0], endPointForDoor);

                    //create list fof door point options along available wall
                    List<Vector3> doorOptions = new List<Vector3>();
                    Vector3 dir = (endPointForDoor - sharedPoints[0]).normalized;
                    //leaving a gap of 1 at each side // room size shouldnt push this too much - system isn't great but i tihnk it catches all problems
                    float gap = 0.5f;//door width here? // clamping on room creation too
                    for (float d = gap; d <= distance - gap; d += 0.1f)
                    {
                        Vector3 p = sharedPoints[0] + (dir * d);
                        // GameObject ex = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        //ex.transform.position = p;
                        //ex.transform.parent = roomsAndSizes[i].room.transform;
                        //ex.transform.localScale *= 0.5f;

                        //add collider stair check? onlyneed if shared points = 2 like above?

                        if (!stairCollider.GetComponent<BoxCollider>().bounds.Contains(p))
                        {
                            doorOptions.Add(p);
                        }

                    }

                    //Debug.Log(doorOptions.Count + "door options " + "thisRoom = " + thisRoom.name + ", target room = " + targetRoom.name);
                    //randomly choose from this list

                    Vector3 doorPoint = Vector3.Lerp(sharedPoints[0], endPointForDoor, 0.5f);

                    if (doorOptions.Count != 0)
                        doorPoint = doorOptions[Random.Range(0, doorOptions.Count)];
                    else
                    {
                       // Debug.Break(); Debug.Log("NO DOOR POINTS, shared points = 1");
                        continue;
                    }




                    //build the wall
                    List<List<GameObject>> doorThenWalls = Divide.DoorWithWall(thisRoom, sharedPoints[0], target, doorPoint, false, false,storeyHeight,doorHeight,doorWidth, divide);//never skip door, need for room items - must delete later - save these to list?
                    List<GameObject> doors = doorThenWalls[0];
                    doors[0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                    //add for interiors
                    List<GameObject> doorList = doorThenWalls[0];
                    doorsBuilt.Add(doorList[0]);
                    //remember edge



                    //determine which way to buiid wall
                    Vector3 centre = quads[i].GetComponent<MeshRenderer>().bounds.center;
                    //create world positions


                    //edgesBuilt.Add(new Vector3[2] { closestPointToTarget, sharedPoints[0] });
                    Divide.RoomAndEdge rae = new Divide.RoomAndEdge();
                    rae.room = thisRoom;
                    rae.edge = new Vector3[2] { sharedPoints[0], target };
                    roomsAndEdges.Add(rae);

                    rae = new Divide.RoomAndEdge();
                    rae.room = targetRoom;
                    rae.edge = new Vector3[2] { sharedPoints[0], target };
                    roomsAndEdges.Add(rae);

                    //let the target room know we have placed a door here. It needs to know so it can also leave a gap for the door
                    Divide.WallWithDoor wwd = new Divide.WallWithDoor();
                    wwd.wallPoint1 = sharedPoints[0];
                    wwd.wallPoint2 = target;
                    wwd.doorPoint = doorPoint;
                    wwd.parent = thisRoom;
                    wwd.target = targetRoom;

                    wallsWithDoors.Add(wwd);

                    //force skip, we have found our door
                    r = 1000;
                }
                //else if we have no shared points with target room, try next room(smaller)
                else if (sharedPoints.Count == 0)
                {
                    Debug.Log("MISSING WALL");
                    continue;
                }
            }
        }

        //build walls around the doors we just placed

        //split rooms in to lists defined by how many doors are in them
        List<Divide.WallWithDoor> oneDoorRooms = new List<Divide.WallWithDoor>();
        List<Divide.WallWithDoor> twoDoorRooms = new List<Divide.WallWithDoor>();
        List<Divide.WallWithDoor> threeDoorRooms = new List<Divide.WallWithDoor>();
        for (int i = 0; i < quads.Count; i++)
        {
            List<Divide.WallWithDoor> tempList = new List<Divide.WallWithDoor>();
            //walls with doors list checks for any door points and build a wall around it
            for (int j = 0; j < wallsWithDoors.Count; j++)
            {
                GameObject thisRoom = quads[i];
                if (thisRoom == wallsWithDoors[j].parent)
                {
                    tempList.Add(wallsWithDoors[j]);
                }
            }

            if (tempList.Count == 1)
            {
                oneDoorRooms.Add(tempList[0]);
            }
            if (tempList.Count == 2)
            {
                twoDoorRooms.Add(tempList[0]);
                twoDoorRooms.Add(tempList[1]);
            }
            if (tempList.Count == 3)
            {
                threeDoorRooms.Add(tempList[0]);
                threeDoorRooms.Add(tempList[1]);
                threeDoorRooms.Add(tempList[2]);
            }

        }
        
        //Debug.Log("One door rooms = " + oneDoorRooms.Count);
        //Debug.Log("Two door rooms = " + oneDoorRooms.Count);
        //Debug.Log("Three door rooms = " + threeDoorRooms.Count);

        for (int j = 0; j < oneDoorRooms.Count; j++)
        {

            Vector3 closest = Divide.ClosestPointOnMesh(oneDoorRooms[j].wallPoint2, oneDoorRooms[j].target);
            #region cubes

            /*
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = oneDoorRooms[j].wallPoint1;
            c.name = " wp 1 ";
            c.transform.parent = oneDoorRooms[j].target.transform;

            c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = oneDoorRooms[j].wallPoint2;
            c.name = " wp 2 ";
            c.transform.parent = oneDoorRooms[j].target.transform;

            c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = closest;
            c.name = " Closest ";
            c.transform.parent = oneDoorRooms[j].target.transform;
            */
            #endregion

            //now find vertice closest to end of wall


            //returns a door, then a list of walls -- not entirely happy with this solution for returning objects. Because i want to keep rotation calculations in HouseBuilder class (even though some calcs happen here anyway..)
            List<List<GameObject>> doorThenWalls = Divide.DoorWithWall(oneDoorRooms[j].target, oneDoorRooms[j].wallPoint1, oneDoorRooms[j].wallPoint2, oneDoorRooms[j].doorPoint, false, false,storeyHeight,doorHeight,doorWidth, divide);
            List<GameObject> doors = doorThenWalls[0];
            doors[0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
            doorsBuilt.Add(doors[0]);

            //build missing part of wall if any
            if (closest != oneDoorRooms[j].wallPoint1 && closest != oneDoorRooms[j].wallPoint2)
            {

                //make sure in line -USING WALLPOINT 2" all the time, possibly would need to check which is closer to "closest" between wallpoint1 and 2
                if (Divide.PointsInLine(oneDoorRooms[j].wallPoint1, closest, oneDoorRooms[j].wallPoint2))
                {
                    //find adjacent room, this room has two shared verts
                    for (int k = 0; k < quads.Count; k++)
                    {
                        List<Vector3> sharedPoints = Divide.SharedPointsWithTargetRoom(oneDoorRooms[j].parent, quads[k]);
                        if (sharedPoints.Count == 2)
                        {
                            bool door = false;
                            //check no doors are here first
                            for (int m = 0; m < quads[k].transform.childCount; m++)
                            {
                                GameObject d = quads[k].transform.GetChild(m).gameObject;
                                if (d.name == "Door")
                                {
                                    if (Divide.PointsInLine(closest, oneDoorRooms[j].wallPoint2, d.transform.position))
                                    {
                                        door = true;
                                        // Debug.Log("A DOOR WAS FOUND!");

                                    }

                                }
                            }


                            if (!door)
                            {
                                //add this to a list of internal walls to build -  we need ot wait until all doors have definitely been built, in case we cover one up using this. This will be check ed when building
                                
                                //determine which way to buiid wall
                                Vector3 centre = quads[k].GetComponent<MeshRenderer>().bounds.center;
                                //create world positions
                                Vector3 p1 = oneDoorRooms[j].wallPoint2;
                                Vector3 p2 = closest;
                                Vector3 midPoint = Vector3.Lerp(p1, p2, 0.5f);

                                //create points each side of the line
                                Vector3 lookDir1 = Quaternion.Euler(0, 90, 0) * (p1 - p2).normalized;
                                Vector3 lookDir2 = Quaternion.Euler(0, -90, 0) * (p1 - p2).normalized;
                                Vector3 lookDir = Vector3.zero;
                                //check which is closest - use that rotation to build door
                                if (Vector3.Distance(midPoint + lookDir1, centre) < Vector3.Distance(midPoint + lookDir2, centre))
                                    lookDir = Quaternion.Euler(0, 90, 0) * (closest - oneDoorRooms[j].wallPoint2).normalized;     //feed local coords to static -- static always applies rotations from room
                                else
                                    lookDir = Quaternion.Euler(0, -90, 0) * (closest - oneDoorRooms[j].wallPoint2).normalized;    //feed local coords to static -- static always applies rotations from room



                                //BUILDING OVER DOOR OF OTHER ROOM?
                                GameObject w1 = Divide.Wall(midPoint, Vector3.Distance(closest, oneDoorRooms[j].wallPoint2), lookDir, false, quads[k],storeyHeight);
                                w1.name = quads[k].ToString();
                                w1.transform.position += storeyHeight * Vector3.up;

                                GameObject w2 = Divide.Wall(midPoint, Vector3.Distance(closest, oneDoorRooms[j].wallPoint2), -lookDir, false, oneDoorRooms[j].target, storeyHeight);
                                w2.name = oneDoorRooms[j].target.ToString() + " This Guy?";
                                w2.transform.position += storeyHeight * Vector3.up;
                                //remember this wall and door
                                //let the target room know we have placed a door here. It needs to know so it can also leave a gap for the door
                                Divide.WallWithDoor wwd = new Divide.WallWithDoor();
                                wwd.wallPoint1 = closest;
                                wwd.wallPoint2 = oneDoorRooms[j].wallPoint2;
                                wwd.doorPoint = Vector3.zero;
                                wwd.parent = quads[k];
                                wwd.target = oneDoorRooms[j].target;

                                wallsWithDoors.Add(wwd);

                                Divide.RoomAndEdge rae = new Divide.RoomAndEdge();
                                rae.room = oneDoorRooms[j].target;
                                rae.edge = new Vector3[2] { closest, oneDoorRooms[j].wallPoint2 };
                                roomsAndEdges.Add(rae);

                                rae = new Divide.RoomAndEdge();
                                rae.room = quads[k];
                                rae.edge = new Vector3[2] { closest, oneDoorRooms[j].wallPoint2 };
                                roomsAndEdges.Add(rae);

                            }
                            //else if door, the wall and door will already have been built. Doors are placed and walla rebuilt on both rooms at the same time
                        }
                    }
                }
            }
            //remember edge
            //edgesBuilt.Add(new Vector3[2] { wallsWithDoors[j].wallPoint1, closest });


        }
        
        //for two door rooms we need to check if any two doors are on the same wall
        //if not we can build wall around door from room edge to room edge
        //check if any two edges are in line with each other
        //create list of edges for this room
        List<Divide.WallWithDoor> roomsWithTwoDoors = new List<Divide.WallWithDoor>();

        for (int a = 0; a < twoDoorRooms.Count; a++)
        {
            //Vector3[] edge = new Vector3[2] { twoDoorRooms[a].wallPoint1, twoDoorRooms[a].wallPoint2 };
            Divide.WallWithDoor ww2d = new Divide.WallWithDoor();
            ww2d.wallPoint1 = twoDoorRooms[a].wallPoint1;
            ww2d.wallPoint2 = twoDoorRooms[a].wallPoint2;
            ww2d.doorPoint = twoDoorRooms[a].doorPoint;
            ww2d.parent = twoDoorRooms[a].parent;
            ww2d.target = twoDoorRooms[a].target;
            roomsWithTwoDoors.Add(ww2d);



        }

        

        //   Debug.Log(edges.Count);
        //go through edges and check if any have the same direction or - direction
        //once we findedges with the same direction, build walls on each of these edges, this will segment the wall correctly
        List<Vector3> edgePointsUsed = new List<Vector3>();
        // Debug.Log(roomsWithTwoDoors.Count + "rooms with 2 doors count");

        //stopping duplicates being built, there is more than one way to find if a door needs built, so just catch it using this list
        // List<Vector3> builtHere = new List<Vector3>();
        for (int e = 0; e < roomsWithTwoDoors.Count; e++)
        {

            //more complicated here beacsue we need to make sure the walls stretch all the way to the end of this room's wall

            Vector3 thisDirection = roomsWithTwoDoors[e].wallPoint2 - roomsWithTwoDoors[e].wallPoint1;
            thisDirection.Normalize();

            //used to discover whole edge

            for (int n = 0; n < roomsWithTwoDoors.Count; n++)
            {
                if (e == n)
                    continue;

                //if (!builtHere.Contains(roomsWithTwoDoors[n].doorPoint))
                //  continue;

                Vector3 otherDirection = roomsWithTwoDoors[n].wallPoint2 - roomsWithTwoDoors[n].wallPoint1;
                otherDirection.Normalize();

                //any match of direction, sometimes edges can be spun round
                if (thisDirection == -otherDirection || -thisDirection == otherDirection)// || thisDirection == -otherDirection || -thisDirection == otherDirection)
                {
                    List<List<GameObject>> doorThenWalls = Divide.DoorWithWall(roomsWithTwoDoors[e].parent, roomsWithTwoDoors[e].wallPoint1, roomsWithTwoDoors[e].wallPoint2, roomsWithTwoDoors[e].doorPoint, false, false,storeyHeight,doorHeight,doorWidth, divide);
                    List<GameObject> doors = doorThenWalls[0];

                    doors[0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                    //always the first point on the outside
                    edgePointsUsed.Add(roomsWithTwoDoors[e].wallPoint1);
                    //edgePointsUsed.Add(wallsWithTwoDoors[e].wallPoint2);


                    //remember build this - avoiding duplicates
                    // builtHere.Add(roomsWithTwoDoors[e].doorPoint);
                    Divide.RoomAndEdge rae = new Divide.RoomAndEdge();
                    rae.room = roomsWithTwoDoors[e].parent;
                    rae.edge = new Vector3[2] { roomsWithTwoDoors[e].wallPoint1, roomsWithTwoDoors[e].wallPoint2 };
                    roomsAndEdges.Add(rae);

                }
                else
                {

                    //if we have a wall with ony one door??? where does this go?
                    //now find vertice closest to end of wall
                    Vector3 closest = Divide.ClosestPointOnMesh(roomsWithTwoDoors[e].wallPoint2, roomsWithTwoDoors[e].parent);

                    List<List<GameObject>> doorThenWalls = Divide.DoorWithWall(roomsWithTwoDoors[e].parent, roomsWithTwoDoors[e].wallPoint1, closest, roomsWithTwoDoors[e].doorPoint, false, false,storeyHeight,doorHeight,doorWidth, divide); //always build door, we need them for item placement
                    List<GameObject> doors = doorThenWalls[0];


                    doors[0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;

                    //remember the edge
                    Divide.RoomAndEdge rae = new Divide.RoomAndEdge();
                    rae.room = roomsWithTwoDoors[e].parent;
                    rae.edge = new Vector3[2] { roomsWithTwoDoors[e].wallPoint1, closest };
                    roomsAndEdges.Add(rae);

                    //remember build this - avoiding duplicates
                    //builtHere.Add(roomsWithTwoDoors[e].doorPoint);

                }

            }
            //create full edge out of split up edges so we can feed the missing walls function - it looks for full edges so it knows not to build there
            //built both inner segments?
            if (edgePointsUsed.Count == 2)
            {
                //remember full edge for missing areas - before we split it upbelow
                Divide.RoomAndEdge rae2 = new Divide.RoomAndEdge();
                rae2.room = wallsWithDoors[e].parent;
                rae2.edge = new Vector3[2] { edgePointsUsed[0], edgePointsUsed[1] };
                // roomsAndEdges.Add(rae2);

                edgePointsUsed = new List<Vector3>();
            }
        }

        List<Divide.WallWithDoor> roomsWithThreeDoors = new List<Divide.WallWithDoor>();

        for (int a = 0; a < threeDoorRooms.Count; a++)
        {
            //Vector3[] edge = new Vector3[2] { twoDoorRooms[a].wallPoint1, twoDoorRooms[a].wallPoint2 };
            Divide.WallWithDoor ww2d = new Divide.WallWithDoor();
            ww2d.wallPoint1 = threeDoorRooms[a].wallPoint1;
            ww2d.wallPoint2 = threeDoorRooms[a].wallPoint2;
            ww2d.doorPoint = threeDoorRooms[a].doorPoint;
            ww2d.parent = threeDoorRooms[a].parent;
            roomsWithThreeDoors.Add(ww2d);


        }

        for (int e = 0; e < roomsWithThreeDoors.Count; e++)
        {

            //simpler atm, i ahven't seen an example where we need to use the "two rooms loop" to find out which wall point to use, thisRoom, or targetRoom's

            List<List<GameObject>> doorThenWalls = Divide.DoorWithWall(roomsWithThreeDoors[e].parent, roomsWithThreeDoors[e].wallPoint1, roomsWithThreeDoors[e].wallPoint2, roomsWithThreeDoors[e].doorPoint, false, false,storeyHeight,doorHeight,doorWidth, divide);

        }

        //save doors for future reference
        doorsOut = doorsBuilt;
    }

    public static void MissingInternals(List<GameObject> quads,Mesh plotMesh,List<GameObject> doorsBuilt,float storeyHeight,int floor) //is lookDir muddled up, usually same vector then spun
    {
        List<Vector3[]> interiorEdges = new List<Vector3[]>();

        for (int i = 0; i < quads.Count; i++)
        {
            List<Vector3> wallPositions = new List<Vector3>();
            //now find any internal wall that hasnt been built and build a windowless wall
            //use edgeBuilt list which we populated with every edge we have built
            //deduce which edge(s) is missing

            //skipppin last room, all walls built - this function only uilds walls between rooms which have no door. All rooms atm going to last rom have alrady built dopors and walls
            //This could possibly change and a better solution will be needed


            //we need to avoid doors, get list of doors in this room
            List<GameObject> doors = new List<GameObject>();
            for (int d = 0; d < quads[i].transform.childCount; d++)
            {
                GameObject g = quads[i].transform.GetChild(d).gameObject;
                if (g.name == "Door")
                {
                    doors.Add(g);
                }
            }

            //  Debug.Log(quads[i].ToString() + doors.Count);


            //create a list of vertices for this room

            List<Vector3[]> allEdges = new List<Vector3[]>();

            Vector3[] vertices = quads[i].GetComponent<MeshFilter>().mesh.vertices;
            //add for floor height too
            for (int j = 0; j < vertices.Length - 1; j++)
            {
                Vector3[] e = new Vector3[2] { vertices[j] + Vector3.up*(storeyHeight*floor), vertices[j + 1] + Vector3.up * (storeyHeight * floor ) };
                allEdges.Add(e);
            }
            
            //close loop //plus floor
            Vector3[] lastEdge = new Vector3[2] { vertices[vertices.Length - 1] +Vector3.up * (storeyHeight * floor ), vertices[0] + Vector3.up * (storeyHeight * floor ) };
            allEdges.Add(lastEdge);

            //loop of plot mesh
            List<Vector3> plotLoop = new List<Vector3>();
            Vector3[] plotVertices = plotMesh.vertices;
            foreach (Vector3 v3 in plotVertices)
            {
                //add floor and storey height to plot 
                Vector3 temp = v3 + Vector3.up * (storeyHeight * floor);

                plotLoop.Add(temp);
            }

            plotLoop.Add(plotVertices[0] + Vector3.up * (storeyHeight * floor)) ;


            

            //noDoors = true;
            List<Vector3[]> exteriorEdges = new List<Vector3[]>();

            for (int j = 0; j < allEdges.Count; j++)
            {
                Vector3[] edge = allEdges[j];

                //is this an exterior wall?
                //is edge between two points
                for (int p = 0; p < plotLoop.Count - 1; p++)
                {
                    bool first = Divide.PointsInLine(plotLoop[p], plotLoop[p + 1], allEdges[j][0]);

                    bool second = Divide.PointsInLine(plotLoop[p], plotLoop[p + 1], allEdges[j][1]);

                    // bool first = false;
                    // bool second = false;

                    //if (allEdges[j][0].x == plotLoop[p].x || allEdges[j][0].z == plotLoop[p].z)
                    //    first = true;

                    //if (allEdges[j][1].x == plotLoop[p].x || allEdges[j][1].z == plotLoop[p].z)
                    //    second = true;

                    if (first && second)
                    {
                        exteriorEdges.Add(edge);
                    }
                }
            }

            //deduce interior edges by checking all edges against exterior

            for (int j = 0; j < allEdges.Count; j++)
            {
                bool exteriorEdge = false;
                //run through all exterior edges and check this edge doesnt match one of them
                for (int k = 0; k < exteriorEdges.Count; k++)
                {
                    if (allEdges[j][0] == exteriorEdges[k][0] && allEdges[j][1] == exteriorEdges[k][1] || allEdges[j][1] == exteriorEdges[k][0] && allEdges[j][0] == exteriorEdges[k][1])
                    {

                        exteriorEdge = true;
                      /*  
                        GameObject ex = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        ex.transform.position = exteriorEdges[k][0];
                        ex.name = "EXT 0";
                        ex.transform.parent = quads[i].transform;
                        ex = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        ex.transform.position = exteriorEdges[k][1];
                        ex.name = "EXT 1";
                        ex.transform.parent = quads[i].transform;
                        */

                    }
                }

                if (!exteriorEdge)
                {
                    /*
                    GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    s.transform.position = allEdges[j][0];
                    s.transform.parent = quads[i].transform;
                    s.name = "edge 0";
                    s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    s.transform.position = allEdges[j][1];
                    s.transform.parent = quads[i].transform;
                    s.name = "edge 1";
                    */
                    

                    //dont add duplicates, including if edge is reversed
                    bool duplicate = false;
                    for (int l = 0; l < interiorEdges.Count; l++)
                        if (interiorEdges[l][0] == allEdges[j][0] && interiorEdges[l][1] == allEdges[j][1] || interiorEdges[l][1] == allEdges[j][0] && interiorEdges[l][0] == allEdges[j][1])
                        {
                            //Debug.Log("duplicate " + roomsAndSizes[i].room.name);
                           // duplicate = true;
                        }
                    //override/
                    //duplicate = false;
                    if (!duplicate)
                    {

                        //check for a door
                        bool noDoors = true;
                        //check each edge for a door

                        for (int d = 0; d < doors.Count; d++)
                        {
                          
                            if (Divide.PointsInLine(allEdges[j][0], allEdges[j][1], doors[d].transform.position))
                            {
                                //do not build a wall here, walls already built around door by now
                                noDoors = false;
                               

                                //if there is a door, find the other room which this door goes to - when doors are built, they are added to a global list - find the matching door

                                //a.name = "Matching door going to " + door.transform.parent.name;
                                foreach (GameObject door in doorsBuilt)
                                {
                                    if (doors[d].transform.position == door.transform.position && door.transform.parent != doors[d].transform.parent)
                                    {

                                        //GameObject a = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                       //// a.transform.position = doors[d].transform.position;
                                        //a.transform.parent = quads[i].transform;
                                        //a.name = "DOORCUBE";
                                        GameObject targetRoom = door.transform.parent.transform.gameObject;

                                        //this room's vertice
                                        Vector3[] targetVertices = targetRoom.GetComponent<MeshFilter>().mesh.vertices;
                                        for (int x = 0; x < targetVertices.Length; x++)
                                        {
                                            targetVertices[x] += Vector3.up * (storeyHeight * floor);
                                        }

                                        for (int v = 0; v < targetVertices.Length; v++)
                                        {
                                            if (targetVertices[v] == allEdges[j][0] || targetVertices[v] == allEdges[j][1])
                                            {
                                                //if any matching vertices, skip
                                            }
                                            else
                                            {
                                                if (Divide.PointsInLine(allEdges[j][0], allEdges[j][1], targetVertices[v]))
                                                {
                                                    //find out which edge point to build to from V - the one which is further away from the door

                                                    //a = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                                    // a.transform.position = targetVertices[v];
                                                    // a.transform.parent = quads[i].transform;
                                                    // a.name = "V";

                                                    //Debug.Break();

                                                    float d0 = Vector3.Distance(allEdges[j][0], door.transform.position);
                                                    float d1 = Vector3.Distance(allEdges[j][1], door.transform.position);

                                                    Vector3 target = allEdges[j][0];
                                                    if (d0 < d1)
                                                    {
                                                        target = allEdges[j][1];
                                                    }

                                                    //a = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                                    //a.transform.position = target;
                                                    // a.transform.parent = roomsAndSizes[i].room.transform;
                                                    // a.name = "target";

                                                    bool buildWall = true;
                                                    //now double check for another door in this room
                                                    foreach (GameObject door2 in doors)
                                                    {
                                                        if (Divide.PointsInLine(targetVertices[v], target, door2.transform.position))
                                                        {
                                                            buildWall = false;
                                                            // a = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                                            //a.transform.position = door2.transform.position;
                                                            //a.transform.parent = roomsAndSizes[i].room.transform;
                                                            //a.name = "DOOR DONT BUILD WALL";
                                                        }
                                                    }

                                                    if (buildWall)
                                                    {
                                                        //determine which way to buiid wall
                                                        Vector3 centre = quads[i].GetComponent<MeshRenderer>().bounds.center;
                                                        //create world positions
                                                        Vector3 p1 = targetVertices[v];
                                                        Vector3 p2 = target;
                                                        Vector3 midPoint = Vector3.Lerp(p1, p2, 0.5f);

                                                        //create points each side of the line
                                                        Vector3 lookDir1 = Quaternion.Euler(0, 90, 0) * (p1 - p2).normalized;
                                                        Vector3 lookDir2 = Quaternion.Euler(0, -90, 0) * (p1 - p2).normalized;
                                                        Vector3 lookDir = Vector3.zero;
                                                        //check which is closest - use that rotation to build door
                                                        if (Vector3.Distance(midPoint + lookDir1, centre) < Vector3.Distance(midPoint + lookDir2, centre))
                                                            lookDir = Quaternion.Euler(0, 90, 0) * (p2 - p1).normalized;     //feed local coords to static -- static always applies rotations from room
                                                        else
                                                            lookDir = Quaternion.Euler(0, -90, 0) * (p2 - p1).normalized;    //feed local coords to static -- static always applies rotations from room

                                                        GameObject w1 = Divide.Wall(midPoint, Vector3.Distance(p1, p2), lookDir, false, quads[i],storeyHeight);
                                                        w1.name = quads[i].ToString() + " NEW WALL";

                                                      //  GameObject w2 = Divide.Wall(midPoint, Vector3.Distance(p1, p2), -lookDir, false, quads[i]);
                                                        //w2.name = quads[i].ToString() + " NEW WALL 2"; //parent?

                                                        //save this wall position, beneatth this we look for other missed walls but sometimes a duplicate can be built, because this solution hasn't been as perfect as I hoped
                                                        wallPositions.Add(midPoint);
                                                        //Debug.Break();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        

                        if (noDoors)
                        {
                            interiorEdges.Add(allEdges[j]);

                            //determine which way to buiid wall
                            Vector3 centre = quads[i].GetComponent<MeshRenderer>().bounds.center;
                            //create world positions
                            Vector3 p1 = allEdges[j][0];
                            Vector3 p2 = allEdges[j][1];
                            Vector3 midPoint = Vector3.Lerp(p1, p2, 0.5f);

                            //create points each side of the line
                            Vector3 lookDir1 = Quaternion.Euler(0, 90, 0) * (p1 - p2).normalized;
                            Vector3 lookDir2 = Quaternion.Euler(0, -90, 0) * (p1 - p2).normalized;
                            Vector3 lookDir = Vector3.zero;
                            //check which is closest - use that rotation to build door
                            if (Vector3.Distance(midPoint + lookDir1, centre) < Vector3.Distance(midPoint + lookDir2, centre))
                                lookDir = Quaternion.Euler(0, 90, 0) * (p2 - p1).normalized;     //feed local coords to static -- static always applies rotations from room
                            else
                                lookDir = Quaternion.Euler(0, -90, 0) * (p2 - p1).normalized;    //feed local coords to static -- static always applies rotations from room

                            //check for a wall already built
                            if (!wallPositions.Contains(midPoint))
                            {
                                GameObject w1 = Divide.Wall(midPoint, Vector3.Distance(p1, p2), lookDir, false, quads[i], storeyHeight);
                                //GameObject w2 = Divide.Wall(midPoint, Vector3.Distance(p1, p2), -lookDir, false, quads[i]);
                                w1.name = quads[i].ToString() + " w1";
                               // w1.name = quads[i].ToString() + " cheap fix";
                                // w1.transform.position += storeyHeight * Vector3.up;
                            }
                            //



                            //FIND THIS
                            //
                            
                            //Using raycasts - just cant find another solution - maybe could use PointInOABB to slim it and make more mathy/less Unity based, but performance won't be any better
                            //shoot out from p1 and p2 to find which room/rooms are there
                            //move in a little so it doesn't hit walls or adjacent rooms
                            List<RaycastHit> hits = new List<RaycastHit>();
                            for (int x = 0; x < 2; x++)
                            {
                                Vector3 p = p1;

                                Vector3 toMiddle = ((p2 - p1).normalized) * 0.2f;
                                if (x == 1)
                                {
                                    p = p2;
                                    toMiddle = -toMiddle;
                                }

                                Vector3 shootFrom = p + (lookDir * 0.2f) + toMiddle + (Vector3.up * 0.1f);
                                RaycastHit hit;
                                if(Physics.Raycast(shootFrom, Vector3.down, out hit, 0.2f))//will need to add layer in delivery project
                                    hits.Add(hit);
                            }

                            for (int x = 0; x < hits.Count; x++)
                            {
                                //GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                // c.transform.position = hits[x].point;
                                // c.transform.localScale *= 0.1f;
                                // c.transform.parent = hits[x].transform;
                                // c.name = "Raycast hit";
                            }
                            if (hits.Count == 2)
                            {
                                //if both raycast hit the same room, great, we can build the wall and parent it this hit room
                                if (hits[0].transform == hits[1].transform)
                                {
                                    //check for a wall already built
                                    if (!wallPositions.Contains(midPoint))
                                    {
                                        //   GameObject w2 = Divide.Wall(midPoint, Vector3.Distance(p1, p2), -lookDir, false, quads[i]);
                                        //   w2.transform.parent = hits[0].transform;
                                        //   w2.transform.position += storeyHeight * Vector3.up;
                                    }
                                }
                                else
                                {
                                    
                                    //Debug.Log("TWO HITS");
                                    //if we hit different rooms, we need to build two seperate half walls and parent them accordingly
                                    //so, we need to find the point at which the walls to the midpoint between the rooms
                                    GameObject room1 = hits[0].transform.gameObject;
                                    GameObject room2 = hits[1].transform.gameObject;
                                    //find the vertice closest to the midpoint between p1 and p2
                                    vertices = room1.GetComponent<MeshFilter>().mesh.vertices;
                                    float distance = Mathf.Infinity;
                                    Vector3 closest = Vector3.zero;
                                    for (int x = 0; x < vertices.Length; x++)
                                    {
                                        float temp = Vector3.Distance(midPoint, vertices[x]);
                                        if (temp < distance)
                                        {
                                            distance = temp;
                                            closest = vertices[x];
                                        }
                                    }

                                    //GameObject d = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                    //d.transform.position = closest;
                                    //d.transform.parent = roomsAndSizes[i].room.transform;
                                    //d.name = "closest";

                                    continue;//catchihg this above????????????
                                             //now build the wall between each room's vertice and the closest vertice to the middle
                                             /*
                                    midPoint = Vector3.Lerp(p1, closest, 0.5f);
                                    GameObject w2 = Divide.Wall(midPoint, Vector3.Distance(p1, closest), -lookDir, false, hits[0].transform.gameObject, storeyHeight);
                                    w2.transform.position += storeyHeight * Vector3.up;
                                    midPoint = Vector3.Lerp(p2, closest, 0.5f);
                                    GameObject w3 = Divide.Wall(midPoint, Vector3.Distance(p2, closest), -lookDir, false, hits[1].transform.gameObject, storeyHeight);
                                    w3.transform.position += storeyHeight * Vector3.up;
                                    */
                                }
                            }
                            else//if less than two hits
                            {
                                
                                //do nothing? space in stairs has ben found
                            }
                        }
                    }
                }
            }
        }
    }

    public static void MissingWallsFromLanding(List<GameObject> quads,GameObject landing)
    {
        //make loops of vertices, so we dont go out of index 
        Vector3[] landingPoints = landing.GetComponent<MeshFilter>().mesh.vertices;
        List<Vector3> landingLoop = new List<Vector3>(landingPoints);
        landingLoop.Add(landingPoints[0]);
        landingLoop.Insert(0,landingPoints[landingPoints.Length-1]);
        //find all edges which share a point with the landing
        for (int i = 0; i < quads.Count; i++)
        {
            Vector3[] quadVertices = quads[i].GetComponent<MeshFilter>().mesh.vertices;
            //make a loop with start and end
            List<Vector3> quadLoop = new List<Vector3>(quadVertices);
            quadLoop.Add(quadVertices[0]);
            quadLoop.Insert(0,quadVertices[ quadVertices.Length - 1]);

            for (int j = 1; j < landingLoop.Count-1; j++)
            {
                for (int k = 1; k < quadLoop.Count-1; k++)
                {
                    if (landingLoop[j] == quadLoop[k])
                    {
                        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        c.transform.position = landingLoop[j] + Vector3.up * 5;
                        c.transform.parent = quads[i].transform;
                        c.name = "landing";
                        c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        c.transform.position = landingLoop[j+1] + Vector3.up * 5;
                        c.transform.parent = quads[i].transform;
                        c.name = "landing2";

                        //get edge which runs along the same direction as landing edge
                        Vector3 landingEdgeDir1 = (landingLoop[j] - landingLoop[j + 1]).normalized;
                        Vector3 landingEdgeDir2 = (landingLoop[j] - landingLoop[j - 1]).normalized;
                        Vector3 quadEdgeDir1 = (quadLoop[k - 1] - quadLoop[k]).normalized;
                        Vector3 quadEdgeDir2 = (quadLoop[k + 1] - quadLoop[k]).normalized;

                        int[] quadEdge = new int[0];
                        if(landingEdgeDir1 == quadEdgeDir1 || landingEdgeDir1 == -quadEdgeDir1)
                        {
                            quadEdge = new int[] { k - 1, k};
                        }

                        if (landingEdgeDir1 == quadEdgeDir2 || landingEdgeDir1 == -quadEdgeDir2)
                        {
                            quadEdge = new int[] { k + 1, k };
                        }

                        if (landingEdgeDir2 == quadEdgeDir1 || landingEdgeDir2 == -quadEdgeDir1)
                        {
                            quadEdge = new int[] { k - 1, k };
                        }

                        if (landingEdgeDir2 == quadEdgeDir2 || landingEdgeDir2 == -quadEdgeDir2)
                        {
                            quadEdge = new int[] { k + 1, k };
                        }



                        if (quadEdge.Length == 0)
                            continue;

                        foreach (int x in quadEdge)
                        {
                            c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            c.transform.position = quadLoop[x] + Vector3.up * 5;
                            c.transform.parent = quads[i].transform;
                            c.name = "edge of quad";
                        }


                        //check there is no door 
                        List<GameObject> doors = new List<GameObject>();
                        for (int a = 0; a < quads[i].transform.childCount; a++)
                        {
                            if(quads[i].transform.GetChild(a).transform.name == "Door")
                            {
                                if(!Divide.PointsInLine(quadLoop[ quadEdge[0]],quadLoop[ quadEdge[1]], quads[i].transform.position))
                                {
                                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                    c.transform.position = Vector3.Lerp(quadLoop[quadEdge[0]], quadLoop[quadEdge[1]], 0.5f) + Vector3.up*5;
                                    c.transform.parent = quads[i].transform;
                                    c.name = "centre of missing wall";
                                }
                            }
                        }

                    }
                }
            }
        }
        //check for a door

        //if no door build a wall
    }

    public static List<GameObject> HallAccess(out List<GameObject> hallsOut,GameObject gameObject, List<GameObject> quadsBuilt,List<GameObject> hallsIn, Vector3[] landingPoints,GameObject hall,float storeyHeight,Vector3 landing1,float widthOfStair,GameObject firstFloor,bool stairsFacingLong)
    {

        List<GameObject> tempHalls = new List<GameObject>(hallsIn);

        List<int> sharedRooms = new List<int>();
        List<GameObject> toAdd = new List<GameObject>();
        List<GameObject> toRemove = new List<GameObject>();

        for (int i = 0; i < quadsBuilt.Count; i++)
        {
            Vector3 c0 = quadsBuilt[i].GetComponent<MeshRenderer>().bounds.center;

            //GameObject c = null;//debug

            List<Vector3> shared = new List<Vector3>();
            int sharedRoom = 0;
            Vector3[] vertices = quadsBuilt[i].GetComponent<MeshFilter>().mesh.vertices;
            for (int a = 0; a < quadsBuilt.Count; a++)
            {
                if (i == a)
                    continue;

                Vector3[] otherVertices = quadsBuilt[a].GetComponent<MeshFilter>().mesh.vertices;
                for (int b = 0; b < vertices.Length; b++)
                {
                    for (int j = 0; j < otherVertices.Length; j++)
                    {
                        if (Vector3.Distance(vertices[b], otherVertices[j]) < 0.1f)
                        //if (vertices[b] == otherVertices[j])
                        {
                            shared.Add(vertices[b]);
                            sharedRoom = a;


                            //c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            //c.transform.position = vertices[b];
                            //c.transform.parent = gameObject.transform;
                            //c.name = "merge";
                            //c.name = i.ToString() + " shared with " + a.ToString() + " shared count is at this point " + shared.Count;
                        }
                    }
                }
                if (shared.Count == 2)
                {
                    //making sure a room that is adjacent to two another room has access to landing

                    if (!sharedRooms.Contains(sharedRoom))
                    {

                        List<Vector3> combined = new List<Vector3>();                        
                        combined.AddRange(vertices);
                        combined.AddRange(otherVertices);

                        List<Vector3> quadPoints = new List<Vector3>();
                        for (int j = 0; j < combined.Count; j++)
                        {
                            if (!shared.Contains(combined[j]))
                            {
                                //GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                // c.transform.position = combined[j];
                                //c.transform.parent = gameObject.transform;


                                quadPoints.Add(combined[j]);
                            }
                        }

                        //chck if any of this pair share points with landing
                        
                        Vector3 c1 = quadsBuilt[i].GetComponent<MeshRenderer>().bounds.center;
                        Vector3 c2 = quadsBuilt[sharedRoom].GetComponent<MeshRenderer>().bounds.center;
                        List<GameObject> pair = new List<GameObject>();
                        pair.Add(quadsBuilt[i]);
                        pair.Add(quadsBuilt[sharedRoom]);

                        //check pair for how many points in total they share with either the hall or the landing
                        List<Vector3> sharedLandingPoints = new List<Vector3>();
                        List<Vector3> sharedHallPoints = new List<Vector3>();
                        for (int z = 0; z < pair.Count; z++)
                        {
                            // c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            // c.transform.position = pair[z].GetComponent<MeshRenderer>().bounds.center;
                            //  c.transform.parent = gameObject.transform;

                            vertices = pair[z].GetComponent<MeshFilter>().mesh.vertices;
                            for (int x = 0; x < vertices.Length; x++)
                            {
                                for (int j = 0; j < landingPoints.Length; j++)
                                {
                                    if (Vector3.Distance(vertices[x], landingPoints[j]) < 0.01f)
                                    {
                                        

                                        //c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                        // c.transform.position = vertices[x] + Vector3.up * storeyHeight;
                                        // c.transform.parent = gameObject.transform;
                                        // c.name = "landing share";


                                        sharedLandingPoints.Add(vertices[x]);
                                    }
                                }
                                //check for hall
                                if (hall != null)
                                {
                                    Vector3[] hallVertices = hall.GetComponent<MeshFilter>().mesh.vertices;
                                    for (int j = 0; j < hallVertices.Length; j++)
                                    {
                                        if (Vector3.Distance(vertices[x], hallVertices[j]) < 0.01f)
                                        {

                                            sharedHallPoints.Add(vertices[x]);

                                            //c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                            //c.transform.position = vertices[x] + Vector3.up * storeyHeight;
                                            //c.transform.parent = gameObject.transform;
                                            //c.name = "hall share";
                                        }
                                    }
                                }
                            }
                        }

                        //bool placedHall = false;

                        if (sharedLandingPoints.Count == 2)
                        {
                            ///USE THIS METHOID WITH POINTS IN LINE _ PUT IN OTHER FUNCTION
                            // Debug.Break();
                            foreach (GameObject p in pair)
                            {
                                //c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                //c.transform.position = p.GetComponent<MeshRenderer>().bounds.center;
                                //c.transform.parent = gameObject.transform;
                                //c.name = "This pair needs access";
                            }

                            //find which of the pair needs access - the one whcih doesn't have andother landing points, besides the one that was shared, between it points

                            for (int j = 0; j < pair.Count; j++)
                            {
                                List<Vector3> verticesLoop = new List<Vector3>(pair[j].GetComponent<MeshFilter>().mesh.vertices);
                                verticesLoop.Add(verticesLoop[0]);

                                bool needsAccess = true;
                                for (int k = 0; k < verticesLoop.Count - 1; k++)
                                {
                                    for (int z = 0; z < landingPoints.Length; z++)
                                    {
                                        if (verticesLoop[k] != landingPoints[z] && verticesLoop[k + 1] != landingPoints[z])
                                        {
                                            if (Divide.PointsInLine(verticesLoop[k], verticesLoop[k + 1], landingPoints[z]))
                                            {
                                                //  c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                                //  c.transform.position = landingPoints[z];
                                                //  c.transform.parent = gameObject.transform;
                                                //   c.name = "p" + j.ToString();

                                                needsAccess = false;
                                            }
                                        }
                                    }
                                }

                                if (needsAccess)
                                {
                                    //   c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                    //  c.transform.position = pair[j].GetComponent<MeshRenderer>().bounds.center;
                                    ////   c.transform.parent = gameObject.transform;
                                    //   c.name = "needs access";
                                }
                                else if (!needsAccess)
                                {
                                    //this is the one to split
                                    Vector3 spinPoint = pair[j].GetComponent<MeshRenderer>().bounds.center;
                                    //in some cases this room could ahve already been given access - check to see if it's there
                                   
                                    
                                    List<GameObject> splits = Divide.Split(gameObject, pair[j], widthOfStair * 2, true, false);
                                    GameObject[] smallerFirst = SpinSplitsToHall(splits, storeyHeight, landing1, spinPoint, firstFloor);

                                    //split this hall so landing shares points 
                                    spinPoint = smallerFirst[0].GetComponent<MeshRenderer>().bounds.center;
                                    splits = Divide.Split(gameObject, smallerFirst[0], widthOfStair * 2, true, false);
                                    GameObject[] hallAndHall = SpinSplitsToHall(splits, storeyHeight, landing1, spinPoint, firstFloor);

                                    

                                    //Debug.Break();
                                    //check this pair haven't already been built -happens sometiems

                                    bool duplicate = false;
                                    for (int q = 0; q < toAdd.Count; q++)
                                    {
                                        if (toAdd[q].GetComponent<MeshRenderer>().bounds.center == smallerFirst[1].GetComponent<MeshRenderer>().bounds.center)
                                            duplicate = true;
                                    }
                                    //duplicate = false;
                                    //Debug.Break();
                                    Debug.Log("duplicate = " + duplicate);
                                    if (duplicate)
                                    {

                                        
                                        //forget about it, already been made
                                        Destroy(hallAndHall[0]);
                                        Destroy(hallAndHall[1]);
                                        Destroy(smallerFirst[1]);
                                        Destroy(smallerFirst[0]);

                                    }
                                    else //let them live
                                    {
                                        Destroy(smallerFirst[0]);

                                        tempHalls.Add(hallAndHall[0]);
                                        tempHalls.Add(hallAndHall[1]);
                                        
                                        toAdd.Add(smallerFirst[1]);
                                        toRemove.Add(pair[j]);
                                    }
                                }
                            }
                        }

                        //if(stairsFacingLong)                        
                        if (sharedLandingPoints.Count <= 1 && sharedHallPoints.Count <= 0)
                        {
                            //Debug.Break();
                          
                            //happens on long house
                            //find adjacent room then...
                            for (int j = 0; j < quadsBuilt.Count; j++)
                            {
                                //otherVertices = quadsBuilt[j].GetComponent<MeshFilter>().mesh.vertices;
                            }

                            GameObject needsAccess = quadsBuilt[i];
                            GameObject doesntneedAccess = quadsBuilt[a];

                            if (Vector3.Distance(quadsBuilt[i].GetComponent<MeshRenderer>().bounds.center,landing1) < Vector3.Distance(quadsBuilt[a].GetComponent<MeshRenderer>().bounds.center, landing1))
                            {
                                needsAccess = quadsBuilt[a];
                                doesntneedAccess = quadsBuilt[i];
                            }


                           // c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                           // c.transform.position = needsAccess.GetComponent<MeshRenderer>().bounds.center;
                           // c.transform.parent = gameObject.transform;
                           // c.name = "needs access";

                           // c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                           // c.transform.position = doesntneedAccess.GetComponent<MeshRenderer>().bounds.center;
                           // c.transform.parent = gameObject.transform;
                           // c.name = "to this";

                            Vector3 spinPoint = doesntneedAccess.GetComponent<MeshRenderer>().bounds.center;
                            
                            List<GameObject> splits = Divide.Split(gameObject, doesntneedAccess, widthOfStair * 2, true, false);
                            GameObject[] smallerFirst = SpinSplitsToHall(splits, storeyHeight, landing1, spinPoint, firstFloor);

                            spinPoint = smallerFirst[0].GetComponent<MeshRenderer>().bounds.center;
                            splits = Divide.Split(gameObject, smallerFirst[0], widthOfStair * 2, true, false);
                            GameObject[] hallAndHall = SpinSplitsToHall(splits, storeyHeight, landing1, spinPoint, firstFloor);
                            //normally we only add [0] which is the hall, but both of these are halls
                           

                            //Destroy(smallerFirst[0]);
                            //Destroy(smallerFirst[1]);

                            hallAndHall[1].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                        
                            //Debug.Break();

                            //not sure if necessary here - doing above should be enough - optimasation, not a long list anyway
                            bool skip = false;
                            for (int q = 0; q < toAdd.Count; q++)
                            {
                                if (toAdd[q].GetComponent<MeshRenderer>().bounds.center == smallerFirst[1].GetComponent<MeshRenderer>().bounds.center)
                                    skip = true;
                            }

                            //skip = false;
                            if (!skip)
                            {


                                // smallerFirst[1].name = "Last";

                                //tempHalls.Add(smallerFirst[0]);
                                tempHalls.Add(hallAndHall[0]);
                                tempHalls.Add(hallAndHall[1]);

                                toAdd.Add(smallerFirst[1]);

                                toRemove.Add(doesntneedAccess);
                                toRemove.Add(smallerFirst[0]);
                                Destroy(smallerFirst[0]);
                            }
                            else
                            {
                                
                                Destroy(hallAndHall[0]);
                                Destroy(hallAndHall[1]);
                            }
                            

                            

                            // Debug.Break();

                            
                        }

                        //add to list to stop this quad being built again
                        //sharedRooms.Add(a);
                        sharedRooms.Add(i);


                    }
                }
               
                
            }
        }
        foreach (GameObject go in toRemove)
            quadsBuilt.Remove(go);

        foreach (GameObject go in toAdd)
            quadsBuilt.Add(go);


        hallsOut = new List<GameObject>(tempHalls);

        return quadsBuilt;
    }

    public static List<GameObject> HallAccessV2(out List<GameObject> halls, GameObject gameObject, List<GameObject> hallsIn, List<GameObject> quadsBuilt, GameObject[] smallerFirst, Vector3[] landingPoints,float widthOfStair,float storeyHeight,Vector3 landing1,GameObject firstFloor,GameObject plot)
    {
        List<GameObject> toRemove = new List<GameObject>();
        List<GameObject> toAdd = new List<GameObject>();
        //List<GameObject> tempHalls = new List<GameObject>(hallsIn);
        //foreach (GameObject go in hallsIn)
          //  tempHalls.Add(go);

        //check if any quad cant make it to the landing
        //we are looking for a pair of rooms, where one has access to the hall/lanind and the other soesnt

        //looking for landing
        List<GameObject> pairOfRooms = new List<GameObject>();
        for (int i = 0; i < quadsBuilt.Count; i++)
        {
            //continue;
            List<Vector3> shared = new List<Vector3>();
            List<Vector3> vertices = new List<Vector3>(quadsBuilt[i].GetComponent<MeshFilter>().mesh.vertices);
            vertices.Add(vertices[0]);
            Vector3[] hallVertices = smallerFirst[0].GetComponent<MeshFilter>().mesh.vertices;

            for (int j = 0; j < vertices.Count - 1; j++)
            {
                for (int k = 0; k < landingPoints.Length; k++)
                {
                    if (vertices[j] == landingPoints[k])// || vertices[j+1] == landingPoints[k])
                        continue;

                    if (Divide.PointsInLine(vertices[j], vertices[j + 1], landingPoints[k]))
                    {
                        if (!shared.Contains(vertices[j]))
                        {
                            shared.Add(vertices[j]);

                            //check if this edge is adjacent to any other quad - it shres 2 points with a quad in this case
                            for (int a = 0; a < quadsBuilt.Count; a++)
                            {
                                if (i == a)
                                    continue;

                                List<Vector3> sharedWithOther = new List<Vector3>();
                                Vector3[] otherVertices = quadsBuilt[a].GetComponent<MeshFilter>().mesh.vertices;
                                for (int b = 0; b < otherVertices.Length; b++)
                                {

                                    if (vertices[j] == otherVertices[b] || vertices[j + 1] == otherVertices[b])
                                        sharedWithOther.Add(vertices[j]);
                                }
                                //Debug.Log(sharedWithOther.Count);
                                if (sharedWithOther.Count == 2)
                                {

                                    //this room will be split..
                                    pairOfRooms.Add(quadsBuilt[i]);
                                    //to give access to-
                                    pairOfRooms.Add(quadsBuilt[a]);

                                    Vector3 centerOfA = quadsBuilt[a].GetComponent<MeshRenderer>().bounds.center;
                                    Vector3 centerOfI = quadsBuilt[i].GetComponent<MeshRenderer>().bounds.center;
                                    List<GameObject> splits = Divide.Split(gameObject, quadsBuilt[i], widthOfStair * 2, true, false);
                                    GameObject[] hallFirst = SpinSplitsToHall(splits, storeyHeight, landing1, centerOfI, firstFloor);


                                    hallFirst[0].name = "Quad";
                                   // if(!tempHalls.Contains(hallFirst[0]))
                                        hallsIn.Add(hallFirst[0]);
                                    hallFirst[0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;

                                    toRemove.Add(quadsBuilt[i]);
                                    toAdd.Add(hallFirst[1]);

                                    //Debug.Break();

                                }
                            }
                        }
                    }
                }
            }            
        }

        foreach (GameObject go in toAdd)
        {
            // go.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;
            //if (!quadsBuilt.Contains(go))
                quadsBuilt.Add(go);
        }

        foreach (GameObject go in toRemove)
            quadsBuilt.Remove(go);



        //looking for adjacent rooms
        toAdd = new List<GameObject>();
        toRemove = new List<GameObject>();
        List<int> quadsAlreadySorted = new List<int>();
        for (int i = 0; i < quadsBuilt.Count; i++)
        {
            
            Vector3[] vertices = quadsBuilt[i].GetComponent<MeshFilter>().mesh.vertices;

            
         
            for (int a = 0; a < quadsBuilt.Count; a++)
            {
                List<Vector3> sharedAdjacent = new List<Vector3>();
                Vector3[] otherVertices = quadsBuilt[a].GetComponent<MeshFilter>().mesh.vertices;
                for (int k = 0; k < otherVertices.Length; k++)
                {
                    for (int l = 0; l < vertices.Length; l++)
                    {
                        if (vertices[l] == otherVertices[k])
                            sharedAdjacent.Add(vertices[l]);
                    }
                }
                if (sharedAdjacent.Count == 2)
                {
                    if (!quadsAlreadySorted.Contains(a) && !quadsAlreadySorted.Contains(i))
                    {
                        quadsAlreadySorted.Add(i);
                        quadsAlreadySorted.Add(a);
                        quadsBuilt[i].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                        quadsBuilt[a].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;

                        //check any of this pair has zero landing points along it's edge
                        GameObject[] pair = new GameObject[] { quadsBuilt[a], quadsBuilt[i] };
                        for (int b = 0; b < pair.Length; b++)
                        {
                            int landingCount = 0;

                            List<Vector3> loop = new List<Vector3>( pair[b].GetComponent<MeshFilter>().mesh.vertices);
                            loop.Add(loop[0]);

                            for (int c = 0; c < loop.Count-1; c++)
                            {
                                for (int x = 0; x < hallsIn.Count; x++)
                                {
                                    Vector3[] hallVertices = hallsIn[x].GetComponent<MeshFilter>().mesh.vertices;
                                    for (int y = 0; y < hallVertices.Length; y++)
                                    {
                                       // GameObject c1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                       // c1.transform.position = hallVertices[y];
                                       //// c1.transform.localScale *= 0.1f;
                                       // c1.transform.parent = pair[b].transform;
                                       // c1.name = "shared hall v";
                                        if (Divide.PointsInLine(loop[c], loop[c + 1], hallVertices[y]))
                                        {
                                            
                                            landingCount++;
                                        }
                                    }
                                    
                                }                                
                            }

                            if(landingCount == 0)
                            {
                                pair[b].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Green") as Material;

                                GameObject thisRoom = pair[b];//one that needs access
                                int otherIndex = 0;
                                if (b == 0)
                                    otherIndex = 1;
                                GameObject otherRoom = pair[otherIndex];
                                List<GameObject> splits = null;
                                //give access to this quad

                               // Debug.Log("QUADS BUILT ATM = " + quadsBuilt.Count);
                                if (quadsBuilt.Count == 2)
                                {
                                    Vector3[] plotVertices = plot.GetComponent<MeshFilter>().mesh.vertices;
                                    int[] longestEdge = Divide.LongestEdge(plotVertices);
                                    Vector3 longDirOfPlot = (plotVertices[longestEdge[0]] - plotVertices[longestEdge[1]]).normalized;
                                    splits = Divide.SplitWithDirection(gameObject, otherRoom, widthOfStair * 2, longDirOfPlot,landing1);
                                    //Debug.Break();
                                }
                                else
                                    splits = Divide.Split(gameObject, otherRoom, widthOfStair * 2, true, false);

                                

                                smallerFirst = SpinSplitsToHall(splits, storeyHeight, landing1, otherRoom.GetComponent<MeshRenderer>().bounds.center, firstFloor);

                                toRemove.Add(otherRoom);
                                toAdd.Add(smallerFirst[1]);

                                smallerFirst[0].name = "Quad";
                                hallsIn.Add(smallerFirst[0]);
                                
                                //smallerFirst[0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Metal") as Material;

                                
                            }
                        }
                    }
                }
            }
        }
        foreach (GameObject go in toAdd)
        {
            // go.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;
            if (!quadsBuilt.Contains(go))
                quadsBuilt.Add(go);
        }
        

        foreach (GameObject go in toRemove)
            quadsBuilt.Remove(go);

        halls = new List<GameObject>(hallsIn);
       // Debug.Log("Halls before pass = " + halls.Count);

        return quadsBuilt;
    }

    public static GameObject[] SpinSplitsToHall(List<GameObject> splits,float storeyHeight,Vector3 landing1,Vector3 spinPoint,GameObject firstFloor)
    {
        //hall can end up on wrong side, meaning it won't be central and thus not connect all rooms
        //is it on the wrong side?
        GameObject smallerSplit = splits[0];
        GameObject largetSplit = splits[1];
        if (splits[0].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude > splits[1].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude)
        {
            smallerSplit = splits[1];
            largetSplit = splits[0];
        }
        smallerSplit.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;

        if (Vector3.Distance(smallerSplit.GetComponent<MeshRenderer>().bounds.center, landing1) > Vector3.Distance(largetSplit.GetComponent<MeshRenderer>().bounds.center, landing1))
        {
            //- let's cheat and spin the two rooms - 
            //Debug.Log("Spinner - Internal Hall Spun");
            //create a pivot point in the centre of the two rooms and spin this pivot by 180 degrees

            foreach (GameObject s in splits)
            {
                //rotate vertices
                Vector3[] vertices = s.GetComponent<MeshFilter>().mesh.vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = Divide.RotatePointAroundPivot(vertices[i], spinPoint, new Vector3(0, 180, 0));
                }
                s.GetComponent<MeshFilter>().mesh.vertices = vertices;

                //s.transform.parent = spinner.transform;                            
                s.transform.position += Vector3.up * storeyHeight;
            }
            //spinner.transform.rotation *= Quaternion.Euler(0, 180, 0);
            //put rooms to correct parent
            foreach (GameObject s in splits)
            {
               // s.transform.parent = firstFloor.transform;
                Destroy(s.GetComponent<MeshCollider>());
                s.AddComponent<MeshCollider>();
            }

        }
        else
        {
            //no need to spin
            foreach (GameObject s in splits)
            {
                s.transform.position += Vector3.up * storeyHeight;
               // s.transform.parent = firstFloor.transform;
            }
        }

        //return sorted array
        smallerSplit.name = "Hall";
        //largetSplit.name = "Room";
        GameObject[] smallerFirst = new GameObject[] { smallerSplit, largetSplit };
        foreach (GameObject go in smallerFirst)
            go.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        return smallerFirst;
    }

    public static List<Vector3> FindIntersects(List<Vector3> plotLoop,Vector3[] points)
    {
        List<Vector3> intersectPoints = new List<Vector3>();
        for (int u = 0; u < points.Length - 1; u++)
        //because the line intersection function returns points on each side, we only need to check half the points
        {
            for (int i = 0; i < plotLoop.Count - 1; i++)
            {
                Vector3 lineFromStair = (points[u + 1] - points[u]).normalized;
                Vector3 lineFromPlotPoint = (plotLoop[i + 1] - plotLoop[i]).normalized;
                Vector3 intersectPoint = new Vector3();
                if (Divide.LineLineIntersection(out intersectPoint, points[u], lineFromStair, plotLoop[i], lineFromPlotPoint))
                {
                    intersectPoints.Add(intersectPoint);
                }
            }
        }

        return intersectPoints;
    }

    public static bool CloseToOppositeWall(out float d, List<Vector3> plotLoop,Vector3[] points,GameObject stairCollider)
    {
        //this is doin a lot of the same things as findIntersects() Should create an option in find interescts function to check if needed (optimisation)

        bool closeToOppositeWall = false;
        float distance = Mathf.Infinity;
        /*
        for (int u = 0; u < points.Length - 1; u++)
        //because the line intersection function returns points on each side, we only need to check half the points
        {
            for (int i = 0; i < plotLoop.Count - 1; i++)
            {
                Vector3 lineFromStair = (points[u + 1] - points[u]).normalized;
                Vector3 lineFromPlotPoint = (plotLoop[i + 1] - plotLoop[i]).normalized;
                Vector3 intersectPoint = new Vector3();
                if (Divide.LineLineIntersection(out intersectPoint, points[u], lineFromStair, plotLoop[i], lineFromPlotPoint))
                {
                    //check for distance to opposite wall dir is facing forward
                    if (-lineFromStair == stairCollider.transform.forward)
                    {
                        float temp = Vector3.Distance(intersectPoint, points[u]);
                        if (temp < stairCollider.transform.localScale.z * 1f)
                        {
                            closeToOppositeWall = true;
                            distance = temp;
                        }


                    }
                }
            }
        }
        */

        for (int i = 0; i < plotLoop.Count - 1; i++)
        {
            Vector3 stairCentre = stairCollider.GetComponent<MeshRenderer>().bounds.center + (stairCollider.transform.forward
                *stairCollider.transform.localScale.z*0.5f);

            //GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //c.transform.position = stairCentre;
            //c.transform.parent = stairCollider.transform;

            Vector3 forwardDir = stairCollider.transform.forward;
            Vector3 plotPoint = plotLoop[i];
            Vector3 dirToNextPlotPoint = (plotLoop[i + 1] - plotLoop[i]).normalized;

            Vector3 intersectPoint = new Vector3();
            if (Divide.LineLineIntersection(out intersectPoint, stairCentre, forwardDir, plotPoint, dirToNextPlotPoint))
            {
                //returns on from both directoins - check if point is the one in front of stair collider
                if ((intersectPoint - stairCentre).normalized == forwardDir)
                {
                    // c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //c.transform.position = intersectPoint;
                   // c.transform.parent = stairCollider.transform;
                   // c.name = "intersectPoint";

                    float temp = Vector3.Distance(intersectPoint, stairCentre);
                    if (temp < stairCollider.transform.localScale.z * 1f)
                    {
                        closeToOppositeWall = true;
                        distance = temp;

                        //Debug.Break();
                    }

                    
                }

                
            }
        }

        d = distance;
        return closeToOppositeWall;
    }

    public static bool ColliderInCorner(out Vector3 point, GameObject collider,Mesh mesh)
    {
        //boxes only
        //if collider in a corner, it means it shares a vertice - find that vertice on the plot (the main rectangle that defines house)
        //8,9,10,11 is top vertices of stair collider

        Vector3[] vertices = collider.GetComponent<MeshFilter>().mesh.vertices;
        List<Vector3> colliderPoints = new List<Vector3>();
        for (int i = 8; i < 12; i++)
        {
            Vector3 p = vertices[i];

            p.x *= collider.transform.localScale.x;
            p.z *= collider.transform.localScale.z;
            p = collider.transform.localRotation * p;
            p += collider.transform.position;
            p.y = 0;

            colliderPoints.Add(p);
        }



        //check these collider points against plotVertices
        Vector3[] plotVertices = mesh.vertices;
        Vector3 corner = Vector3.zero;
        bool cornerFound = false;
        for (int i = 0; i < colliderPoints.Count; i++)
        {
            for (int j = 0; j < plotVertices.Length; j++)
            {
                if (colliderPoints[i] == plotVertices[j])
                {
                    cornerFound = true;
                    corner = colliderPoints[i];
                }
            }
        }
        if (cornerFound)
            point = corner;
        else
            point = Vector3.zero;//must be assigned something

        return cornerFound;
    }

    public static bool StairsFacingLong(GameObject stairCollider, GameObject plot)
    {
        Vector3[] plotVertices = plot.GetComponent<MeshFilter>().mesh.vertices;
        int[] longestEdge = Divide.LongestEdge(plotVertices);
        int[] shortestEdge = Divide.ShortestEdge(plotVertices);
        float shortestDistance = Vector3.Distance(plotVertices[shortestEdge[0]], plotVertices[shortestEdge[1]]);
        bool stairsFacingLong = false;
        if ((plotVertices[longestEdge[1]] - plotVertices[longestEdge[0]]).normalized == stairCollider.transform.forward || (plotVertices[longestEdge[0]] - plotVertices[longestEdge[1]]).normalized == stairCollider.transform.forward)
        {
         //   Debug.Log("Stairs facing long");
            stairsFacingLong = true;
        }
        //else
        //    Debug.Log("Stairs facing short");

        return stairsFacingLong;
    }
    //some relly specific statics
    public static bool SwitchSmallerRoomNearPoint(List<GameObject> splits,Vector3 point,Vector3 spinPoint,bool makeFurthestAway)
    {
        bool switched = false;
        //hall can end up on wrong side, meaning it won't be central and thus not connect all rooms
        //is it on the wrong side?
        GameObject smallerSplit = splits[0];
        GameObject largetSplit = splits[1];
        if (splits[0].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude > splits[1].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude)
        {
            smallerSplit = splits[1];
            largetSplit = splits[0];
            
            
        }
        smallerSplit.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;


        bool doSpin = false;
        if (!makeFurthestAway)
        {
            if (Vector3.Distance(smallerSplit.GetComponent<MeshRenderer>().bounds.center, point) > Vector3.Distance(largetSplit.GetComponent<MeshRenderer>().bounds.center, point))
                doSpin = true;
        }
        else
        {
            if (Vector3.Distance(smallerSplit.GetComponent<MeshRenderer>().bounds.center, point) < Vector3.Distance(largetSplit.GetComponent<MeshRenderer>().bounds.center, point))
                doSpin = true;
        }


        if (doSpin)
        {
            //- let's cheat and spin the two rooms - 
            Debug.Log("Spinner - Internal Hall Spun");
            //create a pivot point in the centre of the two rooms and spin this pivot by 180 degrees

            foreach (GameObject s in splits)
            {
                //rotate vertices
                Vector3[] vertices = s.GetComponent<MeshFilter>().mesh.vertices;
                for (int j = 0; j < vertices.Length; j++)
                {
                    vertices[j] = Divide.RotatePointAroundPivot(vertices[j], spinPoint, new Vector3(0, 180, 0));
                }
                s.GetComponent<MeshFilter>().mesh.vertices = vertices;
                s.GetComponent<MeshFilter>().mesh.RecalculateBounds();
            }

            switched = true;
        }
        else
        {
            //no need to spin
           
        }
        return switched;
    }

    public static List<Vector3> CheckCornersForOverlap(List<Vector3> vertices, List<GameObject> objectsInRoom, float size)
    {
        //checks given vertices to see if any objects are near, returns a list of vertices where objects could potentially be palced without overlap

        List<Vector3> freeVertices = new List<Vector3>();

        for (int i = 0; i < vertices.Count; i++)
        {
            bool free = true;
            for (int j = 0; j < objectsInRoom.Count; j++)
            {
                float distance = Vector3.Distance(vertices[i], objectsInRoom[j].transform.position);
                if (distance < size)
                    free = false;                    
            }

            if (free)
                freeVertices.Add(vertices[i]);

        }

        return freeVertices;
    }
    public static int FurthestVerticeFromObjects(List<GameObject> objects, List<Vector3> vertices)
    {
        int furthest = 0;
        Vector3 avg = Vector3.zero;
        for (int i = 0; i < objects.Count; i++)
        {
            avg += objects[i].transform.position;
        }
        avg /= objects.Count;

        float distance = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            float temp = Vector3.Distance(vertices[i], avg);
            if(temp > distance)
            {
                distance = temp;
                furthest = i;
            }
        }
        return furthest;
    }
    public static List<Vector3> SortByDistanceFromObjects(List<Vector3> vertices, List<GameObject> objects)
    {
        Vector3 avg = Vector3.zero;
        for (int i = 0; i < objects.Count; i++)
        {
            avg += objects[i].transform.position;
        }
        avg /= objects.Count;

        //sort freeVertices by distance from doors
        vertices.Sort(delegate (Vector3 v1, Vector3 v2)
        {
            return Vector3.Distance(avg, v1).CompareTo
                        ((Vector3.Distance(avg, v2)));
        });

        return vertices;
    }
    public static Vector3 DirectionToClosestObject(Vector3 point,List<GameObject>objects)
    {
        Vector3 dir = Vector3.zero;

        List<Vector3> points = new List<Vector3>();
        foreach (GameObject go in objects)
            points.Add(go.transform.position);

        //sort freeVertices by distance from doors
        points.Sort(delegate (Vector3 v1, Vector3 v2)
        {
            return Vector3.Distance(point, v1).CompareTo
                        ((Vector3.Distance(point, v2)));
        });
        
        dir = (points[0] - point).normalized;

        return dir;
    }
    public static float DistanceToClosestObject(Vector3 point,List<GameObject>objects)
    {
        float distance = Mathf.Infinity;

        for (int i = 0; i < objects.Count; i++)
        {
            float temp = Vector3.Distance(point, objects[i].transform.position);
            if(temp < distance)
            {
                distance = temp;
            }
        }
        return distance;
    }
    public static bool CentreOfRoomOnLeft(Vector3 position, Vector3 middle,Vector3 forward)
    {
        bool onLeft = true;

        Vector3 toCentre = middle - position;

        //https://forum.unity3d.com/threads/left-right-test-function.31420/  -- left right test
        Vector3 perp = Vector3.Cross(forward, toCentre);
        float d = Vector3.Dot(perp, Vector3.up);
        if (d < 0.0)
            onLeft = false;

        return onLeft;

    }
    public static List<GameObject> SortObjectByDistanceToPoint(List<GameObject> objects, Vector3 point)
    {
        //make copy and return list sorted by distance
        List<GameObject> sorted = new List<GameObject>();
        foreach (GameObject g in objects)
            sorted.Add(g);
        //sort freeVertices by distance from doors
        objects.Sort(delegate (GameObject g1, GameObject g2)
        {
            return Vector3.Distance(point, g1.transform.position).CompareTo
                        ((Vector3.Distance(point, g2.transform.position)));
        });

        return sorted;
    }
    public static bool CheckForOverLap(List<GameObject> objects, GameObject thisObject)
    {
        //check if any overlapping objects
        bool overlap = false;
        for (int i = 0; i < objects.Count; i++)
        {
            BoxCollider thisBox = thisObject.GetComponent<BoxCollider>();
            BoxCollider targetBox = objects[i].GetComponent<BoxCollider>();

            if (thisBox.bounds.Intersects(targetBox.bounds))
                overlap = true;
            
        }
        return overlap;
    }

   
    public static Vector3[] FindOppositeEdge(GameObject room,List<Vector3[]> exteriorEdges)
    {
        //find "furthest" vertices where kitchen starts to build
        //get doors in room
        List<GameObject> doorsInKitchen = new List<GameObject>();
        for (int j = 0; j < room.transform.childCount; j++)
        {
            if (room.transform.GetChild(j).name == "Door")
                doorsInKitchen.Add(room.transform.GetChild(j).gameObject);
        }
        //get vertices of room
        Vector3[] vertices = room.GetComponent<MeshFilter>().mesh.vertices;
        //work out furthest corner
        List<int> toSkip = new List<int>();
        int furthest = FurthestVerticesFromDoors(doorsInKitchen, vertices, toSkip);
        //find furthest from "furthest"

        Vector3 edgePoint1 = Vector3.zero;
        float distance = -1;
        for (int j = 0; j < vertices.Length; j++)
        {
            float temp = Vector3.Distance(vertices[j], vertices[furthest]);
            if (temp > distance)
            {
                distance = temp;
                edgePoint1 = vertices[j];
            }
        }

        Vector3[] edgeToUse = new Vector3[0];
        Vector3 edgePoint2 = new Vector3();
        //find which other vertice this shaes with an exterior edge                      

        for (int x = 0; x < exteriorEdges.Count; x++)
        {
            if (exteriorEdges[x][0] == edgePoint1)
            {
                edgeToUse = exteriorEdges[x];
                edgePoint2 = exteriorEdges[x][1];
            }
            if (exteriorEdges[x][1] == edgePoint1)
            {
                edgeToUse = exteriorEdges[x];
                edgePoint2 = exteriorEdges[x][0];
            }
        }
        if (edgeToUse.Length == 0)
        {
            edgePoint1 = vertices[furthest];
            //Debug.Break();
            Debug.Log("null - swapping for living room exterior door/ or kitchen?");
            //do again, could put in while loop when safe
            for (int x = 0; x < exteriorEdges.Count; x++)
            {
                if (exteriorEdges[x][0] == vertices[furthest])
                {
                    edgeToUse = exteriorEdges[x];
                    edgePoint2 = exteriorEdges[x][1];
                }
                if (exteriorEdges[x][1] == vertices[furthest])
                {
                    edgeToUse = exteriorEdges[x];
                    edgePoint2 = exteriorEdges[x][0];
                }
            }

            //flip these round so lerp works nicely - door will slip in near tv corner now
            Vector3 temp = edgePoint2;
            edgePoint2 = edgePoint1;
            edgePoint1 = temp;
        }

        return new Vector3[] { edgePoint1, edgePoint2 };
    }
  
}


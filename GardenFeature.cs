using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GardenFeature : MonoBehaviour
{

    public float area;
    public Vector3 centroid;
    public List<Vector3> pointsOnEdge = new List<Vector3>();
    public List<EdgeHelpers.Edge> boundaryPath;
    public Vector3 forwardDir;
 //   public bool cone;
 //   public bool pot;
//    public bool voronoi;
    public bool makeSquare;
    public bool test;
    void Awake()
    {
     //   this.enabled = false;
    }

    void Start()
    {
    ///    pot = true;
    //    cone = false;
    //    voronoi = false;

        //calling statics for now//from?house paths decider
        //if (!test)
           // FindMiterPointsForFeature(gameObject);
        //else
        //    CreateFeature(transform.position, 3.1f);

       // if (voronoi)
      //      CreateFeature(transform.position);
      //  return;


        /*
        CreateListOfEdgePoints();
        //Vector3[] mVertices = transform.FindChild("Junction Corner").GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] mVertices = pointsOnEdge.ToArray();
        // area = Area(mVertices);

        // centroid = FindCentroid(mVertices);
        centroid = compute3DPolygonCentroid(mVertices);


          GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
          cube.transform.position = centroid;
          cube.name = "Centroid";
          StartCoroutine("RaycastForFeatureSize");
          */
    }

    void Realign()
    {

        //makes the transform position the centre of the mesh and moves the mesh vertices so the stay the same in world space
        Mesh mesh = transform.Find("Combined mesh").GetComponent<MeshFilter>().mesh;

        //find the Y offset


        transform.position = mesh.bounds.center;

        Vector3[] verts = mesh.vertices;
        List<Vector3> vertsList = new List<Vector3>();

        for (int i = 0; i < verts.Length; i++)
        {
            vertsList.Add(verts[i] - transform.position);
        }



        mesh.vertices = vertsList.ToArray();


    }

    void CreateListOfEdgePoints()
    {
        Mesh mesh = transform.GetComponent<MeshFilter>().mesh;
        Debug.Log(mesh.vertexCount);
        //use the EdgeHelpers static class to work out the boundary could also be called in BushesForCell?
        boundaryPath = EdgeHelpers.GetEdges(mesh.triangles).FindBoundary().SortEdges();
        //creates a list of points round the edge of a field/cell with no duplicates or poitns within one unity of each other

        //centre point on renderer
        //Used to find out which way the bushes should move towards so that they leave a space around the edge of the field
        // Vector3 centrePoint = transform.FindChild("Combined mesh").GetComponent<MeshRenderer>().bounds.center;
        //use temporary array
        Vector3[] vertices = mesh.vertices;


        for (int i = 0; i < boundaryPath.Count; i++)
        {
            if (i == 0)
            {
                pointsOnEdge.Add(vertices[boundaryPath[i].v1]);
                pointsOnEdge.Add(vertices[boundaryPath[i].v2]);
            }
            if (i > 0)
            {
                Vector3 pos = vertices[boundaryPath[i].v2];
                //if this does not match the previous entry, add it
                if (Vector3.Distance(pos, pointsOnEdge[pointsOnEdge.Count - 1]) > 0.5f)
                    pointsOnEdge.Add(vertices[boundaryPath[i].v2]);
            }

            //the autoweld script leaves some vertices at the end sometimes, so we need ot check if we have finished our loop
            //do this by checking the distance to the first vertice
            //do not do on first 2 points

            if (i < 2)
                continue;

            if (Vector3.Distance(pointsOnEdge[0], pointsOnEdge[pointsOnEdge.Count - 1]) < 0.5f)
            {
                //jump out the for loop
                break;
            }
        }


        foreach (Vector3 v3 in pointsOnEdge)
        {
              //   GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
              //    cube2.name = "FenceCube";
              //  cube2.transform.position = v3;
        }
    }

    void FindRandomPointForFeature()
    {
        //randomy try and fit spherecast as big as we want the feature. 

        Vector3 bounds = GetComponent<MeshFilter>().mesh.bounds.size;
        float size = bounds.magnitude;
        RaycastHit hit;
        Vector3 point = Vector3.zero;
        
        float bushWidth = 1f;
        for(int i = 0; i < 100; i++)
        {

            float featureSize = Random.Range(2.2f, 3.2f);//1.1*2

            Vector3 position = GetComponent<MeshRenderer>().bounds.center;
            position += (Random.insideUnitSphere * size/2);



      

            if (   Physics.SphereCast(position + (Vector3.up * 50f), featureSize + bushWidth, Vector3.down, out hit, 100f, LayerMask.GetMask("TerrainCell", "House", "HouseFeature","Field")) )
            {
                //it has hit the house or the edge, try again
         //       GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
         //       cube2.transform.position = position;
         //       cube2.name = "edgePoint";
        //        cube2.transform.localScale *= featureSize;
            }

            else //if it doesnt hit anything
            {
                RaycastHit hit2;

                if(Physics.Raycast(position + (Vector3.up*50f),Vector3.down,out hit2,100f,LayerMask.GetMask("HouseCell")))
                {
                    if (hit2.transform == transform)
                    {
          //              GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //            cube2.transform.position = position;
              //         cube2.name = "edgePoint1";
                //       cube2.transform.localScale *= featureSize;


                        point = position;
                        //                    Debug.Log(hit2.transform.gameObject.GetComponent<MeshFilter>().mesh.name.ToString());

                     //   CreateFeature(hit2.point,featureSize/2,"Pot");
                    break;//exit for loop

                   
                    }
                }
            }
            
        }
        
    }


    public static bool FindMiterPointsForFeature(GameObject gameObject,float featureSize,float fenceGap,int maxFeatures,bool patioBuilt)
    {
        
        //places features and returns false if failed to do
      //  bool placed = false;
        int featuresPlaced = 0;
        //float spaceBetweenFeatures = 0.5f;
        //use statics to create border
        List<List<Vector3>> featurePositions = BorderTools.featurePositions(gameObject,false, (featureSize),featureSize,true);

        //miter points can be close to each other, remove any which are too close

        //create one list, instead of embedded lists. We don't need that for individual objects. It is used to make mud meshes

        List<Vector3> combinedList = new List<Vector3>();
        
        foreach (List<Vector3> points in featurePositions)
        {
            for (int i = 0; i < points.Count; i++)
            {
                combinedList.Add(points[i]);
            }
        }


        List<Vector3> removedPositions = new List<Vector3>();
        //use this one list to remove any points too close to another
        int lastIntMeasured = 0;
        for (int i = 0; i < combinedList.Count - 1; i++)
        {
            //check distance to next point, if it is too close, do not add to the new postions list
            if (Vector3.Distance(combinedList[lastIntMeasured],combinedList[i+1]) > featureSize )
            {
                removedPositions.Add(combinedList[i]);
                lastIntMeasured = i;
            }
        }

        
        for (int i = 0; i < removedPositions.Count; i+=(int)featureSize)
        {
           

            //spherecast for closest edge and rotate to face
            RaycastHit hit;
            if(!Physics.SphereCast(removedPositions[i] + (Vector3.up * 10),featureSize, Vector3.down, out hit, 20f, LayerMask.GetMask("TerrainCell","Field","HouseFeature","House","Road")))
            {
                //if it doesn't hit outside field or terrain cell, and it doesn't hit house or house feature either
                /*
                if(hit.transform.gameObject.layer == LayerMask.NameToLayer("HouseFeature") || hit.transform.gameObject.layer == LayerMask.NameToLayer("House"))
                {
                    //too close to house or feature, skip
                    continue;
                }
                */

                GameObject feature = new GameObject();
                feature.transform.parent = gameObject.transform;
                feature.name = "Feature";
                feature.transform.position = removedPositions[i];

                //choose feature depending on featureSize. Passed from a for loop in House Paths script
                if (featureSize == 1f)
                {

                    PlantPot.PlantPotMaker(feature.transform, 0.2f, 4, 0.1f, 1f, featureSize /2, true);    //2 for safety margin

                    //make hit point y the same as hit position so we ony spin on Y axis, other wise, it will look down to the ground
                    Vector3 hitPoint = hit.point;
                    hitPoint.y = removedPositions[i].y;

                    Quaternion rot = Quaternion.LookRotation(hitPoint - removedPositions[i]);
                    //only spin on Y, otherwise it will look down

                    feature.transform.rotation = rot;
                }

                if(featureSize == 2)
                {
                    //make this a patio. Largest feature atm
            //        ProceduralToolkit.Examples.UI.TableAndChairs.TableAndChairsMaker(gameObject, removedPositions[i], 1f, 4);

                    //CreateFeature(gameObject, removedPositions[i], featureSize, "Voronoi");
            //        TangoDitch(gameObject,removedPositions[i], 20* (int)featureSize, featureSize,true);


                    //we only need one patio
                    break;


                //      GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //     cube.transform.position = removedPositions[i];
                //    cube.transform.localScale *= featureSize;
                 //    cube.layer = LayerMask.NameToLayer("HouseFeature");
                     

                }
                else if (featureSize >=3)
                {
                    if (!patioBuilt)
                    {

                       // ProceduralToolkit.Examples.UI.TableAndChairs.TableAndChairsMaker(gameObject, removedPositions[i], 1f, 4);

                        //CreateFeature(gameObject, removedPositions[i], featureSize, "Voronoi");
                        TangoDitch(gameObject, removedPositions[i], 20 * (int)featureSize,featureSize/1.5f, true);

                        patioBuilt = true;
                    }
                    else if (patioBuilt)
                    {
                        BranchArray branchArray = GameObject.FindGameObjectWithTag("Code").GetComponent<BranchArray>();
                        branchArray.MakeGardenTree(gameObject, removedPositions[i]);

                        //trees don't have colliders atm, make a box collider the size of the feature for it, so other features do ont build on top of
                        BoxCollider bc = feature.AddComponent<BoxCollider>();
                        bc.size *= featureSize;
                        feature.layer = LayerMask.NameToLayer("HouseFeature");

                        //cheat just now by make trees smaller
                        feature.transform.localScale *= 0.5f;
                    }
                }

                featuresPlaced++;
                if (featuresPlaced == maxFeatures)
                    break;

             //   placed = true;
            }
        }


        return patioBuilt;
    }

    IEnumerator RaycastForFeatureSize()
    {
        //find closest edge of polygon to centre of cell
        RaycastHit hit;
        Physics.SphereCast(centroid + (Vector3.up * 50), 20f, Vector3.down, out hit, 100f, LayerMask.GetMask("TerrainCell"));


        Vector3 edgeOfCell = hit.point;

        GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube2.transform.position = hit.point;
        cube2.name = "edgePoint";

        //find closest point on House
        Physics.SphereCast(centroid + (Vector3.up * 50), 20f, Vector3.down, out hit, 100f, LayerMask.GetMask("House"));


        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = hit.point;
        cube.name = "housePoint";

        Vector3 edgeOfHouse = hit.point;
     

        Vector3 middlePoint = Vector3.Lerp(edgeOfCell, edgeOfHouse, 0.5f);
        float maxRadius = Vector3.Distance(middlePoint, edgeOfCell);
        maxRadius -= 0.5f;//bush placement width

        //edge of house hits the roof, so we need to raycast for the ground

        if (Physics.Raycast(middlePoint, Vector3.down, out hit, 100f, LayerMask.GetMask("HouseCell")))
        {
            GameObject cube3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube3.transform.position = hit.point;
            cube3.name = "MidPoint";
        }
        else
            Debug.Log("Garden Feature did not hit");

        //CreateFeature(hit.point);//old
       
        yield break;
        
    }

     public static void CreateFeature(GameObject gameObject, Vector3 position, float maxRadius,string type)
    {


        //float radius = Random.Range(1.1f, maxRadius/2);//or maxradius?
        float radius = maxRadius;
       // TangoDitch(radius); //dont need soil under?

        GameObject feature = new GameObject();     
        feature.transform.parent = gameObject.transform;
        feature.name = "Feature";

        MeshRenderer meshRenderer = feature.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Resources.Load("Cement") as Material;

        if (type == "Cone")
        {
            Cone coneObj = feature.AddComponent<Cone>();
            coneObj.bottomRadius = radius;
            coneObj.topRadius = radius;
        //    if(makeSquare)
        //    {
        //        coneObj.nbSides = 4;
        //    }
        }

        else if(type == "Pot")
        {
            feature.transform.position = position;
            PlantPot.PlantPotMaker(feature.transform, 0.2f, 4, 0.1f, 1f, maxRadius,true);
            //height
            //sides
            //thickness
            //reduce multiplier for bottom radius
            //radius
        }
        else if(type =="Voronoi")
        {
          //  radius *= 0.5f;
            MeshGenerator mg = feature.AddComponent<MeshGenerator>();
            mg.fillWithPoints = true;
            mg.renderCells = true;
            mg.useSortedGeneration = false;
            mg.shrinkCells = true;
            mg.gardenFeature = true;
            mg.extrudeCells = true;
            mg.moveToAfterBuilding = position + new Vector3(0,0.2f,0);// + depth // each tile may need raycasted for y position if hilly

            //mg makes everything on 0, pass this for reAlign after mesh creation
            

            mg.volume = new Vector3(1000.0f, 0.0f, 1000.0f); //has to be big because mesh generato uses vector3 zero instead of transform.position?

            //   mg.yardPoints.Add(Vector3.zero);
          //  radius *= 0.5f;
            //add a cirlce of points to a voronoi mesh generator

         //   float randomBorder = Random.Range(1.21f, 1.5f); //0.2 is the random amount for inside tiles
            for (float i = 0; i < 360; i+=10)
            {
                Vector3 dir = Vector3.right*(radius); //four rows of circles
                dir = Quaternion.Euler(0f, i, 0f) * dir;

                //  GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                // cube.transform.position = position + dir;

                for (float j = 1; j < radius; j+= radius/4)
                {
                    if (j < radius)
                    {
                        float x = Random.Range(-0.2f, 0.2f);
                        float z = Random.Range(-0.2f, 0.2f);
                        Vector3 random = new Vector3(x, 0f, z);

                        random = new Vector3(x, 0f, z);
                        mg.yardPoints.Add((dir * j) + random);
                    }
                    //add acontrolling border on the last one

                }
               
                    mg.yardPoints.Add(dir * (radius));
                    mg.yardPoints.Add(dir * (radius * 1.2f));
                    //   mg.yardPoints.Add(dir * (j * randomBorder*1.5f));
                    //   mg.yardPoints.Add(dir * (j * randomBorder * 2f));
                


            }

            //add random points within circle
            for(int i = 0; i < 10 * radius; i++)
            {
                Vector2 random = Random.insideUnitCircle;
                Vector3 v3 = new Vector3(random.x, 0f, random.y);
                v3 *= 3*(radius/4); //not all the way out //3f;

                Vector3 p = v3;
                p.y = 0f;

                mg.yardPoints.Add(p);
            }

            for(int i =0; i < mg.yardPoints.Count;i++)
            {               

                Vector3 v3 = new Vector3(mg.yardPoints[i].x, 0, mg.yardPoints[i].z);
                mg.yardPoints[i] = v3;
                
            }
        }

        feature.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        if(type == "Pot")
            feature.transform.rotation = gameObject.transform.GetChild(0).Find("Meshes").GetComponent<StretchQuads>().toRoad;

       // CreateTree(position,radius);

    }

   
    public static void TangoDitch(GameObject gameObject,Vector3 position, int sides, float radius,bool extrude)
    {
        //circle under slabs - e.g A "Tango Ditch" -A. T Campbell
        

        GameObject soil = new GameObject();
        soil.name = "Soil";
        soil.layer = LayerMask.NameToLayer("HouseFeature");
        soil.transform.parent = gameObject.transform;
        soil.transform.position = position;
        
        List<Vector3> soilPoints = new List<Vector3>();        

        for (float i = 0; i <= 360; i += 360/sides) //detail
        {
            //don't build to edge available space // -0.5 //rando?

            Vector3 dir = Vector3.right * (radius - 0.5f);
            

            dir = Quaternion.Euler(0f, i, 0f) * dir;

            soilPoints.Add(dir);
        }

        Mesh mesh = new Mesh();

        Vector3 middle = FindCentralPointByAverage(soilPoints.ToArray());

        List<Vector3> soilPointsWithMiddle = new List<Vector3>();

        //  Debug.Log(soilPoints.Count);
        soilPointsWithMiddle.Add(middle - gameObject.transform.position);

        foreach (Vector3 v3 in soilPoints)
            soilPointsWithMiddle.Add(v3 - gameObject.transform.position);

        //if we want to scale on the x, we need to spin the vertices a bit
        #region SpinVertices
        for (int j = 0; j < soilPointsWithMiddle.Count; j++)
        {
            soilPointsWithMiddle[j] = Quaternion.Euler(0f, 45f, 0f) * soilPointsWithMiddle[j];
        }
        #endregion

        mesh.vertices = soilPointsWithMiddle.ToArray();

        List<int> tris = new List<int>();
        //create triangles using center point as the anchor for each triangle
        for (int i = 0; i < soilPointsWithMiddle.Count - 1; i++)
        {
            if (i == 0)
                continue;

            tris.Add(0);
            tris.Add(i);
            tris.Add(i+1);

        }

        mesh.triangles = tris.ToArray();
        
        //add depth to cell
        if (extrude)
        {
           Mesh extrudedMesh = ExtrudeCell.Extrude(mesh, 0.2f);
           GameObject extrusion = new GameObject();
           extrusion.name = "Soil Extrusion";
            extrusion.transform.parent = soil.transform;
            extrusion.transform.position = position;


            MeshFilter meshFilterEx = extrusion.AddComponent<MeshFilter>();
            meshFilterEx.mesh = extrudedMesh;

            MeshRenderer meshRendererEx = extrusion.AddComponent<MeshRenderer>();
            meshRendererEx.sharedMaterial = Resources.Load("Brown") as Material;

        }

        MeshFilter meshFilter = soil.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = soil.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Resources.Load("Brown") as Material;

        soil.transform.position += Vector3.up * 0.2f * 0.5f; //depth of extrusion halved

        soil.AddComponent<MeshCollider>();

        
        //move down off the edge slightly
        //   Vector3 newPos = soil.transform.position;
        // newPos.y -= potThickness / 2;
        // soil.transform.position = newPos;

        //stretch 
        // soil.transform.localScale += stretch;

    }
    void CreateTree(Vector3 position,float radius)
    {
        BranchArray branchArray = GameObject.FindGameObjectWithTag("Code").GetComponent<BranchArray>();
        branchArray.MakeGardenTree(transform.gameObject, position);
        
    }


    public float Area(Vector3[] mVertices)
    {
        float result = 0;
        for (int p = mVertices.Length - 1, q = 0; q < mVertices.Length; p = q++)
        {
            result += (Vector3.Cross(mVertices[q], mVertices[p])).magnitude;
        }
        return result * 0.5f;
    }

    public static Vector3 FindCentralPointByAverage(Vector3[] mVertices)
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

    Vector3 compute3DPolygonCentroid(Vector3[] vertices)//, int vertexCount)    
    {
        Vector3 centroid = new Vector3();
        float signedArea = 0.0f;
        float x0 = 0.0f; // Current vertex X
        float y0 = 0.0f; // Current vertex Y
        float x1 = 0.0f; // Next vertex X
        float y1 = 0.0f; // Next vertex Y

        float a = 0.0f;  // Partial signed area

        // For all vertices
        int i = 0;
        for (i = 0; i < vertices.Length - 1; i++)
        {
            x0 = vertices[i].x;
            y0 = vertices[i].z;

            x1 = vertices[(i + 1) % vertices.Length].x;
            y1 = vertices[(i + 1) % vertices.Length].z;

            a = (x0 * y1) - (x1 * y0);
            signedArea += a;
            centroid.x += (x0 + x1) * a;
            centroid.z += (y0 + y1) * a;
        }

        signedArea *= 0.5f;
        centroid.x /= (6.0f * signedArea);
        centroid.z /= (6.0f * signedArea);
        
        //raycast for y position
        Vector3 from = new Vector3(centroid.x, 1000f, centroid.z);
        RaycastHit hit;
        if (Physics.Raycast(from, Vector3.down, out hit, 2000f, LayerMask.GetMask("HouseCell"), QueryTriggerInteraction.Ignore))
        {
            centroid = hit.point;
        }
        else
        {
            Debug.Log("Centroid Y finder failed");
        }
        
        return centroid;
    }

}

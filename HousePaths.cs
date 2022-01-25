using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class HousePaths : MonoBehaviour {
    public List<Vector3> pathPoints = new List<Vector3>();
    public float pathDensity = 0.5f;
    private GameObject door;
    public float pathSize;
    public Vector3 gatePos;
    float borderSize = 2f;
    public GameObject gravel;

    public List<Vector3> gizmoPoints = new List<Vector3>();
    // Use this for initialization
    void Start ()
    {

        

        /*
                //get path sizes
                if (gameObject.tag == "HouseCell")
                    pathSize = GetComponent<HouseCellInfo>().fenceIndentation;


                PathFromFrontDoor();
                SlabPathWithSpaces();
                // Plants();
            //    StartCoroutine("AddFence");
                StartCoroutine("HouseBorder");
                //    StartCoroutine("FlowerBeds");

                StartCoroutine("StartGardenFeature");
                */

        //figure out how to limit this to stay inside cell
        borderSize = Random.Range(0.5f, 2f);

        Decider();

        
    }

    void Decider()
    {
        //decides what will be put around the house

        //slabs around house
        //flowerbeds
        //garden feature - border trees/bushes

       

        int options = 5;


        bool flowerbeds = true;//0
        
        bool gardenFeature = false;//2
        bool gravel = false;//3
        bool fence = true;//4 always fence atm
        bool housePath = false;//5

        for (int i = 0; i < options; i++)
        {
            int random = Random.Range(0, 2);
            if(random  == 0)
            {
                if(i == 0)
                {
                    //flowerbeds
                    flowerbeds = true;
                }             
                if (i == 1)
                {
                    //garden feature
                    gardenFeature = true;
                }
                if (i == 2)
                    gravel = true;

                if (i == 3)
                    fence = true;

                if (i == 4)
                    housePath = true;
            }
        }
        

        //call functions //larger objects first/more important

        //finds closest point on edge to the door and palces triggers to make sure there is a space left to get out the house
        PathFromFrontDoor();

        if (gravel)
        {


            int random = Random.Range(0, 2);
            bool buildFrontArea = false;
            if (random == 0)
                buildFrontArea = true;

            Mesh mesh = GravelAroundHouse(buildFrontArea);
            //bricks around gravel

            float brickLength = Random.Range(0.2f, 0.4f);
            float brickDepth = Random.Range(0.1f, 0.2f);
            float brickHeight = Random.Range(0.1f, 0.2f);
            float spaceBetweenBricks = Random.Range(0.01f, 0.05f);
            float randomRotAmt = 2f;

            Material material = Resources.Load("Brick") as Material;
            BrickBorder(mesh, brickLength, brickDepth, brickHeight, spaceBetweenBricks, randomRotAmt, material);//, false);



            //bricks/slabs around outside of gravel on grass
            //if border size is large enough, we can lay slabs inside the gravel

            //A chance to make an extra border
            if (Random.Range(0, 2) == 0)
            {
                bool borderInside = false;
                if (borderSize > 1.5f)
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        //a chance to make it
                        borderInside = true;
                    }
                }

                GravelBorder(mesh, brickDepth, borderInside);//scaled mesh

                //unused HouseBorder can create straight lines of slabs if front area is not built

                //if!buildFrontArea
                //HouseBorder();

                //need to add slab size parameter to houseBorder (bordersize*0.5f)
            }



        }
        else if (!gravel)
        {
            //build a square border. Chance to build flowers
            HouseBorder();
        }

        if (housePath)
        {
            SlabPathWithSpaces();
        }

        if (gardenFeature)
        {
            PointsForGardenFeatures();
        }

        if (flowerbeds)
        {
            Flowerbeds(); //igoniring gravel atm
        }

        if (fence)
            AddFence();

    }

    void PointsForGardenFeatures()
    {
        //use static in GardenFeature to create objects around border

        float featureSize = 10f;
        //  bool placed = false;
        int maxFeatures = 20;
        float fenceGap = 1f;
        //we only want one table and chaits/patio to be built. Garden  Feature returns true when it is built
        bool patioBuilt = false;
        //tries to place garden features and returns true if manages
        //tries to place as big a feature as it can
        for (float i = featureSize; i >= 1f; i--)
        {
            //placed = 
           patioBuilt =  GardenFeature.FindMiterPointsForFeature(gameObject, i, fenceGap, maxFeatures,patioBuilt);

            //if managed to plaec, jumpt out. Do ont keep trying to place smaller features
            //if (placed)
            //    break;
        }
    }
    void PathFromFrontDoor()
    {
        door = transform.GetChild(0).Find("Meshes").Find("Foyer").Find("Door").gameObject;

        Vector3 doorPos = door.transform.position;
   
        //create path points

        //move from door forwards using door.forward(we rotated the transform) checking for end of the cell
        bool foundEnd = false;
        float i = 0;
        float pathDensity = 0.5f;
        Vector3 direction = door.transform.forward;
        while (!foundEnd)
        {
            Vector3 position = door.transform.position + (direction * i);

         //   GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
         //      cube.transform.position = position;

            //add points for path by racasting for this cell
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up, Vector3.down, out hit, 10f, LayerMask.GetMask("HouseCell"), QueryTriggerInteraction.Ignore))
            {


                //   GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //   cube2.transform.position = hit.point;
                pathPoints.Add(hit.point);


                //triggger cubes //maybe a more efficient way to do this?
                Vector3 fenceIntersect = hit.point;

                GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube2.transform.position = fenceIntersect;
                cube2.transform.localScale *= 1.5f;
                cube2.transform.rotation = door.transform.rotation;
                cube2.transform.parent = transform;
                cube2.name = "Fence Trigger";
                cube2.layer = 25; //House Feature. We can re use this layer as the fence is not close to any features on the house
                cube2.GetComponent<BoxCollider>().isTrigger = true; //can maybe use this for character too?
                cube2.GetComponent<MeshRenderer>().enabled = false;

            }
            //look for the cell that isn't this one. Only layers that will be around the cell are field cells or cells at the roads(TerrainCells)
            if (Physics.Raycast(position + Vector3.up, Vector3.down, out hit, 10f, LayerMask.GetMask("TerrainCell","Field"), QueryTriggerInteraction.Ignore))
            {                
                //place a trigger so the fence knows to leave a gap
                //make sure to place trigger where fence will be building. Indented from the edge of the cell by a variable
                Vector3 positionEnd = door.transform.position + (direction * (i));

                //for other functions to use
                gatePos = positionEnd;
                //move back

                //placing below on every hit atm
                /*
                //place a few trigger blocks so fences/bushes etc can use them all at different distances from the edge of the cell

                for (int b = 0; b < 5; b++)
                {
                    Vector3 fenceIntersect = positionEnd + ((-direction* b));

                    GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube2.transform.position = fenceIntersect;
                    cube2.transform.localScale *= 1.5f;
                    cube2.transform.rotation = door.transform.rotation;
                    cube2.transform.parent = transform;
                    cube2.name = "Fence Trigger";
                    cube2.layer = 25; //House Feature. We can re use this layer as the fence is not close to any features on the house
                    cube2.GetComponent<BoxCollider>().isTrigger = true; //can maybe use this for character too?
                    cube2.GetComponent<MeshRenderer>().enabled = false;
                }

                */
                 foundEnd = true;                
            }
           

            i += pathDensity;          
                
        }
    }
    void SlabPathWithSpaces()
    {
     //   Debug.Log("slbs");
        GameObject slabPath = new GameObject();
        slabPath.transform.position = door.transform.position;
        slabPath.transform.parent = transform;
        slabPath.name = "Slab Path";
        slabPath.layer = 25; //house feature


        for(int i = 0; i < pathPoints.Count; i++)
        {
           //doorstep?
            if (i == 0)
                continue;
            //find position before putting cube in. Raycast can hit this cube
            Vector3 pos = pathPoints[i];
            RaycastHit hit;
            if (Physics.Raycast(pathPoints[i] + Vector3.up, Vector3.down, out hit, 2f, LayerMask.GetMask("TerrainCell", "HouseFeature","HouseCell","Default"), QueryTriggerInteraction.Ignore))
            {
                pos = hit.point;
            }

            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.transform.position = pos;
            slab.transform.rotation = door.transform.rotation;
            slab.name = "Slab";
            slab.transform.parent = slabPath.transform;
            slab.layer = LayerMask.NameToLayer("HouseFeature");

            float x = 0.8f;//door width
            float y = 0.1f;
            float z = pathDensity / 2;
            //create doorstep
            if (i == 0)
                z *= 3;

          
            Vector3 size = new Vector3(x, y, z);
            
            slab.transform.localScale = size;
            //move up
            

            slab.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Grey") as Material;
        }
    }

    void OnDrawGizmos()
    {
        foreach(Vector3 v3 in gizmoPoints)
        {
            Gizmos.DrawCube(v3 + transform.GetChild(0).position, Vector3.one * 0.1f);
        }
    }

    void Plants()
    {
        GameObject plantPrefab = Resources.Load("Prefabs/Flora/PottedPlant") as GameObject;

        for(int i = 0; i < 1; i++)//only make 1st on atm        for (int i = 0; i < pathPoints.Count; i++)
        {
            //skip doorstep
            if (i == 0)
                continue;

            Vector3 directionToDoor = (pathPoints[i] - door.transform.position).normalized;

            Vector3 offset = Quaternion.Euler(0f, 90f, 0f) * directionToDoor;
            Vector3 offset2 = Quaternion.Euler(0f, -90f, 0f) * directionToDoor;

            offset *= 0.8f;//doorWidth
            offset2 *= 0.8f;//doorWidth

            Vector3 position = pathPoints[i] + offset;
            Vector3 position2 = pathPoints[i] + offset2;

            GameObject plant = Instantiate(plantPrefab, position, Quaternion.identity) as GameObject;
            plant.GetComponent<PlantController>().roses = true;

            plant.name = "Plant";
            plant.transform.parent = transform.parent;

            GameObject plant2 = Instantiate(plantPrefab, position2, Quaternion.identity) as GameObject;
            plant2.GetComponent<PlantController>().roses = true;

            plant2.name = "Plant";
            plant2.transform.parent = transform.parent;
        }
    }


    List<Vector3> EdgeVector3s(Mesh mesh)
    {
        List<Vector3> edgePoints = new List<Vector3>();

        Vector3[] vertices = mesh.vertices;

        //get edges
        List<int> edgeVertices = FindEdges.EdgeVertices(mesh,0.1f);

        for(int i = 0; i < edgeVertices.Count; i++)
        {
            edgePoints.Add(vertices[edgeVertices[i]]);
        }

        return edgePoints;
    }
 
    void HouseBorder()
    {
        //use static functions to create list of border points

        
        // get quad mesh and order so they create loop
        //0,3,1,2 This is the way the quad is constructed, verts in clockwise direction
        Mesh quadForHouseSizeMesh = transform.GetChild(0).Find("Meshes").GetChild(0).GetComponent<MeshFilter>().mesh;
        
        List<Vector3> quadVertices = new List<Vector3>();
        quadVertices.Add(quadForHouseSizeMesh.vertices[0]);    //house parent has a non zero transform position
        quadVertices.Add(quadForHouseSizeMesh.vertices[3]);
        quadVertices.Add(quadForHouseSizeMesh.vertices[1]);
        quadVertices.Add(quadForHouseSizeMesh.vertices[2]);
        //create loop
        quadVertices.Add(quadForHouseSizeMesh.vertices[0]);
        
     

        bool primitives = false;
        bool bushes = false;
        //randomise this
        int chooser = Random.Range(0, 2);

        //if (chooser == 0)
            primitives = true;
        //if (chooser == 1)
        //  bushes = true;


        //float width = borderSize / 2f;
        //if (primitives)
        //    width = borderSize; // Random.Range(0.4f,1f);
        //create border/miter points

        //choose whether points go inside the gravel (if any) or go outside
        //if they go inside, we need to make sure slabs are small enough to fit. This is governed by borderSize public var
        //place down centre of gap. So half the borderSize
        float tileSize = Random.Range(0.2f, 1f);
        float width = borderSize/2 + (tileSize/4);

        
        List<Vector3> intersectionPoints = BorderTools.IntersectionPoints(quadVertices, -width);
        //create gap

        //lerp
        List<Vector3> lerpedPoints = BorderTools.LerpedPoints(intersectionPoints,true, tileSize);
        
        //if needed
        int plantChooser = Random.Range(0, 2);
        GameObject plantPrefab = Resources.Load("Prefabs/Flora/PottedPlant") as GameObject;

        foreach (Vector3 v3 in lerpedPoints)
        {
            //check for house features, don't place on paths etc


            RaycastHit hit;
            if (Physics.Raycast(v3 + (Vector3.up * 5) + transform.GetChild(0).position, Vector3.down,out hit, 10f, LayerMask.GetMask("HouseFeature"), QueryTriggerInteraction.Collide))
            {
                //skip if we find a path
                if (hit.transform.name == "Fence Trigger")
                {
                    continue;
                }
                //else
                //keep going
            }

            if (primitives)
            {
            //    Debug.Log("slabs");
                GameObject primitive = null;
                int primitiveChooser = Random.Range(0, 2);
                //overide - Adam says nobody has circle slabs
                primitiveChooser = 0;
                if(primitiveChooser == 0)
                   primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                if(primitiveChooser == 1)
                   primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

                //plus procHouse object position
                primitive.name = "Border Slab";
                primitive.transform.position = v3 + transform.GetChild(0).position;
                primitive.transform.parent = transform;
                primitive.layer = LayerMask.NameToLayer("HouseFeature");

                //can't randomise depth, until we know what what side the house is on compared to each slab
                //float depth = width - Random.Range(width * 0.5f, width);
                //make 
                primitive.transform.localScale = new Vector3(tileSize- (tileSize*0.1f), 0.2f, tileSize- (tileSize * 0.1f));

                //already worked out house rotation
                Quaternion rotation = transform.GetChild(0).Find("Meshes").GetComponent<StretchQuads>().toRoad;
                //randomised a bit
                float range = 5f;
                rotation = rotation * Quaternion.Euler(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
                primitive.transform.rotation = rotation;

                //colour
                primitive.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Grey") as Material;

                //add plants
                
                if(plantChooser == 0)
                {
                    GameObject plant = Instantiate(plantPrefab, v3 + transform.GetChild(0).position , Quaternion.identity) as GameObject;
                    plant.GetComponent<PlantController>().roses = true;
                    plant.GetComponent<PlantController>().addPot = true;
                    plant.name = "Plant";
                    plant.transform.parent = transform;
                }
            }

            if (bushes)
            {
                BranchArray branchArray = GameObject.FindGameObjectWithTag("Code").GetComponent<BranchArray>();
                branchArray.MakeShrub(gameObject, v3 + transform.GetChild(0).position,false);
            }
        }




        //yield break;
    }

    Mesh GravelAroundHouse(bool buildFrontArea)
    {
        //makes a gravel border around house and a flowing shape towards the gate
        //grab the quad mesh which the house is biult from

        Mesh quadMesh = transform.GetChild(0).Find("Meshes").GetChild(0).GetComponent<MeshFilter>().mesh;
        List<Vector3> quadVertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        for(int i = 0; i < quadMesh.triangles.Length; i++)
        {
            triangles.Add(quadMesh.triangles[i]);
        }

        foreach (Vector3 v3 in quadMesh.vertices)
            quadVertices.Add(v3);

        //expand this mesh to create a border around house
        //float borderSize = Random.Range(1.2f, 1.5f);//better way?//need to watch out for cell edge//up top now

        //to expand, we need to move away from middle of quad by border size
        Vector3 middle = quadMesh.bounds.center;
        for(int i = 0; i < quadVertices.Count; i++)
        {
            //direction from middle
            Vector3 dir = (quadVertices[i] - middle).normalized;

            quadVertices[i] += dir * borderSize;

            ///quadVertices[i] *= borderSize;
        }

        //pin these points to the cell floor, can be under due to cell not being flat
        for (int i = 0; i < quadVertices.Count; i++)
        {
       

            RaycastHit hit;
            Physics.Raycast(quadVertices[i] + transform.GetChild(0).position + Vector3.up*2f, Vector3.down, out hit, 10f, LayerMask.GetMask("HouseCell"));
          //  quadVertices[i] = hit.point- transform.GetChild(0).position;

            
        }
        //find two closest points to the front gate

        //returns list, furthest away first 
        quadVertices.Sort(delegate (Vector3 c1,Vector3 c2) {
            return Vector3.Distance(gatePos, c1 + transform.GetChild(0).position).CompareTo
                        ((Vector3.Distance(gatePos, c2 + transform.GetChild(0).position)));
        });



        //new vertices
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();
   
        //add vertices in clockwise order
        List<Vector3> clockWiseVertices = ClockwiseVertices(quadVertices[0], quadVertices[1], quadVertices[2]);
        foreach (Vector3 v3 in clockWiseVertices)
            newVertices.Add(v3);

        //add the triangles for these
        newTriangles.Add(0);
        newTriangles.Add(1);
        newTriangles.Add(2);

        //the other half of the quad now
        clockWiseVertices = new List<Vector3>();
        clockWiseVertices = ClockwiseVertices(quadVertices[1], quadVertices[3], quadVertices[2]);
        foreach (Vector3 v3 in clockWiseVertices)
            newVertices.Add(v3);
        
        //triangles
        newTriangles.Add(3);
        newTriangles.Add(4);
        newTriangles.Add(5);

        if (buildFrontArea)
        {
            //the first point in the list is the corner closest to the gate
            //second points is the gate -- make it local
            //third is midpoint between the two closest points to the gate
            Vector3 gatePosLocal = gatePos - transform.GetChild(0).position;
            //move to the side of the path//use global
            Vector3 directionToGateFromDoor = (gatePos - door.transform.position).normalized;
            Vector3 rotated = Quaternion.Euler(0f, 90f, 0f) * directionToGateFromDoor;

            Vector3 midpoint = Vector3.Lerp(quadVertices[0], quadVertices[1], 0.5f);
            Vector3 toTheSide = gatePosLocal + rotated;
            Vector3 toTheSide2 = gatePosLocal - rotated;
           // toTheSide *= borderSize; //gap at fence size?
           // toTheSide2 *= borderSize;

            //make sure two the side is planted on the ground. After rotating it can be floating
            /*
            RaycastHit hit;
            Physics.Raycast(toTheSide + Vector3.up + transform.GetChild(0).position, Vector3.down, out hit, 10f, LayerMask.GetMask("TerrainCell", "HouseCell", "Field"));
            toTheSide = hit.point - transform.GetChild(0).position;

            RaycastHit hit2;
            Physics.Raycast(toTheSide2 + Vector3.up + transform.GetChild(0).position, Vector3.down, out hit2, 2f, LayerMask.GetMask("TerrainCell", "HouseCell", "Field"));
            toTheSide2 = hit2.point - transform.GetChild(0).position;
            */
            clockWiseVertices = new List<Vector3>();
            //find what side point is closer to this corner
            if (Vector3.Distance(quadVertices[0], toTheSide) < Vector3.Distance(quadVertices[0], toTheSide2))
                clockWiseVertices = ClockwiseVertices(quadVertices[0], toTheSide, midpoint);
            else
                clockWiseVertices = ClockwiseVertices(quadVertices[0], toTheSide2, midpoint);

            foreach (Vector3 v3 in clockWiseVertices)
                newVertices.Add(v3);

            //create the triangles
            newTriangles.Add(6);
            newTriangles.Add(7);
            newTriangles.Add(8);

            //other half of front area

            //find what side point is closer to this corner
            clockWiseVertices = new List<Vector3>();
            if (Vector3.Distance(quadVertices[1], toTheSide) < Vector3.Distance(quadVertices[1], toTheSide2))
                clockWiseVertices = ClockwiseVertices(quadVertices[1], toTheSide, midpoint);
            else
                clockWiseVertices = ClockwiseVertices(quadVertices[1], toTheSide2, midpoint);

            //add verts
            foreach (Vector3 v3 in clockWiseVertices)
                newVertices.Add(v3);
            //tris
            newTriangles.Add(9);
            newTriangles.Add(10);
            newTriangles.Add(11);

            //fill in midpoint to path gap
            //this will give a pattern of three triangles along the front of the house, interlocking
            clockWiseVertices = new List<Vector3>();
            clockWiseVertices = ClockwiseVertices(midpoint, toTheSide, toTheSide2);

            //add verts
            foreach (Vector3 v3 in clockWiseVertices)
                newVertices.Add(v3);
            //tris
            newTriangles.Add(12);
            newTriangles.Add(13);
            newTriangles.Add(14);

        }
        //make mesh
        Mesh stretchedMesh = new Mesh();
        stretchedMesh.SetVertices(newVertices);
        stretchedMesh.SetTriangles(newTriangles,0);

        //subdivide
        stretchedMesh = MeshHelper.SubdivideStatic(stretchedMesh, 8);
        //autoweld mesh
        stretchedMesh = AutoWeld.AutoWeldFunction(stretchedMesh, 0.4f, 100f);//if higher than 0.4, border around house can get fucked up
        //trying to stop right angles
        stretchedMesh = AutoWeld.AutoWeldFunction(stretchedMesh, 0.0001f, 100f);//if higher than 0.4, border around house can get fucked up
        //y adjust looking for only house cell layer
        stretchedMesh = CellYAdjust.AdjustYForLayer(transform.GetChild(0),stretchedMesh, "HouseCell", 10f);
        //randomise --learn to use noise? //pin edges to the houce cell layer
        bool skipEdges = true;        
        stretchedMesh = RandomiseVerticesUpOnly(stretchedMesh, 0.2f,skipEdges);//if too high, slabs are buried - ling variable?

        //pin outside edges of mesh to housecell
       /*
        List<int> edgeVertices = FindEdges.EdgeVertices(stretchedMesh);
        for(int i = 0; i < edgeVertices.Count; i ++)
        {
            RaycastHit hit;
            Physics.Raycast(newVertices[edgeVertices[i]] + transform.GetChild(0).position + Vector3.up * 2f, Vector3.down, out hit, 10f, LayerMask.GetMask("HouseCell"));
            newVertices[edgeVertices[i]] = hit.point - transform.GetChild(0).position;
        }
        */
        //make mesh again

        stretchedMesh.RecalculateNormals();

        //gameobject for new stretched mesh

        GameObject border = new GameObject();
        border.name = "Border With Front Area";
        border.layer = LayerMask.NameToLayer("HouseFeature"); //taken out to lay bricks. needing to change after this?
        //smae position as procHouse gameobject
        border.transform.position = transform.GetChild(0).position;
        border.transform.parent = transform.GetChild(0);

        //save this gravel so we can change its layer depending on what we are building
        //some things need to ignore the gravel
        //fixing..

        MeshFilter mf = border.AddComponent<MeshFilter>();
        mf.mesh = stretchedMesh;
        MeshRenderer mr = border.AddComponent<MeshRenderer>();
        mr.sharedMaterial = Resources.Load("Path") as Material;

        MeshCollider mc = border.AddComponent<MeshCollider>();

        return stretchedMesh;
    }

    //border around outside/inside of gravel
    void GravelBorder(Mesh mesh,float brickDepthOfInsideBorder,bool borderInside)
    {
        //new slabs
        float brickLength = Random.Range(0.3f, 0.5f);
        
        //make square
        float brickDepth = brickLength;// Random.Range(0.05f, 0.4f);

        //if border wants to buildInside, make sure tile isn't too big
        if (borderInside)
        {
            //force square. Just looks better
            brickDepth = Mathf.Clamp(brickDepth, 0.01f, borderSize / 2f);
            brickLength = brickDepth;
        }
        float brickHeight = Random.Range(0.1f, 0.3f);
        //make even
        float spaceBetweenBricks = brickLength;// Random.Range(0.1f, 0.2f);
        float randomRotAmt = 2f;

        //scales gravel mesh and places around edge of scaled mesh

        Vector3[] vertices = mesh.vertices;

        //scale mesh
        for (int i =0; i < vertices.Length; i++)
        {
            Vector3 middle = mesh.bounds.center;
            Vector3 dir = (vertices[i] - middle).normalized;

            //move mesh out or in. Allow space for inside border and our brick size here
            if(borderInside)
                vertices[i] -= dir * (brickDepthOfInsideBorder + brickDepth);
            else
                vertices[i] += dir * (brickDepthOfInsideBorder + brickDepth);
        }

        Mesh stretchedMesh = new Mesh();
        stretchedMesh.vertices = vertices;
        stretchedMesh.triangles = mesh.triangles;

        Material material = Resources.Load("Grey") as Material;
       // bool adjustForLocalPosition = true;
        BrickBorder(stretchedMesh, brickLength, brickDepth, brickHeight, spaceBetweenBricks, randomRotAmt, material);//,adjustForLocalPosition);
        
    }

    //border along edge of gravel
    void BrickBorder(Mesh mesh,float brickLength,float brickDepth,float brickHeight,float spaceBetweenBricks,float randomRotAmt,Material material)
    {
        //places bricks around the mesh it is passed
        GameObject brickBorder = new GameObject();
        brickBorder.transform.parent = transform;
        brickBorder.name = "Brick Border";

        //if(adjustForLocalPosition)
        //    brickBorder.transform.position = transform.GetChild(0).position;

        //adjust 
        /*
        float brickLength = Random.Range(0.05f,0.3f);
        float brickDepth = Random.Range(0.05f, 0.4f);
        float brickHeight = Random.Range(0.05f, 0.3f);
        float spaceBetweenBricks = Random.Range(0.1f, 0.2f);
        float randomRotAmt = 2f;
        */
        //find edges of mesh
        List<int> edgeVertices = FindEdges.EdgeVertices(mesh,0.1f);

       
        //remove duplciates. Depeding on the way autoweld removes triangles from vertices, sometimes there can be left over verts where de not want them.
        //These are a lways duplicates on another point. Removing duplicates is a hacky way of getting round this
      //  edgeVertices = edgeVertices.Distinct().ToList();
            
        Vector3[] vertices = mesh.vertices;
        Vector3 lastPoint = Vector3.zero;


        //foreach (int i in edgeVertices)
        //gizmoPoints.Add(vertices[edgeVertices[i]]);

        //choose to make brick look rough

        bool crumple = false;
        if (Random.Range(0, 2) == 0)
            crumple = true;
        

        for (int i = 0; i < edgeVertices.Count - 1; i++)
        {
            

            Vector3 directionToNext = (vertices[edgeVertices[i + 1]] - vertices[edgeVertices[i]]).normalized;
            float distanceToNext = Vector3.Distance(vertices[edgeVertices[i]], vertices[edgeVertices[i + 1]]);

            
            for (float j = brickLength; j < distanceToNext; j += (brickLength) + spaceBetweenBricks)
            {
                //raycast check to make sure it is placed inside the house cell
                Vector3 pos = vertices[edgeVertices[i]] + transform.GetChild(0).position + directionToNext * j;


                //sometimes we can pass a mesh which its center is vector3.zero
                //  if(!adjustForLocalPosition)

                //     pos = vertices[edgeVertices[i]] + directionToNext * j;

                RaycastHit hit;
                if (Physics.Raycast(pos + Vector3.up,Vector3.down,out hit,2f,LayerMask.GetMask("Field","TerrainCell","HouseFeature"),QueryTriggerInteraction.Collide))
                {
                    //if we hit any layers around the house or a house feature. skip placement of brick

                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer("HouseFeature"))
                    {
                        //only look for fence trigger cube
                        if (hit.transform.name == "Fence Trigger")
                            continue;
                        else
                        {
                            //we have hit the gravel. this is ok, place slab
                        }

                    }
                    else
                    {
                        //if we hit a field or a terrain cell, automaticcaly skip/continue
                        continue;
                    }
                }

            //    GameObject cubee = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //    cubee.transform.position = pos;


                GameObject cube = cube = GameObject.CreatePrimitive(PrimitiveType.Cube);           
                cube.transform.position = vertices[edgeVertices[i]] + transform.GetChild(0).position + directionToNext * j;

                //if true, gravel works
                //if false, flowerbeds work
               // if (adjustForLocalPosition)
               //     cube.transform.position = vertices[edgeVertices[i]] + directionToNext * j;

                Quaternion randomRot = Quaternion.Euler(Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt));
                cube.transform.rotation = Quaternion.LookRotation(directionToNext);
                cube.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
                cube.transform.rotation *= randomRot;
                cube.transform.localScale = new Vector3(brickLength, brickHeight, brickDepth);
                cube.transform.parent = brickBorder.transform;
                cube.transform.GetComponent<MeshRenderer>().material = material;// Resources.Load("Brick") as Material;

                if (crumple)
                {
                    //match the height with the smaller of the tow others
                    float y = brickHeight;
                    if (brickDepth < brickLength)
                        y = brickDepth;
                    else
                        y = brickLength;
                    cube.transform.localScale = new Vector3(brickLength, brickHeight, brickDepth);

                    Mesh meshInstance = cube.GetComponent<MeshFilter>().mesh;
                    meshInstance = MeshHelper.SubdivideStatic(meshInstance, 4);
                    meshInstance = AutoWeld.AutoWeldFunction(meshInstance, 0.01f, 64);
                    //smash mesh up a bit
                    meshInstance = RandomiseVertices(meshInstance, 0.1f, false);
                    cube.GetComponent<MeshFilter>().mesh = meshInstance;
                }

            }

            //place on next vertice if not the last
            /*
            Vector3 posLast = vertices[edgeVertices[i + 1]] + transform.GetChild(0).position;

            if (Physics.Raycast(posLast + Vector3.up, Vector3.down, 2f, LayerMask.GetMask("Field", "TerrainCell", "HouseFeature"), QueryTriggerInteraction.Collide))
            {
                //if we hit any layers around the house or a house feature. skip placement of brick
                continue;

            }

            GameObject cubeLast = GameObject.CreatePrimitive(PrimitiveType.Cube);

            //grab icomesh prefab (sorry - icomesh slow to create procedurally)
        
            cubeLast.transform.position = vertices[edgeVertices[i+1]] + transform.GetChild(0).position;
            cubeLast.transform.rotation = Quaternion.LookRotation(directionToNext);
            Quaternion randomRotLast = Quaternion.Euler(Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt));
            cubeLast.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
            cubeLast.transform.rotation *= randomRotLast;
            cubeLast.transform.localScale = new Vector3(brickLength, brickHeight, brickDepth);
            
            cubeLast.transform.parent = brickBorder.transform;
            cubeLast.transform.GetComponent<MeshRenderer>().sharedMaterial = material;

            if (crumple)
            {
                //match the height with the smaller of the tow others
                float y = brickHeight;
                if (brickDepth < brickLength)
                    y = brickDepth;
                else
                    y = brickLength;
                cubeLast.transform.localScale = new Vector3(brickLength, brickHeight, brickDepth);

                Mesh meshInstance = cubeLast.GetComponent<MeshFilter>().mesh;
                meshInstance = MeshHelper.SubdivideStatic(meshInstance,4);
                meshInstance = AutoWeld.AutoWeldFunction(meshInstance, 0.01f, 64);
                //smash mesh up a bit
                meshInstance = RandomiseVertices(meshInstance, 0.1f, false);
                cubeLast.GetComponent<MeshFilter>().mesh = meshInstance;
        

            }

        */
        }

        //close loop
        


        Vector3 directionToNextLast = (vertices[edgeVertices[0]] - vertices[edgeVertices[edgeVertices.Count - 1]]).normalized;
        float distanceToNextLast = Vector3.Distance(vertices[edgeVertices[edgeVertices.Count - 1]], vertices[edgeVertices[0]]);

        for (float j = brickLength + spaceBetweenBricks; j < distanceToNextLast; j += brickLength + spaceBetweenBricks)
        {
            Vector3 pos = vertices[edgeVertices[edgeVertices.Count - 1]] + transform.GetChild(0).position + directionToNextLast * j;
            RaycastHit hit;
            if (Physics.Raycast(pos + Vector3.up, Vector3.down, out hit, 2f, LayerMask.GetMask("Field", "TerrainCell", "HouseFeature"), QueryTriggerInteraction.Collide))
            {
                //if we hit any layers around the house or a house feature. skip placement of brick

                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("HouseFeature"))
                {
                    //only look for fence trigger cube
                    if (hit.transform.name == "Fence Trigger")
                        continue;
                    else
                    {
                        //we have hit the gravel. this is ok, place slab
                    }

                }
                else
                {
                    //if we hit a field or a terrain cell, automaticcaly skip/continue
                    continue;
                }
            }

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            //grab icomesh prefab (sorry - icomesh slow to create procedurally)
          
            cube.transform.position = vertices[edgeVertices[edgeVertices.Count - 1]] + transform.GetChild(0).position + directionToNextLast * j;
            cube.transform.rotation = Quaternion.LookRotation(directionToNextLast);
            Quaternion randomRot = Quaternion.Euler(Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt));
            cube.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
            cube.transform.rotation *= randomRot;
            cube.transform.localScale = new Vector3(brickLength, brickHeight, brickDepth);
            cube.transform.parent = brickBorder.transform;

            if (crumple)
            {
                //match the height with the smaller of the tow others
                float y = brickHeight;
                if (brickDepth < brickLength)
                    y = brickDepth;
                else
                    y = brickLength;
                cube.transform.localScale = new Vector3(brickLength, brickHeight, brickDepth);

                Mesh meshInstance = cube.GetComponent<MeshFilter>().mesh;
                meshInstance = MeshHelper.SubdivideStatic(meshInstance, 4);
                meshInstance = AutoWeld.AutoWeldFunction(meshInstance, 0.01f, 64);
                //smash mesh up a bit
                meshInstance = RandomiseVertices(meshInstance, 0.1f, false);
                cube.GetComponent<MeshFilter>().mesh = meshInstance;


            }

            cube.transform.GetComponent<MeshRenderer>().material = material;// Resources.Load("Brick") as Material;
        }
    }
    void BrickBorderForFlowerBed(Mesh mesh, float brickLength, float brickDepth, float brickHeight, float spaceBetweenBricks, float randomRotAmt, Material material)
    {
        //places bricks around the mesh it is passed
        GameObject brickBorder = new GameObject();
        brickBorder.transform.parent = transform;
        brickBorder.name = "Brick Border";
        //the flowerbed mesh sits just inside the earth. push the bricks up a little
        //brickBorder.transform.position += Vector3.up*brickHeight;

        //if(adjustForLocalPosition)
        //    brickBorder.transform.position = transform.GetChild(0).position;

        //adjust 
        /*
        float brickLength = Random.Range(0.05f,0.3f);
        float brickDepth = Random.Range(0.05f, 0.4f);
        float brickHeight = Random.Range(0.05f, 0.3f);
        float spaceBetweenBricks = Random.Range(0.1f, 0.2f);
        float randomRotAmt = 2f;
        */
        //find edges of mesh
        List<int> edgeVertices = FindEdges.EdgeVertices(mesh,0.1f);


        //remove duplciates. Depeding on the way autoweld removes triangles from vertices, sometimes there can be left over verts where de not want them.
        //These are a lways duplicates on another point. Removing duplicates is a hacky way of getting round this
        //  edgeVertices = edgeVertices.Distinct().ToList();

        Vector3[] vertices = mesh.vertices;
        Vector3 lastPoint = Vector3.zero;


        //foreach (int i in edgeVertices)
        //gizmoPoints.Add(vertices[edgeVertices[i]]);

        //choose to make brick look rough

        bool crumple = false;
        if (Random.Range(0, 2) == 0)
            crumple = true;


        for (int i = 0; i < edgeVertices.Count - 1; i++)
        {


            Vector3 directionToNext = (vertices[edgeVertices[i + 1]] - vertices[edgeVertices[i]]).normalized;
            float distanceToNext = Vector3.Distance(vertices[edgeVertices[i]], vertices[edgeVertices[i + 1]]);


            for (float j = 0; j < distanceToNext; j += (brickLength) + spaceBetweenBricks)
            {
               

                GameObject cube = cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = vertices[edgeVertices[i]] + transform.GetChild(0).position + directionToNext * j;

                //if true, gravel works
                //if false, flowerbeds work
                // if (adjustForLocalPosition)
                     cube.transform.position = vertices[edgeVertices[i]] + directionToNext * j;

                Quaternion randomRot = Quaternion.Euler(Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt));
                cube.transform.rotation = Quaternion.LookRotation(directionToNext);
                cube.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
                cube.transform.rotation *= randomRot;
                cube.transform.localScale = new Vector3(brickLength, brickHeight, brickDepth);
                cube.transform.parent = brickBorder.transform;
                cube.transform.GetComponent<MeshRenderer>().material = material;// Resources.Load("Brick") as Material;

                if (crumple)
                {
                    //match the height with the smaller of the tow others
                    float y = brickHeight;
                    if (brickDepth < brickLength)
                        y = brickDepth;
                    else
                        y = brickLength;
                    cube.transform.localScale = new Vector3(brickLength, brickHeight, brickDepth);

                    Mesh meshInstance = cube.GetComponent<MeshFilter>().mesh;
                    meshInstance = MeshHelper.SubdivideStatic(meshInstance, 4);
                    meshInstance = AutoWeld.AutoWeldFunction(meshInstance, 0.01f, 64);
                    //smash mesh up a bit
                    meshInstance = RandomiseVertices(meshInstance, 0.1f, false);
                    cube.GetComponent<MeshFilter>().mesh = meshInstance;
                }

            }

            //place on next vertice if not the last
            /*
            Vector3 posLast = vertices[edgeVertices[i + 1]] + transform.GetChild(0).position;

            if (Physics.Raycast(posLast + Vector3.up, Vector3.down, 2f, LayerMask.GetMask("Field", "TerrainCell", "HouseFeature"), QueryTriggerInteraction.Collide))
            {
                //if we hit any layers around the house or a house feature. skip placement of brick
                continue;

            }

            GameObject cubeLast = GameObject.CreatePrimitive(PrimitiveType.Cube);

            //grab icomesh prefab (sorry - icomesh slow to create procedurally)
        
            cubeLast.transform.position = vertices[edgeVertices[i+1]] + transform.GetChild(0).position;
            cubeLast.transform.rotation = Quaternion.LookRotation(directionToNext);
            Quaternion randomRotLast = Quaternion.Euler(Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt));
            cubeLast.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
            cubeLast.transform.rotation *= randomRotLast;
            cubeLast.transform.localScale = new Vector3(brickLength, brickHeight, brickDepth);
            
            cubeLast.transform.parent = brickBorder.transform;
            cubeLast.transform.GetComponent<MeshRenderer>().sharedMaterial = material;

            if (crumple)
            {
                //match the height with the smaller of the tow others
                float y = brickHeight;
                if (brickDepth < brickLength)
                    y = brickDepth;
                else
                    y = brickLength;
                cubeLast.transform.localScale = new Vector3(brickLength, brickHeight, brickDepth);

                Mesh meshInstance = cubeLast.GetComponent<MeshFilter>().mesh;
                meshInstance = MeshHelper.SubdivideStatic(meshInstance,4);
                meshInstance = AutoWeld.AutoWeldFunction(meshInstance, 0.01f, 64);
                //smash mesh up a bit
                meshInstance = RandomiseVertices(meshInstance, 0.1f, false);
                cubeLast.GetComponent<MeshFilter>().mesh = meshInstance;
        

            }

        */
        }

        //close loop



        Vector3 directionToNextLast = (vertices[edgeVertices[0]] - vertices[edgeVertices[edgeVertices.Count - 1]]).normalized;
        float distanceToNextLast = Vector3.Distance(vertices[edgeVertices[edgeVertices.Count - 1]], vertices[edgeVertices[0]]);

        for (float j = brickLength + spaceBetweenBricks; j < distanceToNextLast; j += brickLength + spaceBetweenBricks)
        {
            Vector3 pos = vertices[edgeVertices[edgeVertices.Count - 1]] + transform.GetChild(0).position + directionToNextLast * j;
            if (Physics.Raycast(pos + Vector3.up, Vector3.down, 2f, LayerMask.GetMask("Field", "TerrainCell", "HouseFeature"), QueryTriggerInteraction.Collide))
            {
                //if we hit any layers around the house or a house feature. skip placement of brick
                continue;

            }

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            //grab icomesh prefab (sorry - icomesh slow to create procedurally)

            cube.transform.position = vertices[edgeVertices[edgeVertices.Count - 1]] + transform.GetChild(0).position + directionToNextLast * j;
            cube.transform.rotation = Quaternion.LookRotation(directionToNextLast);
            Quaternion randomRot = Quaternion.Euler(Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt));
            cube.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
            cube.transform.rotation *= randomRot;
            cube.transform.localScale = new Vector3(brickLength, brickHeight, brickDepth);
            cube.transform.parent = brickBorder.transform;

            if (crumple)
            {
                //match the height with the smaller of the tow others
                float y = brickHeight;
                if (brickDepth < brickLength)
                    y = brickDepth;
                else
                    y = brickLength;
                cube.transform.localScale = new Vector3(brickLength, brickHeight, brickDepth);

                Mesh meshInstance = cube.GetComponent<MeshFilter>().mesh;
                meshInstance = MeshHelper.SubdivideStatic(meshInstance, 4);
                meshInstance = AutoWeld.AutoWeldFunction(meshInstance, 0.01f, 64);
                //smash mesh up a bit
                meshInstance = RandomiseVertices(meshInstance, 0.1f, false);
                cube.GetComponent<MeshFilter>().mesh = meshInstance;


            }

            cube.transform.GetComponent<MeshRenderer>().material = material;// Resources.Load("Brick") as Material;
        }
    }
    void BrickBorderFromList(List<Vector3> points)
    {
        //places bricks around the mesh it is passed
        GameObject brickBorder = new GameObject();
        brickBorder.transform.parent = transform;

        //adjust 
        float brickLength = 0.2f;//Random.Range(0.05f, 0.3f);
        float brickDepth = Random.Range(0.05f, 0.4f);
        float brickHeight = Random.Range(0.05f, 0.3f);
        float spaceBetweenBricks = Random.Range(0.1f, 0.2f);
        float randomRotAmt = 2f;
        
   
        
        Vector3 lastPoint = Vector3.zero;

        for (int i = 0; i < points.Count - 1; i++)
        {


            Vector3 directionToNext = (points[i + 1] - points[i]).normalized;
            float distanceToNext = Vector3.Distance(points[i], points[i + 1]);


       //     for (float j = brickLength + spaceBetweenBricks; j < distanceToNext - brickLength; j += brickLength + spaceBetweenBricks)
       //     {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = points[i] + transform.GetChild(0).position;// + directionToNext * j;
                Quaternion randomRot = Quaternion.Euler(Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt));
                cube.transform.rotation = Quaternion.LookRotation(directionToNext);
                cube.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
                cube.transform.rotation *= randomRot;
                cube.transform.localScale = new Vector3(brickLength, brickHeight, brickDepth);
                cube.transform.parent = brickBorder.transform;
                cube.transform.GetComponent<MeshRenderer>().material = Resources.Load("Brick") as Material;
        //    }

            //place on next vertice if not the last
            /*
            GameObject cubeLast = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubeLast.transform.position = points[i + 1] + transform.GetChild(0).position;
            cubeLast.transform.rotation = Quaternion.LookRotation(directionToNext);
            Quaternion randomRotLast = Quaternion.Euler(Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt));
            cubeLast.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
            cubeLast.transform.rotation *= randomRotLast;
            cubeLast.transform.localScale = new Vector3(brickLength, brickHeight, brickDepth);

            cubeLast.transform.parent = brickBorder.transform;
            cubeLast.transform.GetComponent<MeshRenderer>().material = Resources.Load("Brick") as Material;
            */
        }

        //close loop



        Vector3 directionToNextLast = (points[0] - points[points.Count - 1]).normalized;
        float distanceToNextLast = Vector3.Distance(points[points.Count - 1], points[0]);

    //    for (float j = brickLength + spaceBetweenBricks; j < distanceToNextLast; j += brickLength + spaceBetweenBricks)
    //    {
            GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube1.transform.position = points[points.Count - 1] + transform.GetChild(0).position;// + directionToNextLast * j;
            cube1.transform.rotation = Quaternion.LookRotation(directionToNextLast);
            Quaternion randomRot1 = Quaternion.Euler(Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt), Random.Range(-randomRotAmt, randomRotAmt));
            cube1.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
            cube1.transform.rotation *= randomRot1;
            cube1.transform.localScale = new Vector3(brickLength, brickHeight, brickDepth);
            cube1.transform.parent = brickBorder.transform;
            cube1.transform.GetComponent<MeshRenderer>().material = Resources.Load("Brick") as Material;
      //  }
    }//not working?

    public Mesh RandomiseVerticesUpOnly(Mesh mesh, float randomScale,bool skipEdges)
    {

        //create list of vertice numbers which are on the outside edge
        List<int> edgeVertices = new List<int>();
        if (skipEdges)
        {
            //work out vertices for edge
            edgeVertices = FindEdges.EdgeVertices(mesh,0.1f);
        }

        Vector3[] vertices = mesh.vertices;
        
        for (int i = 0; i < vertices.Length; i++)
        {
            bool skip = false;

            if (skipEdges)
            {
                for (int j = 0; j < edgeVertices.Count; j++)
                {
                    if (i == edgeVertices[j])
                    {
                        //we are on an edge vertice, skip
                        skip = true;
                        continue;
                    }
                }
            }

            if (skip)
                continue;

            Vector3 random = new Vector3(Random.Range(-randomScale * Random.value, randomScale * Random.value),
                                         Random.Range(0, randomScale * Random.value),//only move up on the y axis
                                         Random.Range(-randomScale * Random.value, randomScale * Random.value));
            
            vertices[i] += random;
            }

        mesh.vertices = vertices;

        return mesh;
    }
    public Mesh RandomiseVertices(Mesh mesh, float randomScale, bool skipEdges)
    {

        //create list of vertice numbers which are on the outside edge
        List<int> edgeVertices = new List<int>();
        if (skipEdges)
        {
            //work out vertices for edge
            edgeVertices = FindEdges.EdgeVertices(mesh,0.1f);
        }

        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            bool skip = false;

            if (skipEdges)
            {
                for (int j = 0; j < edgeVertices.Count; j++)
                {
                    if (i == edgeVertices[j])
                    {
                        //we are on an edge vertice, skip
                        skip = true;
                        continue;
                    }
                }
            }

            if (skip)
                continue;

            Vector3 random = new Vector3(Random.Range(-randomScale * Random.value, randomScale * Random.value),
                                         Random.Range(-randomScale * Random.value, randomScale * Random.value),
                                         Random.Range(-randomScale * Random.value, randomScale * Random.value));

            vertices[i] += random;
        }

        mesh.vertices = vertices;

        mesh.RecalculateNormals();

        return mesh;
    }
    //used to sort distances in a list //checks against gatePos
    int ByDistance(Vector3 a,Vector3 b)
    {
         var dstToA = Vector3.Distance(gatePos, a);
         var dstToB = Vector3.Distance(gatePos, b);
         return dstToA.CompareTo(dstToB);
    }

    void AddFence()
    {
        //wait for triggers to be instantiated
        //yield return new WaitForEndOfFrame();
        //Debug.Log("addingfence");

        

        gameObject.AddComponent<FenceAroundCell>();

        //yield break;
    }

    void Flowerbeds()
    {
        //decide if all flowerbeds are should have borders
        bool giveBorder = true;
        float brickLength = Random.Range(0.1f, 0.2f);
        float brickDepth = Random.Range(0.1f, 0.2f);
        float brickHeight = Random.Range(0.1f, 0.2f);
        float spaceBetweenBricks =  Random.Range(0.01f, 0.05f);
        float randomRotAmt = 2f;
        Material material = Resources.Load("Brick") as Material;
        
        
        //use statics to create border


        List<Vector3> edges = GetComponent<FindEdges>().pointsOnEdge;
        //create border/miter points
        List<Vector3> intersectionPoints = BorderTools.IntersectionPoints(edges, 2);
        //create gap?

        //lerp
        
        
        List<Vector3> pointsTemp = new List<Vector3>();

        bool onlyAlongRoad = false;

        if (Random.Range(0, 2) == 0)
            onlyAlongRoad = true;

        if (onlyAlongRoad)
        {
            for (int i = 0; i < intersectionPoints.Count; i++)
            {//spherecast for road

                RaycastHit hit;
                if (Physics.SphereCast(intersectionPoints[i] + Vector3.up * 10, 10f, Vector3.down, out hit, 20f, LayerMask.GetMask("Road")))
                {
                    //    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //    cube.transform.position = intersectionPoints[i];
                    //    cube.transform.localScale *= 0.1f;
                    //    cube.transform.parent = transform;

                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Road"))
                    {   //we are near the raod side, add flower bed
                        pointsTemp.Add(intersectionPoints[i]);
                    }

                    // GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    // cube.transform.position = v3;
                }

            }
        }
        else
        {
            //overide road atm
            pointsTemp = intersectionPoints;
        }
        //now we have a list of points along edge of road

        //lerp these points

        //complete loop if flower beds are not only along road
        bool completeLoop = !onlyAlongRoad; 

        List<Vector3> lerpedPoints = BorderTools.LerpedPoints(pointsTemp, completeLoop, 0.5f);//do not complete loop - false

        List<List<Vector3>> allPoints = new List<List<Vector3>>();
        //reset temp list. We are going to populate this and add to a list of lists for each mud section
        pointsTemp = new List<Vector3>();
        //now check for house features and skip over. Create a list for each section

       // Debug.Log(lerpedPoints.Count);
        for (int i = 0; i < lerpedPoints.Count; i++)
        {

         //   GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //    cube.transform.position = lerpedPoints[i];
        //    cube.transform.localScale *= 0.5f;
        //    cube.transform.parent = transform;
            RaycastHit hit;
            if (Physics.Raycast(lerpedPoints[i] + Vector3.up*5,Vector3.down, out hit, 10f, LayerMask.GetMask("HouseFeature")))
            {
                //if we hit a house feature, we can't use this point                
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("HouseFeature"))
                {
                    //we have hit a path, skip, and create new list if a list has been populated
                    if (pointsTemp.Count != 0)
                    {
                        //pointsTemp.Add(intersectionPoints[i]);
                        allPoints.Add(pointsTemp);

                        pointsTemp = new List<Vector3>();
                    }

                }
            }
            else
            {
                //this point is clear of any paths etc - add it
                pointsTemp.Add(lerpedPoints[i]);

            }           
        }
        //add remaining points to list
        if(pointsTemp.Count != 0)
            allPoints.Add(pointsTemp);

        //create mud meshes and game objects

        //grab for plants
        BranchArray branchArray = GameObject.FindGameObjectWithTag("Code").GetComponent<BranchArray>();


      //  Debug.Log(allPoints.Count);
        //run through each points lust and create semi circle, add to vertices
        foreach (List<Vector3> points in allPoints)
        {
            if (points.Count == 0)
                continue;

            //mud
            #region mud
            List<Vector3> vertices = new List<Vector3>();

            int xAmt = 10;
            float size = 0.5f;
            
            for (int i = 0; i < points.Count; i++)
            {
                //spin it a little before we start
                for (float j = 10; j < 190; j += xAmt)
                {

                    //move round in a semi circle

                    //"forward" direction

                    Vector3 forwardDir = Vector3.zero;

                    if (i < points.Count - 1)
                        forwardDir = (points[i + 1] - points[i]).normalized;
                    //switch direction for last point
                    else if (i == points.Count - 1)
                        forwardDir = (points[i] - points[i - 1]).normalized;


                    //spun to the side - start of semi circle
                    Vector3 sideDir = Quaternion.Euler(0, 90, 0) * (forwardDir);


                    //rotate around forward axis by j
                    Vector3 dir = Quaternion.AngleAxis(j, forwardDir) * sideDir;

                    dir *= size;
                    Vector3 v3 = points[i] + dir;
                    //    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //    cube.transform.position = v3;
                    //    cube.transform.localScale *= 0.1f;
                    //    cube.transform.parent = transform;

                    vertices.Add(v3);   
                }               

            }          


            List<int> triangles = new List<int>();

            int ringSegments = 180 / (xAmt);

            for (int x = 0; x < points.Count - 1; x++)
            {
                //one ring attached to the next
                for (int i = 0; i < ringSegments - 1; i++)
                {
                    triangles.Add(i + (x * ringSegments));
                    triangles.Add((i + (x * ringSegments) + 1));
                    triangles.Add(i + (x * ringSegments) + (180 / xAmt));

                    triangles.Add((i) + (x * ringSegments) + 1);
                    triangles.Add(i + (x * ringSegments) + 1 + (180 / xAmt));
                    triangles.Add(i + (x * ringSegments) + (180 / xAmt));
                }
            }

            //add caps to the start and the end


            //start
            //create points (we need before we add any more vertices) in between corners
            int verticesInRow = (xAmt - 2) * 2;
            Vector3 middle = (vertices[0] + vertices[verticesInRow]) / 2;

            Vector3 lastPoint = vertices[vertices.Count - 1];
            int firstVerticeInLastRow = vertices.Count - 2 - verticesInRow;
            Vector3 firstPointOnLastRow = vertices[firstVerticeInLastRow];

            Vector3 middleLast = (lastPoint + firstPointOnLastRow) / 2;
       
            //end

            
            /*
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = lastPoint;
            cube.transform.localScale *= 0.1f;
            cube.transform.parent = transform;

            GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube1.transform.position = firstPointOnLastRow;
            cube1.transform.localScale *= 0.1f;
            cube1.transform.parent = transform;
            */

            for (int i = 0; i < verticesInRow; i++)
            {
                vertices.Add(middle);
                vertices.Add(vertices[i + 1]);
                vertices.Add(vertices[i]);


                //add trangles now

                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);              
            }

            for (int i = firstVerticeInLastRow; i < firstVerticeInLastRow + verticesInRow; i++)
            {
                vertices.Add(middleLast);
                vertices.Add(vertices[i]);
                vertices.Add(vertices[i+1]);
                /*
                GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube1.transform.position = vertices[i];
                cube1.transform.localScale *= 0.1f;
                cube1.transform.parent = transform;
                */

                //add trangles now

                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
            }



            GameObject flowerBed = new GameObject();
            flowerBed.name = "FlowerBed";
            flowerBed.transform.parent = transform;

            MeshFilter mf = flowerBed.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();


            MeshHelper meshHelper = new MeshHelper();
            //send this object's mesh, how many subdivisions, and this gameObject to subdivision script
            StartCoroutine(meshHelper.Subdivide(mesh, 4, gameObject, false));

            mesh = AutoWeld.AutoWeldFunction(mesh, 0.1f, 500f);

            //randomise
            Vector3[] verticesArray = mesh.vertices;
            float range = 0.05f;
            for (int i = 0; i < verticesArray.Length; i++)
            {
                Vector3 randomv3 = new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
                verticesArray[i] += randomv3;

                //move down to sit in earth a bit

                //verticesArray[i].y -= size * 0.5f;
            }

            mesh.vertices = verticesArray;

            mf.mesh = mesh;

            MeshRenderer mr = flowerBed.AddComponent<MeshRenderer>();
            mr.sharedMaterial = Resources.Load("Brown") as Material;

            //create border for flowerbed
            BrickBorderForFlowerBed(mesh, brickLength, brickDepth, brickHeight, spaceBetweenBricks, randomRotAmt, material);//,false);
            #endregion

            //plants

        }//end of for each points list

        StartCoroutine(PlantsForFlowerBed(allPoints));

        //addd flowers

  //      yield break;
    }

    IEnumerator PlantsForFlowerBed(List<List<Vector3>> allPoints)
    {

        bool shrubs = false;
        bool roseBushes = true;
        //placement gap for plants
        int random = Random.Range(1, 3);
        //plant type
        //ony have tulips or roses atm
        bool roses = false;
        if (Random.Range(0, 2) == 0)
            roses = true;

        foreach (List<Vector3> points in allPoints)
        {
            for (int i = 1; i < points.Count - 1; i += random)
            {
                if (shrubs)
                {
                    //float range = 0.2f;
                    //Vector3 randomv3 = new Vector3(Random.Range(-range, range), 0f, Random.Range(-range, range));
                    //Vector3 pos = points[i] + randomv3;
                    //branchArray.MakeShrub(gameObject, points[i], false);
                }
                if (roseBushes)
                {
                    float range = 0.5f;
                    Vector3 randomv3 = new Vector3(Random.Range(-range, range), 0f, Random.Range(-range, range));
                    Vector3 pos = points[i] + randomv3;


                    GameObject roseBushPrefab = Resources.Load("Prefabs/Flora/PottedPlant") as GameObject;
                    GameObject roseBush = Instantiate(roseBushPrefab, pos, Quaternion.identity) as GameObject;
                    roseBush.transform.position += 0.6f * Vector3.up;//flowerbed height. ray? get var?
                    roseBush.transform.parent = transform;
                    roseBush.GetComponent<PlantController>().addPot = false;

                    if (roses)
                        roseBush.GetComponent<PlantController>().roses = true;

                    else
                        roseBush.GetComponent<PlantController>().tulips = true;
                }

                yield return new WaitForEndOfFrame();
            }
        }
        yield break;
    }

    IEnumerator StartGardenFeature()
    {
        yield return new WaitForEndOfFrame();
        GardenFeature gf = gameObject.AddComponent<GardenFeature>();
    }

    //returns -1 when to the left, 1 to the right, and 0 for forward/backward
    public float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);
        
        /*
        if (dir > 0.0f)
        {
            return 1.0f;
        }
        else if (dir < 0.0f)
        {
            return -1.0f;
        }
        else {
            return 0.0f;
        }
        */
        return dir;
    }

    public List<Vector3> ClockwiseVertices(Vector3 a,Vector3 b,Vector3 c)
    {
        a.y = 0f;
        b.y = 0f;
        c.y = 0f;

        List<Vector3> clockwiseVertices = new List<Vector3>();

        Vector3 centre = Vector3.Lerp(b, c, 0.5f);
        Vector3 fwd = (centre - a).normalized;
        //returns -1 if left, 1 if right
        float result1 = AngleDir(fwd, b, Vector3.up);
        float result2 = AngleDir(fwd, c, Vector3.up);

        //we are looking to go left from a
        clockwiseVertices.Add(a);

        if (result1 < result2)
        {
            clockwiseVertices.Add(b);
            clockwiseVertices.Add(c);
        }
        else 
        {
            clockwiseVertices.Add(c);
            clockwiseVertices.Add(b);
        }

        return clockwiseVertices;
    }
}

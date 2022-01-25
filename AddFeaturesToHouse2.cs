using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//adds doors/windows etc to house
public class AddFeaturesToHouse2 : MonoBehaviour
{
    public bool makeRandomHouse = true;
    [Range(2, 10)]
    public int outsideBrickSizeMultiplier = 2;
    [Range(1, 4)]
    public int tileSizeMultiplier = 2;
    public float scale = 5;

    public GameObject cubePrefab;
    public Mesh cubePrefabMesh;
    public GameObject littleCube;
    public SplitCube splitCube;
    public List<GameObject> houseBlocks;
    private float layerSize;
    private float brickSize;
    public float tileSize;

    public float tileTiltAngle;
    public float overHangAmount = 0.01f;
    //    public Mesh mesh;
    //   private List<Vector3> vertices = new List<Vector3>();
    //   private List<int> triangles = new List<int>();
    private GameObject cubeWeldedInstance;
    private GameObject foyer;
    private float heightFromStretchQuads;
    public bool doAlign;
    private int[] backVertices;
    private int[] topVertices;
    private int[] bottomVertices;
    private int[] frontVertices;
    private float randomRoofHeight;
    private Vector3 directionToDoor;
    public float doorWidth = 0.8f;
    public float windowWidth = 0.8f;
    private float outsideBrickSize = 1f;
    public float windowSize = 5f;
    public float brickReduce = 0.95f;
    public float brickRotate = 1;
    public bool animate = false;
    private int roofsBuilt = 0;
    private int tobleronesBuilt = 0;
    private int rotationTries = 0;
    private int blocksbuilt = 0;
    private BuildList buildList;
    public bool testEnvironment;
    private float amountX;
    private float amountZ;
    private Material exteriorWallMaterial;
    void Start()
    {
       if (!testEnvironment)
            buildList = GameObject.FindGameObjectWithTag("Code").GetComponent<BuildList>();

        brickSize = GetComponent<StretchQuads>().brickSize;// * 2;
        outsideBrickSize = brickSize * outsideBrickSizeMultiplier;
      
        heightFromStretchQuads = GetComponent<StretchQuads>().height;    
     
        //Set vertices for each side of our prefab cube
        CreateVerticeArrays();

        //Get Material for exterior walls
        exteriorWallMaterial = Resources.Load("RosePink") as Material;


        //Start House
        GetHouseBlocks();
        FrontDoor();

        //StartCoroutine("HousePathAdd");

        

        foreach (GameObject block in houseBlocks)
        {
            //addthefeatures first           
            Windows(block);

            //then get the build the walls to build around them
            StartCoroutine("InteriorWalls", block);
        }

        Vector3 bottomRight = transform.GetChild(0).GetComponent<MeshFilter>().mesh.vertices[0] + transform.position;
        Vector3 bottomLeft = transform.GetChild(0).GetComponent<MeshFilter>().mesh.vertices[0 + 3] + transform.position;
        Vector3 backRight = transform.GetChild(0).GetComponent<MeshFilter>().mesh.vertices[2] + transform.position;
        Vector3 backLeft = transform.GetChild(0).GetComponent<MeshFilter>().mesh.vertices[1] + transform.position;
        StartCoroutine(ExteriorWall(bottomLeft, bottomRight));
        StartCoroutine(Toblerone(bottomLeft,bottomRight,backLeft,backRight));
        
        StartCoroutine(RoofWithParameters(bottomLeft, bottomRight, backRight, false));
        StartCoroutine(RoofWithParameters(bottomLeft, bottomRight, backRight, true));
   
        bottomRight = transform.GetChild(0).GetComponent<MeshFilter>().mesh.vertices[2] + transform.position;
        bottomLeft = transform.GetChild(0).GetComponent<MeshFilter>().mesh.vertices[0] + transform.position;
        StartCoroutine(ExteriorWall(bottomLeft, bottomRight));
      

        bottomRight = transform.GetChild(0).GetComponent<MeshFilter>().mesh.vertices[1] + transform.position;
        bottomLeft = transform.GetChild(0).GetComponent<MeshFilter>().mesh.vertices[2] + transform.position;
        StartCoroutine(ExteriorWall(bottomLeft, bottomRight));

        bottomRight = transform.GetChild(0).GetComponent<MeshFilter>().mesh.vertices[3] + transform.position;
        bottomLeft = transform.GetChild(0).GetComponent<MeshFilter>().mesh.vertices[1] + transform.position;
        StartCoroutine(ExteriorWall(bottomLeft, bottomRight));



        /////////////////////////////////////////////////////////////////

        //bug check beyond this. House paths > Fence Around Cell > Garden Feature - can stop buildlist

        //calling this here atm. sort pipeline
        buildList = GameObject.FindGameObjectWithTag("Code").GetComponent<BuildList>();
        buildList.BuildingFinished();

      //////  StartCoroutine("HousePathAdd");///////// buggy atm

    }

    IEnumerator HousePathAdd()
    {
        yield return new WaitForEndOfFrame();

        //add house paths component to main combined mesh
        transform.parent.parent.gameObject.AddComponent<HousePaths>();
    }

    void CreateVerticeArrays()
    {
        backVertices = new int[]
        {
        0,2,4,6,8,10,12,13,20,21,22,23
         };
        topVertices = new int[]
        {
            2,3,4,5,
            8,9,10,11,
            17,18,
            21,22
        };
        bottomVertices = new int[]
        {
            0,1,
            6,7,
            12,13,14,15,
            16,19,
            20,23
        };
        frontVertices = new int[]
        {
            1,3,5,7,
            9,11,
            14,15,
            16,17,18,19
        };
    }
    void GetHouseBlocks()
    {
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            //first child is collider, not a room
            if (i == 0)
                continue;

            //to save on layers, move the rooms/quads which were previously a feature to just mbeing a house
            //we will make the windows,doors etc, features in this step

            transform.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("House");
            houseBlocks.Add(transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Finds a spot for the door
    /// </summary>
    void FrontDoor()
    {

        //prepare a mesh
        //create cube for manipulating
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(cube.GetComponent<BoxCollider>());

        //The front door is attached to the smallest house block/room
        //this will be our hall/foyer area

        //Let's find that smallest block

        //It isn't as simple as finding the block with the smallest scale because the size of the blocks
        //are defined at mesh level, so we need to do distance checks between mesh points

        //We shall check the distance from the bottom left to the top right
        float distance = Mathf.Infinity;
        int smallestBlockIndex = 0;
        for (int i = 0; i < houseBlocks.Count; i++)
        {
            Vector3 bottomLeft = houseBlocks[i].GetComponent<MeshFilter>().mesh.vertices[5];
            Vector3 topRight = houseBlocks[i].GetComponent<MeshFilter>().mesh.vertices[12];

            float diff = Vector3.Distance(bottomLeft, topRight);
            if (diff < distance)
            {
                distance = diff;
                smallestBlockIndex = i;
            }
        }
        //asign this so the windows function knows not to put windoes in the foyer(cos there's a door)
        foyer = houseBlocks[smallestBlockIndex];

        houseBlocks[smallestBlockIndex].transform.name = "Foyer";

        //now we have the smallest block, we need to find out which side of this block we should put
        //the main door on

        //we can do this by shooting rays out from this block in each direction and checking for hits
        //if we don't hit any other blocks, the path is clear

        Vector3 dir = DirectionCheck(houseBlocks[smallestBlockIndex]);

        //now we have out clear shoot a ray coming back from this direction to find a hit point to place the door
        Vector3 shootDir = -dir;
        //variable for rotating house to face road after building has finished
        directionToDoor = shootDir;
        //using the mesh renderer's centre point here instead of the transform's position--the meshes can be lopsided
        Vector3 shootFrom = houseBlocks[smallestBlockIndex].GetComponent<MeshRenderer>().bounds.center + (dir * 10);
        float x = 0f;
        float y = 0f;
        float z = 0f;
        RaycastHit hit;
        if (Physics.Raycast(shootFrom, shootDir, out hit, 20f, LayerMask.GetMask("House")))
        {
            x = hit.point.x;
            y = hit.point.y;
            z = hit.point.z;
        }

        Mesh mesh = cube.GetComponent<MeshFilter>().mesh;
        //alter mesh
        Vector3[] tempVerts;
        tempVerts = mesh.vertices;
        float amountOfRows = heightFromStretchQuads / (outsideBrickSize * 0.5f);
        amountOfRows = Mathf.Round(amountOfRows);
        //two thirds of wall size
        float doorSize = amountOfRows / 3;
        doorSize *= 2;
        doorSize = Mathf.Round(doorSize);

        Quaternion toRoad = GetComponent<StretchQuads>().toRoad;

        for (int i = 0; i < tempVerts.Length; i++)
        {
            tempVerts[i].z *= 0.1f;
            tempVerts[i] *= doorWidth; //needs to be a multiple of brickSize

         //   tempVerts[i] = toRoad * tempVerts[i];
        }

        float top = heightFromStretchQuads + (outsideBrickSize / 2);
        float bottom = top - ((outsideBrickSize / 2) * amountOfRows);
        bottom -= outsideBrickSize / 4;

        cube.transform.position = new Vector3(x, transform.position.y, z);
        //let's use the arrays setup in Start() to stretch the vertices around
        //imagine pulling multiple vertices in blender

        //move the bottom points to one outer brick size up
        for (int i = 0; i < bottomVertices.Length; i++)
        {

            tempVerts[topVertices[i]].y = bottom + (doorSize * (brickSize));
            tempVerts[bottomVertices[i]].y = bottom; //this is always one above bottom row - what we need!
        }


        mesh.vertices = tempVerts;
        cube.GetComponent<MeshFilter>().mesh = mesh;

        //change rotation depending on what wall we are - we figured out what direction we are facing earlier
        //rotate to face this direction
        Quaternion rotation = Quaternion.identity;
        rotation.SetLookRotation(dir);
        cube.transform.rotation = rotation;   

        cube.name = "Door";
        cube.tag = "Door";
        cube.transform.parent = houseBlocks[smallestBlockIndex].transform;
        cube.layer = 25;

        cube.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("Blue", typeof(Material)) as Material;
        cube.AddComponent<MeshCollider>();

    }


    /// <summary>
    /// Looks for suitable spots for windows and places them
    /// </summary>
    void Windows(GameObject block)
    {
        //do not add windows to foyer
        if (block == foyer)
            return;

        //create cube for manipulating
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(cube.GetComponent<BoxCollider>());

        //find a direction which does not have another block in front of it
        Vector3 dir = DirectionCheck(block);
        //adjust window size if we are on a wall smaller than the window size
        float wallSize = 0f;
        if (dir == Vector3.right || dir == Vector3.left)
        {
            //then we neeed the z size (depth) of the room
            wallSize = block.GetComponent<MeshCollider>().sharedMesh.bounds.size.z;
        }
        else
        {
            wallSize = block.GetComponent<MeshCollider>().sharedMesh.bounds.size.x;
        }
        float windowMultiplier = 0f;

        //if the sie of the wall is smaller than the initial window size, make the window size, two bricks smaller
        //than the wall size
        if (wallSize < (windowSize * outsideBrickSize * 2))
        {
            windowMultiplier = wallSize - (outsideBrickSize * 2);
        }
        else
            windowMultiplier = windowSize * outsideBrickSize;// * 0.5f;



        //find the palce on the wall
        //now we have out clear shoot a ray coming back from this direction to find a hit point to place the door
        Vector3 shootDir = -dir;
        //using the mesh renderer's centre point here instead of the transform's position--the meshes can be lopsided
        Vector3 shootFrom = block.GetComponent<MeshRenderer>().bounds.center + (dir * 10);

        RaycastHit hit;
        float x = 0f;
        float y = 0f;
        float z = 0f;
        if (Physics.Raycast(shootFrom, shootDir, out hit, 20f, LayerMask.GetMask("House")))
        {
            x = hit.point.x;
            y = hit.point.y;
            z = hit.point.z;
        }
        Mesh mesh = cube.GetComponent<MeshFilter>().mesh;
        //alter mesh
        Vector3[] tempVerts;
        tempVerts = mesh.vertices;
        float amountOfRows = heightFromStretchQuads / (outsideBrickSize * 0.5f);
        amountOfRows = Mathf.Round(amountOfRows);
        //let's make the windows half the storey height //randomise?
        float windowHeight = amountOfRows / 2;
        windowHeight = Mathf.Round(windowHeight);

        //we need to know where the top of the door is, this is where we align everything from
        //build down from here
        float doorSize = amountOfRows / 3;
        doorSize *= 2;
        doorSize = Mathf.Round(doorSize);
        Quaternion toRoad = GetComponent<StretchQuads>().toRoad;
        for (int i = 0; i < tempVerts.Length; i++)
        {
            tempVerts[i].z *= 0.1f;
            tempVerts[i] *= windowWidth;
       //     tempVerts[i] = toRoad * tempVerts[i];
        }

        float top = heightFromStretchQuads + (outsideBrickSize / 2);
        float bottom = top - ((outsideBrickSize / 2) * amountOfRows);
        bottom -= outsideBrickSize / 4;
        //we at the very bottom, we probably want to move the window up a number of bricks, so it does not look
        //like a door. 
        float topOfDoor = bottom + (doorSize * (outsideBrickSize / 2));
        float bottomOfWindow = topOfDoor - (windowHeight * outsideBrickSize / 2);


        //let's use the arrays setup in Start() to stretch the vertices around
        //imagine pulling multiple vertices in blender

        //move the bottom points to one outer brick size up
        for (int i = 0; i < bottomVertices.Length; i++)
        {

            tempVerts[topVertices[i]].y = topOfDoor;
            tempVerts[bottomVertices[i]].y = bottomOfWindow;
        }
        //now we can use the mesh
        mesh.vertices = tempVerts;
        cube.GetComponent<MeshFilter>().mesh = mesh;


        //set the door's rotation depending on what side we are coming from
        Quaternion rotation = Quaternion.identity;
        rotation.SetLookRotation(dir);

        //GameObject cube = Instantiate(cubePrefab, hit.point, rotation) as GameObject;

        cube.transform.position = new Vector3(x, transform.position.y, z);
        cube.transform.rotation = rotation;
        cube.name = "Window";
        cube.transform.parent = block.transform;
        cube.layer = 25;

        cube.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("Glass", typeof(Material)) as Material;
        cube.AddComponent<MeshCollider>();

    }

    /// <summary>
    /// creates wall pieces which embed the other features, doors, windows etc
    /// </summary>
    IEnumerator InteriorWalls(GameObject block)
    {
        //interior walls build for right to left.
        //there are some overlaps in the corners at the moment. Don't know if it needs fixed.

        //we need to wait until the features have been instantiated
        yield return new WaitForEndOfFrame();


        //do last wall
        Vector3 bottomRight = block.GetComponent<MeshFilter>().mesh.vertices[3] + block.transform.position;
        Vector3 bottomLeft = block.GetComponent<MeshFilter>().mesh.vertices[0] + block.transform.position;
        StartCoroutine(InteriorWall(block, bottomLeft, bottomRight));
     

        //do first three walls for block
        for (int i = 0; i < 4; i++)
        {
            bottomRight = block.GetComponent<MeshFilter>().mesh.vertices[i] + block.transform.position;
            bottomLeft = block.GetComponent<MeshFilter>().mesh.vertices[i + 1] + block.transform.position;
            StartCoroutine(InteriorWall(block, bottomLeft, bottomRight));
        }
        yield break;

    }

    IEnumerator InteriorWall(GameObject block, Vector3 bottomLeft, Vector3 bottomRight)
    {

        //everything is built on 10 x 10 grid, so raycast through these grids from each side of the house and place a brick
        //if we hit a house wall, e.g not a feature(door/window etc)

        //find our bottom left coordinate for this side
        //this will be mesh.vertices[0]
        //    Vector3 bottomLeft = block.GetComponent<MeshFilter>().mesh.vertices[0] + block.transform.position;// + transform.position;
        //    Vector3 topLeft = block.GetComponent<MeshFilter>().mesh.vertices[5] + block.transform.position;// + transform.position;
        //    Vector3 bottomRight = block.GetComponent<MeshFilter>().mesh.vertices[7] + block.transform.position;// + transform.position;
        //new Gameobject for the wall to go in
        float height = GetComponent<StretchQuads>().height;
        float length = Vector3.Distance(bottomLeft, bottomRight);

        GameObject wallRight = new GameObject();
        wallRight.transform.parent = block.transform;
        wallRight.transform.position = transform.position;
        wallRight.name = "InteriorWall";
        //add combine script for performance
        CombineChildren cc = wallRight.AddComponent<CombineChildren>();
        cc.enabled = false;
        cc.disableColliders = true;


        //from this position, shoot rays along the wall
        //half the brick size in case the house feature is exactly in the middle of the cube
        //if it is, we need to take half steps
        float amountOfBricksX = length / (brickSize * 0.5f);
        float amountOfBricksY = height / (brickSize);

        Quaternion toRoad = GetComponent<StretchQuads>().toRoad;
        //build in left direction
        Vector3 buildDir = ((bottomLeft - bottomRight).normalized);
        Vector3 shootDir = Quaternion.Euler(0f, 90f, 0f) * buildDir;
        Vector3 brickSizeRight = brickSize * buildDir * 0.5f;
        Quaternion faceBuildDir = Quaternion.LookRotation(-shootDir);

        //brickSizeRight = toRoad * brickSizeRight;

        Vector3 brickSizeUp = brickSize * Vector3.up;
        Mesh mesh = new Mesh();

        for (int i = 0; i < amountOfBricksY; i++)
        {
            yield return new WaitForEndOfFrame();
            GameObject currentSlice = null;
            bool lastOneWasAFeature = false;

            for (int j = 0; j < amountOfBricksX; j++)
            {

                Vector3 shootFrom = bottomRight + (brickSizeRight * j) + (brickSizeUp * i);
                //move it out from the wall a little
                shootFrom -= shootDir * brickSize * 2;
                //this wall needs pushed over a little to create a nice complete wall with no overlap
                //- this is the only wall that needs adjusted - probably just a quirk in the way i've mapped the offsets
                shootFrom -= shootDir * brickSize;
                //shootFrom += (toRoad * Vector3.back) * outsideBrickSize;
                //move it up alittle
                shootFrom.y += brickSize / 2;

                //     GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //      cube2.transform.position = shootFrom ;
                //                cube2.transform.rotation = toRoad;
                //                cube2.transform.localScale *= 0.1f;
                //      yield return new WaitForEndOfFrame();
                LayerMask lm = LayerMask.GetMask("House", "HouseFeature");
                RaycastHit hit;
                if (Physics.SphereCast(shootFrom, 0.001f, shootDir, out hit, brickSize * 4, lm))
                {
                    if (hit.transform.gameObject.layer == 24)
                    {
                        if (j == 0 || lastOneWasAFeature)
                        {
                            GameObject brick = GameObject.CreatePrimitive(PrimitiveType.Cube);// Instantiate(cubePrefab, hit.point, Quaternion.identity) as GameObject;
                            brick.transform.position = hit.point;
                            brick.transform.localScale *= brickSize;
                            brick.name = "Brick";
                            brick.transform.parent = wallRight.transform;

                            currentSlice = brick;
                            mesh = currentSlice.GetComponent<MeshFilter>().mesh;
                            //rotate the vertices in this cube to face road
                            Vector3[] vertices = mesh.vertices;
                            for (int v = 0; v < vertices.Length; v++)
                            {
                                //   vertices[v] = toRoad * vertices[v];
                                vertices[v] = faceBuildDir * vertices[v];


                            }
                            mesh.vertices = vertices;

                            lastOneWasAFeature = false;
                            //let unity catch up with creating new objects and mesh
                            yield return new WaitForEndOfFrame();
                            continue;
                        }

                        else if (j > 0)
                        {

                            //grab the end vertices from the current brick and pull them to the end of the building

                            Vector3[] vertices = mesh.vertices;

                            //this does not seem to be in a pattern
                            //I'm using unity's box for the prefab - perhaps it is this way for optimisation
                            //of the triangle patterns

                            Vector3 dir = buildDir;
                            //grab the ends

                            vertices[0] += dir * 0.5f;   //why is this 0.5f, half a unit?
                            vertices[2] += dir * 0.5f;

                            vertices[4] += dir * 0.5f;
                            vertices[6] += dir * 0.5f;

                            vertices[8] += dir * 0.5f;
                            vertices[10] += dir * 0.5f;

                            vertices[12] += dir * 0.5f;
                            vertices[13] += dir * 0.5f;

                            vertices[20] += dir * 0.5f;
                            vertices[21] += dir * 0.5f;

                            vertices[22] += dir * 0.5f;
                            vertices[23] += dir * 0.5f;

                            //give the mesh these temp values
                            mesh.vertices = vertices;

                        }

                        lastOneWasAFeature = false;
                    }
                    if (hit.transform.gameObject.layer == 25)
                    {
                        //we have hit a feature!
                        //this will force a new mesh to be made                        
                        lastOneWasAFeature = true;
                    }
                }
                //    yield return new WaitForFixedUpdate();
                //    GameObject brick2 = Instantiate(cubePrefab, shootFrom, Quaternion.identity) as GameObject;
                //    brick2.transform.localScale *= brickSize;
                //    brick2.name = "Brick2";
            }
            // yield return new WaitForEndOfFrame();
        }

        //   StartCoroutine("InteriorWallLeft", block);
        yield break;
    }
    IEnumerator ExteriorWall(Vector3 bottomLeft, Vector3 bottomRight)
    {
        
        Vector3 buildDir = ((bottomLeft - bottomRight).normalized);
        Vector3 shootDir = Quaternion.Euler(0f, 90f, 0f) * buildDir;
        Vector3 brickSizeRight = brickSize * buildDir * 0.5f;
        Quaternion faceBuildDir = Quaternion.LookRotation(-shootDir);

        //make it build the outside wall along the outside edge of the quad collider    //doing in vertices array atm
        bottomRight -= buildDir * brickSize * 0.5f;
        bottomLeft += buildDir * brickSize / 2;

        float length = Vector3.Distance(bottomLeft, bottomRight);
        length = (float)System.Math.Round(length, 1);// Edited
        
        float height = GetComponent<StretchQuads>().height;


        GameObject wallRight = new GameObject();
        wallRight.transform.parent = transform;
        wallRight.transform.position = transform.position;
        wallRight.name = "ExteriorWall";
        

        //from this position, shoot rays along the wall
        //half the brick size in case the house feature is exactly in the middle of the cube
        //if it is, we need to take half steps
        float amountOfBricksX = length / (brickSize * 0.5f);
        float amountOfBricksY = height / (brickSize);
        
        Vector3 brickSizeUp = brickSize * Vector3.up;
        Mesh mesh = new Mesh();

        for (int i = 0; i < amountOfBricksY; i++)
        {
          //  yield return new WaitForEndOfFrame();
            GameObject currentSlice = null;
            bool lastOneWasAFeature = false;

            for (int j = 0; j < amountOfBricksX; j++)
            {
                Vector3 brickPosition = bottomRight + (brickSizeRight * j) + (brickSizeUp * i);
                brickPosition.y += brickSize / 2;
                Vector3 shootFrom = brickPosition;
                //move it out from the wall a little
                shootFrom -= shootDir * brickSize * 2;
                
                LayerMask lm = LayerMask.GetMask("HouseFeature");
                RaycastHit hit;
                //look for feature
                if (Physics.SphereCast(shootFrom, 0.001f, shootDir, out hit, brickSize * 4, lm))
                {                    
                    lastOneWasAFeature = true;
            
                }
                else {
                    // if (hit.transform.gameObject.layer == 24)
                    //  {
                    if (j == 0 || lastOneWasAFeature)
                    {
                        GameObject brick = GameObject.CreatePrimitive(PrimitiveType.Cube);// Instantiate(cubePrefab, hit.point, Quaternion.identity) as GameObject;
                        brick.transform.position = brickPosition;                        
                        brick.name = "Wall Piece";
                        brick.transform.parent = wallRight.transform;
                        brick.GetComponent<MeshRenderer>().sharedMaterial = exteriorWallMaterial;
                        Destroy(brick.GetComponent<BoxCollider>());
                        currentSlice = brick;
                        mesh = currentSlice.GetComponent<MeshFilter>().mesh;
                        //rotate the vertices in this cube to face road
                        Vector3[] vertices = mesh.vertices;
                        for (int v = 0; v < vertices.Length; v++)
                        {
                            //create slants//cant see
                            //vertices[v].z *= 0.2f;
                            //vertices[v] = Quaternion.Euler(-20f, 0f, 0f) * vertices[v];
                            
                            vertices[v] = faceBuildDir * vertices[v];
                            vertices[v] *= brickSize;
                            

                            //pushes vertices outside of mesh collider
                            vertices[v] -= shootDir * brickSize * 0.5f;

                           
                        }
                        mesh.vertices = vertices;

                        lastOneWasAFeature = false;
                        //let unity catch up with creating new objects and mesh
                        yield return new WaitForEndOfFrame();
                        continue;
                    }

                    else if (j > 0)
                    {

                        //grab the end vertices from the current brick and pull them to the end of the building

                        Vector3[] vertices = mesh.vertices;

                        //this does not seem to be in a pattern
                        //I'm using unity's box for the prefab - perhaps it is this way for optimisation
                        //of the triangle patterns

                        Vector3 dir = buildDir;
                        //grab the ends

                        vertices[0] += brickSizeRight;
                        vertices[2] += brickSizeRight;

                        vertices[4] += brickSizeRight;
                        vertices[6] += brickSizeRight;

                        vertices[8] += brickSizeRight;
                        vertices[10] += brickSizeRight;

                        vertices[12] += brickSizeRight;
                        vertices[13] += brickSizeRight;

                        vertices[20] += brickSizeRight;
                        vertices[21] += brickSizeRight;

                        vertices[22] += brickSizeRight;
                        vertices[23] += brickSizeRight;

                        //give the mesh these temp values
                        mesh.vertices = vertices;
                        mesh.RecalculateBounds();
                        mesh.RecalculateNormals();

                    }

                    lastOneWasAFeature = false;
                }
            }    
        }

        //add combine script for performance
        CombineChildren cc = wallRight.AddComponent<CombineChildren>();
        
        yield break;
    }
    IEnumerator InteriorCeiling(GameObject block)
    {
        //everything is built on 10 x 10 grid, so raycast through these grids from each side of the house and place a brick
        //if we hit a house wall, e.g not a feature(door/window etc)
        Vector3 bottomLeft = block.GetComponent<MeshFilter>().mesh.vertices[5] + block.transform.position;
        Vector3 topLeft = block.GetComponent<MeshFilter>().mesh.vertices[9] + block.transform.position;
        Vector3 bottomRight = block.GetComponent<MeshFilter>().mesh.vertices[4] + block.transform.position;
        float width = Vector3.Distance(topLeft, bottomLeft);
        float length = Vector3.Distance(bottomLeft, bottomRight);


        //     GameObject cube = Instantiate(cubePrefab, topLeft, Quaternion.identity) as GameObject;
        //   cube.transform.localScale *= 0.1f;


        //new Gameobject for the wall to go in
        GameObject roof = new GameObject();
        roof.transform.parent = block.transform;
        roof.transform.position = transform.position;
        roof.name = "Roof";
        //add combine script for performance
        CombineChildren cc = roof.AddComponent<CombineChildren>();
        cc.enabled = false;
        cc.disableColliders = true;

        //from this position, shoot rays along the wall
        float amountOfBricksX = length / (brickSize * 0.5f);
        float amountOfBricksZ = width / (brickSize * 0.5f);
        Vector3 brickSizeFwd = brickSize * Vector3.forward * 0.5f;
        Vector3 brickSizeRight = brickSize * Vector3.right * 0.5f;

        for (int i = 0; i < amountOfBricksX + 1; i++) //plus one to add the last row on top of the wall
        {

            GameObject currentSlice = null;
            bool lastOneWasAFeature = false;
            for (int j = 0; j < amountOfBricksZ + 1; j++) //plus one to add the last row on top of the wall
            {
                Vector3 shootFrom = bottomLeft + (brickSizeFwd * j) + (brickSizeRight * i);
                //move it up a little
                shootFrom += Vector3.up * transform.lossyScale.y * 0.5f;

                LayerMask lm = LayerMask.GetMask("House", "HouseFeature");
                RaycastHit hit;
                if (Physics.Raycast(shootFrom, Vector3.down, out hit, transform.localScale.y * 2, lm))
                {
                    if (hit.transform.gameObject.layer == 24)
                    {
                        if (j == 0 || lastOneWasAFeature)
                        {
                            GameObject brick = GameObject.CreatePrimitive(PrimitiveType.Cube);// Instantiate(cubePrefab, hit.point, Quaternion.identity) as GameObject;
                            brick.transform.position = hit.point;
                            brick.transform.localScale *= brickSize;
                            brick.name = "Brick";
                            brick.transform.parent = roof.transform;
                            currentSlice = brick;
                            lastOneWasAFeature = false;

                            continue;
                        }

                        else if (j > 0)
                        {
                            //grab the end vertices from the current brick and pull them to the end of the building
                            Mesh mesh = currentSlice.GetComponent<MeshFilter>().mesh;
                            Vector3[] vertices = mesh.vertices;

                            //this rotation makes way more sense than above
                            //could just rotate the brick object? could have been easier --if curved walls, rotating gameobject would be way easier
                            //front panel
                            vertices[0] += Vector3.forward * 0.5f; //why is this v3 fwd? should be a variable no? //scale?
                            vertices[1] += Vector3.forward * 0.5f;

                            vertices[2] += Vector3.forward * 0.5f;
                            vertices[3] += Vector3.forward * 0.5f;
                            //top panel//leave two at the back
                            vertices[8] += Vector3.forward * 0.5f;
                            vertices[9] += Vector3.forward * 0.5f;
                            //bottom//only move two again
                            vertices[13] += Vector3.forward * 0.5f;
                            vertices[14] += Vector3.forward * 0.5f;
                            //side panel
                            vertices[16] += Vector3.forward * 0.5f;
                            vertices[17] += Vector3.forward * 0.5f;
                            //side panel, other
                            vertices[22] += Vector3.forward * 0.5f;
                            vertices[23] += Vector3.forward * 0.5f;

                            //give the mesh these temp values
                            mesh.vertices = vertices;

                        }

                        lastOneWasAFeature = false;
                    }
                    if (hit.transform.gameObject.layer == 25)
                    {
                        //we have hit a feature!
                        //this will force a new mesh to be made                        
                        lastOneWasAFeature = true;
                    }

                    // yield return new WaitForEndOfFrame();
                }
            }
            yield return new WaitForEndOfFrame();
        }

        // StartCoroutine("Roof");
        yield break;
    }

    IEnumerator BrickWall(Vector3 bottomLeft, Vector3 bottomRight)
    {
        GameObject brickWall = new GameObject();
        brickWall.name = "BrickWall";
        brickWall.transform.parent = transform;

        MeshRenderer meshRenderer = brickWall.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Resources.Load("Brick", typeof(Material)) as Material;
        MeshFilter meshFilter = brickWall.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
     
        float height = heightFromStretchQuads;
        float amountY = height / (outsideBrickSize * 0.5f);

        Vector3 buildDir = (bottomRight - bottomLeft).normalized;
        Vector3 shootDir = Quaternion.Euler(0f, 90, 0f) * buildDir;
        Quaternion faceBuildDir = Quaternion.LookRotation(-shootDir);

        Vector3 buildX = outsideBrickSize * buildDir;// Vector3.left;
        Vector3 buildY = (outsideBrickSize * 0.5f) * Vector3.up;

        #region CreateCubes
        //use Unity's primitive creation function(instead of using a prefab)
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.GetComponent<MeshRenderer>().enabled = false;
        //resize the cube's mesh to out outside brickSize
        Vector3[] cubeVertices = cube.GetComponent<MeshFilter>().mesh.vertices;

        //make the cube in to a cuboid, where the length is twice as long as the other axis
        for (int i = 0; i < cubeVertices.Length; i++)
        {

            //scale the x
            cubeVertices[i].y *= 0.5f;
            cubeVertices[i].z *= 0.5f;
            //scale the full mesh;
            cubeVertices[i] *= outsideBrickSize;

            //now shave the mesh to make it look like indidivual bricks
            cubeVertices[i] *= brickReduce;

            //rotate to face the rotation the building is at
            cubeVertices[i] = faceBuildDir * cubeVertices[i];
        }
        //make a sqare cube for fittinga round features
        Vector3[] squareBrickVertices = cube.GetComponent<MeshFilter>().mesh.vertices;
        for (int i = 0; i < cubeVertices.Length; i++)
        {

            squareBrickVertices[i].x *= 0.5f;
            squareBrickVertices[i].y *= 0.5f;
            squareBrickVertices[i].z *= 0.5f;
            squareBrickVertices[i] *= outsideBrickSize;
            squareBrickVertices[i] *= brickReduce;

            //   rotate to face the rotation the building is at//already done up above
            squareBrickVertices[i] = faceBuildDir * squareBrickVertices[i];
        }
        #endregion
        //add vertices
        List<Vector3> vertices = new List<Vector3>();
        int triangleSkip = 0;
        for (int j = 0; j <= amountY; j++)
        {
            for (int i = 0; i < amountX; i++) //equal or less to vreate on extra brick to hang over the edge of the wall
            {
                //skip the last brick on every second row, we do not need it
                if (j % 2 != 0) //if on an odd row
                {
                    if (i == amountX)
                    {
                        continue;
                    }
                }
                Vector3 offset = Vector3.zero;
                //for every second line, budge the bricks over half way to create an interlocking
                //pattern around the whole building
                if (j % 2 == 0)
                {
                    offset = bottomLeft + (i * buildX) + (j * buildY);
                }
                else
                {
                    //move it half a brick to the side
                    Vector3 budge = buildDir * outsideBrickSize * 0.5f;
                    offset = bottomLeft + (i * buildX) + (j * buildY) + budge;
                }

                //use these for figuring out what type of brick to place
                bool leftRay = false;
                bool centreRay = false;
                bool rightRay = false;

                //bring the point from where we shoot the ray forward by 2 bricksizes

                Vector3 shootFrom = offset - (shootDir * (brickSize * 2));

                //    GameObject cubesf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //     cubesf.transform.position = shootFrom; 

                //raycast backwards for house features, do not place a brick if we hit    
                LayerMask lm = LayerMask.GetMask("HouseFeature");
                if (Physics.Raycast(shootFrom, shootDir, brickSize * 4, lm))
                {
                    centreRay = true;
                }
                if (Physics.Raycast(shootFrom - (buildDir * outsideBrickSize * 0.5f), Vector3.back, brickSize * 4, lm))
                {
                    leftRay = true;
                }
                if (Physics.Raycast(shootFrom + (buildDir * outsideBrickSize * 0.5f), Vector3.back, brickSize * 4, lm))
                {
                    rightRay = true;
                }

                //if no features are hit, place a big brick
                if (!leftRay && !centreRay && !rightRay)
                {
                    foreach (Vector3 vertice in cubeVertices)
                    {
                        vertices.Add(vertice + offset);
                    }
                }
                //leftonly
                else if (leftRay && !centreRay && !rightRay)
                {
                    foreach (Vector3 vertice in cubeVertices)
                    {
                        vertices.Add(vertice + offset);
                    }
                }
                //rightonly
                else if (!leftRay && !centreRay && rightRay)
                {
                    foreach (Vector3 vertice in cubeVertices)
                    {
                        vertices.Add(vertice + offset);
                    }
                }
                //left only isnt hit
                else if (!leftRay && centreRay && rightRay)
                {
                    Vector3 budge = buildDir * outsideBrickSize * 0.25f;
                    foreach (Vector3 vertice in squareBrickVertices)
                    {
                        vertices.Add(vertice + offset - budge);
                    }
                }

                //right only isnt hit
                else if (leftRay && centreRay && !rightRay)
                {
                    Vector3 budge = -buildDir * outsideBrickSize * 0.25f;
                    foreach (Vector3 vertice in squareBrickVertices)
                    {
                        vertices.Add(vertice + offset - budge);
                    }
                }
                //if they all hit
                else if (leftRay && centreRay && rightRay)
                {
                    //do not add vertices
                    triangleSkip++;
                }
                else
                    triangleSkip++;

            }
        }

        //Move points to origin
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 temp = vertices[i];

            temp.x -= transform.position.x;
            temp.y -= transform.position.y;
            temp.z -= transform.position.z;
            vertices[i] = temp;
        }

        //now set the local position to zero, this, plust the above lines moves the vertices in such a way
        //that the centre of the transform is the origin for the mesh :S
        brickWall.transform.localPosition = Vector3.zero;

        //Create Rotations

        //run back through these vertices and add a rotation every 24 verts//
        //24 verts is one brick
        Quaternion rot = Quaternion.Euler(new Vector3(Random.Range(-brickRotate, brickRotate), Random.Range(-brickRotate, brickRotate), Random.Range(-brickRotate, brickRotate)));
        for (int i = 0; i < vertices.Count; i++)
        {
            //every 24, create new random
            if (i % 24 == 0)
                rot = Quaternion.Euler(new Vector3(Random.Range(-brickRotate, brickRotate), Random.Range(-brickRotate, brickRotate), Random.Range(-brickRotate, brickRotate)));
            //we are rotating from the origin here, if perhaps we do massive buildings, the origin may need changed
            //or we can rotate vertcies around the brick's centre point(work that out :p)
            //note : An alternative way of doing this can be found in Roof() Rotate a copy of the mesh vertices as you input
            //them in to the vertice array initially
            vertices[i] = rot * vertices[i];
        }

        //we do not this cube anymore
        Destroy(cube);

        //add triangles
        List<int> triangles = new List<int>();
        int[] cubeTriangles = cube.GetComponent<MeshFilter>().mesh.triangles;
        int cubeVertexCount = cube.GetComponent<MeshFilter>().mesh.vertexCount;
        //   Debug.Log(triangleSkip + "triskip");
        //for each brick being built
        //reverse the loop so it builds up the way when animating
        for (int i = vertices.Count / cubeVertexCount - 1; i >= 0; i--) //plus ones for the extra bricks at the end
        {
            //randomise material here

            //foreach cube template
            for (int j = 0; j < cubeTriangles.Length; j++)
            {
                //add each triangle
                triangles.Add(cubeTriangles[j] + (cubeVertexCount * i));


            }
            if (animate)
            {
                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                meshFilter.mesh = mesh;
                yield return new WaitForEndOfFrame();
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        yield return new WaitForEndOfFrame();


        yield break;
    }

    IEnumerator RoofWithParameters(Vector3 bottomLeft, Vector3 bottomRight, Vector3 backRight, bool rotate)
    {

        GameObject roof = new GameObject();

        if (rotate)
            roof.name = "RoofFront";
        else
            roof.name = "RoofBack";

        roof.transform.parent = transform;

        if (rotate)
            roof.transform.eulerAngles = new Vector3(0f, 180f, 0f);


        MeshRenderer meshRenderer = roof.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Resources.Load("Grey", typeof(Material)) as Material;
        MeshFilter meshFilter = roof.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        //create primitive
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh meshCube = cube.GetComponent<MeshFilter>().mesh;
        
        MeshCollider meshCollider = transform.GetChild(0).GetComponent<MeshCollider>();

        //find the length//building half at a time, divide by 2

        float length = Vector3.Distance(bottomLeft, bottomRight) * 0.5f;
        float breadth = Vector3.Distance(backRight, bottomRight) * 0.5f;   

        length = (float)System.Math.Round(length, 1);// Edited
        //resize a copy of the prefab mesh 
        Vector3[] tempVerts;
        tempVerts = meshCube.vertices;

        for (int i = 0; i < tempVerts.Length; i++)
        {
            //mesh vertices are built around the centre, push them so they build down from top left corner
            tempVerts[i].x += 0.5f;
            tempVerts[i].y += 0.5f;
            tempVerts[i].z += 0.5f;

            tempVerts[i].x *= tileSize;
            tempVerts[i].y *= tileSize / 4;
            tempVerts[i].z *= tileSize / 2;
        }

        //create co-ordinates for the roofpeak
        
        Vector3 topMiddleOfPeak = Vector3.Lerp(bottomRight, backRight, 0.5f);
        Vector3 topMiddleOfWall = Vector3.Lerp(bottomLeft, bottomRight, 0.5f);
        
        Vector3 directionToMiddleOfWall = topMiddleOfWall - bottomRight;
        topMiddleOfWall.y += heightFromStretchQuads;
        //move to middle of house
        topMiddleOfPeak += directionToMiddleOfWall;
        topMiddleOfPeak.y += (heightFromStretchQuads * 2);

        //move the up a bricksize. The bricks build upwards     
        topMiddleOfWall.y += brickSize;
        
        //Declare lists for temporary lists
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        //Take array sizes from our prefab
        int verticeCount = meshCube.vertexCount;
        int triangleCount = meshCube.triangles.Length;

        //work out how may tiles will be needed
        float amountAcross = length / tileSize;
        //round this number to an int
        amountAcross = Mathf.Round(amountAcross);
        //add a tile each side for an overhang at the side of the building
        //the offset has been pulled back a tile sieze to account for this
        amountAcross += 2;
        //create directional vector for builing across
        Vector3 acrossDir = (bottomRight-bottomLeft).normalized * tileSize;

        Vector3 shootDir = Quaternion.Euler(0f, 90f, 0f) * acrossDir;
//        Vector3 brickSizeRight = brickSize * buildDir * 0.5f;
        Quaternion faceBuildDir = Quaternion.LookRotation(-shootDir);

        //work out how many rows of tiles will be needed from the top of the roof to the top of the ceiling corner
        float slopeLength = Vector3.Distance(topMiddleOfWall, topMiddleOfPeak);
        float amountOfRows = slopeLength / (tileSize / 2f);
        amountOfRows = Mathf.Round(amountOfRows);        

        //create direction in which the tiles will build
        Vector3 slopeDir = (topMiddleOfWall - topMiddleOfPeak) / amountOfRows;

        //we can add some extra rows here to create an extension at side of build/overhang at side. Alternatively, could set top of Wall point as overhang
        amountAcross += 1;
        amountOfRows += 3;
      
        //find angle between slope and and the flat ceiling
        //this is used for the default angle the tile will be rotated before we add a random value
        tileTiltAngle = Vector3.Angle(slopeDir, -shootDir);

        //create roof mesh from singular cubemesh and the directional vectors we have prepared above
        for (int k = 0; k <= amountOfRows; k++)//down
        {
            //offsets are set to the middle for a symmetrical roof, so move startign back to create negative directions for the first half of the roof
            //e.g i = -amountAcross
            for (float i = -amountAcross; i < amountAcross; i++)//right //if we do equals here, we get an extra one to create an overhang
            {
                float random = Random.Range(0f, 20f);
                //add the vertices
                Vector3 v = Vector3.zero;

                for (int j = 0; j < verticeCount; j++)//each cube
                {

                    //for each tile, rotate, then move, then add to vertices array                    
                    Vector3 vertice = tempVerts[j];
                    //rotate
                    vertice = Quaternion.Euler(tileTiltAngle + random, 0f, 0f) * vertice;
                    vertice = faceBuildDir * vertice;

                    
                    vertice += (acrossDir * i) + (slopeDir * k);

                    //now move it up
                    vertice += topMiddleOfPeak;
                    vertices.Add(vertice);

                    v = vertice - tempVerts[j];
                }

            }
        }
        
        //Move points to origin
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 temp = vertices[i];

            temp.x -= transform.position.x;
            temp.y -= transform.position.y;
            temp.z -= transform.position.z;
            vertices[i] = temp;
        }

        //now set the local position to zero, this, plust the above lines moves the vertices in such a way
        //that the centre of the transform is the origin for the mesh :S
        roof.transform.localPosition = Vector3.zero;
        Destroy(cube);


        int[] trianglesTemplate = meshCube.triangles;
        int verts = meshCube.vertexCount;

        //create triangles
        for (int i = 0; i < vertices.Count / verts; i++)
        {
            for (int j = 0; j < trianglesTemplate.Length; j++)
            {
                triangles.Add(trianglesTemplate[j] + (verts * i));

            }

            if (animate)
            {

                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                meshFilter.mesh = mesh;
                yield return new WaitForEndOfFrame();
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        roofsBuilt++;
        if (roofsBuilt == 2)
        {
            //   StartCoroutine("Toblerone");
            //            StartCoroutine("Scale");
        }
        //  if (!testEnvironment)
        //      buildList.BuildingFinished();

        yield break;
    }

    IEnumerator Toblerone(Vector3 bottomLeft,Vector3 bottomRight,Vector3 backLeft,Vector3 backRight)
    {
        //points passed are on the ground. Raise them to the top of the wall

        bottomLeft.y += heightFromStretchQuads;
        bottomRight.y += heightFromStretchQuads;
        backLeft.y += heightFromStretchQuads;
        backRight.y += heightFromStretchQuads;
        Quaternion toRoad = GetComponent<StretchQuads>().toRoad;
        //stretch points to meet outside of exterior wall
        bottomLeft += (toRoad*Vector3.left) *brickSize;
        bottomLeft += (toRoad * Vector3.forward) * brickSize;

        bottomRight += (toRoad * Vector3.left) * brickSize;
        bottomRight += (toRoad * Vector3.back) * brickSize;

        backRight += (toRoad * Vector3.right) * brickSize;
        backRight += (toRoad * Vector3.back) * brickSize;

        backLeft += (toRoad * Vector3.right) * brickSize;
        backLeft += (toRoad * Vector3.forward) * brickSize;

        Vector3 roofPeakRight = Vector3.Lerp(bottomRight, backRight, 0.5f);
        roofPeakRight.y += (heightFromStretchQuads);

        Vector3 roofPeakLeft = Vector3.Lerp(bottomLeft, backLeft, 0.5f);
        roofPeakLeft.y += (heightFromStretchQuads);

        //move to origin
        bottomLeft -= transform.position;
        bottomRight -= transform.position;
        backLeft -= transform.position;
        backRight -= transform.position;
        roofPeakLeft -= transform.position;
        roofPeakRight -= transform.position;
        //now we have all out points,create a mesh with them
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        //create triangles for the front and back

        vertices.Add(bottomLeft);
        vertices.Add(backLeft);
        vertices.Add(roofPeakLeft);

        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(2);

        vertices.Add(bottomRight);
        vertices.Add(roofPeakRight);
        vertices.Add(backRight);

        triangles.Add(3);
        triangles.Add(4);
        triangles.Add(5);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        //create GameObject to put this new mesh in
        GameObject toblerone = new GameObject();
        toblerone.name = "Toblerone";
        toblerone.transform.parent = transform;
        //we moved the vertices, so we now need to centre this
        toblerone.transform.localPosition = Vector3.zero;
        //the rotation is in the mesh
        toblerone.transform.localRotation = Quaternion.identity;
        MeshFilter meshFilter = toblerone.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = toblerone.AddComponent<MeshRenderer>();
        meshRenderer.material = Resources.Load("RosePink", typeof(Material)) as Material;


        //let the build list know this building has stopped building
        // if(!testEnvironment)
        //      buildList.BuildingFinished();   


        // StartCoroutine("Scale");

        yield break;
    }

   
    void RotateToLookAtClosestPointOnRoad()
    {
        RaycastHit hit;
        Vector3 centre = transform.parent.parent.GetComponent<HouseCellInfo>().centroid;
        Physics.SphereCast(centre + (Vector3.up * 50), 20f, Vector3.down, out hit, 100f, LayerMask.GetMask("Road"));//if not hit, do dirt track?

        Vector3 point = hit.point;
        point.y = centre.y;//zero y difference
        Vector3 dir = point - centre;
        Quaternion face = Quaternion.LookRotation(dir);
        // Quaternion face = Quaternion.LookRotation(directionToDoor);

        //point Blue Arrow (Z) towards road
        transform.parent.rotation = face;

        //now roate depending on what way the door is facing
        Debug.Log(directionToDoor);
        if (directionToDoor == Vector3.forward)
        {
            //  Debug.Log("spin180 v3 fwd");
            transform.parent.rotation *= Quaternion.Euler(0, 180f, 0);
        }

        if (directionToDoor == Vector3.left)
        {
            Debug.Log("spin180 v3 left");
            transform.parent.rotation *= Quaternion.Euler(0, -90f, 0);
        }



        //   GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //   cube.transform.position = hit.point;
        //   cube.name = "nearest to middle";

        // PathFromFrontDoor();



        /////////////////////////////////////////////////////////////////

        //bug check beyond this. House paths > Fence Around Cell > Garden Feature - can stop buildlist
        
        //add house paths component to main combined mesh
    //    transform.parent.parent.gameObject.AddComponent<HousePaths>();

        //calling this here atm. sort pipeline
        buildList = GameObject.FindGameObjectWithTag("Code").GetComponent<BuildList>();
        buildList.BuildingFinished();


    }

    /// <summary>
    /// shoots rays in four directions, returns the first direction which does not hit another house block
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>
    Vector3 DirectionCheck(GameObject block)
    {
        Vector3 dir = Vector3.zero;
        Quaternion toRoad = GetComponent<StretchQuads>().toRoad;
        Vector3[] directions = new Vector3[]
        {
           toRoad * Vector3.forward,
           toRoad * Vector3.right,
           toRoad * Vector3.back,
           toRoad * Vector3.left
        };

        for (int i = 0; i < directions.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(block.GetComponent<MeshFilter>().mesh.bounds.center + block.transform.position + (directions[i] * 10),
                -directions[i],
                out hit,
                20f,
                LayerMask.GetMask("House")))
            {

                //if we find our own block
                if (hit.transform.gameObject == block)
                {
                    //  Debug.Log("Direction Check Found Clear Path");
                    dir = directions[i];
                    return dir;
                }
            }
        }
        if (dir == Vector3.zero)

            //we must find another room to add the door to, or look for a space not in the middle of the block?
            Debug.Log("Direction Check did not find clear path");

        return dir;
    }



}

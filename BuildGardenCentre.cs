using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuildGardenCentre : MonoBehaviour {

    
    public int maxFeaturesOnOneWall = 3;

    private int[] rearVertices;
    private int[] topVertices;
    private int[] bottomVertices;
    private int[] frontVertices;
    private int[] rightVertices;
    private int[] leftVertices;
    private int[] rearLeftVertices;
    private int[] rearRightVertices;

    public float brickSize;
    public float outsideBrickSize;
    public float heightFromStretchQuads;
    public float doorWidth = 0.8f;//how to set with grid size?

    private List<Vector3> gridPositions = new List<Vector3>();
    private List<Vector3> outsideEdgePositions = new List<Vector3>();

    private Material exteriorWallMaterial;
    public List<GameObject> houseBlocks = new List<GameObject>();

    //debug
    private Vector3 start;
    private Vector3 end;

    public bool buildWindows = false;
    public bool buildGrid = false;
    public bool buildDoors = false;
    public bool buildWalls = false;
    public bool startBuild = false;
    public int gap = 4;

    private float doorSize;
    private float bottomOfWindow;

    //also need a list of the doors 
    private List<GameObject> doors = new List<GameObject>();     
        void Awake()
    {
       // enabled = false;
    }
    void Start ()
    {
        brickSize = GetComponent<StretchQuads>().brickSize;// * 2;

        float outsideBrickSizeMultiplier = 2; //had previously set this on a slider in inspector. Removed support for outside brick size changing for now
        outsideBrickSize = brickSize * outsideBrickSizeMultiplier;
        heightFromStretchQuads = GetComponent<StretchQuads>().height;
        //Get Material for exterior walls
        exteriorWallMaterial = Resources.Load("RosePink") as Material;

        CreateVerticeArrays();
        GetHouseBlocks();
        GridFromCentre();

        //StartCoroutine("BuildOrder");

        //BuildOnWalls();

    }

    void BuildOnWalls()
    {

        //if front wall, transfrom.forward. 
        //choose how many features (x)
        //split wall in to x, get x positions
        //place door first, place windows hereafter


        //GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //c.transform.position = (transform.rotation * transform.FindChild("Room").GetComponent<MeshFilter>().mesh.vertices[2]) + transform.position;
        //point lies at bottom left corner of room
        Vector3 frontLeft = (transform.rotation * transform.Find("Room").GetComponent<MeshFilter>().mesh.vertices[2]) + transform.position;
        Vector3 frontRight = (transform.rotation * transform.Find("Room").GetComponent<MeshFilter>().mesh.vertices[1]) + transform.position;

        BuildOnWall(frontLeft, frontRight, true,false);
        CornerColumn(frontLeft, frontRight);
        

        frontLeft = (transform.rotation * transform.Find("Room").GetComponent<MeshFilter>().mesh.vertices[1]) + transform.position;
        frontRight = (transform.rotation * transform.Find("Room").GetComponent<MeshFilter>().mesh.vertices[0]) + transform.position;
        BuildOnWall(frontLeft, frontRight, false, false);
        CornerColumn(frontLeft, frontRight);

        frontLeft = (transform.rotation * transform.Find("Room").GetComponent<MeshFilter>().mesh.vertices[3]) + transform.position;
        frontRight = (transform.rotation * transform.Find("Room").GetComponent<MeshFilter>().mesh.vertices[2]) + transform.position;
        BuildOnWall(frontLeft, frontRight, false, false);
        CornerColumn(frontLeft, frontRight);

        frontLeft = (transform.rotation * transform.Find("Room").GetComponent<MeshFilter>().mesh.vertices[0]) + transform.position;
        frontRight = (transform.rotation * transform.Find("Room").GetComponent<MeshFilter>().mesh.vertices[3]) + transform.position;
        BuildOnWall(frontLeft, frontRight, true, false);
        CornerColumn(frontLeft, frontRight);

    }

    void BuildOnWall(Vector3 frontLeft, Vector3 frontRight,bool buildDoor,bool exterior)
    {
        //split wall up in to an amount of "features"
        int amountOfFeatures = Random.Range(1, maxFeaturesOnOneWall);

        //divide wall up in to this amount and create points for features
        float distance = Vector3.Distance(frontLeft, frontRight);
        //split vector by features        
        Vector3 dir = (frontRight - frontLeft) / amountOfFeatures;
        List<Vector3> limits = new List<Vector3>();
        for (int i = 0; i <= amountOfFeatures; i++)
        {
            Vector3 position = frontLeft + (dir * i);
            limits.Add(position);

        }

        List<Vector3> midPoints = new List<Vector3>();
        //get midPoints, these are the centre points for the features
        for (int i = 0; i < limits.Count - 1; i++)
        {
            Vector3 midPoint = Vector3.Lerp(limits[i], limits[i + 1], 0.5f);
            //  GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //  c.transform.position = midPoint;

            midPoints.Add(midPoint);
        }

        float width = distance / amountOfFeatures;
        
        
        Vector3 lookDirection = Quaternion.Euler(0, 90, 0) * (frontRight - frontLeft);

        if (buildDoor)
        {
            //select one of these midpoints to be a door
            int r = Random.Range(0, midPoints.Count);
            Vector3 doorPoint = midPoints[r];

            //remove from possible feature positions
            midPoints.RemoveAt(r);

            
            //create door
            GameObject door = DoorAtPosition(doorPoint,lookDirection);

            //create wall around door
         
            WallAroundDoor(doorPoint, width,lookDirection,false);
            WallAroundDoor(doorPoint, width, lookDirection, true);
        }
        //create windows at positions left over

        for (int i = 0; i < midPoints.Count; i++)
        {
            WindowAtPosition(midPoints[i], width,lookDirection);

            WallAroundWindow(midPoints[i], width,lookDirection,false);
            WallAroundWindow(midPoints[i], width, lookDirection, true);
        }
    }

    GameObject DoorAtPosition(Vector3 position,Vector3 lookDirection)
    {
        //prepare a mesh
        //create cube for manipulating
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(cube.GetComponent<BoxCollider>());

        cube.name = "Door";
        cube.transform.parent = transform.Find("Room");       
        cube.transform.position = position;
        cube.transform.rotation = Quaternion.LookRotation(lookDirection);


        Mesh mesh = cube.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        //scale 
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].z *= 0.1f; //depth?
            vertices[i] *= doorWidth; //needs to be a multiple of brickSize
        }

        //get door height
        //two thirds of full height of wall, rounded to brickSize grid
        float amountOfRows = heightFromStretchQuads / (brickSize);
        amountOfRows = Mathf.Round(amountOfRows);
        //two thirds of wall size
        doorSize = amountOfRows / 3;
        doorSize *= 2;
        doorSize = Mathf.Round(doorSize);

        //stretch vertices on the top of the cube to correct height
        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = doorSize * brickSize;
        }

        //bottom of room
        float bottom = ((transform.rotation * transform.Find("Room").GetComponent<MeshFilter>().mesh.vertices[2])).y;
        //make space for doorstep
        bottom += brickSize;
        //do the same for the bottom
        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = bottom;
        }

        mesh.vertices = vertices;

        cube.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("Blue", typeof(Material)) as Material;

        return cube;
    }

    void WindowAtPosition(Vector3 position, float width, Vector3 lookDirection)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(cube.GetComponent<BoxCollider>());
        cube.transform.parent = transform.Find("Room");
        cube.transform.position = position;
        cube.transform.rotation = Quaternion.LookRotation(lookDirection);

        //let's scale the mesh

        Mesh mesh = cube.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        //scale the whole mesh
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= width - (brickSize * 2);
            vertices[i].z *= 0.05f;
        }

        //get door height
        //two thirds of full height of wall, rounded to brickSize grid
        float amountOfRows = heightFromStretchQuads / (brickSize);
        amountOfRows = Mathf.Round(amountOfRows);
        //two thirds of wall size
        float doorSize = amountOfRows / 3;
        doorSize *= 2;
        doorSize = Mathf.Round(doorSize);

        //stretch vertices on the top of the cube to correct height
        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = doorSize * brickSize;
        }

        bottomOfWindow = amountOfRows / 4;
        bottomOfWindow = Mathf.Round(bottomOfWindow);
        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = bottomOfWindow * brickSize;
        }
        mesh.vertices = vertices;

        cube.AddComponent<MeshCollider>();
        cube.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("Glass", typeof(Material)) as Material;
    }

    void WallAroundDoor(Vector3 position, float segmentWidth, Vector3 lookDirection,bool exterior)
    {
        //build space under door to width of one segment
        GameObject belowDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(belowDoor.GetComponent<BoxCollider>());
        belowDoor.transform.position = position;      
        belowDoor.transform.parent = transform.Find("Room");
        belowDoor.transform.rotation = Quaternion.LookRotation(lookDirection);


        Vector3[] vertices = belowDoor.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = 0;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = brickSize;
        }

        belowDoor.GetComponent<MeshFilter>().mesh.vertices = vertices;
        belowDoor.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        belowDoor.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            belowDoor.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            belowDoor.transform.position += lookDirection.normalized * brickSize * 0.5f;
            belowDoor.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        //above door

        //build space under door to width of one segment
        GameObject aboveDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(aboveDoor.GetComponent<BoxCollider>());
        aboveDoor.transform.position = position;
       

        aboveDoor.transform.parent = transform.Find("Room");
        aboveDoor.transform.rotation = Quaternion.LookRotation(lookDirection);

        vertices = aboveDoor.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = doorSize * brickSize;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = heightFromStretchQuads;
        }

        aboveDoor.GetComponent<MeshFilter>().mesh.vertices = vertices;
        aboveDoor.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        aboveDoor.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            aboveDoor.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            aboveDoor.transform.position += lookDirection.normalized * brickSize * 0.5f;
            aboveDoor.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

            //to each side

            //build space under door to width of one segment
            GameObject rightSideOfDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(rightSideOfDoor.GetComponent<BoxCollider>());
        rightSideOfDoor.transform.position = position;      
        rightSideOfDoor.transform.parent = transform.Find("Room");
        rightSideOfDoor.transform.rotation = Quaternion.LookRotation(lookDirection);

        vertices = rightSideOfDoor.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = brickSize;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = doorSize * brickSize;
        }

        for (int i = 0; i < rightVertices.Length; i++)
        {

            vertices[rightVertices[i]].x = - doorWidth / 2;
        }

        rightSideOfDoor.GetComponent<MeshFilter>().mesh.vertices = vertices;
        rightSideOfDoor.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        rightSideOfDoor.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            rightSideOfDoor.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            rightSideOfDoor.transform.position += lookDirection.normalized * brickSize * 0.5f;
            rightSideOfDoor.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        GameObject leftSideOfDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(leftSideOfDoor.GetComponent<BoxCollider>());
        leftSideOfDoor.transform.position = position;      
        leftSideOfDoor.transform.parent = transform.Find("Room");
        leftSideOfDoor.transform.rotation = Quaternion.LookRotation(lookDirection);

        vertices = leftSideOfDoor.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = brickSize;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = doorSize * brickSize;
        }

        for (int i = 0; i < leftVertices.Length; i++)
        {

            vertices[leftVertices[i]].x = doorWidth / 2;
        }

        leftSideOfDoor.GetComponent<MeshFilter>().mesh.vertices = vertices;
        leftSideOfDoor.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        leftSideOfDoor.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            leftSideOfDoor.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            leftSideOfDoor.transform.position += lookDirection.normalized * brickSize * 0.5f;
            leftSideOfDoor.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }
    }

    void WallAroundWindow(Vector3 position,float segmentWidth,Vector3 lookDirection,bool exterior)
    {
        //build space under width of one segment
        GameObject below = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(below.GetComponent<BoxCollider>());
        below.transform.position = position;
        below.transform.parent = transform.Find("Room");
        below.transform.rotation = Quaternion.LookRotation(lookDirection);

        Vector3[] vertices = below.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = 0;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = bottomOfWindow* brickSize;
        }

        below.GetComponent<MeshFilter>().mesh.vertices = vertices;
        below.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        below.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            below.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            below.transform.position += lookDirection.normalized * brickSize * 0.5f;
            below.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        //above
        GameObject above = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(above.GetComponent<BoxCollider>());
        above.transform.position = position;
        above.transform.parent = transform.Find("Room");
        above.transform.rotation = Quaternion.LookRotation(lookDirection);

        vertices = above.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = doorSize * brickSize;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = heightFromStretchQuads;
        }

        above.GetComponent<MeshFilter>().mesh.vertices = vertices;
        above.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        above.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            above.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            above.transform.position += lookDirection.normalized * brickSize * 0.5f;
            above.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        //side
        GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(left.GetComponent<BoxCollider>());
        left.transform.position = position;
        left.transform.parent = transform.Find("Room");
        left.transform.rotation = Quaternion.LookRotation(lookDirection);

        vertices = left.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = bottomOfWindow*brickSize;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = doorSize * brickSize;
        }

        for (int i = 0; i < leftVertices.Length; i++)
        {

            vertices[leftVertices[i]].x = (segmentWidth/2) - brickSize ;
        }

        left.GetComponent<MeshFilter>().mesh.vertices = vertices;
        left.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        left.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            left.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            left.transform.position += lookDirection.normalized * brickSize * 0.5f;
            left.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        //side
        GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(right.GetComponent<BoxCollider>());
        right.transform.position = position;
        right.transform.parent = transform.Find("Room");
        right.transform.rotation = Quaternion.LookRotation(lookDirection);

        vertices = right.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = bottomOfWindow * brickSize;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = doorSize * brickSize;
        }

        for (int i = 0; i < rightVertices.Length; i++)
        {

            vertices[rightVertices[i]].x = -(segmentWidth / 2) + brickSize;
        }

        right.GetComponent<MeshFilter>().mesh.vertices = vertices;
        right.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        right.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            right.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            right.transform.position += lookDirection.normalized * brickSize * 0.5f;
            right.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }
    }

    void CornerColumn(Vector3 frontLeft, Vector3 frontRight)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(cube.GetComponent<BoxCollider>());
        cube.transform.parent = transform.Find("Room");
        cube.transform.position = frontLeft;
        
        cube.transform.rotation = Quaternion.LookRotation(Quaternion.Euler(0, 180, 0)*(frontRight - frontLeft) );

        cube.transform.position -= cube.transform.right * brickSize * 0.5f;
        cube.transform.position += cube.transform.forward * brickSize * 0.5f;

        Mesh mesh = cube.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        //scale
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] *= brickSize;
        }

        //stretch
        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = heightFromStretchQuads;
        }
        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = 0f;
        }

        mesh.vertices = vertices;

        cube.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
    }

    
    IEnumerator BuildOrder()
    {

        


        Doors();
        yield return new WaitForEndOfFrame();
       

        StartCoroutine("Windows");

        yield break;
    }
    
    void CreateVerticeArrays()
    {
       frontVertices = new int[]
       {
            0,1,2,3,
            8,9,
            13,14,
            16,17,
            22,23
       };

       rearVertices = new int[]
       {
            4,5,6,7,
            10,11,
            12,15,
            18,19,20,21

       };

       rightVertices = new int[]
       {
            0,2,
            4,6,
            8,10,
            12,13,
            20,21,22,23
       };

       leftVertices = new int[]
       {
            1,3,
            5,7,
            9,11,
            14,15,
            16,17,18,19
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
        rearLeftVertices = new int[]
       {
           5,7,11,15,18,19
       };
        rearRightVertices = new int[]
       {
            4,6,10,12,20,21
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

            //to save on layers, move the rooms/quads which were previously a feature to just being a house
            //we will make the windows,doors etc, features in this step

           transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("House");
           houseBlocks.Add(transform.GetChild(i).gameObject);
        }
    }
    void GridFromCentre()
    {
        //box collider needed to figure out bounds check//maybe add whenn building room instead of here
        transform.GetChild(0).gameObject.AddComponent<BoxCollider>();

        float gridSize = GetComponent<StretchQuads>().brickSize;
        Vector3 centreOfRoom = transform.GetChild(0).GetComponent<MeshRenderer>().bounds.center;
        float widthOfRoom = transform.GetChild(0).GetComponent<BoxCollider>().size.x;
        float lengthOfRoom = transform.GetChild(0).GetComponent<BoxCollider>().size.z;

        for (float j = 0; j < lengthOfRoom / 2; j += brickSize)
        {
            //build out from centre both directions
            for (float i = 0; i < widthOfRoom / 2; i += brickSize)
            {
                Vector3 p = centreOfRoom + (transform.right * i) + (transform.forward*j);
                gridPositions.Add(p);

                //save edge postions in other list
                if (j > lengthOfRoom*0.5f - brickSize || i > (widthOfRoom * 0.5f - brickSize))
                    outsideEdgePositions.Add(p);

            }
            //skip middle point, loop above added it
            for (float i = brickSize; i < widthOfRoom / 2; i += brickSize)
            {
                Vector3 p = centreOfRoom + (-transform.right * i) + (transform.forward * j);
                gridPositions.Add(p);

                //save edge postions in other list
                if (j > lengthOfRoom * 0.5f - brickSize || i > (widthOfRoom * 0.5f - brickSize))
                    outsideEdgePositions.Add(p);
            }
        }
        for (float j = brickSize; j < lengthOfRoom / 2; j += brickSize)
        {
            //build out from centre both directions
            for (float i = 0; i < widthOfRoom / 2; i += brickSize)
            {
                Vector3 p = centreOfRoom + (transform.right * i) - (transform.forward * j);
                gridPositions.Add(p);

                //save edge postions in other list
                if (j > lengthOfRoom * 0.5f - brickSize || i > (widthOfRoom * 0.5f - brickSize))
                    outsideEdgePositions.Add(p);

            }
            //skip middle point, loop above added it
            for (float i = brickSize; i < widthOfRoom / 2; i += brickSize)
            {
                Vector3 p = centreOfRoom + (-transform.right * i) - (transform.forward * j);
                gridPositions.Add(p);

                //save edge postions in other list
                if (j > lengthOfRoom * 0.5f - brickSize || i > (widthOfRoom * 0.5f - brickSize))
                    outsideEdgePositions.Add(p);
            }
        }

        foreach(Vector3 v3 in outsideEdgePositions )
        {
        
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = v3;
            c.transform.rotation = transform.rotation;
            c.transform.localScale *= brickSize * 0.5f;
            c.transform.parent = transform;
        
        }
    }
    void GridFromSide()
    {

    }
    void Doors()
    {
        bool backDoor = false;
        StartCoroutine("Door",(backDoor));
        backDoor = true;
        StartCoroutine("Door", (backDoor));

    }
    IEnumerator Door(bool backDoor)
    {
        //prepare a mesh
        //create cube for manipulating
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(cube.GetComponent<BoxCollider>());

        //place the front door at a point on the wall facing the main road

        //choose a random point from the grid points created in Grid()
        int r = Random.Range(0, outsideEdgePositions.Count);
        Vector3 cellPoint = outsideEdgePositions[r];

        Vector3 roadPoint = cellPoint + (transform.forward * 10);

        //find a point on the wall nearest the road. Wall nearest the road is the wall facing transform.forward

        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c.transform.position = (transform.rotation* transform.Find("Room").GetComponent<MeshFilter>().mesh.vertices[1]) + transform.position; 

        /*
        GameObject debug2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debug2.transform.position = hit.point;
        debug2.name = "debug";
        */

        //debug
        start = cellPoint;
        end = roadPoint;
        roadPoint.y = cellPoint.y;

        Vector3 shootDir = -transform.forward;
        
        if (backDoor)
        {
            //flip direction if we are shooting for backdoor
            shootDir = -shootDir;

            //now we need to make the "road point" a point behind the house
            roadPoint = cellPoint - shootDir;

        }

        float length = 100f;

        //now raycast for the wall, check for each edge (width) of the door, to ensure we dont have the door hanging off the side of the building

        //make points 1 door width to the side of the central point we are working from
        Vector3 point1 = roadPoint + (Quaternion.Euler(0, 90, 0) * (shootDir.normalized * doorWidth));
        Vector3 point2 = roadPoint + (Quaternion.Euler(0, -90, 0) * (shootDir.normalized * doorWidth));

     

        bool bothHit = false;
        RaycastHit hit;
        RaycastHit hit2 = new RaycastHit();
        if (Physics.Raycast(point1, shootDir, out hit, length, LayerMask.GetMask("House")))
        {
            if (Physics.Raycast(point2, shootDir, out hit2, length, LayerMask.GetMask("House")))
            {
                bothHit = true;
            }
        }

        if (bothHit == false)
        {
            //start again, choose different cell
            Debug.Log("Bad house cell for door, trying again");
            Destroy(cube);
            yield return new WaitForEndOfFrame();
            StartCoroutine("Door",(backDoor));
            
            yield break;
        }

        //get centre point between two rays
        Vector3 midPoint = Vector3.Lerp(hit.point, hit2.point, 0.5f);   

        Mesh mesh = cube.GetComponent<MeshFilter>().mesh;
        //alter mesh
        Vector3[] tempVerts;
        tempVerts = mesh.vertices;
        float amountOfRows = heightFromStretchQuads / (brickSize);
        amountOfRows = Mathf.Round(amountOfRows);
        //two thirds of wall size
        float doorSize = amountOfRows / 3;
        doorSize *= 2;
        doorSize = Mathf.Round(doorSize);
   
        //scale 
        for (int i = 0; i < tempVerts.Length; i++)
        {
            tempVerts[i].z *= 0.1f; //depth?
            tempVerts[i] *= doorWidth; //needs to be a multiple of brickSize

        }

        float top = heightFromStretchQuads + (brickSize);
        float bottom = top - ((brickSize) * amountOfRows);
        bottom -= brickSize;

        cube.transform.position = new Vector3(midPoint.x, transform.position.y, midPoint.z);
        //let's use the arrays setup in Start() to stretch the vertices around
        //imagine pulling multiple vertices in blender

        //move the bottom points to one brick size up
        for (int i = 0; i < bottomVertices.Length; i++)
        {
            tempVerts[topVertices[i]].y = (doorSize * (brickSize));
            tempVerts[bottomVertices[i]].y = bottom; //this is always one above bottom row - what we need!
        }

        mesh.vertices = tempVerts;
        cube.GetComponent<MeshFilter>().mesh = mesh;
        
        if (backDoor == false)
            cube.name = "Door";
        else
            cube.name = "BackDoor";

        cube.tag = "Door";
        cube.transform.parent = transform.GetChild(0).transform;
        cube.layer = 25;

        //zero the local rotation so it sits with it's parent's rotation

        //TODO      spinning door isnt working, this all needs to jigged
        if (backDoor)
            cube.transform.localRotation = Quaternion.identity;//  Quaternion.Euler(0, 180, 0);
        else
            cube.transform.localRotation = Quaternion.identity;

        cube.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("Blue", typeof(Material)) as Material;

        BoxCollider box = cube.AddComponent<BoxCollider>();
        box.size = new Vector3(doorWidth, doorSize*brickSize, 0.1f); //depth?
        box.center += new Vector3(0, 0.5f * doorSize * brickSize,0);
        //box.center += new Vector3(0,brickSize*0.5f,0);
        //save to a list so the windows know to avoid
        //MOVE SLIGHTLY /// TO DO change how door size works!
        cube.transform.position += new Vector3(0, brickSize*0.5f, 0);
        doors.Add(cube);

    }

    IEnumerator Windows()
    {
        //possible window sites are, to each side of the front/back door, and on each side of the house
        
        foreach (Vector3 v3 in outsideEdgePositions)
        {
            StartCoroutine("WindowWithRaycast", (v3));
            yield return new WaitForEndOfFrame();
        }

        //now windows have been built, we can build walls around the features
        StartCoroutine("InteriorWalls");

    }
    IEnumerator WindowWithRaycast(Vector3 point)
    {
        //possible window points, return with the direction the raycast hit from, so we know which direction to stretch the window
        List<PosAndDir> posAndDirs = PositionsAndDirections(point);

        //multiplies bricksSize to create a lovely gap between features
        
        foreach (PosAndDir p in posAndDirs)
        {
            // now search how far to each side we can stretch the window
            //now, let's stretch the window, to create large lovely windows
            Vector3 leftDir = Quaternion.Euler(0f, -90f, 0f) * p.dir;
            leftDir *= brickSize;
            Vector3 rightDir = Quaternion.Euler(0f, 90f, 0f) * p.dir;
            rightDir *= brickSize;

            bool stop = false;
            int leftCounter = 0;
            while (stop == false)
            {
                if (!Physics.Raycast(p.pos + p.dir + (leftDir * leftCounter), -p.dir, 2f, LayerMask.GetMask("House")))
                {
                    stop = true;
                    leftCounter -= gap;
                }
                else if (Physics.Raycast(p.pos + p.dir + (leftDir * leftCounter), -p.dir, 2f, LayerMask.GetMask("HouseFeature")))
                {
                    stop = true;
                    leftCounter -= gap;

                }
                else
                    leftCounter++;
            }

            float distanceLeft = leftCounter * brickSize;

            //now do for right
            //reset
            stop = false;
            int rightCounter = 0;
            while (stop == false)
            {
                if (!Physics.Raycast(p.pos + p.dir + (rightDir * rightCounter), -p.dir, 2f, LayerMask.GetMask("House")))
                {
                    stop = true;
                    rightCounter -= gap;
                }
                else if (Physics.Raycast(p.pos + p.dir + (rightDir * rightCounter), -p.dir, 2f, LayerMask.GetMask("HouseFeature")))
                {
                    stop = true;
                    rightCounter -= gap;

                }
                else
                    rightCounter++;


            }

            float distanceRight = rightCounter * brickSize;

            //instantiate window object
            GameObject cube = new GameObject();
            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Window";
            cube.transform.position = p.pos;
            cube.transform.rotation = Quaternion.LookRotation(p.dir);
            cube.transform.parent = transform;
            cube.layer = LayerMask.NameToLayer("HouseFeature");

            //stretch vertices  

            float windowHeight = 1.2f;//needs to be a multiple of bricksize i believe
            float windowDepth = 0.01f;

            Vector3[] vertices = cube.transform.GetComponent<MeshFilter>().mesh.vertices;
            //grab all right hand vertices and pull them over

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].x *= 0;
                vertices[i].y *= windowHeight;
                vertices[i].y -= brickSize * 0.5f;
                vertices[i].z *= windowDepth;
            }
            for (int j = 0; j < rightVertices.Length; j++)
            {
                vertices[rightVertices[j]] += distanceRight * Vector3.right;

                //make small adjustment here, stretch half a bricksize across, this lines it up with the grid the interior walls are built with - not sure if it should be done here
                //or if the grid the rays are shooting from in InteriorWalls is wrong. It is simpler to fix here atm.
                //vertices[rightVertices[j]].x += brickSize / 2;

            }
            for (int j = 0; j < leftVertices.Length; j++)
            {
                vertices[leftVertices[j]] += distanceLeft * Vector3.left;
            }

            //asign to mesh
            cube.GetComponent<MeshFilter>().mesh.vertices = vertices;

            //switch the box for a mesh collider. Function below figured out how to apply box size to counters above. Can't get it to work atm. needed? performance?
            Destroy(cube.GetComponent<BoxCollider>());
            cube.AddComponent<MeshCollider>();
        }
        yield break;
    }
    IEnumerator InteriorWalls()
    {

        //do last wall
        GameObject room = transform.GetChild(0).gameObject;
        Vector3 bottomRight = (room.transform.rotation * room.GetComponent<MeshFilter>().mesh.vertices[3]) + room.transform.position;
        Vector3 bottomLeft = (room.transform.rotation * room.GetComponent<MeshFilter>().mesh.vertices[0]) + room.transform.position;
        StartCoroutine(InteriorWall(room, bottomLeft, bottomRight));

        //do first three walls for room
        for (int i = 0; i < 3; i++)
        {
            bottomRight = (room.transform.rotation * room.GetComponent<MeshFilter>().mesh.vertices[i]) + room.transform.position;
            bottomLeft = (room.transform.rotation * room.GetComponent<MeshFilter>().mesh.vertices[i + 1]) + room.transform.position;
            StartCoroutine(InteriorWall(room, bottomLeft, bottomRight));
        }
        yield break;
    }
    IEnumerator InteriorWall(GameObject block, Vector3 bottomLeft, Vector3 bottomRight)
    {

        //everything is built on a grid, so raycast through these grids from each side of the house and place a brick
        //if we hit a house wall, e.g not a feature(door/window etc)

        //currentyl rotatinb mesh vertices inside mesh, possibly should move rotations on to transform, and build mesh on simpler vectors

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
        float amountOfBricksX = length / brickSize;
        //remove two so we dont overlap at each corner
        amountOfBricksX -= 1f;
        float amountOfBricksY = height / (brickSize);

        Quaternion toRoad = GetComponent<StretchQuads>().toRoad;
        //build in left direction
        Vector3 buildDir = ((bottomLeft - bottomRight).normalized);
        //move starting point over a bricksize and half, the wall which builds after it will fill in the gap behind - no overlaps
        //bottomRight += (brickSize*1.5f) * buildDir;
        Vector3 shootDir = Quaternion.Euler(0f, 90f, 0f) * buildDir;
        Vector3 brickSizeRight = brickSize * buildDir;
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
                //test if the next ( j+ 1) space is ok to build in
                Vector3 initialPos = bottomRight + (brickSizeRight * (j + 1 ))  + (brickSizeUp * i);
                //add a little for innacuracy
                Vector3 shootFrom = initialPos + buildDir * brickSize *0.1f;
                //move it out from the wall a little
                shootFrom -= shootDir * brickSize * 2;
                //this wall needs pushed over a little to create a nice complete wall with no overlap
                //- this is the only wall that needs adjusted - probably just a quirk in the way i've mapped the offsets
               // shootFrom -= shootDir * brickSize;
                //shootFrom += (toRoad * Vector3.back) * outsideBrickSize;
                //move it up alittle, just under half a brick size, the y co=ord is in the middle of the brick
                shootFrom.y += brickSize *0.4f;

            //    GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
           //     c.transform.localScale *= 0.1f;
           //     c.transform.position = shootFrom;
                yield return new WaitForEndOfFrame();

                //     
                LayerMask lm = LayerMask.GetMask("House", "HouseFeature");
                RaycastHit hit;
                if (Physics.Raycast(shootFrom, shootDir, out hit, brickSize*4, lm))
                {
                    if (hit.transform.gameObject.layer == 24)
                    {
                        if (j == 0 || lastOneWasAFeature)
                        {
                            GameObject brick = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            Destroy(brick.GetComponent<BoxCollider>());// Instantiate(cubePrefab, hit.point, Quaternion.identity) as GameObject;
                            brick.transform.position = initialPos;
                            //moving brick inside the mesh
                            brick.transform.position += Quaternion.Euler(0, 90, 0) * (buildDir * brickSize * 0.5f);
                            //move back in to corner
                            brick.transform.position -= buildDir * brickSize * 0.5f;

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
                            //if last one was a feature, we need to push the rear vertices on a little
                            if (lastOneWasAFeature)
                            {
                                //these are the vertices we are NOT stretching usually
                                vertices[1] += buildDir;
                                vertices[5] += buildDir;

                                vertices[7] += buildDir;
                                vertices[9] += buildDir;

                                vertices[11] += buildDir;
                                vertices[14] += buildDir;

                                vertices[15] += buildDir;
                                vertices[16] += buildDir;

                                vertices[17] += buildDir;
                                vertices[18] += buildDir;

                                vertices[19] += buildDir;
                                vertices[3] += buildDir;
                            }



                            mesh.vertices = vertices;
                            mesh.RecalculateNormals();
                            mesh.RecalculateBounds();


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

                            vertices[0] += dir;// * 0.5f;   //half of one here because the transform is scaled to bricksize
                            vertices[2] += dir;// * 0.5f;

                            vertices[4] += dir;// * 0.5f;
                            vertices[6] += dir;// * 0.5f;

                            vertices[8] += dir;// * 0.5f;
                            vertices[10] += dir;// * 0.5f;

                            vertices[12] += dir;// * 0.5f;
                            vertices[13] += dir;// * 0.5f;

                            vertices[20] += dir;// * 0.5f;
                            vertices[21] += dir;// * 0.5f;

                            vertices[22] += dir;// * 0.5f;
                            vertices[23] += dir;// * 0.5f;

                            //give the mesh these temp values
                            mesh.vertices = vertices;

                        }

                        lastOneWasAFeature = false;
                    }
                    if (hit.transform.gameObject.layer == 25)
                    {
                        //we have hit a feature!
                        if(!lastOneWasAFeature)
                        {
                            //since we scheck one ahead, stretch this mesh over a little, only do it once (if last one wasnt' a feature

                            Vector3[] vertices = mesh.vertices;

                            //this does not seem to be in a pattern
                            //I'm using unity's box for the prefab - perhaps it is this way for optimisation
                            //of the triangle patterns

                            Vector3 dir = buildDir;
                            //grab the ends

                            vertices[0] += dir;// * 0.5f;   //half of one here because the transform is scaled to bricksize
                            vertices[2] += dir;// * 0.5f;

                            vertices[4] += dir;// * 0.5f;
                            vertices[6] += dir;// * 0.5f;

                            vertices[8] += dir;// * 0.5f;
                            vertices[10] += dir;// * 0.5f;

                            vertices[12] += dir;// * 0.5f;
                            vertices[13] += dir;// * 0.5f;

                            vertices[20] += dir;// * 0.5f;
                            vertices[21] += dir;// * 0.5f;

                            vertices[22] += dir;// * 0.5f;
                            vertices[23] += dir;// * 0.5f;

                            //give the mesh these temp values
                            mesh.vertices = vertices;
                        }

                        //this will force a new mesh to be made                        
                        lastOneWasAFeature = true;

                        //grab the end vertices from the current brick and pull them to the end of the building

                       
                    }
                }
            }

            //the end of the wall needs pulled over half a brick size, then we are complete
            /*
            Vector3[] verticesEnd = mesh.vertices;
            Vector3 dirEnd = buildDir;
            //grab the ends

            verticesEnd[0] += dirEnd * 0.5f;   //half of one here because the transform is scaled to bricksize
            verticesEnd[2] += dirEnd * 0.5f;

            verticesEnd[4] += dirEnd * 0.5f;
            verticesEnd[6] += dirEnd * 0.5f;

            verticesEnd[8] += dirEnd * 0.5f;
            verticesEnd[10] += dirEnd * 0.5f;

            verticesEnd[12] += dirEnd * 0.5f;
            verticesEnd[13] += dirEnd * 0.5f;

            verticesEnd[20] += dirEnd * 0.5f;
            verticesEnd[21] += dirEnd * 0.5f;

            verticesEnd[22] += dirEnd * 0.5f;
            verticesEnd[23] += dirEnd * 0.5f;

            mesh.vertices = verticesEnd;
            */
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
        yield break;
    }

    

    List<PosAndDir> PositionsAndDirections(Vector3 point)
    {
        Quaternion toRoad = transform.rotation;// GetComponent<StretchQuads>().toRoad;

        //to find where the walls are around this point, we raycast towards the grid point we were passed. Any hits, and we can put a window here
        //move round 90 degrees at a time, multiplying by the rotation which the building is built with

        //we will need to save possible positions to a list, we also need to save the rotation this position is facing        
        List<PosAndDir> posAndDirs = new List<PosAndDir>();

        //grab collider for check
        MeshCollider mc = transform.GetChild(0).GetComponent<MeshCollider>();
        BoxCollider box = transform.GetChild(0).gameObject.GetComponent<BoxCollider>();
        for (int i = 0; i < 360; i += 90)
        {
            Vector3 shootDir = Quaternion.Euler(0, i, 0) * Vector3.right;
            shootDir = toRoad * shootDir;
            shootDir.Normalize();
            float distance = 1f;//this should be governed by cell size, which is set in Houses For Voronoi 
            shootDir *= distance;

            //set the point outside the cell to shoot towards the collider. Rays cant hit the inside of a collider
            Vector3 shootFrom = point + shootDir;
            float height = transform.GetChild(0).GetComponent<MeshRenderer>().bounds.center.y;
            //doors are slightly off the ground
            shootFrom.y = height;

            RaycastHit hit;

            //before we start raycasting we, need to seperate the walls from the existent house feature (layers)
            //change Room to House from House Feature. Doors are "HouseFeature"
            transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("House");

            //check there are no features, doors, windows, already built here
            if (Physics.Raycast(shootFrom, -shootDir, out hit, distance * 2, LayerMask.GetMask("HouseFeature")))
            {
                //if we hit a feature,a bort this point, and move to the next
                
                continue;
            }
            //also check to each side
            Vector3 move = (Quaternion.Euler(0, 90, 0) * shootDir) * brickSize*gap;
            if (Physics.Raycast(shootFrom + move, -shootDir, out hit, distance * 2, LayerMask.GetMask("HouseFeature")))
            {
               
                continue;
            }
            if (Physics.Raycast(shootFrom - move, -shootDir, out hit, distance * 2, LayerMask.GetMask("HouseFeature")))
            {
               
                continue;
            }
            int hitHouse = 0;
            
            if (Physics.Raycast(shootFrom, -shootDir, out hit, distance * 2, LayerMask.GetMask("House")))
            {
               
                hitHouse++;
            }
            Vector3 centralHit = hit.point;
            if (Physics.Raycast(shootFrom + move, -shootDir, out hit, distance * 2, LayerMask.GetMask("House")))
            {
                hitHouse++;
            }
            if (Physics.Raycast(shootFrom - move, -shootDir, out hit, distance * 2, LayerMask.GetMask("House")))
            {
                hitHouse++;
            }

            //if all rays hit the house
            if (hitHouse == 3)
            {
                PosAndDir pAndR = new PosAndDir();
                pAndR.pos = centralHit;
                pAndR.dir = shootDir.normalized;
                posAndDirs.Add(pAndR);

                
            }
        }

        return posAndDirs;
    }
    
    class PosAndDir
    {
        public Vector3 pos;
        public Vector3 dir;
    }

    bool PointInOABB(Vector3 point, BoxCollider box)
    {
        point = box.transform.InverseTransformPoint(point) - box.center;

        float halfX = (box.size.x * 0.5f);
        float halfY = (box.size.y * 0.5f);
        float halfZ = (box.size.z * 0.5f);
        if (point.x < halfX && point.x > -halfX &&
           point.y < halfY && point.y > -halfY &&
           point.z < halfZ && point.z > -halfZ)
            return true;
        else
            return false;
    }

    void Update()
    {
        Debug.DrawLine(start, end);

        if(buildWindows)
        {
           // Destroy(transform.GetChild(0).gameObject.GetComponent<BoxCollider>());
            doors = new List<GameObject>();
            doors.Add(transform.GetChild(0).GetChild(0).gameObject);
            doors.Add(transform.GetChild(0).GetChild(1).gameObject);
          //  posAndDirs = new List<PosAndDir>();
            buildWindows = false;
            StartCoroutine("Windows");
        }

        if(buildGrid)
        {
            outsideEdgePositions = new List<Vector3>();
            GridFromCentre();
            buildGrid = false;
        }
        if(buildDoors)
        {
            Doors();
            buildDoors = false;
        }
        if(buildWalls)
        {
            buildWalls = false;
            StartCoroutine("InteriorWalls");
        }
        if(startBuild)
        {
            GameObject room = transform.Find("Room").gameObject;
            for (int i = 0; i < room.transform.childCount; i++)
            {
                Destroy(room.transform.GetChild(i).gameObject);
            }

            startBuild = false;
            BuildOnWalls();
        
        }
    }

}

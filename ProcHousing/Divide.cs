using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Divide : HouseBuilder {

    public Vector3 targetPosition = Vector3.zero;
    public Quaternion targetRotation = Quaternion.identity;
    public float plotX = 0f;
    public float plotZ = 0f;

    public int floors = 2;
    public float storeyHeight = 3f;

    public float doorHeight = 2f;
    public float doorWidth = 1f;
    public float windowHeight = 1.5f;
    public float windowWidth = 2f;
    public int roomAmount;
    public bool randomAmountOfRooms = false;
    private List<RoomsAndSizes> roomsAndSizes = new List<RoomsAndSizes>();
    private List<RoomAndEdge> roomsAndEdges = new List<RoomAndEdge>();
    private List<RoomAndEdge> missingWalls = new List<RoomAndEdge>();
    private List<RoomAndEdge> exteriorWalls = new List<RoomAndEdge>();

    //savign in case we need to move stairs from another script
    public List<GameObject> adjacentRooms = new List<GameObject>();
    //list of shred of points between each room
    public List<List<TargetAndSharedPoints>> listOfRoomsAndSharedPoints = new List<List<TargetAndSharedPoints>>();
    private Vector3 centreOfPlot;
    public Mesh plotMesh;
    public BoxCollider plotBox;
    public GameObject plot;
    public int objectsBuilt = 0;
    public int attemptedObjects = 0;
    public int complexityOfRoom = 0;
    public int safety = 0;
    public bool fillRooms;

    private List<GameObject> doorsBuilt = new List<GameObject>();
    public List<List<GameObject>> interiorAssetsByRoom = new List<List<GameObject>>();
    public List<GameObject> exteriorAssets = new List<GameObject>();

    // public List<GameObject> objectsToBuild = new List<GameObject>();


    //using vars here to debug with coroutines
    public List<Vector3> possible = new List<Vector3>();
    public List<Vector3> possibleOriginals = new List<Vector3>();
    private List<Vector3> corners = new List<Vector3>();
    private IndexAndDirection indexAndDirection = new IndexAndDirection();
    private List<PositionAndDirection> debugLines = new List<PositionAndDirection>();
    private List<PositionAndDirection> debugLinesAngles = new List<PositionAndDirection>();

    public bool doDebugLines = false;


    //stair collider
    public GameObject stairCollider;
    //list 1 is optimal, list 2 has smaller/less items
    private List<ObjectAndSize> objectsBathroom1 = new List<ObjectAndSize>();
    private List<ObjectAndSize> objectsBathroom2 = new List<ObjectAndSize>();
    private List<ObjectAndSize> objectsBedroom1 = new List<ObjectAndSize>();
    private List<ObjectAndSize> objectsBedroom2 = new List<ObjectAndSize>();
    private List<ObjectAndSize> objectsKitchen1 = new List<ObjectAndSize>();
    private List<ObjectAndSize> objectsKitchen2 = new List<ObjectAndSize>();
    private List<ObjectAndSize> objectsLivingroom= new List<ObjectAndSize>();
    //public List<Edge> boundaryPath;


    //house paramters - consistent looks throughout this house
    public float skirtingDepth = 0f;
    public float skirtingHeight = 0f;
    public float interiorDoorPatternOffset = 0f;
    public float interiorDoorRimWidth = 0f;
    public float interiorDoorBorderSize = 0;
    public int interiorDoorRows = 0;
    public List<int> interiorDoorPattern = new List<int>();

    //bool to let window frame know if kitchen has small indow
    public bool smallKitchenWindow = false;

    // Use this for initialization
    void Start()
    {
        plot = CreateQuad(Vector3.zero,plotX,plotZ);

        //save centrePoint        
        plot.name = "Plot";
        //StretchAndRotate(plot);
        plot.GetComponent<MeshFilter>().mesh.RecalculateBounds();
        centreOfPlot = plot.GetComponent<MeshRenderer>().bounds.center;
        plotMesh = plot.GetComponent<MeshFilter>().mesh;
        plotBox = plot.GetComponent<BoxCollider>();
       
        //decide house parametrs
        skirtingDepth = Random.Range(0.02f, 0.1f);
        skirtingHeight = Random.Range(0.1f, 0.3f);
        interiorDoorPatternOffset = Random.Range(-doorHeight * 0.25f, doorHeight*0.75f);
        
        //list of objects to go in room
        PopulateObjectsList();
        
        NameRooms2(plot);

        //place stair placeholder/collider if we have two floors
        if (floors == 2)
        {
            Stairs();
            FixStairCollider();//after doors?

        }
        
        //downstairs floor layout - old way of doing it- todo, change to method above? commented out -need to put doors in first then do it
        //doors and walls for downstairs
       List<GameObject> dbr = DoorsByRoomSize();
        foreach (GameObject db in dbr)
            exteriorAssets.Add(db);
        //missing walls for downstairs
        List<GameObject> missingInternals = MissingInternals();
        foreach (GameObject mI in missingInternals)
            exteriorAssets.Add(mI);
        

        //downstairs celings
        //make list for this
        List<GameObject> downStairsQuads = new List<GameObject>();
        for (int i = 0; i < roomsAndSizes.Count; i++)
        {
            downStairsQuads.Add(roomsAndSizes[i].room);
        }
        
        float floorDepth = 0.2f;
        //make ceilings for ground floor rooms- Whislt making these weneed to cut a hole if there are stairs. We need this section for later. save
        GameObject ceilingSection = RoomPlanner.CeilingsForRooms(downStairsQuads, storeyHeight, stairCollider, 1, floors, floorDepth,this);        
        
        //we need to do layouts at this moment because the windows need to avoid main items in rooms, but need doors to plan   
        
        FillRooms(downStairsQuads);
        //carpets etc
        RoomFloors(downStairsQuads);

        //build upstairs
        if (floors == 2)
        {
            //do first floor - using all statics
            FirstFloorRoomsLayout(ceilingSection);
            //build stair steps            
            StairSteps(stairCollider, storeyHeight);
        }

        List<GameObject> groundWalls = RoomPlanner.GroundFloorExteriorWalls(gameObject, downStairsQuads, plot,storeyHeight, doorHeight, doorWidth,windowHeight,windowWidth,stairCollider,floors,roomAmount,skirtingHeight,skirtingDepth, this);
        foreach (GameObject gW in groundWalls)
            exteriorAssets.Add(gW);

        //out erdge corners/coumns
        List<GameObject> corners = Corners(floors);
        //add to parent exterior object
        foreach (GameObject corner in corners)
            exteriorAssets.Add(corner);
        
        //roof
        GameObject roof = Roof(gameObject,plot,storeyHeight,floors);
        //add for LOD
        exteriorAssets.Add(roof);

        //moves all interior meshes to one parent object
        GroupInteriorsMeshes();
        GroupExteriorsMeshes();

        //make foundation
        Foundation();
        
    }

    private void Update()
    {
        if(fillRooms)
        {
            //
            //FillRooms();
            float innerSteepness = Random.Range(0.5f, 0.9f);
            float bathWidth = 1f;
            float cornerRandomness = Random.Range(bathWidth * .25f, bathWidth * .5f);//any lower than 2.5 and it pinches
            
       //     BathRoomItems.Bath(roomsAndSizes[0].room, GameObject.Find("Bath"),20f,2f,bathWidth,true,false,0.1f,cornerRandomness,innerSteepness, true);
            fillRooms = false;
        }

        if(doDebugLines)
        {
            for (int i = 0; i < debugLines.Count; i++)
            {
                Debug.DrawLine(debugLines[i].position, debugLines[i].direction);
            }

            for (int i = 0; i < debugLinesAngles.Count; i++)
            {
                Debug.DrawLine(debugLinesAngles[i].position, debugLinesAngles[i].direction,Color.blue);
            }
        }
    }
  
    void Foundation()
    {
        //use plot to extrude mesh downwards to cover any gaps which appear when house built on hill
        GameObject foundation = new GameObject();
        foundation.transform.position = plot.GetComponent<MeshRenderer>().bounds.center;
        MeshRenderer meshRenderer = foundation.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Resources.Load("Pink") as Material;
        MeshFilter meshFilter = foundation.AddComponent<MeshFilter>();

        float depth = 5f;
        Vector3[] verts = plotMesh.vertices;
        //swappy
        Vector3 temp = verts[2];
       // verts[2] = verts[1];
       // verts[1] = temp;

        List<Vector3> vertsList = new List<Vector3>();
        List<int> trisList = new List<int>();

        for (int i = 0; i < verts.Length - 1; i++)
        {   

            vertsList.Add(verts[i]);
            vertsList.Add(verts[i + 1]);
            vertsList.Add(verts[i + 1] + (Vector3.down * depth));
            vertsList.Add(verts[i] + (Vector3.down * depth));
        }

        //add joining link/ last one
        vertsList.Add(verts[verts.Length - 1]);
        vertsList.Add(verts[0]);
        vertsList.Add(verts[0] + (Vector3.down * depth));
        vertsList.Add(verts[verts.Length - 1] + (Vector3.down * depth));


        for (int i = 0; i < vertsList.Count - 2; i += 4)
        {

            trisList.Add(i + 0);
            trisList.Add(i + 2);
            trisList.Add(i + 1);

            trisList.Add(i + 0);
            trisList.Add(i + 3);
            trisList.Add(i + 2);

        }

        foreach (Vector3 v3 in vertsList)
        {
               GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
               cube.transform.position = v3;
               cube.transform.localScale *= 0.1f;
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = vertsList.ToArray();
        newMesh.triangles = trisList.ToArray();

        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();


        meshFilter.mesh = newMesh;

        foundation.name = "Foundation";
        foundation.transform.parent = transform; 
    }
    void GroupInteriorsMeshes()
    {
        //add objects to one gameobject then atttach combine script once we wait a frame by WaitAndMerge Coroutine in house V2

        //now parent to interior object so we can combine all interior meshes
        foreach (List<GameObject> roomList in interiorAssetsByRoom)
        {
            foreach (GameObject obj in roomList)
                obj.transform.parent = gameObject.transform.parent.GetComponent<HouseV2>().interiorParent.transform;
        }
    }

    void GroupExteriorsMeshes()
    {
        //add objects to one gameobject then atttach combine script once we wait a frame by WaitAndMerge Coroutine in house V2
        //now parent to exnterior object so we can combine all interior meshes        
        foreach (GameObject obj in exteriorAssets)
            obj.transform.parent = gameObject.transform.parent.GetComponent<HouseV2>().exteriorParent.transform;        
    }

    void PopulateObjectsList()
    {
        //bathroom 1
        ObjectAndSize oas = new ObjectAndSize();
        oas.name = "Bath";
        oas.size = new Vector3(Random.Range(1f, 1.5f), Random.Range(1.25f, 1.75f), Random.Range(1.75f, 2.25f));
        objectsBathroom1.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Sink";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsBathroom1.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Toilet";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsBathroom1.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Toilet";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsBathroom1.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Toilet";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsBathroom1.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Toilet";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsBathroom1.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Toilet";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsBathroom1.Add(oas);



        //bathroom 2       

        oas = new ObjectAndSize();
        oas.name = "Shower";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsBathroom2.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Sink";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsBathroom2.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Toilet";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsBathroom2.Add(oas);


        //bedroom1
        //in order of importance in room ..randomise after bed?
        oas = new ObjectAndSize();
        oas.name = "Bed";
        oas.size = new Vector3(Random.Range(1.5f, 2f), Random.Range(1f, 1f), Random.Range(2.5f, 2.5f));
        objectsBedroom1.Add(oas);
        oas = new ObjectAndSize();
        oas.name = "Wardrobe";
        oas.size = new Vector3(Random.Range(2f, 1f), Random.Range(1f, 2f), Random.Range(.5f, .5f));
        objectsBedroom1.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Radiator";
        oas.size = new Vector3(Random.Range(2f, 1f), Random.Range(1f, 1f), Random.Range(.3f, .3f));
        objectsBedroom1.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Desk";
        oas.size = new Vector3(Random.Range(2f, 1f), Random.Range(1f, 1f), Random.Range(1f, 1f));
        objectsBedroom1.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Wardrobe";
        oas.size = new Vector3(Random.Range(2f, 1f), Random.Range(1f, 2f), Random.Range(.5f, .5f));
        objectsBedroom1.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Wardrobe";
        oas.size = new Vector3(Random.Range(2f, 1f), Random.Range(1f, 2f), Random.Range(.5f, .5f));
        objectsBedroom1.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Wardrobe";
        oas.size = new Vector3(Random.Range(2f, 1f), Random.Range(1f, 2f), Random.Range(.5f, .5f));
        objectsBedroom1.Add(oas);

        

        //bedroom2

        oas = new ObjectAndSize();
        oas.name = "Bed";
        oas.size = new Vector3(1f, 1f, 2f);
        objectsBedroom2.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Wardrobe";
        oas.size = new Vector3(Random.Range(2f, 1f), Random.Range(1f, 2f), Random.Range(.5f, .5f));
        objectsBedroom2.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Wardrobe";
        oas.size = new Vector3(Random.Range(2f, 1f), Random.Range(1f, 2f), Random.Range(.5f, .5f));
        objectsBedroom2.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Wardrobe";
        oas.size = new Vector3(Random.Range(2f, 1f), Random.Range(1f, 2f), Random.Range(.5f, .5f));
        //  objectsBedroom2.Add(oas);

        //kitchen1
        oas = new ObjectAndSize();
        oas.name = "Sink";
        oas.size = new Vector3(2f, 1f, 1f);
        objectsKitchen1.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Fridge";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsKitchen1.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Cooker";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsKitchen1.Add(oas);

        //kitchen2
        oas = new ObjectAndSize();
        oas.name = "Sink";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsKitchen2.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Fridge";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsKitchen2.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Cooker";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsKitchen2.Add(oas);

        //kitchen1
        oas = new ObjectAndSize();
        oas.name = "Sink";
        oas.size = new Vector3(2f, 1f, 1f);
        objectsKitchen1.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Fridge";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsKitchen1.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Cooker";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsKitchen1.Add(oas);

        //kitchen2
        oas = new ObjectAndSize();
        oas.name = "Sink";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsKitchen2.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Fridge";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsKitchen2.Add(oas);

        oas = new ObjectAndSize();
        oas.name = "Cooker";
        oas.size = new Vector3(1f, 1f, 1f);
        objectsKitchen2.Add(oas);

        //living room
        oas.name = "Wardrobe";
        oas.size = new Vector3(2f, 1f, 0.5f);
        objectsLivingroom.Add(oas);
        objectsLivingroom.Add(oas);
        oas = new ObjectAndSize();
        oas.name = "Wardrobe";
        oas.size = new Vector3(1f, 1f, .5f);
        objectsLivingroom.Add(oas);
    }

    void NameRooms()
    {
        //sort rooms by size        

        roomsAndSizes = new List<RoomsAndSizes>();

        for (int i = 0; i < transform.childCount; i++)
        {
            RoomsAndSizes rAndM = new RoomsAndSizes();
            rAndM.room = transform.GetChild(i).gameObject;
            rAndM.size = transform.GetChild(i).GetComponent<MeshRenderer>().bounds.size.sqrMagnitude;

            roomsAndSizes.Add(rAndM);
        }
        //https://forum.unity3d.com/threads/sort-a-list-of-a-class-by-a-int-in-it.224642/
        roomsAndSizes = roomsAndSizes.OrderBy(rAndM => rAndM.size).ToList();

        roomsAndSizes[0].room.name = "Bathroom";
        roomsAndSizes[0].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
        roomsAndSizes[1].room.name = "Bedroom";
        roomsAndSizes[1].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Green") as Material;
        roomsAndSizes[2].room.name = "Kitchen";
        roomsAndSizes[2].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;
        if (roomAmount > 3)
        {
            roomsAndSizes[3].room.name = "LivingRoom";
            roomsAndSizes[3].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Yellow") as Material;
        }
    }

    void NameRooms2(GameObject plot)
    {
        float variance = .1f;
        //split the main plot
        //float here, can limit the split on the room
        List<GameObject> splits = Split(gameObject,plot, variance,false,false);
        //split the smaller half in to bedroom and bathroom
        GameObject roomToSplit = splits[0];
        GameObject largerRoom = splits[1];
        if (splits[0].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude > splits[1].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude)
        {
            roomToSplit = splits[1];
            largerRoom = splits[0];
        }
        
        List<GameObject> rooms = Split(gameObject, roomToSplit, variance,false,false);
        if (rooms[0].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude < rooms[1].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude)
        {
            RoomsAndSizes rAndM = new RoomsAndSizes();
            rAndM.room = rooms[0];
            rAndM.size = rooms[0].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude;
            roomsAndSizes.Add(rAndM);

            rAndM = new RoomsAndSizes();
            rAndM.room = rooms[1];
            rAndM.size = rooms[1].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude;
            roomsAndSizes.Add(rAndM);
        }
        else
        {
            RoomsAndSizes rAndM = new RoomsAndSizes();
            rAndM.room = rooms[1];
            rAndM.size = rooms[1].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude;
            roomsAndSizes.Add(rAndM);

            rAndM = new RoomsAndSizes();
            rAndM.room = rooms[0];
            rAndM.size = rooms[0].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude;
            roomsAndSizes.Add(rAndM);
        }

        if (floors == 1)
        {
        
                roomsAndSizes[0].room.name = "Bathroom";
                roomsAndSizes[0].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;

                roomsAndSizes[1].room.name = "Bedroom";
                roomsAndSizes[1].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Green") as Material;        
            
        }
        if(floors == 2)
        {
            if (roomAmount == 4)
            {
                roomsAndSizes[0].room.name = "Bathroom";
                roomsAndSizes[0].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;

                roomsAndSizes[1].room.name = "Kitchen";
                roomsAndSizes[1].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;

              //  Debug.Log("HEREHERE");
            }

            if (roomAmount == 5)
            {
                roomsAndSizes[0].room.name = "Bathroom";
                roomsAndSizes[0].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;

                roomsAndSizes[1].room.name = "Kitchen";
                roomsAndSizes[1].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;
            }
        }
        //i know this is crazy
        

        if (roomAmount == 3)
        {
            //add the room we never split to rooms list
            RoomsAndSizes rAndM = new RoomsAndSizes();
            rAndM.room = largerRoom;
            rAndM.size = largerRoom.GetComponent<MeshRenderer>().bounds.size.sqrMagnitude;
            roomsAndSizes.Add(rAndM);
            if (floors == 1)
            {
                roomsAndSizes[0].room.name = "Bathroom";
                roomsAndSizes[0].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;

                roomsAndSizes[1].room.name = "Bedroom";
                roomsAndSizes[1].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Green") as Material;

                roomsAndSizes[2].room.name = "Kitchen";
                roomsAndSizes[2].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;
            }
            
            else if (floors == 2)
            {
                roomsAndSizes[0].room.name = "Hall";
                roomsAndSizes[0].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;
                roomsAndSizes[1].room.name = "Kitchen";
                roomsAndSizes[1].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;

                roomsAndSizes[2].room.name = "LivingRoom";
                roomsAndSizes[2].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Yellow") as Material;
            }

            return;
        }

        //split the other half

        rooms = new List<GameObject>();
        rooms = Split(gameObject, largerRoom, variance,false, false);



        if (roomAmount > 3)
        {

            //make living room the largest room
            if (rooms[0].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude < rooms[1].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude)
            {
                RoomsAndSizes rAndM = new RoomsAndSizes();
                rAndM.room = rooms[0];
                rAndM.size = rooms[0].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude;
                roomsAndSizes.Add(rAndM);

                rAndM = new RoomsAndSizes();
                rAndM.room = rooms[1];
                rAndM.size = rooms[1].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude;
                roomsAndSizes.Add(rAndM);
            }
            else
            {
                RoomsAndSizes rAndM = new RoomsAndSizes();
                rAndM.room = rooms[1];
                rAndM.size = rooms[1].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude;
                roomsAndSizes.Add(rAndM);

                rAndM = new RoomsAndSizes();
                rAndM.room = rooms[0];
                rAndM.size = rooms[0].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude;
                roomsAndSizes.Add(rAndM);
            }
            if (floors == 1)
            {
                roomsAndSizes[2].room.name = "Kitchen";
                roomsAndSizes[2].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;

                roomsAndSizes[3].room.name = "LivingRoom";
                roomsAndSizes[3].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Yellow") as Material;
            }
            if(floors== 2)
            {
                roomsAndSizes[2].room.name = "Hall";
                roomsAndSizes[2].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;

                roomsAndSizes[3].room.name = "LivingRoom";
                roomsAndSizes[3].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Yellow") as Material;
            }

        }
        
        if (roomAmount > 4)
        {
            //split largest room in to two - Kitchen and hall
            //leaving living room as largest room (usually)
            roomsAndSizes.RemoveAt(3);
            largerRoom = rooms[0];

            if (rooms[0].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude < rooms[1].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude)
                largerRoom = rooms[1];

            rooms = new List<GameObject>();
            rooms = Split(gameObject, largerRoom, variance,false,false);

            if (rooms[0].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude > rooms[1].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude)
            {
                RoomsAndSizes rAndM = new RoomsAndSizes();
                rAndM.room = rooms[0];
                rAndM.size = rooms[0].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude;
                roomsAndSizes.Add(rAndM);

                rAndM = new RoomsAndSizes();
                rAndM.room = rooms[1];
                rAndM.size = rooms[1].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude;
                roomsAndSizes.Add(rAndM);
            }
            else
            {
                RoomsAndSizes rAndM = new RoomsAndSizes();
                rAndM.room = rooms[1];
                rAndM.size = rooms[1].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude;
                roomsAndSizes.Add(rAndM);

                rAndM = new RoomsAndSizes();
                rAndM.room = rooms[0];
                rAndM.size = rooms[0].GetComponent<MeshRenderer>().bounds.size.sqrMagnitude;
                roomsAndSizes.Add(rAndM);
            }
            if (floors == 1)
            {

                roomsAndSizes[2].room.name = "LivingRoom";
                roomsAndSizes[2].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Yellow") as Material;

                roomsAndSizes[3].room.name = "Kitchen";
                roomsAndSizes[3].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;

                roomsAndSizes[4].room.name = "Hall";
                roomsAndSizes[4].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;
            }
            if (floors == 2)
            {
                roomsAndSizes[2].room.name = "LivingRoom";
                roomsAndSizes[2].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Yellow") as Material;

                roomsAndSizes[3].room.name = "Lounge";
                roomsAndSizes[3].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("White") as Material;

                roomsAndSizes[4].room.name = "Hall";
                roomsAndSizes[4].room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;

            }
        }
        
    }

    void FirstFloorRoomsLayout(GameObject ceilingSection)
    {
        float floorDepth = 0.2f;
        storeyHeight += floorDepth;

        GameObject firstFloor = new GameObject();
        firstFloor.transform.parent = transform;
        firstFloor.name = "First Floor";
        firstFloor.transform.position += Vector3.up * storeyHeight;
        //use stair collider to decide how to lay out rooms
        ceilingSection.transform.parent = firstFloor.transform;
  

        //split uptairs floor in to a useable layout
        List<GameObject> halls = new List<GameObject>();
        List<GameObject> quadsBuilt = RoomPlanner.FirstFloorLayoutV2(out halls, transform.gameObject, firstFloor, plot, storeyHeight, stairCollider);
        


        //Debug.Log("quads built = " + quadsBuilt.Count);

        //asign parent - could possibly do in fucntion since I pass first floor object anyway. But seems quicker and simpler to do here now it's all working
        foreach (GameObject go in quadsBuilt)
            go.transform.parent = firstFloor.transform;
        //also do halls
        foreach (GameObject go in halls)
            go.transform.parent = firstFloor.transform;
        
        //let's add the halls to quads built
        List<GameObject> allQuads = new List<GameObject>(halls);
        allQuads.AddRange(quadsBuilt);

        //now order rooms by size
        quadsBuilt = RoomPlanner.RoomsBySize(quadsBuilt);
        halls = RoomPlanner.RoomsBySize(halls);
        halls.Reverse();

        
        

        float widthOfStair = stairCollider.transform.localScale.x;
        halls = RoomPlanner.PrepareHalls(gameObject, halls, widthOfStair, storeyHeight,1);

        foreach (GameObject hall in halls)
            hall.transform.parent = firstFloor.transform;

        RoomPlanner.DoorsForSegmentedHall(quadsBuilt, halls, storeyHeight,doorHeight,doorWidth,this);

        //interior - rename

        RoomPlanner.WallsForRoom(quadsBuilt,plot,1,storeyHeight,doorHeight,doorWidth,windowHeight,windowWidth,skirtingHeight,skirtingDepth, this);
        //amke walls and return welded hall floor mesh - "weldo" 
        GameObject weldo = RoomPlanner.HallWalls(quadsBuilt,halls,plot,storeyHeight,1,gameObject,stairCollider,doorHeight,doorWidth,firstFloor, skirtingHeight, skirtingDepth,this);
        weldo.transform.parent = firstFloor.transform;
        // 
        //do interiors - we need interiors before exterior walls so we can place windows appropriately - e.g don't put behind a wardrobe        

        quadsBuilt = RoomPlanner.NameUpstairsRooms(quadsBuilt);

        quadsBuilt = FillRooms(quadsBuilt);

        List<GameObject> walls = StairAreaWalls(stairCollider, storeyHeight,firstFloor,plot);
        
        //CEILINGS
        //add welded hall to the quads for celiling         
        List<GameObject> quadsForCeiling = new List<GameObject>(quadsBuilt);
        quadsForCeiling.Add(weldo);
        
        RoomPlanner.CeilingsForRooms(quadsForCeiling, storeyHeight, stairCollider,2,floors, floorDepth,this);
        //weldo's ceiling gets returned too high - proabbly something to with being auto welded, nevermind - quick easy fix here
        weldo.transform.GetChild(0).transform.position -= Vector3.up * storeyHeight;

        //Fillers were built around the stairs on the ground floor when building the hole in the wall - Manually find these and parent them to first floor
        List<GameObject> fillers = new List<GameObject>();
        GameObject oldParent = gameObject.transform.Find("Hall").gameObject;
        for (int i = 0; i < oldParent.transform.childCount; i++)
        {
            if (oldParent.transform.GetChild(i).name == "Filler")
                fillers.Add(oldParent.transform.GetChild(i).gameObject);
        }
        foreach (GameObject go in fillers)
            go.transform.parent = firstFloor.transform;

       
        storeyHeight -= floorDepth;
       // Debug.Log("Halls after pass = " + halls.Count);
        RoomFloors(quadsBuilt);
        RoomFloors(halls);

        return;
    }

    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

    public void Stairs()
    {
        //place stairs in
        if(floors == 2)
        {
            
            //place stairs in Hall
            GameObject hall = null;
            for (int i = 0; i < roomsAndSizes.Count; i++)
            {
                if (roomsAndSizes[i].room.name == "Hall")
                    hall = roomsAndSizes[i].room;
            }
            Vector3[] thisVertices = hall.GetComponent<MeshFilter>().mesh.vertices;

            List<Vector3> listVertices = new List<Vector3>();
            for (int x = 0; x < thisVertices.Length; x++)
            {
                listVertices.Add(thisVertices[x]);
            }

            //Get adjacent rooms - global var
            adjacentRooms = AdjacentRooms(roomsAndSizes, hall, thisVertices);
            //what room do we share a wall with? - returns single room, could change to a list if needed
            // GameObject sharedWallRoom = SharedWallRoom(roomsAndSizes, hall, thisVertices, adjacentRooms); //-- not using?
            
            Vector3 start = Vector3.zero;
            //if one, place stairs -if two, choose randomly between them- no, place furthest from centre of plot
            if (listVertices.Count == 1)
            {
                start = listVertices[0];
                Debug.Log("THIS NEVER HAPPENS");
                Debug.Break();
            }
            else if (listVertices.Count == 2)
            {
                //trying furthest from centre
                float distance = Mathf.Infinity;
                Vector3 furthest = Vector3.zero;
                Vector3 center = plot.GetComponent<MeshRenderer>().bounds.center;
                
                for (int i = 0; i < thisVertices.Length; i++)
                {
                    float temp = Vector3.Distance(thisVertices[i], center);
                    if (temp > distance)
                    {
                        furthest = thisVertices[i];
                        distance = temp;
                    }
                }

                start = furthest;

                Debug.Log("STOPPED HERE");//this never happens?
                Debug.Break(); 
            }
            else
            {
                //using hall? works with plot too..
                //bool doClosest = false;
                //if (adjacentRooms.Count == 1 || roomAmount == 4)//i fwe create central stair with 4, ujpstairs doesnt have enough space
                {

                    //long house

                    //pout in far corner, dnot worry about placing on a shared vertice wall - menas room is on end ofhouse like a bookend
                    List<Vector3> sVs = new List<Vector3>();
                    foreach (Vector3 v3 in thisVertices)
                        sVs.Add(v3);



                    sVs.Sort(delegate (Vector3 v1, Vector3 v2)
                    {
                        return Vector3.Distance(centreOfPlot, v1).CompareTo
                                    ((Vector3.Distance(centreOfPlot, v2)));
                    });

                    start = sVs[sVs.Count - 1];

                 //   Debug.Log("Stairs 1");
                    
                }
               // else
                //{
                    //creates a central staircase,but we don't want a central staircase if the hall is on the corner of the house
                    //Debug.Break();
                   // if (roomAmount != 4)
                      //  start = FindStairStart(adjacentRooms, plot, thisVertices, false);//
                   // else
                   // {
                       // Debug.Log("HOw");
                      //  Debug.Break();
                   // }

                   // Debug.Log("Stairs 2");

                //}
                
            }

            //find two closest vertices of hall from this 
                
            List<Vector3> sortedVertices = new List<Vector3>();
            foreach (Vector3 v3 in thisVertices)
                sortedVertices.Add(v3);
                

            
            sortedVertices.Sort(delegate (Vector3 v1, Vector3 v2)
            {
                return Vector3.Distance(start, v1).CompareTo
                            ((Vector3.Distance(start, v2)));
            });

            //place boxcollider
            for (int i = 1; i < 3; i++)
            {
               // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
               // c.transform.position = sortedVertices[i];
                //c.transform.parent = hall.transform;
            }

            float width =  doorWidth*2;//?
            if (width > Vector3.Distance(start, sortedVertices[1]) * 0.5f)
            {
                width = Vector3.Distance(start, sortedVertices[1]) * 0.5f;
                //below this gets buggy? - go figure
                if (width < 1.2f)
                    width = 1.2f;
                //Debug.Log("WIDTH CHANGED");
            }
            //round - avoiding bugs - skewed floor plans etc
            width = (float)System.Math.Round((double)width, 1);
            float length = 5;
            if (length > Vector3.Distance(start, sortedVertices[2]) - width)
            {
                length = Vector3.Distance(start, sortedVertices[2]) - width;
                //Debug.Log("LENGTH CHANGED");
            }

           
            length= (float)System.Math.Round((double)length, 2);

            Vector3 directionToFurthestPoint = (sortedVertices[2] - start).normalized;
            
            stairCollider = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stairCollider.transform.parent = hall.transform;
            stairCollider.name = "StairCollider";
            stairCollider.transform.position = start;
            
            Quaternion stairColliderRot = Quaternion.LookRotation(directionToFurthestPoint); //can be zero
            stairColliderRot = Quaternion.Euler(0, stairColliderRot.eulerAngles.y, 0);
            stairCollider.transform.rotation = stairColliderRot;

            bool onLeft = RoomPlanner.CentreOfRoomOnLeft(start, hall.GetComponent<MeshRenderer>().bounds.center, directionToFurthestPoint);
            Vector3 sideWays = stairCollider.transform.right;
            if (!onLeft)
                sideWays = -stairCollider.transform.right;



            stairCollider.transform.position += sideWays * width * 0.5f;
            stairCollider.transform.position += stairCollider.transform.forward * length * 0.5f;

            

            //stairCollider.transform.position += Vector3.up * 0.5f;
            stairCollider.transform.localScale = new Vector3(width+0.6f, 1, length+0.2f);//+0.2 for internal wall size and + 0.4f for half a dorr's width - will need to reduce this back after
                


        }//end of room amount == 4
        
    }

    public void StairSteps(GameObject stairCollider,float storeyHeight)
    {
        //make steps. We pass a collider box to govern the size of the steps, however we must leave a gap to get on the stairs at the bottom. At the top there is a landing to walk on to, but not at the bottom
        float stepHeight = 0.2f;

        Vector3 start = stairCollider.transform.position - stairCollider.transform.forward* stairCollider.transform.localScale.z * 0.5f +Vector3.up*stepHeight*0.5f;

        float landingLength = stairCollider.transform.localScale.x;
        start += stairCollider.transform.forward * landingLength;

        
        Vector3 end = stairCollider.transform.position + stairCollider.transform.forward * stairCollider.transform.localScale.z * 0.5f + Vector3.up * (storeyHeight-stepHeight*0.5f);

        //make steps run between these points
        Vector3 dir = (end - start).normalized;
        float distance = Vector3.Distance(end, start);

        float step = distance / 10;
        
        for (float i = stepHeight; i < distance; i+=step)
        {
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = start + dir * i;

            c.transform.localScale = new Vector3(stairCollider.transform.localScale.x, stepHeight, .5f);
            c.transform.rotation = stairCollider.transform.rotation;

            c.transform.parent = stairCollider.transform;

            c.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;

        }

        stairCollider.GetComponent<MeshRenderer>().enabled = false;
        stairCollider.GetComponent<BoxCollider>().enabled = false;
    }

    public List<GameObject> StairAreaWalls(GameObject stairCollider,float storeyHeight,GameObject parent,GameObject plot)
    {

        List<GameObject> walls = new List<GameObject>();
        //build wall for upstairs around stairs
        Vector3 RL = stairCollider.transform.position - stairCollider.transform.forward * stairCollider.transform.localScale.z * 0.5f - stairCollider.transform.right*stairCollider.transform.localScale.x*0.5f;

        Vector3 FL = stairCollider.transform.position + stairCollider.transform.forward * stairCollider.transform.localScale.z * 0.5f - stairCollider.transform.right * stairCollider.transform.localScale.x * 0.5f;

        Vector3 FR = stairCollider.transform.position + stairCollider.transform.forward * stairCollider.transform.localScale.z * 0.5f + stairCollider.transform.right * stairCollider.transform.localScale.x * 0.5f;

        Vector3 RR = stairCollider.transform.position - stairCollider.transform.forward * stairCollider.transform.localScale.z * 0.5f + stairCollider.transform.right * stairCollider.transform.localScale.x * 0.5f;

        //1st wall
        Vector3 midPoint = Vector3.Lerp(RL, FL, 0.5f);
        GameObject wall = HouseBuilder.Wall(midPoint, stairCollider.transform.localScale.z, -stairCollider.transform.right, false, gameObject,storeyHeight);

        wall.transform.position += Vector3.up * storeyHeight;

        walls.Add(wall);
        wall.transform.parent = parent.transform;
        //2nd wall
        midPoint = Vector3.Lerp(FR, RR, 0.5f);
        GameObject wall2 = HouseBuilder.Wall(midPoint, stairCollider.transform.localScale.z, stairCollider.transform.right, false, gameObject, storeyHeight);
        wall2.transform.parent = parent.transform;
        wall2.transform.position += Vector3.up * storeyHeight;

        walls.Add(wall2);

        //wall3 short side
        midPoint = Vector3.Lerp(RL, RR, 0.5f);
        GameObject wall3 = HouseBuilder.Wall(midPoint, stairCollider.transform.localScale.x, -stairCollider.transform.forward, false, gameObject, storeyHeight);
        wall3.transform.parent = parent.transform;
        wall3.transform.position += Vector3.up * storeyHeight;

        //exterior of this wall
        GameObject wall4 = HouseBuilder.Wall(midPoint, stairCollider.transform.localScale.x, -stairCollider.transform.forward, true, gameObject, storeyHeight);
        wall4.transform.parent = parent.transform;
        wall4.transform.position += Vector3.up * storeyHeight;
        wall4.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;

        //we need to check if there any other exterior walls needing built
        //loop of plot mesh
        List<Vector3> plotLoop = new List<Vector3>();
        Vector3[] plotVertices = plot.GetComponent<MeshFilter>().mesh.vertices;
        //int floor = 2;
        foreach (Vector3 v3 in plotVertices)
        {
            //add floor and storey height to plot 
            Vector3 temp = v3;// + Vector3.up * (storeyHeight * floor);
            plotLoop.Add(temp);
            /*
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = temp;
            c.name = "plot";
            */
        }
        plotLoop.Add(plotVertices[0]);// + Vector3.up * (storeyHeight * floor));
        for (int i = 0; i < 2; i++)
        {
            //1st time check the left hand points
            Vector3 v1 = FL;
            Vector3 v2 = RL;
            //second, chck the right..
            if(i == 1)
            {
                v1 = FR;
                v2 = RR;
            }
            /*
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = v1;
            c.name = "v1";
            c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = v2;
            c.name = "v2";
            */
            bool exterior = false;
            for (int k = 0; k < plotLoop.Count - 1; k++)
                //if both points are on outside edge - it's an exterior wall
                if (Divide.PointsInLine(plotLoop[k], plotLoop[k + 1], v1))
                    if (Divide.PointsInLine(plotLoop[k], plotLoop[k + 1], v2))
                        exterior = true;

            if(exterior)
            {
                if(i == 0)
                {
                    midPoint = Vector3.Lerp(RL, FL, 0.5f);
                    wall = HouseBuilder.Wall(midPoint, stairCollider.transform.localScale.z, -stairCollider.transform.right, true, gameObject, storeyHeight);
                    wall.transform.position += Vector3.up * storeyHeight;
                    walls.Add(wall);
                    wall.transform.parent = parent.transform;
                    wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                }
                else if (i==1)
                {
                    midPoint = Vector3.Lerp(FR, RR, 0.5f);
                    wall = HouseBuilder.Wall(midPoint, stairCollider.transform.localScale.z, stairCollider.transform.right, true, gameObject, storeyHeight);
                    wall.transform.parent = parent.transform;
                    wall.transform.position += Vector3.up * storeyHeight;
                    wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                    walls.Add(wall);
                }
            }
        }

        return walls;
    }

    public static List<GameObject> AdjacentRooms(List<RoomsAndSizes> roomsAndSizes, GameObject stairParent, Vector3[] thisVertices)
    {
        List<GameObject> adjacentRooms = new List<GameObject>();

        //just using for .Contains
        List<Vector3> listVertices = new List<Vector3>();
        for (int x = 0; x < thisVertices.Length; x++)
        {
            listVertices.Add(thisVertices[x]);
        }


        GameObject sharedWallRoom = null;
        //find a free vertice, one not shared by another room- put staircase here - how big?
        for (int i = 0; i < roomsAndSizes.Count; i++)
        {
            if (roomsAndSizes[i].room == stairParent)
                continue;

            Vector3[] targetVertices = roomsAndSizes[i].room.GetComponent<MeshFilter>().mesh.vertices;

            for (int j = 0; j < thisVertices.Length; j++)
            {
                for (int k = 0; k < targetVertices.Length; k++)
                {
                    if (thisVertices[j] == targetVertices[k])
                    {
                        if (listVertices.Contains(thisVertices[j]))
                            listVertices.Remove(thisVertices[j]);

                        if (!adjacentRooms.Contains(roomsAndSizes[i].room))
                            adjacentRooms.Add(roomsAndSizes[i].room);
                        else
                        {
                         //   Debug.Log("Shared with " + roomsAndSizes[i].room.name);

                            sharedWallRoom = roomsAndSizes[i].room;//adds largest last so smaller will be overwritten - its better to share verices with bigger rooms
                        }
                    }
                }
            }
        }

        return adjacentRooms;
    }

    public static GameObject SharedWallRoom(List<RoomsAndSizes> roomsAndSizes, GameObject parent, Vector3[]thisVertices,List<GameObject> adjacentRooms)
    {
        GameObject sharedWallRoom = null;

        List<Vector3> listVertices = new List<Vector3>();
        for (int x = 0; x < thisVertices.Length; x++)
        {
            listVertices.Add(thisVertices[x]);
        }

        //find a free vertice, one not shared by another room- put staircase here - how big?
        for (int i = 0; i < roomsAndSizes.Count; i++)
        {
            if (roomsAndSizes[i].room == parent)
                continue;

            Vector3[] targetVertices = roomsAndSizes[i].room.GetComponent<MeshFilter>().mesh.vertices;

            for (int j = 0; j < thisVertices.Length; j++)
            {
                for (int k = 0; k < targetVertices.Length; k++)
                {
                    if (thisVertices[j] == targetVertices[k])
                    {
                        if (listVertices.Contains(thisVertices[j]))
                            listVertices.Remove(thisVertices[j]);

                        if (!adjacentRooms.Contains(roomsAndSizes[i].room))
                            adjacentRooms.Add(roomsAndSizes[i].room);
                        else
                        {
                            Debug.Log("Shared with " + roomsAndSizes[i].room.name);

                            sharedWallRoom = roomsAndSizes[i].room;//adds largest last so smaller will be overwritten - its better to share vertices with bigger rooms /randomise this?
                        }
                    }
                }
            }
        }

        return sharedWallRoom;
    }

    public static Vector3 FindStairStart(List<GameObject> adjacentRooms,GameObject parent,Vector3[] thisVertices,bool findClosest)
    {
        // Debug.Break();
        List<Vector3> pointsOnWall = new List<Vector3>();
        //if none, place furthest from any intersecting vertice along hall's wall
        for (int a = 0; a < adjacentRooms.Count; a++)
        {
            Vector3[] adjAVertices = adjacentRooms[a].GetComponent<MeshFilter>().mesh.vertices;
            for (int b = 0; b < adjacentRooms.Count; b++)
            {
                if (a == b)
                    continue;

                Vector3[] adjBVertices = adjacentRooms[b].GetComponent<MeshFilter>().mesh.vertices;

                //check for any shared vertices
                for (int x = 0; x < adjAVertices.Length; x++)
                {
                    for (int y = 0; y < adjBVertices.Length; y++)
                    {
                        if (adjAVertices[x] == adjBVertices[y])
                        {
                            pointsOnWall.Add(adjAVertices[x]);

                        }
                    }
                }
            }
        }

        float distance = Mathf.Infinity;
        Vector3 closest = Vector3.zero;
        Vector3 center = parent.GetComponent<MeshRenderer>().bounds.center;
        for (int i = 0; i < pointsOnWall.Count; i++)
        {
            float temp = Vector3.Distance(pointsOnWall[i], center);
            if (temp < distance)
            {
                closest = pointsOnWall[i];
                distance = temp;
            }
        }

        /*
        distance = 0;
        float closestDistance = Mathf.Infinity;

        
        Vector3 furthest = Vector3.zero;

        //now find furthest hall vertice from here -NOTE if we have six rooms, we may need to find out the closest to centre of room differently?
        for (int i = 0; i < thisVertices.Length; i++)
        {
            float temp = Vector3.Distance(thisVertices[i], closest);
            if (temp > distance)
            {
                furthest = thisVertices[i];
                distance = temp;
            }

            if (temp < distance)
            {
                closest = thisVertices[i];
                closestDistance = temp;
            }
        }

       */

        List<Vector3> sortedVertices = new List<Vector3>();
        foreach (Vector3 v3 in thisVertices)
            sortedVertices.Add(v3);
        sortedVertices.Sort(delegate (Vector3 v1, Vector3 v2)
        {
            return Vector3.Distance(closest, v1).CompareTo
                        ((Vector3.Distance(closest, v2)));
        });

        Vector3 start = sortedVertices[sortedVertices.Count - 1];
        if (findClosest)
            start = sortedVertices[0];


        return start;
    }

    public void FixStairCollider()
    {
        //puts collider back to correct size - we extended to overlap walls so doors could check if they were hitting it
        Vector3 scale = new Vector3(stairCollider.transform.localScale.x - 0.6f, 1, stairCollider.transform.localScale.z - 0.2f);

        //round this -causing problems
        //scale.x = (float)System.Math.Round((double)scale.x, 1);
        //scale.z = (float)System.Math.Round((double)scale.z, 1);
        stairCollider.transform.localScale = scale;
        //round
    }

    public List<GameObject> DoorsByRoomSize()
    {

        List<GameObject> wallsReturned = new List<GameObject>();

        //the way this function works requires each room to phave a door as a child. Problem is there should only be one door shared between two rooms- We can just delete unneeded ones later. Upstairs method doesn't need this. TODO - UPort upstairs method to downstairs
        List<GameObject> doorsToRemove = new List<GameObject>();

        //rooms are ordered by small to large in roomsAndsizes list

        //list of built doors
        List<WallWithDoor> wallsWithDoors = new List<WallWithDoor>();
        
        //for each room, work out and store how many shared points it has with each other room
        for (int i = 0; i < roomsAndSizes.Count; i++)
        {
            List<TargetAndSharedPoints> targetAndSharedPoints = new List<TargetAndSharedPoints>();

            for (int j = 0; j < roomsAndSizes.Count; j++)
            {
                GameObject thisRoom = roomsAndSizes[i].room;
                GameObject targetRoom = roomsAndSizes[j].room;
                List<Vector3> sharedPoints = SharedPointsWithTargetRoom(thisRoom, targetRoom);

                TargetAndSharedPoints tasp = new TargetAndSharedPoints();
                tasp.room = roomsAndSizes[i].room;
                tasp.target = roomsAndSizes[j].room;
                tasp.sharedPoints = sharedPoints;

                targetAndSharedPoints.Add(tasp);
                //Debug.Log(thisRoom.name  + " Shared Points = " + sharedPoints.Count + ". Target room = " + targetRoom.name);
            }
            listOfRoomsAndSharedPoints.Add(targetAndSharedPoints);
        }
        
        for (int i = 0; i < roomAmount; i++)
        {
            //smallest room is bathroom
            GameObject thisRoom = roomsAndSizes[i].room;

            if (i == roomAmount - 1)
            {                
                continue;
            }

            //rooms to check //start with largest and work down the way - aim here is to create natural flow through house
            for (int r = roomsAndSizes.Count - 1; r >= 0; r--)
            {
                if (roomsAndSizes[i].room == roomsAndSizes[r].room)
                {
                    continue;
                }

                if (roomsAndSizes[i].room.name == "Kitchen" && roomsAndSizes[r].room.name == "Bedroom")
                {
                    //The random layout has given us something undesirable, run random function again?
                    Debug.Log("Kitchen in to bedroom or Bathroom - rebuild?");
                    Debug.Break();
                }

                GameObject targetRoom = roomsAndSizes[r].room;

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
                        countOfOtherRoomsWith2SharedPoints++;
                }

                if (sharedPoints.Count == 1 && countOfOtherRoomsWith2SharedPoints >= 2)
                {
                    Debug.Log("Symetrical layout, " + thisRoom.name + " is diagonal from " + targetRoom.name );
                    //change target room
                    //Debug.Break();
                    if(thisRoom.name != "LivingRoom")
                        continue;
                }
                
                //if we have 2 shared points, we have a simple wall with door to create in to target room
                if (sharedPoints.Count == 2)
                {
                    float distance = Vector3.Distance(sharedPoints[0], sharedPoints[1]);
                    //create list fof door point options along available wall
                    List<Vector3> doorOptions = new List<Vector3>();
                    Vector3 dir = (sharedPoints[1] - sharedPoints[0]).normalized;
                    //leaving a gap of 1 at each side // room size shouldnt push this too much - system isn't great but i tihnk it catches all problems
                    float gap = doorWidth;
                    for (float d = gap; d <= distance - gap; d += 0.1f)
                    {
                        Vector3 p = sharedPoints[0] + (dir * d);
                        if (floors == 1)
                        {
                            doorOptions.Add(p);
                        }
                        else if (floors == 2)
                        {
                            if (!stairCollider.GetComponent<BoxCollider>().bounds.Contains(p))
                            {
                                //GameObject ex = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                //ex.transform.position = p;
                                //ex.transform.parent = roomsAndSizes[i].room.transform;
                                //ex.transform.localScale *= 0.5f;
                                doorOptions.Add(p);
                            }
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
                        Debug.Break();
                        continue;
                    }
                    //build door using house builder class
                    List<List<GameObject>> doorThenWalls = DoorWithWall(thisRoom, sharedPoints[0], sharedPoints[1], doorPoint,false,false, storeyHeight, doorHeight, doorWidth, this);//dont miss door
                    //add to lsit for LODs
                    wallsReturned.Add(doorThenWalls[0][0]);

                    foreach(GameObject w in doorThenWalls[1])
                    {
                        wallsReturned.Add(w);
                    }

                    List<GameObject> doors = doorThenWalls[0];
                    doors[0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                    //make some rooms "private"
                    if (thisRoom.name == "Bedroom" || thisRoom.name == "Bathroom")
                    {
                        doors[0].transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("White") as Material;
                    }
                    //add to a list, we will use the door positions when planning room interiors
                    List<GameObject> doorList = doorThenWalls[0];
                    doorsBuilt.Add(doorList[0]);

                    //remember which edge we have already built
                    RoomAndEdge rae = new RoomAndEdge()
                    {
                        room = thisRoom,
                        edge = new Vector3[2] { sharedPoints[0], sharedPoints[1] }
                    };

                    roomsAndEdges.Add(rae);

                    //lremember where we ahve built a wall
                    WallWithDoor wwd = new WallWithDoor()
                    {
                        wallPoint1 = sharedPoints[0],
                        wallPoint2 = sharedPoints[1],
                        doorPoint = doorPoint,
                        parent = thisRoom,
                        target = targetRoom
                    };
                    
                  
                    wallsWithDoors.Add(wwd);
                    //force skip, we have found our door
                    r = 0;
                    

                }
                //else if we only have shared point with  
                else if (sharedPoints.Count == 1)
                {

                    //Debug.Log(thisRoom.name + " shared points 2");
                    //we need to discover the vertice of the other room, this lets us know where to stop the wall
                    //find closest point to the shared point we are trying to attach to(target Room)    
                    
                    Vector3 closestPointFromThisRoom = ClosestVerticeOnThisRoomToCentreOfTargetRoom(roomsAndSizes[i].room, targetRoom, sharedPoints[0]);//normal
                    Vector3 centreOfTarget = targetRoom.GetComponent<MeshRenderer>().bounds.center;
                    Vector3 closestPointFromTargetRoom = ClosestVerticeOnThisRoomToCentreOfTargetRoom(targetRoom,roomsAndSizes[i].room , sharedPoints[0]);                    
                    
                    //run through target room vertices and find which was in a straight line with shared point [0] and the closest point from this room
                    
                    Vector3[] othervertices = targetRoom.GetComponent<MeshFilter>().mesh.vertices;
                    
                    Vector3 p1 = Vector3.zero;
                    Vector3 p2 = Vector3.zero;
                    for (int v = 0; v < othervertices.Length; v++)
                    {
                        if (othervertices[v] == sharedPoints[0])
                            continue;

                        if (PointsInLine(sharedPoints[0], closestPointFromThisRoom, othervertices[v]))
                        {
                            //Debug.Log("Using 1st option" + thisRoom.name);
                            p1 = closestPointFromThisRoom;
                            p2 = othervertices[v];
                        }

                        if (PointsInLine(sharedPoints[0], othervertices[v], closestPointFromThisRoom))
                        {
                          //  Debug.Log("Using 2nd option" + thisRoom.name);
                            p1 = othervertices[v];
                            p2 = closestPointFromThisRoom;
                        }                        
                    }
                    bool buildExtraWall = false; //not using, this may be a soluition instad of raycasting in MissingWalls, not investigating atm
                    if(p1 == Vector3.zero)
                    {
                        buildExtraWall = true;
                        //Debug.Break();
                        Vector3[] thisvertices = thisRoom.GetComponent<MeshFilter>().mesh.vertices;
                        //we didnt find a suitable wall, using the target room's vertices, let's try this room's instead - note we are using the closest point form target room now
                        Debug.Log("points in line first count was zero, switchin to other room, this room is " + thisRoom.name + " Is living room at the end? no wal between kitchen?");
                        for (int v = 0; v < thisvertices.Length; v++)
                        {
                            if (thisvertices[v] == sharedPoints[0])
                                continue;

                            if (PointsInLine(sharedPoints[0], closestPointFromTargetRoom, thisvertices[v]))
                            {
                                p1 = closestPointFromTargetRoom;
                                p2 = othervertices[v];
                            }

                            if (PointsInLine(sharedPoints[0], thisvertices[v], closestPointFromTargetRoom))
                            {
                                p1 = thisvertices[v];
                                p2 = closestPointFromTargetRoom;
                            }
                        }
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
                        Debug.Break();

                    }

                    //Vector3 endPointForDoor = closestInLine;//testing
                    Vector3 endPointForDoor = closestInLine;
                    
                    //debug

                    #region cubes
                    
                    /*
                    GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
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

                   if(buildExtraWall)
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
                    float gap = doorWidth;
                    for (float d = gap; d <= distance-gap; d+=0.1f)
                    {
                        Vector3 p = sharedPoints[0] + (dir * d);
                        // GameObject ex = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        //ex.transform.position = p;
                        //ex.transform.parent = roomsAndSizes[i].room.transform;
                        //ex.transform.localScale *= 0.5f;

                        //add collider stair check? onlyneed if shared points = 2 like above?
                        if (floors == 1)
                            doorOptions.Add(p);
                        else if (floors == 2)
                        {
                            if (!stairCollider.GetComponent<BoxCollider>().bounds.Contains(p))
                            {
                                doorOptions.Add(p);
                            }
                        }
                        
                    }

                    //Debug.Log(doorOptions.Count + "door options " + "thisRoom = " + thisRoom.name + ", target room = " + targetRoom.name);
                    //randomly choose from this list
                    
                    Vector3 doorPoint = Vector3.Lerp(sharedPoints[0], endPointForDoor, 0.5f);

                    if (doorOptions.Count != 0)
                        doorPoint = doorOptions[Random.Range(0, doorOptions.Count)];
                    else
                    {
                        //Debug.Break();
                        Debug.Log("NO DOOR POINTS, shared points = 1");
                        continue;
                    }

                    //build the wall
                    List<List<GameObject>> doorThenWalls = DoorWithWall(thisRoom, sharedPoints[0], target, doorPoint,false,false, storeyHeight, doorHeight, doorWidth, this);//never skip door, need for room items - must delete later - save these to list?
                    //add for LODs
                    //door
                    wallsReturned.Add(doorThenWalls[0][0]);
                    //walls
                    foreach (GameObject w in doorThenWalls[1])
                        wallsReturned.Add(w);
                    List<GameObject> doors = doorThenWalls[0];
                    doors[0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;


                    //doors are built with glass as standard, we can hide the glass this way if room needs some 'privacy'
                    if(thisRoom.name == "Bedroom" || thisRoom.name == "Bathroom")
                    {
                        doors[0].transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("White") as Material;
                    }

                    //add for interiors
                    List<GameObject> doorList = doorThenWalls[0];
                    doorsBuilt.Add(doorList[0]);
                    //remember edge

                    //determine which way to buiid wall
                    Vector3 centre = roomsAndSizes[i].room.GetComponent<MeshRenderer>().bounds.center;
                    //create world positions                   

                    //edgesBuilt.Add(new Vector3[2] { closestPointToTarget, sharedPoints[0] });
                    RoomAndEdge rae = new RoomAndEdge();
                    rae.room = thisRoom;
                    rae.edge = new Vector3[2] {sharedPoints[0],target };
                    roomsAndEdges.Add(rae);

                    rae = new RoomAndEdge();
                    rae.room = targetRoom;
                    rae.edge = new Vector3[2] { sharedPoints[0], target };
                    roomsAndEdges.Add(rae);

                    //let the target room know we have placed a door here. It needs to know so it can also leave a gap for the door
                    WallWithDoor wwd = new WallWithDoor();
                    wwd.wallPoint1 = sharedPoints[0];
                    wwd.wallPoint2 = target;
                    wwd.doorPoint = doorPoint;
                    wwd.parent = thisRoom;
                    wwd.target = targetRoom;                 

                    wallsWithDoors.Add(wwd);

                    //force skip, we have found our door
                    r = 0;
                }
                //else if we have no shared points with target room, try next room(smaller)
                else if (sharedPoints.Count == 0)
                {
                    continue;
                }
            }            
        }

        //build walls around the doors we just placed

        //split rooms in to lists defined by how many doors are in them
        List<WallWithDoor> oneDoorRooms = new List<WallWithDoor>();
        List<WallWithDoor> twoDoorRooms = new List<WallWithDoor>();
        List<WallWithDoor> threeDoorRooms = new List<WallWithDoor>();
        for (int i = 0; i < roomAmount; i++)
        {
            List<WallWithDoor> tempList = new List<WallWithDoor>();
            //walls with doors list checks for any door points and build a wall around it
            for (int j = 0; j < wallsWithDoors.Count; j++)
            {
                GameObject thisRoom = roomsAndSizes[i].room;
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

            Vector3 closest = ClosestPointOnMesh(oneDoorRooms[j].wallPoint2, oneDoorRooms[j].target);
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
            List<List<GameObject>> doorThenWalls = DoorWithWall(oneDoorRooms[j].target, oneDoorRooms[j].wallPoint1, oneDoorRooms[j].wallPoint2, oneDoorRooms[j].doorPoint,false,false, storeyHeight, doorHeight, doorWidth, this);
            //LODS
            wallsReturned.Add(doorThenWalls[0][0]);
            foreach (GameObject w in doorThenWalls[1])
                wallsReturned.Add(w);

            List<GameObject> doors = doorThenWalls[0];
            doors[0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
            doorsBuilt.Add(doors[0]);
            doorsToRemove.Add(doors[0]);

            //build missing part of wall if any
            if(closest != oneDoorRooms[j].wallPoint1 && closest != oneDoorRooms[j].wallPoint2)
            {
                
                //make sure in line -USING WALLPOINT 2" all the time, possibly would need to check which is closer to "closest" between wallpoint1 and 2
                if (PointsInLine(oneDoorRooms[j].wallPoint1, closest, oneDoorRooms[j].wallPoint2))
                {
                    //find adjacent room, this room has two shared verts
                    for (int k = 0; k < roomsAndSizes.Count; k++)
                    {
                        List<Vector3> sharedPoints = SharedPointsWithTargetRoom(oneDoorRooms[j].parent, roomsAndSizes[k].room);
                        if (sharedPoints.Count == 2)
                        {
                            bool door = false;
                            //check no doors are here first
                            for (int m = 0; m < roomsAndSizes[k].room.transform.childCount; m++)
                            {
                                GameObject d = roomsAndSizes[k].room.transform.GetChild(m).gameObject;
                                if (d.name == "Door")
                                {
                                    if (PointsInLine(closest, oneDoorRooms[j].wallPoint2, d.transform.position))
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
                                Vector3 centre = roomsAndSizes[k].room.GetComponent<MeshRenderer>().bounds.center;
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

                                GameObject w1 = Wall(midPoint, Vector3.Distance(closest, oneDoorRooms[j].wallPoint2), lookDir, false, roomsAndSizes[k].room,storeyHeight);
                                w1.name = roomsAndSizes[k].room.ToString();
                                wallsReturned.Add(w1);

                                GameObject skirt = HouseBuilder.SkirtingWithNoDoor(roomsAndSizes[k].room, p1, p2, lookDir, this, 0);
                                wallsReturned.Add(skirt);

                                GameObject w2 = Wall(midPoint, Vector3.Distance(closest, oneDoorRooms[j].wallPoint2), -lookDir, false, oneDoorRooms[j].target,storeyHeight);
                                w2.name = oneDoorRooms[j].target.ToString() + " This Guy?";
                                wallsReturned.Add(w2);

                                skirt = HouseBuilder.SkirtingWithNoDoor(oneDoorRooms[j].target, p1, p2, -lookDir, this, 0);
                                wallsReturned.Add(skirt);

                                //remember this wall and door
                                //let the target room know we have placed a door here. It needs to know so it can also leave a gap for the door
                                WallWithDoor wwd = new WallWithDoor();
                                wwd.wallPoint1 = closest;
                                wwd.wallPoint2 = oneDoorRooms[j].wallPoint2;
                                wwd.doorPoint = Vector3.zero;
                                wwd.parent = roomsAndSizes[k].room;
                                wwd.target = oneDoorRooms[j].target;

                                wallsWithDoors.Add(wwd);

                                RoomAndEdge rae = new RoomAndEdge();
                                rae.room = oneDoorRooms[j].target;
                                rae.edge = new Vector3[2] { closest, oneDoorRooms[j].wallPoint2 };
                                roomsAndEdges.Add(rae);

                                rae = new RoomAndEdge();
                                rae.room = roomsAndSizes[k].room;
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
        List<WallWithDoor> roomsWithTwoDoors = new List<WallWithDoor>();
        
        for (int a = 0; a < twoDoorRooms.Count; a++)
        {              
            //Vector3[] edge = new Vector3[2] { twoDoorRooms[a].wallPoint1, twoDoorRooms[a].wallPoint2 };
            WallWithDoor ww2d = new WallWithDoor();
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
                    List<List<GameObject>> doorThenWalls = DoorWithWall(roomsWithTwoDoors[e].parent, roomsWithTwoDoors[e].wallPoint1, roomsWithTwoDoors[e].wallPoint2, roomsWithTwoDoors[e].doorPoint, true,false, storeyHeight, doorHeight, doorWidth, this);
                    //add for LODs
                    wallsReturned.Add(doorThenWalls[0][0]);
                    foreach (GameObject w in doorThenWalls[1])
                        wallsReturned.Add(w);

                    List<GameObject> doors = doorThenWalls[0];
                    
                    doors[0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                    doorsToRemove.Add(doors[0]);


                    //always the first point on the outside
                    edgePointsUsed.Add(roomsWithTwoDoors[e].wallPoint1);
                    //edgePointsUsed.Add(wallsWithTwoDoors[e].wallPoint2);


                    //remember build this - avoiding duplicates
                    // builtHere.Add(roomsWithTwoDoors[e].doorPoint);
                    RoomAndEdge rae = new RoomAndEdge();
                    rae.room = roomsWithTwoDoors[e].parent;
                    rae.edge = new Vector3[2] { roomsWithTwoDoors[e].wallPoint1, roomsWithTwoDoors[e].wallPoint2 };
                    roomsAndEdges.Add(rae);
                }
                else
                {
                    
                    //if we have a wall with ony one door??? where does this go?
                    //now find vertice closest to end of wall
                    Vector3 closest = ClosestPointOnMesh(roomsWithTwoDoors[e].wallPoint2, roomsWithTwoDoors[e].parent);

                    List<List<GameObject>> doorThenWalls = DoorWithWall(roomsWithTwoDoors[e].parent, roomsWithTwoDoors[e].wallPoint1, closest, roomsWithTwoDoors[e].doorPoint,true,false, storeyHeight, doorHeight, doorWidth,this); //always build door, we need them for item placement
                    //for LOD
                    wallsReturned.Add(doorThenWalls[0][0]);
                    foreach (GameObject w in doorThenWalls[1])
                        wallsReturned.Add(w);

                    List<GameObject> doors = doorThenWalls[0];

                    doors[0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;
                    doorsToRemove.Add(doors[0]);

                    //remember the edge
                    RoomAndEdge rae = new RoomAndEdge();
                    rae.room = roomsWithTwoDoors[e].parent;
                    rae.edge = new Vector3[2] { roomsWithTwoDoors[e].wallPoint1, closest};
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
                RoomAndEdge rae2 = new RoomAndEdge();
                rae2.room = wallsWithDoors[e].parent;
                rae2.edge = new Vector3[2] { edgePointsUsed[0], edgePointsUsed[1] };
               // roomsAndEdges.Add(rae2);

                edgePointsUsed = new List<Vector3>();
            }
        }
        
        List<WallWithDoor> roomsWithThreeDoors = new List<WallWithDoor>();

        for (int a = 0; a < threeDoorRooms.Count; a++)
        {
            //Vector3[] edge = new Vector3[2] { twoDoorRooms[a].wallPoint1, twoDoorRooms[a].wallPoint2 };
            WallWithDoor ww2d = new WallWithDoor();
            ww2d.wallPoint1 = threeDoorRooms[a].wallPoint1;
            ww2d.wallPoint2 = threeDoorRooms[a].wallPoint2;
            ww2d.doorPoint = threeDoorRooms[a].doorPoint;
            ww2d.parent = threeDoorRooms[a].parent;
            roomsWithThreeDoors.Add(ww2d);


        }

        for (int e = 0; e < roomsWithThreeDoors.Count; e++)
        {

            //simpler atm, i ahven't seen an example where we need to use the "two rooms loop" to find out which wall point to use, thisRoom, or targetRoom's

            List<List<GameObject>> doorThenWalls = DoorWithWall(roomsWithThreeDoors[e].parent, roomsWithThreeDoors[e].wallPoint1, roomsWithThreeDoors[e].wallPoint2, roomsWithThreeDoors[e].doorPoint, true,false, storeyHeight, doorHeight, doorWidth, this);
            //LODS
            wallsReturned.Add(doorThenWalls[0][0]);
            foreach (GameObject w in doorThenWalls[1])
                wallsReturned.Add(w);

        }

        return wallsReturned;
    }

    void ExteriorDoors()
    {
        //get last room/either living room or hall or kitchen
        GameObject room = roomsAndSizes[roomsAndSizes.Count - 1].room;
        //find exterior wall
        Vector3[] plotVertices = plotMesh.vertices;
        
        Vector3[] vertices = room.GetComponent<MeshFilter>().mesh.vertices;

        //loop vertices - we can do this because we always create the quads with the vertices in a clockwise order
        List<Vector3> loop = new List<Vector3>();

        foreach (Vector3 v3 in vertices)
            loop.Add(v3);

        loop.Add(vertices[0]);
        //find out which walls are exterior
        //if both wall points (start and finish) are between any two corners of the house, it is an outside wall
        //to check this
        //overal mesh
        //loop the plot vertices, let's not touch main variable
        List<Vector3> plotLoop = new List<Vector3>();
        foreach (Vector3 v3 in plotVertices)
            plotLoop.Add(v3);
        plotLoop.Add(plotVertices[0]);


        List<int[]> edges = new List<int[]>();
        for (int j = 0; j < loop.Count - 1; j++)
        {
            for (int k = 0; k < plotLoop.Count - 1; k++)
            {
                //lloks to see if points tested make a triangle or not
                //https://stackoverflow.com/questions/17692922/check-is-a-point-x-y-is-between-two-points-drawn-on-a-straight-line
                //are both "outside points" between two outside points
                //first point
                Vector3 A = plotLoop[k];
                Vector3 B = plotLoop[k + 1];
                Vector3 C = loop[j];
                bool onLine1 = false;
                float discrepancy = 0.01f;
                float smallDistance = Vector3.Distance(A, C) + Vector3.Distance(B, C);
                float largeDistance = Vector3.Distance(A, B);
                //within a range, small discrepancies can happen, is ok with what values w are working with
                if (smallDistance >= largeDistance - discrepancy && smallDistance <= largeDistance + discrepancy)
                    onLine1 = true;
                //second point
                C = loop[j + 1];
                smallDistance = Vector3.Distance(A, C) + Vector3.Distance(B, C);
                bool onLine2 = false;
                if (smallDistance >= largeDistance - discrepancy && smallDistance <= largeDistance + discrepancy)
                    onLine2 = true;

                if (onLine1 && onLine2)
                {
                    int[] edge = new int[2] { j, j + 1 };
                    edges.Add(edge);
                }
            }
        }

        int random = Random.Range(0, edges.Count);

        int[] chosenEdge = edges[random];

        //choose a space between the edge for a door
        float distance = Vector3.Distance(loop[chosenEdge[0]], loop[chosenEdge[1]]);
        //create list fof door point options along available wall
        List<Vector3> doorOptions = new List<Vector3>();
        Vector3 dir = (loop[chosenEdge[1]] - loop[chosenEdge[0]]).normalized;
        //leaving a gap of 1 at each side
        float gap = doorWidth * 0.5f;
        for (float d = gap; d <= distance - gap; d += 0.1f)
        {
            doorOptions.Add(loop[chosenEdge[0]] + (dir * d));
        }

        //randomly choose from this list
        //safety catch
        Vector3 doorPoint = Vector3.Lerp(loop[chosenEdge[0]], loop[chosenEdge[1]], 0.5f);
        if (doorOptions.Count != 0)
            doorPoint = doorOptions[Random.Range(0, doorOptions.Count)];
        else
            Debug.Break();

        //build door using house builder class

        //interior
        List<List<GameObject>> doorThenWalls = DoorWithWall(room, loop[chosenEdge[0]], loop[chosenEdge[1]], doorPoint, false,false,storeyHeight,doorHeight,doorWidth, this);//dont miss door
        List<GameObject> doors = doorThenWalls[0];
        doors[0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;

        //exterior
        doorThenWalls = DoorWithWall(room, loop[chosenEdge[0]], loop[chosenEdge[1]], doorPoint, true,true, storeyHeight, doorHeight, doorWidth, this);//dont miss door
        doors = doorThenWalls[0];
        doors[0].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;


    }

    void Exterior()
    {
        //find external wall
        Vector3[] plotVertices = plotMesh.vertices;
        for (int i = 0; i < roomsAndSizes.Count; i++)
        {
            Vector3[] vertices = roomsAndSizes[i].room.GetComponent<MeshFilter>().mesh.vertices;

            //loop vertices - we can do this because we always create the quads with the vertices in a clockwise order
            List<Vector3> loop = new List<Vector3>();

            foreach (Vector3 v3 in vertices)
                loop.Add(v3);

            loop.Add(vertices[0]);
            //find out which walls are exterior
            //if both wall points (start and finish) are between any two corners of the house, it is an outside wall
            //to check this
            //overal mesh
            //loop the plot vertices, let's not touch main variable
            List<Vector3> plotLoop = new List<Vector3>();
            foreach (Vector3 v3 in plotVertices)
                plotLoop.Add(v3);
            plotLoop.Add(plotVertices[0]);


            List<int[]> edges = new List<int[]>();
            for (int j = 0; j < loop.Count - 1; j++)
            {
                for (int k = 0; k < plotLoop.Count - 1; k++)
                {
                    //lloks to see if points tested make a triangle or not
                    //https://stackoverflow.com/questions/17692922/check-is-a-point-x-y-is-between-two-points-drawn-on-a-straight-line
                    //are both "outside points" between two outside points
                    //first point
                    Vector3 A = plotLoop[k];
                    Vector3 B = plotLoop[k + 1];
                    Vector3 C = loop[j];
                    bool onLine1 = false;
                    float discrepancy = 0.01f;
                    float smallDistance = Vector3.Distance(A, C) + Vector3.Distance(B, C);
                    float largeDistance = Vector3.Distance(A, B);
                    //within a range, small discrepancies can happen, is ok with what values w are working with
                    if (smallDistance >= largeDistance - discrepancy && smallDistance <= largeDistance + discrepancy)
                        onLine1 = true;
                    //second point
                    C = loop[j+1];
                    smallDistance = Vector3.Distance(A, C) + Vector3.Distance(B, C);
                    bool onLine2 = false;
                    if (smallDistance >= largeDistance - discrepancy && smallDistance <= largeDistance + discrepancy)
                        onLine2 = true;

                    if(onLine1 && onLine2)
                    {
                        //we have an ouside wall!
                        /*
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = loop[j];
                        cube.name = i.ToString();
                        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = loop[j+1];
                        cube.name = i.ToString();
                        */
                        int[] edge = new int[2] { j, j + 1 };
                        edges.Add(edge);
                    }
                }
            }
            bool buildDoor = false;
          //  bool doorBuilt = false;
            //build door first
            string largestRoomName = null;
            //if we are in largest room of house, place outside door
            if (roomAmount == 3)
            {
                largestRoomName = "Kitchen";
            }
            else if (roomAmount == 4)
            {
                largestRoomName = "LivingRoom";
            }
            else if (roomAmount == 5)
            {
                largestRoomName = "Hall";
            }

            if (roomsAndSizes[i].room.name == largestRoomName)
            {
                buildDoor = true;
            }

            if (buildDoor)
            {
                //find longest edge and place door here

                float distance = 0;
                int[] longestEdge = null;

                for (int k = 0; k < edges.Count; k++)
                {
                    int[] edgeToCheck = edges[k];
                    float temp = Vector3.Distance(loop[edgeToCheck[0]], loop[edgeToCheck[1]]);
                    if (temp > distance)
                    {
                        longestEdge = edgeToCheck;
                        distance = temp;
                    }
                }

                //remember this wall              
                RoomAndEdge rae = new RoomAndEdge();
                rae.room = roomsAndSizes[i].room;
                rae.edge = new Vector3[2] { loop[longestEdge[0]], loop[longestEdge[1]] };
                roomsAndEdges.Add(rae);
                //exteriorWalls.Add(rae);

                //choose how many segments this wall will have -- each segment can ahve one feature
                int segmentAmount = Random.Range(1, 4);
                float segmentLength = distance / segmentAmount;
                Vector3 direction = (loop[longestEdge[1]] - loop[longestEdge[0]]);
                Vector3 buildDir = direction / segmentAmount;
                List<Vector3> limits = new List<Vector3>();

                for (int s = 0; s <= segmentAmount; s++)
                {
                    Vector3 position = loop[longestEdge[0]] + (buildDir * s);
                    limits.Add(position);
                }

                List<Vector3> midPoints = new List<Vector3>();
                //get midPoints, these are the centre points for the features
                for (int m = 0; m < limits.Count - 1; m++)
                {
                    Vector3 midPoint = Vector3.Lerp(limits[m], limits[m + 1], 0.5f);
                    midPoints.Add(midPoint);
                }
                Vector3 lookDir = Quaternion.Euler(0, -90, 0) * (loop[longestEdge[1]] - loop[longestEdge[0]]);

                //select one of these midpoints to be a door
                int r = Random.Range(0, midPoints.Count);
                Vector3 doorPoint = midPoints[r];


                //create door
                //add rotations and transform positions


                GameObject door = DoorAtPosition(doorPoint, lookDir, roomsAndSizes[i].room,storeyHeight,doorHeight,doorWidth,this);
                door.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                //exterior walls
                List<GameObject> frontWalls = WallAroundDoor(doorPoint, segmentLength, lookDir, true, roomsAndSizes[i].room,storeyHeight, doorHeight,doorWidth);
                foreach (GameObject wall in frontWalls)
                    wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;

                //interior
                WallAroundDoor(doorPoint, segmentLength, lookDir, false, roomsAndSizes[i].room,storeyHeight,doorHeight,doorWidth);

                //remove from possible feature positions
                midPoints.RemoveAt(r);

                //put windows in the rest of the midpoints

                for (int m = 0; m < midPoints.Count; m++)
                {
                    GameObject window = WindowAtPosition(midPoints[m],doorHeight,windowHeight, windowWidth, lookDir, roomsAndSizes[i].room,storeyHeight, this);
                    window.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Glass") as Material;
                    //exterior
                    frontWalls = WallAroundWindow(midPoints[m], segmentLength, lookDir, true, roomsAndSizes[i].room,storeyHeight,windowHeight,windowWidth);
                    foreach (GameObject wall in frontWalls)
                        wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                    //interior
                    WallAroundWindow(midPoints[m], segmentLength, lookDir, false, roomsAndSizes[i].room,storeyHeight,windowHeight,windowWidth);
                }

                //now remove this edge from possible edges to build on
                edges.Remove(longestEdge);
            }


            //return;
            //don't think we can use this, windows will be governed by the placement of items in rooms
            for (int j = 0; j < edges.Count; j++)
            {

                //choose how many segments this wall will have -- each segment can have one feature
                int[] edge = edges[j];

                //remember this edge
                //         edgesBuilt.Add(new Vector3[2] { loop[edge[0]], loop[edge[1]] });
                RoomAndEdge rae = new RoomAndEdge();
                rae.room = roomsAndSizes[i].room;
                rae.edge = new Vector3[2] { loop[edge[0]], loop[edge[1]] };
                roomsAndEdges.Add(rae);
                exteriorWalls.Add(rae);

                

                float wallDistance = Vector3.Distance(loop[edge[0]], loop[edge[1]]);
                int segmentAmount = Random.Range(1, 4);
                if(roomsAndSizes[i].room.name == "Bathroom")
                    segmentAmount = 1;//*******************************
                float segmentLength = wallDistance / segmentAmount;

                Vector3 direction = (loop[edge[1]] - loop[edge[0]]);
                Vector3 buildDir = direction / segmentAmount;
                Vector3 lookDir = Quaternion.Euler(0, -90, 0) * (loop[edge[1]] - loop[edge[0]]);
                if (segmentAmount == 1)
                {
                    Vector3 mid = Vector3.Lerp(loop[edge[0]], loop[edge[1]], 0.5f);
                    GameObject wall = Wall(mid, segmentLength, lookDir, true, roomsAndSizes[i].room,storeyHeight);
                    wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;

                    wall = Wall(mid, segmentLength, lookDir, false, roomsAndSizes[i].room, storeyHeight);
                    continue;
                }
                

                List<Vector3> limits = new List<Vector3>();

                for (int s = 0; s <= segmentAmount; s++)
                {
                    Vector3 position = loop[edge[0]] + (buildDir * s);
                    limits.Add(position);
                }

                List<Vector3> midPoints = new List<Vector3>();
                //get midPoints, these are the centre points for the features
                for (int m = 0; m < limits.Count - 1; m++)
                {
                    Vector3 midPoint = Vector3.Lerp(limits[m], limits[m + 1], 0.5f);
                    midPoints.Add(midPoint);
                }

                

                //   float windowSize = 2f; //should make consistent through house?
                for (int m = 0; m < midPoints.Count; m++)
                {
                    GameObject window = WindowAtPosition(midPoints[m],doorHeight,windowHeight,windowWidth, lookDir, roomsAndSizes[i].room,3f, this);
                    window.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Glass") as Material;
                    //exterior
                    List<GameObject> walls = WallAroundWindow(midPoints[m], segmentLength, lookDir, true, roomsAndSizes[i].room,storeyHeight,windowHeight,windowWidth);
                    foreach (GameObject wall in walls)
                        wall.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;

                    //interior
                    WallAroundWindow(midPoints[m], segmentLength, lookDir, false, roomsAndSizes[i].room, storeyHeight, windowHeight, windowWidth);
                }
            }
        }
    }

    void MissingAdjacents()
    {
        for (int i = 0; i < roomsAndSizes.Count; i++)
        {
            for (int a = 0; a < listOfRoomsAndSharedPoints.Count; a++)
            {
                if (listOfRoomsAndSharedPoints[a][i].sharedPoints.Count == 2)
                {
                    

                    //determine which way to buiid wall
                    Vector3 centre = roomsAndSizes[i].room.GetComponent<MeshRenderer>().bounds.center;
                    //create world positions
                    Vector3 p1 = listOfRoomsAndSharedPoints[a][i].sharedPoints[0];
                    Vector3 p2 = listOfRoomsAndSharedPoints[a][i].sharedPoints[1];
                    Vector3 midPoint = Vector3.Lerp(p1, p2, 0.5f);

                    //create points each side of the line
                    Vector3 lookDir1 = Quaternion.Euler(0, 90, 0) * (p1 - p2).normalized;
                    Vector3 lookDir2 = Quaternion.Euler(0, -90, 0) * (p1 - p2).normalized;
                    Vector3 lookDir = Vector3.zero;
                    //check which is closest - use that rotation to build door
                    if (Vector3.Distance(midPoint + lookDir1, centre) > Vector3.Distance(midPoint + lookDir2, centre))
                        lookDir = Quaternion.Euler(0, 90, 0) * (p2 - p1).normalized;     //feed local coords to static -- static always applies rotations from room
                    else
                        lookDir = Quaternion.Euler(0, -90, 0) * (p2 - p1).normalized;    //feed local coords to static -- static always applies rotations from room

                    GameObject w = Wall(midPoint, Vector3.Distance(p1, p2), lookDir, false, roomsAndSizes[i].room, storeyHeight);
                    w.name = "ADJACENT";

                    //add to glboal list of edges we have built
                    //remember which edge we have already built
                    RoomAndEdge rae = new RoomAndEdge()
                    {
                        room = roomsAndSizes[i].room,
                        edge = new Vector3[2] { p1, p2}
                    };

                    roomsAndEdges.Add(rae);

                }
            }
        }
    }

    void MissingWalls()
    {
        //we have been keeping a check on what walls we have built for each room. For the final method, let's check
        //for any missing secttions of wall


        for (int i = 0; i < roomsAndSizes.Count; i++)
        {
            List<Vector3[]> edges = new List<Vector3[]>();
            for (int j = 0; j < roomsAndEdges.Count; j++)
            {
                //grab all edges which belong to this room
             
                if (roomsAndEdges[j].room == roomsAndSizes[i].room)
                    edges.Add(roomsAndEdges[j].edge);

            }
            List<Vector3[]> endEdges = new List<Vector3[]>();
            Debug.Log(edges.Count + roomsAndSizes[i].room.name);
            //to find the hole, find out what edges have matches- if we get 2 it means they are in a linked formation. If we get 1, it means it is an end point
            for (int a = 0; a < edges.Count; a++)
            {
                int count1 = 0;
                //int count2 = 0;

                for (int b = 0; b < edges.Count; b++)
                {
                    if (a == b)
                        continue;


                    bool match1 = false;
                    if (edges[a][0] == edges[b][0] || edges[a][0] == edges[b][1])
                    {
                        match1 = true;
                    }
                    bool match2 = false;
                   if (edges[a][1] == edges[b][0] || edges[a][1] == edges[b][1])
                    {
                        match2 = true;
                    }



                    if (match1 || match2 )
                    {
                        count1++;
                    }
                }

                //this is an edge which only has one link
                if (count1 == 1)
                {
                    endEdges.Add(edges[a]);
                }
            }

            //we now should ahve two edges. Find the closest point to the other edge
            Vector3 closest = endEdges[1][1];
            Vector3 midPoint = Vector3.Lerp(endEdges[0][0], endEdges[0][1], 0.5f);

            if (Vector3.Distance(endEdges[1][0], midPoint) < Vector3.Distance(closest, midPoint))
                closest = endEdges[1][0];

            Vector3 closest2 = endEdges[0][1];
            Vector3 midPoint2 = Vector3.Lerp(endEdges[1][0], endEdges[1][1], 0.5f);

            if (Vector3.Distance(endEdges[0][0], midPoint) < Vector3.Distance(closest, midPoint))
                closest = endEdges[0][0];

            GameObject d = GameObject.CreatePrimitive(PrimitiveType.Cube);
            d.transform.position = closest;
            d.name = "closest";
            d.transform.parent = roomsAndSizes[i].room.transform;


            d = GameObject.CreatePrimitive(PrimitiveType.Cube);
            d.transform.position = closest2;
            d.name = "closest2";
            d.transform.parent = roomsAndSizes[i].room.transform;

        }
    }

    List<GameObject> MissingInternals() //is lookDir muddled up, usually same vector then spun
    {
        List<GameObject> wallsReturned = new List<GameObject>();

        List<Vector3[]> interiorEdges = new List<Vector3[]>();

        for (int i = 0; i < roomsAndSizes.Count; i++)
        {
            List<Vector3> wallPositions = new List<Vector3>();
            //now find any internal wall that hasnt been built and build a windowless wall
            //use edgeBuilt list which we populated with every edge we have built
            //deduce which edge(s) is missing

            //skipppin last room, all walls built - this function only uilds walls between rooms which have no door. All rooms atm going to last rom have alrady built dopors and walls
            //This could possibly change and a better solution will be needed


            //we need to avoid doors, get list of doors in this room
            List<GameObject> doors = new List<GameObject>();
            for (int d = 0; d < roomsAndSizes[i].room.transform.childCount; d++)
            {
                GameObject g = roomsAndSizes[i].room.transform.GetChild(d).gameObject;
                if (g.name == "Door")
                {
                    doors.Add(g);
                }
            }

            //  Debug.Log(roomsAndSizes[i].room.ToString() + doors.Count);


            //create a list of vertices for this room

            List<Vector3[]> allEdges = new List<Vector3[]>();

            Vector3[] vertices = roomsAndSizes[i].room.GetComponent<MeshFilter>().mesh.vertices;

            for (int j = 0; j < vertices.Length - 1; j++)
            {
                Vector3[] e = new Vector3[2] { vertices[j], vertices[j + 1] };
                allEdges.Add(e);
            }

            //close loop
            Vector3[] lastEdge = new Vector3[2] { vertices[vertices.Length - 1], vertices[0] };
            allEdges.Add(lastEdge);

            //loop of plot mesh
            List<Vector3> plotLoop = new List<Vector3>();
            Vector3[] plotVertices = plotMesh.vertices;
            foreach (Vector3 v3 in plotVertices)
                plotLoop.Add(v3);
            plotLoop.Add(plotVertices[0]);

            //noDoors = true;
            List<Vector3[]> exteriorEdges = new List<Vector3[]>();

            for (int j = 0; j < allEdges.Count; j++)
            {
                Vector3[] edge = allEdges[j];

                //is this an exterior wall?
                //is edge between two points
                for (int p = 0; p < plotLoop.Count - 1; p++)
                {
                    bool first = PointsInLine(plotLoop[p], plotLoop[p + 1], allEdges[j][0]);

                    bool second = PointsInLine(plotLoop[p], plotLoop[p + 1], allEdges[j][1]);

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
                        ex.transform.parent = roomsAndSizes[i].room.transform;
                        ex = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        ex.transform.position = exteriorEdges[k][1];
                        ex.name = "EXT 1";
                        ex.transform.parent = roomsAndSizes[i].room.transform;
                        */
                    }
                }

                if (!exteriorEdge)
                {
                    /*
                    GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    s.transform.position = allEdges[j][0];
                    s.transform.parent = roomsAndSizes[i].room.transform;
                    s.name = "edge 0";
                    s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    s.transform.position = allEdges[j][1];
                    s.transform.parent = roomsAndSizes[i].room.transform;
                    s.name = "edge 1";
                    */


                    //dont add duplicates, including if edge is reversed
                    bool duplicate = false;
                    for (int l = 0; l < interiorEdges.Count; l++)
                        if (interiorEdges[l][0] == allEdges[j][0] && interiorEdges[l][1] == allEdges[j][1] || interiorEdges[l][1] == allEdges[j][0] && interiorEdges[l][0] == allEdges[j][1])
                        {
                            //Debug.Log("duplicate " + roomsAndSizes[i].room.name);
                            duplicate = true;
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
                            if (PointsInLine(allEdges[j][0], allEdges[j][1], doors[d].transform.position))
                            {
                                //do not build a wall here, walls already built around door by now
                                noDoors = false;


                                //if there is a door, find the other room which this door goes to - when doors are built, they are added to a global list - find the matching door

                                foreach (GameObject door in doorsBuilt)
                                {
                                    if (doors[d].transform.position == door.transform.position && door.transform.parent != doors[d].transform.parent)
                                    {
                                        //GameObject a = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                        //a.transform.position = door.transform.position;
                                        // a.transform.parent = roomsAndSizes[i].room.transform;
                                        // a.name = "Matching door going to " + door.transform.parent.name;
                                        GameObject targetRoom = door.transform.parent.transform.gameObject;

                                        //this room's vertice
                                        Vector3[] targetVertices = targetRoom.GetComponent<MeshFilter>().mesh.vertices;

                                        for (int v = 0; v < targetVertices.Length; v++)
                                        {
                                            if (targetVertices[v] == allEdges[j][0] || targetVertices[v] == allEdges[j][1])
                                            {
                                                //if any matching vertices, skip
                                            }
                                            else
                                            {
                                                if (PointsInLine(allEdges[j][0], allEdges[j][1], targetVertices[v]))
                                                {
                                                    //find out which edge point to build to from V - the one which is further away from the door

                                                    //a = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                                    // a.transform.position = targetVertices[v];
                                                    // a.transform.parent = roomsAndSizes[i].room.transform;
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
                                                        if (PointsInLine(targetVertices[v], target, door2.transform.position))
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
                                                        Vector3 centre = roomsAndSizes[i].room.GetComponent<MeshRenderer>().bounds.center;
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

                                                        //GameObject w1 = Wall(midPoint, Vector3.Distance(p1, p2), lookDir, false, roomsAndSizes[i].room, storeyHeight);
                                                        //w1.name = roomsAndSizes[i].room.ToString() + " NEW WALL";

                                                        GameObject w1 = HouseBuilder.WallWithLookDirection(roomsAndSizes[i].room, p1, p2, storeyHeight, doorHeight, doorWidth, 1, false, this);
                                                        w1.name ="TestWall1";
                                                        //save this wall position, beneatth this we look for other missed walls but sometimes a duplicate can be built, because this solution hasn't been as perfect as I hoped
                                                        wallPositions.Add(midPoint);
                                                        //Debug.Break();

                                                        wallsReturned.Add(w1);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (roomsAndSizes[i].room.name == "Kitchen")
                        {
                            // Debug.Log("Kitchen edge " + "no doors  = " + noDoors.ToString());
                        }

                        if (noDoors)
                        {
                            interiorEdges.Add(allEdges[j]);

                            //determine which way to buiid wall
                            Vector3 centre = roomsAndSizes[i].room.GetComponent<MeshRenderer>().bounds.center;
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
                                //GameObject w1 = Wall(midPoint, Vector3.Distance(p1, p2), lookDir, false, roomsAndSizes[i].room,storeyHeight);
                                //w1.name = roomsAndSizes[i].room.ToString() + " w1";

                                GameObject w1 = HouseBuilder.WallWithLookDirection(roomsAndSizes[i].room, p1, p2, storeyHeight, doorHeight, doorWidth, 1, false, this);
                                w1.name = "TestWall2";
                                wallsReturned.Add(w1);
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
                                Physics.Raycast(shootFrom, Vector3.down, out hit, 0.2f);//will need to add layer in delivery project
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

                            //if both raycast hit the same room, great, we can build the wall and parent it this hit room
                            if (hits[0].transform == hits[1].transform)
                            {
                                //check for a wall already built
                                if (!wallPositions.Contains(midPoint))
                                {
                                    //manually doing this one cos we need to swap look rotation
                                    GameObject w2 = Wall(midPoint, Vector3.Distance(p1, p2), -lookDir, false, roomsAndSizes[i].room,storeyHeight);
                                    //GameObject w2 = HouseBuilder.WallWithLookDirection(roomsAndSizes[i].room, p1, p2, storeyHeight, doorHeight, doorWidth, 1, false, this);
                                    w2.transform.parent = hits[0].transform;
                                    w2.name = "TestWall3";

                                    GameObject skirt = SkirtingWithNoDoor(hits[0].transform.gameObject, p1, p2, -lookDir, this,0);
                                    interiorAssetsByRoom.Add(new List<GameObject>() { skirt });

                                    wallsReturned.Add(w2);

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

                                continue;//catchihg this above
                                //now build the wall between each room's vertice and the closest vertice to the middle
                                /*
                                midPoint = Vector3.Lerp(p1, closest, 0.5f);
                                GameObject w2 = Wall(midPoint, Vector3.Distance(p1, closest), -lookDir, false, hits[0].transform.gameObject, storeyHeight);
                                midPoint = Vector3.Lerp(p2, closest, 0.5f);
                                GameObject w3 = Wall(midPoint, Vector3.Distance(p2, closest), -lookDir, false, hits[1].transform.gameObject, storeyHeight);
                                */
                            }
                        }
                    }
                }
            }
        }

        return wallsReturned;
    }  

    List<GameObject> Corners(int floors)
    {

        List<GameObject> corners = new List<GameObject>();
        //exterior wall miss the very corners of the house..
        Vector3[] vertices = plotMesh.vertices;

        //finish loop
        Vector3 left = vertices[vertices.Length-1];
        Vector3 right = vertices[0];
        GameObject column = CornerColumn(left, right, gameObject,floors,storeyHeight);
        column.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
        corners.Add(column);
        for (int i = 0; i < vertices.Length - 1; i++)
        {
            left = vertices[i];
            right = vertices[i + 1];
            column = CornerColumn(left, right, gameObject,floors,storeyHeight);
            column.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
            corners.Add(column);
        }
        return corners;
    }

    void Outside()
    {
        GameObject targetRoom = null;
        if (roomAmount == 3)
        {
            targetRoom = transform.Find("Kitchen").gameObject;
        }
        else if (roomAmount == 4)
        {
            targetRoom = transform.Find("LivingRoom").gameObject;
        }

        //find longest edge from centre
        Vector3[] vertices = targetRoom.GetComponent<MeshFilter>().mesh.vertices;

        //find closest edge/point to road
        //transform is rotated to face road, so plot a point at transfomr.forward (or use hitRoad v3?)


        float distance = 0f;
        //edge
        int[] furthest = new int[2];
        for (int i = 0; i < vertices.Length - 1; i++)
        {

            Vector3 midPoint = Vector3.Lerp(vertices[i], vertices[i + 1], 0.5f);


            float temp = Vector3.Distance(midPoint, centreOfPlot);

            if (temp > distance)
            {
                furthest = new int[2] { i, i + 1 };
                distance = temp;
            }

        }

        //last link
        Vector3 midPointLast = (Vector3.Lerp(vertices[vertices.Length - 1], vertices[0], 0.5f));
        float distanceLast = Vector3.Distance(midPointLast, centreOfPlot);


        if (distanceLast > distance)
        {
            furthest = new int[2] { vertices.Length - 1, 0 };
            Debug.Log("last");
        }

        //put door on this wall/edge

        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.transform.position = Vector3.Lerp(vertices[furthest[0]], vertices[furthest[1]], 0.5f);
        door.transform.parent = targetRoom.transform;
        door.transform.localScale *= 0.1f;
        door.name = "Outside Door";
        door.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;

    }

    List<GameObject> FillRooms(List<GameObject> quads)
    {

        //bedroom and bath loop
        for (int i = 0; i < quads.Count; i++)//roomnumber - only doing bedroom and bathroom this way
        {

            //if bathroom
            if (quads[i].name == "Bathroom")
            {
                List<ObjectAndSize> listForThisRoom1 = objectsBathroom1;
                //List<ObjectAndSize> listForThisRoom2 = objectsBathroom1;
                
                //place interior objects using just colliders
                List<GameObject> objectsFinal = RoomLayouts.BathroomLayout(listForThisRoom1, this, quads[i]);

              
                InteriorAssets.BathroomInteriorAssets(quads[i], objectsFinal,this);

                //add this list to a list of interior assets to combine later for performance reasons
                interiorAssetsByRoom.Add(objectsFinal);
            }

            //get correct list of objects for room
            if (quads[i].name == "Bedroom")
            {
                List<ObjectAndSize> listForThisRoom1 = objectsBedroom1;
                //List<ObjectAndSize> listForThisRoom2 = objectsBedroom2;

                List<GameObject> objectsFinal = RoomLayouts.BedroomLayout(listForThisRoom1, this, quads[i]);

                InteriorAssets.BedroomInteriorAssets(quads[i], objectsFinal);

                //add this list to a list of interior assets to combine later for performance reasons
                interiorAssetsByRoom.Add(objectsFinal);
                continue;
            }

            if (quads[i].name == "Kitchen")
            {
                //place objects
                List<GameObject> objectsForKitchen = RoomLayouts.KitchenLayout(quads[i]);
                //make models
                InteriorAssets.KitchenInteriorAssets(quads[i], objectsForKitchen);

                //add this list to a list of interior assets to combine later for performance reasons
                interiorAssetsByRoom.Add(objectsForKitchen);

                continue;
            }

            if (quads[i].name == "LivingRoom")
            {

                List<GameObject> objectsForLivingRoom = RoomLayouts.LivingRoomLayout(quads[i],this);
                objectsForLivingRoom = RoomLayouts.LivingRoomLayoutV2(quads[i],objectsLivingroom, objectsForLivingRoom, this);
                InteriorAssets.LivingroomInteriorAssets(quads[i], objectsForLivingRoom);

                //add this list to a list of interior assets to combine later for performance reasons
                interiorAssetsByRoom.Add(objectsForLivingRoom);

                continue;
            }
            if (quads[i].name == "Hall")
            {

                continue;
            }
        }
        return quads;
    }

    void RoomFloors(List<GameObject> quads)
    {
        //do here so all hall tiles are the same wood
        float hallX = Random.Range(0.9f, 0.9f);
        float hallZ = Random.Range(0.1f, 0.3f);
        

        for (int i = 0; i < quads.Count; i++)
        {
            if (quads[i].name == "Bedroom" || quads[i].name == "LivingRoom")
                Floors.Carpet(quads[i], 32, 0.05f,this);

            if (quads[i].name == "Kitchen" || quads[i].name == "Bathroom")
            {
                float  x = Random.Range(0.1f, 0.9f);
                float z = x + Random.Range(-0.3f, 0.3f);
                z = Mathf.Clamp(z, 0.1f, 0.9f);
                Floors.Tiled(quads[i], x, z, Random.Range(0.01f, 0.09f), this);
            }

            if (quads[i].name == "Hall" || quads[i].name == "Hall1" || quads[i].name == "Hall2" || quads[i].name == "Landing" || quads[i].name == "LandingClose")
            {
                //Floors.Carpet(quads[i], 32, 0.05f);

               
                Floors.FloorBoards(quads[i], hallX, hallZ, 0.01f, 1f, this);
            }

        }
    }       
    
    public static List <Vector3> BorderPointsForRoom(GameObject room,float border)
    {
        GameObject door = room.transform.Find("Door").gameObject;
        Vector3[] vertices = room.GetComponent<MeshFilter>().mesh.vertices;
        //make loop
        vertices = new Vector3[] { vertices[0], vertices[1], vertices[2], vertices[3], vertices[0] };
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].y += room.transform.position.y;
        }
            

        List<Vector3> points = IntersectionPoints(vertices, border); //0.1 is inside wall depth
        //make loop
        points.Add(points[0]);
        //find the furthest vertice from the door
        float distance = 0f;
        int furthest = 0;
        for (int i = 0; i < points.Count; i++)
        {
            float temp = Vector3.Distance(door.transform.position, points[i]);
            if (temp > distance)
            {
                distance = temp;
                furthest = i;
            }
        }
        return points;
    }

    public List<Vector3> StraightPatternForRoom(GameObject room, List<Vector3> points)
    {
        //create object pattern from door to wall, and then perpendicular from here. Room with 3 or more uses a corner to start the pattern
        //list of places items can be
        List<Vector3> options = new List<Vector3>();

        List<GameObject> doors = new List<GameObject>();
        for (int i = 0; i < room.transform.childCount; i++)
        {
            if (room.transform.GetChild(i).name == "Door")
                doors.Add(room.transform.GetChild(i).gameObject);
        }
        GameObject door = doors[doors.Count - 1]; 
        //start it off with door to furthest intersectio point across
        Vector3 startingPoint = door.transform.position;

      //  GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
      //  c.transform.position = startingPoint;
      //  c.name = "starting point";
      //  c.transform.parent = room.transform;

        //find intersection points on the border
        List<Vector3> intersectionPoints = new List<Vector3>();

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 intersect;
            bool intersected = LineLineIntersection(out intersect, startingPoint, -door.transform.forward, points[i], points[i + 1] - points[i]);
            if (intersected)
            {



                //remove any intersection not in room bounds
               // if (room.GetComponent<MeshCollider>().bounds.Contains(intersect))
                {
                    
                    intersectionPoints.Add(intersect);
                }
            }
        }

        //now we have a list of points on the border, what we need is the second closest point to 'halfway'
        //sort list by distance using this lovely piece of code
        intersectionPoints.Sort(delegate (Vector3 v1, Vector3 v2)
        {
            return Vector3.Distance(door.transform.position, v1).CompareTo
                        ((Vector3.Distance(door.transform.position, v2)));
        });

        //second one is furthest away from the door
        Vector3 endPoint = intersectionPoints[1];

        //save        -- dont save first point across from door, makes it too symmetrical?
        //options.Add(endPoint);

        PositionAndDirection pod = new PositionAndDirection();
        pod.position = startingPoint;
        pod.direction = endPoint;
        debugLines.Add(pod);


        int objects = 10;
        for (int j = 0; j < objects-1; j++) //3 + 1 which we worked out above = 4 walls
        {
            //halfway between door and corner
            Vector3 halfWay = Vector3.Lerp(startingPoint, endPoint,0.3f);//been playin around with this..

            //create a directional vector 
            Vector3 dir = endPoint - startingPoint;


            dir = Quaternion.Euler(0, 90, 0) * dir;

            //find intersection points on the border
            intersectionPoints = new List<Vector3>();

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 intersect;
                bool intersected = LineLineIntersection(out intersect, halfWay, dir, points[i], points[i + 1] - points[i]);
                if (intersected)
                {
                  //  GameObject c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                  //  c.transform.position = intersect;
                   // c.transform.parent = room.transform;
                   // c.name = "intersect";
                   // c.transform.localScale *= 0.5f;
                  //  c.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;
                    //remove any intersection not in room bounds
                    //if (room.GetComponent<MeshCollider>().bounds.Contains(intersect))//using this creates bugs..
                    {

                     //   c.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("White") as Material;
                        intersectionPoints.Add(intersect);
                    }
                }
            }
           // Debug.Log("interesection points count b4 " + intersectionPoints.Count);
            //now we have a list of points on the border, what we need is the second closest point to 'halfway'
            //sort list by distance using this lovely piece of code
            intersectionPoints.Sort(delegate (Vector3 v1, Vector3 v2)
            {
                return Vector3.Distance(halfWay, v1).CompareTo
                            ((Vector3.Distance(halfWay, v2)));
            });

            //second point is the one we need

            //Debug.Log("interesection points count " + intersectionPoints.Count);
            //on first, use the longer points to the wall, else use shorter. This creates a circular pattern starting with the point which will give the most space --nope
            Vector3 intersectPointToUse = intersectionPoints[1];
            if (j != 0)
                intersectPointToUse = intersectionPoints[1];
            //check if too close to door, if it is, we have found all the points that will be useful-continuing anyway
            float distanceToDoor = Vector3.Distance(door.transform.position, intersectPointToUse);
            if (distanceToDoor > 1f)
            {
                options.Add(intersectPointToUse);
            }


            //now use intersection point and halfway of previous vector
            startingPoint = halfWay;
            //also save last added point for end pint on next loop
            endPoint = intersectPointToUse;

            pod = new PositionAndDirection();
            pod.position = startingPoint;
            pod.direction = endPoint;
            debugLines.Add(pod);

            

        }

        //round options to 1dt - e.g snap to grid
        for (int i = 0; i < options.Count; i++)
        {
            //round to 1dp
            float x = options[i].x;
            x *= 10;
            x = Mathf.Round(x);
            x *= 0.1f;
            //   Debug.Log(x);
            float z = options[i].z;
            z *= 10;
            z = Mathf.Round(z);
            z *= 0.1f;

            options[i] = new Vector3(x, options[i].y, z);
        }

        //reverse list so the last point found here is the largest object in the room. This is opposite to skewed/ corner pattern where corner object is largest - note. largest != most important
        options.Reverse();

        return options;
    }

    public List<Vector3> SkewedPatternForRoom(GameObject room, List<Vector3> points)
    {
        List<GameObject> doors = new List<GameObject>();
        for (int i = 0; i < room.transform.childCount; i++)
        {
            if (room.transform.GetChild(i).name == "Door")
                doors.Add(room.transform.GetChild(i).gameObject);
        }
        GameObject door = doors[0];
        //find the furthest vertice from the door
        float distance = 0f;
        int furthest = 0;
        for (int i = 0; i < points.Count; i++)
        {
            float temp = Vector3.Distance(door.transform.position, points[i]);
            if (temp > distance)
            {
                distance = temp;
                furthest = i;
            }
        }

        //list of places items can be
        List<Vector3> options = new List<Vector3>();
        //add this corner
        options.Add(points[furthest]);

        //start it off with door to furthest corner from door
        Vector3 startingPoint = door.transform.position;
        Vector3 endPoint = points[furthest];

        PositionAndDirection pod = new PositionAndDirection();
        pod.position = startingPoint;
        pod.direction = endPoint;
        debugLines.Add(pod);

        //room.transform.localScale += new Vector3(0.1f, 0.0f, 0.1f);
        for (int j = 0; j < 10; j++) //3 main abjects of room
        {
            //halfway between door and corner
            Vector3 halfWay = Vector3.Lerp(startingPoint, endPoint, 0.5f);

            //create a directional vector 
            Vector3 dir = endPoint - startingPoint;

            //rotate 90 degrees
            dir = Quaternion.Euler(0, 90, 0) * dir;

            //find intersection points on the border
            List<Vector3> intersectionPoints = new List<Vector3>();
            
            for (int i = 0; i < points.Count - 1; i++)
            {
                

                Vector3 intersect;
                bool intersected = LineLineIntersection(out intersect, halfWay, dir, points[i], points[i + 1] - points[i]);
                if (intersected)
                {
                   // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                   // c.transform.position = intersect;
                   // c.transform.parent = room.transform;
                   // c.name = "intersect";
                   // c.transform.localScale *= 0.5f;
                  //  c.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;
                    //remove any intersection not in room bounds
                    if (room.GetComponent<MeshCollider>().bounds.Contains(intersect))
                    {
                        intersectionPoints.Add(intersect);
                      //  c.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("White") as Material;
                    }
                }
            }

            //now we have a list of points on the border, what we need is the second closest point to 'halfway'
            //sort list by distance using this lovely piece of code
            intersectionPoints.Sort(delegate (Vector3 v1, Vector3 v2)
            {
                return Vector3.Distance(halfWay, v1).CompareTo
                            ((Vector3.Distance(halfWay, v2)));
            });

            if(intersectionPoints.Count < 2)
            {
                Debug.Log("Not enough space in room for layout - solution needed");
                /*
                //could destroy interiorwall and expand hall, or make a cupboard
                bool makeCupboard = true;
                if(makeCupboard)
                {
                    room.name = "HallCupboard";
                    room.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Yellow") as Material;
                    Debug.Break();
                    Debug.Log("Hall cupboard break");
                }
                */
                
                continue;
                
            }
            //second point is the one we need
            //Debug.Log("intersection points count  = " + intersectionPoints.Count);
            //check if too close to door, if it is, we have found all the points that will be useful-continuing anyway
            float distanceToDoor = Vector3.Distance(door.transform.position, intersectionPoints[intersectionPoints.Count-1]);
            if (distanceToDoor > 1f)
            {
                options.Add(intersectionPoints[intersectionPoints.Count-1]);
            }

            
            //now use intersection point and halfway of previous vector
            startingPoint = halfWay;
            //also save last added point for end pint on next loop
            endPoint = intersectionPoints[intersectionPoints.Count-1];

            pod = new PositionAndDirection();
            pod.position = startingPoint;
            pod.direction = endPoint;
            debugLines.Add(pod);


        }
        //room.transform.localScale -= new Vector3(0.1f, 0.0f, 0.1f);
        //round options to 1dt - e.g snap to grid
        for (int i = 0; i < options.Count; i++)
        {
            //round to 1dp
            float x = options[i].x;
            x *= 10;
            x = Mathf.Round(x);
            x *= 0.1f;
            //   Debug.Log(x);
            float z = options[i].z;
            z *= 10;
            z = Mathf.Round(z);
            z *= 0.1f;

            options[i] = new Vector3(x, options[i].y, z);
        }

        return options;
    }

    public List<GameObject> PlaceObjectsAtOptions(GameObject room, List<Vector3> options, List<Vector3> points, List<ObjectAndSize> objectsForRoom,bool straightPatternUNUSED)
    {
        List<GameObject> objectsToBuild = new List<GameObject>();
        
        List<GameObject> doors = new List<GameObject>();
        for (int i = 0; i < room.transform.childCount; i++)
        {
            if (room.transform.GetChild(i).name == "Door")
                doors.Add(room.transform.GetChild(i).gameObject);
        }
        GameObject door = doors[doors.Count - 1];

        //Debug.Log(points.Count + " points count");
        //Debug.Log(objectsForRoom.Count + " objects for room count");
        for (int i = 0; i < options.Count; i++)//was optionsForRoom list
        {
            //we have used all out objects
            if (attemptedObjects >= objectsForRoom.Count)
                continue;

            //first large object tends to look perpendicular to room shape** changed so that it looks along longest wall, easier to get in to facing objects this way i tihnk- still unsure- what would humans do lol?

            //is object in corner?
            bool inCorner = false;

            //Debug.Log("Option count = " + options.Count);
           // Debug.Log("Points count = " + points.Count);

            for (int j = 0; j < points.Count; j++)
            {

                if (options[i] == points[j])
                    inCorner = true;
            }
            //find nearest two vertices to the corner we are in
            List<Vector3> pointsSortedByDistance = new List<Vector3>();
            for (int p = 0; p < points.Count - 1; p++)
            {
                //-1 because points has same end and start for index out of range protection
                //we do not want duplicates here
                pointsSortedByDistance.Add(points[p]);

            }

            pointsSortedByDistance.Sort(delegate (Vector3 v1, Vector3 v2)
            {
                return Vector3.Distance(options[i], v1).CompareTo
                            ((Vector3.Distance(options[i], v2)));
            });
            //points we need ar [1] and [2], [0] is the closest point and should be the point we are using, e.g options[i]

            Vector3 p1 = pointsSortedByDistance[1];
            Vector3 p2 = pointsSortedByDistance[2];

            //which way is the best way to face? point away from next object - if object is last in the list, point away from first
            if (inCorner)
            {
               // Debug.Log("corner");
                //we need the previous point too, so we have three points to measure two angles from - first point will be door, this isn't in the list
                //otherwise, use the pevious point in options
                Vector3 prevPoint = door.transform.position;
                if (i != 0)
                    prevPoint = options[i - 1];

                //directional vectors
                //direction from corner to 1st vertice
                Vector3 dir1 = options[i] - p1;
                //direction from corner to second nearest vertice
                Vector3 dir2 = options[i] - p2;
                //direction from corenr to prev point
                Vector3 dirFromCornerToPrevious = options[i] - prevPoint;
                
                //find larger angle and face the object this way - essentially point object in to larger area
                //angle 1
                float angle1 = Vector3.Angle(dirFromCornerToPrevious, dir1);
                //angle 2
                float angle2 = Vector3.Angle(dirFromCornerToPrevious, dir2);
                //decide direction
                Vector3 objectForward = -dir1;
                Vector3 sideDir = -dir2;
                
                if (angle2 < angle1)
                {
                    objectForward = -dir2;
                    sideDir = -dir1;
                }
                
                
                GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                c.GetComponent<MeshRenderer>().enabled = false;
                c.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                c.transform.rotation = Quaternion.LookRotation(objectForward);
                c.transform.position = options[i];
                c.transform.position += (objectForward.normalized * 0.5f * objectsForRoom[i].size.z) + (sideDir.normalized * 0.5f * objectsForRoom[i].size.x);
                c.transform.localScale = objectsForRoom[i].size;
                c.transform.parent = room.transform;

                c.name = objectsForRoom[attemptedObjects].name;
                objectsToBuild.Add(c);

                attemptedObjects++;

            }
            else if(!inCorner)
            {
                //if not in corenr, object looksin to middle of room

              //  Debug.Log("normal");
                //nearest point to this point
                Vector3 dir = (pointsSortedByDistance[0] - options[i]).normalized;
                
                //find out if left or right to centre of room
                //find what way towards centre of the room
                //is centre of the room on the left or the right hand side of rotated object        
                Vector3 toCentre = room.GetComponent<MeshRenderer>().bounds.center - options[i];

                //https://forum.unity3d.com/threads/left-right-test-function.31420/  -- left right test
                Vector3 perp = Vector3.Cross(dir, toCentre);
                float d = Vector3.Dot(perp, Vector3.up);
                Quaternion spinDirection = Quaternion.Euler(0, 90, 0);
                if (d < 0.0)
                    spinDirection = Quaternion.Euler(0, -90, 0);

                GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                c.GetComponent<MeshRenderer>().enabled = false;
                //Debug.Log("objects for room count = " + objectsForRoom.Count);
                //Debug.Log("attempted objcets = " + attemptedObjects);
                c.name = objectsForRoom[attemptedObjects].name;
                c.transform.position = options[i];
                c.transform.rotation = Quaternion.LookRotation(dir) * spinDirection;
                c.transform.parent = room.transform;
                //if object is too close to a corner, move out from corner
                float distanceToNearestPoint = Vector3.Distance(options[i], pointsSortedByDistance[0]);
                if (distanceToNearestPoint < objectsForRoom[i].size.x)
                {

                    Vector3 sideWaysToCentre = c.transform.right;
                    if (Vector3.Distance(c.transform.position - c.transform.right, room.GetComponent<MeshRenderer>().bounds.center) < Vector3.Distance(c.transform.position + c.transform.right, room.GetComponent<MeshRenderer>().bounds.center))
                    {
                        sideWaysToCentre = -c.transform.right;
                        c.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;
                    }
                    else
                        c.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;

                    c.transform.position = pointsSortedByDistance[0];
                    c.transform.position += sideWaysToCentre* objectsForRoom[attemptedObjects].size.x * 0.5f;//half of width
                  
                    

                }
                c.transform.position += c.transform.forward * objectsForRoom[attemptedObjects].size.z * 0.5f;//length
                c.transform.localScale = objectsForRoom[attemptedObjects].size;
                // c.layer = 31;
                objectsToBuild.Add(c);
                
                attemptedObjects++;
            }
        }

        return objectsToBuild;
    }

    void ResizeObjects(List<GameObject> objectsToBuild, GameObject room, List<Vector3> options, List<ObjectAndSize> objectsForRoom)
    {
        //if we only have two items, make both small, no point in having a bath and a toilet/or sink
        if (objectsToBuild.Count == 2)
        {
            //shrink larger object
            // Debug.Log(objectsToBuild[0].transform.localScale);
            objectsToBuild[0].transform.localScale = Vector3.one;

            //find direction to centre of room
            Vector3 toCentre = room.GetComponent<MeshRenderer>().bounds.center - objectsToBuild[0].transform.position;

            //https://forum.unity3d.com/threads/left-right-test-function.31420/  -- left right test
            Vector3 perp = Vector3.Cross(objectsToBuild[0].transform.forward, toCentre);
            float d = Vector3.Dot(perp, Vector3.up);

            Vector3 dir = objectsToBuild[0].transform.right;
            if (d < 0.0)
                dir = -dir;

            //objectsToBuild[0].transform.position = options[0] + objectsToBuild[0].transform.forward * 0.5f;
            objectsToBuild[0].transform.position = options[0] + .5f * objectsToBuild[0].transform.forward + dir * 0.5f;
            //  Debug.Log("objects 2");

            objectsToBuild[0].name = objectsForRoom[objectsForRoom.Count - 1].name; //what index?
        }
    }

    public bool CheckForOverlap(List<GameObject> objectsToBuild,GameObject room)
    {
        
        //check if any overlapping objects
        bool overlap = false;
        for (int i = 0; i < objectsToBuild.Count; i++)
        {
            BoxCollider thisBox = objectsToBuild[i].GetComponent<BoxCollider>();
            for (int j = 0; j < objectsToBuild.Count; j++)
            {
                if (i == j)
                    continue;

                BoxCollider targetBox = objectsToBuild[j].GetComponent<BoxCollider>();

                if (thisBox.bounds.Intersects(targetBox.bounds))
                    overlap = true;
            }
        }

        //also check for door overlap --could maybe add somewhere else just once, instead of doing it here and then destroying multiple times
        //GameObject door = room.transform.FindChild("Door").gameObject;//checking against all doors in room
        List<GameObject> doors = new List<GameObject>();
        for (int i = 0; i < room.transform.childCount; i++)
        {
            if (room.transform.GetChild(i).name == "Door")
                doors.Add(room.transform.GetChild(i).gameObject);
        }
        //all doors in room
        foreach (GameObject door in doors)
        {
          //  GameObject door = doors[doors.Count - 1];

            BoxCollider bc = door.AddComponent<BoxCollider>();
            //this adds a collider and pushes it in to the room slightly
            bc.center = new Vector3(0f, 0f, -0.5f);
            for (int i = 0; i < objectsToBuild.Count; i++)
            {
                BoxCollider thisBox = objectsToBuild[i].GetComponent<BoxCollider>();

                if (thisBox.bounds.Intersects(bc.bounds))
                    overlap = true;
            }
            Destroy(bc);
        }

       // Debug.Log(overlap);

        return overlap;
    }    
    
    IEnumerator WaitForKeyDown(KeyCode keyCode)
    {
        while (!Input.GetKeyDown(keyCode))
            yield return null;
        yield return new WaitForEndOfFrame();
    }

    private int FindEdgeWithDoor(GameObject room,Vector3[] vertices)
    {
       
        GameObject door = room.transform.Find("Door").gameObject;
        List<Vector3> points = IntersectionPoints(vertices, 0.1f); //0.1f is bricksize
        //make loop
        points.Add(points[0]);
        //find edge with door
        float distance = Mathf.Infinity;
        int closest = 0;
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 midPoint = Vector3.Lerp(room.transform.position + points[i], room.transform.position + points[i + 1], 0.5f); //rotations add?
            float temp = Vector3.Distance(midPoint, door.transform.position);
            if (temp < distance)
            {
                distance = temp;
                closest = i;
            }
        }
        return closest;
    }

    List<Vector3> SplitDoorEdge(List<Vector3> points, int closest,GameObject door)
    {
        List<Vector3> edgesWithDoor = new List<Vector3>();
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (i != closest)
                edgesWithDoor.Add(points[i]);
            else
            {
                edgesWithDoor.Add(points[i]);
                Vector3 direction = (points[i + 1] - points[i]).normalized;
                //float doorWidth = 0.8f;//get from a global? magic number in static which builds it too
                //move door point half a door width back towards points[i]. This creates a new edge from points[i] to the door
                Vector3 doorIndent = door.transform.position + ((Quaternion.Euler(0, 90, 0) * direction) * 0.1f);
                Vector3 startOfDoor = doorIndent - (direction * (doorWidth * 0.5f));
                edgesWithDoor.Add(startOfDoor);
                //now add other side
                Vector3 endOfDoor = startOfDoor + (direction * doorWidth);
                edgesWithDoor.Add(endOfDoor);

            }
        }
        //make possible edge points

        //make loop
        edgesWithDoor.Add(edgesWithDoor[0]);

        return edgesWithDoor;
    }

    List<Vector3> EdgePoints(List<Vector3> edgesWithDoor,int closest,Vector3 doorPosition)
    {
        List<Vector3> possible = new List<Vector3>();
        for (int i = 0; i < edgesWithDoor.Count - 1; i++)
        {
            if (i == closest + 1)
                continue;

            float distance = Vector3.Distance(edgesWithDoor[i], edgesWithDoor[i + 1]);

            for (float j = 0; j < distance; j += 0.1f)//bricksize /or smallest object size?
            {
                Vector3 direction = (edgesWithDoor[i + 1] - edgesWithDoor[i]).normalized;
                Vector3 position = edgesWithDoor[i] + direction * j;

                //round to 1dp

                position.x *= 10f;
                position.x = Mathf.Round(position.x);
                position.x *= 0.1f;

                position.z *= 10f;
                position.z = Mathf.Round(position.z);
                position.z *= 0.1f;

                //leave space for door
                if(Vector3.Distance(doorPosition, position) < 1f)
                {/*
                    GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = position;
                    c.transform.localScale *= 0.1f;
                    */
                    continue;
                }

                //can get duplicates in corners sometimes
                if(!possible.Contains(position))
                    possible.Add(position);
            }
        }

        return possible;
    }

    void RotateAndPlace(float objectLength, float objectWidth,string type,List<GameObject> objectsToBuild)
    {

        //place object
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.GetComponent<MeshRenderer>().enabled = false;
        obj.transform.position = possible[indexAndDirection.index];     
        obj.transform.rotation = Quaternion.LookRotation(indexAndDirection.direction);
        obj.transform.position += obj.transform.right * (objectWidth * 0.5f);
        obj.transform.position += obj.transform.forward * objectLength * 0.5f;
        obj.transform.position += 0.5f*Vector3.up;
        Vector3 scale = new Vector3(objectWidth, 1f, objectLength);
        obj.transform.localScale = scale;
        obj.name = type;


        BoxCollider bc = obj.GetComponent<BoxCollider>();
        bc.size = new Vector3(1.5f, 2f, 1.5f);
        List<Vector3> toRemove = new List<Vector3>();
        for (int i = 0; i < possible.Count; i++)
        {
            bool pointInside = PointInOABB(possible[i], bc);

            if (pointInside)
            {
                if (!toRemove.Contains(possible[i]))
                    toRemove.Add(possible[i]);
            }

        }
        //stertch object alog length and remove any poitns it collides with, make it lsightly wider too to encomapss any wall points it uses
        //now placed, use box collider to look for any points across the room we need to remove in case there is not space to walk
       
        //Z
        bc.size = new Vector3(1, 2f, 5f);
        bc.center = new Vector3(-0.1f, 0f, 0f);
     
        for (int i = 0; i < possible.Count; i++)
        {
            bool pointInside = PointInOABB(possible[i], bc);

            if (pointInside)
            {
               
                if (!toRemove.Contains(possible[i]))
                    toRemove.Add(possible[i]);
            }

        }
        //X
        //now sterch collider to look for points across the other side of the room, squeeze length a little so we dont take from side of box
        bc.size = new Vector3(3f, 2f, 1f);
        bc.center = new Vector3(0f, 0f, 0f);
        for (int i = 0; i < possible.Count; i++)
        {
            bool pointInside = PointInOABB(possible[i], bc);

            if (pointInside)
            {

                if (!toRemove.Contains(possible[i]))
                    toRemove.Add(possible[i]);
            }

        }

        foreach (Vector3 v3 in toRemove)
        {
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = v3;
            c.transform.localScale *= 0.1f;
            c.transform.parent = obj.transform;

            possible.Remove(v3);
        }
        //reset box to correct size, keep fat Y for floor overlap, keep slight overlap for innacuracy
        bc.size = new Vector3(1.1f, 2f, 1.1f);
       
        objectsToBuild.Add(obj);


    }

    void RemoveSurroundingPoints(int r, float objectLength,float objectWidth,float maxWidth)
    {

        if (possible.Count < objectLength / 0.1f)
        {
            Debug.Log("not enough points left - remove surrounding points");
            indexAndDirection = null;
            return;
            //return null;
        }

        //remove these points from possible
        float f = objectLength / 0.1f;        
        int b = (int)(f);
        
        //possible.RemoveRange(r + 1, b - 1);// add to remove //"leave bookends"
        List<Vector3> toRemove = new List<Vector3>();
        /*
        for (int i = 0 ; i < b ; i++)
        {
            int p = i + r;
            //loop //check for max
            if (p > possible.Count - 1)
            {
                p -= possible.Count;
            }
            //  GameObject t1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //  t1.transform.localScale *= 0.1f;
             // t1.transform.position = possible[p];

            // possible.RemoveAt(p);
          //  toRemove.Add(possible[p]);
        }
        foreach (Vector3 v3 in toRemove)
        {
               // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                // c.transform.position = v3;
                // c.transform.localScale *= 0.1f;

           // possible.Remove(v3);

        }
        */
        //check next points (bathwidth amount for a corner
        //if we dont find a corner, move on
        //if we find a corner, remove points up to this corner, and also remove a bath width's( or min object size?) from the corner onwards
        toRemove = new List<Vector3>();
        List<int> tempList = new List<int>();
        //check in front
        float w = maxWidth / 0.1f;
        int q = (int)(w);
        int width = q; //(int)(objectWidth / 0.1f);
       
        //check rear//run backwards from where bath was removed
        //   Debug.Log("Count " + possible.Count);
        //
       
        for (int i = r; i > r - width; i--)
        {
            //check for corner
            //check direction to next point. If not the same as direction from previous point, we have found a corner
           
            int p = i;
            int prev = i - 1;
            int next = i + 1;

            //loop //check for max
            if (p > possible.Count - 1)
            {
                p -= possible.Count;
            }
            if (prev > possible.Count - 1)
            {
                prev -= possible.Count;
            }
            if (next > possible.Count - 1)
            {
                next -= possible.Count;                
            }
            //check for min
            if (p < 0)
                p += possible.Count;
            if (prev < 0)
                prev += possible.Count;
            if (next < 0)
                next += possible.Count;

            Vector3 thisPoint = possible[p];          
            Vector3 prevPoint = possible[prev];

            Vector3 directionPrevious = (thisPoint - prevPoint).normalized;  //can be out
            Vector3 directionNext = (possible[next] - possible[p]).normalized;

            if (directionNext == directionPrevious)
            {
                tempList.Add(p);
            }

            else if (directionNext != directionPrevious)
            {
                //Debug.Log("corner found"); --can fire twice, stop it?? - convered by .contains below
                tempList.Add(p);

                Debug.Log(possible.Count);
                //we are at a corner - if corner was within a bath width's of the end of the bath, remove a bath widths of points upcoming              
                if (Vector3.Distance(possible[p], possible[r]) < maxWidth) //changed to maxWidth -is object width needed now?
                {
                    if (possible.Count < width)
                    {
                        Debug.Log("not enough space?");
                        return;
                    }
                        
                    for (int k = 0; k < width; k++)
                    {
                        //check for loop
                        int check = p - 1 - k;
                        //loop
                        if (check > possible.Count - 1)
                        {
                            check -= possible.Count;
                        }
                        if ((check < 0))
                            check += possible.Count;

                        //if distance is over 0.1f, we have found a gap in the points - this is either a door or another object -skip
                        //check against last point we add to the list
                        if (Vector3.Distance(possible[check], possible[tempList[tempList.Count - 1]]) < 0.2f) //not sure why .1 isn't working..this seems to work fine at .2 //can be out of range if 
                        {                            
                            tempList.Add(check);
                        }
                        
                    }
                }

                foreach (int j in tempList)
                {
                    
                    //checking for duplicates --can't figure out how to stop it checking more after it finds a corner -- should be simple..
                    if (!toRemove.Contains(possible[j]))
                        toRemove.Add(possible[j]);
                }
                //clear
                tempList = new List<int>();
            }
        }
       
        tempList = new List<int>();

   

        for (int i = r + 1; i <= r + width; i++)
        {
            //check for corner
            //check direction to next point. If not the same as direction from previous point, we have found a corner
            

            int p = i;
            int prev = i - 1;
            int next = i + 1;
            //loop
            if (p > possible.Count - 1)            
                p -= possible.Count;
            
            if (prev > possible.Count - 1)            
                prev -= possible.Count;
            
            if (next > possible.Count - 1)            
                next -= possible.Count;                

            //check for min
            if (p < 0)
                p += possible.Count;
            if (prev < 0)
                prev += possible.Count;
            if (next < 0)
                next += possible.Count;

          //  GameObject t1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
          //  t1.transform.localScale *= 0.1f;
          //  t1.transform.position = possible[p];


            Vector3 directionPrevious = (possible[p] - possible[prev]).normalized;  //can be out
            Vector3 directionNext = (possible[next] - possible[p]).normalized;

            if (directionNext == directionPrevious)
            {
                tempList.Add(p);
            }

            else if (directionNext != directionPrevious)
            {

                tempList.Add(p);
                //make sure r is on loop
                int loopedR = r + 1;
                if (loopedR == possible.Count)
                    loopedR = 0;
                //we are at a corner - if corner was within a bath width's of the end of the bath, remove a bath widths of points upcoming
                if (Vector3.Distance(possible[p], possible[loopedR]) < maxWidth) //changed from object width
                {
                    if (possible.Count < width)
                    {
                        Debug.Log("not enough space?");
                        return;
                    }

                    for (int k = 0; k < width; k++)
                    {
                        //check for loop
                        int check = p + 1 + k;
                        //loop
                        if (check > possible.Count - 1)
                        {
                            check -= possible.Count;
                        }
                        if (check < 0)
                            check += possible.Count;


                        //if distance is over 0.1f, we have found a gap in the points - this is either a door or another object -skip
                        //check against last point we add to the list
                        if (Vector3.Distance(possible[check], possible[tempList[tempList.Count - 1]]) < 0.2f) //not sure why .1 isn't working..this seems to work fine at .2
                        {
                            tempList.Add(check);
                        }
                       
                    }
                }

                foreach (int j in tempList)
                {

                    if(!toRemove.Contains(possible[j]))
                     toRemove.Add(possible[j]);


                }
                //clear
                tempList = new List<int>();
            }

            


        }
        foreach (Vector3 v3 in toRemove)
        {
        //    GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
       //     c.transform.position = v3;
       //     c.transform.localScale *= 0.1f;
            
            possible.Remove(v3);

         
        }


        foreach (Vector3 v3 in possible)
        {
            //GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //c.transform.position = v3;
            //c.transform.localScale *= 0.1f;
        }

        //return possible;//moved possibel up top to ensure thread safe

        //now we must check if the poitns on the other side of the room are too close, if there is not enough space in the middle of the room, i.e a door width to the minimum width of the object (bath if bathroom), remove these points

        //check all possible points
        for (int i = 0; i < possible.Count; i++)
        {
            //how do we know if it is on the other side of the room?

            //create box and use length and width = 2x minimim object width, do OABB check
        }


    } //could be tidied/factored-not using

    public class IndexAndDirection
    {
        public int index;
        public Vector3 direction;
    }

    public class ObjectTypeAndBuildDirection
    {
        public string type;
        public Vector3 direction;
        public Vector3 position;
    }

    public class PositionAndDirection
    {
        public Vector3 direction;
        public Vector3 position;
    }
    
    static float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0f)
        {
            return 1f;
        }
        else if (dir < 0f)
        {
            return -1f;
        }
        else
        {
            return 0f;
        }
    }

    Mesh CombineMeshes(MeshFilter[] meshFilters)
    {
        Mesh combinedMesh = new Mesh();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }
        combinedMesh.CombineMeshes(combine);

        return combinedMesh;
    }

    GameObject CreateQuad(Vector3 position,float x, float z)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        int[] indices = new int[6]; //2 triangles, 3 indices each

        //minus a half width so the pivot point is a the centre of the quad
        float m_Width = x;
        float m_Length = z;
        //round
        x = (float)System.Math.Round((double)x, 1);
        z = (float)System.Math.Round((double)z, 1);
        // vertices[0] = new Vector3(-m_Width * 0.5f, 0.0f, -m_Length * 0.5f);
        // vertices[1] = new Vector3(-m_Width * 0.5f, 0.0f, m_Length * 0.5f);
        // vertices[2] = new Vector3(m_Width * 0.5f, 0.0f, m_Length * 0.5f);
        //  vertices[3] = new Vector3(m_Width * 0.5f, 0.0f, -m_Length * 0.5f);


        vertices[0] = new Vector3(0f, 0.0f, 0f);
        vertices[1] = new Vector3(0f, 0.0f, m_Length);
        vertices[2] = new Vector3(m_Width, 0.0f, m_Length);
        vertices[3] = new Vector3(m_Width, 0.0f, 0f);

        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;

        indices[3] = 0;
        indices[4] = 2;
        indices[5] = 3;

        //rotate to face road

        //    Quaternion toRoad = transform.GetComponent<StretchQuads>().toRoad;
        for (int i = 0; i < vertices.Length; i++)
        {
            //   vertices[i] = toRoad * vertices[i];
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            //vertices[i].x *= x;
            vertices[i].x -= x * .5f;//centrering (why 5? -half of ten..rounded?)
           // vertices[i].z *= z;
            vertices[i].z -= z * .5f;

            vertices[i].x = (float)System.Math.Round((double)vertices[i].x, 1);
            vertices[i].z = (float)System.Math.Round((double)vertices[i].z, 1);
        }

        mesh.vertices = vertices;
        mesh.triangles = indices;


        GameObject quad = new GameObject();
        quad.layer = 26;

        //quad.transform.rotation = toRoad;


        MeshFilter meshFilter = quad.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();


        quad.AddComponent<MeshRenderer>();
        //MeshRenderer meshRenderer = 

        //    meshRenderer.enabled = false;
        //        meshRenderer.material = Resources.Load("Grey", typeof(Material)) as Material;
        // meshRenderer.enabled = false;
        //MeshCollider meshCollider = quad.AddComponent<MeshCollider>();
        //meshCollider.sharedMesh = mesh;
        quad.transform.parent = transform;
        quad.transform.position = position;

        return quad;

    }

    public static GameObject Quad(GameObject gameObject, Vector3[] points)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.parent = gameObject.transform;

        //find two furthest points from each other, this will be the hypotenuse in both triangles

        int[] hypotenuse = new int[2];
        float longest = 0f;
        for (int i = 0; i < points.Length; i++)
        {
            for (int j = 0; j < points.Length; j++)
            {
                float temp = Vector3.Distance(points[i], points[j]);
                if (temp > longest)
                {
                    longest = temp;
                    hypotenuse = new int[2] { i, j };
                }
            }
        }

        //other two corners
        List<Vector3> others = new List<Vector3>();
        for (int i = 0; i < points.Length; i++)
        {
            if (i != hypotenuse[0] && i != hypotenuse[1])
                others.Add(points[i]);
        }
        /*
        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c.transform.position = points[hypotenuse[0]];
        c.transform.localScale *= 0.1f;

        GameObject c2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c2.transform.position = points[hypotenuse[1]];
        c2.transform.localScale *= 0.1f;

        GameObject c3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        c3.transform.position = others[0];
        c3.transform.localScale *= 0.1f;

        GameObject c4 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        c4.transform.position = others[1];
        c4.transform.localScale *= 0.1f;
        */

        Mesh mesh = new Mesh();

        //find what point from the others is on the right hand side
        Vector3 hDir = points[hypotenuse[1]] - points[hypotenuse[0]]; ;
        Vector3 dirToTarget = others[0] - points[hypotenuse[0]];
        float f = Divide.AngleDir(hDir.normalized, dirToTarget.normalized, Vector3.up);
        //  Debug.Log(f);

        //vertices must be placed clockwise
        Vector3[] vertices = new Vector3[4];
        //and adjust tri accordingly
        int[] triangles = new int[6];
        if (f == 1)
        {
            vertices[0] = points[hypotenuse[0]];
            vertices[1] = others[1];
            vertices[2] = points[hypotenuse[1]];
            vertices[3] = others[0];

            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;

            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;
        }
        else
        {
            vertices[0] = points[hypotenuse[0]];
            vertices[1] = others[0];
            vertices[2] = points[hypotenuse[1]];
            vertices[3] = others[1];

            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;

            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;
        }





        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();

        quad.GetComponent<MeshFilter>().mesh = mesh;
        quad.GetComponent<MeshCollider>().sharedMesh = mesh;
        return quad;
    }

    void StretchAndRotate(GameObject plot)
    {
        Mesh mesh = plot.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;


        //observations 
        //3 rooms
        // 1 floor (.5,2)
        //2 floor (1.5,2) //if 2 we need to split up more rooms upstairs

        //4 rooms
            //1 floor (.75,2)
            //2 floor(.75,2) // if 2nd floor we need to spit up main r[oom
        
        //5 rooms 
            //1 floor (1f,2f)
            //2 floor (1f,2f) //big room needs splitting?

        
        //2 could be 1.5?

        float x = Random.Range(.6f,2f);
        float z = Random.Range(.6f,2f);

       // x = 3f;
        //z = 3f;
        //plotXSize = x;
        //plotZSize = z;
        if (x < .75f || z < .75f)
        {
            //3.1
            roomAmount = 3;
            floors = 1;
            
        }
        else if (x > .75 && x < 1f || z > .75 && z < 1f)
        {
            //if any between .75 and 1            
            //could be either
            int random = Random.Range(0, 3);
            if (random == 0)
            {
                //3.1

                roomAmount = 3;
                floors = 1;
            }
            if (random == 1)
            {
                //4.1
                roomAmount = 4;
                floors = 1;
            }
            if (random == 2)
            {
                //4.2

                roomAmount = 4;
                floors = 2;
            }
        }
        else if (x > 1f && x < 2f || z > 1f && z < 2f)
        {
            //if any between 1 and 2
            //could be either
            int random = Random.Range(0, 4);
            if (random == 0)
            {
                //3.2

                roomAmount = 3;
                floors = 2;
            }
            if (random == 1)
            {
                //5.1
                roomAmount = 5;
                floors = 1;
            }
            if (random == 2)
            {
                //5.2

                roomAmount = 5;
                floors = 2;
            }
        }

        x = (float)System.Math.Round((double)x, 1);
        z = (float)System.Math.Round((double)z, 1);

        //x = 1.8f;
        //z = 0.5f;
       // Debug.Log(x + " after") ;
        
      //  Debug.Log(z + " after");
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= x;
            vertices[i].x -= x * 5;//centrering (why 5? -half of ten..rounded?)
            vertices[i].z *= z;
            vertices[i].z -= z *5;
        }

        mesh.vertices = vertices;
    }

    public static List<GameObject> Split(GameObject gameObject, GameObject quad,float variance,bool forceSpecificLength,bool longwaysSplit)//variance must be in 0.1 units
    {
        //return this
        List<GameObject> splits = new List<GameObject>();

        Transform transform = gameObject.transform;

        quad.GetComponent<MeshRenderer>().enabled = false;
        Mesh mesh = quad.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        
        //DestroyImmediate(quad);
        //quad.SetActive(false); //settinf inactive atm, may cause problems -  I want to keep plot to check for bounds
        //find longest edge
        int[] longestEdge = LongestEdge(vertices);
        int[] shortestEdge = ShortestEdge(vertices);

        if(longwaysSplit)
        {
            int[] temp =  longestEdge;
            longestEdge = shortestEdge;
            shortestEdge = temp;
        }

        //check for square plot, if plot is square, force edges to be perpendicular to each other - if we dont do this, shortest and longest can be the same edge
        bool symmetrical = false;
        float distanceLongestEdge = Vector3.Distance(vertices[longestEdge[0]], vertices[longestEdge[1]]);
        if (Mathf.Abs(Vector3.Distance(vertices[shortestEdge[0]], vertices[shortestEdge[1]]) - distanceLongestEdge) < 0.1f)
        {
            shortestEdge = new int[2] { 0, 1 };
            longestEdge = new int[2] { 1, 2 };
            Debug.Log("Symmetrical " + quad.name);
            // Debug.Break();
            /*
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = vertices[shortestEdge[0]];
            c.transform.parent = gameObject.transform;
            c.name = "Sedge1";

            c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = vertices[shortestEdge[1]];
            c.transform.parent = gameObject.transform;
            c.name = "Sedge2";

            c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = vertices[longestEdge[0]];
            c.transform.parent = gameObject.transform;
            c.name = "Ledge1";

            c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = vertices[longestEdge[1]];
            c.transform.parent = gameObject.transform;
            c.name = "Ledge2";
            */
            symmetrical = true;
            //Debug.Break();
        }
        //first main split in half
        //float min = 0.4f; //minimum house widh is 0.5 - 0.4 is 0.1 for a door? Is this working?;

        //just randomising direction from centre
        int dir = 1;
        if (!forceSpecificLength)
        {   
            if (Random.Range(0, 2) == 0)
                dir = -1;
        }



        float random = 0.5f + (variance * dir); //0.5f in?
        
        //create a minimum zone and a maximum zone at each side of the room we are splitting, so that there is always space for a door on any wall
        
        
        Vector3 minPoint = vertices[longestEdge[1]];// - dirTo0*0.4f;
        Vector3 maxPoint = vertices[longestEdge[0]];// - dirTo1*0.4f;
        

        Vector3 p = Vector3.Lerp(minPoint, maxPoint, random);

        if(forceSpecificLength)
        {
            Vector3 dirTo0 = (vertices[longestEdge[1]] - vertices[longestEdge[0]]).normalized;
            p = maxPoint + dirTo0 * variance;
        }

        //round to 1dp
        p.x= (float)System.Math.Round((double)p.x, 1);
        p.z = (float)System.Math.Round((double)p.z, 1);
        /*
        p.x *= 10;
        p.x = Mathf.Round(p.x);
        p.x *= 0.1f;

        p.z *= 10;
        p.z = Mathf.Round(p.z);
        p.z *= 0.1f;
        */
        Vector3 d = Quaternion.Euler(0, 90, 0) * (vertices[longestEdge[1]] - vertices[longestEdge[0]]);
        d = d.normalized * Vector3.Distance(vertices[shortestEdge[0]], vertices[shortestEdge[1]]);

        //corresponding point at other side
        Vector3 p2 = p + d;
        p2.x = (float)System.Math.Round((double)p2.x, 1);
        p2.z = (float)System.Math.Round((double)p2.z, 1);
        /*
        p2.x *= 100;
        p2.x = Mathf.Round(p2.x);
        p2.x *= 0.1f;

        p2.z *= 10;
        p2.z = Mathf.Round(p2.z);
        p2.z *= 0.1f;
        */

        //double checking we are using the correct point for that last one

        float distance = Mathf.Infinity;
        int closest = 0;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (i == shortestEdge[0])
                continue;

            float temp = Vector3.Distance(vertices[shortestEdge[0]], vertices[i]);
            if(temp < distance -.1f)
            {
                distance = temp;
                closest = i;
            }
        }

        //give the quad creation function the two shortest edges with the splitting points
        //Vector3[] points = new Vector3[4] { vertices[shortestEdge[0]], p, p2, vertices[closest] };
        
        Vector3[] points = new Vector3[4] { vertices[shortestEdge[0]], p, p2, vertices[shortestEdge[1]]};
        for (int i = 0; i < points.Length; i++)
        {
           // GameObject a = GameObject.CreatePrimitive(PrimitiveType.Cube);
           // a.transform.position = points[i];
           // a.transform.name = i.ToString() + " first";
           // a.transform.parent = gameObject.transform;
        }

        GameObject first = null;
        
        
            first = Quad(gameObject, points);
            first.transform.position += transform.position;
            first.transform.rotation = transform.rotation;
        
        
        

         //not using symmetircal bool anymore - fixsed up above in symmetry protection
         
        int adder = 2;
        if (longwaysSplit)
        {   if(symmetrical)
                Debug.Log("longways split");
            adder = -1;
        }
       
        int edge1 = shortestEdge[0] + adder;
        if (edge1 > vertices.Length - 1)
        {
            if (symmetrical)
                Debug.Log("HERE 3");
            edge1 -= vertices.Length;
        }
        
        if (edge1 < 0)
        {            
            if(!symmetrical)
                edge1 += vertices.Length;
            else
            {
                edge1 += vertices.Length-1; /////????????????? bug fix
            }

        }
        
            

        int edge2 = closest + adder;
        if (edge2 > vertices.Length - 1)
        {
            if (symmetrical)
                Debug.Log("HERE 1");
            edge2 -= vertices.Length;
        }
        if (edge2 < 0)
        {
            if (symmetrical)
            {
                Debug.Break();

                Debug.Log("BROKE FROM HERE 2 - CHECK OK IF WE GOT HERE -CANT REPLICATE");
            }
            if (!symmetrical)
                edge2 += vertices.Length;
            else
            {
                edge2 += vertices.Length - 1; /////????????????? bug fix
            }
        }

        Vector3 p3 = vertices[edge2];
        if (symmetrical)
        {
            p3 = vertices[edge1] + (p2 - p);
        }
        points = new Vector3[4] { vertices[edge1], p, p2, p3  };


        for (int i = 0; i < points.Length; i++)
        {
           // GameObject a = GameObject.CreatePrimitive(PrimitiveType.Cube);
           /// a.transform.position = points[i];
          //  a.transform.name = i.ToString();
          //  a.transform.parent = gameObject.transform;
        }

        GameObject second = Quad(gameObject, points);
        second.transform.position += transform.position;
        second.transform.rotation = transform.rotation;

        //return two gamobjects

        
        splits.Add(first);
        splits.Add(second);

        return splits;
    }
    public static List<GameObject> SplitFromPoint(GameObject gameObject, GameObject quad, float variance, Vector3 closestPoint)
    {
        //return this
        List<GameObject> splits = new List<GameObject>();

        Transform transform = gameObject.transform;

        quad.GetComponent<MeshRenderer>().enabled = false;
        Mesh mesh = quad.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        //DestroyImmediate(quad);
        quad.SetActive(false); //settinf inactive atm, may cause problems -  I want to keep plot to check for bounds
                               //find longest edge



        //find the two closet point to "closest point" -the hall for example

        List<Vector3> sortedByDistance = new List<Vector3>(quad.GetComponent<MeshFilter>().mesh.vertices);

        sortedByDistance.Sort(delegate (Vector3 v1, Vector3 v2)
        {

            return Vector3.Distance(closestPoint, v1).CompareTo
                        ((Vector3.Distance(closestPoint, v2)));
        });

        //find diection facing away from closet to points towards furthest points
        Vector3 mid1 = Vector3.Lerp(sortedByDistance[0], sortedByDistance[1], 0.5f);
        Vector3 mid2 = Vector3.Lerp(sortedByDistance[2], sortedByDistance[3], 0.5f);

        Vector3 directionAway = (mid2 - mid1).normalized;

        Vector3 p2 = sortedByDistance[0] + (directionAway * variance);
        Vector3 p3 = sortedByDistance[1] + (directionAway * variance);


        Vector3[] points = new Vector3[] { sortedByDistance[0], sortedByDistance[1], p2, p3 };

        GameObject first = Quad(gameObject, points);
        first.transform.position += transform.position;
        first.transform.rotation = transform.rotation;






        points = new Vector3[] { p2, p3, sortedByDistance[2], sortedByDistance[3] };


        for (int i = 0; i < points.Length; i++)
        {
            // GameObject a = GameObject.CreatePrimitive(PrimitiveType.Cube);
            /// a.transform.position = points[i];
            //  a.transform.name = i.ToString();
            //  a.transform.parent = gameObject.transform;
        }

        GameObject second = Quad(gameObject, points);
        second.transform.position += transform.position;
        second.transform.rotation = transform.rotation;

        //return two gamobjects


        splits.Add(first);
        splits.Add(second);

        return splits;
    }
    public static List<GameObject> SplitWithDirection(GameObject gameObject, GameObject quad, float variance, Vector3 direction,Vector3 closestPoint)
    {
        //return this
        List<GameObject> splits = new List<GameObject>();

        Transform transform = gameObject.transform;

        quad.GetComponent<MeshRenderer>().enabled = false;
        Mesh mesh = quad.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        //DestroyImmediate(quad);
        quad.SetActive(false); //settinf inactive atm, may cause problems -  I want to keep plot to check for bounds
        //find longest edge
        int[] longestEdge = LongestEdge(vertices);
        int[] shortestEdge = ShortestEdge(vertices);

        //SYMMETRY PROTECTION!
        if(shortestEdge[0] == longestEdge[0])
        {
            //Debug.Break();
            //symmetrical - returned the same edge for shortest and longest
            //find the two vetices not in the edge returned
            List<int> unused = new List<int>();
            for (int i = 0; i < shortestEdge.Length; i++)
            {
                for (int j = 0; j < 4; j++)//four quad points - will i ever do rooms that aren't quads?
                {
                    if (shortestEdge[i] != j)
                        unused.Add(j);
                }
            }

            longestEdge = new int[] { unused[0], unused[1] };
        }



        //find edge with required direction
        int[] forwardEdge = shortestEdge;
        int[] sidewaysEdge = longestEdge;
        Vector3 tempEdgeDir = (vertices[shortestEdge[0]] - vertices[shortestEdge[1]]).normalized;
        if (tempEdgeDir == direction || tempEdgeDir == -direction)
        {
            forwardEdge = longestEdge;
            sidewaysEdge = shortestEdge;
        }

        //find both edges , forward and sidways
        List<int[]> forwardEdges = new List<int[]>();
        List<int[]> sidewaysEdges = new List<int[]>();
        List<Vector3> loop = new List<Vector3>(vertices);
        loop.Add(loop[0]);

        Vector3 forwardEdgeDir = (vertices[forwardEdge[0]] - vertices[forwardEdge[1]]).normalized;
        Vector3 sidewaysEdgeDir = (vertices[sidewaysEdge[0]] - vertices[sidewaysEdge[1]]).normalized;
        //organising edges in to short and long lists
        for (int i = 0; i < loop.Count -1; i++)
        {
            Vector3 edgeDir = (loop[i] - loop[i + 1]).normalized;
             

            if(edgeDir == forwardEdgeDir || edgeDir == -forwardEdgeDir)
                forwardEdges.Add(new int[2]{ i,i+1});

            if (edgeDir == sidewaysEdgeDir || edgeDir == -sidewaysEdgeDir)
                sidewaysEdges.Add(new int[2] { i, i + 1 });
        }

       // Debug.Log("sideways count = " + sidewaysEdges.Count);
      //  Debug.Log("forward count = " + forwardEdges.Count);
        //Debug.Break();

        //find which edge is closest to point we passed (hall,stair etc)
        List<int[]> edgeListToUse = sidewaysEdges;
        //Debug.Log("edge list to use count = " + edgeListToUse.Count);
        edgeListToUse.Sort(delegate (int[] a1,int[] a2)
        {
            return Vector3.Distance(Vector3.Lerp(loop[a1[0]], loop[a1[1]], 0.5f), closestPoint).CompareTo
            (Vector3.Distance(Vector3.Lerp(loop[a2[0]], loop[a2[1]], 0.5f), closestPoint));

        });

        //we have found the closest edge to the point now - first in edge list
        Vector3 p0 = loop[edgeListToUse[0][0]];
        Vector3 p1 = loop[edgeListToUse[0][1]];


       

        /*
        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c.transform.position = p0;// + gameObject.transform.position;
        c.transform.parent = gameObject.transform;
        c.name = "p0";
        c = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c.transform.position = p1;// + gameObject.transform.position;
        c.transform.parent = gameObject.transform;
        c.name = "p1";
        */
        Vector3 p2 = loop[edgeListToUse[1][0]];
        Vector3 p3 = loop[edgeListToUse[1][1]];

        //now get the direction towards the other edge
        Vector3 mid1 = Vector3.Lerp(p0, p1, 0.5f);
        Vector3 mid2 = Vector3.Lerp(p2, p3, 0.5f);

        Vector3 directionAway = (mid2 - mid1).normalized;

        Vector3 intersect1 = p0 + directionAway * variance;
        Vector3 intersect2 = p1 + directionAway * variance;

        GameObject first = null;

        Vector3[] points = new Vector3[4] { p0, p1, intersect1, intersect2 };
        first = Quad(gameObject, points);
        first.transform.position += transform.position;
        first.transform.rotation = transform.rotation;


        points = new Vector3[4] {intersect1, intersect2,p2,p3 };


        GameObject second = Quad(gameObject, points);
        second.transform.position += transform.position;
        second.transform.rotation = transform.rotation;

        //return two gamobjects


        splits.Add(first);
        splits.Add(second);

        return splits;
    }

    public static int[] LongestEdge(Vector3[] vertices)
    {
        float longest = 0f;
        int[] longestEdge = new int[2];
        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = 0;
            if (i < vertices.Length - 1)
                distance = Vector3.Distance(vertices[i], vertices[i + 1]);
            else if (i == vertices.Length - 1)
                distance = Vector3.Distance(vertices[i], vertices[0]);

            if (distance > longest)
            {
                longest = distance;
                if (i < vertices.Length - 1)
                {
                    int[] edge = new int[] { i, i + 1 };
                    longestEdge = edge;
                }
                else if (i == vertices.Length - 1)
                {
                    int[] edge = new int[] { i, 0 };
                    longestEdge = edge;
                }
            }
        }
        return longestEdge;
    }
    public static int[] ShortestEdge(Vector3[] vertices)
    {
        //shortest-create function ? only did it for longest because i needed it elsewhere
        float shortest = Mathf.Infinity;
        int[] shortestEdge = new int[2];
        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = 0;
            if (i < vertices.Length - 1)
                distance = Vector3.Distance(vertices[i], vertices[i + 1]);
            else if (i == vertices.Length - 1)
                distance = Vector3.Distance(vertices[i], vertices[0]);

            if (distance < shortest)
            {
                shortest = distance;
                if (i < vertices.Length - 1)
                {
                    int[] edge = new int[] { i, i + 1 };
                    shortestEdge = edge;
                }
                else if (i == vertices.Length - 1)
                {
                    int[] edge = new int[] { i, 0 };
                    shortestEdge = edge;
                }
            }
        }

        return shortestEdge;
    }

    public class RoomsAndSizes
    {
        public GameObject room;
        public float size;
    }

    public class ObjectAndSize
    {
        public string name;
        public Vector3 size;
    }

    public class WallWithDoor
    {
        public Vector3 wallPoint1;
        public Vector3 wallPoint2;
        public Vector3 doorPoint;
        public GameObject parent;
        public GameObject target;
    }

    public class RoomAndEdge
    {
        public GameObject room;
        public Vector3[] edge;
    }

    public class TargetAndSharedPoints
    {
        public GameObject room;
        public GameObject target;
        public List<Vector3> sharedPoints;
    }
    

    public static Mesh AutoWeldFunction(Mesh mesh, float threshold, float bucketStep)
    {

        Vector3[] oldVertices = mesh.vertices;
        Vector3[] newVertices = new Vector3[oldVertices.Length];
        int[] old2new = new int[oldVertices.Length];
        int newSize = 0;

        // Find AABB
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < oldVertices.Length; i++)
        {
            if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
            if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
            if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
            if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
            if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
            if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
        }

        // Make cubic buckets, each with dimensions "bucketStep"
        int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
        int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
        int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;
        List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

        // Make new vertices
        for (int i = 0; i < oldVertices.Length; i++)
        {
            // Determine which bucket it belongs to
            int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
            int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
            int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);

            // Check to see if it's already been added
            if (buckets[x, y, z] == null)
                buckets[x, y, z] = new List<int>(); // Make buckets lazily

            for (int j = 0; j < buckets[x, y, z].Count; j++)
            {
                Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                if (Vector3.SqrMagnitude(to) < threshold)
                {
                    old2new[i] = buckets[x, y, z][j];
                    goto skip; // Skip to next old vertex if this one is already there
                }
            }

            // Add new vertex
            newVertices[newSize] = oldVertices[i];
            buckets[x, y, z].Add(newSize);
            old2new[i] = newSize;
            newSize++;

        skip:;
        }

        // Make new triangles
        int[] oldTris = mesh.triangles;
        int[] newTris = new int[oldTris.Length];
        for (int i = 0; i < oldTris.Length; i++)
        {
            newTris[i] = old2new[oldTris[i]];
        }

        Vector3[] finalVertices = new Vector3[newSize];
        for (int i = 0; i < newSize; i++)
            finalVertices[i] = newVertices[i];

        Mesh newMesh = new Mesh();
        //newMesh.Clear();
        newMesh.vertices = finalVertices;
        newMesh.triangles = newTris;
        newMesh.RecalculateNormals();
        //newmesh.Optimize ();
        newMesh.name = "AutoWeldedMesh with Threshold " + threshold;


        return newMesh;

    } 

    public static bool PointInOABB(Vector3 point, BoxCollider box)
    {
        point = box.transform.InverseTransformPoint(point) - box.center;

       // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
      //  c.transform.position = point;
       // c.transform.localScale *= 0.1f;

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

    //Calculate the intersection point of two lines. Returns true if lines intersect, otherwise false.
    //Note that in 3d, two lines do not intersect most of the time. So if the two lines are not in the 
    //same plane, use ClosestPointsOnTwoLines() instead.
    public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parallel
        if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }

    public static bool PointsInLine(Vector3 A, Vector3 B, Vector3 C)
    {
        bool inLine = false;

        //lloks to see if points tested make a triangle or not
        //https://stackoverflow.com/questions/17692922/check-is-a-point-x-y-is-between-two-points-drawn-on-a-straight-line
        
        float discrepancy = 0.001f;//was0.01
        float smallDistance = Vector3.Distance(A, C) + Vector3.Distance(B, C);
        float largeDistance = Vector3.Distance(A, B);
        //within a range, small discrepancies can happen, is ok with what values w are working with
        if (smallDistance >= largeDistance - discrepancy && smallDistance <= largeDistance + discrepancy)
            inLine = true;

        return inLine;
    }

   

}
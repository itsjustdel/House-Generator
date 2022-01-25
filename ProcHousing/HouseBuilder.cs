using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseBuilder : MonoBehaviour {

    public static List<List<GameObject>> DoorWithWall(GameObject room, Vector3 p1, Vector3 p2, Vector3 doorPoint, bool skipDoor, bool exterior, float storeyHeight, float doorHeight, float doorWidth, Divide divide)
    {
        List<List<GameObject>> doorThenWalls = new List<List<GameObject>>();
        //apply rotations from room //could alternatively do this before these points are passed, doing here for cloarity on first script
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

        if (!skipDoor)
        {
            GameObject door = DoorAtPosition(doorPoint, lookDir, room, storeyHeight, doorHeight, doorWidth, divide);
            List<GameObject> doorList = new List<GameObject>();
            doorList.Add(door);
            //add to list to send back
            doorThenWalls.Add(doorList);
        }

        if (exterior)
            lookDir = -lookDir;
        //build wall around door
        float distanceOfWall = Vector3.Distance(p1, p2);

        List<GameObject> walls = WallAroundDoorWithOffset(doorPoint, p1, p2, distanceOfWall, lookDir, false, room, storeyHeight, doorHeight, doorWidth);
        doorThenWalls.Add(walls);

        //add skirting
        if (!exterior)
        {
            List<GameObject> skirts = SkirtingWithDoor(room, walls[0], doorPoint, p1, p2, lookDir, doorWidth, divide);
            divide.interiorAssetsByRoom.Add(skirts);
        } 

        return doorThenWalls;

    }

    public static GameObject DoorFrame(GameObject room,GameObject door,Divide divide)
    {

        float adjustedHeight = divide.skirtingHeight * 0.5f;

        GameObject frameParent = new GameObject();
        frameParent.name = "DoorFrameParent";
        frameParent.transform.parent = room.transform;
        List<GameObject> frames= new List<GameObject>();
        
        //TOP
        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
        top.name = "Top";
        top.transform.rotation = door.transform.rotation;
        top.transform.position = door.transform.position;
        top.transform.position += Vector3.up*(divide.doorHeight + divide.skirtingHeight*0.25f);
        top.transform.localScale = new Vector3(divide.doorWidth + divide.skirtingHeight*0.5f, adjustedHeight, 0.2f + divide.skirtingDepth);
        frames.Add(top);

        //side1
        GameObject rightSide= GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightSide.name = "RightSide";
        rightSide.transform.rotation = door.transform.rotation;
        rightSide.transform.position = door.transform.position;
        rightSide.transform.position += Vector3.up * (divide.doorHeight*.5f + adjustedHeight * .25f);
        rightSide.transform.position += door.transform.right * (divide.doorWidth * 0.5f);
        rightSide.transform.localScale = new Vector3(adjustedHeight, divide.doorHeight + adjustedHeight * 0.5f, 0.2f + divide.skirtingDepth);
        frames.Add(rightSide);

        GameObject leftSide = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftSide.name = "LeftSide";
        leftSide.transform.rotation = door.transform.rotation;
        leftSide.transform.position = door.transform.position;
        leftSide.transform.position += Vector3.up * (divide.doorHeight * .5f + adjustedHeight * .25f);
        leftSide.transform.position -= door.transform.right * (divide.doorWidth * 0.5f);
        leftSide.transform.localScale = new Vector3(adjustedHeight, divide.doorHeight + adjustedHeight * 0.5f, 0.2f + divide.skirtingDepth);
        frames.Add(leftSide);

        foreach (GameObject go in frames)
        {
            go.transform.parent = frameParent.transform;
            go.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;
        }

        return frameParent;
    }

    public static GameObject WindowFrame(GameObject room, GameObject door, Divide divide)
    {

        float adjustedHeight = divide.skirtingHeight * 0.5f;

        float windowWidth = divide.windowWidth;

        if (room.name == "Bathroom" || room.name == "Hall" || room.name == "Hall1" || room.name == "Hall2" || room.name == "Landing" || room.name == "LandingClose")
            windowWidth = 1f;

        if (divide.smallKitchenWindow)
            windowWidth = 1f;

        //adjust for storey, its because we moved the wholde first floor up by 0.2f
        

        GameObject frameParent = new GameObject();
        frameParent.name = "WindowFrameParent";
        frameParent.transform.parent = door.transform;
        List<GameObject> frames = new List<GameObject>();

        //TOP
        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
        top.name = "Top";
        top.transform.rotation = door.transform.rotation;
        top.transform.position = door.transform.position;
        top.transform.position += Vector3.up * (divide.doorHeight);
        top.transform.localScale = new Vector3(windowWidth + divide.skirtingHeight * .5f, adjustedHeight, 0.2f + divide.skirtingDepth);
        frames.Add(top);

        //bottom
        GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bottom.name = "Bottom";
        bottom.transform.rotation = door.transform.rotation;
        bottom.transform.position = door.transform.position;
        bottom.transform.position += Vector3.up * (divide.doorHeight - divide.windowHeight);
        float ledgeScaler = Random.Range(1.2f, 1.5f);
        bottom.transform.localScale = new Vector3(windowWidth+ divide.skirtingHeight*.5f, adjustedHeight, 0.2f + divide.skirtingDepth);
        frames.Add(bottom);


        //side1
        GameObject rightSide = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightSide.name = "RightSide";
        rightSide.transform.rotation = door.transform.rotation;
        rightSide.transform.position = door.transform.GetComponent<MeshRenderer>().bounds.center;
        //rightSide.transform.position += Vector3.up * (divide.windowHeight*0.5f);
        rightSide.transform.position += door.transform.right * (windowWidth * 0.5f);
        rightSide.transform.localScale = new Vector3(adjustedHeight, divide.windowHeight + adjustedHeight * 0.5f, 0.2f + divide.skirtingDepth);
        frames.Add(rightSide);

        GameObject leftSide = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftSide.name = "LeftSide";
        leftSide.transform.rotation = door.transform.rotation;
        leftSide.transform.position = door.transform.GetComponent<MeshRenderer>().bounds.center;
        //leftSide.transform.position += Vector3.up * (divide.storeyHeight * .5f + adjustedHeight * .25f);
        leftSide.transform.position -= door.transform.right * (windowWidth* 0.5f);
        leftSide.transform.localScale = new Vector3(adjustedHeight, divide.windowHeight + adjustedHeight * 0.5f, 0.2f + divide.skirtingDepth);
        frames.Add(leftSide);

        foreach (GameObject go in frames)
        {
            go.transform.parent = frameParent.transform;
            go.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;
        }

        return frameParent;
    }

    public static List<GameObject> SkirtingWithDoor(GameObject room, GameObject wall,Vector3 doorPosition, Vector3 p1,Vector3 p2,Vector3 lookDirection,float doorWidth,Divide divide)
    {
        //first floor is fucked
        if (room.transform.position.y == 0) 
            lookDirection = -lookDirection;

        List<GameObject> skirts = new List<GameObject>();
        //upstairs skirting building wrong way, flip direction - not sure why this is happening, and will take me a while to fix- hack it just now

        if (room.transform.position.y > 0)
        {
            lookDirection = -lookDirection;
             doorPosition += room.transform.position;
             p1 += room.transform.position;
             p2 += room.transform.position;
        }



        //add y

        //already adjusted earlier for this - everythin needs standardised and rewritten
        if (room.name == "Hall1" || room.name == "Hall2" || room.name == "Landing")
        {
            
        }
        else
        {
           // doorPosition += room.transform.position;
           // p1 += room.transform.position;
          //  p2 += room.transform.position;
        }
        //build first side
        //find out which wall point to use, it is the point on the right hand side of the door
        Vector3 pointToUse = p1;
        Vector3 angle = Vector3.Cross(lookDirection, doorPosition - p1);

        if (angle.y > 0)
            pointToUse = p2;

        Vector3 endOfDoor = doorPosition + (pointToUse - doorPosition).normalized * doorWidth * 0.5f;

        

        GameObject skirting = GameObject.CreatePrimitive(PrimitiveType.Cube);
        skirting.transform.position = Vector3.Lerp(pointToUse, endOfDoor, 0.5f) + Vector3.up*divide.skirtingHeight*.5f;        
        skirting.transform.LookAt(skirting.transform.position + lookDirection);
        skirting.transform.position += skirting.transform.forward * (divide.skirtingDepth*.25f+.1f);
        skirting.transform.localScale = new Vector3(Vector3.Distance(pointToUse, endOfDoor), divide.skirtingHeight, divide.skirtingDepth/2);
        skirting.name = "Skirting";
        skirting.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;
        skirting.transform.parent = wall.transform;
        skirts.Add(skirting);

        if (pointToUse == p1)
            pointToUse = p2;
        else if (pointToUse == p2)
            pointToUse = p1;

        endOfDoor = doorPosition + (pointToUse - doorPosition).normalized * doorWidth * 0.5f;

        skirting = GameObject.CreatePrimitive(PrimitiveType.Cube);
        skirting.transform.position = Vector3.Lerp(pointToUse, endOfDoor, 0.5f) + Vector3.up * divide.skirtingHeight*.5f ;
        skirting.transform.LookAt(skirting.transform.position + lookDirection);
        skirting.transform.position += skirting.transform.forward * (divide.skirtingDepth*.25f + .1f);
        skirting.transform.localScale = new Vector3(Vector3.Distance(pointToUse, endOfDoor), divide.skirtingHeight, divide.skirtingDepth/2);
        skirting.name = "Skirting";
        skirting.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;

        skirting.transform.parent = wall.transform;
        skirts.Add(skirting);

        return skirts;
    }

    public static GameObject SkirtingWithNoDoor(GameObject wall,Vector3 p1, Vector3 p2,Vector3 lookDirection,Divide divide,int floor)
    {
        lookDirection = -lookDirection;

        GameObject skirting = GameObject.CreatePrimitive(PrimitiveType.Cube);
        skirting.transform.position = Vector3.Lerp(p1, p2, 0.5f) + Vector3.up * divide.skirtingHeight * .5f;
        skirting.transform.LookAt(skirting.transform.position + lookDirection);
        skirting.transform.position += skirting.transform.forward * (divide.skirtingDepth * .25f + .1f);
        //add floor
        //skirting.transform.position -= Vector3.up * floor * divide.storeyHeight;
        skirting.transform.localScale = new Vector3(Vector3.Distance(p1, p2), divide.skirtingHeight, divide.skirtingDepth / 2);
        skirting.name = "Skirting";
        skirting.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;
        skirting.transform.parent = wall.transform;
        skirting.name = "skirtnoDoor " + floor.ToString();
        return skirting;
    }

    public static GameObject OnlyDoor(GameObject room, Vector3 p1, Vector3 p2, Vector3 doorPoint, bool exterior, float storeyHeight, float doorHeight, float doorWidth,Divide divide)
    {
        
        //apply rotations from room //could alternatively do this before these points are passed, doing here for cloarity on first script
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

        GameObject door = DoorAtPosition(doorPoint, lookDir, room, storeyHeight, doorHeight, doorWidth,divide);

        //da frame
        GameObject df = DoorFrame(room, door, divide);
        //add directly to list for interior objects- add as list - thisis different from how we add all other interior assets
        List<GameObject> gos = new List<GameObject>();
        gos.Add(df);
        divide.interiorAssetsByRoom.Add(gos);

        return door;

    }

    public static GameObject DoorAtPosition(Vector3 position, Vector3 lookDirection,GameObject room,float storeyHeight, float doorHeight,float doorWidth,Divide divide)
    {
        //add transform position and rotation to point given
        position = room.transform.position + ( room.transform.rotation * position);
        lookDirection =( room.transform.rotation * lookDirection);

        //prepare a mesh
        //create cube for manipulating
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(cube.GetComponent<BoxCollider>());

        cube.name = "Door";
        cube.transform.parent = room.transform;
        cube.transform.position = position;
        if (lookDirection != Vector3.zero)
            cube.transform.rotation = Quaternion.LookRotation(lookDirection);
        else
            Debug.Log("Look Rotation was zero - applying no rotation to door");
      //  cube.transform.rotation *= room.transform.rotation;

        Mesh mesh = cube.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        //scale 
        //float doorWidth = 0.8f;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].z *= 0.1f; //depth?
            vertices[i] *= doorWidth; //needs to be a multiple of brickSize
        }

        float brickSize = 0.1f;
        //float storeyHeight = 3f;////////?????
        float amountOfRows = storeyHeight / (brickSize);
                
        amountOfRows = Mathf.Round(amountOfRows);
        //two thirds of wall size
        //float doorSize = amountOfRows / 3;

        //doorSize *= 2;
        //doorSize = Mathf.Round(doorSize);
        
        int[] topVertices = VerticeArray("top");
        //stretch vertices on the top of the cube to correct height
        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = doorHeight;// * brickSize;
        }

        //bottom of room
        //float bottom = ((room.transform.rotation * room.GetComponent<MeshFilter>().mesh.vertices[2])).y; 
        float bottom = room.transform.position.y;

        //make space for doorstep

        float doorStepHeight = 0.02f;

        //do the same for the bottom
        int[] bottomVertices = VerticeArray("bottom");
        for (int i = 0; i < bottomVertices.Length; i++)
        {
            //+= doorstep
            //vertices[bottomVertices[i]].y = 0 + brickSize;
            vertices[bottomVertices[i]].y = doorStepHeight;
        }

        mesh.vertices = vertices;

        //cube.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("Blue", typeof(Material)) as Material;

        //door frame
        GameObject df = DoorFrame(room,cube,divide);
        //addin to interor asset list directly
        List<GameObject> gos = new List<GameObject>();
        gos.Add(df);
        divide.interiorAssetsByRoom.Add(gos);


        //door model
        InteriorAssets.Doors.BedroomDoor(cube, divide);

        cube.transform.localScale =new Vector3( 1f - divide.skirtingHeight * .5f, 1f, 1f - divide.skirtingHeight * .5f);

        return cube;
    }

    public static List<GameObject> WallAroundDoor(Vector3 position, float segmentWidth, Vector3 lookDirection, bool exterior, GameObject room,float storeyHeight, float doorHeight,float doorWidth)
    {
        List<GameObject> walls = new List<GameObject>();
        //add rotations and transform positions
        position= room.transform.position + (room.transform.rotation * position);
        lookDirection = (room.transform.rotation * lookDirection);

        //build space under door to width of one segment
        GameObject belowDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(belowDoor.GetComponent<BoxCollider>());
        belowDoor.transform.position = position;
        belowDoor.transform.parent = room.transform;
        belowDoor.transform.rotation = Quaternion.LookRotation(lookDirection);
        belowDoor.name = "Below Door";
        walls.Add(belowDoor);

        Vector3[] vertices = belowDoor.GetComponent<MeshFilter>().mesh.vertices;

        float brickSize = 0.1f;//global

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize; 
        }

        int[] bottomVertices = VerticeArray("bottom");
        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = 0;
        }
        int[] topVertices = VerticeArray("top");
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
           // belowDoor.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        //above door

        //build space under door to width of one segment
        GameObject aboveDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(aboveDoor.GetComponent<BoxCollider>());
        aboveDoor.transform.position = position;
        aboveDoor.transform.parent = room.transform;
        aboveDoor.transform.rotation = Quaternion.LookRotation(lookDirection);
        aboveDoor.name = "Above Door";
        walls.Add(aboveDoor);
        vertices = aboveDoor.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        //float storeyHeight = 3f;//******************************find height in hierarchy?
   //     float amountOfRows = storeyHeight/ (brickSize);
   //     amountOfRows = Mathf.Round(amountOfRows);
        //two thirds of wall size
        //float doorSize = storeyHeight / 3;
       // doorSize *= 2;
        //doorSize = Mathf.Round(doorSize);

        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = doorHeight;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = storeyHeight;
        }

        aboveDoor.GetComponent<MeshFilter>().mesh.vertices = vertices;
        aboveDoor.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        aboveDoor.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            aboveDoor.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            aboveDoor.transform.position += lookDirection.normalized * brickSize * 0.5f;
           // aboveDoor.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        //to each side

        //build space under door to width of one segment
        GameObject rightSideOfDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(rightSideOfDoor.GetComponent<BoxCollider>());
        rightSideOfDoor.transform.position = position;
        rightSideOfDoor.transform.parent = room.transform;
        rightSideOfDoor.transform.rotation = Quaternion.LookRotation(lookDirection);
        rightSideOfDoor.name = "Right Side Of Door";
        walls.Add(rightSideOfDoor);
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
            vertices[topVertices[i]].y = doorHeight;
        }

        int[] rightVertices = VerticeArray("right");
        //float doorWidth = 0.8f; //***********************how to set??

        for (int i = 0; i < rightVertices.Length; i++)
        {

            vertices[rightVertices[i]].x = -doorWidth / 2;
        }

        rightSideOfDoor.GetComponent<MeshFilter>().mesh.vertices = vertices;
        rightSideOfDoor.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        rightSideOfDoor.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            rightSideOfDoor.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            rightSideOfDoor.transform.position += lookDirection.normalized * brickSize * 0.5f;
         //   rightSideOfDoor.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        GameObject leftSideOfDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(leftSideOfDoor.GetComponent<BoxCollider>());
        leftSideOfDoor.transform.position = position;
        leftSideOfDoor.transform.parent = room.transform;
        leftSideOfDoor.transform.rotation = Quaternion.LookRotation(lookDirection);
        leftSideOfDoor.name = "Left Side Of Door";
        walls.Add(leftSideOfDoor);
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
            vertices[topVertices[i]].y = doorHeight;
        }
        int[] leftVertices = VerticeArray("left");
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
          //  leftSideOfDoor.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        return walls;
    }

    public static List<GameObject> WallAroundDoorWithOffset(Vector3 doorPosition, Vector3 wall1, Vector3 wall2,float segmentWidth, Vector3 lookDirection, bool exterior, GameObject room,float storeyHeight,float doorHeight,float doorWidth)
    {
        List<GameObject> walls = new List<GameObject>();
        //add rotations and transform positions to points passed
        doorPosition = room.transform.position + (room.transform.rotation * doorPosition);
        wall1 = room.transform.position + (room.transform.rotation * wall1);
        wall2 = room.transform.position + (room.transform.rotation * wall2);
        lookDirection = (room.transform.rotation * lookDirection);

        //build space under door to width of one segment
        GameObject belowDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(belowDoor.GetComponent<BoxCollider>());
        belowDoor.transform.position = Vector3.Lerp(wall1, wall2, 0.5f);        
        belowDoor.transform.parent = room.transform;
        if(lookDirection != Vector3.zero)
            belowDoor.transform.rotation = Quaternion.LookRotation(lookDirection);
        else
            Debug.Log("Look Rotation was zero - applying no rotation to belowDoor");

        belowDoor.name = "Below Door";
        walls.Add(belowDoor);

        Vector3[] vertices = belowDoor.GetComponent<MeshFilter>().mesh.vertices;

        float brickSize = 0.1f;//global
        float doorStepHeight = 0.02f;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        int[] bottomVertices = VerticeArray("bottom");
        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = 0;
        }
        int[] topVertices = VerticeArray("top");
        for (int i = 0; i < topVertices.Length; i++)
        {
            //            vertices[topVertices[i]].y = brickSize;
            vertices[topVertices[i]].y = doorStepHeight;
        }

        belowDoor.GetComponent<MeshFilter>().mesh.vertices = vertices;
        belowDoor.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        belowDoor.GetComponent<MeshFilter>().mesh.RecalculateBounds();

         
        if (!exterior)
            belowDoor.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            belowDoor.transform.position += lookDirection.normalized * brickSize * 0.5f;
            // belowDoor.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        //above door

        //build space under door to width of one segment
        GameObject aboveDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(aboveDoor.GetComponent<BoxCollider>());
        aboveDoor.transform.position = Vector3.Lerp(wall1, wall2, 0.5f);
        aboveDoor.transform.parent = room.transform;
        aboveDoor.transform.rotation = Quaternion.LookRotation(lookDirection);
        aboveDoor.name = "Above Door";
        walls.Add(aboveDoor);
        vertices = aboveDoor.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

       // float storeyHeight = 3f;//******************************find height in hierarchy?
                                //     float amountOfRows = storeyHeight/ (brickSize);
                                //     amountOfRows = Mathf.Round(amountOfRows);
                                //two thirds of wall size
        //float doorSize = storeyHeight / 3;
        //doorSize *= 2;
        //doorSize = Mathf.Round(doorSize);
        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = doorHeight;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = storeyHeight;
        }

        aboveDoor.GetComponent<MeshFilter>().mesh.vertices = vertices;
        aboveDoor.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        aboveDoor.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            aboveDoor.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            aboveDoor.transform.position += lookDirection.normalized * brickSize * 0.5f;
            // aboveDoor.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        //to each side

        
        GameObject rightSideOfDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(rightSideOfDoor.GetComponent<BoxCollider>());
        rightSideOfDoor.transform.position = doorPosition;
        rightSideOfDoor.transform.parent = room.transform;
        rightSideOfDoor.transform.rotation = Quaternion.LookRotation(lookDirection);
        rightSideOfDoor.name = "Right Side Of Door";
        walls.Add(rightSideOfDoor);


        vertices = rightSideOfDoor.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
           // vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        for (int i = 0; i < bottomVertices.Length; i++)
        {
            //vertices[bottomVertices[i]].y = brickSize;
            vertices[bottomVertices[i]].y = doorStepHeight;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = doorHeight;
        }
        int[] rightVertices = VerticeArray("right");
        //float doorWidth = 0.8f; //***********************how to set??

        //find out which wall point to use, it is the point on the right hand side of the door
        Vector3 wallToUse = wall1;
        Vector3 angle = Vector3.Cross(lookDirection, doorPosition - wall1);
        
        if (angle.y > 0)
            wallToUse = wall2;

        for (int i = 0; i < rightVertices.Length; i++)
        {
            vertices[rightVertices[i]].x = Vector3.Distance(doorPosition, wallToUse);
        }

        int[] leftVertices = VerticeArray("left");
        for (int i = 0; i < leftVertices.Length; i++)
        {
            vertices[leftVertices[i]].x = doorWidth / 2; ;
        }

        rightSideOfDoor.GetComponent<MeshFilter>().mesh.vertices = vertices;
        rightSideOfDoor.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        rightSideOfDoor.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            rightSideOfDoor.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            rightSideOfDoor.transform.position += lookDirection.normalized * brickSize * 0.5f;
            //   rightSideOfDoor.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }
        
        GameObject leftSideOfDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(leftSideOfDoor.GetComponent<BoxCollider>());
        leftSideOfDoor.transform.position = doorPosition;
        leftSideOfDoor.transform.parent = room.transform;
        leftSideOfDoor.transform.rotation = Quaternion.LookRotation(lookDirection);
        leftSideOfDoor.name = "Left Side Of Door";
        walls.Add(leftSideOfDoor);
        vertices = leftSideOfDoor.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
           // vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        for (int i = 0; i < bottomVertices.Length; i++)
        {
            // vertices[bottomVertices[i]].y = brickSize;
            vertices[bottomVertices[i]].y = doorStepHeight;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = doorHeight;
        }
        //find out which wall point to use, this time, it is the point on the LEFT hand side of the door
        wallToUse = wall1;
        angle = Vector3.Cross(lookDirection, doorPosition - wall1);
        
        if (angle.y < 0)
            wallToUse = wall2;

        for (int i = 0; i < leftVertices.Length; i++)
        {
            vertices[leftVertices[i]].x = -Vector3.Distance(doorPosition, wallToUse);            
        }
              

        for (int i = 0; i < rightVertices.Length; i++)
        {
            vertices[rightVertices[i]].x = -doorWidth / 2;
        }

        leftSideOfDoor.GetComponent<MeshFilter>().mesh.vertices = vertices;
        leftSideOfDoor.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        leftSideOfDoor.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            leftSideOfDoor.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            leftSideOfDoor.transform.position += lookDirection.normalized * brickSize * 0.5f;
            //  leftSideOfDoor.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }


        return walls;
    }

    public static List<GameObject> WallAroundWindowWithOffset(Vector3 doorPosition, Vector3 wall1, Vector3 wall2, float segmentWidth, Vector3 lookDirection, bool exterior, GameObject room, float storeyHeight,float doorHeight, float windowHeight, float windowWidth,Divide divide,int floor)
    {
        List<GameObject> walls = new List<GameObject>();
        //add rotations and transform positions to points passed
        doorPosition = room.transform.position + (room.transform.rotation * doorPosition);
        wall1 = room.transform.position + (room.transform.rotation * wall1);
        wall2 = room.transform.position + (room.transform.rotation * wall2);
        lookDirection = (room.transform.rotation * lookDirection);

        //build space under door to width of one segment
        GameObject belowDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(belowDoor.GetComponent<BoxCollider>());
        belowDoor.transform.position = Vector3.Lerp(wall1, wall2, 0.5f);
        belowDoor.transform.parent = room.transform;
        if (lookDirection != Vector3.zero)
            belowDoor.transform.rotation = Quaternion.LookRotation(lookDirection);
        else
            Debug.Log("Look Rotation was zero - applying no rotation to belowDoor");

        belowDoor.name = "Below Door";
        walls.Add(belowDoor);

        Vector3[] vertices = belowDoor.GetComponent<MeshFilter>().mesh.vertices;

        float brickSize = 0.1f;//global

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        int[] bottomVertices = VerticeArray("bottom");
        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = 0;
        }
        int[] topVertices = VerticeArray("top");


        //change??
        float bottomOfWindow = doorHeight - windowHeight; //equal above and below
        
        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = bottomOfWindow;
        }

        belowDoor.GetComponent<MeshFilter>().mesh.vertices = vertices;
        belowDoor.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        belowDoor.GetComponent<MeshFilter>().mesh.RecalculateBounds();


        if (!exterior)
            belowDoor.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            belowDoor.transform.position += lookDirection.normalized * brickSize * 0.5f;
            // belowDoor.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        //above door

        //build space under door to width of one segment
        GameObject aboveDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(aboveDoor.GetComponent<BoxCollider>());
        aboveDoor.transform.position = Vector3.Lerp(wall1, wall2, 0.5f);
        aboveDoor.transform.parent = room.transform;
        aboveDoor.transform.rotation = Quaternion.LookRotation(lookDirection);
        aboveDoor.name = "Above Door";
        walls.Add(aboveDoor);
        vertices = aboveDoor.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        // float storeyHeight = 3f;//******************************find height in hierarchy?
        //     float amountOfRows = storeyHeight/ (brickSize);
        //     amountOfRows = Mathf.Round(amountOfRows);
        //two thirds of wall size
        //float doorSize = storeyHeight / 3;
        //doorSize *= 2;
        //doorSize = Mathf.Round(doorSize);
        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = doorHeight;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = storeyHeight;
        }

        aboveDoor.GetComponent<MeshFilter>().mesh.vertices = vertices;
        aboveDoor.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        aboveDoor.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            aboveDoor.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            aboveDoor.transform.position += lookDirection.normalized * brickSize * 0.5f;
            // aboveDoor.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        //to each side


        GameObject rightSideOfDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(rightSideOfDoor.GetComponent<BoxCollider>());
        rightSideOfDoor.transform.position = doorPosition;
        rightSideOfDoor.transform.parent = room.transform;
        rightSideOfDoor.transform.rotation = Quaternion.LookRotation(lookDirection);
        rightSideOfDoor.name = "Right Side Of Door";
        walls.Add(rightSideOfDoor);


        vertices = rightSideOfDoor.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            // vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = bottomOfWindow;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = doorHeight;
        }
        int[] rightVertices = VerticeArray("right");
        //float doorWidth = 0.8f; //***********************how to set??

        //find out which wall point to use, it is the point on the right hand side of the door
        Vector3 wallToUse = wall1;
        Vector3 angle = Vector3.Cross(lookDirection, doorPosition - wall1);

        if (angle.y > 0)
            wallToUse = wall2;

        for (int i = 0; i < rightVertices.Length; i++)
        {
            vertices[rightVertices[i]].x = Vector3.Distance(doorPosition, wallToUse);
        }

        int[] leftVertices = VerticeArray("left");
        for (int i = 0; i < leftVertices.Length; i++)
        {
            vertices[leftVertices[i]].x = windowWidth / 2; ;
        }

        rightSideOfDoor.GetComponent<MeshFilter>().mesh.vertices = vertices;
        rightSideOfDoor.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        rightSideOfDoor.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            rightSideOfDoor.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            rightSideOfDoor.transform.position += lookDirection.normalized * brickSize * 0.5f;
            //   rightSideOfDoor.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        GameObject leftSideOfDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(leftSideOfDoor.GetComponent<BoxCollider>());
        leftSideOfDoor.transform.position = doorPosition;
        leftSideOfDoor.transform.parent = room.transform;
        leftSideOfDoor.transform.rotation = Quaternion.LookRotation(lookDirection);
        leftSideOfDoor.name = "Left Side Of Door";
        walls.Add(leftSideOfDoor);
        vertices = leftSideOfDoor.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            // vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = bottomOfWindow;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = doorHeight;
        }
        //find out which wall point to use, this time, it is the point on the LEFT hand side of the door
        wallToUse = wall1;
        angle = Vector3.Cross(lookDirection, doorPosition - wall1);

        if (angle.y < 0)
            wallToUse = wall2;

        for (int i = 0; i < leftVertices.Length; i++)
        {
            vertices[leftVertices[i]].x = -Vector3.Distance(doorPosition, wallToUse);
        }


        for (int i = 0; i < rightVertices.Length; i++)
        {
            vertices[rightVertices[i]].x = -windowWidth/ 2;
        }

        leftSideOfDoor.GetComponent<MeshFilter>().mesh.vertices = vertices;
        leftSideOfDoor.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        leftSideOfDoor.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            leftSideOfDoor.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            leftSideOfDoor.transform.position += lookDirection.normalized * brickSize * 0.5f;
            //  leftSideOfDoor.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        if (!exterior)
        {
            //parent to bottom part of wall
            GameObject skirt = SkirtingWithNoDoor(belowDoor, wall1, wall2, lookDirection, divide, floor);
            //add directly to interior list
            divide.interiorAssetsByRoom.Add(new List<GameObject>() { skirt });
        }
        return walls;
    }

    public static GameObject WindowAtPosition(Vector3 position,float doorHeight,float height, float width, Vector3 lookDirection,GameObject room,float storeySize,Divide divide)
    {
    
        //add rotations and transform positions
        position = room.transform.position + (room.transform.rotation * position);
        lookDirection = (room.transform.rotation * lookDirection);
        // float brickSize = 0.1f; //global
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(cube.GetComponent<BoxCollider>());
        cube.transform.parent = room.transform;
        cube.transform.position = position;
        cube.transform.rotation = Quaternion.LookRotation(lookDirection);

        //let's scale the mesh

        Mesh mesh = cube.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        //scale the whole mesh
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= width;// width - (brickSize * 2);
            vertices[i].y *= height;
            vertices[i].z *= 0.05f;
        }
        
      

        //stretch vertices on the top of the cube to correct height
        int[] topVertices = VerticeArray("top");

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = doorHeight;
        }

        float bottomOfWindow = doorHeight - height; //MAGIC, same as wall with window hole - change the 3 to a var
       

        int[] bottomVertices = VerticeArray("bottom");

        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = bottomOfWindow;
        }
        mesh.vertices = vertices;

        cube.AddComponent<MeshCollider>();
        cube.name = "Window";

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        WindowFrame(room, cube, divide);


        return cube;
        //cube.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("Glass", typeof(Material)) as Material;
    }

    public static List<GameObject> WallAroundWindow(Vector3 position, float segmentWidth, Vector3 lookDirection, bool exterior,GameObject room,float storeyHeight,float windowHeight,float windowWidth)
    {
        List<GameObject> walls = new List<GameObject>();

        //add rotations and transform positions
        position = room.transform.position + (room.transform.rotation * position);
        lookDirection = (room.transform.rotation * lookDirection);

        float brickSize = 0.1f;

        //build space under width of one segment
        GameObject below = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(below.GetComponent<BoxCollider>());
        below.transform.position = position;
        below.transform.parent = room.transform;
        below.transform.rotation = Quaternion.LookRotation(lookDirection);
        below.name = "Below Window";
        walls.Add(below);

        Vector3[] vertices = below.GetComponent<MeshFilter>().mesh.vertices;
        

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        int[] bottomVertices = VerticeArray("bottom");
        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = 0;
        }
        int[] topVertices = VerticeArray("top");

        
        float bottomOfWindow = storeyHeight / 4;
        
        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = bottomOfWindow;
        }

        below.GetComponent<MeshFilter>().mesh.vertices = vertices;
        below.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        below.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            below.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            below.transform.position += lookDirection.normalized * brickSize * 0.5f;
          //  below.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        //above
        GameObject above = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(above.GetComponent<BoxCollider>());
        above.transform.position = position;
        above.transform.parent = room.transform;
        above.transform.rotation = Quaternion.LookRotation(lookDirection);
        above.name = "Above Window";
        walls.Add(above);
        vertices = above.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }
        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = windowHeight/2 + storeyHeight/2;// * brickSize;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = storeyHeight;
        }

        above.GetComponent<MeshFilter>().mesh.vertices = vertices;
        above.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        above.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            above.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            above.transform.position += lookDirection.normalized * brickSize * 0.5f;
           // above.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        //side
        GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(left.GetComponent<BoxCollider>());
        left.transform.position = position;
        left.transform.parent = room.transform;
        left.transform.rotation = Quaternion.LookRotation(lookDirection);
        left.name = "Left Of The Window";
        walls.Add(left);
        vertices = left.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }
        
        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = bottomOfWindow;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = storeyHeight / 2 + (windowHeight / 2);
        }

        int[] leftVertices = VerticeArray("left");
        for (int i = 0; i < leftVertices.Length; i++)
        {
            vertices[leftVertices[i]].x = (segmentWidth / 2) - brickSize;
        }

        int[] rightVertices = VerticeArray("right");

        for (int i = 0; i < rightVertices.Length; i++)
        {
            vertices[leftVertices[i]].x = windowWidth/2;
        }

        left.GetComponent<MeshFilter>().mesh.vertices = vertices;
        left.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        left.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            left.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            left.transform.position += lookDirection.normalized * brickSize * 0.5f;
          //  left.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        //side
        GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(right.GetComponent<BoxCollider>());
        right.transform.position = position;
        right.transform.parent = room.transform;
        right.transform.rotation = Quaternion.LookRotation(lookDirection);
        right.name = "Right Of The Window";
        walls.Add(right);
        vertices = right.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;
        }

        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = bottomOfWindow;
        }

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = storeyHeight / 2 + (windowHeight / 2);
        }
        
        for (int i = 0; i < rightVertices.Length; i++)
        {
            vertices[rightVertices[i]].x = -windowWidth / 2;
        }

        for (int i = 0; i < leftVertices.Length; i++)
        {
            vertices[leftVertices[i]].x = -(segmentWidth / 2);
        }

        right.GetComponent<MeshFilter>().mesh.vertices = vertices;
        right.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        right.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            right.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            right.transform.position += lookDirection.normalized * brickSize * 0.5f;
         //   right.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        return walls;
    }

    public static GameObject Wall(Vector3 position, float segmentWidth, Vector3 lookDirection, bool exterior, GameObject room,float storeyHeight)
    {
        //add rotations and transform positions
       // position = room.transform.position + (room.transform.rotation * position);
        lookDirection = (room.transform.rotation * lookDirection);

        float brickSize = 0.1f;

        //build space under width of one segment
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(wall.GetComponent<BoxCollider>());
        wall.transform.position = position;
        wall.transform.parent = room.transform;
        wall.transform.rotation = Quaternion.LookRotation(lookDirection);
        wall.name = "Wall";

        Vector3[] vertices = wall.GetComponent<MeshFilter>().mesh.vertices;
        
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= segmentWidth;
            vertices[i].z *= brickSize;

            
        }

        int[] bottomVertices = VerticeArray("bottom");
        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = 0;
        }
        int[] topVertices = VerticeArray("top");

        //float storeyHeight = 3f;////////?????       
       // float bottomOfWindow = storeyHeight / 4;

        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = storeyHeight;
        }

        wall.GetComponent<MeshFilter>().mesh.vertices = vertices;
        wall.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        wall.GetComponent<MeshFilter>().mesh.RecalculateBounds();

        if (!exterior)
            wall.transform.position -= lookDirection.normalized * brickSize * 0.5f;
        else if (exterior)
        {
            wall.transform.position += lookDirection.normalized * brickSize * 0.5f;
            //  below.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
        }

        

        return wall;
    }

    public static GameObject WallWithLookDirection(GameObject room,Vector3 p1, Vector3 p2, float storeyHeight,float doorHeight,float doorWidth,int floor,bool exterior,Divide divide)
    {
        GameObject wall = null;
        
            //build door using house builder class
            Vector3 centre = room.GetComponent<MeshRenderer>().bounds.center;

        //create points each side of the line - use "world" coordinates, because "centre" is a world co-ordinate
        Vector3 lookDir1 = Quaternion.Euler(0, 90, 0) * (p2 - p1).normalized;
        Vector3 lookDir2 = Quaternion.Euler(0, -90, 0) * (p2 - p1).normalized;
        Vector3 lookDir = Vector3.zero;
        //check which is closest - use that rotation to build wall
        Vector3 mid = Vector3.Lerp(p1, p2, 0.5f);
        if (Vector3.Distance(mid + lookDir1, centre) > Vector3.Distance(mid + lookDir2, centre))
            lookDir = Quaternion.Euler(0, 90, 0) * (p2- p1).normalized;    //save as local direction
        else
            lookDir = Quaternion.Euler(0, -90, 0) * (p2- p1).normalized; //save as local direction

        if (exterior)
            lookDir = -lookDir;

        float length = Vector3.Distance(p1, p2);

        wall = HouseBuilder.Wall(mid, length, lookDir, false, room, storeyHeight*floor);
        //wall.transform.position += Vector3.up * storeyHeight;

        //skirting
        if (!exterior)
        {
            GameObject skirt = SkirtingWithNoDoor(room, p1, p2, lookDir, divide, floor);
            
            //adding directly to list
            divide.interiorAssetsByRoom.Add(new List<GameObject>() { skirt });
        }

        return wall;
    }

    public static GameObject CornerColumn(Vector3 frontLeft, Vector3 frontRight,GameObject room,int floors,float storeyHeight)
    {
        frontLeft = room.transform.position + (room.transform.rotation * frontLeft);
        frontRight = room.transform.position + (room.transform.rotation * frontRight);

        float brickSize = 0.1f;

        //add gap betweenfloors
        if (floors == 2)
            storeyHeight += 0.2f;

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(cube.GetComponent<BoxCollider>());
        cube.transform.parent = room.transform;
        cube.transform.position = frontLeft;

        cube.transform.rotation = Quaternion.LookRotation(Quaternion.Euler(0, 180, 0) * (frontRight - frontLeft));

        cube.transform.position += cube.transform.right * brickSize * 0.5f;
        cube.transform.position += cube.transform.forward * brickSize * 0.5f;

        Mesh mesh = cube.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        //scale
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] *= brickSize;
        }
        int[] topVertices = VerticeArray("top");
            
        //stretch
        for (int i = 0; i < topVertices.Length; i++)
        {
            vertices[topVertices[i]].y = storeyHeight*floors;
        }
        int[] bottomVertices = VerticeArray("bottom");
        for (int i = 0; i < bottomVertices.Length; i++)
        {
            vertices[bottomVertices[i]].y = 0f;
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();

        return cube;

        //cube.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("RosePink", typeof(Material)) as Material;
    }

    public static GameObject Roof(GameObject gameObject,GameObject plot,float storeyHeight,int floors)
    {
        Vector3[] vertices = plot.GetComponent<MeshFilter>().mesh.vertices;
        //we need the length of the shortest edge of the house
        int[] shortestEdge = Divide.ShortestEdge(vertices);
        float shortDistance = Vector3.Distance(vertices[shortestEdge[0]], vertices[shortestEdge[1]]);
        //find long edge, we will have the roof follow this
        int[] longestEdge = Divide.LongestEdge(vertices);
        float longDistance = Vector3.Distance(vertices[longestEdge[0]], vertices[longestEdge[1]]);

        //choose shape of roof
        //how many horizontals will the roof by split by
        int sections = Random.Range(1,3);
        //will the roof have a flat side (brick wall) or slanted
        //it's called a hip apparently
        bool hip = false;
        //if (Random.Range(0, 2) == 0)
            hip = true;
        //flatten?
        bool flatHat = false;
        if (Random.Range(0, 4) == 1)
        {
            flatHat = true;
        }
        //how far will this hip recede?
        float hipLength = Random.Range((shortDistance/3)/sections, (shortDistance / 3) / sections);
        float peakHeight = Random.Range((shortDistance/2)*sections, (shortDistance/1)*sections);
        if (flatHat)
        {
            peakHeight = storeyHeight * Random.Range(1.0f, 1.5f);//looks cool with top hat
            
        }
        //overhang at gable
        float gableOverhang = Random.Range(1f, 2f);
        float frontOverhang = Random.Range(1f, 2f);
        
        
       
        //centre of house
        Vector3 centre = plot.GetComponent<MeshRenderer>().bounds.center;
        
        
        Vector3 start0 = vertices[longestEdge[0]];
        Vector3 start1 = vertices[longestEdge[1]];
        Vector3 midOfLongestEdge = Vector3.Lerp(vertices[longestEdge[0]], vertices[longestEdge[1]], 0.5f);
        
        //add elevation/front overhang
        start0 -= (centre - midOfLongestEdge).normalized * frontOverhang;
        start1 -= (centre - midOfLongestEdge).normalized * frontOverhang;
        //add gable end overhang
        Vector3 dirToCentreFrom0 = (midOfLongestEdge - vertices[longestEdge[0]]).normalized;
        Vector3 dirToCentreFrom1 = (midOfLongestEdge - vertices[longestEdge[1]]).normalized;

        start0 -= dirToCentreFrom0 * gableOverhang;
        start1 -= dirToCentreFrom1 * gableOverhang;

        //create vector to point to peak of roof
        Vector3 midOfAlteredStarts = Vector3.Lerp(start0, start1, 0.5f);
        Vector3 dirToCentreZ = centre - midOfAlteredStarts;
        

        dirToCentreZ += Vector3.up * peakHeight;
        dirToCentreZ /= sections;
                

        //saving points on previous loop
        Vector3 prev0 = start0;
        Vector3 prev1 = start1;
        //if flat top these will be used on second step
        Vector3 prev2 = new Vector3();
        Vector3 prev3 = new Vector3();

        //Lists for vertice data
        List<Vector3> roofVertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        List<Vector3> roofVerticesHip = new List<Vector3>();
        List<int> trianglesHip = new List<int>();

       
       
        for (int i = 0; i < sections; i++)
        {
            Vector3 p0 = prev0;
            Vector3 p1 = prev1;
            Vector3 dirToNext = dirToCentreZ;
            dirToNext -= Vector3.up / sections * (i * Random.Range(1.5f, 1.75f));

            Vector3 p2 = p0 + (dirToNext);
            Vector3 p3 = p1 + (dirToNext);

            if (flatHat && i == 1)
            {
                continue;
            }

            //pull in if hip
            if (hip)
            {
                p2 += dirToCentreFrom0 * (hipLength * (i + 1));
                p3 += dirToCentreFrom1 * (hipLength * (i + 1));
            }
            //we can make a litle peak, or a flat hat

            //flatten upper vertices - we made the peak massive to get a steep angle - this "edge slides" the point back down
            if (flatHat && i == 0)
            {
                p2 = vertices[longestEdge[0]] + Vector3.up * peakHeight;
                p3 = vertices[longestEdge[1]] + Vector3.up * peakHeight;
            }

            roofVertices.Add(p0); roofVertices.Add(p1); roofVertices.Add(p2); roofVertices.Add(p3);
            //0//1//2
            triangles.Add((roofVertices.Count) - 4);//count is 4
            triangles.Add((roofVertices.Count) - 3);//count is 4
            triangles.Add((roofVertices.Count) - 2);//count is 4
                                                    //1/3//2
            triangles.Add((roofVertices.Count) - 3);//count is 4
            triangles.Add((roofVertices.Count) - 1);//count is 4
            triangles.Add((roofVertices.Count) - 2);//count is 4

            prev0 = p2;
            prev1 = p3;
            prev2 = p0;
            prev3 = p1;


        }

        List<Vector3> gableEndVertices = new List<Vector3>();
        List<int> gableEndTriangles = new List<int>();


        //gable end /hip
        //dont'add overhang 
        prev0 = vertices[longestEdge[0]];
        prev1 = vertices[longestEdge[1]];
        
        
       
            if (flatHat)
            {
                roofVerticesHip.Add(roofVertices[0]);
                roofVerticesHip.Add(roofVertices[2] + (centre - midOfAlteredStarts).normalized * shortDistance);
                roofVerticesHip.Add(roofVertices[2]);

                trianglesHip.Add(0);
                trianglesHip.Add(2);
                trianglesHip.Add(1);

                roofVerticesHip.Add(roofVertices[2] + (centre - midOfAlteredStarts).normalized * shortDistance);
                roofVerticesHip.Add(roofVertices[0]);
                roofVerticesHip.Add(roofVertices[0] + (centre - midOfAlteredStarts).normalized * (shortDistance + (frontOverhang * 2)));//woo

                trianglesHip.Add(3);
                trianglesHip.Add(5);
                trianglesHip.Add(4);

                roofVerticesHip.Add(roofVertices[2]);
                roofVerticesHip.Add(roofVertices[2] + (centre - midOfAlteredStarts).normalized * shortDistance);
                roofVerticesHip.Add(roofVertices[3]);

                trianglesHip.Add(6);
                trianglesHip.Add(8);
                trianglesHip.Add(7);

            }
        
        else if (sections == 1)
        {
            roofVerticesHip.Add(roofVertices[0]);
            roofVerticesHip.Add(roofVertices[0] + (centre - midOfAlteredStarts));
            roofVerticesHip.Add(roofVertices[2]);

            trianglesHip.Add(0);
            trianglesHip.Add(2);
            trianglesHip.Add(1);

            roofVerticesHip.Add(roofVertices[0] + (centre - midOfAlteredStarts));
            roofVerticesHip.Add(roofVertices[0] + (centre - midOfAlteredStarts) + (centre - midOfAlteredStarts));
            roofVerticesHip.Add(roofVertices[2]);

            trianglesHip.Add(3);
            trianglesHip.Add(5);
            trianglesHip.Add(4);
        }
        else if (sections == 2)
        {
            roofVerticesHip.Add(roofVertices[0]);
            roofVerticesHip.Add(roofVertices[0] + (centre - midOfAlteredStarts));
            roofVerticesHip.Add(roofVertices[2]);

            trianglesHip.Add(0);
            trianglesHip.Add(2);
            trianglesHip.Add(1);

            roofVerticesHip.Add(roofVertices[2]);
            roofVerticesHip.Add(roofVertices[0] + (centre - midOfAlteredStarts));
            roofVerticesHip.Add(roofVertices[2] + (centre - midOfAlteredStarts));

            trianglesHip.Add(3);
            trianglesHip.Add(5);
            trianglesHip.Add(4);

            roofVerticesHip.Add(roofVertices[6]);
            roofVerticesHip.Add(roofVertices[2]);
            roofVerticesHip.Add(roofVertices[2] + (centre - midOfAlteredStarts));

            trianglesHip.Add(6);
            trianglesHip.Add(8);
            trianglesHip.Add(7);

            roofVerticesHip.Add(roofVertices[0] + (centre - midOfAlteredStarts));
            roofVerticesHip.Add(roofVertices[0] + (centre - midOfAlteredStarts) + (centre - midOfAlteredStarts));
            roofVerticesHip.Add(roofVertices[2] + (centre - midOfAlteredStarts));

            trianglesHip.Add(9);
            trianglesHip.Add(11);
            trianglesHip.Add(10);

        }

        //gutter//underneath lip

        List<Vector3> gutterVertices = new List<Vector3>();
        List<int> gutterTriangles = new List<int>();

        gutterVertices.Add(start0);
        gutterVertices.Add(start1);
        gutterVertices.Add(vertices[longestEdge[0]]);
        gutterVertices.Add(vertices[longestEdge[1]]);

        gutterTriangles.Add(0);
        gutterTriangles.Add(2);
        gutterTriangles.Add(1);

        gutterTriangles.Add(1);
        gutterTriangles.Add(2);
        gutterTriangles.Add(3);

        //shortedge
        if (hip)
        {
           

            gutterVertices.Add(start0);
            gutterVertices.Add(vertices[longestEdge[0]]);
            gutterVertices.Add(start0 + ((centre - midOfAlteredStarts).normalized) * (shortDistance + frontOverhang * 2));
            gutterVertices.Add(vertices[longestEdge[0]] + ((centre - midOfAlteredStarts).normalized) * shortDistance);

            gutterTriangles.Add(4);
            gutterTriangles.Add(6);
            gutterTriangles.Add(5);

            gutterTriangles.Add(5);
            gutterTriangles.Add(6);
            gutterTriangles.Add(7);

        }
        else if(!hip)
        {
            //add little triangle at end, then rectangle across

            gutterVertices.Add(start0);
            gutterVertices.Add(vertices[longestEdge[0]] + ((vertices[longestEdge[1]] - vertices[longestEdge[1]]).normalized) * gableOverhang);
            //gutterVertices.Add(start0 + ((vertices[longestEdge[1]] - vertices[longestEdge[1]]).normalized) * gableOverhang);
            gutterVertices.Add(vertices[longestEdge[1]]);

            gutterTriangles.Add(0);
            gutterTriangles.Add(2);
            gutterTriangles.Add(1);

            //gutterTriangles.Add(1);
           // gutterTriangles.Add(2);
           // gutterTriangles.Add(3);


            gutterVertices.Add(start0);
            gutterVertices.Add(vertices[longestEdge[0]]);
            gutterVertices.Add(vertices[longestEdge[0]] + ((vertices[longestEdge[0]] - vertices[longestEdge[1]]).normalized)*gableOverhang);
            //gutterTriangles.Add(4);
            //gutterTriangles.Add(6);
            //gutterTriangles.Add(5);

        }

        
        

        Mesh mesh = new Mesh();
        mesh.vertices = roofVertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        GameObject roof1 = new GameObject();
        roof1.name = "Roof";
        roof1.transform.parent = gameObject.transform;
        roof1.transform.position =  Vector3.up * (storeyHeight+ (0.2f * (floors - 1))) *floors;
        MeshFilter mf = roof1.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        MeshRenderer mr = roof1.AddComponent<MeshRenderer>();
        mr.sharedMaterial = Resources.Load("Roof") as Material;
        //copy and spin with pivot at bottom
        GameObject roofDuplicate1 = Instantiate(roof1, roof1.transform.position, roof1.transform.rotation);

        
        //hip or gable end
        mesh = new Mesh();
        mesh.vertices = roofVerticesHip.ToArray();
        mesh.triangles = trianglesHip.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        
        

        
        GameObject roof2 = new GameObject();

        if(hip)
            roof2.name = "Gable";
        else
            roof2.name = "Roof";

        roof2.transform.parent = gameObject.transform;
        roof2.transform.position = Vector3.up * (storeyHeight+(0.2f*(floors-1))) * floors;
        mf = roof2.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        mr = roof2.AddComponent<MeshRenderer>();
        if(hip)
            mr.sharedMaterial = Resources.Load("Roof") as Material;
        else
            mr.sharedMaterial = Resources.Load("Pink") as Material;

        //hip duplicate
        GameObject roofDuplicate2 = Instantiate(roof2, roof2.transform.position, roof2.transform.rotation);

        //gutter
        mesh = new Mesh();
        mesh.vertices = gutterVertices.ToArray();
        mesh.triangles = gutterTriangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        GameObject gutter1 = new GameObject();
        gutter1.name = "Gutter1";
        gutter1.transform.position = Vector3.up * (storeyHeight + (0.2f * (floors - 1))) * floors;
        mf = gutter1.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        mr = gutter1.AddComponent<MeshRenderer>();
        mr.sharedMaterial = Resources.Load("Roof") as Material;
        GameObject gutter2 = Instantiate(gutter1, gutter1.transform.position, gutter1.transform.rotation);

        GameObject pivot = new GameObject();
        pivot.transform.position = centre;
        pivot.transform.parent = gameObject.transform;

        gutter2.transform.parent = pivot.transform;

       
        roofDuplicate1.transform.parent = pivot.transform;
        roofDuplicate2.transform.parent = pivot.transform;
      

        pivot.transform.rotation *= Quaternion.Euler(0, 180, 0);

        pivot.name = "RoofParent";
        roof1.transform.parent = pivot.transform;
        roof2.transform.parent = pivot.transform;
        gutter1.transform.parent = pivot.transform;



        ///FOR PROCJAM CAN REMOVE - stoppin z fighting
        pivot.transform.position += Vector3.up * 0.01f;

        return pivot;
    }

    public static Vector3 ClosestPointToCentreOfThisRoom(GameObject thisRoom, GameObject targetRoom, Vector3 sharedPoint)
    {
        //find closest point to the centre of the room we are trying to attach to(livingRoom)
        Vector3 total = Vector3.zero;
        Vector3[] thisVertices = targetRoom.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] targetVertices = targetRoom.GetComponent<MeshFilter>().mesh.vertices;

        for (int j = 0; j < thisVertices.Length; j++)
        {
            total += thisVertices[j];
        }
        Vector3 centreOfLivingArea = total / thisVertices.Length;

        float distance = Mathf.Infinity;
        int closest = 0;
        for (int k = 0; k < targetVertices.Length; k++)
        {
            //don't check the point we already have
            if (targetVertices[k] == sharedPoint)
                continue;

            float temp = Vector3.Distance(targetVertices[k], centreOfLivingArea);
            if (temp < distance)
            {
                distance = temp;
                closest = k;
            }
        }

        Vector3 closestPoint = thisVertices[closest];
        return closestPoint;
    }

    public static List<Vector3> SharedPointsWithTargetRoom(GameObject thisRoom, GameObject targetRoom)
    {
        List<Vector3> sharedPoints = new List<Vector3>();

        Vector3[] thisVertices = thisRoom.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] targetVertices = targetRoom.GetComponent<MeshFilter>().mesh.vertices;

        for (int j = 0; j < thisVertices.Length; j++)
        {
            for (int k = 0; k < targetVertices.Length; k++)
            {
                //if (thisRoom.transform.position + ( thisRoom.transform.rotation*thisVertices[j]) == targetRoom.transform.position+ //(targetRoom.transform.rotation *targetVertices[k]))
                if(Vector3.Distance(thisRoom.transform.position + (thisRoom.transform.rotation * thisVertices[j]), targetRoom.transform.position + (targetRoom.transform.rotation * targetVertices[k])) < 0.01f)
                {
                    sharedPoints.Add(targetVertices[k]);
                }
            }
        }
        return sharedPoints;
    }

    public static Vector3 ClosestPointOnMesh(Vector3 point,GameObject target)
    {
        Vector3 closestPoint = Vector3.one * Mathf.Infinity;

        Vector3[] vertices = target.GetComponent<MeshFilter>().mesh.vertices;

        float distance = Mathf.Infinity;
        for (int i = 0; i < vertices.Length; i++)
        {
            float temp = Vector3.Distance(point, vertices[i]);
            
            if(temp < distance)
            {
                distance = temp;
                closestPoint = vertices[i];
            }
        }
        return closestPoint;
    }

    public static Vector3 ClosestVerticeOnThisRoomToCentreOfTargetRoom(GameObject thisRoom, GameObject targetRoom, Vector3 sharedPoint)
    {
        //find closest point to the centre of the room we are trying to attach to
        Vector3 total = Vector3.zero;
        Vector3[] thisVertices = thisRoom.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] targetVertices = targetRoom.GetComponent<MeshFilter>().mesh.vertices;

        for (int j = 0; j < targetVertices.Length; j++)
        {
            total += targetVertices[j];
        }

        Vector3 centreOftarget= total / targetVertices.Length;

        //find closest point on this room to centre of target
        float distance = Mathf.Infinity;
        int closest = 0;
        for (int k = 0; k < thisVertices.Length; k++)
        {
            //don't check the point we already have
            if (thisVertices[k] == sharedPoint)
                continue;

            float temp = Vector3.Distance(thisVertices[k], centreOftarget);
            if (temp < distance)
            {
                distance = temp;
                closest = k;
            }
        }

        Vector3 closestPoint = thisVertices[closest];
        return closestPoint;
    }

    public static List<Vector3> IntersectionPoints(Vector3[] pointsOnEdge, float pathSize)
    {
           //a bit lazy - rooms must only have 4 points
        //list we will return
        List<Vector3> intersectionPoints = new List<Vector3>();
        //poluations intersectionPoints

        List<Vector3> points = new List<Vector3>();
        List<Vector3> directions = new List<Vector3>();
        //create vectors inside polygon
        for (int i = 0; i < pointsOnEdge.Length; i++)
        {

            Vector3 p0 = pointsOnEdge[i];
            //points on edge is [1] here to create a vector to intersect with from the last point to the second point
            //this gives us the final intersect point at the first vector
            Vector3 p1 = pointsOnEdge[1];

            if (i != pointsOnEdge.Length - 1)
                p1 = pointsOnEdge[i + 1];


            Vector3 dir = (p1 - p0).normalized;

            //always builds clockwise so we only need to rotate right(to the inside of the polygon)
            Vector3 normal = (Quaternion.Euler(0f, 90f, 0f) * dir);

            normal *= pathSize;

            //move points inside
            p0 += normal;
            p1 += normal;

            points.Add(p0);
            directions.Add(dir);

            //  GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //  cube.transform.position = p0;
        }

        //get intersection points

        intersectionPoints = new List<Vector3>();
        //check for intersections and add to list
        for (int i = 0; i < points.Count; i++)
        {
            //miss the last point
            if (i < points.Count - 1)
            {
                Vector3 closestP1;
                Vector3 closestP2;
                if (ClosestPointsOnTwoLines(out closestP1, out closestP2, points[i], directions[i], points[i + 1], directions[i + 1]))
                {

                    //we only need to use the second

                    if (closestP1 != Vector3.zero)
                        intersectionPoints.Add(closestP1);
                }

            }
            //last point has been duplicated by 1st on find edges so we do not need to reverse any directional vectors

        }

        return intersectionPoints;



    }

    public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {

        closestPointLine1 = Vector3.zero;
        closestPointLine2 = Vector3.zero;

        float a = Vector3.Dot(lineVec1, lineVec1);
        float b = Vector3.Dot(lineVec1, lineVec2);
        float e = Vector3.Dot(lineVec2, lineVec2);

        float d = a * e - b * b;

        //lines are not parallel
        if (d != 0.0f)
        {

            Vector3 r = linePoint1 - linePoint2;
            float c = Vector3.Dot(lineVec1, r);
            float f = Vector3.Dot(lineVec2, r);

            float s = (b * f - c * e) / d;
            float t = (a * f - c * b) / d;

            closestPointLine1 = linePoint1 + lineVec1 * s;
            closestPointLine2 = linePoint2 + lineVec2 * t;

            return true;
        }

        else
        {
            return false;
        }
    }
    
    public static int[] VerticeArray(string side)
    {
        int[] vertices = null;

        if (side == "front")
        {

            vertices = new int[]
            {
            0,1,2,3,
            8,9,
            13,14,
            16,17,
            22,23
            };
        }

        else if (side == "rear")
        {
            vertices = new int[]
            {
            4,5,6,7,
            10,11,
            12,15,
            18,19,20,21

            };
        }
        else if (side == "right")
        {
            vertices = new int[]
            {
            0,2,
            4,6,
            8,10,
            12,13,
            20,21,22,23
            };
        }
        else if (side == "left")
        {
            vertices = new int[]
            {
                1,3,
                5,7,
                9,11,
                14,15,
                16,17,18,19
            };
        }
        else if (side == "top")
        {
            vertices = new int[]
            {
                2,3,4,5,
                8,9,10,11,
                17,18,
                21,22
            };
        }
        else if (side == "bottom")
        {
            vertices = new int[]
            {
                0,1,
                6,7,
                12,13,14,15,
                16,19,
                20,23
            };
        }
        else if (side == "rearLeft")
        {
            vertices = new int[]
           {
               5,7,11,15,18,19
           };
        }
        else if (side == "rearRight")
        {
            vertices = new int[]
           {
                4,6,10,12,20,21
           };
        }
        else
            Debug.Log("Incorrect String For Vertice Array Type");

        return vertices;
    }

   

   
    public class Walls 
    {
        public static void TilesOnWall(GameObject room, GameObject bath,Divide divide)
        {
            List<GameObject> tiles = new List<GameObject>();
            //currently instantiating a million cubes - change to inserting vertices in to a mesh

            float tileSize = Random.Range(0.05f, 0.5f);
            float bathHeight = bath.transform.localScale.y - tileSize*2;
            float bathLength = bath.transform.localScale.z;
            float bathWidth = bath.transform.localScale.x;

            Vector3 sideDir = bath.transform.right;
            Vector3 centreOfRoom = room.GetComponent<MeshRenderer>().bounds.center;
            if (Vector3.Distance(bath.transform.position - sideDir, centreOfRoom) < Vector3.Distance(bath.transform.position + sideDir, centreOfRoom))
            {
                Debug.Log("swapped");
                sideDir = -sideDir;
            }


            Vector3 offset = bath.transform.position - (sideDir * bathWidth*.5f);
            offset -= tileSize*2*Vector3.up;
            offset -= bathLength * 0.5f * bath.transform.forward;//2 length
            //offset -= bathWidth * 0.5f * sideDir;//1 width

            offset += Vector3.up*bathHeight*0.5f;

            float storeyHeight = 3f*0.75f;//high limit on tiles building

            bool alternate = false;
            //length of bath
            for (int i = 0; i < bathLength/tileSize; i++)
            {
                alternate = !alternate;
                for (int j = 0; j < storeyHeight / tileSize; j++)
                {
                    Vector3 position = offset + (tileSize * i * bath.transform.forward);
                    position += (tileSize * j * Vector3.up);

                    GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(c.GetComponent<BoxCollider>());
                    c.transform.position = position;
                    c.transform.position += tileSize * 0.5f * bath.transform.forward;

                    c.transform.LookAt(c.transform.position + bath.transform.forward);
                    c.transform.localScale = new Vector3(0.01f, tileSize, tileSize);
                    //grout
                    Vector3 scale = new Vector3( c.transform.localScale.x, c.transform.localScale.y - 0.015f, c.transform.localScale.z - 0.015f);
                    c.transform.localScale = scale;
                    c.transform.parent = room.transform;
                    

                    if (alternate)
                    {
                        c.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;
                        alternate = false;
                    }
                    else
                        alternate = true;

                    tiles.Add(c);
                }

           

            }

            
            offset = bath.transform.position - sideDir*bathWidth*0.5f;
            offset -= tileSize * 2 * Vector3.up;
            offset -= bathLength * 0.5f * bath.transform.forward;//2 length
           // offset -= 1f * 0.5f * sideDir;//1 width
            offset += Vector3.up * bathHeight *0.5f;
            //end of bath
            for (int i = 0; i < bathWidth / tileSize; i++)
            {
                alternate = !alternate;
                for (int j = 0; j < (storeyHeight) / tileSize; j++)
                {
                    Vector3 position = offset + (tileSize * i * sideDir);
                    position += (tileSize * j * Vector3.up);

                    GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(c.GetComponent<BoxCollider>());
                    c.transform.position = position;
                    c.transform.position += tileSize*0.5f * sideDir;
                    c.transform.LookAt(c.transform.position + sideDir);
                    c.transform.localScale = new Vector3(0.01f, tileSize, tileSize);
                    //grout
                    Vector3 scale = new Vector3(c.transform.localScale.x, c.transform.localScale.y - 0.015f, c.transform.localScale.z - 0.015f);
                    c.transform.localScale = scale;
                    c.transform.parent = room.transform; 

                    if (alternate)
                    {
                        c.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;
                        alternate = false;
                    }
                    else
                        alternate = true;

                    //add to list to send to interior assets list
                    tiles.Add(c);

                }
            }

            //this must be in need of optimising
            divide.interiorAssetsByRoom.Add(tiles);
        }
    }

   

  
}

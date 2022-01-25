using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomLayouts : MonoBehaviour
{
    public static List<GameObject> KitchenLayout(GameObject room)
    {
        List<GameObject> objectsToBuild = new List<GameObject>();
        //kitchen
        //We are aiming to find a suitable corner to start building worktops from. This is the corner furthest away from all the doors in the room
        //get doors in room
        List<GameObject> doorsInKitchen = new List<GameObject>();
        for (int i = 0; i < room.transform.childCount; i++)
        {
            if (room.transform.GetChild(i).name == "Door")
                doorsInKitchen.Add(room.transform.GetChild(i).gameObject);
        }
        //get vertices of room
        Vector3[] vertices = room.gameObject.GetComponent<MeshFilter>().mesh.vertices;
        //work out furthest corner
        List<int> toSkip = new List<int>();
        int furthest = RoomPlanner.FurthestVerticesFromDoors(doorsInKitchen, vertices, toSkip);

        //build worktops out from this corner until it hits a door
        //start building in a clockwise direction - this is the way the vertices are ordered
        int nextVertice = furthest + 1;
        //make sure index is in range - just looping around the room
        if (nextVertice > vertices.Length - 1)
        {
            Debug.Log("changed vertices, check if correct - kitchen");
            nextVertice = 0;
        }
        Vector3 directionToNext = (vertices[nextVertice] - vertices[furthest]).normalized;
        float distanceToNext = Vector3.Distance(vertices[nextVertice], vertices[furthest]);

        //bool reversedDirection = false;
        //randomly choose direction

        int prevVertice = furthest - 1;
        //make sure index is in range - just looping around the room
        if (prevVertice < 0)
            prevVertice = vertices.Length - 1;
        Vector3 directionToPrev = (vertices[prevVertice] - vertices[furthest]).normalized;
        float distanceToPrev = Vector3.Distance(vertices[prevVertice], vertices[furthest]);

        //reversedDirection = true;

        //place every 1 unit(metre) until we hit the end wall - create lists for each direction from the corner
        List<Transform> transformsDir1 = new List<Transform>();
        List<Transform> transformsDir2 = new List<Transform>();
        List<Vector3> positionsUsed = new List<Vector3>();

        bool skip = false;
        Vector3 cornerSection = Vector3.zero;

        //clamp the length of the cupboards, othwerwise it can grow all the way along a mega wall
        int maxLimit = 8;
        int limit = Random.Range(3, maxLimit);

        for (int i = 0; i < distanceToNext - 1; i++)
        {
            Vector3 position = vertices[furthest] + (directionToNext * i);
            Vector3 toCentre = room.GetComponent<MeshRenderer>().bounds.center - position;

            //https://forum.unity3d.com/threads/left-right-test-function.31420/  -- left right test
            Vector3 perp = Vector3.Cross(directionToNext, toCentre);
            float d = Vector3.Dot(perp, Vector3.up);
            Quaternion spinDirection = Quaternion.Euler(0, 90, 0);
            if (d < 0.0)
                spinDirection = Quaternion.Euler(0, -90, 0);


            if (i > limit)
            {
                if (i > limit + 1)
                {
                    //place options for exterior door - we will use these in a later function when building walls and windows
                    GameObject doorOption = new GameObject();
                    doorOption.transform.position = position;
                    doorOption.transform.rotation = Quaternion.LookRotation(directionToNext) * spinDirection;
                    //  doorOption.transform.position += doorOption.transform.transform.forward * 0.5f;
                    //doorOption.transform.position += directionToNext * 0.5f;
                    doorOption.transform.parent = room.transform;
                    doorOption.name = "DoorOption";
                }
                continue;
            }

            //skip corner space, we cant place an object here - save for worktop
            if (i == 0)
            {
                cornerSection = vertices[furthest] + (directionToNext * i);
                continue;
            }



            //check for door
            for (int j = 0; j < doorsInKitchen.Count; j++)
            {
                if (Vector3.Distance(position, doorsInKitchen[j].transform.position) < 1.5f)
                    skip = true;
            }
            if (skip)
                continue;



            //using Gameobject so we can eaily rotate it using transform class (lazy)
            GameObject c = new GameObject();
            c.transform.position = position;
            c.transform.rotation = Quaternion.LookRotation(directionToNext) * spinDirection;
            c.transform.position += c.transform.transform.forward * 0.6f;
            c.transform.position += directionToNext * 0.6f;//.6 for +0.1f of wall thickness
            c.transform.position += Vector3.up * 0.5f;
            c.transform.parent = room.transform;
            transformsDir1.Add(c.transform);
            Destroy(c);
        }


        limit = Random.Range(3, maxLimit);
        skip = false;
        for (int i = 0; i < distanceToPrev - 1; i++)
        {
            if (i == 0)
                continue;

            Vector3 position = vertices[furthest] + (directionToPrev * i);

            Vector3 toCentre = room.GetComponent<MeshRenderer>().bounds.center - position;

            //https://forum.unity3d.com/threads/left-right-test-function.31420/  -- left right test
            Vector3 perp = Vector3.Cross(directionToPrev, toCentre);
            float d = Vector3.Dot(perp, Vector3.up);
            Quaternion spinDirection = Quaternion.Euler(0, 90, 0);
            if (d < 0.0)
                spinDirection = Quaternion.Euler(0, -90, 0);

            if (i > limit)
            {
                if (i > limit + 1)
                {
                    //place options for exterior door - we will use these in a later function when building walls and windows
                    GameObject doorOption = new GameObject();
                    doorOption.transform.position = position;
                    doorOption.transform.rotation = Quaternion.LookRotation(directionToPrev) * spinDirection;
                    //  doorOption.transform.position += doorOption.transform.transform.forward * 0.5f;
                    // doorOption.transform.position += directionToPrev * 0.5f;
                    doorOption.transform.parent = room.transform;
                    doorOption.name = "DoorOption";
                }
                continue;
            }



            //check for door
            for (int j = 0; j < doorsInKitchen.Count; j++)
            {
                if (Vector3.Distance(position, doorsInKitchen[j].transform.position) < 1.5f)
                    skip = true;
            }
            if (skip)
                continue;


            //using Gameobject so we can eaily rotate it using transform class (lazy)
            GameObject c = new GameObject();
            c.transform.position = position;
            c.transform.rotation = Quaternion.LookRotation(directionToPrev) * spinDirection;
            c.transform.position += c.transform.transform.forward * 0.6f;
            c.transform.position += directionToPrev * 0.6f;//+.1 for wall thickness
            c.transform.position += Vector3.up * 0.5f;
            c.transform.parent = room.transform;
            transformsDir2.Add(c.transform);
            Destroy(c);

        }

        List<Transform> transforms = new List<Transform>();
        //order positions nicely
        transformsDir1.Reverse();
        foreach (Transform t in transformsDir1)
            transforms.Add(t);
        foreach (Transform t in transformsDir2)
            transforms.Add(t);

        //we have a list of positions against the wall, place some objects
        /*
        for (int i = 0; i < transforms.Count; i++)
        {
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = transforms[i].position;
            c.transform.rotation = transforms[i].rotation;
            c.transform.localScale *= 0.5f;
            c.transform.parent = room.transform;
        }
        */
        //we have organised the potential spaces in to a sequential order running round the wall, from the start of an L shape, to the end
        //optimal kitchen

        if (transforms.Count > 3)
        {
            //we can place big sink (2spaces), cooker (1space)and fridge(1space)

            //start at a random place
            int random = Random.Range(0, transforms.Count);
            //  Debug.Log("random" + random);
            //adding some randomisation on which way it looks to build
            int dir = 1;
            if (Random.Range(0, 2) == 1)
                dir = -1;
            //find two spaces next to each other - with same rotation and next to each other in the list            
            for (int i = 0; i < transforms.Count; i++)
            {
                //check if next in list has same rotation
                //index protection whether we are addin or subtracting to direction
                int firstIndex = random;
                if (firstIndex > transforms.Count - 1)
                    firstIndex = 0;
                if (firstIndex < 0)
                    firstIndex = transforms.Count - 1;
                Transform first = transforms[firstIndex];

                int nextIndex = random + 1;
                if (nextIndex > transforms.Count - 1)
                    nextIndex -= transforms.Count;
                if (nextIndex < 0)
                    nextIndex = transforms.Count - 1;
                Transform second = transforms[nextIndex];

                if (first.rotation == second.rotation)
                {
                    //Debug.Log("found");
                    //stop the search
                    GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = first.transform.position;
                    c.name = "Sink";
                    c.transform.parent = room.transform;
                    c.transform.rotation = first.transform.rotation;
                    //add to list to return
                    objectsToBuild.Add(c);

                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = second.transform.position;
                    c.name = "Sink";
                    c.transform.parent = room.transform;
                    c.transform.rotation = first.transform.rotation;
                    //add to list to return
                    objectsToBuild.Add(c);
                    //remove these positions
                    transforms.Remove(first);
                    transforms.Remove(second);
                    

                    break;
                }

                random += dir;
            }

            //we have placed the sink, now the cooker and fridge

            random = Random.Range(0, transforms.Count);
            GameObject c2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c2.transform.position = transforms[random].position;
            c2.transform.rotation = transforms[random].rotation;
            c2.transform.localScale = new Vector3(1, 2, 1);
            c2.transform.parent = room.transform;
            transforms.RemoveAt(random);

            random = Random.Range(0, transforms.Count);
            GameObject c3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c3.transform.position = transforms[random].position;
            c3.transform.rotation = transforms[random].rotation;
            c3.transform.localScale = new Vector3(1, 2, 1);
            c3.transform.parent = room.transform;
            transforms.RemoveAt(random);

            //make the cooker nearest to the corner, otherwise it is possible to lock the corner from fridge and sink -no access
            if (Vector3.Distance(c2.transform.position, cornerSection) < Vector3.Distance(c3.transform.position, cornerSection))
            {
                c2.name = "Cooker";
                c2.transform.localScale = new Vector3(1, 1, 1);
                //add to list to return
                objectsToBuild.Add(c2);
                GameObject cookerTop = Instantiate(c2);

                cookerTop.transform.position += Vector3.up * 1.5f;
                cookerTop.transform.localScale = new Vector3(1, 0.2f, 1);
                cookerTop.transform.parent = room.transform;
                //add to list to return
                objectsToBuild.Add(cookerTop);

                c3.name = "Fridge";
                //add to list to return
                objectsToBuild.Add(c3);
            }
            else
            {
                c2.name = "Fridge";
                //add to list to return
                objectsToBuild.Add(c2);

                c3.name = "Cooker";
                c3.transform.localScale = new Vector3(1, 1, 1);
                //add to list to return
                objectsToBuild.Add(c3);

                GameObject cookerTop = Instantiate(c3);
                cookerTop.transform.parent = room.transform;
                cookerTop.transform.position += Vector3.up * 1.5f;
                cookerTop.transform.localScale = new Vector3(1, 0.2f, 1);
                //add to list to return
                objectsToBuild.Add(cookerTop);

            }

            //build a washing machine if any space left -this is fun huh?
            if (transforms.Count != 0)
            {
                random = Random.Range(0, transforms.Count);
                GameObject c4 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                c4.transform.parent = room.transform;
                c4.name = "WashingMachine";
                c4.transform.position = transforms[random].position;
                c4.transform.rotation = transforms[random].rotation;
                c4.transform.localScale = new Vector3(1, 1, 1);
                //add to list to return
                objectsToBuild.Add(c4);
                transforms.RemoveAt(random);

                GameObject topCupboard = Instantiate(c4);
                topCupboard.transform.parent = room.transform;
                topCupboard.name = "TopCupboard";
                topCupboard.transform.position += Vector3.up * 2;
                topCupboard.transform.position -= topCupboard.transform.forward * 0.25f;//estimates atm
                topCupboard.transform.localScale = new Vector3(1, 1, 0.66f);
                //add to list to return
                objectsToBuild.Add(topCupboard);
            }

            //finally fill any spaces left with a worktop/cupboard

            GameObject corner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            corner.transform.parent = room.transform;
            corner.name = "Corner";
            corner.transform.position = cornerSection + directionToNext * 0.6f + directionToPrev * 0.6f + Vector3.up * 0.5f;//.1f for wall


            corner.transform.localScale = new Vector3(1, 1f, 1);
            //add to list to return
            objectsToBuild.Add(corner);

            for (int i = 0; i < transforms.Count; i++)
            {
                GameObject cupboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cupboard.transform.parent = room.transform;
                cupboard.name = "Cupboard";
                cupboard.transform.position = transforms[i].position;
                cupboard.transform.rotation = transforms[i].rotation;
                //add to list to return
                objectsToBuild.Add(cupboard);

                GameObject topCupboard = Instantiate(cupboard);
                topCupboard.transform.parent = room.transform;
                topCupboard.name = "TopCupboard";
                topCupboard.transform.position += Vector3.up * 2;
                topCupboard.transform.position -= topCupboard.transform.forward * 0.25f;
                topCupboard.transform.localScale = new Vector3(1, 1, 0.66f);
                //add to list to return
                objectsToBuild.Add(topCupboard);

            }
        }

        else if (transforms.Count == 3)
        {
            int random = Random.Range(0, transforms.Count);
            //sink
            GameObject c1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c1.transform.parent = room.transform;
            c1.name = "Sink";
            c1.transform.position = transforms[random].position;
            c1.transform.rotation = transforms[random].rotation;
            c1.transform.localScale = new Vector3(1, 1, 1);
            //add to list to return
            objectsToBuild.Add(c1);
            transforms.RemoveAt(random);

            //cooker/fridge - making small fridge so we can have an extra worktop on top
            random = Random.Range(0, transforms.Count);
            GameObject c2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c2.transform.parent = room.transform;
            c2.transform.position = transforms[random].position;
            c2.transform.rotation = transforms[random].rotation;
            c2.transform.localScale = new Vector3(1, 1, 1);
            //add to list to return
            objectsToBuild.Add(c2);
            transforms.RemoveAt(random);

            random = Random.Range(0, transforms.Count);
            GameObject c3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c3.transform.parent = room.transform;
            c3.transform.position = transforms[random].position;
            c3.transform.rotation = transforms[random].rotation;
            c3.transform.localScale = new Vector3(1, 1, 1);
            //add to list to return
            objectsToBuild.Add(c3);
            transforms.RemoveAt(random);

            //make the cooker nearest to the corner, otherwise it is possible to lock the corner from fridge and sink -no access
            if (Vector3.Distance(c2.transform.position, cornerSection) < Vector3.Distance(c3.transform.position, cornerSection))
            {
                c2.name = "Cooker";
                c2.transform.localScale = new Vector3(1, 1, 1);
                //add to list to return
                objectsToBuild.Add(c2);

                GameObject cookerTop = Instantiate(c2);
                cookerTop.transform.parent = room.transform;
                cookerTop.transform.position += Vector3.up * 1.5f;
                cookerTop.transform.localScale = new Vector3(1, 0.2f, 1);
                //add to list to return
                objectsToBuild.Add(cookerTop);

                c3.name = "Fridge";
                //add to list to return
                objectsToBuild.Add(c3);

                GameObject topCupboard = Instantiate(c3);
                topCupboard.transform.parent = room.transform;
                topCupboard.name = "TopCupboard";
                topCupboard.transform.position += Vector3.up * 2;
                topCupboard.transform.position -= topCupboard.transform.forward * 0.25f;
                topCupboard.transform.localScale = new Vector3(1, 1, 0.66f);
                //add to list to return
                objectsToBuild.Add(topCupboard);
            }
            else
            {
                c2.name = "Fridge";
                //add to list to return
                objectsToBuild.Add(c2);

                GameObject topCupboard = Instantiate(c2);
                topCupboard.transform.parent = room.transform;
                topCupboard.name = "TopCupboard";
                topCupboard.transform.position += Vector3.up * 2;
                topCupboard.transform.position -= topCupboard.transform.forward * 0.25f;
                topCupboard.transform.localScale = new Vector3(1, 1, 0.66f);
                //add to list to return
                objectsToBuild.Add(topCupboard);

                c3.name = "Cooker";
                c3.transform.localScale = new Vector3(1, 1, 1);
                GameObject cookerTop = Instantiate(c3);
                cookerTop.transform.parent = room.transform;
                cookerTop.transform.position += Vector3.up * 1.5f;
                cookerTop.transform.localScale = new Vector3(1, 0.2f, 1);
                //add to list to return
                objectsToBuild.Add(cookerTop);
            }

            GameObject corner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            corner.transform.parent = room.transform;
            corner.name = "Corner";
            corner.transform.position = cornerSection + directionToNext * 0.5f + directionToPrev * 0.5f + Vector3.up * 0.5f;
            corner.transform.localScale = new Vector3(1, 1f, 1);
            //add to list to return
            objectsToBuild.Add(corner);
        }

        else if (transforms.Count == 2)
        {
            //tiny wee house, they are gettin a hob, they can ahve a microwave/ oven on top of the fridge / or in the corner?- that'll show 'em
            int random = Random.Range(0, transforms.Count);
            //sink
            GameObject c1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c1.transform.parent = room.transform;
            c1.name = "Sink";
            c1.transform.position = transforms[random].position;
            c1.transform.rotation = transforms[random].rotation;
            c1.transform.localScale = new Vector3(1, 1, 1);
            //add to list to return
            objectsToBuild.Add(c1);
            transforms.RemoveAt(random);

            //cooker/fridge - making small fridge so we can have an extra worktop on top
            random = Random.Range(0, transforms.Count);
            GameObject c2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c2.transform.parent = room.transform;
            c2.name = "Fridge";
            c2.transform.position = transforms[random].position;
            c2.transform.rotation = transforms[random].rotation;
            c2.transform.localScale = new Vector3(1, 1, 1);
            //add to list to return
            objectsToBuild.Add(c2);
            transforms.RemoveAt(random);

            GameObject topCupboard = Instantiate(c2);
            topCupboard.transform.parent = room.transform;
            topCupboard.name = "Microwave";
            topCupboard.transform.position = cornerSection + directionToNext * 0.5f + directionToPrev * 0.5f + Vector3.up;            
            topCupboard.transform.localScale = new Vector3(0.66f, 0.66f, 0.66f);
            //add to list to return
            objectsToBuild.Add(topCupboard);

            GameObject corner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            corner.transform.parent = room.transform;
            corner.name = "Corner";
            corner.transform.position = cornerSection + directionToNext * 0.5f + directionToPrev * 0.5f + Vector3.up;
            corner.transform.localScale = new Vector3(1, 1, 1);
            //add to list to return
            objectsToBuild.Add(corner);

        }

        else if (transforms.Count == 1)
        {
            //eh.. a sink and a fridge in the corner? - two units space
            GameObject c1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c1.transform.parent = room.transform;
            c1.name = "Sink";
            c1.transform.position = transforms[0].position;
            c1.transform.rotation = transforms[0].rotation;
            c1.transform.localScale = new Vector3(1, 1, 1);
            //add to list to return
            objectsToBuild.Add(c1);


            //cooker/fridge - making small fridge so we can have an extra worktop on top            
            GameObject c2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c2.transform.parent = room.transform;
            c2.name = "Fridge";
            c2.transform.position = cornerSection + directionToNext * 0.5f + directionToPrev * 0.5f + Vector3.up * 0.5f;
            c2.transform.rotation = transforms[0].rotation;
            c2.transform.localScale = new Vector3(1, 1, 1);
            //add to list to return
            objectsToBuild.Add(c2);


            GameObject topCupboard = Instantiate(c2);
            topCupboard.transform.parent = room.transform;
            topCupboard.name = "Microwave";
            topCupboard.transform.position = cornerSection + directionToNext * 0.5f + directionToPrev * 0.5f + Vector3.up;            
            topCupboard.transform.localScale = new Vector3(0.66f, 0.66f, 0.66f);
            objectsToBuild.Add(topCupboard);
        }

        return objectsToBuild;
    }

    public static List<GameObject> LivingRoomLayout(GameObject room,Divide divide)
    {
       
        List<GameObject> doorsInLivingRoom = new List<GameObject>();
        for (int i = 0; i < room.transform.childCount; i++)
        {
            if (room.transform.GetChild(i).name == "Door")
                doorsInLivingRoom.Add(room.transform.GetChild(i).gameObject);
        }
        //get vertices of room
        Vector3[] vertices = room.gameObject.GetComponent<MeshFilter>().mesh.vertices;
        //work out furthest corner that isn't too near a door
        List<int> verticesToSkip = new List<int>();
        int furthest = 0;

        //check if furthest corner is actually overlapping a door
        List<GameObject> doors = new List<GameObject>();
        for (int i = 0; i < room.transform.childCount; i++)
        {
            if (room.transform.GetChild(i).name == "Door")
                doors.Add(room.transform.GetChild(i).gameObject);
        }
        for (int v = 0; v < vertices.Length; v++)
        {

            //if vertices to skip list is empty (and not first try), it means we are far enough away from the doors
            //else, send it to function to find furthest away corner, NOT including the one we just found
            furthest = RoomPlanner.FurthestVerticesFromDoors(doorsInLivingRoom, vertices, verticesToSkip);
            bool tooClose = false;
            for (int i = 0; i < doors.Count; i++)
            {
                if (Vector3.Distance(doors[i].transform.position, vertices[furthest]) < 1)
                {
                    //too close, find next nearest
                    verticesToSkip.Add(furthest);
                    tooClose = true;
                }
            }

            //if list wan't populated, it means we were not near a door, this is what we wanted to check. jump out this god forsaken loop
            if (tooClose == false)
                break;
            else
            {
                Debug.Log("Corner too close to door");
                //GameObject s =  GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //s.transform.position = vertices[furthest];
                //s.transform.parent = room.transform.parent;
            }

            if (vertices.Length - 1 == v)
                Debug.Log("Could not find any corner far enough away from a door");
        }



        //build worktops out from this corner until it hits a door
        //start building in a clockwise direction - this is the way the vertices are ordered
        int nextVertice = furthest + 1;
        //make sure index is in range - just looping around the room
        if (nextVertice > vertices.Length - 1)
        {
            Debug.Log("changed vertice, check if correct - living room");
            nextVertice = 0;
        }

        Vector3 directionToNext = (vertices[nextVertice] - vertices[furthest]).normalized;
        float distanceToNext = Vector3.Distance(vertices[nextVertice], vertices[furthest]);

        int prevVertice = furthest - 1;
        //make sure index is in range - just looping around the room
        if (prevVertice < 0)
            prevVertice = vertices.Length - 1;
        Vector3 directionToPrev = (vertices[prevVertice] - vertices[furthest]).normalized;
        float distanceToPrev = Vector3.Distance(vertices[prevVertice], vertices[furthest]);

        //reversedDirection = true;

        //place every 1 unit(metre) until we hit the end wall - create lists for each direction from the corner
        List<Transform> transformsDir1 = new List<Transform>();
        List<Transform> transformsDir2 = new List<Transform>();
        List<Vector3> positionsUsed = new List<Vector3>();


        Vector3 cornerSection = Vector3.zero;

        //governs max distance from corner for couch
        int limit = Random.Range(3, 6);
        //limit = 6;
        for (int i = 0; i < distanceToNext - 1; i++)
        {
            //skip corner space, we cant place an object here - save for worktop
            if (i == 0)
            {
                cornerSection = vertices[furthest] + (directionToNext * i);
                continue;
            }

            Vector3 position = vertices[furthest] + (directionToNext * i);



            Vector3 toCentre = room.GetComponent<MeshRenderer>().bounds.center - position;

            //https://forum.unity3d.com/threads/left-right-test-function.31420/  -- left right test
            Vector3 perp = Vector3.Cross(directionToNext, toCentre);
            float d = Vector3.Dot(perp, Vector3.up);
            Quaternion spinDirection = Quaternion.Euler(0, 90, 0);
            if (d < 0.0)
                spinDirection = Quaternion.Euler(0, -90, 0);

            //using Gameobject so we can eaily rotate it using transform class (lazy)
            GameObject c = new GameObject();
            c.transform.position = position;
            c.transform.rotation = Quaternion.LookRotation(directionToNext) * spinDirection;
            c.transform.position += c.transform.transform.forward * 0.5f;
            c.transform.position += directionToNext * 0.5f;
            c.transform.position += Vector3.up * 0.5f;

            if (transformsDir1.Count < limit)
            {
                bool closeToDoor = false;
                foreach (GameObject door in doors)
                {
                    if (Vector3.Distance(door.transform.position, c.transform.position) < 1)
                        closeToDoor = true;

                }

                if (!closeToDoor)
                    transformsDir1.Add(c.transform);


            }
            else if (i < distanceToNext - 2)
            {
                //place optiopns for an exterior door - picked up later
                GameObject doorOption = new GameObject();
                doorOption.transform.position = position;
                doorOption.transform.rotation = Quaternion.LookRotation(directionToNext) * spinDirection;
                //doorOption.transform.position += c.transform.transform.forward * 0.5f;
                //doorOption.transform.position += directionToNext * 0.5f;
                //doorOption.transform.position += Vector3.up * 0.5f;
                doorOption.name = "DoorOption";
                doorOption.transform.parent = room.transform;
            }


            Destroy(c);
        }



        for (int i = 0; i < distanceToPrev - 1; i++)
        {
            if (i == 0)
                continue;
            Vector3 position = vertices[furthest] + (directionToPrev * i);


            Vector3 toCentre = room.GetComponent<MeshRenderer>().bounds.center - position;

            //https://forum.unity3d.com/threads/left-right-test-function.31420/  -- left right test
            Vector3 perp = Vector3.Cross(directionToPrev, toCentre);
            float d = Vector3.Dot(perp, Vector3.up);
            Quaternion spinDirection = Quaternion.Euler(0, 90, 0);
            if (d < 0.0)
                spinDirection = Quaternion.Euler(0, -90, 0);

            //using Gameobject so we can eaily rotate it using transform class (lazy)
            GameObject c = new GameObject();
            c.transform.position = position;
            c.transform.rotation = Quaternion.LookRotation(directionToPrev) * spinDirection;
            c.transform.position += c.transform.transform.forward * 0.5f;
            c.transform.position += directionToPrev * 0.5f;
            c.transform.position += Vector3.up * 0.5f;

            if (transformsDir2.Count < limit)
            {
                bool closeToDoor = false;
                foreach (GameObject door in doors)
                {
                    if (Vector3.Distance(door.transform.position, c.transform.position) < 2f)
                        closeToDoor = true;

                }

                if (!closeToDoor)
                    transformsDir2.Add(c.transform);


            }
            else if (i < distanceToPrev - 2)
            {
                //place optiopns for an exterior door - picked up later
                GameObject doorOption = new GameObject();
                doorOption.transform.position = position;
                doorOption.transform.rotation = Quaternion.LookRotation(directionToPrev) * spinDirection;
                //c.transform.position += c.transform.transform.forward * 0.5f;
                //c.transform.position += directionToPrev * 0.5f;
                //c.transform.position += Vector3.up * 0.5f;
                doorOption.name = "DoorOption";
                doorOption.transform.parent = room.transform;
            }


            Destroy(c);


        }

        List<Transform> transforms = new List<Transform>();
        //order positions nicely
        transformsDir1.Reverse();
        foreach (Transform t in transformsDir1)
            transforms.Add(t);
        foreach (Transform t in transformsDir2)
            transforms.Add(t);

        //we have a list of positions against the wall, place some objects
        for (int i = 0; i < transforms.Count; i++)
        {
            //continue;
            /*
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = transforms[i].position;
            c.transform.rotation = transforms[i].rotation;
            c.transform.parent = room.transform.parent;
            c.transform.localScale *= 0.5f;
            */
        }

        //so the aim of the game here is to find a layout where we can put in a 2 seater couch
        int twoSeatersPlaced = 0;
        int singleSeatersPlaced = 0;

        //keep a track of what we placed
        List<GameObject> placedObjects = new List<GameObject>();

        GameObject corner = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corner.transform.parent = room.transform;
        corner.name = "TV";

        corner.transform.position = vertices[furthest];
        
        corner.transform.LookAt(Vector3.Lerp(vertices[furthest]+ directionToNext,vertices[furthest]+ directionToPrev,.5f));

        corner.transform.position += directionToNext * 0.6f + directionToPrev * 0.6f;// + Vector3.up;//.6 for wall thickness(+.1)
        corner.transform.localScale = new Vector3(1, 1, 1);
        //rotate to face half way between directions
       

        placedObjects.Add(corner);

        Vector3 directionOfCouch = Vector3.zero;

        Vector3 randomScale = new Vector3(Random.Range(1f, 2f), Random.Range(1f, 1f), Random.Range(0.75f,1.25f));
        Vector3 randomScale2 = new Vector3(Random.Range(1f, 2f), Random.Range(1f, 1f), randomScale.z);

        if (transformsDir1.Count > 1)
        {
            GameObject c2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c2.transform.localScale = randomScale;
            c2.transform.parent = room.transform;
            c2.name = "Couch1";
            c2.transform.position = transformsDir1[0].position;
            c2.transform.rotation = Quaternion.LookRotation(-directionToNext);
            directionOfCouch = directionToNext;
            c2.transform.position += directionToPrev * c2.transform.localScale.x * 0.5f;
            bool tooClose = false;
            //find out if we can make this couch a 2 seater
            for (int i = 0; i < doors.Count; i++)
            {
                if (Vector3.Distance(doors[i].transform.position, c2.transform.position) < 2f)
                {
                    //tooClose = true;
                }
            }

            if (transformsDir2.Count < 3)
                tooClose = true;

            if (!tooClose)
            {
                //c2.transform.localScale = randomScale;
                //kinda cheating using this? maybe if i end up iusing 3 sides of the room, might need to work this out using left/right code avaible in Divide/here
                //ranoo? move out from wall

                twoSeatersPlaced++;
            }
            else
                singleSeatersPlaced++;

            placedObjects.Add(c2);
        }
        if (transformsDir2.Count > 1)
        {
            GameObject c2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c2.transform.parent = room.transform;
            c2.name = "Couch";
            c2.transform.position = transformsDir2[transformsDir2.Count - 1].position;
            c2.transform.rotation = Quaternion.LookRotation(-directionToPrev);
            directionOfCouch = directionToPrev;
            bool tooClose = false;
            //find out if we can make this couch a 2 seater
            for (int i = 0; i < doors.Count; i++)
            {
                if (Vector3.Distance(doors[i].transform.position, c2.transform.position) < 1.5f)
                {
                    //tooClose = true;
                }
            }
           // if (transformsDir1.Count < 3)
           //     tooClose = true;

            if (!tooClose)
            {
                c2.transform.localScale = randomScale2;
                c2.transform.position += directionToNext * c2.transform.localScale.x*.5f;

                twoSeatersPlaced++;
            }
            else
                singleSeatersPlaced++;

            placedObjects.Add(c2);

            //if we ahve placed 3 items = list.count = 3, then we have palced all our items, but let's move the couches out from the wall a bit, it makes it look more natural i think

            if (singleSeatersPlaced == 1 && twoSeatersPlaced == 1 || singleSeatersPlaced == 0 && twoSeatersPlaced == 2)
            {
                Vector3 directionToMiddle = directionToNext;
                if (placedObjects[1].transform.forward != directionToPrev)
                    directionToMiddle = directionToPrev;

                placedObjects[1].transform.position += directionToMiddle * 0.5f;

                //switch for other dir
                directionToMiddle = directionToPrev;
                if (placedObjects[2].transform.forward != directionToNext)
                    directionToMiddle = directionToNext;

                placedObjects[2].transform.position += directionToMiddle * 0.5f;
            }
        }

        if (twoSeatersPlaced == 0 && placedObjects.Count < 2)
        {
            // Debug.Log("Only Onesies");
            //we have found a crap layout. Change this layout to a "straight" pattern. The living has essentially tried to build a "skewed" pattern here, where the first object is in a corner - let's now
            //try and find a layour where the first object (tv/fireplace) is placed on a wall. which wall? One without a door on it preferably
            //How do we find that? We already have one vertice [furthest] so, let's get the direction vectors from furthest to each door, and check them against the directional vectors to the next/previous vertices (already worked out driToNext and DirToPrev)
            //if we get a match, then we have NOT found our boy/vertice,

            //destroy the object we placed earlier, we have to make new plan - wait and check if the other pattern will be any better
            for (int i = 0; i < placedObjects.Count; i++)
            {
                Destroy(placedObjects[i]);
            }
            placedObjects = new List<GameObject>();




            //run through doors
            int wallPoint2 = 0;
            Vector3 dir = Vector3.zero;
            List<Vector3> directionsToDoors = new List<Vector3>();
            for (int i = 0; i < doors.Count; i++)
            {
                Vector3 toDoor = (doors[i].transform.position - vertices[furthest]).normalized;
                directionsToDoors.Add(toDoor);
            }

            int tries = 0;
            //this finds if a door is on the wall connected to the corner which is furthest from all doors
            for (int i = 0; i < directionsToDoors.Count; i++)
            {
                if (directionsToDoors[i] == directionToNext)
                {
                    dir = directionToPrev;
                    tries++;
                }

                if (directionsToDoors[i] == directionToPrev)
                {
                    dir = directionToNext;
                    tries++;
                }
            }

           // Debug.Log("Tries " + tries);

            if (tries == 0)
            {
                //no door on any wall connected to furthest vertice from doors - a 1 or two door room
                //let's try random
                if (Random.Range(0, 2) == 0)
                    dir = directionToNext;
                else
                    dir = directionToPrev;
            }
            if (tries == 2)
            {
                //Debug.Break();
                //this means there is a door on each wall attached to the corner furthest away from the avergae of all doors
                //we need to find the wall with no doors on it - atm, living room will always have 1 wall free

                //run through all vertices and try same check again
                dir = Vector3.zero;
                bool skip = false;
                for (int j = 0; j < vertices.Length; j++)
                {
                    if (skip)
                        continue;
                    //same piece of code as above, seperate function?

                    directionsToDoors = new List<Vector3>();
                    for (int i = 0; i < doors.Count; i++)
                    {
                        Vector3 toDoor = (doors[i].transform.position - vertices[j]).normalized;
                        directionsToDoors.Add(toDoor);
                    }

                    int nextIndex = j + 1;
                    int prevIndex = j - 1;
                    if (j + 1 > vertices.Length - 1)
                    {
                        nextIndex = 0;
                        //Debug.Break();
                    }
                    if (j - 1 < 0)
                    {
                        prevIndex = vertices.Length - 1;
                        // Debug.Break();
                    }
                    directionToNext = (vertices[nextIndex] - vertices[j]).normalized;
                    directionToPrev = (vertices[prevIndex] - vertices[j]).normalized;


                    GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = vertices[j];
                    c.transform.parent = room.transform;
                    c.name = "J" + j;
                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = vertices[j] + directionToPrev;
                    c.transform.parent = room.transform;
                    c.name = "dir to prev " + prevIndex;
                    c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.position = vertices[j] + directionToNext;
                    c.transform.parent = room.transform;
                    c.name = "dir to next " + nextIndex;



                    tries = 0;
                    //this finds if a door is on the wall connected to the corner which is furthest from all doors
                    for (int i = 0; i < directionsToDoors.Count; i++)
                    {
                        if (directionsToDoors[i] == directionToNext)
                        {
                            dir = directionToPrev;
                            tries++;
                        }

                        if (directionsToDoors[i] == directionToPrev)
                        {
                            dir = directionToNext;
                            tries++;
                        }
                    }

                    //we have found an empty wall - skip the rest of this loop
                    if (tries < 2)
                    {
                        skip = true;
                        //overwrite "furthest" with this vertice from out for loop
                        furthest = j;
                    }
                }
            }

            //Debug.Log("dir " + dir);

            if (dir != Vector3.zero)
            {
                if (dir == directionToNext)
                    wallPoint2 = furthest + 1;
                else
                    wallPoint2 = furthest - 1;

                //make sure index is in range - just looping around the room
                if (wallPoint2 < 0)
                {
                    Debug.Log("altered vertice");
                    wallPoint2 = vertices.Length - 1;
                }
                else if (wallPoint2 > vertices.Length - 1)
                {
                    Debug.Log("altered vertice");
                    wallPoint2 = 0;
                }

                //place main object of room in middle of wall with no doors!
                GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cube);
                s.transform.parent = room.transform;
                s.name = "TV Middle";

                s.transform.position = Vector3.Lerp(vertices[furthest], vertices[wallPoint2], 0.5f);

                Vector3 toCentre = room.GetComponent<MeshRenderer>().bounds.center - s.transform.position;

                //https://forum.unity3d.com/threads/left-right-test-function.31420/  -- left right test
                Vector3 perp = Vector3.Cross(directionToPrev, toCentre);
                float d = Vector3.Dot(perp, Vector3.up);
                Quaternion spinDirection = Quaternion.Euler(0, 90, 0);
                if (d < 0.0)
                    spinDirection = Quaternion.Euler(0, -90, 0);

                s.transform.rotation = Quaternion.LookRotation(dir) * spinDirection;

                s.transform.position += Vector3.up;
                s.transform.position += s.transform.forward * 0.1f;
                s.transform.localScale += Vector3.right;
                s.transform.localScale -= Vector3.forward * 0.9f;

                GameObject centerObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                centerObj.transform.parent = room.transform;
                centerObj.transform.position = room.GetComponent<MeshRenderer>().bounds.center;
                centerObj.name = "Center";
                Quaternion r = Quaternion.LookRotation(s.transform.position - centerObj.transform.position);
                r = Quaternion.Euler(0, r.eulerAngles.y, 0);
                centerObj.transform.rotation = r;
                //find out how large we can make the couch
                //distance to nearest door
                bool tooClose = false;
                //find out if we can make this couch a 2 seater
                float closestDistance = Mathf.Infinity;
                Vector3 closestDoorPos = Vector3.zero;
                for (int i = 0; i < doors.Count; i++)
                {
                    float distance = Vector3.Distance(doors[i].transform.position, centerObj.transform.position);
                    if (distance < 1.5f)
                        tooClose = true;

                    //remember closest door's distance
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestDoorPos = doors[i].transform.position;
                    }
                }

                placedObjects.Add(centerObj);

                if (!tooClose)
                {
                    centerObj.transform.localScale = randomScale;
                    //we could possibly fit in another couch
                    if (closestDistance > 2f)
                    {
                        /*
                        //move first couch away closest door
                        //check if closest door isn't behind - if it is it moves the coucyh towards the tv, which eliminates space rather than creating some for another couch
                        Vector3 directionAwayFromDoor = (centerObj.transform.position - closestDoorPos).normalized;
                        centerObj.transform.position += directionAwayFromDoor * 1;//1 random?
                        r = Quaternion.LookRotation(s.transform.position - centerObj.transform.position);
                        r = Quaternion.Euler(0, r.eulerAngles.y, 0);
                        centerObj.transform.rotation = r;
                        Debug.Log("MOVED COUCH");

                        //try and place a new couch bewteen the 'closest door' and our center/new couch pos/doors avg?
                        Vector3 doorsAvg = Vector3.zero;
                        foreach (GameObject door in doors)
                            doorsAvg += door.transform.position;
                        doorsAvg /= doors.Count;
                        GameObject couch2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        couch2.transform.parent = room.transform;
                        couch2.transform.position = Vector3.Lerp(doorsAvg, closestDoorPos, 0.5f);
                        couch2.name = "Couch 2";
                        //look at tv/fireplace
                        r = Quaternion.LookRotation(s.transform.position - couch2.transform.position);
                        r = Quaternion.Euler(0, r.eulerAngles.y, 0);
                        couch2.transform.rotation = r;

                        placedObjects.Add(couch2);
                        */

                        //leaving couch in middle of room
                        Debug.Log("closest distance over 2");
                        // Debug.Break();

                    }
                    else
                    {
                        Debug.Log("Sad lonely lil living room");
                        //put tv in corner and face one central seat at it - lonely sad house
                        corner = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        corner.transform.parent = room.transform;
                        corner.name = "TV";
                        corner.transform.position = cornerSection + directionToNext * 0.5f + directionToPrev * 0.5f + Vector3.up;


                        centerObj.transform.localScale = new Vector3(1, randomScale.y, 1);
                        centerObj.name = "Couch";

                        //find spot for couch in the middle]
                        Vector3 closest = centerObj.transform.position;
                        float distance = Mathf.Infinity;
                        //get closest transform/options point to center / will auto skip if no transforms and default ot center                        
                        foreach (Transform t in transforms)
                        {
                            if (Vector3.Distance(t.transform.position, centerObj.transform.position) < distance)
                                closest = t.transform.position;
                        }

                        //put all th epositions in a pot and cook up the average ( I don't know how human brains work!)
                        Vector3 avg = Vector3.zero;
                        avg += corner.transform.position;
                        avg += centerObj.transform.position;
                        foreach (GameObject door in doors)
                            avg += door.transform.position;

                        avg /= doors.Count + 2;
                        centerObj.transform.position = avg;// Vector3.Lerp(centerObj.transform.position, avg, 0.5f);
                        //rotate to look at tv
                        r = Quaternion.LookRotation(corner.transform.position - centerObj.transform.position);
                        r = Quaternion.Euler(0, r.eulerAngles.y, 0);
                        centerObj.transform.rotation = r;

                        //get rid of that big middle tv
                        Destroy(s);


                    }

                }
            }

            if (tries == 0)
            {
                //  Debug.Log("MIDDLE TV WRONG? TRIES " + tries);                
            }
        }

        else if (twoSeatersPlaced == 1 && singleSeatersPlaced == 0)
        {
            //let's try to fit in another couch in this "straight pattern"
            //find the point half way towards the first couch from the corner/tv

            Vector3 halfway = Vector3.Lerp(placedObjects[0].transform.position, placedObjects[1].transform.position, 0.5f);
            GameObject couch2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            couch2.transform.parent = room.transform;
            couch2.name = "Couch 2";
            couch2.transform.position = halfway;
            //use the direction in which the first couch was placed
            Vector3 directionToMiddle = directionToNext;
            if (directionOfCouch != directionToPrev)
                directionToMiddle = directionToPrev;

            //move out the longer list's length - limit list length? couch can be too far away?
            int listLength = transformsDir1.Count;
            if (transformsDir2.Count > listLength)
                listLength = transformsDir2.Count;

            couch2.transform.position += directionToMiddle * couch2.transform.localScale.x*0.5f;// (listLength-2); -- dont really like this - just puttin at 2 just now, maybe randomise

            //make couch larger if list was long enough- basically, if the wall is long enough that it's facing
            couch2.transform.localScale += Vector3.right * (listLength - 2 - 1); //-1 because primitve is already 1


            if (listLength > 3)
                //now spin to face the halfway point
                couch2.transform.rotation = Quaternion.LookRotation(halfway - couch2.transform.position);
            else
            {
                //now spin to face the tv if single seater
                Quaternion r = Quaternion.LookRotation(Vector3.Lerp(halfway, cornerSection, 0.66f) - couch2.transform.position); //rando
                r = Quaternion.Euler(0, r.eulerAngles.y, 0);
                couch2.transform.rotation = r;
            }

            //catching a rare
            if (couch2.transform.localScale.x == 0)
            {
                Destroy(couch2);
                Debug.Log("Couch 2 scale too small, removing");
            }
            else
                placedObjects.Add(couch2);
        }

        //sitting area 
        //can we fit a table in? -table types. island,normal
        //breakfast bar between doors - 4 room house too
        float tableSize = 3;
        //check for other objects/copy vertice array to list
        List<Vector3> freeVertices = new List<Vector3>();
        for (int i = 0; i < vertices.Length; i++)
        {
            freeVertices.Add(vertices[i]);
        }
        //check for placed objects
        freeVertices = RoomPlanner.CheckCornersForOverlap(freeVertices, placedObjects, tableSize);
        //check for doors
        freeVertices = RoomPlanner.CheckCornersForOverlap(freeVertices, doors, tableSize);
        //out of all the free corners, find the furthest one  awy from door/doors
        int furthestVertice = RoomPlanner.FurthestVerticeFromObjects(doors, freeVertices);

        //sort vector3 list in order from doors--no, closest vertice to doors, 2nd sort loop,needed?
        freeVertices = RoomPlanner.SortByDistanceFromObjects(freeVertices, doors);
        //needed?

        /*
        for (int i = 0; i < freeVertices.Count; i++)
        {
            GameObject cF = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cF.transform.position = freeVertices[i];
            cF.transform.name = "Free Corner";
            cF.transform.parent = room.transform.parent;
        }
       */

        //placing table is a bit crazy - it is working like this but the code feels a bit mad- Basically, I couldn't find an elegant solution, so it's all ifs and buts
        if (freeVertices.Count > 0)
        {

            Vector3 closestFreeToDoor = freeVertices[0];
            //sort vertices by distance from doors
            freeVertices.Sort(delegate (Vector3 v1, Vector3 v2)
            {
                return Vector3.Distance(closestFreeToDoor, v1).CompareTo
                            ((Vector3.Distance(closestFreeToDoor, v2)));
            });
            //end of needed?
            //we found the vertices nearest the door, now sort the vertices in order from this corner point
            List<Vector3> sortedFreeVerticesFromCorner = new List<Vector3>();
            foreach (Vector3 v3 in freeVertices)
                sortedFreeVerticesFromCorner.Add(v3);
            //sort vertices by distance from doors
            sortedFreeVerticesFromCorner.Sort(delegate (Vector3 v1, Vector3 v2)
            {
                return Vector3.Distance(freeVertices[0], v1).CompareTo
                            ((Vector3.Distance(freeVertices[0], v2)));
            });

            //sort placed objects by distance to point we will build from
            List<GameObject> sortedPlacedObjects = RoomPlanner.SortObjectByDistanceToPoint(placedObjects, freeVertices[0]);



            //from freevertices[0], find two closet vertices
            List<Vector3> sorted = new List<Vector3>();
            foreach (Vector3 v3 in vertices)
                sorted.Add(v3);
            //sort vertices by distance from doors
            sorted.Sort(delegate (Vector3 v1, Vector3 v2)
            {
                return Vector3.Distance(freeVertices[0], v1).CompareTo
                            ((Vector3.Distance(freeVertices[0], v2)));
            });

            /*
            foreach(Vector3 v3 in sorted)
            {
                GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cube);
                s.transform.position = v3;
                s.name = "sorted";
                s.transform.parent = room.transform;
            }

            GameObject c1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c1.transform.position = sorted[1];
            c1.transform.parent = room.transform;
            c1.name = "Closest 1";

            GameObject c2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c2.transform.position = sorted[2];
            c2.transform.parent = room.transform;
            c2.name = "Closest 2";
            */
            //find if any doors are in any of these directions
            List<float> dimensions = new List<float>();

            for (int i = 1; i < 3; i++)//1 and 2
            {

                List<GameObject> doorsInLine = new List<GameObject>();
                List<float> temps = new List<float>();
                for (int j = 0; j < doors.Count; j++)
                {
                    if (Divide.PointsInLine(freeVertices[0], sorted[i], doors[j].transform.position))
                    {
                        doorsInLine.Add(doors[j]);
                    }
                }

                //sort doors in line by distance to free vertice 0

                float distanceToVertice = Vector3.Distance(freeVertices[0], sorted[i]);

                float distanceToCouch = Vector3.Distance(freeVertices[0], sortedPlacedObjects[sortedPlacedObjects.Count - 1].transform.position);
                float dimension = distanceToVertice;

                //Debug.Log(sortedPlacedObjects[sortedPlacedObjects.Count - 1].name);
                // Debug.Log("distance to vertice" + distanceToVertice);
                // Debug.Log("distance to couch" + distanceToCouch);

                if (doorsInLine.Count >= 1)
                {
                    //sort doors by distance
                    doorsInLine = RoomPlanner.SortObjectByDistanceToPoint(doorsInLine, sorted[i]);//was freevert[0]
                    float distanceToClosestDoor = Vector3.Distance(freeVertices[0], doorsInLine[0].transform.position);
                    // Debug.Log("distance to door" + distanceToClosestDoor);
                    // GameObject c3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    // c3.transform.position = doorsInLine[0].transform.position;
                    // c3.transform.parent = room.transform;
                    // c3.name = "Door";

                    dimension = distanceToClosestDoor;
                    //Debug.Log("closest to door" +i);

                }

                if (distanceToCouch * 0.6 < dimension)//i guess some of this could be randomised if things look samey
                {
                    //dimension = distanceToCouch * 0.6f - 1;
                    Debug.Log("ACTUALLY CLOSEST TO COUCH + " + sortedPlacedObjects[sortedPlacedObjects.Count-1] + i);
                    // Debug.Log(placedObjects.Count);
                    //Debug.Break();
                }
                dimensions.Add(dimension);


            }
            //make table face towards further away point
            float width = dimensions[0] - 1;
            float length = dimensions[1] - 1;
            Vector3 directionToFurthestPoint = (sorted[2] - freeVertices[0]).normalized;
            if (width > length)
            {
                // width = dimensions[1];
                //length = dimensions[0];
                //directionToFurthestPoint = (sorted[1] - freeVertices[0]).normalized;

            }



            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.transform.parent = room.transform;
            table.name = "Table";
            table.transform.position = freeVertices[0];
            //now spin to face the tv if single seater
            //Debug.Log("direction to door " + directionToClosestDoor);
            Quaternion tableRot = Quaternion.LookRotation(directionToFurthestPoint); //can be zero
            tableRot = Quaternion.Euler(0, tableRot.eulerAngles.y, 0);
            table.transform.rotation = tableRot;

            bool onLeft = RoomPlanner.CentreOfRoomOnLeft(freeVertices[0], room.GetComponent<MeshRenderer>().bounds.center, directionToFurthestPoint);
            Vector3 sideWays = table.transform.right;
            if (!onLeft)
                sideWays = -table.transform.right;

            table.transform.position += table.transform.forward * 0.1f + sideWays * 0.1f;



            table.transform.position += sideWays * width * 0.5f;
            table.transform.position += table.transform.forward * length * 0.5f;
            table.transform.position += Vector3.up * table.transform.localScale.y * 0.5f;
            //table.transform.position += Vector3.up * 0.5f;
            table.transform.localScale = new Vector3(Mathf.Abs( width), 1,Mathf.Abs( length));
            //we have placed the container box for the table! - but we must check if it has overlapped any couches, some times they can be squinty in the middle
            if (RoomPlanner.CheckForOverLap(placedObjects, table) == true)
            {
                Debug.Log("Table Overlap - removing");
                // Debug.Break();
                 Destroy(table);

            }
            else
                placedObjects.Add(table);
        }

        return placedObjects;

        //need to catch if free corner is closer to the couches than the other corners?
    }
    public static List<GameObject> LivingRoomLayoutV2(GameObject room,List<Divide.ObjectAndSize> objectsToBuild,List<GameObject> objectsAlreadyBuilt, Divide divide)
    {
       
        //neeeds moved from Divide
        List<Vector3> points = Divide.BorderPointsForRoom(room, 0.1f);
        ////get straight pattern
        List<Vector3> options = divide.StraightPatternForRoom(room, points);

        //place objects at options
        List<GameObject> allObjects = divide.PlaceObjectsAtOptions(room, options, points, objectsToBuild, true);
        
        //check overlaps
        List<GameObject> objectsFinalStraight = RemoveObjectsThatOverlap(allObjects, room);

        //don't like i have to reset a counter from here
        divide.attemptedObjects = 0;
        options = divide.SkewedPatternForRoom(room, points);

        //place objects at options
        allObjects = divide.PlaceObjectsAtOptions(room, options, points, objectsToBuild, false);
        //don't like i have to reset a counter from here
        divide.attemptedObjects = 0;
        //check overlaps
        List<GameObject> objectsFinalSkewed = RemoveObjectsThatOverlap(allObjects, room);

        //add list together and checka again for overlap
        //this decies if we prioritise skewed bed position or straight
        List<GameObject> finalBoth = new List<GameObject>();
        if (objectsFinalSkewed.Count > objectsFinalStraight.Count)
        {
            foreach (GameObject go in objectsFinalSkewed)
            {

                finalBoth.Add(go);
            }

            foreach (GameObject go in objectsFinalStraight)
            {
                //second list has a bed in it - get that skipped
                if (go.name != "TV")
                    finalBoth.Add(go);
                else
                    Destroy(go);

            }
        }
        else
        {
            foreach (GameObject go in objectsFinalStraight)
                finalBoth.Add(go);

            foreach (GameObject go in objectsFinalSkewed)
            {
                if (go.name != "TV")
                    finalBoth.Add(go);
                else
                    Destroy(go);
            }
        }
        //check this combined list for overlap
        List<GameObject> objectsFinal = RemoveObjectsThatOverlap(finalBoth, room);


        //add new list to old
        foreach (GameObject go in objectsFinal)
            objectsAlreadyBuilt.Add(go);

        //now check this for overlaps, new builds best layout with old couches and tv
        objectsAlreadyBuilt = RemoveObjectsThatOverlap(objectsAlreadyBuilt, room);

        //try and add coffee table
        //centre of couches and tv
        Vector3 avg = Vector3.zero;
        int count = objectsAlreadyBuilt.Count;
        List<GameObject> couches = new List<GameObject>();
        foreach (GameObject go in objectsAlreadyBuilt)
        {
            if (go.name != "Table")
                avg += go.transform.position;
            else
                count--;

            if (go.name == "Couch" || go.name == "Couch1" || go.name == "Couch2")
            {
                couches.Add(go);
            }
        }
        avg /= count;
        //size

        if (couches.Count > 0)
        {
            GameObject alphaCouch = couches[Random.Range(0, couches.Count)];


            GameObject coffeeTable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            coffeeTable.transform.position = alphaCouch.transform.position + alphaCouch.transform.forward * 1.5f - (alphaCouch.transform.localScale.y * 0.5f * Vector3.up);
            coffeeTable.transform.rotation = alphaCouch.transform.rotation;
            coffeeTable.transform.localScale = new Vector3(alphaCouch.transform.localScale.x * 0.75f, Random.Range(.25f, .5f), .75f);
            coffeeTable.transform.position += coffeeTable.transform.localScale.y * 0.5f * Vector3.up;
            coffeeTable.name = "CoffeeTable";
            objectsAlreadyBuilt.Add(coffeeTable);
            coffeeTable.transform.parent = room.transform;
        }
        //do we need to check overlap with coffee table ever?
        

        return objectsAlreadyBuilt;
    }
    public static List<GameObject> BedroomLayout(List<Divide.ObjectAndSize> objectsToBuild,Divide divide,GameObject room)
    {

        //neeeds moved from Divide
        List<Vector3> points = Divide.BorderPointsForRoom(room, 0.1f);
        ////get straight pattern
        List<Vector3> options = divide.StraightPatternForRoom(room, points);
        bool options1lessthan2 = false;
        if (options.Count < 2) options1lessthan2 = true;

        
        //place objects at options
        List<GameObject> allObjects = divide.PlaceObjectsAtOptions(room, options, points, objectsToBuild, true);
        //check overlaps
        List<GameObject> objectsFinalStraight = RemoveObjectsThatOverlap(allObjects, room);
        
        //don't like i have to reset a counter from here
        divide.attemptedObjects = 0;
        options = divide.SkewedPatternForRoom(room, points);
        bool options2lessthan2 = false;
        if (options.Count < 2)
            options2lessthan2 = true;

        if(options1lessthan2 && options2lessthan2)
        {
            Debug.Log("Both optoins too few");
            Debug.Break();
        }

        //place objects at options
        allObjects = divide.PlaceObjectsAtOptions(room, options, points, objectsToBuild, false);
        //don't like i have to reset a counter from here
        divide.attemptedObjects = 0;
        //check overlaps
        List<GameObject> objectsFinalSkewed = RemoveObjectsThatOverlap(allObjects, room);

        //add list together and checka again for overlap
        //this decies if we prioritise skewed bed position or straight
        List<GameObject> finalBoth = new List<GameObject>();
        if (objectsFinalSkewed.Count > objectsFinalStraight.Count)
        {
            foreach (GameObject go in objectsFinalSkewed)
            {
                
                finalBoth.Add(go);
            }

            foreach (GameObject go in objectsFinalStraight)
            {
                //second list has a bed in it - get that skipped
                if (go.name != "Bed")
                    finalBoth.Add(go);
                else
                    Destroy(go);

            }
        }
        else            
        {
            foreach (GameObject go in objectsFinalStraight)
                finalBoth.Add(go);

            foreach (GameObject go in objectsFinalSkewed)
            {
                if (go.name != "Bed")
                    finalBoth.Add(go);
                else
                    Destroy(go);
            }
        }
        //check this combined list for overlap
        List<GameObject> objectsFinal = RemoveObjectsThatOverlap(finalBoth, room);
        //find out which has more objects --- if a tie, use straight

        return objectsFinal;
    }

    public static List<GameObject> BathroomLayout(List<Divide.ObjectAndSize> objectsToBuild, Divide divide, GameObject room)
    {

        //neeeds moved from Divide
        List<Vector3> points = Divide.BorderPointsForRoom(room, 0.1f);
        ////get straight pattern
        List<Vector3> options = divide.SkewedPatternForRoom(room, points);
        
        //place objects at options
        List<GameObject> allObjects = divide.PlaceObjectsAtOptions(room, options, points, objectsToBuild,false);
        //don't like i have to reset a counter from here
        divide.attemptedObjects = 0;
        
        //check for overlap
        List<GameObject> objectsFinal = RemoveObjectsThatOverlap(allObjects, room);

        //Debug.Log("Objecst final count for bathroom = " + objectsFinal.Count);
        if(objectsFinal.Count < 3)
        {
            //remove palced objects
            foreach (GameObject go in objectsFinal)
            {
                //go.GetComponent<MeshRenderer>().enabled = true;
                //go.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;
                Destroy(go);
            }
            //reset list
            

            //remove the bath and try squeezing a shower in
            objectsToBuild[0].size = new Vector3(1f, 0.2f, 1f);
            objectsToBuild[0].name = "Shower";         

            //try agai with shower
            allObjects = divide.PlaceObjectsAtOptions(room, options, points, objectsToBuild, false);
            //don't like i have to reset a counter from here
            divide.attemptedObjects = 0;
            //check overlaps
            objectsFinal = RemoveObjectsThatOverlap(allObjects, room);

            //check for overlap
            objectsFinal = RemoveObjectsThatOverlap(objectsFinal, room);

            if (objectsFinal.Count < 3)
            {
                //remove placed objects
                //remove palced objects
                foreach (GameObject go in objectsFinal)
                {
                    //go.GetComponent<MeshRenderer>().enabled = true;
                    //go.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                    Destroy(go);
                }
                objectsFinal = new List<GameObject>();
                //remove the shower, just make bathroom with toilet and sink
                objectsToBuild.RemoveAt(0);

                //try agai with shower
                allObjects = divide.PlaceObjectsAtOptions(room, options, points, objectsToBuild, false);
                //don't like i have to reset a counter from here
                divide.attemptedObjects = 0;
                //check overlaps
                objectsFinal = RemoveObjectsThatOverlap(allObjects, room);
                //if there are only two items, we should try using the straight pattern, with two items, it looks more natural than a toilet stuck in the corner//doing?
                
            }
        }

        //we put extra toilets in to make sure it checks all the options - This method isn't the greatest
        //sp, remove any extra toilets if room was big enough to hold more - put cupboards in or laundry baskets?

        if (objectsFinal.Count > 3)
        {
            for (int i = objectsFinal.Count-1; i > 2; i--)
            {
                Destroy(objectsFinal[i]);
                objectsFinal.RemoveAt(i);                
            }
        }
        int toilets = 0;
        List<GameObject> toRemove = new List<GameObject>();
        for (int i = 0; i < objectsFinal.Count; i++)
        {
            //make sure there isn't a duplcaite toilet ~(dont like this at all!)
            if (objectsFinal[i].name == "Toilet")
                if (toilets == 0)
                    toilets++;
                else                
                    toRemove.Add(objectsFinal[i]);
         
                
        }
        foreach (GameObject go in toRemove)
        {
            Destroy(go);
            objectsFinal.Remove(go);
        }

        return objectsFinal;
        
    }

    public static List<GameObject> RemoveObjectsThatOverlap(List<GameObject> objectsToBuild, GameObject room)
    {
        List<GameObject> toRemove = new List<GameObject>();
        //check if any overlapping objects
        //firs object is most important so if we get an overlpa, remove second etc
        //bool overlap = false;
        for (int i = 0; i < objectsToBuild.Count; i++)
        {
            //we already dont want it?
            if (toRemove.Contains(objectsToBuild[i]))
                continue;

            BoxCollider thisBox = objectsToBuild[i].GetComponent<BoxCollider>();
            for (int j = 0; j < objectsToBuild.Count; j++)
            {
                if (i == j)
                    continue;

                if (toRemove.Contains(objectsToBuild[j]))
                    continue;

                BoxCollider targetBox = objectsToBuild[j].GetComponent<BoxCollider>();

                if (thisBox.bounds.Intersects(targetBox.bounds))
                {
                    toRemove.Add(objectsToBuild[j]);
                    //overlap = true;
                }
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
                if (toRemove.Contains(objectsToBuild[i]))
                    continue;

                BoxCollider thisBox = objectsToBuild[i].GetComponent<BoxCollider>();

                if (thisBox.bounds.Intersects(bc.bounds))
                {
                    //overlap = true;
                    toRemove.Add(objectsToBuild[i]);
                }

            }
            Destroy(bc);
        }

        // Debug.Log(overlap);

        foreach (GameObject go in toRemove)
        {
            objectsToBuild.Remove(go);
            Destroy(go);

        }
        return objectsToBuild;
    }
}


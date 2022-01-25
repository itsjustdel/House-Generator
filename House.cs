using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    public bool buildRooms = false;
    private float brickSize = 0.1f;
    private float height = 1f;
    private Vector3 roundedCentre;
    private GameObject site;
    // Use this for initialization
    void Update()
    {
        if(buildRooms)
        {
            buildRooms = false;
            List<GameObject> rooms = new List<GameObject>();
            for (int i = 0; i < transform.childCount; i++)
            {
                if (i == 0)
                    continue;

                Destroy(transform.GetChild(i).gameObject);
            }

            PlaceRooms();

           
        }
    }

    void Start()
    {
        //choose a random site from the cells which make up the plot
        site = ChooseSite();
        //asign this to HouseCell layer
        site.layer = LayerMask.NameToLayer("House");
        site.GetComponent<MeshCollider>().enabled = true;
        //Now we have our site, place a a quad and stretch it until it hits the
        roundedCentre = RoundV3To1DecimalPoint( site.transform.GetComponent<MeshRenderer>().bounds.center); 
        GameObject quad = CreateQuad(roundedCentre);
        quad.name = "Foundation";
        quad.layer = LayerMask.NameToLayer("House");
        //rotate this quad
        quad.transform.rotation = RotateToRoad(quad);
        //stretch this quad to site limits
        StartCoroutine(Stretch(site,quad));

        //once coroutine is finished stretching, we need to place rooms :D
       
    }

    GameObject ChooseSite()
    {
        //gett all cells attached to the game object
        List<GameObject> allCells = new List<GameObject>();
        int totalChildren = transform.parent.childCount;
        for (int i = 0; i < totalChildren - 1; i++)
        {
            allCells.Add(transform.parent.GetChild(i).gameObject);
        }

        //choose one randomly
        GameObject site;
        site = allCells[Random.Range(0, allCells.Count)];

        return site;
    }

    GameObject CreateQuad(Vector3 position)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        int[] indices = new int[6]; //2 triangles, 3 indices each

        //minus a half width so the pivot point is a the centre of the quad
        float m_Width = brickSize/2;
        float m_Length = brickSize/2;

       // vertices[0] = new Vector3(-m_Width * 0.5f, 0.0f, -m_Length * 0.5f);
       // vertices[1] = new Vector3(-m_Width * 0.5f, 0.0f, m_Length * 0.5f);
       // vertices[2] = new Vector3(m_Width * 0.5f, 0.0f, m_Length * 0.5f);
      //  vertices[3] = new Vector3(m_Width * 0.5f, 0.0f, -m_Length * 0.5f);

        
        vertices[0] = new Vector3(0f, 0.0f, 0f);
        vertices[1] = new Vector3(0f, 0.0f, m_Length);
        vertices[2] = new Vector3(m_Width , 0.0f, m_Length);
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


        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject quad = new GameObject();
        quad.layer = 26;
      
//        quad.transform.rotation = toRoad;
        MeshFilter meshFilter = quad.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = quad.AddComponent<MeshRenderer>();
    //    meshRenderer.enabled = false;
        meshRenderer.material = Resources.Load("Grey", typeof(Material)) as Material;
        // meshRenderer.enabled = false;
        quad.AddComponent<MeshCollider>();
        quad.transform.parent = transform;
        quad.transform.position = position;
        
        quad.AddComponent<QuadData>();


      
        return quad;

    }

    Quaternion RotateToRoad(GameObject quad)
    {
        //rotate to face road
        RaycastHit sphereHit;

        //if it doesnt hit at 100 radius, do 200, lower number give better accuracy
        Vector3 p = (quad.transform.rotation* (quad.transform.position)) + (Vector3.up * 200);
        float radius = 10f;

        while (!Physics.SphereCast(p, radius, Vector3.down, out sphereHit, 400f, LayerMask.GetMask("Road")))
        {
            // Debug.Log(radius + " missed");
            radius += 10f;

        }
        //zero so rotations arent wonky
     //   Vector3 zeroedHit = sphereHit.point;
      //  zeroedHit.y = transform.position.y;

        Quaternion toRoad = Quaternion.LookRotation(sphereHit.point - quad.transform.position);
        toRoad = Quaternion.Euler(0f, toRoad.eulerAngles.y, 0f);

        return toRoad;
    }

    IEnumerator Stretch(GameObject site, GameObject quad)
    {

        yield return new WaitForEndOfFrame();
        //check all corners are within site
        Mesh quadMesh = quad.GetComponent<MeshFilter>().mesh;
        Vector3[] quadVertices = quadMesh.vertices;
        QuadData qd = quad.GetComponent<QuadData>();

        bool finishedStretching = false;
        int counter = 0;
        while (!finishedStretching)
        {
            quadVertices[0] += Vector3.back * qd.backStretch;
            quadVertices[0] += Vector3.left * qd.leftStretch;
                
            quadVertices[1] += Vector3.forward * qd.forwardStretch;
            quadVertices[1] += Vector3.left * qd.leftStretch;

            quadVertices[2] += Vector3.forward * qd.forwardStretch;
            quadVertices[2] += Vector3.right * qd.rightStretch;

            quadVertices[3] += Vector3.back * qd.backStretch;
            quadVertices[3] += Vector3.right * qd.rightStretch;

            Raycasts(quad, quadMesh, 0.1f);

            counter++;

            if (counter == 100)
            {
                finishedStretching = true;
                Debug.Log("safety counter stopped stretch");
            }

            if (qd.forwardStretch == 0 && qd.backStretch == 0 && qd.leftStretch == 0 && qd.rightStretch == 0)
            {
                finishedStretching = true;
            }

            quadMesh.vertices = quadVertices;

            yield return new WaitForEndOfFrame();
        }

        //apply collider
        quad.GetComponent<MeshCollider>().sharedMesh = quadMesh;
        quadMesh.RecalculateBounds();

        //now we have finsihed stretching our foundation quad, set site's layer back to Terrain Cell - this is just to limit the number of layers we are using
        site.layer = LayerMask.NameToLayer("TerrainCell");

        Debug.Log(quad.GetComponent<MeshRenderer>().bounds.size);

        PlaceRooms();
    }

    /// <summary>
    /// expands quad to limits to cell layer "House"
    /// </summary>
    /// <param name="quad"></param>
    /// <param name="mesh"></param>
    /// <param name="gap"></param>
    public static void Raycasts(GameObject quad, Mesh mesh, float gap)
    {
        QuadData qd = quad.GetComponent<QuadData>();
        //rays need to be shot down on to the single plane quad
        //so the position shot from needs adjusted up, and then over, and then pointed downwards
        /*
        vertices
            fwd
        1 * 2
    left *   * right
        0 * 3
            back
        */

        //how much of a gap to leave at border
        float lookAmt = gap;
        #region forward
        //check three points along the side, left middle, and right

        //forward left

        Vector3 pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[1]);
        //move forward
        pos += quad.transform.forward * lookAmt;
        pos += Vector3.up * 10;
        //Debug.DrawRay(pos, Vector3.down * 20f, Color.green);
        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.forwardStretch = 0f;


            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = pos;
            c.transform.localScale *= gap;
            c.transform.rotation = quad.transform.rotation;
        }
   

        //forward right
        pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[2]);
        //move forward
        pos += quad.transform.forward * lookAmt;// * brickSize * 2;       
        pos += Vector3.up * 10;

        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.forwardStretch = 0f;
        }
        //shoot down



        //adjust to the middle
        pos = Vector3.Lerp(quad.transform.position + (quad.transform.rotation * mesh.vertices[1]), quad.transform.position + (quad.transform.rotation * mesh.vertices[2]), 0.5f);
        //move forward
        pos += quad.transform.transform.forward * lookAmt;// * brickSize * 2;       
        pos += Vector3.up * 10;
        //shoot down

        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.forwardStretch = 0f;
        }
        #endregion
        #region back
        //back


        pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[0]);
        //move back
        pos += -quad.transform.forward * lookAmt;// * brickSize * 2;

        pos += Vector3.up * 10;
        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.backStretch = 0f;
        }


        //Debug.DrawRay(pos, Vector3.down * 20f, Color.green);

        pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[3]);
        pos += Vector3.up * 10;
        //move back
        pos += -quad.transform.forward * lookAmt;// * brickSize * 2;* brickSize * 2;
        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.backStretch = 0f;
        }
      

        pos = Vector3.Lerp(quad.transform.position + (quad.transform.rotation * mesh.vertices[0]), quad.transform.position + (quad.transform.rotation * mesh.vertices[3]), 0.5f);
        pos += Vector3.up * 10;
        //move back
        pos += -quad.transform.forward * lookAmt;// * brickSize * 2;* brickSize * 2;
        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.backStretch = 0f;
        }
    
        #endregion
        #region right
        //right

        pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[2]);
        //move right
        pos += quad.transform.right * lookAmt;
        pos += Vector3.up * 10;
        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.rightStretch = 0f;
        }
        
        pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[3]);
        //move up
        pos += Vector3.up * 10;
        //move right
        pos += quad.transform.right * lookAmt;// * brickSize * 2;* brickSize * 2;
        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.rightStretch = 0f;
        }
   
        pos = Vector3.Lerp(quad.transform.position + (quad.transform.rotation * mesh.vertices[2]), quad.transform.position + (quad.transform.rotation * mesh.vertices[3]), 0.5f);
        //move up
        pos += Vector3.up * 10;
        //move right
        pos += quad.transform.right * lookAmt;// * brickSize * 2;* brickSize * 2;

        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.rightStretch = 0f;
        }
        #endregion
        #region left
        //left

        pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[0]);
        //move left
        pos += -quad.transform.right * lookAmt;// * brickSize * 2;* brickSize * 2;

        //move up
        pos += Vector3.up * 10;

        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.leftStretch = 0f;
        }

        //left (top)
        pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[1]);
        //move up
        pos += Vector3.up * 10;
        //move left
        pos += -quad.transform.right * lookAmt;// * brickSize * 2;* brickSize * 2;

        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.leftStretch = 0f;
        }

        pos = Vector3.Lerp(quad.transform.position + (quad.transform.rotation * mesh.vertices[0]), quad.transform.position + (quad.transform.rotation * mesh.vertices[1]), 0.5f);
        //move up
        pos += Vector3.up * 10;
        //move left
        pos += -quad.transform.right * lookAmt;// * brickSize * 2; * brickSize * 2;

        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.leftStretch = 0f;
        }
        #endregion
    }

    Vector3 RoundV3To1DecimalPoint(Vector3 position)
    {
        Vector3 v3 = new Vector3();

        float x = position.x;
        x *= 10;
        x = Mathf.Round(x);
        x /= 10;

        float y = position.y;

        float z = position.z;
        z *= 10;
        z = Mathf.Round(z);
        z /= 10;

        v3.x = x;
        v3.y = y;
        v3.z = z;

        return v3;
    }
 
    void PlaceRooms()
    {
        //to create a floor plane, first choose how mayn rooms will be on ground floor
        int roomNumber = 2;
        
        //spin round point and place
        List<GameObject> rooms = new List<GameObject>();
        for (int i = 0; i < roomNumber; i++)
        {
           
             Vector3 position = roundedCentre;
             position += transform.GetChild(0).rotation * new Vector3(i, 0, 0);
            GameObject room = CreateQuad(position);
            //room.transform.position = roundedCentre;    
            room.name = "Room " + i.ToString();
            //match rotation to the "Foundation";
            room.transform.rotation = transform.GetChild(0).rotation;
            room.layer = LayerMask.NameToLayer("HouseFeature");
            rooms.Add(room);
        }

        StartCoroutine(StretchRooms(rooms));
    }

    IEnumerator StretchRooms(List<GameObject> quads)
    {
        bool finishedStretching = false;

        while (!finishedStretching)//for (int j = 0; j < 2000; j++)
        {

            //Debug.Log("stretching");
            for (int i = 0; i < quads.Count; i++)// GameObject quad in quads)
            {
                yield return new WaitForEndOfFrame();

                Mesh mesh = quads[i].GetComponent<MeshFilter>().mesh;
                QuadData qd = quads[i].GetComponent<QuadData>();
                //check for other quads


                //look for gap + 0.1 for accuracy error
                Raycasts(quads[i], mesh, 0.11f, true);


                Vector3[] tempVertices = mesh.vertices;

                tempVertices[0] += Vector3.back * qd.backStretch;
                tempVertices[0] += Vector3.left * qd.leftStretch;

                tempVertices[1] += Vector3.forward * qd.forwardStretch;
                tempVertices[1] += Vector3.left * qd.leftStretch;

                tempVertices[2] += Vector3.forward * qd.forwardStretch;
                tempVertices[2] += Vector3.right * qd.rightStretch;

                tempVertices[3] += Vector3.back * qd.backStretch;
                tempVertices[3] += Vector3.right * qd.rightStretch;

                mesh.vertices = tempVertices;
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                MeshRenderer meshRenderer = quads[i].GetComponent<MeshRenderer>();

                MeshCollider meshCollider = quads[i].GetComponent<MeshCollider>();
                meshCollider.sharedMesh = mesh;

            }

            //check to see if all quads have stopped stretching
            int quadsFinished = 0;
            foreach (GameObject quad in quads)
            {
                QuadData qd = quad.GetComponent<QuadData>();
                if (qd.forwardStretch == 0 && qd.backStretch == 0 && qd.leftStretch == 0 && qd.rightStretch == 0)
                {
                    quadsFinished++;
                }
            }
            if (quadsFinished == quads.Count)
            {
             //   finishedStretching = true;

            }

            //  yield return new WaitForEndOfFrame();
        }

        //   yield return new WaitForEndOfFrame();

        //  VerticeCheck();
        //VerticeCheckV2();
        // yield return new WaitForEndOfFrame();
        yield break;


    }

    public static void Raycasts(GameObject quad, Mesh mesh, float gap, bool forRoom)
    {
        LayerMask lm = LayerMask.GetMask("HouseFeature");
        //LayerMask lm = LayerMask.GetMask(layer);

        QuadData qd = quad.GetComponent<QuadData>();
        //rays need to be shot down on to the single plane quad
        //so the position shot from needs adjusted up, and then over, and then pointed downwards
        /*
        vertices
         fwd
        1 * 2
   left *   * right
        0 * 3
         back
        */

        //float lookAmt = 0.04f; //double the bricksize in AddFeature2
        float lookAmt = gap;
        #region forward
        //check three points along the side, left middle, and right

        //forward left

        Vector3 pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[1]);

        
        //move forward
        pos += quad.transform.forward * lookAmt;
        pos += Vector3.up * 10;
      //  Debug.DrawRay(pos, Vector3.down * 20f, Color.green);
        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.forwardStretch = 0f;

       

        }
        //now adjust for the raycast
        //move up



        //shoot ray down
        RaycastHit hit;
        if (Physics.Raycast(pos, Vector3.down, out hit, 20f, lm))
        {

        //    GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
       //     c.transform.position = pos;
        //    c.transform.localScale *= gap;
         //   c.transform.rotation = quad.transform.rotation;

                qd.forwardStretch = 0f;

            if (forRoom)
                hit.transform.GetComponent<QuadData>().backStretch = 0f;
        }


        //forward right
        pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[2]);
        //move forward
        pos += quad.transform.forward * lookAmt;// * brickSize * 2;       
        pos += Vector3.up * 10;

        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.forwardStretch = 0f;
        }
        //shoot down

        if (Physics.Raycast(pos, Vector3.down, out hit, 20f, lm))
        {
           // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
         //   c.transform.position = pos;
         //   c.transform.localScale *= gap;
         //   c.transform.rotation = quad.transform.rotation;

            qd.forwardStretch = 0f;
            if (forRoom)
                hit.transform.GetComponent<QuadData>().backStretch = 0f;


        }


        //adjust to the middle
        pos = Vector3.Lerp(quad.transform.position + (quad.transform.rotation * mesh.vertices[1]), quad.transform.position + (quad.transform.rotation * mesh.vertices[2]), 0.5f);
        //move forward
        pos += quad.transform.forward * lookAmt;// * brickSize * 2;       
        pos += Vector3.up * 10;
        //shoot down

        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.forwardStretch = 0f;
        }

        if (Physics.Raycast(pos, Vector3.down, out hit, 20f, lm))
        {
            qd.forwardStretch = 0f;
            if (forRoom)
                hit.transform.GetComponent<QuadData>().backStretch = 0f;

        }

        #endregion
        #region back
        //back


        pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[0]);
        //move back
        pos += -quad.transform.forward * lookAmt;// * brickSize * 2;

        pos += Vector3.up * 10;
        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.backStretch = 0f;
        }

        //shoot down
        if (Physics.Raycast(pos, Vector3.down, out hit, 20f, lm))
        {
            qd.backStretch = 0f;
            if (forRoom)
                hit.transform.GetComponent<QuadData>().forwardStretch = 0f;

        }

        //Debug.DrawRay(pos, Vector3.down * 20f, Color.green);

        pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[3]);
        pos += Vector3.up * 10;
        //move back
        pos += -quad.transform.forward * lookAmt;// * brickSize * 2;* brickSize * 2;
        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.backStretch = 0f;
        }
        //shoot down
        if (Physics.Raycast(pos, Vector3.down, out hit, 20f, lm))
        {
            //              qd.backStretch = 0f;
            if (forRoom)
                hit.transform.GetComponent<QuadData>().forwardStretch = 0f;


        }
        //       Debug.DrawRay(pos, Vector3.back, Color.green);

        pos = Vector3.Lerp(quad.transform.position + (quad.transform.rotation * mesh.vertices[0]), quad.transform.position + (quad.transform.rotation * mesh.vertices[3]), 0.5f);
        pos += Vector3.up * 10;
        //move back
        pos += -quad.transform.forward * lookAmt;// * brickSize * 2;* brickSize * 2;
        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.backStretch = 0f;
        }
        //shoot down
        if (Physics.Raycast(pos, Vector3.down, out hit, 20f, lm))
        {
            //             qd.backStretch = 0f;
            if (forRoom)
                hit.transform.GetComponent<QuadData>().forwardStretch = 0f;

        }
        //      Debug.DrawRay(pos, Vector3.back, Color.green);
        #endregion
        #region right
        //right

        pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[2]);

        //move right
        pos += quad.transform.right * lookAmt;// * brickSize * 2;* brickSize * 2;
                                              //move up
        pos += Vector3.up * 10;

        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.rightStretch = 0f;
        }

        //shoot down
        if (Physics.Raycast(pos, Vector3.down, out hit, 20f, lm))
        {
            //              qd.rightStretch = 0f;
            if (forRoom)
                hit.transform.GetComponent<QuadData>().leftStretch = 0f;


        }



        pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[3]);
        //move up
        pos += Vector3.up * 10;
        //move right
        pos += quad.transform.right * lookAmt;// * brickSize * 2;* brickSize * 2;
        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.rightStretch = 0f;
        }
        //shoot down
        if (Physics.Raycast(pos, Vector3.down, out hit, 20f, lm))
        {
            //             qd.rightStretch = 0f;
            if (forRoom)
                hit.transform.GetComponent<QuadData>().leftStretch = 0f;

        }
        pos = Vector3.Lerp(quad.transform.position + (quad.transform.rotation * mesh.vertices[2]), quad.transform.position + (quad.transform.rotation * mesh.vertices[3]), 0.5f);
        //move up
        pos += Vector3.up * 10;
        //move right
        pos += quad.transform.right * lookAmt;// * brickSize * 2;* brickSize * 2;

        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.rightStretch = 0f;
        }
        //shoot down
        if (Physics.Raycast(pos, Vector3.down, out hit, 20f, lm))
        {
            //            qd.rightStretch = 0f;
            if (forRoom)
                hit.transform.GetComponent<QuadData>().leftStretch = 0f;

        }
        #endregion
        #region left
        //left

        pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[0]);
        //move left
        pos += -quad.transform.right * lookAmt;// * brickSize * 2;* brickSize * 2;



        //move up
        pos += Vector3.up * 10;

        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.leftStretch = 0f;
        }
        //shoot down
        if (Physics.Raycast(pos, Vector3.down, out hit, 20f, lm))
        {
            //             qd.leftStretch = 0f;
            if (forRoom)
                hit.transform.GetComponent<QuadData>().rightStretch = 0f;

        }

        //left (top)
        pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[1]);
        //move up
        pos += Vector3.up * 10;
        //move left
        pos += -quad.transform.right * lookAmt;// * brickSize * 2;* brickSize * 2;

        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.leftStretch = 0f;
        }
        //shoot down
        //    bool leftTopHandSide = false;
        if (Physics.Raycast(pos, Vector3.down, out hit, 20f, lm))
        {
            //              qd.leftStretch = 0f;
            if (forRoom)
                hit.transform.GetComponent<QuadData>().rightStretch = 0f;

        }

        pos = Vector3.Lerp(quad.transform.position + (quad.transform.rotation * mesh.vertices[0]), quad.transform.position + (quad.transform.rotation * mesh.vertices[1]), 0.5f);
        //move up
        pos += Vector3.up * 10;
        //move left
        pos += -quad.transform.right * lookAmt;// * brickSize * 2; * brickSize * 2;

        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.leftStretch = 0f;
        }
        //shoot down
        if (Physics.Raycast(pos, Vector3.down, out hit, 20f, lm))
        {

            //             qd.leftStretch = 0f;
            if (forRoom)
                hit.transform.GetComponent<QuadData>().rightStretch = 0f;


        }
        #endregion
    }

    Mesh CubeFromPlane(GameObject quad, float height)
    {
        Mesh quadMesh = quad.GetComponent<MeshFilter>().mesh;
        //  float length = Vector3.Distance(quadMesh.vertices[0], quadMesh.vertices[3]) ;
        // float width = 1f;// Vector3.Distance(quadMesh.vertices[0], quadMesh.vertices[3]);
        #region Vertices
        Vector3 p0 = new Vector3(quadMesh.vertices[0].x, quadMesh.vertices[0].y, quadMesh.vertices[0].z);
        Vector3 p1 = new Vector3(quadMesh.vertices[1].x, quadMesh.vertices[1].y, quadMesh.vertices[1].z);
        Vector3 p2 = new Vector3(quadMesh.vertices[2].x, quadMesh.vertices[2].y, quadMesh.vertices[2].z);
        Vector3 p3 = new Vector3(quadMesh.vertices[3].x, quadMesh.vertices[3].y, quadMesh.vertices[3].z);

        Vector3 p4 = new Vector3(quadMesh.vertices[0].x, height, quadMesh.vertices[0].z);
        Vector3 p5 = new Vector3(quadMesh.vertices[1].x, height, quadMesh.vertices[1].z);
        Vector3 p6 = new Vector3(quadMesh.vertices[2].x, height, quadMesh.vertices[2].z);
        Vector3 p7 = new Vector3(quadMesh.vertices[3].x, height, quadMesh.vertices[3].z);

        //quadMesh.vertices[0].y + 

        // You can change that line to provide another MeshFilter
        MeshFilter filter = quad.GetComponent<MeshFilter>();
        Mesh mesh = filter.mesh;
        mesh.Clear();

        Vector3[] vertices = new Vector3[]
        {
	// Bottom
	p0, p1, p2, p3,
 
	// Left
	p7, p4, p0, p3,
 
	// Front
	p4, p5, p1, p0,
 
	// Back
	p6, p7, p3, p2,
 
	// Right
	p5, p6, p2, p1,
 
	// Top
	p7, p6, p5, p4
        };
        #endregion

        #region Normales
        Vector3 up = Vector3.up;
        Vector3 down = Vector3.down;
        Vector3 front = Vector3.forward;
        Vector3 back = Vector3.back;
        Vector3 left = Vector3.left;
        Vector3 right = Vector3.right;

        Vector3[] normales = new Vector3[]
        {
	// Bottom
	down, down, down, down,
 
	// Left
	left, left, left, left,
 
	// Front
	front, front, front, front,
 
	// Back
	back, back, back, back,
 
	// Right
	right, right, right, right,
 
	// Top
	up, up, up, up
        };
        #endregion

        #region UVs
        Vector2 _00 = new Vector2(0f, 0f);
        Vector2 _10 = new Vector2(1f, 0f);
        Vector2 _01 = new Vector2(0f, 1f);
        Vector2 _11 = new Vector2(1f, 1f);

        Vector2[] uvs = new Vector2[]
        {
	// Bottom
	_11, _01, _00, _10,
 
	// Left
	_11, _01, _00, _10,
 
	// Front
	_11, _01, _00, _10,
 
	// Back
	_11, _01, _00, _10,
 
	// Right
	_11, _01, _00, _10,
 
	// Top
	_11, _01, _00, _10,
        };
        #endregion

        #region Triangles
        int[] triangles = new int[]
        {
	// Bottom
	3, 1, 0,
    3, 2, 1,			
 
	// Left
	3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
    3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
 
	// Front
	3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
    3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
 
	// Back
	3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
    3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
 
	// Right
	3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
    3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
 
	// Top
	3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
    3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,

        };
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        ;

        return mesh;

    }
}
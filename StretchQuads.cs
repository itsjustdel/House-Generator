using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class StretchQuads : ProcBase
{

    public int minimumSize = 50;
    public int maximumSize = 100;

    public GameObject housePrefab;

    public List<GameObject> quads;
    //  public float leftStretch = 0.02f;
    //  public float rightStretch = 0.02f;
    //  public float forwardStretch = 0.02f;
    //  public float backStretch = 0.02f;

    [Range(0.01f, 1f)]
    public float brickSize = 0.2f;
    public float height = 0.4f;
    // Use this for initialization
    public float meshColliderX = 1f;
    public float meshColliderY = 1f;
    public float meshColliderZ = 1f;
    public List<Vector3> vertices;// = new List<Vector3>();
    private float outsideBrickSize;
    private float tileSize;

    public bool animate = false;

    public float targetSizeX = 10f;
    public float targetSizeZ = 10f;

    public bool limitHit0 = false;
    public bool limitHit1 = false;
    public bool limitHit2 = false;
    public bool limitHit3 = false;
    public Quaternion toRoad;
    public Vector3 dirToRoad;
    public Vector3 dirToSide;

    public string plotType;

    void Awake()
    {
        // enabled = false;
    }
    void Start()
    {
        //   transform.rotation = Quaternion.identity;

        if (GetComponent<AddFeaturesToHouse2>().makeRandomHouse)
        {
            GetComponent<AddFeaturesToHouse2>().outsideBrickSizeMultiplier = Random.Range(4, 6); //small limits just now
                                                                                                 // GetComponent<AddFeaturesToHouse2>().tileSizeMultiplier = Random.Range(1, 3); //ony working with multipler at 1 
        }

        //this number is the brickSize(grid size, really, multipled by a scaler) //why is brickize *2?
        outsideBrickSize = (brickSize * 2) * GetComponent<AddFeaturesToHouse2>().outsideBrickSizeMultiplier;
        tileSize = outsideBrickSize * GetComponent<AddFeaturesToHouse2>().tileSizeMultiplier;

        // AdjustMeshCollider();

        vertices = new List<Vector3>();
        quads = new List<GameObject>();


        RotateToRoad();

        //StartCoroutine("StretchMeshCollider2");

        GetComponent<FloorPlanQuads>().amountOfRooms = 1;
        GetComponent<FloorPlanQuads>().CreateQuads();
        GetQuads();


        StartCoroutine("StretchRooms");



    }

    void AdjustMeshCollider()
    {
        int multiplier = GetComponent<AddFeaturesToHouse2>().outsideBrickSizeMultiplier;
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        Vector3[] vertices = mesh.vertices;

        //make mesh collider X bricks along
        float xSize = targetSizeX * brickSize * multiplier;
        float zSize = targetSizeZ * brickSize * multiplier;

        //takes half a brick width off the full length
        //this is because the corner bricks slot in to each other
        xSize -= brickSize * multiplier * 0.5f;
        zSize -= brickSize * multiplier * 0.5f;

        //  Debug.Log(xSize);

        for (int i = 0; i < mesh.vertexCount; i++)
        {
            //make quad the size of the scale
            //    vertices[i].x *= outsideBrickSize;
            //    vertices[i].z *= outsideBrickSize;

            //then multiply
            vertices[i].x *= xSize;
            //round to 2 decimal point
            /*          vertices[i].x *= 100;
                      vertices[i].x = Mathf.Round(vertices[i].x);
                      vertices[i].x *= 0.01f;

                      vertices[i].z *= 100;
                      vertices[i].z = Mathf.Round(vertices[i].z);
                      vertices[i].z *= 0.01f;
                      */
            vertices[i].z *= zSize;

            //note - when appled to mesh collider size,there can be small discrepencies
            //Not sure if it's just the way PhysX/Unity has to implement it, but it just means, when doing raycast checks
            //or any other accurate checks, put at least 0.001 range in (spherecast radius)
        }

        mesh.vertices = vertices;

        GetComponent<MeshCollider>().sharedMesh = mesh;

        // Debug.Log("Mesh collider x size" + GetComponent<MeshCollider>().bounds.size.x);
    }
    void RotateToRoad()
    {
        //rotate to face road
        RaycastHit sphereHit;

        //if it doesnt hit at 100 radius, do 200, lower number give better accuracy
        Vector3 p = (transform.rotation * transform.position) + (Vector3.up * 200);
        float radius = 10f;

        while (!Physics.SphereCast(p, radius, Vector3.down, out sphereHit, 400f, LayerMask.GetMask("Road")))
        {
// Debug.Log(radius + " missed");
            radius += 10f;
            
        }

        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c.transform.position = transform.position;

        GameObject c2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c2.transform.position = sphereHit.point;

        //zero so rotations arent wonky
        Vector3 zeroedHit = sphereHit.point;
        zeroedHit.y = transform.position.y;

        //save to public
        dirToRoad = (zeroedHit - transform.position).normalized;
        dirToSide = (Quaternion.Euler(0f, -90, 0f) * dirToRoad).normalized;

        toRoad = Quaternion.LookRotation(sphereHit.point - transform.position);
        toRoad = Quaternion.Euler(0f, toRoad.eulerAngles.y, 0f);
        
       // toRoad = Quaternion.LookRotation(transform.parent.parent.parent.FindChild("Combined mesh").GetComponent<GardenCentre>().toRoad);

        //make parent face road
        transform.rotation = toRoad;
    }
    IEnumerator StretchMeshCollider()
    {

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.parent = transform;

        float range = 0f;
        Vector3 random = new Vector3(Random.Range(-range, range), 0f, Random.Range(-range, range));
        quad.transform.position = transform.position;// + random;
        quad.layer = LayerMask.NameToLayer("House");
        quad.name = "Quad For House Size";

        quad.GetComponent<MeshRenderer>().enabled = false;

        //rotate to face road

        RaycastHit sphereHit;
        Physics.SphereCast(quad.transform.position + (Vector3.up * 50), 40f, Vector3.down, out sphereHit, 100f, LayerMask.GetMask("Road"));

        //   GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //   cube.transform.position = sphereHit.point;
        //    cube.name = "road";

        //rotate quad to face road
        //this makes the quad's z axis face the road. Forward is towards the road

        //zero so rotations arent wonky
        Vector3 zeroedHit = sphereHit.point;
        zeroedHit.y = quad.transform.position.y;

        dirToRoad = (zeroedHit - quad.transform.position).normalized;
        dirToSide = (Quaternion.Euler(0f, -90, 0f) * dirToRoad).normalized;
        //   quad.transform.rotation = toRoad;

        toRoad = Quaternion.LookRotation(sphereHit.point - quad.transform.position);
        toRoad = Quaternion.Euler(0f, toRoad.eulerAngles.y, 0f);

        //the primitive quad's vertices are placed on the x,y axis so it stands up, we need it to lie on its back
        //change the vertices ratther han the transform's rotation
        Mesh mesh = quad.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = Quaternion.Euler(90f, 0f, 0f) * vertices[i];
            vertices[i] *= brickSize;
            vertices[i] = toRoad * vertices[i];
        }

        mesh.vertices = vertices;
        quad.GetComponent<MeshCollider>().sharedMesh = mesh;



        QuadData qd = quad.AddComponent<QuadData>();
        qd.forwardStretch = brickSize;
        qd.backStretch = brickSize;
        qd.leftStretch = brickSize;
        qd.rightStretch = brickSize;
        //** note, left and right stretch may be used with wrong directions below. Doesn't matter just now as all stretches
        //are begin done using the same scale



        bool finishedStretching = false;

        while (!finishedStretching)//// for (int j = 0; j < 100; j++)
        {


            Vector3[] tempVertices = mesh.vertices;

            float limitX = Random.Range(2f, 5f);
            float limitZ = Random.Range(2f, 5f);
            //force limits  ************************************************************
            limitX = 100f;
            limitZ = 100f;
            //*****************************************************

            //this is one of the rear vertices
            int count = 0;

            //if potential point has not hit terrain cell
            RaycastHit hit;


            //    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //     cube.transform.position = tempVertices[0] + quad.transform.position + Vector3.up;
            if (Physics.Raycast(tempVertices[0] + quad.transform.position + Vector3.up, Vector3.down, out hit, 2f, LayerMask.GetMask("HouseCell", "TerrainCell")))
            {
                //  GameObject cuber = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //  cuber.transform.position = tempVertices[0] + quad.transform.position + Vector3.up;


                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("TerrainCell"))
                {
                    limitHit0 = true;
                    // qd.forwardStretch = 0f;
                    qd.leftStretch = 0f;
                    GameObject cuber = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cuber.transform.position = tempVertices[0] + quad.transform.position + Vector3.up;
                }
                else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("HouseCell"))
                {
                    float distance = Vector3.Distance(tempVertices[0] + quad.transform.position, quad.transform.position);
                    //if distance is less than house limit size
                    if (distance < limitX)
                    {
                        //stretch vertices

                        //tempVertices[0] += (quad.transform.rotation * Vector3.back * qd.backStretch);
                        tempVertices[0] += dirToSide * qd.leftStretch;
                        count++;

                    }
                    if (distance < limitZ)
                    {
                        tempVertices[0] += -dirToRoad * qd.backStretch;
                        //   tempVertices[0] += (quad.transform.rotation * Vector3.left * qd.leftStretch);
                        count++;
                    }
                }
            }
            //if has hit limit
            if (count == 0)
                limitHit0 = true;

            //reset
            count = 0;

            if (Physics.Raycast(tempVertices[1] + quad.transform.position + Vector3.up, Vector3.down, out hit, 2f, LayerMask.GetMask("HouseCell", "TerrainCell")))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("TerrainCell"))
                {
                    limitHit1 = true;
                }
                else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("HouseCell"))
                {
                    float distance = Vector3.Distance(tempVertices[1] + quad.transform.position, quad.transform.position);

                    if (distance < limitX)
                    {
                        //tempVertices[1] += (quad.transform.rotation * Vector3.forward * qd.forwardStretch);
                        tempVertices[1] += -dirToSide * qd.leftStretch;
                        count++;
                    }
                    if (distance < limitZ)
                    {
                        tempVertices[1] += dirToRoad * qd.forwardStretch;
                        //tempVertices[1] += (quad.transform.rotation * Vector3.left * qd.leftStretch);
                        count++;
                    }
                }
            }

            if (count == 0)
                limitHit1 = true;

            count = 0;

            if (Physics.Raycast(tempVertices[2] + quad.transform.position + Vector3.up, Vector3.down, out hit, 2f, LayerMask.GetMask("HouseCell", "TerrainCell")))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("TerrainCell"))
                {
                    limitHit2 = true;
                }
                else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("HouseCell"))
                {
                    float distance = Vector3.Distance(tempVertices[2] + quad.transform.position, quad.transform.position);
                    if (distance < limitX)
                    {
                        //  tempVertices[2] += (quad.transform.rotation * Vector3.forward * qd.forwardStretch);
                        tempVertices[2] += -dirToSide * qd.rightStretch;
                        count++;
                    }
                    if (distance < limitZ)
                    {
                        tempVertices[2] += -dirToRoad * qd.backStretch;
                        //  tempVertices[2] += (quad.transform.rotation * Vector3.right * qd.rightStretch);
                        count++;
                    }
                }
            }

            if (count == 0)
                limitHit2 = true;

            count = 0;


            if (Physics.Raycast(tempVertices[3] + quad.transform.position + Vector3.up, Vector3.down, out hit, 2f, LayerMask.GetMask("HouseCell", "TerrainCell")))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("TerrainCell"))
                {
                    limitHit3 = true;
                }
                else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("HouseCell"))
                {
                    float distance = Vector3.Distance(tempVertices[3] + quad.transform.position, quad.transform.position);
                    if (distance < limitX)
                    {
                        // tempVertices[3] += (quad.transform.rotation * Vector3.back * qd.backStretch);
                        tempVertices[3] += dirToSide * qd.rightStretch;
                        count++;
                    }
                    if (distance < limitZ)
                    {
                        tempVertices[3] += dirToRoad * qd.forwardStretch;
                        //tempVertices[3] += (quad.transform.rotation * Vector3.right * qd.rightStretch);
                        count++;
                    }
                }
            }

            if (count == 0)
                limitHit3 = true;

            count = 0;

            mesh.vertices = tempVertices;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            MeshRenderer meshRenderer = quad.GetComponent<MeshRenderer>();

            MeshCollider meshCollider = quad.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            //Vector3 resize = new Vector3(meshRenderer.bounds.size.x *, 0.1f, meshRenderer.bounds.size.z);
            //meshCollider.size = resize;            

            // if (animate)
            //  yield return new WaitForEndOfFrame();



            //check for any overlapping vertices



            //check to see if quad have stopped stretching

            //  if (qd.forwardStretch == 0 && qd.backStretch == 0 && qd.leftStretch == 0 && qd.rightStretch == 0 || 
            if (limitHit0 && limitHit1 && limitHit2 && limitHit3)
            {
                finishedStretching = true;
            }

            yield return new WaitForEndOfFrame();
        }

        // CubeFromPlane(quad);

        yield return new WaitForEndOfFrame();

        GetComponent<FloorPlanQuads>().CreateQuads();
        GetQuads();

        StartCoroutine("StretchRooms");

        yield break;

    }
    IEnumerator StretchMeshCollider2()
    {

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        int[] indices = new int[6]; //2 triangles, 3 indices each

        //minus a half width so the pivot point is a the centre of the quad
        float m_Width = 0.5f;
        float m_Length = 0.5f;
        vertices[0] = new Vector3(-m_Width * 0.5f, 0.0f, -m_Length * 0.5f);
        vertices[1] = new Vector3(-m_Width * 0.5f, 0.0f, m_Length * 0.5f);
        vertices[2] = new Vector3(m_Width * 0.5f, 0.0f, m_Length * 0.5f);
        vertices[3] = new Vector3(m_Width * 0.5f, 0.0f, -m_Length * 0.5f);

        /*
        vertices[0] = new Vector3(0f, 0.0f, 0f);
        vertices[1] = new Vector3(0f, 0.0f, m_Length);
        vertices[2] = new Vector3(m_Width , 0.0f, m_Length);
        vertices[3] = new Vector3(m_Width, 0.0f, 0f);
        */
        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;

        indices[3] = 0;
        indices[4] = 2;
        indices[5] = 3;


        GameObject quad = new GameObject();

        MeshFilter meshFilter = quad.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = quad.AddComponent<MeshRenderer>();
        //meshRenderer.enabled = false;
        meshRenderer.material = Resources.Load("Grey", typeof(Material)) as Material;
        // meshRenderer.enabled = false;
        quad.AddComponent<MeshCollider>();
        quad.transform.parent = transform;
        quad.layer = LayerMask.NameToLayer("HouseFeature");
        //  quad.AddComponent<QuadData>();


        float range = 0f;
        Vector3 random = new Vector3(Random.Range(-range, range), 0f, Random.Range(-range, range));
        quad.transform.position = transform.position;// + random;
        quad.layer = LayerMask.NameToLayer("House");
        quad.name = "Quad For House Size";

        quad.GetComponent<MeshRenderer>().enabled = false;
        //rotate to face road
        RaycastHit sphereHit;
        Physics.SphereCast(quad.transform.position + (Vector3.up * 50), 40f, Vector3.down, out sphereHit, 100f, LayerMask.GetMask("Road"));

        //zero so rotations arent wonky
        Vector3 zeroedHit = sphereHit.point;
        zeroedHit.y = quad.transform.position.y;

        //save to public
        dirToRoad = (zeroedHit - quad.transform.position).normalized;
        dirToSide = (Quaternion.Euler(0f, -90, 0f) * dirToRoad).normalized;

        toRoad = Quaternion.LookRotation(sphereHit.point - quad.transform.position);
        toRoad = Quaternion.Euler(0f, toRoad.eulerAngles.y, 0f);


        ////working? getting used??
        //make parent face road
        transform.rotation = toRoad;
        //now parent has been rotated, child this quad
        quad.transform.parent = transform;



        for (int i = 0; i < vertices.Length; i++)
        {
            // vertices[i] = Quaternion.Euler(90f, 0f, 0f) * vertices[i];
            vertices[i] *= brickSize;
            vertices[i] = toRoad * vertices[i];
        }

        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        quad.GetComponent<MeshCollider>().sharedMesh = mesh;


        QuadData qd = quad.AddComponent<QuadData>();
        qd.forwardStretch = brickSize;
        qd.backStretch = brickSize;
        qd.leftStretch = brickSize;
        qd.rightStretch = brickSize;
        //** note, left and right stretch may be used with wrong directions below. Doesn't matter just now as all stretches
        //are being done using the same scale



        bool finishedStretching = false;

        while (!finishedStretching)//// for (int j = 0; j < 100; j++)
        {


            Vector3[] tempVertices = mesh.vertices;

            //check for other quads


            Raycasts(quad, mesh, "TerrainCell", false);


            tempVertices[0] += toRoad * Vector3.back * qd.backStretch;
            tempVertices[0] += toRoad * Vector3.left * qd.leftStretch;

            tempVertices[1] += toRoad * Vector3.forward * qd.forwardStretch;
            tempVertices[1] += toRoad * Vector3.left * qd.leftStretch;

            tempVertices[2] += toRoad * Vector3.forward * qd.forwardStretch;
            tempVertices[2] += toRoad * Vector3.right * qd.rightStretch;

            tempVertices[3] += toRoad * Vector3.back * qd.backStretch;
            tempVertices[3] += toRoad * Vector3.right * qd.rightStretch;

            mesh.vertices = tempVertices;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            //meshRenderer = quad.GetComponent<MeshRenderer>();

            MeshCollider meshCollider = quad.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;


            //check to see if quad have stopped stretching


            if (qd.forwardStretch == 0 && qd.backStretch == 0 && qd.leftStretch == 0 && qd.rightStretch == 0)
            {
                finishedStretching = true;
            }

            yield return new WaitForEndOfFrame();
        }

        // CubeFromPlane(quad);


        yield return new WaitForEndOfFrame();

        GetComponent<FloorPlanQuads>().amountOfRooms = 1;
        GetComponent<FloorPlanQuads>().CreateQuads();
        GetQuads();

        StartCoroutine("StretchRooms");

        yield break;

    }

    void GetQuads()
    {
        quads.Clear();
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            //1st child is the collider //changed
            //  if (i == 0)
            //      continue;

            if (!animate)
                transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().enabled = false;

            quads.Add(transform.GetChild(i).gameObject);
        }
    }

    IEnumerator StretchRooms()
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


                Raycasts(quads[i], mesh, "HouseFeature", true);


                Vector3[] tempVertices = mesh.vertices;

                tempVertices[0] += Vector3.back * qd.backStretch;
                tempVertices[0] += Vector3.left * qd.leftStretch;

                tempVertices[1] += Vector3.forward * qd.forwardStretch;
                tempVertices[1] += Vector3.left * qd.leftStretch;

                tempVertices[2] += Vector3.forward * qd.forwardStretch;
                tempVertices[2] += Vector3.right * qd.rightStretch;

                tempVertices[3] += Vector3.back * qd.backStretch;
                tempVertices[3] += Vector3.right * qd.rightStretch;



                //using parent transform rotation as governing rotation now
                /*
                tempVertices[0] += toRoad * Vector3.back * qd.backStretch;
                tempVertices[0] += toRoad * Vector3.left * qd.leftStretch;

                tempVertices[1] += toRoad * Vector3.forward * qd.forwardStretch;      
                tempVertices[1] += toRoad * Vector3.left * qd.leftStretch;

                tempVertices[2] += toRoad * Vector3.forward * qd.forwardStretch;
                tempVertices[2] += toRoad * Vector3.right * qd.rightStretch;

                tempVertices[3] += toRoad * Vector3.back * qd.backStretch;
                tempVertices[3] += toRoad * Vector3.right * qd.rightStretch;
                */


                mesh.vertices = tempVertices;
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                MeshRenderer meshRenderer = quads[i].GetComponent<MeshRenderer>();

                MeshCollider meshCollider = quads[i].GetComponent<MeshCollider>();
                meshCollider.sharedMesh = mesh;

                //Vector3 resize = new Vector3(meshRenderer.bounds.size.x *, 0.1f, meshRenderer.bounds.size.z);
                //meshCollider.size = resize;            

                // if (animate)
                //  yield return new WaitForEndOfFrame();



                //check for any overlapping vertices

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
                finishedStretching = true;

            }

            //  yield return new WaitForEndOfFrame();
        }

        //   yield return new WaitForEndOfFrame();

        //  VerticeCheck();
        VerticeCheckV2();
        // yield return new WaitForEndOfFrame();
        yield break;


    }

    IEnumerator Stretch()
    {
        yield return new WaitForEndOfFrame();
        bool finishedStretching = false;

        while (!finishedStretching)//for (int j = 0; j < 2000; j++)
        {
            for (int i = 0; i < quads.Count; i++)// GameObject quad in quads)
            {

                Mesh mesh = quads[i].GetComponent<MeshFilter>().sharedMesh;
                QuadData qd = quads[i].GetComponent<QuadData>();
                //check for other quads
                Raycasts(quads[i], mesh, "House", true);
                //check for edge of house collider.


                Vector3[] tempVertices = mesh.vertices;
                tempVertices[0] += toRoad * Vector3.back * qd.backStretch;
                tempVertices[0] += toRoad * Vector3.left * qd.leftStretch;

                tempVertices[1] += toRoad * Vector3.forward * qd.forwardStretch;
                tempVertices[1] += toRoad * Vector3.left * qd.leftStretch;

                tempVertices[2] += toRoad * Vector3.forward * qd.forwardStretch;
                tempVertices[2] += toRoad * Vector3.right * qd.rightStretch;

                tempVertices[3] += toRoad * Vector3.back * qd.backStretch;
                tempVertices[3] += toRoad * Vector3.right * qd.rightStretch;

                mesh.vertices = tempVertices;
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                MeshRenderer meshRenderer = quads[i].GetComponent<MeshRenderer>();

                MeshCollider meshCollider = quads[i].GetComponent<MeshCollider>();
                meshCollider.sharedMesh = mesh;

                //Vector3 resize = new Vector3(meshRenderer.bounds.size.x *, 0.1f, meshRenderer.bounds.size.z);
                //meshCollider.size = resize;            

                // if (animate)
                //  yield return new WaitForEndOfFrame();



                //check for any overlapping vertices

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
                finishedStretching = true;
                //     Debug.Log("Finished Stretching");
            }

            // yield return new WaitForEndOfFrame();
        }

        //   yield return new WaitForEndOfFrame();

        //  VerticeCheck();
        VerticeCheckV2();
        // yield return new WaitForEndOfFrame();
        yield break;


    }
    void VerticeCheck()
    {
        vertices.Clear();
        //create a list of vertices from each quad;
        foreach (GameObject quad in quads)
        {
            Vector3[] quadVertices = quad.GetComponent<MeshFilter>().mesh.vertices;

            foreach (Vector3 vertice in quadVertices)
                vertices.Add(vertice + quad.transform.position);
        }

        //check to see if any vertices are at the same point or extremely close to each other
        //not sure why all points aren't on the grid. Rounding is not working

        bool duplicates = false;
        for (int i = 0; i < vertices.Count; i++)
        {
            for (int j = 0; j < vertices.Count; j++)
            {
                //don't check the same array index against each other, it will obviously be the at the same point
                if (i == j)
                    continue;

                if (Vector3.Distance(vertices[i], vertices[j]) < 0.01)
                {
                    Debug.Log("duplicates - Stretch Quads");
                    duplicates = true;

                }
            }
        }
        if (duplicates)
        {
            StartCoroutine("Resetter");
            AdjustMeshCollider();
            // Reset();
        }
        //if there are no duplicates, let's go ahead and make these quads cubes!
        if (!duplicates)
        {
            CubeThoseQuads();
        }

    }
    void VerticeCheckV2()
    {
        //from each corner of each quad raycast to check for any overlaps

        bool reset = false;
        foreach (GameObject quad in quads)
        {
            Vector3[] quadVertices = quad.GetComponent<MeshFilter>().mesh.vertices;

            foreach (Vector3 vertice in quadVertices)
            {
                LayerMask lm = LayerMask.GetMask("HouseFeature");
                RaycastHit[] hits = Physics.RaycastAll(quad.transform.position + vertice + (Vector3.up * 0.1f), Vector3.down, 1f, lm);
                if (hits.Length > 1)
                {
                    reset = true;
                }
            }
        }

        if (reset)
            StartCoroutine("Resetter");
        else if (!reset)
        {
            CubeThoseQuads();
            //Debug.Log("continuing on..");
        }
    }
    void CubeThoseQuads()
    {
        foreach (GameObject quad in quads)
        {
            Mesh newMesh = CubeFromPlane(quad);
            quad.GetComponent<MeshFilter>().mesh = newMesh;
            quad.GetComponent<MeshCollider>().sharedMesh = newMesh;

        }

        //once the cubes are created we can start the next script

        StartCoroutine("StartFeatures");
    }
    IEnumerator StartFeatures()
    {
        yield return new WaitForEndOfFrame();

        //if garden centre
        if (transform.parent.parent.name == "GardenCentreBuildingPlot")
        {
              gameObject.AddComponent<BuildGardenCentre>().enabled = true;
           // Debug.Log("buildGardenCetnre");

        }
        else
            GetComponent<AddFeaturesToHouse2>().enabled = true;
    }

    /// <summary>
    /// creates a cube out of a plane
    /// </summary>
    Mesh CubeFromPlane(GameObject quad)
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

    IEnumerator Resetter()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log("Resetting - Reset Coroutine");
        // GetQuads();

        //there was a a failure,  reset/delete quads and try again
        for (int i = 0; i < quads.Count; i++)
        {
            Destroy(quads[i]);
        }
        //   yield return new WaitForEndOfFrame();
        //   quads.Clear();
        //   vertices.Clear();
        yield return new WaitForEndOfFrame();

        //     Destroy(gameObject);
        //     Instantiate(housePrefab, transform.position, Quaternion.identity);

        //restart
        GetComponent<FloorPlanQuads>().CreateQuads();
        yield return new WaitForEndOfFrame();
        GetQuads();
        StartCoroutine("Stretch");

        //     Start();
        yield break;

    }

    void Raycasts(GameObject quad, Mesh mesh, string layer, bool forRoom)
    {
        //LayerMask lm = LayerMask.GetMask("House", "HouseCell");
        LayerMask lm = LayerMask.GetMask(layer);

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
        float lookAmt = brickSize;// * 2;
        #region forward
        //check three points along the side, left middle, and right

        //forward left

        Vector3 pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[1]);
        //move forward
        pos += transform.forward * lookAmt;
        pos += Vector3.up * 10;
        Debug.DrawRay(pos, Vector3.down * 20f, Color.green);
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

            qd.forwardStretch = 0f;
            if (forRoom)
                hit.transform.GetComponent<QuadData>().backStretch = 0f;
        }


        //forward right
        pos = quad.transform.position + (quad.transform.rotation * mesh.vertices[2]);
        //move forward
        pos += transform.forward * lookAmt;// * brickSize * 2;       
        pos += Vector3.up * 10;

        //check if in plot area
        if (!Physics.Raycast(pos, Vector3.down, 20f, LayerMask.GetMask("House")))
        {
            qd.forwardStretch = 0f;
        }
        //shoot down

        if (Physics.Raycast(pos, Vector3.down, out hit, 20f, lm))
        {
            qd.forwardStretch = 0f;
            if (forRoom)
                hit.transform.GetComponent<QuadData>().backStretch = 0f;


        }


        //adjust to the middle
        pos = Vector3.Lerp(quad.transform.position + (quad.transform.rotation * mesh.vertices[1]), quad.transform.position + (quad.transform.rotation * mesh.vertices[2]), 0.5f);
        //move forward
        pos += transform.forward * lookAmt;// * brickSize * 2;       
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
        pos += -transform.forward * lookAmt;// * brickSize * 2;

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
        pos += -transform.forward * lookAmt;// * brickSize * 2;* brickSize * 2;
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
        pos += -transform.forward * lookAmt;// * brickSize * 2;* brickSize * 2;
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
        pos += transform.right * lookAmt;// * brickSize * 2;* brickSize * 2;
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
        pos += transform.right * lookAmt;// * brickSize * 2;* brickSize * 2;
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
        pos += transform.right * lookAmt;// * brickSize * 2;* brickSize * 2;

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
        pos += -transform.right * lookAmt;// * brickSize * 2;* brickSize * 2;



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
        pos += -transform.right * lookAmt;// * brickSize * 2;* brickSize * 2;

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
        pos += -transform.right * lookAmt;// * brickSize * 2; * brickSize * 2;

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

    /// <summary>
    /// returns true if point is inside box
    /// </summary>
    /// <param name="point"></param>
    /// <param name="box"></param>
    /// <returns></returns>
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

    float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
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
}

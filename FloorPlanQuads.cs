using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class FloorPlanQuads : MonoBehaviour {


    public int amountOfRooms = 5;
    public float m_Width; //start quad size
    public float m_Length;
    public List<Vector3> quadSpawnPositions = new List<Vector3>();
    public List<GameObject> quads = new List<GameObject>();

    void Awake()
    {
        this.enabled = false;
    }

    void Start()
    {
        

        CreateQuads();
    }
   
    public void CreateQuads()
    {
        amountOfRooms = 1;

        quadSpawnPositions.Clear();
        quads.Clear();
        for (int i = 0; i < amountOfRooms; i++)
        {
            CreateQuad();
        }

           //     PlaceQuads();
    }
	

    void CreateQuad()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        int[] indices = new int[6]; //2 triangles, 3 indices each

        //minus a half width so the pivot point is a the centre of the quad
        
        vertices[0] = new Vector3(-m_Width*0.5f, 0.0f, -m_Length*0.5f);
        vertices[1] = new Vector3(-m_Width*0.5f, 0.0f, m_Length*0.5f);
        vertices[2] = new Vector3(m_Width*0.5f, 0.0f, m_Length*0.5f);
        vertices[3] = new Vector3(m_Width*0.5f, 0.0f, -m_Length*0.5f);
        
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

        //rotate to face road

        Quaternion toRoad = transform.GetComponent<StretchQuads>().toRoad;
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
        quad.name = "Room";
        quad.transform.rotation = toRoad;
        MeshFilter meshFilter = quad.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = quad.AddComponent<MeshRenderer>();
        meshRenderer.enabled = false;
        meshRenderer.material = Resources.Load("Grey", typeof(Material)) as Material;
       // meshRenderer.enabled = false;
        quad.AddComponent<MeshCollider>();
        quad.transform.parent = transform;
        quad.layer = LayerMask.NameToLayer("HouseFeature");
        quad.AddComponent<QuadData>();

        quads.Add(quad);
        RandomlyPlace(quad);
    }  


    void PlaceQuads() //unused
    {
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        float brickSize = GetComponent<StretchQuads>().brickSize;
        //create unique points
        for (int i = 0; i < quads.Count; i++)
        {
            //work how to place quad on a 0.02 grid
            float sizeX = meshCollider.bounds.size.x;
            Debug.Log("SizeX" + sizeX);
            float sizeZ = meshCollider.bounds.size.z;
            Debug.Log("SizeZ" + sizeZ);
            float amountX = sizeX / (brickSize*2);
            Debug.Log(amountX + "AMTX");
            float amountZ = sizeZ / (brickSize*2);
            Debug.Log(amountZ + "AMTZ");

            int randomX = Random.Range(0, (int)amountX);
            while(randomX % 2 !=0)// || randomX == amountX/2)
                    randomX = Random.Range(0, (int)amountX);
            Debug.Log("RDNMX" + randomX);

            int randomZ = Random.Range(0, (int)amountZ);
            while(randomZ % 2 != 0)
                randomZ = Random.Range(0, (int)amountZ);

            Debug.Log("RDNMZ" + randomZ);
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale *= 0.01f;

            float x = meshCollider.bounds.center.x - (meshCollider.bounds.size.x * 0.5f);
            x += randomX * brickSize * 2;
            x *= 10f;
            x = Mathf.Round(x);
            x *= 0.1f;
            float y = meshCollider.bounds.center.y;
            float z = meshCollider.bounds.center.z - (meshCollider.bounds.size.z * 0.5f);
            z += randomZ * brickSize * 2;

            Vector3 pos = new Vector3(x, y, z);            
            cube.transform.parent = transform;
            cube.transform.localPosition = pos;
            
        }
    }

    void RandomlyPlace(GameObject quad)
    {
        //randomly place
        //   int intX = (int)x;
        //   int intZ = (int)z;

        MeshCollider meshCollider = transform.GetChild(0).GetComponent<MeshCollider>();
        float x = meshCollider.bounds.size.x/2;
        float z = meshCollider.bounds.size.z/2;
        //    float z = GetComponent<MeshCollider>().bounds.size.z;
        //     x *= 0.5f;
        //    z *= 0.5f;
        float randomX = Random.Range(-x*0.4f, x*0.4f);///if we make it just belowe half, the quads
        float randomZ = Random.Range(-z*0.4f, z*0.4f);///are not palced on the edge of the collider

        float modX = randomX;// * 0.2f;
        float modZ = randomZ;// * 0.2f;

        //round to 1 dp
        modX *= 10;
        modX = Mathf.Round(modX);
        modX *= 0.1f;
        modZ *= 10;
        modZ = Mathf.Round(modZ);
        modZ *= 0.1f;
        Vector3 pos = new Vector3(transform.position.x + modX, transform.position.y, transform.position.z + modZ);

        






        //check to see if this position has already been used
  //      GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
   //     cube.transform.localScale *= 0.1f;
   //     cube.transform.position = pos + transform.position;
        //if our list does not contain pos
        if (!quadSpawnPositions.Contains( transform.position + pos))
        {
            //add
            quadSpawnPositions.Add(transform.position + pos);
            quad.transform.position += pos;
       }
        else 
        {
            //restart this function
            Debug.Log("Re placing - Floor Plan Quads");
            RandomlyPlace(quad);
        }
        
    }

    /// <summary>
    /// Places room in order from foyer to livingroom, to kitchen etc//unused
    /// </summary>
    void PlaceRoomsInOrder()
    {
        //variables for clamping

        //the length of the cube's side is the minimum distance to stop it overlapping
        float minimumDistance = m_Length;
        //move it out half a quad's length
        minimumDistance += m_Length * 0.5f;
        //the length of the box collider is out maximum
        float maximumDistance = transform.localScale.x * 0.5f;
        //move it in half aquad's size so it has space to spawn
        maximumDistance -= m_Length * 0.5f;


        //Place first room, this room will have the front door. We Will call this the foyer
        for (int i = 0; i < amountOfRooms; i++)
        {

            //place foyer
            if (i == 0)
            {
                //grab object from this object, each quad has been childed in createQaud()
                GameObject quad = transform.GetChild(i).gameObject;
                quad.transform.name = "Foyer";
                //move the quad to the top of the allocated area
                quad.transform.position = transform.position + (transform.localScale.z * 0.5f * Vector3.forward);
                //move it back half a quad size so it fits in the box collider of the house
                quad.transform.position += Vector3.back * m_Length * 0.5f;
            }

            //place living room
            if (i == 1)
            {
                //grab our child to make the living room
                GameObject quad = transform.GetChild(i).gameObject;
                quad.transform.name = "LivingRoom";

                //grab foyer
                GameObject foyer = transform.GetChild(i - 1).gameObject;

                //randomly create a positon around the foyer, clamping it so it does not overlap or go above/beyond
                //the house's box collider                

                Vector3 randomV3 = new Vector3(Random.Range(0, 1f), 0f, Random.Range(0, 1f));
                randomV3.x = Mathf.Clamp(randomV3.x, minimumDistance, maximumDistance);
                randomV3.z = Mathf.Clamp(randomV3.z, minimumDistance, maximumDistance);

                //this vector is clamped to be positive

                //set the position on our quad
                quad.transform.position = randomV3;
            }

            //place kitchen
            if (i == 2)
            {
                //grab our child to make the living room
                GameObject quad = transform.GetChild(i).gameObject;
                quad.transform.name = "Kitchen";

                //randomly create a positon around the foyer, clamping it so it does not overlap or go above/beyond
                //the house's box collider                

                Vector3 randomV3 = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
                randomV3.x = Mathf.Clamp(randomV3.x, minimumDistance, maximumDistance);
                randomV3.z = Mathf.Clamp(randomV3.z, minimumDistance, maximumDistance);

                //set the position on our quad
                quad.transform.position = randomV3;
            }


        }        
    }
}

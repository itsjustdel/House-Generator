using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floors : MonoBehaviour
{

    bool meshHasBeenSubdivided = false;
    public static GameObject Carpet(GameObject gameObject, int subDivisions, float randomisation,Divide divide)
    {

        MeshFilter MF = gameObject.GetComponent<MeshFilter>();
        Mesh originalMesh = MF.mesh;

        GameObject carpetedQuad = new GameObject();





        return carpetedQuad;
    

        //////house is moved and rotated and carpet is worng place **TODO- sort 

        carpetedQuad.transform.position = gameObject.transform.position;
        carpetedQuad.transform.parent = gameObject.transform;
        MeshFilter carpetedFilter = carpetedQuad.AddComponent<MeshFilter>();
        MeshRenderer carpetedRenderer = carpetedQuad.AddComponent<MeshRenderer>();
        carpetedRenderer.material = Resources.Load("Door") as Material;

        //copy mesh
        Mesh subMesh = new Mesh();
        subMesh.vertices = originalMesh.vertices;
        subMesh.triangles = originalMesh.triangles;
        subMesh.normals = originalMesh.normals;

        //send this object's mesh, how many subdivisions, and this gameObject to subdivision script
        

        //create instance of Mesh helper class

        MeshHelper meshHelper = new MeshHelper();
        //send this object's mesh, how many subdivisions, and this gameObject to subdivision script
        SubdivideMesh sdm = gameObject.AddComponent<SubdivideMesh>();
        sdm.StartCoroutine(meshHelper.Subdivide(subMesh, subDivisions, carpetedQuad, false)); //96 max  // divides a single quad into 6x6 quads // normally 32

        sdm.StartCoroutine(sdm.WaitForMeshThenRandomise(carpetedQuad, 0.05f, false));

        carpetedFilter.mesh = subMesh;

        gameObject.GetComponent<MeshRenderer>().enabled = false;
        
        //disable quad's mesh - keep in case we need the simple outline
        ///quad.GetComponent<MeshRenderer>().enabled = false;
        divide.interiorAssetsByRoom.Add(new List<GameObject>() { carpetedQuad });
        return carpetedQuad;
    }

    public static GameObject Tiled(GameObject gameObject, float tileSizeX, float tileSizeZ,float grouting,Divide divide)
    {
        //nneds to change to vertices and mesh instead of icreating lots of cubes

        GameObject tiledFloor = new GameObject();
        tiledFloor.transform.parent = gameObject.transform;
        tiledFloor.name = "Tiled Floor";
        tiledFloor.transform.position = gameObject.transform.position;

        float xSize = gameObject.GetComponent<MeshRenderer>().bounds.size.x;
        float zSize = gameObject.GetComponent<MeshRenderer>().bounds.size.z;
        Vector3 start = gameObject.GetComponent<MeshRenderer>().bounds.center;
        start.x -= xSize * 0.5f;
        start.z -= zSize * 0.5f;

        float stepSize = xSize / tileSizeX;
        //tile size is always un der 1, so we divide
        int patternStepX = Random.Range(1, 6);
        int patternStepZ = Random.Range(1, 6);
        int stepX = 1;
        int stepZ = 1;

        PrimitiveType pType = PrimitiveType.Cube;
       // if (Random.Range(0, 6) == 0)
        //    pType = PrimitiveType.Sphere;//doesnt work because sphere is tretched, dont notice when square is stretched
        int patternType = Random.Range(0, 2);

        for ( float i = 0; i < xSize; i+=tileSizeX)
        {
            for (float j = 0; j < zSize; j+=tileSizeZ)
            {
                stepZ++;
                stepX++;
                //place tile
                GameObject tile = GameObject.CreatePrimitive(pType);
                Destroy(tile.GetComponent<BoxCollider>());

                tile.transform.localScale = new Vector3(Mathf.Abs(tileSizeX-grouting), 0.1f, Mathf.Abs(tileSizeZ-grouting));
                tile.transform.position = start + (gameObject.transform.right* i) + (j *gameObject.transform.forward);
                tile.transform.position += gameObject.transform.right * tileSizeX*0.5f;
                tile.transform.position += gameObject.transform.forward * tileSizeZ*0.5f;
                tile.transform.parent = tiledFloor.transform;
                //catch last row and change tile scale and position
                if (i > xSize - (tileSizeX) && j > zSize - (tileSizeZ))
                {
                    //tile.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;
                    tile.transform.position = start + gameObject.transform.right * (i + ((xSize - i) / 2)) + ((j + (zSize - j) / 2) * gameObject.transform.forward);
                    tile.transform.localScale = new Vector3(Mathf.Abs((xSize-i))-grouting, 0.1f,Mathf.Abs (zSize - j)- grouting);

                }
                else if ( i > xSize-(tileSizeX))
                {
                    //tile.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                   
                    tile.transform.position = start + gameObject.transform.right * (i + ((xSize-i)/2)) + (j * gameObject.transform.forward);
                    tile.transform.position += gameObject.transform.forward * tileSizeZ * 0.5f;
                    tile.transform.localScale = new Vector3(Mathf.Abs((xSize - i) - grouting),0.1f,Mathf.Abs(tileSizeZ - grouting));

                }
                else if (j > zSize - (tileSizeZ))
                {
                    tile.transform.position = start + gameObject.transform.right * (i) + ((j + (zSize - j) / 2) * gameObject.transform.forward);
                    
                    tile.transform.localScale = new Vector3(Mathf.Abs( tileSizeX-grouting), 0.1f, Mathf.Abs((zSize - j)- grouting));
                    tile.transform.position += gameObject.transform.right * tileSizeX * 0.5f;
                }
                
                if (patternType == 0)
                {
                    if (stepX % patternStepX == 0)
                    {
                        tile.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;
                        stepX++;
                    }

                    if (stepZ % patternStepZ == 0)
                    {
                        tile.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;
                        stepZ++;
                    }
                }
                else
                    tile.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;

                
            }
           
        }

        //add to LOD list 
        divide.interiorAssetsByRoom.Add(new List<GameObject>() { tiledFloor });

        //fill in last row

        return tiledFloor;
    }

    public static GameObject FloorBoards(GameObject gameObject, float tileSizeX, float tileSizeZ, float grouting,float variance,Divide divide)
    {
        GameObject floorBoardedFloor = new GameObject();
        floorBoardedFloor.transform.parent = gameObject.transform;
        floorBoardedFloor.name = "FloorBoarded Floor";
        floorBoardedFloor.transform.position = gameObject.transform.position;

        float xSize = gameObject.GetComponent<MeshRenderer>().bounds.size.x;
        if (tileSizeX > xSize*0.5f)
            tileSizeX = xSize*0.5f;
        float zSize = gameObject.GetComponent<MeshRenderer>().bounds.size.z;
        if (tileSizeZ > zSize * 0.5f)
            tileSizeZ = zSize * 0.5f;

        Vector3 start = gameObject.GetComponent<MeshRenderer>().bounds.center;
        start.x -= xSize * 0.5f;
        start.z -= zSize * 0.5f;

        float stepSize = xSize / tileSizeX;
        //tile size is always un der 1, so we divide
        int patternStepX = Random.Range(1, 6);
        int patternStepZ = Random.Range(1, 6);
        int stepX = 1;
        int stepZ = 1;

        PrimitiveType pType = PrimitiveType.Cube;
        // if (Random.Range(0, 6) == 0)
        //    pType = PrimitiveType.Sphere;//doesnt work because sphere is tretched, dont notice when square is stretched
        int patternType = Random.Range(0, 2);
        float random = 0;
        
        //for pulling vertice
        int[] rightVertices = HouseBuilder.VerticeArray("right");
        
        for (float j = 0; j < zSize; j += tileSizeZ)
        {
            float prevI = 0;
            //random = 0;
            for (float i = 0; i < xSize; i += tileSizeX + random)
            {

                float tempZSize = tileSizeZ - grouting;
                if (j > zSize - tileSizeZ)
                {
                    tempZSize = (zSize - j - grouting);
                }

                if (i > 0)
                {

                    stepZ++;
                    stepX++;
                    //place tile
                    GameObject tile = GameObject.CreatePrimitive(pType);
                    Destroy(tile.GetComponent<BoxCollider>());


                    Vector3[] vertices = tile.GetComponent<MeshFilter>().mesh.vertices;

                    //working at mesh level
                    for (int x = 0; x < vertices.Length; x++)
                    {


                        //move over
                        vertices[x].x += 0.5f;
                        vertices[x].z += 0.5f;

                        //scale
                        vertices[x].y *= 0.1f;
                        vertices[x].z *= tempZSize;// (tileSizeZ- grouting);




                    }
                    for (int y = 0; y < rightVertices.Length; y++)
                    {
                        vertices[rightVertices[y]].x = (tileSizeX + random) - grouting;
                        //vertices[rightVertices[y]].y += 0.1f;

                    }

                    tile.GetComponent<MeshFilter>().mesh.vertices = vertices;
                    tile.GetComponent<MeshFilter>().mesh.RecalculateBounds();

                    //tile.transform.localScale = new Vector3(1, 0.1f, 1);
                    tile.transform.position = start + (gameObject.transform.right * prevI) + (j * gameObject.transform.forward);
                    //tile.transform.position += gameObject.transform.right * tileSizeX * 0.5f;
                    //tile.transform.position += gameObject.transform.forward * tileSizeZ * 0.5f;
                    //tile.transform.parent = floorBoardedFloor.transform;
                    //catch last row and change tile scale and position

                    //rotate
                    tile.GetComponent<MeshFilter>().mesh.RecalculateBounds();
                    //put tile in pivot gamobject then rotate - will need to move this all to transforms for optimastion so we dont spawn a million gameobjects
                    GameObject pivot = new GameObject();
                    pivot.transform.position = tile.GetComponent<MeshRenderer>().bounds.center;
                    tile.transform.parent = pivot.transform;
                    pivot.transform.parent = gameObject.transform;
                    float rotRange = 1f;
                    pivot.transform.rotation *= Quaternion.Euler(Random.Range(-rotRange, rotRange), Random.Range(-rotRange, rotRange), Random.Range(-rotRange, rotRange));

                    //material 
                    tile.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;
                    int matChooser = Random.Range(0, 3);
                    if (matChooser == 1)
                        tile.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown2") as Material;
                    if (matChooser == 2)
                        tile.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown3") as Material;
                }

                //check if we are finishing the row
                
                if (xSize - i < tileSizeX+variance)
                {
                    //build the last tile.plank to the end
                   // tile.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                    GameObject tileExtra = GameObject.CreatePrimitive(pType);
                    Vector3[] vertices = tileExtra.GetComponent<MeshFilter>().mesh.vertices;
                    
                    //working at mesh level
                    for (int x = 0; x < vertices.Length; x++)
                    {


                        //move over
                        vertices[x].x += 0.5f;
                        vertices[x].z += 0.5f;

                        //scale
                        vertices[x].y *= 0.1f;
                        vertices[x].z *= tempZSize;// (tileSizeZ - grouting);




                    }
                    for (int y = 0; y < rightVertices.Length; y++)
                    {
                        vertices[rightVertices[y]].x = (xSize - i) - grouting;
                        //vertices[rightVertices[y]].y += 0.1f;

                    }

                    tileExtra.GetComponent<MeshFilter>().mesh.vertices = vertices;


                    //tile.transform.localScale = new Vector3(1, 0.1f, 1);
                    tileExtra.transform.position = start + (gameObject.transform.right * i) + (j * gameObject.transform.forward);
                   

                    //rotate
                    tileExtra.GetComponent<MeshFilter>().mesh.RecalculateBounds();
                    //put tile in pivot gamobject then rotate - will need to move this all to transforms for optimastion so we dont spawn a million gameobjects
                    GameObject pivotExtra = new GameObject();
                    pivotExtra.name = "extra";
                    pivotExtra.transform.parent = gameObject.transform;
                    pivotExtra.transform.position = tileExtra.GetComponent<MeshRenderer>().bounds.center;
                    tileExtra.transform.parent = pivotExtra.transform;
                    float rotRangeExtra = 1f;
                    pivotExtra.transform.rotation *= Quaternion.Euler(Random.Range(-rotRangeExtra, rotRangeExtra), Random.Range(-rotRangeExtra, rotRangeExtra), Random.Range(-rotRangeExtra, rotRangeExtra));

                    //material 
                    tileExtra.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;
                    int matChooseExtra = Random.Range(0, 3);
                    if (matChooseExtra == 1)
                        tileExtra.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown2") as Material;
                    if (matChooseExtra == 2)
                        tileExtra.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown3") as Material;
                    //tileExtra.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                    //force end of row
                    i = Mathf.Infinity;

                }



                prevI = i;
                random = Random.Range(0, variance);
                
            }
            
        }
        divide.interiorAssetsByRoom.Add(new List<GameObject>() { floorBoardedFloor });

        //fill in last row

        return floorBoardedFloor;
    }
}

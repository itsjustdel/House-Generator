using UnityEngine;
using System.Collections;

//adds doors/windows etc to house
public class AddFeaturesToHouse : MonoBehaviour {

    public GameObject cubePrefab;
    public SplitCube splitCube;
    private float layerSize;
    private float brickSize;
	void Start () {
        
        layerSize = (transform.localScale.y / splitCube.divisionAmount);
        brickSize = layerSize / 10;

        Door();
        Windows();
        StartCoroutine("Walls");
      //  StartCoroutine("Roof");
        //StartCoroutine("EnableCombineScripts");
    }

    /// <summary>
    /// Finds a spot for the door
    /// </summary>
    void Door()
    {      
        //find the front of the building
        Vector3 outFront = transform.position + (Vector3.forward * transform.localScale.z);
        //move it to the bottom layer
        outFront += (transform.localScale.y*0.5f) * Vector3.down;
        //move it to the middle of the bottom layer
        outFront += brickSize*5 * Vector3.up;

        //shoot a ray and look for a wall, then place a this point
        RaycastHit hit;
        LayerMask lm = LayerMask.GetMask("House");
        if (Physics.Raycast(outFront, Vector3.back,out hit, transform.localScale.z * 2, lm))
        {
            GameObject cube = Instantiate(cubePrefab, hit.point, Quaternion.identity) as GameObject;
            cube.name = "Door";
            cube.transform.parent = this.transform;
            cube.layer = 25;
                       
            
            Vector3 newPos = new Vector3(hit.point.x, hit.point.y  , hit.point.z);
            cube.transform.position = newPos;
            //slim the door size
            cube.transform.localScale = new Vector3(brickSize*6, brickSize * 8, 0.01f);
            cube.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("Blue", typeof(Material)) as Material;
        }
            
    }

    /// <summary>
    /// Looks for suitable spots for windows and places them
    /// </summary>
    void Windows()
    {       
                        
        Vector3 outRight = transform.position + (Vector3.right * transform.localScale.x);
        //go to the bottom of the house
        outRight += (transform.localScale.y * 0.5f) * Vector3.down;
        //move it two thirds of a layer up /6 bricks
        outRight += 6*brickSize * Vector3.up;

        //shoot a ray and look for a wall, then place a this point
        RaycastHit hit;
        LayerMask lm = LayerMask.GetMask("House");
        if (Physics.Raycast(outRight, -Vector3.right, out hit, transform.localScale.x * 2, lm))
        {
            GameObject cube = Instantiate(cubePrefab, hit.point, Quaternion.identity) as GameObject;
            cube.name = "Window";
            cube.transform.parent = this.transform;
            cube.layer = 25;
            
            Vector3 newPos = new Vector3(hit.point.x, hit.point.y, hit.point.z);
            cube.transform.position = newPos;
            //slim the window size
            cube.transform.localScale = new Vector3(0.01f, brickSize*6, layerSize);
            cube.transform.GetComponent<Renderer>().sharedMaterial = Resources.Load("Glass", typeof(Material)) as Material;
        }
    }

    /// <summary>
    /// creates wall pieces which embed the other features, doors, windows etc
    /// </summary>
    IEnumerator Walls()
    {
       //we need to wait until the features have been instantiated
       yield return new WaitForEndOfFrame();

       StartCoroutine("WallLeft");
    //   StartCoroutine("WallRight");
    //   StartCoroutine("WallFront"); //Daisy chaining these to make sure the wall are built before combing them
    //   StartCoroutine("WallBack");

       yield break;
       
    }
    IEnumerator WallLeft()
    {

        //everything is built on 10 x 10 grid, so raycast through these grids from each side of the house and place a brick
        //if we hit a house wall, e.g not a feature(door/window etc)

        //find our bottom left coordinate for this side
        //this is half the length of the house plus its transform position
        Vector3 bottomLeft = transform.position + (transform.localScale.z * 0.5f * Vector3.back);
        //we need to move it half a brick inwards to line it up
        bottomLeft += brickSize * 0.5f * Vector3.forward;
        bottomLeft += brickSize * 0.5f * Vector3.left;
        bottomLeft += brickSize * 0.5f * Vector3.up;
        //now move it to the side of the house
        bottomLeft += transform.localScale.z * 0.5f * Vector3.right;
        //now move it to the bottom - we have our starting point! phew!
        bottomLeft += transform.localScale.z * 0.5f * Vector3.down;


        //from this position, shoot rays along the wall, shoot from each division point, looking for interior walls as well as exterior

        //new Gameobject for the wall to go in
        GameObject wallLeft = new GameObject();
        wallLeft.transform.parent = this.transform;
        wallLeft.transform.position = transform.position;
        wallLeft.name = "WallLeft";
        //add combine script for performance
        CombineChildren cc = wallLeft.AddComponent<CombineChildren>();
        cc.enabled = false;
        cc.disableColliders = true;
        

        float amountOfBricksX = splitCube.divisionAmount * 10;
        float amountOfBricksY = splitCube.floors * 10;
        Vector3 brickSizeFwd = brickSize * Vector3.forward;
        Vector3 brickSizeUp = brickSize * Vector3.up;
        Vector3 layerLeft = layerSize * Vector3.left;


        for (int h = 0; h < splitCube.divisionAmount; h++)
        {
            for (int i = 0; i < amountOfBricksY; i++)
            {
                for (int j = 0; j < amountOfBricksX; j++)
                {
                    Vector3 shootFrom = bottomLeft + (layerLeft * h) + (brickSizeFwd * j) + (brickSizeUp * i);
                    //move it out from the wall a little
                    shootFrom += Vector3.right * brickSize * 2;

                    LayerMask lm = LayerMask.GetMask("House", "HouseFeature");
                    RaycastHit hit;
                    if (Physics.Raycast(shootFrom, Vector3.left, out hit, brickSize * 4, lm))
                    {
                        //miss out any 'House features' = they are on layer 25
                        if (hit.transform.gameObject.layer == 25)
                            continue;

                        //otherwise
                        GameObject brick = Instantiate(cubePrefab, hit.point, Quaternion.identity) as GameObject;
                        brick.transform.localScale *= brickSize;
                        brick.name = "Brick";
                        brick.transform.parent = wallLeft.transform;

                    }
                    
                }
                yield return new WaitForFixedUpdate();
            }
        }
        StartCoroutine("WallRight");
        yield break;
    }
    IEnumerator WallRight()
    {

        //everything is built on 10 x 10 grid, so raycast through these grids from each side of the house and place a brick
        //if we hit a house wall, e.g not a feature(door/window etc)

        //find our bottom left coordinate for this side
        //this is half the length of the house plus its transform position
        Vector3 bottomRight = transform.position + (transform.localScale.z * 0.5f * Vector3.back);
        //we need to move it half a brick inwards to line it up
        bottomRight += brickSize * 0.5f * Vector3.forward;
        bottomRight += brickSize * 0.5f * Vector3.right;
        bottomRight += brickSize * 0.5f * Vector3.up;
        //now move it to the side of the house
        bottomRight += transform.localScale.z * 0.5f * Vector3.left;
        //now move it to the bottom - we have our starting point! phew!
        bottomRight += transform.localScale.z * 0.5f * Vector3.down;

        //new Gameobject for the wall to go in
        GameObject wallRight = new GameObject();
        wallRight.transform.parent = this.transform;
        wallRight.transform.position = transform.position;
        wallRight.name = "WallRight";
        //add combine script for performance
        CombineChildren cc = wallRight.AddComponent<CombineChildren>();
        cc.enabled = false;
        cc.disableColliders = true;


        //from this position, shoot rays along the wall
        float amountOfBricksX = splitCube.divisionAmount * 10;
        float amountOfBricksY = splitCube.floors * 10;
        Vector3 brickSizeFwd = brickSize * Vector3.forward;
        Vector3 brickSizeUp = brickSize * Vector3.up;
        Vector3 layerRight = layerSize * Vector3.right;

        for (int h = 0; h < splitCube.divisionAmount; h++)
        {
            for (int i = 0; i < amountOfBricksY; i++)
            {
                for (int j = 0; j < amountOfBricksX; j++)
                {
                    Vector3 shootFrom = bottomRight + (layerRight * h) + (brickSizeFwd * j) + (brickSizeUp * i);
                    //move it out from the wall a little
                    shootFrom += Vector3.left * brickSize * 2;

                    LayerMask lm = LayerMask.GetMask("House", "HouseFeature");
                    RaycastHit hit;
                    if (Physics.Raycast(shootFrom, Vector3.right, out hit, brickSize * 4, lm))
                    {
                        //miss out any 'House features' = they are on layer 25
                        if (hit.transform.gameObject.layer == 25)
                            continue;

                        //otherwise
                        GameObject brick = Instantiate(cubePrefab, hit.point, Quaternion.identity) as GameObject;
                        brick.transform.localScale *= brickSize;
                        brick.name = "Brick";
                        brick.transform.parent = wallRight.transform;


                    }
                }
                yield return new WaitForFixedUpdate();
            }
        }
        StartCoroutine("WallFront");
        yield break;
    }
    IEnumerator WallFront()
    {

        //everything is built on 10 x 10 grid, so raycast through these grids from each side of the house and place a brick
        //if we hit a house wall, e.g not a feature(door/window etc)

        //find our bottom left coordinate for this side
        //this is half the length of the house plus its transform position
        Vector3 bottomLeft = transform.position + (transform.localScale.z * 0.5f * Vector3.forward);
        //we need to move it half a brick inwards to line it up
        bottomLeft += brickSize * 0.5f * Vector3.forward;
        bottomLeft += brickSize * 0.5f * Vector3.left;
        bottomLeft += brickSize * 0.5f * Vector3.up;
        //now move it to the side of the house
        bottomLeft += transform.localScale.z * 0.5f * Vector3.right;
        //now move it to the bottom - we have our starting point! phew!
        bottomLeft += transform.localScale.z * 0.5f * Vector3.down;

        //new Gameobject for the wall to go in
        GameObject wallFront = new GameObject();
        wallFront.transform.parent = this.transform;
        wallFront.transform.position = transform.position;
        wallFront.name = "WallFront";
        //add combine script for performance
        CombineChildren cc = wallFront.AddComponent<CombineChildren>();
        cc.enabled = false;
        cc.disableColliders = true;

        //from this position, shoot rays along the wall
        float amountOfBricksX = splitCube.divisionAmount * 10;
        float amountOfBricksY = splitCube.floors * 10;
        Vector3 brickSizeFwd = brickSize * Vector3.left;
        Vector3 brickSizeUp = brickSize * Vector3.up;
        Vector3 layerBack = layerSize * Vector3.back;

        for (int h = 0; h < splitCube.divisionAmount; h++)
        {
            for (int i = 0; i < amountOfBricksY; i++)
            {
                for (int j = 0; j < amountOfBricksX; j++)
                {
                    Vector3 shootFrom = bottomLeft + (layerBack * h) + (brickSizeFwd * j) + (brickSizeUp * i);
                    //move it out from the wall a little
                    shootFrom += Vector3.forward * brickSize * 2;

                    LayerMask lm = LayerMask.GetMask("House", "HouseFeature");
                    RaycastHit hit;
                    if (Physics.Raycast(shootFrom, Vector3.back, out hit, brickSize * 4, lm))
                    {
                        //miss out any 'House features' = they are on layer 25
                        if (hit.transform.gameObject.layer == 25)
                            continue;

                        //otherwise
                        GameObject brick = Instantiate(cubePrefab, hit.point, Quaternion.identity) as GameObject;
                        brick.transform.localScale *= brickSize;
                        brick.name = "Brick";
                        brick.transform.parent = wallFront.transform;

                        
                    }
                }
            }
            yield return new WaitForFixedUpdate();
        }
        StartCoroutine("WallBack");
        yield break;
    }
    IEnumerator WallBack()
    {

        //everything is built on 10 x 10 grid, so raycast through these grids from each side of the house and place a brick
        //if we hit a house wall, e.g not a feature(door/window etc)

        //find our bottom left coordinate for this side
        //this is half the length of the house plus its transform position
        Vector3 bottomLeft = transform.position + (transform.localScale.z * 0.5f * Vector3.back);
        //we need to move it half a brick inwards to line it up
        bottomLeft += brickSize * 0.5f * Vector3.forward;
        bottomLeft += brickSize * 0.5f * Vector3.left;
        bottomLeft += brickSize * 0.5f * Vector3.up;
        //now move it to the side of the house
        bottomLeft += transform.localScale.z * 0.5f * Vector3.right;
        //now move it to the bottom - we have our starting point! phew!
        bottomLeft += transform.localScale.z * 0.5f * Vector3.down;

        //new Gameobject for the wall to go in
        GameObject wallBack = new GameObject();
        wallBack.transform.parent = this.transform;
        wallBack.transform.position = transform.position;
        wallBack.name = "WallBack";
        //add combine script for performance
        CombineChildren cc = wallBack.AddComponent<CombineChildren>();
        cc.enabled = false;
        cc.disableColliders = true;

        //from this position, shoot rays along the wall
        float amountOfBricksX = splitCube.divisionAmount * 10;
        float amountOfBricksY = splitCube.floors * 10;
        Vector3 brickSizeFwd = brickSize * Vector3.left;
        Vector3 brickSizeUp = brickSize * Vector3.up;
        Vector3 layerFwd = layerSize * Vector3.forward;

        for (int h = 0; h < splitCube.divisionAmount; h++)
        {
            for (int i = 0; i < amountOfBricksY; i++)
            {
                for (int j = 0; j < amountOfBricksX; j++)
                {
                    Vector3 shootFrom = bottomLeft + (layerFwd * h) + (brickSizeFwd * j) + (brickSizeUp * i);
                    //move it out from the wall a little
                    shootFrom += Vector3.back * brickSize * 2;

                    LayerMask lm = LayerMask.GetMask("House", "HouseFeature");
                    RaycastHit hit;
                    if (Physics.Raycast(shootFrom, Vector3.forward, out hit, brickSize * 4, lm))
                    {
                        //miss out any 'House features' = they are on layer 25
                        if (hit.transform.gameObject.layer == 25)
                            continue;

                        //otherwise
                        GameObject brick = Instantiate(cubePrefab, hit.point, Quaternion.identity) as GameObject;
                        brick.transform.localScale *= brickSize;
                        brick.name = "Brick";
                        brick.transform.parent = wallBack.transform;                        
                    }
                }
            }
            yield return new WaitForFixedUpdate();
        }
        StartCoroutine("Roof");
        yield break;
    }
    IEnumerator Roof()
    {
        //everything is built on 10 x 10 grid, so raycast through these grids from each side of the house and place a brick
        //if we hit a house wall, e.g not a feature(door/window etc)

        //find our top left coordinate for this side
        //this is half the length of the house plus its transform position
        Vector3 bottomLeft = transform.position + (transform.localScale.z * 0.5f * Vector3.back);
        //we need to move it half a brick inwards to line it up
        bottomLeft += brickSize * 0.5f * Vector3.forward;
        bottomLeft += brickSize * 0.5f * Vector3.left;
        bottomLeft += brickSize * 0.5f * Vector3.up;
        //now move it to the side of the house
        bottomLeft += transform.localScale.z * 0.5f * Vector3.right;
        //now move it to the bottom - we have our starting point! phew!
        bottomLeft += transform.localScale.z * 0.5f * Vector3.up;

        //new Gameobject for the wall to go in
        GameObject roof = new GameObject();
        roof.transform.parent = this.transform;
        roof.transform.position = transform.position;
        roof.name = "Roof";
        //add combine script for performance
        CombineChildren cc = roof.AddComponent<CombineChildren>();
        cc.enabled = false;
        cc.disableColliders = true;

        //from this position, shoot rays along the wall
        float amountOfBricksX = splitCube.divisionAmount * 10;
        float amountOfBricksZ = splitCube.divisionAmount * 10;
        Vector3 brickSizeFwd = brickSize * Vector3.forward;
        Vector3 brickSizeLeft = brickSize * Vector3.left;
        for (int i = 0; i < amountOfBricksZ; i++)
        {
            for (int j = 0; j < amountOfBricksX; j++)
            {
                Vector3 shootFrom = bottomLeft + (brickSizeFwd * j) + (brickSizeLeft * i);
                //move it up a little
                shootFrom += Vector3.up * transform.lossyScale.y * 0.5f;

                LayerMask lm = LayerMask.GetMask("House", "HouseFeature");
                RaycastHit hit;
                if (Physics.Raycast(shootFrom, Vector3.down, out hit, transform.localScale.y * 2, lm))
                {
                    //miss out any 'House features' = they are on layer 25
                    if (hit.transform.gameObject.layer == 25)
                        continue;

                    //otherwise
                    GameObject brick = Instantiate(cubePrefab, hit.point, Quaternion.identity) as GameObject;
                    brick.transform.localScale *= brickSize;
                    brick.name = "Brick";
                    brick.transform.parent = roof.transform;

                    
                }
            }
            yield return new WaitForFixedUpdate();
        }

        StartCoroutine("EnableCombineScripts");
    yield break;
    }

    IEnumerator EnableCombineScripts()
    {
        yield return new WaitForEndOfFrame();
        CombineChildren[] scripts = gameObject.GetComponentsInChildren<CombineChildren>();
        foreach(CombineChildren script in scripts)
        {
            script.enabled = true;
        }
    }
}

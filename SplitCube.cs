using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Splits a cube in to smaller cubes and disables renderer
public class SplitCube : MonoBehaviour {

    public GameObject cubePrefab;
    public int divisionAmount = 5;
  
    public int floors = 1;

	void Start () {
        
        
        float x = transform.localScale.x;
        float y = transform.localScale.y;
        float z = transform.localScale.z;

        //create points inside cube to instantiate new cubes
        //split side in to twice the amount of divisions we want
        List<Vector3> pointsX = new List<Vector3>();
        //create an offset so we start at the left hand side of the cube- we only know the central transform point
        //at the moment
        Vector3 offsetX = new Vector3 (transform.position.x - (x * 0.5f), transform.position.y,transform.position.z);
        Vector3 offsetY = new Vector3(transform.position.x , transform.position.y - (y * 0.5f), transform.position.z);
        Vector3 offsetZ = new Vector3(transform.position.x, transform.position.y , transform.position.z - (z * 0.5f));
        //The proportion at which to move along the x axis each time we place a cube
        Vector3 amountToMoveX = (x / divisionAmount) * Vector3.right;
        Vector3 amountToMoveY = (x / divisionAmount) * Vector3.up;
        Vector3 amountToMoveZ = (x / divisionAmount) * Vector3.forward;
        //adjust offsetX forward half a division amount so the cubes instantiate flush with the side of the cube
        //this needs done because the cube is instantiated from its central point, not its edge.
        offsetX += amountToMoveX * 0.5f;
        offsetY += amountToMoveY * 0.5f;
        offsetZ += amountToMoveZ * 0.5f;
        Vector3 offset = offsetX + offsetY + offsetZ;

        for (int i = 0; i < divisionAmount; i++)
        {
            for (int j = 0; j < floors; j++)
            {
                for (int k = 0; k < divisionAmount; k++)
                {
                    //randomise if block gets added
                    //make it more likely to not gett added the higher we go
                    if (j == 0)
                    {
                        int random = Random.Range(0, 10);
                        if (random == 0)
                            continue;
                    }
                    if (j == 1)
                    {
                        int random = Random.Range(0, 3);
                        if (random == 0)
                            continue;
                    }
                    if (j == 2)
                    {
                        int random = Random.Range(0, 2);
                        if (random == 0)
                            continue;
                    }
                    //add points along the side of the cube - X axis
                    Vector3 position = offset + (amountToMoveX * i) + (amountToMoveY * j) + (amountToMoveZ * k);
                    GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity) as GameObject;
                    Vector3 newScale = cube.transform.localScale;
                    newScale.x = newScale.x / divisionAmount;
                    newScale.y = newScale.y / divisionAmount;
                    newScale.z = newScale.z / divisionAmount;
                    cube.transform.localScale = newScale;
                    cube.layer = 24;
                    cube.GetComponent<Renderer>().enabled = false;
                    cube.transform.parent = this.transform;
                }
            }
        }

     //   GetComponent<AddFeaturesToHouse>().enabled = true;         
    }
	
	
}

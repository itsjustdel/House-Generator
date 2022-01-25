using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class GardenCentre : MonoBehaviour {

    //added to the combined mesh from Combine Children. Combine Children added from Fields

    //this script divides up the plot received from the voronoi in to two plots. One for the garden centre building itself, and one for the outside/surrounding areas
    //the garden centre plot is given BuildGardenCentre.cs to control the creation of the building

    public List<GameObject> cellsForBuilding = new List<GameObject>();
    public Vector3 toRoad;
    // Use this for initialization
    void Start ()
    {
        StartCoroutine("House");
	}
	
    IEnumerator House()
    {
 

        //create a list of valid cells where we can place the house. These don't include border cells

        //so to create the list, dont add the first row beside the road, or any border cells around the edge at the field

        List<GameObject> allCells = new List<GameObject>();
        int totalChildren = transform.parent.childCount;
        //don't check the last child, it is the combined mesh - not an individual cell
        for (int i = 0; i < totalChildren - 1; i++)
        {
            allCells.Add(transform.parent.GetChild(i).gameObject);
        }


        int cellsHit = 0;
        
        Vector3 centre = Vector3.zero;
        RaycastHit hit;
        Vector3 roadHit = Vector3.zero;
        int x = 5;
        int y = 5;
        List<Vector3> corners = new List<Vector3>();


        //if not all corenrs are inside the plot, start again
        while (cellsHit != 4)
        {
            yield return new WaitForEndOfFrame();
         //   Debug.Log("in while");

            //reset all lists if starting again
            cellsHit = 0;
            cellsForBuilding = new List<GameObject>();
            corners = new List<Vector3>();        
          
            //randomly choose a starting point
            int target = Random.Range(0, allCells.Count);
            centre = allCells[target].GetComponent<MeshRenderer>().bounds.center;


            if (Physics.SphereCast(centre + Vector3.up * 100, 100f, Vector3.down, out hit, 200f, LayerMask.GetMask("Road")))
            {
                roadHit = hit.point;
            }
            else
                Debug.Log("no ray hit on garden centre");


         //   GameObject c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
         //   c.transform.position = centre;
         //   c.transform.localScale *= Vector3.Distance(hit.point, centre) *2;

            Vector3 directionToRoad = (hit.point - centre).normalized;
            //save to public 
            toRoad = directionToRoad;

            //raycast in a square, if all points hit a valid cell, we have an acceptable zone,
            //if not, choose another cell

            Vector3 p1 = centre + (directionToRoad * x * 0.5f) + ((Quaternion.Euler(0, 90, 0) * directionToRoad) * y * 0.5f); 
            Vector3 p2 = centre - (directionToRoad * x * 0.5f) + ((Quaternion.Euler(0, 90, 0) * directionToRoad) * y * 0.5f);
            Vector3 p3 = centre + (directionToRoad * x * 0.5f) - ((Quaternion.Euler(0 ,90, 0) * directionToRoad) * y * 0.5f);
            Vector3 p4 = centre - (directionToRoad * x * 0.5f) - ((Quaternion.Euler(0, 90, 0) * directionToRoad) * y * 0.5f);
            corners.Add(p1);
            corners.Add(p2);
            corners.Add(p3);
            corners.Add(p4);

            foreach (Vector3 v3 in corners)
            {
                RaycastHit h;
                if (Physics.Raycast(v3 + Vector3.up * 5, Vector3.down, out h, 10f, LayerMask.GetMask("TerrainCell")))
                {
                    if (h.transform.parent.name == "GardenCentrePlot")
                    {
                        cellsHit++;
             
                    }
                }
            }
       }

        //if we have made it out the while loop, we have successfully found four corners within the pre designated plot for the garden centre building

        //create a rectangle/cube and do a sweeptest, gather all the cells we hit

        GameObject sweepTestObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sweepTestObject.transform.position = centre ;
        sweepTestObject.transform.LookAt(roadHit);
        sweepTestObject.transform.rotation = Quaternion.Euler(0, sweepTestObject.transform.rotation.eulerAngles.y, 0);

        BoxCollider boxC = sweepTestObject.GetComponent<BoxCollider>();
        boxC.size = new Vector3(x, 1, y); 
        //sweepTestObject.transform.localScale = new Vector3(x, 1, y);

        //let the object enter the world

        //run through all cells which make up the whole garden centre area, check if they are within the object's bounds
        //add to list if they are
        

        foreach (GameObject cell in allCells)
        {
            Vector3 c = cell.GetComponent<MeshFilter>().mesh.vertices[0];
            //add centre point automatically, we don't need to test this
            if(PointInOABB(c,boxC))
            {
                cellsForBuilding.Add(cell);
            }
        }


        Destroy(sweepTestObject);

        foreach (GameObject cell in cellsForBuilding)
        {
        //      GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //       cube.transform.position = cell.GetComponent<MeshRenderer>().bounds.center;
        //    cube.transform.localScale *= 0.1f;
        }



        //yield break;

 //       RaycastHit[] hits = Physics.SphereCastAll(centre + Vector3.up * 100, 5f, Vector3.down,200, LayerMask.GetMask("TerrainCell"));

        //add to cells for building
  //      foreach (RaycastHit h in hits)
  //      {
          //  cellsForBuilding.Add(h.transform.gameObject);
  //      }
        //        cellsForBuilding.Add(h.transform.gameObject);

      


        //combine these cell meshes in to a new mesh
        List<CombineInstance> combine = new List<CombineInstance>();
        foreach(GameObject cell in cellsForBuilding)
        {
            CombineInstance ci = new CombineInstance();
            ci.mesh = cell.GetComponent<MeshFilter>().mesh;
            ci.transform = cell.GetComponent<MeshFilter>().transform.localToWorldMatrix;
            combine.Add(ci);

            cell.SetActive(false);

            //save cell centrepoint for possible points of house features
           // centrePointsOfBuildingCells.Add(cell.GetComponent<MeshRenderer>().bounds.center);
        }

        GameObject gardenCentreBuilding = new GameObject();
        gardenCentreBuilding.layer = 24;//house layer
        gardenCentreBuilding.name = "GardenCentreBuildingPlot";
        gardenCentreBuilding.transform.parent = transform.parent;
        MeshFilter mf = gardenCentreBuilding.AddComponent<MeshFilter>();
        mf.mesh.CombineMeshes(combine.ToArray());

        MeshRenderer mr = gardenCentreBuilding.AddComponent<MeshRenderer>();
        mr.sharedMaterial = Resources.Load("Path") as Material;

        MeshCollider mc = gardenCentreBuilding.AddComponent<MeshCollider>();


        //find out if we need to build new meshes
        GameObject previousGardenCentre = GameObject.FindGameObjectWithTag("Code").GetComponent<NewWorld>().gardenCentre;
        if (previousGardenCentre == null)
        {            
            GameObject procHouse = Instantiate(Resources.Load("Prefabs/Housing/ProcHouse") as GameObject, mr.GetComponent<MeshRenderer>().bounds.center, Quaternion.identity) as GameObject;
            procHouse.tag = "GardenCentreBuilding";
            procHouse.transform.parent = gardenCentreBuilding.transform;

            //procHouse.transform.FindChild("Meshes").gameObject.GetComponent<StretchQuads>().enabled = true;

            //transform.parent.FindChild("GardenCentreBuildingPlot").FindChild("ProcHouse(Clone)").FindChild("Meshes").GetComponent<StretchQuads>().enabled = true;   //not the best way to find this..
            //save this house
            GameObject.FindGameObjectWithTag("Code").GetComponent<NewWorld>().gardenCentre = procHouse; //maybe?

        }
        //if a a garden centre has been built 
        else
        {
            Debug.Log("moving garden centre");
            //move the saved centre to this new spot - using mesh renderer bounds centre, asigned above
            previousGardenCentre.transform.position = centre;

          //  Vector3 rotateFrom = previousGardenCentre.transform.FindChild("Meshes").FindChild("Room").transform.position;
            //find rotation to Road
            Physics.SphereCast(centre + (Vector3.up * 200), 100f, Vector3.down, out hit, 200f, LayerMask.GetMask("Road"));

            //previousGardenCentre.transform.LookAt(hit.point);
// Quaternion toRoad = Quaternion.LookRotation(hit.point - centre);

            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = hit.point;

            //use door's rotation to line the building with the road. We need to use the door's rotation because the house vertices are rotated while the transform.rotation is zero

            //**Note//
            //I did this to simplify the vertice stretching code so I didn't need to include +transform.rotation all the time. Most of my procedural mesh cuts out transform/local positions, so that meshes can be created
            //far from vector.zero. ReAlign() can be used to recentre the vertices and the transforms, but I don't call it most of the time. I haven't noticed a difference in performance.

            // Quaternion doorRot = previousGardenCentre.transform.FindChild("Meshes").FindChild("Room").FindChild("Door").gameObject.transform.rotation;
            //keep the y, and zero the others
            //doorRot = Quaternion.Euler(0, doorRot.eulerAngles.y, 0);

            //previousGardenCentre.transform.FindChild("Meshes").transform.rotation *= doorRot; 
            Quaternion toRoad = Quaternion.LookRotation(hit.point - centre);
            toRoad = Quaternion.Euler(0, toRoad.eulerAngles.y, 0);
            previousGardenCentre.transform.Find("Meshes").transform.rotation = toRoad;

        }

    }
    static public bool IsInside(Collider test, Vector3 point)
    {
        Vector3 center;
        Vector3 direction;
        Ray ray;
        RaycastHit hitInfo;
        bool hit;

        // Use collider bounds to get the center of the collider. May be inaccurate
        // for some colliders (i.e. MeshCollider with a 'plane' mesh)
        center = test.bounds.center;

        // Cast a ray from point to center
        direction = center - point;        
        ray = new Ray(point, direction.normalized);
        hit = test.Raycast(ray, out hitInfo, direction.magnitude);

        // If we hit the collider, point is outside. So we return !hit
        return !hit;
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseV2 : MonoBehaviour {
    //this gets attached to the combined mesh created from the voronoi graph which defines the house plot

    //create objects to hold interiors and exteriors so we can switch them on and off (level of detail switcher)
    public GameObject interiorParent;
    public GameObject exteriorParent;
    public bool reportedBack = false;

    // Use this for initialization
    void Start ()
    {

        
        //get size of plot from bounds - we can half to make sure it stays inside plot
        Vector2 plotSize = PlotSize();
        Vector3 center = gameObject.GetComponent<MeshRenderer>().bounds.center;
        GameObject parent = new GameObject();
        parent.transform.parent = gameObject.transform;
        parent.name = "House";

        //create objects to hold interiors and exteriors so we can switch them on and off (level of detail switcher)
        interiorParent = new GameObject();
        interiorParent.name = "Interior Parent";
        interiorParent.transform.parent = parent.transform;
        exteriorParent = new GameObject();
        exteriorParent.name = "Exterior Parent";
        exteriorParent.transform.parent = parent.transform;
        
        //place script to build house, 
        Divide divide = parent.AddComponent<Divide>();

        divide.plot = parent;
        divide.plotX = plotSize.x;
        divide.plotZ = plotSize.y;

        //choose room amount and floor size -, randomised later
        divide.roomAmount = 3;
        divide.floors = 1;

        divide.targetPosition = center;
        //what way is the road?
        //parent.transform.position = center;
        Vector3 dirToRoad = DirectionToRoad();

       

        //once building has finished, merge all objects
        StartCoroutine(WaitAndMerge(parent, center, dirToRoad));

       
    }

    void HeightAdjust()
    {
        //raycast for heighest collision from each corner of the plotmesh
        GameObject house = transform.Find("House").gameObject;
        GameObject plot = house.transform.Find("Plot").gameObject;
        Vector3[] verts = plot.GetComponent<MeshFilter>().mesh.vertices;
        float highY = -Mathf.Infinity;
        foreach(Vector3 v3 in verts)
        {
            RaycastHit hit;
            if(Physics.Raycast(v3 + 100f*Vector3.up + house.transform.position,Vector3.down,out hit,500f,LayerMask.GetMask("HouseCell")))
            {
                if (hit.point.y > highY)
                    highY = hit.point.y;
            }

        }

        if (highY == -Mathf.Infinity)
        {
            Debug.Break();
            Debug.Log("House y position didnt work");
        }
        else
        {

            //adjust to this heighest height
            house.transform.position = new Vector3(house.transform.position.x, highY, house.transform.position.z);
        }
    }

    IEnumerator WaitAndMerge(GameObject parent, Vector3 position, Vector3 lookDirection)
    {
        //we need to wait a frame until the scale has been apllied to some meshes (rounded cube) (bleh)
        yield return new WaitForEndOfFrame();

        //now we've finished combine all the meshes to reduce draw calls
        //currently combing all room interiors in to 1 mesh - is there any need to do room by room? Review performance in house
        CombineChildren ccInt = interiorParent.AddComponent<CombineChildren>();
        ccInt.ignoreDisabledRenderers = true;
        ccInt.addLod = true;

        //exterior
        CombineChildren ccExt = exteriorParent.AddComponent<CombineChildren>();
        ccExt.ignoreDisabledRenderers = true;
        ccExt.addLod = true;

        

        StartCoroutine(WaitAndPlace(parent, position, lookDirection));

        yield return new WaitForEndOfFrame();
        


        
        yield break;
    }

    IEnumerator WaitAndPlace(GameObject parent,Vector3 position,Vector3 lookDirection)
    {
        yield return new WaitForEndOfFrame();

        parent.transform.position = position;
        parent.transform.LookAt(position - lookDirection);
        //flatten so house isnt looking downwards
        parent.transform.rotation = Quaternion.Euler(0, parent.transform.rotation.eulerAngles.y, 0);
        
        HeightAdjust();

        //return to build list
        GameObject.FindGameObjectWithTag("Code").GetComponent<BuildList>().BuildingFinished();
        reportedBack = true;
        yield break;
    }

    Vector2 PlotSize()
    {
        Vector2 plotSize = new Vector2();

        float x = gameObject.GetComponent<MeshRenderer>().bounds.size.x/2;
        float z = gameObject.GetComponent<MeshRenderer>().bounds.size.z/2;
        plotSize = new Vector2(x, z);
        return plotSize;
    }

    Vector3 DirectionToRoad()
    {
        //rotate to face road
        RaycastHit sphereHit;
        Vector3 center = gameObject.GetComponent<MeshRenderer>().bounds.center;

        
        
        //if it doesnt hit at 100 radius, do 200, lower number give better accuracy
        Vector3 p = (center) + (Vector3.up * 200);
        float radius = 1f;
        int safety = 0;
        while (!Physics.SphereCast(p, radius, Vector3.down, out sphereHit, 400f, LayerMask.GetMask("Road")) && safety < 100)
        {
            safety++;
            // Debug.Log(radius + " missed");
            radius += 1f;
        }

        if (safety >= 99)
            Debug.Log("Safery breached here - no road found");

        //GameObject parent = GameObject.CreatePrimitive(PrimitiveType.Sphere);
       // parent.transform.position = sphereHit.point;

        Vector3 dirToRoad = (sphereHit.point - center).normalized;

        return dirToRoad;
    }
}


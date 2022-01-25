using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Reflection;
public class TestSpawner : MonoBehaviour {

    public int roomAmount = 3;
    public float respawnRate = 2f;
    GameObject c;
    bool spawn = true;
    public bool keepRespawning = true;
    public bool doors = false;
    public bool random = false;
    public bool rotate = false;
    public bool slide = false;
    public bool rotateAndSlide = false;

    private float longPosCount = 0;
    private float shortPosCount = 0;

    public bool builtButNotSliding = false;
    // Use this for initialization
    void Start ()
    {
        if(rotateAndSlide)
            StartCoroutine("Activate");
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (spawn &&keepRespawning)
        {
            StartCoroutine("Spawner");
        }

        if (doors)
        {
            for (int i = 0; i < c.transform.childCount; i++)
            {
                for (int j = 0; j < c.transform.GetChild(i).childCount; j++)
                {
                    Destroy(c.transform.GetChild(i).GetChild(j).gameObject);
                }
            }
                

            doors = false;
            c.GetComponent<Divide>().DoorsByRoomSize();
        }

        if(rotate)
        {
            c.transform.rotation *= Quaternion.Euler(0, 0.5f, 0);
        }
        if(slide)
        {
            Vector3[] vertices = c.GetComponent<Divide>().plot.GetComponent<MeshFilter>().mesh.vertices;
            int[] shortestEdge = Divide.ShortestEdge(vertices);
            int[] longestEdge= Divide.LongestEdge(vertices);
            Vector3 longEdgeDir = vertices[longestEdge[1]] - vertices[longestEdge[0]];
            float longLength = longEdgeDir.magnitude;
            
            Vector3 shortEdgeDir = vertices[shortestEdge[1]] - vertices[shortestEdge[0]];
            float shortLength = shortEdgeDir.magnitude;
            //pull apart storeys
            GameObject roof = c.transform.Find("RoofParent").gameObject;

            if (longPosCount < longLength)
            {
                float speed = 0.1f;
                roof.transform.localPosition += longEdgeDir.normalized * speed;
                longPosCount += speed;
            }
            else if (c.GetComponent<Divide>().floors == 2 && shortPosCount < shortLength)
            {
                GameObject firstFloor = c.transform.Find("First Floor").gameObject;
                float speed = 0.1f;
                firstFloor.transform.localPosition += shortEdgeDir.normalized * speed;
                shortPosCount += speed;
            }
            else
                builtButNotSliding = false;
            
        }
	}

    IEnumerator Spawner()
    {
        spawn = false;
        c = new GameObject();
        
        
        Divide d = c.AddComponent<Divide>();
        d.randomAmountOfRooms = false;
        d.roomAmount = 3;// roomAmount;
        yield return new WaitForSeconds(respawnRate);
        if (!keepRespawning)
            yield break;

        Destroy(c);
        ClearLogConsole();
        spawn = true;
        slide = false;
        rotate = false;
        longPosCount = 0;
        shortPosCount= 0;
        if(rotateAndSlide)
            StartCoroutine("Activate");

        yield break;
    }
    IEnumerator Activate()
    {
        yield return new WaitForSeconds(2f);
        slide = true;
        //rotate = true;
        yield break;
    }

    public void Build()
    {
        if (c != null)
            Destroy(c);

        longPosCount = 0;
        shortPosCount = 0;
        slide = false;
        c = new GameObject();
        Divide d = c.AddComponent<Divide>();
        d.randomAmountOfRooms = false;
        d.roomAmount = 3;

        builtButNotSliding = true;

        
    }

    public void StartSlide()
    {
        StartCoroutine("Activate");
    }

    public static void ClearLogConsole()
    {
#if UNITY_EDITOR
        Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
        System.Type logEntries = assembly.GetType("UnityEditorInternal.LogEntries");
        MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
        clearConsoleMethod.Invoke(new object(), null);
#endif
    }
}

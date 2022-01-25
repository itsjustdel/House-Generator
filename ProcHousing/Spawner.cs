
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Reflection;
public class Spawner : MonoBehaviour
{

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
    void Start()
    {
       
        StartCoroutine("SpawnerCo");
    }

    // Update is called once per frame
    void Update()
    {
        if (spawn && keepRespawning)
        {
            StartCoroutine("SpawnerCo");
        }

    }

    IEnumerator SpawnerCo()
    {
        if (c != null)
            Destroy(c);

        spawn = false;
        c = new GameObject();
        Divide d = c.AddComponent<Divide>();
        d.randomAmountOfRooms = false;
        d.roomAmount = 3;

        yield return new WaitForSeconds(respawnRate);
        if (!keepRespawning)
            yield break;

        Destroy(c);        
        ClearLogConsole();
        
        spawn = true;

        yield break;
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

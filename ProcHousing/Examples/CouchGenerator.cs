using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CouchGenerator : MonoBehaviour
{

    public GameObject couch;
    int counter = 0;
    public int timer = 200;
    // Use this for initialization
    void Start()
    {
        couch= GameObject.CreatePrimitive(PrimitiveType.Cube);
        couch.transform.position = transform.position;
        couch.transform.rotation = transform.rotation;
        couch.transform.localScale = new Vector3(Random.Range(1f, 2f), Random.Range(1f, 1f), Random.Range(1f, 1f));
        couch.GetComponent<MeshRenderer>().enabled = false;

        float lower = 0.2f;//make small if we want skinny legs
        float upperMax = 0.3f;//make large if we want fat little guys
        float feetX = Random.Range(lower, upperMax);
        float feetY = upperMax - feetX;
        //float feetZ = Random.Range(0.2f, 0.4f);
        float feetZ = feetX;
        float bottomSize = Random.Range(0.2f, 0.4f);
        float backWidth = Random.Range(0.05f, 0.4f);
        float backHeight = Random.Range(0.2f, 0.5f);
        float armRestWidth = Random.Range(0.05f, 0.2f);
        float armRestHeight = Random.Range(0.05f, 0.2f);
        PrimitiveType pt = PrimitiveType.Cube;
        if (Random.Range(0, 2) == 0)
            pt = PrimitiveType.Cylinder;

        InteriorAssets.LivingroomItems.Couch(couch, feetX, feetY, feetZ, bottomSize, backWidth, backHeight, armRestWidth, armRestHeight,pt);
           

    }
    // Update is called once per frame
    void Update()
    {

        counter++;
        if (counter > timer)
        {
            counter = 0;
            Destroy(couch);
            Start();
        }

        //couch.transform.rotation *= Quaternion.Euler(0, 0.5f, 0);
    }
}

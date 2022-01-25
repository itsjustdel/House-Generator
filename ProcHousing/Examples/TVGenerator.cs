using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVGenerator : MonoBehaviour
{

    public GameObject tv;
    int counter = 0;
    public int timer = 200;
    // Use this for initialization
    void Start()
    {
        tv= GameObject.CreatePrimitive(PrimitiveType.Cube);
        tv.transform.position = transform.position;
       // tv.transform.position += 1f * Vector3.up;
        tv.transform.localScale = new Vector3(Random.Range(1f, 1f), Random.Range(1f, 1f), Random.Range(1f, 1f));
        tv.GetComponent<MeshRenderer>().enabled = false;
        tv.name = "TV";

        GameObject g = InteriorAssets.LivingroomItems.Television(gameObject, tv,true);
        g.transform.localScale *= Random.Range(1f, 2f);
        g.transform.position += Vector3.up * g.transform.localScale.y * 0.25f;


    }
    // Update is called once per frame
    void Update()
    {

        counter++;
        if (counter > timer)
        {
            counter = 0;
            Destroy(tv);
            Start();
        }

        //tv.transform.rotation *= Quaternion.Euler(0, 0.5f, 0);
    }
}

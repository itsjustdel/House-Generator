using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BedGenerator : MonoBehaviour
{

    public GameObject bed;
    int counter = 0;
    public int timer = 200;
    // Use this for initialization
    void Start()
    {
        bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bed.transform.position = transform.position;
        bed.transform.localScale = new Vector3(Random.Range(1f, 2f), Random.Range(1f, 1f), Random.Range(2.5f, 2.5f));
        bed.GetComponent<MeshRenderer>().enabled = false;
        

        InteriorAssets.BedroomItems.Bed(bed);

    }
    // Update is called once per frame
    void Update()
    {

        counter++;
        if (counter > timer)
        {
            counter = 0;
            Destroy(bed);
            Start();
        }

        bed.transform.rotation *= Quaternion.Euler(0, 0.5f, 0);
    }
}

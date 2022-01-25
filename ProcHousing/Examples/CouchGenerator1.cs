using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorGenerator: MonoBehaviour
{

    public GameObject wardrobe;
    int counter = 0;
    public int timer = 200;
    // Use this for initialization
    void Start()
    {
        wardrobe = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wardrobe.transform.position = transform.position;
        wardrobe.transform.position += 1f * Vector3.up;
        wardrobe.transform.localScale = new Vector3(Random.Range(2f, 1f), Random.Range(1f, 2f), Random.Range(1f, 1f));
        wardrobe.GetComponent<MeshRenderer>().enabled = false;


        InteriorAssets.BedroomItems.Wardrobe(wardrobe);

    }
    // Update is called once per frame
    void Update()
    {

        counter++;
        if (counter > timer)
        {
            counter = 0;
            Destroy(wardrobe);
            Start();
        }

        wardrobe.transform.rotation *= Quaternion.Euler(0, 0.5f, 0);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableAndChairsGenerator : MonoBehaviour
{

    public GameObject tac;
    int counter = 0;
    public int timer = 200;
    // Use this for initialization
    void Start()
    {
        tac = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tac.transform.position = gameObject.transform.position;
        tac.transform.localScale = new Vector3(Random.Range(.5f, 4f), Random.Range(.5f, 2f), Random.Range(.5f, 4f));
        tac.GetComponent<MeshRenderer>().enabled = false;

        InteriorAssets.LivingroomItems.TableAndChairsMaker(tac, 4);

        tac.transform.position += Vector3.up * tac.transform.localScale.y * .5f;

    }
    // Update is called once per frame
    void Update()
    {

        counter++;
        if (counter > timer)
        {
            counter = 0;
            Destroy(tac);
            Start();
        }

        tac.transform.rotation *= Quaternion.Euler(0, 0.5f, 0);
    }
}

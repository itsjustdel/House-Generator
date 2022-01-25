using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookShelfGenerator : MonoBehaviour
{

    public GameObject bookshelf;
    int counter = 0;
    public int timer = 200;
    // Use this for initialization
    void Start()
    {
        bookshelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bookshelf.transform.position = transform.position;
        bookshelf.transform.rotation = transform.rotation;
        bookshelf.transform.localScale = new Vector3(Random.Range(2f, 1f), Random.Range(1f, 2f), Random.Range(.5f, .5f));
        bookshelf.GetComponent<MeshRenderer>().enabled = false;
        

        InteriorAssets.BedroomItems.BookShelf(bookshelf);

    }
    // Update is called once per frame
    void Update()
    {

        counter++;
        if (counter > timer)
        {
            counter = 0;
            Destroy(bookshelf);
            Start();
        }

        bookshelf.transform.rotation *= Quaternion.Euler(0, 0.5f, 0);
    }
}

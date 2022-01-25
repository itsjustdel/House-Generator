using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testFLoor : MonoBehaviour {

	// Use this for initialization
	void Start () {

        int random = Random.Range(0, 3);
   
        if (random == 0)
        {
            //Floors.Carpet(gameObject, 8, 1f);
            float x = Random.Range(0.9f, 0.9f);
            float z = Random.Range(0.1f, 0.1f);
            z = Mathf.Clamp(z, 0.1f, 0.9f);
            Floors.FloorBoards(gameObject, x, z, 0.01f,x*0.5f,gameObject.AddComponent<Divide>());
        }
        if (random == 1)
        {
            Floors.Carpet(gameObject, 8, 1f, gameObject.AddComponent<Divide>());
            
        }
        if (random == 2)
        {
            //Floors.Carpet(gameObject, 8, 1f);
            float x = Random.Range(0.1f, 0.9f);
            float z = Random.Range(-0.3f, 0.3f);
            z += x + z;
            z = Mathf.Clamp(z, 0.1f, 0.9f);
            Floors.Tiled(gameObject, x, z, 0.01f, gameObject.AddComponent<Divide>());
        }

    }
	
}

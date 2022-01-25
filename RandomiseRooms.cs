using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomiseRooms : MonoBehaviour {

    public List<GameObject> rooms;
	// Use this for initialization
	void Start () {
        GetHouseBlocks();
        Randomise();
	}

    void GetHouseBlocks()
    {
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            rooms.Add(transform.GetChild(i).gameObject);
        }
    }

    void Randomise()
    {
        float limitXLeft = transform.position.x - (transform.localScale.x * 0.2f);        
        float limitXRight = transform.position.x + (transform.localScale.x * 0.2f);
        
        float limitZTop = transform.position.x + (transform.localScale.x * 0.2f);
        float limitZBottom = transform.position.x - (transform.localScale.x * 0.2f);

        foreach (GameObject room in rooms)
        {

            int randomintX = Random.Range(-10,10);            
            float move = randomintX * 0.02f;
            
            float randomFloatZ = Random.Range(limitZTop, limitZBottom);
            Vector3 randomV3 = new Vector3(transform.position.x + move, transform.position.y, randomFloatZ);
            room.transform.position = randomV3;
        }
    }
}

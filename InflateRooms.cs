using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InflateRooms : MonoBehaviour {
    public AddFeaturesToHouse2 adf2;
    public StretchQuads sq;
    public float power;
    public List<GameObject> houseBlocks;
    float xmultiplier = 0.001f;
    float ymultiplier = 0.001f;
    float zmultiplier = 0.001f;
    private float brickSize;
    // Use this for initialization
    void Start () {
        // GetHouseBlocks();
        brickSize = sq.brickSize;

        xmultiplier = brickSize;
        ymultiplier = 0;
        zmultiplier = brickSize;
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        
            float random = Random.Range(0f, 2f);


            Vector3 scaleMultiplier = new Vector3(xmultiplier, ymultiplier, zmultiplier);
          transform.localScale += scaleMultiplier;
      //  transform.localScale *= 1.02f;
        Raycasts();
	
	}

    void GetHouseBlocks()
    {
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            houseBlocks.Add(transform.GetChild(i).gameObject);
        }
    }

    void Raycasts()
    {
        //raycast in 4 directions checking for hits
        //need to shoot from the corners as well as the middle, so 3 per direction :/

        //forward
        Vector3 pos = transform.position + (Vector3.forward * transform.localScale.z * 0.5f);
        if(Physics.Raycast(pos, Vector3.forward, brickSize * 2))
        {
            zmultiplier = 0f;
        }

        pos = transform.position + (Vector3.forward * transform.localScale.z * 0.5f);
        pos -= (transform.localScale.x * 0.5f) * Vector3.right;
        if (Physics.Raycast(pos,Vector3.forward,brickSize*2))
        {
            zmultiplier = 0f;
        }
        //adjust to the other side
        pos += transform.localScale.x * Vector3.right;
        if(Physics.Raycast(pos, Vector3.forward, brickSize*2))
        {
            zmultiplier = 0f;
        }
        //back
        pos = transform.position + (Vector3.back * transform.localScale.z * 0.5f);
        if (Physics.Raycast(pos, Vector3.back, brickSize * 2))
        {
            zmultiplier = 0f;
        }

        pos -= (transform.localScale.x * 0.5f) * Vector3.right;
        if (Physics.Raycast(pos, Vector3.back, brickSize*2))
        {
            zmultiplier = 0f;
        }
        
        pos += transform.localScale.x * Vector3.right ;
        if (Physics.Raycast(pos, Vector3.back, brickSize*2))
        {
            zmultiplier = 0f;
        }

        //right
        pos = transform.position + (Vector3.right * transform.localScale.x * 0.5f);
        if (Physics.Raycast(pos, Vector3.right, brickSize * 2))
        {
            xmultiplier = 0f;
        }

        pos -= (transform.localScale.z * 0.5f) * Vector3.forward;
        if (Physics.Raycast(pos, Vector3.right, brickSize*2))
        {
            xmultiplier = 0f;
        }
        pos += transform.localScale.z * Vector3.forward;
        if (Physics.Raycast(pos, Vector3.right, brickSize * 2))
        {
            xmultiplier = 0f;
        }
        
        //left
        pos = transform.position + (Vector3.left * transform.localScale.x * 0.5f);       
        if (Physics.Raycast(pos, Vector3.left, brickSize*2))
        {
            xmultiplier = 0f;
        }

        pos -= (transform.localScale.z * 0.5f) * Vector3.forward;
        if (Physics.Raycast(pos, Vector3.left, brickSize * 2))
        {
            xmultiplier = 0f;
        }

        pos += transform.localScale.z * Vector3.forward;
        if (Physics.Raycast(pos, Vector3.left, brickSize * 2))
        {
            xmultiplier = 0f;
        }
    }
    void OnTriggerStay(Collider other)
    {
        //if colliding with another house layer
   //     if (other.gameObject.layer == 24)
     //   {
       //     other.transform.localScale *= 0.99f;
     // }
    }
}

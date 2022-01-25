using UnityEngine;
using System.Collections;

public class StopGrowing : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        Vector3 resize = transform.parent.localScale;
        resize.z += 0.1f;
        transform.parent.localScale = resize;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == 24)
        { 
            if (transform.name == "Down" || transform.name == "Up")
            {
                //Destroy(this);
                Debug.Log("hit");
            }
        }
    }
}

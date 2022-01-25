using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BathGenerator : MonoBehaviour {

    public GameObject bath;
    int counter = 0;
    public int timer = 200;
    // Use this for initialization
    void Start()
    {

        //
        bath = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bath.transform.localScale = new Vector3(Random.Range(1f,1.5f), Random.Range(1f,2f), Random.Range(1.5f, 2f));
        bath.GetComponent<MeshRenderer>().enabled = false;

        //create variables for bathroom suite, all curves will look similiar
        float innerSteepness = Random.Range(.2f, .5f);
        float length = bath.transform.localScale.z;
        float width = bath.transform.localScale.x;
        float depth = bath.transform.localScale.y*0.6f;
        float cornerRandomness = Random.Range(0.5f, 1f);//any lower than 2.5 and it pinches

        //make second control point for the curve always be less than the first, this ensures the tap side of th bath is always a sharper corner
        float secondCornerRandomness = Random.Range(0f, cornerRandomness);

        bool outsideCurved = false;
        if (Random.Range(0, 2) == 0) //skew outside?
            outsideCurved = true;
        //if outside is curved, inside has to be too
        bool insideCurved = false;
        if (outsideCurved)
            insideCurved = true;
        //if  outside is not curved, choose if inside is curved
        if (!outsideCurved)
            if (Random.Range(0, 2) == 0)
                insideCurved = true;

        //outsideCurved = false;
        //insideCurved= true;

        float detail = 20f; // detail % 2 != odd number, valid, 12,16,20,24,32,40,60 etc
        float rimWidth = 0.1f;
        float tapAreaDepth = length / 10;
        //always do outerRi, just looks better,
        bool outerRim = true;
        //if (Random.Range(0, 2) == 0)
        //  outerRim = false;

        bool panel = true;
        if (Random.Range(0, 2) == 0)// && outsideCurved)
            panel = false;

        if (!outerRim)
            panel = false;

        bool innerRim = false;
        //amin bath object                                                                                                                                                                                      

        GameObject bathObject =InteriorAssets.BathRoomItems.Bath(gameObject, bath, detail, length, width, depth, outerRim, innerRim, rimWidth, tapAreaDepth, cornerRandomness, secondCornerRandomness, innerSteepness, false, insideCurved, panel);//forcing outside bath straight

        
        bathObject.transform.position += depth * Vector3.up;
        bathObject.transform.parent = bath.transform;

        //taps for bath
        GameObject taps = InteriorAssets.BathRoomItems.Taps(bath, width, length, tapAreaDepth, rimWidth);
        taps.transform.position += Vector3.up * depth;

        //shower 
        //shower unit / control box
        GameObject showerController = InteriorAssets.BathRoomItems.ShowerController(bath, length, width);

        //choose screen or curtain
        
        bool doScreen = false;
        if (Random.Range(0, 2) == 0)
            doScreen = true;



        if (!doScreen)
        {
            GameObject showerCurtain = InteriorAssets.BathRoomItems.ShowerCurtain(gameObject, bath, width, length, 2.5f, .5f, 20f);
            // showerCurtain.transform.position += Vector3.up * depth;
            //to make cutrina hang off bath a bit..
            showerCurtain.transform.localScale += Vector3.one * 0.02f;
        }
        else if (doScreen)
        {
            int amountOfPanels = Random.Range(1, 5);
            GameObject screen = InteriorAssets.BathRoomItems.ShowerScreen(gameObject, bath, width, length * 0.5f / amountOfPanels, rimWidth, amountOfPanels);
            //screen needs moved back, builds in centre of bath, same code used for shower screen
            screen.transform.position -= bath.transform.forward * length * 0.5f - (bath.transform.forward * length / amountOfPanels) * 0.25f;// amountOfPanels;
            screen.transform.position += depth * Vector3.up;
            screen.transform.parent = bath.transform;

        }
        //showerhead
        GameObject showerHead = InteriorAssets.BathRoomItems.ShowerHead(bathObject, width, length, Random.Range( 1.5f,2f));
        //tube
        InteriorAssets.BathRoomItems.ShowerTube(showerHead, showerController, bath);

        Divide emptyDivide = gameObject.AddComponent<Divide>();//test
        HouseBuilder.Walls.TilesOnWall(bath, bath,emptyDivide);//needs lenght options


    }
    // Update is called once per frame
    void Update () {

        counter++;
        if (counter > timer)
        {
            counter = 0;
            Destroy(bath);
            Start();
        }

        bath.transform.rotation *= Quaternion.Euler(0,0.5f, 0);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteriorAssets : MonoBehaviour
{

    public static void BathroomInteriorAssets(GameObject room, List<GameObject> objectsToBuild,Divide divide)
    {

        //Floors.Tiled(roomsAndSizes[0].room,corners);

        //another way? 
        // GameObject bath = objectsToBuild[0];



        //create variables for bathroom suite, all curves will look similiar
        float innerSteepness = Random.Range(.2f, .5f);
        
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
        
        //always do outerRi, just looks better,
        bool outerRim = true;
        //always do panel, underneath doesnt look good atm, istnncorrect drop
        bool panel = true;
     

        bool innerRim = false;
        //amin bath object                                                                                                                                                                                      

        for (int i = 0; i < objectsToBuild.Count; i++)
        {
            //destroying parent's renderer - we don't need it and can be turned on by LOD switcher if we keep it - just going to ignore disabled renderers when adding to lsit
            //Destroy(objectsToBuild[i].GetComponent<MeshRenderer>());

            float length = objectsToBuild[i].transform.localScale.z;
            float width = objectsToBuild[i].transform.localScale.x;
            float depth = objectsToBuild[i].transform.localScale.y * 0.6f;
            float tapAreaDepth = length / 10;

            if (objectsToBuild[i].name == "Bath")
            {
                

                GameObject bathObject = BathRoomItems.Bath(room, objectsToBuild[i], detail, objectsToBuild[i].transform.localScale.z, objectsToBuild[i].transform.localScale.x, depth, outerRim, innerRim, rimWidth, tapAreaDepth, cornerRandomness, secondCornerRandomness, innerSteepness, false, insideCurved, panel);//forcing outside bath straight

                bathObject.transform.position += depth * Vector3.up;
                bathObject.transform.parent = objectsToBuild[i].transform;

                //taps for bath
                GameObject taps = BathRoomItems.Taps(objectsToBuild[i], width, length, tapAreaDepth, rimWidth);
                taps.transform.position += Vector3.up * depth;

                //shower 
                //shower unit / control box
                GameObject showerController = BathRoomItems.ShowerController(objectsToBuild[i], length, width);

                //choose screen or curtain

                bool doScreen = false;
                if (Random.Range(0, 2) == 0)
                    doScreen = true;

                   

                if (!doScreen)
                {
                    GameObject showerCurtain = BathRoomItems.ShowerCurtain(room, objectsToBuild[i], objectsToBuild[i].transform.localScale.x, objectsToBuild[i].transform.localScale.z,Random.Range(2f,2.5f), .5f, 20f);
                    // showerCurtain.transform.position += Vector3.up * depth;
                    //to make cutrina hang off bath a bit..
                    showerCurtain.transform.localScale += Vector3.one * 0.02f;
                }
                else if (doScreen)
                {
                    int amountOfPanels = Random.Range(1, 5);
                    GameObject screen = BathRoomItems.ShowerScreen(room, objectsToBuild[i], objectsToBuild[i].transform.localScale.x, objectsToBuild[i].transform.localScale.z*0.5f/ amountOfPanels, Random.Range(-1f,-0.5f), amountOfPanels);
                    //screen needs moved back, builds in centre of bath, same code used for shower screen
                    screen.transform.position -= objectsToBuild[i].transform.forward * objectsToBuild[i].transform.localScale.z * 0.5f - (objectsToBuild[i].transform.forward * objectsToBuild[i].transform.localScale.x / amountOfPanels) * 0.25f;// amountOfPanels;
                    screen.transform.position += depth * Vector3.up;
                    screen.transform.parent = objectsToBuild[i].transform;


                }
                //showerhead
                GameObject showerHead = BathRoomItems.ShowerHead(bathObject, objectsToBuild[i].transform.localScale.x, objectsToBuild[i].transform.localScale.z, Random.Range(1.5f, 2f));
                //tube
                BathRoomItems.ShowerTube(showerHead, showerController, objectsToBuild[i]);

                HouseBuilder.Walls.TilesOnWall(room, objectsToBuild[i],divide);//needs lenght options
                
            }
            if (objectsToBuild[i].name == "Shower")
            {
                Debug.Log("Shower - check parent for LOD");
                width = 1f;//grab from up top? //or call as soon as placed? maybe better
                length = 1f;
                tapAreaDepth = 0f;
                depth = 0.2f;
                //force symmetrical
                GameObject bathObject = BathRoomItems.Bath(room, objectsToBuild[i], detail, length, width, depth, outerRim, innerRim, rimWidth, tapAreaDepth, cornerRandomness, cornerRandomness, innerSteepness, false, insideCurved, true);//force panel on shower
                bathObject.name = "Shower";
                bathObject.transform.position += -Vector3.up * 0.5f + Vector3.up * (depth);// + rimWidth);
                //shower 
                //shower unit / control box
                GameObject showerController = BathRoomItems.ShowerController(objectsToBuild[i], length, width);

                //choose screen or curtain
                bool doScreen = false;

                if (Random.Range(0, 2) == 0)
                    doScreen = true;
                // else
                //     doCurtain = true;

                

                if (!doScreen)
                    BathRoomItems.ShowerCurtain(room,bathObject, width, length, 2f, .5f, 20f);
                else if (doScreen)
                {
                    BathRoomItems.ShowerScreen(room,bathObject, width, length, rimWidth, 1);
                    //make again and move and rotate for end of of cabinet
                    GameObject secondScreen = BathRoomItems.ShowerScreen(room,bathObject, width, length, rimWidth, 1);
                    secondScreen.transform.position += objectsToBuild[i].transform.forward * length;

                    secondScreen.transform.rotation *= Quaternion.Euler(0, -90, 0);

                }
                //showerhead
                GameObject showerHead = BathRoomItems.ShowerHead(bathObject, width, length, Random.Range(1.5f, 2f));
                //tube
                BathRoomItems.ShowerTube(showerHead, showerController, objectsToBuild[i]);
            }
            if (objectsToBuild[i].name == "Toilet")
            {

                width = .7f;//grab from up top? //or call as soon as placed? maybe better
                length = .8f;

                rimWidth = 0.1f;//ratio?
                tapAreaDepth = rimWidth;//length / 3;
                depth = .5f;
                //   outsideCurved = true;
                //   insideCurved = true;
                GameObject toilet = BathRoomItems.Bath(room, objectsToBuild[i], detail, length, width, depth, outerRim, innerRim, rimWidth, tapAreaDepth, cornerRandomness, secondCornerRandomness, innerSteepness / 2, true, insideCurved, false);
                // toilet.transform.rotation = Quaternion.Euler(0, 90, 0) * toilet.transform.rotation;
                toilet.transform.position = objectsToBuild[i].transform.position + (Vector3.up*(depth - rimWidth));
                toilet.transform.rotation = objectsToBuild[i].transform.rotation;
                toilet.name = "Toilet";
                toilet.transform.parent = objectsToBuild[i].transform;
                GameObject cistern = BathRoomItems.Cistern(toilet);
                cistern.transform.localScale = new Vector3(width / 5, width / 5, width / 10);//5 is cube detail so * 4
                cistern.transform.rotation = toilet.transform.rotation;
               // cistern.transform.position = objectsToBuild[i].transform.position;
                cistern.transform.position -= cistern.transform.forward * ((length * 0.5f) + rimWidth);
                cistern.transform.position += Vector3.up * 0.3f;
                cistern.transform.parent = objectsToBuild[i].transform;
                GameObject cisternLid = BathRoomItems.Cistern(toilet);
                cisternLid.transform.localScale = new Vector3(width / 5, width / 20, width / 10);//5 is cube detail
                cisternLid.transform.localScale *= 1f + rimWidth;
                cisternLid.transform.rotation = toilet.transform.rotation;
                cisternLid.transform.position -= cistern.transform.forward * ((length * 0.5f) + rimWidth);
                cisternLid.transform.position += Vector3.up * 0.3f + (Vector3.up * (width * 0.45f));//5 is detail used on static function, just - minus a half so it sits nicely on top
                cisternLid.transform.parent = objectsToBuild[i].transform;
                GameObject toiletHandle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                toiletHandle.name = "Toilet Handle";
                toiletHandle.transform.rotation = toilet.transform.rotation;
                toiletHandle.transform.localScale = new Vector3(0.07f, 0.02f, 0.02f);
                toiletHandle.transform.position = cistern.transform.position + (toilet.transform.forward * width / 4) - (toilet.transform.right * width / 3) + Vector3.up * width / 4;
                toiletHandle.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Black") as Material;
                toiletHandle.transform.parent = objectsToBuild[i].transform;

                //adjusto 
                objectsToBuild[i].transform.position += toilet.transform.forward * ((tapAreaDepth)+rimWidth);

            }
            if (objectsToBuild[i].name == "Sink")
            {
                width = 1f;//grab from up top? //or call as soon as placed? maybe better
                length = .75f;
                depth = 0.5f;
                tapAreaDepth = length / 5;


                GameObject sink = BathRoomItems.Bath(room, objectsToBuild[i], detail, length, width, depth, outerRim, innerRim, rimWidth, tapAreaDepth, cornerRandomness, secondCornerRandomness, innerSteepness, outsideCurved, insideCurved, panel);
                //sink.transform.rotation = Quaternion.Euler(0, 90, 0) * sink.transform.rotation;
                sink.transform.position -= sink.transform.forward * (tapAreaDepth);
                sink.transform.position += (depth + rimWidth) * Vector3.up;
                sink.name = "Sink";
                sink.transform.parent = objectsToBuild[i].transform;

                BathRoomItems.Taps(sink, width, length, tapAreaDepth, rimWidth);

                //sink.transform.localScale = new Vector3(1f, 0.5f, 1f);
                // sink.transform.parent = objectsToBuild[2].transform;
                sink.transform.position += 0.5f * Vector3.up;
                //BathRoomItems.Taps(GameObject.Find("Sink"), width, length, tapAreaDepth, rimWidth);
            }

            
        }





    }

    public static List<GameObject> KitchenInteriorAssets(GameObject room, List<GameObject> objectsToBuild)
    {
        //we are psased a bunch of cubes with correct rotations - create models inside the cubes

        //first, we must find out how many sink cubes have been placed
        List<GameObject> sinks = new List<GameObject>();
        for (int i = 0; i < objectsToBuild.Count; i++)
        {
            if (objectsToBuild[i].name == "Sink")
            {
                sinks.Add(objectsToBuild[i]);

            }
        }
        //make new sink cube 
        if(sinks.Count == 0)
        {
            //room is too small for kitchen - remake house with less rooms?
            Debug.Log("Kitchen too small , no sinks were placed");
            return objectsToBuild;
        }
        Vector3 avg = Vector3.zero;
        foreach (GameObject s in sinks)
        {
            avg += s.transform.position;
            s.GetComponent<MeshRenderer>().enabled = false;
            s.GetComponent<BoxCollider>().enabled = false;
            //objectsToBuild.Remove(s);
        }
        avg /= sinks.Count;
        GameObject sinkParent = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sinkParent.transform.position = avg;


        sinkParent.transform.localScale = new Vector3(sinks.Count, 1, 1);
        sinkParent.name = "SinkParent";
        sinkParent.transform.parent = room.transform;

        //add this parent to the list
        objectsToBuild.Add(sinkParent);

        //we can use bath method to make a sink - Bath builds along it's forward axis, so we need to spin the sink parent

        float depth = 0.6f;
        bool innerRim = true;
        bool outerRim = true;//needs to be on
        float rimWidth = Random.Range(0.05f, 0.2f);
        float tapAreaDepth = Random.Range(.5f, .8f);
        if (sinks.Count == 1)
            tapAreaDepth = 0f;
        float cornerRandomness = .5f;
        float secondCornerRandomness = 0.5f;
        float innerSteepness = .5f - rimWidth; //.5 scale for half a metre (scale.x)

        bool outsideCurved = false;
        bool insideCurved = false;//bath curve too complex to create spurely symmetrical curve - create simpler Sink function if we want this**Never actually perfect atm

        bool panel = false;// we will build cupboards
        //not  we sway the x and z scale values to make it build the correct way since we rotated

        //grab sinks and create new object//
        #region sink
        GameObject sink = BathRoomItems.Sink(room, sinkParent, 16, sinkParent.transform.localScale.x, sinkParent.transform.localScale.z, depth, outerRim, innerRim, rimWidth, tapAreaDepth, cornerRandomness, secondCornerRandomness, innerSteepness, outsideCurved, insideCurved, panel);
        sink.transform.parent = sinkParent.transform;
        //sink.transform.FindChild("Panel").GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Green") as Material;
        sinkParent.transform.rotation = sinks[0].transform.rotation;
        sinkParent.transform.Find("Inner Bath").GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("LightMetal") as Material;

        sinkParent.transform.rotation *= Quaternion.Euler(0, 90, 0);
        sink.transform.position += Vector3.up * (0.5f+0.05f);

        sinkParent.GetComponent<MeshRenderer>().enabled = false;
        List<GameObject> bits = new List<GameObject>();
        GameObject quadForFrontOfSink = GameObject.CreatePrimitive(PrimitiveType.Cube);
        quadForFrontOfSink.transform.position = sinkParent.transform.position;
        quadForFrontOfSink.transform.rotation = sinkParent.transform.rotation;
        quadForFrontOfSink.transform.rotation *= Quaternion.Euler(0, 90, 0);
        quadForFrontOfSink.transform.position += sinks[0].transform.forward*(0.5f - 0.1f/2);
        quadForFrontOfSink.transform.localScale = new Vector3(sinks.Count, 1 - 0.1f*2, 0.1f);// 0.1f is worktop thinckness need to pull up if we want to randomise, it is in assets functions
        quadForFrontOfSink.transform.position -= Vector3.up * 0.1f/2f;
        quadForFrontOfSink.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Yellow") as Material;
        bits.Add(quadForFrontOfSink);
        

        GameObject quadForSide1= GameObject.CreatePrimitive(PrimitiveType.Cube);
        quadForSide1.transform.position = sinkParent.transform.position;
        quadForSide1.transform.rotation = sinkParent.transform.rotation;
        //quadForSide1.transform.rotation *= Quaternion.Euler(0, 90, 0);
        quadForSide1.transform.position += sinks[0].transform.right * ((sinks.Count / 2) - 0.05f);
        quadForSide1.transform.localScale = new Vector3(1-0.1f*2, 1 - 0.1f * 2, 0.1f);// 0.1f is worktop thinckness need to pull up if we want to randomise, it is in assets functions
        quadForSide1.transform.position -= Vector3.up * 0.1f / 2f;
        quadForSide1.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Yellow") as Material;
        bits.Add(quadForSide1);

        GameObject quadForSide2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        quadForSide2.transform.position = sinkParent.transform.position;
        quadForSide2.transform.rotation = sinkParent.transform.rotation;
        quadForSide1.transform.rotation *= Quaternion.Euler(0, 180, 0);
        quadForSide2.transform.position -= sinks[0].transform.right * ((sinks.Count / 2) - 0.05f);
        quadForSide2.transform.localScale = new Vector3(1-0.1f*2, 1 - 0.1f * 2, 0.1f);// 0.1f is worktop thinckness need to pull up if we want to randomise, it is in assets functions
        quadForSide2.transform.position -= Vector3.up * 0.1f / 2f;
        quadForSide2.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Yellow") as Material;
        bits.Add(quadForSide2);

        //make smaller
        sinkParent.transform.localScale = new Vector3( sinkParent.transform.localScale.x* 0.8f,1f, sinkParent.transform.localScale.z* 0.8f); ;
        
        //build worktops around

        GameObject w1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        w1.transform.rotation = sinks[0].transform.rotation;
        w1.transform.position = sinkParent.transform.position + Vector3.up * (0.5f - 0.1f / 2) + w1.transform.right*((sinks.Count/2)-0.1f);//-0.1 here puts in the middle of the end of the sink and the end of the cubeParent
        w1.transform.localScale = new Vector3(0.2f, 0.1f, 1f);//0.2 is because we reduce sink scale by .8 ( 1-.8 = .2)
        w1.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Metal") as Material;
        bits.Add(w1);
        GameObject w2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        w2.transform.rotation = sinks[0].transform.rotation;
        w2.transform.position = sinkParent.transform.position + Vector3.up * (0.5f - 0.1f / 2) - w2.transform.right * ((sinks.Count / 2) - 0.1f);//-0.1 here puts in the middle of the end of the sink and the end of the cubeParent
        w2.transform.localScale = new Vector3(0.2f, 0.1f, 1f);//0.2 is because we reduce sink scale by .8 ( 1-.8 = .2)
        w2.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Metal") as Material;
        bits.Add(w2);
        GameObject w3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        w3.transform.rotation = sinks[0].transform.rotation;
        w3.transform.position = sinkParent.transform.position + Vector3.up * (0.5f - 0.1f / 2) - w3.transform.forward * 0.45f;
        w3.transform.localScale = new Vector3(sinks.Count-0.4f, 0.1f, 0.1f);//0.2 is because we reduce sink scale by .8 ( 1-.8 = .2)
        w3.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Metal") as Material;
        bits.Add(w3);
        GameObject w4 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        w4.transform.rotation = sinks[0].transform.rotation;
        w4.transform.position = sinkParent.transform.position + Vector3.up * (0.5f - 0.1f / 2) + w4.transform.forward * 0.45f;
        w4.transform.localScale = new Vector3(sinks.Count - 0.4f, 0.1f, 0.1f);//0.2 is because we reduce sink scale by .8 ( 1-.8 = .2)
        w4.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Metal") as Material;
        bits.Add(w4);
        #endregion
        //destroying parent's renderer - we don't need it and can be turned on by LOD switcher if we keep it
        Destroy(sinkParent.GetComponent<MeshRenderer>());
        //now for the rest of the objects
        for (int i = 0; i < objectsToBuild.Count; i++)
        {
            //destroying parent's renderer - we don't need it and can be turned on by LOD switcher if we keep it
            Destroy(objectsToBuild[i].GetComponent<MeshRenderer>());

            if (objectsToBuild[i].name == "Fridge")
            {
                KitchenItems.Fridge(room, objectsToBuild[i]);
            }

            if (objectsToBuild[i].name == "Cooker")
            {
                KitchenItems.Cooker(room, objectsToBuild[i]);                
            }
            if (objectsToBuild[i].name == "WashingMachine")
            {
                KitchenItems.WashingMachine(room, objectsToBuild[i]);
            }
            if (objectsToBuild[i].name == "Cupboard")
            {
                KitchenItems.Cupboard(objectsToBuild[i]);
            }
            if (objectsToBuild[i].name == "TopCupboard")
            {
                objectsToBuild[i].transform.localScale = new Vector3(1, 1, 0.6f);
                KitchenItems.Cupboard(objectsToBuild[i]);
            }
            if (objectsToBuild[i].name == "Corner")
            {
                KitchenItems.Cupboard(objectsToBuild[i]);
            }
            if (objectsToBuild[i].name == "Sink")
            {
                KitchenItems.Cupboard(objectsToBuild[i]);
            }

            if(objectsToBuild[i].name == "Table")
            {
                LivingroomItems.TableAndChairsMaker(objectsToBuild[i], 4);
            }
        }


        //parent quads we built - doing it here because scale can change in building assets
        foreach (GameObject bit in bits)
            bit.transform.parent = sinkParent.transform;


        return objectsToBuild;
    }

    public static void BedroomInteriorAssets(GameObject room, List<GameObject> objectsToBuild)
    {
        //now for the rest of the objects
        for (int i = 0; i < objectsToBuild.Count; i++)
        {
            //destroying parent's renderer - we don't need it and can be turned on by LOD switcher if we keep it
            Destroy(objectsToBuild[i].GetComponent<MeshRenderer>());

            if (objectsToBuild[i].name == "Bed")
            {
                BedroomItems.Bed(objectsToBuild[i]);
            }
            if (objectsToBuild[i].name == "Wardrobe")
            {
                if (Random.Range(0, 2) == 0)
                    BedroomItems.Wardrobe(objectsToBuild[i]);
                else
                    BedroomItems.BookShelf(objectsToBuild[i]);
            }
            if (objectsToBuild[i].name == "Desk")
            {
                objectsToBuild[i].GetComponent<MeshRenderer>().enabled = true;
                objectsToBuild[i].transform.position += Vector3.up* objectsToBuild[i].transform.localScale.y * 0.5f;
                //true do chair
                BedroomItems.Desk(objectsToBuild[i],true);
            }
            if(objectsToBuild[i].name == "Radiator")
            {
                //unit/books/cupboards?
                //if we are doing unit, adjust z size, we dont need it as big as a table would be
                objectsToBuild[i].transform.position += Vector3.up * objectsToBuild[i].transform.localScale.y*.5f;
                float width = 0.05f;
                float depth = objectsToBuild[i].transform.localScale.z*0.75f;
                BedroomItems.Radiator(objectsToBuild[i],width,depth);
                //make white
                for (int j = 0; j < objectsToBuild[i].transform.childCount; j++)
                {
                    if(objectsToBuild[i].transform.GetChild(j).GetComponent<MeshRenderer>() != null)
                        objectsToBuild[i].transform.GetChild(j).GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("White") as Material;
                }

                
            }
        }
    }

    public static void LivingroomInteriorAssets(GameObject room, List<GameObject> objectsToBuild)
    {
        //suite options
        //either make fat nad lfat or long and skinny
        float lower = 0.2f;//make small if we want skinny legs
        float upperMax = 0.3f;//make large if we want fat little guys
        float feetX = Random.Range(lower, upperMax);
        float feetY = upperMax - feetX;
        //float feetZ = Random.Range(0.2f, 0.4f);
        float feetZ = feetX;
        float bottomSize = Random.Range(0.2f, 0.6f);//extreme?
        float backWidth = Random.Range(0.2f, 0.6f);
        float backHeight = Random.Range(0.4f, 0.6f);
        float armRestWidth = Random.Range(0.05f, 0.2f);
        float armRestHeight = Random.Range(0.05f, 0.2f);
        //choose shape
        PrimitiveType pt = PrimitiveType.Cube;
        if (Random.Range(0, 2) == 0)
            pt = PrimitiveType.Cylinder;

        for (int i = 0; i < objectsToBuild.Count; i++)
        {
            if (objectsToBuild[i].name == "Couch" || objectsToBuild[i].name == "Couch1" || objectsToBuild[i].name == "Couch2")
            {
                LivingroomItems.Couch(objectsToBuild[i], feetX, feetY, feetZ, bottomSize, backWidth, backHeight, armRestWidth, armRestHeight, pt);
            }

            if (objectsToBuild[i].name == "TV")
            {
                InteriorAssets.LivingroomItems.Television(room, objectsToBuild[i], true);//false not working atm

            }
            if (objectsToBuild[i].name == "Table")
            {
                //if big enough, build table and chairs
                if (objectsToBuild[i].transform.localScale.x < 3f || objectsToBuild[i].transform.localScale.z < 3f)
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        //unit/books/cupboards?
                        //if we are doing unit, adjust z size, we dont need it as big as a table would be
                        objectsToBuild[i].transform.position -= objectsToBuild[i].transform.forward * objectsToBuild[i].transform.localScale.z * 0.5f;
                        objectsToBuild[i].transform.position += objectsToBuild[i].transform.forward * .5f * 0.5f;
                        objectsToBuild[i].transform.localScale = new Vector3(objectsToBuild[i].transform.localScale.x, objectsToBuild[i].transform.localScale.y, .5f);
                        float width = Random.Range(0.01f, 0.1f);
                        BedroomItems.UnitHousing(objectsToBuild[i], 1, 1, width, width);///1 means no shelves
                    }
                    else
                        //desk
                        BedroomItems.Desk(objectsToBuild[i],true);
                }
                else
                    LivingroomItems.TableAndChairsMaker(objectsToBuild[i], 4);
            }

            if (objectsToBuild[i].name == "CoffeeTable")
            {
                objectsToBuild[i].GetComponent<MeshRenderer>().enabled = false;
                GameObject cT = BedroomItems.Desk(objectsToBuild[i],false);
               // cT.transform.parent = objectsToBuild[i].transform;
             
            }

            if (objectsToBuild[i].name == "Wardrobe")
            {

                //unit/books/cupboards?
                objectsToBuild[i].transform.position += Vector3.up * objectsToBuild[i].transform.localScale.y*.5f;
                objectsToBuild[i].transform.position -= objectsToBuild[i].transform.forward * objectsToBuild[i].transform.localScale.z * 0.5f;
                objectsToBuild[i].transform.position += objectsToBuild[i].transform.forward * .5f * 0.5f;
                objectsToBuild[i].transform.localScale = new Vector3(objectsToBuild[i].transform.localScale.x, objectsToBuild[i].transform.localScale.y, .5f);
                float width = Random.Range(0.01f, 0.1f);
                BedroomItems.UnitHousing(objectsToBuild[i], 1, 1, width, width);///1 means no shelves
            }
        }
        
    }

    public class KitchenItems
    {
        public static GameObject Cooker(GameObject room, GameObject parent)
        {
           
            //make hobs
            for (int i = 0; i < 4; i++)
            {
                Vector3 dir = (parent.transform.forward + parent.transform.right) * 0.25f;
                //sssh im hungover
                if (i == 1)
                    dir = (-parent.transform.forward + parent.transform.right) * 0.25f;
                if (i == 2)
                    dir = (-parent.transform.forward - parent.transform.right) * 0.25f;
                if (i == 3)
                    dir = (parent.transform.forward - parent.transform.right) * 0.25f;

                GameObject hob = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                hob.transform.position = parent.transform.position + dir + Vector3.up * .5f;
                hob.transform.parent = parent.transform;
                float randomScale = Random.Range(0.2f, 0.4f);
                hob.transform.localScale = new Vector3(randomScale, 0.02f, randomScale);
                hob.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Black") as Material;
            }
            //hob top
            GameObject hobTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hobTop.transform.rotation = parent.transform.rotation;
            hobTop.transform.position = parent.transform.position + Vector3.up * 0.4f;
            hobTop.transform.localScale = new Vector3(1f, 0.2f, 1f);
            hobTop.transform.parent = parent.transform;

            //knobs
            for (int i = 0; i < 6; i++)
            {
                if (i == 0)
                    continue;

                //go to top corner
                Vector3 start = parent.transform.position + (parent.transform.forward + parent.transform.right) * .5f + Vector3.up * 0.4f;
                Vector3 dir = (-parent.transform.right / 6) * i;

                GameObject knob = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                knob.transform.localScale = new Vector3(.1f, .01f, .1f);
                knob.transform.position = start + dir;
                knob.transform.rotation = parent.transform.rotation;
                knob.transform.rotation *= Quaternion.Euler(90, 0, 0);
                knob.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Black") as Material;
                knob.transform.parent = parent.transform;
            }

            //oven handle
            GameObject ovenHandle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ovenHandle.transform.position = parent.transform.position + Vector3.up * 0.3f + parent.transform.forward * 0.5f;
            ovenHandle.transform.rotation = parent.transform.rotation;
            ovenHandle.transform.localScale = new Vector3(1f, 0.025f, 0.05f);
            ovenHandle.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Black") as Material;
            ovenHandle.transform.parent = parent.transform;

            //oven front
            GameObject front = OvenFront(room, parent, 16f, .6f, 1f, 1f, true, true, 0.1f, 0.0f, 1f, 1f, 0f, false, true, false);
            front.transform.rotation *= Quaternion.Euler(90, 0, 0);
            front.transform.position += parent.transform.forward * 0.5f;
            front.transform.parent = parent.transform;

            //glass front
            GameObject ovenGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ovenGlass.transform.position = parent.transform.position + parent.transform.forward * 0.4f;// + Vector3.up*0.25f;
            ovenGlass.transform.rotation = parent.transform.rotation;
            ovenGlass.transform.rotation *= Quaternion.Euler(90, 0, 0);
            ovenGlass.transform.localScale = new Vector3(0.8f, 0.05f, 0.4f);
            ovenGlass.transform.parent = parent.transform;
            ovenGlass.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("DarkGlass") as Material;
            //create oven box
            GameObject ovenBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ovenBox.transform.position = parent.transform.position;
            ovenBox.transform.rotation = parent.transform.rotation;
            ovenBox.transform.localScale = new Vector3(1f, .4f, 1f);
            ovenBox.transform.parent = parent.transform;
            //flip normals so we can see in to it, but not out
            ReverseNormals(ovenBox);

            //add sides//and back why not
            for (int i = 0; i <3; i++)
            {
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.position = parent.transform.position;
                quad.transform.rotation = parent.transform.rotation;
                if(i==0)
                {
                    quad.transform.position -= parent.transform.forward * 0.5f;
                }
                if (i == 1)
                {
                    quad.transform.position -= parent.transform.right * 0.5f;
                    quad.transform.position -= parent.transform.forward * 0.05f;
                    quad.transform.rotation *= Quaternion.Euler(0, 90, 0);
                    quad.transform.localScale -= Vector3.right * 0.1f;
                }
                if (i == 2)
                {
                    quad.transform.position += parent.transform.right * 0.5f;
                    quad.transform.position -= parent.transform.forward * 0.05f;
                    quad.transform.rotation *= Quaternion.Euler(0, -90, 0);
                    quad.transform.localScale -= Vector3.right * 0.1f;
                }
                quad.transform.parent = parent.transform;
            }

            //add block for bottom
            GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bottom.transform.localScale = new Vector3(parent.transform.localScale.x, .15f, parent.transform.localScale.z);
            bottom.transform.position = parent.transform.position - Vector3.up * ((0.4f * 0.5f) + 0.2f);
            bottom.transform.rotation = parent.transform.rotation;
            bottom.transform.parent = parent.transform;
            bottom.name = "OvenBottom";

            parent.transform.localScale *= 0.95f;

            parent.GetComponent<MeshRenderer>().enabled = false;
            //washin machine good values
            //GameObject front = BathRoomItems.Sink(room, parent, 16f, 1f, 1f, 1f, true, true, 0.2f, 0.0f, 0f, 0f, 0f, false, true, true);
            //front.transform.rotation *= Quaternion.Euler(90, 0, 0);
            //front.transform.position += parent.transform.forward * 0.5f;

            return parent;
        }
        public static GameObject WashingMachine(GameObject room, GameObject parent)
        {
            //worktops
            GameObject worktop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            float worktopThickness = 0.1f;
            worktop.transform.position = parent.transform.position + Vector3.up * (.5f - worktopThickness * .5f);
            worktop.transform.localScale = new Vector3(1f, worktopThickness, 1f);
            
            worktop.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Metal") as Material;


            parent.transform.position += parent.transform.forward * .2f;
            //knobs
            int sections = Random.Range(3,8);
            
            for (int i = 0; i < sections; i++)
            {
                if (i == 0)
                    continue;

                //go to top corner
                Vector3 start = parent.transform.position + (parent.transform.forward + parent.transform.right) * .5f + Vector3.up * 0.4f;
                Vector3 dir = (-parent.transform.right / sections) * i;

                GameObject knob = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                knob.transform.localScale = new Vector3(.1f, .01f, .1f);
                knob.transform.position = start + dir;
                knob.transform.rotation = parent.transform.rotation;
                knob.transform.rotation *= Quaternion.Euler(90, 0, 0);
                knob.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;
                if(Random.Range(0,3) == 0)
                    knob.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;
                if (Random.Range(0, 3) == 1)
                    knob.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                knob.transform.parent = parent.transform;
            }


            //oven front
            float holeSize = Random.Range(0.2f, 0.3f);
            GameObject front = OvenFront(room, parent, 32f, 1f, 1f, 1f, true, true, holeSize, 0.0f, 0f, 0f, 0f, false, true, false);
            front.transform.rotation *= Quaternion.Euler(90, 0, 0);
            front.transform.position += parent.transform.forward * 0.5f;
            front.transform.parent = parent.transform;

            //glass front
            GameObject ovenGlass = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ovenGlass.transform.position = parent.transform.position + parent.transform.forward * 0.4f;// + Vector3.up*0.25f;
            ovenGlass.transform.rotation = parent.transform.rotation;
            ovenGlass.transform.rotation *= Quaternion.Euler(90, 0, 0);
            ovenGlass.transform.localScale = new Vector3(1f- holeSize - 0.2f, .3f, 1f - holeSize - 0.2f);
            ovenGlass.transform.parent = parent.transform;
            ovenGlass.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("DarkGlass") as Material;
            //create oven box
            GameObject ovenBox = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ovenBox.transform.position = parent.transform.position;
            ovenBox.transform.rotation = parent.transform.rotation;
            ovenBox.transform.rotation *= Quaternion.Euler(90, 0, 0);
            ovenBox.transform.localScale = new Vector3(.8f, .4f, .8f);
            ovenBox.transform.parent = parent.transform;
            ovenGlass.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Metal") as Material;
            //flip normals so we can see in to it, but not out
            ReverseNormals(ovenBox);

            //add sides//and back why not//and top
            for (int i = 0; i < 4; i++)
            {
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.position = parent.transform.position;
                quad.transform.rotation = parent.transform.rotation;
                if (i == 0)
                {
                    quad.transform.position -= parent.transform.forward * 0.5f;
                }
                if (i == 1)
                {
                    quad.transform.position -= parent.transform.right * 0.5f;
                    quad.transform.position -= parent.transform.forward * 0.1f;
                    quad.transform.rotation *= Quaternion.Euler(0, 90, 0);
                    quad.transform.localScale -= Vector3.right * 0.2f;
                }
                if (i == 2)
                {
                    quad.transform.position += parent.transform.right * 0.5f;
                    quad.transform.position -= parent.transform.forward * 0.1f;
                    quad.transform.rotation *= Quaternion.Euler(0, -90, 0);
                    quad.transform.localScale -= Vector3.right * 0.2f;
                }
                if (i == 3)
                {
                    quad.transform.position += parent.transform.up * 0.5f;
                    quad.transform.position -= parent.transform.forward * 0.1f;
                    quad.transform.rotation *= Quaternion.Euler(90, 90, 0);
                    quad.transform.localScale -= Vector3.right * 0.2f;
                }
                quad.transform.parent = parent.transform;
            }

            //having to do this to squeeze under - sink/bath code needs to be of size 1f at least to work
            parent.transform.localScale *= 0.8f;
           
            parent.GetComponent<MeshRenderer>().enabled = false;

            //put this back now, it shouldnt be mvoed with the parnet forward move
            worktop.transform.parent = parent.transform;


            return parent;
        }
        public static GameObject Fridge(GameObject room,GameObject parent)
        {
            GameObject fridge = new GameObject();
            fridge.transform.position = parent.transform.position + Vector3.up*0.3f;
            fridge.transform.rotation = parent.transform.rotation;
            fridge.transform.parent = parent.transform;
            
            MeshFilter mf = fridge.AddComponent<MeshFilter>();
            MeshRenderer mr = fridge.AddComponent<MeshRenderer>();
            mr.sharedMaterial = Resources.Load("Yellow") as Material;

            RoundedCube rc = fridge.AddComponent<RoundedCube>();
            rc.xSize = 10;
            rc.ySize = 10;
            rc.zSize = 10;
            rc.roundness = 1;

            fridge.transform.localScale = new Vector3(0.1f, 0.075f, 0.1f);
           if (Random.Range(0, 2) == 0)
            {
                //do 1 big door
                GameObject door1 = Instantiate(fridge, fridge.transform.position, fridge.transform.rotation);
                door1.name = "FridgeDoor";
                //match size of front
                door1.transform.localScale = new Vector3(0.1f, 0.15f, 0.01f);
                //now make smaller
                door1.transform.localScale *= 0.9f;
                door1.transform.position += door1.transform.forward * 0.5f;
                door1.transform.parent = parent.transform;

                GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                handle.transform.position = fridge.transform.position + fridge.transform.forward * 0.5f + fridge.transform.right * 0.25f;
                handle.transform.rotation = fridge.transform.rotation;
                handle.transform.localScale = new Vector3(0.2f, 0.05f, 0.2f);
                handle.name = "FridgeHandle";
                handle.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Black") as Material;
                handle.transform.parent = parent.transform;

            }
            else
            {

                fridge.transform.position += 0.15f * Vector3.up;//this is all fucked because rounded cube script builds at a amassive size
                //two doors
                GameObject door1 = Instantiate(fridge, fridge.transform.position, fridge.transform.rotation);
                door1.name = "FridgeDoorSmall";
                //match size of front
                door1.transform.localScale = new Vector3(0.1f, 0.05f, 0.01f);
                //now make smaller
                door1.transform.localScale *= 0.9f;
                door1.transform.position += door1.transform.forward * 0.5f;
                door1.transform.position += Vector3.up * 0.4f;
                door1.transform.parent = parent.transform;

                GameObject door2= Instantiate(fridge, fridge.transform.position, fridge.transform.rotation);
                door2.transform.localScale = new Vector3(0.1f, 0.1f, 0.01f);
                //now make smaller
                door2.transform.localScale *= 0.9f;
                door2.transform.position += door2.transform.forward * 0.5f;
                door2.transform.position -= Vector3.up * 0.3f;
                door2.transform.parent = parent.transform;
                GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                handle.transform.position = fridge.transform.position + fridge.transform.forward * 0.5f + fridge.transform.right*0.25f;
                handle.transform.rotation = fridge.transform.rotation;
                handle.transform.localScale = new Vector3(0.2f, 0.05f, 0.2f);
                handle.name = "FridgeHandle";
                handle.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Black") as Material;
                

                GameObject handle2 = Instantiate(handle, handle.transform.position, handle.transform.rotation);
                handle2.transform.position += Vector3.up * 0.4f;

                handle.transform.parent = parent.transform;
                handle2.transform.parent = parent.transform;

                parent.transform.localScale += Vector3.up;
            }
            parent.GetComponent<MeshRenderer>().enabled = false;

            return parent;
        }
        public static GameObject Cupboard(GameObject parent)
        {

            //worktops
            float worktopThickness = 0.1f;
            if (parent.name == "Cupboard" || parent.name == "Corner")
            {
                GameObject worktop = GameObject.CreatePrimitive(PrimitiveType.Cube);
               
                worktop.transform.position = parent.transform.position + Vector3.up * (.5f - worktopThickness * .5f);
                worktop.transform.localScale = new Vector3(1f, worktopThickness, 1f);
                worktop.transform.parent = parent.transform;
                worktop.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Metal") as Material;
            }

            //only build a worktop in the corner
            if (parent.name == "Corner")
            {
                parent.GetComponent<MeshRenderer>().enabled = false;
                return parent;
            }

            //always do cupboard?
            if (Random.Range(0,1) == 0)//canceling this, always build
            {
                //how many doors/drawers? (rows/columns)
                if (parent.name == "Cupboard" || parent.name == "TopCupboard")
                {
                    GameObject cupboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cupboard.transform.position = parent.transform.position - Vector3.up * worktopThickness * .5f;
                    cupboard.transform.rotation = parent.transform.rotation;
                    cupboard.transform.localScale = new Vector3(parent.transform.localScale.x, 1f - worktopThickness * 2, parent.transform.localScale.z);
                    cupboard.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Yellow") as Material;
                    cupboard.transform.parent = parent.transform;
                }
                int rows = Random.Range(1,4);
                int columns = Random.Range(1, 4);

                //don't put drawers up top or under sink
                if (parent.name == "TopCupboard" || parent.name == "Sink")
                    rows = 1;
                //just make large doors for under sink
                if (parent.name == "Sink")
                    columns = 1;

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j <columns; j++)
                    {
                        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        door.transform.position = parent.transform.position - Vector3.up * worktopThickness * .5f;//centre of cupboard
                        //x
                        door.transform.position -= parent.transform.right*0.5f;//side of cupobard
                        door.transform.position -= (parent.transform.right * 1f/columns)*.5f;//1 width of door in
                        door.transform.position += ((1f + j) / columns) * parent.transform.right;
                        //y
                        float height = 1f - worktopThickness * 3;
                        float fraction = height / rows;
                        door.transform.position += Vector3.up * height*.5f;
                        door.transform.position -= Vector3.up * (fraction * (i));
                        door.transform.position -= Vector3.up * (fraction * (.5f));
                        //z
                        door.transform.position += parent.transform.forward * (parent.transform.localScale.z*0.5f);
                        door.transform.rotation = parent.transform.rotation;
                        door.transform.localScale = new Vector3((1f - worktopThickness)/columns, ((1f-worktopThickness*3)/rows), 0.1f);
                        door.transform.localScale -= Vector3.up * 0.05f;
                        door.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Green") as Material;
                        door.transform.parent = parent.transform;

                        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        handle.transform.position = door.transform.position;
                        handle.transform.position += parent.transform.forward * (worktopThickness/2);
                        handle.transform.rotation = parent.transform.rotation;
                        handle.transform.localScale = new Vector3(0.1f, 0.05f, 0.05f);
                        if (rows == 1)
                            handle.transform.position += parent.transform.right * ((1f / columns)*0.25f);

                        handle.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Black") as Material;
                        handle.transform.parent = parent.transform;
                    }
                }
                
            }

            parent.GetComponent<MeshRenderer>().enabled = false;
            return parent;
        }
        public static GameObject OvenFront(GameObject room, GameObject parent, float frequency, float bathLength, float bathWidth, float depth, bool outerRim, bool innerRim, float rimWidth, float tapAreaDepth, float cornerRandomness, float secondCornerRandomness, float innerSteepness, bool outsideCurved, bool insideCurved, bool panel)
        {

            List<BezierSpline> splines = new List<BezierSpline>();
            List<List<Vector3>> curvedPoints = new List<List<Vector3>>();
            List<List<Vector3>> straight = new List<List<Vector3>>();
            //outside half points if no case for the bath
            List<Vector3> lowerHalf = new List<Vector3>();
            List<Mesh> meshes = new List<Mesh>();

            int start = 0;
            int end = 2;
            if (outsideCurved && insideCurved)
            {
                //both curved
                start = 0;
                end = 2;
            }
            else if (outsideCurved && !insideCurved)
            {
                Debug.Log("Weird bath, how did it get here");
                start = 1;
                end = 2;
            }
            else if (!outsideCurved && !insideCurved)
            {
                //skip curves loops
                start = 0;
                end = 0;
            }
            else if (!outsideCurved && insideCurved)
            {
                //only do first loop, the inside one
                start = 0;
                end = 1;
            }
            else
                Debug.Log("shouldn't get here");


            for (int s = start; s < end; s++)
            {
                BezierSpline spline = room.AddComponent<BezierSpline>(); //*******MENTAL if you cahnge parent to sink/bath whatever

                //start plotting points form taps and round to the side, then on to the foot of the bath

                //second rim is outside rim, zero the rim
                float bathRimMod = rimWidth;
                if (s == 1)
                    bathRimMod = 0f;

                float tapMod = tapAreaDepth;
                if (s == 1)
                    tapMod = 0f;

                List<Vector3> points = new List<Vector3>();

                //secondCornerRandomness = 1f;
                //start at foot end
                Vector3 bathBase = (-Vector3.forward * 0.5f * bathLength) + ((Vector3.forward * bathRimMod)) + (Vector3.forward * tapMod);
                Vector3 firstCorner = bathBase + (Vector3.right * 0.5f * bathWidth) - (Vector3.right * bathRimMod);// + (Vector3.forward * tapMod);
                Vector3 lerpPoint1a = Vector3.Lerp(bathBase, firstCorner, cornerRandomness);
                Vector3 lerpPoint1b = Vector3.Lerp(firstCorner, bathBase, cornerRandomness);
                Vector3 leftMid = (Vector3.right * bathWidth * 0.5f) - (Vector3.right * bathRimMod);
                Vector3 lerpPoint2a = Vector3.Lerp(firstCorner, leftMid, cornerRandomness);
                Vector3 lerpPoint2b = Vector3.Lerp(leftMid, firstCorner, cornerRandomness);
                Vector3 secondCorner = (Vector3.forward * 0.5f * bathLength) + (Vector3.right * bathWidth * 0.5f) - ((Vector3.forward * bathRimMod)) - (Vector3.right * bathRimMod);
                Vector3 lerpPoint3a = Vector3.Lerp(leftMid, secondCorner, secondCornerRandomness);
                Vector3 lerpPoint3b = Vector3.Lerp(secondCorner, leftMid, secondCornerRandomness);
                Vector3 lastPoint = (Vector3.forward * 0.5f * bathLength) - ((Vector3.forward * bathRimMod));
                Vector3 lerpPoint4 = Vector3.Lerp(secondCorner, lastPoint, cornerRandomness);


                points.Add(bathBase);
                //points.Add(lerpPoint1a);
                //points.Add(lerpPoint1b);
                points.Add(firstCorner);
                // points.Add(firstCorner);
                //points.Add(lerpPoint2a);
                points.Add(lerpPoint2b);
                points.Add(leftMid);
                points.Add(lerpPoint3a);
                //points.Add(lerpPoint3b);
                //points.Add(secondCorner);
                points.Add(secondCorner);
                //points.Add(lerpPoint4);
                //points.Add(lerpPoint4);
                points.Add(lastPoint);

                //give to bezier
                spline.points = points.ToArray();

                splines.Add(spline);

                for (int i = 0; i < points.Count; i++)
                {
                    // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //  c.transform.localScale *= 0.1f;
                    //  c.transform.position = points[i];
                    //  c.name = "Curve";
                }
            }

            foreach (BezierSpline spline in splines)
            {
                float stepSize = frequency;// * items.Length;
                stepSize = 1f / (frequency - 1);
                List<Vector3> tempCurved = new List<Vector3>();

                for (int i = 0; i < frequency; i++)
                {
                    Vector3 position = spline.GetPoint(i * stepSize);

                    tempCurved.Add(position);
                }
                curvedPoints.Add(tempCurved);
            }

            if (outsideCurved && insideCurved)
            {
                //curves loop already built
                start = 0;
                end = 0;
            }
            else if (outsideCurved && !insideCurved)
            {
                Debug.Log("Weird bath, how did it get here");
                start = 0;
                end = 1;
            }
            else if (!outsideCurved && !insideCurved)
            {
                //do both straight lines in loop below
                start = 0;
                end = 2;
            }
            else if (!outsideCurved && insideCurved)
            {
                start = 1;
                end = 2;
            }
            for (int s = start; s < end; s++)
            {
                List<Vector3> temp = new List<Vector3>();
                //second rim is outside rim, zero the rim
                float bathRimMod = rimWidth;
                if (s == 1)
                    bathRimMod = 0f;

                float tapMod = tapAreaDepth;
                if (s == 1)
                    tapMod = 0f;

                //create square corner, start from taps
                List<Vector3> points = new List<Vector3>();
                //taps
                Vector3 taps = -(Vector3.forward * bathLength * 0.5f) + (Vector3.forward * bathRimMod) + (Vector3.forward * tapMod);
                points.Add(taps);
                //
                Vector3 firstCorner = taps + (Vector3.right * bathWidth * 0.5f) - (Vector3.right * bathRimMod);
                points.Add(firstCorner);
                //left point
                Vector3 p = (Vector3.right * bathWidth * 0.5f) - (Vector3.right * bathRimMod);

                points.Add(p);
                //corner point
                p += (bathLength * 0.5f) * Vector3.forward - ((Vector3.forward * bathRimMod));// - (cornerRandomness * Vector3.forward); ;
                points.Add(p);
                //middle top
                p = (bathLength * 0.5f * Vector3.forward) - (Vector3.forward * bathRimMod);// - (cornerRandomness * Vector3.forward); ;
                points.Add(p);

                //now lerp from each point to the next by half frequency

                for (int i = 0; i < points.Count - 1; i++)
                {
                    float distance = Vector3.Distance(points[i], points[i + 1]);
                    Vector3 dir = (points[i + 1] - points[i]).normalized;


                    for (float j = 0; j < distance; j += distance / (frequency / 4))
                    {
                        Vector3 pos = points[i] + dir * j;
                        //temp.Add(pos);
                    }

                    for (float j = 0; j < (frequency / 4); j++)
                    {
                        //lerp needs to be 0 and 1, 
                        float lerp = 1f / (frequency / 4);
                        lerp *= j;

                        Vector3 pos = Vector3.Lerp(points[i], points[i + 1], lerp);
                        temp.Add(pos);
                    }
                }

                straight.Add(temp);
            }

            //fix last point, needs dragged to middle instead of one fraction before it -  just couldnt quite figure out how to extend to the end point without adding an extra point whiihc would then skew the vertice count
            for (int i = 0; i < straight.Count; i++)
            {
                float bathRimMod = rimWidth;
                //zero the rim on the last list ;) Last list is outside points all the time
                if (i == straight.Count - 1)
                    bathRimMod = 0f;

                straight[i][straight[i].Count - 1] = Vector3.forward * bathLength * 0.5f - (Vector3.forward * bathRimMod);
            }

            List<Vector3> insideVertices = new List<Vector3>();
            List<Vector3> outsideVertices = new List<Vector3>();//create logic to grab right on when random
            if (outsideCurved && insideCurved)
            {
                insideVertices = curvedPoints[0];
                outsideVertices = curvedPoints[1];
            }
            else if (outsideCurved && !insideCurved)
            {
                insideVertices = straight[0];
                outsideVertices = curvedPoints[0];
            }
            else if (!outsideCurved && !insideCurved)
            {
                insideVertices = straight[0];
                outsideVertices = straight[1];
            }
            else if (!outsideCurved && insideCurved)
            {
                insideVertices = curvedPoints[0];
                outsideVertices = straight[0];
            }
            //top rim

            //Debug.Log(insideVertices.Count);
            //Debug.Log(outsideVertices.Count);

            List<Vector3> vertices = new List<Vector3>();
            //make sure we add the inside point first - curves built before

            for (int i = 0; i < outsideVertices.Count; i++)
            {
                vertices.Add(insideVertices[i]);
                vertices.Add(outsideVertices[i]);
            }

            List<int> triangles = new List<int>();
            //snake em up
            for (int i = 0; i < vertices.Count - 2; i += 2)
            {
                triangles.Add(i);
                triangles.Add(i + 2);
                triangles.Add(i + 1);

                triangles.Add(i + 1);
                triangles.Add(i + 2);
                triangles.Add(i + 3);
            }


            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            meshes.Add(mesh);
            //get inside points and attach them to a scaled down version of themselves

            if (outerRim)
            {
                //side of lip
                Mesh meshSide = new Mesh();

                vertices = new List<Vector3>();
                //grab outside vertices

                //Debug.Log(outsideVertices.Count);
                for (int i = 0; i < outsideVertices.Count; i++)
                {
                    vertices.Add(outsideVertices[i]);
                    vertices.Add(outsideVertices[i] - (Vector3.up * rimWidth));
                }
                meshSide.vertices = vertices.ToArray();
                meshSide.triangles = triangles.ToArray();

                meshes.Add(meshSide);
            }

            //outside of bath

            if (outerRim)
            {
                Mesh outsideMesh = new Mesh();
                List<Vector3> outerBathVertices = new List<Vector3>();

                for (int i = 0; i < outsideVertices.Count; i++)
                {
                    Vector3 p = outsideVertices[i] - (Vector3.up * rimWidth);


                    //move back half a rim, this will be the thickness of the bottom part of the bath
                    Vector3 dir = outsideVertices[i] - insideVertices[i];
                    //no need to normailze, just half this vector
                    p -= (dir * 0.5f);

                    //Vector3 scaled = outsideVertices[i] * innerSteepness;
                    Vector3 scaled = outsideVertices[i];
                    //stretch the rear of the bottom to the back of the bath so we can put a plug in
                    if (scaled.z > 0)
                    {
                        //work on the top half, scale
                        // scaled.z *= innerSteepness;
                        //make the sides twice as steep as the part you lie on
                        scaled.x *= innerSteepness * 2f;
                    }
                    else
                    {
                        //bottom half, this stretchees the botto of the bath towards the taps
                        scaled.x *= innerSteepness * 2f;
                    }
                    //push whole of bottom slighlty away from taps
                    scaled.z += rimWidth;

                    scaled.y += -depth + rimWidth;
                    // scaled.y -= 0.5f;// + rimWidth;
                    // bottomEdge.Add(scaled);

                    Vector3 b = outsideVertices[i] - Vector3.up;

                    outerBathVertices.Add(p);
                    outerBathVertices.Add(scaled);

                }

                outsideMesh.vertices = outerBathVertices.ToArray();
                outsideMesh.triangles = triangles.ToArray();

                //  meshes.Add(outsideMesh); //this is broken
            }


            if (outerRim)
            {
                //inside lip
                mesh = new Mesh();
                vertices = new List<Vector3>();
                //underneath of lip
                for (int i = 0; i < insideVertices.Count; i++)
                {
                    Vector3 down = insideVertices[i] - (Vector3.up * rimWidth);
                    vertices.Add(down);
                    Vector3 up = insideVertices[i];
                    vertices.Add(up);

                }

                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();

                meshes.Add(mesh);
            }








            //create a list of mirrored meshes 
            List<Mesh> tempMeshes = new List<Mesh>();
            tempMeshes = BathRoomItems.MirrorMeshes(meshes);

            //add to main list
            foreach (Mesh m in tempMeshes)
                meshes.Add(m);

            //one quarter of the bath complete, join the meshes we have created

            //transfer meshes from list to combine instance
            Mesh combinedMesh = new Mesh();
            CombineInstance[] combine = new CombineInstance[meshes.Count];
            for (int i = 0; i < meshes.Count; i++)
            {
                combine[i].mesh = meshes[i];
            }
            //we don't need add transform positions, we are not using them, normally we would need to - skipping in combine meshes()
            //combine, meger in to one sub mesh, and ignore transform positions, we will rotate and position ourselves
            combinedMesh.CombineMeshes(combine, true, false);
            combinedMesh.RecalculateBounds();
            combinedMesh.RecalculateNormals();

            GameObject quarter = new GameObject();
            quarter.transform.parent = parent.transform.parent;
            quarter.name = "OvenFront";
            quarter.transform.position = parent.transform.position;
            quarter.transform.rotation = parent.transform.rotation;
            MeshFilter meshFilterQuarter = quarter.AddComponent<MeshFilter>();
            meshFilterQuarter.mesh = combinedMesh;
            MeshRenderer meshRendererQuarter = quarter.AddComponent<MeshRenderer>();
            Material[] materials = new Material[1];
            materials[0] = Resources.Load("White") as Material;
            // materials[1] = Resources.Load("Blue") as Material; // I failed at submeshes - was tryi to put bath panel in same mesh as different submesh, just making different object now, no probs
            meshRendererQuarter.sharedMaterials = materials;

            //make bath panelling?





            //quarter.transform.parent = parent.transform;
            return quarter;

        }


    }
    public class BathRoomItems
    {
        public static GameObject Bath(GameObject room, GameObject parent, float frequency, float bathLength, float bathWidth, float depth, bool outerRim, bool innerRim, float rimWidth, float tapAreaDepth, float cornerRandomness, float secondCornerRandomness, float innerSteepness, bool outsideCurved, bool insideCurved, bool panel)
        {

            List<BezierSpline> splines = new List<BezierSpline>();
            List<List<Vector3>> curvedPoints = new List<List<Vector3>>();
            List<List<Vector3>> straight = new List<List<Vector3>>();
            //outside half points if no case for the bath
            List<Vector3> lowerHalf = new List<Vector3>();
            List<Mesh> meshes = new List<Mesh>();

            int start = 0;
            int end = 2;
            if (outsideCurved && insideCurved)
            {
                //both curved
                start = 0;
                end = 2;
            }
            else if (outsideCurved && !insideCurved)
            {
                Debug.Log("Weird bath, how did it get here");
                start = 1;
                end = 2;
            }
            else if (!outsideCurved && !insideCurved)
            {
                //skip curves loops
                start = 0;
                end = 0;
            }
            else if (!outsideCurved && insideCurved)
            {
                //only do first loop, the inside one
                start = 0;
                end = 1;
            }
            else
                Debug.Log("shouldn't get here");


            for (int s = start; s < end; s++)
            {
                BezierSpline spline = room.AddComponent<BezierSpline>(); //*******MENTAL if you cahnge parent to sink/bath whatever

                //start plotting points form taps and round to the side, then on to the foot of the bath

                //second rim is outside rim, zero the rim
                float bathRimMod = rimWidth;
                if (s == 1)
                    bathRimMod = 0f;

                float tapMod = tapAreaDepth;
                if (s == 1)
                    tapMod = 0f;

                List<Vector3> points = new List<Vector3>();

                //secondCornerRandomness = 1f;
                //start at foot end
                Vector3 bathBase = (-Vector3.forward * 0.5f * bathLength) + ((Vector3.forward * bathRimMod)) + (Vector3.forward * tapMod);
                Vector3 firstCorner = bathBase + (Vector3.right * 0.5f * bathWidth) - (Vector3.right * bathRimMod);// + (Vector3.forward * tapMod);
                Vector3 lerpPoint1a = Vector3.Lerp(bathBase, firstCorner, cornerRandomness);
                Vector3 lerpPoint1b = Vector3.Lerp(firstCorner, bathBase, cornerRandomness);
                Vector3 leftMid = (Vector3.right * bathWidth * 0.5f) - (Vector3.right * bathRimMod);
                Vector3 lerpPoint2a = Vector3.Lerp(firstCorner, leftMid, cornerRandomness);
                Vector3 lerpPoint2b = Vector3.Lerp(leftMid, firstCorner, cornerRandomness);
                Vector3 secondCorner = (Vector3.forward * 0.5f * bathLength) + (Vector3.right * bathWidth * 0.5f) - ((Vector3.forward * bathRimMod)) - (Vector3.right * bathRimMod);
                Vector3 lerpPoint3a = Vector3.Lerp(leftMid, secondCorner, secondCornerRandomness);
                Vector3 lerpPoint3b = Vector3.Lerp(secondCorner, leftMid, secondCornerRandomness);
                Vector3 lastPoint = (Vector3.forward * 0.5f * bathLength) - ((Vector3.forward * bathRimMod));
                Vector3 lerpPoint4 = Vector3.Lerp(secondCorner, lastPoint, cornerRandomness);


                points.Add(bathBase);
                //points.Add(lerpPoint1a);
                //points.Add(lerpPoint1b);
                points.Add(firstCorner);
                // points.Add(firstCorner);
                //points.Add(lerpPoint2a);
                points.Add(lerpPoint2b);
                points.Add(leftMid);
                points.Add(lerpPoint3a);
                //points.Add(lerpPoint3b);
                //points.Add(secondCorner);
                points.Add(secondCorner);
                //points.Add(lerpPoint4);
                //points.Add(lerpPoint4);
                points.Add(lastPoint);

                //adjsut curve for storey height.. surprised this works
                for (int i = 0; i < points.Count; i++)
                {
                    // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //  c.transform.localScale *= 0.1f;
                    //  c.transform.position = points[i];
                    //  c.name = "Curve";
                    points[i] = new Vector3(points[i].x,points[i].y - room.transform.position.y,points[i].z);

                }

                //give to bezier
                spline.points = points.ToArray();

                splines.Add(spline);

                
            }

            foreach (BezierSpline spline in splines)
            {
                float stepSize = frequency;// * items.Length;
                stepSize = 1f / (frequency - 1);
                List<Vector3> tempCurved = new List<Vector3>();

                for (int i = 0; i < frequency; i++)
                {
                    Vector3 position = spline.GetPoint(i * stepSize);

                    tempCurved.Add(position);
                }
                curvedPoints.Add(tempCurved);
            }

            if (outsideCurved && insideCurved)
            {
                //curves loop already built
                start = 0;
                end = 0;
            }
            else if (outsideCurved && !insideCurved)
            {
                Debug.Log("Weird bath, how did it get here");
                start = 0;
                end = 1;
            }
            else if (!outsideCurved && !insideCurved)
            {
                //do both straight lines in loop below
                start = 0;
                end = 2;
            }
            else if (!outsideCurved && insideCurved)
            {
                start = 1;
                end = 2;
            }
            for (int s = start; s < end; s++)
            {
                List<Vector3> temp = new List<Vector3>();
                //second rim is outside rim, zero the rim
                float bathRimMod = rimWidth;
                if (s == 1)
                    bathRimMod = 0f;

                float tapMod = tapAreaDepth;
                if (s == 1)
                    tapMod = 0f;

                //create square corner, start from taps
                List<Vector3> points = new List<Vector3>();
                //taps
                Vector3 taps = -(Vector3.forward * bathLength * 0.5f) + (Vector3.forward * bathRimMod) + (Vector3.forward * tapMod);
                points.Add(taps);
                //
                Vector3 firstCorner = taps + (Vector3.right * bathWidth * 0.5f) - (Vector3.right * bathRimMod);
                points.Add(firstCorner);
                //left point
                Vector3 p = (Vector3.right * bathWidth * 0.5f) - (Vector3.right * bathRimMod);

                points.Add(p);
                //corner point
                p += (bathLength * 0.5f) * Vector3.forward - ((Vector3.forward * bathRimMod));// - (cornerRandomness * Vector3.forward); ;
                points.Add(p);
                //middle top
                p = (bathLength * 0.5f * Vector3.forward) - (Vector3.forward * bathRimMod);// - (cornerRandomness * Vector3.forward); ;
                points.Add(p);

                //now lerp from each point to the next by half frequency

                for (int i = 0; i < points.Count - 1; i++)
                {
                    float distance = Vector3.Distance(points[i], points[i + 1]);
                    Vector3 dir = (points[i + 1] - points[i]).normalized;


                    for (float j = 0; j < distance; j += distance / (frequency / 4))
                    {
                        Vector3 pos = points[i] + dir * j;
                        //temp.Add(pos);
                    }

                    for (float j = 0; j < (frequency / 4); j++)
                    {
                        //lerp needs to be 0 and 1, 
                        float lerp = 1f / (frequency / 4);
                        lerp *= j;

                        Vector3 pos = Vector3.Lerp(points[i], points[i + 1], lerp);
                        temp.Add(pos);
                    }
                }

                straight.Add(temp);
            }

            //fix last point, needs dragged to middle instead of one fraction before it -  just couldnt quite figure out how to extend to the end point without adding an extra point whiihc would then skew the vertice count
            for (int i = 0; i < straight.Count; i++)
            {
                float bathRimMod = rimWidth;
                //zero the rim on the last list ;) Last list is outside points all the time
                if (i == straight.Count - 1)
                    bathRimMod = 0f;

                straight[i][straight[i].Count - 1] = Vector3.forward * bathLength * 0.5f - (Vector3.forward * bathRimMod);
            }

            List<Vector3> insideVertices = new List<Vector3>();
            List<Vector3> outsideVertices = new List<Vector3>();//create logic to grab right on when random
            if (outsideCurved && insideCurved)
            {
                insideVertices = curvedPoints[0];
                outsideVertices = curvedPoints[1];
            }
            else if (outsideCurved && !insideCurved)
            {
                insideVertices = straight[0];
                outsideVertices = curvedPoints[0];
            }
            else if (!outsideCurved && !insideCurved)
            {
                insideVertices = straight[0];
                outsideVertices = straight[1];
            }
            else if (!outsideCurved && insideCurved)
            {
                insideVertices = curvedPoints[0];
                outsideVertices = straight[0];
            }
            //top rim

            //Debug.Log(insideVertices.Count);
            //Debug.Log(outsideVertices.Count);

            List<Vector3> vertices = new List<Vector3>();
            //make sure we add the inside point first - curves built before

            for (int i = 0; i < outsideVertices.Count; i++)
            {
                vertices.Add(insideVertices[i]);
                vertices.Add(outsideVertices[i]);
            }

            List<int> triangles = new List<int>();
            //snake em up
            for (int i = 0; i < vertices.Count - 2; i += 2)
            {
                triangles.Add(i);
                triangles.Add(i + 2);
                triangles.Add(i + 1);

                triangles.Add(i + 1);
                triangles.Add(i + 2);
                triangles.Add(i + 3);
            }


            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            meshes.Add(mesh);
            //get inside points and attach them to a scaled down version of themselves

            if (outerRim)
            {
                //side of lip
                Mesh meshSide = new Mesh();

                vertices = new List<Vector3>();
                //grab outside vertices

                //Debug.Log(outsideVertices.Count);
                for (int i = 0; i < outsideVertices.Count; i++)
                {
                    vertices.Add(outsideVertices[i]);
                    vertices.Add(outsideVertices[i] - (Vector3.up * rimWidth));
                }
                meshSide.vertices = vertices.ToArray();
                meshSide.triangles = triangles.ToArray();

                meshes.Add(meshSide);
            }


            if (outerRim)
            {
                Mesh meshUnder = new Mesh();
                vertices = new List<Vector3>();
                //underneath of lip
                for (int i = 0; i < outsideVertices.Count; i++)
                {
                    Vector3 p = outsideVertices[i] - (Vector3.up * rimWidth);
                    vertices.Add(p);

                    //move back half a rim, this will be the thickness of the bottom part of the bath
                    Vector3 dir = outsideVertices[i] - insideVertices[i];
                    //no need to normailze, just half this vector
                    p -= (dir * 0.5f);
                    vertices.Add(p);
                }

                meshUnder.vertices = vertices.ToArray();
                meshUnder.triangles = triangles.ToArray();

                meshes.Add(meshUnder);
            }

            //outside of bath

            if (outerRim)
            {
                Mesh outsideMesh = new Mesh();
                List<Vector3> outerBathVertices = new List<Vector3>();

                for (int i = 0; i < outsideVertices.Count; i++)
                {
                    Vector3 p = outsideVertices[i] - (Vector3.up * rimWidth);


                    //move back half a rim, this will be the thickness of the bottom part of the bath
                    Vector3 dir = outsideVertices[i] - insideVertices[i];
                    //no need to normailze, just half this vector
                    p -= (dir * 0.5f);

                    //Vector3 scaled = outsideVertices[i] * innerSteepness;
                    Vector3 scaled = outsideVertices[i];
                    //stretch the rear of the bottom to the back of the bath so we can put a plug in
                    if (scaled.z > 0)
                    {
                        //work on the top half, scale
                        scaled.z *= innerSteepness;
                        //make the sides twice as steep as the part you lie on
                        scaled.x *= innerSteepness * 2f;
                    }
                    else
                    {
                        //bottom half, this stretchees the botto of the bath towards the taps
                        scaled.x *= innerSteepness * 2f;
                    }
                    //push whole of bottom slighlty away from taps
                    scaled.z += rimWidth;

                    scaled.y += -depth + rimWidth;
                    // scaled.y -= 0.5f;// + rimWidth;
                    // bottomEdge.Add(scaled);

                    Vector3 b = outsideVertices[i] - Vector3.up;

                    outerBathVertices.Add(p);
                    outerBathVertices.Add(scaled);

                }

                outsideMesh.vertices = outerBathVertices.ToArray();
                outsideMesh.triangles = triangles.ToArray();

                meshes.Add(outsideMesh);
            }


            if (outerRim)
            {
                //inside lip
                mesh = new Mesh();
                vertices = new List<Vector3>();
                //underneath of lip
                for (int i = 0; i < insideVertices.Count; i++)
                {
                    Vector3 down = insideVertices[i] - (Vector3.up * rimWidth);
                    vertices.Add(down);
                    Vector3 up = insideVertices[i];
                    vertices.Add(up);

                }

                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();

                meshes.Add(mesh);
            }
            if (outerRim)
            {
                //main inside part/slope
                mesh = new Mesh();
                List<Vector3> v = new List<Vector3>();
                //underneath of lip
                for (int i = 0; i < insideVertices.Count; i++)
                {
                    Vector3 scaled = insideVertices[i];
                    //stretch the rear of the bottom to the back of the bath so we can put a plug in
                    if (scaled.z > 0)
                    {
                        //work on the top half, scale - make more slopey? sink is symmetrical
                        if (parent.transform.name == "SinkParent")
                            scaled.z *= innerSteepness * 2;
                        else
                            scaled.z *= innerSteepness;
                        //make the sides twice as steep as the part you lie on
                        scaled.x *= innerSteepness * 2f;
                    }
                    else
                    {
                        //bottom half, this stretchees the botto of the bath towards the taps
                        scaled.x *= innerSteepness * 2f;


                    }
                    //push whole of bottom slightly away from taps
                    scaled.z += rimWidth;

                    scaled.y += -depth + rimWidth;
                    v.Add(scaled);

                    Vector3 down = insideVertices[i] - (Vector3.up * rimWidth);
                    v.Add(down);
                }

                mesh.vertices = v.ToArray();
                mesh.triangles = triangles.ToArray();

                meshes.Add(mesh);
            }

            //outside of bath

            if (!outerRim && !innerRim)
            {
                //stretch outside vertices to the ground
                Mesh outsideMesh = new Mesh();
                List<Vector3> outerBathVertices = new List<Vector3>();
                //underneath of lip
                for (int i = 0; i < outsideVertices.Count; i++)
                {
                    /*
                    Vector3 p = outsideVertices[i];

                    Vector3 scaled = outsideVertices[i] * (innerSteepness + rimWidth);
                    scaled.y -= 0.5f;// + rimWidth;
                                     // bottomEdge.Add(scaled);

                    Vector3 b = outsideVertices[i] - Vector3.up;

                    outerBathVertices.Add(p);
                    outerBathVertices.Add(scaled);
                    */
                    Vector3 scaled = outsideVertices[i];
                    //stretch the rear of the bottom to the back of the bath so we can put a plug in
                    if (scaled.z > 0)
                    {
                        //work on the top half, scale
                        scaled.z *= innerSteepness;
                        //make the sides twice as steep as the part you lie on
                        scaled.x *= innerSteepness * 2f;
                    }
                    else
                    {
                        //bottom half, this stretchees the botto of the bath towards the taps
                        scaled.x *= innerSteepness * 2f;


                    }
                    //push whole of bottom slighlty away from taps
                    scaled.z += rimWidth;

                    scaled.y += -depth + rimWidth;


                    Vector3 mid = new Vector3(0f, scaled.y, 0f);
                    //join from centre point, build inside ring always first
                    outerBathVertices.Add(outsideVertices[i]);//plus bottomdepth

                    //use same maths as above to create bottom lip
                    //scale this down

                    outerBathVertices.Add(scaled);

                }

                outsideMesh.vertices = outerBathVertices.ToArray();
                outsideMesh.triangles = triangles.ToArray();

                meshes.Add(outsideMesh);
            }
            if (!outerRim && !innerRim)
            {
                //main inside part/slope
                mesh = new Mesh();
                List<Vector3> v = new List<Vector3>();

                for (int i = 0; i < insideVertices.Count; i++)
                {
                    Vector3 scaled = insideVertices[i];
                    //stretch the rear of the bottom to the back of the bath so we can put a plug in
                    if (scaled.z > 0)
                    {
                        //work on the top half, scale
                        scaled.z *= innerSteepness;
                        //make the sides twice as steep as the part you lie on
                        scaled.x *= innerSteepness * 2f;
                    }
                    else
                    {
                        //bottom half, this stretchees the botto of the bath towards the taps
                        scaled.x *= innerSteepness * 2f;


                    }
                    //push whole of bottom slighlty away from taps
                    scaled.z += rimWidth;

                    scaled.y += -depth + rimWidth;

                    v.Add(scaled);

                    Vector3 down = insideVertices[i];// - (Vector3.up * rimWidth);
                    v.Add(down);
                }
                mesh.vertices = v.ToArray();
                mesh.triangles = triangles.ToArray();

                meshes.Add(mesh);
            }

            //bottom section
            //we have already made out bottomEdge, so all we need to do is create a flat panel, adding all points to a centre point should be enough
            List<Vector3> bottomPanel = new List<Vector3>();
            for (int i = 0; i < frequency; i++)
            {
                Vector3 scaled = insideVertices[i];
                //stretch the rear of the bottom to the back of the bath so we can put a plug in
                if (scaled.z > 0)
                {
                    if (parent.transform.name == "SinkParent")
                        scaled.z *= innerSteepness * 2;
                    else
                        scaled.z *= innerSteepness;
                    //make the sides twice as steep as the part you lie on
                    scaled.x *= innerSteepness * 2f;
                }
                else
                {
                    //bottom half, this stretchees the botto of the bath towards the taps
                    scaled.x *= innerSteepness * 2f;


                }
                //push whole of bottom slighlty away from taps
                scaled.z += rimWidth;

                scaled.y += -depth + rimWidth;


                Vector3 mid = new Vector3(0f, scaled.y, 0f);
                //join from centre point, build inside ring always first
                bottomPanel.Add(mid);//plus bottomdepth

                //use same maths as above to create bottom lip
                //scale this down

                bottomPanel.Add(scaled);
            }
            Mesh bottom = new Mesh();
            bottom.vertices = bottomPanel.ToArray();
            bottom.triangles = triangles.ToArray();
            meshes.Add(bottom);




            //create a list of mirrored meshes 
            List<Mesh> tempMeshes = new List<Mesh>();
            tempMeshes = MirrorMeshes(meshes);

            //add to main list
            foreach (Mesh m in tempMeshes)
                meshes.Add(m);

            //one quarter of the bath complete, join the meshes we have created

            //transfer meshes from list to combine instance
            Mesh combinedMesh = new Mesh();
            CombineInstance[] combine = new CombineInstance[meshes.Count];
            for (int i = 0; i < meshes.Count; i++)
            {
                combine[i].mesh = meshes[i];
            }
            //we don't need add transform positions, we are not using them, normally we would need to - skipping in combine meshes()
            //combine, meger in to one sub mesh, and ignore transform positions, we will rotate and position ourselves
            combinedMesh.CombineMeshes(combine, true, false);
            combinedMesh.RecalculateBounds();
            combinedMesh.RecalculateNormals();

            GameObject quarter = new GameObject();
            quarter.transform.parent = parent.transform.parent;
            quarter.name = "Inner Bath";
            quarter.transform.position = parent.transform.position;
            quarter.transform.rotation = parent.transform.rotation;
            MeshFilter meshFilterQuarter = quarter.AddComponent<MeshFilter>();
            meshFilterQuarter.mesh = combinedMesh;
            MeshRenderer meshRendererQuarter = quarter.AddComponent<MeshRenderer>();
            Material[] materials = new Material[1];
            materials[0] = Resources.Load("White") as Material;
            // materials[1] = Resources.Load("Blue") as Material; // I failed at submeshes - was tryi to put bath panel in same mesh as different submesh, just making different object now, no probs
            meshRendererQuarter.sharedMaterials = materials;

            //make bath panelling?


            if (panel)
            {
                float depthForPanel = depth;
                if (parent.name == "SinkParent")
                    depthForPanel = 1 - (rimWidth + 0.1f);
                Mesh panelMesh = BathPanel(outsideVertices, parent, frequency, rimWidth, depthForPanel);
                List<Mesh> panelMeshes = new List<Mesh>();
                panelMeshes.Add(panelMesh);
                List<Mesh> mirrored = MirrorMeshes(panelMeshes);
                panelMeshes.Add(mirrored[0]);


                CombineInstance[] panelCombine = new CombineInstance[panelMeshes.Count];
                for (int i = 0; i < panelMeshes.Count; i++)
                {
                    panelCombine[i].mesh = panelMeshes[i];
                }

                Mesh panelCombinedMesh = new Mesh();
                panelCombinedMesh.CombineMeshes(panelCombine, true, false);
                panelCombinedMesh.RecalculateBounds();
                panelCombinedMesh.RecalculateNormals();

                GameObject panelObj = new GameObject();
                panelObj.name = "Panel";
                panelObj.transform.position = parent.transform.position;
                panelObj.transform.rotation = parent.transform.rotation;
                MeshFilter panelObjMf = panelObj.AddComponent<MeshFilter>();
                panelObjMf.mesh = panelCombinedMesh;
                MeshRenderer mRpanel = panelObj.AddComponent<MeshRenderer>();
                mRpanel.sharedMaterial = Resources.Load("Door") as Material;

                panelObj.transform.parent = quarter.transform;

            }

            //plug
            if (parent.name == "Bath" || parent.name == "Sink" || parent.name == "Shower" || parent.name == "SinkParent")
            {
                //find plug position
                //end of bath
                Vector3 position = parent.transform.position - (parent.transform.forward * (bathLength * 0.5f));
                //forward tap and rim width// add an extra rim width- the botto panel start at rin*2
                position += (tapAreaDepth + (rimWidth * 3)) * parent.transform.forward;
                //lower
                position.y += -depth + rimWidth;//bath depth?

                GameObject plug = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                plug.name = "Plug";
                plug.transform.position = position;
                Vector3 scale = Vector3.one * 0.05f;
                scale.y *= 0.1f;
                plug.transform.localScale = scale;

                plug.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Black") as Material;

                plug.transform.parent = quarter.transform;
            }
            if (parent.name == "Toilet")
            {
                GameObject toiletSeat = ToiletSeat(outsideVertices, insideVertices, parent, rimWidth, bathLength, tapAreaDepth);
                toiletSeat.transform.rotation = parent.transform.rotation;
                toiletSeat.transform.parent = quarter.transform;

            }

            //quarter.transform.parent = parent.transform;
            return quarter;

        }

        public static GameObject Sink(GameObject room, GameObject parent, float frequency, float bathLength, float bathWidth, float depth, bool outerRim, bool innerRim, float rimWidth, float tapAreaDepth, float cornerRandomness, float secondCornerRandomness, float innerSteepness, bool outsideCurved, bool insideCurved, bool panel)
        {

            List<BezierSpline> splines = new List<BezierSpline>();
            List<List<Vector3>> curvedPoints = new List<List<Vector3>>();
            List<List<Vector3>> straight = new List<List<Vector3>>();
            //outside half points if no case for the bath
            List<Vector3> lowerHalf = new List<Vector3>();
            List<Mesh> meshes = new List<Mesh>();

            int start = 0;
            int end = 2;
            if (outsideCurved && insideCurved)
            {
                //both curved
                start = 0;
                end = 2;
            }
            else if (outsideCurved && !insideCurved)
            {
                Debug.Log("Weird bath, how did it get here");
                start = 1;
                end = 2;
            }
            else if (!outsideCurved && !insideCurved)
            {
                //skip curves loops
                start = 0;
                end = 0;
            }
            else if (!outsideCurved && insideCurved)
            {
                //only do first loop, the inside one
                start = 0;
                end = 1;
            }
            else
                Debug.Log("shouldn't get here");


            for (int s = start; s < end; s++)
            {
                BezierSpline spline = room.AddComponent<BezierSpline>(); //*******MENTAL if you cahnge parent to sink/bath whatever

                //start plotting points form taps and round to the side, then on to the foot of the bath

                //second rim is outside rim, zero the rim
                float bathRimMod = rimWidth;
                if (s == 1)
                    bathRimMod = 0f;

                float tapMod = tapAreaDepth;
                if (s == 1)
                    tapMod = 0f;

                List<Vector3> points = new List<Vector3>();

                //secondCornerRandomness = 1f;
                //start at foot end
                Vector3 bathBase = (-Vector3.forward * 0.5f * bathLength) + ((Vector3.forward * bathRimMod)) + (Vector3.forward * tapMod);
                Vector3 firstCorner = bathBase + (Vector3.right * 0.5f * bathWidth) - (Vector3.right * bathRimMod);// + (Vector3.forward * tapMod);
                Vector3 lerpPoint1a = Vector3.Lerp(bathBase, firstCorner, cornerRandomness);
                Vector3 lerpPoint1b = Vector3.Lerp(firstCorner, bathBase, cornerRandomness);
                Vector3 leftMid = (Vector3.right * bathWidth * 0.5f) - (Vector3.right * bathRimMod);
                Vector3 lerpPoint2a = Vector3.Lerp(firstCorner, leftMid, cornerRandomness);
                Vector3 lerpPoint2b = Vector3.Lerp(leftMid, firstCorner, cornerRandomness);
                Vector3 secondCorner = (Vector3.forward * 0.5f * bathLength) + (Vector3.right * bathWidth * 0.5f) - ((Vector3.forward * bathRimMod)) - (Vector3.right * bathRimMod);
                Vector3 lerpPoint3a = Vector3.Lerp(leftMid, secondCorner, secondCornerRandomness);
                Vector3 lerpPoint3b = Vector3.Lerp(secondCorner, leftMid, secondCornerRandomness);
                Vector3 lastPoint = (Vector3.forward * 0.5f * bathLength) - ((Vector3.forward * bathRimMod));
                Vector3 lerpPoint4 = Vector3.Lerp(secondCorner, lastPoint, cornerRandomness);


                points.Add(bathBase);
                //points.Add(lerpPoint1a);
                //points.Add(lerpPoint1b);
                points.Add(firstCorner);
                // points.Add(firstCorner);
                //points.Add(lerpPoint2a);
                points.Add(lerpPoint2b);
                points.Add(leftMid);
                points.Add(lerpPoint3a);
                //points.Add(lerpPoint3b);
                //points.Add(secondCorner);
                points.Add(secondCorner);
                //points.Add(lerpPoint4);
                //points.Add(lerpPoint4);
                points.Add(lastPoint);

                //give to bezier
                spline.points = points.ToArray();

                splines.Add(spline);

                for (int i = 0; i < points.Count; i++)
                {
                    // GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //  c.transform.localScale *= 0.1f;
                    //  c.transform.position = points[i];
                    //  c.name = "Curve";
                }
            }

            foreach (BezierSpline spline in splines)
            {
                float stepSize = frequency;// * items.Length;
                stepSize = 1f / (frequency - 1);
                List<Vector3> tempCurved = new List<Vector3>();

                for (int i = 0; i < frequency; i++)
                {
                    Vector3 position = spline.GetPoint(i * stepSize);

                    tempCurved.Add(position);
                }
                curvedPoints.Add(tempCurved);
            }

            if (outsideCurved && insideCurved)
            {
                //curves loop already built
                start = 0;
                end = 0;
            }
            else if (outsideCurved && !insideCurved)
            {
                Debug.Log("Weird bath, how did it get here");
                start = 0;
                end = 1;
            }
            else if (!outsideCurved && !insideCurved)
            {
                //do both straight lines in loop below
                start = 0;
                end = 2;
            }
            else if (!outsideCurved && insideCurved)
            {
                start = 1;
                end = 2;
            }
            for (int s = start; s < end; s++)
            {
                List<Vector3> temp = new List<Vector3>();
                //second rim is outside rim, zero the rim
                float bathRimMod = rimWidth;
                if (s == 1)
                    bathRimMod = 0f;

                float tapMod = tapAreaDepth;
                if (s == 1)
                    tapMod = 0f;

                //create square corner, start from taps
                List<Vector3> points = new List<Vector3>();
                //taps
                Vector3 taps = -(Vector3.forward * bathLength * 0.5f) + (Vector3.forward * bathRimMod) + (Vector3.forward * tapMod);
                points.Add(taps);
                //
                Vector3 firstCorner = taps + (Vector3.right * bathWidth * 0.5f) - (Vector3.right * bathRimMod);
                points.Add(firstCorner);
                //left point
                Vector3 p = (Vector3.right * bathWidth * 0.5f) - (Vector3.right * bathRimMod);

                points.Add(p);
                //corner point
                p += (bathLength * 0.5f) * Vector3.forward - ((Vector3.forward * bathRimMod));// - (cornerRandomness * Vector3.forward); ;
                points.Add(p);
                //middle top
                p = (bathLength * 0.5f * Vector3.forward) - (Vector3.forward * bathRimMod);// - (cornerRandomness * Vector3.forward); ;
                points.Add(p);

                //now lerp from each point to the next by half frequency

                for (int i = 0; i < points.Count - 1; i++)
                {
                    float distance = Vector3.Distance(points[i], points[i + 1]);
                    Vector3 dir = (points[i + 1] - points[i]).normalized;


                    for (float j = 0; j < distance; j += distance / (frequency / 4))
                    {
                        Vector3 pos = points[i] + dir * j;
                        //temp.Add(pos);
                    }

                    for (float j = 0; j < (frequency / 4); j++)
                    {
                        //lerp needs to be 0 and 1, 
                        float lerp = 1f / (frequency / 4);
                        lerp *= j;

                        Vector3 pos = Vector3.Lerp(points[i], points[i + 1], lerp);
                        temp.Add(pos);
                    }
                }

                straight.Add(temp);
            }

            //fix last point, needs dragged to middle instead of one fraction before it -  just couldnt quite figure out how to extend to the end point without adding an extra point whiihc would then skew the vertice count
            for (int i = 0; i < straight.Count; i++)
            {
                float bathRimMod = rimWidth;
                //zero the rim on the last list ;) Last list is outside points all the time
                if (i == straight.Count - 1)
                    bathRimMod = 0f;

                straight[i][straight[i].Count - 1] = Vector3.forward * bathLength * 0.5f - (Vector3.forward * bathRimMod);
            }

            List<Vector3> insideVertices = new List<Vector3>();
            List<Vector3> outsideVertices = new List<Vector3>();//create logic to grab right on when random
            if (outsideCurved && insideCurved)
            {
                insideVertices = curvedPoints[0];
                outsideVertices = curvedPoints[1];
            }
            else if (outsideCurved && !insideCurved)
            {
                insideVertices = straight[0];
                outsideVertices = curvedPoints[0];
            }
            else if (!outsideCurved && !insideCurved)
            {
                insideVertices = straight[0];
                outsideVertices = straight[1];
            }
            else if (!outsideCurved && insideCurved)
            {
                insideVertices = curvedPoints[0];
                outsideVertices = straight[0];
            }
            //top rim

            //Debug.Log(insideVertices.Count);
            //Debug.Log(outsideVertices.Count);

            List<Vector3> vertices = new List<Vector3>();
            //make sure we add the inside point first - curves built before

            for (int i = 0; i < outsideVertices.Count; i++)
            {
                vertices.Add(insideVertices[i]);
                vertices.Add(outsideVertices[i]);
            }

            List<int> triangles = new List<int>();
            //snake em up
            for (int i = 0; i < vertices.Count - 2; i += 2)
            {
                triangles.Add(i);
                triangles.Add(i + 2);
                triangles.Add(i + 1);

                triangles.Add(i + 1);
                triangles.Add(i + 2);
                triangles.Add(i + 3);
            }


            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            meshes.Add(mesh);
            //get inside points and attach them to a scaled down version of themselves

            if (outerRim)
            {
                //side of lip
                Mesh meshSide = new Mesh();

                vertices = new List<Vector3>();
                //grab outside vertices

                //Debug.Log(outsideVertices.Count);
                for (int i = 0; i < outsideVertices.Count; i++)
                {
                    vertices.Add(outsideVertices[i]);
                    vertices.Add(outsideVertices[i] - (Vector3.up * rimWidth));
                }
                meshSide.vertices = vertices.ToArray();
                meshSide.triangles = triangles.ToArray();

                meshes.Add(meshSide);
            }

            //outside of bath

            if (outerRim)
            {
                Mesh outsideMesh = new Mesh();
                List<Vector3> outerBathVertices = new List<Vector3>();

                for (int i = 0; i < outsideVertices.Count; i++)
                {
                    Vector3 p = outsideVertices[i] - (Vector3.up * rimWidth);


                    //move back half a rim, this will be the thickness of the bottom part of the bath
                    Vector3 dir = outsideVertices[i] - insideVertices[i];
                    //no need to normailze, just half this vector
                    p -= (dir * 0.5f);

                    //Vector3 scaled = outsideVertices[i] * innerSteepness;
                    Vector3 scaled = outsideVertices[i];
                    //stretch the rear of the bottom to the back of the bath so we can put a plug in
                    if (scaled.z > 0)
                    {
                        //work on the top half, scale
                        // scaled.z *= innerSteepness;
                        //make the sides twice as steep as the part you lie on
                        scaled.x *= innerSteepness * 2f;
                    }
                    else
                    {
                        //bottom half, this stretchees the botto of the bath towards the taps
                        scaled.x *= innerSteepness * 2f;
                    }
                    //push whole of bottom slighlty away from taps
                    scaled.z += rimWidth;

                    scaled.y += -depth + rimWidth;
                    // scaled.y -= 0.5f;// + rimWidth;
                    // bottomEdge.Add(scaled);

                    Vector3 b = outsideVertices[i] - Vector3.up;

                    outerBathVertices.Add(p);
                    outerBathVertices.Add(scaled);

                }

                outsideMesh.vertices = outerBathVertices.ToArray();
                outsideMesh.triangles = triangles.ToArray();

                //  meshes.Add(outsideMesh); //this is broken
            }


            if (outerRim)
            {
                //inside lip
                mesh = new Mesh();
                vertices = new List<Vector3>();
                //underneath of lip
                for (int i = 0; i < insideVertices.Count; i++)
                {
                    Vector3 down = insideVertices[i] - (Vector3.up * rimWidth);
                    vertices.Add(down);
                    Vector3 up = insideVertices[i];
                    vertices.Add(up);

                }

                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();

                meshes.Add(mesh);
            }
            if (outerRim)
            {
                //main inside part/slope
                mesh = new Mesh();
                List<Vector3> v = new List<Vector3>();
                //underneath of lip
                for (int i = 0; i < insideVertices.Count; i++)
                {
                    Vector3 scaled = insideVertices[i];
                    //stretch the rear of the bottom to the back of the bath so we can put a plug in
                    scaled.x *= (parent.transform.localScale.x * .5f) - rimWidth;
                    if (scaled.z > 0)
                    {
                        //work on the top half, scale - make more slopey? sink is symmetrical

                        scaled.z -= rimWidth;
                        //make the sides twice as steep as the part you lie on

                    }
                    else
                    {
                        //bottom half, this stretchees the botto of the bath towards the taps

                        scaled.z += rimWidth;

                    }
                    //push whole of bottom slightly away from taps


                    scaled.y += -depth + rimWidth;
                    v.Add(scaled);

                    Vector3 down = insideVertices[i] - (Vector3.up * rimWidth);
                    v.Add(down);
                }

                mesh.vertices = v.ToArray();
                mesh.triangles = triangles.ToArray();

                meshes.Add(mesh);
            }


            //bottom section
            //we have already made out bottomEdge, so all we need to do is create a flat panel, adding all points to a centre point should be enough
            List<Vector3> bottomPanel = new List<Vector3>();
            for (int i = 0; i < frequency; i++)
            {
                Vector3 scaled = insideVertices[i];
                scaled.x *= (parent.transform.localScale.x * .5f) - rimWidth;
                if (scaled.z > 0)
                {
                    //work on the top half, scale - make more slopey? sink is symmetrical

                    scaled.z -= rimWidth;
                    //make the sides twice as steep as the part you lie on

                }
                else
                {
                    //bottom half, this stretchees the botto of the bath towards the taps

                    scaled.z += rimWidth;

                }

                // scaled.z += rimWidth;

                scaled.y += -depth + rimWidth;


                Vector3 mid = new Vector3(0f, scaled.y, 0f);
                //join from centre point, build inside ring always first
                bottomPanel.Add(mid);//plus bottomdepth

                //use same maths as above to create bottom lip
                //scale this down

                bottomPanel.Add(scaled);
            }
            Mesh bottom = new Mesh();
            bottom.vertices = bottomPanel.ToArray();
            bottom.triangles = triangles.ToArray();
            meshes.Add(bottom);




            //create a list of mirrored meshes 
            List<Mesh> tempMeshes = new List<Mesh>();
            tempMeshes = MirrorMeshes(meshes);

            //add to main list
            foreach (Mesh m in tempMeshes)
                meshes.Add(m);

            //one quarter of the bath complete, join the meshes we have created

            //transfer meshes from list to combine instance
            Mesh combinedMesh = new Mesh();
            CombineInstance[] combine = new CombineInstance[meshes.Count];
            for (int i = 0; i < meshes.Count; i++)
            {
                combine[i].mesh = meshes[i];
            }
            //we don't need add transform positions, we are not using them, normally we would need to - skipping in combine meshes()
            //combine, meger in to one sub mesh, and ignore transform positions, we will rotate and position ourselves
            combinedMesh.CombineMeshes(combine, true, false);
            combinedMesh.RecalculateBounds();
            combinedMesh.RecalculateNormals();

            GameObject quarter = new GameObject();
            quarter.transform.parent = parent.transform.parent;
            quarter.name = "Inner Bath";
            quarter.transform.position = parent.transform.position;
            quarter.transform.rotation = parent.transform.rotation;
            MeshFilter meshFilterQuarter = quarter.AddComponent<MeshFilter>();
            meshFilterQuarter.mesh = combinedMesh;
            MeshRenderer meshRendererQuarter = quarter.AddComponent<MeshRenderer>();
            Material[] materials = new Material[1];
            materials[0] = Resources.Load("White") as Material;
            // materials[1] = Resources.Load("Blue") as Material; // I failed at submeshes - was tryi to put bath panel in same mesh as different submesh, just making different object now, no probs
            meshRendererQuarter.sharedMaterials = materials;

            //make bath panelling?


            if (panel)
            {
                float depthForPanel = depth;
                if (parent.name == "SinkParent")
                    depthForPanel = 1 - (rimWidth + 0.1f);
                Mesh panelMesh = BathPanel(outsideVertices, parent, frequency, rimWidth, depthForPanel);
                List<Mesh> panelMeshes = new List<Mesh>();
                panelMeshes.Add(panelMesh);
                List<Mesh> mirrored = MirrorMeshes(panelMeshes);
                panelMeshes.Add(mirrored[0]);


                CombineInstance[] panelCombine = new CombineInstance[panelMeshes.Count];
                for (int i = 0; i < panelMeshes.Count; i++)
                {
                    panelCombine[i].mesh = panelMeshes[i];
                }

                Mesh panelCombinedMesh = new Mesh();
                panelCombinedMesh.CombineMeshes(panelCombine, true, false);
                panelCombinedMesh.RecalculateBounds();
                panelCombinedMesh.RecalculateNormals();

                GameObject panelObj = new GameObject();
                panelObj.name = "Panel";
                panelObj.transform.position = parent.transform.position;
                panelObj.transform.rotation = parent.transform.rotation;
                MeshFilter panelObjMf = panelObj.AddComponent<MeshFilter>();
                panelObjMf.mesh = panelCombinedMesh;
                MeshRenderer mRpanel = panelObj.AddComponent<MeshRenderer>();
                mRpanel.sharedMaterial = Resources.Load("Door") as Material;

                panelObj.transform.parent = quarter.transform;

            }

            //plug
            if (parent.name == "Bath" || parent.name == "Sink" || parent.name == "Shower" || parent.name == "SinkParent")
            {
                //find plug position
                //end of bath
                Vector3 position = parent.transform.position - (parent.transform.forward * (bathLength * 0.5f));
                //forward tap and rim width// add an extra rim width- the botto panel start at rin*2
                position += (tapAreaDepth + (rimWidth * 3)) * parent.transform.forward;
                //lower
                position.y += -depth + rimWidth;//bath depth?

                GameObject plug = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                plug.name = "Plug";
                plug.transform.position = position;
                Vector3 scale = Vector3.one * 0.05f;
                scale.y *= 0.1f;
                plug.transform.localScale = scale;

                plug.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Black") as Material;

                plug.transform.parent = quarter.transform;
            }
            if (parent.name == "Toilet")
            {
                GameObject toiletSeat = ToiletSeat(outsideVertices, insideVertices, parent, rimWidth, bathLength, tapAreaDepth);
                toiletSeat.transform.rotation = parent.transform.rotation;
                toiletSeat.transform.parent = quarter.transform;

            }

            //quarter.transform.parent = parent.transform;
            return quarter;

        }

        public static GameObject ToiletSeat(List<Vector3> outsideVertices, List<Vector3> insideVertices, GameObject parent, float rimWidth, float length, float tapAreaDepth)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Mesh> meshes = new List<Mesh>();
            //make sure we add the inside point first - curves built before

            for (int i = 0; i < outsideVertices.Count; i++)
            {
                vertices.Add(insideVertices[i] * (1f - rimWidth));//inside
                vertices.Add(insideVertices[i] * (1f + rimWidth * 5));//dunno why 5//outside                
            }

            List<int> triangles = new List<int>();
            //snake em up
            for (int i = 0; i < vertices.Count - 2; i += 2)
            {
                triangles.Add(i);
                triangles.Add(i + 2);
                triangles.Add(i + 1);

                triangles.Add(i + 1);
                triangles.Add(i + 2);
                triangles.Add(i + 3);
            }
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            meshes.Add(mesh);
            //outer depth
            vertices = new List<Vector3>();
            triangles = new List<int>();

            float depth = 0.05f;
            for (int i = 0; i < outsideVertices.Count; i++)
            {
                //add a little depth
                vertices.Add(insideVertices[i] * (1f + rimWidth * 5));//dunno why 5
                vertices.Add(insideVertices[i] * (1f + rimWidth * 5) - Vector3.up * depth);
            }
            //snake em up
            for (int i = 0; i < vertices.Count - 2; i += 2)
            {
                triangles.Add(i);
                triangles.Add(i + 2);
                triangles.Add(i + 1);

                triangles.Add(i + 1);
                triangles.Add(i + 2);
                triangles.Add(i + 3);
            }
            mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            meshes.Add(mesh);

            //inner depth
            vertices = new List<Vector3>();
            triangles = new List<int>();
            for (int i = 0; i < insideVertices.Count; i++)
            {
                //add a little depth
                vertices.Add(insideVertices[i] * (1f - rimWidth) - Vector3.up * depth);
                vertices.Add(insideVertices[i] * (1f - rimWidth));//dunno why 5

            }
            //snake em up
            for (int i = 0; i < vertices.Count - 2; i += 2)
            {
                triangles.Add(i);
                triangles.Add(i + 2);
                triangles.Add(i + 1);

                triangles.Add(i + 1);
                triangles.Add(i + 2);
                triangles.Add(i + 3);
            }
            mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            meshes.Add(mesh);
            //bottom banel of upper
            vertices = new List<Vector3>();
            triangles = new List<int>();
            for (int i = 0; i < insideVertices.Count; i++)
            {
                vertices.Add(insideVertices[i] * (1f + rimWidth * 5) - Vector3.up * depth);//dunno why 5//outside          
                vertices.Add(insideVertices[i] * (1f - rimWidth) - Vector3.up * depth);//inside                

            }
            //snake em up
            for (int i = 0; i < vertices.Count - 2; i += 2)
            {
                triangles.Add(i);
                triangles.Add(i + 2);
                triangles.Add(i + 1);

                triangles.Add(i + 1);
                triangles.Add(i + 2);
                triangles.Add(i + 3);
            }
            mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            meshes.Add(mesh);

            //create a list of mirrored meshes 
            List<Mesh> tempMeshes = new List<Mesh>();
            tempMeshes = MirrorMeshes(meshes);

            //add to main list
            foreach (Mesh m in tempMeshes)
                meshes.Add(m);

            GameObject lowerSeat = BuildSection(meshes, "Toilet Seat Lower", parent);

            //upper seat
            meshes = new List<Mesh>();

            vertices = new List<Vector3>();
            triangles = new List<int>();
            for (int i = 0; i < outsideVertices.Count; i++)
            {

                vertices.Add(Vector3.zero);//dunno why 5
                vertices.Add(insideVertices[i] * (1f + rimWidth * 5));
            }
            for (int i = 0; i < vertices.Count - 2; i += 2)
            {
                triangles.Add(i);
                triangles.Add(i + 2);
                triangles.Add(i + 1);

                triangles.Add(i + 1);
                triangles.Add(i + 2);
                triangles.Add(i + 3);
            }
            mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            meshes.Add(mesh);

            //upper outer depth
            vertices = new List<Vector3>();
            triangles = new List<int>();
            for (int i = 0; i < outsideVertices.Count; i++)
            {

                vertices.Add(insideVertices[i] * (1f + rimWidth * 5));
                vertices.Add(insideVertices[i] * (1f + rimWidth * 5) - Vector3.up * depth);
            }
            for (int i = 0; i < vertices.Count - 2; i += 2)
            {
                triangles.Add(i);
                triangles.Add(i + 2);
                triangles.Add(i + 1);

                triangles.Add(i + 1);
                triangles.Add(i + 2);
                triangles.Add(i + 3);
            }
            mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            meshes.Add(mesh);

            //upper seat bottom panel

            vertices = new List<Vector3>();
            triangles = new List<int>();
            for (int i = 0; i < outsideVertices.Count; i++)
            {
                vertices.Add(insideVertices[i] * (1f + rimWidth * 5) - Vector3.up * depth);
                vertices.Add(Vector3.zero - Vector3.up * depth);//dunno why 5                
            }
            for (int i = 0; i < vertices.Count - 2; i += 2)
            {
                triangles.Add(i);
                triangles.Add(i + 2);
                triangles.Add(i + 1);

                triangles.Add(i + 1);
                triangles.Add(i + 2);
                triangles.Add(i + 3);
            }
            mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            meshes.Add(mesh);

            //create a list of mirrored meshes 
            tempMeshes = new List<Mesh>();
            tempMeshes = MirrorMeshes(meshes);

            //add to main list
            foreach (Mesh m in tempMeshes)
                meshes.Add(m);


            GameObject seat = new GameObject();
            seat.name = "Seat";
            seat.transform.position = parent.transform.position;
            seat.transform.rotation = parent.transform.rotation;

            GameObject upperSeat = BuildSection(meshes, "Toilet Seat Upper", seat);
            upperSeat.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Black") as Material;
            upperSeat.transform.localScale = new Vector3(0.97f, 1f, 0.97f);//shrink a little

            GameObject water = Instantiate(upperSeat);
            water.transform.localScale = new Vector3(0.5f, 1f, 0.5f);//shrink a little
            water.transform.parent = parent.transform;
            water.transform.localPosition = new Vector3(0,0.3f, 0);//y height guess good enough?
            water.transform.rotation = parent.transform.rotation;

            water.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;
            water.name = "ToiletWater";
            

            GameObject upperHinge = new GameObject();
            upperHinge.name = "UpperHinge";
            upperHinge.transform.position = parent.transform.position - parent.transform.forward * (length * 0.5f) + parent.transform.forward * (tapAreaDepth);// + rimWidth);
            upperHinge.transform.position += Vector3.up * depth;
            upperHinge.transform.rotation = parent.transform.rotation;
            upperSeat.transform.parent = upperHinge.transform;
            upperHinge.transform.parent = seat.transform;

            lowerSeat.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Black") as Material;

            GameObject lowerHinge = new GameObject();
            lowerHinge.name = "LowerHinge";
            lowerHinge.transform.position = parent.transform.position - parent.transform.forward * (length * 0.5f) + parent.transform.forward * (tapAreaDepth + rimWidth);
            lowerHinge.transform.rotation = parent.transform.rotation;
            lowerSeat.transform.parent = lowerHinge.transform;
            lowerHinge.transform.parent = seat.transform;

            //add random upper lid rotation//make it mostly up
            if (Random.Range(0, 5) != 0)
                upperHinge.transform.rotation *= Quaternion.Euler(-92, 0, 0);

            lowerHinge.transform.position += Vector3.up * depth;
            upperHinge.transform.position += Vector3.up * 0.04f;

            //rubber feet are missing, perhaps add with bottle placing code, yet to be written?

            return seat;
        }

        public static GameObject ShowerController(GameObject parent, float length, float width)
        {
            GameObject unit = new GameObject();
            unit.name = "Shower Controller";
            //unit.transform.transform.parent = parent.transform; //if we do this here, it messes with rounded cube component - hacked fix, do after rounded cube script, other than that, could make cube script a function and call it first
            RoundedCube rc = unit.AddComponent<RoundedCube>();
            rc.enabled = false;
            int detail = 10;
            rc.xSize = detail;
            rc.ySize = detail;
            rc.zSize = detail;
            rc.roundness = Random.Range(1, 5);

            //scale it down a lot because the script which creates the rounded cube uses in to specify xSize etc
            float x = Random.Range(.01f, .03f);
            float y = Random.Range(.01f, .03f);
            float z = Random.Range(.025f, .03f); //needs to min 0.025 so tube can attach at same z co-ord
            Vector3 scale = new Vector3(x, y, z);
            //scale the whole box down            
            unit.transform.localScale = scale;
            //buttons

            unit.transform.position = parent.transform.position - (parent.transform.forward * length * 0.5f) + Vector3.up * Random.Range(1.75f, 2f) - (parent.transform.right * Random.Range(x * detail, (width * 0.5f) - (x * detail)));


            float limit = x;
            if (y > x)
                limit = y;

            Vector3 centre = unit.transform.position + parent.transform.forward * detail * 0.5f * z;
            float offset = Random.Range(0f, limit);
            for (int i = -1; i <= 1; i += 2)
            {
                Vector3 dir = Vector3.zero;
                Vector3 dir2 = Vector3.zero;
                if (limit == x)
                {
                    //make direction go left/right
                    dir = parent.transform.right;
                    dir2 = Vector3.up;
                }

                else
                {
                    dir = Vector3.up;
                    dir2 = parent.transform.right;
                }
                PrimitiveType shape = PrimitiveType.Cube;
                if (Random.Range(0, 2) == 0)
                    shape = PrimitiveType.Cylinder;
                else
                    shape = PrimitiveType.Cube;

                GameObject button = GameObject.CreatePrimitive(shape);
                if (i == -1)
                    button.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                else if (i == 1)
                    button.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Red") as Material;

                float yLimit = limit / 4;
                if (shape == PrimitiveType.Cube)
                    yLimit = limit / 2;

                Vector3 s = new Vector3(limit, yLimit, limit);
                button.transform.localScale = s * 2;
                button.transform.position = unit.transform.position;
                button.transform.position += parent.transform.forward * z * detail * 0.5f;
                //add sideways dir/or up
                button.transform.position += (dir * (limit + offset) * i);
                //add offset only if buttons are arranged upwards, looks weird if sideways
                if (limit == y)
                    button.transform.position += dir2 * offset * i;

                button.transform.rotation = parent.transform.rotation;
                button.transform.rotation *= Quaternion.Euler(90, 0, 0);

                button.transform.parent = parent.transform;//putting as unit changes positin, does it matter if only bath parent? not atm
            }

            unit.transform.rotation = parent.transform.rotation;
            unit.transform.rotation *= Quaternion.Euler(0, 180, 0);
            unit.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("White") as Material;
            unit.GetComponent<RoundedCube>().enabled = true;

            //wait for cube script to finish then change parent. Mucks up otherwise
            unit.transform.parent = parent.transform;

            return unit;

        }

        public static GameObject ShowerScreen(GameObject room, GameObject parent, float bathWidth, float bathLength, float YAdjust, int amountOfPanels)
        {
            float frameWidth = Random.Range(0.01f, 0.03f);
            float frameHeight = 4f + YAdjust;//will break if changed
            GameObject screen = new GameObject();
            screen.name = "Screen";
            screen.transform.parent = parent.transform;
            //check which side of bath is clsoer to the cetnre of the room
            Vector3 centreOfRoom = room.GetComponent<MeshRenderer>().bounds.center;            
            Vector3 sideDir = parent.transform.right;
            if (Vector3.Distance(parent.transform.position - sideDir, centreOfRoom) < Vector3.Distance(parent.transform.position + sideDir, centreOfRoom))
                sideDir = -sideDir;


            screen.transform.position = parent.transform.position - (bathLength * parent.transform.forward * 0.5f) + (sideDir * (bathWidth * 0.5f));

            for (int i = 0; i < amountOfPanels; i++)
            {


                //create pivot point
                GameObject pivotObj = new GameObject();
                pivotObj.name = "Pivot";

                pivotObj.transform.position = parent.transform.position - (bathLength * parent.transform.forward * 0.5f) + (sideDir * (bathWidth * 0.5f));
                pivotObj.transform.position += parent.transform.forward * bathLength * i;
                //adjust for frame width
                // pivotObj.transform.position += parent.transform.forward * frameWidth * 0.5f;
                // pivotObj.transform.position = sideDir * frameWidth * 0.5f;
                pivotObj.transform.rotation = parent.transform.rotation;


                GameObject glassWithFrame = GlassWithFrame(bathWidth, bathLength, frameHeight, 0.01f, 0.04f, true);
                glassWithFrame.transform.position = pivotObj.transform.position + pivotObj.transform.forward * bathLength * 0.5f + Vector3.up * frameHeight * 0.25f;// + pivotObj.transform.forward * frameWidth * 0.5f;
                glassWithFrame.transform.rotation = parent.transform.rotation;
                glassWithFrame.transform.parent = pivotObj.transform;

                pivotObj.transform.parent = screen.transform;

            }
            return screen;

            
        }

        public static GameObject GlassWithFrame(float width, float length, float height, float glasssThickness, float frameWidth, bool addRunner)
        {
            GameObject glassWithFrame = new GameObject();
            glassWithFrame.name = "Glass With Frame";

            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glass.transform.parent = glassWithFrame.transform;
            glass.name = "Glass";
            glass.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Glass") as Material;

            Vector3 scale = new Vector3(glasssThickness, (height / 2) - frameWidth, length - frameWidth);
            glass.transform.localScale = scale;

            GameObject frameObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameObj.transform.parent = glassWithFrame.transform;
            frameObj.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("White") as Material;
            frameObj.name = "FrameShower";
            frameObj.transform.position += (Vector3.up * height / 4) - Vector3.up * frameWidth * 0.25f;//
            frameObj.transform.localScale = new Vector3(frameWidth * 0.5f, frameWidth * 0.5f, length);// + (frameWidth*0.5f));

            frameObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameObj.transform.parent = glassWithFrame.transform;
            frameObj.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("White") as Material;
            frameObj.name = "FrameShower2";
            frameObj.transform.position += Vector3.forward * length / 2 - Vector3.forward * frameWidth * 0.25f;
            frameObj.transform.localScale = new Vector3(frameWidth * 0.5f, (height / 2) - frameWidth, frameWidth * 0.5f);// + (frameWidth*0.5f));


            frameObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameObj.transform.parent = glassWithFrame.transform;
            frameObj.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("White") as Material;
            frameObj.name = "FrameShower3";
            frameObj.transform.position -= Vector3.forward * length / 2 - Vector3.forward * frameWidth * 0.25f;
            frameObj.transform.localScale = new Vector3(frameWidth * 0.5f, (height / 2) - frameWidth, frameWidth * 0.5f);// + (frameWidth*0.5f));


            frameObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameObj.transform.parent = glassWithFrame.transform;
            frameObj.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("White") as Material;
            frameObj.name = "FrameShower4";
            frameObj.transform.position -= (Vector3.up * height / 4) - Vector3.up * frameWidth * 0.25f;
            frameObj.transform.localScale = new Vector3(frameWidth * 0.5f, frameWidth * 0.5f, length);// + (frameWidth*0.5f));


            if (addRunner)
            {
                float runnerThickness = 0.01f;
                //bump whole object up 
                for (int i = 0; i < glassWithFrame.transform.childCount; i++)
                {
                    glassWithFrame.transform.GetChild(i).transform.position += (runnerThickness * 0.5f) * Vector3.up;
                }

                frameObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                frameObj.transform.parent = glassWithFrame.transform;
                frameObj.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Black") as Material;
                frameObj.name = "Runner";
                frameObj.transform.position -= (Vector3.up * height / 4);// - Vector3.up * runnerThickness*0.5f;
                frameObj.transform.localScale = new Vector3(frameWidth, runnerThickness, length);// + (frameWidth*0.5f));
            }


            return glassWithFrame;
        }

        public static GameObject ShowerCurtain(GameObject room, GameObject parent, float width, float length, float height, float cornerDepth, float frequency)
        {
            

            GameObject RailAndCurtain = new GameObject();
            RailAndCurtain.name = "Shower Rail";

            //check which side of bath is clsoer to the cetnre of the room
            Vector3 centreOfRoom = room.GetComponent<MeshRenderer>().bounds.center;
            Vector3 sideDir = parent.transform.right;
            if (Vector3.Distance(parent.transform.position - sideDir, centreOfRoom) < Vector3.Distance(parent.transform.position + sideDir, centreOfRoom))
                sideDir = -sideDir;

            //craete all the points we need to defin the shower rail
            Vector3 start = (sideDir * width * 0.5f) - (parent.transform.forward * length * 0.5f) + (height * Vector3.up);

            Vector3 endOfBath = (sideDir * width * 0.5f) + (parent.transform.forward * length * 0.5f) + (height * Vector3.up);

            Vector3 startOfCorner = endOfBath - (parent.transform.forward * cornerDepth);

            Vector3 endOfCorner = endOfBath - (sideDir * cornerDepth);

            Vector3 wall = endOfBath - (sideDir * width);

            //create curve with these

            BezierSpline spline = RailAndCurtain.AddComponent<BezierSpline>();

            List<Vector3> points = new List<Vector3>();

            //curve is drawn between p2 and p4 with the points before them influencing their shape
            points.Add(Vector3.Lerp(startOfCorner, endOfBath, 0.5f));
            points.Add(endOfBath);
            points.Add(endOfBath);
            points.Add(Vector3.Lerp(endOfBath, endOfCorner, 0.5f));

            spline.points = points.ToArray();

            float stepSize = frequency;// * items.Length;
            stepSize = 1f / (frequency - 1);

            List<Vector3> tubePoints = new List<Vector3>();
            //first add the wall
            tubePoints.Add(start);
            for (int i = 0; i < frequency; i++)
            {
                //then add from corner curve
                Vector3 p = spline.GetPoint(i * stepSize);
                tubePoints.Add(p);

                //GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //c.transform.position = p;
                //c.transform.localScale *= 0.1f;
            }
            //now add end
            tubePoints.Add(wall);

            List<float> thicknesses = new List<float>();
            thicknesses.Add(0.01f);
            Mesh tubeMesh = TubeBuilder.TubeMesh(tubePoints, thicknesses, 8, new Vector3(0, 90, 0));

            GameObject tube = new GameObject();
            tube.name = "Rail";
            tube.transform.position = parent.transform.position;

            MeshFilter meshFilter = tube.AddComponent<MeshFilter>();
            meshFilter.mesh = tubeMesh;
            MeshRenderer meshRenderer = tube.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = Resources.Load("Metal") as Material;


            //curtain
            //use the curve we already made to plot a grid
            List<Vector3> vertices = new List<Vector3>();
            float clothDetail = 0.1f;

            for (float j = 0; j < height; j += clothDetail)
            {
                for (float i = 0; i < length * 0.5f; i += clothDetail)
                {

                    Vector3 p = start + (parent.transform.forward * i) - (Vector3.up * j);

                    //GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //c.transform.position = p;
                    //c.transform.localScale *= 0.01f;
                    vertices.Add(p);
                }
            }



            List<int> triangles = new List<int>();
            int rowLength = Mathf.RoundToInt(length * 0.5f / clothDetail);// Debug.Log(rowLength); //ok?
            int heightLength = Mathf.RoundToInt(height / clothDetail);
            //ruffle
            for (int i = rowLength; i < vertices.Count; i++)
            {
                vertices[i] += Random.Range(-0.01f, 0.01f) * Vector3.one;
            }
            for (int i = 0; i < rowLength - 1; i++)
            {
                for (int j = 0; j < heightLength - 1; j++)
                {
                    triangles.Add(i + (j * rowLength));
                    triangles.Add(i + 1 + (j * rowLength));
                    triangles.Add(i + rowLength + (j * rowLength));

                    //can skip these to make cool pattern?

                    triangles.Add(i + 1 + (j * rowLength));
                    triangles.Add(i + rowLength + 1 + (j * rowLength));
                    triangles.Add(i + rowLength + (j * rowLength));

                }
            }

            //make cloth http://answers.unity3d.com/questions/966554/set-unity-5-cloth-constraints-from-code.html

            //also need back of curtain

            Mesh curtainMesh = new Mesh();
            curtainMesh.vertices = vertices.ToArray();
            curtainMesh.triangles = triangles.ToArray();
            curtainMesh.RecalculateNormals();

            GameObject curtain = new GameObject();
            curtain.transform.position = parent.transform.position;
            curtain.name = "Curtain";
            meshFilter = curtain.AddComponent<MeshFilter>();
            meshFilter.mesh = curtainMesh;
            meshRenderer = curtain.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = Resources.Load("White") as Material;

            //make back of curtain by duplicating and, then reverse normals

            Mesh curtainMesh2 = new Mesh();
            curtainMesh2.vertices = vertices.ToArray();
            curtainMesh2.triangles = triangles.ToArray();
            curtainMesh2.RecalculateNormals();

            GameObject curtain2 = new GameObject();
            curtain2.transform.position = parent.transform.position;
            
            curtain2.name = "CurtainBack";
            MeshFilter meshFilter2 = curtain2.AddComponent<MeshFilter>();
            meshFilter2.mesh = curtainMesh2;
            MeshRenderer meshRenderer2 = curtain2.AddComponent<MeshRenderer>();
            meshRenderer2.sharedMaterial = Resources.Load("White") as Material;
            ReverseNormals(curtain2);


            RailAndCurtain.transform.position = parent.transform.position;
            RailAndCurtain.transform.parent = parent.transform;
            tube.transform.parent = RailAndCurtain.transform;
            curtain.transform.parent = RailAndCurtain.transform;
            curtain2.transform.parent = RailAndCurtain.transform;
            return RailAndCurtain;
        }

        public static GameObject ShowerHead(GameObject parent, float width, float length, float height)
        {
            GameObject showerHead = new GameObject();
            showerHead.name = "ShowerHead";

            Vector3 position = (parent.transform.forward * -length * 0.5f) + (Vector3.up * height);
            //position += parent.transform.position;
            //showerHead.transform.position = position;

            //create a curve

            BezierSpline spline = showerHead.AddComponent<BezierSpline>();
            List<Vector3> points = new List<Vector3>();

            float showerHeadLength = 0.2f;

            //tube builder must build on it's side, so tilt points

            //cp point
            points.Add(Vector3.zero);
            //p1
            Vector3 p1 = (Vector3.forward * showerHeadLength);
            points.Add(Vector3.Lerp(Vector3.zero, p1, 0.5f));
            points.Add(p1);
            //end
            Vector3 endOfShaft = (Vector3.forward * showerHeadLength * 0.5f) + (Vector3.right * showerHeadLength * 0.5f);

            points.Add(endOfShaft);
            points.Add(endOfShaft);


            spline.points = points.ToArray();

            float frequency = 12f;
            float stepSize = (1 / (frequency - 1));
            List<Vector3> tubePoints = new List<Vector3>();
            List<float> thicknesses = new List<float>();
            //thicknesses.Add(0.01f);
            //add points and thickness at this points to lists
            for (int i = 0; i < frequency; i++)
            {
                Vector3 p = spline.GetPoint(i * stepSize);
                tubePoints.Add(p);

            }
            float stemThickness = Random.Range(0.01f, 0.02f);
            //create thicknesses
            for (int i = 0; i < frequency * 2 / 3; i++)
            {
                thicknesses.Add(stemThickness);
            }
            float stemMultiplier = Random.Range(0.01f, 0.05f);
            for (int i = 0; i < frequency / 3; i++)
            {
                thicknesses.Add(stemThickness + i * stemMultiplier); //this is pretty random but makes some decent shapes
            }
            //tighten last one
            thicknesses[thicknesses.Count - 1] = stemThickness * 2;
            //Debug.Log(tubePoints.Count);

            //use these lists to create tube
            Mesh mesh = new Mesh();
            mesh = TubeBuilder.TubeMesh(tubePoints, thicknesses, 12, new Vector3(0, 90, 0));
            mesh.RecalculateNormals();
            //Debug.Log(mesh.vertexCount);
            MeshFilter mf = showerHead.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            MeshRenderer mr = showerHead.AddComponent<MeshRenderer>();
            mr.sharedMaterial = Resources.Load("White") as Material;

            //add holding cylinder
            GameObject holder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            holder.name = "Shower Holder";
            holder.transform.parent = showerHead.transform;
            //shower head will be positioned correctly, we can parent it and use a zero local position
            holder.transform.localPosition = Vector3.zero;
            holder.transform.localScale *= stemThickness * 3;
            holder.transform.localPosition += (Vector3.up * stemThickness * 2); //up because shower head gets rotated
            
            //add holding tube
            //add holding cylinder
            GameObject holdingTube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            holdingTube.name = "Shower Holding Tube";
            //holdingTube.transform.parent = showerHead.transform;
            //shower head will be positioned correctly, we can parent it and use a zero local position
            Vector3 tubePos = new Vector3();
            tubePos = parent.transform.position - parent.transform.forward * 0.5f * length + parent.transform.forward * 0.1f; //last one is moving out from wall 
            tubePos.y += 1.5f;// 
            tubePos += parent.transform.right * stemThickness * 3;
            holdingTube.transform.position = tubePos;
            holdingTube.transform.rotation = Quaternion.identity;
            float tubeHeight = 0.3f;
            Vector3 scale = new Vector3(stemThickness, tubeHeight, stemThickness);
            holdingTube.transform.localScale = scale;
            holdingTube.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Metal") as Material;
            //add cylinders to pin tube to the wall

            GameObject t1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            t1.name = "Tube Attachment to Wall";
            t1.transform.rotation = parent.transform.rotation;
            t1.transform.rotation *= Quaternion.Euler(90, 0, 0);/// * t1.transform.rotation;
            scale = new Vector3(stemThickness * 3, stemThickness * 4, stemThickness * 3);
            t1.transform.localScale = scale;
            t1.transform.position = tubePos;// parent.transform.position - parent.transform.forward * 0.5f * length + parent.transform.forward * 0.1f; //last one is moving out from wall  
            t1.transform.position += -Vector3.up * tubeHeight - parent.transform.forward * (0.1f * 0.5f) + parent.transform.forward * (stemThickness * 0.5f);

            GameObject t2 = Instantiate(t1, t1.transform.position, t1.transform.rotation);
            t2.transform.position += Vector3.up * tubeHeight*2;

            showerHead.transform.position = (parent.transform.forward * -length * 0.5f) + Vector3.up *( height ) + parent.transform.position;
            //move out from wall
            showerHead.transform.position += parent.transform.forward * 0.1f;
            showerHead.transform.position -= Vector3.up * (Random.Range(0.1f, 0.3f));
            showerHead.transform.rotation = parent.transform.rotation;
            //tilt randomly
            showerHead.transform.rotation *= Quaternion.Euler(Random.Range(-70, -45), 0, -90);
            showerHead.transform.parent = parent.transform;

            holdingTube.transform.parent = parent.transform;
            t1.transform.parent = parent.transform;
            t2.transform.parent = parent.transform;

            return showerHead;
        }

        public static GameObject Taps(GameObject parent, float objectWidth, float objectLength, float tapAreaDepth, float rimWidth)
        {
            GameObject taps = new GameObject();
            taps.name = "Taps";
            taps.transform.position = parent.transform.position;

            Vector3 centre = -(parent.transform.forward * objectLength * 0.5f) + parent.transform.position + (parent.transform.forward * tapAreaDepth) + (parent.transform.forward * rimWidth * 0.5f);
            centre.y = parent.transform.transform.position.y;

            float tapGap = Random.Range(0.05f, 0.15f);
            float tapSize = Random.Range(0.05f, rimWidth);
            //create two taps at each side of centre

            Vector3 scale = new Vector3(tapSize, Random.Range(0.05f, 0.2f), tapSize);
            for (int i = 0; i < 2; i++)
            {
                Vector3 dir = parent.transform.right * tapGap;
                if (i == 0)
                    dir = -dir;

                Vector3 offset = centre + dir;

                GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Destroy(c.GetComponent<CapsuleCollider>());
                c.transform.position = offset;
                c.transform.localScale = scale;
                c.transform.rotation = parent.transform.rotation;
                c.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Metal") as Material;



                GameObject c2 = Instantiate(c);
                Destroy(c2.GetComponent<CapsuleCollider>());
                c2.transform.localScale *= 0.5f;
                c2.transform.rotation *= Quaternion.Euler(90, 0, 0);// * c.transform.rotation;
                c2.transform.position += Vector3.up * (scale.y * 0.5f);
                c2.transform.position += c.transform.forward * (tapSize * 0.5f);

                c.transform.parent = taps.transform;
                c2.transform.parent = taps.transform;
            }

            taps.transform.parent = parent.transform;
            return taps;
        }

        public static GameObject ShowerTube(GameObject showerHead, GameObject showerController, GameObject bath)
        {
            //tube from head to box
            //create curve
            GameObject showerTube = new GameObject();
            BezierSpline spline = showerTube.AddComponent<BezierSpline>();
            List<Vector3> splinePoints = new List<Vector3>();
            float yDist = showerHead.transform.position.y - showerController.transform.position.y;
            Vector3 p1 = Vector3.zero;
            Vector3 p2 = p1 - Vector3.up * yDist - (Vector3.up * 0.5f);
            Vector3 p3 = (showerController.transform.position - showerHead.transform.position) - Vector3.up * 0.5f;
            p3 += bath.transform.forward * 0.1f;
            Vector3 p4 = showerController.transform.position - showerHead.transform.position;
            p4 += bath.transform.forward * 0.1f;

            splinePoints.Add(p1);
            splinePoints.Add(p2);
            splinePoints.Add(p3);
            splinePoints.Add(p4);

            spline.points = splinePoints.ToArray();
            //select points along curve
            float frequency = 20f;
            float stepSize = 1 / (frequency - 1);
            List<Vector3> tubePoints = new List<Vector3>();
            List<float> thicknesses = new List<float>();
            for (int i = 0; i < frequency; i++)
            {
                Vector3 p = spline.GetPoint(i * stepSize);
                tubePoints.Add(p);
                thicknesses.Add(0.005f);

            }
            //send to tube function
            //axis
            //find if showerhead and contrller are aligned on the x, or z axis - this is to stop twists in the tube mesh
            //round floats, small differences 
            Vector3 axis = new Vector3(90, 0, 0);//works with right, works with left
            float p1ZRounded = Mathf.RoundToInt(p1.z * 10);
            float p4ZRounded = Mathf.RoundToInt(p4.z * 10);
            if (p1ZRounded == p4ZRounded)
                axis = new Vector3(0, 0, 90);

            Mesh tubeMesh = TubeBuilder.TubeMesh(tubePoints, thicknesses, 10, axis);

            MeshFilter mf = showerTube.AddComponent<MeshFilter>();
            mf.mesh = tubeMesh;
            MeshRenderer mr = showerTube.AddComponent<MeshRenderer>();
            mr.sharedMaterial = Resources.Load("Metal") as Material;

            showerTube.transform.position = showerHead.transform.position;
            
            showerTube.name = "Shower Tube";
            showerTube.transform.parent = bath.transform;

            return showerTube;
        }

        private static Mesh BathPanel(List<Vector3> outsidePoints, GameObject bath, float frequency, float bathRim, float height)
        {
            //  Debug.Log(outsidePoints.Count);
            //bottom panel
            //we have already made out bottomEdge, so all we need to do is create a flat panel, adding all points to a centre point should be enough
            List<Vector3> panel = new List<Vector3>();
            for (int i = 0; i < outsidePoints.Count; i++)
            {
                panel.Add(outsidePoints[i] - (Vector3.up * bathRim));
                panel.Add(outsidePoints[i] - Vector3.up * height);

                //   GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //   c.transform.position = outsidePoints[i];
                //  c.transform.localScale *= 0.1f;

            }

            List<int> bottomTris = new List<int>();
            for (int i = 0; i < panel.Count - 2; i += 2)
            {
                bottomTris.Add(i);
                bottomTris.Add(i + 2);
                bottomTris.Add(i + 1);

                bottomTris.Add(i + 1);
                bottomTris.Add(i + 2);
                bottomTris.Add(i + 3);
            }
            Mesh bottom = new Mesh();
            bottom.vertices = panel.ToArray();
            bottom.triangles = bottomTris.ToArray();


            return bottom;

        }

        public static GameObject Cistern(GameObject parent)
        {
            GameObject cistern = new GameObject();
            cistern.name = "Cistern";

            cistern.transform.position = parent.transform.position;
            RoundedCube rc = cistern.AddComponent<RoundedCube>();

            int detail = 5;
            rc.xSize = detail;
            rc.ySize = detail;
            rc.zSize = detail;
            rc.roundness = Random.Range(1, 2);

            cistern.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("White") as Material;

            return cistern;
        }

        public static List<Mesh> MirrorMeshes(List<Mesh> meshes)
        {
            List<Mesh> tempMeshes = new List<Mesh>();
            foreach (Mesh m in meshes)
            {
                //let's cheat and copy this mesh, then spin on x axis, and flip normals, this will give us the other side of the top end of the bath
                Mesh duplicateMesh = new Mesh();


                Vector3[] v = m.vertices;
                //flip vertices around for each direction
                float x = 0;
                float y = 0;
                float z = 0;

                x = 0;
                y = 0;
                z = 180;

                Quaternion rot = Quaternion.Euler(x, y, z);

                //spin vertices, and reverse heights-  think it of it like flipping it over so it is upside down, and pulling the high vertices downwards (making it inside out)
                for (int i = 0; i < v.Length; i++)
                {
                    v[i] = rot * v[i];
                    v[i].y = -v[i].y;
                }

                //reverse triangles
                List<int> t = new List<int>();
                //snake em up
                for (int i = 0; i < v.Length - 2; i += 2)
                {
                    if (m.name != "Exterior")
                    {
                        t.Add(i);
                        t.Add(i + 1);
                        t.Add(i + 2);

                        t.Add(i + 1);
                        t.Add(i + 3);
                        t.Add(i + 2);
                    }
                    //if exterior mesh we have to flip, not sure why.. but, working
                    else if (m.name == "Exterior")
                    {
                        t.Add(i);
                        t.Add(i + 2);
                        t.Add(i + 1);

                        t.Add(i + 1);
                        t.Add(i + 2);
                        t.Add(i + 3);
                    }
                }

                duplicateMesh.vertices = v;
                //duplicateMesh.triangles = t.ToArray();

                duplicateMesh.SetTriangles(t, 0);


                tempMeshes.Add(duplicateMesh);
            }

            return tempMeshes;
        }

        private static GameObject BuildSection(List<Mesh> meshes, string name, GameObject parent)
        {
            //transfer meshes from list to combine instance
            Mesh combinedMesh = new Mesh();
            CombineInstance[] combine = new CombineInstance[meshes.Count];
            for (int i = 0; i < meshes.Count; i++)
            {
                combine[i].mesh = meshes[i];
            }
            //we don't need add transform positions, we are not using them, normally we would need to - skipping in combine meshes()
            //combine, meger in to one sub mesh, and ignore transform positions, we will rotate and position ourselves
            combinedMesh.CombineMeshes(combine, true, false);
            combinedMesh.RecalculateBounds();
            combinedMesh.RecalculateNormals();

            GameObject quarter = new GameObject();
            quarter.name = name;
            quarter.transform.position = parent.transform.position;
            quarter.transform.rotation = parent.transform.rotation;
            MeshFilter meshFilterQuarter = quarter.AddComponent<MeshFilter>();
            meshFilterQuarter.mesh = combinedMesh;
            MeshRenderer meshRendererQuarter = quarter.AddComponent<MeshRenderer>();
            Material[] materials = new Material[1];
            materials[0] = Resources.Load("White") as Material;
            if (parent.name == "Toilet")
            {
                materials[0] = Resources.Load("Black") as Material;
            }
            // materials[1] = Resources.Load("Blue") as Material; // I failed at submeshes - was tryi to put bath panel in same mesh as different submesh, just making different object now, no probs
            meshRendererQuarter.sharedMaterials = materials;

            return quarter;
        }
    }
    public class BedroomItems
    {
        public static GameObject Bed(GameObject parent)
        {

            parent.transform.position += (parent.transform.localScale.y*.25f)* Vector3.up;

            float bottomSize = parent.transform.localScale.y / 3;
            GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bottom.transform.localScale = new Vector3(parent.transform.localScale.x, bottomSize, parent.transform.localScale.z);
            bottom.transform.position = parent.transform.position;
            bottom.transform.position -= Vector3.up * bottomSize*0.5f;
            bottom.transform.rotation = parent.transform.rotation;
            bottom.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Roof") as Material;
            bottom.transform.parent = parent.transform;
            if (Random.Range(0, 2) == 0)
            {
                GameObject headBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                headBoard.transform.position = parent.transform.position;
                headBoard.transform.rotation = parent.transform.rotation;
                headBoard.transform.localScale = new Vector3(parent.transform.localScale.x, parent.transform.localScale.y - bottomSize, 0.1f);
                headBoard.transform.position += (Vector3.up * (parent.transform.localScale.y - bottomSize) * .5f) - headBoard.transform.forward * (parent.transform.localScale.z * 0.5f);
                headBoard.transform.position += (0.1f * .5f) * parent.transform.forward;
                headBoard.transform.parent = parent.transform;
                headBoard.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Roof") as Material;
            }
            //mattress
            GameObject mattress = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mattress.transform.position = parent.transform.position - (parent.transform.forward * ( - 0.05f)) + ( Vector3.up* (parent.transform.localScale.y / (3*2)));
            mattress.transform.rotation = parent.transform.rotation;
            mattress.transform.localScale = new Vector3(parent.transform.localScale.x, parent.transform.localScale.y/3, parent.transform.localScale.z - 0.1f);
           

            RoundedCube rc = mattress.AddComponent<RoundedCube>();
            rc.xSize = 10;
            rc.ySize = 10;
            rc.zSize = 10;
            rc.roundness = 1;

            mattress.transform.localScale /= 10;
            mattress.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Yellow") as Material;

            //quilt/duvet
            GameObject duvet = Instantiate(mattress, mattress.transform.position, mattress.transform.rotation);
            //duvet.transform.position += Vector3.up * mattress.transform.localScale.y * (0.5f*10);
            //reduce half a pillow in scale.z and move fwd half a pillow
            duvet.transform.localScale -= Vector3.forward*0.05f;
            duvet.transform.position += duvet.transform.forward * 0.5f/2;
            //puff up
            duvet.transform.localScale += Vector3.up * 0.015f;
            duvet.transform.localScale *= 1.1f;
            
            IcoMesh icoduvet=  duvet.AddComponent<IcoMesh>();
            icoduvet.randomScale = 0.1f;

            duvet.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;

            duvet.transform.parent = parent.transform;
            mattress.transform.parent = parent.transform;
            //pillows
            List<GameObject> pillows = new List<GameObject>();
            int pillowsAmount = 2;
            if (parent.transform.localScale.x < 1.5f)
                pillowsAmount = 1;
            for (int i = 0; i < pillowsAmount; i++)
            {


                GameObject pillow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillow.transform.position = parent.transform.position - (parent.transform.forward * ((parent.transform.localScale.z * .5f) - 0.35f));//.35 is half of headboard plus half pilow.z
                pillow.transform.rotation = parent.transform.rotation;

                rc = pillow.AddComponent<RoundedCube>();
                rc.xSize = 10;
                rc.ySize = 10;
                rc.zSize = 10;
                rc.roundness = 3;
                pillow.transform.localScale = new Vector3(0.08f, 0.03f, 0.05f);//divided by ten - pillow made up of 10-too many but easy maths
                pillow.transform.position += Vector3.up * (parent.transform.localScale.y / (3)) + (Vector3.up * 0.1f);
                float randomRange = 0.00f;
                pillow.transform.localScale += new Vector3(Random.Range(-randomRange, randomRange), Random.Range(-randomRange, randomRange), Random.Range(-randomRange, randomRange));
                pillow.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Door") as Material;
                pillow.transform.parent = parent.transform;
                pillows.Add(pillow);
            }
            //mess pillow up - randomise mesh
            foreach (GameObject p in pillows)
            {
                IcoMesh ic = p.AddComponent<IcoMesh>();
                ic.randomScale = 0.1f;
            }

            for (int i = 0; i < pillows.Count; i++)
            {
                if (pillows.Count == 1)
                    continue;

                if(i == 0)
                {
                    pillows[i].transform.position += pillows[i].transform.right * ( parent.transform.localScale.x*0.25f);//half a pillow's scale * pillow segnmetns plus lil gap
                    if(pillows.Count ==4)
                        pillows[i].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Yellow") as Material;

                    pillows[i].transform.rotation *= Quaternion.Euler(5 + Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));
                }
                if (i == 1)
                {
                    pillows[i].transform.position -= pillows[i].transform.right * (parent.transform.localScale.x * 0.25f);
                    if (pillows.Count == 4)
                        pillows[i].GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Yellow") as Material;

                    pillows[i].transform.rotation *= Quaternion.Euler(5 + Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));
                }
                if (i == 2)
                {
                    pillows[i].transform.position += pillows[i].transform.right * (pillows[i].transform.localScale.x * (0.5f * 10) + 0.05f);
                    pillows[i].transform.position += Vector3.up * (pillows[i].transform.localScale.y * (0.5f * 10) + 0.01f);
                    pillows[i].transform.position += parent.transform.forward * 0.1f;
                    pillows[i].transform.rotation *= Quaternion.Euler(5 + Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                }
                if (i == 3)
                {
                    pillows[i].transform.position -= pillows[i].transform.right * (pillows[i].transform.localScale.x * (0.5f * 10) + 0.05f);
                    pillows[i].transform.position += Vector3.up * (pillows[i].transform.localScale.y * (0.5f * 10) + 0.01f);
                    pillows[i].transform.position += parent.transform.forward * 0.1f;
                    pillows[i].transform.rotation *= Quaternion.Euler(5+ Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                }
            }

            return parent;
        }
        public static GameObject Wardrobe(GameObject parent)
        {

            parent.transform.position += Vector3.up * parent.transform.localScale.y*0.5f;
            //how many doors/drawers? (rows/columns)
           
            GameObject cupboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cupboard.transform.position = parent.transform.position;
            cupboard.transform.rotation = parent.transform.rotation;
            cupboard.transform.localScale = new Vector3(parent.transform.localScale.x, parent.transform.localScale.y, parent.transform.localScale.z);
            cupboard.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Yellow") as Material;
            cupboard.transform.parent = parent.transform;
           
            int rows = Random.Range(1, 3);
            int columns = 0;

            //if it is a wardroble height
            if (parent.transform.localScale.y > 1.5f)
            {
                //make one or two doors
                columns = Random.Range(2, 3);
                rows = 1;
            }
            else if (parent.transform.localScale.x < 1.5f)
            //make a drawers
            {
                columns = 1;
                rows = Random.Range(3, 6);
            }
            //make small cupboar
            else
            {
                columns = Random.Range(2, 4);
                rows = 1;
            }

            if (parent.name == "TV")
            {
                rows = 1;
                columns = Random.Range(1, 4);
            }

            //columns = 2;
            //rows = 1;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    door.transform.position = parent.transform.position  - Vector3.up * parent.transform.localScale.y*0.5f;//centre of cupboard
                    //x
                    door.transform.position -= parent.transform.right * 0.5f * parent.transform.localScale.x;//side of cupobard
                    door.transform.position += (parent.transform.right * (parent.transform.localScale.x / columns))*.5f;//1 width of door in
                    door.transform.position += ((parent.transform.localScale.x * j) / columns) * parent.transform.right;
                    //y
                    float height = parent.transform.localScale.y;// * 3;
                    float fraction = height / rows;
                    door.transform.position += Vector3.up * height;
                    door.transform.position -= Vector3.up * (fraction * (i));
                    door.transform.position -= Vector3.up * (fraction * (.5f));
                    //z
                    door.transform.position += parent.transform.forward * (parent.transform.localScale.z * 0.5f);
                    door.transform.rotation = parent.transform.rotation;
                    door.transform.localScale = new Vector3((parent.transform.localScale.x) / columns, ((parent.transform.localScale.y) / rows), 0.1f);
                    door.transform.localScale -= Vector3.one * 0.05f;
                    
                    door.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Green") as Material;
                    door.transform.parent = parent.transform;

                    GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    handle.transform.position = door.transform.position;
                    handle.transform.position += parent.transform.forward*parent.transform.localScale.z*( 0.05f);
                    handle.transform.rotation = parent.transform.rotation;
                    handle.transform.localScale = new Vector3(0.1f, 0.05f, 0.05f);
                    //if (rows == 1)
                    //    handle.transform.position += parent.transform.right * ((1f / columns) * 0.25f);

                    handle.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Black") as Material;
                    handle.transform.parent = parent.transform;
                }
            }

            

            parent.GetComponent<MeshRenderer>().enabled = false;
            return parent;
        }
        public static GameObject BookShelf(GameObject parent)
        {
            float width = Random.Range( 0.01f,0.1f);

            //top and bottom//and shelves
            int lower = 1;
            int upper = 6;
            if (parent.transform.localScale.y < 1.5f)
                upper = 4;
            if (parent.transform.localScale.y < 1.2f)
                upper = 2;

            if (parent.transform.localScale.y > 1f)
                lower= 2;
            if (parent.transform.localScale.y > 1.25f)
                lower = 3;

            int shelves = Random.Range(lower, upper); ;//includiung top and bottom - so having 4 here will have 3 middle shelves - <= below so it builds on top. doesnt matter really anyways
            for (int i = 0; i <= shelves; i++)
            {
                GameObject shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shelf.transform.rotation = parent.transform.rotation;
                shelf.transform.position = parent.transform.position - ((parent.transform.localScale.y*.5f)-width*0.5f)*Vector3.up;
                float fraction = parent.transform.localScale.y / shelves;
                shelf.transform.position += Vector3.up * fraction * i;

                shelf.transform.localScale = new Vector3(parent.transform.localScale.x-width*2, width, parent.transform.localScale.z);
                shelf.name = "Shelf";
                shelf.transform.parent = parent.transform;

                shelf.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;
            }

            //sides
            int sides = 2;//if we want ikea style box shleves, need to add suppoirt in with shelves too, overlapping witll z fight possibly, maybe ok,if same colour
            sides = Random.Range(1, 4);
            for (int i = 0; i <= sides; i++)
            {
                GameObject shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shelf.transform.rotation = parent.transform.rotation;
                shelf.transform.position = parent.transform.position - ((parent.transform.localScale.x * .5f) - width*0.5f) * parent.transform.right;
                float fraction = ((parent.transform.localScale.x - width) / sides);
                shelf.transform.position += (shelf.transform.right * fraction * i);
                shelf.transform.position += Vector3.up * width * 0.5f;
                shelf.transform.localScale = new Vector3(Mathf.Abs( width), Mathf.Abs( parent.transform.localScale.y+width), Mathf.Abs(parent.transform.localScale.z));
                shelf.name = "Side" + i.ToString(); ;
                shelf.transform.parent = parent.transform;

                shelf.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;
            }

            //back
            GameObject back= GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.transform.rotation = parent.transform.rotation;
            back.transform.position = parent.transform.position - ((parent.transform.localScale.z * .5f) - width * 0.5f) * parent.transform.forward;
            back.transform.position += Vector3.up * width * 0.5f;
            back.transform.localScale = new Vector3(parent.transform.localScale.x, parent.transform.localScale.y + width, width);
            back.transform.parent = parent.transform;
            back.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;

            //books
            
            for (int j = 0; j < shelves; j++)
            {
                for (float i = 0; i < parent.transform.localScale.x - 0.3f;)
                {
                    //skip sometimes
                    if (Random.Range(0, 6) == 0)
                        continue;

                    float random = Random.Range(0.02f,0.1f);

                    GameObject pivot = new GameObject();
                    
                    pivot.transform.rotation = parent.transform.rotation;
                    //bottom
                    pivot.transform.position = parent.transform.position - ((parent.transform.localScale.x * .5f) - width) * parent.transform.right - Vector3.up * parent.transform.localScale.y * (0.5f);
                    //up a width
                    pivot.transform.position += Vector3.up * width;
                    pivot.transform.position += parent.transform.right * (i + random);
                    //up a shelf
                    pivot.transform.position += (Vector3.up * (parent.transform.localScale.y / shelves))*j;


                    GameObject book = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    book.transform.rotation = parent.transform.rotation;
                    float bookHeight = (parent.transform.localScale.y / shelves) - width;
                    bookHeight = Mathf.Clamp(bookHeight, bookHeight, 0.5f);
                    bookHeight *= Random.Range(.5f, 1f);
                    
                    float bookDepth = 0.3f;
                    book.transform.position = pivot.transform.position + Vector3.up*bookHeight*0.5f;
                    book.transform.position -= parent.transform.right*random*0.5f;
                    
                    book.transform.localScale = new Vector3(Mathf.Abs( random),Mathf.Abs( bookHeight),Mathf.Abs( bookDepth));
                    

                    //pages
                    GameObject pages = Instantiate(book, book.transform.position, book.transform.rotation);
                    pages.transform.localScale = new Vector3(Mathf.Abs(book.transform.localScale.x - 0.025f), book.transform.localScale.y + 0.01f, book.transform.localScale.z - 0.025f);
                    pages.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("White") as Material;
                    pages.transform.parent = book.transform;
                    book.transform.parent = pivot.transform;

                    i += random;

                    int colourChooser = Random.Range(0, 4);
                    if(colourChooser == 0)
                        book.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Yellow") as Material;
                    if (colourChooser == 1)
                        book.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Blue") as Material;
                    if (colourChooser == 2)
                        book.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Pink") as Material;
                    if (colourChooser == 3)
                        book.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Green") as Material;

                    pivot.transform.parent = parent.transform;

                }
            }

            parent.transform.position += Vector3.up * parent.transform.localScale.y * .5f;// * shelves * 0.5f;

            return parent;
        }
        public static GameObject UnitHousing(GameObject parent, int shelves,int sides,float width,float depth)
        {
            parent.GetComponent<MeshRenderer>().enabled = false;

            //float width = Random.Range(0.01f, 0.1f);

            //top and bottom//and shelves
            int lower = 1;
            int upper = 6;
            if (parent.transform.localScale.y < 1.5f)
                upper = 4;
            if (parent.transform.localScale.y < 1.2f)
                upper = 2;

            if (parent.transform.localScale.y > 1f)
                lower = 2;
            if (parent.transform.localScale.y > 1.25f)
                lower = 3;

            //int shelves = Random.Range(lower, upper); ;//includiung top and bottom - so having 4 here will have 3 middle shelves - <= below so it builds on top. doesnt matter really anyways
            for (int i = 0; i <= shelves; i++)
            {
                GameObject shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shelf.transform.rotation = parent.transform.rotation;
                shelf.transform.position = parent.transform.position - ((parent.transform.localScale.y * .5f) - width * 0.5f) * Vector3.up;
                shelf.transform.position += parent.transform.forward * depth*0.5f;
                float fraction = parent.transform.localScale.y / shelves;
                shelf.transform.position += Vector3.up * fraction * i;

                shelf.transform.localScale = new Vector3(parent.transform.localScale.x, width, parent.transform.localScale.z- depth);
                shelf.name = "Shelf";
                shelf.transform.parent = parent.transform;

                shelf.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;
            }

            //sides
            //int sides = 2;//if we want ikea style box shleves, need to add suppoirt in with shelves too, overlapping witll z fight possibly, maybe ok,if same colour
            //sides = Random.Range(1, 4);
            for (int i = 0; i <= sides; i++)
            {
                GameObject shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shelf.transform.rotation = parent.transform.rotation;
                shelf.transform.position = parent.transform.position - ((parent.transform.localScale.x * .5f) - width * 0.5f) * parent.transform.right;
                float fraction = ((parent.transform.localScale.x - width) / sides);
                shelf.transform.position += (shelf.transform.right * fraction * i);
                shelf.transform.position += Vector3.up * width * 0.5f;
                shelf.transform.position += parent.transform.forward * depth * 0.5f;
                shelf.transform.localScale = new Vector3(width, parent.transform.localScale.y - width, parent.transform.localScale.z- depth);
                shelf.name = "Side" + i.ToString(); ;
                shelf.transform.parent = parent.transform;

                shelf.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;
            }

            //back
            GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.transform.rotation = parent.transform.rotation;
            back.transform.position = parent.transform.position - ((parent.transform.localScale.z * .5f) - width * 0.5f) * parent.transform.forward;
            back.transform.position += Vector3.up * width * 0.5f;
            back.transform.position += parent.transform.forward * depth * 0.5f;
            back.transform.localScale = new Vector3(parent.transform.localScale.x, parent.transform.localScale.y + width, depth);
            back.transform.parent = parent.transform;
            back.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;

           

            return parent;
        }
        public static GameObject Desk(GameObject parent,bool doChair)
        {

            int chairNumber = 1;
            Vector3 centre = parent.GetComponent<MeshRenderer>().bounds.center;
            //table
            parent.GetComponent<MeshRenderer>().enabled = false;
            float tableDepth = Random.Range(0.05f, 0.2f);

            //chairs
            float legWidth = Random.Range(0.01f, 0.1f);
            legWidth = tableDepth * 0.5f;

            float legHeight = parent.transform.localScale.y * 0.5f;
            float seatWidth = Random.Range(0.3f, 0.5f);
            //make bench on occasion
            // if (Random.Range(0, 10) == 0)
            //   seatWidth *= 2;

            float seatDepth = Random.Range(0.3f, 0.45f);
            float seatHeight = Random.Range(0.05f, 0.2f);
            float backHeight = Random.Range(0.2f, 0.8f);
            bool hasStretchers = true;
            if (Random.Range(0, 2) == 0)
                hasStretchers = false;
            bool hasArmrests = false;//they dont look that good and stop the chair from going under table

            //table
            GameObject primitive = null;
            PrimitiveType pt = PrimitiveType.Cube;
            float reducer = 0f;

            primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            primitive.transform.localScale = new Vector3(parent.transform.localScale.x - reducer, tableDepth, parent.transform.localScale.z - reducer);
            
            Destroy(primitive.GetComponent<Collider>());

            primitive.name = "Table";
            primitive.transform.rotation = parent.transform.rotation;
            //primitive.transform.parent = parent.transform;

            //colour
            primitive.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Brown") as Material;
            //make table height two depths above seat height
            primitive.transform.position = centre + (Vector3.up * parent.transform.localScale.y * (0.5f));// - tableDepth*.5f));// (legHeight + seatHeight + (tableDepth * 2)));

            primitive.transform.position -= Vector3.up * tableDepth * 0.5f;

            //duplicate adn move underneath for lower rim
            GameObject underneath = Instantiate(primitive);

            underneath.transform.position = primitive.transform.position;
            Destroy(underneath.GetComponent<Collider>());
            //underneath.transform.position = primitive.transform.position;
            underneath.transform.rotation = primitive.transform.rotation;


            float xzScaler = Random.Range(.7f, .9f);

            underneath.transform.localScale = new Vector3(primitive.transform.localScale.x * xzScaler, tableDepth, primitive.transform.localScale.z * xzScaler);

            underneath.transform.position -= Vector3.up * (tableDepth * .5f);
            underneath.transform.position -= Vector3.up * (underneath.transform.localScale.y * 0.5f);

            //legs
            float feetX = 0.1f;
            float feetY = parent.transform.localScale.y - tableDepth;
            float feetZ = 0.1f;
            float feetHeight = (feetY);
            //put legs inside this, then we can just scale the parent object to draaw the legs in -(lazy)
            GameObject legsParent = new GameObject();
            legsParent.name = "Legs";
            legsParent.transform.position = primitive.transform.position;
            // legsParent.transform.parent = parent.transform;
            for (int i = 0; i < 4; i++)
            {
                Vector3 sideDir = parent.transform.right;
                if (i > 1)
                    sideDir = -sideDir;
                Vector3 fwdDir = parent.transform.forward;
                if (i == 1 || i == 3)
                    fwdDir = -fwdDir;


                GameObject foot = GameObject.CreatePrimitive(PrimitiveType.Cube); //can't get cylinder legs matched up*TODO
                float scaler = 0.8f;


                foot.transform.localScale = new Vector3(feetX * scaler, feetY, feetZ * scaler);

                foot.transform.position = primitive.transform.position;

                float distance = underneath.transform.localScale.x * 0.5f;
                if (pt == PrimitiveType.Cylinder)
                    distance -= distance * .5f;
                foot.transform.position += sideDir * (distance);

                foot.transform.position -= Vector3.up * foot.transform.localScale.y * 0.5f;// * ptYscaler;
                foot.transform.position -= Vector3.up * tableDepth * .5f;
                //grumble
                //if(pt == PrimitiveType.Cylinder)
                //  foot.transform.position -= Vector3.up * tableDepth;
                distance = underneath.transform.localScale.z * 0.5f;
                // if (pt == PrimitiveType.Cylinder)
                //     distance -= distance*.5f;
                foot.transform.position += fwdDir * (distance);
                foot.transform.parent = legsParent.transform;
                foot.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;

            }

         
            //move out in to room a little

            //create points each side of the line - use "world" coordinates, because "centre" is a world co-ordinate
            Vector3 lookDir1 = Quaternion.Euler(0, 90, 0) * (parent.transform.forward).normalized;
            Vector3 lookDir2 = Quaternion.Euler(0, -90, 0) * (parent.transform.forward).normalized;
            Vector3 lookDir = Vector3.zero;
            //check which is closest - use that rotation to build wall
            if (Vector3.Distance(parent.transform.position + lookDir1, centre) > Vector3.Distance(parent.transform.position + lookDir2, centre))
                lookDir = Quaternion.Euler(0, 90, 0) * (parent.transform.forward);    //save as local direction
            else
                lookDir = Quaternion.Euler(0, -90, 0) * parent.transform.forward; //save as local direction

            // parent.transform.position += parent.transform.forward*0.5f;
            //  parent.transform.position += lookDir * 0.5f;

            //create object which has 1x1x1 scale and parent it to box passed to this function
            //child all items to this- this prevents distorttion as our "parent" has a non uniform scale
            GameObject container = new GameObject(); container.name = "Container";
            container.transform.parent = parent.transform;
            container.transform.position = parent.transform.position;

            primitive.transform.parent = container.transform;
            underneath.transform.parent = container.transform;
            legsParent.transform.parent = container.transform;

            //chairs
            if (doChair)
            {
                Mesh chairMesh = LivingroomItems.ChairMesh(legWidth, legHeight, seatWidth, seatDepth, seatHeight, backHeight, hasStretchers, hasArmrests);

                for (int i = 2; i < 3; i++)//forcing to 2 cos im lazy just chooses direction
                {
                    //a chance to miss chair
                    ///if (Random.Range(0, 5) == 0)
                    //    continue;

                    GameObject chair = new GameObject();
                    chair.name = "Chair";
                    MeshRenderer mr = chair.AddComponent<MeshRenderer>();
                    mr.material = Resources.Load("Materials/Brown") as Material;
                    MeshFilter mf = chair.AddComponent<MeshFilter>();
                    mf.mesh = chairMesh;

                    //work out position away from centre
                    //Vector3 direction = Quaternion.Euler(0f, i * (360f / chairNumber), 0f) * Vector3.right;
                    float distance = (primitive.transform.localScale.x * .5f);
                    float otherDistance = primitive.transform.localScale.z * .5f;
                    Vector3 dir = parent.transform.right;
                    Vector3 otherDir = parent.transform.forward;

                    if (i > 1)
                    {
                        distance = (primitive.transform.localScale.z * .5f);
                        otherDistance = primitive.transform.localScale.x * .5f;
                        dir = parent.transform.forward;
                        otherDir = parent.transform.right;
                    }
                    if (i == 1 || i == 3)
                        dir = -dir;

                    Vector3 directionAndDistance = distance * dir;

                    //move out half way and a little bit more for size of seat
                    //direction *= (parent.transform.localScale.x * 0.5f) + (seatDepth * 0.5f);

                    chair.transform.position = centre + directionAndDistance;

                    float randomRange = 0.25f * seatDepth;
                    chair.transform.position += new Vector3(Random.Range(-randomRange, randomRange), 0f, Random.Range(-randomRange, randomRange));


                    //look towards centre
                    Quaternion rot = Quaternion.LookRotation(chair.transform.position - centre);
                    chair.transform.rotation = rot;
                    chair.transform.position -= Vector3.up * parent.transform.localScale.y * .5f;




                    //is the table a long 'un?
                    if (otherDistance > 1.5f)
                    {
                        GameObject c2 = Instantiate(chair);
                        c2.transform.position = chair.transform.position;// - Vector3.up * parent.transform.localScale.y * .5f; 
                        c2.transform.position += otherDistance * 0.5f * otherDir;
                        c2.name = "Chair2";

                        rot = Quaternion.LookRotation(c2.transform.position - (centre - Vector3.up * parent.transform.localScale.y * .5f));

                        chair.transform.position -= otherDistance * 0.5f * otherDir;


                        if (pt == PrimitiveType.Cylinder)
                        {
                            //if circle table, chairs need to look towards ventre
                            c2.transform.rotation = rot;
                            //also make original chair look inward too -                         
                            rot = Quaternion.LookRotation(chair.transform.position - (centre - Vector3.up * parent.transform.localScale.y * .5f));
                            chair.transform.rotation = rot;
                            //chair.transform.position += towardsTable * (Random.Range(0.1f, 0.4f)*seatDepth);
                        }


                        c2.transform.parent = container.transform;
                    }
                    chair.transform.parent = container.transform;
                }
            }
            return parent;
        }

        public static GameObject Radiator(GameObject parent,float width,float depth)
        {
            int bumps = Random.Range(10, 20);
            UnitHousing(parent,1, bumps,width,depth);
         

            //scale on y to make it float and sit under window
            parent.transform.localScale -= Vector3.up * 0.4f;
            parent.transform.localScale -= Vector3.right * 0.2f;
            parent.transform.localScale -= Vector3.forward * 0.2f;
            parent.transform.position -= parent.transform.forward * 0.05f;

            //add knob
            GameObject cylinders = new GameObject();
            for (int i = 1; i < 4; i++)
            {
                GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cylinder.transform.position -= Vector3.up * 1f/i;
                cylinder.transform.localScale = new Vector3((float)(i)/4, (1f / i),(float)(i) / 4);
                //stops z fightin cos mymaths is so perfect lol
                cylinder.transform.localScale *= 0.99f;
                cylinder.transform.parent = cylinders.transform;
                cylinder.name = i.ToString();

                if (i == 1)
                    cylinder.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("White") as Material;
                if (i == 2)
                    cylinder.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("White") as Material;
                if (i == 3)
                    cylinder.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Black") as Material;

            }

            cylinders.transform.position = parent.transform.position + parent.transform.right * parent.transform.localScale.x*0.55f;
            cylinders.transform.position -= Vector3.up * .33f;
            cylinders.transform.localScale *= 0.1f;
            cylinders.transform.parent = parent.transform;



            return parent;
        }
    }
    public class LivingroomItems
    {
        public static GameObject Couch(GameObject parent,float feetX,float feetY, float feetZ,float bottomSize,float backWidth,float backHeight, float armRestWidth,float armRestHeight,PrimitiveType pt)
        {
            

            float feetHeight = (feetY);
            for (int i = 0; i < 4; i++)
            {
                Vector3 sideDir = parent.transform.right;
                if (i > 1)                
                    sideDir = -sideDir;
                Vector3 fwdDir = parent.transform.forward;
                if (i == 1 || i == 3)
                    fwdDir = -fwdDir;

                GameObject foot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                float scaler = 0.8f;
                foot.transform.localScale = new Vector3(feetX * scaler, feetY, feetZ * scaler);
                foot.transform.position = parent.transform.position;
                foot.transform.position +=sideDir * (parent.transform.localScale.x * .5f - feetX * .5f);

                foot.transform.position += Vector3.up * ((-parent.transform.localScale.y*0.5f) + feetHeight*.5f);
                foot.transform.position += fwdDir * (parent.transform.localScale.z * .5f - feetZ * .5f);
                foot.transform.parent = parent.transform;

                foot.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;
            }
            

           
            GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bottom.transform.localScale = new Vector3(parent.transform.localScale.x, bottomSize, parent.transform.localScale.z);
            
            bottom.transform.position = parent.transform.position;
            bottom.transform.position -= Vector3.up * parent.transform.localScale.y * .5f;
            bottom.transform.position += Vector3.up * bottomSize*0.5f;
            bottom.transform.position += Vector3.up * feetHeight;

            bottom.transform.rotation = parent.transform.rotation;
            bottom.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Roof") as Material;
            bottom.transform.parent = parent.transform;

            

            GameObject headBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            headBoard.transform.position = parent.transform.position;
            headBoard.transform.rotation = parent.transform.rotation;
            headBoard.transform.localScale = new Vector3(parent.transform.localScale.x, backHeight, backWidth);
            headBoard.transform.position -=  headBoard.transform.forward * (parent.transform.localScale.z * 0.5f);
            headBoard.transform.position += Vector3.up * (-parent.transform.localScale.y * .5f + bottomSize + backHeight*.5f);
            headBoard.transform.position += Vector3.up * feetHeight;
            headBoard.transform.position += (backWidth * .5f) * parent.transform.forward;
            headBoard.transform.parent = parent.transform;
            headBoard.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Roof") as Material;

            //armrest
            
          
            for (int i = 0; i < 2; i++)
            {
                GameObject side = GameObject.CreatePrimitive(pt);
                side.transform.parent = parent.transform;
                side.transform.position = parent.transform.position;
                side.transform.rotation = parent.transform.rotation;
                side.transform.rotation *= Quaternion.Euler(90, 0, 0);

                float yScale = (parent.transform.localScale.z - backWidth);
                //annoying
                if (pt == PrimitiveType.Cylinder)
                    yScale *= .5f;

                side.transform.localScale = new Vector3(armRestWidth,yScale, armRestHeight);

                
                //move to rear
                side.transform.position -= parent.transform.forward * parent.transform.localScale.z * .5f;
                //now move forward  
                side.transform.position += ( backWidth + (parent.transform.localScale.z - backWidth)*.5f) * parent.transform.forward;
                //now up
                side.transform.position -= Vector3.up * parent.transform.localScale.y*.5f;
                side.transform.position += Vector3.up * ((bottomSize) + armRestHeight*.5f);
                side.transform.position += Vector3.up * feetHeight;

                //sideways
                Vector3 sideways = parent.transform.right;
                if (i == 1)
                    sideways = -sideways;
                side.transform.position += sideways * (parent.transform.localScale.x * .5f - (armRestWidth * .5f));
                side.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Roof") as Material;
                //side.transform.parent = parent.transform;
            }
            //side.transform.position += (Vector3.up * (parent.transform.localScale.y - bottomSize) * .5f) - headBoard.transform.forward * (parent.transform.localScale.z * 0.5f);
            //headBoard.transform.position += (0.1f * .5f) * parent.transform.forward;
            headBoard.transform.parent = parent.transform;
            headBoard.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Roof") as Material;

            // parent.transform.position += Vector3.up * 0.5f;
            parent.GetComponent<MeshRenderer>().enabled = false;

            return parent;
        }

        public static GameObject Television(GameObject room, GameObject parent,bool makeCornerUnit)
        {
            //make a unit which is tapered towards the back
            float height = Random.Range(0.2f, 0.75f);
            if (makeCornerUnit)
            {
                bool wedge = true;
                if(wedge)
                {
                    GameObject unit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    unit.transform.parent = parent.transform;
                    unit.transform.position = parent.transform.position;
                    unit.transform.rotation = parent.transform.rotation;
                    //get a lsit of vertices from this pritive which are at the rear
                    Mesh mesh = new Mesh();
                    Vector3[] vertices = unit.GetComponent<MeshFilter>().mesh.vertices;
                    int[] rearVertices = HouseBuilder.VerticeArray("rear");
                    float depth = Random.Range(0.1f,0.5f);//how much gap in behind //relationship to scale? does it need randomised?
                    for (int i = 0; i < rearVertices.Length; i++)
                    {
                        vertices[rearVertices[i]].x *= depth;
                        vertices[rearVertices[i]].z *= -depth;
                    }
                    

                    //hide front triangles
                    List<int> triangles1 = new List<int>();
                    List<int> triangles2 = new List<int>();
                    int[] tris = unit.GetComponent<MeshFilter>().mesh.triangles;
                    for (int i = 0; i < tris.Length; i++)
                    {
                        //three tris a pop, first two equals 6
                        if (i < 6)
                            triangles1.Add(tris[i]);
                        else
                            triangles2.Add(tris[i]);
                    }
                    mesh.vertices = vertices;
                    mesh.subMeshCount = 2;
                    mesh.SetTriangles(triangles1, 0);
                    mesh.SetTriangles(triangles2, 1);

                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();
                    Material[] m = new Material[]{
                        Resources.Load("Transparent") as Material,
                     Resources.Load("Materials/Brown") as Material,
                     
                     };

                    unit.GetComponent<MeshRenderer>().sharedMaterials = m;
                    unit.GetComponent<MeshFilter>().mesh = mesh;
                    float holeSize = 0.1f;
                    //duplicate, reverse normals and scale to make inside
                    GameObject inside = Instantiate(unit, unit.transform.position, unit.transform.rotation);
                    ReverseNormals(inside);
                    inside.transform.localScale *= 0.9f;
                    inside.transform.position += holeSize*0.5f* parent.transform.forward;
                    inside.transform.parent = unit.transform;
                    
                    
                    GameObject front = KitchenItems.OvenFront(room, parent,8f, 1f, 1f, 1f, true, true, holeSize, 0.0f, 0f, 0f, 0f,false, false,false);
                    front.transform.position = parent.transform.position + parent.transform.forward*(0.5f + holeSize);
                    front.transform.rotation = parent.transform.rotation;
                    front.transform.rotation *= Quaternion.Euler(90, 0, 0);
                    front.transform.GetComponent<MeshRenderer>().sharedMaterial = m[1];
                    front.transform.parent = unit.transform;


                    unit.transform.localScale = new Vector3(1f, height, 1f);
                    unit.transform.position -= (Vector3.up * 0.5f);// height * 0.5f) - Vector3.up*0.5f;
                    unit.transform.position += Vector3.up * height*0.5f;

                    
                    unit.transform.position -= parent.transform.forward * 0.5f;
                }
            }

            else
            {
                parent.transform.localScale = new Vector3(Random.Range(1f, 1f), Random.Range(.3f, .5f), Random.Range(.2f, .4f));
                BedroomItems.Wardrobe(parent);
            }

            bool makeTv = true;
            if (makeTv)
            {
                GameObject unit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                unit.transform.rotation = parent.transform.rotation;
                Mesh mesh = new Mesh();
                Vector3[] vertices = unit.GetComponent<MeshFilter>().mesh.vertices;
                bool taper = false;
                if (Random.Range(0, 2) == 0)
                    taper = true;

                
                int[] rearVertices = HouseBuilder.VerticeArray("rear");
                float depth = Random.Range(0.1f, 0.5f);//how much gap in behind //relationship to scale? does it need randomised?
                for (int i = 0; i < rearVertices.Length; i++)
                {
                if (taper)
                    
                    vertices[rearVertices[i]].x *= depth;
                    vertices[rearVertices[i]].z *= -depth;
                }
                

                //hide front triangles
                List<int> triangles1 = new List<int>();
                List<int> triangles2 = new List<int>();
                int[] tris = unit.GetComponent<MeshFilter>().mesh.triangles;
                for (int i = 0; i < tris.Length; i++)
                {
                    //three tris a pop, first two equals 6
                    if (i < 6)
                        triangles1.Add(tris[i]);
                    else
                        triangles2.Add(tris[i]);
                }
                mesh.vertices = vertices;
                mesh.subMeshCount = 2;
                mesh.SetTriangles(triangles1, 0);
                mesh.SetTriangles(triangles2, 1);

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                Material[] m = new Material[]{
                        Resources.Load("Transparent") as Material,
                     Resources.Load("Black") as Material,

                     };

                unit.GetComponent<MeshRenderer>().sharedMaterials = m;
                unit.GetComponent<MeshFilter>().mesh = mesh;
                float holeSize = 0.1f;
                //duplicate, reverse normals and scale to make inside
                GameObject inside = Instantiate(unit, unit.transform.position, unit.transform.rotation);
                ReverseNormals(inside);
                inside.transform.localScale *= 0.9f;
                inside.transform.position += holeSize * 0.5f * parent.transform.forward;
                inside.transform.parent = unit.transform;


                GameObject front = KitchenItems.OvenFront(room, parent, 8f, 1f, 1f, 1f, true, true, holeSize, 0.0f, 0f, 0f, 0f, false, false, false);
                front.transform.position = unit.transform.position + parent.transform.forward * (0.5f + holeSize);
                front.transform.rotation = parent.transform.rotation;
                front.transform.rotation *= Quaternion.Euler(90, 0, 0);
                front.transform.GetComponent<MeshRenderer>().sharedMaterial = m[1];
                front.transform.parent = unit.transform;


                //parent.transform.localScale -= Vector3.up * height;
                unit.transform.position = parent.transform.position;
                unit.transform.position -= (Vector3.up * 0.5f);
                unit.transform.position += Vector3.up * height;

                unit.transform.localScale = new Vector3(4, 3, 5);
                unit.transform.localScale /= 5;

                unit.transform.position += Vector3.up * unit.transform.localScale.y * 0.5f;
                unit.transform.parent = parent.transform;

                GameObject glass = new GameObject();
                glass.transform.position = front.transform.position;
                glass.transform.rotation = parent.transform.rotation;
                glass.transform.parent = parent.transform;
                RoundedCube rc = glass.AddComponent<RoundedCube>();
                rc.xSize = 10;
                rc.ySize = 10;
                rc.zSize = 10;
                rc.roundness = 2;
                glass.transform.localScale = new Vector3( unit.transform.localScale.x / 10, unit.transform.localScale.y / 10,0.01f);
                glass.transform.localScale *= 1f - 0.2f;//rimwidth*1
                glass.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Glass") as Material;

                if (!makeCornerUnit)
                {
                    unit.transform.position -= parent.transform.forward * 0.5f;
                    unit.transform.position += Vector3.up*((height*.5f));
                    glass.transform.position -= parent.transform.forward * 0.5f;
                }
                else
                { unit.transform.position -= unit.transform.forward * 0.55f;
                    glass.transform.position -= unit.transform.forward * 0.55f;
                }

            }
            if(makeCornerUnit)
                parent.transform.position += Vector3.up * .55f;//aabove carpt
            
            parent.GetComponent<MeshRenderer>().enabled = false;
            return parent;
        }

        public static void TableAndChairsMaker(GameObject parent, int chairNumber)
        {

         


            Vector3 centre = parent.GetComponent<MeshRenderer>().bounds.center;
            //table
            parent.GetComponent<MeshRenderer>().enabled = false;
            float tableDepth = Random.Range( 0.05f,0.2f);

            //chairs
            float legWidth = Random.Range(0.01f, 0.1f);
            legWidth = tableDepth * 0.5f;
            
            float legHeight = parent.transform.localScale.y * 0.5f;
            float seatWidth = Random.Range(0.3f, 0.5f);
            //make bench on occasion
           // if (Random.Range(0, 10) == 0)
             //   seatWidth *= 2;

            float seatDepth = Random.Range(0.3f, 0.45f);
            float seatHeight = Random.Range(0.05f, 0.2f);
            float backHeight = Random.Range(0.2f, 0.8f);
            bool hasStretchers =  true;
            if (Random.Range(0, 2) == 0)
                hasStretchers = false;
            bool hasArmrests = false;//they dont look that good and stop the chair from going under table

            //table
            GameObject primitive = null;
            PrimitiveType pt = PrimitiveType.Cube;
            float reducer = 2f;
            
            if (Random.Range(0, 2) == 0)
            {
                pt = PrimitiveType.Cylinder;
            
                primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

                primitive.transform.localScale = new Vector3(parent.transform.localScale.x - reducer, tableDepth / 2, parent.transform.localScale.z - reducer);
                
            }
            else
            {
                primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                primitive.transform.localScale = new Vector3(parent.transform.localScale.x - reducer, tableDepth, parent.transform.localScale.z - reducer);
            }
            Destroy(primitive.GetComponent<Collider>());

            primitive.name = "Table";
            primitive.transform.rotation = parent.transform.rotation;
            //primitive.transform.parent = parent.transform;

            //colour
            primitive.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Brown") as Material;
            //make table height two depths above seat height
            primitive.transform.position = centre + (Vector3.up * parent.transform.localScale.y * (0.5f));// - tableDepth*.5f));// (legHeight + seatHeight + (tableDepth * 2)));

            primitive.transform.position -= Vector3.up * tableDepth * 0.5f;
            
            //duplicate adn move underneath for lower rim
            GameObject underneath = Instantiate(primitive);
           
            underneath.transform.position = primitive.transform.position;
            Destroy(underneath.GetComponent<Collider>());
            //underneath.transform.position = primitive.transform.position;
            underneath.transform.rotation= primitive.transform.rotation;
            
          
            float xzScaler = Random.Range(.7f, .9f);

            underneath.transform.localScale = new Vector3(primitive.transform.localScale.x * xzScaler, tableDepth, primitive.transform.localScale.z * xzScaler);
         
            underneath.transform.position -= Vector3.up * (tableDepth * .5f);
            underneath.transform.position -= Vector3.up*(underneath.transform.localScale.y*0.5f);
            
            //legs
            float feetX = 0.1f;
            float feetY = parent.transform.localScale.y - tableDepth;
            float feetZ = 0.1f;
            float feetHeight = (feetY);
            //put legs inside this, then we can just scale the parent object to draaw the legs in -(lazy)
            GameObject legsParent = new GameObject();
            legsParent.name = "Legs";
            legsParent.transform.position = primitive.transform.position;
           // legsParent.transform.parent = parent.transform;
            for (int i = 0; i < 4; i++)
            {
                Vector3 sideDir = parent.transform.right;
                if (i > 1)
                    sideDir = -sideDir;
                Vector3 fwdDir = parent.transform.forward;
                if (i == 1 || i == 3)
                    fwdDir = -fwdDir;

                
                GameObject foot = GameObject.CreatePrimitive(PrimitiveType.Cube); //can't get cylinder legs matched up*TODO
                float scaler = 0.8f;
             
              
                foot.transform.localScale = new Vector3(feetX * scaler, feetY, feetZ * scaler);

                foot.transform.position = primitive.transform.position;

                float distance = underneath.transform.localScale.x*0.5f;
                if (pt == PrimitiveType.Cylinder)
                    distance -= distance*.5f;
                foot.transform.position += sideDir * (distance);

                foot.transform.position -= Vector3.up * foot.transform.localScale.y * 0.5f;// * ptYscaler;
                foot.transform.position -= Vector3.up * tableDepth * .5f;
                //grumble
                //if(pt == PrimitiveType.Cylinder)
                //  foot.transform.position -= Vector3.up * tableDepth;
                distance = underneath.transform.localScale.z*0.5f;
               // if (pt == PrimitiveType.Cylinder)
               //     distance -= distance*.5f;
                foot.transform.position += fwdDir * (distance);
                foot.transform.parent = legsParent.transform;
                foot.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/Brown") as Material;
                
            }
            //grumble
          

            //now we have made the table, place inside a new gamobject and reduce it's scale. This is because the cibe we pass for it to be made in  also defines hte area where chairs should be




            //forwards needs to be the long side
            if (parent.transform.forward.x < parent.transform.forward.z)
            {
                //container.transform.rotation *= Quaternion.Euler(0, 90, 0);
                Debug.Log("Needs Swapped?");
            }

            //scale the table and charis down to fit inside the parent box
            //  float tableReducer = 1f;//half is the only safe value
            //  container.transform.localScale *= tableReducer;

            //move out in to room a little

            //create points each side of the line - use "world" coordinates, because "centre" is a world co-ordinate
            Vector3 lookDir1 = Quaternion.Euler(0, 90, 0) * (parent.transform.forward).normalized;
            Vector3 lookDir2 = Quaternion.Euler(0, -90, 0) * (parent.transform.forward).normalized;
            Vector3 lookDir = Vector3.zero;
            //check which is closest - use that rotation to build wall
            if (Vector3.Distance(parent.transform.position + lookDir1, centre) > Vector3.Distance(parent.transform.position + lookDir2, centre))
                lookDir = Quaternion.Euler(0, 90, 0) * (parent.transform.forward);    //save as local direction
            else
                lookDir = Quaternion.Euler(0, -90, 0) * parent.transform.forward; //save as local direction

            // parent.transform.position += parent.transform.forward*0.5f;
            //  parent.transform.position += lookDir * 0.5f;

            //create object which has 1x1x1 scale and parent it to box passed to this function
            //child all items to this- this prevents distorttion as our "parent" has a non uniform scale
            GameObject container = new GameObject(); container.name = "Container";
            container.transform.parent = parent.transform;
            container.transform.position = parent.transform.position;

            primitive.transform.parent = container.transform;
            underneath.transform.parent = container.transform;
            legsParent.transform.parent = container.transform;

            //chairs

            Mesh chairMesh = ChairMesh(legWidth, legHeight, seatWidth, seatDepth, seatHeight, backHeight, hasStretchers, hasArmrests);

            for (int i = 0; i < chairNumber; i++)
            {
                //a chance to miss chair
                ///if (Random.Range(0, 5) == 0)
                //    continue;

                GameObject chair = new GameObject();
                chair.name = "Chair";
                MeshRenderer mr = chair.AddComponent<MeshRenderer>();
                mr.material = Resources.Load("Materials/Brown") as Material;
                MeshFilter mf = chair.AddComponent<MeshFilter>();
                mf.mesh = chairMesh;

                //work out position away from centre
                //Vector3 direction = Quaternion.Euler(0f, i * (360f / chairNumber), 0f) * Vector3.right;
                float distance = (primitive.transform.localScale.x * .5f);
                float otherDistance = primitive.transform.localScale.z * .5f;
                Vector3 dir = parent.transform.right;
                Vector3 otherDir = parent.transform.forward;
                
                if (i > 1)
                {
                    distance = (primitive.transform.localScale.z * .5f);
                    otherDistance = primitive.transform.localScale.x * .5f;
                    dir = parent.transform.forward;
                    otherDir = parent.transform.right;
                }
                if (i == 1 || i == 3)
                    dir = -dir;

                Vector3 directionAndDistance = distance * dir;

                //move out half way and a little bit more for size of seat
                //direction *= (parent.transform.localScale.x * 0.5f) + (seatDepth * 0.5f);

                chair.transform.position = centre + directionAndDistance;

                float randomRange = 0.25f * seatDepth;
                chair.transform.position += new Vector3(Random.Range(-randomRange, randomRange), 0f, Random.Range(-randomRange, randomRange));
                

                //look towards centre
                Quaternion rot = Quaternion.LookRotation(chair.transform.position - centre);      
                chair.transform.rotation = rot;
                chair.transform.position -= Vector3.up * parent.transform.localScale.y * .5f;


               
                
                //is the table a long 'un?
                if (otherDistance >1.5f)
                {
                    GameObject c2 = Instantiate(chair);
                    c2.transform.position = chair.transform.position;// - Vector3.up * parent.transform.localScale.y * .5f; 
                    c2.transform.position += otherDistance*0.5f*otherDir;
                    c2.name = "Chair2";
                    
                    rot = Quaternion.LookRotation(c2.transform.position - (centre - Vector3.up * parent.transform.localScale.y * .5f) );
                    
                    chair.transform.position -= otherDistance * 0.5f * otherDir;


                    if (pt == PrimitiveType.Cylinder)
                    {
                        //if circle table, chairs need to look towards ventre
                        c2.transform.rotation = rot;
                        //also make original chair look inward too -                         
                        rot = Quaternion.LookRotation(chair.transform.position - (centre - Vector3.up * parent.transform.localScale.y * .5f));
                        chair.transform.rotation = rot;
                        //chair.transform.position += towardsTable * (Random.Range(0.1f, 0.4f)*seatDepth);
                    }

                    
                    c2.transform.parent = container.transform;
                }
                chair.transform.parent = container.transform;
            }
        }

        public static Mesh ChairMesh(float legWidth, float legHeight, float seatWidth, float seatDepth, float seatHeight, float backHeight, bool hasStretchers, bool hasArmrests)
        {
            //chairs

            //create chair mesh using unity proc gen example
            var draft = ChairGenerator.Chair(legWidth, legHeight, seatWidth, seatDepth, seatHeight, backHeight,
                     hasStretchers, hasArmrests);
            Mesh mesh = draft.ToMesh();


            return mesh;
        }




    }
    public class Doors
    {
        public static GameObject BedroomDoor(GameObject parent,Divide divide)
        {
            parent.GetComponent<MeshRenderer>().enabled = false;

            GameObject meshesParent = new GameObject();
            meshesParent.transform.position = parent.transform.position;
            meshesParent.transform.parent = parent.transform;
            meshesParent.transform.rotation = parent.transform.rotation;

            List<GameObject> bits = new List<GameObject>();
            //door will ahve two levels of geometry , iner base (wood or glass)
            //and outer, decorative features
            //centre board/base level
            GameObject inner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            inner.name = "innerDoor";
            inner.transform.parent = meshesParent.transform;
            inner.transform.rotation = parent.transform.rotation;
            inner.transform.position = parent.transform.position;
            inner.transform.position += Vector3.up * (divide.doorHeight*0.5f);
            inner.transform.localScale = new Vector3(divide.doorWidth, divide.doorHeight, 0.01f);
            inner.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Glass") as Material;

            //border
            float borderSize = Random.Range(0.05f, 0.4f);// divide.skirtingHeight;
            if (divide.interiorDoorBorderSize == 0)
                divide.interiorDoorBorderSize = borderSize;
            else
                borderSize = divide.interiorDoorBorderSize;

            //probably doesnt need randomised - just half a random value anyway
            float reliefDepth = 0.1f +divide.skirtingDepth*.5f;//0.1f is door thickness as standard

            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "Top";
            top.transform.rotation = parent.transform.rotation;
            top.transform.position = parent.transform.position;
            top.transform.position += (-borderSize*.5f + divide.doorHeight)*Vector3.up;
            top.transform.localScale = new Vector3(divide.doorWidth, borderSize, reliefDepth);
            bits.Add(top);

            GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bottom.name = "Bottom";
            bottom.transform.rotation = parent.transform.rotation;
            bottom.transform.position = parent.transform.position;
            bottom.transform.position += (borderSize * .5f) * Vector3.up;
            bottom.transform.localScale = new Vector3(divide.doorWidth, borderSize, reliefDepth);
            bits.Add(bottom);

            GameObject rightSide = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightSide.name = "RightSide";
            rightSide.transform.rotation = parent.transform.rotation;
            rightSide.transform.position = parent.transform.position;
            rightSide.transform.position += (divide.doorHeight * .5f) * Vector3.up;
            rightSide.transform.position += parent.transform.right * (-borderSize * .5f + divide.doorWidth * .5f);
            rightSide.transform.localScale = new Vector3(borderSize, divide.doorHeight, reliefDepth);
            bits.Add(rightSide);

            GameObject leftSide = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftSide.name = "LeftSide";
            leftSide.transform.rotation = parent.transform.rotation;
            leftSide.transform.position = parent.transform.position;
            leftSide.transform.position += (divide.doorHeight * .5f) * Vector3.up;
            leftSide.transform.position -= parent.transform.right * (-borderSize*.5f + divide.doorWidth * .5f);
            leftSide.transform.localScale = new Vector3(borderSize, divide.doorHeight, reliefDepth);
            bits.Add(leftSide);

            //place interior design
            Vector3 startY = parent.transform.position + borderSize * Vector3.up;
            Vector3 endY = parent.transform.position + divide.doorHeight * Vector3.up - borderSize * Vector3.up;
            GameObject startYgo = new GameObject();
            startYgo.transform.position = startY;
            GameObject endYgo = new GameObject();
            endYgo.transform.position = endY;
            startYgo.transform.parent= parent.transform;
            endYgo.transform.parent = parent.transform;

            Vector3 startX = parent.transform.position -(divide.doorWidth*.5f - borderSize)  * parent.transform.right;
            startX += Vector3.up * borderSize;
            Vector3 endX = parent.transform.position + (divide.doorWidth*.5f - borderSize) * parent.transform.right;
            endX+= Vector3.up * borderSize;
            GameObject startXgo = new GameObject();
            startXgo.transform.position = startX;
            GameObject endXgo = new GameObject();
            endXgo.transform.position = endX;
            startXgo.transform.parent = parent.transform;
            endXgo.transform.parent = parent.transform;

            Vector3 start = parent.transform.position + borderSize * Vector3.up;
            start -= (divide.doorWidth * .5f - borderSize) * parent.transform.right;
            Vector3 end = parent.transform.position + borderSize * Vector3.up;
            end += (divide.doorWidth * .5f - borderSize) * parent.transform.right;
            GameObject sGo = new GameObject();
            sGo.transform.position = start;
            GameObject sEnd = new GameObject();
            sEnd.transform.position = end;
            sGo.transform.parent = parent.transform;
            sEnd.transform.parent = parent.transform;


            //across sections//probably should be called columns - oh well
            //try and grab rows from divde script,other wise create and save- Ensures all doors inside have the same pattern
            int rows = Random.Range(1,4);
            if (divide.interiorDoorRows != 0)
                rows = divide.interiorDoorRows;
            else
                divide.interiorDoorRows = rows;
            
            Vector3 yDir = (endY - startY).normalized;
            Vector3 xDir = (end - start).normalized;
            float distanceX = Vector3.Distance(start, end);
            float step = distanceX / rows;

            //grab rimiwdth from divide script, so it is the same for all doors - asign a value if not already done(first time it tries)
            float rimWidth = Random.Range(0.01f, 0.05f);
            if (divide.interiorDoorRimWidth == 0f)
                divide.interiorDoorRimWidth = rimWidth;
            else
                rimWidth = divide.interiorDoorRimWidth;

            //same as above with pattern
            List<int> interiorDoorPattern = new List<int>();
            if (divide.interiorDoorPattern.Count == 0)
            {
                for (float i = 0; i < distanceX; i += step)
                {
                    int countForThisRow = Random.Range(1, 7);
                    interiorDoorPattern.Add(countForThisRow);
                }

                divide.interiorDoorPattern = interiorDoorPattern;
            }
            else
                interiorDoorPattern = divide.interiorDoorPattern;

            for (float i = 0, j = 0; i < distanceX; i += step, j++)
            {
                //int countForThisRow = Random.Range(1, 7);
                //if (j > interiorDoorPattern.Count)
                    //Debug.Log("Pattern out of range?, j count = " + j.ToString() + " and list count is " + interiorDoorPattern.Count);
                //bug catch, doesnt really matter, but pattern can go out of index
                int indexTouse = (int)(j);
                if (j >= interiorDoorPattern.Count)
                {
                    indexTouse = interiorDoorPattern.Count - 1;
                   // Debug.Break();
                   // Debug.Log("Saved from here");
                }

                int countForThisRow = interiorDoorPattern[indexTouse];
                float distanceY = Vector3.Distance(startY, endY);
                float stepY = distanceY / countForThisRow;
                for (float k = 0; k < distanceY; k+=stepY)
                {
                    GameObject test = new GameObject();
                   // test.transform.localScale *= 0.1f;
                    test.transform.position = start;
                    test.name = "pivot";
                    test.transform.parent = parent.transform;

                    test.transform.position += xDir * (i);
                    test.transform.position += Vector3.up * (k);

                    //now add spars down and behind
                   

                    GameObject vertSpar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    vertSpar.transform.position = test.transform.position + Vector3.up * stepY * 0.5f;
                    vertSpar.transform.parent = test.transform;
                    vertSpar.transform.rotation = parent.transform.rotation;                    
                    vertSpar.transform.localScale = new Vector3(rimWidth, stepY, rimWidth);
                    vertSpar.transform.position += xDir * rimWidth*0.5f;

                    vertSpar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    vertSpar.transform.position = test.transform.position + Vector3.up * stepY * 0.5f;
                    vertSpar.transform.parent = test.transform;
                    vertSpar.transform.rotation = parent.transform.rotation;                    
                    vertSpar.transform.localScale = new Vector3(rimWidth, stepY, rimWidth);
                    vertSpar.transform.position -= xDir * rimWidth * 0.5f;
                    vertSpar.transform.position += xDir * step;

                    GameObject horzSpar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    horzSpar.transform.position = test.transform.position + Vector3.up * stepY * 0.5f;
                    horzSpar.transform.parent = test.transform;
                    horzSpar.transform.rotation = parent.transform.rotation;
                    horzSpar.transform.localScale = new Vector3(step, rimWidth, rimWidth);
                    horzSpar.transform.position += xDir * step * 0.5f;
                    horzSpar.transform.position += Vector3.up * stepY * 0.5f;
                    horzSpar.transform.position -= Vector3.up * rimWidth * 0.5f;

                    horzSpar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    horzSpar.transform.position = test.transform.position + Vector3.up * stepY * 0.5f;
                    horzSpar.transform.parent = test.transform;
                    horzSpar.transform.rotation = parent.transform.rotation;
                    horzSpar.transform.localScale = new Vector3(step, rimWidth, rimWidth);
                    horzSpar.transform.position += xDir * step * 0.5f;
                    horzSpar.transform.position -= Vector3.up * stepY * 0.5f;
                    horzSpar.transform.position += Vector3.up * rimWidth * 0.5f;


                }
            }
            

           

            foreach (GameObject go in bits)
            {
                go.transform.parent = meshesParent.transform;
            }

            
            return parent;
        }
    }
    public class TubeBuilder
    {
        public static Mesh TubeMesh(List<Vector3> tubePoints, List<float> thicknesses, int tubeDetail, Vector3 spinAxis)
        {
            //takes a list of points and thickness and build a tube. Always builds parallel to the ground, so points passed must reflect this


            //create a ring of points around each tube points
            List<Vector3> vertices = new List<Vector3>();
            //int tubeDetail = 10; //must be even, so it keeps nice numbers after we divide by it - 3 and 5 and 9 work tho. go figure.. prim numbers above 10 dont work? why does 7 not work.. probably because we divide 360 by this. some relation there
            //float tubeThickness = 0.01f;
            for (int i = 0; i < tubePoints.Count - 1; i++)
            {
                //use tubeThickness from list, if list only has 1 value, make thickness uniform
                float t = 0.05f;// tubeThickness;// + Random.Range(-0.01f, 0.01f); - ideas
                if (thicknesses.Count == 1)
                    t = thicknesses[0];
                else
                    t = thicknesses[i];

                for (int j = 0; j < 360; j += 360 / tubeDetail)
                {
                    Vector3 toNext = (tubePoints[i + 1] - tubePoints[i]).normalized;

                    //spun to the side - start of circle
                    //spin Axis, one axis has to be straight
                    Vector3 sideDir = Quaternion.Euler(spinAxis) * (toNext);
                    //rotate around forward axis by j
                    Vector3 dir = Quaternion.AngleAxis(j, toNext) * sideDir;
                    //add thickenss

                    //dir *= tubeThickness;                   
                    dir *= t;
                    Vector3 position = tubePoints[i] + dir;

                    //GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //c.transform.position = position;
                    //c.transform.localScale *= 0.01f;

                    vertices.Add(position);// - parent.transform.position);
                }
            }
            //add last tube point

            for (int j = 0; j < 360; j += 360 / tubeDetail)
            {
                Vector3 toLast = (tubePoints[tubePoints.Count - 1] - tubePoints[tubePoints.Count - 2]).normalized;

                //spun to the side - start of circle
                Vector3 sideDir = Quaternion.Euler(spinAxis) * (toLast);
                //rotate around forward axis by j
                Vector3 dir = Quaternion.AngleAxis(j, toLast) * sideDir;
                //add thickness
                dir *= thicknesses[thicknesses.Count - 1];

                Vector3 position = tubePoints[tubePoints.Count - 1] + dir;

                //GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //c.transform.position = position;
                //c.transform.localScale *= 0.01f;

                vertices.Add(position);// - parent.transform.position);
            }

            List<int> triangles = new List<int>();
            for (int i = 0; i < tubePoints.Count - 1; i++)//***********range?>
            {
                for (int j = 0; j < tubeDetail - 1; j++)
                {
                    triangles.Add(j + (i * tubeDetail));
                    triangles.Add(j + 1 + (i * tubeDetail));
                    triangles.Add(j + tubeDetail + (i * tubeDetail));

                    triangles.Add(j + 1 + (i * tubeDetail));
                    triangles.Add(j + tubeDetail + 1 + (i * tubeDetail));
                    triangles.Add(j + tubeDetail + (i * tubeDetail));
                }

                //add linking to first point -- just did this trial and error - logic may not be consistent
                triangles.Add(-tubeDetail + ((i + 1) * tubeDetail));
                triangles.Add(((i + 1) * tubeDetail));//first on next**good
                triangles.Add(tubeDetail - 1 + ((i + 1) * tubeDetail));//last one on next ring**good

                triangles.Add(-tubeDetail + ((i + 1) * tubeDetail));
                triangles.Add(tubeDetail - 1 + ((i + 1) * tubeDetail));
                triangles.Add(-1 + ((i + 1) * tubeDetail));
            }

            Mesh tubeMesh = new Mesh();
            tubeMesh.vertices = vertices.ToArray();
            tubeMesh.triangles = triangles.ToArray();
            //tubeMesh.RecalculateNormals();



            return tubeMesh;
        }
    }

    public static GameObject ReverseNormals(GameObject go)
    {
        MeshFilter filter = go.GetComponent(typeof(MeshFilter)) as MeshFilter;
        if (filter != null)
        {
            Mesh mesh = filter.mesh;

            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
                normals[i] = -normals[i];
            mesh.normals = normals;

            for (int m = 0; m < mesh.subMeshCount; m++)
            {
                int[] triangles = mesh.GetTriangles(m);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int temp = triangles[i + 0];
                    triangles[i + 0] = triangles[i + 1];
                    triangles[i + 1] = temp;
                }
                mesh.SetTriangles(triangles, m);
            }

            mesh.RecalculateNormals();
        }

        

        return go;
    }
}


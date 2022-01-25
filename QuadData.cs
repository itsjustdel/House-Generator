using UnityEngine;
using System.Collections;

public class QuadData : MonoBehaviour {

    public float forwardStretch = 0f;

    public float forwardStretch0 = 0f;
    public float forwardStretch1 = 0f;
    public float forwardStretch2 = 0f;
    public float forwardStretch3 = 0f;


    public float backStretch = 0;

    public float backStretch0 = 0;
    public float backStretch1 = 0;
    public float backStretch2 = 0;
    public float backStretch3 = 0;

    public float rightStretch = 0f;

    public float rightStretch0 = 0f;
    public float rightStretch1 = 0f;
    public float rightStretch2 = 0f;
    public float rightStretch3 = 0f
        ;
    public float leftStretch = 0f;

    public float leftStretch0 = 0f;
    public float leftStretch1 = 0f;
    public float leftStretch2 = 0f;
    public float leftStretch3 = 0f;




    void Awake()
    {

        //brickSize is 0.1 global atm

        /*
        StretchQuads sq = GetComponentInParent<StretchQuads>();
        forwardStretch = sq.brickSize*2;
        backStretch = sq.brickSize*2;
        rightStretch = sq.brickSize*2;
        leftStretch = sq.brickSize*2;
        */

        forwardStretch = 0.1f;
        backStretch = 0.1f;
        rightStretch = 0.1f;
        leftStretch = 0.1f;

    }
}

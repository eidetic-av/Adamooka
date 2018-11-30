using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eidetic.Devices;
using Midi;

public class BongoCircleGenerator : MonoBehaviour
{

    public int XCount = 4;
    public int YCount = 4;
    public Vector2 GridSpan = new Vector2(1f, 1f);

    //Tanzmaus Tanzmaus;

    int XPosition = 1;
    int YPosition = 1;
    int HorizontalDirection = 1;
    int VerticalDirection = 1;

    GameObject[,] Circles;

    // Use this for initialization
    void Start()
    {
        //Tanzmaus = new Tanzmaus();
        //Tanzmaus.AddNoteOnAction(NoteOn);
        Circles = new GameObject[XCount, YCount];

        float xDist = GridSpan.x / (float)XCount;
        float yDist = GridSpan.y / (float)YCount;
        for (int x = 0; x < XCount; x++)
        {
            for (int y = 0; y < YCount; y++)
            {
                var newCircle = (GameObject)Resources.Load("FlatCircle");
                Circles[x, y] = GameObject.Instantiate(newCircle);
                newCircle.name = "Circles[" + x + ", " + y + "]";
                newCircle.hideFlags = HideFlags.HideInHierarchy;

                Color circleColor = newCircle.GetComponent<Renderer>().sharedMaterial.color;
                circleColor.a = 0;

                float xPos = (xDist * x) - (GridSpan.x / 2f);
                float yPos = (yDist * y) + (GridSpan.y / 2f);
                newCircle.transform.position = new Vector3(xPos, yPos);

            }
        }
    }

    // Update is called once per frame
    void NoteOn(int noteNumber, int velocity)
    {
        Circles[XPosition, 0].GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        XPosition += 1 * HorizontalDirection;
        if (XPosition % XCount == 0)
        {
            YPosition = (YPosition + (1 * VerticalDirection)) % YCount;
            HorizontalDirection = HorizontalDirection * -1;
        }
        if (YPosition % YCount == 0)
        {
            VerticalDirection = VerticalDirection * -1;
            Debug.Log(VerticalDirection);
        }
    }

}


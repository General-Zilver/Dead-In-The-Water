using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private float startPos, length;
    public GameObject cam;
    public float parallaxEffect; // The speed in which the background moves relative to the camera

    void Start()
    {
        startPos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void FixedUpdate()
    {
        float distance = cam.transform.position.x * parallaxEffect; // 0 = move with cam 1 = move with cam at same speed 0.5 = move with cam at half speed
        float movement = cam.transform.position.x * (1 - parallaxEffect);


        transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);

        // Check if the background needs to be wrapped
        if (movement > startPos + length)
        {
            startPos += length;
        }
        else if (movement < startPos - length)
        {
            startPos -= length;
        }


    }
}

/*
 * Following the code example in:
 * Gibson Bond J.,  Introduction to Game Design, Prototyping, and Development: From Concept to Playable Game with Unity and C#, First Edition, Addison Wesley, 2014
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    // Singleton. Only one instance, so we can store it in a static variable S.
    static public BoidSpawner S;

    //public GameObject boidsParent;

    // Fields to allow behaviour of the Boids as a group;
    public int          numBoids = 100;
    public GameObject   boidPrefab;

    public float spawnRadius = 100f;
    public float spawnVelocity = 10f;

    public float minVelocity = 0f;
    public float maxVelocity = 30f;

    public float nearDist = 30f;
    public float collisionDist = 5f;

    public float velocityMatchingAmt = 0.01f;
    public float flockCenteringAmt = 0.15f;
    public float collisionAvoidanceAmt = -0.5f;

    public float mouseAttractionAmt = 0.01f;
    public float mouseAvoidanceAmt = 0.75f;
    public float mouseAvoidanceDist = 15f;
    public float velocityLerpAmt = 0.25f;

    public bool _____________;

    public Vector3 mousePos;


    void Start()
    {
        // This instance of BoidSpawner goes into the Singleton S
        S = this;

        // or Instantiate(boidPrefab, boidsParent.transform)
        for (int i = 0; i < numBoids; i++) Instantiate(boidPrefab);
    }

    void LateUpdate()
    {
        // Track the mouse postion.
        Vector3 mousePos2d = new Vector3(Input.mousePosition.x, Input.mousePosition.y, this.transform.position.y);
        mousePos = Camera.main.ScreenToWorldPoint(mousePos2d);
    }

}

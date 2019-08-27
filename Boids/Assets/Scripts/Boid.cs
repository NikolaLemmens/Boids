/*
 * Following the code example in:
 * Gibson Bond J.,  Introduction to Game Design, Prototyping, and Development: From Concept to Playable Game with Unity and C#, First Edition, Addison Wesley, 2014
 * Based on pseudocode for Boids by conras Parker: 
 * http://www.kfish.org/boids/pseudocode.html
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    // Static List holds all Boid instances & is shared among them
    static public List<Boid> boids;

    // We don't use the Rigidbody's velocity (and the Physics system), but handle velocity directly
    public Vector3 velocity; // Current velocity
    public Vector3 newVelocity; // Velocity for the next frame
    public Vector3 newPosition; // Position for the next frame

    public List<Boid> neighbors; // Nearby
    public List<Boid> collisionRisks; // Too close
    public Boid closest; // Single closest boid

    void Awake()
    {
        // Define the boids List if it is still null
        if (boids == null)
            boids = new List<Boid>();

        // Add this Boid to boids
        boids.Add(this);

        // Give this Boid instance a random position and velocity
        Vector3 randPos = Random.insideUnitSphere * BoidSpawner.S.spawnRadius;
        randPos.y = 0; // Only move in the XZ plane (flatten the Boid)
        this.transform.position = randPos;
        velocity = Random.onUnitSphere;
        velocity *= BoidSpawner.S.spawnVelocity;

        // Initialize the two Lists
        neighbors = new List<Boid>();
        collisionRisks = new List<Boid>();

        // Make this.transform a child of the Boids go in the hierarchy
        this.transform.parent = GameObject.Find("Boids").transform;
        //this.transform.SetParent(boidsParent);

        // Give the Boid a random color, not too dark
        Color randColor = Color.black;
        while (randColor.r + randColor.g + randColor.b < 1)
        {
            randColor = new Color(Random.value, Random.value, Random.value);
        }
        Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
        {
            r.material.color = randColor;  
        }

    }

    void Update()
    {
        // Get this Boid's neighbors
        // Change name to tempNeighbors
        List<Boid> neighbors = GetNeighbors(this);

        // Initialize newVelocity and newPosition to the current values
        newVelocity = velocity;
        newPosition = this.transform.position;

        // Rule 1: Velocity Matching: Set the velocity of the Boid to be similar to that of its neighbours
        Vector3 neighborVel = GetAverageVelocity(neighbors);
        // Customise with fields from BoidSpawner S
        newVelocity += neighborVel * BoidSpawner.S.velocityMatchingAmt;

        // Rule 2: Flock Centering: Move towards middle of neighbors
        Vector3 neighborCenterOffset = GetAveragePosition(neighbors) - this.transform.position;
        newVelocity += neighborCenterOffset * BoidSpawner.S.flockCenteringAmt;

        // Rule 3: Collision Avoidance: Avoid running into Boids that are too close
        Vector3 dist;
        if(collisionRisks.Count > 0)
        {
            Vector3 collisionAveragePosition = GetAveragePosition(collisionRisks);
            dist = collisionAveragePosition - this.transform.position;
            newVelocity += dist * BoidSpawner.S.collisionAvoidanceAmt;
        }
        // Mouse Attraction: Move toward the mouse no matter how far away
        dist = BoidSpawner.S.mousePos - this.transform.position;
        if (dist.magnitude > BoidSpawner.S.mouseAvoidanceDist)
        {
            newVelocity += dist * BoidSpawner.S.mouseAttractionAmt;
        }
        else
        {
            // If the mouse is too close, move away quickly!
            newVelocity -= dist.normalized * BoidSpawner.S.mouseAvoidanceDist * BoidSpawner.S.mouseAvoidanceAmt;
        }

        // NewVelocity and NewPosition are ready, but wait until LateUpdate()
        // to set them so that this Boid doesn't move before others have had a chance to calculate their new values.

    }

    // By allowing all Boids to Update() themselves before any Boids move,
    // we avoid race conditions that could be caused by some Boids moving before other have decided where to go.
    void LateUpdate()
    {
        // Adjust the current velocity based on the newVelocity using a lineair interpolation
        velocity = (1-BoidSpawner.S.velocityLerpAmt)*velocity + BoidSpawner.S.velocityLerpAmt*newVelocity;

        // Make sure the velocity is within the min and max limits
        if(velocity.magnitude > BoidSpawner.S.maxVelocity)
        {
            velocity = velocity.normalized * BoidSpawner.S.maxVelocity;
        }
        if (velocity.magnitude < BoidSpawner.S.minVelocity)
        {
            velocity = velocity.normalized * BoidSpawner.S.maxVelocity;
        }

        //Decide on the newPosittion
        newPosition = this.transform.position + velocity * Time.deltaTime;
        // Keep everthing in the XZ plane
        newPosition.y = 0;
        // Look from the old position at the new position to orient the model
        this.transform.LookAt(newPosition);
        // Actually move to the new position
        this.transform.position = newPosition;
    }


    //Following methods don't need to be public

    // Find which Boids are near enough to be considered neighbors
    // boi is BoidOfInterest, the Boid we're focusing on
    public List<Boid> GetNeighbors (Boid boi)
    {
        float closestDist = float.MaxValue;
        Vector3 delta;
        float dist;
        neighbors.Clear();
        collisionRisks.Clear();

        foreach (Boid b in boids)
        {
            if (b == boi) continue;
            delta = b.transform.position - boi.transform.position;
            dist = delta.magnitude;

            if(dist < closestDist)
            {
                closestDist = dist;
                closest = b;
            }
            if(dist < BoidSpawner.S.nearDist)
            {
                neighbors.Add(b);
            }
            if(dist < BoidSpawner.S.collisionDist)
            {
                collisionRisks.Add(b);
            }
        }
        if (neighbors.Count == 0)
        {
            neighbors.Add(closest);
        }
        return neighbors;
    }

    // Get the average velocity of the boids in a List<Boid>
    public Vector3 GetAverageVelocity (List<Boid> someBoids)
    {
        Vector3 sum = Vector3.zero;
        foreach (Boid b in someBoids)
        {
            sum += b.velocity;
        }
        Vector3 avg = sum / someBoids.Count;
        return avg;
    }
    // Get the average position of the boids in a List<Boid>
    public Vector3 GetAveragePosition(List<Boid> someBoids)
    {
        Vector3 sum = Vector3.zero;
        foreach (Boid b in someBoids)
        {
            sum += b.transform.position;
        }
        Vector3 center = sum / someBoids.Count;
        return center;
    }



}

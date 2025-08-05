using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vehicleAiController : MonoBehaviour{

    private Rigidbody rb;
    
    public float totalPower = 50f;
    public float maxSteerAngle = 30f;
    public float vertical , horizontal;

    private float radius = 8 , distance;
    public carNode currentNode;

    private Vector3 velocity ,Destination, lastPosition;

    void Start(){
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("vehicleAiController requires a Rigidbody component!");
        }
        
        // Find the first carNode if currentNode is not assigned
        if (currentNode == null)
        {
            currentNode = FindObjectOfType<carNode>();
        }
    }

    void FixedUpdate(){
        try{
        checkDistance();
        steerVehicle();
        }
        catch{}
    
    }


    void checkDistance(){

            if(Vector3.Distance(transform.position , currentNode.transform.position) <= 3){
                reachedDestination();
            }

        
    }

        
    private void reachedDestination(){
        if(currentNode.nextWaypoint == null ){
            currentNode = currentNode.previousWaypont;
            return;
        }
        if(currentNode.previousWaypont == null ){
            currentNode = currentNode.nextWaypoint;
            return;
        }

        if(currentNode.link != null && Random.Range(0 , 100) <= 20)
            currentNode = currentNode.link;
        else
            currentNode = currentNode.nextWaypoint;
        
    



    }


    private void steerVehicle(){
        if (currentNode == null || rb == null) return;

        Vector3 relativeVector = transform.InverseTransformPoint(currentNode.transform.position);
        relativeVector /= relativeVector.magnitude;
        
        // Calculate steering
        float newSteer = (relativeVector.x / relativeVector.magnitude) * 2;
        horizontal = Mathf.Clamp(newSteer, -1f, 1f);
        
        // Apply motor force
        Vector3 forwardForce = transform.forward * totalPower;
        rb.AddForce(forwardForce, ForceMode.Acceleration);
        
        // Apply steering by rotating the vehicle
        float steerAmount = horizontal * maxSteerAngle * Time.fixedDeltaTime;
        transform.Rotate(0, steerAmount, 0, Space.World);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if(currentNode != null)
        Gizmos.DrawSphere(currentNode.transform.position ,0.5f);
    }

}

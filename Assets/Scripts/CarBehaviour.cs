using UnityEngine;

public class CarBehaviour : MonoBehaviour
{
    public Rigidbody SphereRB;

    private float moveInput;
    private float turnInput;

    public float fwdSpeed = 50f;
    public float revSpeed = 50f;
    public float turnSpeed = 40f;
    public float brakeForce = 100f;
    public float maxBrakeSpeed = 20f;

    void Start()
    {
        //detach rigidbody from car
        SphereRB.transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        float rawMoveInput = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");
        bool isBraking = Input.GetKey(KeyCode.Space);

        //adjust speed of car 
        moveInput = rawMoveInput * (rawMoveInput > 0 ? fwdSpeed : revSpeed);

        //set cars position to the sphere's position
        transform.position = SphereRB.transform.position;

        //set cars rotation based on turn input (only when moving and not braking heavily)
        if (!isBraking || SphereRB.linearVelocity.magnitude < maxBrakeSpeed)
        {
            float newRotation = turnInput * turnSpeed * Time.deltaTime;
            transform.Rotate(0, newRotation, 0, Space.World);
        }

        // Apply braking
        if (isBraking)
        {
            ApplyBraking();
        }
    }

    private void FixedUpdate()
    {
        SphereRB.AddForce(transform.forward * moveInput, ForceMode.Acceleration);
    }

    private void ApplyBraking()
    {
        // Get current velocity
        Vector3 velocity = SphereRB.linearVelocity;
        
        // Apply brake force opposite to velocity direction
        if (velocity.magnitude > 0.1f) // Only brake if moving
        {
            Vector3 brakeDirection = -velocity.normalized;
            SphereRB.AddForce(brakeDirection * brakeForce, ForceMode.Acceleration);
        }
        else
        {
            // Stop completely if moving very slowly
            SphereRB.linearVelocity = Vector3.zero;
        }
    }
}

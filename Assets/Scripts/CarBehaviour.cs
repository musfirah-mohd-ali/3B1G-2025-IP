using UnityEngine;

public class CarBehaviour : MonoBehaviour
{
    public Rigidbody SphereRB;

    private float moveInput;

    public float fwdSpeed = 10f;
    public float revSpeed = 100f;

    void Start()
    {
        //detach rigidbody from car
        SphereRB.transform.parent = null;

    }

    // Update is called once per frame
    void Update()
    {
        moveInput = Input.GetAxisRaw("Vertical");
        moveInput *= fwdSpeed;

        if (moveInput > 0)
        {
            moveInput *= fwdSpeed;
        }
        else if (moveInput < 0)
        {
            moveInput *= revSpeed;
        }

        transform.position = SphereRB.transform.position;


    }

    private void FixedUpdate()
    {
        SphereRB.AddForce(transform.forward * moveInput, ForceMode.Acceleration);
    }

}

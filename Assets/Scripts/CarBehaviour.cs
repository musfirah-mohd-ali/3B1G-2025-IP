using UnityEngine;

public class CarBehaviour : MonoBehaviour
{
    public Rigidbody SphereRB;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = SphereRB.transform.position;
    }
}

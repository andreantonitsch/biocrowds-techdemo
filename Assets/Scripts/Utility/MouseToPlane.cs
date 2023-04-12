using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseToPlane : MonoBehaviour
{
    [SerializeField]
    private GameObject _center;

    private Rigidbody rb;
    private Plane p;

    private void Start()
    {
        //Cursor.visible = false;
        p = new Plane(Vector3.up, 0);
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetButton("Fire1")) { 
            Vector3 mousePos = Vector3.zero;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (p.Raycast(ray, out float distance))
            {
                mousePos = ray.GetPoint(distance);
            }

            rb.position = mousePos;
        }
    }


}

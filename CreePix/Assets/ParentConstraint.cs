using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentConstraint : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform pelvis;
    // Update is called once per frame

    public Vector3 offset;
    private void Awake()
    {
        offset = pelvis.position - transform.position;
    }

    void Update()
    {
        if (pelvis.position - transform.position != offset)
            transform.position = pelvis.position - offset;
    }
}

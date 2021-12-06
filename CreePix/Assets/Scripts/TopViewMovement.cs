using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopViewMovement : MonoBehaviour
{
    private float movement_speed;
    private float scroll_speed;

    // Start is called before the first frame update
    void Start()
    {
        movement_speed = 200;
        scroll_speed = 50;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GetComponent<Camera>().enabled)
            return;

        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * movement_speed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Translate(Vector3.left * movement_speed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.Z))
        {
            transform.Translate(Vector3.forward * movement_speed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * movement_speed * Time.deltaTime, Space.World);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(scroll_speed * scroll * Vector3.down, Space.World);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float movement_speed;
    private float rotation_speed;

    // Start is called before the first frame update
    void Start()
    {
        movement_speed = 50;
        rotation_speed = 25;
    }

    // Update is called once per frame
    void Update()
    {
        float theta = -2 * Mathf.PI * transform.eulerAngles.y / 360;
        if (Input.GetKey(KeyCode.RightArrow))
        {
            Vector3 dir = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta));
            transform.Translate(dir * movement_speed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Vector3 dir = new Vector3(-Mathf.Cos(theta), 0, -Mathf.Sin(theta));
            transform.Translate(dir * movement_speed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Vector3 dir = new Vector3(-Mathf.Sin(theta), 0, Mathf.Cos(theta));
            transform.Translate(dir * movement_speed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            Vector3 dir = new Vector3(Mathf.Sin(theta), 0, -Mathf.Cos(theta));
            transform.Translate(dir * movement_speed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.Z))
        {
            transform.eulerAngles += Vector3.right * rotation_speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.eulerAngles += Vector3.left * rotation_speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.eulerAngles += Vector3.down * rotation_speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.eulerAngles += Vector3.up * rotation_speed * Time.deltaTime;
        }
    }
}

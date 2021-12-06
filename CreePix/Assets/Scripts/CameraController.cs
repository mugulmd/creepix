using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Camera player_cam;

    [SerializeField]
    private Camera top_view_cam;

    // Start is called before the first frame update
    void Start()
    {
        player_cam.enabled = true;
        top_view_cam.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Tab))
        {
            player_cam.enabled = !player_cam.enabled;
            top_view_cam.enabled = !top_view_cam.enabled;
        }
    }
}

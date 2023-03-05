using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float sensitivity;
    public float zoom;
    Camera cam;
    public Camera renderCam;
    public RenderTexture tex;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!OverworldController.Instance.active)
            return;
        if (Input.GetMouseButton(2))
        {
            transform.Translate(new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0) * sensitivity * (cam.orthographicSize / 26f) * Time.deltaTime);
        }
        if(Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            cam.orthographicSize = cam.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * zoom * Time.deltaTime;
            renderCam.orthographicSize = cam.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * zoom * Time.deltaTime;
            if (cam.orthographicSize < 5f)
            {
                cam.orthographicSize = 5f;
                renderCam.orthographicSize = 5f;
            }
            else if(cam.orthographicSize > 26f)
            {
                cam.orthographicSize = 26f;
                renderCam.orthographicSize = 26f;
            }
        }
    }
}

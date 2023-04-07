using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _zoomSpeed;

    [SerializeField] private Vector2 _zoomMinMax;

    private Camera cam;

    [SerializeField] private Vector2 velocity;
    [SerializeField] private float scroll;


    public float maxZoomT = 0.00025f;
    public float minZoomT = 0.0004f;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        var x = Input.GetAxis("Horizontal");
        var z = Input.GetAxis("Vertical");

        var move = x * Vector3.right + z * Vector3.forward;

        transform.Translate(move * _moveSpeed * Time.deltaTime);

        scroll = -Input.GetAxis("Mouse ScrollWheel");


        var orthosize = new Vector2(cam.orthographicSize, 0f);
        var orthosizeNew = new Vector2(cam.orthographicSize + (scroll * _zoomSpeed), 0f);

        var new_size = Mathf.Clamp(Vector2.SmoothDamp(orthosize, orthosizeNew, ref velocity, 0.3f).x, _zoomMinMax.x, _zoomMinMax.y);
        cam.orthographicSize = new_size;


        var new_size_t = Mathf.Lerp(minZoomT, maxZoomT, (new_size - _zoomMinMax.x) / (_zoomMinMax.y - _zoomMinMax.x));

        Shader.SetGlobalFloat("_CameraZoomT", new_size_t);




        //var zoom = cam.orthographicSize * scroll;

        //cam.orthographicSize = Mathf.Clamp(zoom, _zoomMinMax.x, _zoomMinMax.y);



    }
}

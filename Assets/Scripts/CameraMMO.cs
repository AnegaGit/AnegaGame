/*Anega Copyright 2019 www.anega.de

This program is free software: you can redistribute it and / or modify it under the
terms of the MIT X11.
This part based on uMMORPG. You have to purchase the asset at the Unity store.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.
-----------------------------------------------*/
// We developed a simple but useful MMORPG style camera. The player can zoom in
// and out with the mouse wheel and rotate the camera around the hero by holding
// down the right mouse button.
using UnityEngine;

public class CameraMMO : MonoBehaviour
{
    public Transform target;

    int mouseButton = 2;

    public float distance = PlayerPreferences.cameraDistance;
    float minDistance = GlobalVar.cameraMinDistance;
    float maxDistance = GlobalVar.cameraMaxDistance;

    public float zoomSpeedMouse = PlayerPreferences.cameraZoomSpeed;
    float zoomSpeedTouch = GlobalVar.cameraZoomSpeedTouch;
    public float rotationSpeed = PlayerPreferences.cameraRotationSpeed;

    float xAngle = PlayerPreferences.cameraAngle;
    float xMinAngle = GlobalVar.cameraXMinAngle;
    float xMaxAngle = GlobalVar.cameraXMaxAngle;

    bool isDetailZoom = false;
    float fieldOfViewDefault = GlobalVar.cameraFieldOfViewDefault;
    float fieldOfView;

    // the target position can be adjusted by an offset in order to foucs on a
    // target's head for example
    public Vector3 offset = Vector3.zero;

    // view blocking
    // note: only works against objects with colliders.
    //       uMMORPG has almost none by default for performance reasons
    // note: remember to disable the entity layers so the camera doesn't zoom in
    //       all the way when standing inside another entity
    public LayerMask viewBlockingLayers;

    // store rotation so that unity never modifies it, otherwise unity will put
    // it back to 360 as soon as it's <0, which makes a negative min angle
    // impossible
    Vector3 rotation;
    bool rotationFollow = true;
    bool cameraInTouch = false;
    float timeMouseDown = 0f;

    public bool rotationFollowPlayer
    {
        get { return rotationFollow; }
        set { rotationFollow = value; }
    }
    private void Start()
    {
        rotation = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(xAngle, rotation.y, 0);
        fieldOfView = GetComponent<Camera>().fieldOfView;
    }

    //ANEGA changed to other behaviour
    void LateUpdate()
    {
        if (!target) return;
        Player player = Player.localPlayer;

        Vector3 targetPos = target.position + offset;

        // rotation and zoom should only happen if not in a UI right now
        if (!Utils.IsCursorOverUserInterface())
        {
            // right mouse rotation if we have a mouse
            if (Input.mousePresent)
            {
                if (Input.GetMouseButtonDown(mouseButton))
                {
                    // initialize the base rotation if not initialized yet.
                    // (only after first mouse click and not in Awake because
                    //  we might rotate the camera inbetween, e.g. during
                    //  character selection. this would cause a sudden jump to
                    timeMouseDown = Time.time;
                    rotation = transform.eulerAngles;
                    cameraInTouch = true;
                }
                if ((Time.time - timeMouseDown) > PlayerPreferences.keyPressedLongClick && cameraInTouch)
                {
                    // note: mouse x is for y rotation and vice versa
                    rotation.y += Input.GetAxis("Mouse X") * rotationSpeed;
                    rotation.x -= Input.GetAxis("Mouse Y") * rotationSpeed;
                    rotation.x = Mathf.Clamp(rotation.x, xMinAngle, xMaxAngle);
                    transform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0);
                }
                if (Input.GetMouseButtonUp(mouseButton))
                {
                    if ((Time.time - timeMouseDown) <= PlayerPreferences.keyPressedLongClick)
                        rotationFollow = !rotationFollow;
                    cameraInTouch = false;
                }
                if (Input.GetKeyDown(PlayerPreferences.keyToggleCamera))
                {
                    rotationFollow = !rotationFollow;
                }
            }
            else
            {
                // forced 45 degree if there is no mouse to rotate (for mobile)
                transform.rotation = Quaternion.Euler(new Vector3(45, 0, 0));
            }
            if (rotationFollow & !cameraInTouch)
            {
                transform.rotation = Quaternion.Euler(new Vector3(rotation.x, target.transform.localEulerAngles.y, target.transform.localEulerAngles.z));
            }

            // zoom
            float speed = Input.mousePresent ? zoomSpeedMouse : zoomSpeedTouch;
            float step = Utils.GetZoomUniversal() * speed;
            if (isDetailZoom && player != null)
            {
                GetComponent<Camera>().fieldOfView = Mathf.Clamp(GetComponent<Camera>().fieldOfView -= step, player.detailViewMin, fieldOfViewDefault);
                if (GetComponent<Camera>().fieldOfView > fieldOfViewDefault - 0.05f)
                {
                    isDetailZoom = false;
                    distance = minDistance;
                    GetComponent<Camera>().fieldOfView = fieldOfViewDefault;
                }
            }
            else
            {
                distance = Mathf.Clamp(distance - step, minDistance, maxDistance);
                if (distance - step < minDistance && player != null)
                {
                    isDetailZoom = true;
                    distance = -0.1f;
                    GetComponent<Camera>().fieldOfView = fieldOfViewDefault - 0.1f;
                }
            }

        }
        else
            cameraInTouch = false;

        // target follow
        transform.position = targetPos - (transform.rotation * Vector3.forward * distance);

        // avoid view blocking (disabled, see comment at the top)
        RaycastHit hit;
        if (Physics.Linecast(targetPos, transform.position, out hit, viewBlockingLayers))
        {
            // calculate a better distance (with some space between it)
            float d = Vector3.Distance(targetPos, hit.point) - 0.1f;

            // set the final cam position
            transform.position = targetPos - (transform.rotation * Vector3.forward * d);
        }
    }
}

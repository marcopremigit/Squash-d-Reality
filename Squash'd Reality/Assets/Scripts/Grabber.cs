﻿using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Grabber : NetworkBehaviour
{
    RaycastHit hit;
    RaycastHit hit1;
    RaycastHit hit2;
    private LevelManager.LevelManager _levelManager;

    GameObject toGrab;
    private Light light;
    private float luminosity = 10;
    private bool isGrabbing = false;
    private bool needToToggleLight = false;

    bool hitDetect;
    bool hitDetect1;
    bool hitDetect2;
    [SerializeField] private float maxDist = 0.5f;
    int layerMask = 1 << 31;
    private Scene scene;

    private string playerName;
    
    
    void Start()
    {
        playerName = transform.gameObject.GetComponent<PlayerMoveset>().playerName;
        scene = SceneManager.GetActiveScene();
        _levelManager = Object.FindObjectOfType<LevelManager.LevelManager>();
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            if (transform.GetChild(i).name == "PlayerLight")
            {
                light = transform.GetChild(i).gameObject.GetComponent<Light>();
            }
        }

        needToToggleLight = _levelManager.getCurrentLevel().isDark;
        if (needToToggleLight) {
            askToggleLight(true);
        }
        else {
            light.intensity = 0f;
        }
    }

    // Update is called once per frame
    void Update() {
        if (hasAuthority) {
            Grab();
        }
    }

    void Grab()
    {
        bool interacting = Input.GetButton("Interact");
        if (interacting && !isGrabbing) {
            hitDetect = Physics.Raycast(transform.position, transform.forward, out hit, maxDist, layerMask);
            hitDetect1 = Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), transform.forward, out hit1, maxDist, layerMask);
            hitDetect2 = Physics.Raycast(transform.position + new Vector3(0, -0.5f, 0), transform.forward, out hit2, maxDist, layerMask);

            if (hitDetect) setToGrab(hit.collider.gameObject, playerName);
            else if (hitDetect1) setToGrab(hit1.collider.gameObject, playerName);
            else if (hitDetect2) setToGrab(hit2.collider.gameObject, playerName);
        }

        if (!interacting && toGrab != null) {
            removeGrab();
        }

        if (!interacting && isGrabbing)
        {
            isGrabbing = false;
        }
    }

    public void removeGrab() {
        if (scene.name == "CookingTime") {
            toGrab.GetComponent<GrabbableMovementCookingTime>().cubeMovement = false;
        }
        else {
            GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerController>().CmdSetGrabebd(toGrab, false);
            //toGrab.GetComponent<GrabbableMovement>().cubeMovement = false;
        }
        if(toGrab.tag == "Pipe")
        {
            transform.GetComponent<PlayerMoveset>().playerCanMove = true;
            StartCoroutine(waitReleaseGrab());
        }
        else
        {
            gameObject.GetComponent<AudioManager>().playReleaseSound();
            GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerController>().CmdRemoveAuthority(toGrab);
            toGrab = null;
            isGrabbing = false;
        }
        
        if(needToToggleLight) askToggleLight(true);
    }


    IEnumerator waitReleaseGrab()
    {
        yield return new WaitForSeconds(0.3f);
        if (toGrab != null)
        {
            gameObject.GetComponent<AudioManager>().playReleaseSound();
            toGrab.GetComponent<Pipe>().releasedPipe();
            GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerController>().CmdRemoveAuthority(toGrab);
        }
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerController>().CmdSetMesh(gameObject, true);
        toGrab = null;
        isGrabbing = false;
    }
    public void toggleLight(bool val) {
        if(_levelManager.getCurrentLevel().isDark) {
            light.intensity = val ? luminosity : 0;
        }
    }

    private void setToGrab(GameObject go, string playerName) {
        toGrab = go;
        if (toGrab.tag == "Pipe") {
            GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerController>().CmdSetMesh(gameObject, false);
            transform.GetComponent<PlayerMoveset>().playerCanMove = false;
        }

        if (scene.name == "CookingTime") {
            toGrab.GetComponent<GrabbableMovementCookingTime>().cubeMovement = true;
            toGrab.GetComponent<GrabbableMovementCookingTime>().grabbedBy = playerName;
        }
        else {
            GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerController>().CmdSetGrabebd(toGrab, true);
            toGrab.GetComponent<GrabbableMovement>().grabbedBy = playerName;
        }
        isGrabbing = true;
        GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerController>().CmdAssignAuthority(toGrab);
        if(needToToggleLight) askToggleLight(false); 
    }
    
    void OnDrawGizmos() {
        Gizmos.color = Color.red;

        //Check if there has been a hit yet
        if (hitDetect) {
            //Draw a Ray forward from GameObject toward the hit
            Gizmos.DrawRay(transform.position, transform.forward * hit.distance);            
        }
        //If there hasn't been a hit yet, draw the ray at the maximum distance
        else {
            //Draw a Ray forward from GameObject toward the maximum distance
            Gizmos.DrawRay(transform.position, transform.forward * maxDist);
            Gizmos.DrawRay(transform.position + new Vector3(0, 0.5f, 0), transform.forward * maxDist);
            Gizmos.DrawRay(transform.position + new Vector3(0, -0.5f, 0), transform.forward * maxDist);
        }
    }

    public void askToggleLight(bool value) {
        GameObject localPlayer = GameObject.FindGameObjectWithTag("LocalPlayer");
        if (SceneManager.GetActiveScene().name == "DarkPuzzle" && localPlayer != null) {

            string playerName = GetComponentInParent<PlayerMoveset>().playerName;
            if ( playerName == "Markus Nobel") localPlayer.GetComponent<PlayerController>().CmdsetLight1(value);
            else if (playerName == "Ken Nolo") localPlayer.GetComponent<PlayerController>().CmdsetLight2(value);
            else if (playerName == "Kam Brylla") localPlayer.GetComponent<PlayerController>().CmdsetLight3(value);
            else if (playerName == "Raphael Nosun") localPlayer.GetComponent<PlayerController>().CmdsetLight4(value);
        }

        if (SceneManager.GetActiveScene().name != "DarkPuzzle")
        {
            light.intensity = 0f;
        }
    }
}

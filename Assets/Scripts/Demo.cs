/*
This script is a demo of how to:
    - spawn AR objects
        - anchored objects should only be spawned directly on top of real world surfaces, as they are used in calculating the scene depth
        - check the prefabs for reference
    - get and react to touch input
        - check if a selectable AR object is selected
    - calculate distances to AR objects
    - display stuff on UI Canvas
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class Demo : MonoBehaviour{
    
    // device rear camera
    Camera arCamera;
    // virtual camera positioned at the users eyes
    Camera eyesCamera;

    public GameObject cubePrefab;
    public GameObject spherePrefab;

    // UI
    public TMP_Text selctedObjectDistanceText;


    void Start(){
        // get GameObjects from hierarchy
        arCamera = GameObject.FindWithTag("ARCamera").GetComponent<Camera>();
        eyesCamera = GameObject.FindWithTag("EyesCamera").GetComponent<Camera>();
    }

    void Update(){

        // check for touch inputs that are not over UI elements
        if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject()){
            // cast ray to check if an AR object was selected
            Ray ray = eyesCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitData;
            if (Physics.Raycast(ray, out hitData, 1000)){
                GameObject objHit = hitData.transform.gameObject;
                if (objHit != null && objHit.tag == "ARObjectSelectable"){
                    // calculate distance from head position to gameobject
                    float distance = (objHit.transform.position - this.gameObject.GetComponent<FaceCoordinates>().getTrackedPoint()).magnitude;
                    // display distanc on UI and destroz AR object
                    selctedObjectDistanceText.text = "distance: " + distance.ToString("F3") + "m";
                    Destroy(objHit);
                }
            }

        }
                
    }




    public void SpawnCube(){
        GameObject newCube = Instantiate<GameObject>(cubePrefab, arCamera.transform.position, Quaternion.identity);
        newCube.GetComponent<MeshRenderer>().sharedMaterial.renderQueue = 2999; // for occlusion by LIDAR mesh
    }

    public void SpawnSphere(){
        GameObject newSphere = Instantiate<GameObject>(spherePrefab, arCamera.transform.position, Quaternion.identity);
        newSphere.GetComponent<MeshRenderer>().sharedMaterial.renderQueue = 2999; // for occlusion by LIDAR mesh
    }
   
   




   
}

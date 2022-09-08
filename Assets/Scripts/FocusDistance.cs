/*
This script estimates the scene depth by using AR planes, the LiDAR mesh and anchored AR objects
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FocusDistance : MonoBehaviour{
    
    // device rear camera
    Camera arCamera;
    // virtual camera positioned at the users eyes
    Camera eyesCamera;
    // approximate distance in meters to real world scene behind the camera 
    AROcclusionManager occlusionManager;


    float focusDistance = 2;



    // UI Debug-Info
    public TMP_Text backgroundDistanceText;
    public TMP_Text objectDistanceText;
    public TMP_Text focusDistanceText;



    private void Start() {
        // get GameObjects from hierarchy
        arCamera = GameObject.FindWithTag("ARCamera").GetComponent<Camera>();
        eyesCamera = GameObject.FindWithTag("EyesCamera").GetComponent<Camera>();
        occlusionManager = FindObjectOfType<AROcclusionManager>();
    }


    // gets called in ImagePlane.cs Update()
    // calculates an approximate distance to the scene behind the camera in real world
    public float EstimateFocusDistance(){
        
        // distance to LIDAR mesh, used when no AR objects in FOV
        float? backgroundDistanceN = EstimateBackgroundDistance();
        float backgroundDistance = focusDistance;
        if(backgroundDistanceN != null){
            backgroundDistance = backgroundDistanceN.Value;
        }

        
        // if therer is an ARObject in FOV that is anchored to the real world (not floating around), we do not need to estimate the distance
        // find closest anchored objec to center of screen, assuming there lies the focus
        GameObject[] anchoreds = GameObject.FindGameObjectsWithTag("anchor");
        float minCenterDistance = int.MaxValue;
        int indexClosest = 0;
        for(int i=0; i<anchoreds.Length; i++){
            float d = GetCenterDistance(anchoreds[i].transform.position);
            if(d < minCenterDistance){
                minCenterDistance = d;
                indexClosest = i;
            }
        }
        // if the closest ARObject is too far away (not even in FOV), we still return the estimated background distance
        int threshhold = 2000;
        if(minCenterDistance > threshhold){
            focusDistance = backgroundDistance;
            // display distances on UI Debug-Info
            backgroundDistanceText.text = "estimated Background Distance: " + backgroundDistance.ToString("F3");
            objectDistanceText.text = "Distance to Anchored Object in view: -";
            focusDistanceText.text = "estimated Focus Distance: " + focusDistance.ToString("F3");
            return backgroundDistance;
        }
        
        // weighted average
        // the closer the ARObject is to the center the less weight is put on the background distance
        float backgroundFactor = minCenterDistance/threshhold;
        // get position and distance to ARObject
        Vector3 anchoredPos = anchoreds[indexClosest].transform.position;
        Vector3 toAnchored = anchoredPos - arCamera.transform.position;
        // save weighted average for next frame, in case EstimateBackgroundDistance() returns null
        focusDistance = backgroundFactor*backgroundDistance + (1-backgroundFactor)*toAnchored.magnitude;


        // display distances on UI Debug-Info
        backgroundDistanceText.text = "estimated Background Distance: " + backgroundDistance.ToString("F3");
        objectDistanceText.text = "Distance to Anchored Object in view: " + toAnchored.magnitude.ToString("F3");
        focusDistanceText.text = "estimated Focus Distance: " + focusDistance.ToString("F3");
        
        return focusDistance;
    }



    // returns distance to screen center of a world coordinate
    float GetCenterDistance(Vector3 worldPos){
        Vector2 screenPos = eyesCamera.WorldToScreenPoint(worldPos);
        Vector2 toCenter = new Vector2(Screen.width/2, Screen.height/2) - screenPos;
        return toCenter.magnitude;
    }



   

    // casts 9 rays to the LIDAR mesh to estimate background distance
    float? EstimateBackgroundDistance(){
        
        Vector2 p1 = new Vector2(Screen.width/3, Screen.height*0.666f);
        Vector2 p2 = new Vector2(Screen.width/2, Screen.height*0.666f);
        Vector2 p3 = new Vector2(Screen.width*0.666f, Screen.height*0.666f);

        Vector2 p4 = new Vector2(Screen.width/3, Screen.height/2);
        Vector2 p5 = new Vector2(Screen.width/2, Screen.height/2);
        Vector2 p6 = new Vector2(Screen.width*0.666f, Screen.height/2);

        Vector2 p7 = new Vector2(Screen.width/3, Screen.height/3);
        Vector2 p8 = new Vector2(Screen.width/2, Screen.height/3);
        Vector2 p9 = new Vector2(Screen.width*0.666f, Screen.height/3);

        Vector2[] points = new Vector2[]{p1, p2, p3, p4, p5, p6, p7, p8, p9};
        

        // calculate sum to draw an average
        float sumOfDistances = 0;
        float numberOfHits = 0;
        for(int i = 0; i<9; i++){

            float? distancePi = DistanceToMeshPlanesFromScreenPoint(points[i]);
            if(distancePi != null){
                sumOfDistances += distancePi.Value;
                numberOfHits += 1;
            }

        }
        // retrun average if enough rays hit
        if(sumOfDistances <= 2){
            return null;
        }
        return sumOfDistances/numberOfHits;

    }


    // casts a ray from screen point forward and calculates a hit point with the LiDAR mesh or the ARPlanes
    // -> depth estimation more accurate when LiDAR enabled
    float? DistanceToMeshPlanesFromScreenPoint(Vector2 screenPoint){
        Ray ray = eyesCamera.ScreenPointToRay(screenPoint);
        RaycastHit hitData;
        // the LiDAR mesh and the ARPlanes are on layer 6
        if(Physics.Raycast(ray, out hitData, 1000, layerMask: 1 << 6)){
                Vector3 worldPosition = hitData.point;
                Vector3 camToPoint = worldPosition - arCamera.transform.position;
                // return the distance
                return camToPoint.magnitude;
        }else{
            return null;
        }

    }


}

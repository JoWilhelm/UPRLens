/*
This script sets the position and rotation of the camera image plane, depending on the scene depth calculated in FocusDistance.cs and the position of the user's eyes
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ImagePlane : MonoBehaviour{


    // device rear camera
    Camera arCamera;
    // virtual camera positioned at the users eyes
    Camera eyesCamera;
    GameObject master;



    // UI Debug-Info
    public TMP_Text eyesScreenAngleText;
    public TMP_Text zDistanceText;
    public TMP_Text rotationText;


    void Start(){
        // get GameObjects from hierarchy
        arCamera = GameObject.FindWithTag("ARCamera").GetComponent<Camera>();
        eyesCamera = GameObject.FindWithTag("EyesCamera").GetComponent<Camera>();
        master = GameObject.FindWithTag("Master");
    }


    void Update(){

        // real life distance in meters from device camera to objects on screen
        float focusDistance = master.GetComponent<FocusDistance>().EstimateFocusDistance();
        // distance of the ImagePlane to the ARCamera, so that sizes of objects on screen match with sizes in real world
        float zDistance = CalculateImagePlaneZDistance(focusDistance);
        SetImagePlaneZDistance(zDistance);
        // rotate the ImagePlane around the ARCamera, so that position of objects on screen match with positios in real world
        SetImagePlaneRotation();

        // UI Debug-Info
        zDistanceText.text = "Image Plane Z-Distance: " + zDistance.ToString("F3");
    }

    // function probably to some degree dependent on the device used
    float CalculateImagePlaneZDistance(float focusDistance){
        float a = 23.5f;
        float b = 1.85f;
        float c = 10.0f;
        return a * Mathf.Exp(-b * focusDistance) + c;
    }

    public void SetImagePlaneZDistance(float newZDistance){
        Vector3 localPos = transform.localPosition;
        localPos.z = newZDistance;
        transform.localPosition = localPos;
    }

    void SetImagePlaneRotation(){
        // calculate horizontal and vertical angle between eyes position and device camera
        Vector3 eyesCameraRelative = arCamera.transform.InverseTransformPoint(eyesCamera.transform.position); 
        float alpha = Mathf.Atan(-eyesCameraRelative.x/eyesCameraRelative.z);
        float beta = Mathf.Atan(-eyesCameraRelative.y/eyesCameraRelative.z);
        
        float d = transform.localPosition.z;
        float pX = d*Mathf.Tan(alpha);
        float pY = d*Mathf.Tan(beta);

        float wX = d*Mathf.Tan(0.464f);     // This is half the degrees of FOV of the device camera
        float wY = d*Mathf.Tan(0.588f);     // These are just estimatesm, don't know the actual values

        float x = -5*(pX/wX);            // 5 and 6.666 come from the width and height of the image plane
        float y = -6.666f*(pY/wY);

        float aX = Mathf.Atan(x/d);
        float aY = Mathf.Atan(y/d);

        float rotationHorizontal = -alpha - aX;
        float rotationVertical = -beta - aY;

        transform.parent.localRotation = Quaternion.Euler(-rotationVertical*(180/Mathf.PI), rotationHorizontal*(180/Mathf.PI), 0);  
        
        // display angles on UI Debug-Info
        rotationText.text = "Image Plane Rotation: x: " + rotationHorizontal.ToString("F3") + ", y: " + rotationVertical.ToString("F3");
        eyesScreenAngleText.text = "Eyes Anlge to Screen: alpha: " + alpha.ToString("F3") + ", beta: " + beta.ToString("F3");
    } 


    
}

/*
This script tracks the user's face and eye position
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARKit;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using TMPro;

public class FaceCoordinates : MonoBehaviour{

    // device rear camera
    Camera arCamera;
    // virtual camera positioned at the users eyes
    Camera eyesCamera;

    ARFaceManager faceManager;
    ARFace face;
    ARKitFaceSubsystem faceSubsystem;

    bool isLeftEyeClosed;
    bool isRightEyeClosed;

    // window of tracked positions in the last few frames for smoothing
    List<Vector3> slidingWindow = new List<Vector3>();
    int slidingWindowSize = 3;



    // UI Debug-Info
    public TMP_Text eyesDeviceText;
    public TMP_Text eyesWorldText;
    public TMP_Text cameraWorldText;
    
 
    Vector3 trackedPointWorldSpaceSmoothed;


    void Start(){
        // get GameObjects from hierarchy
        arCamera = GameObject.FindWithTag("ARCamera").GetComponent<Camera>();
        eyesCamera = GameObject.FindWithTag("EyesCamera").GetComponent<Camera>();
        faceManager = FindObjectOfType<ARFaceManager>();
        faceSubsystem = (ARKitFaceSubsystem)faceManager.subsystem;
        // subscribe to FacesChanged Event
        faceManager.facesChanged += OnFacesChanged;
    }


    // gets called every time a trackable change of faces in front camera occurs
    public void OnFacesChanged(ARFacesChangedEventArgs args){
        if(args.updated != null && args.updated.Count > 0 && face == null){
            face = args.updated[0];
        }
    }


    void Update(){

        // return if there is no face detected
        if (face == null){ return; }

        // get positions of Eyes
        Vector3 leftEyePos = face.leftEye.position;
        Vector3 rightEyePos = face.rightEye.position;
        Vector3 betweenEyesPos = leftEyePos + 0.5f*(rightEyePos-leftEyePos);

        // choose point for virtual camera
        // between the eyes if both open, otherwise the open one
        checkForClosedEye();
        Vector3 trackedPoint = betweenEyesPos;
        if(isLeftEyeClosed){
            trackedPoint = rightEyePos;
        }else if (isRightEyeClosed){
            trackedPoint = leftEyePos;
        }

        // Change coordinate system of trackedPoint. Changing from world-coordinates to coordinates relative to ARCamera and device.
        // Also smoothing the position by appling average of last few frames
        Vector3 trackedPointCamSpaceSmoothed = trackedPointSmoothing(worldSpaceToCamSpace(trackedPoint));
        trackedPointWorldSpaceSmoothed = camSpaceToWorldSpace(trackedPointCamSpaceSmoothed);
        
        // call function in EyesCameraScript to change position and frustum of EyesCamera
        eyesCamera.gameObject.GetComponent<EyesCamera>().PositionChange(trackedPointCamSpaceSmoothed);

        // display tracked coordinates on UI Debug-Info
        eyesDeviceText.text = "Eyes Device-Space: x: " + trackedPointCamSpaceSmoothed.x.ToString("F3") + ", y: " + trackedPointCamSpaceSmoothed.y.ToString("F3") + ", z: " + trackedPointCamSpaceSmoothed.z.ToString("F3");
        eyesWorldText.text = "Eyes World-Space: x: " + trackedPointWorldSpaceSmoothed.x.ToString("F3") + ", y: " + trackedPointWorldSpaceSmoothed.y.ToString("F3") + ", z: " + trackedPointWorldSpaceSmoothed.z.ToString("F3");
        cameraWorldText.text = "Camera World-Space: x: " + arCamera.transform.position.x.ToString("F3") + ", y: " + arCamera.transform.position.y.ToString("F3") + ", z: " + arCamera.transform.position.z.ToString("F3");      
        
    }

    public Vector3 getTrackedPoint(){
        return trackedPointWorldSpaceSmoothed;
    }



    // checks if one of both eyes is closed 
    private void checkForClosedEye(){
        float leftEyeOpenAmount = 1;
        float rightEyeOpenAmount = 1;
        // access ARKit blendshapes and iterate over them to find the ones for blinking
        Unity.Collections.NativeArray<ARKitBlendShapeCoefficient> blendShapes = faceSubsystem.GetBlendShapeCoefficients(face.trackableId, Unity.Collections.Allocator.Temp);
        foreach(ARKitBlendShapeCoefficient blendShape in blendShapes){
            // blendshape coefficients indicate how far an eye is closed
            if(blendShape.blendShapeLocation == ARKitBlendShapeLocation.EyeBlinkLeft){
                leftEyeOpenAmount = 1-blendShape.coefficient;
            }
            if(blendShape.blendShapeLocation == ARKitBlendShapeLocation.EyeBlinkRight){
                rightEyeOpenAmount = 1-blendShape.coefficient;
            }  
        } 
        // determine an eye is closed iff the other one is more than 1.25x wide open
        float t = 1.25f;
        isLeftEyeClosed = false;
        isRightEyeClosed = false;
        if(rightEyeOpenAmount > leftEyeOpenAmount*t){
            isLeftEyeClosed = true;
        }
        else if(leftEyeOpenAmount > rightEyeOpenAmount*t){
            isRightEyeClosed = true;
        }
    }


    // changes coordinate system of point
    private Vector3 worldSpaceToCamSpace(Vector3 inPos){
        return arCamera.transform.InverseTransformPoint(inPos);
    }

    // changes coordinate system of point
    private Vector3 camSpaceToWorldSpace(Vector3 inPos){
        return arCamera.transform.TransformPoint(inPos);
    }


    // returns an updated average of positions over the last few frames
    Vector3 trackedPointSmoothing(Vector3 newTrackedPoint){        
        if(slidingWindow.Count < slidingWindowSize){
            slidingWindow.Add(newTrackedPoint);
        }else{
            slidingWindow.RemoveAt(0);
            slidingWindow.Add(newTrackedPoint);
        }
        Vector3 sum = new Vector3(0, 0, 0);
        for(int i=0; i<slidingWindow.Count; i++){
            sum += slidingWindow[i];
        }
        Vector3 average = sum/slidingWindow.Count;
        return average;
    }




}

/*
This script calculates the camera frustum, depending on the position of the user's eyes calculated in FaceCoordinates.cs, the device, and the active render-method
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyesCamera : MonoBehaviour{

    // stores size of the device's screen and camera position in meters
    // is set in Sttart()
    float deviceScreenWidth;
    float deviceScreenHeight;
    float cameraFromUpperScreenEdge;
    float cameraFromRightScreenEdge;


    // device rear camera
    Camera arCamera;
    // virtual camera positioned at the users eyes
    Camera eyesCamera;


    // plane with the camera Image and its corners
    // corners are needed device perspective
    GameObject imagePlanePoints;
    GameObject imagePlaneUL;
    GameObject imagePlaneLL;
    GameObject imagePlaneLR;
    GameObject imagePlaneUR;

    
    // in meters from the device camera / ARCamera / (0, 0, 0)
    Vector3 deviceCenter;
    Vector3 cornerUL;
    Vector3 cornerLL;
    Vector3 cornerLR;
    Vector3 cornerUR;
    Vector3 centerToUL;
    Vector3 centerToLL;
    Vector3 centerToLR;
    Vector3 centerToUR;

    // for two methods M1_FOV and M3_Rot
    public float userDeviceInterpolation = 0;
    public float fovScale = 1;

    // used as fallback values when frustum lock to image plane is activated
    Vector3 previousPosition;
    Matrix4x4 previousProjectionMatrix;
    Vector3 previousImagePlanePointsLocalPosition;
    public bool lockToImagePlane = false;

    public enum renderMethod {UPR, DPR, BiggerFOV, UPRDPRInterpolation, Rotation};
    public renderMethod activeRenderMethod;


    void Start(){
        // get GameObjects from hierarchy
        arCamera = GameObject.FindWithTag("ARCamera").GetComponent<Camera>();
        eyesCamera = GameObject.FindWithTag("EyesCamera").GetComponent<Camera>();
        imagePlanePoints = GameObject.FindWithTag("ImagePlane").transform.GetChild(0).gameObject;
        imagePlaneUL = imagePlanePoints.transform.GetChild(0).gameObject;
        imagePlaneLL = imagePlanePoints.transform.GetChild(1).gameObject;
        imagePlaneLR = imagePlanePoints.transform.GetChild(2).gameObject;
        imagePlaneUR = imagePlanePoints.transform.GetChild(3).gameObject;

        // not render screen plane, it would be in the way
        this.gameObject.GetComponentInParent<MeshRenderer>().enabled = false;

        // check on which device the application is running
        string deviceIdentifier = SystemInfo.deviceModel;
        Debug.Log(deviceIdentifier);
        if(deviceIdentifier.Equals("iPhone13,3")){
            // device specifics in meters for iPhone 12 Pro Max
            deviceScreenWidth = 0.0645f;
            deviceScreenHeight = 0.1395f;
            cameraFromUpperScreenEdge = 0.009f;
            cameraFromRightScreenEdge = 0.009f;
        }else if(deviceIdentifier.Equals("iPad14,1")){
            // device specifics in meters for iPad mini 6
            deviceScreenWidth = 0.116f;
            deviceScreenHeight = 0.177f;
            cameraFromUpperScreenEdge = 0.003f;
            cameraFromRightScreenEdge = 0.003f;            
        }

        // adjust local position of imagePlane children (*13.333f due to imagePlaneScale)
        float w = deviceScreenWidth/deviceScreenHeight * 10f * 1.333f / 2;
        imagePlaneUL.transform.localPosition = new Vector3(imagePlaneUL.transform.localPosition.x, imagePlaneUL.transform.localPosition.y, -w);
        imagePlaneLL.transform.localPosition = new Vector3(imagePlaneLL.transform.localPosition.x, imagePlaneLL.transform.localPosition.y, -w);
        imagePlaneLR.transform.localPosition = new Vector3(imagePlaneLR.transform.localPosition.x, imagePlaneLR.transform.localPosition.y, w);
        imagePlaneUR.transform.localPosition = new Vector3(imagePlaneUR.transform.localPosition.x, imagePlaneUR.transform.localPosition.y, w);

        // device specific vectors
        deviceCenter = new Vector3(-(deviceScreenWidth/2)+cameraFromRightScreenEdge, -(deviceScreenHeight/2)+cameraFromUpperScreenEdge, 0);
        cornerUL = new Vector3(-deviceScreenWidth + cameraFromRightScreenEdge, cameraFromUpperScreenEdge, 0); 
        cornerLL = new Vector3(-deviceScreenWidth + cameraFromRightScreenEdge, -deviceScreenHeight + cameraFromUpperScreenEdge, 0);
        cornerLR = new Vector3(cameraFromRightScreenEdge, -deviceScreenHeight + cameraFromUpperScreenEdge);
        cornerUR = new Vector3(cameraFromRightScreenEdge, cameraFromUpperScreenEdge);
        centerToUL = cornerUL-deviceCenter;
        centerToLL = cornerLL-deviceCenter;
        centerToLR = cornerLR-deviceCenter;
        centerToUR = cornerUR-deviceCenter;



        previousPosition = transform.position;
        lockToImagePlane = false;

        activeRenderMethod = renderMethod.UPR;
    }






    // gets called from FaceCoordinates.cs whenever the users eyes move relative to the device
    // changes position of EyesCamera and calculates parameters for projection matrix
    public void PositionChange(Vector3 newPos){

        // other function for M2_Rot render method
        if(activeRenderMethod == renderMethod.Rotation){
            PositionChange_MethodRotation(newPos);
            return;
        }

        // new position is at the users eyes and at the device camera / ARCamera when userDeviceInterpolation is set to 1
        previousPosition = transform.position;
        transform.position = arCamera.transform.position + (1-userDeviceInterpolation) * (arCamera.transform.TransformPoint(newPos) - arCamera.transform.position);
        
        // calculate screen corners
        // convert screen corners to world coordinates, also add FOV scaling
        Vector3 pUL = arCamera.transform.TransformPoint( deviceCenter + (centerToUL*fovScale) );
        Vector3 pLL = arCamera.transform.TransformPoint( deviceCenter + (centerToLL*fovScale) );
        Vector3 pLR = arCamera.transform.TransformPoint( deviceCenter + (centerToLR*fovScale) );
        Vector3 pUR = arCamera.transform.TransformPoint( deviceCenter + (centerToUR*fovScale) );
        Vector3 pC = arCamera.transform.TransformPoint(deviceCenter);
        // screen corners will be the corners of ImagePlane when userDeviceInterpolation is set to 1
        pUL += userDeviceInterpolation*(imagePlaneUL.transform.position - pUL);
        pLL += userDeviceInterpolation*(imagePlaneLL.transform.position - pLL);
        pLR += userDeviceInterpolation*(imagePlaneLR.transform.position - pLR);
        pUR += userDeviceInterpolation*(imagePlaneUR.transform.position - pUR);
        Vector3 pC_DeviceInterpolated = pC + userDeviceInterpolation*(imagePlanePoints.transform.position - pC);
 
        // distance to plane of corners
        float d = eyesCamera.transform.InverseTransformPoint(pC_DeviceInterpolated).z;
        // distances to near and far clip planes
        float f = eyesCamera.farClipPlane;
        float n = eyesCamera.transform.InverseTransformPoint(pC).z; // distance to actual screen
        //n = 0.01f;    //for AR objects to be able to 'come out of the screen', and not be cut off

        setProjectionMatrix(pUL, pLL, pLR, pUR, d, n, f);
    }



    // changes positiion+rotation of EyesCamera and calculates parameters for projection matrix
    public void PositionChange_MethodRotation(Vector3 newPos){
        
        previousPosition = transform.position;
        transform.position = arCamera.transform.position;

        // calculate horizontal and vertical angle between eyes position and device screen normal
        Vector3 eyesRelative = newPos; 
        float alpha = Mathf.Atan((eyesRelative.x + 0.02125f)/(-eyesRelative.z));
        float beta = Mathf.Atan((eyesRelative.y + 0.05875f)/(-eyesRelative.z));

        float slideFactor = 10f;
        float slideHorizontal = alpha*slideFactor;
        float slideVertical = beta*slideFactor;
        if(lockToImagePlane){
            slideVertical = 0;
        }
        // slide the frustum corner points on the image plane
        // the coordinates are swiched up because of the imagePlane's local rotation
        previousImagePlanePointsLocalPosition = imagePlanePoints.transform.localPosition;
        imagePlanePoints.transform.localPosition = new Vector3(slideVertical, 0, -slideHorizontal);

        // corners
        Vector3 pUL = imagePlaneUL.transform.position;
        Vector3 pLL = imagePlaneLL.transform.position;
        Vector3 pLR = imagePlaneLR.transform.position;
        Vector3 pUR = imagePlaneUR.transform.position;
        Vector3 pC = imagePlanePoints.transform.position;

        // distance to plane of corners
        float d = eyesCamera.transform.InverseTransformPoint(pC).z;
        // distances to near and far clip planes
        float f = eyesCamera.farClipPlane;
        float n = 0.01f;
         
        setProjectionMatrix(pUL, pLL, pLR, pUR, d, n, f);
    }




    // sets the projection matrix for the camera frustum
    // for reference and further explanations: http://www.songho.ca/opengl/gl_projectionmatrix.html
    void setProjectionMatrix(Vector3 pUL, Vector3 pLL, Vector3 pLR, Vector3 pUR, float d, float n, float f){

        Vector3 pE = transform.position;
        Vector3 toUL = pUL - pE;
        Vector3 toLL = pLL - pE;
        Vector3 toLR = pLR - pE;
        Vector3 toUR = pUR - pE;
        
        float l = eyesCamera.transform.InverseTransformDirection(toUL).x * (n/d);
        float r = eyesCamera.transform.InverseTransformDirection(toUR).x * (n/d);
        float t = eyesCamera.transform.InverseTransformDirection(toUL).y * (n/d);
        float b = eyesCamera.transform.InverseTransformDirection(toLL).y * (n/d);
 
        Matrix4x4 pm = new Matrix4x4();
        pm[0, 0] = 2*n / (r - l);
        pm[0, 2] = (r + l) / (r - l);
        pm[1, 1] = 2*n / (t - b);
        pm[1, 2] = (t + b) / (t - b);
        pm[2, 2] = (f + n) / (n - f);
        pm[2, 3] = 2*f*n / (n - f);
        pm[3, 2] = -1;

        previousProjectionMatrix = eyesCamera.projectionMatrix;
        eyesCamera.projectionMatrix = pm;

        if(lockToImagePlane){
            LockFrustumToImagePlane();
        }
    }


    // locks frustum to camera image to avoid black borders
    // undos changes if any of the frustum coreners are outside the image plane
    void LockFrustumToImagePlane(){
        // check if one of the four frustum corners is outside the image plane by casting rays
        Vector2[] screenCorners = new Vector2[]{ new Vector2(0, 0),
                                                    new Vector2(0, Screen.height),
                                                    new Vector2(Screen.width, 0),
                                                    new Vector2(Screen.width, Screen.height)
                                                    };
        for(int i=0; i<4; i++){
            Vector2 screenPoint = screenCorners[i]; 
            Ray ray = eyesCamera.ScreenPointToRay(screenPoint);
            RaycastHit hitData;
            // the image plane is on layer 7
            if(!Physics.Raycast(ray, out hitData, 1000, layerMask: 1 << 7)){
                // outside image plane
                // undo update on position and projection matrix
                this.transform.position = previousPosition;
                eyesCamera.projectionMatrix = previousProjectionMatrix;
                imagePlanePoints.transform.localPosition = previousImagePlanePointsLocalPosition;
                return;
            }
        }
    }
        


}

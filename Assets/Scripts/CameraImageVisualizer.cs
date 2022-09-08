/*
This script grabs the lates image captured by the device's rear camera and puts it as a texture on the camera image plane
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

public class CameraImageVisualizer : MonoBehaviour{
      
    ARCameraManager cameraManager;
    // material of ImagePlane to put the new camera image texture on
    public Material imagePlaneMaterial;
    Texture2D imageTexture;



    private void Start() {
        cameraManager = FindObjectOfType<ARCameraManager>();
    }

    private void Update() {
        // try to access latest camera image on the cpu
        if ( cameraManager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage)){
            using (cpuImage){
                UpdateCameraImage(cpuImage);
            }
        }
    }

    void UpdateCameraImage(XRCpuImage cpuImage){
        // create Texture2D (only on first call of function)
        if(imageTexture == null){
            imageTexture = new Texture2D(cpuImage.width, cpuImage.height, TextureFormat.RGBA32, false);
        }
        // mirror vertical 
        var conversionParams = new XRCpuImage.ConversionParams(cpuImage, TextureFormat.RGBA32, XRCpuImage.Transformation.MirrorY);
        // convert cpuImage to Texture2D
        var rawTextureData = imageTexture.GetRawTextureData<byte>();
        cpuImage.Convert(conversionParams, rawTextureData);
        imageTexture.Apply();

        // set image as texture of ImagePlane
        imagePlaneMaterial.mainTexture = imageTexture;
    }
    

}

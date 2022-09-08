/*
This script handles the Menu UI and host all functions triggered by buttons in the Menu
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Menu : MonoBehaviour{

    public GameObject menu;
    public GameObject debugInfo;
    public GameObject FOVSlider;
    public Toggle FOVSliderToggle;
    public TMP_Text FOVSliderValueText;

    public GameObject UPRDPRInterpolationSlider; 
    public Toggle UPRDPRInterpolationSliderToggle;
    public TMP_Text UPRDPRInterpolationSliderValueText;

    EyesCamera eyesCameraScript;


    void Start(){
        eyesCameraScript = GameObject.FindWithTag("EyesCamera").GetComponent<EyesCamera>();
    }

    // on button click
    public void ShowMenu(){
        menu.SetActive(true);   
    }
    // on button click
    public void HideMenu(){
        menu.SetActive(false);
    }


    // on button click
    public void SelectUPR(){
        FOVSliderToggle.interactable = false;
        UPRDPRInterpolationSliderToggle.interactable = false;

        eyesCameraScript.activeRenderMethod = EyesCamera.renderMethod.UPR;
        eyesCameraScript.userDeviceInterpolation = 0;
        eyesCameraScript.fovScale = 1;
        UPRDPRInterpolationSlider.SetActive(false);
        FOVSlider.SetActive(false);
    }
    // on button click
    public void SelectDPR(){
        FOVSliderToggle.interactable = false;
        UPRDPRInterpolationSliderToggle.interactable = false;

        eyesCameraScript.activeRenderMethod = EyesCamera.renderMethod.DPR;
        eyesCameraScript.userDeviceInterpolation = 0.95f;
        eyesCameraScript.fovScale = 1;
        UPRDPRInterpolationSlider.SetActive(false);
        FOVSlider.SetActive(false);
    }
    // on button click
    public void SelectUPRDPRInterpolation(){
        UPRDPRInterpolationSliderToggle.interactable = true;
        FOVSliderToggle.interactable = false;

        eyesCameraScript.activeRenderMethod = EyesCamera.renderMethod.UPRDPRInterpolation;
        eyesCameraScript.userDeviceInterpolation = UPRDPRInterpolationSlider.GetComponent<Slider>().value;;
        eyesCameraScript.fovScale = 1;

        FOVSlider.SetActive(false);
        UPRDPRInterpolationSlider.SetActive(UPRDPRInterpolationSliderToggle.isOn);
    }
    // on button click
    public void SelectBiggerFOV(){
        FOVSliderToggle.interactable = true;
        UPRDPRInterpolationSliderToggle.interactable = false;

        eyesCameraScript.activeRenderMethod = EyesCamera.renderMethod.BiggerFOV;
        eyesCameraScript.userDeviceInterpolation = 0;
        eyesCameraScript.fovScale = FOVSlider.GetComponent<Slider>().value;
        UPRDPRInterpolationSlider.SetActive(false);
        FOVSlider.SetActive(FOVSliderToggle.isOn);
    }
    // on button click
    public void SelectRotation(){
        FOVSliderToggle.interactable = false;
        UPRDPRInterpolationSliderToggle.interactable = false;
        
        eyesCameraScript.activeRenderMethod = EyesCamera.renderMethod.Rotation;
        eyesCameraScript.userDeviceInterpolation = 0;
        eyesCameraScript.fovScale = 1;
        UPRDPRInterpolationSlider.SetActive(false);
        FOVSlider.SetActive(false);
    }


    // reacts to changes in the UI Sliders
    public void SetUserDeviceInterpolation(float newInterpolation){
        UPRDPRInterpolationSliderValueText.text = newInterpolation.ToString("F3");
        eyesCameraScript.userDeviceInterpolation = newInterpolation;
    }
    public void SetFOVScale(float newFOVScale){
        FOVSliderValueText.text = newFOVScale.ToString("F3");
        eyesCameraScript.fovScale = newFOVScale;
    }


    // react to changes in the UI Toggles
    public void OnFOVSliderToggleChange(bool newValue){
        FOVSlider.SetActive(newValue);
    }
    public void OnInterpolationSliderToggleChange(bool newValue){
        UPRDPRInterpolationSlider.SetActive(newValue);
    }
    public void OnDebugInfoToggleChange(bool newValue){
        debugInfo.SetActive(newValue);
    }
    public void OnLockFrustumToggleChange(bool newValue){
        eyesCameraScript.lockToImagePlane = newValue;
    }






}

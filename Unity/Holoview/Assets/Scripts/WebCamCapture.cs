using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Windows.WebCam;
using UnityEngine.WSA;

public class WebCamCapture : MonoBehaviour
{
    public static WebCamCapture instance;

    PhotoCapture photoCaptureObject = null;
    Texture2D targetTexture = null;
    Resolution cameraResolution;
    string ipAddress = "";

    // Use this for initialization
    void Start()
    {
        instance = this;
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

        // Create a PhotoCapture object
        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject)
        {
            photoCaptureObject = captureObject;
            CameraParameters cameraParameters = new CameraParameters();
            cameraParameters.hologramOpacity = 0.0f;
            cameraParameters.cameraResolutionWidth = cameraResolution.width;
            cameraParameters.cameraResolutionHeight = cameraResolution.height;
            cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

            // Activate the camera
            photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result)
            {
#if UNITY_EDITOR
                WebCamCapture.SetIpAddress("192.168.31.81");
#endif
            });
        });


    }

    public static void SetIpAddress(string ipAddress)
    {
        WebCamCapture.instance.ipAddress = ipAddress;
        StartCapturing();
    }

    static void StartCapturing()
    {
        WebCamCapture.instance.photoCaptureObject.TakePhotoAsync(WebCamCapture.instance.OnCapturedPhotoToMemory);
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {

        if (!result.success)
        {
            Debug.LogError("OnCapturedPhotoToMemory success = false");
        }
        // Copy the raw image data into the target texture
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);

        byte[] bytes = targetTexture.EncodeToPNG();

        StartCoroutine(SendRequest(bytes));
    }

    IEnumerator SendRequest(byte[] bytes)
    {
        QrCodeRequest qrCodeRequest = new QrCodeRequest();
        qrCodeRequest.qrCode = bytes;
        string jsonData = JsonUtility.ToJson(qrCodeRequest);
        Debug.Log(jsonData);
        using (UnityWebRequest www = UnityWebRequest.Post("http://" + ipAddress + ":3000/Qr", jsonData))
        {
            www.SetRequestHeader("content-type", "application/json");
            www.uploadHandler.contentType = "application/json";
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));

            yield return www.SendWebRequest();


            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                Launcher.LaunchUri(www.downloadHandler.text, false);
            }
        }

        photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        // Shutdown the photo capture resource
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    [System.Serializable]
    class QrCodeRequest
    {
        public byte[] qrCode;
    }
}

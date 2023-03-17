using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Windows.WebCam;
public class WebCamCapture : MonoBehaviour
{
    PhotoCapture photoCaptureObject = null;
    Texture2D targetTexture = null;
    float timer = 0;
    //GameObject quad;
    //Renderer quadRenderer;

    // Use this for initialization
    void Start()
    {
        //quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        //quadRenderer = quad.GetComponent<Renderer>() as Renderer;
        //quadRenderer.material = new Material(Shader.Find("Custom/Unlit/UnlitTexture"));

        //quad.transform.parent = this.transform;
        //quad.transform.localPosition = new Vector3(0.0f, 0.0f, 3.0f);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 5)
        {
            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
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
                    // Take a picture
                    photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
                });
            });

            timer = 0;
        }
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if(!result.success)
        {
            Debug.LogError("OnCapturedPhotoToMemory success = false");
        }
        // Copy the raw image data into the target texture
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);
        
        //quadRenderer.material.SetTexture("_MainTex", targetTexture);

        byte[] bytes = targetTexture.EncodeToPNG();
        
        //string bytesAsString = UnicodeEncoding.Unicode.GetString(bytes);

        StartCoroutine(SendRequest(bytes));

        // Deactivate the camera
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    IEnumerator SendRequest(byte[] bytes)
    {
        QrCodeRequest qrCodeRequest = new QrCodeRequest();
        qrCodeRequest.qrCode = bytes;
        string jsonData = JsonUtility.ToJson(qrCodeRequest);
        Debug.Log(jsonData);
        using (UnityWebRequest www = UnityWebRequest.Post("http://192.168.31.81:3000/Qr", jsonData)) 
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
                Debug.Log(www.downloadHandler.data);
            }
        }
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

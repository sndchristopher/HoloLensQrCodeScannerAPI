using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IpManager : MonoBehaviour
{
    UnityEngine.TouchScreenKeyboard keyboard = null;
    [SerializeField] TMP_Text txtIpAddress;

    // Update is called once per frame
    void Update()
    {
        if(keyboard != null)
        {
            txtIpAddress.text = keyboard.text;
        }

        if(keyboard != null && keyboard.status == TouchScreenKeyboard.Status.Done)
        {
            string ipAdress = keyboard.text;
            Debug.Log(ipAdress);
            WebCamCapture.SetIpAddress(ipAdress);

            gameObject.SetActive(false);
            keyboard = null;
        }
    }

    public void OnChangeIpClicked()
    {
        keyboard = TouchScreenKeyboard.Open("192.168.31.81", TouchScreenKeyboardType.DecimalPad);
    }
}
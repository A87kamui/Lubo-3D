using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Information : MonoBehaviour
{
    public static Information instance;
    public TextMeshProUGUI infoText;

    private void Awake()
    {
        instance = this;
        infoText.text = "";
    }

    public void ShowMessage(string _text)
    {
        infoText.text = _text;
    }
}

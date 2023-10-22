using System;
using System.Collections;
using System.Collections.Generic;
using Vrs.Internal;
using UnityEngine;

public class IPDeditor : MonoBehaviour
{
    // Start is called before the first frame update

    private TextMesh ipdText;
    private float ipd = 64;

    public void incIPD()
    {
        if (ipd+1f > 70) return;
        ipd += 1f;
        applyIPD();
    }

    public void decIPD()
    {
        if (ipd-1f <= 50) return;
        ipd -= 1f;
        applyIPD();
    }

    private void applyIPD()
    {
        PlayerPrefs.SetFloat("ipd", ipd);
        ipdText.text = String.Format("Межзрачковое расстояние: {0} мм", ipd);
        var dist = ipd / 1000.0f;
        VrsViewer.Instance.SetIpd(dist);
    }

    void Start()
    {
        ipdText = GameObject.Find("IPDtext").GetComponent<TextMesh>();

        if (PlayerPrefs.HasKey("ipd"))
        {
            ipd = PlayerPrefs.GetFloat("ipd");
            applyIPD();
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}

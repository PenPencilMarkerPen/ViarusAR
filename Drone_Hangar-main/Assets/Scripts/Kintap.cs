using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kintap : MonoBehaviour
{
    // Start is called before the first frame update
    private UniWebView webViewf;
    void Start()
    {
        webViewf = gameObject.AddComponent<UniWebView>();
        webViewf.Frame = new Rect(200, 200, 1000, 800);
        webViewf.SetTransparencyClickingThroughEnabled(true);
        // Make Unity scene visible.
        webViewf.BackgroundColor = new Color(1f, 1f, 1f, 0.3f);
        webViewf.Load("https://yandex.ru/");
        webViewf.Show();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using Vrs.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FovSettter : MonoBehaviour
{
    [SerializeField] private float FOV;

    void Start()
    {
        VrsViewer.Instance.UpdateCameraFov(FOV);
    }
}

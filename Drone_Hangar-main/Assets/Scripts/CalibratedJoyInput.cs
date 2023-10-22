using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibratedJoyInput : MonoBehaviour
{
    private DroneMovement droneScript;

    private float yaw_deadzone = 0.05f;
    private float throttle_deadzone = 0.01f;
    private float roll_deadzone = 0.05f;
    private float pitch_deadzone = 0.05f;

    private float offset = 0.2f;
    private float coeff = 0.3683f;

    public float debug;
    public bool suppress;
    public bool noJoy;
    private TextMesh joy;

    private float map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    // Start is called before the first frame update
    void Start()
    {
        droneScript = GetComponent<DroneMovement>();
        //joy = GameObject.Find("JoyInfoText").GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
        bool haveEdgeTX = String.Join("\n", Input.GetJoystickNames()).IndexOf("Jumper TLite")!=-1;

        float yaw;
        if (!haveEdgeTX)
            yaw = Input.GetAxis("Left_X");
        else
            yaw = Input.GetAxis("Left_X_2");

        float throttle = Input.GetAxis("Left_Y");
        float roll = Input.GetAxis("Horizontal");
        float pitch = Input.GetAxis("Vertical");

        noJoy = (yaw == 0 && throttle == 0 && roll == 0 && pitch == 0) && !haveEdgeTX; // USB-свисток не воткнут, ничего не надо делать!

        if (!haveEdgeTX)
        {

            throttle -= offset;
            throttle = map(throttle, -coeff, coeff, -1, 1);
            if (Mathf.Abs(throttle) < throttle_deadzone) throttle = 0;
            throttle *= 0.25f;

            yaw -= offset;
            yaw = map(yaw, -coeff, coeff, -1, 1);
            if (Mathf.Abs(yaw) < yaw_deadzone) yaw = 0;

            roll -= offset;
            roll = map(roll, -coeff, coeff, -1, 1);
            if (Mathf.Abs(roll) < roll_deadzone) roll = 0;

            pitch -= offset;
            pitch = map(pitch, -coeff, coeff, -1, 1);
            if (Mathf.Abs(pitch) < pitch_deadzone) pitch = 0;
        }
        else
        {
            throttle *= -1;
            roll *= -1;
            pitch *= -1;
        }

        if (suppress || noJoy)
        {
            yaw = 0;
            roll = 0;
            pitch = 0;
            throttle = 0;
        }

        if (yaw > 0)
        { droneScript.customFeed_rotateLeft = 0; droneScript.customFeed_rotateRight = yaw; }
        else { droneScript.customFeed_rotateLeft = Mathf.Abs(yaw); droneScript.customFeed_rotateRight = 0; }

        if (roll > 0)
        { droneScript.customFeed_leftward = 0; droneScript.customFeed_rightward = roll; }
        else { droneScript.customFeed_leftward = Mathf.Abs(roll); droneScript.customFeed_rightward = 0; }

        if (pitch > 0) { droneScript.customFeed_backward = 0; droneScript.customFeed_forward = pitch; }
        else { droneScript.customFeed_backward = Mathf.Abs(pitch); droneScript.customFeed_forward = 0; }

        if (throttle > 0) { droneScript.customFeed_downward = 0; droneScript.customFeed_upward = throttle; }
        else { droneScript.customFeed_downward = Mathf.Abs(throttle); droneScript.customFeed_upward = 0; }

    }
}

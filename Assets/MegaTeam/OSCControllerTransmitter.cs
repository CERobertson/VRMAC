using UnityEngine;
using System.Collections;
using System;
using OSCsharp.Data;
using Valve.VR;
using System.Collections.Generic;

public class OSCControllerTransmitter : OSCTransmitter
{
    public Sound soundMarker;
    public LastStroke lastStroke;
    public float width = 0.001f;

    private List<Vector3> points;
    private GameObject strokes;
    private GameObject stroke;

    SteamVR_TrackedController tracked_controller;
    SteamVR_Controller.Device controller;

    private string trigger_address;
    private string audiomesh_address = "/Audiomesh";
    private string reset_address = "/Clear";
    private string scene_object = "Scene";
    private string stroke_object = "Stroke";
    private int stroke_index = 0;
    private bool clicked = false;

    // Use this for initialization
    void Start()
    {
        Init();
        strokes = new GameObject();
        points = new List<Vector3>();
        trigger_address = string.Format("/{0}/Trigger", device);

        tracked_controller = GetComponent<SteamVR_TrackedController>();
        tracked_controller.TriggerClicked += HandleTriggerClicked;
        tracked_controller.TriggerUnclicked += Controller_TriggerUnclicked;
        tracked_controller.MenuButtonUnclicked += Controller_MenuUnclicked;
    }

    // Update is called once per frame
    void Update()
    {
        controller = SteamVR_Controller.Input((int)tracked_controller.controllerIndex);
        if (clicked)
        {
            points.Add(transform.position);

            Vector2 triggerPosition = controller.GetAxis(EVRButtonId.k_EButton_SteamVR_Trigger);

            OscMessage m = new OscMessage(trigger_address);
            m.Append(triggerPosition.x);
            transmitter.Send(m);

            SendPosition();

            soundMarker.transform.position = transform.position;
            soundMarker.AudioIndex = Sound.GlobalIndex;
            stroke_index++;
            soundMarker.Index = stroke_index;
            Instantiate(soundMarker, stroke.transform);
        }
    }

    private void HandleTriggerClicked(object sender, ClickedEventArgs e)
    {
        clicked = true;
        points.Clear();
        stroke = new GameObject(stroke_object, typeof(Stroke));
        stroke.transform.parent = strokes.transform;
        stroke_index = -1;

        OscMessage m = new OscMessage(trigger_address);
        m.Append(transform.position.x);
        transmitter.Send(m);
    }

    private void Controller_TriggerUnclicked(object sender, ClickedEventArgs e)
    {
        clicked = false;
        lastStroke.points = points.ToArray();
        stroke.GetComponent<Stroke>().NumberOfStrokes = points.Count;

        OscMessage m = new OscMessage(trigger_address);
        m.Append(0);
        transmitter.Send(m);

        soundMarker.AudioIndex = Sound.GlobalIndex++;
    }

    private void Controller_MenuUnclicked(object sender, ClickedEventArgs e)
    {
        OscMessage m = new OscMessage(reset_address);
        m.Append(0.0f);
        transmitter.Send(m);
        DestroyImmediate(strokes);
        strokes = new GameObject();
        Sound.GlobalIndex = 1;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!clicked)
        {
            var s = stroke.GetComponent<Stroke>();
            if (s.NumberOfStrokes > 0)
            {
                var sound = collision.gameObject.GetComponent<Sound>();
                OscMessage m = new OscMessage(audiomesh_address);
                m.Append(sound.AudioIndex);
                m.Append(0.0);
                var percentage = ((float)sound.Index) / s.NumberOfStrokes;
                m.Append(percentage);
                transmitter.Send(m);
            }
        }
    }
}
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using Obsidian.Elements;
using Obsidian;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Devices;

[NodeName("MIDI Input Events")]
[NodeCategory("Obsidian/Devices")]
public class MIDI_InputEvents : VoidNode<FrooxEngineContext>
{
    public readonly GlobalRef<MIDI_InputDevice> Device;

    public Call NoteOn;

    public Call NoteOff;

    //public readonly ObjectOutput<Component> Source;

    //public readonly ValueOutput<float3> GlobalPoint;

    private ObjectStore<MIDI_InputDevice> _currentDevice;

    private ObjectStore<InputDeviceEventHandler> _noteOn;

    private ObjectStore<InputDeviceEventHandler> _noteOff;

    public override bool CanBeEvaluated => false;

    private void OnDeviceChanged(MIDI_InputDevice device, FrooxEngineContext context)
    {
        MIDI_InputDevice device2 = _currentDevice.Read(context);
        if (device == device2)
        {
            return;
        }
        if (device2 != null)
        {
            device2.NoteOn -= _noteOn.Read(context);
            device2.NoteOff -= _noteOff.Read(context);
        }
        if (device != null)
        {
            NodeContextPath path = context.CaptureContextPath();
            context.GetEventDispatcher(out var dispatcher);
            //ButtonEventHandler value = delegate (IButton b, ButtonEventData e)
            //{
            //    dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
            //    {
            //        OnPressed(b, in e, c);
            //    });
            //};
            InputDeviceEventHandler value = delegate (MIDI_InputDevice dev)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnNoteOn(dev, c);
                });
            };
            InputDeviceEventHandler value2 = delegate (MIDI_InputDevice dev)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnNoteOff(dev, c);
                });
            };
            _currentDevice.Write(device, context);
            _noteOn.Write(value, context);
            _noteOff.Write(value2, context);
            device.NoteOn += value;
            device.NoteOff += value2;
        }
        else
        {
            _currentDevice.Clear(context);
            _noteOn.Clear(context);
            _noteOff.Clear(context);
        }
    }

    private void WriteEventData(in ButtonEventData eventData, FrooxEngineContext context)
    {
        //Source.Write(eventData.source, context);
        //GlobalPoint.Write(eventData.globalPoint, context);
        //LocalPoint.Write(eventData.localPoint, context);
        //NormalizedPoint.Write(eventData.normalizedPressPoint, context);
    }

    private void OnNoteOn(MIDI_InputDevice device, FrooxEngineContext context)
    {
        //WriteEventData(in eventData, context);
        NoteOn.Execute(context);
    }

    private void OnNoteOff(MIDI_InputDevice device, FrooxEngineContext context)
    {
        //WriteEventData(in eventData, context);
        NoteOff.Execute(context);
    }

    public MIDI_InputEvents()
    {
        Device = new GlobalRef<MIDI_InputDevice>(this, 0);
        //Source = new ObjectOutput<Component>(this);
        //GlobalPoint = new ValueOutput<float3>(this);
        //LocalPoint = new ValueOutput<float2>(this);
        //NormalizedPoint = new ValueOutput<float2>(this);
    }
}
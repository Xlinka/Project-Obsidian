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

[NodeName("MIDI Polyphonic Aftertouch Event")]
[NodeCategory("Obsidian/Devices")]
public class MIDI_AftertouchEvent : VoidNode<FrooxEngineContext>
{
    public readonly GlobalRef<MIDI_InputDevice> Device;

    public Call Aftertouch;

    public readonly ValueOutput<int> Channel;

    public readonly ValueOutput<int> Note;

    public readonly ValueOutput<int> Pressure;

    public readonly ValueOutput<float> NormalizedPressure;

    private ObjectStore<MIDI_InputDevice> _currentDevice;

    private ObjectStore<MIDI_AftertouchEventHandler> _aftertouch;

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
            device2.Aftertouch -= _aftertouch.Read(context);
        }
        if (device != null)
        {
            NodeContextPath path = context.CaptureContextPath();
            context.GetEventDispatcher(out var dispatcher);
            MIDI_AftertouchEventHandler value3 = delegate (MIDI_InputDevice dev, MIDI_AftertouchEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnAftertouch(dev, in e, c);
                });
            };
            _currentDevice.Write(device, context);
            _aftertouch.Write(value3, context);
            device.Aftertouch += value3;
        }
        else
        {
            _currentDevice.Clear(context);
            _aftertouch.Clear(context);
        }
    }

    private void WriteAftertouchEventData(in MIDI_AftertouchEventData eventData, FrooxEngineContext context)
    {
        Channel.Write(eventData.channel, context);
        Note.Write(eventData.note, context);
        Pressure.Write(eventData.pressure, context);
        NormalizedPressure.Write(eventData.pressure / 127f, context);
    }

    private void OnAftertouch(MIDI_InputDevice device, in MIDI_AftertouchEventData eventData, FrooxEngineContext context)
    {
        WriteAftertouchEventData(in eventData, context);
        Aftertouch.Execute(context);
    }

    public MIDI_AftertouchEvent()
    {
        Device = new GlobalRef<MIDI_InputDevice>(this, 0);
        Channel = new ValueOutput<int>(this);
        Note = new ValueOutput<int>(this);
        Pressure = new ValueOutput<int>(this);
        NormalizedPressure = new ValueOutput<float>(this);
    }
}
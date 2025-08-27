using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using Obsidian.Elements;
using Obsidian.Components.Devices.MIDI;
using Elements.Data;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Devices;

[NodeName("MIDI Polyphonic Aftertouch Event")]
[NodeCategory("Obsidian/Devices/MIDI")]
[OldTypeName("FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Devices.MIDI_AftertouchEvent")]
public class MIDI_PolyphonicAftertouchEvent : VoidNode<FrooxEngineContext>
{
    public readonly GlobalRef<MIDI_InputDevice> Device;

    public Call PolyphonicAftertouch;

    public readonly ValueOutput<int> Channel;

    public readonly ValueOutput<int> Note;

    public readonly ValueOutput<int> Pressure;

    public readonly ValueOutput<float> NormalizedPressure;

    private ObjectStore<MIDI_InputDevice> _currentDevice;

    private ObjectStore<MIDI_PolyphonicAftertouchEventHandler> _polyphonicAftertouch;

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
            device2.PolyphonicAftertouch -= _polyphonicAftertouch.Read(context);
        }
        if (device != null)
        {
            NodeContextPath path = context.CaptureContextPath();
            context.GetEventDispatcher(out var dispatcher);
            MIDI_PolyphonicAftertouchEventHandler value3 = delegate (IMidiInputListener sender, MIDI_PolyphonicAftertouchEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnPolyphonicAftertouch(sender, in e, c);
                });
            };
            _currentDevice.Write(device, context);
            _polyphonicAftertouch.Write(value3, context);
            device.PolyphonicAftertouch += value3;
        }
        else
        {
            _currentDevice.Clear(context);
            _polyphonicAftertouch.Clear(context);
        }
    }

    private void WritePolyphonicAftertouchEventData(in MIDI_PolyphonicAftertouchEventData eventData, FrooxEngineContext context)
    {
        Channel.Write(eventData.channel, context);
        Note.Write(eventData.note, context);
        Pressure.Write(eventData.pressure, context);
        NormalizedPressure.Write(eventData.normalizedPressure, context);
    }

    private void OnPolyphonicAftertouch(IMidiInputListener sender, in MIDI_PolyphonicAftertouchEventData eventData, FrooxEngineContext context)
    {
        WritePolyphonicAftertouchEventData(in eventData, context);
        PolyphonicAftertouch.Execute(context);
    }

    public MIDI_PolyphonicAftertouchEvent()
    {
        Device = new GlobalRef<MIDI_InputDevice>(this, 0);
        Channel = new ValueOutput<int>(this);
        Note = new ValueOutput<int>(this);
        Pressure = new ValueOutput<int>(this);
        NormalizedPressure = new ValueOutput<float>(this);
    }
}
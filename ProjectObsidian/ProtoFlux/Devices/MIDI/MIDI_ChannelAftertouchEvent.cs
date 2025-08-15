using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using Obsidian.Elements;
using Components.Devices.MIDI;
using Elements.Data;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Devices;

[NodeName("MIDI Channel Aftertouch Event")]
[NodeCategory("Obsidian/Devices/MIDI")]
[OldTypeName("FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Devices.MIDI_ChannelPressureEvent")]
public class MIDI_ChannelAftertouchEvent : VoidNode<FrooxEngineContext>
{
    public readonly GlobalRef<MIDI_InputDevice> Device;

    public Call ChannelAftertouch;

    public readonly ValueOutput<int> Channel;

    public readonly ValueOutput<int> Pressure;

    public readonly ValueOutput<float> NormalizedPressure;

    private ObjectStore<MIDI_InputDevice> _currentDevice;

    private ObjectStore<MIDI_ChannelAftertouchEventHandler> _channelAftertouch;

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
            device2.ChannelAftertouch -= _channelAftertouch.Read(context);
        }
        if (device != null)
        {
            NodeContextPath path = context.CaptureContextPath();
            context.GetEventDispatcher(out var dispatcher);
            MIDI_ChannelAftertouchEventHandler value3 = delegate (IMidiInputListener sender, MIDI_ChannelAftertouchEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnChannelAftertouch(sender, in e, c);
                });
            };
            _currentDevice.Write(device, context);
            _channelAftertouch.Write(value3, context);
            device.ChannelAftertouch += value3;
        }
        else
        {
            _currentDevice.Clear(context);
            _channelAftertouch.Clear(context);
        }
    }

    private void WriteChannelAftertouchEventData(in MIDI_ChannelAftertouchEventData eventData, FrooxEngineContext context)
    {
        Channel.Write(eventData.channel, context);
        Pressure.Write(eventData.pressure, context);
        NormalizedPressure.Write(eventData.normalizedPressure, context);
    }

    private void OnChannelAftertouch(IMidiInputListener sender, in MIDI_ChannelAftertouchEventData eventData, FrooxEngineContext context)
    {
        WriteChannelAftertouchEventData(in eventData, context);
        ChannelAftertouch.Execute(context);
    }

    public MIDI_ChannelAftertouchEvent()
    {
        Device = new GlobalRef<MIDI_InputDevice>(this, 0);
        Channel = new ValueOutput<int>(this);
        Pressure = new ValueOutput<int>(this);
        NormalizedPressure = new ValueOutput<float>(this);
    }
}
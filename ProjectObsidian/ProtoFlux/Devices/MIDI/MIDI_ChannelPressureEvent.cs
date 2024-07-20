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

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Devices;

[NodeName("MIDI Channel Pressure Event")]
[NodeCategory("Obsidian/Devices/MIDI")]
public class MIDI_ChannelPressureEvent : VoidNode<FrooxEngineContext>
{
    public readonly GlobalRef<MIDI_InputDevice> Device;

    public Call ChannelPressure;

    public readonly ValueOutput<int> Channel;

    public readonly ValueOutput<int> Pressure;

    public readonly ValueOutput<float> NormalizedPressure;

    private ObjectStore<MIDI_InputDevice> _currentDevice;

    private ObjectStore<MIDI_ChannelPressureEventHandler> _channelPressure;

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
            device2.ChannelPressure -= _channelPressure.Read(context);
        }
        if (device != null)
        {
            NodeContextPath path = context.CaptureContextPath();
            context.GetEventDispatcher(out var dispatcher);
            MIDI_ChannelPressureEventHandler value3 = delegate (MIDI_InputDevice dev, MIDI_ChannelPressureEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnChannelPressure(dev, in e, c);
                });
            };
            _currentDevice.Write(device, context);
            _channelPressure.Write(value3, context);
            device.ChannelPressure += value3;
        }
        else
        {
            _currentDevice.Clear(context);
            _channelPressure.Clear(context);
        }
    }

    private void WriteChannelPressureEventData(in MIDI_ChannelPressureEventData eventData, FrooxEngineContext context)
    {
        Channel.Write(eventData.channel, context);
        Pressure.Write(eventData.pressure, context);
        NormalizedPressure.Write(eventData.pressure / 127f, context);
    }

    private void OnChannelPressure(MIDI_InputDevice device, in MIDI_ChannelPressureEventData eventData, FrooxEngineContext context)
    {
        WriteChannelPressureEventData(in eventData, context);
        ChannelPressure.Execute(context);
    }

    public MIDI_ChannelPressureEvent()
    {
        Device = new GlobalRef<MIDI_InputDevice>(this, 0);
        Channel = new ValueOutput<int>(this);
        Pressure = new ValueOutput<int>(this);
        NormalizedPressure = new ValueOutput<float>(this);
    }
}
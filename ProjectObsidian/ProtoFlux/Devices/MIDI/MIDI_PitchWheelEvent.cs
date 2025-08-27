using System;
using System.Linq;
using ProtoFlux.Core;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using Obsidian.Elements;
using Obsidian.Components.Devices.MIDI;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Devices;

[NodeName("MIDI Pitch Wheel Event")]
[NodeCategory("Obsidian/Devices/MIDI")]
public class MIDI_PitchWheelEvent : VoidNode<FrooxEngineContext>
{
    public readonly GlobalRef<MIDI_InputDevice> Device;

    public Call PitchWheel;

    public readonly ValueOutput<int> Channel;

    public readonly ValueOutput<int> Value;

    public readonly ValueOutput<float> NormalizedValue;

    private ObjectStore<MIDI_InputDevice> _currentDevice;

    private ObjectStore<MIDI_PitchWheelEventHandler> _pitchWheel;

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
            device2.PitchWheel -= _pitchWheel.Read(context);
        }
        if (device != null)
        {
            NodeContextPath path = context.CaptureContextPath();
            context.GetEventDispatcher(out var dispatcher);
            MIDI_PitchWheelEventHandler value3 = delegate (IMidiInputListener sender, MIDI_PitchWheelEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnPitch(sender, in e, c);
                });
            };
            _currentDevice.Write(device, context);
            _pitchWheel.Write(value3, context);
            device.PitchWheel += value3;
        }
        else
        {
            _currentDevice.Clear(context);
            _pitchWheel.Clear(context);
        }
    }

    private void WritePitchEventData(in MIDI_PitchWheelEventData eventData, FrooxEngineContext context)
    {
        Channel.Write(eventData.channel, context);
        Value.Write(eventData.value, context);

        // should be 1 at 16383, -1 at 0
        NormalizedValue.Write(eventData.normalizedValue, context);
    }

    private void OnPitch(IMidiInputListener sender, in MIDI_PitchWheelEventData eventData, FrooxEngineContext context)
    {
        WritePitchEventData(in eventData, context);
        PitchWheel.Execute(context);
    }

    public MIDI_PitchWheelEvent()
    {
        Device = new GlobalRef<MIDI_InputDevice>(this, 0);
        Channel = new ValueOutput<int>(this);
        Value = new ValueOutput<int>(this);
        NormalizedValue = new ValueOutput<float>(this);
    }
}
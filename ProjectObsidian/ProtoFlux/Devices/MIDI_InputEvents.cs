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

    public Call ChannelPressure;

    //public readonly ObjectOutput<Component> Source;

    public readonly ValueOutput<int> Channel;

    public readonly ValueOutput<int> Note;

    public readonly ValueOutput<float> Velocity;

    public readonly ValueOutput<float> Pressure;

    private ObjectStore<MIDI_InputDevice> _currentDevice;

    private ObjectStore<MIDI_NoteOnOffEventHandler> _noteOn;

    private ObjectStore<MIDI_NoteOnOffEventHandler> _noteOff;

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
            device2.NoteOn -= _noteOn.Read(context);
            device2.NoteOff -= _noteOff.Read(context);
            device2.ChannelPressure -= _channelPressure.Read(context);
        }
        if (device != null)
        {
            NodeContextPath path = context.CaptureContextPath();
            context.GetEventDispatcher(out var dispatcher);
            MIDI_NoteOnOffEventHandler value = delegate (MIDI_InputDevice dev, MIDI_NoteOnOffEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnNoteOn(dev, in e, c);
                });
            };
            MIDI_NoteOnOffEventHandler value2 = delegate (MIDI_InputDevice dev, MIDI_NoteOnOffEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnNoteOff(dev, in e, c);
                });
            };
            MIDI_ChannelPressureEventHandler value3 = delegate (MIDI_InputDevice dev, MIDI_ChannelPressureEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnChannelPressure(dev, in e, c);
                });
            };
            _currentDevice.Write(device, context);
            _noteOn.Write(value, context);
            _noteOff.Write(value2, context);
            _channelPressure.Write(value3, context);
            device.NoteOn += value;
            device.NoteOff += value2;
            device.ChannelPressure += value3;
        }
        else
        {
            _currentDevice.Clear(context);
            _noteOn.Clear(context);
            _noteOff.Clear(context);
            _channelPressure.Clear(context);
        }
    }

    private void WriteNoteOnOffEventData(in MIDI_NoteOnOffEventData eventData, FrooxEngineContext context)
    {
        Channel.Write(eventData.channel, context);
        Note.Write(eventData.note, context);
        Velocity.Write(eventData.velocity, context);
    }

    private void WriteChannelPressureEventData(in MIDI_ChannelPressureEventData eventData, FrooxEngineContext context)
    {
        Channel.Write(eventData.channel, context);
        Pressure.Write(eventData.pressure, context);
    }

    private void OnNoteOn(MIDI_InputDevice device, in MIDI_NoteOnOffEventData eventData, FrooxEngineContext context)
    {
        WriteNoteOnOffEventData(in eventData, context);
        NoteOn.Execute(context);
    }

    private void OnNoteOff(MIDI_InputDevice device, in MIDI_NoteOnOffEventData eventData, FrooxEngineContext context)
    {
        WriteNoteOnOffEventData(in eventData, context);
        NoteOff.Execute(context);
    }

    private void OnChannelPressure(MIDI_InputDevice device, in MIDI_ChannelPressureEventData eventData, FrooxEngineContext context)
    {
        WriteChannelPressureEventData(in eventData, context);
        ChannelPressure.Execute(context);
    }

    public MIDI_InputEvents()
    {
        Device = new GlobalRef<MIDI_InputDevice>(this, 0);
        Channel = new ValueOutput<int>(this);
        Note = new ValueOutput<int>(this);
        Velocity = new ValueOutput<float>(this);
        Pressure = new ValueOutput<float>(this);
    }
}
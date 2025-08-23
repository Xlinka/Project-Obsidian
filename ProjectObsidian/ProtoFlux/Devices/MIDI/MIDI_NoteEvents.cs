using System;
using System.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using Obsidian.Elements;
using Obsidian.Components.Devices.MIDI;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Devices;

[NodeName("MIDI Note Events")]
[NodeCategory("Obsidian/Devices/MIDI")]
public class MIDI_NoteEvents : VoidNode<FrooxEngineContext>
{
    public readonly GlobalRef<MIDI_InputDevice> Device;

    public Call NoteOn;

    public Call NoteOff;

    public readonly ValueOutput<int> Channel;

    public readonly ValueOutput<int> Note;

    public readonly ValueOutput<int> Velocity;

    public readonly ValueOutput<float> NormalizedVelocity;

    private ObjectStore<MIDI_InputDevice> _currentDevice;

    private ObjectStore<MIDI_NoteEventHandler> _noteOn;

    private ObjectStore<MIDI_NoteEventHandler> _noteOff;

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
            MIDI_NoteEventHandler value = delegate (IMidiInputListener sender, MIDI_NoteEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnNoteOn(sender, in e, c);
                });
            };
            MIDI_NoteEventHandler value2 = delegate (IMidiInputListener sender, MIDI_NoteEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnNoteOff(sender, in e, c);
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

    private void WriteNoteOnOffEventData(in MIDI_NoteEventData eventData, FrooxEngineContext context)
    {
        Channel.Write(eventData.channel, context);
        Note.Write(eventData.note, context);
        Velocity.Write(eventData.velocity, context);
        NormalizedVelocity.Write(eventData.normalizedVelocity, context);
    }

    private void OnNoteOn(IMidiInputListener sender, in MIDI_NoteEventData eventData, FrooxEngineContext context)
    {
        WriteNoteOnOffEventData(in eventData, context);
        NoteOn.Execute(context);
    }

    private void OnNoteOff(IMidiInputListener sender, in MIDI_NoteEventData eventData, FrooxEngineContext context)
    {
        WriteNoteOnOffEventData(in eventData, context);
        NoteOff.Execute(context);
    }

    public MIDI_NoteEvents()
    {
        Device = new GlobalRef<MIDI_InputDevice>(this, 0);
        Channel = new ValueOutput<int>(this);
        Note = new ValueOutput<int>(this);
        Velocity = new ValueOutput<int>(this);
        NormalizedVelocity = new ValueOutput<float>(this);
    }
}
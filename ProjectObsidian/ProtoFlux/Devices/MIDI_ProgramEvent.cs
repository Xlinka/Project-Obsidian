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

[NodeName("MIDI Program Event")]
[NodeCategory("Obsidian/Devices/MIDI")]
public class MIDI_ProgramEvent : VoidNode<FrooxEngineContext>
{
    public readonly GlobalRef<MIDI_InputDevice> Device;

    public Call Program;

    public readonly ValueOutput<int> Channel;

    public readonly ValueOutput<int> ProgramValue;

    private ObjectStore<MIDI_InputDevice> _currentDevice;

    private ObjectStore<MIDI_ProgramEventHandler> _program;

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
            device2.Program -= _program.Read(context);
        }
        if (device != null)
        {
            NodeContextPath path = context.CaptureContextPath();
            context.GetEventDispatcher(out var dispatcher);
            MIDI_ProgramEventHandler value = delegate (MIDI_InputDevice dev, MIDI_ProgramEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnNoteOn(dev, in e, c);
                });
            };
            _currentDevice.Write(device, context);
            _program.Write(value, context);
            device.Program += value;
        }
        else
        {
            _currentDevice.Clear(context);
            _program.Clear(context);
        }
    }

    private void WriteNoteOnOffEventData(in MIDI_ProgramEventData eventData, FrooxEngineContext context)
    {
        Channel.Write(eventData.channel, context);
        ProgramValue.Write(eventData.program, context);
    }

    private void OnNoteOn(MIDI_InputDevice device, in MIDI_ProgramEventData eventData, FrooxEngineContext context)
    {
        WriteNoteOnOffEventData(in eventData, context);
        Program.Execute(context);
    }

    public MIDI_ProgramEvent()
    {
        Device = new GlobalRef<MIDI_InputDevice>(this, 0);
        Channel = new ValueOutput<int>(this);
        ProgramValue = new ValueOutput<int>(this);
    }
}
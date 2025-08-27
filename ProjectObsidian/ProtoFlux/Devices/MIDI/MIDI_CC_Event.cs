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

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Devices;

[NodeName("MIDI CC Event")]
[NodeCategory("Obsidian/Devices/MIDI")]
public class MIDI_CC_Event : VoidNode<FrooxEngineContext>
{
    public readonly GlobalRef<MIDI_InputDevice> Device;

    public Call ControlChange;

    public readonly ValueOutput<int> Channel;

    public readonly ValueOutput<int> Controller;

    public readonly ValueOutput<MIDI_CC_Definition> ControllerDefinition;

    public readonly ValueOutput<int> Value;

    public readonly ValueOutput<float> NormalizedValue;

    private ObjectStore<MIDI_InputDevice> _currentDevice;

    private ObjectStore<MIDI_CC_EventHandler> _controlChange;

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
            device2.Control -= _controlChange.Read(context);
        }
        if (device != null)
        {
            NodeContextPath path = context.CaptureContextPath();
            context.GetEventDispatcher(out var dispatcher);
            MIDI_CC_EventHandler value3 = delegate (IMidiInputListener sender, MIDI_CC_EventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnControl(sender, in e, c);
                });
            };
            _currentDevice.Write(device, context);
            _controlChange.Write(value3, context);
            device.Control += value3;
        }
        else
        {
            _currentDevice.Clear(context);
            _controlChange.Clear(context);
        }
    }

    private void WriteCCEventData(in MIDI_CC_EventData eventData, FrooxEngineContext context)
    {
        Channel.Write(eventData.channel, context);
        Controller.Write(eventData.controller, context);
        if (Enum.IsDefined(typeof(MIDI_CC_Definition), eventData.controller))
        {
            ControllerDefinition.Write((MIDI_CC_Definition)Enum.ToObject(typeof(MIDI_CC_Definition), eventData.controller), context);
        }
        else
        {
            ControllerDefinition.Write(MIDI_CC_Definition.UNDEFINED, context);
        }
        Value.Write(eventData.value, context);
        NormalizedValue.Write(eventData.normalizedValue, context);
    }

    private void OnControl(IMidiInputListener sender, in MIDI_CC_EventData eventData, FrooxEngineContext context)
    {
        WriteCCEventData(in eventData, context);
        ControlChange.Execute(context);
    }

    public MIDI_CC_Event()
    {
        Device = new GlobalRef<MIDI_InputDevice>(this, 0);
        Channel = new ValueOutput<int>(this);
        Controller = new ValueOutput<int>(this);
        ControllerDefinition = new ValueOutput<MIDI_CC_Definition>(this);
        Value = new ValueOutput<int>(this);
        NormalizedValue = new ValueOutput<float>(this);
    }
}
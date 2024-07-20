using Elements.Core;
using FrooxEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Commons.Music.Midi.RtMidi;
using CoreMidi;
using Commons.Music.Midi;
using Obsidian.Elements;

namespace Obsidian;

[Category(new string[] { "Obsidian/Devices" })]
public class MIDI_CC_Value : Component
{
    public readonly SyncRef<MIDI_InputDevice> InputDevice;

    public readonly Sync<bool> AutoMap;

    public readonly Sync<int> Channel;

    public readonly Sync<int> ControllerNumber;

    public readonly Sync<MIDI_CC_Definition?> OverrideDefinition;

    public readonly Sync<int> Value;

    public readonly Sync<float> NormalizedValue;

    private MIDI_InputDevice _device;

    protected override void OnStart()
    {
        base.OnStart();
        InputDevice.OnTargetChange += OnTargetChange;
        if (InputDevice.Target != null)
        {
            _device = InputDevice.Target;
            InputDevice.Target.Control += OnControl;
        }
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        if (_device != null)
        {
            _device.Control -= OnControl;
            _device = null;
        }
    }

    private void OnControl(MIDI_InputDevice device, MIDI_CC_EventData eventData)
    {
        RunSynchronously(() => 
        {
            if (AutoMap.Value)
            {
                AutoMap.Value = false;
                ControllerNumber.Value = eventData.controller;
                Channel.Value = eventData.channel;
                if (OverrideDefinition.Value.HasValue)
                {
                    if (Enum.IsDefined(typeof(MIDI_CC_Definition), eventData.controller))
                    {
                        OverrideDefinition.Value = (MIDI_CC_Definition)Enum.ToObject(typeof(MIDI_CC_Definition), eventData.controller);
                    }
                    else
                    {
                        OverrideDefinition.Value = MIDI_CC_Definition.UNDEFINED;
                    }
                }
            }
            if (Channel.Value == eventData.channel)
            {
                if (OverrideDefinition.Value.HasValue && OverrideDefinition.Value.Value != MIDI_CC_Definition.UNDEFINED)
                {
                    if (eventData.controller == (int)OverrideDefinition.Value.Value)
                    {
                        Value.Value = eventData.value;
                        NormalizedValue.Value = eventData.value / 127f;
                    }
                }
                else
                {
                    if (eventData.controller == ControllerNumber.Value)
                    {
                        Value.Value = eventData.value;
                        NormalizedValue.Value = eventData.value / 127f;
                    }
                }
            }
        });
    }

    private void OnTargetChange(SyncRef<MIDI_InputDevice> syncRef)
    {
        if (syncRef.Target == null && _device != null)
        {
            _device.Control -= OnControl;
            _device = null;
        }
        else if (syncRef.Target != null)
        {
            if (_device != null)
            {
                _device.Control -= OnControl;
            }
            _device = syncRef.Target;
            _device.Control += OnControl;
        }
    }
}
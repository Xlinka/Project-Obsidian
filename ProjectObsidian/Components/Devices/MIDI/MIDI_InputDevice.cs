using Elements.Core;
using FrooxEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Commons.Music.Midi;
using Obsidian.Elements;
using Obsidian;
using System;

namespace Components.Devices.MIDI;

[Category(new string[] { "Obsidian/Devices/MIDI" })]
[OldTypeName("Obsidian.MIDI_InputDevice")]
public class MIDI_InputDevice : Component, IMidiInputListener
{
    [NoContinuousParsing]
    public readonly Sync<string> DeviceName;

    public readonly Sync<bool> IsConnected;

    public readonly UserRef HandlingUser;

    private bool _lastIsConnected;

    private MidiInputConnection Connection;

    private MIDI_Settings _settings => Settings.GetActiveSetting<MIDI_Settings>();

    public event MIDI_NoteEventHandler NoteOn;
    public event MIDI_NoteEventHandler NoteOff;

    // Pressure for whole keyboard
    public event MIDI_ChannelAftertouchEventHandler ChannelAftertouch;

    // Pressure for individual notes (polyphonic)
    public event MIDI_PolyphonicAftertouchEventHandler PolyphonicAftertouch;
    public event MIDI_CC_EventHandler Control;
    public event MIDI_PitchWheelEventHandler PitchWheel;
    public event MIDI_ProgramEventHandler Program;
    public event MIDI_SystemRealtimeEventHandler MidiClock;
    public event MIDI_SystemRealtimeEventHandler MidiTick;
    public event MIDI_SystemRealtimeEventHandler MidiStart;
    public event MIDI_SystemRealtimeEventHandler MidiStop;
    public event MIDI_SystemRealtimeEventHandler MidiContinue;
    public event MIDI_SystemRealtimeEventHandler ActiveSense;
    public event MIDI_SystemRealtimeEventHandler Reset;

    public void TriggerNoteOn(MIDI_NoteEventData eventData) => NoteOn?.Invoke(this, eventData);
    public void TriggerNoteOff(MIDI_NoteEventData eventData) => NoteOff?.Invoke(this, eventData);
    public void TriggerChannelAftertouch(MIDI_ChannelAftertouchEventData eventData) => ChannelAftertouch?.Invoke(this, eventData);
    public void TriggerPolyphonicAftertouch(MIDI_PolyphonicAftertouchEventData eventData) => PolyphonicAftertouch?.Invoke(this, eventData);
    public void TriggerControl(MIDI_CC_EventData eventData) => Control?.Invoke(this, eventData);
    public void TriggerPitchWheel(MIDI_PitchWheelEventData eventData) => PitchWheel?.Invoke(this, eventData);
    public void TriggerProgram(MIDI_ProgramEventData eventData) => Program?.Invoke(this, eventData);
    public void TriggerMidiClock(MIDI_SystemRealtimeEventData eventData) => MidiClock?.Invoke(this, eventData);
    public void TriggerMidiTick(MIDI_SystemRealtimeEventData eventData) => MidiTick?.Invoke(this, eventData);
    public void TriggerMidiStart(MIDI_SystemRealtimeEventData eventData) => MidiStart?.Invoke(this, eventData);
    public void TriggerMidiStop(MIDI_SystemRealtimeEventData eventData) => MidiStop?.Invoke(this, eventData);
    public void TriggerMidiContinue(MIDI_SystemRealtimeEventData eventData) => MidiContinue?.Invoke(this, eventData);
    public void TriggerActiveSense(MIDI_SystemRealtimeEventData eventData) => ActiveSense?.Invoke(this, eventData);
    public void TriggerReset(MIDI_SystemRealtimeEventData eventData) => Reset?.Invoke(this, eventData);

    protected override void OnStart()
    {
        base.OnStart();
        Settings.GetActiveSetting<MIDI_Settings>();
        Settings.RegisterValueChanges<MIDI_Settings>(OnInputDeviceSettingsChanged);
        RunInUpdates(30, Update);
    }

    private void OnInputDeviceSettingsChanged(MIDI_Settings setting)
    {
        UniLog.Log("MIDI Settings Changed!");
        MarkChangeDirty();
    }

    protected override void OnChanges()
    {
        base.OnChanges();
        if (IsConnected.WasChanged)
        {
            IsConnected.Value = _lastIsConnected;
            IsConnected.WasChanged = false;
            return;
        }
        Update();
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        var emptyArray = new Delegate[] { };
        foreach (Delegate d in NoteOn?.GetInvocationList().ToArray() ?? emptyArray)
        {
            NoteOn -= (MIDI_NoteEventHandler)d;
        }
        foreach (Delegate d in NoteOff?.GetInvocationList().ToArray() ?? emptyArray)
        {
            NoteOff -= (MIDI_NoteEventHandler)d;
        }
        foreach (Delegate d in ChannelAftertouch?.GetInvocationList().ToArray() ?? emptyArray)
        {
            ChannelAftertouch -= (MIDI_ChannelAftertouchEventHandler)d;
        }
        foreach (Delegate d in PolyphonicAftertouch?.GetInvocationList().ToArray() ?? emptyArray)
        {
            PolyphonicAftertouch -= (MIDI_PolyphonicAftertouchEventHandler)d;
        }
        foreach (Delegate d in Control?.GetInvocationList().ToArray() ?? emptyArray)
        {
            Control -= (MIDI_CC_EventHandler)d;
        }
        foreach (Delegate d in PitchWheel?.GetInvocationList().ToArray() ?? emptyArray)
        {
            PitchWheel -= (MIDI_PitchWheelEventHandler)d;
        }
        foreach (Delegate d in Program?.GetInvocationList().ToArray() ?? emptyArray)
        {
            Program -= (MIDI_ProgramEventHandler)d;
        }
        foreach (Delegate d in MidiClock?.GetInvocationList().ToArray() ?? emptyArray)
        {
            MidiClock -= (MIDI_SystemRealtimeEventHandler)d;
        }
        foreach (Delegate d in MidiTick?.GetInvocationList().ToArray() ?? emptyArray)
        {
            MidiTick -= (MIDI_SystemRealtimeEventHandler)d;
        }
        foreach (Delegate d in MidiStart?.GetInvocationList().ToArray() ?? emptyArray)
        {
            MidiStart -= (MIDI_SystemRealtimeEventHandler)d;
        }
        foreach (Delegate d in MidiStop?.GetInvocationList().ToArray() ?? emptyArray)
        {
            MidiStop -= (MIDI_SystemRealtimeEventHandler)d;
        }
        foreach (Delegate d in MidiContinue?.GetInvocationList().ToArray() ?? emptyArray)
        {
            MidiContinue -= (MIDI_SystemRealtimeEventHandler)d;
        }
        foreach (Delegate d in ActiveSense?.GetInvocationList().ToArray() ?? emptyArray)
        {
            ActiveSense -= (MIDI_SystemRealtimeEventHandler)d;
        }
        foreach (Delegate d in Reset?.GetInvocationList().ToArray() ?? emptyArray)
        {
            Reset -= (MIDI_SystemRealtimeEventHandler)d;
        }
        MidiDeviceConnectionManager.UnregisterInputListener(this);
    }

    private void SetIsConnected(bool val)
    {
        IsConnected.Value = val;
        _lastIsConnected = val;
    }

    private void Update()
    {
        if (HandlingUser.Target == null)
        {
            MidiDeviceConnectionManager.UnregisterInputListener(this);
            SetIsConnected(false);
            return;
        }

        if (LocalUser != HandlingUser.Target)
        {
            MidiDeviceConnectionManager.UnregisterInputListener(this);
            return;
        }

        if (!Enabled)
        {
            MidiDeviceConnectionManager.UnregisterInputListener(this);
            SetIsConnected(false);
            return;
        }

        if (!string.IsNullOrWhiteSpace(DeviceName))
        {
            UniLog.Log("Device name: " + DeviceName.Value);
            if (!_settings.InputDevices.Any(dev => dev.DeviceName.Value == DeviceName.Value && dev.AllowConnections.Value == true))
            {
                UniLog.Log("Device connection not allowed.");
                MidiDeviceConnectionManager.UnregisterInputListener(this);
                SetIsConnected(false);
                return;
            }

            if (Connection != null
                && (Connection.Input.Connection == MidiPortConnectionState.Open || Connection.Input.Connection == MidiPortConnectionState.Pending))
            {
                if (MidiAccessManager.Default.Inputs.Any(inp => inp.Name == DeviceName.Value))
                {
                    UniLog.Log("Already connected. Connection state: " + Connection.Input.Connection.ToString());
                    return;
                }
                else
                {
                    UniLog.Log("Device was removed after a conection.");
                    MidiDeviceConnectionManager.UnregisterInputListener(this);
                    SetIsConnected(false);
                    return;
                }
            }

            var access = MidiAccessManager.Default;
            var targetDevice = access.Inputs.FirstOrDefault(dev => dev.Name == DeviceName.Value);
            if (targetDevice != null)
            {
                UniLog.Log("Found the target device.");
                Connection = MidiDeviceConnectionManager.RegisterInputListener(this, targetDevice);
                SetIsConnected(true);
                UniLog.Log("Connected.");
            }
            else
            {
                UniLog.Log("Could not find target device.");
                SetIsConnected(false);
            }
        }
        else
        {
            MidiDeviceConnectionManager.UnregisterInputListener(this);
            SetIsConnected(false);
        }
    }
}
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
public class MIDI_InputDevice : Component
{
    [NoContinuousParsing]
    public readonly Sync<string> DeviceName;

    public readonly Sync<bool> IsConnected;

    public readonly UserRef HandlingUser;

    public readonly Sync<string> _lastEvent;

    private bool _lastIsConnected;

    private IMidiInput _inputDevice;

    private MIDI_Settings _settings => Settings.GetActiveSetting<MIDI_Settings>();

    public event MIDI_NoteEventHandler NoteOn;

    public event MIDI_NoteEventHandler NoteOff;

    // Pressure for whole keyboard
    public event MIDI_ChannelPressureEventHandler ChannelPressure;

    // Pressure for individual notes (polyphonic)
    public event MIDI_AftertouchEventHandler Aftertouch;

    public event MIDI_CC_EventHandler Control;

    public event MIDI_PitchWheelEventHandler PitchWheel;

    private const bool DEBUG = false;

    protected override void OnStart()
    {
        base.OnStart();
        Settings.GetActiveSetting<MIDI_Settings>();
        Settings.RegisterValueChanges<MIDI_Settings>(OnInputDeviceSettingsChanged);
        RunInUpdates(7, Update);
    }

    private void OnInputDeviceSettingsChanged(MIDI_Settings setting)
    {
        UniLog.Log("MIDI Settings Changed!");
        MarkChangeDirty();
    }

    protected override void OnChanges()
    {
        //UniLog.Log("OnChanges");
        base.OnChanges();
        if (_lastEvent.WasChanged)
        {
            _lastEvent.WasChanged = false;
            return;
        }
        if (IsConnected.WasChanged)
        {
            IsConnected.Value = _lastIsConnected;
            IsConnected.WasChanged = false;
            return;
        }
        Update();
    }

    private async void ReleaseDeviceAsync()
    {
        UniLog.Log("Releasing device...");
        await _inputDevice.CloseAsync();
        UniLog.Log("Device released.");
        _inputDevice = null;
    }

    protected override void OnPrepareDestroy()
    {
        base.OnPrepareDestroy();
        if (_inputDevice != null)
        {
            ReleaseDeviceAsync();
        }
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
            if (_inputDevice != null)
            {
                ReleaseDeviceAsync();
            }
            SetIsConnected(false);
            return;
        }

        if (LocalUser != HandlingUser.Target)
        {
            if (_inputDevice != null)
            {
                ReleaseDeviceAsync();
            }
            return;
        }

        if (!Enabled)
        {
            if (_inputDevice != null)
            {
                ReleaseDeviceAsync();
            }
            SetIsConnected(false);
            return;
        }

        if (!string.IsNullOrWhiteSpace(DeviceName))
        {
            UniLog.Log("Device name: " + DeviceName.Value);
            if (!_settings.InputDevices.Any(dev => dev.DeviceName.Value == DeviceName.Value && dev.AllowConnections.Value == true))
            {
                UniLog.Log("Device connection not allowed.");
                if (_inputDevice != null)
                {
                    ReleaseDeviceAsync();
                }
                SetIsConnected(false);
                return;
            }

            if (_inputDevice != null 
                && (_inputDevice.Connection == MidiPortConnectionState.Open || _inputDevice.Connection == MidiPortConnectionState.Pending) 
                && _inputDevice.Details.Name == DeviceName.Value) 
            {
                UniLog.Log("Already connected.");
                return;
            }

            var access = MidiAccessManager.Default;
            var targetDevice = access.Inputs.FirstOrDefault(dev => dev.Name == DeviceName.Value);
            if (targetDevice != null)
            {
                UniLog.Log("Found the target device.");
                _inputDevice = access.OpenInputAsync(targetDevice.Id).Result;
                _inputDevice.MessageReceived += OnMessageReceived;
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
            if (_inputDevice != null)
            {
                ReleaseDeviceAsync();
            }
            SetIsConnected(false);
        }
    }

    // What
    private ushort CombineBytes(byte First, byte Second)
    {
        ushort _14bit;
        _14bit = Second;
        _14bit <<= 7;
        _14bit |= First;
        return _14bit;
    }

    private void OnMessageReceived(object sender, MidiReceivedEventArgs args)
    {
        if (DEBUG) UniLog.Log($"Received {args.Length} bytes");
        if (DEBUG) UniLog.Log($"Timestamp: {args.Timestamp}");
        if (DEBUG) UniLog.Log($"Start: {args.Start}");
        var events = MidiEvent.Convert(args.Data, args.Start, args.Length);
        foreach (var e in events)
        {
            UniLog.Log(e.ToString());
            RunSynchronously(() =>
            {
                _lastEvent.Value = e.ToString();
            });
            switch (e.EventType)
            {
                case MidiEvent.NoteOn:
                    NoteOn?.Invoke(this, new MIDI_NoteEventData(e.Channel, e.Msb, e.Lsb));
                    break;
                case MidiEvent.NoteOff:
                    NoteOff?.Invoke(this, new MIDI_NoteEventData(e.Channel, e.Msb, e.Lsb));
                    break;
                case MidiEvent.CAf:
                    ChannelPressure?.Invoke(this, new MIDI_ChannelPressureEventData(e.Channel, e.Msb));
                    break;
                case MidiEvent.CC:
                    Control?.Invoke(this, new MIDI_CC_EventData(e.Channel, e.Msb, e.Lsb));
                    break;
                case MidiEvent.Pitch:
                    PitchWheel?.Invoke(this, new MIDI_PitchWheelEventData(e.Channel, CombineBytes(e.Msb, e.Lsb)));
                    break;
                case MidiEvent.PAf:
                    Aftertouch?.Invoke(this, new MIDI_AftertouchEventData(e.Channel, e.Msb, e.Lsb));
                    break;

                // Unhandled events:

                //SysEx events are probably not worth handling
                case MidiEvent.SysEx1:
                    //if (DEBUG) UniLog.Log("UnhandledEvent: SysEx1");
                    break;
                case MidiEvent.SysEx2:
                    // Same as EndSysEx
                    //if (DEBUG) UniLog.Log("UnhandledEvent: SysEx2");
                    break;

                case MidiEvent.Program:
                    if (DEBUG) UniLog.Log("UnhandledEvent: Program");
                    break;
                case MidiEvent.MtcQuarterFrame:
                    if (DEBUG) UniLog.Log("UnhandledEvent: MtcQuarterFrame");
                    break;
                case MidiEvent.SongPositionPointer:
                    if (DEBUG) UniLog.Log("UnhandledEvent: SongPositionPointer");
                    break;
                case MidiEvent.SongSelect:
                    if (DEBUG) UniLog.Log("UnhandledEvent: SongSelect");
                    break;
                case MidiEvent.TuneRequest:
                    if (DEBUG) UniLog.Log("UnhandledEvent: TuneRequest");
                    break;
                case MidiEvent.MidiClock:
                    if (DEBUG) UniLog.Log("UnhandledEvent: Clock");
                    break;
                case MidiEvent.MidiTick:
                    if (DEBUG) UniLog.Log("UnhandledEvent: MidiTick");
                    break;
                case MidiEvent.MidiStart:
                    if (DEBUG) UniLog.Log("UnhandledEvent: MidiStart");
                    break;
                case MidiEvent.MidiStop:
                    if (DEBUG) UniLog.Log("UnhandledEvent: MidiStart");
                    break;
                case MidiEvent.MidiContinue:
                    if (DEBUG) UniLog.Log("UnhandledEvent: MidiContinue");
                    break;
                case MidiEvent.ActiveSense:
                    if (DEBUG) UniLog.Log("UnhandledEvent: ActiveSense");
                    break;
                case MidiEvent.Reset:
                    // Same as Meta
                    if (DEBUG) UniLog.Log("UnhandledEvent: Reset");
                    break;
                default:
                    break;
            }
        }
    }
}
using Elements.Core;
using FrooxEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Commons.Music.Midi.RtMidi;
using CoreMidi;
using Commons.Music.Midi;
using FrooxEngine.ProtoFlux;

namespace Obsidian;

[DataModelType]
public readonly struct MIDI_NoteOnOffEventData
{
    public readonly int channel;

    public readonly int note;

    public readonly int velocity;

    public MIDI_NoteOnOffEventData(in int _channel, in int _note, in int _velocity)
    {
        channel = _channel;
        note = _note;
        velocity = _velocity;
    }
}

[DataModelType]
public readonly struct MIDI_ChannelPressureEventData
{
    public readonly int channel;

    public readonly int pressure;

    public MIDI_ChannelPressureEventData(in int _channel, in int _pressure)
    {
        channel = _channel;
        pressure = _pressure;
    }
}

[DataModelType]
public delegate void MIDI_NoteOnOffEventHandler(MIDI_InputDevice device, MIDI_NoteOnOffEventData eventData);

[DataModelType]
public delegate void MIDI_ChannelPressureEventHandler(MIDI_InputDevice device, MIDI_ChannelPressureEventData eventData);

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

    public event MIDI_NoteOnOffEventHandler NoteOn;

    public event MIDI_NoteOnOffEventHandler NoteOff;

    // Aftertouch?
    public event MIDI_ChannelPressureEventHandler ChannelPressure;

    protected override void OnStart()
    {
        base.OnStart();
        //_settings = ;
        Settings.RegisterValueChanges<MIDI_Settings>(OnInputDeviceSettingsChanged);
        Update();
    }

    private void OnInputDeviceSettingsChanged(MIDI_Settings setting)
    {
        UniLog.Log("MIDI Settings Changed!");
        MarkChangeDirty();
    }

    protected override void OnChanges()
    {
        UniLog.Log("OnChanges");
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
        UniLog.Log("Releasing device");
        await _inputDevice.CloseAsync();
        UniLog.Log("Setting device to null");
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

            if (!_settings.InputDevices.Any(dev => dev.DeviceName.Value == DeviceName.Value && dev.AllowConnections.Value == true))
            {
                UniLog.Log("Device connection not allowed: " + DeviceName.Value);
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
                UniLog.Log("Already connected: " + DeviceName.Value);
                return;
            }

            var access = MidiAccessManager.Default;
            var targetDevice = access.Inputs.FirstOrDefault(dev => dev.Name == DeviceName.Value);
            if (targetDevice != null)
            {
                UniLog.Log("Found the target device: " + targetDevice.Name);
                _inputDevice = access.OpenInputAsync(targetDevice.Id).Result;
                _inputDevice.MessageReceived += OnMessageReceived;
                SetIsConnected(true);
                UniLog.Log("Connected.");
            }
            else
            {
                UniLog.Log("Could not find target device: " + DeviceName.Value);
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

    private void OnMessageReceived(object sender, MidiReceivedEventArgs args)
    {
        UniLog.Log($"Received {args.Length} bytes");
        UniLog.Log($"Timestamp: {args.Timestamp}");
        UniLog.Log($"Start: {args.Start}");
        var events = MidiEvent.Convert(args.Data, args.Start, args.Length);
        foreach (var e in events)
        {
            switch (e.EventType)
            {
                case MidiEvent.NoteOn:
                    NoteOn?.Invoke(this, new MIDI_NoteOnOffEventData(e.Channel, e.Msb, e.Lsb));
                    break;
                case MidiEvent.NoteOff:
                    NoteOff?.Invoke(this, new MIDI_NoteOnOffEventData(e.Channel, e.Msb, e.Lsb));
                    break;
                case MidiEvent.CAf:
                    ChannelPressure?.Invoke(this, new MIDI_ChannelPressureEventData(e.Channel, e.Msb));
                    break;
                default:
                    break;
            }
            UniLog.Log(e.ToString());
            RunSynchronously(() =>
            {
                _lastEvent.Value = e.ToString();
            });
        }
    }
}
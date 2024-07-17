using Elements.Core;
using FrooxEngine;
using System;
using System.Linq;
using Melanchall.DryWetMidi;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using System.Threading.Tasks;
using System.Collections.Generic;

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

    private Melanchall.DryWetMidi.Multimedia.InputDevice _inputDevice;

    private MIDI_Settings _settings;

    protected override void OnStart()
    {
        base.OnStart();
        _settings = Settings.GetActiveSetting<MIDI_Settings>();
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

    private void ReleaseDevice()
    {
        if (_inputDevice.IsListeningForEvents)
        {
            _inputDevice.StopEventsListening();
        }
        _inputDevice.Dispose();
        _inputDevice = null;
    }

    protected override void OnPrepareDestroy()
    {
        base.OnPrepareDestroy();
        if (_inputDevice != null)
        {
            ReleaseDevice();
        }
    }

    private bool DeviceExists()
    {
        try
        {
            var name = _inputDevice.Name;
            return true;
        }
        catch
        {
            return false;
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
                ReleaseDevice();
            }
            SetIsConnected(false);
            return;
        }

        if (LocalUser != HandlingUser.Target)
        {
            if (_inputDevice != null)
            {
                ReleaseDevice();
            }
            return;
        }

        if (!Enabled) 
        {
            if (_inputDevice != null)
            {
                ReleaseDevice();
            }
            SetIsConnected(false);
            return;
        }

        if (!string.IsNullOrWhiteSpace(DeviceName))
        {
            if (!_settings.InputDevices.Any(dev => dev.DeviceName.Value == DeviceName.Value && dev.AllowConnections.Value == true))
            {
                if (_inputDevice != null)
                {
                    ReleaseDevice();
                }
                SetIsConnected(false);
                return;
            }

            if (_inputDevice != null && _inputDevice.IsListeningForEvents && _inputDevice.Name == DeviceName.Value) return;

            if (_inputDevice != null)
            {
                ReleaseDevice();
            }

            try
            {
                _inputDevice = Melanchall.DryWetMidi.Multimedia.InputDevice.GetByName(DeviceName.Value);
            }
            catch
            {
                SetIsConnected(false);
                return;
            }

            _inputDevice.EventReceived += OnEventReceived;
            _inputDevice.StartEventsListening();
            _inputDevice.ErrorOccurred += (object sender, ErrorOccurredEventArgs args) => 
            {
                UniLog.Error(args.Exception.ToString());
                //_inputDevice.Dispose();
                //_inputDevice = null;
                RunSynchronously(() => 
                {
                    _lastEvent.Value = args.Exception.Message;
                });
            };

            SetIsConnected(true);
        }
        else
        {
            if (_inputDevice != null)
            {
                ReleaseDevice();
            }
            SetIsConnected(false);
        }
    }

    private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
    {
        var midiDevice = (Melanchall.DryWetMidi.Multimedia.MidiDevice)sender;
        string str = $"Event received from '{midiDevice.Name}': {e.Event}";
        UniLog.Log(str);
        RunSynchronously(() => 
        {
            _lastEvent.Value = $"{e.Event}";
        });
    }
}
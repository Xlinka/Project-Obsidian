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
using Obsidian;
using System.Threading;

namespace Components.Devices.MIDI;

[Category(new string[] { "Obsidian/Devices/MIDI" })]
public class MIDI_InputDevice : Component
{
    [NoContinuousParsing]
    public readonly Sync<string> DeviceName;

    public readonly Sync<bool> IsConnected;

    public readonly UserRef HandlingUser;

    public readonly Sync<string> _lastEvent;

    public readonly Sync<string> _lastEventType;

    public readonly Sync<string> _lastSystemRealtimeEvent;

    public readonly Sync<string> _lastSystemRealtimeEventType;

    private bool _lastIsConnected;

    private IMidiInput _inputDevice;

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

    private const bool DEBUG = true;

    protected override void OnStart()
    {
        base.OnStart();
        Settings.GetActiveSetting<MIDI_Settings>();
        Settings.RegisterValueChanges<MIDI_Settings>(OnInputDeviceSettingsChanged);
        //if (!Engine.Current.IsInitialized)
        //{
        //    Engine.Current.RunPostInit(() => RunInUpdates(30, Update));
        //}
        RunInUpdates(30, Update);
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
        if (_lastEventType.WasChanged)
        {
            _lastEventType.WasChanged = false;
            return;
        }
        if (_lastSystemRealtimeEvent.WasChanged)
        {
            _lastSystemRealtimeEvent.WasChanged = false;
            return;
        }
        if (_lastSystemRealtimeEventType.WasChanged)
        {
            _lastSystemRealtimeEventType.WasChanged = false;
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
        await Task.WhenAny(_inputDevice.CloseAsync(), Task.Delay(10000));
        UniLog.Log("Device released.");
        _inputDevice = null;
    }

    protected override void OnDispose()
    {
        base.OnDispose();
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

    private struct TimestampedMidiEvent
    {
        public MidiEvent midiEvent;
        public long timestamp;
        public TimestampedMidiEvent(MidiEvent _midiEvent, long _timestamp)
        {
            midiEvent = _midiEvent;
            timestamp = _timestamp;
        }
    }

    // I am using this like a Queue so it could possibly be turned into a Queue instead...
    private List<TimestampedMidiEvent> _eventBuffer = new();

    private const long BATCH_TIME_SIZE_MILLISECONDS = 3;
    private const long CC_FINE_MESSAGE_GAP_MILLISECONDS = 1;

    private bool IsCCFineMessage()
    {
        long timestamp = _eventBuffer[0].timestamp;
        if (_eventBuffer.Count >= 2 && _eventBuffer[0].midiEvent.EventType == MidiEvent.CC && _eventBuffer[1].midiEvent.EventType == MidiEvent.CC && _eventBuffer[1].timestamp - timestamp <= CC_FINE_MESSAGE_GAP_MILLISECONDS)
        {
            return true;
        }
        return false;
    }

    private void ProcessMessageBatch()
    {
        var batchStartTime = _eventBuffer[0].timestamp;
        if (DEBUG) UniLog.Log("Processing message batch: " + batchStartTime.ToString());

        while (_eventBuffer.Count() > 0 && _eventBuffer[0].timestamp - batchStartTime <= BATCH_TIME_SIZE_MILLISECONDS)
        {

            while (IsCCFineMessage())
            {
                var e1 = _eventBuffer[0].midiEvent;
                var e2 = _eventBuffer[1].midiEvent;
                var finalValue = CombineBytes(e2.Lsb, e1.Lsb);
                if (DEBUG) UniLog.Log($"CC fine. Value: " + finalValue.ToString());
                Control?.Invoke(this, new MIDI_CC_EventData(e1.Channel, e1.Msb, finalValue));
                _eventBuffer.RemoveRange(0, 2);
            }

            if (_eventBuffer.Count() == 0) break;

            var e = _eventBuffer[0].midiEvent;
            switch (e.EventType)
            {
                case MidiEvent.NoteOn:
                    if (DEBUG) UniLog.Log("NoteOn");
                    NoteOn?.Invoke(this, new MIDI_NoteEventData(e.Channel, e.Msb, e.Lsb));
                    _lastEventType.Value = "NoteOn";
                    break;
                case MidiEvent.NoteOff:
                    if (DEBUG) UniLog.Log("NoteOff");
                    NoteOff?.Invoke(this, new MIDI_NoteEventData(e.Channel, e.Msb, e.Lsb));
                    _lastEventType.Value = "NoteOff";
                    break;
                case MidiEvent.CAf:
                    if (DEBUG) UniLog.Log("CAf");
                    ChannelAftertouch?.Invoke(this, new MIDI_ChannelAftertouchEventData(e.Channel, e.Msb));
                    _lastEventType.Value = "CAf";
                    break;
                case MidiEvent.CC:
                    if (DEBUG) UniLog.Log("CC");
                    Control?.Invoke(this, new MIDI_CC_EventData(e.Channel, e.Msb, e.Lsb));
                    _lastEventType.Value = "CC";
                    break;
                case MidiEvent.Pitch:
                    if (DEBUG) UniLog.Log("Pitch");
                    PitchWheel?.Invoke(this, new MIDI_PitchWheelEventData(e.Channel, CombineBytes(e.Msb, e.Lsb)));
                    _lastEventType.Value = "Pitch";
                    break;
                case MidiEvent.PAf:
                    if (DEBUG) UniLog.Log("PAf");
                    PolyphonicAftertouch?.Invoke(this, new MIDI_PolyphonicAftertouchEventData(e.Channel, e.Msb, e.Lsb));
                    _lastEventType.Value = "PAf";
                    break;
                case MidiEvent.Program:
                    if (DEBUG) UniLog.Log("Program");
                    Program?.Invoke(this, new MIDI_ProgramEventData(e.Channel, e.Msb));
                    _lastEventType.Value = "Program";
                    break;

                // Unhandled events:

                //SysEx events are probably not worth handling
                case MidiEvent.SysEx1:
                    if (DEBUG) UniLog.Log("UnhandledEvent: SysEx1");
                    _lastEventType.Value = "SysEx1";
                    break;
                case MidiEvent.SysEx2:
                    // Same as EndSysEx
                    if (DEBUG) UniLog.Log("UnhandledEvent: SysEx2");
                    _lastEventType.Value = "SysEx2";
                    break;
                case MidiEvent.MtcQuarterFrame:
                    if (DEBUG) UniLog.Log("UnhandledEvent: MtcQuarterFrame");
                    _lastEventType.Value = "MtcQuarterFrame";
                    break;
                case MidiEvent.SongPositionPointer:
                    if (DEBUG) UniLog.Log("UnhandledEvent: SongPositionPointer");
                    _lastEventType.Value = "SongPositionPointer";
                    break;
                case MidiEvent.SongSelect:
                    if (DEBUG) UniLog.Log("UnhandledEvent: SongSelect");
                    _lastEventType.Value = "SongSelect";
                    break;
                case MidiEvent.TuneRequest:
                    if (DEBUG) UniLog.Log("UnhandledEvent: TuneRequest");
                    _lastEventType.Value = "TuneRequest";
                    break;
                default:
                    break;
            }
            _eventBuffer.RemoveAt(0);
        }
        UniLog.Log("End event batch: " + batchStartTime.ToString());
        UniLog.Log("Remaining events in event buffer: " + _eventBuffer.Count.ToString());
    }

    private long _lastBatchStartTime = 0;

    private void OnMessageReceived(object sender, MidiReceivedEventArgs args)
    {
        if (DEBUG) UniLog.Log($"* Received {args.Length} bytes");
        if (DEBUG) UniLog.Log($"* Timestamp: {args.Timestamp}");

        var events = MidiEvent.Convert(args.Data, args.Start, args.Length);

        if (args.Length == 1)
        {
            // system realtime message, do not buffer these, execute immediately
            if (DEBUG) UniLog.Log($"* System realtime message");
            foreach (var e in events)
            {
                var str = e.ToString();
                if (DEBUG) UniLog.Log("* " + str);
                RunSynchronously(() =>
                {
                    _lastSystemRealtimeEvent.Value = str;
                });
                switch (e.StatusByte)
                {
                    case MidiEvent.MidiClock:
                        if (DEBUG) UniLog.Log("UnhandledEvent: MidiClock");
                        RunSynchronously(() =>
                        {
                            _lastSystemRealtimeEventType.Value = "MidiClock";
                        });
                        break;
                    case MidiEvent.MidiTick:
                        if (DEBUG) UniLog.Log("UnhandledEvent: MidiTick");
                        RunSynchronously(() =>
                        {
                            _lastSystemRealtimeEventType.Value = "MidiTick";
                        });
                        break;
                    case MidiEvent.MidiStart:
                        if (DEBUG) UniLog.Log("UnhandledEvent: MidiStart");
                        RunSynchronously(() =>
                        {
                            _lastSystemRealtimeEventType.Value = "MidiStart";
                        });
                        break;
                    case MidiEvent.MidiStop:
                        if (DEBUG) UniLog.Log("UnhandledEvent: MidiStop");
                        RunSynchronously(() =>
                        {
                            _lastSystemRealtimeEventType.Value = "MidiStop";
                        });
                        break;
                    case MidiEvent.MidiContinue:
                        if (DEBUG) UniLog.Log("UnhandledEvent: MidiContinue");
                        RunSynchronously(() =>
                        {
                            _lastSystemRealtimeEventType.Value = "MidiContinue";
                        });
                        break;
                    case MidiEvent.ActiveSense:
                        if (DEBUG) UniLog.Log("UnhandledEvent: ActiveSense");
                        RunSynchronously(() =>
                        {
                            _lastSystemRealtimeEventType.Value = "ActiveSense";
                        });
                        break;
                    case MidiEvent.Reset:
                        // Same as Meta
                        if (DEBUG) UniLog.Log("UnhandledEvent: Reset");
                        RunSynchronously(() =>
                        {
                            _lastSystemRealtimeEventType.Value = "Reset";
                        });
                        break;
                }
            }
            return;
        }
        
        // other types of messages: channel message (voice or channel mode), system common message, system exclusive message
        foreach(var e in events)
        {
            var str = e.ToString();
            if (DEBUG) UniLog.Log("* " + str);
            RunSynchronously(() => 
            {
                _lastEvent.Value = str;
            });
            _eventBuffer.Add(new TimestampedMidiEvent(e, args.Timestamp));
        }
        if (events.Count() > 0 && args.Timestamp - _lastBatchStartTime > BATCH_TIME_SIZE_MILLISECONDS)
        {
            _lastBatchStartTime = args.Timestamp;
            if (DEBUG) UniLog.Log("* New message batch created: " + args.Timestamp.ToString());
            RunInUpdates(2, ProcessMessageBatch);
        }
    }
}
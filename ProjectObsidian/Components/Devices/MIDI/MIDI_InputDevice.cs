using Elements.Core;
using FrooxEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Commons.Music.Midi;
using Obsidian.Elements;
using Obsidian;

namespace Components.Devices.MIDI;

[Category(new string[] { "Obsidian/Devices/MIDI" })]
[OldTypeName("Obsidian.MIDI_InputDevice")]
public class MIDI_InputDevice : Component
{
    [NoContinuousParsing]
    public readonly Sync<string> DeviceName;

    public readonly Sync<bool> IsConnected;

    public readonly UserRef HandlingUser;

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

    public event MIDI_SystemRealtimeEventHandler MidiClock;

    public event MIDI_SystemRealtimeEventHandler MidiTick;

    public event MIDI_SystemRealtimeEventHandler MidiStart;

    public event MIDI_SystemRealtimeEventHandler MidiStop;

    public event MIDI_SystemRealtimeEventHandler MidiContinue;

    public event MIDI_SystemRealtimeEventHandler ActiveSense;

    public event MIDI_SystemRealtimeEventHandler Reset;

    private const bool DEBUG = true;

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

    private async Task ReleaseDeviceAsync()
    {
        UniLog.Log("Releasing device...");
        await Task.WhenAny(_inputDevice.CloseAsync(), Task.Delay(5000));
        UniLog.Log("Device released.");
        _inputDevice = null;
        _eventBuffer.Clear();
        _lastBatchStartTime = 0;
    }

    private async void ReleaseDeviceAndConnectAsync(IMidiAccess access, string deviceId)
    {
        if (_inputDevice != null)
        {
            await ReleaseDeviceAsync();
        }
        _inputDevice = access.OpenInputAsync(deviceId).Result;
        _inputDevice.MessageReceived += OnMessageReceived;
        SetIsConnected(true);
        UniLog.Log("Connected.");
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
                UniLog.Log("Already connected. Connection state: " + _inputDevice.Connection.ToString());
                return;
            }

            var access = MidiAccessManager.Default;
            var targetDevice = access.Inputs.FirstOrDefault(dev => dev.Name == DeviceName.Value);
            if (targetDevice != null)
            {
                UniLog.Log("Found the target device.");
                ReleaseDeviceAndConnectAsync(access, targetDevice.Id);
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

    private bool IsCCFineMessage()
    {
        if (_eventBuffer.Count == 0) return false;
        long timestamp = _eventBuffer[0].timestamp;
        if (_eventBuffer.Count >= 2 
            && _eventBuffer[0].midiEvent.EventType == MidiEvent.CC && _eventBuffer[1].midiEvent.EventType == MidiEvent.CC 
            && _eventBuffer[0].midiEvent.Msb == _eventBuffer[1].midiEvent.Msb - 32)
        {
            return true;
        }
        return false;
    }

    private void FlushMessageBuffer()
    {
        if (_eventBuffer.Count == 0) 
        {
            UniLog.Log("Message buffer empty.");
            return;
        }

        var batchStartTime = _eventBuffer[0].timestamp;
        if (DEBUG) UniLog.Log("Flushing message buffer from start time: " + batchStartTime.ToString());

        while (_eventBuffer.Count > 0)
        {

            while (IsCCFineMessage())
            {
                var e1 = _eventBuffer[0].midiEvent;
                if (DEBUG) UniLog.Log(e1.ToString());
                var e2 = _eventBuffer[1].midiEvent;
                if (DEBUG) UniLog.Log(e2.ToString());
                var finalValue = CombineBytes(e2.Lsb, e1.Lsb);
                if (DEBUG) UniLog.Log($"CC fine. Value: " + finalValue.ToString());
                Control?.Invoke(this, new MIDI_CC_EventData(e1.Channel, e1.Msb, finalValue, _coarse: false));
                _eventBuffer.RemoveRange(0, 2);
                _bufferedMessagesToHandle -= 2;
            }

            if (_eventBuffer.Count == 0) break;

            var e = _eventBuffer[0].midiEvent;
            if (DEBUG) UniLog.Log(e.ToString());
            switch (e.EventType)
            {
                case MidiEvent.CC:
                    if (DEBUG) UniLog.Log("CC");
                    Control?.Invoke(this, new MIDI_CC_EventData(e.Channel, e.Msb, e.Lsb, _coarse: true));
                    break;
                // Program events are buffered because they can be sent after a CC fine message for Bank Select, one of my devices sends consecutively: CC (Bank Select) -> CC (Bank Select Lsb) -> Program for some buttons
                case MidiEvent.Program:
                    if (DEBUG) UniLog.Log("Program");
                    Program?.Invoke(this, new MIDI_ProgramEventData(e.Channel, e.Msb));
                    break;

                // Unhandled events:

                //SysEx events are probably not worth handling
                case MidiEvent.SysEx1:
                    if (DEBUG) UniLog.Log("UnhandledEvent: SysEx1");
                    break;
                case MidiEvent.SysEx2:
                    // Same as EndSysEx
                    if (DEBUG) UniLog.Log("UnhandledEvent: SysEx2");
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
                default:
                    break;
            }
            _eventBuffer.RemoveAt(0);
            _bufferedMessagesToHandle -= 1;
        }
        if (DEBUG) UniLog.Log("Finished flushing message buffer from start time: " + batchStartTime.ToString());
        if (_bufferedMessagesToHandle != 0)
        {
            // Just in case some messages got lost somehow
            UniLog.Warning("Did not handle all buffered messages! " + _bufferedMessagesToHandle.ToString());
        }
    }

    private long _lastBatchStartTime = 0;

    private int _bufferedMessagesToHandle = 0;

    private async void OnMessageReceived(object sender, MidiReceivedEventArgs args)
    {
        if (DEBUG) UniLog.Log($"*** New midi message");
        if (DEBUG) UniLog.Log($"* Received {args.Length} bytes");
        if (DEBUG) UniLog.Log($"* Timestamp: {args.Timestamp}");

        var events = MidiEvent.Convert(args.Data, args.Start, args.Length);

        //if (events.Count() == 0) return;

        if (args.Length == 1)
        {
            // system realtime message, do not buffer these, execute immediately
            if (DEBUG) UniLog.Log($"* System realtime message");
            foreach (var e in events)
            {
                var str = e.ToString();
                if (DEBUG) UniLog.Log("* " + str);
                switch (e.StatusByte)
                {
                    case MidiEvent.MidiClock:
                        if (DEBUG) UniLog.Log("* MidiClock");
                        MidiClock?.Invoke(this, new MIDI_SystemRealtimeEventData());
                        break;
                    case MidiEvent.MidiTick:
                        if (DEBUG) UniLog.Log("* MidiTick");
                        MidiTick?.Invoke(this, new MIDI_SystemRealtimeEventData());
                        break;
                    case MidiEvent.MidiStart:
                        if (DEBUG) UniLog.Log("* MidiStart");
                        MidiStart?.Invoke(this, new MIDI_SystemRealtimeEventData());
                        break;
                    case MidiEvent.MidiStop:
                        if (DEBUG) UniLog.Log("* MidiStop");
                        MidiStop?.Invoke(this, new MIDI_SystemRealtimeEventData());
                        break;
                    case MidiEvent.MidiContinue:
                        if (DEBUG) UniLog.Log("* MidiContinue");
                        MidiContinue?.Invoke(this, new MIDI_SystemRealtimeEventData());
                        break;
                    case MidiEvent.ActiveSense:
                        if (DEBUG) UniLog.Log("* ActiveSense");
                        ActiveSense?.Invoke(this, new MIDI_SystemRealtimeEventData());
                        break;
                    case MidiEvent.Reset:
                        // Same as Meta
                        if (DEBUG) UniLog.Log("* Reset");
                        Reset?.Invoke(this, new MIDI_SystemRealtimeEventData());
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

            switch (e.EventType)
            {
                case MidiEvent.NoteOn:
                    if (DEBUG) UniLog.Log("* NoteOn");
                    if (e.Lsb == 0)
                    {
                        if (DEBUG) UniLog.Log("* Zero velocity, so it's actually a NoteOff");
                        NoteOff?.Invoke(this, new MIDI_NoteEventData(e.Channel, e.Msb, e.Lsb));
                        return;
                    }
                    NoteOn?.Invoke(this, new MIDI_NoteEventData(e.Channel, e.Msb, e.Lsb));
                    return;
                case MidiEvent.NoteOff:
                    if (DEBUG) UniLog.Log("* NoteOff");
                    NoteOff?.Invoke(this, new MIDI_NoteEventData(e.Channel, e.Msb, e.Lsb));
                    return;
                case MidiEvent.CAf:
                    if (DEBUG) UniLog.Log("* CAf");
                    ChannelAftertouch?.Invoke(this, new MIDI_ChannelAftertouchEventData(e.Channel, e.Msb));
                    return;
                case MidiEvent.Pitch:
                    if (DEBUG) UniLog.Log("* Pitch");
                    PitchWheel?.Invoke(this, new MIDI_PitchWheelEventData(e.Channel, CombineBytes(e.Msb, e.Lsb)));
                    return;
                case MidiEvent.PAf:
                    if (DEBUG) UniLog.Log("* PAf");
                    PolyphonicAftertouch?.Invoke(this, new MIDI_PolyphonicAftertouchEventData(e.Channel, e.Msb, e.Lsb));
                    return;
                default:
                    break;
            }
            
            // buffer CC messages because consecutive ones may need to be combined
            // also buffer Program messages
            _eventBuffer.Add(new TimestampedMidiEvent(e, args.Timestamp));
            _bufferedMessagesToHandle += 1;
        }

        if (events.Count() > 0 && args.Timestamp - _lastBatchStartTime > BATCH_TIME_SIZE_MILLISECONDS)
        {
            _lastBatchStartTime = args.Timestamp;
            if (DEBUG) UniLog.Log("* New message batch created: " + args.Timestamp.ToString());
            await Task.Delay((int)BATCH_TIME_SIZE_MILLISECONDS);
            FlushMessageBuffer();
        }
    }
}
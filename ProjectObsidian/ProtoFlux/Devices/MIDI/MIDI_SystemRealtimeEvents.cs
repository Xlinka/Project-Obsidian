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

[NodeName("MIDI System Realtime Events")]
[NodeCategory("Obsidian/Devices/MIDI")]
public class MIDI_SystemRealtimeEvents : VoidNode<FrooxEngineContext>
{
    public readonly GlobalRef<MIDI_InputDevice> Device;

    public Call Clock;

    public Call Tick;

    public Call Start;

    public Call Stop;

    public Call Continue;

    public Call ActiveSense;

    public Call Reset;

    private ObjectStore<MIDI_InputDevice> _currentDevice;

    private ObjectStore<MIDI_SystemRealtimeEventHandler> _clock;

    private ObjectStore<MIDI_SystemRealtimeEventHandler> _tick;

    private ObjectStore<MIDI_SystemRealtimeEventHandler> _start;

    private ObjectStore<MIDI_SystemRealtimeEventHandler> _stop;

    private ObjectStore<MIDI_SystemRealtimeEventHandler> _continue;

    private ObjectStore<MIDI_SystemRealtimeEventHandler> _activeSense;

    private ObjectStore<MIDI_SystemRealtimeEventHandler> _reset;

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
            device2.MidiClock -= _clock.Read(context);
            device2.MidiTick -= _tick.Read(context);
            device2.MidiStart -= _start.Read(context);
            device2.MidiStop -= _stop.Read(context);
            device2.MidiContinue -= _continue.Read(context);
            device2.ActiveSense -= _activeSense.Read(context);
            device2.Reset -= _reset.Read(context);
        }
        if (device != null)
        {
            NodeContextPath path = context.CaptureContextPath();
            context.GetEventDispatcher(out var dispatcher);
            MIDI_SystemRealtimeEventHandler value = delegate (IMidiInputListener sender, MIDI_SystemRealtimeEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnClock(sender, in e, c);
                });
            };
            MIDI_SystemRealtimeEventHandler value2 = delegate (IMidiInputListener sender, MIDI_SystemRealtimeEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnTick(sender, in e, c);
                });
            };
            MIDI_SystemRealtimeEventHandler value3 = delegate (IMidiInputListener sender, MIDI_SystemRealtimeEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnStart(sender, in e, c);
                });
            };
            MIDI_SystemRealtimeEventHandler value4 = delegate (IMidiInputListener sender, MIDI_SystemRealtimeEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnStop(sender, in e, c);
                });
            };
            MIDI_SystemRealtimeEventHandler value5 = delegate (IMidiInputListener sender, MIDI_SystemRealtimeEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnContinue(sender, in e, c);
                });
            };
            MIDI_SystemRealtimeEventHandler value6 = delegate (IMidiInputListener sender, MIDI_SystemRealtimeEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnActiveSense(sender, in e, c);
                });
            };
            MIDI_SystemRealtimeEventHandler value7 = delegate (IMidiInputListener sender, MIDI_SystemRealtimeEventData e)
            {
                dispatcher.ScheduleEvent(path, delegate (FrooxEngineContext c)
                {
                    OnReset(sender, in e, c);
                });
            };
            _currentDevice.Write(device, context);
            _clock.Write(value, context);
            _tick.Write(value2, context);
            _start.Write(value3, context);
            _stop.Write(value4, context);
            _continue.Write(value5, context);
            _activeSense.Write(value6, context);
            _reset.Write(value7, context);
            device.MidiClock += value;
            device.MidiTick += value2;
            device.MidiStart += value3;
            device.MidiStop += value4;
            device.MidiContinue += value5;
            device.ActiveSense += value6;
            device.Reset += value7;
        }
        else
        {
            _currentDevice.Clear(context);
            _clock.Clear(context);
            _tick.Clear(context);
            _start.Clear(context);
            _stop.Clear(context);
            _continue.Clear(context);
            _activeSense.Clear(context);
            _reset.Clear(context);
        }
    }

    private void WriteSystemRealtimeEventData(in MIDI_SystemRealtimeEventData eventData, FrooxEngineContext context)
    {
    }

    private void OnClock(IMidiInputListener sender, in MIDI_SystemRealtimeEventData eventData, FrooxEngineContext context)
    {
        WriteSystemRealtimeEventData(eventData, context);
        Clock.Execute(context);
    }

    private void OnTick(IMidiInputListener sender, in MIDI_SystemRealtimeEventData eventData, FrooxEngineContext context)
    {
        WriteSystemRealtimeEventData(eventData, context);
        Tick.Execute(context);
    }

    private void OnStart(IMidiInputListener sender, in MIDI_SystemRealtimeEventData eventData, FrooxEngineContext context)
    {
        WriteSystemRealtimeEventData(eventData, context);
        Start.Execute(context);
    }

    private void OnStop(IMidiInputListener sender, in MIDI_SystemRealtimeEventData eventData, FrooxEngineContext context)
    {
        WriteSystemRealtimeEventData(eventData, context);
        Stop.Execute(context);
    }

    private void OnContinue(IMidiInputListener sender, in MIDI_SystemRealtimeEventData eventData, FrooxEngineContext context)
    {
        WriteSystemRealtimeEventData(eventData, context);
        Continue.Execute(context);
    }

    private void OnActiveSense(IMidiInputListener sender, in MIDI_SystemRealtimeEventData eventData, FrooxEngineContext context)
    {
        WriteSystemRealtimeEventData(eventData, context);
        ActiveSense.Execute(context);
    }

    private void OnReset(IMidiInputListener sender, in MIDI_SystemRealtimeEventData eventData, FrooxEngineContext context)
    {
        WriteSystemRealtimeEventData(eventData, context);
        Reset.Execute(context);
    }

    public MIDI_SystemRealtimeEvents()
    {
        Device = new GlobalRef<MIDI_InputDevice>(this, 0);
    }
}
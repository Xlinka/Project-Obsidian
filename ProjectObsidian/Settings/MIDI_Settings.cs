using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrooxEngine;
using Melanchall.DryWetMidi.Multimedia;
using Elements.Core;
using Elements.Assets;

namespace Obsidian;

[SettingCategory("Obsidian")]
public class MIDI_Settings : SettingComponent<MIDI_Settings>
{
    public override bool UserspaceOnly => true;

    public class MIDI_Device : SyncObject
    {
        [SettingIndicatorProperty(null, null, null, null, false, 0L)]
        public readonly Sync<string> DeviceName;

        [SettingProperty(null, null, null, false, 0L, null, null)]
        public readonly Sync<bool> AllowConnections;

        public MidiDevice Device { get; internal set; }

        protected override void OnAwake()
        {
            base.OnAwake();
        }
    }

    [NonPersistent]
    [SettingSubcategoryList("DeviceToItem", null, null, null, null, null)]
    public readonly SyncList<MIDI_Device> InputDevices;

    [NonPersistent]
    [SettingSubcategoryList("DeviceToItem", null, null, null, null, null)]
    public readonly SyncList<MIDI_Device> OutputDevices;

    private LocaleData _localeData;

    private DataFeedItem DeviceToItem(ISyncMember item)
    {
        MIDI_Device device = (MIDI_Device)item;
        DataFeedGroup dataFeedGroup = new DataFeedGroup();
        List<DataFeedItem> list = new List<DataFeedItem>();
        string subcategory = device.Device is OutputDevice ? "OutputDevices" : "InputDevices";
        string getter = device.Device is OutputDevice ? "GetOutputDeviceForSubsetting" : "GetInputDeviceForSubsetting";
        foreach (DataFeedItem item2 in SettingsDataFeed.EnumerateSettingProperties(typeof(MIDI_Device), null, typeof(MIDI_Settings), subcategory, getter, device.Device.Name))
        {
            list.Add(item2);
        }
        dataFeedGroup.InitBase(device.Device.Name, null, null, device.Device.Name, null, null, null, list);
        return dataFeedGroup;
    }

    [SyncMethod(typeof(SubsettingGetter), new string[] { })]
    public SyncObject GetInputDeviceForSubsetting(string key)
    {
        return InputDevices.FirstOrDefault((d) => d.Device.Name == key);
    }

    [SyncMethod(typeof(SubsettingGetter), new string[] { })]
    public SyncObject GetOutputDeviceForSubsetting(string key)
    {
        return OutputDevices.FirstOrDefault((d) => d.Device.Name == key);
    }

    protected override void OnStart()
    {
        base.OnStart();
        _localeData = new LocaleData();
        _localeData.LocaleCode = "en";
        _localeData.Authors = new List<string>() { "Nytra" };
        _localeData.Messages = new Dictionary<string, string>();
        _localeData.Messages.Add("Settings.Category.Obsidian", "Obsidian");
        _localeData.Messages.Add("Settings.MIDI_Settings", "MIDI");
        _localeData.Messages.Add("Settings.MIDI_Settings.RefreshDeviceLists", "Refresh Device Lists");
        _localeData.Messages.Add("Settings.MIDI_Settings.InputDevices", "Input Devices");
        _localeData.Messages.Add("Settings.MIDI_Settings.OutputDevices", "Output Devices");
        _localeData.Messages.Add("Settings.MIDI_Settings.InputDevices.DeviceName", "Input Device Name");
        _localeData.Messages.Add("Settings.MIDI_Settings.OutputDevices.DeviceName", "Output Device Name");
        _localeData.Messages.Add("Settings.MIDI_Settings.OutputDevices.Breadcrumb", "Output Devices");
        _localeData.Messages.Add("Settings.MIDI_Settings.InputDevices.Breadcrumb", "Input Devices");
        _localeData.Messages.Add("Settings.MIDI_Settings.InputDevices.AllowConnections", "Allow Connections");
        _localeData.Messages.Add("Settings.MIDI_Settings.OutputDevices.AllowConnections", "Allow Connections");

        // Sometimes the locale is null in here, so wait a bit I guess
        RunInUpdates(7, () =>
        {
            UpdateLocale();
        });

        Settings.RegisterValueChanges<LocaleSettings>(UpdateLocale);

        RefreshDeviceLists();
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        Settings.UnregisterValueChanges<LocaleSettings>(UpdateLocale);
    }

    private void UpdateLocale(LocaleSettings settings = null)
    {
        this.GetCoreLocale()?.Asset?.Data.LoadDataAdditively(_localeData);
    }

    [SettingProperty(null, null, null, false, 0L, null, null)]
    [SyncMethod(typeof(Action), new string[] { })]
    public void RefreshDeviceLists()
    {
        InputDevices.Clear();
        OutputDevices.Clear();
        foreach (var device in Melanchall.DryWetMidi.Multimedia.InputDevice.GetAll())
        {
            RegisterInputDevice(device);
        }
        foreach (var device in OutputDevice.GetAll())
        {
            RegisterOutputDevice(device);
        }
    }

    private void RegisterInputDevice(Melanchall.DryWetMidi.Multimedia.InputDevice inputDevice)
    {
        if (string.IsNullOrEmpty(inputDevice.Name))
        {
            return;
        }
        MIDI_Device device = InputDevices.FirstOrDefault((d) => d.DeviceName.Value == inputDevice.Name);
        if (device == null)
        {
            device = InputDevices.Add();
            device.Device = inputDevice;
            device.DeviceName.Value = inputDevice.Name;
        }
        else
        {
            device.Device = inputDevice;
        }
    }

    private void RegisterOutputDevice(OutputDevice outputDevice)
    {
        if (string.IsNullOrEmpty(outputDevice.Name))
        {
            return;
        }
        MIDI_Device device = OutputDevices.FirstOrDefault((d) => d.DeviceName.Value == outputDevice.Name);
        if (device == null)
        {
            device = OutputDevices.Add();
            device.Device = outputDevice;
            device.DeviceName.Value = outputDevice.Name;
        }
        else
        {
            device.Device = outputDevice;
        }
    }
}
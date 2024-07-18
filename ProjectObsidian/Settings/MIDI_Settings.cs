using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrooxEngine;
//using Melanchall.DryWetMidi.Multimedia;
using Elements.Core;
using Elements.Assets;
using Commons.Music.Midi;

namespace Obsidian;

[SettingCategory("Obsidian")]
public class MIDI_Settings : SettingComponent<MIDI_Settings>
{
    public override bool UserspaceOnly => true;

    public class MIDI_Device : SyncObject
    {
        [SettingIndicatorProperty(null, null, null, null, false, 0L)]
        public readonly Sync<string> DeviceName;

        [NonPersistent]
        [SettingIndicatorProperty(null, null, null, null, false, 0L)]
        public readonly Sync<bool> DeviceFound;

        [SettingProperty(null, null, null, false, 0L, null, null)]
        public readonly Sync<bool> AllowConnections;

        public IMidiPortDetails Details { get; internal set; }

        public bool IsOutput => this.Parent.Name == "OutputDevices";

        protected override void OnAwake()
        {
            base.OnAwake();
        }

        [SettingProperty(null, null, null, false, 0L, null, null)]
        [SyncMethod(typeof(Action), new string[] { })]
        public void Remove()
        {
            this.FindNearestParent<SyncList<MIDI_Device>>().Remove(this);
        }
    }

    //[NonPersistent]
    [SettingSubcategoryList("DeviceToItem", null, null, null, null, null)]
    public readonly SyncList<MIDI_Device> InputDevices;

    //[NonPersistent]
    [SettingSubcategoryList("DeviceToItem", null, null, null, null, null)]
    public readonly SyncList<MIDI_Device> OutputDevices;

    private LocaleData _localeData;

    //private DataFeedItem InputDeviceToItem(ISyncMember item)
    //{
    //    MIDI_Device device = (MIDI_Device)item;
    //    DataFeedGroup dataFeedGroup = new DataFeedGroup();
    //    List<DataFeedItem> list = new List<DataFeedItem>();
    //    foreach (DataFeedItem item2 in SettingsDataFeed.EnumerateSettingProperties(typeof(MIDI_Device), null, typeof(MIDI_Settings), "InputDevices", "GetInputDeviceForSubsetting", device.DeviceName.Value))
    //    {
    //        //UniLog.Log("ItemKey: " + item2.ItemKey);
    //        //var idx = item2.ItemKey.IndexOf(".InputDevices");
    //        //var changedKey = item2.ItemKey.Substring(0, idx);
    //        //UniLog.Log("ChangedKey: " + changedKey);
    //        //item2.InitBase(changedKey, null, null, changedKey);
    //        //item2.InitBase(item2.ItemKey + "." + device.DeviceName.Value, null, null, item2.ItemKey);
    //        var parts = item2.ItemKey.Split('.');
    //        var newLocaleKey = "Settings." + string.Join(".", parts.Take(2));
    //        item2.InitBase(item2.ItemKey, null, null, newLocaleKey.AsLocaleKey());
    //        list.Add(item2);
    //    }
    //    dataFeedGroup.InitBase(device.DeviceName.Value, null, null, device.DeviceName.Value, null, null, null, list);
    //    return dataFeedGroup;
    //}

    private DataFeedItem DeviceToItem(ISyncMember item)
    {
        MIDI_Device device = (MIDI_Device)item;
        DataFeedGroup dataFeedGroup = new DataFeedGroup();
        List<DataFeedItem> list = new List<DataFeedItem>();
        var subcat = device.IsOutput ? "OutputDevices" : "InputDevices";
        var getter = device.IsOutput ? "GetOutputDeviceForSubsetting" : "GetInputDeviceForSubsetting";
        foreach (DataFeedItem item2 in SettingsDataFeed.EnumerateSettingProperties(typeof(MIDI_Device), null, typeof(MIDI_Settings), subcat, getter, device.DeviceName.Value))
        {
            var parts = item2.ItemKey.Split('.');
            var newLocaleKey = "Settings." + string.Join(".", parts.Take(2));
            item2.InitBase(item2.ItemKey, null, null, newLocaleKey.AsLocaleKey());
            list.Add(item2);
        }
        dataFeedGroup.InitBase(device.DeviceName.Value, null, null, device.DeviceName.Value, null, null, null, list);
        return dataFeedGroup;
    }

    [SyncMethod(typeof(SubsettingGetter), new string[] { })]
    public SyncObject GetInputDeviceForSubsetting(string key)
    {
        return InputDevices.FirstOrDefault((d) => d.DeviceName.Value == key);
    }

    [SyncMethod(typeof(SubsettingGetter), new string[] { })]
    public SyncObject GetOutputDeviceForSubsetting(string key)
    {
        return OutputDevices.FirstOrDefault((d) => d.DeviceName.Value == key);
    }

    protected override void OnStart()
    {
        base.OnStart();
        _localeData = new LocaleData();
        _localeData.LocaleCode = "en";
        _localeData.Authors = new List<string>() { "Nytra" };
        _localeData.Messages = new Dictionary<string, string>();
        _localeData.Messages.Add("Settings.Category.Obsidian", "Obsidian");
        _localeData.Messages.Add("Settings.MIDI_Settings", "MIDI Settings");
        _localeData.Messages.Add("Settings.MIDI_Settings.RefreshDeviceLists", "Refresh Devices");
        _localeData.Messages.Add("Settings.MIDI_Settings.InputDevices", "Input Devices");
        _localeData.Messages.Add("Settings.MIDI_Settings.OutputDevices", "Output Devices");

        _localeData.Messages.Add("Settings.MIDI_Settings.DeviceName", "Device Name");
        _localeData.Messages.Add("Settings.MIDI_Settings.OutputDevices.Breadcrumb", "Output Devices");
        _localeData.Messages.Add("Settings.MIDI_Settings.InputDevices.Breadcrumb", "Input Devices");
        _localeData.Messages.Add("Settings.MIDI_Settings.AllowConnections", "Allow Connections");
        _localeData.Messages.Add("Settings.MIDI_Settings.DeviceFound", "Device Found");
        _localeData.Messages.Add("Settings.MIDI_Settings.Remove", "Remove");

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
        //InputDevices.Clear();
        //OutputDevices.Clear();
        foreach(var device in InputDevices.Concat(OutputDevices)) 
        {
            device.DeviceFound.Value = false;
        }
        var access = MidiAccessManager.Default;
        foreach (var input in access.Inputs)
        {
            RegisterInputDevice(input);
        }
        foreach (var output in access.Outputs)
        {
            RegisterOutputDevice(output);
        }
        //foreach (var device in Melanchall.DryWetMidi.Multimedia.InputDevice.GetAll())
        //{
        //    RegisterInputDevice(device);
        //}
        //foreach (var device in OutputDevice.GetAll())
        //{
        //    RegisterOutputDevice(device);
        //}
    }

    private void RegisterInputDevice(IMidiPortDetails details)
    {
        if (string.IsNullOrEmpty(details.Name))
        {
            return;
        }
        MIDI_Device device = InputDevices.FirstOrDefault((d) => d.DeviceName.Value == details.Name);
        if (device == null)
        {
            device = InputDevices.Add();
            //device.Details = details;
            device.DeviceName.Value = details.Name;
        }
        else
        {
            //device.Device = inputDevice;
        }
        device.Details = details;
        device.DeviceFound.Value = true;
        //device.IsOutput = false;
    }

    private void RegisterOutputDevice(IMidiPortDetails details)
    {
        if (string.IsNullOrEmpty(details.Name))
        {
            return;
        }
        MIDI_Device device = OutputDevices.FirstOrDefault((d) => d.DeviceName.Value == details.Name);
        if (device == null)
        {
            device = OutputDevices.Add();
            device.DeviceName.Value = details.Name;
        }
        device.Details = details;
        device.DeviceFound.Value = true;
        //device.IsOutput = true;
    }
}
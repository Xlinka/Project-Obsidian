using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elements.Core;
using Valve.VR;

namespace Obsidian;

public static class OpenVR_Helper
{
    private static bool _initialized = false;
    private static bool _unableToConnect = false;
    public static CVRSystem VRSystem {get; private set;}
    public static bool TryInitialize()
    {
        if (_initialized) return true;
        if (_unableToConnect) return false;
        EVRInitError error = EVRInitError.None;
        VRSystem = OpenVR.Init(ref error, eApplicationType: EVRApplicationType.VRApplication_Background);
        if (error == EVRInitError.None)
        {
            UniLog.Log("Got SteamVR connection");
            _initialized = true;
            return true;
        }
        else
        {
            // EVRInitError.Init_NoServerForBackgroundApp means SteamVR is not running
            UniLog.Error($"Error when initializing OpenVR: {error.ToString()}");
            _unableToConnect = true;
            return false;
        }
    }
}
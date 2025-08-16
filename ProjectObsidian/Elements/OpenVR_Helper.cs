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
    private static bool _steamVrNotRunning = false;
    public static CVRSystem VRSystem {get; private set;}
    public static bool TryInitialize()
    {
        if (_initialized) return true;
        if (_steamVrNotRunning) return false;
        EVRInitError error = EVRInitError.None;
        VRSystem = OpenVR.Init(ref error, eApplicationType: EVRApplicationType.VRApplication_Background);
        if (error == EVRInitError.Init_NoServerForBackgroundApp)
        {
            UniLog.Log("SteamVR is not running");
            _steamVrNotRunning = true;
            return false;
        }
        else
        {
            UniLog.Log("Got SteamVR connection");
            _initialized = true;
            return true;
        }
    }
}
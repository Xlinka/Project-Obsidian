using System;
using System.Runtime.InteropServices;
using Elements.Core;
using Valve.VR;

namespace FrooxEngine;

public enum ImuErrorCode
{
    None = 0,
    AlreadyOpened = 1,
    AlreadyClosed = 2,
    UnknownException = 3,
    PathIsNullOrEmpty = 4,
    IOBuffer_OperationFailed = 100,
    IOBuffer_InvalidHandle = 101,
    IOBuffer_InvalidArgument = 102,
    IOBuffer_PathExists = 103,
    IOBuffer_PathDoesNotExist = 104,
    IOBuffer_Permission = 105
}

[Category("Obsidian/Devices")]
public class ImuInfo : Component
{
    public readonly Sync<bool> Simulate;
    public readonly SyncRef<User> SimulatingUser;
    public readonly Sync<string> Path;
    public readonly Sync<bool> Connected;
    public readonly Sync<double> SampleTime;
    public readonly Sync<double3> VAccel;
    public readonly Sync<double3> VGyro;
    public readonly Sync<Imu_OffScaleFlags> OffScaleFlags;
    private ulong _buffer;
    private string _previousPath;
    private bool _previousSimulating;

    ~ImuInfo()
    {
        CloseBuffer();
    }

    private void CloseBuffer()
    {
        if (_buffer == 0) return;
        OpenVR.IOBuffer.Close(_buffer);
        _previousPath = null;
        _buffer = 0;
        Connected.Value = false;
    }
    protected override void OnCommonUpdate()
    {
        base.OnCommonUpdate();

        if (Simulate.Value && LocalUser == SimulatingUser.Target)
        {
            if (!LocalUser.VR_Active) CloseBuffer(); //we aren't in VR, we probably shouldn't be sampling
            else if (_buffer == 0 || _previousPath != Path.Value)
            {
                CloseBuffer();
                unsafe
                {
                    ulong buffer = default;
                    //TODO: make this not run every frame if it fails
                    var errorCode = OpenVR.IOBuffer.Open(Path.Value, EIOBufferMode.Read, (uint)sizeof(ImuSample_t), 0u, ref buffer);
                    UniLog.Log($"{buffer}, {errorCode.ToString()}");
                    if (errorCode == EIOBufferError.IOBuffer_Success)
                    {
                        _buffer = buffer;
                        _previousPath = Path.Value;
                    }
                }
            }
            if (_buffer != 0)
            {
                unsafe
                {
                    var punRead = 0u;
                    var imuSample_t = default(ImuSample_t);

                    var error = OpenVR.IOBuffer.Read(_buffer, (IntPtr)(&imuSample_t), (uint)sizeof(ImuSample_t), ref punRead);
                    
                    if (error == EIOBufferError.IOBuffer_Success && punRead == sizeof(ImuSample_t))
                    {
                        var fSampleTime = imuSample_t.fSampleTime;
                        var vAccel = new double3(imuSample_t.vAccel.v0, imuSample_t.vAccel.v1, imuSample_t.vAccel.v2);
                        var vGyro = new double3(imuSample_t.vGyro.v0, imuSample_t.vGyro.v1, imuSample_t.vGyro.v2);
                        var flags = (Imu_OffScaleFlags)imuSample_t.unOffScaleFlags;

                        SampleTime.Value = fSampleTime;
                        VAccel.Value = vAccel;
                        VGyro.Value = vGyro;
                        OffScaleFlags.Value = flags;
                    }
                }
            }
        }
        else
        {
            CloseBuffer();
        }

        if (LocalUser != SimulatingUser.Target) return;
        
        var connectedValue = _buffer != 0;
        if (connectedValue != Connected.Value) Connected.Value = connectedValue;
    }
}
/*
using System;
using System.Threading;
using System.Threading.Tasks;
using Elements.Core;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Valve.VR;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian;

public enum ImuErrorCode : int
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
    
[NodeCategory("Obsidian/Devices/OpenVR")]
public class ImuReader : AsyncActionNode<FrooxEngineContext>
{
    public ObjectInput<string> DevicePath;
    [PossibleContinuations("OnClosed", "OnFail")]

    //public readonly Operation Close;

    public AsyncCall OnOpened;

    public Continuation OnClosed;

    public Continuation OnFail;

    public AsyncCall OnData;

    public readonly ValueOutput<bool> IsOpened;

    public readonly ValueOutput<ImuErrorCode> FailReason;

    [ContinuouslyChanging]
    public readonly ValueOutput<double> FSampleTime;

    [ContinuouslyChanging]
    public readonly ValueOutput<double3> VAccel;

    [ContinuouslyChanging]
    public readonly ValueOutput<double3> VGyro;

    [ContinuouslyChanging]
    public readonly ValueOutput<Imu_OffScaleFlags> UnOffScaleFlags;

    private ulong _pulBuffer;

    private Thread _thread;

    protected override async Task<IOperation> RunAsync(FrooxEngineContext context)
    {
        var path = DevicePath.Evaluate(context);
        if (string.IsNullOrEmpty(path)) return Fail(ImuErrorCode.PathIsNullOrEmpty, context);
            
        var errorCode = OpenVR.IOBuffer.Open(path, EIOBufferMode.Read, sizeof(int), 0u, ref _pulBuffer);
            
        if (errorCode != EIOBufferError.IOBuffer_Success) return Fail((ImuErrorCode)errorCode, context);
            
        await OnOpened.ExecuteAsync(context);
            
        _thread = new Thread((ThreadStart)delegate
        {
            ReadLoopAsync(context);
        });
        _thread.Start();
        IsOpened.Write(value: true, context);
            
        return OnOpened.Target;
    }

    public IOperation DoClose(FrooxEngineContext context)
    {
        if (_pulBuffer == 0) return Fail(ImuErrorCode.AlreadyClosed, context);
            
        var eIOBufferError = OpenVR.IOBuffer.Close(_pulBuffer);
        if (eIOBufferError == EIOBufferError.IOBuffer_Success)
        {
            IsOpened.Write(value: false, context);
            _pulBuffer = 0uL;
            if (_thread != null)
            {
                _thread.Abort();
                _thread = null;
            }
            return OnClosed.Target;
        }
            
        FailReason.Write((ImuErrorCode)eIOBufferError, context);
        return OnClosed.Target;
    }

    ~ImuReader()
    {
        if (_thread != null)
        {
            _thread.Abort();
            _thread = null;
        }
        if (_pulBuffer != 0)
        {
            OpenVR.IOBuffer.Close(_pulBuffer);
            _pulBuffer = 0uL;
        }
    }

    private unsafe void ReadLoopAsync(FrooxEngineContext context)
    {
        Task.Run(delegate
        {
            while (true)
            {
                var punRead = 0u;
                var imuSample_t = default(ImuSample_t);
                try
                {
                    if (OpenVR.IOBuffer.Read(_pulBuffer, (IntPtr)(&imuSample_t), (uint)sizeof(ImuSample_t), ref punRead) == EIOBufferError.IOBuffer_Success && punRead == sizeof(ImuSample_t))
                    {
                        var fSampleTime = imuSample_t.fSampleTime;
                        var vAccel = new double3(imuSample_t.vAccel.v0, imuSample_t.vAccel.v1, imuSample_t.vAccel.v2);
                        var vGyro = new double3(imuSample_t.vGyro.v0, imuSample_t.vGyro.v1, imuSample_t.vGyro.v2);
                        var flags = (Imu_OffScaleFlags)imuSample_t.unOffScaleFlags;
                        OnData.ExecuteAsync(context);
                        context.World.RunSynchronously(delegate
                        {
                            FSampleTime.Write(fSampleTime, context);
                            VAccel.Write(vAccel, context);
                            VGyro.Write(vGyro, context);
                            UnOffScaleFlags.Write(flags, context);
                        });
                    }
                    else Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    break;
                }
            }
            if (_pulBuffer != 0)
            {
                OpenVR.IOBuffer.Close(_pulBuffer);
                _pulBuffer = 0uL;
            }
            context.World.RunSynchronously(delegate
            {
                IsOpened.Write(value: false, context);
                FailReason.Write(ImuErrorCode.UnknownException, context);
            });
        });
    }

    public ImuReader()
    {
        //Close = new Operation(this, 0);
        IsOpened = new ValueOutput<bool>(this);
        FailReason = new ValueOutput<ImuErrorCode>(this);
        FSampleTime = new ValueOutput<double>(this);
        VAccel = new ValueOutput<double3>(this);
        VGyro = new ValueOutput<double3>(this);
        UnOffScaleFlags = new ValueOutput<Imu_OffScaleFlags>(this);
    }


    private IOperation Fail(ImuErrorCode error, FrooxEngineContext context)
    {
        IsOpened.Write(value: false, context);
        FailReason.Write(error, context);
        return OnFail.Target;
    }
}
*/
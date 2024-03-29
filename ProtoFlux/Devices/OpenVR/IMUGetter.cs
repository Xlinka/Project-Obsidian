using System;
using System.Threading;
using System.Threading.Tasks;
using Elements.Core;
using FrooxEngine.ProtoFlux;
using OpenvrDataGetter;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Valve.VR;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian
{
    [NodeCategory("OpenvrDataGetter")]
    public class ImuReader : AsyncActionNode<FrooxEngineContext>
    {
        public ObjectInput<string> DevicePath;

        public AsyncCall OnOpened;

        public Continuation OnClosed;

        public Continuation OnFail;

        public AsyncCall OnData;

        public readonly ValueOutput<bool> IsOpened;

        public readonly ValueOutput<ErrorCode> FailReason;

        [ContinuouslyChanging]
        public readonly ValueOutput<double> FSampleTime;

        [ContinuouslyChanging]
        public readonly ValueOutput<double3> VAccel;

        [ContinuouslyChanging]
        public readonly ValueOutput<double3> VGyro;

        [ContinuouslyChanging]
        public readonly ValueOutput<Imu_OffScaleFlags> UnOffScaleFlags;

        private ulong pulBuffer = 0uL;

        private Thread thread = null;
        public enum ErrorCode
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
        protected unsafe override async Task<IOperation> RunAsync(FrooxEngineContext context)
        {
            UniLog.Log("RunAsync: Starting");
            string path = DevicePath.Evaluate(context);
            if (string.IsNullOrEmpty(path))
            {
                UniLog.Log("RunAsync: Path is null or empty.");
                FailReason.Write(ErrorCode.PathIsNullOrEmpty, context);
                return OnFail.Target;
            }
            EIOBufferError errorcode = OpenVR.IOBuffer.Open(path, EIOBufferMode.Read, (uint)sizeof(ImuSample_t), 0u, ref pulBuffer);
            if (errorcode == EIOBufferError.IOBuffer_Success)
            {
                UniLog.Log("RunAsync: OpenVR.IOBuffer.Open succeeded.");
                OnOpened.ExecuteAsync(context);
                thread = new Thread((ThreadStart)delegate
                {
                    ReadLoopAsync(context);
                });
                thread.Start();
                IsOpened.Write(value: true, context);
                return OnOpened.Target;
            }
            UniLog.Log($"RunAsync: OpenVR.IOBuffer.Open failed with error code {errorcode}.");
            IsOpened.Write(value: false, context);
            FailReason.Write((ErrorCode)errorcode, context);
            return OnFail.Target;
        }

        public IOperation Close(FrooxEngineContext context)
        {
            if (pulBuffer == 0)
            {
                FailReason.Write(ErrorCode.AlreadyClosed, context);
                return OnFail.Target;
            }
            EIOBufferError eIOBufferError = OpenVR.IOBuffer.Close(pulBuffer);
            if (eIOBufferError == EIOBufferError.IOBuffer_Success)
            {
                IsOpened.Write(value: false, context);
                pulBuffer = 0uL;
                if (thread != null)
                {
                    thread.Abort();
                    thread = null;
                }
                return OnClosed.Target;
            }
            FailReason.Write((ErrorCode)eIOBufferError, context);
            return OnFail.Target;
        }

        private new void Dispose()
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
            if (pulBuffer != 0)
            {
                OpenVR.IOBuffer.Close(pulBuffer);
                pulBuffer = 0uL;
            }
        }

        private unsafe Task<IOperation> ReadLoopAsync(FrooxEngineContext context)
        {
            return (Task<IOperation>)Task.Run(delegate
            {
                while (true)
                {
                    uint punRead = 0u;
                    ImuSample_t imuSample_t = default(ImuSample_t);
                    try
                    {
                        if (OpenVR.IOBuffer.Read(pulBuffer, (IntPtr)(&imuSample_t), (uint)sizeof(ImuSample_t), ref punRead) == EIOBufferError.IOBuffer_Success && punRead == sizeof(ImuSample_t))
                        {
                            double fSampleTime = imuSample_t.fSampleTime;
                            double3 vAccel = new double3(imuSample_t.vAccel.v0, imuSample_t.vAccel.v1, imuSample_t.vAccel.v2);
                            double3 vGyro = new double3(imuSample_t.vGyro.v0, imuSample_t.vGyro.v1, imuSample_t.vGyro.v2);
                            Imu_OffScaleFlags flags = (Imu_OffScaleFlags)imuSample_t.unOffScaleFlags;
                            OnData.ExecuteAsync(context);
                            context.World.RunSynchronously(delegate
                            {
                                FSampleTime.Write(fSampleTime, context);
                                VAccel.Write(vAccel, context);
                                VGyro.Write(vGyro, context);
                                UnOffScaleFlags.Write(flags, context);
                                UniLog.Log($"ImuReader: Successfully read IMU data. Time: {fSampleTime}, Accel: {vAccel}, Gyro: {vGyro}, Flags: {flags}");
                            });
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                    }
                    catch (Exception ex)
                    {
                        UniLog.Log("ImuReader: Exception in ReadLoop: " + ex.Message);
                        break;
                    }
                }
                if (pulBuffer != 0)
                {
                    OpenVR.IOBuffer.Close(pulBuffer);
                    pulBuffer = 0uL;
                }
                context.World.RunSynchronously(delegate
                {
                    IsOpened.Write(value: false, context);
                    FailReason.Write(ErrorCode.UnknownException, context);
                });
            });
        }

        public ImuReader()
        {
            IsOpened = new ValueOutput<bool>(this);
            FailReason = new ValueOutput<ErrorCode>(this);
            FSampleTime = new ValueOutput<double>(this);
            VAccel = new ValueOutput<double3>(this);
            VGyro = new ValueOutput<double3>(this);
            UnOffScaleFlags = new ValueOutput<Imu_OffScaleFlags>(this);
        }


        private IOperation Fail(ErrorCode error, FrooxEngineContext context)
        {
            IsOpened.Write(value: false, context);
            FailReason.Write(error, context);
            return OnFail.Target;
        }
    }

}


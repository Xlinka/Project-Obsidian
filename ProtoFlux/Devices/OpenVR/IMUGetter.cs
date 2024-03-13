using System;
using System.Threading.Tasks;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using Valve.VR;
using System.Threading;
using ProtoFlux.Runtimes.Execution;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;

namespace OpenvrDataGetter
{
    [NodeCategory("OpenvrDataGetter")]
    public class ImuReader : AsyncActionNode<FrooxEngineContext>
    {
        public ObjectInput<string> DevicePath;

        public AsyncCall OnOpened;
        public Continuation OnClosed;
        public Continuation OnFail;
        public Continuation OnData;

        public readonly ValueOutput<bool> IsOpened;
        public readonly ValueOutput<ErrorCode> FailReason;
        public readonly ValueOutput<double> FSampleTime;
        public readonly ValueOutput<double3> VAccel;
        public readonly ValueOutput<double3> VGyro;
        public readonly ValueOutput<Imu_OffScaleFlags> UnOffScaleFlags;

        ulong pulBuffer = 0;
        Thread thread = null;

        protected override async Task<IOperation> RunAsync(FrooxEngineContext context)
        {
            string path = DevicePath.Evaluate(context);
            if (string.IsNullOrEmpty(path))
            {
                Fail(ErrorCode.PathIsNullOrEmpty, context);
                return null;
            }
            if (pulBuffer == 0)
            {
                try
                {
                    EIOBufferError errorcode;
                    unsafe
                    {
                        errorcode = OpenVR.IOBuffer.Open(path, EIOBufferMode.Read, (uint)sizeof(ImuSample_t), 0, ref pulBuffer);
                    }
                    if (errorcode != EIOBufferError.IOBuffer_Success)
                    {
                        Fail((ErrorCode)errorcode, context);
                        return null;
                    }
                    thread = new Thread(() => ReadLoop(context));
                    thread.Start();
                }
                catch (Exception e)
                {
                    UniLog.Log(e);
                    Fail(ErrorCode.UnknownException, context);
                    return null;
                }
                return OnOpened.Target;
            }
            else
            {
                Fail(ErrorCode.AlreadyOpened, context);
            }
       


            return null;
        }

        public IOperation Close(FrooxEngineContext context)
        {
            if (pulBuffer == 0)
            {
                Fail(ErrorCode.AlreadyClosed, context);
                return null;
            }
            var error = OpenVR.IOBuffer.Close(pulBuffer);
            if (error != EIOBufferError.IOBuffer_Success)
            {
                Fail((ErrorCode)error, context);
                return null;
            }
            try
            {
                thread.Abort();
            }
            catch (Exception e)
            {
                UniLog.Log(e);
                Fail(ErrorCode.UnknownException, context);
                return null;
            }
            thread = null;
            pulBuffer = 0;
            
            return OnClosed.Target;
        }



        void Dispose()
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
            if (pulBuffer != 0)
            {
                OpenVR.IOBuffer.Close(pulBuffer);
            }
        }

        private unsafe void ReadLoop(FrooxEngineContext context)
        {
            EIOBufferError failReason = EIOBufferError.IOBuffer_Success;
            const uint arraySize = 10;
            ImuSample_t[] samples = new ImuSample_t[arraySize];
            try
            {
                while (true)
                {
                    uint punRead = new();
                    unsafe
                    {
                        fixed (ImuSample_t* pSamples = samples)
                        {
                            failReason = OpenVR.IOBuffer.Read(pulBuffer, (IntPtr)pSamples, (uint)sizeof(ImuSample_t) * arraySize, ref punRead);
                        }
                    }
                    if (failReason != EIOBufferError.IOBuffer_Success)
                    {
                        throw new Exception("read retuned: " + failReason.ToString());
                    }
                    int unreadSize = (int)punRead / sizeof(ImuSample_t);
                    for (int i = 0; i < unreadSize; i++)
                    {
                        var sample = samples[i];
                        context.World.RunSynchronously(() =>
                        {
                            FSampleTime.Write(sample.fSampleTime, context);
                            VAccel.Write(new double3(sample.vAccel.v0, sample.vAccel.v1, sample.vAccel.v2), context);
                            VGyro.Write(new double3(sample.vGyro.v0, sample.vGyro.v1, sample.vGyro.v2), context);
                            UnOffScaleFlags.Write((Imu_OffScaleFlags)sample.unOffScaleFlags, context);
                            OnData.Execute(context);
                        });
                    }

                    if (unreadSize == 0) Thread.Sleep(10);
                }

            }
            catch (Exception e)
                {
                    UniLog.Log(e);

                    thread = null;
                context.World.RunSynchronously(() =>
                    {
                    IsOpened.Equals(false);
                        Fail(failReason == EIOBufferError.IOBuffer_Success ? ErrorCode.UnknownException : (ErrorCode)failReason, context);
                    });
                    OpenVR.IOBuffer.Close(pulBuffer);
                    pulBuffer = 0;
                }
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
        private void Fail(ErrorCode error, FrooxEngineContext context)
        {
            {
                FailReason.Write(error, context);
                OnFail.Execute(context);
                FailReason.Equals(ErrorCode.None);
            }
        }
        
        public enum ErrorCode
        {
            None = 0,
            AlreadyOpened,
            AlreadyClosed,
            UnknownException,
            PathIsNullOrEmpty,
            IOBuffer_OperationFailed = 100,
            IOBuffer_InvalidHandle = 101,
            IOBuffer_InvalidArgument = 102,
            IOBuffer_PathExists = 103,
            IOBuffer_PathDoesNotExist = 104,
            IOBuffer_Permission = 105
        }
     
    }
}

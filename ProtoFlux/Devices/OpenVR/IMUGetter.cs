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

namespace OpenvrDataGetter
{
    [NodeCategory("OpenvrDataGetter")]
    public class ImuReader : AsyncActionNode<FrooxEngineContext>, IDisposable
    {
        public ObjectInput<string> DevicePath;

        public Call OnOpened;
        public Call OnClosed;
        public Call OnFail;
        public Call OnData;

        public readonly ValueOutput<bool> IsOpened;
        public readonly ValueOutput<ErrorCode> FailReason;
        public readonly ValueOutput<double> FSampleTime;
        public readonly ValueOutput<double3> VAccel;
        public readonly ValueOutput<double3> VGyro;
        public readonly ValueOutput<Imu_OffScaleFlags> UnOffScaleFlags;

        private ulong bufferHandle = 0;
        private Thread readThread;
        private CancellationTokenSource cancellationTokenSource;

        protected override async Task<IOperation> RunAsync(FrooxEngineContext context)
        {
            string path = DevicePath.Evaluate(context);
            if (string.IsNullOrEmpty(path))
            {
                Fail(ErrorCode.PathIsNullOrEmpty, context);
                return null;
            }

            if (bufferHandle == 0)
            {
                try
                {
                    bufferHandle = await OpenBufferAsync(path);
                    IsOpened.Write(true, context);
                    OnOpened.Execute(context);

                    cancellationTokenSource = new CancellationTokenSource();
                    readThread = new Thread(() => ReadLoop(context, cancellationTokenSource.Token));
                    readThread.Start();
                }
                catch (Exception e)
                {
                    UniLog.Log(e);
                    Fail(ErrorCode.UnknownException, context);
                }
            }
            else
            {
                Fail(ErrorCode.AlreadyOpened, context);
            }

            return null;
        }

        public void Close()
        {
            if (bufferHandle != 0)
            {
                cancellationTokenSource?.Cancel();
                readThread?.Join();
                CloseBuffer(bufferHandle);
                bufferHandle = 0;
            }
        }

        public void Dispose()
        {
            Close();
        }

        private void ReadLoop(FrooxEngineContext context, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ImuSample_t[] samples = new ImuSample_t[10];
                uint punRead = 0;
                EIOBufferError error;

                unsafe
                {
                    fixed (ImuSample_t* pSamples = samples)
                    {
                        error = OpenVR.IOBuffer.Read(bufferHandle, (IntPtr)pSamples, (uint)(Marshal.SizeOf<ImuSample_t>() * 10), ref punRead);
                    }
                }

                if (error != EIOBufferError.IOBuffer_Success)
                {
                    UniLog.Log($"Error reading from buffer: {error}");
                    Fail(ErrorCode.IOBuffer_OperationFailed, context);
                    return;
                }

                for (int i = 0; i < punRead / Marshal.SizeOf<ImuSample_t>(); i++)
                {
                    ImuSample_t sample = samples[i];
                    context.World.RunSynchronously(() =>
                    {
                        FSampleTime.Write(sample.fSampleTime, context);
                        VAccel.Write(new double3(sample.vAccel.v0, sample.vAccel.v1, sample.vAccel.v2), context);
                        VGyro.Write(new double3(sample.vGyro.v0, sample.vGyro.v1, sample.vGyro.v2), context);
                        UnOffScaleFlags.Write((Imu_OffScaleFlags)sample.unOffScaleFlags, context);
                        OnData.Execute(context);
                    });
                }

                if (punRead == 0)
                    Thread.Sleep(10);
            }
        }


        private async Task<ulong> OpenBufferAsync(string path)
        {
            return await Task.Run(() =>
            {
                ulong bufferHandle = 0;
                var error = OpenVR.IOBuffer.Open(path, EIOBufferMode.Read, (uint)Marshal.SizeOf<ImuSample_t>(), 0, ref bufferHandle);
                if (error != EIOBufferError.IOBuffer_Success)
                {
                    throw new Exception($"Failed to open buffer: {error}");
                }
                return bufferHandle;
            });
        }


        private void CloseBuffer(ulong bufferHandle)
        {
            OpenVR.IOBuffer.Close(bufferHandle);
        }

        private void Fail(ErrorCode error, FrooxEngineContext context)
        {
            FailReason.Write(error, context);
            OnFail.Execute(context);
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

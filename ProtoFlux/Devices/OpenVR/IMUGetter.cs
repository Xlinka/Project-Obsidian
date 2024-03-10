using System;
using System.Threading.Tasks;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using Valve.VR;

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

        private ulong _pulBuffer = 0;
        private Task _readTask;
        private bool _continueReading = false;

     
        protected override async Task<IOperation> RunAsync(FrooxEngineContext context)
        {
            string path = DevicePath.Evaluate(context);
            if (string.IsNullOrEmpty(path))
            {
                Fail(ErrorCode.PathIsNullOrEmpty, context);
                return null;
            }
            // OpenVR and IOBuffer operations would need to be adapted to ProtoFlux's async patterns
            if (_pulBuffer == 0)
            {
                try
                {
                    _pulBuffer = await OpenBufferAsync(path);
                    _continueReading = true;
                    _readTask = Task.Run(() => ReadLoop(context));
                    IsOpened.Write(true, context);
                    OnOpened.Execute(context);
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

        public void Close(FrooxEngineContext context)
        {
            if (_pulBuffer != 0)
            {
                CloseBuffer(_pulBuffer);
                _pulBuffer = 0;
                _continueReading = false;
                _readTask?.Wait();
                _readTask = null;
                IsOpened.Write(false, context);
                OnClosed.Execute(context);
            }
            else
            {
                Fail(ErrorCode.AlreadyClosed, context);
            }
        }

        private void ReadLoop(FrooxEngineContext context) // this isnt the real one its a placeholder and not working 
        {
            while (_continueReading)
            {
                // Simulated reading process
                Task.Delay(10).Wait(); // Replace with actual data reading and handling
                // Simulate data output and triggers
                FSampleTime.Write(0.0, context);
                VAccel.Write(new double3(0, 0, 0), context);
                VGyro.Write(new double3(0, 0, 0), context);
                UnOffScaleFlags.Write(Imu_OffScaleFlags.None, context);
                OnData.Execute(context);
            }
        }

        public void Dispose()
        {
            _continueReading = false;
            _readTask?.Wait();
            CloseBuffer(_pulBuffer);
        }

        private void Fail(ErrorCode error, FrooxEngineContext context)
        {
            FailReason.Write(error, context);
            OnFail.Execute(context);
        }
        private Task<ulong> OpenBufferAsync(string path)
        {
            // Open the buffer using OpenVR's IOBuffer interface
            return Task.Run(() =>
            {
                ulong pulBuffer = 0;
                var error = OpenVR.IOBuffer.Open(path, OpenVR.VRIOBufferMode_Read, out pulBuffer);
                if (error != EVRIOBufferError.IOBuffer_Success)
                {
                    throw new Exception($"Failed to open buffer: {error}");
                }
                return pulBuffer;
            });
        }

        private void CloseBuffer(ulong buffer)
        {
            OpenVR.IOBuffer.Close(buffer);
        }
    }
    public enum ErrorCode
    {
        None = 0, //IOBuffer_Success = 0,
        AlreadyOpened,
        AlreadyClosed,
        UnknownException,
        PathIsNullOrEmpty,
        OpenVrNotFound,
        IOBuffer_OperationFailed = 100,
        IOBuffer_InvalidHandle = 101,
        IOBuffer_InvalidArgument = 102,
        IOBuffer_PathExists = 103,
        IOBuffer_PathDoesNotExist = 104,
        IOBuffer_Permission = 105
    }
}
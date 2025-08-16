using System;
using System.Runtime.InteropServices;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using Valve.VR;
using ProtoFlux.Runtimes.Execution;
using Elements.Core;
using System.Threading.Tasks;
using Obsidian;



namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Devices
{
    [NodeCategory("Obsidian/Devices")]
    public class ImuDataNode : AsyncActionNode<FrooxEngineContext>
    {
        public ObjectInput<User> SimulatingUser;
        public ObjectInput<string> Path;
        public ValueInput<bool> Simulate;

        public ValueOutput<double> SampleTime;
        public ValueOutput<double3> VAccel;
        public ValueOutput<double3> VGyro;
        public ValueOutput<Imu_OffScaleFlags> OffScaleFlags;

        public AsyncCall OnSimulationStart;
        public Continuation OnDataUpdated;
        public Continuation OnError;

        private ulong _buffer;
        private string _previousPath;

        public ImuDataNode()
        {
            SampleTime = new ValueOutput<double>(this);
            VAccel = new ValueOutput<double3>(this);
            VGyro = new ValueOutput<double3>(this);
            OffScaleFlags = new ValueOutput<Imu_OffScaleFlags>(this);
        }

        protected override async Task<IOperation> RunAsync(FrooxEngineContext context)
        {
            User user = SimulatingUser.Evaluate(context);
            bool simulate = Simulate.Evaluate(context);
            string path = Path.Evaluate(context);

            if (!simulate || user != context.LocalUser || string.IsNullOrEmpty(path))
            {
                CloseBuffer();
                return OnError.Target;
            }

            await OnSimulationStart.ExecuteAsync(context);

            if (_buffer == 0 || _previousPath != path)
            {
                CloseBuffer();
                if (!OpenBuffer(path))
                {
                    return OnError.Target;
                }
            }

            ReadImuData(context);
            return OnDataUpdated.Target;
        }

        private bool OpenBuffer(string path)
        {
            if (!OpenVR_Helper.TryInitialize()) return false;
            unsafe
            {
                ulong buffer = default;
                var errorCode = OpenVR.IOBuffer.Open(path, EIOBufferMode.Read, (uint)sizeof(ImuSample_t), 0u, ref buffer);
                if (errorCode == EIOBufferError.IOBuffer_Success)
                {
                    _buffer = buffer;
                    _previousPath = path;
                    return true;
                }
                return false;
            }
        }

        private void ReadImuData(FrooxEngineContext context)
        {
            unsafe
            {
                var punRead = 0u;
                var imuSample_t = default(ImuSample_t);
                var error = OpenVR.IOBuffer.Read(_buffer, (IntPtr)(&imuSample_t), (uint)sizeof(ImuSample_t), ref punRead);

                if (error == EIOBufferError.IOBuffer_Success && punRead == sizeof(ImuSample_t))
                {
                    // Writing each IMU data point with context to ensure correct synchronization and data handling
                    SampleTime.Write(imuSample_t.fSampleTime, context);
                    VAccel.Write(new double3(imuSample_t.vAccel.v0, imuSample_t.vAccel.v1, imuSample_t.vAccel.v2), context);
                    VGyro.Write(new double3(imuSample_t.vGyro.v0, imuSample_t.vGyro.v1, imuSample_t.vGyro.v2), context);
                    OffScaleFlags.Write((Imu_OffScaleFlags)imuSample_t.unOffScaleFlags, context);
                }
            }
        }

        private void CloseBuffer()
        {
            if (_buffer == 0) return;
            OpenVR.IOBuffer.Close(_buffer);
            _buffer = 0;
        }
    }

}

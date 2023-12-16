using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Elements.Core;
using FrooxEngine;

namespace Obsidian
{

    [Category(new string[] { "Obsidian/Network/ArtNet" })]
    public class ArtNetClient : Component
    {
        public readonly Sync<Uri> URL;
        public readonly UserRef HandlingUser;
        public readonly Sync<string> AccessReason;
        public readonly Sync<float> ConnectRetryInterval;
        public readonly Sync<bool> IsConnected;

        private Uri _currentURL;
        private UdpClient _udpClient;

        public event Action<ArtNetClient> Connected;
        public event Action<ArtNetClient> Closed;
        public event Action<ArtNetClient, string> Error;
        public event Action<ArtNetClient, byte[]> PacketReceived;  // For ArtNet packets
        public event Action<ArtNetClient, byte[]> DMXDataReceived; // For DMX512 data

        protected override void OnAwake()
        {
            ConnectRetryInterval.Value = 10f;
        }

        protected override void OnChanges()
        {
            Uri uri = (Enabled ? URL.Value : null);
            if (HandlingUser.Target != LocalUser)
            {
                uri = null;
            }

            if (uri != _currentURL)
            {
                _currentURL = uri;
                CloseCurrent();
                IsConnected.Value = false;
                if (_currentURL != null)
                {
                    StartTask(async delegate
                    {
                        await ConnectTo(_currentURL);
                    });
                }
            }
        }

        private async Task ConnectTo(Uri target)
        {
            if (target.Scheme != "artnet")
            {
                Error?.Invoke(this, "Invalid URL scheme. Expected 'artnet://'.");
                return;
            }

            if (await Engine.Security.RequestAccessPermission(target.Host, target.Port, AccessReason.Value ?? "ArtNet Client") == HostAccessPermission.Allowed && !(target != _currentURL) && !IsRemoved)
            {
                _udpClient = new UdpClient(target.Port);
                IsConnected.Value = true;
                Connected?.Invoke(this);
                StartTask(ReceiveLoop);
            }
        }

        private async Task ReceiveLoop()
        {
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (IsConnected.Value && _udpClient != null)
            {
                try
                {
                    UdpReceiveResult result = await _udpClient.ReceiveAsync();
                    byte[] receivedData = result.Buffer;

                    // Check if the data is an ArtNet packet or DMX512 data
                    if (IsArtNetPacket(receivedData))
                    {
                        PacketReceived?.Invoke(this, receivedData);
                    }
                    else
                    {
                        DMXDataReceived?.Invoke(this, receivedData);
                    }
                }
                catch (Exception ex)
                {
                    Error?.Invoke(this, ex.Message);
                    break;
                }
            }
        }

        private bool IsArtNetPacket(byte[] data)
        {
            // Art-Net packets start with the ASCII sequence for "Art-Net" followed by a null byte.
            byte[] artNetHeader = new byte[] { 65, 114, 116, 45, 78, 101, 116, 0 };

            if (data.Length < artNetHeader.Length)
            {
                return false;
            }

            for (int i = 0; i < artNetHeader.Length; i++)
            {
                if (data[i] != artNetHeader[i])
                {
                    return false;
                }
            }

            return true;
        }

        protected override void OnDispose()
        {
            CloseCurrent();
            base.OnDispose();
        }

        private void CloseCurrent()
        {
            if (_udpClient != null)
            {
                UdpClient udpClient = _udpClient;
                _udpClient = null;
                try
                {
                    Closed?.Invoke(this);
                }
                catch (Exception ex)
                {
                    UniLog.Error("Exception in running Closed event on ArtNetClient:\n" + ex);
                }
                udpClient.Close();
            }
        }

        public static implicit operator ArtNetClient(ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Network.WebsocketConnect v)
        {
            throw new NotImplementedException();
        }
    }
}
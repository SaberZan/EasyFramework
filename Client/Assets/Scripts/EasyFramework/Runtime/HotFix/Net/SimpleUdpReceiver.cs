using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Easy
{

    public class SimpleUdpReceiver
    {
        private Socket _receiveSocket;

        private Thread _receiveThread;

        private IPEndPoint _iep;

        private Queue<byte[]> _receiveDatas = new Queue<byte[]>();

        private Action<byte[]> _receiveCallback;

        public void Start(IPAddress ipAddress, int port, Action<byte[]> receiveCallback = null)
        {
            _receiveCallback = receiveCallback;

            _receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _iep = new IPEndPoint(ipAddress, port);
            _receiveSocket.Bind(_iep);

            _receiveThread = new Thread(Receive);
            _receiveThread.IsBackground = true;
            _receiveThread.Start(_receiveSocket);
        }

        public void Close()
        {
            if (_receiveThread != null)
            {
                _receiveThread.Abort();
                _receiveThread = null;
            }

            if (_receiveSocket != null)
            {
                _receiveSocket.Close();
                _receiveSocket = null;
            }
        }

        private void Receive(object socket)
        {
            var senderSocket = socket as Socket;
            EndPoint ep = (EndPoint)_iep;
            while (true)
            {
                byte[] data = new byte[1024];//设置缓冲数据流
                var len = senderSocket.ReceiveFrom(data, ref ep);            //接收数据,并确把数据设置到缓冲流里面
                if (len >= 0)
                {
                    byte[] buffer = new byte[len];
                    System.Array.Copy(data , 0, buffer, 0, len);
                    lock (_receiveDatas)
                    {
                        _receiveDatas.Enqueue(data);
                    }
                }
                Thread.Sleep(10);
            }
        }

        // Update is called once per frame
        public void Update()
        {
            lock (_receiveDatas)
            {
                while (_receiveDatas.Count > 0)
                {
                    byte[] cmd = _receiveDatas.Dequeue();
                    _receiveCallback?.Invoke(cmd);
                }
            }
        }

    }

}

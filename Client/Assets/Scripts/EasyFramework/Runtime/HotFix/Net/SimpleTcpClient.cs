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

    public class SimpleTcpClient
    {
        private Socket _clickSocket;

        private Thread _thread;

        private Queue<byte[]> _receiveCmds = new Queue<byte[]>();

        private Action<byte[]> _receiveCallback;

        private const int _HEAD_LENGTH = 4;

        public void Start(IPAddress ipAddress, int port, Action<byte[]> receiveCallback = null)
        {
            _receiveCallback = receiveCallback;
            _receiveCmds.Clear();
            //创建实例
            _clickSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            //进行连接
            _clickSocket.Connect(ipEndPoint);

            _thread = new Thread(Receive);
            _thread.IsBackground = true;
            _thread.Start(_clickSocket);
        }

        private void Close()
        {
            if (_thread != null)
            {
                _thread.Abort();
                _thread = null;
            }

            if (_clickSocket != null)
            {
                _clickSocket.Close();
                _clickSocket = null;
            }

        }

        public void Receive(object socket)
        {
            var serverSocket = socket as Socket;
            int bufferLength = 0;
            byte[] buffer = new byte[1024 * 1024 * 2];
            while (true)
            {
                //获取发送过来的消息
                byte[] data = new byte[1024];
                var len = serverSocket.Receive(data);
                if (len >= 0)
                {
                    if(bufferLength + len > buffer.Length - 1)
                    {
                        System.Array.Resize(ref buffer, bufferLength + len);
                    }
                    System.Array.Copy(data , 0, buffer, bufferLength, len);
                    bufferLength += len;
                    if(bufferLength >= _HEAD_LENGTH)
                    {
                        int dataLen = BitConverter.ToInt32(buffer, 0);
                        if(dataLen <= bufferLength)
                        {
                            byte[] cmdBytes = new byte[dataLen];
                            System.Array.Copy(buffer, _HEAD_LENGTH , cmdBytes,  0, dataLen);
                            lock (_receiveCmds)
                            {
                                _receiveCmds.Enqueue(cmdBytes);
                            }
                            System.Array.Copy(buffer, _HEAD_LENGTH + dataLen, buffer,  0, buffer.Length - _HEAD_LENGTH - dataLen);
                            bufferLength -= _HEAD_LENGTH + dataLen;
                        }
                    }
                }
                Thread.Sleep(10);
            }
        }

        // Update is called once per frame
        public void Update()
        {
            lock (_receiveCmds)
            {
                while (_receiveCmds.Count > 0)
                {
                    byte[] cmd = _receiveCmds.Dequeue();
                    _receiveCallback?.Invoke(cmd);
                }
            }
        }

        public void Send(byte[] data)
        {
            if (_clickSocket != null)
            {
                byte[] sendBuffer = new byte[data.Length + _HEAD_LENGTH];
                System.Array.Copy(sendBuffer, 0, BitConverter.GetBytes(data.Length),  0, _HEAD_LENGTH);
                System.Array.Copy(sendBuffer, _HEAD_LENGTH, data,  0, data.Length); 
                _clickSocket.Send(sendBuffer);
            }
        }
    }
}
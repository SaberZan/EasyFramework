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

    public class SimpleSocketServer
    {
        private List<Socket> _clientSockets = new List<Socket>();

        private List<Thread> _threads = new List<Thread>();

        private Queue<byte[]> _broadcastDatas = new Queue<byte[]>();

        private Queue<byte[]> _receiveDatas = new Queue<byte[]>();

        private int _maxNum;

        private Socket _serverSocket;

        private Thread _thread;

        private Thread _broadcastThread;

        private Action<byte[]> _receiveCallback;

        private const int _HEAD_LENGTH = 4;

        public void Start(int port, int maxNum, Action<byte[]> receiveCallback = null)
        {
            _receiveCallback = receiveCallback;
            
            _clientSockets.Clear();
            _broadcastDatas.Clear();
            _receiveDatas.Clear();
            _maxNum = maxNum;

            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Any;
            IPEndPoint iep = new IPEndPoint(ip, port);
            //socket绑定监听地址
            _serverSocket.Bind(iep);
            //设置同时连接个数
            _serverSocket.Listen(_maxNum);

            //利用线程后台执行监听,否则程序会假死
            _thread = new Thread(Listen);
            _thread.IsBackground = true;
            _thread.Start(_serverSocket);

            _broadcastThread = new Thread(Broadcast);
            _broadcastThread.IsBackground = true;
            _broadcastThread.Start();
        }


        public void Close()
        {
            if (_serverSocket != null)
            {
                _serverSocket.Close();
                _clientSockets.Clear();
            }

            if (_thread != null)
            {
                _thread.Abort();
                _thread = null;
            }

            if (_broadcastThread != null)
            {
                _broadcastThread.Abort();
                _broadcastThread = null;
            }

            _threads.ForEach(thead => thead.Abort());
            _threads.Clear();
        }

        /// <summary>
        /// 监听连接
        /// </summary>
        /// <param name="socket"></param>
        private void Listen(object socket)
        {
            var serverSocket = socket as Socket;
            while (true)
            {
                //等待连接并且创建一个负责通讯的socket
                var client = serverSocket.Accept();
                _clientSockets.Add(client);
                //获取链接的IP地址
                var sendIpoint = client.RemoteEndPoint.ToString();

                //开启一个新线程不停接收消息
                Thread thread = new Thread(Receive);
                thread.IsBackground = true;
                thread.Start(client);
                _threads.Add(thread);
            }
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="socket"></param>
        private void Receive(object socket)
        {
            var clientSocket = socket as Socket;
            int bufferLength = 0;
            byte[] buffer = new byte[1024 * 1024 * 2];
            while (true)
            {
                //获取发送过来的消息
                byte[] data = new byte[1024];
                var len = clientSocket.Receive(data);
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
                            lock (_receiveDatas)
                            {
                                _receiveDatas.Enqueue(cmdBytes);
                            }
                            System.Array.Copy(buffer, _HEAD_LENGTH + dataLen, buffer,  0, buffer.Length - _HEAD_LENGTH - dataLen);
                            bufferLength -= _HEAD_LENGTH + dataLen;
                        }
                    }
                }
            }
        }

        private void Broadcast()
        {
            while (true)
            {
                lock (_broadcastDatas)
                {
                    while (_broadcastDatas.Count > 0)
                    {
                        byte[] sendBuffer = _broadcastDatas.Dequeue();
                        foreach (Socket socket in _clientSockets)
                        {
                            socket.Send(sendBuffer);
                        }
                    }
                }
                Thread.Sleep(33);
            }
        }

        public void SendBroadcastCmd(byte[] data)
        {
            lock (_broadcastDatas)
            {
                byte[] sendBuffer = new byte[data.Length + _HEAD_LENGTH];
                System.Array.Copy(sendBuffer, 0, BitConverter.GetBytes(data.Length),  0, _HEAD_LENGTH);
                System.Array.Copy(sendBuffer, _HEAD_LENGTH, data,  0, data.Length); 
                _broadcastDatas.Enqueue(sendBuffer);
            }
        }

        public void Send(int index, byte[] data)
        {
            Socket socketClient = null;
            if(index < _clientSockets.Count)
            {
                socketClient = _clientSockets[index];
            }
            if(socketClient != null)
            {
                byte[] sendBuffer = new byte[data.Length + _HEAD_LENGTH];
                System.Array.Copy(sendBuffer, 0, BitConverter.GetBytes(data.Length),  0, _HEAD_LENGTH);
                System.Array.Copy(sendBuffer, _HEAD_LENGTH, data,  0, data.Length); 
                socketClient.Send(sendBuffer);
            }
        }
    }

}
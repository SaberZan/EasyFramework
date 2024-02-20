using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class SimpleUdpSender
{
    private Socket _sendSocket;

    private IPEndPoint _iep;

    /// <summary>
    /// 创建一个广播消息实例
    /// </summary>
    public void Start(IPAddress ipAddress, int port)
    {
        _iep = new IPEndPoint(ipAddress, port);
        _sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public void Close()
    {
        if (_sendSocket != null)
        {
            _sendSocket.Close();
            _sendSocket = null;
        }
    }

    /// <summary>
    /// 轮播消息线程方法
    /// </summary>
    private void Send(byte[] data)
    {
        _sendSocket.SendTo(data, _iep);
    }
}
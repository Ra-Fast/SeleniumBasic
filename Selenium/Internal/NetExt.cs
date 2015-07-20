﻿using Selenium.Core;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Selenium.Internal {

    static class NetExt {

        /// <summary>
        /// Lock a new TCP end point and returns the socket so it can be unlocked later.
        /// </summary>
        public static void LockNewEndPoint(IPAddress address, out Socket socket, out IPEndPoint endpoint) {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            socket.ReceiveBufferSize = 0;
            socket.SendBufferSize = 0;
            socket.Bind(new IPEndPoint(address, 0));
            endpoint = (IPEndPoint)socket.LocalEndPoint;
            //Disable inheritance to the child processes so the main process can close the
            //socket once a child process is launched.
            NativeMethods.SetHandleInformation(socket.Handle, 1, 0);
        }

        /// <summary>
        /// Returns true if a given host:port is connectable, false otherwise
        /// </summary>
        /// <param name="endPoint">Endpoint holding the host ip and port</param>
        /// <param name="timeout">Timeout in millisecond</param>
        /// <param name="delay">Delay to retry in millisecond</param>
        /// <returns>True if succeed, false otherwise</returns>
        public static bool WaitForPortConnectable(IPEndPoint endPoint, int timeout, int delay) {
            var endtime = DateTime.UtcNow.AddMilliseconds(timeout);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.ReceiveTimeout = socket.SendTimeout = 1;
            socket.NoDelay = true;
            try {
                while (true) {
                    DateTime start = DateTime.UtcNow;
                    try {
                        socket.Connect(endPoint);
                        return true;
                    } catch (SocketException) {
                    } catch (Exception ex) {
                        throw new SeleniumException(ex);
                    }

                    if (DateTime.UtcNow > endtime)
                        return false;
                    SysWaiter.Wait(delay);
                }
            } finally {
                socket.Disconnect(false);
                socket.Close(0);
            }
        }

        /// <summary>
        /// Waits for a local port to be listening on the Loopback or Any address.
        /// </summary>
        /// <param name="port">Port number</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <param name="delay">Time to wait in milliseconds to wait before checking again</param>
        /// <returns></returns>
        public static unsafe bool WaitForLocalPortListening(int port, int timeout, int delay) {
            DateTime endtime = DateTime.UtcNow.AddMilliseconds(timeout);
            int portBE = (port & 0xFF) << 8 | (port & 0xFF00) >> 8;  //Convert the port number to big endian

            int bufferSize = 1024;
            IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
            try {
                while (true) {
                    int result = 0;
                    while (122 == (result = NativeMethods.GetTcpTable(buffer, ref bufferSize, false))) {
                        Marshal.FreeHGlobal(buffer);
                        buffer = Marshal.AllocHGlobal(bufferSize);
                    }
                    if (result != 0)
                        throw new NetworkInformationException(result);

                    // Buffer MIB_TCPTABLE :
                    // int dwNumEntries 
                    // int dwState      //MIB_TCPROW entry 1
                    // int dwLocalAddr
                    // int dwLocalPort
                    // int dwRemoteAddr
                    // int dwRemotePort
                    // int dwState      //MIB_TCPROW entry 2
                    // ...

                    int* tcpTable = (int*)buffer;
                    int dwNumEntries = tcpTable[0];
                    int tcpTableLength = 1 + dwNumEntries * 5;

                    //loop over each entry to find the port (1 entry is 5 integers)
                    for (int i = 3; i < tcpTableLength; i += 5) {
                        int dwLocalPort = tcpTable[i];
                        if (dwLocalPort == portBE) {
                            int dwLocalAddr = tcpTable[i - 1];
                            if (dwLocalAddr == 0 || dwLocalAddr == 0x0100007f) { //Any or Loopback
                                int dwState = tcpTable[i - 2];
                                if (dwState == 2) // 2=listening
                                    return true;
                                break;
                            }
                        }
                    }

                    if (DateTime.UtcNow > endtime)
                        return false;

                    SysWaiter.Wait(delay);
                }
            } finally {
                Marshal.FreeHGlobal(buffer);
            }
        }

        static class NativeMethods {

            const string IPHLPAPI = "Iphlpapi.dll";
            const string KERNEL32 = "kernel32.dll";

            [DllImport(IPHLPAPI)]
            internal extern static int GetTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool order);

            [DllImport(KERNEL32)]
            internal static extern bool SetHandleInformation(IntPtr hObject, int dwMask, int dwFlags);

        }

    }

}

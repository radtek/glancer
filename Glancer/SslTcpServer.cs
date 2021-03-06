﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Threading.Tasks;

namespace Glancer
{
    public static class SERVER_REPONSE{
        public static string CONNECTION_ESTABLISHED = "{0} 200 Connection established{1}";
    }
    
    public class ConnectionInfo{
        public string host = string.Empty;
        public string port = string.Empty;
        public string protocol = string.Empty;
    }

    public sealed class SslTcpServer
    {
        public static X509Certificate _serverCertificate = null;
        int _lisstenPort;
        public static TraceLogger _traceLogger = new TraceLogger();
        public static bool _protocolLog { set; get; }
        public static bool _traceLog { set; get; }
        public string _protocolLogDir { set; get; }
        public string _traceLogDir { set; get; }
        public static int _read_timeout = 0;
        public static int _write_timeout = 0;

        public SslTcpServer(X509Certificate secret, int lisstenPort)
        {
            _serverCertificate = secret;
            _lisstenPort = lisstenPort;
        }

        public void RunServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, _lisstenPort);
            listener.Start();

            _traceLogger.InitLogger(_traceLogDir, true, true, "tcp.log");
            _traceLogger._isLogOutput = _traceLog;
            _traceLogger.OutputLog("RunServer().");
            _traceLogger.OutputLog("Listen Start.");

            while (true)
            {
                TcpClient serverSocket = listener.AcceptTcpClient();

                _traceLogger.OutputLog("AcceptTcpClient().");

                // Create TCP Session.
                string session = Guid.NewGuid().ToString();
                ServerProcess(serverSocket, session);
            }
        }

        async void ServerProcess(TcpClient serverSocket, string session)
        {
            await Task.Run(() => ServerProcessAction(serverSocket, session));
        }

        void ServerProcessAction(TcpClient serverSocket, string session)
        {
            try
            {
                // Create Event Listener.
                TraceLogger protocolLogger = new TraceLogger();
                protocolLogger.InitLogger(_protocolLogDir, false, false, session + ".log");
                protocolLogger._isLogOutput = _protocolLog;
                HttpEventListener listner = new HttpEventListener(protocolLogger);

                // Proxy work.
                HttpStreamProxy.Proxy(serverSocket, listner);

            }
            catch (Exception e)
            {
                _traceLogger.InputLog("Exception");
                _traceLogger.OutputLog(e.Message);
            }
        }
    }
}
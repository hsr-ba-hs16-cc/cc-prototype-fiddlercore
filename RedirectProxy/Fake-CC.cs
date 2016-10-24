namespace RediretProxy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Net.Sockets;
    using System.Net;
    using System.IO;
    using System.Threading;
    using System.Diagnostics;


    class SimpleFakeCC
    {
        private Thread _serverThread;
        private HttpListener _listener;
        private int _port, _sslPort;

        /// <summary>
        /// Construct server with given port.
        /// </summary>
        /// <param name="path">Directory path to serve.</param>
        /// <param name="port">Port of the server.</param>
        public SimpleFakeCC(int port, int sslPort)
        {
            this.Initialize(port, sslPort);
        }

        /// <summary>
        /// Stop server and dispose all functions.
        /// </summary>
        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }

        private void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
            _listener.Prefixes.Add("https://*:" + _sslPort.ToString() + "/");
            _listener.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private void Process(HttpListenerContext context)
        {
            //Adding permanent http response headers
            context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
            context.Response.AddHeader("Connection", "close");
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.Close();
        }

        private void Initialize(int port, int sslPort)
        {
            this._port = port;
            this._sslPort = sslPort;
            _serverThread = new Thread(this.Listen);
            _serverThread.Start();
        }


    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using Fiddler;
using System.Net;

namespace RediretProxy
{
    class Program
    {
        static Proxy oSecureEndpoint;
        static string sSecureEndpointHostname = "localhost";
        static int iSecureEndpointPort = 7777;
        static HashSet<String> filteredHosts = new HashSet<String>();

        public static void WriteCommandResponse(string s)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ForegroundColor = oldColor;
        }

        public static void DoQuit()
        {
            WriteCommandResponse("Shutting down...");
            if (null != oSecureEndpoint) oSecureEndpoint.Dispose();
            Fiddler.FiddlerApplication.Shutdown();
            Thread.Sleep(500);
        }
        private static string Ellipsize(string s, int iLen)
        {
            if (s.Length <= iLen) return s;
            return s.Substring(0, iLen - 3) + "...";
        }


        private static void WriteSessionList(List<Fiddler.Session> oAllSessions)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Session list contains...");
            try
            {
                Monitor.Enter(oAllSessions);
                foreach (Session oS in oAllSessions)
                {
                    Console.Write(String.Format("{0} {1} {2}\n{3} {4}\n\n", oS.id, oS.oRequest.headers.HTTPMethod, Ellipsize(oS.fullUrl, 60), oS.responseCode, oS.oResponse.MIMEType));
                }
            }
            finally
            {
                Monitor.Exit(oAllSessions);
            }
            Console.WriteLine();
            Console.ForegroundColor = oldColor;
        }

        static void Main(string[] args)
        {
            // Trust All Certificates, very insecure but for testing :)
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            SimpleFakeCC httpServer = new SimpleFakeCC(9080,9443);
            Client client = new Client();

            List<Fiddler.Session> oAllSessions = new List<Fiddler.Session>();
  
            // <-- Personalize for your Application, 64 chars or fewer
            Fiddler.FiddlerApplication.SetAppDisplayName("Redirect Proxy");

            #region AttachEventListeners

            Fiddler.FiddlerApplication.BeforeRequest += delegate (Fiddler.Session oS)
            {
                oS.bBufferResponse = false;
                
                Monitor.Enter(oAllSessions);
                oAllSessions.Add(oS);
                Monitor.Exit(oAllSessions);

                //oS.bypassGateway = true;
                Console.WriteLine("Request {0}:HTTP for Host: {1}", oS.id, oS.host);

                /*
                    Proxy Redirection
                */


                // Redirect HTTP Traffic according to Header Virus
                if (oS.HTTPMethodIs("CONNECT") && oS.oRequest.headers.Exists("Trojan")) {   oS.PathAndQuery = "127.0.0.1:9080";}
                if (oS.oRequest.headers.Exists("Trojan")) { filteredHosts.Add(oS.host); oS.host = "127.0.0.1:9080"; }

                //If already in FilteredHosts redirect Request -> Only HTTP
                if (oS.HTTPMethodIs("CONNECT") && filteredHosts.Contains(oS.host)) { oS.PathAndQuery = "127.0.0.1:9080";}
                if (filteredHosts.Contains(oS.host)) oS.host = "127.0.0.1:9080";

                //Redirect HTTPS Traffic
                if (oS.HTTPMethodIs("CONNECT") && (oS.PathAndQuery == "wikipedia.org:443")) { oS.PathAndQuery = "127.0.0.1:9443"; oS["X-OverrideCertCN"] = "redProxy";}
                if (oS.oRequest.headers.Exists("TrojanSecure")) oS.host = "127.0.0.1:9443";

            };

            
            Fiddler.FiddlerApplication.BeforeResponse += delegate(Fiddler.Session oS) {
                Console.WriteLine("Repsonse {0}:HTTP {1} for {2}", oS.id, oS.responseCode, oS.fullUrl);
            };


            Fiddler.FiddlerApplication.AfterSessionComplete += delegate (Fiddler.Session oS)
            {
                //Console.WriteLine("Finished session:\t" + oS.fullUrl); 
                Console.Title = ("Session list contains: " + oAllSessions.Count.ToString() + " sessions");
            };

            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            #endregion AttachEventListeners


            // TODO Make Right Certs to Trust
            Fiddler.CONFIG.IgnoreServerCertErrors = true;

            FiddlerCoreStartupFlags oFCSF = FiddlerCoreStartupFlags.Default;

            // NOTE: In the next line, you can pass 0 for the port (instead of 8877) to have FiddlerCore auto-select an available port
            int iPort = 8877;
            Fiddler.FiddlerApplication.Startup(iPort, oFCSF);

            Console.WriteLine("Hit CTRL+C to end session.");

            // We'll also create a HTTPS listener, useful for when FiddlerCore is masquerading as a HTTPS server
            // instead of acting as a normal CERN-style proxy server.
            oSecureEndpoint = FiddlerApplication.CreateProxyEndpoint(iSecureEndpointPort, true, sSecureEndpointHostname);
            if (null != oSecureEndpoint)
            {
                FiddlerApplication.Log.LogFormat("Created secure endpoint listening on port {0}, using a HTTPS certificate for '{1}'", iSecureEndpointPort, sSecureEndpointHostname);
            }
        }

        /// <summary>
        /// When the user hits CTRL+C, this event fires.  We use this to shut down and unregister our FiddlerCore.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            DoQuit();
        }
    }
}


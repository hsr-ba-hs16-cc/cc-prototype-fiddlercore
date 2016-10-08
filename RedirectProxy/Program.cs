﻿using System;
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
            //
            // It is important to understand that FiddlerCore calls event handlers on session-handling
            // background threads.  If you need to properly synchronize to the UI-thread (say, because
            // you're adding the sessions to a list view) you must call .Invoke on a delegate on the 
            // window handle.
            // 
            // If you are writing to a non-threadsafe data structure (e.g. List<t>) you must
            // use a Monitor or other mechanism to ensure safety.
            //

            // Simply echo notifications to the console.  Because Fiddler.CONFIG.QuietMode=true 
            // by default, we must handle notifying the user ourselves.
            Fiddler.FiddlerApplication.OnNotification += delegate (object sender, NotificationEventArgs oNEA) { Console.WriteLine("** NotifyUser: " + oNEA.NotifyString); };
            Fiddler.FiddlerApplication.Log.OnLogString += delegate (object sender, LogEventArgs oLEA) { Console.WriteLine("** LogString: " + oLEA.LogString); };

            Fiddler.FiddlerApplication.BeforeRequest += delegate (Fiddler.Session oS)
            {
                // Console.WriteLine("Before request for:\t" + oS.fullUrl);
                // In order to enable response tampering, buffering mode MUST
                // be enabled; this allows FiddlerCore to permit modification of
                // the response in the BeforeResponse handler rather than streaming
                // the response to the client as the response comes in.
                oS.bBufferResponse = false;
                Monitor.Enter(oAllSessions);
                oAllSessions.Add(oS);
                Monitor.Exit(oAllSessions);

                // Set this property if you want FiddlerCore to automatically authenticate by
                // answering Digest/Negotiate/NTLM/Kerberos challenges itself
                //oS["X-AutoAuth"] = "(default)";

                //oS.bypassGateway = true;


                /*if ((oS.oRequest.pipeClient.LocalPort == iSecureEndpointPort) && (oS.hostname == sSecureEndpointHostname))
                {    
                    oS.utilCreateResponseAndBypassServer();
                    oS.oResponse.headers.SetStatus(200, "Ok");
                    oS.oResponse["Content-Type"] = "text/html; charset=UTF-8";
                    oS.oResponse["Cache-Control"] = "private, max-age=0";
                    oS.utilSetResponseBody("<html><body>Request for httpS://" + sSecureEndpointHostname + ":" + iSecureEndpointPort.ToString() + " received. Your request was:<br /><plaintext>" + oS.oRequest.headers.ToString());
                }*/

                /*
                    Proxy Redirection
                */


                /*
                // Redirect HTTP Traffic according to Header Virus
                if (oS.HTTPMethodIs("CONNECT") && !oS.isHTTPS && oS.oRequest.headers.Exists("Trojan"))
                {
                    oS.PathAndQuery = "127.0.0.1:9080";
                }
                if (oS.oRequest.headers.Exists("Trojan") && !oS.isHTTPS) oS.host = "127.0.0.1:9080";
                */
                //Redirect HTTPS Traffic

                /*if (oS.HTTPMethodIs("CONNECT") && oS.oRequest.headers.Exists("TrojanSecure"))
                {
                    oS.PathAndQuery = "127.0.0.1:9443";
                }
                if (oS.RequestHeaders.Exists("TrojanSecure") && oS.isHTTPS) oS.host = "127.0.0.1:9443";
               */

                if (oS.HTTPMethodIs("CONNECT") &&  (oS.PathAndQuery == "wikipedia.org:443"))
                {
                    oS["OriginalHostname"] = oS.hostname;
                    oS.PathAndQuery = "127.0.0.1:9080";
                }

                if (oS.PathAndQuery == "wikipedia.org:80") oS.host = "127.0.0.1:9080";

                // If it's an HTTPS tunnel, override the certificate
                if (oS.HTTPMethodIs("CONNECT") && (null != oS["OriginalHostname"]))
                {
                    oS["x-overrideCertCN"] = oS["OriginalHostname"];
                    oS["X-IgnoreCertCNMismatch"] = "Server's hostname may not match what we're expecting...";
                }
                oS.bypassGateway = true;

            };

            
            Fiddler.FiddlerApplication.BeforeResponse += delegate(Fiddler.Session oS) {
                Console.WriteLine("{0}:HTTP {1} for {2}", oS.id, oS.responseCode, oS.fullUrl);
                
                // Uncomment the following two statements to decompress/unchunk the
                // HTTP response and subsequently modify any HTTP responses to replace 
                // instances of the word "Microsoft" with "Bayden". You MUST also
                // set bBufferResponse = true inside the beforeREQUEST method above.
                //
                //oS.utilDecodeResponse(); oS.utilReplaceInResponse("Microsoft", "Bayden");
            };


            Fiddler.FiddlerApplication.AfterSessionComplete += delegate (Fiddler.Session oS)
            {
                //Console.WriteLine("Finished session:\t" + oS.fullUrl); 
                Console.Title = ("Session list contains: " + oAllSessions.Count.ToString() + " sessions");
            };

            // Tell the system console to handle CTRL+C by calling our method that
            // gracefully shuts down the FiddlerCore.
            //
            // Note, this doesn't handle the case where the user closes the window with the close button.
            // See http://geekswithblogs.net/mrnat/archive/2004/09/23/11594.aspx for info on that...
            //
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            #endregion AttachEventListeners

            string sSAZInfo = "NoSAZ";

            Console.WriteLine(String.Format("Starting {0} ({1})...", Fiddler.FiddlerApplication.GetVersionString(), sSAZInfo));

            // TODO Make Right Certs to Trust
            Fiddler.CONFIG.IgnoreServerCertErrors = true;

            FiddlerCoreStartupFlags oFCSF = FiddlerCoreStartupFlags.Default;

            // E.g. If you want to add a flag, start with the .Default and "OR" the new flag on:
            // oFCSF = (oFCSF | FiddlerCoreStartupFlags.CaptureFTP);

            // ... or if you don't want a flag in the defaults, "and not" it out:
            // Uncomment the next line if you don't want FiddlerCore to act as the system proxy

            //**** WICHTIIG ****

            // oFCSF = (oFCSF & ~FiddlerCoreStartupFlags.RegisterAsSystemProxy);

            // *******************************
            // Important HTTPS Decryption Info
            // *******************************
            // When FiddlerCoreStartupFlags.DecryptSSL is enabled, you must include either
            //
            //     MakeCert.exe
            //
            // *or*
            //
            //     CertMaker.dll
            //     BCMakeCert.dll
            //
            // ... in the folder where your executable and FiddlerCore.dll live. These files
            // are needed to generate the self-signed certificates used to man-in-the-middle
            // secure traffic. MakeCert.exe uses Windows APIs to generate certificates which
            // are stored in the user's \Personal\ Certificates store. These certificates are
            // NOT compatible with iOS devices which require specific fields in the certificate
            // which are not set by MakeCert.exe. 
            //
            // In contrast, CertMaker.dll uses the BouncyCastle C# library (BCMakeCert.dll) to
            // generate new certificates from scratch. These certificates are stored in memory
            // only, and are compatible with iOS devices.

            // Uncomment the next line if you don't want to decrypt SSL traffic.
            // oFCSF = (oFCSF & ~FiddlerCoreStartupFlags.DecryptSSL);

            // NOTE: In the next line, you can pass 0 for the port (instead of 8877) to have FiddlerCore auto-select an available port
            int iPort = 8877;
            Fiddler.FiddlerApplication.Startup(iPort, oFCSF);

            FiddlerApplication.Log.LogFormat("Created endpoint listening on port {0}", iPort);

            FiddlerApplication.Log.LogFormat("Starting with settings: [{0}]", oFCSF);
            FiddlerApplication.Log.LogFormat("Gateway: {0}", CONFIG.UpstreamGateway.ToString());

            Console.WriteLine("Hit CTRL+C to end session.");

            // We'll also create a HTTPS listener, useful for when FiddlerCore is masquerading as a HTTPS server
            // instead of acting as a normal CERN-style proxy server.
            oSecureEndpoint = FiddlerApplication.CreateProxyEndpoint(iSecureEndpointPort, true, sSecureEndpointHostname);
            if (null != oSecureEndpoint)
            {
                FiddlerApplication.Log.LogFormat("Created secure endpoint listening on port {0}, using a HTTPS certificate for '{1}'", iSecureEndpointPort, sSecureEndpointHostname);
            }

            bool bDone = false;
            do
            {
                Console.WriteLine("\nEnter a command [C=Clear; L=List; G=Collect Garbage; W=write SAZ; R=read SAZ;\n\tS=Toggle Forgetful Streaming; T=Trust Root Certificate; Q=Quit]:");
                Console.Write(">");
                ConsoleKeyInfo cki = Console.ReadKey();
                Console.WriteLine();
                switch (Char.ToLower(cki.KeyChar))
                {
                    case 'c':
                        Monitor.Enter(oAllSessions);
                        oAllSessions.Clear();
                        Monitor.Exit(oAllSessions);
                        WriteCommandResponse("Clear...");
                        FiddlerApplication.Log.LogString("Cleared session list.");
                        break;

                    case 'd':
                        FiddlerApplication.Log.LogString("FiddlerApplication::Shutdown.");
                        FiddlerApplication.Shutdown();
                        break;

                    case 'l':
                        WriteSessionList(oAllSessions);
                        break;

                    case 'g':
                        Console.WriteLine("Working Set:\t" + Environment.WorkingSet.ToString("n0"));
                        Console.WriteLine("Begin GC...");
                        GC.Collect();
                        Console.WriteLine("GC Done.\nWorking Set:\t" + Environment.WorkingSet.ToString("n0"));
                        break;

                    case 'q':
                        bDone = true;
                        DoQuit();
                        break;

                    case 'r':
#if SAZ_SUPPORT
                        ReadSessions(oAllSessions);
#else
                        WriteCommandResponse("This demo was compiled without SAZ_SUPPORT defined");
#endif
                        break;

                    case 'w':
#if SAZ_SUPPORT
                        if (oAllSessions.Count > 0)
                        {
                            SaveSessionsToDesktop(oAllSessions);
                        }
                        else
                        {
                            WriteCommandResponse("No sessions have been captured");
                        }
#else
                        WriteCommandResponse("This demo was compiled without SAZ_SUPPORT defined");
#endif
                        break;

                    case 't':
                        try
                        {
                            WriteCommandResponse("Result: " + Fiddler.CertMaker.trustRootCert().ToString());
                        }
                        catch (Exception eX)
                        {
                            WriteCommandResponse("Failed: " + eX.ToString());
                        }
                        break;

                    // Forgetful streaming
                    case 's':
                        bool bForgetful = !FiddlerApplication.Prefs.GetBoolPref("fiddler.network.streaming.ForgetStreamedData", false);
                        FiddlerApplication.Prefs.SetBoolPref("fiddler.network.streaming.ForgetStreamedData", bForgetful);
                        Console.WriteLine(bForgetful ? "FiddlerCore will immediately dump streaming response data." : "FiddlerCore will keep a copy of streamed response data.");
                        break;

                }
            } while (!bDone);
        }

        /*
        /// <summary>
        /// This callback allows your code to evaluate the certificate for a site and optionally override default validation behavior for that certificate.
        /// You should not implement this method unless you understand why it is a security risk.
        /// </summary>
        static void CheckCert(object sender, ValidateServerCertificateEventArgs e)
        {
            if (null != e.ServerCertificate)
            {
                Console.WriteLine("Certificate for " + e.ExpectedCN + " was for site " + e.ServerCertificate.Subject + " and errors were " + e.CertificatePolicyErrors.ToString());

                if (e.ServerCertificate.Subject.Contains("fiddler2.com"))
                {
                    Console.WriteLine("Got a certificate for fiddler2.com. We'll say this is also good for any other site, like https://fiddlertool.com.");
                    e.ValidityState = CertificateValidity.ForceValid;
                }
            }
        }
        */

        /*
        // This event handler is called on every socket read for the HTTP Response. You almost certainly don't want
        // to add a handler for this event, but the code below shows how you can use it to mess up your HTTP traffic.
        static void FiddlerApplication_OnReadResponseBuffer(object sender, RawReadEventArgs e)
        {
            // NOTE: arrDataBuffer is a fixed-size array. Only bytes 0 to iCountOfBytes should be read/manipulated.
            //
            // Just for kicks, lowercase every byte. Note that this will obviously break any binary content.
            for (int i = 0; i < e.iCountOfBytes; i++)
            {
                if ((e.arrDataBuffer[i] > 0x40) && (e.arrDataBuffer[i] < 0x5b))
                {
                    e.arrDataBuffer[i] = (byte)(e.arrDataBuffer[i] + (byte)0x20);
                }
            }
            Console.WriteLine(String.Format("Read {0} response bytes for session {1}", e.iCountOfBytes, e.sessionOwner.id));
        }
        */

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


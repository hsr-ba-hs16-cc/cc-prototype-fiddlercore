using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;

class Client
{
    System.Timers.Timer virusTimer, randomTimer;
    private static readonly Random random = new Random();
    private static readonly HashSet<String> urls = new HashSet<String>();
    private static readonly HashSet<String> secureUrls = new HashSet<String>();
    //Add Client Handler ro use Fiddler Local Proxy
    HttpClientHandler handler = new HttpClientHandler
    {
        Proxy = new WebProxy("http://localhost:8877", false, new string[] { }),
        UseProxy = true,
    };
    public Client()
    {
        // Create a timer
        virusTimer = new System.Timers.Timer(8000);
        virusTimer.Elapsed += new ElapsedEventHandler(virusEvent);     
        virusTimer.Enabled = true;
        randomTimer = new System.Timers.Timer(9000);
        randomTimer.Elapsed += new ElapsedEventHandler(randomEvent);
        randomTimer.Enabled = true;
        //Add Urls to do Some Requests thanks to http://www.theuselessweb.com/ :)
        urls.Add("http://cant-not-tweet-this.com/");
        urls.Add("http://endless.horse/");
        urls.Add("http://ducksarethebest.com/");
        urls.Add("http://www.movenowthinklater.com/");
        urls.Add("http://weirdorconfusing.com/");
        urls.Add("http://www.trypap.com/");
        urls.Add("http://www.republiquedesmangues.fr/");
        urls.Add("http://beesbeesbees.com/");
        urls.Add("http://www.everydayim.com/");
        urls.Add("http://www.haneke.net/");
        //Now some Secure Urls more serious Things
        secureUrls.Add("https://www.linkedin.com/");
        secureUrls.Add("https://www.salt.ch");
        secureUrls.Add("https://outlook.com/");
        secureUrls.Add("https://www.microsoft.com");
        secureUrls.Add("https://slack.com/");
        secureUrls.Add("https://www.fiddler.com/");
        secureUrls.Add("https://about.gitlab.com/");
        secureUrls.Add("https://www.wikipedia.org/");
        secureUrls.Add("https://duckduckgo.com/");
        secureUrls.Add("https://wikileaks.com/");
    }


    private void virusEvent(object source, ElapsedEventArgs e) {
        Task.Factory.StartNew(() => sendRequests(false, "Trojan", "Watch Out"));
        Task.Factory.StartNew(() => sendRequests(true, "TrojanSecure", "Watch Out", "https://wikipedia.org"));
    }

    private void randomEvent(object source, ElapsedEventArgs e)
    {
        Task.Factory.StartNew(() => sendRequests());
        Task.Factory.StartNew(() => sendRequests(true));
    }

    async void sendRequests(Boolean secure = false,String Header = null, String HeaderContent = null,String page = null)
    {
        var client = new HttpClient(handler);
        String tempPage = "";
        //Get Random URLS
        if(page == null)
        {
            if (secure) { tempPage = secureUrls.ElementAt(random.Next(urls.Count)); }
            else { tempPage = urls.ElementAt(random.Next(urls.Count)); }
        }
        else { tempPage = page; }
        //Add Header
        if(Header != null){client.DefaultRequestHeaders.Add(Header, HeaderContent);}
        var response = await client.GetAsync(tempPage);
    }

}
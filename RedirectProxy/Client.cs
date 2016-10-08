using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;

class Client
{

    Task virus,virusSecure,virusHeaderless, randomTask, randomTaskSecure;
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
        virusTimer = new System.Timers.Timer(5000);
        virusTimer.Elapsed += new ElapsedEventHandler(virusEvent);     
        virusTimer.Enabled = true;
        //randomTimer = new System.Timers.Timer(5000);
        //randomTimer.Elapsed += new ElapsedEventHandler(randomEvent);
        //randomTimer.Enabled = true;
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
        //virus = new Task(SendVirusRequestHeader);
        //virus.Start();
        virusSecure = new Task(SendVirusRequestHeaderSecure);
        virusSecure.Start();
        //virusHeaderless = new Task(SendVirusWithoutHeader);
        //virusHeaderless.Start();
    }

    private void randomEvent(object source, ElapsedEventArgs e)
    {
        randomTask = new Task(SendRandomRequest);
        randomTask.Start();
        randomTaskSecure = new Task(SendRandomRequestSecure);
        randomTaskSecure.Start();
    }

    async void sendRequests(Boolean secure = false,String Header = null, String HeaderContent = null)
    {
        var client = new HttpClient(handler);
        //Get Random URLS
        if (secure) { String page = secureUrls.ElementAt(random.Next(urls.Count));}
        else { String page = urls.ElementAt(random.Next(urls.Count));}
        //Add Header
        if(secure && Header != null){client.DefaultRequestHeaders.Add("Virus", "Watch Out");} 
    }

    //Make a Request with a Virus HTTP Header
    async void SendVirusRequestHeader()
    {
        // ... Target page.
        String page = urls.ElementAt(random.Next(urls.Count));
        //Add Client Handler to use Fiddler Local Proxy
        // ... Use HttpClient.
        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("Trojan", "Watch Out");
        var response = await client.GetAsync(page);
    }

    //Send a HTTPS Request with Virus Header
    async void SendVirusRequestHeaderSecure()
    {
        // ... Target page.
        String page = "https://wikipedia.org";

        // ... Use HttpClient.
        HttpClient client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("TrojanSecure", "Watch Out");
        HttpResponseMessage response = await client.GetAsync(page);
    }

    async void SendVirusWithoutHeader()
    {
        // ... Target page.
        String page = secureUrls.ElementAt(random.Next(urls.Count));

        // ... Use HttpClient.
        HttpClient client = new HttpClient(handler);
        HttpResponseMessage response = await client.GetAsync(page);
    }

    async void SendRandomRequest()
    {
        String page = urls.ElementAt(random.Next(urls.Count));
        HttpClient client = new HttpClient(handler);
        HttpResponseMessage response = await client.GetAsync(page);
    }

    async void SendRandomRequestSecure()
    {
        String page = secureUrls.ElementAt(random.Next(secureUrls.Count));
        HttpClient client = new HttpClient(handler);
        HttpResponseMessage response = await client.GetAsync(page);
    }
}
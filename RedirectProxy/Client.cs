using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

class Client
{

   Task t;
   public Client()
    {
        t = new Task(SendVirusRequest);
        t.Start();
        // Add More Tasks (Timeout would also be cool)
    }

    static async void SendVirusRequest()
    {
        // ... Target page.
        string page = "http://en.wikipedia.org/";
        //Add Client Handler ro use Fiddler Local Proxy
        var handler = new HttpClientHandler
        {
            Proxy = new WebProxy("http://localhost:8877", false, new string[] { }),
            UseProxy = true,
        };

        // ... Use HttpClient.
        HttpClient client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("Virus", "Watch Out");
        HttpResponseMessage response = await client.GetAsync(page);
        Console.WriteLine(response);
        /*using (HttpContent content = response.Content)
        {
            // ... Read the string.
            string result = await content.ReadAsStringAsync();
            // ... Display the result.
            if (result != null &&
            result.Length >= 50)
            {
                Console.WriteLine(result.Substring(0, 50) + "...");
            }
        }*/
    }
}
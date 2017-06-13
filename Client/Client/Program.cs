using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client
{
    public class Program
    {
        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            var dicso = await DiscoveryClient.GetAsync("http://localhost:5000");

            var tokenClient = new TokenClient(dicso.TokenEndpoint, "client", "secret");
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1");

            if (tokenResponse.IsError) {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");
            await NewMethod(tokenResponse);

            Console.WriteLine("Press any key to make an API Call using ResourceOwner Credentials");
            Console.Read();

            tokenClient = new TokenClient(dicso.TokenEndpoint, "ro.client", "secret");
            tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("testuser1", "$abcd1234", "api1");

            if (tokenResponse.IsError) {
                Console.Write(tokenResponse.Error);
                return;
            }

            Console.WriteLine("New Token is : ");
            Console.Write(tokenResponse.Json);
            Console.WriteLine("\n\n");
            await NewMethod(tokenResponse);

            Console.WriteLine("Press any key to exit");
            Console.Read();
        }

        private static async Task NewMethod(TokenResponse tokenResponse)
        {
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var response = await client.GetAsync("http://localhost:5001/identity");
            if (!response.IsSuccessStatusCode) {
                Console.WriteLine(response.StatusCode);
            } else {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
        }
    }
}
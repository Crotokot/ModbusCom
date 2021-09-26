using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;


namespace ModbusCom
{
    class PostQueryMaker
    {
        public static void PostQuery(string address, 
            Dictionary<string, string> queryParams)
        {
            var ans = GetRequest(address, queryParams)?.Result;
            if (ans != null)
                Console.WriteLine(ans.RequestMessage);
        }

        private static async Task<HttpResponseMessage> 
            GetRequest(string address, Dictionary<string, string> queryParams)
        {
            HttpClient client = new HttpClient();

            try
            {
                FormUrlEncodedContent content = new FormUrlEncodedContent(queryParams);
                return await client.PostAsync(address, content);
            }
            catch (Exception)
            {
                Console.WriteLine("PostRequest: Anything was wrong!");
            }
            finally
            {
                client.Dispose();
            }
            return null;
        }
    }
}

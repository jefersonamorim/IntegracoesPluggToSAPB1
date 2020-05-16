using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Configuration;
using System.Net.Http.Headers;

namespace IntegracoesPluggto.Service
{
    
    class BaseService
    {
        public static HttpClient BuildClient()
        {
            string baseUriPedido = ConfigurationManager.AppSettings["api"];

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(baseUriPedido);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json");

            return client;
        }
    }
}

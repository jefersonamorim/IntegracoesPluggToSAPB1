
using IntegracoesPluggto.Entity;
using IntegracoesPluggto.Service;
using IntegracoesPluggto.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IntegracoesPluggto.Business
{
    class Repositorio : BaseService
    {
        private Log log;

        internal Repositorio() {
            this.log = new Log();
        }

        public async Task<HttpResponseMessage> RecuperarPedidos()
        {
            try
            {
                string _paramToken = "?access_token=" + ConfigurationManager.AppSettings["access_token"];

                var dataAnterior = DateTime.Now.AddDays(-Convert.ToDouble(ConfigurationManager.AppSettings["qtdDiasRecuperar"]));

                var dataAtual = DateTime.Now;

                string filter = "&created="+ dataAnterior.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "to"+dataAtual.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)+"&status=approved&orderby=created&orderdirection=asc&limit=100";

                string uriRequestOrder = "orders" + _paramToken + filter;

                HttpResponseMessage responseOrder = await BuildClient().GetAsync(uriRequestOrder);

                return responseOrder;
            }
            catch (Exception e)
            {
                this.log.WriteLogPedido("Exception RecuperarPedidos "+ e.InnerException.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> RecuperarPedidoById(string orderId)
        {
            try
            {
                string _paramToken = "?access_token=" + ConfigurationManager.AppSettings["access_token"];

                string uriRequest = "orders/"+ orderId + _paramToken;

                HttpResponseMessage responseOrder = await BuildClient().GetAsync(uriRequest);

                return responseOrder;
            }
            catch (Exception e)
            {
                this.log.WriteLogPedido("Excpetion RecuperarPedidoById " + e.InnerException.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> AtualizarStatusPedido(string orderId) {
            try
            {
                string _paramToken = "?access_token=" + ConfigurationManager.AppSettings["access_token"];

                string uriRequest = "orders/" + orderId + _paramToken;

                var bodyUpdateOrder = new {
                    status = "waiting_invoice"
                };

                HttpResponseMessage response = await BuildClient().PutAsync(uriRequest, new StringContent(JsonConvert.SerializeObject(bodyUpdateOrder), UnicodeEncoding.UTF8, "application/json"));

                return response;
            }
            catch (Exception e)
            {
                this.log.WriteLogPedido("Excpetion AtualizarStatusPedido "+e.InnerException.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> EnvioInfoNF(Order pedido, string nfe_key, string nfe_number, string nfe_serie, string nfe_date)
        {
            try
            {
                string _paramToken = "?access_token=" + ConfigurationManager.AppSettings["access_token"];

                string uriRequest = "orders/" + pedido.id + _paramToken;

                string shippmentId = string.Empty;

                foreach (Shipment shippment in pedido.shipments)
                {
                    if (!string.IsNullOrEmpty(shippment.id))
                    {
                        shippmentId = shippment.id;
                    }
                }

                var objShippment = new
                {
                    id = shippmentId,
                    status = "shipped",
                    date_shipped = nfe_date,
                    nfe_key,
                    nfe_number,
                    nfe_serie,
                    nfe_date
                };

                var listShippment = new List<Object>();
                listShippment.Add(objShippment);

                var jsonInformacoesNF = new {
                    status = "shipping_informed",
                    shipments = listShippment
                };

                HttpResponseMessage response = await BuildClient().PutAsync(uriRequest, new StringContent(JsonConvert.SerializeObject(jsonInformacoesNF), UnicodeEncoding.UTF8, "application/json"));

                return response;
            }
            catch (Exception e)
            {
                this.log.WriteLogPedido("Excpetion AtualizarStatusPedido " + e.InnerException.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> RecuperarItemPorSKU(string _sku)
        {
            try
            {
                string _paramToken = "?access_token=" + ConfigurationManager.AppSettings["access_token"];

                string uriItemSku = "skus/" + _sku + _paramToken;

                HttpResponseMessage responseOrder = await BuildClient().GetAsync(uriItemSku);

                return responseOrder;
            }
            catch (HttpRequestException e)
            {
                this.log.WriteLogEstoque("Exception RecuperarItemPorSKUSAP " + e.InnerException.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> AtualizarQuantidadeEstoque(string _skuId, int qtd)
        {
            try
            {
                string _paramToken = "?access_token=" + ConfigurationManager.AppSettings["access_token"];

                string uriAttEstoque = "skus/" + _skuId +"/stock"+ _paramToken;

                var updateStock = new
                {
                    action      = "update",
                    quantity    = qtd,
                    sku         = _skuId
                };

                HttpResponseMessage response = await BuildClient().PutAsync(uriAttEstoque, new StringContent(JsonConvert.SerializeObject(updateStock), UnicodeEncoding.UTF8, "application/json")); ;

                return response;
            }
            catch (HttpRequestException e)
            {
                this.log.WriteLogEstoque("Exception AtualizarQuantidadeEstoque " + e.InnerException.Message);
                throw;
            }
        }


    }
}

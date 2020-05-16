
using IntegracoesPluggto.DAL;
using IntegracoesPluggto.Entity;
using IntegracoesPluggto.Util;
using Newtonsoft.Json;
using RestSharp;
using SAPbobsCOM;
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
    public class IntegracaoService
    {
        private Log log;

        public IntegracaoService()
        {
            this.log = new Log();
        }

        public void GetNewAccessToken()
        {
            var client = new RestClient("https://api.plugg.to/oauth/token");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", "grant_type=password&client_id="+ConfigurationManager.AppSettings["client_id"] + "&client_secret=" + ConfigurationManager.AppSettings["client_secret"] + "&username=" + ConfigurationManager.AppSettings["user"] + "&password=" + ConfigurationManager.AppSettings["password"], ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseAccessToken = JsonConvert.DeserializeObject<AccessToken>(response.Content);

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["token_type"].Value = responseAccessToken.token_type;
                config.AppSettings.Settings["access_token"].Value = responseAccessToken.access_token;
                config.AppSettings.Settings["expires_in"].Value = responseAccessToken.expires_in.ToString();
                config.AppSettings.Settings["refresh_token"].Value = responseAccessToken.refresh_token;

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

                log.WriteLogPedido("Access Token para acesso à API gerado com sucesso.");
                log.WriteLogEstoque("Access Token para acesso à API gerado com sucesso.");
                log.WriteLogRetornoNF("Access Token para acesso à API gerado com sucesso.");
            }
            else
            {
                log.WriteLogPedido("Não foi possível obter o Access Token");
                log.WriteLogEstoque("Não foi possível obter o Access Token");
                log.WriteLogRetornoNF("Não foi possível obter o Access Token");
            }

        }

        public void IniciarIntegracaoEstoque(SAPbobsCOM.Company oCompany)
        {
            try
            {
                Repositorio repositorio = new Repositorio();

                WarehouseDAL whsDAL = new WarehouseDAL();

                SAPbobsCOM.Recordset recordset = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                recordset = whsDAL.RecuperarSaldoEstoqueSAP(oCompany);

                if (recordset != null && recordset.RecordCount > 0)
                {
                    for (int i = 0; i < recordset.RecordCount; i++)
                    {

                        try
                        {
                            string _itemCode = recordset.Fields.Item("ItemCode").Value.ToString();
                            Int16 _onHand = System.Convert.ToInt16(recordset.Fields.Item("OnHand").Value.ToString());

                            Task<HttpResponseMessage> itemResp = repositorio.RecuperarItemPorSKU(_itemCode);

                            if (itemResp.Result.IsSuccessStatusCode)
                            {
                                Task<HttpResponseMessage> responseAttEstoque = repositorio.AtualizarQuantidadeEstoque(_itemCode, _onHand);

                                if (responseAttEstoque.Result.IsSuccessStatusCode)
                                {
                                    this.log.WriteLogTable(oCompany, EnumTipoIntegracao.Estoque, _itemCode, _itemCode, EnumStatusIntegracao.Sucesso, "Estoque atualizado com sucesso.");
                                    this.log.WriteLogEstoque("Quantidade de estoque do Produto " + _itemCode + " atualizada com sucesso.");
                                }
                                else if (responseAttEstoque.Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                {
                                    this.GetNewAccessToken();
                                }
                            }
                            else if (itemResp.Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                this.GetNewAccessToken();
                            }

                        }
                        catch (Exception)
                        {
                            throw;
                        }

                        recordset.MoveNext();
                    }
                }

                if (recordset != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(recordset);
                }
            }
            catch (Exception e)
            {
                this.log.WriteLogTable(oCompany, EnumTipoIntegracao.Estoque, "","", EnumStatusIntegracao.Erro, e.Message);
                this.log.WriteLogEstoque("Exception IntegracaoService.IniciarIntegracaoEstoque " + e.Message);
            }
        }

        public void IniciarIntegracaoPedido(SAPbobsCOM.Company oCompany) {
            try
            {
                Repositorio repositorio = new Repositorio();

                var jsonSerializeconfig = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                Task<HttpResponseMessage> responseOrdersFiltered = repositorio.RecuperarPedidos();

                if (responseOrdersFiltered.Result.IsSuccessStatusCode)
                {
                    var jsonOrderFiltered = responseOrdersFiltered.Result.Content.ReadAsStringAsync().Result;

                    OrderFiltered orderFiltered = JsonConvert.DeserializeObject<OrderFiltered>(jsonOrderFiltered, jsonSerializeconfig);

                    if (orderFiltered.total > 0)
                    {

                        foreach (var orderResult in orderFiltered.result)
                        {
                            if (orderResult.order.status.Equals("approved"))
                            {
                                //recuperar pedido pluggto
                                Task<HttpResponseMessage> responseOrder = repositorio.RecuperarPedidoById(orderResult.order.id);

                                if (responseOrder.Result.IsSuccessStatusCode)
                                {
                                    var jsonOrder = responseOrder.Result.Content.ReadAsStringAsync().Result;

                                    OrderPluggto pedido = JsonConvert.DeserializeObject<OrderPluggto>(jsonOrder, jsonSerializeconfig);

                                    //integrar cliente
                                    this.InserirCliente(oCompany, pedido);

                                    //inserir pedido pluggto
                                    this.InserirPedidoVenda(oCompany, pedido);

                                    //Atualizar status do pedido para waiting_invoice
                                    Task<HttpResponseMessage> responseAttStatusPedido = repositorio.AtualizarStatusPedido(pedido.Order.id);

                                    if (!responseAttStatusPedido.Result.IsSuccessStatusCode)
                                    {
                                        //logar não foi possível atualizar nota do pedido
                                        this.log.WriteLogTable(oCompany, EnumTipoIntegracao.PedidoVenda, pedido.Order.id, "", EnumStatusIntegracao.Erro, "Não foi possível atualizar status do pedido.");
                                        this.log.WriteLogPedido("Não foi possível atualizar status do pedido " + pedido.Order.id + " Erro:" + responseAttStatusPedido.Result.ReasonPhrase);
                                    }
                                    else if (responseAttStatusPedido.Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                    {
                                        this.GetNewAccessToken();
                                    }

                                }
                                else if (responseOrder.Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                {
                                    this.GetNewAccessToken();
                                }
                            }
                        }
                    }

                }
                else if (responseOrdersFiltered.Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    this.GetNewAccessToken();
                }
            }
            catch (Exception e)
            {
                this.log.WriteLogPedido("Exception IntegracaoService.IniciarIntegracaoPedido - Erro: "+e.Message);
            }
        }

        private void InserirCliente(SAPbobsCOM.Company company, OrderPluggto pedidoPluggto)
        {
            try
            {
                BusinessPartnersDAL bpDAL = new BusinessPartnersDAL();

                string errorMessage;

                bpDAL.InserirBusinessPartner(company, pedidoPluggto, out errorMessage);
            }
            catch (Exception e)
            {
                this.log.WriteLogPedido("Exception inserirClientes " + e.Message);
                throw;
            }
        }

        private int InserirPedidoVenda(SAPbobsCOM.Company oCompany, OrderPluggto pedidoPluggto)
        {
            try
            {
                if (oCompany.Connected)
                {
                    OrdersDAL orderDAL = new OrdersDAL(oCompany);
                    string messageError = "";
                    int oOrderNum = 0;
                    Boolean inserir = true;

                    foreach (Item item in pedidoPluggto.Order.items)
                    {
                        if (string.IsNullOrEmpty(item.sku) && inserir)
                        {
                            this.log.WriteLogTable(oCompany, EnumTipoIntegracao.PedidoVenda, pedidoPluggto.Order.id, "", EnumStatusIntegracao.Erro, "Um ou mais item(s) do pedido está com o código de referência inválido.");
                            this.log.WriteLogPedido("Um ou mais item(s) do pedido está com o código de referência inválido.");
                            inserir = false;
                        }
                    }

                    if (inserir)
                    {
                        oOrderNum = orderDAL.InsertOrder(pedidoPluggto.Order, out messageError);

                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                this.log.WriteLogPedido("Exception InserirPedidoVenda " + e.Message);
                throw;
            }
        }

        public void IniciarIntegracaoRetornoNF(SAPbobsCOM.Company oCompany)
        {
            try
            {
                Repositorio repositorio = new Repositorio();

                var jsonSerializeconfig = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                int contador = 0;

                if (oCompany.Connected)
                {
                    OrdersDAL orders = new OrdersDAL(oCompany);

                    SAPbobsCOM.Recordset recordSet = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);

                    recordSet = orders.RecuperarNumeroNF();

                    if (recordSet.RecordCount > 0)
                    {
                        for (int i = 0; i < recordSet.RecordCount; i++)
                        {
                            contador = recordSet.RecordCount;

                            this.log.WriteLogRetornoNF("Existem "+contador +"NF's a serem retornadas.");

                            string nfKey = recordSet.Fields.Item("nfeKey").Value.ToString();
                            string docSAP = recordSet.Fields.Item("docSAP").Value.ToString();
                            string externalId = recordSet.Fields.Item("externalId").Value.ToString();

                            string idOrderPluggto = recordSet.Fields.Item("idOrderIntegracoesPluggto").Value.ToString();
                            string idOrderPluggto2 = recordSet.Fields.Item("idOrderIntegracoesPluggto2").Value.ToString();

                            string invoiceNumber = recordSet.Fields.Item("invoiceNumber").Value.ToString();

                            string invoiceOrderSeries = recordSet.Fields.Item("invoiceOrderSeries").Value.ToString();

                            string invoiceDate = recordSet.Fields.Item("invoiceDate").Value.ToString().Split(' ').FirstOrDefault();
                            CultureInfo provider = CultureInfo.InvariantCulture;
                            DateTime invoiceDt = DateTime.ParseExact(invoiceDate, "dd/MM/yyyy", provider);

                            invoiceDate = invoiceDt.ToString("yyyy-MM-dd");

                            Task<HttpResponseMessage> responseOrder = repositorio.RecuperarPedidoById(!string.IsNullOrEmpty(idOrderPluggto) ? idOrderPluggto:idOrderPluggto2);

                            if (responseOrder.Result.IsSuccessStatusCode)
                            {
                                string jsonOrder = responseOrder.Result.Content.ReadAsStringAsync().Result;

                                OrderPluggto orderPluggto = JsonConvert.DeserializeObject<OrderPluggto>(jsonOrder, jsonSerializeconfig);

                                if (orderPluggto.Order.status.Equals("waiting_invoice"))
                                {
                                    //Enviar dados e atualizar status
                                    Task<HttpResponseMessage> responseEnvioNF = repositorio.EnvioInfoNF(orderPluggto.Order, nfKey, invoiceNumber, invoiceOrderSeries, invoiceDate);

                                    int updatePedidoNum1 = orders.AtualizarPedidoVenda(oCompany, Convert.ToInt32(externalId));
                                    if (responseEnvioNF.Result.IsSuccessStatusCode)
                                    {
                                        //atualizar Pedido Venda SAP
                                        //Atualizando campo de usuário U_EnvioNFVTEX
                                        int updatePedidoNum = orders.AtualizarPedidoVenda(oCompany, Convert.ToInt32(externalId));

                                        if (updatePedidoNum == 0)
                                        {
                                            this.log.WriteLogTable(oCompany, EnumTipoIntegracao.NF,idOrderPluggto, docSAP, EnumStatusIntegracao.Sucesso, "Número NF " + invoiceNumber + " enviado para Pluggto com sucesso.");
                                            this.log.WriteLogRetornoNF("Número NF para o Pedido de Venda " + docSAP + " enviado para Pluggto com sucesso.");
                                        }
                                        else
                                        {
                                            this.log.WriteLogTable(oCompany, EnumTipoIntegracao.NF, idOrderPluggto, docSAP, EnumStatusIntegracao.Erro, "Número NF " + invoiceNumber + " retornado porém não foi possivél atualizar campo de usuário (U_EnvioNFPluggto) do Pedido de Venda");
                                            this.log.WriteLogRetornoNF("Falha ao atualizar Pedido de Venda " + docSAP);
                                        }
                                    }
                                    else if (responseEnvioNF.Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                    {
                                        this.GetNewAccessToken();
                                    }
                                    else
                                    {
                                        var jsonResponseError = responseEnvioNF.Result.Content.ReadAsStringAsync().Result;

                                        var objResponse = JsonConvert.DeserializeObject<ResponseErrorRetNF>(jsonResponseError);

                                        this.log.WriteLogTable(oCompany, EnumTipoIntegracao.NF, idOrderPluggto, docSAP, EnumStatusIntegracao.Erro, objResponse.type + " " + objResponse.details);
                                        this.log.WriteLogRetornoNF(objResponse.type + " " + objResponse.details);
                                    }
                                }
                            }
                            else if (responseOrder.Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                this.GetNewAccessToken();
                            }

                            recordSet.MoveNext();
                        }
                    }
                    else
                    {
                        this.log.WriteLogRetornoNF("Não há nenhuma NF a ser retornada.");
                    }
                }
                else
                {
                    this.log.WriteLogRetornoNF("Não conectado ao SAP.");
                }
                
            }
            catch (Exception e)
            {
                this.log.WriteLogRetornoNF("Exception IntegracaoService.IniciarIntegracaoRetornoNF " + e.Message);
                throw;
            }
        }
    }
}

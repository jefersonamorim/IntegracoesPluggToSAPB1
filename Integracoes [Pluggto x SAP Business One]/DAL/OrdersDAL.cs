using IntegracoesPluggto.Entity;
using IntegracoesPluggto.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegracoesPluggto.DAL
{
    public class OrdersDAL
    {
        private SAPbobsCOM.Company oCompany;

        private Log log;
        internal OrdersDAL(SAPbobsCOM.Company company) {
            this.oCompany = company;
        }

        public int InsertOrder(Order pedido, out string messageError) {
            this.log = new Log();
            try
            {
                int oOrderNum = 0;

                log.WriteLogPedido("Inserindo Pedido de Venda "+pedido.id);

                SAPbobsCOM.Documents oOrder = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);

                int filial = Convert.ToInt32(ConfigurationManager.AppSettings["Empresa"]);
                string usage = ConfigurationManager.AppSettings["Usage"];
                string WhsCode = ConfigurationManager.AppSettings["WhsCode"];
                int SlpCode = Convert.ToInt32(ConfigurationManager.AppSettings["SlpCode"]);
                string comments = ConfigurationManager.AppSettings["Comments"];
                string plataforma = ConfigurationManager.AppSettings["Plataforma"];
                string carrier = ConfigurationManager.AppSettings["Carrier"];
                string packDesc = ConfigurationManager.AppSettings["PackDesc"];
                int qoP = Convert.ToInt32(ConfigurationManager.AppSettings["QoP"]);
                int expnsCode = Convert.ToInt32(ConfigurationManager.AppSettings["ExpnsCode"]);
                string expnsTax = ConfigurationManager.AppSettings["ExpnsTax"];
                string cardCodePrefix = ConfigurationManager.AppSettings["CardCodePrefix"];
                string pickRemark = ConfigurationManager.AppSettings["PickRemark"];
                string document = String.Empty;

                oOrder.BPL_IDAssignedToInvoice = filial;
                oOrder.NumAtCard = pedido.id;
                oOrder.SalesPersonCode = SlpCode;
                oOrder.Comments = comments;
                oOrder.UserFields.Fields.Item("U_PLATF").Value = plataforma;
                oOrder.UserFields.Fields.Item("U_NumPedEXT").Value = pedido.id;
                oOrder.TaxExtension.Carrier = carrier;
                oOrder.TaxExtension.PackDescription = packDesc;
                oOrder.TaxExtension.PackQuantity = qoP;
                oOrder.Expenses.ExpenseCode = expnsCode;
                oOrder.Expenses.TaxCode = expnsTax;

                if (!String.IsNullOrEmpty(pedido.payer_cpf))
                {
                    document = pedido.payer_cpf;
                }
                else if (!String.IsNullOrEmpty(pedido.payer_cnpj))
                {
                    document = pedido.payer_cnpj;
                }

                oOrder.CardCode = cardCodePrefix + document;

                if (pedido.expected_delivery_date != null)
                {
                    oOrder.DocDueDate = pedido.expected_delivery_date;
                }
                else
                {
                    oOrder.DocDueDate = DateTime.Today.AddDays(5);
                }

                oOrder.PickRemark = pickRemark;

                /*
                double _valorFrete = 0.00;
                double _valorDescont = 0.00;
                double _valorTaxa = 0.00;

                despesas adicionais
                if (pedido.totals.Length > 0)
                {
                    foreach (Total total in pedido.totals)
                    {
                        if (total.id.Equals("Discounts"))
                        {
                            if (total.value != 0)
                            {
                                _valorDescont = Convert.ToDouble(total.value.ToString().Insert(total.value.ToString().Length - 2, ","));
                            }
                        }
                        if (total.id.Equals("Shipping"))
                        {
                            if (total.value != 0)
                            {
                                _valorFrete = Convert.ToDouble(total.value.ToString().Insert(total.value.ToString().Length - 2, ","));
                            }
                        }
                        if (total.id.Equals("Tax"))
                        {
                            if (total.value != 0)
                            {
                                _valorTaxa = Convert.ToDouble(total.value.ToString().Insert(total.value.ToString().Length - 2, ","));
                            }
                        }
                    }
                }
                oOrder.Expenses.LineGross = _valorFrete;
                 */

                //DocumentLines
                if (pedido.items.Length > 0)
                {
                    //_valorFrete.ToString().Insert(1,".");
                    int _lineNum = 0;

                    foreach (Item item in pedido.items)
                    {
                        if (!String.IsNullOrEmpty(item.sku))
                        {
                            oOrder.Lines.ItemCode = item.sku;
                            oOrder.Lines.Quantity = item.quantity;
                            oOrder.Lines.WarehouseCode = WhsCode;
                            oOrder.Lines.Usage = usage;
                            oOrder.Lines.SetCurrentLine(_lineNum);
                            oOrder.Lines.Add();
                        }

                        _lineNum++;
                    }
                }

                oOrderNum = oOrder.Add();

                if (oOrderNum != 0)
                {
                    messageError = oCompany.GetLastErrorDescription();
                    log.WriteLogTable(oCompany, EnumTipoIntegracao.PedidoVenda, pedido.id, "", EnumStatusIntegracao.Erro, messageError);
                    log.WriteLogPedido("InsertOrder error SAP: " + messageError);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oOrder);
                    return oOrderNum;
                }
                else
                {
                    messageError = "";
                    string docNum = oCompany.GetNewObjectKey();
                    log.WriteLogTable(oCompany, EnumTipoIntegracao.PedidoVenda, pedido.id, docNum, EnumStatusIntegracao.Sucesso, "Pedido de venda inserido com sucesso.");
                    log.WriteLogPedido("Pedido de venda inserido com sucesso.");
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oOrder);
                    return oOrderNum;
                }


            }
            catch (Exception e)
            {
                log.WriteLogTable(oCompany, EnumTipoIntegracao.PedidoVenda, pedido.id, "", EnumStatusIntegracao.Erro, e.Message);
                log.WriteLogPedido("Excpetion InsertOrder. "+e.Message);

                throw;
            }
        }

        public SAPbobsCOM.Recordset RecuperarNumeroNF()
        {
            SAPbobsCOM.Recordset recordSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

            try
            {
                string _query = string.Empty;

                if (this.oCompany.Connected)
                {
                    recordSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                    _query = string.Format("SELECT " +
                            "T0.DocNum AS docNPIntegracoesPluggto " +
                            ",T0.NumAtCard AS idOrderIntegracoesPluggto " +
                            ", T0.U_NumPedEXT AS idOrderIntegracoesPluggto2 " +
                            ",T2.DocEntry AS externalId " +
                            ",T2.DocNum	AS docSAP " +
                            ",T2.Serial AS invoiceNumber " +
                            ",T2.DocDate AS invoiceDate " +
                            ",T3.KeyNfe AS nfeKey " +
                            ",T0.PickRmrk AS shippingMethod " +
                            ",T2.SeriesStr AS invoiceOrderSeries " +
                            ",T1.ItemCode AS codItem " +
                            ",T1.Price AS precoItem " +
                            ",T1.Quantity AS qtdItem " +
                            ",T0.DocTotal AS totalNF " +
                            "FROM    ORDR T0 " +
                            "INNER JOIN INV1 T1 ON T0.DocEntry = T1.BaseEntry  " +
                            "INNER JOIN OINV T2 ON T1.DocEntry = T2.DocEntry and T0.BPLId = T2.BPLId  " +
                            "INNER JOIN [DBInvOne].[dbo].[Process] T3 on T3.DocEntry = T2.DocEntry " +
                            "WHERE	T0.U_PLATF = '{0}' " +
                            "AND T0.U_EnvioNFPluggto IS NULL", ConfigurationManager.AppSettings["Plataforma"]);

                    recordSet.DoQuery(_query);

                    if (recordSet.RecordCount > 0)
                    {

                        return recordSet;
                    }
                }
            }
            catch (Exception e)
            {
                this.log = new Log();
                this.log.WriteLogEstoque("Exception recuperarSaldoEstoqueSAP " + e.Message);
                throw;
            }

            return recordSet;
        }

        public int AtualizarPedidoVenda(SAPbobsCOM.Company company, int docEntry) {
            this.log = new Log();
            try
            {
                this.oCompany = company;

                log.WriteLogRetornoNF("Atualizando Pedido de Venda - NF enviada p/ IntegracoesPluggto");

                SAPbobsCOM.Documents oInvoice = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

                if (oInvoice.GetByKey(docEntry))
                {
                    //SAPbobsCOM.Documents oOrderUpdate = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);
                    //oOrderUpdate = oOrder;

                    oInvoice.UserFields.Fields.Item("U_EnvioNFPluggto").Value = "S";

                    int updateOrderNum = oInvoice.Update();

                    if (updateOrderNum != 0)
                    {
                        string messageError = oCompany.GetLastErrorDescription();
                        log.WriteLogRetornoNF("AtualizarPedidoVenda error SAP: " + messageError);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oInvoice);
                        return 1;
                    }
                    else
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oInvoice);
                        return 0;
                    }
                }
                return 1;
            }
            catch (Exception)
            {
                return 1;
                throw;
            }
        }
    }
}

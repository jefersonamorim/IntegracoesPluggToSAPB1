using IntegracoesPluggto.Entity;
using IntegracoesPluggto.Util;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegracoesPluggto.DAL
{
    public class BusinessPartnersDAL
    {

        private SAPbobsCOM.Company oCompany;

        private Log log;

        internal BusinessPartnersDAL()
         {
            this.log = new Log();
            //this.oCompany = company;
        }

        public void InserirBusinessPartner(SAPbobsCOM.Company company, OrderPluggto pedido, out string messageError)
        {
            int addBPNumber = 0;

            string document = string.Empty;
            Boolean isCorporate = false;
            //Boolean marketPlace = false;

            if (!String.IsNullOrEmpty(pedido.Order.payer_cpf))
            {
                document = pedido.Order.payer_cpf;
            }
            else if (!String.IsNullOrEmpty(pedido.Order.payer_cnpj))
            {
                document = pedido.Order.payer_cnpj;
                isCorporate = true;
            }

            try
            {
                CountyDAL countyDAL = new CountyDAL();

                this.oCompany = company;

                int _groupCode = Convert.ToInt32(ConfigurationManager.AppSettings["GroupCode"]);
                int _splCode = Convert.ToInt32(ConfigurationManager.AppSettings["SlpCode"]);
                int _QoP = Convert.ToInt32(ConfigurationManager.AppSettings["QoP"]);
                int groupNum = Convert.ToInt32(ConfigurationManager.AppSettings["GroupNum"]);
                string indicadorIE = ConfigurationManager.AppSettings["IndicadorIE"];
                string indicadorOpConsumidor = ConfigurationManager.AppSettings["IndicadorOpConsumidor"];
                string gerente = ConfigurationManager.AppSettings["Gerente"];
                int priceList = Convert.ToInt32(ConfigurationManager.AppSettings["PriceList"]);
                string cardCodePrefix = ConfigurationManager.AppSettings["CardCodePrefix"];
                int categoriaCliente = Convert.ToInt32(ConfigurationManager.AppSettings["CategoriaCliente"]);
                
                this.log.WriteLogPedido("Inserindo Cliente " + cardCodePrefix + document);

                BusinessPartners oBusinessPartner = null;
                oBusinessPartner = (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(BoObjectTypes.oBusinessPartners);

                BusinessPartners oBusinessPartnerUpdateTest = null;
                oBusinessPartnerUpdateTest = (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(BoObjectTypes.oBusinessPartners);

                if (oBusinessPartnerUpdateTest.GetByKey(cardCodePrefix + document))
                {
                    oBusinessPartner = oBusinessPartnerUpdateTest;
                }

                //Setando campos padrões
                oBusinessPartner.CardCode = cardCodePrefix + document;

                if (isCorporate)
                {
                    oBusinessPartner.CardForeignName = pedido.Order.payer_razao_social;
                }

                oBusinessPartner.CardName = pedido.Order.receiver_name + " " + pedido.Order.receiver_lastname;

                oBusinessPartner.EmailAddress = pedido.Order.receiver_email;
                
                oBusinessPartner.CardType = BoCardTypes.cCustomer;
                oBusinessPartner.GroupCode = _groupCode;
                oBusinessPartner.SalesPersonCode = _splCode;
                oBusinessPartner.PayTermsGrpCode = groupNum;
                oBusinessPartner.PriceListNum = priceList;
                //oBusinessPartner.CardForeignName = "Teste";

                //Setando campos de usuário
                oBusinessPartner.UserFields.Fields.Item("U_TX_IndIEDest").Value = indicadorIE;
                oBusinessPartner.UserFields.Fields.Item("U_TX_IndFinal").Value = indicadorOpConsumidor;
                oBusinessPartner.UserFields.Fields.Item("U_Gerente").Value = gerente;
                oBusinessPartner.UserFields.Fields.Item("U_CategoriaCliente").Value = gerente;

                if (!string.IsNullOrEmpty(pedido.Order.receiver_phone_area))
                {
                    oBusinessPartner.Phone2 = pedido.Order.receiver_phone_area;
                }

                //removendo o ddd
                if (!String.IsNullOrEmpty(pedido.Order.receiver_phone))
                {
                    if (pedido.Order.receiver_phone.Length >= 9)
                    {
                        oBusinessPartner.Cellular = pedido.Order.receiver_phone.Substring(2);
                    }
                    else
                    {
                        oBusinessPartner.Cellular = pedido.Order.receiver_phone;
                    }
                   
                }
                else if (!String.IsNullOrEmpty(pedido.Order.receiver_phone2))
                {
                    if (pedido.Order.receiver_phone2.Length >= 9 )
                    {
                        //oBusinessPartner.Phone1 = cliente.homePhone.Substring(2);
                        oBusinessPartner.Phone1 = pedido.Order.receiver_phone2.Substring(2);
                    }
                    else
                    {
                        //oBusinessPartner.Phone1 = cliente.homePhone.Substring(2);
                        oBusinessPartner.Phone1 = pedido.Order.receiver_phone2;
                    }
                }

                string codMunicipio = string.Empty;

                codMunicipio = countyDAL.RecuperarCodigoMunicipio(pedido.Order.receiver_city, this.oCompany);

                //Adicionando endereços
                //COBRANÇA
                oBusinessPartner.Addresses.SetCurrentLine(0);
                oBusinessPartner.Addresses.AddressType = BoAddressType.bo_BillTo;
                oBusinessPartner.Addresses.AddressName = "COBRANCA";

                oBusinessPartner.Addresses.City = pedido.Order.receiver_city;

                if (!String.IsNullOrEmpty(pedido.Order.receiver_address_complement) && pedido.Order.receiver_address_complement.Length <= 100)
                {
                    oBusinessPartner.Addresses.BuildingFloorRoom = pedido.Order.receiver_address_complement;
                }

                //oBusinessPartner.Addresses.Country = "1058";
                oBusinessPartner.Addresses.Block = pedido.Order.receiver_neighborhood;
                oBusinessPartner.Addresses.StreetNo = pedido.Order.receiver_address_number;
                oBusinessPartner.Addresses.ZipCode = pedido.Order.receiver_zipcode;
                oBusinessPartner.Addresses.State = pedido.Order.receiver_state;
                oBusinessPartner.Addresses.Street = pedido.Order.receiver_address;
                oBusinessPartner.Addresses.County = codMunicipio;
                //oBusinessPartner.Addresses.Country = "br";

                oBusinessPartner.Addresses.Add();


                //FATURAMENTO
                oBusinessPartner.Addresses.SetCurrentLine(1);
                oBusinessPartner.Addresses.AddressType = BoAddressType.bo_ShipTo;
                oBusinessPartner.Addresses.AddressName = "FATURAMENTO";

                oBusinessPartner.Addresses.City = pedido.Order.receiver_city;

                if (!String.IsNullOrEmpty(pedido.Order.receiver_address_complement) && pedido.Order.receiver_address_complement.Length <= 100)
                {
                    oBusinessPartner.Addresses.BuildingFloorRoom = pedido.Order.receiver_address_complement;
                }

                //oBusinessPartner.Addresses.Country = "1058";
                oBusinessPartner.Addresses.Block = pedido.Order.receiver_neighborhood;
                oBusinessPartner.Addresses.StreetNo = pedido.Order.receiver_address_number;
                oBusinessPartner.Addresses.ZipCode = pedido.Order.receiver_zipcode;
                oBusinessPartner.Addresses.State = pedido.Order.receiver_state;
                oBusinessPartner.Addresses.Street = pedido.Order.receiver_address;
                oBusinessPartner.Addresses.County = codMunicipio;
                //oBusinessPartner.Addresses.Country = "br";

                oBusinessPartner.Addresses.Add();

                oBusinessPartner.FiscalTaxID.Address = "FATURAMENTO";

                if (isCorporate)
                {
                    oBusinessPartner.FiscalTaxID.TaxId0 = document;
                }
                else
                {
                    oBusinessPartner.FiscalTaxID.TaxId4 = document;
                    oBusinessPartner.FiscalTaxID.TaxId1 = "Isento";
                }

                oBusinessPartner.FiscalTaxID.Add();



                #region ENDEREÇO FOR

                /*for (int i = 0; i < 2; i++)
                {
                    if (i > 0)
                    {
                        oBusinessPartner.Addresses.SetCurrentLine(i);
                        oBusinessPartner.Addresses.AddressType = BoAddressType.bo_ShipTo;
                        oBusinessPartner.Addresses.AddressName = "FATURAMENTO";
                    }
                    else
                    {
                        oBusinessPartner.Addresses.SetCurrentLine(i);
                        oBusinessPartner.Addresses.AddressType = BoAddressType.bo_BillTo;
                        oBusinessPartner.Addresses.AddressName = "COBRANCA";

                        if (!oBusinessPartnerUpdateTest.GetByKey(cardCodePrefix + document))
                        {
                            oBusinessPartner.Addresses.Add();
                        }
                    }

                    oBusinessPartner.Addresses.City = pedido.Order.receiver_city;

                    if (!String.IsNullOrEmpty(pedido.Order.receiver_address_complement) && pedido.Order.receiver_address_complement.Length <= 100)
                    {
                        oBusinessPartner.Addresses.BuildingFloorRoom = pedido.Order.receiver_address_complement;
                    }

                    //oBusinessPartner.Addresses.Country = "1058";
                    oBusinessPartner.Addresses.Block = pedido.Order.receiver_neighborhood;
                    oBusinessPartner.Addresses.StreetNo = pedido.Order.receiver_address_number;
                    oBusinessPartner.Addresses.ZipCode = pedido.Order.receiver_zipcode;
                    oBusinessPartner.Addresses.State = pedido.Order.receiver_state;
                    oBusinessPartner.Addresses.Street = pedido.Order.receiver_address;
                    oBusinessPartner.Addresses.County = codMunicipio;
                    //oBusinessPartner.Addresses.Country = "br";

                    oBusinessPartner.Addresses.Add();
                }*/
                #endregion

                oBusinessPartner.BilltoDefault = "COBRANCA";
                oBusinessPartner.ShipToDefault = "FATURAMENTO";

                BusinessPartners oBusinessPartnerUpdate = null;
                oBusinessPartnerUpdate = (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(BoObjectTypes.oBusinessPartners);

                if (oBusinessPartnerUpdate.GetByKey(cardCodePrefix + document))
                {
                    addBPNumber = oBusinessPartner.Update();

                    if (addBPNumber != 0)
                    {
                        messageError = oCompany.GetLastErrorDescription();
                        this.log.WriteLogTable(oCompany, EnumTipoIntegracao.Cliente, document, cardCodePrefix+document, EnumStatusIntegracao.Erro, messageError);
                    }
                    else
                    {
                        messageError = "";
                        this.log.WriteLogTable(oCompany,EnumTipoIntegracao.Cliente,document, cardCodePrefix + document,EnumStatusIntegracao.Sucesso,"Cliente atualizado com sucesso.");

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oBusinessPartner);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oBusinessPartnerUpdate);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oBusinessPartnerUpdateTest);
                    }

                }
                else
                {
                    //Setando informações Fiscais
                    //oBusinessPartner.FiscalTaxID.SetCurrentLine(0);
                    /*if (isCorporate)
                    {
                        oBusinessPartner.FiscalTaxID.TaxId0 = document;
                    }
                    else {

                        oBusinessPartner.FiscalTaxID.TaxId4 = document;
                        oBusinessPartner.FiscalTaxID.TaxId1 = "Isento";
                    }*/
                    //oBusinessPartner.FiscalTaxID.Address = "FATURAMENTO";
                    //oBusinessPartner.FiscalTaxID.Add();

                    addBPNumber = oBusinessPartner.Add();

                    if (addBPNumber != 0)
                    {
                        messageError = oCompany.GetLastErrorDescription();
                        this.log.WriteLogTable(oCompany, EnumTipoIntegracao.Cliente, document, "", EnumStatusIntegracao.Erro, messageError);
                        this.log.WriteLogPedido("Falha ao inserir cliente - "+messageError);
                    }
                    else
                    {
                        string CardCode = oCompany.GetNewObjectKey();
                        this.log.WriteLogTable(oCompany, EnumTipoIntegracao.Cliente, document, CardCode, EnumStatusIntegracao.Sucesso, "Cliente inserido com sucesso.");
                        this.log.WriteLogPedido("Cliente "+CardCode+" inserido com sucesso.");
                        messageError = "";
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oBusinessPartner);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oBusinessPartnerUpdateTest);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oBusinessPartnerUpdate);
            }
            catch (Exception e)
            {
                this.log.WriteLogTable(oCompany, EnumTipoIntegracao.Cliente, document, "", EnumStatusIntegracao.Erro, e.Message);
                this.log.WriteLogPedido("InserirBusinessPartner Exception: " + e.Message);
                throw;
            }

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegracoesPluggto.Entity
{
    public class OrderPluggto
    {
        public Order Order { get; set; }
    }

    public class Order
    {
        public Tags tags { get; set; }
        public Commission commission { get; set; }
        public object[] status_history { get; set; }
        public object[] substatus_history { get; set; }
        public object[] log_history { get; set; }
        public object[] log_permanent { get; set; }
        public Payment[] payments { get; set; }
        public Item[] items { get; set; }
        public Shipment[] shipments { get; set; }
        public string status { get; set; }
        public string receiver_name { get; set; }
        public string receiver_lastname { get; set; }
        public string receiver_address { get; set; }
        public string receiver_address_number { get; set; }
        public string receiver_address_complement { get; set; }
        public string receiver_neighborhood { get; set; }
        public string receiver_state { get; set; }
        public string receiver_city { get; set; }
        public string receiver_country { get; set; }
        public string receiver_zipcode { get; set; }
        public string receiver_phone { get; set; }
        public string receiver_phone2 { get; set; }
        public string receiver_email { get; set; }
        public string payer_email { get; set; }
        public string payer_name { get; set; }
        public string payer_lastname { get; set; }
        public string payer_address { get; set; }
        public string payer_address_number { get; set; }
        public string payer_address_complement { get; set; }
        public string payer_city { get; set; }
        public string payer_state { get; set; }
        public string payer_country { get; set; }
        public string payer_zipcode { get; set; }
        public string payer_neighborhood { get; set; }
        public string payer_phone { get; set; }
        public string payer_phone2 { get; set; }
        public string payer_cpf { get; set; }
        public string external { get; set; }
        public string channel { get; set; }
        public string original_id { get; set; }
        public float shipping { get; set; }
        public float total_paid { get; set; }
        public float total { get; set; }
        public float subtotal { get; set; }
        public DateTime expected_delivery_date { get; set; }
        public string delivery_type { get; set; }
        public string user_id { get; set; }
        public DateTime created { get; set; }
        public string created_by { get; set; }
        public bool ack { get; set; }
        public DateTime modified { get; set; }
        public string modified_by { get; set; }
        public int timestamp { get; set; }
        public DateTime expected_send_date { get; set; }
        public long order_id { get; set; }
        public string user_client_id { get; set; }
        public string input_service { get; set; }
        public string sub_status { get; set; }
        public string discount { get; set; }
        public string discount_description { get; set; }
        public string channel_account { get; set; }
        public string currency { get; set; }
        public string receiver_address_reference { get; set; }
        public string receiver_additional_info { get; set; }
        public string receiver_phone_area { get; set; }
        public string receiver_phone2_area { get; set; }
        public string receiver_cpf { get; set; }
        public string receiver_schedule_date { get; set; }
        public string receiver_schedule_period { get; set; }
        public string payer_fullname { get; set; }
        public string payer_gender { get; set; }
        public string payer_address_reference { get; set; }
        public string payer_additional_info { get; set; }
        public string payer_phone_area { get; set; }
        public string payer_phone2_area { get; set; }
        public string payer_schedule_date { get; set; }
        public string payer_schedule_period { get; set; }
        public string payer_cnpj { get; set; }
        public string payer_razao_social { get; set; }
        public string payer_ie { get; set; }
        public string payer_im { get; set; }
        public string payer_tax_id { get; set; }
        public string payer_document { get; set; }
        public string payer_company_name { get; set; }
        public string deleted { get; set; }
        public string comission { get; set; }
        public string marked_as_delivered { get; set; }
        public string marked_as_shipped { get; set; }
        public string id { get; set; }
    }

    public class Tags
    {
        public string has_invoice_info { get; set; }
        public string has_invoice_doc { get; set; }
        public string has_gnre_doc { get; set; }
        public string has_track { get; set; }
        public string has_track_url { get; set; }
        public string has_label { get; set; }
    }

    public class Commission
    {
        public string _fixed { get; set; }
        public string tax { get; set; }
        public string total_charged { get; set; }
        public string comment { get; set; }
    }

    public class Payment
    {
        public string payment_type { get; set; }
        public string payment_method { get; set; }
        public string payment_installments { get; set; }
        public string payment_total { get; set; }
        public string payment_quota { get; set; }
        public string payment_interest { get; set; }
        public string id { get; set; }
    }

    public class Item
    {
        public Commission1 commission { get; set; }
        public Variation variation { get; set; }
        public double total { get; set; }
        public string external { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public double price { get; set; }
        public int quantity { get; set; }
        public string original_sku { get; set; }
        public string photo_url { get; set; }
        public string id { get; set; }
        public string location { get; set; }
        public string discount { get; set; }
        public string supplier_id { get; set; }
        public string stock_code { get; set; }
        public string price_code { get; set; }
        public string shipping_cost { get; set; }
    }

    public class Commission1
    {
        public string _fixed { get; set; }
        public string tax { get; set; }
        public string total_charged { get; set; }
        public string comment { get; set; }
    }

    public class Variation
    {
        public object[] attributes { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string external { get; set; }
        public string sku { get; set; }
        public string ean { get; set; }
        public string photo_url { get; set; }
    }

    public class Shipment
    {
        public Label_Info label_info { get; set; }
        public object[] issues { get; set; }
        public object[] shipping_items { get; set; }
        public object[] documents { get; set; }
        public DateTime estimate_delivery_date { get; set; }
        public string shipping_company { get; set; }
        public string external { get; set; }
        public string error_message { get; set; }
        public string quote { get; set; }
        public string shipping_method { get; set; }
        public string shipping_method_id { get; set; }
        public string description { get; set; }
        public string track_code { get; set; }
        public string track_url { get; set; }
        public string status { get; set; }
        public string comment { get; set; }
        public string date_shipped { get; set; }
        public string date_delivered { get; set; }
        public string date_cancelled { get; set; }
        public string nfe_key { get; set; }
        public string nfe_link { get; set; }
        public string nfe_number { get; set; }
        public string nfe_serie { get; set; }
        public string nfe_date { get; set; }
        public string cfops { get; set; }
        public string printed { get; set; }
        public string label_type { get; set; }
        public string id { get; set; }
    }

    public class Label_Info
    {
        public string plp { get; set; }
        public string logotipo { get; set; }
        public string sender_name { get; set; }
        public string sender_address { get; set; }
        public string sender_city { get; set; }
        public string sender_state { get; set; }
        public string sender_zipcode { get; set; }
    }

}

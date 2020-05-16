using IntegracoesPluggto.Business;
using IntegracoesPluggto.DAL;
using IntegracoesPluggto.Util;
using System;
using System.Configuration;
using System.ServiceProcess;
using System.Timers;

namespace IntegracoesPluggto
{
    public partial class Scheduler : ServiceBase
    {
        private SAPbobsCOM.Company oCompany;

        private Timer timerEstoque = null;

        private Timer timerPedidos = null;

        private Timer timerRetNF = null;

        private string _path = ConfigurationManager.AppSettings["Path"];

        private Boolean jobIntegracaoPedido = Convert.ToBoolean(ConfigurationManager.AppSettings["jobIntegracaoPedido"]);

        private Boolean jobIntegracaoRetornoNF = Convert.ToBoolean(ConfigurationManager.AppSettings["jobIntegracaoRetornoNF"]);

        private Boolean jobIntegracaoEstoque = Convert.ToBoolean(ConfigurationManager.AppSettings["jobIntegracaoEstoque"]);

        private Log log;

        public Scheduler()
        {
            log = new Log();

            log.WriteLogPedido("Conectando ao SAPB1 ...");
            log.WriteLogEstoque("Conectando ao SAPB1 ...");
            log.WriteLogRetornoNF("Conectando ao SAPB1 ...");

            oCompany = CommonConn.InitializeCompany();

            IntegracaoService integracaoService = new IntegracaoService();

            integracaoService.GetNewAccessToken();

            //integracaoService.IniciarIntegracaoPedido(oCompany);

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                if (jobIntegracaoPedido)
                {
                    this.timerPedidos = new Timer();

                    string intervaloExecucaoPedido = ConfigurationManager.AppSettings["intervaloExecucaoPedido"];

                    this.timerPedidos.Interval = Convert.ToInt32(intervaloExecucaoPedido);

                    timerPedidos.Enabled = true;

                    this.timerPedidos.Elapsed += new System.Timers.ElapsedEventHandler(this.IntegracaoPedido);
                }

                if (jobIntegracaoEstoque)
                {
                    this.timerEstoque = new Timer();

                    string intervaloExecucaoEstoque = ConfigurationManager.AppSettings["intervaloExecucaoEstoque"] + ",01";

                    this.timerEstoque.Interval = TimeSpan.FromHours(Convert.ToDouble(intervaloExecucaoEstoque)).TotalMilliseconds;

                    timerEstoque.Enabled = true;

                    this.timerEstoque.Elapsed += new System.Timers.ElapsedEventHandler(this.IntegracaoEstoque);
                }

                if (jobIntegracaoRetornoNF)
                {
                    this.timerRetNF = new Timer();

                    string intervaloExecucaoRetNF = ConfigurationManager.AppSettings["intervaloExecucaoRetNF"] ;

                    this.timerRetNF.Interval = Convert.ToInt32(intervaloExecucaoRetNF);

                    timerRetNF.Enabled = true;

                    this.timerRetNF.Elapsed += new System.Timers.ElapsedEventHandler(this.IntegracaoEstoque);
                }
            }
            catch (Exception e)
            {
                log.WriteLogPedido("Falha OnStart "+e.Message);
                log.WriteLogEstoque("Falha OnStart " + e.Message);
                log.WriteLogRetornoNF("Falha OnStart " + e.Message);
            }
            
        }

        private void IntegracaoPedido(object sender, ElapsedEventArgs e)
        {
            try
            {
                timerPedidos.Enabled = false;
                timerPedidos.AutoReset = false;

                this.log.WriteLogPedido("#### INTEGRAÇÃO DE PEDIDOS INICIALIZADA");

                IntegracaoService integracaoService = new IntegracaoService();

                integracaoService.IniciarIntegracaoPedido(this.oCompany);

                timerPedidos.Enabled = true;

                //System.Runtime.InteropServices.Marshal.ReleaseComObject(oCompany);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch (Exception ex)
            {
                this.log.WriteLogPedido("Exception IntegracaoPedido " + ex.Message);
                throw;
            }
        }

        private void IntegracaoEstoque(object sender, ElapsedEventArgs e)
        {
            try
            {
                timerEstoque.Enabled = false;
                timerEstoque.AutoReset = false;

                this.log.WriteLogEstoque("#### INTEGRAÇÃO DE ESTOQUE INICIALIZADA");

                IntegracaoService integracaoService = new IntegracaoService();

                integracaoService.IniciarIntegracaoEstoque(this.oCompany);

                timerEstoque.Enabled = true;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch (Exception ex)
            {
                this.log.WriteLogEstoque("Exception IntegracaoEstoque " + ex.Message);
                throw;
            }
        }

        private void IntegracaoRetornoNF(object sender, ElapsedEventArgs e)
        {
            try
            {
                timerRetNF.Enabled = false;
                timerRetNF.AutoReset = false;

                this.log.WriteLogRetornoNF("#### INTEGRAÇÃO DE RERTORNO DE nf INICIALIZADA");

                IntegracaoService integracaoService = new IntegracaoService();

                integracaoService.IniciarIntegracaoRetornoNF(this.oCompany);

                timerRetNF.Enabled = true;

                //System.Runtime.InteropServices.Marshal.ReleaseComObject(oCompany);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch (Exception ex)
            {
                this.log.WriteLogRetornoNF("Exception IntegracaoRetornoNF " + ex.Message);
                throw;
            }
        }
        protected override void OnStop()
        {
            this.timerPedidos.Stop();
            this.timerEstoque.Stop();
            this.timerRetNF.Stop();
        }
    }
}

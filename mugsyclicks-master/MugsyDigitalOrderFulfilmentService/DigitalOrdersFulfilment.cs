using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace MugsyDigitalOrderFulfilmentService
{
    public partial class DigitalOrdersFulfilment : ServiceBase
    {
        private Timer _timer = null;
        private static bool _IsDoingSomething = false;
        public DigitalOrdersFulfilment()
        {
            InitializeComponent();
            IntiTimer();

            //InitFirstTrigger();
        }

        private void InitFirstTrigger()
        {
            _IsDoingSomething = true;
            ProcessDigitalOrders.ProcessOrder();
            _IsDoingSomething = false;
        }

        private void IntiTimer()
        {
            double timerInterval = 300000;
            double.TryParse( ConfigurationManager.AppSettings["TimeInterval"].ToString(),out timerInterval);
            _timer = new Timer(timerInterval);
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!_IsDoingSomething)
                {
                    _IsDoingSomething = true;
                    ProcessDigitalOrders.ProcessOrder();
                    _IsDoingSomething = false;
                }
            }
            catch (Exception ex)
            {
                _IsDoingSomething = false;
                ProcessDigitalOrders.InsertLog(string.Format("\n\nError : {0}\n\t{1}", ex.Message, ex.StackTrace));
            }
        }

        protected override void OnStart(string[] args)
        {
            if (_timer != null)
            {
                _timer.Start();
            }
            ProcessDigitalOrders.InsertLog(string.Format("\n All Services Started At : {0}", DateTime.Now));
        }

        protected override void OnStop()
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
            ProcessDigitalOrders.InsertLog(string.Format("\n All Services Stopped At : {0}", DateTime.Now));
        }
    }
}

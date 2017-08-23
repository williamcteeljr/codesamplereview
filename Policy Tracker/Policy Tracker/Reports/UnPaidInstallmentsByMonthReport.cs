using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;

namespace Policy_Tracker.Reports
{
    public partial class UnPaidInstallmentsByMonthReport : DevExpress.XtraReports.UI.XtraReport
    {
        public UnPaidInstallmentsByMonthReport()
        {
            InitializeComponent();
        }

        public void BoundNotIssuedReport_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
        }
    }
}

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;

namespace Policy_Tracker.Reports
{
    public partial class BoundNotIssuedReport : DevExpress.XtraReports.UI.XtraReport
    {
        public BoundNotIssuedReport()
        {
            InitializeComponent();
            BeforePrint += BoundNotIssuedReport_BeforePrint;
        }

        public void BoundNotIssuedReport_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
           
            //if (Branch.Value != "")
            //{
            //    DevExpress.DataAccess.Sql.QueryParameter branchParameter = new DevExpress.DataAccess.Sql.QueryParameter();
            //    branchParameter.Name = "Branch";
            //    branchParameter.Type = typeof(DevExpress.DataAccess.Expression);
            //    branchParameter.Value = new DevExpress.DataAccess.Expression("[Parameters.Branch]");
            //    tq.Parameters.Add(branchParameter);
            //    tq.FilterString += " AND Branch = ?Branch";
            //}
            //if (ProductLine.Value != "")
            //{
            //    DevExpress.DataAccess.Sql.QueryParameter productLineParameter = new DevExpress.DataAccess.Sql.QueryParameter();
            //    productLineParameter.Name = "ProductLine";
            //    productLineParameter.Type = typeof(DevExpress.DataAccess.Expression);
            //    productLineParameter.Value = new DevExpress.DataAccess.Expression("[Parameters.ProductLine]");
            //    tq.Parameters.Add(productLineParameter);
            //    tq.FilterString += " AND ProductLine = ?ProductLine";
            //}
            //this.Risks.Queries[0] = tq;
            //this.xrLabel8.Text = tq.
        }
    }
}

#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASP
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Mvc.Html;
    using System.Web.Optimization;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using DevExpress.Data.PivotGrid;
    using DevExpress.Utils;
    using DevExpress.Web;
    using DevExpress.Web.ASPxHtmlEditor;
    using DevExpress.Web.ASPxPivotGrid;
    using DevExpress.Web.ASPxRichEdit;
    using DevExpress.Web.ASPxScheduler;
    using DevExpress.Web.ASPxSpellChecker;
    using DevExpress.Web.ASPxSpreadsheet;
    using DevExpress.Web.ASPxThemes;
    using DevExpress.Web.Mvc;
    using DevExpress.Web.Mvc.UI;
    using DevExpress.XtraCharts;
    using DevExpress.XtraCharts.Web;
    using DevExpress.XtraPivotGrid;
    using DevExpress.XtraReports;
    using DevExpress.XtraReports.UI;
    using DevExpress.XtraReports.Web;
    using DevExpress.XtraReports.Web.DocumentViewer;
    using DevExpress.XtraRichEdit;
    using DevExpress.XtraScheduler;
    using DevExpress.XtraScheduler.Native;
    using PolicyTracker.BusinessServices;
    using PolicyTracker.BusinessServices.Security;
    
    #line 2 "..\..\Views\Policy\CommercialRatingApp.cshtml"
    using PolicyTracker.DomainModel.Policy;
    
    #line default
    #line hidden
    using PolicyTracker.HtmlHelpers;
    using PolicyTracker.Utilities;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Policy/CommercialRatingApp.cshtml")]
    public partial class _Views_Policy_CommercialRatingApp_cshtml : System.Web.Mvc.WebViewPage<PolicyTracker.DomainModel.Policy.Risk>
    {
        public _Views_Policy_CommercialRatingApp_cshtml()
        {
        }
        public override void Execute()
        {
WriteLiteral("\r\n");

            
            #line 4 "..\..\Views\Policy\CommercialRatingApp.cshtml"
  
    var URL = new System.Text.StringBuilder();
    var finalDispositions = new[] { RiskStatus.ISSUED.Value, RiskStatus.DECLINED.Value, RiskStatus.CANCELED.Value, RiskStatus.BOUND.Value };
    
    if (!finalDispositions.Contains(Model.Status))
    {
        URL.Append("/quoteengine/pages/RiskQuoteWizard.aspx?");
        URL.Append(String.Format("UID={0}&PID=1&STRID=3&WIZID=3", PolicyTracker.Platform.Security.SessionManager.GetCurrentSession().User.UserId));
        URL.Append(String.Format("&CTN={0}&QOT={1}", Model.ControlNumber, Model.QuoteType));
        URL.Append(String.Format("&WKS={0}", (Convert.ToInt32(Model.PolicySuffix) > 1 ? "Renewal" : "Quote")));
        URL.Append(String.Format("&QID={0}", Model.Id));
        URL.Append(String.Format("&Mode={0}", (String.IsNullOrEmpty(Model.CompletedWizardId) ? "Edit" : "View")));
    }
    else
    {
        URL.Append("/quoteengine/pages/AccountCoverageInfo.aspx?");
        URL.Append(String.Format("UID={0}", PolicyTracker.Platform.Security.SessionManager.GetCurrentSession().User.UserId));
        URL.Append("&PID=1&STRID=3");
        URL.Append(String.Format("&CTN={0}&QOT={1}&QID={2}", Model.ControlNumber, Model.QuoteType, Model.Id));
    }

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n<iframe");

WriteAttribute("src", Tuple.Create(" src=\"", 1333), Tuple.Create("\"", 1343)
            
            #line 26 "..\..\Views\Policy\CommercialRatingApp.cshtml"
, Tuple.Create(Tuple.Create("", 1339), Tuple.Create<System.Object, System.Int32>(URL
            
            #line default
            #line hidden
, 1339), false)
);

WriteLiteral(" style=\"position: absolute; height: 100%; width: 100%; border: none\"");

WriteLiteral("></iframe>");

        }
    }
}
#pragma warning restore 1591
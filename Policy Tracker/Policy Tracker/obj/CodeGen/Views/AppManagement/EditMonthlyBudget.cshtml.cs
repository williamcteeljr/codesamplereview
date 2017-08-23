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
    using PolicyTracker.HtmlHelpers;
    using PolicyTracker.Utilities;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/AppManagement/EditMonthlyBudget.cshtml")]
    public partial class _Views_AppManagement_EditMonthlyBudget_cshtml : System.Web.Mvc.WebViewPage<PolicyTracker.DomainModel.Policy.MonthlyBudget>
    {
        public _Views_AppManagement_EditMonthlyBudget_cshtml()
        {
        }
        public override void Execute()
        {
WriteLiteral("<form");

WriteLiteral(" id=\"MonthlyBudgetEditForm\"");

WriteLiteral(" class=\"oraForm form-horizontal\"");

WriteLiteral(">\r\n");

WriteLiteral("    ");

            
            #line 4 "..\..\Views\AppManagement\EditMonthlyBudget.cshtml"
Write(Html.ValidationSummary());

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n    <div");

WriteLiteral(" class=\"box\"");

WriteLiteral(">\r\n        <div");

WriteLiteral(" class=\"box-header with-border\"");

WriteLiteral(">\r\n            <h3");

WriteLiteral(" class=\"box-title\"");

WriteLiteral(">Monthly Budget</h3>\r\n        </div>\r\n        <div");

WriteLiteral(" class=\"box-body\"");

WriteLiteral(">\r\n");

WriteLiteral("            ");

            
            #line 11 "..\..\Views\AppManagement\EditMonthlyBudget.cshtml"
       Write(Html.HiddenFor(m => Model.BudgetId));

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n            <div");

WriteLiteral(" class=\"form-group\"");

WriteLiteral(">\r\n                <label");

WriteLiteral(" class=\"col-lg-2 col-sm-3 control-label\"");

WriteLiteral(">Product Line:</label>\r\n                <div");

WriteLiteral(" class=\"col-lg-10 col-sm-9\"");

WriteLiteral(">\r\n");

WriteLiteral("                    ");

            
            #line 16 "..\..\Views\AppManagement\EditMonthlyBudget.cshtml"
               Write(Html.DropDownListFor(m => Model.ProductLine, new SelectList(ViewBag.ProductLines, "Name", "Name"), "", new { @class = "form-control" }));

            
            #line default
            #line hidden
WriteLiteral("\r\n                </div>\r\n            </div>\r\n\r\n\r\n            <div");

WriteLiteral(" class=\"form-group\"");

WriteLiteral(">\r\n                <label");

WriteLiteral(" class=\"col-lg-2 col-sm-3 control-label\"");

WriteLiteral(">Branch:</label>\r\n                <div");

WriteLiteral(" class=\"col-lg-10 col-sm-9\"");

WriteLiteral(">\r\n");

WriteLiteral("                    ");

            
            #line 24 "..\..\Views\AppManagement\EditMonthlyBudget.cshtml"
               Write(Html.DropDownListFor(m => Model.Branch, new SelectList(ViewBag.Branches, "Value", "DisplayText"), "", new { @class = "form-control" }));

            
            #line default
            #line hidden
WriteLiteral("\r\n                </div>\r\n            </div>\r\n\r\n\r\n            <div");

WriteLiteral(" class=\"form-group\"");

WriteLiteral(">\r\n                <label");

WriteLiteral(" class=\"col-lg-2 col-sm-3 control-label\"");

WriteLiteral(">Amount:</label>\r\n                <div");

WriteLiteral(" class=\"col-lg-10 col-sm-9\"");

WriteLiteral(">\r\n");

WriteLiteral("                    ");

            
            #line 32 "..\..\Views\AppManagement\EditMonthlyBudget.cshtml"
               Write(Html.DevExpress().TextBoxFor(model => Model.Amount, settings =>
                       {
                           settings.Name = "Amount";
                           settings.ControlStyle.CssClass = "form-control";
                           settings.Properties.MaskSettings.Mask = "<0..999999999g>";
                           settings.Properties.MaskSettings.IncludeLiterals = MaskIncludeLiteralsMode.DecimalSymbol;
                       }).Bind(Model.Amount.ToString("N0")).GetHtml());

            
            #line default
            #line hidden
WriteLiteral("\r\n                </div>\r\n            </div>\r\n\r\n\r\n            <div");

WriteLiteral(" class=\"form-group\"");

WriteLiteral(">\r\n                <label");

WriteLiteral(" class=\"col-lg-2 col-sm-3 control-label\"");

WriteLiteral(">Month:</label>\r\n                <div");

WriteLiteral(" class=\"col-lg-10 col-sm-9\"");

WriteLiteral(">\r\n");

WriteLiteral("                    ");

            
            #line 46 "..\..\Views\AppManagement\EditMonthlyBudget.cshtml"
               Write(Html.DropDownListFor(m => Model.DateMonth, new SelectList(ViewBag.Months, "Value", "DisplayText"), new { @class = "form-control" }));

            
            #line default
            #line hidden
WriteLiteral("\r\n                </div>\r\n            </div>\r\n\r\n\r\n            <div");

WriteLiteral(" class=\"form-group\"");

WriteLiteral(">\r\n                <label");

WriteLiteral(" class=\"col-lg-2 col-sm-3 control-label\"");

WriteLiteral(">Year:</label>\r\n                <div");

WriteLiteral(" class=\"col-lg-10 col-sm-9\"");

WriteLiteral(">\r\n");

WriteLiteral("                    ");

            
            #line 54 "..\..\Views\AppManagement\EditMonthlyBudget.cshtml"
               Write(Html.DropDownListFor(m => Model.DateYear, new SelectList(ViewBag.Years), DateTime.Now.Year.ToString(), new { @class = "form-control" }));

            
            #line default
            #line hidden
WriteLiteral("\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </div>\r\n</form>" +
"");

        }
    }
}
#pragma warning restore 1591
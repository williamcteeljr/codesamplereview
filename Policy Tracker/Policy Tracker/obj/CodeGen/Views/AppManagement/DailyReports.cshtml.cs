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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/AppManagement/DailyReports.cshtml")]
    public partial class _Views_AppManagement_DailyReports_cshtml : System.Web.Mvc.WebViewPage<dynamic>
    {
        public _Views_AppManagement_DailyReports_cshtml()
        {
        }
        public override void Execute()
        {
WriteLiteral("<div");

WriteLiteral(" class=\"row\"");

WriteLiteral(">\r\n    <div");

WriteLiteral(" class=\"col-xs-12\"");

WriteLiteral(">\r\n        <div");

WriteLiteral(" class=\"box box-solid\"");

WriteLiteral(">\r\n            <div");

WriteLiteral(" class=\"box-header\"");

WriteLiteral(">\r\n                <h3");

WriteLiteral(" class=\"box-title\"");

WriteLiteral(">Reports</h3>\r\n            </div><!-- /.box-header -->\r\n            <div");

WriteLiteral(" class=\"box-body table-responsive no-padding\"");

WriteLiteral(">\r\n                <table");

WriteLiteral(" class=\"table table-hover\"");

WriteLiteral(@">
                    <tbody>
                        <tr>
                            <th>Report</th>
                            <th>Emails</th>
                            <th></th>
                        </tr>
                        <tr>
                            <td>This Months Expiring Policies</td>
                            <td><input");

WriteLiteral(" type=\"text\"");

WriteLiteral(" class=\"form-control\"");

WriteLiteral(" placeholder=\"Email Addresses\"");

WriteLiteral("/></td>\r\n                            <td");

WriteLiteral(" class=\"text-right\"");

WriteLiteral("><button");

WriteLiteral(" type=\"button\"");

WriteLiteral(" class=\"btn btn-default btn-lg MonthlyExpiredPolicies\"");

WriteLiteral(">Send</button></td>\r\n                        </tr>\r\n                    </tbody>\r" +
"\n                </table>\r\n            </div><!-- /.box-body -->\r\n        </div>" +
"<!-- /.box -->\r\n    </div>\r\n</div>\r\n\r\n<script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(@">
    $(document).ready(function () {
        $('.MonthlyExpiredPolicies').click(function () {
            $.get('/policytracker/reporting/MonthlyExpiringAccounts')
                .done(function (data) {  })
                .fail(function (data) {  })
        })
    })
</script>");

        }
    }
}
#pragma warning restore 1591
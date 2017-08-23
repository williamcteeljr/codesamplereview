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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Home/UserFilters.cshtml")]
    public partial class _Views_Home_UserFilters_cshtml : System.Web.Mvc.WebViewPage<dynamic>
    {
        public _Views_Home_UserFilters_cshtml()
        {
        }
        public override void Execute()
        {
WriteLiteral("<script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(@">
    $(document).ready(function () {
        $(""#WLMyFilter"").change(function () {
            var filter = $(""#WLMyFilter"").val();
            grid = ASPxClientControl.GetControlCollection().GetByName('WorkingListGrid');
            grid.ApplyFilter(filter);
        });

        $(""#DeleteMyFilter"").click(function () {
            var userId = $(""#CurrUserId"").val();
            var filterName = $(""#WLMyFilter option:selected"").text();
            ora.Dialog.Confirm({
                id: ""ConfirmFilterDelete"",
                message: ""Are you sure you want to delete filter'"" + filterName + ""'"",
                AcceptCallback: function () {
                    ora.Ajax.Ajax({
                        url: ""policytracker/Home/DeleteFilterSetting?userId="" + userId + ""&filterName="" + filterName,
                        dataType: ""HTML"",
                        type: ""DELETE"",
                        successCallback: function (data) { $(""#UserFilterPanel"").html(data); }
                    });
                },
            })

        });
    });
</script>

<div");

WriteLiteral(" class=\"form-group\"");

WriteLiteral(">\r\n    <label");

WriteLiteral(" class=\"col-lg-2 col-sm-2 control-label\"");

WriteLiteral(">Filters:</label>\r\n    <div");

WriteLiteral(" class=\"col-lg-10 col-sm-10\"");

WriteLiteral(">\r\n        <div");

WriteLiteral(" class=\"input-group input-group-sm\"");

WriteLiteral(">\r\n");

WriteLiteral("            ");

            
            #line 33 "..\..\Views\Home\UserFilters.cshtml"
       Write(Html.DropDownList("WLMyFilter", new SelectList(ViewBag.MyFilters, "Expression", "Name"), "", new { @class = "form-control input-sm", @title = "Apply previously saved filters you have created" }));

            
            #line default
            #line hidden
WriteLiteral("\r\n            <span");

WriteLiteral(" class=\"input-group-btn\"");

WriteLiteral(">\r\n                <button");

WriteLiteral(" id=\"DeleteMyFilter\"");

WriteLiteral(" type=\"button\"");

WriteLiteral(" class=\"btn btn-danger\"");

WriteLiteral(" title=\"Delete Selected Filter\"");

WriteLiteral("><i");

WriteLiteral(" class=\"fa fa-trash-o\"");

WriteLiteral("></i></button>\r\n            </span>\r\n        </div>\r\n    </div>\r\n</div>\r\n\r\n\r\n");

        }
    }
}
#pragma warning restore 1591
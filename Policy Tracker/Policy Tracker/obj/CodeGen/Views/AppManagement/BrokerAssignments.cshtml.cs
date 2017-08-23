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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/AppManagement/BrokerAssignments.cshtml")]
    public partial class _Views_AppManagement_BrokerAssignments_cshtml : System.Web.Mvc.WebViewPage<dynamic>
    {
        public _Views_AppManagement_BrokerAssignments_cshtml()
        {
        }
        public override void Execute()
        {
WriteLiteral("<script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(">\r\n    ora.UI.Grid.Grid({\r\n        id: \"BrokerAssignments\",\r\n        pager: \"Brok" +
"erAssignmentsPager\",\r\n        height: 500,\r\n        restUrl: \'/policytracker/api" +
"/AppManagement/GetBrokerAssignments/\',\r\n        rowId: \'AssignmentId\',\r\n        " +
"caption: \'\',\r\n        columnNames: [\'AssignmentId\', \'BrokerCode\', \'UserId\', \'Use" +
"rName\', \'ProductLineId\', \'Product Line\'],\r\n        columnModel: [\r\n            {" +
" name: \'AssignmentId\', width: 70, search: false, hidden: true },\r\n            { " +
"name: \'BrokerCode\', width: 100, search: true },\r\n            { name: \'UserId\', w" +
"idth: 70, align: \'center\' },\r\n            { name: \'UserName\', width: 70, },\r\n   " +
"         { name: \'ProductLineId\', width: 70, align: \'center\', hidden: true },\r\n " +
"           { name: \'ProductLine\', width: 70, search: false },\r\n        ]\r\n    })" +
";\r\n\r\n    ora.UI.Grid.Pager(\r\n        {\r\n            id: \"BrokerAssignments\", pag" +
"er: \"BrokerAssignmentsPager\",\r\n            deleteURL: \"/policytracker/AppManagem" +
"ent/DeleteBrokerAssignment?assignmentId={0}\",\r\n            editParams: { Assignm" +
"entId: \"assignmentId\" },\r\n            getURL: \"/policytracker/AppManagement/Edit" +
"BrokerAssignment\",\r\n            form: { id: \'BrokerAssignmentEditForm\', grid: \"B" +
"rokerAssignments\", submitURL: \"/policytracker/AppManagement/EditBrokerAssignment" +
"\", container: \"BrokerAssignment_dg\", dialog: \"BrokerAssignment_dg\" },\r\n         " +
"   type: ora.Dialog.ActionFormSizes.Medium, modal: true\r\n        },\r\n        {\r\n" +
"            hasWriteAccess: true,\r\n            hasDeleteAccess: true\r\n        }\r" +
"\n    );\r\n\r\n    jQuery(\"#BrokerAssignments\").filterToolbar({ searchOnEnter: true," +
" enableClear: true });\r\n</script>\r\n\r\n<div");

WriteLiteral(" class=\"box box-solid\"");

WriteLiteral(">\r\n    <div");

WriteLiteral(" class=\"box-header with-border\"");

WriteLiteral(">\r\n        <h3");

WriteLiteral(" class=\"box-title\"");

WriteLiteral(">Broker To Underwriter Assignments</h3>\r\n    </div>\r\n    <div");

WriteLiteral(" class=\"box-body\"");

WriteLiteral(">\r\n");

WriteLiteral("        ");

            
            #line 43 "..\..\Views\AppManagement\BrokerAssignments.cshtml"
   Write(Html.jqGrid("BrokerAssignments", true));

            
            #line default
            #line hidden
WriteLiteral("\r\n    </div>\r\n</div>");

        }
    }
}
#pragma warning restore 1591
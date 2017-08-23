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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/AppManagement/Products.cshtml")]
    public partial class _Views_AppManagement_Products_cshtml : System.Web.Mvc.WebViewPage<dynamic>
    {
        public _Views_AppManagement_Products_cshtml()
        {
        }
        public override void Execute()
        {
WriteLiteral("<script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(@">
    ora.UI.Grid.Grid({
        id: ""ProductsGrid"",
        pager: ""ProductsGridPager"",
        height: 500,
        restUrl: '/policytracker/api/AppManagement/GetProducts/',
        rowId: 'Id',
        caption: '',
        columnNames: ['ProductId', 'Prefix', 'Description', 'QuoteType', 'IsActive'],
        columnModel: [
            { name: 'ProductId', width: 70, search: false, hidden: true },
            { name: 'Prefix', width: 50, search: true },
            { name: 'Description', width: 150, search: true },
            { name: 'QuoteType', width: 70 },
            { name: 'IsActive', width: 70, }
        ]
    });

    ora.UI.Grid.Pager(
        {
            id: ""ProductsGrid"", pager: ""ProductsGridPager"",
            deleteURL: ""/policytracker/api/AppManagement/DeleteProduct/{0}"",
            editParams: { ProductId: ""productId"" },
            getURL: ""/policytracker/AppManagement/EditProduct"",
            form: { id: 'ProductEditForm', grid: ""ProductsGrid"", submitURL: ""/policytracker/AppManagement/EditProduct"", container: ""Product_dg"", dialog: ""Product_dg"" },
            type: ora.Dialog.ActionFormSizes.Medium, modal: true
        },
        {
            hasWriteAccess: true,
            hasDeleteAccess: true
        }
    );

    jQuery(""#ProductsGrid"").filterToolbar({ searchOnEnter: true, enableClear: true });
</script>

<div");

WriteLiteral(" class=\"box box-solid\"");

WriteLiteral(">\r\n    <div");

WriteLiteral(" class=\"box-header with-border\"");

WriteLiteral(">\r\n        <h3");

WriteLiteral(" class=\"box-title\"");

WriteLiteral(">Products (Policy Prefixes)</h3>\r\n    </div>\r\n    <div");

WriteLiteral(" class=\"box-body\"");

WriteLiteral(">\r\n");

WriteLiteral("        ");

            
            #line 42 "..\..\Views\AppManagement\Products.cshtml"
   Write(Html.jqGrid("ProductsGrid", true));

            
            #line default
            #line hidden
WriteLiteral("\r\n    </div>\r\n</div>");

        }
    }
}
#pragma warning restore 1591
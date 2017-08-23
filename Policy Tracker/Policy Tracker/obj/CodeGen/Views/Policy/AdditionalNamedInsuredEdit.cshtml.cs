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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Policy/AdditionalNamedInsuredEdit.cshtml")]
    public partial class _Views_Policy_AdditionalNamedInsuredEdit_cshtml : System.Web.Mvc.WebViewPage<PolicyTracker.DomainModel.Policy.AdditionalNamedInsured>
    {
        public _Views_Policy_AdditionalNamedInsuredEdit_cshtml()
        {
        }
        public override void Execute()
        {
WriteLiteral("<form");

WriteLiteral(" id=\"AdditionalNamedInsuredEditForm\"");

WriteLiteral(" class=\"oraForm form-horizontal\"");

WriteLiteral(">\r\n");

WriteLiteral("    ");

            
            #line 4 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
Write(Html.ValidationSummary());

            
            #line default
            #line hidden
WriteLiteral("\r\n");

WriteLiteral("    ");

            
            #line 5 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
Write(Html.HiddenFor(m => Model.AdditionalNamedInsuredId));

            
            #line default
            #line hidden
WriteLiteral("\r\n");

WriteLiteral("    ");

            
            #line 6 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
Write(Html.HiddenFor(m => Model.QuoteId));

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n    <script>\r\n        $(function () {\r\n            $(\"#btnSelectAllAddLocatio" +
"ns\").bind(\"click\", function () {\r\n                ");

WriteLiteral("\r\n\r\n            //Remove Selected Locations\r\n                $(\"#AddInsuredLocati" +
"ons\").find(\"option\").attr(\"selected\", true);\r\n           \r\n        });\r\n        " +
"});\r\n    </script>\r\n   \r\n    <div");

WriteLiteral(" class=\"col-lg-8\"");

WriteLiteral(">\r\n        <div");

WriteLiteral(" class=\"box box-primary\"");

WriteLiteral(">\r\n            <div");

WriteLiteral(" class=\"box-header with-border\"");

WriteLiteral(">\r\n                <i");

WriteLiteral(" class=\"fa fa-plane\"");

WriteLiteral("></i>\r\n                <h3");

WriteLiteral(" class=\"box-title\"");

WriteLiteral(">Named Insured</h3>\r\n            </div><!-- /.box-header -->\r\n            <!-- fo" +
"rm start -->\r\n            <div");

WriteLiteral(" class=\"box-body\"");

WriteLiteral(">\r\n\r\n                <div");

WriteLiteral(" class=\"form-group\"");

WriteLiteral(">\r\n                    <label");

WriteLiteral(" class=\"col-lg-3 col-sm-3 control-label\"");

WriteLiteral(">New Named Insured</label>\r\n                    <div");

WriteLiteral(" class=\"col-lg-9 col-sm-9\"");

WriteLiteral(">\r\n");

WriteLiteral("                        ");

            
            #line 39 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
                   Write(Html.TextBoxFor(m => Model.CompanyName, new { @class = "form-control" }));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </div>\r\n                </div>\r\n\r\n                <div");

WriteLiteral(" class=\"form-group\"");

WriteLiteral(">\r\n                    <label");

WriteLiteral(" class=\"col-lg-3 col-sm-3 control-label\"");

WriteLiteral(">Name 2</label>\r\n                    <div");

WriteLiteral(" class=\"col-lg-9 col-sm-9\"");

WriteLiteral(">\r\n");

WriteLiteral("                        ");

            
            #line 46 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
                   Write(Html.TextBoxFor(m => Model.Name2, new { @class = "form-control" }));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </div>\r\n                </div>\r\n\r\n                <div");

WriteLiteral(" class=\"form-group\"");

WriteLiteral(">\r\n                    <label");

WriteLiteral(" class=\"col-lg-3 col-sm-3 control-label\"");

WriteLiteral(">FEIN #:</label>\r\n                    <div");

WriteLiteral(" class=\"col-lg-9 col-sm-9\"");

WriteLiteral(">\r\n");

WriteLiteral("                        ");

            
            #line 53 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
                   Write(Html.TextBoxFor(m => Model.FEIN, new { @class = "form-control" }));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </div>\r\n                </div>\r\n\r\n                <div");

WriteLiteral(" class=\"form-group\"");

WriteLiteral(">\r\n                    <label");

WriteLiteral(" class=\"col-lg-3 col-sm-3 control-label\"");

WriteLiteral(">Street Address:</label>\r\n                    <div");

WriteLiteral(" class=\"col-lg-9 col-sm-9\"");

WriteLiteral(">\r\n");

WriteLiteral("                        ");

            
            #line 60 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
                   Write(Html.TextBoxFor(m => Model.StreetAddress1, new { @class = "form-control" }));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </div>\r\n                </div>\r\n\r\n                <div");

WriteLiteral(" class=\"form-group\"");

WriteLiteral(">\r\n                    <label");

WriteLiteral(" class=\"col-lg-3 col-sm-3 control-label\"");

WriteLiteral(">Street Address 2:</label>\r\n                    <div");

WriteLiteral(" class=\"col-lg-9 col-sm-9\"");

WriteLiteral(">\r\n");

WriteLiteral("                        ");

            
            #line 67 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
                   Write(Html.TextBoxFor(m => Model.StreetAddress2, new { @class = "form-control" }));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </div>\r\n                </div>\r\n\r\n                <div");

WriteLiteral(" class=\"form-group\"");

WriteLiteral(">\r\n                    <label");

WriteLiteral(" class=\"col-lg-3 col-sm-3 control-label\"");

WriteLiteral(">City:</label>\r\n                    <div");

WriteLiteral(" class=\"col-lg-9 col-sm-9\"");

WriteLiteral(">\r\n");

WriteLiteral("                        ");

            
            #line 74 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
                   Write(Html.TextBoxFor(m => Model.City, new { @class = "form-control" }));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </div>\r\n                </div>\r\n\r\n                <div");

WriteLiteral(" class=\"form-group\"");

WriteLiteral(">\r\n                    <label");

WriteLiteral(" class=\"col-lg-3 col-sm-3 control-label\"");

WriteLiteral(">State:</label>\r\n                    <div");

WriteLiteral(" class=\"col-lg-9 col-sm-9\"");

WriteLiteral(">\r\n");

WriteLiteral("                        ");

            
            #line 81 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
                   Write(Html.TextBoxFor(m => Model.State, new { @class = "form-control upper editable" }));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </div>\r\n                </div>\r\n\r\n                <div");

WriteLiteral(" class=\"form-group\"");

WriteLiteral(">\r\n                    <label");

WriteLiteral(" class=\"col-lg-3 col-sm-3 control-label\"");

WriteLiteral(">Zip Code:</label>\r\n                    <div");

WriteLiteral(" class=\"col-lg-9 col-sm-9\"");

WriteLiteral(">\r\n");

WriteLiteral("                        ");

            
            #line 88 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
                   Write(Html.TextBoxFor(m => Model.Zip, new { @class = "form-control" }));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </div>\r\n                </div>\r\n\r\n                <div");

WriteLiteral(" class=\"form-group\"");

WriteLiteral(">\r\n                    <label");

WriteLiteral(" class=\"col-lg-3 col-sm-3 control-label\"");

WriteLiteral(">Locations:</label>\r\n                    <div");

WriteLiteral(" class=\"col-lg-9 col-sm-9\"");

WriteLiteral(">\r\n");

WriteLiteral("                        ");

            
            #line 95 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
                   Write(Html.ListBoxFor(x => x.SelectedItemsIds, Model.OptionList, new { id = "AddInsuredLocations", SelectionMode = "multiple", style = "width:100%" }));

            
            #line default
            #line hidden
WriteLiteral("\r\n                        <input");

WriteLiteral(" type=\"button\"");

WriteLiteral(" id=\"btnSelectAllAddLocations\"");

WriteLiteral(" value=\"Select All Locations\"");

WriteLiteral(" />\r\n                    </div>\r\n                </div>\r\n\r\n            </div>\r\n  " +
"      </div>\r\n    </div>\r\n\r\n    <div");

WriteLiteral(" class=\"col-lg-4\"");

WriteLiteral(">\r\n        <div");

WriteLiteral(" class=\"box box-danger\"");

WriteLiteral(">\r\n             <!--form start -->\r\n            <div");

WriteLiteral(" class=\"box-body\"");

WriteLiteral(" style=\"text-align:center\"");

WriteLiteral(">\r\n                <i");

WriteLiteral(" class=\"fa fa-users\"");

WriteLiteral(" style=\"font-size:14em\"");

WriteLiteral("></i>\r\n            </div>\r\n        </div>\r\n    </div>\r\n");

            
            #line 112 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
    
            
            #line default
            #line hidden
            
            #line 112 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
     if (Model.AdditionalNamedInsuredId != 0)
    {

            
            #line default
            #line hidden
WriteLiteral("    <div");

WriteLiteral(" class=\"col-lg-4\"");

WriteLiteral(">\r\n        <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(">\r\n                        $(\"#AddNamedInsuredEndorsement\").click(function () {\r\n" +
"                            ora.Dialog.SimpleAjax({\r\n                           " +
"     Id: \'PostingNoticePartial_dg\', Title: \'Endorsement Confirmation\',\r\n        " +
"                        Url: \'/policytracker/Policy/AdditionalInsuredEndorsement" +
"\',\r\n                                MinWidth: function () { return 100 },\r\n     " +
"                           MaxWidth: function () { return 450 },\r\n              " +
"                  Width: function () { return 415 },\r\n                          " +
"      MaxHeight: function () { return 500 },\r\n                                Mi" +
"nHeight: function () { return 100 },\r\n                                Height: fu" +
"nction () { return 250 },\r\n\r\n                                buttons: [\r\n       " +
"                             {\r\n                                        text: \"Y" +
"es\", id: \"SaveClose\", name: \"SaveClose\",\r\n                                      " +
"  //icons: { primary: \"\", secondary: \"ui-icon-disk\" },\r\n                        " +
"                click: function () {\r\n                                          " +
"  //var d = $(this);\r\n                                            //if ($(d.cont" +
"ext).find(\"form\").length == 0) { $(d).dialog(\'destroy\').remove(); }\r\n           " +
"                                 //else {\r\n                                     " +
"       //    $(d.context).closest(\'[aria-describedby|=\"\' + d.context.id + \'\"]\')." +
"find(\".ui-dialog-buttonpane\").find(\".ui-dialog-buttonset\").find(\"#Save\").removeC" +
"lass(\"PostedForm\");\r\n                                            //    $(d.conte" +
"xt).find(\"form\").submit();\r\n                                            //}\r\n   " +
"                                         var currentInsuredId = ");

            
            #line 138 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
                                                              Write(Model.AdditionalNamedInsuredId);

            
            #line default
            #line hidden
WriteLiteral(";\r\n                                            var currentRiskId = ");

            
            #line 139 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
                                                           Write(Model.QuoteId);

            
            #line default
            #line hidden
WriteLiteral(";\r\n                                            var dataObject = JSON.stringify({ " +
"InsuredId : currentInsuredId, RiskId : currentRiskId });\r\n                      " +
"                      $.ajax({\r\n                                                " +
"url: \"");

            
            #line 142 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
                                                 Write(Url.Action("PostingNoticeEndorsedInsuredSubmission"));

            
            #line default
            #line hidden
WriteLiteral("\",\r\n                                                type: \"POST\",\r\n              " +
"                                  data: dataObject,\r\n                           " +
"                     contentType: \"application/json; charset=utf-8\",\r\n          " +
"                                      success: function (response) {\r\n          " +
"                                          console.log(JSON.stringify(response));" +
"\r\n                                                    $.each(response, function(" +
"index, responses){\r\n                                                        var " +
"value = responses;\r\n                                                        cons" +
"ole.log(value);\r\n                                                        if(valu" +
"e.toString() === \"0\")\r\n                                                        {" +
"\r\n                                                            alert(\"Error! Your" +
" Confirmation Number is \" + value);\r\n                                           " +
"                 $(\'#postingNoticeInsuredEndorsement\').html(\"There is an error w" +
"ith your posting notice request and we are unable to process your request at thi" +
"s time.\"\r\n                                                                + \" Pl" +
"ease check the Logs for details\");\r\n                                            " +
"            }\r\n                                                        else\r\n   " +
"                                                     {\r\n                        " +
"                                    alert(\"Success! Your Confirmation Number is " +
"\" + value);\r\n                                                            $(\'#pos" +
"tingNoticeInsuredEndorsement\').html(\"Success! Your Confirmation Number is \" + va" +
"lue + \". Please check the Logs for additional information.\");\r\n                 " +
"                                           //$(\"#SaveClose\").attr(\"disabled\", tr" +
"ue);\r\n                                                                          " +
"                                           \r\n                                   " +
"                             //$(\"#WLRefresh\").click();\r\n                       " +
"                                     \r\n                                         " +
"               }\r\n                                                    });\r\n     " +
"                                           },\r\n                                 " +
"               error: function (response) {\r\n                                   " +
"                 console.log(\'error: \' + response);\r\n                           " +
"                     }\r\n                                            });\r\n\r\n     " +
"                                   }\r\n                                    },\r\n  " +
"                                  {\r\n                                        tex" +
"t: \"No\", id: \"Cancel\", name: \"No\",\r\n                                        clic" +
"k: function () {\r\n                                            $(this).dialog(\'de" +
"stroy\').remove();\r\n                                        }\r\n                  " +
"                  }\r\n                                ]\r\n                        " +
"    });\r\n                        });\r\n        </script>\r\n        <button");

WriteLiteral(" id=\"AddNamedInsuredEndorsement\"");

WriteLiteral(" type=\"button\"");

WriteLiteral(" class=\"btn-danger btn\"");

WriteLiteral(">Send Endorsement</button>\r\n    </div>\r\n");

            
            #line 187 "..\..\Views\Policy\AdditionalNamedInsuredEdit.cshtml"
    }

            
            #line default
            #line hidden
WriteLiteral("</form>\r\n");

        }
    }
}
#pragma warning restore 1591
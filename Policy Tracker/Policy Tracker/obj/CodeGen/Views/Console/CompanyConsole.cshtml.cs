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
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Console/CompanyConsole.cshtml")]
    public partial class _Views_Console_CompanyConsole_cshtml : System.Web.Mvc.WebViewPage<dynamic>
    {
        public _Views_Console_CompanyConsole_cshtml()
        {
        }
        public override void Execute()
        {
            
            #line 1 "..\..\Views\Console\CompanyConsole.cshtml"
  
    ViewBag.Title = "Company Console";
    Layout = "~/Views/Shared/_DevExtremeLayout.cshtml";

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n");

            
            #line 6 "..\..\Views\Console\CompanyConsole.cshtml"
Write(Scripts.Render("~/angular/controllers"));

            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 7 "..\..\Views\Console\CompanyConsole.cshtml"
Write(Scripts.Render("~/angular/directives"));

            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 8 "..\..\Views\Console\CompanyConsole.cshtml"
Write(Scripts.Render("~/plugins/ui-bootstrap"));

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n<section");

WriteLiteral(" class=\"content-header\"");

WriteLiteral(">\r\n    <h1>\r\n        Company Console\r\n        <small>Year to Date</small>\r\n    </" +
"h1>\r\n    <ol");

WriteLiteral(" class=\"breadcrumb\"");

WriteLiteral(">\r\n        <li>Consoles</li>\r\n        <li");

WriteLiteral(" class=\"active\"");

WriteLiteral("><strong>Company</strong></li>\r\n    </ol>\r\n</section>\r\n\r\n<section");

WriteLiteral(" class=\"content\"");

WriteLiteral(" ng-app=\"CompanyConsole\"");

WriteLiteral(" ng-controller=\"companyController\"");

WriteLiteral(">\r\n    <div");

WriteLiteral(" class=\"row\"");

WriteLiteral(">\r\n        <div");

WriteLiteral(" class=\"col-lg-2 col-md-3\"");

WriteLiteral(">\r\n            <!-- Profile Image -->\r\n            <div");

WriteLiteral(" class=\"box box-primary\"");

WriteLiteral(">\r\n                <div");

WriteLiteral(" class=\"box-body box-profile\"");

WriteLiteral(">\r\n                    <p");

WriteLiteral(" class=\"lead\"");

WriteLiteral("><strong>Product Line Premiums</strong></p>\r\n                    <ul");

WriteLiteral(" class=\"list-group list-group-unbordered\"");

WriteLiteral(">\r\n                        <li");

WriteLiteral(" class=\"list-group-item clearfix\"");

WriteLiteral(" ng-repeat=\"model in productLinePremiumData | orderBy: \'TotalPremium\' : true\"");

WriteLiteral(">\r\n                            <div");

WriteLiteral(" class=\"col-lg-8 col-md-12 no-padding clearfix\"");

WriteLiteral(">\r\n                                <span");

WriteLiteral(" ng-attr-title=\"{{model.ProductLine}}\"");

WriteLiteral("><strong>{{model.ProductLine}}</strong></span>\r\n                            </div" +
">\r\n                            <div");

WriteLiteral(" class=\"col-lg-4 col-md-12 no-padding clearfix text-right\"");

WriteLiteral(@">
                                <a>{{model.TotalPremium | currency:""$"":0}}</a>
                            </div>
                        </li>
                    </ul>
                </div><!-- /.box-body -->
            </div><!-- /.box -->

            <div");

WriteLiteral(" class=\"box box-primary\"");

WriteLiteral(">\r\n                <div");

WriteLiteral(" class=\"box-body\"");

WriteLiteral(">\r\n                    <p");

WriteLiteral(" class=\"lead\"");

WriteLiteral("><strong>Product Line Counts</strong></p>\r\n                    <ul");

WriteLiteral(" class=\"list-group list-group-unbordered\"");

WriteLiteral(">\r\n                        <li");

WriteLiteral(" class=\"list-group-item clearfix\"");

WriteLiteral(" ng-repeat=\"model in productLinePolicyCountData | orderBy: \'TotalRisks\' : true\"");

WriteLiteral(">\r\n                            <div");

WriteLiteral(" class=\"col-lg-8 col-md-12 no-padding clearfix\"");

WriteLiteral(">\r\n                                <span");

WriteLiteral(" ng-attr-title=\"{{model.ProductLine}}\"");

WriteLiteral("><strong>{{model.ProductLine}}</strong></span>\r\n                            </div" +
">\r\n                            <div");

WriteLiteral(" class=\"col-lg-4 col-md-12 no-padding clearfix text-right\"");

WriteLiteral(@">
                                <a>{{model.TotalRisks}}</a>
                            </div>
                        </li>
                    </ul>
                </div><!-- /.box-body -->
            </div><!-- /.box -->
        </div><!-- /.col -->

        <div");

WriteLiteral(" class=\"col-lg-8 col-md-6\"");

WriteLiteral(">\r\n            <div");

WriteLiteral(" class=\"nav-tabs-custom\"");

WriteLiteral(">\r\n                <ul");

WriteLiteral(" class=\"nav nav-tabs\"");

WriteLiteral(">\r\n                    <li");

WriteLiteral(" class=\"active\"");

WriteLiteral("><a");

WriteLiteral(" href=\"#section2tab_1-1\"");

WriteLiteral(" data-toggle=\"tab\"");

WriteLiteral(" aria-expanded=\"true\"");

WriteLiteral(">Top 20 Inforce Policies</a></li>\r\n                    <li");

WriteLiteral(" class=\"\"");

WriteLiteral("><a");

WriteLiteral(" href=\"#section2tab_2-2\"");

WriteLiteral(" data-toggle=\"tab\"");

WriteLiteral(" aria-expanded=\"false\"");

WriteLiteral(">Top 10 Quotes</a></li>\r\n                </ul>\r\n                <div");

WriteLiteral(" class=\"tab-content\"");

WriteLiteral(">\r\n                    <div");

WriteLiteral(" class=\"tab-pane active\"");

WriteLiteral(" id=\"section2tab_1-1\"");

WriteLiteral(">\r\n                        <div");

WriteLiteral(" dx-data-grid=\"topTwentyAccountsSettings\"");

WriteLiteral("></div>\r\n                    </div><!-- /.tab-pane -->\r\n                    <div");

WriteLiteral(" class=\"tab-pane\"");

WriteLiteral(" id=\"section2tab_2-2\"");

WriteLiteral(">\r\n                        <div");

WriteLiteral(" dx-data-grid=\"topTenQuotesSettings\"");

WriteLiteral("></div>\r\n                    </div><!-- /.tab-pane -->\r\n                </div><!-" +
"- /.tab-content -->\r\n            </div>\r\n\r\n            <div");

WriteLiteral(" class=\"box box-primary\"");

WriteLiteral(">\r\n                <div");

WriteLiteral(" class=\"box-header with-border\"");

WriteLiteral(">\r\n                    <h3");

WriteLiteral(" class=\"box-title\"");

WriteLiteral(">Budget/Goal Projection</h3>\r\n                    <div");

WriteLiteral(" class=\"pull-right\"");

WriteLiteral(">\r\n                        <select");

WriteLiteral(" ng-model=\"budgetTimeframe\"");

WriteLiteral(" ng-change=\"budgetTimeframeChange()\"");

WriteLiteral(" class=\"form-control\"");

WriteLiteral(">\r\n                            <option");

WriteLiteral(" value=\"Monthly\"");

WriteLiteral(">Monthly</option>\r\n                            <option");

WriteLiteral(" value=\"Yearly\"");

WriteLiteral(">Yearly</option>\r\n                        </select>\r\n                    </div>\r\n" +
"                    <div");

WriteLiteral(" class=\"pull-right\"");

WriteLiteral(" style=\"font-size:25px;margin-right:10px\"");

WriteLiteral("><i");

WriteLiteral(" class=\"fa fa-adjust\"");

WriteLiteral(" title=\"Show Details\"");

WriteLiteral(" ng-click=\"showDetails = !showDetails\"");

WriteLiteral(" style=\"cursor:pointer\"");

WriteLiteral("></i></div>\r\n                </div><!-- /.box-header -->\r\n                <div");

WriteLiteral(" class=\"box-body\"");

WriteLiteral(">\r\n                    <loading-indicator");

WriteLiteral(" ng-show=\"loadingBudget\"");

WriteLiteral("></loading-indicator>\r\n                    <div");

WriteLiteral(" ng-show=\"!loadingBudget\"");

WriteLiteral(">\r\n                        <table");

WriteLiteral(" class=\"table\"");

WriteLiteral(" ng-show=\"showDetails\"");

WriteLiteral(">\r\n                            <tbody>\r\n                                <tr>\r\n   " +
"                                 <th>Product Line</th>\r\n                        " +
"            <th");

WriteLiteral(" colspan=\"5\"");

WriteLiteral("></th>\r\n                                    <th>% of Budget</th>\r\n               " +
"                 </tr>\r\n                                <tr");

WriteLiteral(" ng-repeat=\"budget in budgetData\"");

WriteLiteral(">\r\n                                    <td");

WriteLiteral(" style=\"width:170px\"");

WriteLiteral(">\r\n                                        {{budget.ProductLine}}\r\n              " +
"                      </td>\r\n                                    <td");

WriteLiteral(" colspan=\"5\"");

WriteLiteral(">\r\n                                        <uib-progressbar");

WriteLiteral(" class=\"progress\"");

WriteLiteral(" value=\"budget.budgetPercent\"");

WriteLiteral(" type=\"primary\"");

WriteLiteral("></uib-progressbar>\r\n                                    </td>\r\n                 " +
"                   <td");

WriteLiteral(" style=\"width:80px\"");

WriteLiteral(">\r\n                                        <span");

WriteLiteral(" class=\"badge bg-light-blue\"");

WriteLiteral(">{{budget.budgetPercent | number : 0}}%</span>\r\n                                 " +
"   </td>\r\n                                </tr>\r\n                            </t" +
"body>\r\n                        </table>\r\n\r\n                        <table");

WriteLiteral(" class=\"table\"");

WriteLiteral(" ng-show=\"!showDetails\"");

WriteLiteral(@">
                            <tbody>
                                <tr>
                                    <th>Product Line</th>
                                    <th>Prior Year Written</th>
                                    <th>Current Written</th>
                                    <th>Quoted</th>
                                    <th>Total</th>
                                    <th>Budget</th>
                                    <th>% of Budget</th>
                                </tr>
                                <tr");

WriteLiteral(" ng-repeat=\"budget in budgetData\"");

WriteLiteral(@">
                                    <td>{{budget.ProductLine}}</td>
                                    <td>{{budget.Actual | currency:""$"":0}}</td>
                                    <td>{{budget.Current | currency:""$"":0}}</td>
                                    <td>{{budget.CurrentOutstanding | currency:""$"":0}}</td>
                                    <td>{{budget.Total | currency:""$"":0}}</td>
                                    <td>{{budget.Budget | currency:""$"":0}}</td>
                                    <td>{{budget.budgetPercent | number : 2}}%</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div><!-- /.box-body -->
            </div>
        </div>

        <div");

WriteLiteral(" class=\"col-lg-2 col-md-3\"");

WriteLiteral(">\r\n            <div");

WriteLiteral(" class=\"box box-primary\"");

WriteLiteral(">\r\n                <div");

WriteLiteral(" class=\"box-body box-profile\"");

WriteLiteral(">\r\n                    <h3><strong>Renewal Retention</strong> <br/> <small>(Rolli" +
"ng 12)</small></h3>\r\n                    <ul");

WriteLiteral(" class=\"list-group list-group-unbordered\"");

WriteLiteral(">\r\n                        <li");

WriteLiteral(" class=\"list-group-item clearfix\"");

WriteLiteral(">\r\n                            <div");

WriteLiteral(" class=\"col-lg-12 col-md-12 col-sm-12 no-padding clearfix\"");

WriteLiteral(">\r\n                                <span><strong>By Count</strong></span>\r\n      " +
"                      </div>\r\n                            <table");

WriteLiteral(" class=\"table table-condensed\"");

WriteLiteral(" style=\"margin-bottom:0px\"");

WriteLiteral(">\r\n                                <tbody>\r\n                                    <" +
"tr>\r\n                                        <td><uib-progressbar");

WriteLiteral(" class=\"progress progress-xs progress-striped\"");

WriteLiteral(" value=\"retention.TotalRetainedAsPercent\"");

WriteLiteral(" type=\"{{retention.TotalRetainedGrade}}\"");

WriteLiteral("></uib-progressbar></td>\r\n                                        <td");

WriteLiteral(" style=\"width:40px\"");

WriteLiteral("><span");

WriteLiteral(" ng-class=\"retention.TotalRetainedBadgeGrade\"");

WriteLiteral(" class=\"badge\"");

WriteLiteral(">{{retention.TotalRetainedAsPercent}}%</span></td>\r\n                             " +
"       </tr>\r\n                                </tbody>\r\n                        " +
"    </table>\r\n                        </li>\r\n                        <li");

WriteLiteral(" class=\"list-group-item clearfix\"");

WriteLiteral(">\r\n                            <div");

WriteLiteral(" class=\"col-lg-12 col-md-12 col-sm-12 no-padding clearfix\"");

WriteLiteral(">\r\n                                <span><strong>By Premium</strong></span>\r\n    " +
"                        </div>\r\n                            <table");

WriteLiteral(" class=\"table table-condensed\"");

WriteLiteral(" style=\"margin-bottom:0px\"");

WriteLiteral(">\r\n                                <tbody>\r\n                                    <" +
"tr>\r\n                                        <td><uib-progressbar");

WriteLiteral(" class=\"progress progress-xs progress-striped\"");

WriteLiteral(" value=\"retention.ValueRetainedAsPercent\"");

WriteLiteral(" type=\"{{retention.TotalValueRetainedGrade}}\"");

WriteLiteral("></uib-progressbar></td>\r\n                                        <td");

WriteLiteral(" style=\"width:40px\"");

WriteLiteral("><span");

WriteLiteral(" ng-class=\"retention.TotalValueRetainedBadgeGrade\"");

WriteLiteral(" class=\"badge\"");

WriteLiteral(@">{{retention.ValueRetainedAsPercent}}%</span></td>
                                    </tr>
                                </tbody>
                            </table>
                        </li>
                    </ul>
                </div><!-- /.box-body -->
            </div>

            <div");

WriteLiteral(" class=\"box box-primary\"");

WriteLiteral(">\r\n                <div");

WriteLiteral(" class=\"box-body box-profile\"");

WriteLiteral(">\r\n                    <h3><strong>PL Growth</strong> <br /> <small>(Prior vs. Cu" +
"rrent)</small></h3>\r\n                    <ul");

WriteLiteral(" class=\"list-group list-group-unbordered\"");

WriteLiteral(">\r\n                        <li");

WriteLiteral(" class=\"list-group-item clearfix\"");

WriteLiteral(" ng-repeat=\"model in growthData | orderBy: \'TotalPremium\' : true\"");

WriteLiteral(">\r\n                            <div");

WriteLiteral(" class=\"col-lg-12 col-md-12 no-padding clearfix\"");

WriteLiteral(">\r\n                                <span");

WriteLiteral(" ng-attr-title=\"{{model.ProductLine}}\"");

WriteLiteral("><strong>{{model.ProductLine | limitTo : 20 : 0}}</strong></span>\r\n              " +
"              </div>\r\n                            <div");

WriteLiteral(" class=\"col-lg-6 col-md-6 no-padding clearfix\"");

WriteLiteral(">\r\n                                <a");

WriteLiteral(" title=\"Prior\"");

WriteLiteral(">{{model.CurrentAmount | currency:\"$\":0}}</a>\r\n                            </div>" +
"\r\n                            <div");

WriteLiteral(" class=\"col-lg-6 col-md-6 no-padding clearfix text-right\"");

WriteLiteral(">\r\n                                <a");

WriteLiteral(" title=\"Current\"");

WriteLiteral(">{{model.PriorAmount | currency:\"$\":0}}</a>\r\n                            </div>\r\n" +
"                        </li>\r\n                    </ul>\r\n                </div>" +
"<!-- /.box-body -->\r\n            </div>\r\n        </div>\r\n    </div>\r\n</section>");

        }
    }
}
#pragma warning restore 1591

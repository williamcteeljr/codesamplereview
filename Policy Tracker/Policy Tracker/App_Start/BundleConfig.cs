using System.Configuration;
using System.Web;
using System.Web.Optimization;


public class BundleConfig
{
    // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
    public static void RegisterBundles(BundleCollection bundles)
    {
        BundleTable.EnableOptimizations = true;

        // App Controllers
        bundles.Add(new ScriptBundle("~/angular/controllers").Include(
            "~/scripts/angular/UnderwriterConsole.js",
            "~/scripts/angular/BranchConsole.js",
            "~/scripts/angular/CompanyConsole.js",
            "~/scripts/angular/ProductLineConsole.js",
            "~/scripts/angular/ProductLineMaster.js",
            "~/scripts/angular/Reporting.js"
            ));

        // App Directives
        bundles.Add(new ScriptBundle("~/angular/directives").Include("~/scripts/angular/directives.js"));

        // App Factories
        bundles.Add(new ScriptBundle("~/angular/factories").Include("~/scripts/angular/factory.js"));

        // App Filters
        bundles.Add(new ScriptBundle("~/angular/filters").Include("~/scripts/angular/filters.js"));

        // App Filters
        bundles.Add(new ScriptBundle("~/devextreme/jszip").Include("~/scripts/jszip.min.js"));

        // UI-Bootstrap : AngularJS Bootstrap components for building UIs
        bundles.Add(new ScriptBundle("~/plugins/ui-bootstrap").Include("~/scripts/plugins/ui-bootstrap-tpls-0.14.3.min.js"));

        // FCSA Number : AngularJS Numeric Only Input
        bundles.Add(new ScriptBundle("~/plugins/fcsaNumber").Include("~/Scripts/plugins/fcsaNumber.js"));

        bundles.Add(new ScriptBundle("~/bundles/sub").Include(
            "~/Content/jquery-ui/jquery-ui.js",

            "~/jqGrid/js/i18n/grid.locale-en.js",
            "~/jqGrid/js/jquery.jqGrid.src.js",

            "~/scripts/ORA/ora.js",
            "~/scripts/ORA/ora.Dialog.js",
            "~/scripts/ORA/ora.Utilities.js",
            "~/scripts/ORA/ora.UI.js",
            "~/scripts/ORA/ora.Risk.js",
            "~/scripts/ORA/ora.DX.js",
            "~/scripts/ORA/ora.Ajax.js",
            "~/scripts/ORA/ora.jQueryPlugins.js",

            "~/scripts/AdminLTE/app.min.js",

            "~/scripts/lib/jquery.are-you-sure.js",
            "~/scripts/lib/jquery.numeric.js",
            "~/scripts/lib/bootstrap.js",
            "~/scripts/lib/metisMenu.js",
            "~/scripts/lib/sugar.js",
            "~/scripts/lib/jquery.barrating.min.js"
            ));

        bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                    "~/Scripts/jquery-ui-{version}.js"));

        bundles.Add(new ScriptBundle("~/bundles/devextreme").Include(
            "~/scripts/dx.viz-web.js"));

        // Use the development version of Modernizr to develop with and learn from. Then, when you're
        // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
        bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                    "~/Scripts/modernizr-*"));

        bundles.Add(new StyleBundle("~/Content/css")
            .Include("~/Content/bootstrap.css")
            .Include("~/Content/metisMenu.css")
            .Include("~/Content/AdminLTE/skin-blue.min.css")
            .Include("~/Content/AdminLTE/AdminLTE.min.css")
            .Include("~/Content/site.css")
            .Include("~/Content/jquery-ui/jquery-ui.css")
            .Include("~/Content/jQuery-UI-Mods.css")
            .Include("~/Content/FormLayout.css")
            .Include("~/Content/dx.common.css")
            .Include("~/Content/dx.light.css")
        );

        bundles.Add(new StyleBundle("~/Content/font-awesome/css")
            .Include("~/Content/font-awesome/css/font-awesome.css")
            .Include("~/Content/fontawesome-stars.css")
        );

        bundles.Add(new StyleBundle("~/Content/Errors/css")
            .Include("~/Content/bootstrap.css")
            .Include("~/Content/AdminLTE/skin-blue.min.css")
            .Include("~/Content/AdminLTE/AdminLTE.min.css")
            .Include("~/Content/font-awesome/css/font-awesome.css")
        );

        bundles.Add(new StyleBundle("~/jqGrid/css")
            .Include("~/jqGrid/css/ui.jqgrid.css")
            .Include("~/jqGrid/css/ui.jqgrid-bootstrap.css")
        );

        bundles.Add(new ScriptBundle("~/bundles/scripts/jquery").Include(
            "~/Scripts/jquery-2.1.4.min.js",
            "~/Scripts/jquery.globalize/globalize.js"
            ));

        bundles.Add(new ScriptBundle("~/bundles/scripts/jquery/validation").Include(
                    "~/Scripts/jquery.unobtrusive*",
                    "~/Scripts/jquery.validate*"));
    }
}
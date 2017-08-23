using DevExpress.Web.ASPxHtmlEditor;
using DevExpress.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PolicyTracker.DX
{
    public static class DXHelperSettings
    {
        public static HtmlEditorSettings HtmlEditorExportSettings(string html)
        {
            HtmlEditorSettings settings = new HtmlEditorSettings();
            settings.Name = "StatsExport";
            settings.CallbackRouteValues = new { Controller = "Home", Action = "ExportStatChart" };
            settings.ExportRouteValues = new { Controller = "Home", Action = "ExportStatChartTo" };
            settings.Width = System.Web.UI.WebControls.Unit.Percentage(100);

            settings.ActiveView = HtmlEditorView.Design;
            settings.Settings.AllowHtmlView = false;
            settings.Settings.AllowDesignView = true;

            var toolbar = settings.Toolbars.Add();
            ToolbarExportDropDownButton saveButton = new ToolbarExportDropDownButton(true);
            saveButton.CreateDefaultItems();
            toolbar.Items.Add(saveButton);

            //settings.CssFiles.Add("~/Content/HtmlEditor/DemoCss/Export.css");
            settings.Html = html;
            settings.CssFiles.Add("~/Content/bootstrap.css");
            settings.CssFiles.Add("~/Content/site.css");
            settings.CssFiles.Add("~/Content/sb-admin-2.css");

            return settings;
        }
    }
}
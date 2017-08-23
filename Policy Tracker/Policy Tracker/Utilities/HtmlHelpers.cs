using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace PolicyTracker.HtmlHelpers
{ 
    public static class HelperExtensions
    {
        /// <summary>
        /// Returns the Html needed to create a jqGrid and pager
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="gridId">ID of the Grid. The word "Grid" will be added to the end as well.</param>
        /// <param name="pager">Determine if a pager html should be output as well. If so the id of the pager will be the grid ID + "GridPager"</param>
        /// <returns></returns>
        public static MvcHtmlString jqGrid(this HtmlHelper helper, string gridId, bool pager)
        {
            MvcHtmlString results = new MvcHtmlString(pager == true ? "<table id=\"" + gridId + "\"><tr><td/></tr></table><div id=\"" + gridId + "Pager\"></div>" : "<table id=\"" + gridId + "\"><tr><td/></tr></table>");
            return (results);
        }

        public static MvcHtmlString RawActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, AjaxOptions ajaxOptions, object routeValues = null, object htmlAttributes = null)
        {
            var repID = Guid.NewGuid().ToString();
            var lnk = ajaxHelper.ActionLink(repID, actionName, controllerName, routeValues, ajaxOptions, htmlAttributes);
            return MvcHtmlString.Create(lnk.ToString().Replace(repID, linkText));
        }

        public static MvcHtmlString Button(this HtmlHelper helper, string id, object htmlAttributes = null)
        {
            string innerHtml = id;
            if (htmlAttributes == null) htmlAttributes = new { };
            return Button(helper, id, innerHtml, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString Button(this HtmlHelper helper, string id, string innerHtml, object htmlAttributes)
        {
            return Button(helper, id, innerHtml, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString Button(this HtmlHelper helper, string id, string innerHtml, IDictionary<string, object> htmlAttributes)
        {
            var builder = new TagBuilder("button");
            htmlAttributes.Add(new KeyValuePair<string, object>("id", id));
            htmlAttributes.Add(new KeyValuePair<string, object>("tabindex", -1));
            htmlAttributes.Add(new KeyValuePair<string, object>("type", "button"));

            if (htmlAttributes.ContainsKey("class"))
            {
                htmlAttributes["class"] = htmlAttributes["class"] + " pt-input actionButton";
            }
            else
            {
                htmlAttributes.Add(new KeyValuePair<string, object>("class", "pt-input actionButton"));
            }

            builder.InnerHtml = innerHtml;
            builder.MergeAttributes(htmlAttributes);

            return MvcHtmlString.Create(builder.ToString());
        }
    }

    public static class WebAppDisplay
    {
        public static string GetDateDisplayValue(DateTime? date)
        {
            string result = String.Empty;

            if (date != null)
            {
                result = date.Value.ToShortDateString();
            }

            return result;
        }
    }
}
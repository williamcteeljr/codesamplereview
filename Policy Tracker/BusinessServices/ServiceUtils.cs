using DevExpress.Web;
using DevExpress.Web.Mvc;
using PolicyTracker.Platform.Utilities;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.Platform.Caching;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTracker.BusinessServices
{
    public static class ServiceUtils
    {
        public static List<PropertyFilter> GetFiltersFromGrid<T>(GridViewColumnStateCollection columns) where T : BaseEntity
        {
            var filters = new List<PropertyFilter>();
            var customArgs = (List<PropertyFilter>)CacheManager.RequestCache.GetValue("CustomArgs");
            if (customArgs != null) filters.AddRange(customArgs);
            var properties = typeof(T).GetProperties();

            foreach (var c in columns)
            {
                if (c.FilterExpression != null && !String.IsNullOrEmpty(c.FilterExpression))
                {
                    PropertyFilter filter = null;
                    var prop = properties.Where(x => x.Name == c.FieldName).First();
                    var type = prop.GetType().GetType();

                    string[] stringSeparators = new string[] {" And ", " Or "};

                    var filterInstances = c.FilterExpression.Split(stringSeparators, StringSplitOptions.None);

                    if (filterInstances.Length == 2)
                    {
                        var operand = GetComparatorFromDXFilterExpression(filterInstances[0]);
                        var val1 = GetValueFromDXFilterExpression(filterInstances[0], prop.PropertyType, operand);
                        operand = GetComparatorFromDXFilterExpression(filterInstances[1]);
                        var val2 = GetValueFromDXFilterExpression(filterInstances[1], prop.PropertyType, operand);
                        filter = ObjectUtils.GetPropertyFilter(c.FieldName, prop.PropertyType, "Between", val1, val2);
                        filters.Add(filter);
                    }
                    else
                    {
                        foreach (var filterInstance in filterInstances)
                        {
                            var operand = GetComparatorFromDXFilterExpression(filterInstance);
                            var val = GetValueFromDXFilterExpression(filterInstance, prop.PropertyType, operand);
                            if (filterInstance.Contains("Contains")) val = val.ToString().Split(',');
                            if (val != null)
                            {
                                filter = ObjectUtils.GetPropertyFilter(c.FieldName, prop.PropertyType, operand, val);
                                filters.Add(filter);
                            }
                        }
                    }
                }
            }

            return filters;
        }

        public static object GetValueFromDXFilterExpression(string expression, Type type, string operand)
        {
            object val = null;

            string[] opGroup = new[] { "LessEquals", "Less", "Equals", "NotEquals", "Greater", "GreaterEquals" };

            if (opGroup.Contains(operand))
            {
                val = expression.Split(' ')[2];
            }
            else
            {
                var beginningIndex = expression.IndexOf('\'') + 1;
                var lastIndex = expression.LastIndexOf('\'');
                var lengthBetween = lastIndex - beginningIndex;

                val = expression.Substring(beginningIndex, lengthBetween);
                if (type == typeof(String))
                    val = val.ToString().Replace("''", "'");
            }

            return val;
        }

        public static string GetComparatorFromDXFilterExpression(string exp)
        {
            var op = "Equals";

            if (exp.Contains("Contains")) op = "In";
            else if (exp.Contains("StartsWith")) op = "StartsWith";
            else if (exp.Contains("Not Contains")) op = "NotIn";
            else if (exp.Contains(">")) op = "Greater";
            else if (exp.Contains(">=")) op = "GreaterEquals";
            else if (exp.Contains("<")) op = "Less";
            else if (exp.Contains("<=")) op = "LessEquals";

            return op;
        }

        public static List<PropertyFilter> GetFiltersForExport(IList<GridViewDataColumn> columns)
        {
            var filters = new List<PropertyFilter>();

            foreach (var c in columns)
            {
                if (c.FilterExpression != null && !String.IsNullOrEmpty(c.FilterExpression))
                {
                    var beginningIndex = c.FilterExpression.IndexOf('\'') + 1;
                    var lastIndex = c.FilterExpression.LastIndexOf('\'');
                    var lengthBetween = lastIndex - beginningIndex;
                    filters.Add(new PropertyFilter(c.FieldName, PropertyFilter.Comparator.StartsWith, c.FilterExpression.Substring(beginningIndex, lengthBetween)));
                }
            }

            return filters;
        }
    }
}

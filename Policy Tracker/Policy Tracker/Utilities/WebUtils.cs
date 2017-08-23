using DevExpress.Web.Mvc;
using PolicyTracker.Platform.Utilities;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.Platform.Caching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PolicyTracker.Utilities
{
    public static class DevExUtils
    {
        public static string GetListBoxValuesFromFilterExpression(GridViewModel model, string field)
        {
            string result = "";
            var exp = model.Columns.Where(x => x.FieldName == field).First().FilterExpression;
            if (exp.Length > 0)
            {
                var beginningIndex = exp.IndexOf('\'') + 1;
                var lastIndex = exp.LastIndexOf('\'');
                var lengthBetween = lastIndex - beginningIndex;

                result = exp.Substring(beginningIndex, lengthBetween);
            }

            return result;
        }
    }

    public static class WebUtils
    {
        public static List<KeyValuePair<String, String>> FormToList(NameValueCollection form)
        {
            var list = new List<KeyValuePair<String, String>>();

            foreach (var key in form.AllKeys)
            {
                list.Add(new KeyValuePair<string, string>(key, form[key]));
            }

            return list;
        }

        public static void BuildCustomArgs(NameValueCollection requestParams)
        {
            var customArgs = new List<PropertyFilter>();

            foreach (var key in requestParams.AllKeys)
            {
                var keyValue = key.ToString();
                if (keyValue.StartsWith("CustomArg"))
                {
                    var field = keyValue.Replace("CustomArg_", "");
                    var operand = requestParams[keyValue].Split('|')[0];
                    object value = requestParams[keyValue].Split('|')[1];
                    if (operand == "In") value = value.ToString().Split(',');

                    var filter = GetCustomArgPropertyFilter(field, operand, value);
                    customArgs.Add(filter);
                }
                CacheManager.RequestCache.SetValue("CustomArgs", customArgs);
            }
        }

        public static PropertyFilter GetCustomArgPropertyFilter(string field, string comparer, object val)
        {
            switch (comparer)
            {
                case "In":
                    return new PropertyFilter(field, PropertyFilter.Comparator.In, val);
                case "Eq":
                    return new PropertyFilter(field, val);
                case "Le":
                    return new PropertyFilter(field, PropertyFilter.Comparator.LessEquals, val);
                default:
                    return new PropertyFilter(field, val);
            }
        }

        public static List<PropertyFilter> ConvertRequestParamsToPropertyFilters(Type referenceType, IEnumerable<KeyValuePair<String, String>> stringParams)
        {
            List<PropertyFilter> filters = new List<PropertyFilter>();
            foreach (var param in stringParams)
            {
                if (!String.IsNullOrEmpty(param.Value))
                {
                    PropertyInfo prop = referenceType.GetProperties().Where(p => p.Name.Equals(param.Key, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
                    if (prop != null)
                    {
                        if (prop.PropertyType == typeof(System.String))
                        {
                            string value = param.Value.ToString();
                            filters.Add(new PropertyFilter(prop.Name, PropertyFilter.Comparator.StartsWith, value));
                        }
                        if (prop.PropertyType == typeof(Int32))
                        {
                            string value = param.Value.ToString();
                            filters.Add(new PropertyFilter(prop.Name, PropertyFilter.Comparator.StartsWith, value));
                        }

                        else if (prop.PropertyType == typeof(long))
                        {
                            long value;
                            if (!Int64.TryParse(param.Value, out value)) value = Int64.MaxValue;
                            filters.Add(new PropertyFilter(prop.Name, PropertyFilter.Comparator.StartsWith, value));
                        }
                        else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                        {
                            DateTime? value = null;
                            //if (!DateTime.TryParse(param.Value, out value)) value = Response4W.DefaultDate;
                            filters.Add(new PropertyFilter(prop.Name, value));
                        }
                        else if (prop.PropertyType == null)
                        {
                            //dont add it to the list
                        }
                    }
                }
            }
            return filters;
        }

        public static T FormDataToModel<T>(T model, NameValueCollection formData, bool isNestedObject = false)
        {
            //var inputs = formData.Cast<string>().Select(s => new { Key = s.Split('.').LastOrDefault(), Value = formData[s] });
            var inputs = formData.Cast<string>().Select(s => new { Key = s, Value = formData[s] });
            var formDictionary = inputs.ToDictionary(p => p.Key, p => p.Value);

            Type referenceType = model.GetType();

            foreach (var data in formDictionary)
            {
                PropertyInfo property = ObjectUtils.GetNestedProperty(model, data.Key);
                if (property != null)
                {
                    if (data.Key != null)
                    {
                        var propertyName = data.Key.Split('.').Last();
                        var DXValueInput = formDictionary.Where(x => x.Key == data.Key + "_VI").FirstOrDefault();
                        object value = (DXValueInput.Key != null) ? DXValueInput.Value : data.Value;
                        Type propType = ObjectUtils.GetNestedProperty(model, data.Key).PropertyType;

                        if (value != null)
                        {
                            if (propType == typeof(String))
                            {
                                if (property.GetCustomAttributes(typeof(PhoneNumber), false).Any())
                                {
                                    value = Regex.Replace(value.ToString(), "[^0-9]", "");
                                }
                                ObjectUtils.SetProperty(data.Key, model, value);
                            }
                            else if (propType == typeof(Int32))
                            {
                                try
                                {
                                    ObjectUtils.SetProperty(data.Key, model, Convert.ToInt32(Regex.Replace(value.ToString(), @"[^\d]", String.Empty)));
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                            else if (propType == typeof(Int64))
                            {
                                ObjectUtils.SetProperty(data.Key, model, Convert.ToInt64(Regex.Replace(value.ToString(), @"[^\d]", String.Empty)));
                            }
                            else if (propType == typeof(Single))
                            {
                                ObjectUtils.SetProperty(data.Key, model, Convert.ToSingle(value));
                            }
                            else if (propType == typeof(System.DateTime?))
                            {
                                if (String.IsNullOrEmpty((string)value)) ObjectUtils.SetProperty(data.Key, model, null);
                                else ObjectUtils.SetProperty(data.Key, model, Convert.ToDateTime(value));
                            }
                            else if (propType == typeof(Decimal))
                            {
                                ObjectUtils.SetProperty(data.Key, model, Convert.ToDecimal(value));
                            }
                            else if (propType == typeof(Boolean))
                            {
                                if (data.Value.IndexOf("true") > -1) ObjectUtils.SetProperty(data.Key, model, true);
                                else ObjectUtils.SetProperty(data.Key, model, Convert.ToBoolean(value));
                            }
                            else if (propType.BaseType == typeof(StringEnum))
                            {
                                var method = propType.BaseType.GetMethod("Parse");
                                var typeMethod = method.MakeGenericMethod(propType);
                                var result = typeMethod.Invoke(null, new[] { value });

                                if (!String.IsNullOrEmpty(value.ToString())) ObjectUtils.SetProperty(data.Key, model, result);
                            }
                            else if (typeof(IEnumerable).IsAssignableFrom(propType))
                            {
                                if (data.Key.ToString() == "MainLocationsSelectedItemsIds") { }//skip
                                else
                                {
                                    ObjectUtils.SetProperty(data.Key, model, Convert.ToString(value).Split(','));
                                }
                            }
                            else ObjectUtils.SetProperty(data.Key, model, value);
                        }
                    }
                }
            }
            return model;
        }
    }
}
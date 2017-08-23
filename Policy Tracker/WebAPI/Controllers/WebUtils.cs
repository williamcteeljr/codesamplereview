using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PolicyTracker.DomainModel;
using System.Web.UI.WebControls;
using System.Web.Mvc;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Framework;
using System.Reflection;
using System.Collections.Specialized;
using PolicyTracker.Platform.Utilities;
using System.Collections;
using System.Text.RegularExpressions;
using PolicyTracker.Platform.Caching;
using System.IO;
using System.Text;

namespace PolicyTracker.Utilities
{

    public static class WebAPIUtilities
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
                            filters.Add(new PropertyFilter(prop.Name, value));
                        }

                        else if (prop.PropertyType == typeof(long))
                        {
                            long value;
                            if (!Int64.TryParse(param.Value, out value)) value = Int64.MaxValue;
                            filters.Add(new PropertyFilter(prop.Name, PropertyFilter.Comparator.StartsWith, value));
                        }
                        else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                        {
                            var dateArray = param.Value.Split('|');
                            if (dateArray.Length > 1)
                            {
                                DateTime startDate;
                                DateTime endDate;
                                bool success;

                                success = DateTime.TryParse(dateArray[0], out startDate);
                                success = DateTime.TryParse(dateArray[1], out endDate);

                                if (success)
                                    filters.Add(new PropertyFilter(prop.Name, startDate, endDate));
                            }
                            else
                            {
                                DateTime value;
                                var success = DateTime.TryParse(param.Value, out value);
                                if (success)
                                    filters.Add(new PropertyFilter(prop.Name, value));
                            }
                            
                        }
                        else if (prop.PropertyType == typeof(Boolean))
                        {
                            var value = Convert.ToBoolean(param.Value);
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

        public static T FormDataToModel<T>(T model, NameValueCollection formData)
        {
            var inputs = formData.Cast<string>().Select(s => new { Key = s, Value = formData[s] }).Where(x => x.Key != null && x.Key != String.Empty);
            var formDictionary = inputs.ToDictionary(p => p.Key, p => p.Value);

            foreach (var data in formDictionary)
            {
                PropertyInfo property = ObjectUtils.GetNestedProperty(model, data.Key);

                // Property == null when form input is not a property of the model type
                if (property != null)
                {
                    var propertyName = data.Key.Split('.').Last();

                    /* For users of DevExtreme
                     * DevExtreme injects a lot of additional HTML to output a simple textbox. The actual input eventaully is given a suffix of _VI to indicate the value input
                     */
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
                                
                            ObjectUtils.SetProperty(data.Key, model, Convert.ToInt32(value));
                        }
                        else if (propType == typeof(Int64))
                        {
                            ObjectUtils.SetProperty(data.Key, model, Convert.ToInt64(value));
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
                            ObjectUtils.SetProperty(data.Key, model, Convert.ToString(value).Split(','));
                        }
                        else ObjectUtils.SetProperty(data.Key, model, value);
                    }
                }
            }
            return model;
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace PolicyTracker.DomainModel.Framework
{
    public static class DomainValidator
    {
        public static void Validate()
        {
            //LogManager.Log(LogLevel.INFO, "Validating Domain Component");
            var domainAssembly = Assembly.GetExecutingAssembly();
            foreach (Type t in domainAssembly.GetTypes())
            {
                // Only check instances of BaseEntity
                if (t.IsSubclassOf(typeof(BaseEntity)))
                {
                    var properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    foreach (var prop in properties)
                    {
                        // Check for missing StringLength attributes
                        if (prop.PropertyType == typeof(String))
                        {
                            var strengthLengthAttribute = prop.GetCustomAttribute(typeof(StringLengthAttribute));
                            if (strengthLengthAttribute == null)
                            {
                                //LogManager.Log(LogLevel.WARN, "Type [{0}] is missing StringLength attribute for Property [{1}]", t.Name, prop.Name);
                            }
                        }
                    }

                    //if (!hasRequiredAttribute) LogManager.Log(LogLevel.WARN, "Type [{0}] does not have a Required property", t.Name);
                }
            }
        }
    }

    public interface ISelfTracking : IChangeTracking
    {
        // Empty Interface Used for Behavior
    }

    public abstract class AuditedEntity : BaseEntity
    {
        [StringLength(30)]
        public string CreatedBy { get; set; }
        [StringLength(30)]
        public string LastEdit { get; set; }
        [StringLength(30)]
        public string PriorEdit { get; set; }
    }

    public abstract class BaseEntity
    {
        private HashSet<string> _ChangedProperties = new HashSet<string>();

        // This method creates a shallow copy by creating a new object.
        // - The non-static fields of the current object are copied to the new object.
        // - If a field is a value type, a bit-by-bit copy of the field is performed.
        // - If a field is a reference type, the reference is copied but the referred object is not; therefore, the original object and its clone refer to the same object.
        public BaseEntity Clone()
        {
            return (BaseEntity)this.MemberwiseClone();
        }

        // This method creates a copy of the object, using the following process:
        // - Creates a new object instance from the type
        // - Enumerate though all public instance properties that are read/write
        // - Set value on new instance read from old instance
        public BaseEntity Copy()
        {
            Type t = this.GetType();
            BaseEntity copy = Activator.CreateInstance(t) as BaseEntity;
            var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanRead && x.CanWrite);
            foreach (var prop in properties)
            {
                prop.SetValue(copy, prop.GetValue(this));
            }

            return copy;
        }

        public void SetPropertyChanged(string name)
        {
            _ChangedProperties.Add(name);
        }

/*
        protected void SetProperty<T>(ref T oldValue, T newValue, string name)
        {
            if (ReferenceEquals(oldValue, newValue)) return;
            oldValue = newValue;
            _ChangedProperties.Add(name);
        }
*/

        // Source:  http://danrigby.com/2012/03/01/inotifypropertychanged-the-net-4-5-way/
        protected void SetProperty<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (Equals(oldValue, newValue)) return;
            oldValue = newValue;
            _ChangedProperties.Add(propertyName);
        }

        // IChangeTracking.IsChanged
        [IgnoreDataMember]
        public bool IsChanged
        {
            get { return _ChangedProperties.Count > 0; }
        }

        // IChangeTracking.AcceptChanges()
        public void AcceptChanges()
        {
            _ChangedProperties.Clear();
        }

        [IgnoreDataMember]
        public IEnumerable<string> ChangedProperties
        {
            get { return _ChangedProperties; }
        }
    }

    public class PaginatedList<T>
    {
        public PaginatedList() { }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalResults { get; set; }
        public IEnumerable<T> Results { get; set; }
    }

    public class PaginationCriteria
    {
        public static PaginationCriteria Default
        {
            get
            {
                return new PaginationCriteria()
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
            }
        }

        public PaginationCriteria() { }

        public PaginationCriteria(int size, int page, OrderFilter order = null)
        {
            PageSize = size;
            PageNumber = page;
            Filters = new List<PropertyFilter>();
            OrderFilters = new[] { order };
        }

        public PaginationCriteria(int size, int page, List<OrderFilter> order = null)
        {
            PageSize = size;
            PageNumber = page;
            Filters = new List<PropertyFilter>();
            OrderFilters = order;
        }

        public PaginationCriteria(int beginningIndex, int endIndex, string filterValue, OrderFilter order = null)
        {
            PageSize = (endIndex + 1) - beginningIndex;
            PageNumber = (endIndex + 1) / PageSize;
            Filters = new List<PropertyFilter>();
            OrderFilters = new[] { order };
        }

        public int PageSize { get; private set; }
        public int PageNumber { get; private set; }
        public List<PropertyFilter> Filters { get; set; }
        public IEnumerable<OrderFilter> OrderFilters { get; private set; }
    }

    public class PropertyFilter
    {
        public enum Comparator { LessEquals, Less, Equals, NotEquals, Greater, GreaterEquals, StartsWith, Like, In, NotIn, Between }

        public static PropertyFilter RecordId(long value)
        {
            return new PropertyFilter("RecordId", value);
        }

        public PropertyFilter() { }

        public PropertyFilter(string propName, object val) : this(propName, Comparator.Equals, val) {}

        public PropertyFilter(string propName, Comparator comp, object val, int group)
        {
            PropertyName = propName;
            Operand = comp;
            Value = val;
            Group = group;
        }

        public PropertyFilter(string propName, Comparator comp, object val)
        {
            PropertyName = propName;
            Operand = comp;
            Value = val;
            Group = 0;
        }

        public PropertyFilter(string propName, object val1, object val2)
        {
            PropertyName = propName;
            Operand = Comparator.Between;
            Value = val1;
            Value2 = val2;
            Group = 0;
        }

        public PropertyFilter(string propName, object val1, object val2, int group)
        {
            PropertyName = propName;
            Operand = Comparator.Between;
            Value = val1;
            Value2 = val2;
            Group = group;
        }

        public PropertyFilter(string propName, Comparator comp, object val1, object val2)
        {
            PropertyName = propName;
            Operand = comp;
            Value = val1;
            Value2 = val2;
            Group = 0;
        }

        //Changed this to support filtering on nested objects
        //public string PropertyName { get; private set; }
        public string PropertyName { get; set; }
        public Comparator Operand { get; set; }
        public int Group { get; set; }
        public object Value { get; set; }
        public object Value2 { get; set; }
    }

    public class OrderFilter
    {
        public enum Comparator { Ascending, Descending }

        public static OrderFilter RecordId
        {
            get { return new OrderFilter("RecordId"); }
        }

        public OrderFilter() { }

        public OrderFilter(string propName) : this(propName, Comparator.Ascending) { }

        public OrderFilter(string propName, Comparator comp)
        {
            PropertyName = propName;
            Operand = comp;
        }

        public OrderFilter(string propName, string sortOrder)
        {
            PropertyName = propName;
            Operand = (sortOrder == "Ascending") ? Comparator.Ascending : Comparator.Descending;
        }

        public string PropertyName { get; private set; }
        public Comparator Operand { get; private set; }
    }

    public abstract class StringEnum
    {
        // Source:  http://lostechies.com/jimmybogard/2008/08/12/enumeration-classes/

        protected StringEnum() { }
        protected StringEnum(string val, string text)
        {
            Value = val;
            DisplayText = text;
        }

        public String Value
        {
            get;
            protected set;
        }

        public String DisplayText
        {
            get;
            protected set;
        }

        public int Order
        {
            get;
            protected set;
        }

        public override bool Equals(object obj)
        {
            var otherValue = obj as StringEnum;
            if (otherValue == null) return false;
            var typeMatches = GetType().Equals(obj.GetType());
            var valueMatches = Value.Equals(otherValue.Value);
            return typeMatches && valueMatches;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }

        public static IEnumerable<T> GetAll<T>() where T : StringEnum
        {
            var type = typeof(T);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            var instance = (T)Activator.CreateInstance(typeof(T), true);

            foreach (var info in fields)
            {
                //var instance = (T)Activator.CreateInstance(typeof(T), true);
                var locatedValue = info.GetValue(instance) as T;

                if (locatedValue != null)
                {
                    yield return locatedValue;
                }
            }
        }

        public static T Parse<T>(string value) where T : StringEnum
        {
            T match = GetAll<T>().Where(x => x.Value == value).SingleOrDefault();
            return match;
        }
    }
}
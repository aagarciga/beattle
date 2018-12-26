using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Beattle.SPAUI.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MinimumCountAttribute : ValidationAttribute
    {
        private const string defaultError = "'{0}' must have at least {1} item.";
        private readonly int value;
        private readonly bool required;
        private readonly bool allowEmptyStringValues;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="required"></param>
        /// <param name="allowEmptyStringValues"></param>
        public MinimumCountAttribute(int value, bool required = true, bool allowEmptyStringValues = false)
            : base(defaultError)
        {
            this.value = value;
            this.required = required;
            this.allowEmptyStringValues = allowEmptyStringValues;
        }

        /// <summary>
        /// 
        /// </summary>
        public MinimumCountAttribute() : this(1) { }

        public override bool IsValid(object value)
        {
            if (value == null)
                return !required;
            ICollection<string> collection = value as ICollection<string>;
            if (!allowEmptyStringValues && collection != null)
                return collection.Count(s => !string.IsNullOrWhiteSpace(s)) >= this.value;
            if (collection != null)
                return collection.Count >= this.value;
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(ErrorMessageString, name, this.value);
        }

    }
}

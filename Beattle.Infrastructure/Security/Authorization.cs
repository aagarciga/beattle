using System;
using System.Collections.Generic;
using System.Text;

namespace Beattle.Infrastructure.Security
{
    /// <summary>
    /// Application autorization class
    /// </summary>
    public class Authorization
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Group { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Authorization"/>
        /// </summary>
        public Authorization()
        {

        }

        /// <summary>
        /// Initializes a new instance of <see cref="Authorization"/>
        /// </summary>
        /// <param name="name">The autorization name</param>
        /// <param name="value">The autorization value</param>
        /// <param name="group">The autorization group</param>
        /// <param name="description">The autorization description</param>
        public Authorization(string name, string value, string group, string description)
        {
            Name = name;
            Value = value;
            Group = group;
            Description = description;
        }

        public override string ToString()
        {
            return Value;
        }

        public static implicit operator string(Authorization authorization)
        {
            return authorization.Value;
        }
    }
}

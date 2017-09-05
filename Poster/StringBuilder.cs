using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Poster
{
    public sealed class StringBuilder
    {
        private string _sequence;

        public StringBuilder()
        {
        }

        public StringBuilder Append(string value)
        {
            if (value != null)
            {
                _sequence = string.Concat(_sequence, value);
            }

            return this;
        }

        public StringBuilder AppendLine(string value)
        {
            this.Append(Environment.NewLine);
            return this.Append(value);
        }

        public StringBuilder AppendLine()
        {
            return this.Append(Environment.NewLine);
        }

        public override string ToString()
        {
            return _sequence;
        }

        internal static string GetSeparatedLine(ItemCollection collection, string separator)
        {
            if (collection.Count == 0)
            {
                return String.Empty;
            }

            if (collection.Count == 1)
            {
                return (string)collection[0];
            }

            if (string.IsNullOrEmpty(separator))
            {
                throw new Exception("Undefined string separator.");
            }

            string result = String.Empty;

            foreach (var item in collection)
            {
                result += item;

                if (collection.IndexOf(item) != collection.Count - 1)
                {
                    result += separator;
                }
            }

            return result;
        }

        internal static void GetSeparatedLine(List<string> collection, string separator)
        {
            throw new NotImplementedException();
        }
    }
}

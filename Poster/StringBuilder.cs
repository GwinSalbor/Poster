using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}

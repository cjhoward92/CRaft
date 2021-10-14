using CRaft.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRaft.Models
{
    public class StringEntry : IEntry
    {
        private ulong term;
        private ulong index;
        private string data;

        public ulong Term => term;

        public ulong Index => index;

        public string Data => data;

        public StringEntry(ulong term, ulong index, string data)
        {
            ValidationUtils.IsAboveZero(nameof(index), index);
            ValidationUtils.IsRequired(nameof(data), data);

            this.term = term;
            this.index = index;
            this.data = data;
        }
    }
}

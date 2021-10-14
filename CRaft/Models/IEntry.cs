using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRaft.Models
{
    public interface IEntry
    {
        ulong Term { get; }
        ulong Index { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TygerbergNeonatalAddin
{
    public class SequenceEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>
    {
        bool IEqualityComparer<IEnumerable<T>>.Equals(IEnumerable<T> x, IEnumerable<T> y)
        {
            return x.SequenceEqual(y);
        }

        int IEqualityComparer<IEnumerable<T>>.GetHashCode(IEnumerable<T> obj)
        {
            return obj.Aggregate(17, (acc, elem) => acc * 23 + elem.GetHashCode());
        }
    }
}

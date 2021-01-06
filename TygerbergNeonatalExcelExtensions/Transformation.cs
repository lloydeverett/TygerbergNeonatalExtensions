using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TygerbergNeonatalAddin
{
    public interface ITransformation
    {
        Table Apply(Table table);
    }
}

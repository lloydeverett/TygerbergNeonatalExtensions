using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TygerbergNeonatalAddin
{
    [Serializable()]
    public class Model
    {
        public List<Filter> Filters { get; set; } = new List<Filter>();
        public FindClustersTransformation FindClustersTransformation { get; set; } = new FindClustersTransformation();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TygerbergNeonatalAddin
{
    class TransformationUtil
    {
        public static void ThrowExceptionIfHeaderDoesNotUniquelyReferenceAColumnInTable(ITransformation transformation, string header, Table table)
        {
            try
            {
                int index = table.IndexForColumnWithHeader(header);
            }
            catch (ArgumentDoesNotUniquelyIdentifyValueException)
            {
                throw new MultipleColumnsWithThisHeaderException(transformation, header);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ColumnHeaderNotFoundException(transformation, header);
            }
        }
    }
}

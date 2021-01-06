using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TygerbergNeonatalAddin
{
    public class FindClustersTransformation : ITransformation
    {
        public string DateColumnHeader { get; set; } = "";
        public TimeSpan MaximumAdjacentSpan { get; set; } = TimeSpan.FromDays(7);
        public List<string> ClusterDefiningColumnHeaders { get; set; } = new List<string>();
        public int MinimumNumberOfInstancesPerCluster { get; set; } = 3;

        public Table Apply(Table table)
        {
            TransformationUtil.ThrowExceptionIfHeaderDoesNotUniquelyReferenceAColumnInTable(this, DateColumnHeader, table);
            foreach (string header in ClusterDefiningColumnHeaders)
            {
                TransformationUtil.ThrowExceptionIfHeaderDoesNotUniquelyReferenceAColumnInTable(this, header, table);
            }
            if (table.Columns[table.IndexForColumnWithHeader(DateColumnHeader)].typeHint != Table.TypeHint.Date)
            {
                throw new ExpectedDateColumnException(this, DateColumnHeader);
            }
            
            List<Table.Column> columns = new List<Table.Column>();
            foreach (string header in ClusterDefiningColumnHeaders)
            {
                columns.Add(table.Columns[table.IndexForColumnWithHeader(header)]);
            }
            columns.Add(new Table.Column("First instance", Table.TypeHint.Date));
            columns.Add(new Table.Column("Last instance", Table.TypeHint.Date));
            columns.Add(new Table.Column("Number of instances", Table.TypeHint.General));
            Table result = new Table(columns);

            List<List<IRow>> clusters = Clusters.FindClustersInTable(table, MaximumAdjacentSpan, DateColumnHeader, MinimumNumberOfInstancesPerCluster, ClusterDefiningColumnHeaders);

            foreach (List<IRow> cluster in clusters)
            {
                List<string> resultingRow = new List<string>(columns.Count);
                foreach (string header in ClusterDefiningColumnHeaders)
                {
                    resultingRow.Add(cluster[0][header]);
                }
                resultingRow.Add(cluster[0][DateColumnHeader]);
                resultingRow.Add(cluster.Last()[DateColumnHeader]);
                resultingRow.Add(cluster.Count.ToString());
                result.AddRow(resultingRow);
            }

            return result;
        }
    }
}

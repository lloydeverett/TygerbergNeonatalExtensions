using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TygerbergNeonatalAddin
{
    static class Clusters
    {
        public static List<List<IRow>> FindClustersInTable(Table table, TimeSpan maximumAdjacentSpan, string dateColumnHeader, int minimumNumberOfInstancesPerCluster,
            List<string> headersForColumnsWhoseValuesShouldBeUniformAcrossEachCluster = null)
        {
            List<string> uniformHeaders = headersForColumnsWhoseValuesShouldBeUniformAcrossEachCluster;
            if (uniformHeaders == null)
            {
                uniformHeaders= new List<string>();
            }

            var lookup = table.Rows.ToLookup(
                keySelector: (row) => (from header in uniformHeaders select row[header].ToLowerInvariant()),
                comparer: new SequenceEqualityComparer<string>()
                );
            
            List<List<IRow>> ret = new List<List<IRow>>();
            foreach (var rowSet in lookup)
            {
                ret.AddRange(FindClusters(rowSet, maximumAdjacentSpan, (row) => DateTime.Parse(row[dateColumnHeader])).Where((instances) => instances.Count >= minimumNumberOfInstancesPerCluster));
            }
            return ret;
        }

        public static List<List<T>> FindClusters<T>(IEnumerable<T> values, TimeSpan maximumAdjacentSpan, Func<T, DateTime> getDateTimeForValueFunc)
        {
            List<T> list = new List<T>(values.OrderBy(getDateTimeForValueFunc));
            if (list.Count == 0) return new List<List<T>>();
            
            List<List<T>> clusters = new List<List<T>>();
            List<T> currentCluster = new List<T>();

            currentCluster.Add(list[0]);

            for (int i = 1; i < list.Count; i++)
            {
                DateTime lastDate = getDateTimeForValueFunc(list[i - 1]);
                DateTime thisDate = getDateTimeForValueFunc(list[i]);

                if (thisDate - lastDate <= maximumAdjacentSpan)
                {
                    currentCluster.Add(list[i]);
                }
                else
                {
                    clusters.Add(currentCluster);
                    currentCluster = new List<T>();
                    currentCluster.Add(list[i]);
                }
            }

            clusters.Add(currentCluster);
            currentCluster = null;

            return clusters;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Collections;

namespace TygerbergNeonatalAddin
{
    public class ArgumentDoesNotUniquelyIdentifyValueException : ArgumentException
    {
        public ArgumentDoesNotUniquelyIdentifyValueException() : base() { }
        public ArgumentDoesNotUniquelyIdentifyValueException(string message) : base(message) { }
        public ArgumentDoesNotUniquelyIdentifyValueException(string message, Exception innerException) : base(message, innerException) { }
        public ArgumentDoesNotUniquelyIdentifyValueException(string message, string paramName) : base(message, paramName) { }
        public ArgumentDoesNotUniquelyIdentifyValueException(string message, string paramName, Exception innerException) : base(message, paramName, innerException) { }
    }

    public class Table : IReadOnlyList<IRow>
    {
        class Row : IRow
        {
            List<string> values;
            Table table;
            int index;

            public int Index => index;

            public Table Table => table;

            public Row(Table table, int index, IEnumerable<string> values)
            {
                this.values = new List<string>(values);
                this.table = table;
                this.index = index;
            }

            public string this[int key]
            {
                get
                {
                    return values[key];
                }
                set
                {
                    values[key] = value;
                }
            }

            public string this[string key]
            {
                get
                {
                    int index = table.IndexForColumnWithHeader(key);
                    return values[index];
                }
                set
                {
                    int index = table.IndexForColumnWithHeader(key);
                    values[index] = value;
                }
            }

            IEnumerator<string> IEnumerable<string>.GetEnumerator() => values.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => values.GetEnumerator();
            public int Count => values.Count;
        }

        public enum TypeHint
        {
            General,
            Date,
        }
        public struct Column
        {
            public readonly string header;
            public readonly TypeHint typeHint;

            public Column(string header, TypeHint typeHint)
            {
                this.header = header;
                this.typeHint = typeHint;
            }
        }
        ImmutableList<Column> columns;
        List<Row> rows = new List<Row>();

        public List<string> ColumnHeaders => (from column in columns select column.header).ToList();

        public int ColumnsCount => columns.Count;
        public int RowsCount => rows.Count;

        public IImmutableList<Column> Columns => columns;
        public IReadOnlyList<IRow> Rows => rows;

        public Table(IEnumerable<Column> columns)
        {
            this.columns = columns.ToImmutableList();
        }

        public Table(IEnumerable<Column> columns, IEnumerable<IReadOnlyList<string>> rows)
        {
            this.columns = columns.ToImmutableList();
            foreach (var elem in rows) { AddRow(elem); }
        }

        public void AddRow(IReadOnlyList<string> values)
        {
            if (values.Count != columns.Count)
            {
                throw new InvalidOperationException("Mismatch between number of column headers and number of values in row being added");
            }

            rows.Add(new Row(this, rows.Count, values));
        }
        
        public int IndexForColumnWithHeader(string header)
        {
            int index = -1;
            for (int i = 0; i < columns.Count; i++)
            {
                if (string.Equals(header, ColumnHeaders[i], StringComparison.OrdinalIgnoreCase))
                {
                    if (index != -1)
                    {
                        throw new ArgumentDoesNotUniquelyIdentifyValueException("More than one column in the table has this header.", "header");
                    }

                    index = i;
                }
            }

            if (index == -1)
            {
                throw new ArgumentOutOfRangeException("header", "No columns in the table have this header.");
            }

            return index;
        }

        IEnumerator<IRow> IEnumerable<IRow>.GetEnumerator() => rows.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => rows.GetEnumerator();
        public int Count => rows.Count;
        public IRow this[int key]
        {
            get
            {
                return rows[key];
            }
        }

    }

    public interface IRow : IReadOnlyList<string>
    {
        Table Table { get; }

        string this[string key] { get; set; }

        int Index { get; }
    }
}

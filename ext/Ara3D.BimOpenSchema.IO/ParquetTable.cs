using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ara3D.Collections;
using Ara3D.DataTable;
using DataColumn = Parquet.Data.DataColumn;

namespace Ara3D.BimOpenSchema.IO;

public class ParquetTable<T> : IReadOnlyList<T>, IDataTable
{
    public string Name { get; }
    public IReadOnlyList<IDataRow> Rows { get;  }
    public IReadOnlyList<IDataColumn> Columns { get; }
    public object this[int column, int row] => GetRow(row).Values[column];
    private IReadOnlyList<DataColumn> _columns { get; }
    private Func<object[], T> _ctor;

    public ParquetTable(string name, IReadOnlyList<DataColumn> columns, Func<object[], T> ctor)
    {
        _columns = columns;
        Name = name;
        Count = _columns.Count > 0 ? _columns[0].NumValues : 0;
        _ctor = ctor;

        var fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (fields.Length != _columns.Count)
            throw new InvalidOperationException($"Field count ({fields.Length}) != column count ({_columns.Count}).");
        
        Rows = new ReadOnlyList<IDataRow>(Count, GetRow);
        Columns = _columns.Select((c, i) => new ParquetColumnAdapter(c, i)).ToList();
    }

    public IDataRow GetRow(int n)
        => new DataRow(this, n);

    public T this[int n]
    {
        get
        {
            var vals = new object[_columns.Count];
            for (int i = 0; i < _columns.Count; i++)
                vals[i] = _columns[i].Data.GetValue(n);
            return _ctor(vals);
        }
    }

    public int Count { get; }

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public override string ToString()
        => $"Table {Name}, {Columns.Count} Columns, {Count} Rows";
}
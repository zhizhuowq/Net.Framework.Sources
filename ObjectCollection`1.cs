// Decompiled with JetBrains decompiler
// Type: System.Net.Http.Headers.ObjectCollection`1
// Assembly: System.Net.Http, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: D8D12B97-8555-48F6-96B3-BBC6BF89FCBA
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Net.Http.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Net.Http.Headers
{
  internal sealed class ObjectCollection<T> : Collection<T> where T : class
  {
    private static readonly Action<T> s_defaultValidator = new Action<T>(ObjectCollection<T>.CheckNotNull);
    private readonly Action<T> _validator;

    public ObjectCollection()
      : this(ObjectCollection<T>.s_defaultValidator)
    {
    }

    public ObjectCollection(Action<T> validator)
      : base((IList<T>) new List<T>())
    {
      this._validator = validator;
    }

    public List<T>.Enumerator GetEnumerator()
    {
      return ((List<T>) this.Items).GetEnumerator();
    }

    protected override void InsertItem(int index, T item)
    {
      this._validator(item);
      base.InsertItem(index, item);
    }

    protected override void SetItem(int index, T item)
    {
      this._validator(item);
      base.SetItem(index, item);
    }

    private static void CheckNotNull(T item)
    {
      if ((object) item == null)
        throw new ArgumentNullException(nameof (item));
    }
  }
}

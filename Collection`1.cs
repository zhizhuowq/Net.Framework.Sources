// Decompiled with JetBrains decompiler
// Type: System.Collections.ObjectModel.Collection`1
// Assembly: System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// MVID: BBB3B884-123D-47EA-9CD1-5BED540D02AE
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Private.CoreLib.dll

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections.ObjectModel
{
  [DebuggerDisplay("Count = {Count}")]
  [DebuggerTypeProxy(typeof (ICollectionDebugView<>))]
  [TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
  [NullableContext(1)]
  [Nullable(0)]
  [Serializable]
  public class Collection<[Nullable(2)] T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>
  {
    private IList<T> items;

    public Collection()
    {
      this.items = (IList<T>) new List<T>();
    }

    public Collection(IList<T> list)
    {
      if (list == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.list);
      this.items = list;
    }

    public int Count
    {
      get
      {
        return this.items.Count;
      }
    }

    protected IList<T> Items
    {
      get
      {
        return this.items;
      }
    }

    public T this[int index]
    {
      get
      {
        return this.items[index];
      }
      set
      {
        if (this.items.IsReadOnly)
          ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        if ((uint) index >= (uint) this.items.Count)
          ThrowHelper.ThrowArgumentOutOfRange_IndexException();
        this.SetItem(index, value);
      }
    }

    public void Add(T item)
    {
      if (this.items.IsReadOnly)
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
      this.InsertItem(this.items.Count, item);
    }

    public void Clear()
    {
      if (this.items.IsReadOnly)
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
      this.ClearItems();
    }

    public void CopyTo(T[] array, int index)
    {
      this.items.CopyTo(array, index);
    }

    public bool Contains(T item)
    {
      return this.items.Contains(item);
    }

    public IEnumerator<T> GetEnumerator()
    {
      return this.items.GetEnumerator();
    }

    public int IndexOf(T item)
    {
      return this.items.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
      if (this.items.IsReadOnly)
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
      if ((uint) index > (uint) this.items.Count)
        ThrowHelper.ThrowArgumentOutOfRange_IndexException();
      this.InsertItem(index, item);
    }

    public bool Remove(T item)
    {
      if (this.items.IsReadOnly)
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
      int index = this.items.IndexOf(item);
      if (index < 0)
        return false;
      this.RemoveItem(index);
      return true;
    }

    public void RemoveAt(int index)
    {
      if (this.items.IsReadOnly)
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
      if ((uint) index >= (uint) this.items.Count)
        ThrowHelper.ThrowArgumentOutOfRange_IndexException();
      this.RemoveItem(index);
    }

    protected virtual void ClearItems()
    {
      this.items.Clear();
    }

    protected virtual void InsertItem(int index, T item)
    {
      this.items.Insert(index, item);
    }

    protected virtual void RemoveItem(int index)
    {
      this.items.RemoveAt(index);
    }

    protected virtual void SetItem(int index, T item)
    {
      this.items[index] = item;
    }

    bool ICollection<T>.IsReadOnly
    {
      get
      {
        return this.items.IsReadOnly;
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.items.GetEnumerator();
    }

    bool ICollection.IsSynchronized
    {
      get
      {
        return false;
      }
    }

    object ICollection.SyncRoot
    {
      get
      {
        return !(this.items is ICollection items) ? (object) this : items.SyncRoot;
      }
    }

    void ICollection.CopyTo(Array array, int index)
    {
      if (array == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
      if (array.Rank != 1)
        ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
      if (array.GetLowerBound(0) != 0)
        ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
      if (index < 0)
        ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
      if (array.Length - index < this.Count)
        ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
      if (array is T[] array1)
      {
        this.items.CopyTo(array1, index);
      }
      else
      {
        Type elementType = array.GetType().GetElementType();
        Type c = typeof (T);
        if (!elementType.IsAssignableFrom(c) && !c.IsAssignableFrom(elementType))
          ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
        if (!(array is object[] objArray))
          ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
        int count = this.items.Count;
        try
        {
          for (int index1 = 0; index1 < count; ++index1)
          {
            int index2 = index++;
            // ISSUE: variable of a boxed type
            __Boxed<T> local = (object) this.items[index1];
            objArray[index2] = (object) local;
          }
        }
        catch (ArrayTypeMismatchException ex)
        {
          ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
        }
      }
    }

    [Nullable(2)]
    object IList.this[int index]
    {
      get
      {
        return (object) this.items[index];
      }
      set
      {
        ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
        try
        {
          this[index] = (T) value;
        }
        catch (InvalidCastException ex)
        {
          ThrowHelper.ThrowWrongValueTypeArgumentException<object>(value, typeof (T));
        }
      }
    }

    bool IList.IsReadOnly
    {
      get
      {
        return this.items.IsReadOnly;
      }
    }

    bool IList.IsFixedSize
    {
      get
      {
        return this.items is IList items ? items.IsFixedSize : this.items.IsReadOnly;
      }
    }

    int IList.Add(object value)
    {
      if (this.items.IsReadOnly)
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
      ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
      try
      {
        this.Add((T) value);
      }
      catch (InvalidCastException ex)
      {
        ThrowHelper.ThrowWrongValueTypeArgumentException<object>(value, typeof (T));
      }
      return this.Count - 1;
    }

    bool IList.Contains(object value)
    {
      return Collection<T>.IsCompatibleObject(value) && this.Contains((T) value);
    }

    int IList.IndexOf(object value)
    {
      return Collection<T>.IsCompatibleObject(value) ? this.IndexOf((T) value) : -1;
    }

    void IList.Insert(int index, object value)
    {
      if (this.items.IsReadOnly)
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
      ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
      try
      {
        this.Insert(index, (T) value);
      }
      catch (InvalidCastException ex)
      {
        ThrowHelper.ThrowWrongValueTypeArgumentException<object>(value, typeof (T));
      }
    }

    void IList.Remove(object value)
    {
      if (this.items.IsReadOnly)
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
      if (!Collection<T>.IsCompatibleObject(value))
        return;
      this.Remove((T) value);
    }

    private static bool IsCompatibleObject(object value)
    {
      if (value is T)
        return true;
      return value == null && (object) default (T) == null;
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.List`1
// Assembly: System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// MVID: BBB3B884-123D-47EA-9CD1-5BED540D02AE
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Private.CoreLib.dll

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
  [NullableContext(1)]
  [TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
  [DebuggerDisplay("Count = {Count}")]
  [Nullable(0)]
  [DebuggerTypeProxy(typeof (ICollectionDebugView<>))]
  [Serializable]
  public class List<[Nullable(2)] T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>
  {
    private static readonly T[] s_emptyArray = new T[0];
    private T[] _items;
    private int _size;
    private int _version;

    public List()
    {
      this._items = List<T>.s_emptyArray;
    }

    public List(int capacity)
    {
      if (capacity < 0)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
      if (capacity == 0)
        this._items = List<T>.s_emptyArray;
      else
        this._items = new T[capacity];
    }

    public List(IEnumerable<T> collection)
    {
      if (collection == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
      if (collection is ICollection<T> objs)
      {
        int count = objs.Count;
        if (count == 0)
        {
          this._items = List<T>.s_emptyArray;
        }
        else
        {
          this._items = new T[count];
          objs.CopyTo(this._items, 0);
          this._size = count;
        }
      }
      else
      {
        this._size = 0;
        this._items = List<T>.s_emptyArray;
        foreach (T obj in collection)
          this.Add(obj);
      }
    }

    public int Capacity
    {
      get
      {
        return this._items.Length;
      }
      set
      {
        if (value < this._size)
          ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value, ExceptionResource.ArgumentOutOfRange_SmallCapacity);
        if (value == this._items.Length)
          return;
        if (value > 0)
        {
          T[] objArray = new T[value];
          if (this._size > 0)
            Array.Copy((Array) this._items, 0, (Array) objArray, 0, this._size);
          this._items = objArray;
        }
        else
          this._items = List<T>.s_emptyArray;
      }
    }

    public int Count
    {
      get
      {
        return this._size;
      }
    }

    bool IList.IsFixedSize
    {
      get
      {
        return false;
      }
    }

    bool ICollection<T>.IsReadOnly
    {
      get
      {
        return false;
      }
    }

    bool IList.IsReadOnly
    {
      get
      {
        return false;
      }
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
        return (object) this;
      }
    }

    public T this[int index]
    {
      get
      {
        if ((uint) index >= (uint) this._size)
          ThrowHelper.ThrowArgumentOutOfRange_IndexException();
        return this._items[index];
      }
      set
      {
        if ((uint) index >= (uint) this._size)
          ThrowHelper.ThrowArgumentOutOfRange_IndexException();
        this._items[index] = value;
        ++this._version;
      }
    }

    private static bool IsCompatibleObject(object value)
    {
      if (value is T)
        return true;
      return value == null && (object) default (T) == null;
    }

    [Nullable(2)]
    object IList.this[int index]
    {
      get
      {
        return (object) this[index];
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
      ++this._version;
      T[] items = this._items;
      int size = this._size;
      if ((uint) size < (uint) items.Length)
      {
        this._size = size + 1;
        items[size] = item;
      }
      else
        this.AddWithResize(item);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddWithResize(T item)
    {
      int size = this._size;
      this.EnsureCapacity(size + 1);
      this._size = size + 1;
      this._items[size] = item;
    }

    int IList.Add(object item)
    {
      ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(item, ExceptionArgument.item);
      try
      {
        this.Add((T) item);
      }
      catch (InvalidCastException ex)
      {
        ThrowHelper.ThrowWrongValueTypeArgumentException<object>(item, typeof (T));
      }
      return this.Count - 1;
    }

    public void AddRange(IEnumerable<T> collection)
    {
      this.InsertRange(this._size, collection);
    }

    public ReadOnlyCollection<T> AsReadOnly()
    {
      return new ReadOnlyCollection<T>((IList<T>) this);
    }

    public int BinarySearch(int index, int count, T item, [Nullable(new byte[] {2, 1})] IComparer<T> comparer)
    {
      if (index < 0)
        ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
      if (count < 0)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
      if (this._size - index < count)
        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
      return Array.BinarySearch<T>(this._items, index, count, item, comparer);
    }

    public int BinarySearch(T item)
    {
      return this.BinarySearch(0, this.Count, item, (IComparer<T>) null);
    }

    public int BinarySearch(T item, [Nullable(new byte[] {2, 1})] IComparer<T> comparer)
    {
      return this.BinarySearch(0, this.Count, item, comparer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
      ++this._version;
      if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
      {
        int size = this._size;
        this._size = 0;
        if (size <= 0)
          return;
        Array.Clear((Array) this._items, 0, size);
      }
      else
        this._size = 0;
    }

    public bool Contains(T item)
    {
      return this._size != 0 && this.IndexOf(item) != -1;
    }

    bool IList.Contains(object item)
    {
      return List<T>.IsCompatibleObject(item) && this.Contains((T) item);
    }

    public List<TOutput> ConvertAll<[Nullable(2)] TOutput>(
      Converter<T, TOutput> converter)
    {
      if (converter == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.converter);
      List<TOutput> outputList = new List<TOutput>(this._size);
      for (int index = 0; index < this._size; ++index)
        outputList._items[index] = converter(this._items[index]);
      outputList._size = this._size;
      return outputList;
    }

    public void CopyTo(T[] array)
    {
      this.CopyTo(array, 0);
    }

    void ICollection.CopyTo(Array array, int arrayIndex)
    {
      if (array != null && array.Rank != 1)
        ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
      try
      {
        Array.Copy((Array) this._items, 0, array, arrayIndex, this._size);
      }
      catch (ArrayTypeMismatchException ex)
      {
        ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
      }
    }

    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
      if (this._size - index < count)
        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
      Array.Copy((Array) this._items, index, (Array) array, arrayIndex, count);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      Array.Copy((Array) this._items, 0, (Array) array, arrayIndex, this._size);
    }

    private void EnsureCapacity(int min)
    {
      if (this._items.Length >= min)
        return;
      int num = this._items.Length == 0 ? 4 : this._items.Length * 2;
      if ((uint) num > 2146435071U)
        num = 2146435071;
      if (num < min)
        num = min;
      this.Capacity = num;
    }

    public bool Exists(Predicate<T> match)
    {
      return this.FindIndex(match) != -1;
    }

    [return: MaybeNull]
    public T Find(Predicate<T> match)
    {
      if (match == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
      for (int index = 0; index < this._size; ++index)
      {
        if (match(this._items[index]))
          return this._items[index];
      }
      return default (T);
    }

    public List<T> FindAll(Predicate<T> match)
    {
      if (match == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
      List<T> objList = new List<T>();
      for (int index = 0; index < this._size; ++index)
      {
        if (match(this._items[index]))
          objList.Add(this._items[index]);
      }
      return objList;
    }

    public int FindIndex(Predicate<T> match)
    {
      return this.FindIndex(0, this._size, match);
    }

    public int FindIndex(int startIndex, Predicate<T> match)
    {
      return this.FindIndex(startIndex, this._size - startIndex, match);
    }

    public int FindIndex(int startIndex, int count, Predicate<T> match)
    {
      if ((uint) startIndex > (uint) this._size)
        ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
      if (count < 0 || startIndex > this._size - count)
        ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
      if (match == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
      int num = startIndex + count;
      for (int index = startIndex; index < num; ++index)
      {
        if (match(this._items[index]))
          return index;
      }
      return -1;
    }

    [return: MaybeNull]
    public T FindLast(Predicate<T> match)
    {
      if (match == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
      for (int index = this._size - 1; index >= 0; --index)
      {
        if (match(this._items[index]))
          return this._items[index];
      }
      return default (T);
    }

    public int FindLastIndex(Predicate<T> match)
    {
      return this.FindLastIndex(this._size - 1, this._size, match);
    }

    public int FindLastIndex(int startIndex, Predicate<T> match)
    {
      return this.FindLastIndex(startIndex, startIndex + 1, match);
    }

    public int FindLastIndex(int startIndex, int count, Predicate<T> match)
    {
      if (match == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
      if (this._size == 0)
      {
        if (startIndex != -1)
          ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
      }
      else if ((uint) startIndex >= (uint) this._size)
        ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
      if (count < 0 || startIndex - count + 1 < 0)
        ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
      int num = startIndex - count;
      for (int index = startIndex; index > num; --index)
      {
        if (match(this._items[index]))
          return index;
      }
      return -1;
    }

    public void ForEach(Action<T> action)
    {
      if (action == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);
      int version = this._version;
      for (int index = 0; index < this._size && version == this._version; ++index)
        action(this._items[index]);
      if (version == this._version)
        return;
      ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
    }

    [NullableContext(0)]
    public List<T>.Enumerator GetEnumerator()
    {
      return new List<T>.Enumerator(this);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return (IEnumerator<T>) new List<T>.Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) new List<T>.Enumerator(this);
    }

    public List<T> GetRange(int index, int count)
    {
      if (index < 0)
        ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
      if (count < 0)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
      if (this._size - index < count)
        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
      List<T> objList = new List<T>(count);
      Array.Copy((Array) this._items, index, (Array) objList._items, 0, count);
      objList._size = count;
      return objList;
    }

    public int IndexOf(T item)
    {
      return Array.IndexOf<T>(this._items, item, 0, this._size);
    }

    int IList.IndexOf(object item)
    {
      return List<T>.IsCompatibleObject(item) ? this.IndexOf((T) item) : -1;
    }

    public int IndexOf(T item, int index)
    {
      if (index > this._size)
        ThrowHelper.ThrowArgumentOutOfRange_IndexException();
      return Array.IndexOf<T>(this._items, item, index, this._size - index);
    }

    public int IndexOf(T item, int index, int count)
    {
      if (index > this._size)
        ThrowHelper.ThrowArgumentOutOfRange_IndexException();
      if (count < 0 || index > this._size - count)
        ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
      return Array.IndexOf<T>(this._items, item, index, count);
    }

    public void Insert(int index, T item)
    {
      if ((uint) index > (uint) this._size)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_ListInsert);
      if (this._size == this._items.Length)
        this.EnsureCapacity(this._size + 1);
      if (index < this._size)
        Array.Copy((Array) this._items, index, (Array) this._items, index + 1, this._size - index);
      this._items[index] = item;
      ++this._size;
      ++this._version;
    }

    void IList.Insert(int index, object item)
    {
      ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(item, ExceptionArgument.item);
      try
      {
        this.Insert(index, (T) item);
      }
      catch (InvalidCastException ex)
      {
        ThrowHelper.ThrowWrongValueTypeArgumentException<object>(item, typeof (T));
      }
    }

    public void InsertRange(int index, IEnumerable<T> collection)
    {
      if (collection == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
      if ((uint) index > (uint) this._size)
        ThrowHelper.ThrowArgumentOutOfRange_IndexException();
      if (collection is ICollection<T> objs)
      {
        int count = objs.Count;
        if (count > 0)
        {
          this.EnsureCapacity(this._size + count);
          if (index < this._size)
            Array.Copy((Array) this._items, index, (Array) this._items, index + count, this._size - index);
          if (this == objs)
          {
            Array.Copy((Array) this._items, 0, (Array) this._items, index, index);
            Array.Copy((Array) this._items, index + count, (Array) this._items, index * 2, this._size - index);
          }
          else
            objs.CopyTo(this._items, index);
          this._size += count;
        }
      }
      else
      {
        foreach (T obj in collection)
          this.Insert(index++, obj);
      }
      ++this._version;
    }

    public int LastIndexOf(T item)
    {
      return this._size == 0 ? -1 : this.LastIndexOf(item, this._size - 1, this._size);
    }

    public int LastIndexOf(T item, int index)
    {
      if (index >= this._size)
        ThrowHelper.ThrowArgumentOutOfRange_IndexException();
      return this.LastIndexOf(item, index, index + 1);
    }

    public int LastIndexOf(T item, int index, int count)
    {
      if (this.Count != 0 && index < 0)
        ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
      if (this.Count != 0 && count < 0)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
      if (this._size == 0)
        return -1;
      if (index >= this._size)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
      if (count > index + 1)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
      return Array.LastIndexOf<T>(this._items, item, index, count);
    }

    public bool Remove(T item)
    {
      int index = this.IndexOf(item);
      if (index < 0)
        return false;
      this.RemoveAt(index);
      return true;
    }

    void IList.Remove(object item)
    {
      if (!List<T>.IsCompatibleObject(item))
        return;
      this.Remove((T) item);
    }

    public int RemoveAll(Predicate<T> match)
    {
      if (match == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
      int index1 = 0;
      while (index1 < this._size && !match(this._items[index1]))
        ++index1;
      if (index1 >= this._size)
        return 0;
      int index2 = index1 + 1;
      while (index2 < this._size)
      {
        while (index2 < this._size && match(this._items[index2]))
          ++index2;
        if (index2 < this._size)
          this._items[index1++] = this._items[index2++];
      }
      if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        Array.Clear((Array) this._items, index1, this._size - index1);
      int num = this._size - index1;
      this._size = index1;
      ++this._version;
      return num;
    }

    public void RemoveAt(int index)
    {
      if ((uint) index >= (uint) this._size)
        ThrowHelper.ThrowArgumentOutOfRange_IndexException();
      --this._size;
      if (index < this._size)
        Array.Copy((Array) this._items, index + 1, (Array) this._items, index, this._size - index);
      if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        this._items[this._size] = default (T);
      ++this._version;
    }

    public void RemoveRange(int index, int count)
    {
      if (index < 0)
        ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
      if (count < 0)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
      if (this._size - index < count)
        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
      if (count <= 0)
        return;
      this._size -= count;
      if (index < this._size)
        Array.Copy((Array) this._items, index + count, (Array) this._items, index, this._size - index);
      ++this._version;
      if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        return;
      Array.Clear((Array) this._items, this._size, count);
    }

    public void Reverse()
    {
      this.Reverse(0, this.Count);
    }

    public void Reverse(int index, int count)
    {
      if (index < 0)
        ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
      if (count < 0)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
      if (this._size - index < count)
        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
      if (count > 1)
        Array.Reverse<T>(this._items, index, count);
      ++this._version;
    }

    public void Sort()
    {
      this.Sort(0, this.Count, (IComparer<T>) null);
    }

    public void Sort([Nullable(new byte[] {2, 1})] IComparer<T> comparer)
    {
      this.Sort(0, this.Count, comparer);
    }

    public void Sort(int index, int count, [Nullable(new byte[] {2, 1})] IComparer<T> comparer)
    {
      if (index < 0)
        ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
      if (count < 0)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
      if (this._size - index < count)
        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
      if (count > 1)
        Array.Sort<T>(this._items, index, count, comparer);
      ++this._version;
    }

    public void Sort(Comparison<T> comparison)
    {
      if (comparison == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparison);
      if (this._size > 1)
        ArraySortHelper<T>.Sort(this._items, 0, this._size, comparison);
      ++this._version;
    }

    public T[] ToArray()
    {
      if (this._size == 0)
        return List<T>.s_emptyArray;
      T[] objArray = new T[this._size];
      Array.Copy((Array) this._items, 0, (Array) objArray, 0, this._size);
      return objArray;
    }

    public void TrimExcess()
    {
      if (this._size >= (int) ((double) this._items.Length * 0.9))
        return;
      this.Capacity = this._size;
    }

    public bool TrueForAll(Predicate<T> match)
    {
      if (match == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
      for (int index = 0; index < this._size; ++index)
      {
        if (!match(this._items[index]))
          return false;
      }
      return true;
    }

    [NullableContext(0)]
    public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
    {
      private readonly List<T> _list;
      private int _index;
      private readonly int _version;
      [MaybeNull]
      [AllowNull]
      private T _current;

      internal Enumerator(List<T> list)
      {
        this._list = list;
        this._index = 0;
        this._version = list._version;
        this._current = default (T);
      }

      public void Dispose()
      {
      }

      public bool MoveNext()
      {
        List<T> list = this._list;
        if (this._version != list._version || (uint) this._index >= (uint) list._size)
          return this.MoveNextRare();
        this._current = list._items[this._index];
        ++this._index;
        return true;
      }

      private bool MoveNextRare()
      {
        if (this._version != this._list._version)
          ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
        this._index = this._list._size + 1;
        this._current = default (T);
        return false;
      }

      [Nullable(1)]
      public T Current
      {
        [NullableContext(1)] get
        {
          return this._current;
        }
      }

      [Nullable(2)]
      object IEnumerator.Current
      {
        get
        {
          if (this._index == 0 || this._index == this._list._size + 1)
            ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
          return (object) this.Current;
        }
      }

      void IEnumerator.Reset()
      {
        if (this._version != this._list._version)
          ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
        this._index = 0;
        this._current = default (T);
      }
    }
  }
}

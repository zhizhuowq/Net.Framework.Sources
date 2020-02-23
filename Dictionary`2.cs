// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.Dictionary`2
// Assembly: System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// MVID: BBB3B884-123D-47EA-9CD1-5BED540D02AE
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Private.CoreLib.dll

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System.Collections.Generic
{
  [TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
  [Nullable(0)]
  [NullableContext(1)]
  [DebuggerTypeProxy(typeof (IDictionaryDebugView<,>))]
  [DebuggerDisplay("Count = {Count}")]
  [Serializable]
  public class Dictionary<TKey, [Nullable(2)] TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary, ICollection, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, ISerializable, IDeserializationCallback
  {
    private int[] _buckets;
    private Dictionary<TKey, TValue>.Entry[] _entries;
    private int _count;
    private int _freeList;
    private int _freeCount;
    private int _version;
    private IEqualityComparer<TKey> _comparer;
    private Dictionary<TKey, TValue>.KeyCollection _keys;
    private Dictionary<TKey, TValue>.ValueCollection _values;

    public Dictionary()
      : this(0, (IEqualityComparer<TKey>) null)
    {
    }

    public Dictionary(int capacity)
      : this(capacity, (IEqualityComparer<TKey>) null)
    {
    }

    public Dictionary([Nullable(new byte[] {2, 1})] IEqualityComparer<TKey> comparer)
      : this(0, comparer)
    {
    }

    public Dictionary(int capacity, [Nullable(new byte[] {2, 1})] IEqualityComparer<TKey> comparer)
    {
      if (capacity < 0)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity);
      if (capacity > 0)
        this.Initialize(capacity);
      if (comparer != EqualityComparer<TKey>.Default)
        this._comparer = comparer;
      if (!(typeof (TKey) == typeof (string)) || this._comparer != null)
        return;
      this._comparer = (IEqualityComparer<TKey>) NonRandomizedStringEqualityComparer.Default;
    }

    public Dictionary(IDictionary<TKey, TValue> dictionary)
      : this(dictionary, (IEqualityComparer<TKey>) null)
    {
    }

    public Dictionary(IDictionary<TKey, TValue> dictionary, [Nullable(new byte[] {2, 1})] IEqualityComparer<TKey> comparer)
      : this(dictionary != null ? dictionary.Count : 0, comparer)
    {
      if (dictionary == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
      if (dictionary.GetType() == typeof (Dictionary<TKey, TValue>))
      {
        Dictionary<TKey, TValue> dictionary1 = (Dictionary<TKey, TValue>) dictionary;
        int count = dictionary1._count;
        Dictionary<TKey, TValue>.Entry[] entries = dictionary1._entries;
        for (int index = 0; index < count; ++index)
        {
          if (entries[index].next >= -1)
            this.Add(entries[index].key, entries[index].value);
        }
      }
      else
      {
        foreach (KeyValuePair<TKey, TValue> keyValuePair in (IEnumerable<KeyValuePair<TKey, TValue>>) dictionary)
          this.Add(keyValuePair.Key, keyValuePair.Value);
      }
    }

    public Dictionary([Nullable(new byte[] {1, 0, 1, 1})] IEnumerable<KeyValuePair<TKey, TValue>> collection)
      : this(collection, (IEqualityComparer<TKey>) null)
    {
    }

    public Dictionary(
      [Nullable(new byte[] {1, 0, 1, 1})] IEnumerable<KeyValuePair<TKey, TValue>> collection,
      [Nullable(new byte[] {2, 1})] IEqualityComparer<TKey> comparer)
      : this(collection is ICollection<KeyValuePair<TKey, TValue>> keyValuePairs ? keyValuePairs.Count : 0, comparer)
    {
      if (collection == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
      foreach (KeyValuePair<TKey, TValue> keyValuePair in collection)
        this.Add(keyValuePair.Key, keyValuePair.Value);
    }

    protected Dictionary(SerializationInfo info, StreamingContext context)
    {
      HashHelpers.SerializationInfoTable.Add((object) this, info);
    }

    public IEqualityComparer<TKey> Comparer
    {
      get
      {
        return this._comparer != null && !(this._comparer is NonRandomizedStringEqualityComparer) ? this._comparer : (IEqualityComparer<TKey>) EqualityComparer<TKey>.Default;
      }
    }

    public int Count
    {
      get
      {
        return this._count - this._freeCount;
      }
    }

    [Nullable(new byte[] {1, 0, 0})]
    public Dictionary<TKey, TValue>.KeyCollection Keys
    {
      [return: Nullable(new byte[] {1, 0, 0})] get
      {
        if (this._keys == null)
          this._keys = new Dictionary<TKey, TValue>.KeyCollection(this);
        return this._keys;
      }
    }

    ICollection<TKey> IDictionary<TKey, TValue>.Keys
    {
      get
      {
        if (this._keys == null)
          this._keys = new Dictionary<TKey, TValue>.KeyCollection(this);
        return (ICollection<TKey>) this._keys;
      }
    }

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
    {
      get
      {
        if (this._keys == null)
          this._keys = new Dictionary<TKey, TValue>.KeyCollection(this);
        return (IEnumerable<TKey>) this._keys;
      }
    }

    [Nullable(new byte[] {1, 0, 0})]
    public Dictionary<TKey, TValue>.ValueCollection Values
    {
      [return: Nullable(new byte[] {1, 0, 0})] get
      {
        if (this._values == null)
          this._values = new Dictionary<TKey, TValue>.ValueCollection(this);
        return this._values;
      }
    }

    ICollection<TValue> IDictionary<TKey, TValue>.Values
    {
      get
      {
        if (this._values == null)
          this._values = new Dictionary<TKey, TValue>.ValueCollection(this);
        return (ICollection<TValue>) this._values;
      }
    }

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
    {
      get
      {
        if (this._values == null)
          this._values = new Dictionary<TKey, TValue>.ValueCollection(this);
        return (IEnumerable<TValue>) this._values;
      }
    }

    public TValue this[TKey key]
    {
      get
      {
        int entry = this.FindEntry(key);
        if (entry >= 0)
          return this._entries[entry].value;
        ThrowHelper.ThrowKeyNotFoundException<TKey>(key);
        return default (TValue);
      }
      set
      {
        this.TryInsert(key, value, InsertionBehavior.OverwriteExisting);
      }
    }

    public void Add(TKey key, TValue value)
    {
      this.TryInsert(key, value, InsertionBehavior.ThrowOnExisting);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(
      KeyValuePair<TKey, TValue> keyValuePair)
    {
      this.Add(keyValuePair.Key, keyValuePair.Value);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(
      KeyValuePair<TKey, TValue> keyValuePair)
    {
      int entry = this.FindEntry(keyValuePair.Key);
      return entry >= 0 && EqualityComparer<TValue>.Default.Equals(this._entries[entry].value, keyValuePair.Value);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(
      KeyValuePair<TKey, TValue> keyValuePair)
    {
      int entry = this.FindEntry(keyValuePair.Key);
      if (entry < 0 || !EqualityComparer<TValue>.Default.Equals(this._entries[entry].value, keyValuePair.Value))
        return false;
      this.Remove(keyValuePair.Key);
      return true;
    }

    public void Clear()
    {
      int count = this._count;
      if (count <= 0)
        return;
      Array.Clear((Array) this._buckets, 0, this._buckets.Length);
      this._count = 0;
      this._freeList = -1;
      this._freeCount = 0;
      Array.Clear((Array) this._entries, 0, count);
    }

    public bool ContainsKey(TKey key)
    {
      return this.FindEntry(key) >= 0;
    }

    public bool ContainsValue(TValue value)
    {
      Dictionary<TKey, TValue>.Entry[] entries = this._entries;
      if ((object) value == null)
      {
        for (int index = 0; index < this._count; ++index)
        {
          if (entries[index].next >= -1 && (object) entries[index].value == null)
            return true;
        }
      }
      else if ((object) default (TValue) != null)
      {
        for (int index = 0; index < this._count; ++index)
        {
          if (entries[index].next >= -1 && EqualityComparer<TValue>.Default.Equals(entries[index].value, value))
            return true;
        }
      }
      else
      {
        EqualityComparer<TValue> equalityComparer = EqualityComparer<TValue>.Default;
        for (int index = 0; index < this._count; ++index)
        {
          if (entries[index].next >= -1 && equalityComparer.Equals(entries[index].value, value))
            return true;
        }
      }
      return false;
    }

    private void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
    {
      if (array == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
      if ((uint) index > (uint) array.Length)
        ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
      if (array.Length - index < this.Count)
        ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
      int count = this._count;
      Dictionary<TKey, TValue>.Entry[] entries = this._entries;
      for (int index1 = 0; index1 < count; ++index1)
      {
        if (entries[index1].next >= -1)
          array[index++] = new KeyValuePair<TKey, TValue>(entries[index1].key, entries[index1].value);
      }
    }

    [NullableContext(0)]
    public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
      return new Dictionary<TKey, TValue>.Enumerator(this, 2);
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
      return (IEnumerator<KeyValuePair<TKey, TValue>>) new Dictionary<TKey, TValue>.Enumerator(this, 2);
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.info);
      info.AddValue("Version", this._version);
      info.AddValue("Comparer", (object) (this._comparer ?? (IEqualityComparer<TKey>) EqualityComparer<TKey>.Default), typeof (IEqualityComparer<TKey>));
      info.AddValue("HashSize", this._buckets == null ? 0 : this._buckets.Length);
      if (this._buckets == null)
        return;
      KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[this.Count];
      this.CopyTo(array, 0);
      info.AddValue("KeyValuePairs", (object) array, typeof (KeyValuePair<TKey, TValue>[]));
    }

    private int FindEntry(TKey key)
    {
      if ((object) key == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
      int index = -1;
      int[] buckets = this._buckets;
      Dictionary<TKey, TValue>.Entry[] entries = this._entries;
      int num = 0;
      if (buckets != null)
      {
        IEqualityComparer<TKey> comparer = this._comparer;
        if (comparer == null)
        {
          uint hashCode = (uint) key.GetHashCode();
          index = buckets[(int) (hashCode % (uint) buckets.Length)] - 1;
          if ((object) default (TKey) != null)
          {
            while ((uint) index < (uint) entries.Length && ((int) entries[index].hashCode != (int) hashCode || !EqualityComparer<TKey>.Default.Equals(entries[index].key, key)))
            {
              index = entries[index].next;
              if (num >= entries.Length)
                ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
              ++num;
            }
          }
          else
          {
            EqualityComparer<TKey> equalityComparer = EqualityComparer<TKey>.Default;
            while ((uint) index < (uint) entries.Length && ((int) entries[index].hashCode != (int) hashCode || !equalityComparer.Equals(entries[index].key, key)))
            {
              index = entries[index].next;
              if (num >= entries.Length)
                ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
              ++num;
            }
          }
        }
        else
        {
          uint hashCode = (uint) comparer.GetHashCode(key);
          index = buckets[(int) (hashCode % (uint) buckets.Length)] - 1;
          while ((uint) index < (uint) entries.Length && ((int) entries[index].hashCode != (int) hashCode || !comparer.Equals(entries[index].key, key)))
          {
            index = entries[index].next;
            if (num >= entries.Length)
              ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
            ++num;
          }
        }
      }
      return index;
    }

    private int Initialize(int capacity)
    {
      int prime = HashHelpers.GetPrime(capacity);
      this._freeList = -1;
      this._buckets = new int[prime];
      this._entries = new Dictionary<TKey, TValue>.Entry[prime];
      return prime;
    }

    private bool TryInsert(TKey key, TValue value, InsertionBehavior behavior)
    {
      if ((object) key == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
      if (this._buckets == null)
        this.Initialize(0);
      Dictionary<TKey, TValue>.Entry[] entries = this._entries;
      IEqualityComparer<TKey> comparer = this._comparer;
      uint num1 = comparer == null ? (uint) key.GetHashCode() : (uint) comparer.GetHashCode(key);
      int num2 = 0;
      ref int local1 = ref this._buckets[(int) (num1 % (uint) this._buckets.Length)];
      int index1 = local1 - 1;
      if (comparer == null)
      {
        if ((object) default (TKey) != null)
        {
          while ((uint) index1 < (uint) entries.Length)
          {
            if ((int) entries[index1].hashCode == (int) num1 && EqualityComparer<TKey>.Default.Equals(entries[index1].key, key))
            {
              switch (behavior)
              {
                case InsertionBehavior.OverwriteExisting:
                  entries[index1].value = value;
                  ++this._version;
                  return true;
                case InsertionBehavior.ThrowOnExisting:
                  ThrowHelper.ThrowAddingDuplicateWithKeyArgumentException<TKey>(key);
                  break;
              }
              return false;
            }
            index1 = entries[index1].next;
            if (num2 >= entries.Length)
              ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
            ++num2;
          }
        }
        else
        {
          EqualityComparer<TKey> equalityComparer = EqualityComparer<TKey>.Default;
          while ((uint) index1 < (uint) entries.Length)
          {
            if ((int) entries[index1].hashCode == (int) num1 && equalityComparer.Equals(entries[index1].key, key))
            {
              switch (behavior)
              {
                case InsertionBehavior.OverwriteExisting:
                  entries[index1].value = value;
                  ++this._version;
                  return true;
                case InsertionBehavior.ThrowOnExisting:
                  ThrowHelper.ThrowAddingDuplicateWithKeyArgumentException<TKey>(key);
                  break;
              }
              return false;
            }
            index1 = entries[index1].next;
            if (num2 >= entries.Length)
              ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
            ++num2;
          }
        }
      }
      else
      {
        while ((uint) index1 < (uint) entries.Length)
        {
          if ((int) entries[index1].hashCode == (int) num1 && comparer.Equals(entries[index1].key, key))
          {
            switch (behavior)
            {
              case InsertionBehavior.OverwriteExisting:
                entries[index1].value = value;
                ++this._version;
                return true;
              case InsertionBehavior.ThrowOnExisting:
                ThrowHelper.ThrowAddingDuplicateWithKeyArgumentException<TKey>(key);
                break;
            }
            return false;
          }
          index1 = entries[index1].next;
          if (num2 >= entries.Length)
            ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
          ++num2;
        }
      }
      bool flag = false;
      int index2;
      if (this._freeCount > 0)
      {
        index2 = this._freeList;
        flag = true;
        --this._freeCount;
      }
      else
      {
        int count = this._count;
        if (count == entries.Length)
        {
          this.Resize();
          local1 = ref this._buckets[(int) (num1 % (uint) this._buckets.Length)];
        }
        index2 = count;
        this._count = count + 1;
        entries = this._entries;
      }
      ref Dictionary<TKey, TValue>.Entry local2 = ref entries[index2];
      if (flag)
        this._freeList = -3 - entries[this._freeList].next;
      local2.hashCode = num1;
      local2.next = local1 - 1;
      local2.key = key;
      local2.value = value;
      local1 = index2 + 1;
      ++this._version;
      if ((object) default (TKey) == null && num2 > 100 && comparer is NonRandomizedStringEqualityComparer)
      {
        this._comparer = (IEqualityComparer<TKey>) null;
        this.Resize(entries.Length, true);
      }
      return true;
    }

    [NullableContext(2)]
    public virtual void OnDeserialization(object sender)
    {
      SerializationInfo serializationInfo;
      HashHelpers.SerializationInfoTable.TryGetValue((object) this, out serializationInfo);
      if (serializationInfo == null)
        return;
      int int32_1 = serializationInfo.GetInt32("Version");
      int int32_2 = serializationInfo.GetInt32("HashSize");
      this._comparer = (IEqualityComparer<TKey>) serializationInfo.GetValue("Comparer", typeof (IEqualityComparer<TKey>));
      if (int32_2 != 0)
      {
        this.Initialize(int32_2);
        KeyValuePair<TKey, TValue>[] keyValuePairArray = (KeyValuePair<TKey, TValue>[]) serializationInfo.GetValue("KeyValuePairs", typeof (KeyValuePair<TKey, TValue>[]));
        if (keyValuePairArray == null)
          ThrowHelper.ThrowSerializationException(ExceptionResource.Serialization_MissingKeys);
        for (int index = 0; index < keyValuePairArray.Length; ++index)
        {
          if ((object) keyValuePairArray[index].Key == null)
            ThrowHelper.ThrowSerializationException(ExceptionResource.Serialization_NullKey);
          this.Add(keyValuePairArray[index].Key, keyValuePairArray[index].Value);
        }
      }
      else
        this._buckets = (int[]) null;
      this._version = int32_1;
      HashHelpers.SerializationInfoTable.Remove((object) this);
    }

    private void Resize()
    {
      this.Resize(HashHelpers.ExpandPrime(this._count), false);
    }

    private void Resize(int newSize, bool forceNewHashCodes)
    {
      int[] numArray = new int[newSize];
      Dictionary<TKey, TValue>.Entry[] entryArray = new Dictionary<TKey, TValue>.Entry[newSize];
      int count = this._count;
      Array.Copy((Array) this._entries, 0, (Array) entryArray, 0, count);
      if ((object) default (TKey) == null & forceNewHashCodes)
      {
        for (int index = 0; index < count; ++index)
        {
          if (entryArray[index].next >= -1)
            entryArray[index].hashCode = (uint) entryArray[index].key.GetHashCode();
        }
      }
      for (int index = 0; index < count; ++index)
      {
        if (entryArray[index].next >= -1)
        {
          uint num = entryArray[index].hashCode % (uint) newSize;
          entryArray[index].next = numArray[(int) num] - 1;
          numArray[(int) num] = index + 1;
        }
      }
      this._buckets = numArray;
      this._entries = entryArray;
    }

    public bool Remove(TKey key)
    {
      if ((object) key == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
      int[] buckets = this._buckets;
      Dictionary<TKey, TValue>.Entry[] entries = this._entries;
      int num1 = 0;
      if (buckets != null)
      {
        IEqualityComparer<TKey> comparer1 = this._comparer;
        uint num2 = comparer1 != null ? (uint) comparer1.GetHashCode(key) : (uint) key.GetHashCode();
        uint num3 = num2 % (uint) buckets.Length;
        int index1 = -1;
        int index2 = buckets[(int) num3] - 1;
        while (index2 >= 0)
        {
          ref Dictionary<TKey, TValue>.Entry local = ref entries[index2];
          if ((int) local.hashCode == (int) num2)
          {
            IEqualityComparer<TKey> comparer2 = this._comparer;
            if ((comparer2 != null ? (comparer2.Equals(local.key, key) ? 1 : 0) : (EqualityComparer<TKey>.Default.Equals(local.key, key) ? 1 : 0)) != 0)
            {
              if (index1 < 0)
                buckets[(int) num3] = local.next + 1;
              else
                entries[index1].next = local.next;
              local.next = -3 - this._freeList;
              if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
                local.key = default (TKey);
              if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                local.value = default (TValue);
              this._freeList = index2;
              ++this._freeCount;
              return true;
            }
          }
          index1 = index2;
          index2 = local.next;
          if (num1 >= entries.Length)
            ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
          ++num1;
        }
      }
      return false;
    }

    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
      if ((object) key == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
      int[] buckets = this._buckets;
      Dictionary<TKey, TValue>.Entry[] entries = this._entries;
      int num1 = 0;
      if (buckets != null)
      {
        IEqualityComparer<TKey> comparer1 = this._comparer;
        uint num2 = comparer1 != null ? (uint) comparer1.GetHashCode(key) : (uint) key.GetHashCode();
        uint num3 = num2 % (uint) buckets.Length;
        int index1 = -1;
        int index2 = buckets[(int) num3] - 1;
        while (index2 >= 0)
        {
          ref Dictionary<TKey, TValue>.Entry local = ref entries[index2];
          if ((int) local.hashCode == (int) num2)
          {
            IEqualityComparer<TKey> comparer2 = this._comparer;
            if ((comparer2 != null ? (comparer2.Equals(local.key, key) ? 1 : 0) : (EqualityComparer<TKey>.Default.Equals(local.key, key) ? 1 : 0)) != 0)
            {
              if (index1 < 0)
                buckets[(int) num3] = local.next + 1;
              else
                entries[index1].next = local.next;
              value = local.value;
              local.next = -3 - this._freeList;
              if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
                local.key = default (TKey);
              if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                local.value = default (TValue);
              this._freeList = index2;
              ++this._freeCount;
              return true;
            }
          }
          index1 = index2;
          index2 = local.next;
          if (num1 >= entries.Length)
            ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
          ++num1;
        }
      }
      value = default (TValue);
      return false;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
      int entry = this.FindEntry(key);
      if (entry >= 0)
      {
        value = this._entries[entry].value;
        return true;
      }
      value = default (TValue);
      return false;
    }

    public bool TryAdd(TKey key, TValue value)
    {
      return this.TryInsert(key, value, InsertionBehavior.None);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
    {
      get
      {
        return false;
      }
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(
      KeyValuePair<TKey, TValue>[] array,
      int index)
    {
      this.CopyTo(array, index);
    }

    void ICollection.CopyTo(Array array, int index)
    {
      if (array == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
      if (array.Rank != 1)
        ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
      if (array.GetLowerBound(0) != 0)
        ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
      if ((uint) index > (uint) array.Length)
        ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
      if (array.Length - index < this.Count)
        ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
      switch (array)
      {
        case KeyValuePair<TKey, TValue>[] array1:
          this.CopyTo(array1, index);
          break;
        case DictionaryEntry[] dictionaryEntryArray:
          Dictionary<TKey, TValue>.Entry[] entries1 = this._entries;
          for (int index1 = 0; index1 < this._count; ++index1)
          {
            if (entries1[index1].next >= -1)
            {
              int index2 = index++;
              DictionaryEntry dictionaryEntry = new DictionaryEntry((object) entries1[index1].key, (object) entries1[index1].value);
              dictionaryEntryArray[index2] = dictionaryEntry;
            }
          }
          break;
        case object[] objArray:
label_18:
          try
          {
            int count = this._count;
            Dictionary<TKey, TValue>.Entry[] entries2 = this._entries;
            for (int index1 = 0; index1 < count; ++index1)
            {
              if (entries2[index1].next >= -1)
              {
                int index2 = index++;
                // ISSUE: variable of a boxed type
                __Boxed<KeyValuePair<TKey, TValue>> local = (ValueType) new KeyValuePair<TKey, TValue>(entries2[index1].key, entries2[index1].value);
                objArray[index2] = (object) local;
              }
            }
            break;
          }
          catch (ArrayTypeMismatchException ex)
          {
            ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
            break;
          }
        default:
          ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
          goto label_18;
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) new Dictionary<TKey, TValue>.Enumerator(this, 2);
    }

    public int EnsureCapacity(int capacity)
    {
      if (capacity < 0)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity);
      int num = this._entries == null ? 0 : this._entries.Length;
      if (num >= capacity)
        return num;
      ++this._version;
      if (this._buckets == null)
        return this.Initialize(capacity);
      int prime = HashHelpers.GetPrime(capacity);
      this.Resize(prime, false);
      return prime;
    }

    public void TrimExcess()
    {
      this.TrimExcess(this.Count);
    }

    public void TrimExcess(int capacity)
    {
      if (capacity < this.Count)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity);
      int prime = HashHelpers.GetPrime(capacity);
      Dictionary<TKey, TValue>.Entry[] entries1 = this._entries;
      int num1 = entries1 == null ? 0 : entries1.Length;
      if (prime >= num1)
        return;
      int count = this._count;
      ++this._version;
      this.Initialize(prime);
      Dictionary<TKey, TValue>.Entry[] entries2 = this._entries;
      int[] buckets = this._buckets;
      int index1 = 0;
      for (int index2 = 0; index2 < count; ++index2)
      {
        uint hashCode = entries1[index2].hashCode;
        if (entries1[index2].next >= -1)
        {
          ref Dictionary<TKey, TValue>.Entry local = ref entries2[index1];
          local = entries1[index2];
          uint num2 = hashCode % (uint) prime;
          local.next = buckets[(int) num2] - 1;
          buckets[(int) num2] = index1 + 1;
          ++index1;
        }
      }
      this._count = index1;
      this._freeCount = 0;
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

    bool IDictionary.IsFixedSize
    {
      get
      {
        return false;
      }
    }

    bool IDictionary.IsReadOnly
    {
      get
      {
        return false;
      }
    }

    ICollection IDictionary.Keys
    {
      get
      {
        return (ICollection) this.Keys;
      }
    }

    ICollection IDictionary.Values
    {
      get
      {
        return (ICollection) this.Values;
      }
    }

    [Nullable(2)]
    object IDictionary.this[object key]
    {
      get
      {
        if (Dictionary<TKey, TValue>.IsCompatibleKey(key))
        {
          int entry = this.FindEntry((TKey) key);
          if (entry >= 0)
            return (object) this._entries[entry].value;
        }
        return (object) null;
      }
      set
      {
        if (key == null)
          ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
        ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, ExceptionArgument.value);
        try
        {
          TKey index = (TKey) key;
          try
          {
            this[index] = (TValue) value;
          }
          catch (InvalidCastException ex)
          {
            ThrowHelper.ThrowWrongValueTypeArgumentException<object>(value, typeof (TValue));
          }
        }
        catch (InvalidCastException ex)
        {
          ThrowHelper.ThrowWrongKeyTypeArgumentException<object>(key, typeof (TKey));
        }
      }
    }

    private static bool IsCompatibleKey(object key)
    {
      if (key == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
      return key is TKey;
    }

    void IDictionary.Add(object key, object value)
    {
      if (key == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
      ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, ExceptionArgument.value);
      try
      {
        TKey key1 = (TKey) key;
        try
        {
          this.Add(key1, (TValue) value);
        }
        catch (InvalidCastException ex)
        {
          ThrowHelper.ThrowWrongValueTypeArgumentException<object>(value, typeof (TValue));
        }
      }
      catch (InvalidCastException ex)
      {
        ThrowHelper.ThrowWrongKeyTypeArgumentException<object>(key, typeof (TKey));
      }
    }

    bool IDictionary.Contains(object key)
    {
      return Dictionary<TKey, TValue>.IsCompatibleKey(key) && this.ContainsKey((TKey) key);
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
      return (IDictionaryEnumerator) new Dictionary<TKey, TValue>.Enumerator(this, 1);
    }

    void IDictionary.Remove(object key)
    {
      if (!Dictionary<TKey, TValue>.IsCompatibleKey(key))
        return;
      this.Remove((TKey) key);
    }

    private struct Entry
    {
      public int next;
      public uint hashCode;
      public TKey key;
      public TValue value;
    }

    [NullableContext(0)]
    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator, IDictionaryEnumerator
    {
      private readonly Dictionary<TKey, TValue> _dictionary;
      private readonly int _version;
      private int _index;
      private KeyValuePair<TKey, TValue> _current;
      private readonly int _getEnumeratorRetType;

      internal Enumerator(Dictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
      {
        this._dictionary = dictionary;
        this._version = dictionary._version;
        this._index = 0;
        this._getEnumeratorRetType = getEnumeratorRetType;
        this._current = new KeyValuePair<TKey, TValue>();
      }

      public bool MoveNext()
      {
        if (this._version != this._dictionary._version)
          ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
        while ((uint) this._index < (uint) this._dictionary._count)
        {
          ref Dictionary<TKey, TValue>.Entry local = ref this._dictionary._entries[this._index++];
          if (local.next >= -1)
          {
            this._current = new KeyValuePair<TKey, TValue>(local.key, local.value);
            return true;
          }
        }
        this._index = this._dictionary._count + 1;
        this._current = new KeyValuePair<TKey, TValue>();
        return false;
      }

      [Nullable(new byte[] {0, 1, 1})]
      public KeyValuePair<TKey, TValue> Current
      {
        [return: Nullable(new byte[] {0, 1, 1})] get
        {
          return this._current;
        }
      }

      public void Dispose()
      {
      }

      [Nullable(2)]
      object IEnumerator.Current
      {
        get
        {
          if (this._index == 0 || this._index == this._dictionary._count + 1)
            ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
          return this._getEnumeratorRetType == 1 ? (object) new DictionaryEntry((object) this._current.Key, (object) this._current.Value) : (object) new KeyValuePair<TKey, TValue>(this._current.Key, this._current.Value);
        }
      }

      void IEnumerator.Reset()
      {
        if (this._version != this._dictionary._version)
          ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
        this._index = 0;
        this._current = new KeyValuePair<TKey, TValue>();
      }

      DictionaryEntry IDictionaryEnumerator.Entry
      {
        get
        {
          if (this._index == 0 || this._index == this._dictionary._count + 1)
            ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
          return new DictionaryEntry((object) this._current.Key, (object) this._current.Value);
        }
      }

      [Nullable(1)]
      object IDictionaryEnumerator.Key
      {
        get
        {
          if (this._index == 0 || this._index == this._dictionary._count + 1)
            ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
          return (object) this._current.Key;
        }
      }

      [Nullable(2)]
      object IDictionaryEnumerator.Value
      {
        get
        {
          if (this._index == 0 || this._index == this._dictionary._count + 1)
            ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
          return (object) this._current.Value;
        }
      }
    }

    [DebuggerTypeProxy(typeof (DictionaryKeyCollectionDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    [NullableContext(0)]
    public sealed class KeyCollection : ICollection<TKey>, IEnumerable<TKey>, IEnumerable, ICollection, IReadOnlyCollection<TKey>
    {
      private Dictionary<TKey, TValue> _dictionary;

      [NullableContext(1)]
      public KeyCollection(Dictionary<TKey, TValue> dictionary)
      {
        if (dictionary == null)
          ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
        this._dictionary = dictionary;
      }

      public Dictionary<TKey, TValue>.KeyCollection.Enumerator GetEnumerator()
      {
        return new Dictionary<TKey, TValue>.KeyCollection.Enumerator(this._dictionary);
      }

      [NullableContext(1)]
      public void CopyTo(TKey[] array, int index)
      {
        if (array == null)
          ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        if (index < 0 || index > array.Length)
          ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
        if (array.Length - index < this._dictionary.Count)
          ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
        int count = this._dictionary._count;
        Dictionary<TKey, TValue>.Entry[] entries = this._dictionary._entries;
        for (int index1 = 0; index1 < count; ++index1)
        {
          if (entries[index1].next >= -1)
            array[index++] = entries[index1].key;
        }
      }

      public int Count
      {
        get
        {
          return this._dictionary.Count;
        }
      }

      bool ICollection<TKey>.IsReadOnly
      {
        get
        {
          return true;
        }
      }

      void ICollection<TKey>.Add(TKey item)
      {
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
      }

      void ICollection<TKey>.Clear()
      {
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
      }

      bool ICollection<TKey>.Contains(TKey item)
      {
        return this._dictionary.ContainsKey(item);
      }

      bool ICollection<TKey>.Remove(TKey item)
      {
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
        return false;
      }

      IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
      {
        return (IEnumerator<TKey>) new Dictionary<TKey, TValue>.KeyCollection.Enumerator(this._dictionary);
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return (IEnumerator) new Dictionary<TKey, TValue>.KeyCollection.Enumerator(this._dictionary);
      }

      void ICollection.CopyTo(Array array, int index)
      {
        if (array == null)
          ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        if (array.Rank != 1)
          ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
        if (array.GetLowerBound(0) != 0)
          ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
        if ((uint) index > (uint) array.Length)
          ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
        if (array.Length - index < this._dictionary.Count)
          ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
        switch (array)
        {
          case TKey[] array1:
            this.CopyTo(array1, index);
            break;
          case object[] objArray:
label_13:
            int count = this._dictionary._count;
            Dictionary<TKey, TValue>.Entry[] entries = this._dictionary._entries;
            try
            {
              for (int index1 = 0; index1 < count; ++index1)
              {
                if (entries[index1].next >= -1)
                {
                  int index2 = index++;
                  // ISSUE: variable of a boxed type
                  __Boxed<TKey> key = (object) entries[index1].key;
                  objArray[index2] = (object) key;
                }
              }
              break;
            }
            catch (ArrayTypeMismatchException ex)
            {
              ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
              break;
            }
          default:
            ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
            goto label_13;
        }
      }

      bool ICollection.IsSynchronized
      {
        get
        {
          return false;
        }
      }

      [Nullable(1)]
      object ICollection.SyncRoot
      {
        get
        {
          return ((ICollection) this._dictionary).SyncRoot;
        }
      }

      public struct Enumerator : IEnumerator<TKey>, IDisposable, IEnumerator
      {
        private readonly Dictionary<TKey, TValue> _dictionary;
        private int _index;
        private readonly int _version;
        [AllowNull]
        [MaybeNull]
        private TKey _currentKey;

        internal Enumerator(Dictionary<TKey, TValue> dictionary)
        {
          this._dictionary = dictionary;
          this._version = dictionary._version;
          this._index = 0;
          this._currentKey = default (TKey);
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
          if (this._version != this._dictionary._version)
            ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
          while ((uint) this._index < (uint) this._dictionary._count)
          {
            ref Dictionary<TKey, TValue>.Entry local = ref this._dictionary._entries[this._index++];
            if (local.next >= -1)
            {
              this._currentKey = local.key;
              return true;
            }
          }
          this._index = this._dictionary._count + 1;
          this._currentKey = default (TKey);
          return false;
        }

        [Nullable(1)]
        public TKey Current
        {
          [NullableContext(1)] get
          {
            return this._currentKey;
          }
        }

        [Nullable(2)]
        object IEnumerator.Current
        {
          get
          {
            if (this._index == 0 || this._index == this._dictionary._count + 1)
              ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
            return (object) this._currentKey;
          }
        }

        void IEnumerator.Reset()
        {
          if (this._version != this._dictionary._version)
            ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
          this._index = 0;
          this._currentKey = default (TKey);
        }
      }
    }

    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof (DictionaryValueCollectionDebugView<,>))]
    [NullableContext(0)]
    public sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>, IEnumerable, ICollection, IReadOnlyCollection<TValue>
    {
      private Dictionary<TKey, TValue> _dictionary;

      [NullableContext(1)]
      public ValueCollection(Dictionary<TKey, TValue> dictionary)
      {
        if (dictionary == null)
          ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
        this._dictionary = dictionary;
      }

      public Dictionary<TKey, TValue>.ValueCollection.Enumerator GetEnumerator()
      {
        return new Dictionary<TKey, TValue>.ValueCollection.Enumerator(this._dictionary);
      }

      [NullableContext(1)]
      public void CopyTo(TValue[] array, int index)
      {
        if (array == null)
          ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        if ((long) (uint) index > (long) array.Length)
          ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
        if (array.Length - index < this._dictionary.Count)
          ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
        int count = this._dictionary._count;
        Dictionary<TKey, TValue>.Entry[] entries = this._dictionary._entries;
        for (int index1 = 0; index1 < count; ++index1)
        {
          if (entries[index1].next >= -1)
            array[index++] = entries[index1].value;
        }
      }

      public int Count
      {
        get
        {
          return this._dictionary.Count;
        }
      }

      bool ICollection<TValue>.IsReadOnly
      {
        get
        {
          return true;
        }
      }

      void ICollection<TValue>.Add(TValue item)
      {
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
      }

      bool ICollection<TValue>.Remove(TValue item)
      {
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
        return false;
      }

      void ICollection<TValue>.Clear()
      {
        ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
      }

      bool ICollection<TValue>.Contains(TValue item)
      {
        return this._dictionary.ContainsValue(item);
      }

      IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
      {
        return (IEnumerator<TValue>) new Dictionary<TKey, TValue>.ValueCollection.Enumerator(this._dictionary);
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return (IEnumerator) new Dictionary<TKey, TValue>.ValueCollection.Enumerator(this._dictionary);
      }

      void ICollection.CopyTo(Array array, int index)
      {
        if (array == null)
          ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        if (array.Rank != 1)
          ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
        if (array.GetLowerBound(0) != 0)
          ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
        if ((uint) index > (uint) array.Length)
          ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
        if (array.Length - index < this._dictionary.Count)
          ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
        switch (array)
        {
          case TValue[] array1:
            this.CopyTo(array1, index);
            break;
          case object[] objArray:
label_13:
            int count = this._dictionary._count;
            Dictionary<TKey, TValue>.Entry[] entries = this._dictionary._entries;
            try
            {
              for (int index1 = 0; index1 < count; ++index1)
              {
                if (entries[index1].next >= -1)
                {
                  int index2 = index++;
                  // ISSUE: variable of a boxed type
                  __Boxed<TValue> local = (object) entries[index1].value;
                  objArray[index2] = (object) local;
                }
              }
              break;
            }
            catch (ArrayTypeMismatchException ex)
            {
              ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
              break;
            }
          default:
            ThrowHelper.ThrowArgumentException_Argument_InvalidArrayType();
            goto label_13;
        }
      }

      bool ICollection.IsSynchronized
      {
        get
        {
          return false;
        }
      }

      [Nullable(1)]
      object ICollection.SyncRoot
      {
        get
        {
          return ((ICollection) this._dictionary).SyncRoot;
        }
      }

      public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator
      {
        private readonly Dictionary<TKey, TValue> _dictionary;
        private int _index;
        private readonly int _version;
        [MaybeNull]
        [AllowNull]
        private TValue _currentValue;

        internal Enumerator(Dictionary<TKey, TValue> dictionary)
        {
          this._dictionary = dictionary;
          this._version = dictionary._version;
          this._index = 0;
          this._currentValue = default (TValue);
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
          if (this._version != this._dictionary._version)
            ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
          while ((uint) this._index < (uint) this._dictionary._count)
          {
            ref Dictionary<TKey, TValue>.Entry local = ref this._dictionary._entries[this._index++];
            if (local.next >= -1)
            {
              this._currentValue = local.value;
              return true;
            }
          }
          this._index = this._dictionary._count + 1;
          this._currentValue = default (TValue);
          return false;
        }

        [Nullable(1)]
        public TValue Current
        {
          [NullableContext(1)] get
          {
            return this._currentValue;
          }
        }

        [Nullable(2)]
        object IEnumerator.Current
        {
          get
          {
            if (this._index == 0 || this._index == this._dictionary._count + 1)
              ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
            return (object) this._currentValue;
          }
        }

        void IEnumerator.Reset()
        {
          if (this._version != this._dictionary._version)
            ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion();
          this._index = 0;
          this._currentValue = default (TValue);
        }
      }
    }
  }
}

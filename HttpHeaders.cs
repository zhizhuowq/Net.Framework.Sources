﻿// Decompiled with JetBrains decompiler
// Type: System.Net.Http.Headers.HttpHeaders
// Assembly: System.Net.Http, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: D8D12B97-8555-48F6-96B3-BBC6BF89FCBA
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Net.Http.dll

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System.Net.Http.Headers
{
  /// <summary>RFC 2616 中定义标头及其值的集合。</summary>
  public abstract class HttpHeaders : IEnumerable<KeyValuePair<string, IEnumerable<string>>>, IEnumerable
  {
    private Dictionary<HeaderDescriptor, HttpHeaders.HeaderStoreItemInfo> _headerStore;
    private readonly HttpHeaderType _allowedHeaderTypes;
    private readonly HttpHeaderType _treatAsCustomHeaderTypes;

    /// <summary>初始化 <see cref="T:System.Net.Http.Headers.HttpHeaders" /> 类的新实例。</summary>
    protected HttpHeaders()
      : this(HttpHeaderType.All, HttpHeaderType.None)
    {
    }

    internal HttpHeaders(HttpHeaderType allowedHeaderTypes, HttpHeaderType treatAsCustomHeaderTypes)
    {
      this._allowedHeaderTypes = allowedHeaderTypes;
      this._treatAsCustomHeaderTypes = treatAsCustomHeaderTypes;
    }

    /// <summary>添加指定的标头及其值到 <see cref="T:System.Net.Http.Headers.HttpHeaders" /> 集合中。</summary>
    /// <param name="name">要添加到集合中的标头。</param>
    /// <param name="value">标头的内容。</param>
    public void Add(string name, string value)
    {
      this.Add(this.GetHeaderDescriptor(name), value);
    }

    internal void Add(HeaderDescriptor descriptor, string value)
    {
      HttpHeaders.HeaderStoreItemInfo info;
      bool addToStore;
      this.PrepareHeaderInfoForAdd(descriptor, out info, out addToStore);
      this.ParseAndAddValue(descriptor, info, value);
      if (!addToStore || info.ParsedValue == null)
        return;
      this.AddHeaderToStore(descriptor, info);
    }

    /// <summary>添加指定的标头及其值到 <see cref="T:System.Net.Http.Headers.HttpHeaders" /> 集合中。</summary>
    /// <param name="name">要添加到集合中的标头。</param>
    /// <param name="values">要向集合中添加的标头值的列表。</param>
    public void Add(string name, IEnumerable<string> values)
    {
      this.Add(this.GetHeaderDescriptor(name), values);
    }

    internal void Add(HeaderDescriptor descriptor, IEnumerable<string> values)
    {
      if (values == null)
        throw new ArgumentNullException(nameof (values));
      HttpHeaders.HeaderStoreItemInfo info;
      bool addToStore;
      this.PrepareHeaderInfoForAdd(descriptor, out info, out addToStore);
      try
      {
        foreach (string str in values)
          this.ParseAndAddValue(descriptor, info, str);
      }
      finally
      {
        if (addToStore && info.ParsedValue != null)
          this.AddHeaderToStore(descriptor, info);
      }
    }

    /// <summary>返回一个值，该值指示指定的标头及其值是否添加到了 <see cref="T:System.Net.Http.Headers.HttpHeaders" /> 集合，而无需验证提供的信息。</summary>
    /// <param name="name">要添加到集合中的标头。</param>
    /// <param name="value">标头的内容。</param>
    /// <returns>如果指定的标头 <paramref name="name" /> 和 <paramref name="value" /> 可能被添加到集合，则为 <see langword="true" />；否则为 <see langword="false" />。</returns>
    public bool TryAddWithoutValidation(string name, string value)
    {
      HeaderDescriptor descriptor;
      return this.TryGetHeaderDescriptor(name, out descriptor) && this.TryAddWithoutValidation(descriptor, value);
    }

    internal bool TryAddWithoutValidation(HeaderDescriptor descriptor, string value)
    {
      if (value == null)
        value = string.Empty;
      HttpHeaders.AddValue(this.GetOrCreateHeaderInfo(descriptor, false), (object) value, HttpHeaders.StoreLocation.Raw);
      return true;
    }

    /// <summary>返回一个值，该值指示指定的标头及其值是否添加到 <see cref="T:System.Net.Http.Headers.HttpHeaders" /> 集合，而无需验证提供的信息。</summary>
    /// <param name="name">要添加到集合中的标头。</param>
    /// <param name="values">标头的值。</param>
    /// <returns>如果指定的标头 <paramref name="name" /> 和 <paramref name="values" /> 可能被添加到集合，则为 <see langword="true" />；否则为 <see langword="false" />。</returns>
    public bool TryAddWithoutValidation(string name, IEnumerable<string> values)
    {
      HeaderDescriptor descriptor;
      return this.TryGetHeaderDescriptor(name, out descriptor) && this.TryAddWithoutValidation(descriptor, values);
    }

    internal bool TryAddWithoutValidation(HeaderDescriptor descriptor, IEnumerable<string> values)
    {
      if (values == null)
        throw new ArgumentNullException(nameof (values));
      HttpHeaders.HeaderStoreItemInfo headerInfo = this.GetOrCreateHeaderInfo(descriptor, false);
      foreach (string str in values)
        HttpHeaders.AddValue(headerInfo, (object) (str ?? string.Empty), HttpHeaders.StoreLocation.Raw);
      return true;
    }

    /// <summary>从 <see cref="T:System.Net.Http.Headers.HttpHeaders" /> 集合中移除所有标头。</summary>
    public void Clear()
    {
      if (this._headerStore == null)
        return;
      this._headerStore.Clear();
    }

    /// <summary>从 <see cref="T:System.Net.Http.Headers.HttpHeaders" /> 集合中移除指定的标头。</summary>
    /// <param name="name">要从集合中移除的标头名称。</param>
    /// <returns>返回 <see cref="T:System.Boolean" />。</returns>
    public bool Remove(string name)
    {
      return this.Remove(this.GetHeaderDescriptor(name));
    }

    /// <summary>返回存储于 <see cref="T:System.Net.Http.Headers.HttpHeaders" /> 集合中的指定标头的所有标头值。</summary>
    /// <param name="name">要为其返回值的指定标头。</param>
    /// <returns>标头字符串数组。</returns>
    /// <exception cref="T:System.InvalidOperationException">找不到标头。</exception>
    public IEnumerable<string> GetValues(string name)
    {
      return this.GetValues(this.GetHeaderDescriptor(name));
    }

    internal IEnumerable<string> GetValues(HeaderDescriptor descriptor)
    {
      IEnumerable<string> values;
      if (!this.TryGetValues(descriptor, out values))
        throw new InvalidOperationException(SR.net_http_headers_not_found);
      return values;
    }

    /// <summary>如果指定的标头和指定的值存储在 <see cref="T:System.Net.Http.Headers.HttpHeaders" /> 集合中，则返回。</summary>
    /// <param name="name">指定的标头。</param>
    /// <param name="values">指定的标头值。</param>
    /// <returns>如果指定的标头 <paramref name="name" /> 和 <see langword="values" /> 存储在集合中，则为 <see langword="true" />；否则为 <see langword="false" />。</returns>
    public bool TryGetValues(string name, out IEnumerable<string> values)
    {
      HeaderDescriptor descriptor;
      if (this.TryGetHeaderDescriptor(name, out descriptor))
        return this.TryGetValues(descriptor, out values);
      values = (IEnumerable<string>) null;
      return false;
    }

    internal bool TryGetValues(HeaderDescriptor descriptor, out IEnumerable<string> values)
    {
      if (this._headerStore == null)
      {
        values = (IEnumerable<string>) null;
        return false;
      }
      HttpHeaders.HeaderStoreItemInfo info = (HttpHeaders.HeaderStoreItemInfo) null;
      if (this.TryGetAndParseHeaderInfo(descriptor, out info))
      {
        values = (IEnumerable<string>) HttpHeaders.GetValuesAsStrings(descriptor, info, (object) null);
        return true;
      }
      values = (IEnumerable<string>) null;
      return false;
    }

    /// <summary>在 <see cref="T:System.Net.Http.Headers.HttpHeaders" /> 集合中存在特定标头时返回。</summary>
    /// <param name="name">特定标头。</param>
    /// <returns>如果集合中存在特定标头，则为 <see langword="true" />；否则为 <see langword="false" />。</returns>
    public bool Contains(string name)
    {
      return this.Contains(this.GetHeaderDescriptor(name));
    }

    internal bool Contains(HeaderDescriptor descriptor)
    {
      if (this._headerStore == null)
        return false;
      HttpHeaders.HeaderStoreItemInfo info = (HttpHeaders.HeaderStoreItemInfo) null;
      return this.TryGetAndParseHeaderInfo(descriptor, out info);
    }

    /// <summary>返回表示当前 <see cref="T:System.Net.Http.Headers.HttpHeaders" /> 对象的字符串。</summary>
    /// <returns>表示当前对象的字符串。</returns>
    public override string ToString()
    {
      if (this._headerStore == null || this._headerStore.Count == 0)
        return string.Empty;
      StringBuilder stringBuilder = new StringBuilder();
      foreach (KeyValuePair<string, string> headerString in this.GetHeaderStrings())
      {
        stringBuilder.Append(headerString.Key);
        stringBuilder.Append(": ");
        stringBuilder.Append(headerString.Value);
        stringBuilder.Append("\r\n");
      }
      return stringBuilder.ToString();
    }

    internal IEnumerable<KeyValuePair<string, string>> GetHeaderStrings()
    {
      if (this._headerStore != null)
      {
        foreach (KeyValuePair<HeaderDescriptor, HttpHeaders.HeaderStoreItemInfo> keyValuePair in this._headerStore)
        {
          string headerString = this.GetHeaderString(keyValuePair.Key, keyValuePair.Value, (object) null);
          yield return new KeyValuePair<string, string>(keyValuePair.Key.Name, headerString);
        }
      }
    }

    internal string GetHeaderString(HeaderDescriptor descriptor, object exclude = null)
    {
      HttpHeaders.HeaderStoreItemInfo info;
      return !this.TryGetHeaderInfo(descriptor, out info) ? string.Empty : this.GetHeaderString(descriptor, info, exclude);
    }

    private string GetHeaderString(
      HeaderDescriptor descriptor,
      HttpHeaders.HeaderStoreItemInfo info,
      object exclude = null)
    {
      string[] valuesAsStrings = HttpHeaders.GetValuesAsStrings(descriptor, info, exclude);
      string str;
      if (valuesAsStrings.Length == 1)
      {
        str = valuesAsStrings[0];
      }
      else
      {
        string separator = ", ";
        if (descriptor.Parser != null && descriptor.Parser.SupportsMultipleValues)
          separator = descriptor.Parser.Separator;
        str = string.Join(separator, valuesAsStrings);
      }
      return str;
    }

    /// <summary>返回可循环访问 <see cref="T:System.Net.Http.Headers.HttpHeaders" /> 实例的枚举器。</summary>
    /// <returns>用于 <see cref="T:System.Net.Http.Headers.HttpHeaders" /> 的枚举数。</returns>
    public IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator()
    {
      return this._headerStore == null || this._headerStore.Count <= 0 ? ((IEnumerable<KeyValuePair<string, IEnumerable<string>>>) Array.Empty<KeyValuePair<string, IEnumerable<string>>>()).GetEnumerator() : this.GetEnumeratorCore();
    }

    private IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumeratorCore()
    {
      foreach (KeyValuePair<HeaderDescriptor, HttpHeaders.HeaderStoreItemInfo> keyValuePair in this._headerStore)
      {
        HeaderDescriptor key = keyValuePair.Key;
        HttpHeaders.HeaderStoreItemInfo info = keyValuePair.Value;
        if (!this.ParseRawHeaderValues(key, info, false))
        {
          this._headerStore.Remove(key);
        }
        else
        {
          string[] valuesAsStrings = HttpHeaders.GetValuesAsStrings(key, info, (object) null);
          yield return new KeyValuePair<string, IEnumerable<string>>(key.Name, (IEnumerable<string>) valuesAsStrings);
        }
      }
    }

    internal IEnumerable<KeyValuePair<HeaderDescriptor, string[]>> GetHeaderDescriptorsAndValues()
    {
      return this._headerStore == null || this._headerStore.Count <= 0 ? (IEnumerable<KeyValuePair<HeaderDescriptor, string[]>>) Array.Empty<KeyValuePair<HeaderDescriptor, string[]>>() : this.GetHeaderDescriptorsAndValuesCore();
    }

    private IEnumerable<KeyValuePair<HeaderDescriptor, string[]>> GetHeaderDescriptorsAndValuesCore()
    {
      foreach (KeyValuePair<HeaderDescriptor, HttpHeaders.HeaderStoreItemInfo> keyValuePair in this._headerStore)
      {
        HeaderDescriptor key = keyValuePair.Key;
        HttpHeaders.HeaderStoreItemInfo info = keyValuePair.Value;
        if (!this.ParseRawHeaderValues(key, info, false))
        {
          this._headerStore.Remove(key);
        }
        else
        {
          string[] valuesAsStrings = HttpHeaders.GetValuesAsStrings(key, info, (object) null);
          yield return new KeyValuePair<HeaderDescriptor, string[]>(key, valuesAsStrings);
        }
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) this.GetEnumerator();
    }

    internal void AddParsedValue(HeaderDescriptor descriptor, object value)
    {
      HttpHeaders.AddValue(this.GetOrCreateHeaderInfo(descriptor, true), value, HttpHeaders.StoreLocation.Parsed);
    }

    internal void SetParsedValue(HeaderDescriptor descriptor, object value)
    {
      HttpHeaders.HeaderStoreItemInfo headerInfo = this.GetOrCreateHeaderInfo(descriptor, true);
      headerInfo.InvalidValue = (object) null;
      headerInfo.ParsedValue = (object) null;
      headerInfo.RawValue = (object) null;
      HttpHeaders.AddValue(headerInfo, value, HttpHeaders.StoreLocation.Parsed);
    }

    internal void SetOrRemoveParsedValue(HeaderDescriptor descriptor, object value)
    {
      if (value == null)
        this.Remove(descriptor);
      else
        this.SetParsedValue(descriptor, value);
    }

    internal bool Remove(HeaderDescriptor descriptor)
    {
      return this._headerStore != null && this._headerStore.Remove(descriptor);
    }

    internal bool RemoveParsedValue(HeaderDescriptor descriptor, object value)
    {
      if (this._headerStore == null)
        return false;
      HttpHeaders.HeaderStoreItemInfo info = (HttpHeaders.HeaderStoreItemInfo) null;
      if (!this.TryGetAndParseHeaderInfo(descriptor, out info))
        return false;
      bool flag = false;
      if (info.ParsedValue == null)
        return false;
      IEqualityComparer comparer = descriptor.Parser.Comparer;
      if (!(info.ParsedValue is List<object> parsedValue))
      {
        if (this.AreEqual(value, info.ParsedValue, comparer))
        {
          info.ParsedValue = (object) null;
          flag = true;
        }
      }
      else
      {
        foreach (object storeValue in parsedValue)
        {
          if (this.AreEqual(value, storeValue, comparer))
          {
            flag = parsedValue.Remove(storeValue);
            break;
          }
        }
        if (parsedValue.Count == 0)
          info.ParsedValue = (object) null;
      }
      if (info.IsEmpty)
        this.Remove(descriptor);
      return flag;
    }

    internal bool ContainsParsedValue(HeaderDescriptor descriptor, object value)
    {
      if (this._headerStore == null)
        return false;
      HttpHeaders.HeaderStoreItemInfo info = (HttpHeaders.HeaderStoreItemInfo) null;
      if (!this.TryGetAndParseHeaderInfo(descriptor, out info) || info.ParsedValue == null)
        return false;
      List<object> parsedValue = info.ParsedValue as List<object>;
      IEqualityComparer comparer = descriptor.Parser.Comparer;
      if (parsedValue == null)
        return this.AreEqual(value, info.ParsedValue, comparer);
      foreach (object storeValue in parsedValue)
      {
        if (this.AreEqual(value, storeValue, comparer))
          return true;
      }
      return false;
    }

    internal virtual void AddHeaders(HttpHeaders sourceHeaders)
    {
      if (sourceHeaders._headerStore == null)
        return;
      foreach (KeyValuePair<HeaderDescriptor, HttpHeaders.HeaderStoreItemInfo> keyValuePair in sourceHeaders._headerStore)
      {
        if (this._headerStore == null || !this._headerStore.ContainsKey(keyValuePair.Key))
        {
          HttpHeaders.HeaderStoreItemInfo headerStoreItemInfo = keyValuePair.Value;
          if (!sourceHeaders.ParseRawHeaderValues(keyValuePair.Key, headerStoreItemInfo, false))
            sourceHeaders._headerStore.Remove(keyValuePair.Key);
          else
            this.AddHeaderInfo(keyValuePair.Key, headerStoreItemInfo);
        }
      }
    }

    private void AddHeaderInfo(
      HeaderDescriptor descriptor,
      HttpHeaders.HeaderStoreItemInfo sourceInfo)
    {
      HttpHeaders.HeaderStoreItemInfo addHeaderToStore = this.CreateAndAddHeaderToStore(descriptor);
      if (descriptor.Parser == null)
      {
        addHeaderToStore.ParsedValue = HttpHeaders.CloneStringHeaderInfoValues(sourceInfo.ParsedValue);
      }
      else
      {
        addHeaderToStore.InvalidValue = HttpHeaders.CloneStringHeaderInfoValues(sourceInfo.InvalidValue);
        if (sourceInfo.ParsedValue == null)
          return;
        if (!(sourceInfo.ParsedValue is List<object> parsedValue))
        {
          HttpHeaders.CloneAndAddValue(addHeaderToStore, sourceInfo.ParsedValue);
        }
        else
        {
          foreach (object source in parsedValue)
            HttpHeaders.CloneAndAddValue(addHeaderToStore, source);
        }
      }
    }

    private static void CloneAndAddValue(
      HttpHeaders.HeaderStoreItemInfo destinationInfo,
      object source)
    {
      if (source is ICloneable cloneable)
        HttpHeaders.AddValue(destinationInfo, cloneable.Clone(), HttpHeaders.StoreLocation.Parsed);
      else
        HttpHeaders.AddValue(destinationInfo, source, HttpHeaders.StoreLocation.Parsed);
    }

    private static object CloneStringHeaderInfoValues(object source)
    {
      if (source == null)
        return (object) null;
      return !(source is List<object> objectList) ? source : (object) new List<object>((IEnumerable<object>) objectList);
    }

    private HttpHeaders.HeaderStoreItemInfo GetOrCreateHeaderInfo(
      HeaderDescriptor descriptor,
      bool parseRawValues)
    {
      HttpHeaders.HeaderStoreItemInfo info = (HttpHeaders.HeaderStoreItemInfo) null;
      if (!(!parseRawValues ? this.TryGetHeaderInfo(descriptor, out info) : this.TryGetAndParseHeaderInfo(descriptor, out info)))
        info = this.CreateAndAddHeaderToStore(descriptor);
      return info;
    }

    private HttpHeaders.HeaderStoreItemInfo CreateAndAddHeaderToStore(
      HeaderDescriptor descriptor)
    {
      HttpHeaders.HeaderStoreItemInfo info = new HttpHeaders.HeaderStoreItemInfo();
      this.AddHeaderToStore(descriptor, info);
      return info;
    }

    private void AddHeaderToStore(HeaderDescriptor descriptor, HttpHeaders.HeaderStoreItemInfo info)
    {
      if (this._headerStore == null)
        this._headerStore = new Dictionary<HeaderDescriptor, HttpHeaders.HeaderStoreItemInfo>();
      this._headerStore.Add(descriptor, info);
    }

    private bool TryGetHeaderInfo(
      HeaderDescriptor descriptor,
      out HttpHeaders.HeaderStoreItemInfo info)
    {
      if (this._headerStore != null)
        return this._headerStore.TryGetValue(descriptor, out info);
      info = (HttpHeaders.HeaderStoreItemInfo) null;
      return false;
    }

    private bool TryGetAndParseHeaderInfo(
      HeaderDescriptor key,
      out HttpHeaders.HeaderStoreItemInfo info)
    {
      return this.TryGetHeaderInfo(key, out info) && this.ParseRawHeaderValues(key, info, true);
    }

    private bool ParseRawHeaderValues(
      HeaderDescriptor descriptor,
      HttpHeaders.HeaderStoreItemInfo info,
      bool removeEmptyHeader)
    {
      lock (info)
      {
        if (info.RawValue != null)
        {
          if (!(info.RawValue is List<string> rawValue))
            HttpHeaders.ParseSingleRawHeaderValue(descriptor, info);
          else
            HttpHeaders.ParseMultipleRawHeaderValues(descriptor, info, rawValue);
          info.RawValue = (object) null;
          if (info.InvalidValue == null)
          {
            if (info.ParsedValue == null)
            {
              if (removeEmptyHeader)
                this._headerStore.Remove(descriptor);
              return false;
            }
          }
        }
      }
      return true;
    }

    private static void ParseMultipleRawHeaderValues(
      HeaderDescriptor descriptor,
      HttpHeaders.HeaderStoreItemInfo info,
      List<string> rawValues)
    {
      if (descriptor.Parser == null)
      {
        foreach (string rawValue in rawValues)
        {
          if (!HttpHeaders.ContainsInvalidNewLine(rawValue, descriptor.Name))
            HttpHeaders.AddValue(info, (object) rawValue, HttpHeaders.StoreLocation.Parsed);
        }
      }
      else
      {
        foreach (string rawValue in rawValues)
        {
          if (!HttpHeaders.TryParseAndAddRawHeaderValue(descriptor, info, rawValue, true) && NetEventSource.IsEnabled)
            NetEventSource.Log.HeadersInvalidValue(descriptor.Name, rawValue);
        }
      }
    }

    private static void ParseSingleRawHeaderValue(
      HeaderDescriptor descriptor,
      HttpHeaders.HeaderStoreItemInfo info)
    {
      string rawValue = info.RawValue as string;
      if (descriptor.Parser == null)
      {
        if (HttpHeaders.ContainsInvalidNewLine(rawValue, descriptor.Name))
          return;
        HttpHeaders.AddValue(info, (object) rawValue, HttpHeaders.StoreLocation.Parsed);
      }
      else
      {
        if (HttpHeaders.TryParseAndAddRawHeaderValue(descriptor, info, rawValue, true) || !NetEventSource.IsEnabled)
          return;
        NetEventSource.Log.HeadersInvalidValue(descriptor.Name, rawValue);
      }
    }

    internal bool TryParseAndAddValue(HeaderDescriptor descriptor, string value)
    {
      HttpHeaders.HeaderStoreItemInfo info;
      bool addToStore;
      this.PrepareHeaderInfoForAdd(descriptor, out info, out addToStore);
      bool addRawHeaderValue = HttpHeaders.TryParseAndAddRawHeaderValue(descriptor, info, value, false);
      if (addRawHeaderValue & addToStore && info.ParsedValue != null)
        this.AddHeaderToStore(descriptor, info);
      return addRawHeaderValue;
    }

    private static bool TryParseAndAddRawHeaderValue(
      HeaderDescriptor descriptor,
      HttpHeaders.HeaderStoreItemInfo info,
      string value,
      bool addWhenInvalid)
    {
      if (!info.CanAddValue(descriptor.Parser))
      {
        if (addWhenInvalid)
          HttpHeaders.AddValue(info, (object) (value ?? string.Empty), HttpHeaders.StoreLocation.Invalid);
        return false;
      }
      int index = 0;
      object parsedValue = (object) null;
      if (descriptor.Parser.TryParseValue(value, info.ParsedValue, ref index, out parsedValue))
      {
        if (value == null || index == value.Length)
        {
          if (parsedValue != null)
            HttpHeaders.AddValue(info, parsedValue, HttpHeaders.StoreLocation.Parsed);
          return true;
        }
        List<object> objectList = new List<object>();
        if (parsedValue != null)
          objectList.Add(parsedValue);
        while (index < value.Length)
        {
          if (descriptor.Parser.TryParseValue(value, info.ParsedValue, ref index, out parsedValue))
          {
            if (parsedValue != null)
              objectList.Add(parsedValue);
          }
          else
          {
            if (!HttpHeaders.ContainsInvalidNewLine(value, descriptor.Name) & addWhenInvalid)
              HttpHeaders.AddValue(info, (object) value, HttpHeaders.StoreLocation.Invalid);
            return false;
          }
        }
        foreach (object obj in objectList)
          HttpHeaders.AddValue(info, obj, HttpHeaders.StoreLocation.Parsed);
        return true;
      }
      if (!HttpHeaders.ContainsInvalidNewLine(value, descriptor.Name) & addWhenInvalid)
        HttpHeaders.AddValue(info, (object) (value ?? string.Empty), HttpHeaders.StoreLocation.Invalid);
      return false;
    }

    private static void AddValue(
      HttpHeaders.HeaderStoreItemInfo info,
      object value,
      HttpHeaders.StoreLocation location)
    {
      switch (location)
      {
        case HttpHeaders.StoreLocation.Raw:
          object rawValue = info.RawValue;
          HttpHeaders.AddValueToStoreValue<string>(value, ref rawValue);
          info.RawValue = rawValue;
          break;
        case HttpHeaders.StoreLocation.Invalid:
          object invalidValue = info.InvalidValue;
          HttpHeaders.AddValueToStoreValue<string>(value, ref invalidValue);
          info.InvalidValue = invalidValue;
          break;
        case HttpHeaders.StoreLocation.Parsed:
          object parsedValue = info.ParsedValue;
          HttpHeaders.AddValueToStoreValue<object>(value, ref parsedValue);
          info.ParsedValue = parsedValue;
          break;
      }
    }

    private static void AddValueToStoreValue<T>(object value, ref object currentStoreValue) where T : class
    {
      if (currentStoreValue == null)
      {
        currentStoreValue = value;
      }
      else
      {
        if (!(currentStoreValue is List<T> objList))
        {
          objList = new List<T>(2);
          objList.Add(currentStoreValue as T);
          currentStoreValue = (object) objList;
        }
        objList.Add(value as T);
      }
    }

    internal object GetParsedValues(HeaderDescriptor descriptor)
    {
      HttpHeaders.HeaderStoreItemInfo info = (HttpHeaders.HeaderStoreItemInfo) null;
      return !this.TryGetAndParseHeaderInfo(descriptor, out info) ? (object) null : info.ParsedValue;
    }

    private void PrepareHeaderInfoForAdd(
      HeaderDescriptor descriptor,
      out HttpHeaders.HeaderStoreItemInfo info,
      out bool addToStore)
    {
      info = (HttpHeaders.HeaderStoreItemInfo) null;
      addToStore = false;
      if (this.TryGetAndParseHeaderInfo(descriptor, out info))
        return;
      info = new HttpHeaders.HeaderStoreItemInfo();
      addToStore = true;
    }

    private void ParseAndAddValue(
      HeaderDescriptor descriptor,
      HttpHeaders.HeaderStoreItemInfo info,
      string value)
    {
      if (descriptor.Parser == null)
      {
        HttpHeaders.CheckInvalidNewLine(value);
        HttpHeaders.AddValue(info, (object) (value ?? string.Empty), HttpHeaders.StoreLocation.Parsed);
      }
      else
      {
        if (!info.CanAddValue(descriptor.Parser))
          throw new FormatException(SR.Format((IFormatProvider) CultureInfo.InvariantCulture, SR.net_http_headers_single_value_header, (object) descriptor.Name));
        int index = 0;
        object obj1 = descriptor.Parser.ParseValue(value, info.ParsedValue, ref index);
        if (value == null || index == value.Length)
        {
          if (obj1 == null)
            return;
          HttpHeaders.AddValue(info, obj1, HttpHeaders.StoreLocation.Parsed);
        }
        else
        {
          List<object> objectList = new List<object>();
          if (obj1 != null)
            objectList.Add(obj1);
          while (index < value.Length)
          {
            object obj2 = descriptor.Parser.ParseValue(value, info.ParsedValue, ref index);
            if (obj2 != null)
              objectList.Add(obj2);
          }
          foreach (object obj2 in objectList)
            HttpHeaders.AddValue(info, obj2, HttpHeaders.StoreLocation.Parsed);
        }
      }
    }

    private HeaderDescriptor GetHeaderDescriptor(string name)
    {
      if (string.IsNullOrEmpty(name))
        throw new ArgumentException(SR.net_http_argument_empty_string, nameof (name));
      HeaderDescriptor descriptor;
      if (!HeaderDescriptor.TryGet(name, out descriptor))
        throw new FormatException(SR.net_http_headers_invalid_header_name);
      if ((descriptor.HeaderType & this._allowedHeaderTypes) != HttpHeaderType.None)
        return descriptor;
      if ((descriptor.HeaderType & this._treatAsCustomHeaderTypes) != HttpHeaderType.None)
        return descriptor.AsCustomHeader();
      throw new InvalidOperationException(SR.net_http_headers_not_allowed_header_name);
    }

    private bool TryGetHeaderDescriptor(string name, out HeaderDescriptor descriptor)
    {
      if (string.IsNullOrEmpty(name))
      {
        descriptor = new HeaderDescriptor();
        return false;
      }
      if (!HeaderDescriptor.TryGet(name, out descriptor))
        return false;
      if ((descriptor.HeaderType & this._allowedHeaderTypes) != HttpHeaderType.None)
        return true;
      if ((descriptor.HeaderType & this._treatAsCustomHeaderTypes) == HttpHeaderType.None)
        return false;
      descriptor = descriptor.AsCustomHeader();
      return true;
    }

    private static void CheckInvalidNewLine(string value)
    {
      if (value != null && HttpRuleParser.ContainsInvalidNewLine(value))
        throw new FormatException(SR.net_http_headers_no_newlines);
    }

    private static bool ContainsInvalidNewLine(string value, string name)
    {
      if (!HttpRuleParser.ContainsInvalidNewLine(value))
        return false;
      if (NetEventSource.IsEnabled)
        NetEventSource.Error((object) null, (object) SR.Format(SR.net_http_log_headers_no_newlines, (object) name, (object) value), nameof (ContainsInvalidNewLine));
      return true;
    }

    private static string[] GetValuesAsStrings(
      HeaderDescriptor descriptor,
      HttpHeaders.HeaderStoreItemInfo info,
      object exclude = null)
    {
      int valueCount = HttpHeaders.GetValueCount(info);
      string[] values;
      if (valueCount > 0)
      {
        values = new string[valueCount];
        int currentIndex = 0;
        HttpHeaders.ReadStoreValues<string>(values, info.RawValue, (HttpHeaderParser) null, (string) null, ref currentIndex);
        HttpHeaders.ReadStoreValues<object>(values, info.ParsedValue, descriptor.Parser, exclude, ref currentIndex);
        HttpHeaders.ReadStoreValues<string>(values, info.InvalidValue, (HttpHeaderParser) null, (string) null, ref currentIndex);
        if (currentIndex < valueCount)
        {
          string[] strArray = new string[currentIndex];
          Array.Copy((Array) values, 0, (Array) strArray, 0, currentIndex);
          values = strArray;
        }
      }
      else
        values = Array.Empty<string>();
      return values;
    }

    private static int GetValueCount(HttpHeaders.HeaderStoreItemInfo info)
    {
      int valueCount = 0;
      HttpHeaders.UpdateValueCount<string>(info.RawValue, ref valueCount);
      HttpHeaders.UpdateValueCount<string>(info.InvalidValue, ref valueCount);
      HttpHeaders.UpdateValueCount<object>(info.ParsedValue, ref valueCount);
      return valueCount;
    }

    private static void UpdateValueCount<T>(object valueStore, ref int valueCount)
    {
      if (valueStore == null)
        return;
      if (valueStore is List<T> objList)
        valueCount += objList.Count;
      else
        ++valueCount;
    }

    private static void ReadStoreValues<T>(
      string[] values,
      object storeValue,
      HttpHeaderParser parser,
      T exclude,
      ref int currentIndex)
    {
      if (storeValue == null)
        return;
      if (!(storeValue is List<T> objList))
      {
        if (!HttpHeaders.ShouldAdd<T>(storeValue, parser, exclude))
          return;
        values[currentIndex] = parser == null ? storeValue.ToString() : parser.ToString(storeValue);
        ++currentIndex;
      }
      else
      {
        foreach (T obj in objList)
        {
          object storeValue1 = (object) obj;
          if (HttpHeaders.ShouldAdd<T>(storeValue1, parser, exclude))
          {
            values[currentIndex] = parser == null ? storeValue1.ToString() : parser.ToString(storeValue1);
            ++currentIndex;
          }
        }
      }
    }

    private static bool ShouldAdd<T>(object storeValue, HttpHeaderParser parser, T exclude)
    {
      bool flag = true;
      if (parser != null && (object) exclude != null)
        flag = parser.Comparer == null ? !exclude.Equals(storeValue) : !parser.Comparer.Equals((object) exclude, storeValue);
      return flag;
    }

    private bool AreEqual(object value, object storeValue, IEqualityComparer comparer)
    {
      return comparer != null ? comparer.Equals(value, storeValue) : value.Equals(storeValue);
    }

    private enum StoreLocation
    {
      Raw,
      Invalid,
      Parsed,
    }

    private class HeaderStoreItemInfo
    {
      private object _rawValue;
      private object _invalidValue;
      private object _parsedValue;

      internal object RawValue
      {
        get
        {
          return this._rawValue;
        }
        set
        {
          this._rawValue = value;
        }
      }

      internal object InvalidValue
      {
        get
        {
          return this._invalidValue;
        }
        set
        {
          this._invalidValue = value;
        }
      }

      internal object ParsedValue
      {
        get
        {
          return this._parsedValue;
        }
        set
        {
          this._parsedValue = value;
        }
      }

      internal bool CanAddValue(HttpHeaderParser parser)
      {
        if (parser.SupportsMultipleValues)
          return true;
        return this._invalidValue == null && this._parsedValue == null;
      }

      internal bool IsEmpty
      {
        get
        {
          return this._rawValue == null && this._invalidValue == null && this._parsedValue == null;
        }
      }

      internal HeaderStoreItemInfo()
      {
      }
    }
  }
}

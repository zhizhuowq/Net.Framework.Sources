// Decompiled with JetBrains decompiler
// Type: System.Net.Http.Headers.NameValueHeaderValue
// Assembly: System.Net.Http, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: D8D12B97-8555-48F6-96B3-BBC6BF89FCBA
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Net.Http.dll

using System.Globalization;
using System.Text;

namespace System.Net.Http.Headers
{
  /// <summary>表示用于各种标头的在 RFC 2616 中定义的名称/值对。</summary>
  public class NameValueHeaderValue : ICloneable
  {
    private static readonly Func<NameValueHeaderValue> s_defaultNameValueCreator = new Func<NameValueHeaderValue>(NameValueHeaderValue.CreateNameValue);
    private string _name;
    private string _value;

    /// <summary>获取标头名称。</summary>
    /// <returns>标头名称。</returns>
    public string Name
    {
      get
      {
        return this._name;
      }
    }

    /// <summary>获取标头值。</summary>
    /// <returns>标头值。</returns>
    public string Value
    {
      get
      {
        return this._value;
      }
      set
      {
        NameValueHeaderValue.CheckValueFormat(value);
        this._value = value;
      }
    }

    internal NameValueHeaderValue()
    {
    }

    /// <summary>初始化 <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> 类的新实例。</summary>
    /// <param name="name">标头名称。</param>
    public NameValueHeaderValue(string name)
      : this(name, (string) null)
    {
    }

    /// <summary>初始化 <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> 类的新实例。</summary>
    /// <param name="name">标头名称。</param>
    /// <param name="value">标头值。</param>
    public NameValueHeaderValue(string name, string value)
    {
      NameValueHeaderValue.CheckNameValueFormat(name, value);
      this._name = name;
      this._value = value;
    }

    /// <summary>初始化 <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> 类的新实例。</summary>
    /// <param name="source">用于初始化新实例的 <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> 对象。</param>
    protected NameValueHeaderValue(NameValueHeaderValue source)
    {
      this._name = source._name;
      this._value = source._value;
    }

    /// <summary>充当 <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> 对象的哈希函数。</summary>
    /// <returns>当前对象的哈希代码。</returns>
    public override int GetHashCode()
    {
      int hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(this._name);
      if (string.IsNullOrEmpty(this._value))
        return hashCode;
      return this._value[0] == '"' ? hashCode ^ this._value.GetHashCode() : hashCode ^ StringComparer.OrdinalIgnoreCase.GetHashCode(this._value);
    }

    /// <summary>确定指定的 <see cref="T:System.Object" /> 是否等于当前的 <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> 对象。</summary>
    /// <param name="obj">要与当前对象进行比较的对象。</param>
    /// <returns>如果指定的 <see cref="T:System.Object" /> 等于当前的对象，则为 <see langword="true" />；否则为 <see langword="false" />。</returns>
    public override bool Equals(object obj)
    {
      if (!(obj is NameValueHeaderValue valueHeaderValue) || !string.Equals(this._name, valueHeaderValue._name, StringComparison.OrdinalIgnoreCase))
        return false;
      if (string.IsNullOrEmpty(this._value))
        return string.IsNullOrEmpty(valueHeaderValue._value);
      return this._value[0] == '"' ? string.Equals(this._value, valueHeaderValue._value, StringComparison.Ordinal) : string.Equals(this._value, valueHeaderValue._value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>将字符串转换为 <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> 实例。</summary>
    /// <param name="input">表示名称值标头值信息的字符串。</param>
    /// <returns>一个 <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> 实例。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="input" /> 是一个 <see langword="null" /> 引用。</exception>
    /// <exception cref="T:System.FormatException">
    /// <paramref name="input" /> 为无效名称值标头值信息。</exception>
    public static NameValueHeaderValue Parse(string input)
    {
      int index = 0;
      return (NameValueHeaderValue) GenericHeaderParser.SingleValueNameValueParser.ParseValue(input, (object) null, ref index);
    }

    /// <summary>确定一个字符串是否为有效的 <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> 信息。</summary>
    /// <param name="input">要验证的字符串。</param>
    /// <param name="parsedValue">字符串的 <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> 版本。</param>
    /// <returns>如果 <paramref name="input" /> 为有效 <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> 信息，则为 <see langword="true" />；否则为 <see langword="false" />。</returns>
    public static bool TryParse(string input, out NameValueHeaderValue parsedValue)
    {
      int index = 0;
      parsedValue = (NameValueHeaderValue) null;
      object parsedValue1;
      if (!GenericHeaderParser.SingleValueNameValueParser.TryParseValue(input, (object) null, ref index, out parsedValue1))
        return false;
      parsedValue = (NameValueHeaderValue) parsedValue1;
      return true;
    }

    /// <summary>返回表示当前 <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> 对象的字符串。</summary>
    /// <returns>表示当前对象的字符串。</returns>
    public override string ToString()
    {
      return !string.IsNullOrEmpty(this._value) ? this._name + "=" + this._value : this._name;
    }

    private void AddToStringBuilder(StringBuilder sb)
    {
      if (this.GetType() != typeof (NameValueHeaderValue))
      {
        sb.Append(this.ToString());
      }
      else
      {
        sb.Append(this._name);
        if (string.IsNullOrEmpty(this._value))
          return;
        sb.Append('=');
        sb.Append(this._value);
      }
    }

    internal static void ToString(
      ObjectCollection<NameValueHeaderValue> values,
      char separator,
      bool leadingSeparator,
      StringBuilder destination)
    {
      if (values == null || values.Count == 0)
        return;
      foreach (NameValueHeaderValue valueHeaderValue in values)
      {
        if (leadingSeparator || destination.Length > 0)
        {
          destination.Append(separator);
          destination.Append(' ');
        }
        valueHeaderValue.AddToStringBuilder(destination);
      }
    }

    internal static int GetHashCode(ObjectCollection<NameValueHeaderValue> values)
    {
      if (values == null || values.Count == 0)
        return 0;
      int num = 0;
      foreach (NameValueHeaderValue valueHeaderValue in values)
        num ^= valueHeaderValue.GetHashCode();
      return num;
    }

    internal static int GetNameValueLength(
      string input,
      int startIndex,
      out NameValueHeaderValue parsedValue)
    {
      return NameValueHeaderValue.GetNameValueLength(input, startIndex, NameValueHeaderValue.s_defaultNameValueCreator, out parsedValue);
    }

    internal static int GetNameValueLength(
      string input,
      int startIndex,
      Func<NameValueHeaderValue> nameValueCreator,
      out NameValueHeaderValue parsedValue)
    {
      parsedValue = (NameValueHeaderValue) null;
      if (string.IsNullOrEmpty(input) || startIndex >= input.Length)
        return 0;
      int tokenLength = HttpRuleParser.GetTokenLength(input, startIndex);
      if (tokenLength == 0)
        return 0;
      string str = input.Substring(startIndex, tokenLength);
      int startIndex1 = startIndex + tokenLength;
      int startIndex2 = startIndex1 + HttpRuleParser.GetWhitespaceLength(input, startIndex1);
      if (startIndex2 == input.Length || input[startIndex2] != '=')
      {
        parsedValue = nameValueCreator();
        parsedValue._name = str;
        return startIndex2 + HttpRuleParser.GetWhitespaceLength(input, startIndex2) - startIndex;
      }
      int startIndex3 = startIndex2 + 1;
      int startIndex4 = startIndex3 + HttpRuleParser.GetWhitespaceLength(input, startIndex3);
      int valueLength = NameValueHeaderValue.GetValueLength(input, startIndex4);
      if (valueLength == 0)
        return 0;
      parsedValue = nameValueCreator();
      parsedValue._name = str;
      parsedValue._value = input.Substring(startIndex4, valueLength);
      int startIndex5 = startIndex4 + valueLength;
      return startIndex5 + HttpRuleParser.GetWhitespaceLength(input, startIndex5) - startIndex;
    }

    internal static int GetNameValueListLength(
      string input,
      int startIndex,
      char delimiter,
      ObjectCollection<NameValueHeaderValue> nameValueCollection)
    {
      if (string.IsNullOrEmpty(input) || startIndex >= input.Length)
        return 0;
      int startIndex1 = startIndex + HttpRuleParser.GetWhitespaceLength(input, startIndex);
      int index;
      while (true)
      {
        NameValueHeaderValue parsedValue = (NameValueHeaderValue) null;
        int nameValueLength = NameValueHeaderValue.GetNameValueLength(input, startIndex1, NameValueHeaderValue.s_defaultNameValueCreator, out parsedValue);
        if (nameValueLength != 0)
        {
          nameValueCollection.Add(parsedValue);
          int startIndex2 = startIndex1 + nameValueLength;
          index = startIndex2 + HttpRuleParser.GetWhitespaceLength(input, startIndex2);
          if (index != input.Length && (int) input[index] == (int) delimiter)
          {
            int startIndex3 = index + 1;
            startIndex1 = startIndex3 + HttpRuleParser.GetWhitespaceLength(input, startIndex3);
          }
          else
            goto label_6;
        }
        else
          break;
      }
      return 0;
label_6:
      return index - startIndex;
    }

    internal static NameValueHeaderValue Find(
      ObjectCollection<NameValueHeaderValue> values,
      string name)
    {
      if (values == null || values.Count == 0)
        return (NameValueHeaderValue) null;
      foreach (NameValueHeaderValue valueHeaderValue in values)
      {
        if (string.Equals(valueHeaderValue.Name, name, StringComparison.OrdinalIgnoreCase))
          return valueHeaderValue;
      }
      return (NameValueHeaderValue) null;
    }

    internal static int GetValueLength(string input, int startIndex)
    {
      if (startIndex >= input.Length)
        return 0;
      int length = HttpRuleParser.GetTokenLength(input, startIndex);
      return length == 0 && HttpRuleParser.GetQuotedStringLength(input, startIndex, out length) != HttpParseResult.Parsed ? 0 : length;
    }

    private static void CheckNameValueFormat(string name, string value)
    {
      HeaderUtilities.CheckValidToken(name, nameof (name));
      NameValueHeaderValue.CheckValueFormat(value);
    }

    private static void CheckValueFormat(string value)
    {
      if (!string.IsNullOrEmpty(value) && NameValueHeaderValue.GetValueLength(value, 0) != value.Length)
        throw new FormatException(SR.Format((IFormatProvider) CultureInfo.InvariantCulture, SR.net_http_headers_invalid_value, (object) value));
    }

    private static NameValueHeaderValue CreateNameValue()
    {
      return new NameValueHeaderValue();
    }

    object ICloneable.Clone()
    {
      return (object) new NameValueHeaderValue(this);
    }
  }
}

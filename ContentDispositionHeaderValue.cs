// Decompiled with JetBrains decompiler
// Type: System.Net.Http.Headers.ContentDispositionHeaderValue
// Assembly: System.Net.Http, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: D8D12B97-8555-48F6-96B3-BBC6BF89FCBA
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Net.Http.dll

using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System.Net.Http.Headers
{
  /// <summary>表示 Content-Disposition 标头的值。</summary>
  public class ContentDispositionHeaderValue : ICloneable
  {
    private ObjectCollection<NameValueHeaderValue> _parameters;
    private string _dispositionType;

    /// <summary>内容正文部分的处置类型。</summary>
    /// <returns>处置类型。</returns>
    public string DispositionType
    {
      get
      {
        return this._dispositionType;
      }
      set
      {
        ContentDispositionHeaderValue.CheckDispositionTypeFormat(value, nameof (value));
        this._dispositionType = value;
      }
    }

    /// <summary>一组包含 Content-disposition 标头的参数。</summary>
    /// <returns>参数的集合。</returns>
    public ICollection<NameValueHeaderValue> Parameters
    {
      get
      {
        if (this._parameters == null)
          this._parameters = new ObjectCollection<NameValueHeaderValue>();
        return (ICollection<NameValueHeaderValue>) this._parameters;
      }
    }

    /// <summary>内容正文部分的名称。</summary>
    /// <returns>内容正文部分的名称。</returns>
    public string Name
    {
      get
      {
        return this.GetName("name");
      }
      set
      {
        this.SetName("name", value);
      }
    }

    /// <summary>有关如果实体被分离并存储在单独的文件中时，如何构建用于存储要使用的消息有效载荷的文件名的建议。</summary>
    /// <returns>建议的文件名。</returns>
    public string FileName
    {
      get
      {
        return this.GetName("filename");
      }
      set
      {
        this.SetName("filename", value);
      }
    }

    /// <summary>一条建议，提议在实体被分离并存储在单独文件中时如何构建文件名来存储要使用的消息有效负载。</summary>
    /// <returns>建议的表单文件名*。</returns>
    public string FileNameStar
    {
      get
      {
        return this.GetName("filename*");
      }
      set
      {
        this.SetName("filename*", value);
      }
    }

    /// <summary>创建文件的日期。</summary>
    /// <returns>文件创建日期。</returns>
    public DateTimeOffset? CreationDate
    {
      get
      {
        return this.GetDate("creation-date");
      }
      set
      {
        this.SetDate("creation-date", value);
      }
    }

    /// <summary>上次修改文件的日期。</summary>
    /// <returns>文件修改日期。</returns>
    public DateTimeOffset? ModificationDate
    {
      get
      {
        return this.GetDate("modification-date");
      }
      set
      {
        this.SetDate("modification-date", value);
      }
    }

    /// <summary>上次读取文件的日期。</summary>
    /// <returns>上次读取日期。</returns>
    public DateTimeOffset? ReadDate
    {
      get
      {
        return this.GetDate("read-date");
      }
      set
      {
        this.SetDate("read-date", value);
      }
    }

    /// <summary>文件的近似大小（以字节为单位）。</summary>
    /// <returns>以字节为单位的近似大小。</returns>
    public long? Size
    {
      get
      {
        NameValueHeaderValue valueHeaderValue = NameValueHeaderValue.Find(this._parameters, "size");
        ulong result;
        return valueHeaderValue != null && ulong.TryParse(valueHeaderValue.Value, NumberStyles.Integer, (IFormatProvider) CultureInfo.InvariantCulture, out result) ? new long?((long) result) : new long?();
      }
      set
      {
        NameValueHeaderValue valueHeaderValue = NameValueHeaderValue.Find(this._parameters, "size");
        if (!value.HasValue)
        {
          if (valueHeaderValue == null)
            return;
          this._parameters.Remove(valueHeaderValue);
        }
        else
        {
          long? nullable = value;
          long num = 0;
          if (nullable.GetValueOrDefault() < num & nullable.HasValue)
            throw new ArgumentOutOfRangeException(nameof (value));
          if (valueHeaderValue != null)
            valueHeaderValue.Value = value.Value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
          else
            this.Parameters.Add(new NameValueHeaderValue("size", value.Value.ToString((IFormatProvider) CultureInfo.InvariantCulture)));
        }
      }
    }

    internal ContentDispositionHeaderValue()
    {
    }

    /// <summary>初始化 <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> 类的新实例。</summary>
    /// <param name="source">
    /// <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" />。</param>
    protected ContentDispositionHeaderValue(ContentDispositionHeaderValue source)
    {
      this._dispositionType = source._dispositionType;
      if (source._parameters == null)
        return;
      foreach (ICloneable parameter in source._parameters)
        this.Parameters.Add((NameValueHeaderValue) parameter.Clone());
    }

    /// <summary>初始化 <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> 类的新实例。</summary>
    /// <param name="dispositionType">包含 <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> 的字符串。</param>
    public ContentDispositionHeaderValue(string dispositionType)
    {
      ContentDispositionHeaderValue.CheckDispositionTypeFormat(dispositionType, nameof (dispositionType));
      this._dispositionType = dispositionType;
    }

    /// <summary>返回表示当前 <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> 对象的字符串。</summary>
    /// <returns>表示当前对象的字符串。</returns>
    public override string ToString()
    {
      StringBuilder stringBuilder = StringBuilderCache.Acquire(16);
      stringBuilder.Append(this._dispositionType);
      NameValueHeaderValue.ToString(this._parameters, ';', true, stringBuilder);
      return StringBuilderCache.GetStringAndRelease(stringBuilder);
    }

    /// <summary>确定指定的 <see cref="T:System.Object" /> 是否等于当前的 <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> 对象。</summary>
    /// <param name="obj">要与当前对象进行比较的对象。</param>
    /// <returns>如果指定的 <see cref="T:System.Object" /> 等于当前的对象，则为 <see langword="true" />；否则为 <see langword="false" />。</returns>
    public override bool Equals(object obj)
    {
      return obj is ContentDispositionHeaderValue dispositionHeaderValue && string.Equals(this._dispositionType, dispositionHeaderValue._dispositionType, StringComparison.OrdinalIgnoreCase) && HeaderUtilities.AreEqualCollections<NameValueHeaderValue>(this._parameters, dispositionHeaderValue._parameters);
    }

    /// <summary>充当 <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> 对象的哈希函数。</summary>
    /// <returns>当前对象的哈希代码。</returns>
    public override int GetHashCode()
    {
      return StringComparer.OrdinalIgnoreCase.GetHashCode(this._dispositionType) ^ NameValueHeaderValue.GetHashCode(this._parameters);
    }

    object ICloneable.Clone()
    {
      return (object) new ContentDispositionHeaderValue(this);
    }

    /// <summary>将字符串转换为 <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> 实例。</summary>
    /// <param name="input">表示内容处置标头值信息的字符串。</param>
    /// <returns>一个 <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> 实例。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="input" /> 是一个 <see langword="null" /> 引用。</exception>
    /// <exception cref="T:System.FormatException">
    /// <paramref name="input" /> 为无效内容处置标头值信息。</exception>
    public static ContentDispositionHeaderValue Parse(string input)
    {
      int index = 0;
      return (ContentDispositionHeaderValue) GenericHeaderParser.ContentDispositionParser.ParseValue(input, (object) null, ref index);
    }

    /// <summary>确定一个字符串是否为有效的 <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> 信息。</summary>
    /// <param name="input">要验证的字符串。</param>
    /// <param name="parsedValue">字符串的 <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> 版本。</param>
    /// <returns>如果 <paramref name="input" /> 为有效 <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> 信息，则为 <see langword="true" />；否则为 <see langword="false" />。</returns>
    public static bool TryParse(string input, out ContentDispositionHeaderValue parsedValue)
    {
      int index = 0;
      parsedValue = (ContentDispositionHeaderValue) null;
      object parsedValue1;
      if (!GenericHeaderParser.ContentDispositionParser.TryParseValue(input, (object) null, ref index, out parsedValue1))
        return false;
      parsedValue = (ContentDispositionHeaderValue) parsedValue1;
      return true;
    }

    internal static int GetDispositionTypeLength(
      string input,
      int startIndex,
      out object parsedValue)
    {
      parsedValue = (object) null;
      if (string.IsNullOrEmpty(input) || startIndex >= input.Length)
        return 0;
      string dispositionType = (string) null;
      int expressionLength = ContentDispositionHeaderValue.GetDispositionTypeExpressionLength(input, startIndex, out dispositionType);
      if (expressionLength == 0)
        return 0;
      int startIndex1 = startIndex + expressionLength;
      int index = startIndex1 + HttpRuleParser.GetWhitespaceLength(input, startIndex1);
      ContentDispositionHeaderValue dispositionHeaderValue = new ContentDispositionHeaderValue();
      dispositionHeaderValue._dispositionType = dispositionType;
      if (index < input.Length && input[index] == ';')
      {
        int startIndex2 = index + 1;
        int nameValueListLength = NameValueHeaderValue.GetNameValueListLength(input, startIndex2, ';', (ObjectCollection<NameValueHeaderValue>) dispositionHeaderValue.Parameters);
        if (nameValueListLength == 0)
          return 0;
        parsedValue = (object) dispositionHeaderValue;
        return startIndex2 + nameValueListLength - startIndex;
      }
      parsedValue = (object) dispositionHeaderValue;
      return index - startIndex;
    }

    private static int GetDispositionTypeExpressionLength(
      string input,
      int startIndex,
      out string dispositionType)
    {
      dispositionType = (string) null;
      int tokenLength = HttpRuleParser.GetTokenLength(input, startIndex);
      if (tokenLength == 0)
        return 0;
      dispositionType = input.Substring(startIndex, tokenLength);
      return tokenLength;
    }

    private static void CheckDispositionTypeFormat(string dispositionType, string parameterName)
    {
      if (string.IsNullOrEmpty(dispositionType))
        throw new ArgumentException(SR.net_http_argument_empty_string, parameterName);
      string dispositionType1;
      if (ContentDispositionHeaderValue.GetDispositionTypeExpressionLength(dispositionType, 0, out dispositionType1) == 0 || dispositionType1.Length != dispositionType.Length)
        throw new FormatException(SR.Format((IFormatProvider) CultureInfo.InvariantCulture, SR.net_http_headers_invalid_value, (object) dispositionType));
    }

    private DateTimeOffset? GetDate(string parameter)
    {
      NameValueHeaderValue valueHeaderValue = NameValueHeaderValue.Find(this._parameters, parameter);
      if (valueHeaderValue != null)
      {
        ReadOnlySpan<char> input = (ReadOnlySpan<char>) valueHeaderValue.Value;
        if (ContentDispositionHeaderValue.IsQuoted(input))
          input = input.Slice(1, input.Length - 2);
        DateTimeOffset result;
        if (HttpDateParser.TryStringToDate(input, out result))
          return new DateTimeOffset?(result);
      }
      return new DateTimeOffset?();
    }

    private void SetDate(string parameter, DateTimeOffset? date)
    {
      NameValueHeaderValue valueHeaderValue = NameValueHeaderValue.Find(this._parameters, parameter);
      if (!date.HasValue)
      {
        if (valueHeaderValue == null)
          return;
        this._parameters.Remove(valueHeaderValue);
      }
      else
      {
        string str = "\"" + HttpDateParser.DateToString(date.Value) + "\"";
        if (valueHeaderValue != null)
          valueHeaderValue.Value = str;
        else
          this.Parameters.Add(new NameValueHeaderValue(parameter, str));
      }
    }

    private string GetName(string parameter)
    {
      NameValueHeaderValue valueHeaderValue = NameValueHeaderValue.Find(this._parameters, parameter);
      if (valueHeaderValue == null)
        return (string) null;
      string output1;
      string output2;
      return parameter.EndsWith("*", StringComparison.Ordinal) ? (ContentDispositionHeaderValue.TryDecode5987(valueHeaderValue.Value, out output1) ? output1 : (string) null) : (ContentDispositionHeaderValue.TryDecodeMime(valueHeaderValue.Value, out output2) ? output2 : valueHeaderValue.Value);
    }

    private void SetName(string parameter, string value)
    {
      NameValueHeaderValue valueHeaderValue = NameValueHeaderValue.Find(this._parameters, parameter);
      if (string.IsNullOrEmpty(value))
      {
        if (valueHeaderValue == null)
          return;
        this._parameters.Remove(valueHeaderValue);
      }
      else
      {
        string empty = string.Empty;
        string str = !parameter.EndsWith("*", StringComparison.Ordinal) ? ContentDispositionHeaderValue.EncodeAndQuoteMime(value) : HeaderUtilities.Encode5987(value);
        if (valueHeaderValue != null)
          valueHeaderValue.Value = str;
        else
          this.Parameters.Add(new NameValueHeaderValue(parameter, str));
      }
    }

    private static string EncodeAndQuoteMime(string input)
    {
      string input1 = input;
      bool flag = false;
      if (ContentDispositionHeaderValue.IsQuoted((ReadOnlySpan<char>) input1))
      {
        input1 = input1.Substring(1, input1.Length - 2);
        flag = true;
      }
      if (input1.Contains('"'))
        throw new ArgumentException(SR.Format((IFormatProvider) CultureInfo.InvariantCulture, SR.net_http_headers_invalid_value, (object) input));
      if (HeaderUtilities.ContainsNonAscii(input1))
      {
        flag = true;
        input1 = ContentDispositionHeaderValue.EncodeMime(input1);
      }
      else if (!flag && HttpRuleParser.GetTokenLength(input1, 0) != input1.Length)
        flag = true;
      if (flag)
        input1 = "\"" + input1 + "\"";
      return input1;
    }

    private static bool IsQuoted(ReadOnlySpan<char> value)
    {
      return value.Length > 1 && value[0] == '"' && value[value.Length - 1] == '"';
    }

    private static string EncodeMime(string input)
    {
      return "=?utf-8?B?" + Convert.ToBase64String(Encoding.UTF8.GetBytes(input)) + "?=";
    }

    private static bool TryDecodeMime(string input, out string output)
    {
      output = (string) null;
      string str = input;
      if (!ContentDispositionHeaderValue.IsQuoted((ReadOnlySpan<char>) str) || str.Length < 10)
        return false;
      string[] strArray = str.Split('?', StringSplitOptions.None);
      if (strArray.Length == 5 && !(strArray[0] != "\"=") && !(strArray[4] != "=\""))
      {
        if (!(strArray[2].ToLowerInvariant() != "b"))
        {
          try
          {
            Encoding encoding = Encoding.GetEncoding(strArray[1]);
            byte[] bytes = Convert.FromBase64String(strArray[3]);
            output = encoding.GetString(bytes, 0, bytes.Length);
            return true;
          }
          catch (ArgumentException ex)
          {
          }
          catch (FormatException ex)
          {
          }
          return false;
        }
      }
      return false;
    }

    private static bool TryDecode5987(string input, out string output)
    {
      output = (string) null;
      int length = input.IndexOf('\'');
      if (length == -1)
        return false;
      int num = input.LastIndexOf('\'');
      if (length == num || input.IndexOf('\'', length + 1) != num)
        return false;
      string name = input.Substring(0, length);
      string pattern = input.Substring(num + 1, input.Length - (num + 1));
      StringBuilder stringBuilder = new StringBuilder();
      try
      {
        Encoding encoding = Encoding.GetEncoding(name);
        byte[] bytes = new byte[pattern.Length];
        int count = 0;
        for (int index = 0; index < pattern.Length; ++index)
        {
          if (Uri.IsHexEncoding(pattern, index))
          {
            bytes[count++] = (byte) Uri.HexUnescape(pattern, ref index);
            --index;
          }
          else
          {
            if (count > 0)
            {
              stringBuilder.Append(encoding.GetString(bytes, 0, count));
              count = 0;
            }
            stringBuilder.Append(pattern[index]);
          }
        }
        if (count > 0)
          stringBuilder.Append(encoding.GetString(bytes, 0, count));
      }
      catch (ArgumentException ex)
      {
        return false;
      }
      output = stringBuilder.ToString();
      return true;
    }
  }
}

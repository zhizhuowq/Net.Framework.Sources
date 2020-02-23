// Decompiled with JetBrains decompiler
// Type: System.Net.Http.Headers.MediaTypeHeaderValue
// Assembly: System.Net.Http, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: D8D12B97-8555-48F6-96B3-BBC6BF89FCBA
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Net.Http.dll

using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System.Net.Http.Headers
{
  /// <summary>表示使用 Content-Type 标头的在 RFC 2616 中定义的媒体类型。</summary>
  public class MediaTypeHeaderValue : ICloneable
  {
    private ObjectCollection<NameValueHeaderValue> _parameters;
    private string _mediaType;

    /// <summary>获取或设置字符集。</summary>
    /// <returns>字符集。</returns>
    public string CharSet
    {
      get
      {
        return NameValueHeaderValue.Find(this._parameters, "charset")?.Value;
      }
      set
      {
        NameValueHeaderValue valueHeaderValue = NameValueHeaderValue.Find(this._parameters, "charset");
        if (string.IsNullOrEmpty(value))
        {
          if (valueHeaderValue == null)
            return;
          this._parameters.Remove(valueHeaderValue);
        }
        else if (valueHeaderValue != null)
          valueHeaderValue.Value = value;
        else
          this.Parameters.Add(new NameValueHeaderValue("charset", value));
      }
    }

    /// <summary>获取或设置媒体类型标头值参数。</summary>
    /// <returns>媒体类型标头值参数。</returns>
    public ICollection<NameValueHeaderValue> Parameters
    {
      get
      {
        if (this._parameters == null)
          this._parameters = new ObjectCollection<NameValueHeaderValue>();
        return (ICollection<NameValueHeaderValue>) this._parameters;
      }
    }

    /// <summary>获取或设置媒体类型标头值。</summary>
    /// <returns>媒体类型标头值参数。</returns>
    public string MediaType
    {
      get
      {
        return this._mediaType;
      }
      set
      {
        MediaTypeHeaderValue.CheckMediaTypeFormat(value, nameof (value));
        this._mediaType = value;
      }
    }

    internal MediaTypeHeaderValue()
    {
    }

    /// <summary>初始化 <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> 类的新实例。</summary>
    /// <param name="source">用于初始化新实例的 <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> 对象。</param>
    protected MediaTypeHeaderValue(MediaTypeHeaderValue source)
    {
      this._mediaType = source._mediaType;
      if (source._parameters == null)
        return;
      foreach (ICloneable parameter in source._parameters)
        this.Parameters.Add((NameValueHeaderValue) parameter.Clone());
    }

    /// <summary>初始化 <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> 类的新实例。</summary>
    /// <param name="mediaType">一个以用于初始化新实例的字符串的形式表示的源。</param>
    public MediaTypeHeaderValue(string mediaType)
    {
      MediaTypeHeaderValue.CheckMediaTypeFormat(mediaType, nameof (mediaType));
      this._mediaType = mediaType;
    }

    /// <summary>返回表示当前 <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> 对象的字符串。</summary>
    /// <returns>表示当前对象的字符串。</returns>
    public override string ToString()
    {
      StringBuilder stringBuilder = StringBuilderCache.Acquire(16);
      stringBuilder.Append(this._mediaType);
      NameValueHeaderValue.ToString(this._parameters, ';', true, stringBuilder);
      return StringBuilderCache.GetStringAndRelease(stringBuilder);
    }

    /// <summary>确定指定的 <see cref="T:System.Object" /> 是否等于当前的 <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> 对象。</summary>
    /// <param name="obj">要与当前对象进行比较的对象。</param>
    /// <returns>如果指定的 <see cref="T:System.Object" /> 等于当前的对象，则为 <see langword="true" />；否则为 <see langword="false" />。</returns>
    public override bool Equals(object obj)
    {
      return obj is MediaTypeHeaderValue mediaTypeHeaderValue && string.Equals(this._mediaType, mediaTypeHeaderValue._mediaType, StringComparison.OrdinalIgnoreCase) && HeaderUtilities.AreEqualCollections<NameValueHeaderValue>(this._parameters, mediaTypeHeaderValue._parameters);
    }

    /// <summary>充当 <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> 对象的哈希函数。</summary>
    /// <returns>当前对象的哈希代码。</returns>
    public override int GetHashCode()
    {
      return StringComparer.OrdinalIgnoreCase.GetHashCode(this._mediaType) ^ NameValueHeaderValue.GetHashCode(this._parameters);
    }

    /// <summary>将字符串转换为 <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> 实例。</summary>
    /// <param name="input">表示媒体类型标头值信息的字符串。</param>
    /// <returns>一个 <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> 实例。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="input" /> 是一个 <see langword="null" /> 引用。</exception>
    /// <exception cref="T:System.FormatException">
    /// <paramref name="input" /> 为无效媒体类型标头值信息。</exception>
    public static MediaTypeHeaderValue Parse(string input)
    {
      int index = 0;
      return (MediaTypeHeaderValue) MediaTypeHeaderParser.SingleValueParser.ParseValue(input, (object) null, ref index);
    }

    /// <summary>确定一个字符串是否为有效的 <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> 信息。</summary>
    /// <param name="input">要验证的字符串。</param>
    /// <param name="parsedValue">字符串的 <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> 版本。</param>
    /// <returns>如果 <paramref name="input" /> 为有效 <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> 信息，则为 <see langword="true" />；否则为 <see langword="false" />。</returns>
    public static bool TryParse(string input, out MediaTypeHeaderValue parsedValue)
    {
      int index = 0;
      parsedValue = (MediaTypeHeaderValue) null;
      object parsedValue1;
      if (!MediaTypeHeaderParser.SingleValueParser.TryParseValue(input, (object) null, ref index, out parsedValue1))
        return false;
      parsedValue = (MediaTypeHeaderValue) parsedValue1;
      return true;
    }

    internal static int GetMediaTypeLength(
      string input,
      int startIndex,
      Func<MediaTypeHeaderValue> mediaTypeCreator,
      out MediaTypeHeaderValue parsedValue)
    {
      parsedValue = (MediaTypeHeaderValue) null;
      if (string.IsNullOrEmpty(input) || startIndex >= input.Length)
        return 0;
      string mediaType = (string) null;
      int expressionLength = MediaTypeHeaderValue.GetMediaTypeExpressionLength(input, startIndex, out mediaType);
      if (expressionLength == 0)
        return 0;
      int startIndex1 = startIndex + expressionLength;
      int index = startIndex1 + HttpRuleParser.GetWhitespaceLength(input, startIndex1);
      if (index < input.Length && input[index] == ';')
      {
        MediaTypeHeaderValue mediaTypeHeaderValue = mediaTypeCreator();
        mediaTypeHeaderValue._mediaType = mediaType;
        int startIndex2 = index + 1;
        int nameValueListLength = NameValueHeaderValue.GetNameValueListLength(input, startIndex2, ';', (ObjectCollection<NameValueHeaderValue>) mediaTypeHeaderValue.Parameters);
        if (nameValueListLength == 0)
          return 0;
        parsedValue = mediaTypeHeaderValue;
        return startIndex2 + nameValueListLength - startIndex;
      }
      MediaTypeHeaderValue mediaTypeHeaderValue1 = mediaTypeCreator();
      mediaTypeHeaderValue1._mediaType = mediaType;
      parsedValue = mediaTypeHeaderValue1;
      return index - startIndex;
    }

    private static int GetMediaTypeExpressionLength(
      string input,
      int startIndex,
      out string mediaType)
    {
      mediaType = (string) null;
      int tokenLength1 = HttpRuleParser.GetTokenLength(input, startIndex);
      if (tokenLength1 == 0)
        return 0;
      int startIndex1 = startIndex + tokenLength1;
      int index = startIndex1 + HttpRuleParser.GetWhitespaceLength(input, startIndex1);
      if (index >= input.Length || input[index] != '/')
        return 0;
      int startIndex2 = index + 1;
      int num = startIndex2 + HttpRuleParser.GetWhitespaceLength(input, startIndex2);
      int tokenLength2 = HttpRuleParser.GetTokenLength(input, num);
      if (tokenLength2 == 0)
        return 0;
      int length = num + tokenLength2 - startIndex;
      mediaType = tokenLength1 + tokenLength2 + 1 != length ? input.AsSpan(startIndex, tokenLength1).ToString() + (ReadOnlySpan<char>) "/" + input.AsSpan(num, tokenLength2) : input.Substring(startIndex, length);
      return length;
    }

    private static void CheckMediaTypeFormat(string mediaType, string parameterName)
    {
      if (string.IsNullOrEmpty(mediaType))
        throw new ArgumentException(SR.net_http_argument_empty_string, parameterName);
      string mediaType1;
      if (MediaTypeHeaderValue.GetMediaTypeExpressionLength(mediaType, 0, out mediaType1) == 0 || mediaType1.Length != mediaType.Length)
        throw new FormatException(SR.Format((IFormatProvider) CultureInfo.InvariantCulture, SR.net_http_headers_invalid_value, (object) mediaType));
    }

    object ICloneable.Clone()
    {
      return (object) new MediaTypeHeaderValue(this);
    }
  }
}

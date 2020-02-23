// Decompiled with JetBrains decompiler
// Type: System.Net.Http.Headers.HeaderUtilities
// Assembly: System.Net.Http, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: D8D12B97-8555-48F6-96B3-BBC6BF89FCBA
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Net.Http.dll

using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;
using System.Text;

namespace System.Net.Http.Headers
{
  internal static class HeaderUtilities
  {
    internal static readonly TransferCodingHeaderValue TransferEncodingChunked = new TransferCodingHeaderValue("chunked");
    internal static readonly NameValueWithParametersHeaderValue ExpectContinue = new NameValueWithParametersHeaderValue("100-continue");
    internal static readonly Action<HttpHeaderValueCollection<string>, string> TokenValidator = new Action<HttpHeaderValueCollection<string>, string>(HeaderUtilities.ValidateToken);
    private static readonly char[] s_hexUpperChars = new char[16]
    {
      '0',
      '1',
      '2',
      '3',
      '4',
      '5',
      '6',
      '7',
      '8',
      '9',
      'A',
      'B',
      'C',
      'D',
      'E',
      'F'
    };

    internal static void SetQuality(
      ObjectCollection<NameValueHeaderValue> parameters,
      double? value)
    {
      NameValueHeaderValue valueHeaderValue = NameValueHeaderValue.Find(parameters, "q");
      if (value.HasValue)
      {
        double? nullable = value;
        double num1 = 0.0;
        if (!(nullable.GetValueOrDefault() < num1 & nullable.HasValue))
        {
          nullable = value;
          double num2 = 1.0;
          if (!(nullable.GetValueOrDefault() > num2 & nullable.HasValue))
          {
            string str = value.Value.ToString("0.0##", (IFormatProvider) NumberFormatInfo.InvariantInfo);
            if (valueHeaderValue != null)
            {
              valueHeaderValue.Value = str;
              return;
            }
            parameters.Add(new NameValueHeaderValue("q", str));
            return;
          }
        }
        throw new ArgumentOutOfRangeException(nameof (value));
      }
      if (valueHeaderValue == null)
        return;
      parameters.Remove(valueHeaderValue);
    }

    internal static bool ContainsNonAscii(string input)
    {
      foreach (char ch in input)
      {
        if (ch > '\x007F')
          return true;
      }
      return false;
    }

    internal static string Encode5987(string input)
    {
      StringBuilder stringBuilder = StringBuilderCache.Acquire(16);
      byte[] numArray = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(input.Length));
      int bytes = Encoding.UTF8.GetBytes(input, 0, input.Length, numArray, 0);
      stringBuilder.Append("utf-8''");
      for (int index = 0; index < bytes; ++index)
      {
        byte c = numArray[index];
        if (c > (byte) 127)
          HeaderUtilities.AddHexEscaped(c, stringBuilder);
        else if (!HttpRuleParser.IsTokenChar((char) c) || c == (byte) 42 || (c == (byte) 39 || c == (byte) 37))
          HeaderUtilities.AddHexEscaped(c, stringBuilder);
        else
          stringBuilder.Append((char) c);
      }
      Array.Clear((Array) numArray, 0, bytes);
      ArrayPool<byte>.Shared.Return(numArray, false);
      return StringBuilderCache.GetStringAndRelease(stringBuilder);
    }

    private static void AddHexEscaped(byte c, StringBuilder destination)
    {
      destination.Append('%');
      destination.Append(HeaderUtilities.s_hexUpperChars[((int) c & 240) >> 4]);
      destination.Append(HeaderUtilities.s_hexUpperChars[(int) c & 15]);
    }

    internal static double? GetQuality(ObjectCollection<NameValueHeaderValue> parameters)
    {
      NameValueHeaderValue valueHeaderValue = NameValueHeaderValue.Find(parameters, "q");
      if (valueHeaderValue != null)
      {
        double result = 0.0;
        if (double.TryParse(valueHeaderValue.Value, NumberStyles.AllowDecimalPoint, (IFormatProvider) NumberFormatInfo.InvariantInfo, out result))
          return new double?(result);
        if (NetEventSource.IsEnabled)
          NetEventSource.Error((object) null, (object) SR.Format(SR.net_http_log_headers_invalid_quality, (object) valueHeaderValue.Value), nameof (GetQuality));
      }
      return new double?();
    }

    internal static void CheckValidToken(string value, string parameterName)
    {
      if (string.IsNullOrEmpty(value))
        throw new ArgumentException(SR.net_http_argument_empty_string, parameterName);
      if (HttpRuleParser.GetTokenLength(value, 0) != value.Length)
        throw new FormatException(SR.Format((IFormatProvider) CultureInfo.InvariantCulture, SR.net_http_headers_invalid_value, (object) value));
    }

    internal static void CheckValidComment(string value, string parameterName)
    {
      if (string.IsNullOrEmpty(value))
        throw new ArgumentException(SR.net_http_argument_empty_string, parameterName);
      int length = 0;
      if (HttpRuleParser.GetCommentLength(value, 0, out length) != HttpParseResult.Parsed || length != value.Length)
        throw new FormatException(SR.Format((IFormatProvider) CultureInfo.InvariantCulture, SR.net_http_headers_invalid_value, (object) value));
    }

    internal static void CheckValidQuotedString(string value, string parameterName)
    {
      if (string.IsNullOrEmpty(value))
        throw new ArgumentException(SR.net_http_argument_empty_string, parameterName);
      int length = 0;
      if (HttpRuleParser.GetQuotedStringLength(value, 0, out length) != HttpParseResult.Parsed || length != value.Length)
        throw new FormatException(SR.Format((IFormatProvider) CultureInfo.InvariantCulture, SR.net_http_headers_invalid_value, (object) value));
    }

    internal static bool AreEqualCollections<T>(ObjectCollection<T> x, ObjectCollection<T> y) where T : class
    {
      return HeaderUtilities.AreEqualCollections<T>(x, y, (IEqualityComparer<T>) null);
    }

    internal static bool AreEqualCollections<T>(
      ObjectCollection<T> x,
      ObjectCollection<T> y,
      IEqualityComparer<T> comparer)
      where T : class
    {
      if (x == null)
        return y == null || y.Count == 0;
      if (y == null)
        return x.Count == 0;
      if (x.Count != y.Count)
        return false;
      if (x.Count == 0)
        return true;
      bool[] flagArray = new bool[x.Count];
      foreach (T x1 in x)
      {
        int index = 0;
        bool flag = false;
        foreach (T y1 in y)
        {
          if (!flagArray[index] && (comparer == null && x1.Equals((object) y1) || comparer != null && comparer.Equals(x1, y1)))
          {
            flagArray[index] = true;
            flag = true;
            break;
          }
          ++index;
        }
        if (!flag)
          return false;
      }
      return true;
    }

    internal static int GetNextNonEmptyOrWhitespaceIndex(
      string input,
      int startIndex,
      bool skipEmptyValues,
      out bool separatorFound)
    {
      separatorFound = false;
      int index1 = startIndex + HttpRuleParser.GetWhitespaceLength(input, startIndex);
      if (index1 == input.Length || input[index1] != ',')
        return index1;
      separatorFound = true;
      int startIndex1 = index1 + 1;
      int index2 = startIndex1 + HttpRuleParser.GetWhitespaceLength(input, startIndex1);
      int startIndex2;
      if (skipEmptyValues)
      {
        for (; index2 < input.Length && input[index2] == ','; index2 = startIndex2 + HttpRuleParser.GetWhitespaceLength(input, startIndex2))
          startIndex2 = index2 + 1;
      }
      return index2;
    }

    internal static DateTimeOffset? GetDateTimeOffsetValue(
      HeaderDescriptor descriptor,
      HttpHeaders store,
      DateTimeOffset? defaultValue = null)
    {
      object parsedValues = store.GetParsedValues(descriptor);
      if (parsedValues != null)
        return new DateTimeOffset?((DateTimeOffset) parsedValues);
      return defaultValue.HasValue && store.Contains(descriptor) ? defaultValue : new DateTimeOffset?();
    }

    internal static TimeSpan? GetTimeSpanValue(
      HeaderDescriptor descriptor,
      HttpHeaders store)
    {
      object parsedValues = store.GetParsedValues(descriptor);
      return parsedValues != null ? new TimeSpan?((TimeSpan) parsedValues) : new TimeSpan?();
    }

    internal static bool TryParseInt32(string value, out int result)
    {
      return HeaderUtilities.TryParseInt32(value, 0, value.Length, out result);
    }

    internal static bool TryParseInt32(string value, int offset, int length, out int result)
    {
      if (offset < 0 || length < 0 || offset > value.Length - length)
      {
        result = 0;
        return false;
      }
      int num1 = 0;
      int num2 = offset;
      int num3 = offset + length;
      while (num2 < num3)
      {
        int num4 = (int) value[num2++] - 48;
        switch (num4)
        {
          case 0:
          case 1:
          case 2:
          case 3:
          case 4:
          case 5:
          case 6:
          case 7:
          case 8:
          case 9:
            if (num1 <= 214748364 && (num1 != 214748364 || num4 <= 7))
            {
              num1 = num1 * 10 + num4;
              continue;
            }
            break;
        }
        result = 0;
        return false;
      }
      result = num1;
      return true;
    }

    internal static bool TryParseInt64(string value, int offset, int length, out long result)
    {
      if (offset < 0 || length < 0 || offset > value.Length - length)
      {
        result = 0L;
        return false;
      }
      long num1 = 0;
      int num2 = offset;
      int num3 = offset + length;
      while (num2 < num3)
      {
        int num4 = (int) value[num2++] - 48;
        switch (num4)
        {
          case 0:
          case 1:
          case 2:
          case 3:
          case 4:
          case 5:
          case 6:
          case 7:
          case 8:
          case 9:
            if (num1 <= 922337203685477580L && (num1 != 922337203685477580L || num4 <= 7))
            {
              num1 = num1 * 10L + (long) num4;
              continue;
            }
            break;
        }
        result = 0L;
        return false;
      }
      result = num1;
      return true;
    }

    internal static void DumpHeaders(StringBuilder sb, params HttpHeaders[] headers)
    {
      sb.Append("{\r\n");
      for (int index = 0; index < headers.Length; ++index)
      {
        if (headers[index] != null)
        {
          foreach (KeyValuePair<string, IEnumerable<string>> keyValuePair in headers[index])
          {
            foreach (string str in keyValuePair.Value)
            {
              sb.Append("  ");
              sb.Append(keyValuePair.Key);
              sb.Append(": ");
              sb.Append(str);
              sb.Append("\r\n");
            }
          }
        }
      }
      sb.Append('}');
    }

    internal static bool IsValidEmailAddress(string value)
    {
      try
      {
        MailAddressParser.ParseAddress(value);
        return true;
      }
      catch (FormatException ex)
      {
        if (NetEventSource.IsEnabled)
          NetEventSource.Error((object) null, (object) SR.Format(SR.net_http_log_headers_wrong_email_format, (object) value, (object) ex.Message), nameof (IsValidEmailAddress));
      }
      return false;
    }

    private static void ValidateToken(HttpHeaderValueCollection<string> collection, string value)
    {
      HeaderUtilities.CheckValidToken(value, "item");
    }
  }
}

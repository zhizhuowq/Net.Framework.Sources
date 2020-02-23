// Decompiled with JetBrains decompiler
// Type: System.Net.Http.HttpRuleParser
// Assembly: System.Net.Http, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: D8D12B97-8555-48F6-96B3-BBC6BF89FCBA
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Net.Http.dll

using System.Text;

namespace System.Net.Http
{
  internal static class HttpRuleParser
  {
    private static readonly bool[] s_tokenChars = HttpRuleParser.CreateTokenChars();
    internal static readonly Encoding DefaultHttpEncoding = Encoding.GetEncoding(28591);

    private static bool[] CreateTokenChars()
    {
      bool[] flagArray = new bool[128];
      for (int index = 33; index < (int) sbyte.MaxValue; ++index)
        flagArray[index] = true;
      flagArray[40] = false;
      flagArray[41] = false;
      flagArray[60] = false;
      flagArray[62] = false;
      flagArray[64] = false;
      flagArray[44] = false;
      flagArray[59] = false;
      flagArray[58] = false;
      flagArray[92] = false;
      flagArray[34] = false;
      flagArray[47] = false;
      flagArray[91] = false;
      flagArray[93] = false;
      flagArray[63] = false;
      flagArray[61] = false;
      flagArray[123] = false;
      flagArray[125] = false;
      return flagArray;
    }

    internal static bool IsTokenChar(char character)
    {
      return character <= '\x007F' && HttpRuleParser.s_tokenChars[(int) character];
    }

    internal static int GetTokenLength(string input, int startIndex)
    {
      if (startIndex >= input.Length)
        return 0;
      for (int index = startIndex; index < input.Length; ++index)
      {
        if (!HttpRuleParser.IsTokenChar(input[index]))
          return index - startIndex;
      }
      return input.Length - startIndex;
    }

    internal static bool IsToken(string input)
    {
      for (int index = 0; index < input.Length; ++index)
      {
        if (!HttpRuleParser.IsTokenChar(input[index]))
          return false;
      }
      return true;
    }

    internal static bool IsToken(ReadOnlySpan<byte> input)
    {
      for (int index = 0; index < input.Length; ++index)
      {
        if (!HttpRuleParser.IsTokenChar((char) input[index]))
          return false;
      }
      return true;
    }

    internal static string GetTokenString(ReadOnlySpan<byte> input)
    {
      return Encoding.ASCII.GetString(input);
    }

    internal static int GetWhitespaceLength(string input, int startIndex)
    {
      if (startIndex >= input.Length)
        return 0;
      int index = startIndex;
      while (index < input.Length)
      {
        switch (input[index])
        {
          case '\t':
          case ' ':
            ++index;
            continue;
          case '\r':
            if (index + 2 < input.Length && input[index + 1] == '\n')
            {
              switch (input[index + 2])
              {
                case '\t':
                case ' ':
                  index += 3;
                  continue;
              }
            }
            else
              break;
            break;
        }
        return index - startIndex;
      }
      return input.Length - startIndex;
    }

    internal static bool ContainsInvalidNewLine(string value)
    {
      return HttpRuleParser.ContainsInvalidNewLine(value, 0);
    }

    internal static bool ContainsInvalidNewLine(string value, int startIndex)
    {
      for (int index1 = startIndex; index1 < value.Length; ++index1)
      {
        if (value[index1] == '\r')
        {
          int index2 = index1 + 1;
          if (index2 < value.Length && value[index2] == '\n')
          {
            index1 = index2 + 1;
            if (index1 == value.Length)
              return true;
            switch (value[index1])
            {
              case '\t':
              case ' ':
                continue;
              default:
                return true;
            }
          }
        }
      }
      return false;
    }

    internal static int GetNumberLength(string input, int startIndex, bool allowDecimal)
    {
      int index = startIndex;
      bool flag = !allowDecimal;
      if (input[index] == '.')
        return 0;
      while (index < input.Length)
      {
        char ch = input[index];
        switch (ch)
        {
          case '0':
          case '1':
          case '2':
          case '3':
          case '4':
          case '5':
          case '6':
          case '7':
          case '8':
          case '9':
            ++index;
            continue;
          default:
            if (!flag && ch == '.')
            {
              flag = true;
              ++index;
              continue;
            }
            goto label_7;
        }
      }
label_7:
      return index - startIndex;
    }

    internal static int GetHostLength(
      string input,
      int startIndex,
      bool allowToken,
      out string host)
    {
      host = (string) null;
      if (startIndex >= input.Length)
        return 0;
      int index = startIndex;
      bool flag = true;
      for (; index < input.Length; ++index)
      {
        char character = input[index];
        switch (character)
        {
          case '\t':
          case '\r':
          case ' ':
          case ',':
            goto label_7;
          case '/':
            return 0;
          default:
            flag = flag && HttpRuleParser.IsTokenChar(character);
            continue;
        }
      }
label_7:
      int length = index - startIndex;
      if (length == 0)
        return 0;
      string host1 = input.Substring(startIndex, length);
      if ((!allowToken || !flag) && !HttpRuleParser.IsValidHostName(host1))
        return 0;
      host = host1;
      return length;
    }

    internal static HttpParseResult GetCommentLength(
      string input,
      int startIndex,
      out int length)
    {
      return HttpRuleParser.GetExpressionLength(input, startIndex, '(', ')', true, 1, out length);
    }

    internal static HttpParseResult GetQuotedStringLength(
      string input,
      int startIndex,
      out int length)
    {
      return HttpRuleParser.GetExpressionLength(input, startIndex, '"', '"', false, 1, out length);
    }

    internal static HttpParseResult GetQuotedPairLength(
      string input,
      int startIndex,
      out int length)
    {
      length = 0;
      if (input[startIndex] != '\\')
        return HttpParseResult.NotParsed;
      if (startIndex + 2 > input.Length || input[startIndex + 1] > '\x007F')
        return HttpParseResult.InvalidFormat;
      length = 2;
      return HttpParseResult.Parsed;
    }

    private static HttpParseResult GetExpressionLength(
      string input,
      int startIndex,
      char openChar,
      char closeChar,
      bool supportsNesting,
      int nestedCount,
      out int length)
    {
      length = 0;
      if ((int) input[startIndex] != (int) openChar)
        return HttpParseResult.NotParsed;
      int startIndex1 = startIndex + 1;
      while (startIndex1 < input.Length)
      {
        int length1 = 0;
        if (startIndex1 + 2 < input.Length && HttpRuleParser.GetQuotedPairLength(input, startIndex1, out length1) == HttpParseResult.Parsed)
          startIndex1 += length1;
        else if (supportsNesting && (int) input[startIndex1] == (int) openChar)
        {
          if (nestedCount > 5)
            return HttpParseResult.InvalidFormat;
          int length2 = 0;
          switch (HttpRuleParser.GetExpressionLength(input, startIndex1, openChar, closeChar, supportsNesting, nestedCount + 1, out length2))
          {
            case HttpParseResult.Parsed:
              startIndex1 += length2;
              continue;
            case HttpParseResult.InvalidFormat:
              return HttpParseResult.InvalidFormat;
            default:
              continue;
          }
        }
        else
        {
          if ((int) input[startIndex1] == (int) closeChar)
          {
            length = startIndex1 - startIndex + 1;
            return HttpParseResult.Parsed;
          }
          ++startIndex1;
        }
      }
      return HttpParseResult.InvalidFormat;
    }

    private static bool IsValidHostName(string host)
    {
      return Uri.TryCreate("http://u@" + host + "/", UriKind.Absolute, out Uri _);
    }
  }
}

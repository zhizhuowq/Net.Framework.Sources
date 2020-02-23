// Decompiled with JetBrains decompiler
// Type: System.Globalization.CharUnicodeInfo
// Assembly: System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// MVID: BBB3B884-123D-47EA-9CD1-5BED540D02AE
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Private.CoreLib.dll

using Internal.Runtime.CompilerServices;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Globalization
{
  public static class CharUnicodeInfo
  {
    private static unsafe ReadOnlySpan<byte> CategoryLevel1Index
    {
      get
      {
        return new ReadOnlySpan<byte>((void*) &\u003CPrivateImplementationDetails\u003E.E042E6886B3C5817FAF534605ACC9AAACDBC373E, 2176);
      }
    }

    private static unsafe ReadOnlySpan<byte> CategoryLevel2Index
    {
      get
      {
        return new ReadOnlySpan<byte>((void*) &\u003CPrivateImplementationDetails\u003E.\u0037802354AE39660744010DD869AC44C40E9E989E7, 5952);
      }
    }

    private static unsafe ReadOnlySpan<byte> CategoryLevel3Index
    {
      get
      {
        return new ReadOnlySpan<byte>((void*) &\u003CPrivateImplementationDetails\u003E.\u00387052999E324B8D11FA5A435BB9B59EF4E19BC83, 10800);
      }
    }

    private static unsafe ReadOnlySpan<byte> CategoriesValue
    {
      get
      {
        return new ReadOnlySpan<byte>((void*) &\u003CPrivateImplementationDetails\u003E.B3D1E79ACDC52D7C70FC92660E1FE31E49285232, 172);
      }
    }

    private static unsafe ReadOnlySpan<byte> NumericLevel1Index
    {
      get
      {
        return new ReadOnlySpan<byte>((void*) &\u003CPrivateImplementationDetails\u003E.BB38C8E230B223122B51508586D4018AE1A49311, 761);
      }
    }

    private static unsafe ReadOnlySpan<byte> NumericLevel2Index
    {
      get
      {
        return new ReadOnlySpan<byte>((void*) &\u003CPrivateImplementationDetails\u003E.E67653B760B07938A045A7DBE30FE416061D563A, 1024);
      }
    }

    private static unsafe ReadOnlySpan<byte> NumericLevel3Index
    {
      get
      {
        return new ReadOnlySpan<byte>((void*) &\u003CPrivateImplementationDetails\u003E.F41FAABC030686D84E39728DDC265124723CAD47, 1824);
      }
    }

    private static unsafe ReadOnlySpan<byte> NumericValues
    {
      get
      {
        return new ReadOnlySpan<byte>((void*) &\u003CPrivateImplementationDetails\u003E.D787EC00ED57A318EDA2E15AD878126370ABF81A, 1320);
      }
    }

    private static unsafe ReadOnlySpan<byte> DigitValues
    {
      get
      {
        return new ReadOnlySpan<byte>((void*) &\u003CPrivateImplementationDetails\u003E.FC56FFAD5DF0B682247A84CBE400CA5AD2E43423, 330);
      }
    }

    internal static int InternalConvertToUtf32(string s, int index)
    {
      if (index < s.Length - 1)
      {
        int num1 = (int) s[index] - 55296;
        if ((uint) num1 <= 1023U)
        {
          int num2 = (int) s[index + 1] - 56320;
          if ((uint) num2 <= 1023U)
            return num1 * 1024 + num2 + 65536;
        }
      }
      return (int) s[index];
    }

    internal static int InternalConvertToUtf32(StringBuilder s, int index)
    {
      int num1 = (int) s[index];
      if (index < s.Length - 1)
      {
        int num2 = num1 - 55296;
        if ((uint) num2 <= 1023U)
        {
          int num3 = (int) s[index + 1] - 56320;
          if ((uint) num3 <= 1023U)
            return num2 * 1024 + num3 + 65536;
        }
      }
      return num1;
    }

    internal static int InternalConvertToUtf32(string s, int index, out int charLength)
    {
      charLength = 1;
      if (index < s.Length - 1)
      {
        int num1 = (int) s[index] - 55296;
        if ((uint) num1 <= 1023U)
        {
          int num2 = (int) s[index + 1] - 56320;
          if ((uint) num2 <= 1023U)
          {
            ++charLength;
            return num1 * 1024 + num2 + 65536;
          }
        }
      }
      return (int) s[index];
    }

    internal static double InternalGetNumericValue(int ch)
    {
      int index = ch >> 8;
      if ((uint) index >= (uint) CharUnicodeInfo.NumericLevel1Index.Length)
        return -1.0;
      int num1 = (int) CharUnicodeInfo.NumericLevel1Index[index];
      ReadOnlySpan<byte> readOnlySpan = CharUnicodeInfo.NumericLevel2Index;
      int num2 = (int) readOnlySpan[(num1 << 4) + (ch >> 4 & 15)];
      readOnlySpan = CharUnicodeInfo.NumericLevel3Index;
      int num3 = (int) readOnlySpan[(num2 << 4) + (ch & 15)];
      readOnlySpan = CharUnicodeInfo.NumericValues;
      ref byte local = ref Unsafe.AsRef<byte>(in readOnlySpan[num3 * 8]);
      return BitConverter.IsLittleEndian ? Unsafe.ReadUnaligned<double>(ref local) : BitConverter.Int64BitsToDouble(BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<long>(ref local)));
    }

    internal static byte InternalGetDigitValues(int ch, int offset)
    {
      int index = ch >> 8;
      if ((uint) index >= (uint) CharUnicodeInfo.NumericLevel1Index.Length)
        return byte.MaxValue;
      int num1 = (int) CharUnicodeInfo.NumericLevel1Index[index];
      ReadOnlySpan<byte> readOnlySpan = CharUnicodeInfo.NumericLevel2Index;
      int num2 = (int) readOnlySpan[(num1 << 4) + (ch >> 4 & 15)];
      readOnlySpan = CharUnicodeInfo.NumericLevel3Index;
      int num3 = (int) readOnlySpan[(num2 << 4) + (ch & 15)];
      readOnlySpan = CharUnicodeInfo.DigitValues;
      return readOnlySpan[num3 * 2 + offset];
    }

    public static double GetNumericValue(char ch)
    {
      return CharUnicodeInfo.InternalGetNumericValue((int) ch);
    }

    [NullableContext(1)]
    public static double GetNumericValue(string s, int index)
    {
      if (s == null)
        throw new ArgumentNullException(nameof (s));
      if (index < 0 || index >= s.Length)
        throw new ArgumentOutOfRangeException(nameof (index), SR.ArgumentOutOfRange_Index);
      return CharUnicodeInfo.InternalGetNumericValue(CharUnicodeInfo.InternalConvertToUtf32(s, index));
    }

    public static int GetDecimalDigitValue(char ch)
    {
      return (int) (sbyte) CharUnicodeInfo.InternalGetDigitValues((int) ch, 0);
    }

    [NullableContext(1)]
    public static int GetDecimalDigitValue(string s, int index)
    {
      if (s == null)
        throw new ArgumentNullException(nameof (s));
      if (index < 0 || index >= s.Length)
        throw new ArgumentOutOfRangeException(nameof (index), SR.ArgumentOutOfRange_Index);
      return (int) (sbyte) CharUnicodeInfo.InternalGetDigitValues(CharUnicodeInfo.InternalConvertToUtf32(s, index), 0);
    }

    public static int GetDigitValue(char ch)
    {
      return (int) (sbyte) CharUnicodeInfo.InternalGetDigitValues((int) ch, 1);
    }

    [NullableContext(1)]
    public static int GetDigitValue(string s, int index)
    {
      if (s == null)
        throw new ArgumentNullException(nameof (s));
      if (index < 0 || index >= s.Length)
        throw new ArgumentOutOfRangeException(nameof (index), SR.ArgumentOutOfRange_Index);
      return (int) (sbyte) CharUnicodeInfo.InternalGetDigitValues(CharUnicodeInfo.InternalConvertToUtf32(s, index), 1);
    }

    public static UnicodeCategory GetUnicodeCategory(char ch)
    {
      return CharUnicodeInfo.GetUnicodeCategory((int) ch);
    }

    [NullableContext(1)]
    public static UnicodeCategory GetUnicodeCategory(string s, int index)
    {
      if (s == null)
        throw new ArgumentNullException(nameof (s));
      if ((uint) index >= (uint) s.Length)
        throw new ArgumentOutOfRangeException(nameof (index));
      return CharUnicodeInfo.InternalGetUnicodeCategory(s, index);
    }

    public static UnicodeCategory GetUnicodeCategory(int codePoint)
    {
      return (UnicodeCategory) CharUnicodeInfo.InternalGetCategoryValue(codePoint, 0);
    }

    internal static byte InternalGetCategoryValue(int ch, int offset)
    {
      int num = (int) Unsafe.ReadUnaligned<ushort>(ref Unsafe.AsRef<byte>(in CharUnicodeInfo.CategoryLevel2Index[((int) CharUnicodeInfo.CategoryLevel1Index[ch >> 9] << 6) + (ch >> 3 & 62)]));
      if (!BitConverter.IsLittleEndian)
        num = (int) BinaryPrimitives.ReverseEndianness((ushort) num);
      return CharUnicodeInfo.CategoriesValue[(int) CharUnicodeInfo.CategoryLevel3Index[(num << 4) + (ch & 15)] * 2 + offset];
    }

    internal static UnicodeCategory InternalGetUnicodeCategory(
      string value,
      int index)
    {
      return CharUnicodeInfo.GetUnicodeCategory(CharUnicodeInfo.InternalConvertToUtf32(value, index));
    }

    internal static BidiCategory GetBidiCategory(string s, int index)
    {
      if (s == null)
        throw new ArgumentNullException(nameof (s));
      if ((uint) index >= (uint) s.Length)
        throw new ArgumentOutOfRangeException(nameof (index));
      return (BidiCategory) CharUnicodeInfo.InternalGetCategoryValue(CharUnicodeInfo.InternalConvertToUtf32(s, index), 1);
    }

    internal static BidiCategory GetBidiCategory(StringBuilder s, int index)
    {
      return (BidiCategory) CharUnicodeInfo.InternalGetCategoryValue(CharUnicodeInfo.InternalConvertToUtf32(s, index), 1);
    }

    internal static UnicodeCategory InternalGetUnicodeCategory(
      string str,
      int index,
      out int charLength)
    {
      return CharUnicodeInfo.GetUnicodeCategory(CharUnicodeInfo.InternalConvertToUtf32(str, index, out charLength));
    }

    internal static bool IsCombiningCategory(UnicodeCategory uc)
    {
      return uc == UnicodeCategory.NonSpacingMark || uc == UnicodeCategory.SpacingCombiningMark || uc == UnicodeCategory.EnclosingMark;
    }
  }
}

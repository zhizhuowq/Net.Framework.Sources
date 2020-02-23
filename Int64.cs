// Decompiled with JetBrains decompiler
// Type: System.Int64
// Assembly: System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// MVID: BBB3B884-123D-47EA-9CD1-5BED540D02AE
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Private.CoreLib.dll

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace System
{
  [TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
  [Serializable]
  public readonly struct Int64 : IComparable, IConvertible, IFormattable, IComparable<long>, IEquatable<long>, ISpanFormattable
  {
    private readonly long m_value;
    public const long MaxValue = 9223372036854775807;
    public const long MinValue = -9223372036854775808;

    [NullableContext(2)]
    public int CompareTo(object value)
    {
      if (value == null)
        return 1;
      if (!(value is long num))
        throw new ArgumentException(SR.Arg_MustBeInt64);
      if (this < num)
        return -1;
      return this > num ? 1 : 0;
    }

    public int CompareTo(long value)
    {
      if (this < value)
        return -1;
      return this > value ? 1 : 0;
    }

    [NullableContext(2)]
    public override bool Equals(object obj)
    {
      return obj is long num && this == num;
    }

    [NonVersionable]
    public bool Equals(long obj)
    {
      return this == obj;
    }

    public override int GetHashCode()
    {
      return (int) this ^ (int) (this >> 32);
    }

    [NullableContext(1)]
    public override string ToString()
    {
      return Number.FormatInt64(this, (ReadOnlySpan<char>) (char[]) null, (IFormatProvider) null);
    }

    [NullableContext(1)]
    public string ToString([Nullable(2)] IFormatProvider provider)
    {
      return Number.FormatInt64(this, (ReadOnlySpan<char>) (char[]) null, provider);
    }

    [NullableContext(1)]
    public string ToString([Nullable(2)] string format)
    {
      return Number.FormatInt64(this, (ReadOnlySpan<char>) format, (IFormatProvider) null);
    }

    [NullableContext(2)]
    [return: Nullable(1)]
    public string ToString(string format, IFormatProvider provider)
    {
      return Number.FormatInt64(this, (ReadOnlySpan<char>) format, provider);
    }

    public bool TryFormat(
      Span<char> destination,
      out int charsWritten,
      ReadOnlySpan<char> format = default (ReadOnlySpan<char>),
      [Nullable(2)] IFormatProvider provider = null)
    {
      return Number.TryFormatInt64(this, format, provider, destination, out charsWritten);
    }

    [NullableContext(1)]
    public static long Parse(string s)
    {
      if (s == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
      return Number.ParseInt64((ReadOnlySpan<char>) s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
    }

    [NullableContext(1)]
    public static long Parse(string s, NumberStyles style)
    {
      NumberFormatInfo.ValidateParseStyleInteger(style);
      if (s == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
      return Number.ParseInt64((ReadOnlySpan<char>) s, style, NumberFormatInfo.CurrentInfo);
    }

    [NullableContext(1)]
    public static long Parse(string s, [Nullable(2)] IFormatProvider provider)
    {
      if (s == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
      return Number.ParseInt64((ReadOnlySpan<char>) s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
    }

    [NullableContext(1)]
    public static long Parse(string s, NumberStyles style, [Nullable(2)] IFormatProvider provider)
    {
      NumberFormatInfo.ValidateParseStyleInteger(style);
      if (s == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
      return Number.ParseInt64((ReadOnlySpan<char>) s, style, NumberFormatInfo.GetInstance(provider));
    }

    public static long Parse(ReadOnlySpan<char> s, NumberStyles style = NumberStyles.Integer, [Nullable(2)] IFormatProvider provider = null)
    {
      NumberFormatInfo.ValidateParseStyleInteger(style);
      return Number.ParseInt64(s, style, NumberFormatInfo.GetInstance(provider));
    }

    [NullableContext(2)]
    public static bool TryParse(string s, out long result)
    {
      if (s != null)
        return Number.TryParseInt64IntegerStyle((ReadOnlySpan<char>) s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result) == Number.ParsingStatus.OK;
      result = 0L;
      return false;
    }

    public static bool TryParse(ReadOnlySpan<char> s, out long result)
    {
      return Number.TryParseInt64IntegerStyle(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result) == Number.ParsingStatus.OK;
    }

    [NullableContext(2)]
    public static bool TryParse(
      string s,
      NumberStyles style,
      IFormatProvider provider,
      out long result)
    {
      NumberFormatInfo.ValidateParseStyleInteger(style);
      if (s != null)
        return Number.TryParseInt64((ReadOnlySpan<char>) s, style, NumberFormatInfo.GetInstance(provider), out result) == Number.ParsingStatus.OK;
      result = 0L;
      return false;
    }

    public static bool TryParse(
      ReadOnlySpan<char> s,
      NumberStyles style,
      [Nullable(2)] IFormatProvider provider,
      out long result)
    {
      NumberFormatInfo.ValidateParseStyleInteger(style);
      return Number.TryParseInt64(s, style, NumberFormatInfo.GetInstance(provider), out result) == Number.ParsingStatus.OK;
    }

    public TypeCode GetTypeCode()
    {
      return TypeCode.Int64;
    }

    bool IConvertible.ToBoolean(IFormatProvider provider)
    {
      return Convert.ToBoolean(this);
    }

    char IConvertible.ToChar(IFormatProvider provider)
    {
      return Convert.ToChar(this);
    }

    sbyte IConvertible.ToSByte(IFormatProvider provider)
    {
      return Convert.ToSByte(this);
    }

    byte IConvertible.ToByte(IFormatProvider provider)
    {
      return Convert.ToByte(this);
    }

    short IConvertible.ToInt16(IFormatProvider provider)
    {
      return Convert.ToInt16(this);
    }

    ushort IConvertible.ToUInt16(IFormatProvider provider)
    {
      return Convert.ToUInt16(this);
    }

    int IConvertible.ToInt32(IFormatProvider provider)
    {
      return Convert.ToInt32(this);
    }

    uint IConvertible.ToUInt32(IFormatProvider provider)
    {
      return Convert.ToUInt32(this);
    }

    long IConvertible.ToInt64(IFormatProvider provider)
    {
      return this;
    }

    ulong IConvertible.ToUInt64(IFormatProvider provider)
    {
      return Convert.ToUInt64(this);
    }

    float IConvertible.ToSingle(IFormatProvider provider)
    {
      return Convert.ToSingle(this);
    }

    double IConvertible.ToDouble(IFormatProvider provider)
    {
      return Convert.ToDouble(this);
    }

    Decimal IConvertible.ToDecimal(IFormatProvider provider)
    {
      return Convert.ToDecimal(this);
    }

    DateTime IConvertible.ToDateTime(IFormatProvider provider)
    {
      throw new InvalidCastException(SR.Format(SR.InvalidCast_FromTo, (object) nameof (Int64), (object) "DateTime"));
    }

    object IConvertible.ToType(Type type, IFormatProvider provider)
    {
      return Convert.DefaultToType((IConvertible) this, type, provider);
    }
  }
}

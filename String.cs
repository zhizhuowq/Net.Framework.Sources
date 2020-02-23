// Decompiled with JetBrains decompiler
// Type: System.String
// Assembly: System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// MVID: BBB3B884-123D-47EA-9CD1-5BED540D02AE
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Private.CoreLib.dll

using Internal.Runtime.CompilerServices;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace System
{
  [TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
  [Nullable(0)]
  [NullableContext(1)]
  [Serializable]
  public sealed class String : IComparable, IEnumerable, IConvertible, IEnumerable<char>, IComparable<string>, IEquatable<string>, ICloneable
  {
    [NonSerialized]
    private int _stringLength;
    [NonSerialized]
    private char _firstChar;
    [Intrinsic]
    public static readonly string Empty;

    [IndexerName("Chars")]
    public extern char this[int index] { [MethodImpl(MethodImplOptions.InternalCall)] get; }

    public extern int Length { [MethodImpl(MethodImplOptions.InternalCall)] get; }

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern string FastAllocateString(int length);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal extern void SetTrailByte(byte data);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal extern bool TryGetTrailByte(out byte data);

    [MethodImpl(MethodImplOptions.InternalCall)]
    private extern string Intern();

    [MethodImpl(MethodImplOptions.InternalCall)]
    private extern string IsInterned();

    public static string Intern(string str)
    {
      if (str == null)
        throw new ArgumentNullException(nameof (str));
      return str.Intern();
    }

    [return: Nullable(2)]
    public static string IsInterned(string str)
    {
      if (str == null)
        throw new ArgumentNullException(nameof (str));
      return str.IsInterned();
    }

    internal static unsafe void InternalCopy(string src, IntPtr dest, int len)
    {
      if (len == 0)
        return;
      fixed (char* chPtr = &src._firstChar)
        Buffer.Memcpy((byte*) (void*) dest, (byte*) chPtr, len);
    }

    internal unsafe int GetBytesFromEncoding(
      byte* pbNativeBuffer,
      int cbNativeBuffer,
      Encoding encoding)
    {
      fixed (char* chars = &this._firstChar)
        return encoding.GetBytes(chars, this.Length, pbNativeBuffer, cbNativeBuffer);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern String(char[] value);

    private unsafe string Ctor(char[] value)
    {
      if (value == null || value.Length == 0)
        return string.Empty;
      string str = string.FastAllocateString(value.Length);
      fixed (char* dmem = &str._firstChar)
        fixed (char* smem = value)
          string.wstrcpy(dmem, smem, value.Length);
      return str;
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern String(char[] value, int startIndex, int length);

    private unsafe string Ctor(char[] value, int startIndex, int length)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      if (startIndex < 0)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_StartIndex);
      if (length < 0)
        throw new ArgumentOutOfRangeException(nameof (length), SR.ArgumentOutOfRange_NegativeLength);
      if (startIndex > value.Length - length)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_Index);
      if (length == 0)
        return string.Empty;
      string str = string.FastAllocateString(length);
      fixed (char* dmem = &str._firstChar)
        fixed (char* chPtr = value)
          string.wstrcpy(dmem, chPtr + startIndex, length);
      return str;
    }

    [CLSCompliant(false)]
    [NullableContext(0)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern unsafe String(char* value);

    private unsafe string Ctor(char* ptr)
    {
      if ((IntPtr) ptr == IntPtr.Zero)
        return string.Empty;
      int num = string.wcslen(ptr);
      if (num == 0)
        return string.Empty;
      string str = string.FastAllocateString(num);
      fixed (char* dmem = &str._firstChar)
        string.wstrcpy(dmem, ptr, num);
      return str;
    }

    [CLSCompliant(false)]
    [NullableContext(0)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern unsafe String(char* value, int startIndex, int length);

    private unsafe string Ctor(char* ptr, int startIndex, int length)
    {
      if (length < 0)
        throw new ArgumentOutOfRangeException(nameof (length), SR.ArgumentOutOfRange_NegativeLength);
      if (startIndex < 0)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_StartIndex);
      char* smem = ptr + startIndex;
      if (smem < ptr)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_PartialWCHAR);
      if (length == 0)
        return string.Empty;
      if ((IntPtr) ptr == IntPtr.Zero)
        throw new ArgumentOutOfRangeException(nameof (ptr), SR.ArgumentOutOfRange_PartialWCHAR);
      string str = string.FastAllocateString(length);
      fixed (char* dmem = &str._firstChar)
        string.wstrcpy(dmem, smem, length);
      return str;
    }

    [NullableContext(0)]
    [CLSCompliant(false)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern unsafe String(sbyte* value);

    private unsafe string Ctor(sbyte* value)
    {
      byte* pb = (byte*) value;
      if ((IntPtr) pb == IntPtr.Zero)
        return string.Empty;
      int numBytes = string.strlen((byte*) value);
      return string.CreateStringForSByteConstructor(pb, numBytes);
    }

    [NullableContext(0)]
    [CLSCompliant(false)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern unsafe String(sbyte* value, int startIndex, int length);

    private unsafe string Ctor(sbyte* value, int startIndex, int length)
    {
      if (startIndex < 0)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_StartIndex);
      if (length < 0)
        throw new ArgumentOutOfRangeException(nameof (length), SR.ArgumentOutOfRange_NegativeLength);
      if ((IntPtr) value == IntPtr.Zero)
      {
        if (length == 0)
          return string.Empty;
        throw new ArgumentNullException(nameof (value));
      }
      byte* pb = (byte*) (value + startIndex);
      if (pb < value)
        throw new ArgumentOutOfRangeException(nameof (value), SR.ArgumentOutOfRange_PartialWCHAR);
      return string.CreateStringForSByteConstructor(pb, length);
    }

    private static unsafe string CreateStringForSByteConstructor(byte* pb, int numBytes)
    {
      if (numBytes == 0)
        return string.Empty;
      int wideChar1 = Interop.Kernel32.MultiByteToWideChar(0U, 1U, pb, numBytes, (char*) null, 0);
      if (wideChar1 == 0)
        throw new ArgumentException(SR.Arg_InvalidANSIString);
      string str = string.FastAllocateString(wideChar1);
      int wideChar2;
      fixed (char* lpWideCharStr = &str._firstChar)
        wideChar2 = Interop.Kernel32.MultiByteToWideChar(0U, 1U, pb, numBytes, lpWideCharStr, wideChar1);
      if (wideChar2 == 0)
        throw new ArgumentException(SR.Arg_InvalidANSIString);
      return str;
    }

    [CLSCompliant(false)]
    [NullableContext(0)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern unsafe String(sbyte* value, int startIndex, int length, [Nullable(1)] Encoding enc);

    private unsafe string Ctor(sbyte* value, int startIndex, int length, Encoding enc)
    {
      if (enc == null)
        return new string(value, startIndex, length);
      if (length < 0)
        throw new ArgumentOutOfRangeException(nameof (length), SR.ArgumentOutOfRange_NeedNonNegNum);
      if (startIndex < 0)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_StartIndex);
      if ((IntPtr) value == IntPtr.Zero)
      {
        if (length == 0)
          return string.Empty;
        throw new ArgumentNullException(nameof (value));
      }
      byte* numPtr = (byte*) (value + startIndex);
      if (numPtr < value)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_PartialWCHAR);
      return enc.GetString(new ReadOnlySpan<byte>((void*) numPtr, length));
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern String(char c, int count);

    private unsafe string Ctor(char c, int count)
    {
      if (count <= 0)
      {
        if (count == 0)
          return string.Empty;
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_NegativeCount);
      }
      string str = string.FastAllocateString(count);
      if (c != char.MinValue)
      {
        fixed (char* chPtr = &str._firstChar)
        {
          uint num = (uint) c << 16 | (uint) c;
          uint* numPtr = (uint*) chPtr;
          if (count >= 4)
          {
            count -= 4;
            do
            {
              *numPtr = num;
              numPtr[1] = num;
              numPtr += 2;
              count -= 4;
            }
            while (count >= 0);
          }
          if ((count & 2) != 0)
          {
            *numPtr = num;
            ++numPtr;
          }
          if ((count & 1) != 0)
            *(short*) numPtr = (short) c;
        }
      }
      return str;
    }

    [NullableContext(0)]
    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern String(ReadOnlySpan<char> value);

    private string Ctor(ReadOnlySpan<char> value)
    {
      if (value.Length == 0)
        return string.Empty;
      string str = string.FastAllocateString(value.Length);
      Buffer.Memmove<char>(ref str._firstChar, ref MemoryMarshal.GetReference<char>(value), (ulong) (uint) value.Length);
      return str;
    }

    public static string Create<[Nullable(2)] TState>(
      int length,
      TState state,
      SpanAction<char, TState> action)
    {
      if (action == null)
        throw new ArgumentNullException(nameof (action));
      if (length <= 0)
      {
        if (length == 0)
          return string.Empty;
        throw new ArgumentOutOfRangeException(nameof (length));
      }
      string str = string.FastAllocateString(length);
      action(new Span<char>(ref str.GetRawStringData(), length), state);
      return str;
    }

    [NullableContext(0)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<char>([Nullable(2)] string value)
    {
      return value == null ? new ReadOnlySpan<char>() : new ReadOnlySpan<char>(ref value.GetRawStringData(), value.Length);
    }

    public object Clone()
    {
      return (object) this;
    }

    public static unsafe string Copy(string str)
    {
      if (str == null)
        throw new ArgumentNullException(nameof (str));
      string str1 = string.FastAllocateString(str.Length);
      fixed (char* dmem = &str1._firstChar)
        fixed (char* smem = &str._firstChar)
          string.wstrcpy(dmem, smem, str.Length);
      return str1;
    }

    public unsafe void CopyTo(
      int sourceIndex,
      char[] destination,
      int destinationIndex,
      int count)
    {
      if (destination == null)
        throw new ArgumentNullException(nameof (destination));
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_NegativeCount);
      if (sourceIndex < 0)
        throw new ArgumentOutOfRangeException(nameof (sourceIndex), SR.ArgumentOutOfRange_Index);
      if (count > this.Length - sourceIndex)
        throw new ArgumentOutOfRangeException(nameof (sourceIndex), SR.ArgumentOutOfRange_IndexCount);
      if (destinationIndex > destination.Length - count || destinationIndex < 0)
        throw new ArgumentOutOfRangeException(nameof (destinationIndex), SR.ArgumentOutOfRange_IndexCount);
      fixed (char* chPtr1 = &this._firstChar)
        fixed (char* chPtr2 = destination)
          string.wstrcpy(chPtr2 + destinationIndex, chPtr1 + sourceIndex, count);
    }

    public unsafe char[] ToCharArray()
    {
      if (this.Length == 0)
        return Array.Empty<char>();
      char[] chArray = new char[this.Length];
      fixed (char* smem = &this._firstChar)
        fixed (char* dmem = &chArray[0])
          string.wstrcpy(dmem, smem, this.Length);
      return chArray;
    }

    public unsafe char[] ToCharArray(int startIndex, int length)
    {
      if (startIndex < 0 || startIndex > this.Length || startIndex > this.Length - length)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_Index);
      if (length <= 0)
      {
        if (length == 0)
          return Array.Empty<char>();
        throw new ArgumentOutOfRangeException(nameof (length), SR.ArgumentOutOfRange_Index);
      }
      char[] chArray = new char[length];
      fixed (char* chPtr = &this._firstChar)
        fixed (char* dmem = &chArray[0])
          string.wstrcpy(dmem, chPtr + startIndex, length);
      return chArray;
    }

    [NonVersionable]
    [NullableContext(2)]
    public static bool IsNullOrEmpty([NotNullWhen(false)] string value)
    {
      return value == null || 0U >= (uint) value.Length;
    }

    [NullableContext(2)]
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] string value)
    {
      if (value == null)
        return true;
      for (int index = 0; index < value.Length; ++index)
      {
        if (!char.IsWhiteSpace(value[index]))
          return false;
      }
      return true;
    }

    [NonVersionable]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ref readonly char GetPinnableReference()
    {
      return ref this._firstChar;
    }

    internal ref char GetRawStringData()
    {
      return ref this._firstChar;
    }

    internal static unsafe string CreateStringFromEncoding(
      byte* bytes,
      int byteLength,
      Encoding encoding)
    {
      int charCount = encoding.GetCharCount(bytes, byteLength);
      if (charCount == 0)
        return string.Empty;
      string str = string.FastAllocateString(charCount);
      fixed (char* chars = &str._firstChar)
        encoding.GetChars(bytes, byteLength, chars, charCount);
      return str;
    }

    internal static string CreateFromChar(char c)
    {
      string str = string.FastAllocateString(1);
      str._firstChar = c;
      return str;
    }

    internal static string CreateFromChar(char c1, char c2)
    {
      string str = string.FastAllocateString(2);
      str._firstChar = c1;
      Unsafe.Add<char>(ref str._firstChar, 1) = c2;
      return str;
    }

    internal static unsafe void wstrcpy(char* dmem, char* smem, int charCount)
    {
      Buffer.Memmove((byte*) dmem, (byte*) smem, (ulong) (uint) (charCount * 2));
    }

    public override string ToString()
    {
      return this;
    }

    public string ToString([Nullable(2)] IFormatProvider provider)
    {
      return this;
    }

    public CharEnumerator GetEnumerator()
    {
      return new CharEnumerator(this);
    }

    IEnumerator<char> IEnumerable<char>.GetEnumerator()
    {
      return (IEnumerator<char>) new CharEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) new CharEnumerator(this);
    }

    public StringRuneEnumerator EnumerateRunes()
    {
      return new StringRuneEnumerator(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe int wcslen(char* ptr)
    {
      int num = SpanHelpers.IndexOf(ref *ptr, char.MinValue, int.MaxValue);
      if (num < 0)
        string.ThrowMustBeNullTerminatedString();
      return num;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe int strlen(byte* ptr)
    {
      int num = SpanHelpers.IndexOf(ref *ptr, (byte) 0, int.MaxValue);
      if (num < 0)
        string.ThrowMustBeNullTerminatedString();
      return num;
    }

    [DoesNotReturn]
    private static void ThrowMustBeNullTerminatedString()
    {
      throw new ArgumentException(SR.Arg_MustBeNullTerminatedString);
    }

    public TypeCode GetTypeCode()
    {
      return TypeCode.String;
    }

    bool IConvertible.ToBoolean(IFormatProvider provider)
    {
      return Convert.ToBoolean(this, provider);
    }

    char IConvertible.ToChar(IFormatProvider provider)
    {
      return Convert.ToChar(this, provider);
    }

    sbyte IConvertible.ToSByte(IFormatProvider provider)
    {
      return Convert.ToSByte(this, provider);
    }

    byte IConvertible.ToByte(IFormatProvider provider)
    {
      return Convert.ToByte(this, provider);
    }

    short IConvertible.ToInt16(IFormatProvider provider)
    {
      return Convert.ToInt16(this, provider);
    }

    ushort IConvertible.ToUInt16(IFormatProvider provider)
    {
      return Convert.ToUInt16(this, provider);
    }

    int IConvertible.ToInt32(IFormatProvider provider)
    {
      return Convert.ToInt32(this, provider);
    }

    uint IConvertible.ToUInt32(IFormatProvider provider)
    {
      return Convert.ToUInt32(this, provider);
    }

    long IConvertible.ToInt64(IFormatProvider provider)
    {
      return Convert.ToInt64(this, provider);
    }

    ulong IConvertible.ToUInt64(IFormatProvider provider)
    {
      return Convert.ToUInt64(this, provider);
    }

    float IConvertible.ToSingle(IFormatProvider provider)
    {
      return Convert.ToSingle(this, provider);
    }

    double IConvertible.ToDouble(IFormatProvider provider)
    {
      return Convert.ToDouble(this, provider);
    }

    Decimal IConvertible.ToDecimal(IFormatProvider provider)
    {
      return Convert.ToDecimal(this, provider);
    }

    DateTime IConvertible.ToDateTime(IFormatProvider provider)
    {
      return Convert.ToDateTime(this, provider);
    }

    object IConvertible.ToType(Type type, IFormatProvider provider)
    {
      return Convert.DefaultToType((IConvertible) this, type, provider);
    }

    public bool IsNormalized()
    {
      return this.IsNormalized(NormalizationForm.FormC);
    }

    public bool IsNormalized(NormalizationForm normalizationForm)
    {
      return this.IsAscii() && (normalizationForm == NormalizationForm.FormC || normalizationForm == NormalizationForm.FormKC || (normalizationForm == NormalizationForm.FormD || normalizationForm == NormalizationForm.FormKD)) || Normalization.IsNormalized(this, normalizationForm);
    }

    public string Normalize()
    {
      return this.Normalize(NormalizationForm.FormC);
    }

    public string Normalize(NormalizationForm normalizationForm)
    {
      return this.IsAscii() && (normalizationForm == NormalizationForm.FormC || normalizationForm == NormalizationForm.FormKC || (normalizationForm == NormalizationForm.FormD || normalizationForm == NormalizationForm.FormKD)) ? this : Normalization.Normalize(this, normalizationForm);
    }

    private unsafe bool IsAscii()
    {
      fixed (char* pBuffer = &this._firstChar)
        return (long) ASCIIUtility.GetIndexOfFirstNonAsciiChar(pBuffer, (ulong) (uint) this.Length) == (long) (uint) this.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool EqualsHelper(string strA, string strB)
    {
      return SpanHelpers.SequenceEqual(ref Unsafe.As<char, byte>(ref strA.GetRawStringData()), ref Unsafe.As<char, byte>(ref strB.GetRawStringData()), (ulong) strA.Length * 2UL);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CompareOrdinalHelper(
      string strA,
      int indexA,
      int countA,
      string strB,
      int indexB,
      int countB)
    {
      return SpanHelpers.SequenceCompareTo(ref Unsafe.Add<char>(ref strA.GetRawStringData(), indexA), countA, ref Unsafe.Add<char>(ref strB.GetRawStringData(), indexB), countB);
    }

    private static bool EqualsOrdinalIgnoreCase(string strA, string strB)
    {
      return CompareInfo.EqualsOrdinalIgnoreCase(ref strA.GetRawStringData(), ref strB.GetRawStringData(), strB.Length);
    }

    private static unsafe int CompareOrdinalHelper(string strA, string strB)
    {
      int num1 = Math.Min(strA.Length, strB.Length);
      fixed (char* chPtr1 = &strA._firstChar)
        fixed (char* chPtr2 = &strB._firstChar)
        {
          char* chPtr3 = chPtr1;
          char* chPtr4 = chPtr2;
          if ((int) chPtr3[1] == (int) chPtr4[1])
          {
            int num2 = num1 - 2;
            chPtr3 += 2;
            chPtr4 += 2;
            while (num2 >= 12)
            {
              if (*(long*) chPtr3 == *(long*) chPtr4)
              {
                if (*(long*) (chPtr3 + 4) == *(long*) (chPtr4 + 4))
                {
                  if (*(long*) (chPtr3 + 8) == *(long*) (chPtr4 + 8))
                  {
                    num2 -= 12;
                    chPtr3 += 12;
                    chPtr4 += 12;
                    continue;
                  }
                  chPtr3 += 4;
                  chPtr4 += 4;
                }
                chPtr3 += 4;
                chPtr4 += 4;
              }
              if (*(int*) chPtr3 == *(int*) chPtr4)
              {
                chPtr3 += 2;
                chPtr4 += 2;
                goto label_15;
              }
              else
                goto label_15;
            }
            while (num2 > 0)
            {
              if (*(int*) chPtr3 == *(int*) chPtr4)
              {
                num2 -= 2;
                chPtr3 += 2;
                chPtr4 += 2;
              }
              else
                goto label_15;
            }
            return strA.Length - strB.Length;
label_15:
            if ((int) *chPtr3 != (int) *chPtr4)
              return (int) *chPtr3 - (int) *chPtr4;
          }
          return (int) chPtr3[1] - (int) chPtr4[1];
        }
    }

    [NullableContext(2)]
    public static int Compare(string strA, string strB)
    {
      return string.Compare(strA, strB, StringComparison.CurrentCulture);
    }

    [NullableContext(2)]
    public static int Compare(string strA, string strB, bool ignoreCase)
    {
      StringComparison comparisonType = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
      return string.Compare(strA, strB, comparisonType);
    }

    [NullableContext(2)]
    public static int Compare(string strA, string strB, StringComparison comparisonType)
    {
      if ((object) strA == (object) strB)
      {
        string.CheckStringComparison(comparisonType);
        return 0;
      }
      if (strA == null)
      {
        string.CheckStringComparison(comparisonType);
        return -1;
      }
      if (strB == null)
      {
        string.CheckStringComparison(comparisonType);
        return 1;
      }
      switch (comparisonType)
      {
        case StringComparison.CurrentCulture:
        case StringComparison.CurrentCultureIgnoreCase:
          return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.InvariantCulture:
        case StringComparison.InvariantCultureIgnoreCase:
          return CompareInfo.Invariant.Compare(strA, strB, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.Ordinal:
          return (int) strA._firstChar != (int) strB._firstChar ? (int) strA._firstChar - (int) strB._firstChar : string.CompareOrdinalHelper(strA, strB);
        case StringComparison.OrdinalIgnoreCase:
          return CompareInfo.CompareOrdinalIgnoreCase(strA, strB);
        default:
          throw new ArgumentException(SR.NotSupported_StringComparison, nameof (comparisonType));
      }
    }

    [NullableContext(2)]
    public static int Compare(
      string strA,
      string strB,
      CultureInfo culture,
      CompareOptions options)
    {
      return (culture ?? CultureInfo.CurrentCulture).CompareInfo.Compare(strA, strB, options);
    }

    [NullableContext(2)]
    public static int Compare(string strA, string strB, bool ignoreCase, CultureInfo culture)
    {
      CompareOptions options = ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
      return string.Compare(strA, strB, culture, options);
    }

    [NullableContext(2)]
    public static int Compare(string strA, int indexA, string strB, int indexB, int length)
    {
      return string.Compare(strA, indexA, strB, indexB, length, false);
    }

    [NullableContext(2)]
    public static int Compare(
      string strA,
      int indexA,
      string strB,
      int indexB,
      int length,
      bool ignoreCase)
    {
      int num1 = length;
      int num2 = length;
      if (strA != null)
        num1 = Math.Min(num1, strA.Length - indexA);
      if (strB != null)
        num2 = Math.Min(num2, strB.Length - indexB);
      CompareOptions options = ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
      return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num1, strB, indexB, num2, options);
    }

    [NullableContext(2)]
    public static int Compare(
      string strA,
      int indexA,
      string strB,
      int indexB,
      int length,
      bool ignoreCase,
      CultureInfo culture)
    {
      CompareOptions options = ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
      return string.Compare(strA, indexA, strB, indexB, length, culture, options);
    }

    [NullableContext(2)]
    public static int Compare(
      string strA,
      int indexA,
      string strB,
      int indexB,
      int length,
      CultureInfo culture,
      CompareOptions options)
    {
      CultureInfo cultureInfo = culture ?? CultureInfo.CurrentCulture;
      int num1 = length;
      int num2 = length;
      if (strA != null)
        num1 = Math.Min(num1, strA.Length - indexA);
      if (strB != null)
        num2 = Math.Min(num2, strB.Length - indexB);
      return cultureInfo.CompareInfo.Compare(strA, indexA, num1, strB, indexB, num2, options);
    }

    [NullableContext(2)]
    public static int Compare(
      string strA,
      int indexA,
      string strB,
      int indexB,
      int length,
      StringComparison comparisonType)
    {
      string.CheckStringComparison(comparisonType);
      if (strA == null || strB == null)
      {
        if ((object) strA == (object) strB)
          return 0;
        return strA != null ? 1 : -1;
      }
      if (length < 0)
        throw new ArgumentOutOfRangeException(nameof (length), SR.ArgumentOutOfRange_NegativeLength);
      if (indexA < 0 || indexB < 0)
        throw new ArgumentOutOfRangeException(indexA < 0 ? nameof (indexA) : nameof (indexB), SR.ArgumentOutOfRange_Index);
      if (strA.Length - indexA < 0 || strB.Length - indexB < 0)
        throw new ArgumentOutOfRangeException(strA.Length - indexA < 0 ? nameof (indexA) : nameof (indexB), SR.ArgumentOutOfRange_Index);
      if (length == 0 || (object) strA == (object) strB && indexA == indexB)
        return 0;
      int num1 = Math.Min(length, strA.Length - indexA);
      int num2 = Math.Min(length, strB.Length - indexB);
      switch (comparisonType)
      {
        case StringComparison.CurrentCulture:
        case StringComparison.CurrentCultureIgnoreCase:
          return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num1, strB, indexB, num2, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.InvariantCulture:
        case StringComparison.InvariantCultureIgnoreCase:
          return CompareInfo.Invariant.Compare(strA, indexA, num1, strB, indexB, num2, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.Ordinal:
          return string.CompareOrdinalHelper(strA, indexA, num1, strB, indexB, num2);
        default:
          return CompareInfo.CompareOrdinalIgnoreCase(strA, indexA, num1, strB, indexB, num2);
      }
    }

    [NullableContext(2)]
    public static int CompareOrdinal(string strA, string strB)
    {
      if ((object) strA == (object) strB)
        return 0;
      if (strA == null)
        return -1;
      if (strB == null)
        return 1;
      return (int) strA._firstChar != (int) strB._firstChar ? (int) strA._firstChar - (int) strB._firstChar : string.CompareOrdinalHelper(strA, strB);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int CompareOrdinal(ReadOnlySpan<char> strA, ReadOnlySpan<char> strB)
    {
      return SpanHelpers.SequenceCompareTo(ref MemoryMarshal.GetReference<char>(strA), strA.Length, ref MemoryMarshal.GetReference<char>(strB), strB.Length);
    }

    [NullableContext(2)]
    public static int CompareOrdinal(
      string strA,
      int indexA,
      string strB,
      int indexB,
      int length)
    {
      if (strA == null || strB == null)
      {
        if ((object) strA == (object) strB)
          return 0;
        return strA != null ? 1 : -1;
      }
      if (length < 0)
        throw new ArgumentOutOfRangeException(nameof (length), SR.ArgumentOutOfRange_NegativeCount);
      if (indexA < 0 || indexB < 0)
        throw new ArgumentOutOfRangeException(indexA < 0 ? nameof (indexA) : nameof (indexB), SR.ArgumentOutOfRange_Index);
      int countA = Math.Min(length, strA.Length - indexA);
      int countB = Math.Min(length, strB.Length - indexB);
      if (countA < 0 || countB < 0)
        throw new ArgumentOutOfRangeException(countA < 0 ? nameof (indexA) : nameof (indexB), SR.ArgumentOutOfRange_Index);
      return length == 0 || (object) strA == (object) strB && indexA == indexB ? 0 : string.CompareOrdinalHelper(strA, indexA, countA, strB, indexB, countB);
    }

    [NullableContext(2)]
    public int CompareTo(object value)
    {
      if (value == null)
        return 1;
      if (!(value is string strB))
        throw new ArgumentException(SR.Arg_MustBeString);
      return this.CompareTo(strB);
    }

    [NullableContext(2)]
    public int CompareTo(string strB)
    {
      return string.Compare(this, strB, StringComparison.CurrentCulture);
    }

    public bool EndsWith(string value)
    {
      return this.EndsWith(value, StringComparison.CurrentCulture);
    }

    public bool EndsWith(string value, StringComparison comparisonType)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      if ((object) this == (object) value)
      {
        string.CheckStringComparison(comparisonType);
        return true;
      }
      if (value.Length == 0)
      {
        string.CheckStringComparison(comparisonType);
        return true;
      }
      switch (comparisonType)
      {
        case StringComparison.CurrentCulture:
        case StringComparison.CurrentCultureIgnoreCase:
          return CultureInfo.CurrentCulture.CompareInfo.IsSuffix(this, value, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.InvariantCulture:
        case StringComparison.InvariantCultureIgnoreCase:
          return CompareInfo.Invariant.IsSuffix(this, value, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.Ordinal:
          int start = this.Length - value.Length;
          return (uint) start <= (uint) this.Length && this.AsSpan(start).SequenceEqual<char>((ReadOnlySpan<char>) value);
        case StringComparison.OrdinalIgnoreCase:
          return this.Length >= value.Length && CompareInfo.CompareOrdinalIgnoreCase(this, this.Length - value.Length, value.Length, value, 0, value.Length) == 0;
        default:
          throw new ArgumentException(SR.NotSupported_StringComparison, nameof (comparisonType));
      }
    }

    public bool EndsWith(string value, bool ignoreCase, [Nullable(2)] CultureInfo culture)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      return (object) this == (object) value || (culture ?? CultureInfo.CurrentCulture).CompareInfo.IsSuffix(this, value, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
    }

    public bool EndsWith(char value)
    {
      int index = this.Length - 1;
      return (uint) index < (uint) this.Length && (int) this[index] == (int) value;
    }

    [NullableContext(2)]
    public override bool Equals(object obj)
    {
      if ((object) this == obj)
        return true;
      return obj is string strB && this.Length == strB.Length && string.EqualsHelper(this, strB);
    }

    [NullableContext(2)]
    public bool Equals(string value)
    {
      if ((object) this == (object) value)
        return true;
      return value != null && this.Length == value.Length && string.EqualsHelper(this, value);
    }

    [NullableContext(2)]
    public bool Equals(string value, StringComparison comparisonType)
    {
      if ((object) this == (object) value)
      {
        string.CheckStringComparison(comparisonType);
        return true;
      }
      if (value == null)
      {
        string.CheckStringComparison(comparisonType);
        return false;
      }
      switch (comparisonType)
      {
        case StringComparison.CurrentCulture:
        case StringComparison.CurrentCultureIgnoreCase:
          return CultureInfo.CurrentCulture.CompareInfo.Compare(this, value, string.GetCaseCompareOfComparisonCulture(comparisonType)) == 0;
        case StringComparison.InvariantCulture:
        case StringComparison.InvariantCultureIgnoreCase:
          return CompareInfo.Invariant.Compare(this, value, string.GetCaseCompareOfComparisonCulture(comparisonType)) == 0;
        case StringComparison.Ordinal:
          return this.Length == value.Length && string.EqualsHelper(this, value);
        case StringComparison.OrdinalIgnoreCase:
          return this.Length == value.Length && string.EqualsOrdinalIgnoreCase(this, value);
        default:
          throw new ArgumentException(SR.NotSupported_StringComparison, nameof (comparisonType));
      }
    }

    [NullableContext(2)]
    public static bool Equals(string a, string b)
    {
      if ((object) a == (object) b)
        return true;
      return a != null && b != null && a.Length == b.Length && string.EqualsHelper(a, b);
    }

    [NullableContext(2)]
    public static bool Equals(string a, string b, StringComparison comparisonType)
    {
      if ((object) a == (object) b)
      {
        string.CheckStringComparison(comparisonType);
        return true;
      }
      if (a == null || b == null)
      {
        string.CheckStringComparison(comparisonType);
        return false;
      }
      switch (comparisonType)
      {
        case StringComparison.CurrentCulture:
        case StringComparison.CurrentCultureIgnoreCase:
          return CultureInfo.CurrentCulture.CompareInfo.Compare(a, b, string.GetCaseCompareOfComparisonCulture(comparisonType)) == 0;
        case StringComparison.InvariantCulture:
        case StringComparison.InvariantCultureIgnoreCase:
          return CompareInfo.Invariant.Compare(a, b, string.GetCaseCompareOfComparisonCulture(comparisonType)) == 0;
        case StringComparison.Ordinal:
          return a.Length == b.Length && string.EqualsHelper(a, b);
        case StringComparison.OrdinalIgnoreCase:
          return a.Length == b.Length && string.EqualsOrdinalIgnoreCase(a, b);
        default:
          throw new ArgumentException(SR.NotSupported_StringComparison, nameof (comparisonType));
      }
    }

    [NullableContext(2)]
    public static bool operator ==(string a, string b)
    {
      return string.Equals(a, b);
    }

    [NullableContext(2)]
    public static bool operator !=(string a, string b)
    {
      return !string.Equals(a, b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
      ulong defaultSeed = Marvin.DefaultSeed;
      return Marvin.ComputeHash32(ref Unsafe.As<char, byte>(ref this._firstChar), (uint) (this._stringLength * 2), (uint) defaultSeed, (uint) (defaultSeed >> 32));
    }

    public int GetHashCode(StringComparison comparisonType)
    {
      return StringComparer.FromComparison(comparisonType).GetHashCode(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int GetHashCodeOrdinalIgnoreCase()
    {
      ulong defaultSeed = Marvin.DefaultSeed;
      return Marvin.ComputeHash32OrdinalIgnoreCase(ref this._firstChar, this._stringLength, (uint) defaultSeed, (uint) (defaultSeed >> 32));
    }

    [NullableContext(0)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetHashCode(ReadOnlySpan<char> value)
    {
      ulong defaultSeed = Marvin.DefaultSeed;
      return Marvin.ComputeHash32(ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference<char>(value)), (uint) (value.Length * 2), (uint) defaultSeed, (uint) (defaultSeed >> 32));
    }

    [NullableContext(0)]
    public static int GetHashCode(ReadOnlySpan<char> value, StringComparison comparisonType)
    {
      switch (comparisonType)
      {
        case StringComparison.CurrentCulture:
        case StringComparison.CurrentCultureIgnoreCase:
          return CultureInfo.CurrentCulture.CompareInfo.GetHashCode(value, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.InvariantCulture:
        case StringComparison.InvariantCultureIgnoreCase:
          return CultureInfo.InvariantCulture.CompareInfo.GetHashCode(value, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.Ordinal:
          return string.GetHashCode(value);
        case StringComparison.OrdinalIgnoreCase:
          return string.GetHashCodeOrdinalIgnoreCase(value);
        default:
          ThrowHelper.ThrowArgumentException(ExceptionResource.NotSupported_StringComparison, ExceptionArgument.comparisonType);
          return 0;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetHashCodeOrdinalIgnoreCase(ReadOnlySpan<char> value)
    {
      ulong defaultSeed = Marvin.DefaultSeed;
      return Marvin.ComputeHash32OrdinalIgnoreCase(ref MemoryMarshal.GetReference<char>(value), value.Length, (uint) defaultSeed, (uint) (defaultSeed >> 32));
    }

    internal unsafe int GetNonRandomizedHashCode()
    {
      fixed (char* chPtr = &this._firstChar)
      {
        uint num1 = 352654597;
        uint num2 = num1;
        uint* numPtr = (uint*) chPtr;
        int length = this.Length;
        while (length > 2)
        {
          length -= 4;
          num1 = BitOperations.RotateLeft(num1, 5) + num1 ^ *numPtr;
          num2 = BitOperations.RotateLeft(num2, 5) + num2 ^ numPtr[1];
          numPtr += 2;
        }
        if (length > 0)
          num2 = BitOperations.RotateLeft(num2, 5) + num2 ^ *numPtr;
        return (int) num1 + (int) num2 * 1566083941;
      }
    }

    public bool StartsWith(string value)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      return this.StartsWith(value, StringComparison.CurrentCulture);
    }

    public bool StartsWith(string value, StringComparison comparisonType)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      if ((object) this == (object) value)
      {
        string.CheckStringComparison(comparisonType);
        return true;
      }
      if (value.Length == 0)
      {
        string.CheckStringComparison(comparisonType);
        return true;
      }
      switch (comparisonType)
      {
        case StringComparison.CurrentCulture:
        case StringComparison.CurrentCultureIgnoreCase:
          return CultureInfo.CurrentCulture.CompareInfo.IsPrefix(this, value, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.InvariantCulture:
        case StringComparison.InvariantCultureIgnoreCase:
          return CompareInfo.Invariant.IsPrefix(this, value, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.Ordinal:
          if (this.Length < value.Length || (int) this._firstChar != (int) value._firstChar)
            return false;
          return value.Length == 1 || SpanHelpers.SequenceEqual(ref Unsafe.As<char, byte>(ref this.GetRawStringData()), ref Unsafe.As<char, byte>(ref value.GetRawStringData()), (ulong) value.Length * 2UL);
        case StringComparison.OrdinalIgnoreCase:
          return this.Length >= value.Length && CompareInfo.EqualsOrdinalIgnoreCase(ref this.GetRawStringData(), ref value.GetRawStringData(), value.Length);
        default:
          throw new ArgumentException(SR.NotSupported_StringComparison, nameof (comparisonType));
      }
    }

    public bool StartsWith(string value, bool ignoreCase, [Nullable(2)] CultureInfo culture)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      return (object) this == (object) value || (culture ?? CultureInfo.CurrentCulture).CompareInfo.IsPrefix(this, value, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
    }

    public bool StartsWith(char value)
    {
      return this.Length != 0 && (int) this._firstChar == (int) value;
    }

    internal static void CheckStringComparison(StringComparison comparisonType)
    {
      switch (comparisonType)
      {
        case StringComparison.CurrentCulture:
        case StringComparison.CurrentCultureIgnoreCase:
        case StringComparison.InvariantCulture:
        case StringComparison.InvariantCultureIgnoreCase:
        case StringComparison.Ordinal:
        case StringComparison.OrdinalIgnoreCase:
          break;
        default:
          ThrowHelper.ThrowArgumentException(ExceptionResource.NotSupported_StringComparison, ExceptionArgument.comparisonType);
          break;
      }
    }

    internal static CompareOptions GetCaseCompareOfComparisonCulture(
      StringComparison comparisonType)
    {
      return (CompareOptions) (comparisonType & StringComparison.CurrentCultureIgnoreCase);
    }

    private static unsafe void FillStringChecked(string dest, int destPos, string src)
    {
      if (src.Length > dest.Length - destPos)
        throw new IndexOutOfRangeException();
      fixed (char* chPtr = &dest._firstChar)
        fixed (char* smem = &src._firstChar)
          string.wstrcpy(chPtr + destPos, smem, src.Length);
    }

    public static string Concat([Nullable(2)] object arg0)
    {
      return arg0?.ToString() ?? string.Empty;
    }

    [NullableContext(2)]
    [return: Nullable(1)]
    public static string Concat(object arg0, object arg1)
    {
      if (arg0 == null)
        arg0 = (object) string.Empty;
      if (arg1 == null)
        arg1 = (object) string.Empty;
      return arg0.ToString() + arg1.ToString();
    }

    [NullableContext(2)]
    [return: Nullable(1)]
    public static string Concat(object arg0, object arg1, object arg2)
    {
      if (arg0 == null)
        arg0 = (object) string.Empty;
      if (arg1 == null)
        arg1 = (object) string.Empty;
      if (arg2 == null)
        arg2 = (object) string.Empty;
      return arg0.ToString() + arg1.ToString() + arg2.ToString();
    }

    public static string Concat([Nullable(new byte[] {1, 2})] params object[] args)
    {
      if (args == null)
        throw new ArgumentNullException(nameof (args));
      if (args.Length <= 1)
        return args.Length != 0 ? args[0]?.ToString() ?? string.Empty : string.Empty;
      string[] strArray = new string[args.Length];
      int length = 0;
      for (int index = 0; index < args.Length; ++index)
      {
        string str = args[index]?.ToString() ?? string.Empty;
        strArray[index] = str;
        length += str.Length;
        if (length < 0)
          throw new OutOfMemoryException();
      }
      if (length == 0)
        return string.Empty;
      string dest = string.FastAllocateString(length);
      int destPos = 0;
      for (int index = 0; index < strArray.Length; ++index)
      {
        string src = strArray[index];
        string.FillStringChecked(dest, destPos, src);
        destPos += src.Length;
      }
      return dest;
    }

    public static string Concat<[Nullable(2)] T>(IEnumerable<T> values)
    {
      if (values == null)
        throw new ArgumentNullException(nameof (values));
      if (typeof (T) == typeof (char))
      {
        using (IEnumerator<char> enumerator = Unsafe.As<IEnumerable<char>>((object) values).GetEnumerator())
        {
          if (!enumerator.MoveNext())
            return string.Empty;
          char current1 = enumerator.Current;
          if (!enumerator.MoveNext())
            return string.CreateFromChar(current1);
          StringBuilder sb = StringBuilderCache.Acquire(16);
          sb.Append(current1);
          do
          {
            char current2 = enumerator.Current;
            sb.Append(current2);
          }
          while (enumerator.MoveNext());
          return StringBuilderCache.GetStringAndRelease(sb);
        }
      }
      else
      {
        using (IEnumerator<T> enumerator = values.GetEnumerator())
        {
          if (!enumerator.MoveNext())
            return string.Empty;
          string str = enumerator.Current?.ToString();
          if (!enumerator.MoveNext())
            return str ?? string.Empty;
          StringBuilder sb = StringBuilderCache.Acquire(16);
          sb.Append(str);
          do
          {
            T current = enumerator.Current;
            if ((object) current != null)
              sb.Append(current.ToString());
          }
          while (enumerator.MoveNext());
          return StringBuilderCache.GetStringAndRelease(sb);
        }
      }
    }

    public static string Concat([Nullable(new byte[] {1, 2})] IEnumerable<string> values)
    {
      if (values == null)
        throw new ArgumentNullException(nameof (values));
      using (IEnumerator<string> enumerator = values.GetEnumerator())
      {
        if (!enumerator.MoveNext())
          return string.Empty;
        string current = enumerator.Current;
        if (!enumerator.MoveNext())
          return current ?? string.Empty;
        StringBuilder sb = StringBuilderCache.Acquire(16);
        sb.Append(current);
        do
        {
          sb.Append(enumerator.Current);
        }
        while (enumerator.MoveNext());
        return StringBuilderCache.GetStringAndRelease(sb);
      }
    }

    [NullableContext(2)]
    [return: Nullable(1)]
    public static string Concat(string str0, string str1)
    {
      if (string.IsNullOrEmpty(str0))
        return string.IsNullOrEmpty(str1) ? string.Empty : str1;
      if (string.IsNullOrEmpty(str1))
        return str0;
      int length = str0.Length;
      string dest = string.FastAllocateString(length + str1.Length);
      string.FillStringChecked(dest, 0, str0);
      string.FillStringChecked(dest, length, str1);
      return dest;
    }

    [NullableContext(2)]
    [return: Nullable(1)]
    public static string Concat(string str0, string str1, string str2)
    {
      if (string.IsNullOrEmpty(str0))
        return str1 + str2;
      if (string.IsNullOrEmpty(str1))
        return str0 + str2;
      if (string.IsNullOrEmpty(str2))
        return str0 + str1;
      string dest = string.FastAllocateString(str0.Length + str1.Length + str2.Length);
      string.FillStringChecked(dest, 0, str0);
      string.FillStringChecked(dest, str0.Length, str1);
      string.FillStringChecked(dest, str0.Length + str1.Length, str2);
      return dest;
    }

    [NullableContext(2)]
    [return: Nullable(1)]
    public static string Concat(string str0, string str1, string str2, string str3)
    {
      if (string.IsNullOrEmpty(str0))
        return str1 + str2 + str3;
      if (string.IsNullOrEmpty(str1))
        return str0 + str2 + str3;
      if (string.IsNullOrEmpty(str2))
        return str0 + str1 + str3;
      if (string.IsNullOrEmpty(str3))
        return str0 + str1 + str2;
      string dest = string.FastAllocateString(str0.Length + str1.Length + str2.Length + str3.Length);
      string.FillStringChecked(dest, 0, str0);
      string.FillStringChecked(dest, str0.Length, str1);
      string.FillStringChecked(dest, str0.Length + str1.Length, str2);
      string.FillStringChecked(dest, str0.Length + str1.Length + str2.Length, str3);
      return dest;
    }

    [NullableContext(0)]
    [return: Nullable(1)]
    public static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1)
    {
      int length = checked (str0.Length + str1.Length);
      if (length == 0)
        return string.Empty;
      string str = string.FastAllocateString(length);
      Span<char> destination = new Span<char>(ref str.GetRawStringData(), str.Length);
      str0.CopyTo(destination);
      str1.CopyTo(destination.Slice(str0.Length));
      return str;
    }

    [NullableContext(0)]
    [return: Nullable(1)]
    public static string Concat(
      ReadOnlySpan<char> str0,
      ReadOnlySpan<char> str1,
      ReadOnlySpan<char> str2)
    {
      int length = checked (str0.Length + str1.Length + str2.Length);
      if (length == 0)
        return string.Empty;
      string str = string.FastAllocateString(length);
      Span<char> destination = new Span<char>(ref str.GetRawStringData(), str.Length);
      str0.CopyTo(destination);
      destination = destination.Slice(str0.Length);
      str1.CopyTo(destination);
      destination = destination.Slice(str1.Length);
      str2.CopyTo(destination);
      return str;
    }

    [NullableContext(0)]
    [return: Nullable(1)]
    public static string Concat(
      ReadOnlySpan<char> str0,
      ReadOnlySpan<char> str1,
      ReadOnlySpan<char> str2,
      ReadOnlySpan<char> str3)
    {
      int length = checked (str0.Length + str1.Length + str2.Length + str3.Length);
      if (length == 0)
        return string.Empty;
      string str = string.FastAllocateString(length);
      Span<char> destination1 = new Span<char>(ref str.GetRawStringData(), str.Length);
      str0.CopyTo(destination1);
      Span<char> destination2 = destination1.Slice(str0.Length);
      str1.CopyTo(destination2);
      destination2 = destination2.Slice(str1.Length);
      str2.CopyTo(destination2);
      destination2 = destination2.Slice(str2.Length);
      str3.CopyTo(destination2);
      return str;
    }

    public static string Concat([Nullable(new byte[] {1, 2})] params string[] values)
    {
      if (values == null)
        throw new ArgumentNullException(nameof (values));
      if (values.Length <= 1)
        return values.Length != 0 ? values[0] ?? string.Empty : string.Empty;
      long num = 0;
      for (int index = 0; index < values.Length; ++index)
      {
        string str = values[index];
        if (str != null)
          num += (long) str.Length;
      }
      if (num > (long) int.MaxValue)
        throw new OutOfMemoryException();
      int length1 = (int) num;
      if (length1 == 0)
        return string.Empty;
      string dest = string.FastAllocateString(length1);
      int destPos = 0;
      for (int index = 0; index < values.Length; ++index)
      {
        string src = values[index];
        if (!string.IsNullOrEmpty(src))
        {
          int length2 = src.Length;
          if (length2 > length1 - destPos)
          {
            destPos = -1;
            break;
          }
          string.FillStringChecked(dest, destPos, src);
          destPos += length2;
        }
      }
      return destPos != length1 ? string.Concat((string[]) values.Clone()) : dest;
    }

    public static string Format(string format, [Nullable(2)] object arg0)
    {
      return string.FormatHelper((IFormatProvider) null, format, new ParamsArray(arg0));
    }

    public static string Format(string format, [Nullable(2)] object arg0, [Nullable(2)] object arg1)
    {
      return string.FormatHelper((IFormatProvider) null, format, new ParamsArray(arg0, arg1));
    }

    [NullableContext(2)]
    [return: Nullable(1)]
    public static string Format([Nullable(1)] string format, object arg0, object arg1, object arg2)
    {
      return string.FormatHelper((IFormatProvider) null, format, new ParamsArray(arg0, arg1, arg2));
    }

    public static string Format(string format, [Nullable(new byte[] {1, 2})] params object[] args)
    {
      if (args == null)
        throw new ArgumentNullException(format == null ? nameof (format) : nameof (args));
      return string.FormatHelper((IFormatProvider) null, format, new ParamsArray(args));
    }

    public static string Format([Nullable(2)] IFormatProvider provider, string format, [Nullable(2)] object arg0)
    {
      return string.FormatHelper(provider, format, new ParamsArray(arg0));
    }

    [NullableContext(2)]
    [return: Nullable(1)]
    public static string Format(IFormatProvider provider, [Nullable(1)] string format, object arg0, object arg1)
    {
      return string.FormatHelper(provider, format, new ParamsArray(arg0, arg1));
    }

    [NullableContext(2)]
    [return: Nullable(1)]
    public static string Format(
      IFormatProvider provider,
      [Nullable(1)] string format,
      object arg0,
      object arg1,
      object arg2)
    {
      return string.FormatHelper(provider, format, new ParamsArray(arg0, arg1, arg2));
    }

    public static string Format([Nullable(2)] IFormatProvider provider, string format, [Nullable(new byte[] {1, 2})] params object[] args)
    {
      if (args == null)
        throw new ArgumentNullException(format == null ? nameof (format) : nameof (args));
      return string.FormatHelper(provider, format, new ParamsArray(args));
    }

    private static string FormatHelper(IFormatProvider provider, string format, ParamsArray args)
    {
      if (format == null)
        throw new ArgumentNullException(nameof (format));
      return StringBuilderCache.GetStringAndRelease(StringBuilderCache.Acquire(format.Length + args.Length * 8).AppendFormatHelper(provider, format, args));
    }

    public unsafe string Insert(int startIndex, string value)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      if (startIndex < 0 || startIndex > this.Length)
        throw new ArgumentOutOfRangeException(nameof (startIndex));
      int length1 = this.Length;
      int length2 = value.Length;
      if (length1 == 0)
        return value;
      if (length2 == 0)
        return this;
      string str = string.FastAllocateString(length1 + length2);
      fixed (char* smem1 = &this._firstChar)
        fixed (char* smem2 = &value._firstChar)
          fixed (char* dmem = &str._firstChar)
          {
            string.wstrcpy(dmem, smem1, startIndex);
            string.wstrcpy(dmem + startIndex, smem2, length2);
            string.wstrcpy(dmem + startIndex + length2, smem1 + startIndex, length1 - startIndex);
          }
      return str;
    }

    public static string Join(char separator, [Nullable(new byte[] {1, 2})] params string[] value)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      return string.Join(separator, value, 0, value.Length);
    }

    public static unsafe string Join(char separator, [Nullable(new byte[] {1, 2})] params object[] values)
    {
      return string.JoinCore(&separator, 1, values);
    }

    public static unsafe string Join<[Nullable(2)] T>(char separator, IEnumerable<T> values)
    {
      return string.JoinCore<T>(&separator, 1, values);
    }

    public static unsafe string Join(char separator, [Nullable(new byte[] {1, 2})] string[] value, int startIndex, int count)
    {
      return string.JoinCore(&separator, 1, value, startIndex, count);
    }

    public static string Join([Nullable(2)] string separator, [Nullable(new byte[] {1, 2})] params string[] value)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      return string.Join(separator, value, 0, value.Length);
    }

    public static unsafe string Join([Nullable(2)] string separator, [Nullable(new byte[] {1, 2})] params object[] values)
    {
      separator = separator ?? string.Empty;
      fixed (char* separator1 = &separator._firstChar)
        return string.JoinCore(separator1, separator.Length, values);
    }

    public static unsafe string Join<[Nullable(2)] T>(
      [Nullable(2)] string separator,
      IEnumerable<T> values)
    {
      separator = separator ?? string.Empty;
      fixed (char* separator1 = &separator._firstChar)
        return string.JoinCore<T>(separator1, separator.Length, values);
    }

    public static string Join([Nullable(2)] string separator, [Nullable(new byte[] {1, 2})] IEnumerable<string> values)
    {
      if (values == null)
        throw new ArgumentNullException(nameof (values));
      using (IEnumerator<string> enumerator = values.GetEnumerator())
      {
        if (!enumerator.MoveNext())
          return string.Empty;
        string current = enumerator.Current;
        if (!enumerator.MoveNext())
          return current ?? string.Empty;
        StringBuilder sb = StringBuilderCache.Acquire(16);
        sb.Append(current);
        do
        {
          sb.Append(separator);
          sb.Append(enumerator.Current);
        }
        while (enumerator.MoveNext());
        return StringBuilderCache.GetStringAndRelease(sb);
      }
    }

    public static unsafe string Join([Nullable(2)] string separator, [Nullable(new byte[] {1, 2})] string[] value, int startIndex, int count)
    {
      separator = separator ?? string.Empty;
      fixed (char* separator1 = &separator._firstChar)
        return string.JoinCore(separator1, separator.Length, value, startIndex, count);
    }

    private static unsafe string JoinCore(char* separator, int separatorLength, object[] values)
    {
      if (values == null)
        throw new ArgumentNullException(nameof (values));
      if (values.Length == 0)
        return string.Empty;
      string str = values[0]?.ToString();
      if (values.Length == 1)
        return str ?? string.Empty;
      StringBuilder sb = StringBuilderCache.Acquire(16);
      sb.Append(str);
      for (int index = 1; index < values.Length; ++index)
      {
        sb.Append(separator, separatorLength);
        object obj = values[index];
        if (obj != null)
          sb.Append(obj.ToString());
      }
      return StringBuilderCache.GetStringAndRelease(sb);
    }

    private static unsafe string JoinCore<T>(
      char* separator,
      int separatorLength,
      IEnumerable<T> values)
    {
      if (values == null)
        throw new ArgumentNullException(nameof (values));
      using (IEnumerator<T> enumerator = values.GetEnumerator())
      {
        if (!enumerator.MoveNext())
          return string.Empty;
        string str = enumerator.Current?.ToString();
        if (!enumerator.MoveNext())
          return str ?? string.Empty;
        StringBuilder sb = StringBuilderCache.Acquire(16);
        sb.Append(str);
        do
        {
          T current = enumerator.Current;
          sb.Append(separator, separatorLength);
          if ((object) current != null)
            sb.Append(current.ToString());
        }
        while (enumerator.MoveNext());
        return StringBuilderCache.GetStringAndRelease(sb);
      }
    }

    private static unsafe string JoinCore(
      char* separator,
      int separatorLength,
      string[] value,
      int startIndex,
      int count)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      if (startIndex < 0)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_StartIndex);
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_NegativeCount);
      if (startIndex > value.Length - count)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_IndexCountBuffer);
      if (count <= 1)
        return count != 0 ? value[startIndex] ?? string.Empty : string.Empty;
      long num = (long) (count - 1) * (long) separatorLength;
      if (num > (long) int.MaxValue)
        throw new OutOfMemoryException();
      int length1 = (int) num;
      int index1 = startIndex;
      for (int index2 = startIndex + count; index1 < index2; ++index1)
      {
        string str = value[index1];
        if (str != null)
        {
          length1 += str.Length;
          if (length1 < 0)
            throw new OutOfMemoryException();
        }
      }
      string dest = string.FastAllocateString(length1);
      int destPos = 0;
      int index3 = startIndex;
      for (int index2 = startIndex + count; index3 < index2; ++index3)
      {
        string src = value[index3];
        if (src != null)
        {
          int length2 = src.Length;
          if (length2 > length1 - destPos)
          {
            destPos = -1;
            break;
          }
          string.FillStringChecked(dest, destPos, src);
          destPos += length2;
        }
        if (index3 < index2 - 1)
        {
          fixed (char* chPtr = &dest._firstChar)
          {
            if (separatorLength == 1)
              chPtr[destPos] = *separator;
            else
              string.wstrcpy(chPtr + destPos, separator, separatorLength);
          }
          destPos += separatorLength;
        }
      }
      return destPos != length1 ? string.JoinCore(separator, separatorLength, (string[]) value.Clone(), startIndex, count) : dest;
    }

    public string PadLeft(int totalWidth)
    {
      return this.PadLeft(totalWidth, ' ');
    }

    public unsafe string PadLeft(int totalWidth, char paddingChar)
    {
      if (totalWidth < 0)
        throw new ArgumentOutOfRangeException(nameof (totalWidth), SR.ArgumentOutOfRange_NeedNonNegNum);
      int length = this.Length;
      int num = totalWidth - length;
      if (num <= 0)
        return this;
      string str = string.FastAllocateString(totalWidth);
      fixed (char* chPtr = &str._firstChar)
      {
        for (int index = 0; index < num; ++index)
          chPtr[index] = paddingChar;
        fixed (char* smem = &this._firstChar)
          string.wstrcpy(chPtr + num, smem, length);
      }
      return str;
    }

    public string PadRight(int totalWidth)
    {
      return this.PadRight(totalWidth, ' ');
    }

    public unsafe string PadRight(int totalWidth, char paddingChar)
    {
      if (totalWidth < 0)
        throw new ArgumentOutOfRangeException(nameof (totalWidth), SR.ArgumentOutOfRange_NeedNonNegNum);
      int length = this.Length;
      int num = totalWidth - length;
      if (num <= 0)
        return this;
      string str = string.FastAllocateString(totalWidth);
      fixed (char* dmem = &str._firstChar)
      {
        fixed (char* smem = &this._firstChar)
          string.wstrcpy(dmem, smem, length);
        for (int index = 0; index < num; ++index)
          dmem[length + index] = paddingChar;
      }
      return str;
    }

    public unsafe string Remove(int startIndex, int count)
    {
      if (startIndex < 0)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_StartIndex);
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_NegativeCount);
      int length1 = this.Length;
      if (count > length1 - startIndex)
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_IndexCount);
      if (count == 0)
        return this;
      int length2 = length1 - count;
      if (length2 == 0)
        return string.Empty;
      string str = string.FastAllocateString(length2);
      fixed (char* smem = &this._firstChar)
        fixed (char* dmem = &str._firstChar)
        {
          string.wstrcpy(dmem, smem, startIndex);
          string.wstrcpy(dmem + startIndex, smem + startIndex + count, length2 - startIndex);
        }
      return str;
    }

    public string Remove(int startIndex)
    {
      if (startIndex < 0)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_StartIndex);
      if (startIndex >= this.Length)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_StartIndexLessThanLength);
      return this.Substring(0, startIndex);
    }

    public string Replace(string oldValue, [Nullable(2)] string newValue, bool ignoreCase, [Nullable(2)] CultureInfo culture)
    {
      return this.ReplaceCore(oldValue, newValue, culture, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
    }

    public string Replace(string oldValue, [Nullable(2)] string newValue, StringComparison comparisonType)
    {
      switch (comparisonType)
      {
        case StringComparison.CurrentCulture:
        case StringComparison.CurrentCultureIgnoreCase:
          return this.ReplaceCore(oldValue, newValue, CultureInfo.CurrentCulture, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.InvariantCulture:
        case StringComparison.InvariantCultureIgnoreCase:
          return this.ReplaceCore(oldValue, newValue, CultureInfo.InvariantCulture, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.Ordinal:
          return this.Replace(oldValue, newValue);
        case StringComparison.OrdinalIgnoreCase:
          return this.ReplaceCore(oldValue, newValue, CultureInfo.InvariantCulture, CompareOptions.OrdinalIgnoreCase);
        default:
          throw new ArgumentException(SR.NotSupported_StringComparison, nameof (comparisonType));
      }
    }

    private unsafe string ReplaceCore(
      string oldValue,
      string newValue,
      CultureInfo culture,
      CompareOptions options)
    {
      switch (oldValue)
      {
        case "":
          throw new ArgumentException(SR.Argument_StringZeroLength, nameof (oldValue));
        case null:
          throw new ArgumentNullException(nameof (oldValue));
        default:
          if (newValue == null)
            newValue = string.Empty;
          CultureInfo cultureInfo = culture ?? CultureInfo.CurrentCulture;
          StringBuilder sb = StringBuilderCache.Acquire(16);
          int startIndex = 0;
          int num1 = 0;
          bool flag = false;
          CompareInfo compareInfo = cultureInfo.CompareInfo;
          int num2;
          do
          {
            num2 = compareInfo.IndexOf(this, oldValue, startIndex, this.Length - startIndex, options, &num1);
            if (num2 >= 0)
            {
              sb.Append(this, startIndex, num2 - startIndex);
              sb.Append(newValue);
              startIndex = num2 + num1;
              flag = true;
            }
            else
            {
              if (!flag)
              {
                StringBuilderCache.Release(sb);
                return this;
              }
              sb.Append(this, startIndex, this.Length - startIndex);
            }
          }
          while (num2 >= 0);
          return StringBuilderCache.GetStringAndRelease(sb);
      }
    }

    public unsafe string Replace(char oldChar, char newChar)
    {
      if ((int) oldChar == (int) newChar)
        return this;
      int length = this.Length;
      fixed (char* chPtr1 = &this._firstChar)
      {
        for (char* chPtr2 = chPtr1; length > 0 && (int) *chPtr2 != (int) oldChar; ++chPtr2)
          --length;
      }
      if (length == 0)
        return this;
      string str = string.FastAllocateString(this.Length);
      fixed (char* smem = &this._firstChar)
        fixed (char* dmem = &str._firstChar)
        {
          int charCount = this.Length - length;
          if (charCount > 0)
            string.wstrcpy(dmem, smem, charCount);
          char* chPtr1 = smem + charCount;
          char* chPtr2 = dmem + charCount;
          do
          {
            char ch = *chPtr1;
            if ((int) ch == (int) oldChar)
              ch = newChar;
            *chPtr2 = ch;
            --length;
            ++chPtr1;
            ++chPtr2;
          }
          while (length > 0);
        }
      return str;
    }

    public unsafe string Replace(string oldValue, [Nullable(2)] string newValue)
    {
      switch (oldValue)
      {
        case "":
          throw new ArgumentException(SR.Argument_StringZeroLength, nameof (oldValue));
        case null:
          throw new ArgumentNullException(nameof (oldValue));
        default:
          if (newValue == null)
            newValue = string.Empty;
          // ISSUE: untyped stack allocation
          ValueListBuilder<int> valueListBuilder = new ValueListBuilder<int>(new Span<int>((void*) __untypedstackalloc(new IntPtr(512)), 128));
          fixed (char* chPtr1 = &this._firstChar)
          {
            int num1 = 0;
            int num2 = this.Length - oldValue.Length;
label_12:
            while (num1 <= num2)
            {
              char* chPtr2 = chPtr1 + num1;
              for (int index = 0; index < oldValue.Length; ++index)
              {
                if ((int) chPtr2[index] != (int) oldValue[index])
                {
                  ++num1;
                  goto label_12;
                }
              }
              valueListBuilder.Append(num1);
              num1 += oldValue.Length;
            }
          }
          if (valueListBuilder.Length == 0)
            return this;
          string str = this.ReplaceHelper(oldValue.Length, newValue, valueListBuilder.AsSpan());
          valueListBuilder.Dispose();
          return str;
      }
    }

    private string ReplaceHelper(int oldValueLength, string newValue, ReadOnlySpan<int> indices)
    {
      long num1 = (long) this.Length + (long) (newValue.Length - oldValueLength) * (long) indices.Length;
      if (num1 > (long) int.MaxValue)
        throw new OutOfMemoryException();
      string str = string.FastAllocateString((int) num1);
      Span<char> span = new Span<char>(ref str.GetRawStringData(), str.Length);
      int start1 = 0;
      int start2 = 0;
      for (int index = 0; index < indices.Length; ++index)
      {
        int num2 = indices[index];
        int length = num2 - start1;
        if (length != 0)
        {
          this.AsSpan(start1, length).CopyTo(span.Slice(start2));
          start2 += length;
        }
        start1 = num2 + oldValueLength;
        newValue.AsSpan().CopyTo(span.Slice(start2));
        start2 += newValue.Length;
      }
      this.AsSpan(start1).CopyTo(span.Slice(start2));
      return str;
    }

    public string[] Split(char separator, StringSplitOptions options = StringSplitOptions.None)
    {
      return this.SplitInternal(new ReadOnlySpan<char>(ref separator, 1), int.MaxValue, options);
    }

    public string[] Split(char separator, int count, StringSplitOptions options = StringSplitOptions.None)
    {
      return this.SplitInternal(new ReadOnlySpan<char>(ref separator, 1), count, options);
    }

    public string[] Split([Nullable(2)] params char[] separator)
    {
      return this.SplitInternal((ReadOnlySpan<char>) separator, int.MaxValue, StringSplitOptions.None);
    }

    public string[] Split([Nullable(2)] char[] separator, int count)
    {
      return this.SplitInternal((ReadOnlySpan<char>) separator, count, StringSplitOptions.None);
    }

    public string[] Split([Nullable(2)] char[] separator, StringSplitOptions options)
    {
      return this.SplitInternal((ReadOnlySpan<char>) separator, int.MaxValue, options);
    }

    public string[] Split([Nullable(2)] char[] separator, int count, StringSplitOptions options)
    {
      return this.SplitInternal((ReadOnlySpan<char>) separator, count, options);
    }

    private unsafe string[] SplitInternal(
      ReadOnlySpan<char> separators,
      int count,
      StringSplitOptions options)
    {
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_NegativeCount);
      if (options < StringSplitOptions.None || options > StringSplitOptions.RemoveEmptyEntries)
        throw new ArgumentException(SR.Format(SR.Arg_EnumIllegalVal, (object) options));
      bool flag = options == StringSplitOptions.RemoveEmptyEntries;
      if (count == 0 || flag && this.Length == 0)
        return Array.Empty<string>();
      if (count == 1)
        return new string[1]{ this };
      // ISSUE: untyped stack allocation
      ValueListBuilder<int> sepListBuilder = new ValueListBuilder<int>(new Span<int>((void*) __untypedstackalloc(new IntPtr(512)), 128));
      this.MakeSeparatorList(separators, ref sepListBuilder);
      ReadOnlySpan<int> sepList = sepListBuilder.AsSpan();
      if (sepList.Length == 0)
        return new string[1]{ this };
      string[] strArray = flag ? this.SplitOmitEmptyEntries(sepList, new ReadOnlySpan<int>(), 1, count) : this.SplitKeepEmptyEntries(sepList, new ReadOnlySpan<int>(), 1, count);
      sepListBuilder.Dispose();
      return strArray;
    }

    public string[] Split([Nullable(2)] string separator, StringSplitOptions options = StringSplitOptions.None)
    {
      return this.SplitInternal(separator ?? string.Empty, (string[]) null, int.MaxValue, options);
    }

    public string[] Split([Nullable(2)] string separator, int count, StringSplitOptions options = StringSplitOptions.None)
    {
      return this.SplitInternal(separator ?? string.Empty, (string[]) null, count, options);
    }

    public string[] Split([Nullable(new byte[] {2, 1})] string[] separator, StringSplitOptions options)
    {
      return this.SplitInternal((string) null, separator, int.MaxValue, options);
    }

    public string[] Split([Nullable(new byte[] {2, 1})] string[] separator, int count, StringSplitOptions options)
    {
      return this.SplitInternal((string) null, separator, count, options);
    }

    private unsafe string[] SplitInternal(
      string separator,
      string[] separators,
      int count,
      StringSplitOptions options)
    {
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_NegativeCount);
      if (options < StringSplitOptions.None || options > StringSplitOptions.RemoveEmptyEntries)
        throw new ArgumentException(SR.Format(SR.Arg_EnumIllegalVal, (object) (int) options));
      bool flag1 = options == StringSplitOptions.RemoveEmptyEntries;
      bool flag2 = separator != null;
      if (!flag2 && (separators == null || separators.Length == 0))
        return this.SplitInternal(new ReadOnlySpan<char>(), count, options);
      if (count == 0 || flag1 && this.Length == 0)
        return Array.Empty<string>();
      if (count == 1 || flag2 && separator.Length == 0)
        return new string[1]{ this };
      if (flag2)
        return this.SplitInternal(separator, count, options);
      // ISSUE: untyped stack allocation
      ValueListBuilder<int> sepListBuilder = new ValueListBuilder<int>(new Span<int>((void*) __untypedstackalloc(new IntPtr(512)), 128));
      // ISSUE: untyped stack allocation
      ValueListBuilder<int> lengthListBuilder = new ValueListBuilder<int>(new Span<int>((void*) __untypedstackalloc(new IntPtr(512)), 128));
      this.MakeSeparatorList(separators, ref sepListBuilder, ref lengthListBuilder);
      ReadOnlySpan<int> sepList = sepListBuilder.AsSpan();
      ReadOnlySpan<int> lengthList = lengthListBuilder.AsSpan();
      if (sepList.Length == 0)
        return new string[1]{ this };
      string[] strArray = flag1 ? this.SplitOmitEmptyEntries(sepList, lengthList, 0, count) : this.SplitKeepEmptyEntries(sepList, lengthList, 0, count);
      sepListBuilder.Dispose();
      lengthListBuilder.Dispose();
      return strArray;
    }

    private unsafe string[] SplitInternal(string separator, int count, StringSplitOptions options)
    {
      // ISSUE: untyped stack allocation
      ValueListBuilder<int> sepListBuilder = new ValueListBuilder<int>(new Span<int>((void*) __untypedstackalloc(new IntPtr(512)), 128));
      this.MakeSeparatorList(separator, ref sepListBuilder);
      ReadOnlySpan<int> sepList = sepListBuilder.AsSpan();
      if (sepList.Length == 0)
        return new string[1]{ this };
      string[] strArray = options == StringSplitOptions.RemoveEmptyEntries ? this.SplitOmitEmptyEntries(sepList, new ReadOnlySpan<int>(), separator.Length, count) : this.SplitKeepEmptyEntries(sepList, new ReadOnlySpan<int>(), separator.Length, count);
      sepListBuilder.Dispose();
      return strArray;
    }

    private string[] SplitKeepEmptyEntries(
      ReadOnlySpan<int> sepList,
      ReadOnlySpan<int> lengthList,
      int defaultLength,
      int count)
    {
      int startIndex = 0;
      int index1 = 0;
      --count;
      int num = sepList.Length < count ? sepList.Length : count;
      string[] strArray = new string[num + 1];
      for (int index2 = 0; index2 < num && startIndex < this.Length; ++index2)
      {
        strArray[index1++] = this.Substring(startIndex, sepList[index2] - startIndex);
        startIndex = sepList[index2] + (lengthList.IsEmpty ? defaultLength : lengthList[index2]);
      }
      if (startIndex < this.Length && num >= 0)
        strArray[index1] = this.Substring(startIndex);
      else if (index1 == num)
        strArray[index1] = string.Empty;
      return strArray;
    }

    private string[] SplitOmitEmptyEntries(
      ReadOnlySpan<int> sepList,
      ReadOnlySpan<int> lengthList,
      int defaultLength,
      int count)
    {
      int length1 = sepList.Length;
      int length2 = length1 < count ? length1 + 1 : count;
      string[] strArray1 = new string[length2];
      int startIndex = 0;
      int length3 = 0;
      for (int index = 0; index < length1 && startIndex < this.Length; ++index)
      {
        if (sepList[index] - startIndex > 0)
          strArray1[length3++] = this.Substring(startIndex, sepList[index] - startIndex);
        startIndex = sepList[index] + (lengthList.IsEmpty ? defaultLength : lengthList[index]);
        if (length3 == count - 1)
        {
          while (index < length1 - 1 && startIndex == sepList[++index])
            startIndex += lengthList.IsEmpty ? defaultLength : lengthList[index];
          break;
        }
      }
      if (startIndex < this.Length)
        strArray1[length3++] = this.Substring(startIndex);
      string[] strArray2 = strArray1;
      if (length3 != length2)
      {
        strArray2 = new string[length3];
        for (int index = 0; index < length3; ++index)
          strArray2[index] = strArray1[index];
      }
      return strArray2;
    }

    private unsafe void MakeSeparatorList(
      ReadOnlySpan<char> separators,
      ref ValueListBuilder<int> sepListBuilder)
    {
      switch (separators.Length)
      {
        case 0:
          for (int index = 0; index < this.Length; ++index)
          {
            if (char.IsWhiteSpace(this[index]))
              sepListBuilder.Append(index);
          }
          break;
        case 1:
          char ch1 = separators[0];
          for (int index = 0; index < this.Length; ++index)
          {
            if ((int) this[index] == (int) ch1)
              sepListBuilder.Append(index);
          }
          break;
        case 2:
          char ch2 = separators[0];
          char ch3 = separators[1];
          for (int index = 0; index < this.Length; ++index)
          {
            char ch4 = this[index];
            if ((int) ch4 == (int) ch2 || (int) ch4 == (int) ch3)
              sepListBuilder.Append(index);
          }
          break;
        case 3:
          char ch5 = separators[0];
          char ch6 = separators[1];
          char ch7 = separators[2];
          for (int index = 0; index < this.Length; ++index)
          {
            char ch4 = this[index];
            if ((int) ch4 == (int) ch5 || (int) ch4 == (int) ch6 || (int) ch4 == (int) ch7)
              sepListBuilder.Append(index);
          }
          break;
        default:
          uint* charMap = (uint*) &new String.ProbabilisticMap();
          string.InitializeProbabilisticMap(charMap, separators);
          for (int index = 0; index < this.Length; ++index)
          {
            char ch4 = this[index];
            if (string.IsCharBitSet(charMap, (byte) ch4) && string.IsCharBitSet(charMap, (byte) ((uint) ch4 >> 8)) && separators.Contains<char>(ch4))
              sepListBuilder.Append(index);
          }
          break;
      }
    }

    private void MakeSeparatorList(string separator, ref ValueListBuilder<int> sepListBuilder)
    {
      int length = separator.Length;
      for (int start = 0; start < this.Length; ++start)
      {
        if ((int) this[start] == (int) separator[0] && length <= this.Length - start && (length == 1 || this.AsSpan(start, length).SequenceEqual<char>((ReadOnlySpan<char>) separator)))
        {
          sepListBuilder.Append(start);
          start += length - 1;
        }
      }
    }

    private void MakeSeparatorList(
      string[] separators,
      ref ValueListBuilder<int> sepListBuilder,
      ref ValueListBuilder<int> lengthListBuilder)
    {
      int length1 = separators.Length;
      for (int start = 0; start < this.Length; ++start)
      {
        for (int index = 0; index < separators.Length; ++index)
        {
          string separator = separators[index];
          if (!string.IsNullOrEmpty(separator))
          {
            int length2 = separator.Length;
            if ((int) this[start] == (int) separator[0] && length2 <= this.Length - start && (length2 == 1 || this.AsSpan(start, length2).SequenceEqual<char>((ReadOnlySpan<char>) separator)))
            {
              sepListBuilder.Append(start);
              lengthListBuilder.Append(length2);
              start += length2 - 1;
              break;
            }
          }
        }
      }
    }

    public string Substring(int startIndex)
    {
      return this.Substring(startIndex, this.Length - startIndex);
    }

    public string Substring(int startIndex, int length)
    {
      if (startIndex < 0)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_StartIndex);
      if (startIndex > this.Length)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_StartIndexLargerThanLength);
      if (length < 0)
        throw new ArgumentOutOfRangeException(nameof (length), SR.ArgumentOutOfRange_NegativeLength);
      if (startIndex > this.Length - length)
        throw new ArgumentOutOfRangeException(nameof (length), SR.ArgumentOutOfRange_IndexLength);
      if (length == 0)
        return string.Empty;
      return startIndex == 0 && length == this.Length ? this : this.InternalSubString(startIndex, length);
    }

    private unsafe string InternalSubString(int startIndex, int length)
    {
      string str = string.FastAllocateString(length);
      fixed (char* dmem = &str._firstChar)
        fixed (char* chPtr = &this._firstChar)
          string.wstrcpy(dmem, chPtr + startIndex, length);
      return str;
    }

    public string ToLower()
    {
      return this.ToLower((CultureInfo) null);
    }

    public string ToLower([Nullable(2)] CultureInfo culture)
    {
      return (culture ?? CultureInfo.CurrentCulture).TextInfo.ToLower(this);
    }

    public string ToLowerInvariant()
    {
      return CultureInfo.InvariantCulture.TextInfo.ToLower(this);
    }

    public string ToUpper()
    {
      return this.ToUpper((CultureInfo) null);
    }

    public string ToUpper([Nullable(2)] CultureInfo culture)
    {
      return (culture ?? CultureInfo.CurrentCulture).TextInfo.ToUpper(this);
    }

    public string ToUpperInvariant()
    {
      return CultureInfo.InvariantCulture.TextInfo.ToUpper(this);
    }

    public string Trim()
    {
      return this.TrimWhiteSpaceHelper(String.TrimType.Both);
    }

    public unsafe string Trim(char trimChar)
    {
      return this.TrimHelper(&trimChar, 1, String.TrimType.Both);
    }

    public unsafe string Trim([Nullable(2)] params char[] trimChars)
    {
      if (trimChars == null || trimChars.Length == 0)
        return this.TrimWhiteSpaceHelper(String.TrimType.Both);
      fixed (char* trimChars1 = &trimChars[0])
        return this.TrimHelper(trimChars1, trimChars.Length, String.TrimType.Both);
    }

    public string TrimStart()
    {
      return this.TrimWhiteSpaceHelper(String.TrimType.Head);
    }

    public unsafe string TrimStart(char trimChar)
    {
      return this.TrimHelper(&trimChar, 1, String.TrimType.Head);
    }

    public unsafe string TrimStart([Nullable(2)] params char[] trimChars)
    {
      if (trimChars == null || trimChars.Length == 0)
        return this.TrimWhiteSpaceHelper(String.TrimType.Head);
      fixed (char* trimChars1 = &trimChars[0])
        return this.TrimHelper(trimChars1, trimChars.Length, String.TrimType.Head);
    }

    public string TrimEnd()
    {
      return this.TrimWhiteSpaceHelper(String.TrimType.Tail);
    }

    public unsafe string TrimEnd(char trimChar)
    {
      return this.TrimHelper(&trimChar, 1, String.TrimType.Tail);
    }

    public unsafe string TrimEnd([Nullable(2)] params char[] trimChars)
    {
      if (trimChars == null || trimChars.Length == 0)
        return this.TrimWhiteSpaceHelper(String.TrimType.Tail);
      fixed (char* trimChars1 = &trimChars[0])
        return this.TrimHelper(trimChars1, trimChars.Length, String.TrimType.Tail);
    }

    private string TrimWhiteSpaceHelper(String.TrimType trimType)
    {
      int end = this.Length - 1;
      int start = 0;
      if (trimType != String.TrimType.Tail)
      {
        start = 0;
        while (start < this.Length && char.IsWhiteSpace(this[start]))
          ++start;
      }
      if (trimType != String.TrimType.Head)
      {
        end = this.Length - 1;
        while (end >= start && char.IsWhiteSpace(this[end]))
          --end;
      }
      return this.CreateTrimmedString(start, end);
    }

    private unsafe string TrimHelper(
      char* trimChars,
      int trimCharsLength,
      String.TrimType trimType)
    {
      int end = this.Length - 1;
      int start = 0;
      if (trimType != String.TrimType.Tail)
      {
        for (start = 0; start < this.Length; ++start)
        {
          char ch = this[start];
          int index = 0;
          while (index < trimCharsLength && (int) trimChars[index] != (int) ch)
            ++index;
          if (index == trimCharsLength)
            break;
        }
      }
      if (trimType != String.TrimType.Head)
      {
        for (end = this.Length - 1; end >= start; --end)
        {
          char ch = this[end];
          int index = 0;
          while (index < trimCharsLength && (int) trimChars[index] != (int) ch)
            ++index;
          if (index == trimCharsLength)
            break;
        }
      }
      return this.CreateTrimmedString(start, end);
    }

    private string CreateTrimmedString(int start, int end)
    {
      int length = end - start + 1;
      if (length == this.Length)
        return this;
      return length != 0 ? this.InternalSubString(start, length) : string.Empty;
    }

    public bool Contains(string value)
    {
      if (value == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
      return SpanHelpers.IndexOf(ref this._firstChar, this.Length, ref value._firstChar, value.Length) >= 0;
    }

    public bool Contains(string value, StringComparison comparisonType)
    {
      return this.IndexOf(value, comparisonType) >= 0;
    }

    public bool Contains(char value)
    {
      return SpanHelpers.Contains(ref this._firstChar, value, this.Length);
    }

    public bool Contains(char value, StringComparison comparisonType)
    {
      return this.IndexOf(value, comparisonType) != -1;
    }

    public int IndexOf(char value)
    {
      return SpanHelpers.IndexOf(ref this._firstChar, value, this.Length);
    }

    public int IndexOf(char value, int startIndex)
    {
      return this.IndexOf(value, startIndex, this.Length - startIndex);
    }

    public int IndexOf(char value, StringComparison comparisonType)
    {
      switch (comparisonType)
      {
        case StringComparison.CurrentCulture:
        case StringComparison.CurrentCultureIgnoreCase:
          return CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.InvariantCulture:
        case StringComparison.InvariantCultureIgnoreCase:
          return CompareInfo.Invariant.IndexOf(this, value, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.Ordinal:
          return this.IndexOf(value);
        case StringComparison.OrdinalIgnoreCase:
          return CompareInfo.Invariant.IndexOf(this, value, CompareOptions.OrdinalIgnoreCase);
        default:
          throw new ArgumentException(SR.NotSupported_StringComparison, nameof (comparisonType));
      }
    }

    public int IndexOf(char value, int startIndex, int count)
    {
      if ((uint) startIndex > (uint) this.Length)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_Index);
      if ((uint) count > (uint) (this.Length - startIndex))
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_Count);
      int num = SpanHelpers.IndexOf(ref Unsafe.Add<char>(ref this._firstChar, startIndex), value, count);
      return num != -1 ? num + startIndex : num;
    }

    public int IndexOfAny(char[] anyOf)
    {
      return this.IndexOfAny(anyOf, 0, this.Length);
    }

    public int IndexOfAny(char[] anyOf, int startIndex)
    {
      return this.IndexOfAny(anyOf, startIndex, this.Length - startIndex);
    }

    public int IndexOfAny(char[] anyOf, int startIndex, int count)
    {
      if (anyOf == null)
        throw new ArgumentNullException(nameof (anyOf));
      if ((uint) startIndex > (uint) this.Length)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_Index);
      if ((uint) count > (uint) (this.Length - startIndex))
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_Count);
      if (anyOf.Length != 0 && anyOf.Length <= 5)
      {
        int num = new ReadOnlySpan<char>(ref Unsafe.Add<char>(ref this._firstChar, startIndex), count).IndexOfAny<char>((ReadOnlySpan<char>) anyOf);
        return num != -1 ? num + startIndex : num;
      }
      return anyOf.Length > 5 ? this.IndexOfCharArray(anyOf, startIndex, count) : -1;
    }

    private unsafe int IndexOfCharArray(char[] anyOf, int startIndex, int count)
    {
      uint* charMap = (uint*) &new String.ProbabilisticMap();
      string.InitializeProbabilisticMap(charMap, (ReadOnlySpan<char>) anyOf);
      fixed (char* chPtr1 = &this._firstChar)
      {
        char* chPtr2 = chPtr1 + startIndex;
        while (count > 0)
        {
          int num = (int) *chPtr2;
          if (string.IsCharBitSet(charMap, (byte) num) && string.IsCharBitSet(charMap, (byte) (num >> 8)) && string.ArrayContains((char) num, anyOf))
            return (int) (chPtr2 - chPtr1);
          --count;
          ++chPtr2;
        }
        return -1;
      }
    }

    private static unsafe void InitializeProbabilisticMap(uint* charMap, ReadOnlySpan<char> anyOf)
    {
      bool flag = false;
      uint* charMap1 = charMap;
      for (int index = 0; index < anyOf.Length; ++index)
      {
        int num1 = (int) anyOf[index];
        string.SetCharBit(charMap1, (byte) num1);
        int num2 = num1 >> 8;
        if (num2 == 0)
          flag = true;
        else
          string.SetCharBit(charMap1, (byte) num2);
      }
      if (!flag)
        return;
      uint* numPtr = charMap1;
      int num = (int) *numPtr | 1;
      *numPtr = (uint) num;
    }

    private static bool ArrayContains(char searchChar, char[] anyOf)
    {
      for (int index = 0; index < anyOf.Length; ++index)
      {
        if ((int) anyOf[index] == (int) searchChar)
          return true;
      }
      return false;
    }

    private static unsafe bool IsCharBitSet(uint* charMap, byte value)
    {
      return (charMap[(int) value & 7] & (uint) (1 << ((int) value >> 3))) > 0U;
    }

    private static unsafe void SetCharBit(uint* charMap, byte value)
    {
      uint* numPtr = charMap + ((int) value & 7);
      *numPtr = *numPtr | (uint) (1 << ((int) value >> 3));
    }

    public int IndexOf(string value)
    {
      return this.IndexOf(value, StringComparison.CurrentCulture);
    }

    public int IndexOf(string value, int startIndex)
    {
      return this.IndexOf(value, startIndex, StringComparison.CurrentCulture);
    }

    public int IndexOf(string value, int startIndex, int count)
    {
      if (startIndex < 0 || startIndex > this.Length)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_Index);
      if (count < 0 || count > this.Length - startIndex)
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_Count);
      return this.IndexOf(value, startIndex, count, StringComparison.CurrentCulture);
    }

    public int IndexOf(string value, StringComparison comparisonType)
    {
      return this.IndexOf(value, 0, this.Length, comparisonType);
    }

    public int IndexOf(string value, int startIndex, StringComparison comparisonType)
    {
      return this.IndexOf(value, startIndex, this.Length - startIndex, comparisonType);
    }

    public int IndexOf(string value, int startIndex, int count, StringComparison comparisonType)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      if (startIndex < 0 || startIndex > this.Length)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_Index);
      if (count < 0 || startIndex > this.Length - count)
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_Count);
      if (comparisonType == StringComparison.Ordinal)
      {
        int num = SpanHelpers.IndexOf(ref Unsafe.Add<char>(ref this._firstChar, startIndex), count, ref value._firstChar, value.Length);
        return (num >= 0 ? startIndex : 0) + num;
      }
      switch (comparisonType)
      {
        case StringComparison.CurrentCulture:
        case StringComparison.CurrentCultureIgnoreCase:
          return CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, startIndex, count, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.InvariantCulture:
        case StringComparison.InvariantCultureIgnoreCase:
          return CompareInfo.Invariant.IndexOf(this, value, startIndex, count, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.OrdinalIgnoreCase:
          return CompareInfo.Invariant.IndexOfOrdinal(this, value, startIndex, count, (uint) string.GetCaseCompareOfComparisonCulture(comparisonType) > 0U);
        default:
          throw new ArgumentException(SR.NotSupported_StringComparison, nameof (comparisonType));
      }
    }

    public int LastIndexOf(char value)
    {
      return SpanHelpers.LastIndexOf(ref this._firstChar, value, this.Length);
    }

    public int LastIndexOf(char value, int startIndex)
    {
      return this.LastIndexOf(value, startIndex, startIndex + 1);
    }

    public int LastIndexOf(char value, int startIndex, int count)
    {
      if (this.Length == 0)
        return -1;
      if ((uint) startIndex >= (uint) this.Length)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_Index);
      if ((uint) count > (uint) (startIndex + 1))
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_Count);
      int elementOffset = startIndex + 1 - count;
      int num = SpanHelpers.LastIndexOf(ref Unsafe.Add<char>(ref this._firstChar, elementOffset), value, count);
      return num != -1 ? num + elementOffset : num;
    }

    public int LastIndexOfAny(char[] anyOf)
    {
      return this.LastIndexOfAny(anyOf, this.Length - 1, this.Length);
    }

    public int LastIndexOfAny(char[] anyOf, int startIndex)
    {
      return this.LastIndexOfAny(anyOf, startIndex, startIndex + 1);
    }

    public int LastIndexOfAny(char[] anyOf, int startIndex, int count)
    {
      if (anyOf == null)
        throw new ArgumentNullException(nameof (anyOf));
      if (this.Length == 0)
        return -1;
      if ((uint) startIndex >= (uint) this.Length)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_Index);
      if (count < 0 || count - 1 > startIndex)
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_Count);
      if (anyOf.Length > 1)
        return this.LastIndexOfCharArray(anyOf, startIndex, count);
      return anyOf.Length == 1 ? this.LastIndexOf(anyOf[0], startIndex, count) : -1;
    }

    private unsafe int LastIndexOfCharArray(char[] anyOf, int startIndex, int count)
    {
      uint* charMap = (uint*) &new String.ProbabilisticMap();
      string.InitializeProbabilisticMap(charMap, (ReadOnlySpan<char>) anyOf);
      fixed (char* chPtr1 = &this._firstChar)
      {
        char* chPtr2 = chPtr1 + startIndex;
        while (count > 0)
        {
          int num = (int) *chPtr2;
          if (string.IsCharBitSet(charMap, (byte) num) && string.IsCharBitSet(charMap, (byte) (num >> 8)) && string.ArrayContains((char) num, anyOf))
            return (int) (chPtr2 - chPtr1);
          --count;
          --chPtr2;
        }
        return -1;
      }
    }

    public int LastIndexOf(string value)
    {
      return this.LastIndexOf(value, this.Length - 1, this.Length, StringComparison.CurrentCulture);
    }

    public int LastIndexOf(string value, int startIndex)
    {
      return this.LastIndexOf(value, startIndex, startIndex + 1, StringComparison.CurrentCulture);
    }

    public int LastIndexOf(string value, int startIndex, int count)
    {
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_Count);
      return this.LastIndexOf(value, startIndex, count, StringComparison.CurrentCulture);
    }

    public int LastIndexOf(string value, StringComparison comparisonType)
    {
      return this.LastIndexOf(value, this.Length - 1, this.Length, comparisonType);
    }

    public int LastIndexOf(string value, int startIndex, StringComparison comparisonType)
    {
      return this.LastIndexOf(value, startIndex, startIndex + 1, comparisonType);
    }

    public int LastIndexOf(
      string value,
      int startIndex,
      int count,
      StringComparison comparisonType)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      if (this.Length == 0 && (startIndex == -1 || startIndex == 0))
        return value.Length != 0 ? -1 : 0;
      if (startIndex < 0 || startIndex > this.Length)
        throw new ArgumentOutOfRangeException(nameof (startIndex), SR.ArgumentOutOfRange_Index);
      if (startIndex == this.Length)
      {
        --startIndex;
        if (count > 0)
          --count;
      }
      if (count < 0 || startIndex - count + 1 < 0)
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_Count);
      if (value.Length == 0)
        return startIndex;
      switch (comparisonType)
      {
        case StringComparison.CurrentCulture:
        case StringComparison.CurrentCultureIgnoreCase:
          return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.InvariantCulture:
        case StringComparison.InvariantCultureIgnoreCase:
          return CompareInfo.Invariant.LastIndexOf(this, value, startIndex, count, string.GetCaseCompareOfComparisonCulture(comparisonType));
        case StringComparison.Ordinal:
        case StringComparison.OrdinalIgnoreCase:
          return CompareInfo.Invariant.LastIndexOfOrdinal(this, value, startIndex, count, (uint) string.GetCaseCompareOfComparisonCulture(comparisonType) > 0U);
        default:
          throw new ArgumentException(SR.NotSupported_StringComparison, nameof (comparisonType));
      }
    }

    private enum TrimType
    {
      Head,
      Tail,
      Both,
    }

    [StructLayout(LayoutKind.Explicit, Size = 32)]
    private struct ProbabilisticMap
    {
    }
  }
}

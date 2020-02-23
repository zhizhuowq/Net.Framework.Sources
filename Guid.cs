// Decompiled with JetBrains decompiler
// Type: System.Guid
// Assembly: System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// MVID: BBB3B884-123D-47EA-9CD1-5BED540D02AE
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Private.CoreLib.dll

using Internal.Runtime.CompilerServices;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace System
{
  [TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
  [NonVersionable]
  [Serializable]
  public struct Guid : IFormattable, IComparable, IComparable<Guid>, IEquatable<Guid>, ISpanFormattable
  {
    public static readonly Guid Empty;
    private int _a;
    private short _b;
    private short _c;
    private byte _d;
    private byte _e;
    private byte _f;
    private byte _g;
    private byte _h;
    private byte _i;
    private byte _j;
    private byte _k;

    [NullableContext(1)]
    public Guid(byte[] b)
    {
      byte[] array = b;
      if (array == null)
        throw new ArgumentNullException(nameof (b));
      this = new Guid(new ReadOnlySpan<byte>(array));
    }

    public Guid(ReadOnlySpan<byte> b)
    {
      if (b.Length != 16)
        throw new ArgumentException(SR.Format(SR.Arg_GuidArrayCtor, (object) "16"), nameof (b));
      if (BitConverter.IsLittleEndian)
      {
        this = MemoryMarshal.Read<Guid>(b);
      }
      else
      {
        this._k = b[15];
        this._a = (int) b[3] << 24 | (int) b[2] << 16 | (int) b[1] << 8 | (int) b[0];
        this._b = (short) ((int) b[5] << 8 | (int) b[4]);
        this._c = (short) ((int) b[7] << 8 | (int) b[6]);
        this._d = b[8];
        this._e = b[9];
        this._f = b[10];
        this._g = b[11];
        this._h = b[12];
        this._i = b[13];
        this._j = b[14];
      }
    }

    [CLSCompliant(false)]
    public Guid(
      uint a,
      ushort b,
      ushort c,
      byte d,
      byte e,
      byte f,
      byte g,
      byte h,
      byte i,
      byte j,
      byte k)
    {
      this._a = (int) a;
      this._b = (short) b;
      this._c = (short) c;
      this._d = d;
      this._e = e;
      this._f = f;
      this._g = g;
      this._h = h;
      this._i = i;
      this._j = j;
      this._k = k;
    }

    [NullableContext(1)]
    public Guid(int a, short b, short c, byte[] d)
    {
      if (d == null)
        throw new ArgumentNullException(nameof (d));
      if (d.Length != 8)
        throw new ArgumentException(SR.Format(SR.Arg_GuidArrayCtor, (object) "8"), nameof (d));
      this._a = a;
      this._b = b;
      this._c = c;
      this._k = d[7];
      this._d = d[0];
      this._e = d[1];
      this._f = d[2];
      this._g = d[3];
      this._h = d[4];
      this._i = d[5];
      this._j = d[6];
    }

    public Guid(
      int a,
      short b,
      short c,
      byte d,
      byte e,
      byte f,
      byte g,
      byte h,
      byte i,
      byte j,
      byte k)
    {
      this._a = a;
      this._b = b;
      this._c = c;
      this._d = d;
      this._e = e;
      this._f = f;
      this._g = g;
      this._h = h;
      this._i = i;
      this._j = j;
      this._k = k;
    }

    [NullableContext(1)]
    public Guid(string g)
    {
      if (g == null)
        throw new ArgumentNullException(nameof (g));
      Guid.GuidResult result = new Guid.GuidResult(Guid.GuidParseThrowStyle.All);
      Guid.TryParseGuid((ReadOnlySpan<char>) g, ref result);
      this = result._parsedGuid;
    }

    [NullableContext(1)]
    public static Guid Parse(string input)
    {
      if (input == null)
        throw new ArgumentNullException(nameof (input));
      return Guid.Parse((ReadOnlySpan<char>) input);
    }

    public static Guid Parse(ReadOnlySpan<char> input)
    {
      Guid.GuidResult result = new Guid.GuidResult(Guid.GuidParseThrowStyle.AllButOverflow);
      Guid.TryParseGuid(input, ref result);
      return result._parsedGuid;
    }

    [NullableContext(2)]
    public static bool TryParse(string input, out Guid result)
    {
      if (input != null)
        return Guid.TryParse((ReadOnlySpan<char>) input, out result);
      result = new Guid();
      return false;
    }

    public static bool TryParse(ReadOnlySpan<char> input, out Guid result)
    {
      Guid.GuidResult result1 = new Guid.GuidResult(Guid.GuidParseThrowStyle.None);
      if (Guid.TryParseGuid(input, ref result1))
      {
        result = result1._parsedGuid;
        return true;
      }
      result = new Guid();
      return false;
    }

    [NullableContext(1)]
    public static Guid ParseExact(string input, string format)
    {
      if (input == null)
        throw new ArgumentNullException(nameof (input));
      ReadOnlySpan<char> input1 = (ReadOnlySpan<char>) input;
      if (format == null)
        throw new ArgumentNullException(nameof (format));
      ReadOnlySpan<char> format1 = (ReadOnlySpan<char>) format;
      return Guid.ParseExact(input1, format1);
    }

    public static Guid ParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format)
    {
      if (format.Length != 1)
        throw new FormatException(SR.Format_InvalidGuidFormatSpecification);
      input = input.Trim();
      Guid.GuidResult result = new Guid.GuidResult(Guid.GuidParseThrowStyle.AllButOverflow);
      bool flag;
      switch ((char) ((uint) format[0] | 32U))
      {
        case 'b':
          flag = Guid.TryParseExactB(input, ref result);
          break;
        case 'd':
          flag = Guid.TryParseExactD(input, ref result);
          break;
        case 'n':
          flag = Guid.TryParseExactN(input, ref result);
          break;
        case 'p':
          flag = Guid.TryParseExactP(input, ref result);
          break;
        case 'x':
          flag = Guid.TryParseExactX(input, ref result);
          break;
        default:
          throw new FormatException(SR.Format_InvalidGuidFormatSpecification);
      }
      return result._parsedGuid;
    }

    [NullableContext(2)]
    public static bool TryParseExact(string input, string format, out Guid result)
    {
      if (input != null)
        return Guid.TryParseExact((ReadOnlySpan<char>) input, (ReadOnlySpan<char>) format, out result);
      result = new Guid();
      return false;
    }

    public static bool TryParseExact(
      ReadOnlySpan<char> input,
      ReadOnlySpan<char> format,
      out Guid result)
    {
      if (format.Length != 1)
      {
        result = new Guid();
        return false;
      }
      input = input.Trim();
      Guid.GuidResult result1 = new Guid.GuidResult(Guid.GuidParseThrowStyle.None);
      bool flag = false;
      switch ((char) ((uint) format[0] | 32U))
      {
        case 'b':
          flag = Guid.TryParseExactB(input, ref result1);
          break;
        case 'd':
          flag = Guid.TryParseExactD(input, ref result1);
          break;
        case 'n':
          flag = Guid.TryParseExactN(input, ref result1);
          break;
        case 'p':
          flag = Guid.TryParseExactP(input, ref result1);
          break;
        case 'x':
          flag = Guid.TryParseExactX(input, ref result1);
          break;
      }
      if (flag)
      {
        result = result1._parsedGuid;
        return true;
      }
      result = new Guid();
      return false;
    }

    private static bool TryParseGuid(ReadOnlySpan<char> guidString, ref Guid.GuidResult result)
    {
      guidString = guidString.Trim();
      if (guidString.Length == 0)
      {
        result.SetFailure(false, "Format_GuidUnrecognized");
        return false;
      }
      switch (guidString[0])
      {
        case '(':
          return Guid.TryParseExactP(guidString, ref result);
        case '{':
          return !guidString.Contains<char>('-') ? Guid.TryParseExactX(guidString, ref result) : Guid.TryParseExactB(guidString, ref result);
        default:
          return !guidString.Contains<char>('-') ? Guid.TryParseExactN(guidString, ref result) : Guid.TryParseExactD(guidString, ref result);
      }
    }

    private static bool TryParseExactB(ReadOnlySpan<char> guidString, ref Guid.GuidResult result)
    {
      if (guidString.Length == 38 && guidString[0] == '{' && guidString[37] == '}')
        return Guid.TryParseExactD(guidString.Slice(1, 36), ref result);
      result.SetFailure(false, "Format_GuidInvLen");
      return false;
    }

    private static bool TryParseExactD(ReadOnlySpan<char> guidString, ref Guid.GuidResult result)
    {
      if (guidString.Length != 36)
      {
        result.SetFailure(false, "Format_GuidInvLen");
        return false;
      }
      if (guidString[8] != '-' || guidString[13] != '-' || (guidString[18] != '-' || guidString[23] != '-'))
      {
        result.SetFailure(false, "Format_GuidDashes");
        return false;
      }
      ref Guid local = ref result._parsedGuid;
      uint result1;
      if (Guid.TryParseHex(guidString.Slice(0, 8), out Unsafe.As<int, uint>(ref local._a)) && Guid.TryParseHex(guidString.Slice(9, 4), out result1))
      {
        local._b = (short) result1;
        if (Guid.TryParseHex(guidString.Slice(14, 4), out result1))
        {
          local._c = (short) result1;
          if (Guid.TryParseHex(guidString.Slice(19, 4), out result1))
          {
            local._d = (byte) (result1 >> 8);
            local._e = (byte) result1;
            if (Guid.TryParseHex(guidString.Slice(24, 4), out result1))
            {
              local._f = (byte) (result1 >> 8);
              local._g = (byte) result1;
              if (uint.TryParse(guidString.Slice(28, 8), NumberStyles.AllowHexSpecifier, (IFormatProvider) null, out result1))
              {
                local._h = (byte) (result1 >> 24);
                local._i = (byte) (result1 >> 16);
                local._j = (byte) (result1 >> 8);
                local._k = (byte) result1;
                return true;
              }
            }
          }
        }
      }
      result.SetFailure(false, "Format_GuidInvalidChar");
      return false;
    }

    private static bool TryParseExactN(ReadOnlySpan<char> guidString, ref Guid.GuidResult result)
    {
      if (guidString.Length != 32)
      {
        result.SetFailure(false, "Format_GuidInvLen");
        return false;
      }
      ref Guid local = ref result._parsedGuid;
      uint result1;
      if (uint.TryParse(guidString.Slice(0, 8), NumberStyles.AllowHexSpecifier, (IFormatProvider) null, out Unsafe.As<int, uint>(ref local._a)) && uint.TryParse(guidString.Slice(8, 8), NumberStyles.AllowHexSpecifier, (IFormatProvider) null, out result1))
      {
        local._b = (short) (result1 >> 16);
        local._c = (short) result1;
        if (uint.TryParse(guidString.Slice(16, 8), NumberStyles.AllowHexSpecifier, (IFormatProvider) null, out result1))
        {
          local._d = (byte) (result1 >> 24);
          local._e = (byte) (result1 >> 16);
          local._f = (byte) (result1 >> 8);
          local._g = (byte) result1;
          if (uint.TryParse(guidString.Slice(24, 8), NumberStyles.AllowHexSpecifier, (IFormatProvider) null, out result1))
          {
            local._h = (byte) (result1 >> 24);
            local._i = (byte) (result1 >> 16);
            local._j = (byte) (result1 >> 8);
            local._k = (byte) result1;
            return true;
          }
        }
      }
      result.SetFailure(false, "Format_GuidInvalidChar");
      return false;
    }

    private static bool TryParseExactP(ReadOnlySpan<char> guidString, ref Guid.GuidResult result)
    {
      if (guidString.Length == 38 && guidString[0] == '(' && guidString[37] == ')')
        return Guid.TryParseExactD(guidString.Slice(1, 36), ref result);
      result.SetFailure(false, "Format_GuidInvLen");
      return false;
    }

    private static bool TryParseExactX(ReadOnlySpan<char> guidString, ref Guid.GuidResult result)
    {
      guidString = Guid.EatAllWhitespace(guidString);
      if (guidString.Length == 0 || guidString[0] != '{')
      {
        result.SetFailure(false, "Format_GuidBrace");
        return false;
      }
      if (!Guid.IsHexPrefix(guidString, 1))
      {
        result.SetFailure(false, "Format_GuidHexPrefix");
        return false;
      }
      int start1 = 3;
      int length1 = guidString.Slice(start1).IndexOf<char>(',');
      if (length1 <= 0)
      {
        result.SetFailure(false, "Format_GuidComma");
        return false;
      }
      bool overflow = false;
      if (!Guid.TryParseHex(guidString.Slice(start1, length1), out Unsafe.As<int, uint>(ref result._parsedGuid._a), ref overflow) | overflow)
      {
        result.SetFailure(overflow, overflow ? "Overflow_UInt32" : "Format_GuidInvalidChar");
        return false;
      }
      if (!Guid.IsHexPrefix(guidString, start1 + length1 + 1))
      {
        result.SetFailure(false, "Format_GuidHexPrefix");
        return false;
      }
      int start2 = start1 + length1 + 3;
      int length2 = guidString.Slice(start2).IndexOf<char>(',');
      if (length2 <= 0)
      {
        result.SetFailure(false, "Format_GuidComma");
        return false;
      }
      if (!Guid.TryParseHex(guidString.Slice(start2, length2), out result._parsedGuid._b, ref overflow) | overflow)
      {
        result.SetFailure(overflow, overflow ? "Overflow_UInt32" : "Format_GuidInvalidChar");
        return false;
      }
      if (!Guid.IsHexPrefix(guidString, start2 + length2 + 1))
      {
        result.SetFailure(false, "Format_GuidHexPrefix");
        return false;
      }
      int start3 = start2 + length2 + 3;
      int length3 = guidString.Slice(start3).IndexOf<char>(',');
      if (length3 <= 0)
      {
        result.SetFailure(false, "Format_GuidComma");
        return false;
      }
      if (!Guid.TryParseHex(guidString.Slice(start3, length3), out result._parsedGuid._c, ref overflow) | overflow)
      {
        result.SetFailure(overflow, overflow ? "Overflow_UInt32" : "Format_GuidInvalidChar");
        return false;
      }
      if ((uint) guidString.Length <= (uint) (start3 + length3 + 1) || guidString[start3 + length3 + 1] != '{')
      {
        result.SetFailure(false, "Format_GuidBrace");
        return false;
      }
      int length4 = length3 + 1;
      for (int elementOffset = 0; elementOffset < 8; ++elementOffset)
      {
        if (!Guid.IsHexPrefix(guidString, start3 + length4 + 1))
        {
          result.SetFailure(false, "Format_GuidHexPrefix");
          return false;
        }
        start3 = start3 + length4 + 3;
        if (elementOffset < 7)
        {
          length4 = guidString.Slice(start3).IndexOf<char>(',');
          if (length4 <= 0)
          {
            result.SetFailure(false, "Format_GuidComma");
            return false;
          }
        }
        else
        {
          length4 = guidString.Slice(start3).IndexOf<char>('}');
          if (length4 <= 0)
          {
            result.SetFailure(false, "Format_GuidBraceAfterLastNumber");
            return false;
          }
        }
        uint result1;
        if (!Guid.TryParseHex(guidString.Slice(start3, length4), out result1, ref overflow) | overflow || result1 > (uint) byte.MaxValue)
        {
          result.SetFailure(overflow, overflow ? "Overflow_UInt32" : (result1 > (uint) byte.MaxValue ? "Overflow_Byte" : "Format_GuidInvalidChar"));
          return false;
        }
        Unsafe.Add<byte>(ref result._parsedGuid._d, elementOffset) = (byte) result1;
      }
      if (start3 + length4 + 1 >= guidString.Length || guidString[start3 + length4 + 1] != '}')
      {
        result.SetFailure(false, "Format_GuidEndBrace");
        return false;
      }
      if (start3 + length4 + 1 == guidString.Length - 1)
        return true;
      result.SetFailure(false, "Format_ExtraJunkAtEnd");
      return false;
    }

    private static bool TryParseHex(
      ReadOnlySpan<char> guidString,
      out short result,
      ref bool overflow)
    {
      uint result1;
      bool hex = Guid.TryParseHex(guidString, out result1, ref overflow);
      result = (short) result1;
      return hex;
    }

    private static bool TryParseHex(ReadOnlySpan<char> guidString, out uint result)
    {
      bool overflow = false;
      return Guid.TryParseHex(guidString, out result, ref overflow);
    }

    private static bool TryParseHex(
      ReadOnlySpan<char> guidString,
      out uint result,
      ref bool overflow)
    {
      if ((uint) guidString.Length > 0U)
      {
        if (guidString[0] == '+')
          guidString = guidString.Slice(1);
        switch (guidString.Length)
        {
          case 0:
          case 1:
            break;
          default:
            if (guidString[0] == '0' && ((int) guidString[1] | 32) == 120)
            {
              guidString = guidString.Slice(2);
              break;
            }
            break;
        }
      }
      int index = 0;
      while (index < guidString.Length && guidString[index] == '0')
        ++index;
      int num1 = 0;
      ReadOnlySpan<byte> charToHexLookup = Number.CharToHexLookup;
      uint num2 = 0;
      for (; index < guidString.Length; ++index)
      {
        char ch = guidString[index];
        int num3;
        if ((uint) ch >= (uint) charToHexLookup.Length || (num3 = (int) charToHexLookup[(int) ch]) == (int) byte.MaxValue)
        {
          if (num1 > 8)
            overflow = true;
          result = 0U;
          return false;
        }
        num2 = (uint) ((int) num2 * 16 + num3);
        ++num1;
      }
      if (num1 > 8)
        overflow = true;
      result = num2;
      return true;
    }

    private static ReadOnlySpan<char> EatAllWhitespace(ReadOnlySpan<char> str)
    {
      int length1 = 0;
      while (length1 < str.Length && !char.IsWhiteSpace(str[length1]))
        ++length1;
      if (length1 == str.Length)
        return str;
      char[] array = new char[str.Length];
      int length2 = 0;
      if (length1 > 0)
      {
        length2 = length1;
        str.Slice(0, length1).CopyTo((Span<char>) array);
      }
      for (; length1 < str.Length; ++length1)
      {
        char c = str[length1];
        if (!char.IsWhiteSpace(c))
          array[length2++] = c;
      }
      return new ReadOnlySpan<char>(array, 0, length2);
    }

    private static bool IsHexPrefix(ReadOnlySpan<char> str, int i)
    {
      return i + 1 < str.Length && str[i] == '0' && ((int) str[i + 1] | 32) == 120;
    }

    [NullableContext(1)]
    public byte[] ToByteArray()
    {
      byte[] numArray = new byte[16];
      if (BitConverter.IsLittleEndian)
        MemoryMarshal.TryWrite<Guid>((Span<byte>) numArray, ref this);
      else
        this.TryWriteBytes((Span<byte>) numArray);
      return numArray;
    }

    public bool TryWriteBytes(Span<byte> destination)
    {
      if (BitConverter.IsLittleEndian)
        return MemoryMarshal.TryWrite<Guid>(destination, ref this);
      if (destination.Length < 16)
        return false;
      destination[15] = this._k;
      destination[0] = (byte) this._a;
      destination[1] = (byte) (this._a >> 8);
      destination[2] = (byte) (this._a >> 16);
      destination[3] = (byte) (this._a >> 24);
      destination[4] = (byte) this._b;
      destination[5] = (byte) ((uint) this._b >> 8);
      destination[6] = (byte) this._c;
      destination[7] = (byte) ((uint) this._c >> 8);
      destination[8] = this._d;
      destination[9] = this._e;
      destination[10] = this._f;
      destination[11] = this._g;
      destination[12] = this._h;
      destination[13] = this._i;
      destination[14] = this._j;
      return true;
    }

    [NullableContext(1)]
    public override string ToString()
    {
      return this.ToString("D", (IFormatProvider) null);
    }

    public override int GetHashCode()
    {
      return this._a ^ Unsafe.Add<int>(ref this._a, 1) ^ Unsafe.Add<int>(ref this._a, 2) ^ Unsafe.Add<int>(ref this._a, 3);
    }

    [NullableContext(2)]
    public override bool Equals(object o)
    {
      return o != null && o is Guid guid && (guid._a == this._a && Unsafe.Add<int>(ref guid._a, 1) == Unsafe.Add<int>(ref this._a, 1)) && Unsafe.Add<int>(ref guid._a, 2) == Unsafe.Add<int>(ref this._a, 2) && Unsafe.Add<int>(ref guid._a, 3) == Unsafe.Add<int>(ref this._a, 3);
    }

    public bool Equals(Guid g)
    {
      return g._a == this._a && Unsafe.Add<int>(ref g._a, 1) == Unsafe.Add<int>(ref this._a, 1) && Unsafe.Add<int>(ref g._a, 2) == Unsafe.Add<int>(ref this._a, 2) && Unsafe.Add<int>(ref g._a, 3) == Unsafe.Add<int>(ref this._a, 3);
    }

    private int GetResult(uint me, uint them)
    {
      return me >= them ? 1 : -1;
    }

    [NullableContext(2)]
    public int CompareTo(object value)
    {
      if (value == null)
        return 1;
      if (!(value is Guid guid))
        throw new ArgumentException(SR.Arg_MustBeGuid, nameof (value));
      if (guid._a != this._a)
        return this.GetResult((uint) this._a, (uint) guid._a);
      if ((int) guid._b != (int) this._b)
        return this.GetResult((uint) this._b, (uint) guid._b);
      if ((int) guid._c != (int) this._c)
        return this.GetResult((uint) this._c, (uint) guid._c);
      if ((int) guid._d != (int) this._d)
        return this.GetResult((uint) this._d, (uint) guid._d);
      if ((int) guid._e != (int) this._e)
        return this.GetResult((uint) this._e, (uint) guid._e);
      if ((int) guid._f != (int) this._f)
        return this.GetResult((uint) this._f, (uint) guid._f);
      if ((int) guid._g != (int) this._g)
        return this.GetResult((uint) this._g, (uint) guid._g);
      if ((int) guid._h != (int) this._h)
        return this.GetResult((uint) this._h, (uint) guid._h);
      if ((int) guid._i != (int) this._i)
        return this.GetResult((uint) this._i, (uint) guid._i);
      if ((int) guid._j != (int) this._j)
        return this.GetResult((uint) this._j, (uint) guid._j);
      return (int) guid._k != (int) this._k ? this.GetResult((uint) this._k, (uint) guid._k) : 0;
    }

    public int CompareTo(Guid value)
    {
      if (value._a != this._a)
        return this.GetResult((uint) this._a, (uint) value._a);
      if ((int) value._b != (int) this._b)
        return this.GetResult((uint) this._b, (uint) value._b);
      if ((int) value._c != (int) this._c)
        return this.GetResult((uint) this._c, (uint) value._c);
      if ((int) value._d != (int) this._d)
        return this.GetResult((uint) this._d, (uint) value._d);
      if ((int) value._e != (int) this._e)
        return this.GetResult((uint) this._e, (uint) value._e);
      if ((int) value._f != (int) this._f)
        return this.GetResult((uint) this._f, (uint) value._f);
      if ((int) value._g != (int) this._g)
        return this.GetResult((uint) this._g, (uint) value._g);
      if ((int) value._h != (int) this._h)
        return this.GetResult((uint) this._h, (uint) value._h);
      if ((int) value._i != (int) this._i)
        return this.GetResult((uint) this._i, (uint) value._i);
      if ((int) value._j != (int) this._j)
        return this.GetResult((uint) this._j, (uint) value._j);
      return (int) value._k != (int) this._k ? this.GetResult((uint) this._k, (uint) value._k) : 0;
    }

    public static bool operator ==(Guid a, Guid b)
    {
      return a._a == b._a && Unsafe.Add<int>(ref a._a, 1) == Unsafe.Add<int>(ref b._a, 1) && Unsafe.Add<int>(ref a._a, 2) == Unsafe.Add<int>(ref b._a, 2) && Unsafe.Add<int>(ref a._a, 3) == Unsafe.Add<int>(ref b._a, 3);
    }

    public static bool operator !=(Guid a, Guid b)
    {
      return a._a != b._a || Unsafe.Add<int>(ref a._a, 1) != Unsafe.Add<int>(ref b._a, 1) || Unsafe.Add<int>(ref a._a, 2) != Unsafe.Add<int>(ref b._a, 2) || Unsafe.Add<int>(ref a._a, 3) != Unsafe.Add<int>(ref b._a, 3);
    }

    [NullableContext(1)]
    public string ToString([Nullable(2)] string format)
    {
      return this.ToString(format, (IFormatProvider) null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static char HexToChar(int a)
    {
      a &= 15;
      return a > 9 ? (char) (a - 10 + 97) : (char) (a + 48);
    }

    private static unsafe int HexsToChars(char* guidChars, int a, int b)
    {
      *guidChars = Guid.HexToChar(a >> 4);
      guidChars[1] = Guid.HexToChar(a);
      guidChars[2] = Guid.HexToChar(b >> 4);
      guidChars[3] = Guid.HexToChar(b);
      return 4;
    }

    private static unsafe int HexsToCharsHexOutput(char* guidChars, int a, int b)
    {
      *guidChars = '0';
      guidChars[1] = 'x';
      guidChars[2] = Guid.HexToChar(a >> 4);
      guidChars[3] = Guid.HexToChar(a);
      guidChars[4] = ',';
      guidChars[5] = '0';
      guidChars[6] = 'x';
      guidChars[7] = Guid.HexToChar(b >> 4);
      guidChars[8] = Guid.HexToChar(b);
      return 9;
    }

    [NullableContext(2)]
    [return: Nullable(1)]
    public string ToString(string format, IFormatProvider provider)
    {
      if (string.IsNullOrEmpty(format))
        format = "D";
      if (format.Length != 1)
        throw new FormatException(SR.Format_InvalidGuidFormatSpecification);
      int length;
      switch (format[0])
      {
        case 'B':
        case 'P':
        case 'b':
        case 'p':
          length = 38;
          break;
        case 'D':
        case 'd':
          length = 36;
          break;
        case 'N':
        case 'n':
          length = 32;
          break;
        case 'X':
        case 'x':
          length = 68;
          break;
        default:
          throw new FormatException(SR.Format_InvalidGuidFormatSpecification);
      }
      string str = string.FastAllocateString(length);
      this.TryFormat(new Span<char>(ref str.GetRawStringData(), str.Length), out int _, (ReadOnlySpan<char>) format);
      return str;
    }

    public unsafe bool TryFormat(
      Span<char> destination,
      out int charsWritten,
      ReadOnlySpan<char> format = default (ReadOnlySpan<char>))
    {
      if (format.Length == 0)
        format = (ReadOnlySpan<char>) "D";
      if (format.Length != 1)
        throw new FormatException(SR.Format_InvalidGuidFormatSpecification);
      bool flag1 = true;
      bool flag2 = false;
      int num1 = 0;
      int num2;
      switch (format[0])
      {
        case 'B':
        case 'b':
          num1 = 8192123;
          num2 = 38;
          break;
        case 'D':
        case 'd':
          num2 = 36;
          break;
        case 'N':
        case 'n':
          flag1 = false;
          num2 = 32;
          break;
        case 'P':
        case 'p':
          num1 = 2687016;
          num2 = 38;
          break;
        case 'X':
        case 'x':
          num1 = 8192123;
          flag1 = false;
          flag2 = true;
          num2 = 68;
          break;
        default:
          throw new FormatException(SR.Format_InvalidGuidFormatSpecification);
      }
      if (destination.Length < num2)
      {
        charsWritten = 0;
        return false;
      }
      fixed (char* chPtr1 = &MemoryMarshal.GetReference<char>(destination))
      {
        char* guidChars1 = chPtr1;
        if (num1 != 0)
          *guidChars1++ = (char) num1;
        char* chPtr2;
        if (flag2)
        {
          char* chPtr3 = guidChars1;
          char* chPtr4 = (char*) ((IntPtr) chPtr3 + 2);
          *chPtr3 = '0';
          char* chPtr5 = chPtr4;
          char* guidChars2 = (char*) ((IntPtr) chPtr5 + 2);
          *chPtr5 = 'x';
          char* guidChars3 = guidChars2 + Guid.HexsToChars(guidChars2, this._a >> 24, this._a >> 16);
          char* chPtr6 = guidChars3 + Guid.HexsToChars(guidChars3, this._a >> 8, this._a);
          char* chPtr7 = (char*) ((IntPtr) chPtr6 + 2);
          *chPtr6 = ',';
          char* chPtr8 = chPtr7;
          char* chPtr9 = (char*) ((IntPtr) chPtr8 + 2);
          *chPtr8 = '0';
          char* chPtr10 = chPtr9;
          char* guidChars4 = (char*) ((IntPtr) chPtr10 + 2);
          *chPtr10 = 'x';
          char* chPtr11 = guidChars4 + Guid.HexsToChars(guidChars4, (int) this._b >> 8, (int) this._b);
          char* chPtr12 = (char*) ((IntPtr) chPtr11 + 2);
          *chPtr11 = ',';
          char* chPtr13 = chPtr12;
          char* chPtr14 = (char*) ((IntPtr) chPtr13 + 2);
          *chPtr13 = '0';
          char* chPtr15 = chPtr14;
          char* guidChars5 = (char*) ((IntPtr) chPtr15 + 2);
          *chPtr15 = 'x';
          char* chPtr16 = guidChars5 + Guid.HexsToChars(guidChars5, (int) this._c >> 8, (int) this._c);
          char* chPtr17 = (char*) ((IntPtr) chPtr16 + 2);
          *chPtr16 = ',';
          char* chPtr18 = chPtr17;
          char* guidChars6 = (char*) ((IntPtr) chPtr18 + 2);
          *chPtr18 = '{';
          char* chPtr19 = guidChars6 + Guid.HexsToCharsHexOutput(guidChars6, (int) this._d, (int) this._e);
          char* guidChars7 = (char*) ((IntPtr) chPtr19 + 2);
          *chPtr19 = ',';
          char* chPtr20 = guidChars7 + Guid.HexsToCharsHexOutput(guidChars7, (int) this._f, (int) this._g);
          char* guidChars8 = (char*) ((IntPtr) chPtr20 + 2);
          *chPtr20 = ',';
          char* chPtr21 = guidChars8 + Guid.HexsToCharsHexOutput(guidChars8, (int) this._h, (int) this._i);
          char* guidChars9 = (char*) ((IntPtr) chPtr21 + 2);
          *chPtr21 = ',';
          char* chPtr22 = guidChars9 + Guid.HexsToCharsHexOutput(guidChars9, (int) this._j, (int) this._k);
          chPtr2 = (char*) ((IntPtr) chPtr22 + 2);
          *chPtr22 = '}';
        }
        else
        {
          char* guidChars2 = guidChars1 + Guid.HexsToChars(guidChars1, this._a >> 24, this._a >> 16);
          char* guidChars3 = guidChars2 + Guid.HexsToChars(guidChars2, this._a >> 8, this._a);
          if (flag1)
            *guidChars3++ = '-';
          char* guidChars4 = guidChars3 + Guid.HexsToChars(guidChars3, (int) this._b >> 8, (int) this._b);
          if (flag1)
            *guidChars4++ = '-';
          char* guidChars5 = guidChars4 + Guid.HexsToChars(guidChars4, (int) this._c >> 8, (int) this._c);
          if (flag1)
            *guidChars5++ = '-';
          char* guidChars6 = guidChars5 + Guid.HexsToChars(guidChars5, (int) this._d, (int) this._e);
          if (flag1)
            *guidChars6++ = '-';
          char* guidChars7 = guidChars6 + Guid.HexsToChars(guidChars6, (int) this._f, (int) this._g);
          char* guidChars8 = guidChars7 + Guid.HexsToChars(guidChars7, (int) this._h, (int) this._i);
          chPtr2 = guidChars8 + Guid.HexsToChars(guidChars8, (int) this._j, (int) this._k);
        }
        if (num1 != 0)
        {
          char* chPtr3 = chPtr2;
          char* chPtr4 = (char*) ((IntPtr) chPtr3 + 2);
          int num3 = (int) (ushort) (num1 >> 16);
          *chPtr3 = (char) num3;
        }
      }
      charsWritten = num2;
      return true;
    }

    bool ISpanFormattable.TryFormat(
      Span<char> destination,
      out int charsWritten,
      ReadOnlySpan<char> format,
      IFormatProvider provider)
    {
      return this.TryFormat(destination, out charsWritten, format);
    }

    public static Guid NewGuid()
    {
      Guid guid1;
      int guid2 = Interop.Ole32.CoCreateGuid(out guid1);
      if (guid2 != 0)
        throw new Exception() { HResult = guid2 };
      return guid1;
    }

    private enum GuidParseThrowStyle : byte
    {
      None,
      All,
      AllButOverflow,
    }

    private struct GuidResult
    {
      private readonly Guid.GuidParseThrowStyle _throwStyle;
      internal Guid _parsedGuid;

      internal GuidResult(Guid.GuidParseThrowStyle canThrow)
        : this()
      {
        this._throwStyle = canThrow;
      }

      internal void SetFailure(bool overflow, string failureMessageID)
      {
        if (this._throwStyle == Guid.GuidParseThrowStyle.None)
          return;
        if (!overflow)
          throw new FormatException(SR.GetResourceString(failureMessageID));
        if (this._throwStyle == Guid.GuidParseThrowStyle.All)
          throw new OverflowException(SR.GetResourceString(failureMessageID));
        throw new FormatException(SR.Format_GuidUnrecognized);
      }
    }
  }
}

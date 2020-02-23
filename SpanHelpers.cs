// Decompiled with JetBrains decompiler
// Type: System.SpanHelpers
// Assembly: System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// MVID: BBB3B884-123D-47EA-9CD1-5BED540D02AE
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Private.CoreLib.dll

using Internal.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace System
{
  internal static class SpanHelpers
  {
    public static void ClearWithoutReferences(ref byte b, ulong byteLength)
    {
      if (byteLength == 0UL)
        return;
      if (byteLength <= 768UL)
        Unsafe.InitBlockUnaligned(ref b, (byte) 0, (uint) byteLength);
      else
        RuntimeImports.RhZeroMemory(ref b, byteLength);
    }

    public static unsafe void ClearWithReferences(ref IntPtr ip, ulong pointerSizeLength)
    {
      for (; pointerSizeLength >= 8UL; pointerSizeLength -= 8UL)
      {
        *(IntPtr*) ref Unsafe.Add<IntPtr>(ref Unsafe.Add<IntPtr>(ref ip, (IntPtr) (long) pointerSizeLength), -1) = new IntPtr();
        *(IntPtr*) ref Unsafe.Add<IntPtr>(ref Unsafe.Add<IntPtr>(ref ip, (IntPtr) (long) pointerSizeLength), -2) = new IntPtr();
        *(IntPtr*) ref Unsafe.Add<IntPtr>(ref Unsafe.Add<IntPtr>(ref ip, (IntPtr) (long) pointerSizeLength), -3) = new IntPtr();
        *(IntPtr*) ref Unsafe.Add<IntPtr>(ref Unsafe.Add<IntPtr>(ref ip, (IntPtr) (long) pointerSizeLength), -4) = new IntPtr();
        *(IntPtr*) ref Unsafe.Add<IntPtr>(ref Unsafe.Add<IntPtr>(ref ip, (IntPtr) (long) pointerSizeLength), -5) = new IntPtr();
        *(IntPtr*) ref Unsafe.Add<IntPtr>(ref Unsafe.Add<IntPtr>(ref ip, (IntPtr) (long) pointerSizeLength), -6) = new IntPtr();
        *(IntPtr*) ref Unsafe.Add<IntPtr>(ref Unsafe.Add<IntPtr>(ref ip, (IntPtr) (long) pointerSizeLength), -7) = new IntPtr();
        *(IntPtr*) ref Unsafe.Add<IntPtr>(ref Unsafe.Add<IntPtr>(ref ip, (IntPtr) (long) pointerSizeLength), -8) = new IntPtr();
      }
      if (pointerSizeLength < 4UL)
      {
        if (pointerSizeLength < 2UL)
        {
          if (pointerSizeLength <= 0UL)
            return;
          goto label_8;
        }
      }
      else
      {
        *(IntPtr*) ref Unsafe.Add<IntPtr>(ref ip, 2) = new IntPtr();
        *(IntPtr*) ref Unsafe.Add<IntPtr>(ref ip, 3) = new IntPtr();
        *(IntPtr*) ref Unsafe.Add<IntPtr>(ref Unsafe.Add<IntPtr>(ref ip, (IntPtr) (long) pointerSizeLength), -3) = new IntPtr();
        *(IntPtr*) ref Unsafe.Add<IntPtr>(ref Unsafe.Add<IntPtr>(ref ip, (IntPtr) (long) pointerSizeLength), -2) = new IntPtr();
      }
      *(IntPtr*) ref Unsafe.Add<IntPtr>(ref ip, 1) = new IntPtr();
      *(IntPtr*) ref Unsafe.Add<IntPtr>(ref Unsafe.Add<IntPtr>(ref ip, (IntPtr) (long) pointerSizeLength), -1) = new IntPtr();
label_8:
      ip = new IntPtr();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BinarySearch<T, TComparable>(
      this ReadOnlySpan<T> span,
      TComparable comparable)
      where TComparable : IComparable<T>
    {
      if ((object) comparable == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparable);
      return SpanHelpers.BinarySearch<T, TComparable>(ref MemoryMarshal.GetReference<T>(span), span.Length, comparable);
    }

    public static int BinarySearch<T, TComparable>(
      ref T spanStart,
      int length,
      TComparable comparable)
      where TComparable : IComparable<T>
    {
      int num1 = 0;
      int num2 = length - 1;
      while (num1 <= num2)
      {
        int elementOffset = (int) ((uint) (num2 + num1) >> 1);
        int num3 = comparable.CompareTo(Unsafe.Add<T>(ref spanStart, elementOffset));
        if (num3 == 0)
          return elementOffset;
        if (num3 > 0)
          num1 = elementOffset + 1;
        else
          num2 = elementOffset - 1;
      }
      return ~num1;
    }

    public static int IndexOf(
      ref byte searchSpace,
      int searchSpaceLength,
      ref byte value,
      int valueLength)
    {
      if (valueLength == 0)
        return 0;
      byte num1 = value;
      ref byte local = ref Unsafe.Add<byte>(ref value, 1);
      int length1 = valueLength - 1;
      int length2 = searchSpaceLength - length1;
      int elementOffset = 0;
      while (length2 > 0)
      {
        int num2 = SpanHelpers.IndexOf(ref Unsafe.Add<byte>(ref searchSpace, elementOffset), num1, length2);
        if (num2 != -1)
        {
          int num3 = length2 - num2;
          int num4 = elementOffset + num2;
          if (num3 > 0)
          {
            if (SpanHelpers.SequenceEqual<byte>(ref Unsafe.Add<byte>(ref searchSpace, num4 + 1), ref local, length1))
              return num4;
            length2 = num3 - 1;
            elementOffset = num4 + 1;
          }
          else
            break;
        }
        else
          break;
      }
      return -1;
    }

    public static int IndexOfAny(
      ref byte searchSpace,
      int searchSpaceLength,
      ref byte value,
      int valueLength)
    {
      if (valueLength == 0)
        return -1;
      int num1 = -1;
      for (int elementOffset = 0; elementOffset < valueLength; ++elementOffset)
      {
        int num2 = SpanHelpers.IndexOf(ref searchSpace, Unsafe.Add<byte>(ref value, elementOffset), searchSpaceLength);
        if ((uint) num2 < (uint) num1)
        {
          num1 = num2;
          searchSpaceLength = num2;
          if (num1 == 0)
            break;
        }
      }
      return num1;
    }

    public static int LastIndexOfAny(
      ref byte searchSpace,
      int searchSpaceLength,
      ref byte value,
      int valueLength)
    {
      if (valueLength == 0)
        return -1;
      int num1 = -1;
      for (int elementOffset = 0; elementOffset < valueLength; ++elementOffset)
      {
        int num2 = SpanHelpers.LastIndexOf(ref searchSpace, Unsafe.Add<byte>(ref value, elementOffset), searchSpaceLength);
        if (num2 > num1)
          num1 = num2;
      }
      return num1;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe bool Contains(ref byte searchSpace, byte value, int length)
    {
      uint num1 = (uint) value;
      IntPtr num2 = (IntPtr) 0;
      IntPtr num3 = (IntPtr) length;
      if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
        num3 = SpanHelpers.UnalignedCountVector(ref searchSpace);
      while (true)
      {
        while ((UIntPtr) (void*) num3 >= new UIntPtr(8))
        {
          num3 -= 8;
          if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 0) && (int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 1) && ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 2) && (int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 3)) && ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 4) && (int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 5) && ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 6) && (int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 7))))
            num2 += 8;
          else
            goto label_19;
        }
        if ((UIntPtr) (void*) num3 >= new UIntPtr(4))
        {
          num3 -= 4;
          if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 0) && (int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 1) && ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 2) && (int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 3)))
            num2 += 4;
          else
            goto label_19;
        }
        while ((UIntPtr) (void*) num3 > UIntPtr.Zero)
        {
          num3 -= 1;
          if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2))
            num2 += 1;
          else
            goto label_19;
        }
        if (Vector.IsHardwareAccelerated && (int) (void*) num2 < length)
        {
          IntPtr num4 = (IntPtr) (length - (int) (void*) num2 & ~(Vector<byte>.Count - 1));
          Vector<byte> left = new Vector<byte>(value);
          while ((void*) num4 > (void*) num2)
          {
            if (Vector<byte>.Zero.Equals(Vector.Equals<byte>(left, SpanHelpers.LoadVector(ref searchSpace, num2))))
              num2 += Vector<byte>.Count;
            else
              goto label_19;
          }
          if ((int) (void*) num2 < length)
            num3 = (IntPtr) (length - (int) (void*) num2);
          else
            break;
        }
        else
          break;
      }
      return false;
label_19:
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe int IndexOf(ref byte searchSpace, byte value, int length)
    {
      uint num1 = (uint) value;
      IntPtr num2 = (IntPtr) 0;
      IntPtr num3 = (IntPtr) length;
      if (Avx2.IsSupported || Sse2.IsSupported)
      {
        if (length >= Vector128<byte>.Count * 2)
          num3 = SpanHelpers.UnalignedCountVector128(ref searchSpace);
      }
      else if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
        num3 = SpanHelpers.UnalignedCountVector(ref searchSpace);
      int num4;
      int num5;
      int num6;
      while (true)
      {
        while ((UIntPtr) (void*) num3 >= new UIntPtr(8))
        {
          num3 -= 8;
          if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2))
          {
            if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 1))
            {
              if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 2))
              {
                if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 3))
                {
                  if ((int) num1 == (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 4))
                    return (int) (void*) (num2 + 4);
                  if ((int) num1 == (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 5))
                    return (int) (void*) (num2 + 5);
                  if ((int) num1 == (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 6))
                    return (int) (void*) (num2 + 6);
                  if ((int) num1 == (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 7))
                    return (int) (void*) (num2 + 7);
                  num2 += 8;
                }
                else
                  goto label_62;
              }
              else
                goto label_61;
            }
            else
              goto label_60;
          }
          else
            goto label_59;
        }
        if ((UIntPtr) (void*) num3 >= new UIntPtr(4))
        {
          num3 -= 4;
          if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2))
          {
            if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 1))
            {
              if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 2))
              {
                if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2 + 3))
                  num2 += 4;
                else
                  goto label_62;
              }
              else
                goto label_61;
            }
            else
              goto label_60;
          }
          else
            goto label_59;
        }
        while ((UIntPtr) (void*) num3 > UIntPtr.Zero)
        {
          num3 -= 1;
          if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, num2))
            num2 += 1;
          else
            goto label_59;
        }
        if (Avx2.IsSupported)
        {
          if ((int) (void*) num2 < length)
          {
            if (((long) Unsafe.AsPointer<byte>(ref searchSpace) + (long) num2 & (long) (Vector256<byte>.Count - 1)) != 0L)
            {
              num4 = Sse2.MoveMask(Sse2.CompareEqual(Vector128.Create(value), SpanHelpers.LoadVector128(ref searchSpace, num2)));
              if (num4 == 0)
                num2 += Vector128<byte>.Count;
              else
                break;
            }
            IntPtr vector256SpanLength = SpanHelpers.GetByteVector256SpanLength(num2, length);
            if ((void*) vector256SpanLength > (void*) num2)
            {
              Vector256<byte> left = Vector256.Create(value);
              do
              {
                Vector256<byte> right = SpanHelpers.LoadVector256(ref searchSpace, num2);
                num5 = Avx2.MoveMask(Avx2.CompareEqual(left, right));
                if (num5 == 0)
                  num2 += Vector256<byte>.Count;
                else
                  goto label_34;
              }
              while ((void*) vector256SpanLength > (void*) num2);
            }
            if ((void*) SpanHelpers.GetByteVector128SpanLength(num2, length) > (void*) num2)
            {
              num6 = Sse2.MoveMask(Sse2.CompareEqual(Vector128.Create(value), SpanHelpers.LoadVector128(ref searchSpace, num2)));
              if (num6 == 0)
                num2 += Vector128<byte>.Count;
              else
                goto label_38;
            }
            if ((int) (void*) num2 < length)
              num3 = (IntPtr) (length - (int) (void*) num2);
            else
              goto label_58;
          }
          else
            goto label_58;
        }
        else if (Sse2.IsSupported)
        {
          if ((int) (void*) num2 < length)
          {
            IntPtr vector128SpanLength = SpanHelpers.GetByteVector128SpanLength(num2, length);
            Vector128<byte> left = Vector128.Create(value);
            while ((void*) vector128SpanLength > (void*) num2)
            {
              Vector128<byte> right = SpanHelpers.LoadVector128(ref searchSpace, num2);
              int num7 = Sse2.MoveMask(Sse2.CompareEqual(left, right));
              if (num7 != 0)
                return (int) (void*) num2 + BitOperations.TrailingZeroCount(num7);
              num2 += Vector128<byte>.Count;
            }
            if ((int) (void*) num2 < length)
              num3 = (IntPtr) (length - (int) (void*) num2);
            else
              goto label_58;
          }
          else
            goto label_58;
        }
        else if (Vector.IsHardwareAccelerated && (int) (void*) num2 < length)
        {
          IntPtr vectorSpanLength = SpanHelpers.GetByteVectorSpanLength(num2, length);
          Vector<byte> left = new Vector<byte>(value);
          while ((void*) vectorSpanLength > (void*) num2)
          {
            Vector<byte> vector = Vector.Equals<byte>(left, SpanHelpers.LoadVector(ref searchSpace, num2));
            if (!Vector<byte>.Zero.Equals(vector))
              return (int) (void*) num2 + SpanHelpers.LocateFirstFoundByte(vector);
            num2 += Vector<byte>.Count;
          }
          if ((int) (void*) num2 < length)
            num3 = (IntPtr) (length - (int) (void*) num2);
          else
            goto label_58;
        }
        else
          goto label_58;
      }
      return (int) (void*) num2 + BitOperations.TrailingZeroCount(num4);
label_34:
      return (int) (void*) num2 + BitOperations.TrailingZeroCount(num5);
label_38:
      return (int) (void*) num2 + BitOperations.TrailingZeroCount(num6);
label_58:
      return -1;
label_59:
      return (int) (void*) num2;
label_60:
      return (int) (void*) (num2 + 1);
label_61:
      return (int) (void*) (num2 + 2);
label_62:
      return (int) (void*) (num2 + 3);
    }

    public static int LastIndexOf(
      ref byte searchSpace,
      int searchSpaceLength,
      ref byte value,
      int valueLength)
    {
      if (valueLength == 0)
        return 0;
      byte num1 = value;
      ref byte local = ref Unsafe.Add<byte>(ref value, 1);
      int length1 = valueLength - 1;
      int num2 = 0;
      int num3;
      while (true)
      {
        int length2 = searchSpaceLength - num2 - length1;
        if (length2 > 0)
        {
          num3 = SpanHelpers.LastIndexOf(ref searchSpace, num1, length2);
          if (num3 != -1)
          {
            if (!SpanHelpers.SequenceEqual<byte>(ref Unsafe.Add<byte>(ref searchSpace, num3 + 1), ref local, length1))
              num2 += length2 - num3;
            else
              break;
          }
          else
            goto label_8;
        }
        else
          goto label_8;
      }
      return num3;
label_8:
      return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe int LastIndexOf(ref byte searchSpace, byte value, int length)
    {
      uint num1 = (uint) value;
      IntPtr byteOffset = (IntPtr) length;
      IntPtr num2 = (IntPtr) length;
      if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
        num2 = SpanHelpers.UnalignedCountVectorFromEnd(ref searchSpace, length);
      while (true)
      {
        while ((UIntPtr) (void*) num2 >= new UIntPtr(8))
        {
          num2 -= 8;
          byteOffset -= 8;
          if ((int) num1 == (int) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 7))
            return (int) (void*) (byteOffset + 7);
          if ((int) num1 == (int) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 6))
            return (int) (void*) (byteOffset + 6);
          if ((int) num1 == (int) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 5))
            return (int) (void*) (byteOffset + 5);
          if ((int) num1 == (int) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 4))
            return (int) (void*) (byteOffset + 4);
          if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3))
          {
            if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2))
            {
              if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1))
              {
                if ((int) num1 == (int) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset))
                  goto label_27;
              }
              else
                goto label_28;
            }
            else
              goto label_29;
          }
          else
            goto label_30;
        }
        if ((UIntPtr) (void*) num2 >= new UIntPtr(4))
        {
          num2 -= 4;
          byteOffset -= 4;
          if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3))
          {
            if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2))
            {
              if ((int) num1 != (int) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1))
              {
                if ((int) num1 == (int) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset))
                  goto label_27;
              }
              else
                goto label_28;
            }
            else
              goto label_29;
          }
          else
            goto label_30;
        }
        while ((UIntPtr) (void*) num2 > UIntPtr.Zero)
        {
          num2 -= 1;
          byteOffset -= 1;
          if ((int) num1 == (int) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset))
            goto label_27;
        }
        if (Vector.IsHardwareAccelerated && (UIntPtr) (void*) byteOffset > UIntPtr.Zero)
        {
          IntPtr num3 = (IntPtr) ((int) (void*) byteOffset & ~(Vector<byte>.Count - 1));
          Vector<byte> left = new Vector<byte>(value);
          while ((UIntPtr) (void*) num3 > (UIntPtr) (Vector<byte>.Count - 1))
          {
            Vector<byte> vector = Vector.Equals<byte>(left, SpanHelpers.LoadVector(ref searchSpace, byteOffset - Vector<byte>.Count));
            if (!Vector<byte>.Zero.Equals(vector))
              return (int) byteOffset - Vector<byte>.Count + SpanHelpers.LocateLastFoundByte(vector);
            byteOffset -= Vector<byte>.Count;
            num3 -= Vector<byte>.Count;
          }
          if ((UIntPtr) (void*) byteOffset > UIntPtr.Zero)
            num2 = byteOffset;
          else
            break;
        }
        else
          break;
      }
      return -1;
label_27:
      return (int) (void*) byteOffset;
label_28:
      return (int) (void*) (byteOffset + 1);
label_29:
      return (int) (void*) (byteOffset + 2);
label_30:
      return (int) (void*) (byteOffset + 3);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe int IndexOfAny(
      ref byte searchSpace,
      byte value0,
      byte value1,
      int length)
    {
      uint num1 = (uint) value0;
      uint num2 = (uint) value1;
      IntPtr num3 = (IntPtr) 0;
      IntPtr num4 = (IntPtr) length;
      if (Avx2.IsSupported || Sse2.IsSupported)
      {
        if (length >= Vector128<byte>.Count * 2)
          num4 = SpanHelpers.UnalignedCountVector128(ref searchSpace);
      }
      else if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
        num4 = SpanHelpers.UnalignedCountVector(ref searchSpace);
      int num5;
      int num6;
      while (true)
      {
        while ((UIntPtr) (void*) num4 >= new UIntPtr(8))
        {
          num4 -= 8;
          uint num7 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num3);
          if ((int) num1 != (int) num7 && (int) num2 != (int) num7)
          {
            uint num8 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num3 + 1);
            if ((int) num1 != (int) num8 && (int) num2 != (int) num8)
            {
              uint num9 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num3 + 2);
              if ((int) num1 != (int) num9 && (int) num2 != (int) num9)
              {
                uint num10 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num3 + 3);
                if ((int) num1 != (int) num10 && (int) num2 != (int) num10)
                {
                  uint num11 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num3 + 4);
                  if ((int) num1 == (int) num11 || (int) num2 == (int) num11)
                    return (int) (void*) (num3 + 4);
                  uint num12 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num3 + 5);
                  if ((int) num1 == (int) num12 || (int) num2 == (int) num12)
                    return (int) (void*) (num3 + 5);
                  uint num13 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num3 + 6);
                  if ((int) num1 == (int) num13 || (int) num2 == (int) num13)
                    return (int) (void*) (num3 + 6);
                  uint num14 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num3 + 7);
                  if ((int) num1 == (int) num14 || (int) num2 == (int) num14)
                    return (int) (void*) (num3 + 7);
                  num3 += 8;
                }
                else
                  goto label_58;
              }
              else
                goto label_57;
            }
            else
              goto label_56;
          }
          else
            goto label_55;
        }
        if ((UIntPtr) (void*) num4 >= new UIntPtr(4))
        {
          num4 -= 4;
          uint num7 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num3);
          if ((int) num1 != (int) num7 && (int) num2 != (int) num7)
          {
            uint num8 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num3 + 1);
            if ((int) num1 != (int) num8 && (int) num2 != (int) num8)
            {
              uint num9 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num3 + 2);
              if ((int) num1 != (int) num9 && (int) num2 != (int) num9)
              {
                uint num10 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num3 + 3);
                if ((int) num1 != (int) num10 && (int) num2 != (int) num10)
                  num3 += 4;
                else
                  goto label_58;
              }
              else
                goto label_57;
            }
            else
              goto label_56;
          }
          else
            goto label_55;
        }
        while ((UIntPtr) (void*) num4 > UIntPtr.Zero)
        {
          num4 -= 1;
          uint num7 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num3);
          if ((int) num1 != (int) num7 && (int) num2 != (int) num7)
            num3 += 1;
          else
            goto label_55;
        }
        if (Avx2.IsSupported)
        {
          if ((int) (void*) num3 < length)
          {
            IntPtr vector256SpanLength = SpanHelpers.GetByteVector256SpanLength(num3, length);
            if ((void*) vector256SpanLength > (void*) num3)
            {
              Vector256<byte> left1 = Vector256.Create(value0);
              Vector256<byte> left2 = Vector256.Create(value1);
              do
              {
                Vector256<byte> right = SpanHelpers.LoadVector256(ref searchSpace, num3);
                num5 = Avx2.MoveMask(Avx2.Or(Avx2.CompareEqual(left1, right), Avx2.CompareEqual(left2, right)));
                if (num5 == 0)
                  num3 += Vector256<byte>.Count;
                else
                  goto label_30;
              }
              while ((void*) vector256SpanLength > (void*) num3);
            }
            if ((void*) SpanHelpers.GetByteVector128SpanLength(num3, length) > (void*) num3)
            {
              Vector128<byte> left1 = Vector128.Create(value0);
              Vector128<byte> left2 = Vector128.Create(value1);
              Vector128<byte> right = SpanHelpers.LoadVector128(ref searchSpace, num3);
              num6 = Sse2.MoveMask(Sse2.Or(Sse2.CompareEqual(left1, right), Sse2.CompareEqual(left2, right)));
              if (num6 == 0)
                num3 += Vector128<byte>.Count;
              else
                goto label_34;
            }
            if ((int) (void*) num3 < length)
              num4 = (IntPtr) (length - (int) (void*) num3);
            else
              goto label_54;
          }
          else
            goto label_54;
        }
        else if (Sse2.IsSupported)
        {
          if ((int) (void*) num3 < length)
          {
            IntPtr vector128SpanLength = SpanHelpers.GetByteVector128SpanLength(num3, length);
            Vector128<byte> left1 = Vector128.Create(value0);
            Vector128<byte> left2 = Vector128.Create(value1);
            while ((void*) vector128SpanLength > (void*) num3)
            {
              Vector128<byte> right = SpanHelpers.LoadVector128(ref searchSpace, num3);
              int num7 = Sse2.MoveMask(Sse2.Or(Sse2.CompareEqual(left1, right), Sse2.CompareEqual(left2, right)));
              if (num7 != 0)
                return (int) (void*) num3 + BitOperations.TrailingZeroCount(num7);
              num3 += Vector128<byte>.Count;
            }
            if ((int) (void*) num3 < length)
              num4 = (IntPtr) (length - (int) (void*) num3);
            else
              goto label_54;
          }
          else
            goto label_54;
        }
        else if (Vector.IsHardwareAccelerated && (int) (void*) num3 < length)
        {
          IntPtr vectorSpanLength = SpanHelpers.GetByteVectorSpanLength(num3, length);
          Vector<byte> right1 = new Vector<byte>(value0);
          Vector<byte> right2 = new Vector<byte>(value1);
          while ((void*) vectorSpanLength > (void*) num3)
          {
            Vector<byte> left = SpanHelpers.LoadVector(ref searchSpace, num3);
            Vector<byte> vector = Vector.BitwiseOr<byte>(Vector.Equals<byte>(left, right1), Vector.Equals<byte>(left, right2));
            if (!Vector<byte>.Zero.Equals(vector))
              return (int) (void*) num3 + SpanHelpers.LocateFirstFoundByte(vector);
            num3 += Vector<byte>.Count;
          }
          if ((int) (void*) num3 < length)
            num4 = (IntPtr) (length - (int) (void*) num3);
          else
            goto label_54;
        }
        else
          goto label_54;
      }
label_30:
      return (int) (void*) num3 + BitOperations.TrailingZeroCount(num5);
label_34:
      return (int) (void*) num3 + BitOperations.TrailingZeroCount(num6);
label_54:
      return -1;
label_55:
      return (int) (void*) num3;
label_56:
      return (int) (void*) (num3 + 1);
label_57:
      return (int) (void*) (num3 + 2);
label_58:
      return (int) (void*) (num3 + 3);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe int IndexOfAny(
      ref byte searchSpace,
      byte value0,
      byte value1,
      byte value2,
      int length)
    {
      uint num1 = (uint) value0;
      uint num2 = (uint) value1;
      uint num3 = (uint) value2;
      IntPtr num4 = (IntPtr) 0;
      IntPtr num5 = (IntPtr) length;
      if (Avx2.IsSupported || Sse2.IsSupported)
      {
        if (length >= Vector128<byte>.Count * 2)
          num5 = SpanHelpers.UnalignedCountVector128(ref searchSpace);
      }
      else if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
        num5 = SpanHelpers.UnalignedCountVector(ref searchSpace);
      int num6;
      int num7;
      while (true)
      {
        while ((UIntPtr) (void*) num5 >= new UIntPtr(8))
        {
          num5 -= 8;
          uint num8 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num4);
          if ((int) num1 != (int) num8 && (int) num2 != (int) num8 && (int) num3 != (int) num8)
          {
            uint num9 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num4 + 1);
            if ((int) num1 != (int) num9 && (int) num2 != (int) num9 && (int) num3 != (int) num9)
            {
              uint num10 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num4 + 2);
              if ((int) num1 != (int) num10 && (int) num2 != (int) num10 && (int) num3 != (int) num10)
              {
                uint num11 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num4 + 3);
                if ((int) num1 != (int) num11 && (int) num2 != (int) num11 && (int) num3 != (int) num11)
                {
                  uint num12 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num4 + 4);
                  if ((int) num1 == (int) num12 || (int) num2 == (int) num12 || (int) num3 == (int) num12)
                    return (int) (void*) (num4 + 4);
                  uint num13 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num4 + 5);
                  if ((int) num1 == (int) num13 || (int) num2 == (int) num13 || (int) num3 == (int) num13)
                    return (int) (void*) (num4 + 5);
                  uint num14 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num4 + 6);
                  if ((int) num1 == (int) num14 || (int) num2 == (int) num14 || (int) num3 == (int) num14)
                    return (int) (void*) (num4 + 6);
                  uint num15 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num4 + 7);
                  if ((int) num1 == (int) num15 || (int) num2 == (int) num15 || (int) num3 == (int) num15)
                    return (int) (void*) (num4 + 7);
                  num4 += 8;
                }
                else
                  goto label_58;
              }
              else
                goto label_57;
            }
            else
              goto label_56;
          }
          else
            goto label_55;
        }
        if ((UIntPtr) (void*) num5 >= new UIntPtr(4))
        {
          num5 -= 4;
          uint num8 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num4);
          if ((int) num1 != (int) num8 && (int) num2 != (int) num8 && (int) num3 != (int) num8)
          {
            uint num9 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num4 + 1);
            if ((int) num1 != (int) num9 && (int) num2 != (int) num9 && (int) num3 != (int) num9)
            {
              uint num10 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num4 + 2);
              if ((int) num1 != (int) num10 && (int) num2 != (int) num10 && (int) num3 != (int) num10)
              {
                uint num11 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num4 + 3);
                if ((int) num1 != (int) num11 && (int) num2 != (int) num11 && (int) num3 != (int) num11)
                  num4 += 4;
                else
                  goto label_58;
              }
              else
                goto label_57;
            }
            else
              goto label_56;
          }
          else
            goto label_55;
        }
        while ((UIntPtr) (void*) num5 > UIntPtr.Zero)
        {
          num5 -= 1;
          uint num8 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, num4);
          if ((int) num1 != (int) num8 && (int) num2 != (int) num8 && (int) num3 != (int) num8)
            num4 += 1;
          else
            goto label_55;
        }
        if (Avx2.IsSupported)
        {
          if ((int) (void*) num4 < length)
          {
            IntPtr vector256SpanLength = SpanHelpers.GetByteVector256SpanLength(num4, length);
            if ((void*) vector256SpanLength > (void*) num4)
            {
              Vector256<byte> left1 = Vector256.Create(value0);
              Vector256<byte> left2 = Vector256.Create(value1);
              Vector256<byte> left3 = Vector256.Create(value2);
              do
              {
                Vector256<byte> right = SpanHelpers.LoadVector256(ref searchSpace, num4);
                num6 = Avx2.MoveMask(Avx2.Or(Avx2.Or(Avx2.CompareEqual(left1, right), Avx2.CompareEqual(left2, right)), Avx2.CompareEqual(left3, right)));
                if (num6 == 0)
                  num4 += Vector256<byte>.Count;
                else
                  goto label_30;
              }
              while ((void*) vector256SpanLength > (void*) num4);
            }
            if ((void*) SpanHelpers.GetByteVector128SpanLength(num4, length) > (void*) num4)
            {
              Vector128<byte> left1 = Vector128.Create(value0);
              Vector128<byte> left2 = Vector128.Create(value1);
              Vector128<byte> left3 = Vector128.Create(value2);
              Vector128<byte> right = SpanHelpers.LoadVector128(ref searchSpace, num4);
              num7 = Sse2.MoveMask(Sse2.Or(Sse2.Or(Sse2.CompareEqual(left1, right), Sse2.CompareEqual(left2, right)), Sse2.CompareEqual(left3, right)));
              if (num7 == 0)
                num4 += Vector128<byte>.Count;
              else
                goto label_34;
            }
            if ((int) (void*) num4 < length)
              num5 = (IntPtr) (length - (int) (void*) num4);
            else
              goto label_54;
          }
          else
            goto label_54;
        }
        else if (Sse2.IsSupported)
        {
          if ((int) (void*) num4 < length)
          {
            IntPtr vector128SpanLength = SpanHelpers.GetByteVector128SpanLength(num4, length);
            Vector128<byte> left1 = Vector128.Create(value0);
            Vector128<byte> left2 = Vector128.Create(value1);
            Vector128<byte> left3 = Vector128.Create(value2);
            while ((void*) vector128SpanLength > (void*) num4)
            {
              Vector128<byte> right = SpanHelpers.LoadVector128(ref searchSpace, num4);
              int num8 = Sse2.MoveMask(Sse2.Or(Sse2.Or(Sse2.CompareEqual(left1, right), Sse2.CompareEqual(left2, right)), Sse2.CompareEqual(left3, right)));
              if (num8 != 0)
                return (int) (void*) num4 + BitOperations.TrailingZeroCount(num8);
              num4 += Vector128<byte>.Count;
            }
            if ((int) (void*) num4 < length)
              num5 = (IntPtr) (length - (int) (void*) num4);
            else
              goto label_54;
          }
          else
            goto label_54;
        }
        else if (Vector.IsHardwareAccelerated && (int) (void*) num4 < length)
        {
          IntPtr vectorSpanLength = SpanHelpers.GetByteVectorSpanLength(num4, length);
          Vector<byte> right1 = new Vector<byte>(value0);
          Vector<byte> right2 = new Vector<byte>(value1);
          Vector<byte> right3 = new Vector<byte>(value2);
          while ((void*) vectorSpanLength > (void*) num4)
          {
            Vector<byte> left = SpanHelpers.LoadVector(ref searchSpace, num4);
            Vector<byte> vector = Vector.BitwiseOr<byte>(Vector.BitwiseOr<byte>(Vector.Equals<byte>(left, right1), Vector.Equals<byte>(left, right2)), Vector.Equals<byte>(left, right3));
            if (!Vector<byte>.Zero.Equals(vector))
              return (int) (void*) num4 + SpanHelpers.LocateFirstFoundByte(vector);
            num4 += Vector<byte>.Count;
          }
          if ((int) (void*) num4 < length)
            num5 = (IntPtr) (length - (int) (void*) num4);
          else
            goto label_54;
        }
        else
          goto label_54;
      }
label_30:
      return (int) (void*) num4 + BitOperations.TrailingZeroCount(num6);
label_34:
      return (int) (void*) num4 + BitOperations.TrailingZeroCount(num7);
label_54:
      return -1;
label_55:
      return (int) (void*) num4;
label_56:
      return (int) (void*) (num4 + 1);
label_57:
      return (int) (void*) (num4 + 2);
label_58:
      return (int) (void*) (num4 + 3);
    }

    public static unsafe int LastIndexOfAny(
      ref byte searchSpace,
      byte value0,
      byte value1,
      int length)
    {
      uint num1 = (uint) value0;
      uint num2 = (uint) value1;
      IntPtr byteOffset = (IntPtr) length;
      IntPtr num3 = (IntPtr) length;
      if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
        num3 = SpanHelpers.UnalignedCountVectorFromEnd(ref searchSpace, length);
      while (true)
      {
        while ((UIntPtr) (void*) num3 >= new UIntPtr(8))
        {
          num3 -= 8;
          byteOffset -= 8;
          uint num4 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 7);
          if ((int) num1 == (int) num4 || (int) num2 == (int) num4)
            return (int) (void*) (byteOffset + 7);
          uint num5 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 6);
          if ((int) num1 == (int) num5 || (int) num2 == (int) num5)
            return (int) (void*) (byteOffset + 6);
          uint num6 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 5);
          if ((int) num1 == (int) num6 || (int) num2 == (int) num6)
            return (int) (void*) (byteOffset + 5);
          uint num7 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 4);
          if ((int) num1 == (int) num7 || (int) num2 == (int) num7)
            return (int) (void*) (byteOffset + 4);
          uint num8 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3);
          if ((int) num1 != (int) num8 && (int) num2 != (int) num8)
          {
            uint num9 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2);
            if ((int) num1 != (int) num9 && (int) num2 != (int) num9)
            {
              uint num10 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1);
              if ((int) num1 != (int) num10 && (int) num2 != (int) num10)
              {
                uint num11 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
                if ((int) num1 == (int) num11 || (int) num2 == (int) num11)
                  goto label_27;
              }
              else
                goto label_28;
            }
            else
              goto label_29;
          }
          else
            goto label_30;
        }
        if ((UIntPtr) (void*) num3 >= new UIntPtr(4))
        {
          num3 -= 4;
          byteOffset -= 4;
          uint num4 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3);
          if ((int) num1 != (int) num4 && (int) num2 != (int) num4)
          {
            uint num5 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2);
            if ((int) num1 != (int) num5 && (int) num2 != (int) num5)
            {
              uint num6 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1);
              if ((int) num1 != (int) num6 && (int) num2 != (int) num6)
              {
                uint num7 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
                if ((int) num1 == (int) num7 || (int) num2 == (int) num7)
                  goto label_27;
              }
              else
                goto label_28;
            }
            else
              goto label_29;
          }
          else
            goto label_30;
        }
        while ((UIntPtr) (void*) num3 > UIntPtr.Zero)
        {
          num3 -= 1;
          byteOffset -= 1;
          uint num4 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
          if ((int) num1 == (int) num4 || (int) num2 == (int) num4)
            goto label_27;
        }
        if (Vector.IsHardwareAccelerated && (UIntPtr) (void*) byteOffset > UIntPtr.Zero)
        {
          IntPtr num4 = (IntPtr) ((int) (void*) byteOffset & ~(Vector<byte>.Count - 1));
          Vector<byte> right1 = new Vector<byte>(value0);
          Vector<byte> right2 = new Vector<byte>(value1);
          while ((UIntPtr) (void*) num4 > (UIntPtr) (Vector<byte>.Count - 1))
          {
            Vector<byte> left = SpanHelpers.LoadVector(ref searchSpace, byteOffset - Vector<byte>.Count);
            Vector<byte> vector = Vector.BitwiseOr<byte>(Vector.Equals<byte>(left, right1), Vector.Equals<byte>(left, right2));
            if (!Vector<byte>.Zero.Equals(vector))
              return (int) byteOffset - Vector<byte>.Count + SpanHelpers.LocateLastFoundByte(vector);
            byteOffset -= Vector<byte>.Count;
            num4 -= Vector<byte>.Count;
          }
          if ((UIntPtr) (void*) byteOffset > UIntPtr.Zero)
            num3 = byteOffset;
          else
            break;
        }
        else
          break;
      }
      return -1;
label_27:
      return (int) (void*) byteOffset;
label_28:
      return (int) (void*) (byteOffset + 1);
label_29:
      return (int) (void*) (byteOffset + 2);
label_30:
      return (int) (void*) (byteOffset + 3);
    }

    public static unsafe int LastIndexOfAny(
      ref byte searchSpace,
      byte value0,
      byte value1,
      byte value2,
      int length)
    {
      uint num1 = (uint) value0;
      uint num2 = (uint) value1;
      uint num3 = (uint) value2;
      IntPtr byteOffset = (IntPtr) length;
      IntPtr num4 = (IntPtr) length;
      if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
        num4 = SpanHelpers.UnalignedCountVectorFromEnd(ref searchSpace, length);
      while (true)
      {
        while ((UIntPtr) (void*) num4 >= new UIntPtr(8))
        {
          num4 -= 8;
          byteOffset -= 8;
          uint num5 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 7);
          if ((int) num1 == (int) num5 || (int) num2 == (int) num5 || (int) num3 == (int) num5)
            return (int) (void*) (byteOffset + 7);
          uint num6 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 6);
          if ((int) num1 == (int) num6 || (int) num2 == (int) num6 || (int) num3 == (int) num6)
            return (int) (void*) (byteOffset + 6);
          uint num7 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 5);
          if ((int) num1 == (int) num7 || (int) num2 == (int) num7 || (int) num3 == (int) num7)
            return (int) (void*) (byteOffset + 5);
          uint num8 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 4);
          if ((int) num1 == (int) num8 || (int) num2 == (int) num8 || (int) num3 == (int) num8)
            return (int) (void*) (byteOffset + 4);
          uint num9 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3);
          if ((int) num1 != (int) num9 && (int) num2 != (int) num9 && (int) num3 != (int) num9)
          {
            uint num10 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2);
            if ((int) num1 != (int) num10 && (int) num2 != (int) num10 && (int) num3 != (int) num10)
            {
              uint num11 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1);
              if ((int) num1 != (int) num11 && (int) num2 != (int) num11 && (int) num3 != (int) num11)
              {
                uint num12 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
                if ((int) num1 == (int) num12 || (int) num2 == (int) num12 || (int) num3 == (int) num12)
                  goto label_27;
              }
              else
                goto label_28;
            }
            else
              goto label_29;
          }
          else
            goto label_30;
        }
        if ((UIntPtr) (void*) num4 >= new UIntPtr(4))
        {
          num4 -= 4;
          byteOffset -= 4;
          uint num5 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3);
          if ((int) num1 != (int) num5 && (int) num2 != (int) num5 && (int) num3 != (int) num5)
          {
            uint num6 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2);
            if ((int) num1 != (int) num6 && (int) num2 != (int) num6 && (int) num3 != (int) num6)
            {
              uint num7 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1);
              if ((int) num1 != (int) num7 && (int) num2 != (int) num7 && (int) num3 != (int) num7)
              {
                uint num8 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
                if ((int) num1 == (int) num8 || (int) num2 == (int) num8 || (int) num3 == (int) num8)
                  goto label_27;
              }
              else
                goto label_28;
            }
            else
              goto label_29;
          }
          else
            goto label_30;
        }
        while ((UIntPtr) (void*) num4 > UIntPtr.Zero)
        {
          num4 -= 1;
          byteOffset -= 1;
          uint num5 = (uint) Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
          if ((int) num1 == (int) num5 || (int) num2 == (int) num5 || (int) num3 == (int) num5)
            goto label_27;
        }
        if (Vector.IsHardwareAccelerated && (UIntPtr) (void*) byteOffset > UIntPtr.Zero)
        {
          IntPtr num5 = (IntPtr) ((int) (void*) byteOffset & ~(Vector<byte>.Count - 1));
          Vector<byte> right1 = new Vector<byte>(value0);
          Vector<byte> right2 = new Vector<byte>(value1);
          Vector<byte> right3 = new Vector<byte>(value2);
          while ((UIntPtr) (void*) num5 > (UIntPtr) (Vector<byte>.Count - 1))
          {
            Vector<byte> left = SpanHelpers.LoadVector(ref searchSpace, byteOffset - Vector<byte>.Count);
            Vector<byte> vector = Vector.BitwiseOr<byte>(Vector.BitwiseOr<byte>(Vector.Equals<byte>(left, right1), Vector.Equals<byte>(left, right2)), Vector.Equals<byte>(left, right3));
            if (!Vector<byte>.Zero.Equals(vector))
              return (int) byteOffset - Vector<byte>.Count + SpanHelpers.LocateLastFoundByte(vector);
            byteOffset -= Vector<byte>.Count;
            num5 -= Vector<byte>.Count;
          }
          if ((UIntPtr) (void*) byteOffset > UIntPtr.Zero)
            num4 = byteOffset;
          else
            break;
        }
        else
          break;
      }
      return -1;
label_27:
      return (int) (void*) byteOffset;
label_28:
      return (int) (void*) (byteOffset + 1);
label_29:
      return (int) (void*) (byteOffset + 2);
label_30:
      return (int) (void*) (byteOffset + 3);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe bool SequenceEqual(ref byte first, ref byte second, ulong length)
    {
      if (!Unsafe.AreSame<byte>(ref first, ref second))
      {
        IntPtr num1 = (IntPtr) 0;
        IntPtr num2 = (IntPtr) (void*) length;
        if (Vector.IsHardwareAccelerated && (UIntPtr) (void*) num2 >= (UIntPtr) Vector<byte>.Count)
        {
          IntPtr offset = num2 - Vector<byte>.Count;
          while ((void*) offset > (void*) num1)
          {
            if (!(SpanHelpers.LoadVector(ref first, num1) != SpanHelpers.LoadVector(ref second, num1)))
              num1 += Vector<byte>.Count;
            else
              goto label_17;
          }
          return SpanHelpers.LoadVector(ref first, offset) == SpanHelpers.LoadVector(ref second, offset);
        }
        if ((UIntPtr) (void*) num2 >= (UIntPtr) sizeof (UIntPtr))
        {
          IntPtr offset = num2 - sizeof (UIntPtr);
          while ((void*) offset > (void*) num1)
          {
            if (!(SpanHelpers.LoadUIntPtr(ref first, num1) != SpanHelpers.LoadUIntPtr(ref second, num1)))
              num1 += sizeof (UIntPtr);
            else
              goto label_17;
          }
          return SpanHelpers.LoadUIntPtr(ref first, offset) == SpanHelpers.LoadUIntPtr(ref second, offset);
        }
        while ((void*) num2 > (void*) num1)
        {
          if ((int) Unsafe.AddByteOffset<byte>(ref first, num1) == (int) Unsafe.AddByteOffset<byte>(ref second, num1))
            num1 += 1;
          else
            goto label_17;
        }
        goto label_16;
label_17:
        return false;
      }
label_16:
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int LocateFirstFoundByte(Vector<byte> match)
    {
      Vector<ulong> vector = Vector.AsVectorUInt64<byte>(match);
      ulong match1 = 0;
      int index;
      for (index = 0; index < Vector<ulong>.Count; ++index)
      {
        match1 = vector[index];
        if (match1 != 0UL)
          break;
      }
      return index * 8 + SpanHelpers.LocateFirstFoundByte(match1);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe int SequenceCompareTo(
      ref byte first,
      int firstLength,
      ref byte second,
      int secondLength)
    {
      if (!Unsafe.AreSame<byte>(ref first, ref second))
      {
        IntPtr num1 = (IntPtr) (firstLength < secondLength ? firstLength : secondLength);
        IntPtr num2 = (IntPtr) 0;
        IntPtr num3 = (IntPtr) (void*) num1;
        if (Avx2.IsSupported)
        {
          if ((UIntPtr) (void*) num3 >= (UIntPtr) Vector256<byte>.Count)
          {
            IntPtr num4 = num3 - Vector256<byte>.Count;
            uint num5;
            while ((void*) num4 > (void*) num2)
            {
              num5 = (uint) Avx2.MoveMask(Avx2.CompareEqual(SpanHelpers.LoadVector256(ref first, num2), SpanHelpers.LoadVector256(ref second, num2)));
              if (num5 == uint.MaxValue)
                num2 += Vector256<byte>.Count;
              else
                goto label_8;
            }
            num2 = num4;
            num5 = (uint) Avx2.MoveMask(Avx2.CompareEqual(SpanHelpers.LoadVector256(ref first, num2), SpanHelpers.LoadVector256(ref second, num2)));
            if (num5 == uint.MaxValue)
              goto label_35;
label_8:
            uint num6 = ~num5;
            IntPtr byteOffset = (IntPtr) ((int) (void*) num2 + BitOperations.TrailingZeroCount((int) num6));
            return Unsafe.AddByteOffset<byte>(ref first, byteOffset).CompareTo(Unsafe.AddByteOffset<byte>(ref second, byteOffset));
          }
          if ((UIntPtr) (void*) num3 >= (UIntPtr) Vector128<byte>.Count)
          {
            IntPtr num4 = num3 - Vector128<byte>.Count;
            uint num5;
            if ((void*) num4 > (void*) num2)
            {
              num5 = (uint) Sse2.MoveMask(Sse2.CompareEqual(SpanHelpers.LoadVector128(ref first, num2), SpanHelpers.LoadVector128(ref second, num2)));
              if (num5 == (uint) ushort.MaxValue)
              {
                IntPtr num6 = num2 + Vector128<byte>.Count;
              }
              else
                goto label_14;
            }
            num2 = num4;
            num5 = (uint) Sse2.MoveMask(Sse2.CompareEqual(SpanHelpers.LoadVector128(ref first, num2), SpanHelpers.LoadVector128(ref second, num2)));
            if (num5 == (uint) ushort.MaxValue)
              goto label_35;
label_14:
            uint num7 = ~num5;
            IntPtr byteOffset = (IntPtr) ((int) (void*) num2 + BitOperations.TrailingZeroCount((int) num7));
            return Unsafe.AddByteOffset<byte>(ref first, byteOffset).CompareTo(Unsafe.AddByteOffset<byte>(ref second, byteOffset));
          }
        }
        else if (Sse2.IsSupported)
        {
          if ((UIntPtr) (void*) num3 >= (UIntPtr) Vector128<byte>.Count)
          {
            IntPtr num4 = num3 - Vector128<byte>.Count;
            uint num5;
            while ((void*) num4 > (void*) num2)
            {
              num5 = (uint) Sse2.MoveMask(Sse2.CompareEqual(SpanHelpers.LoadVector128(ref first, num2), SpanHelpers.LoadVector128(ref second, num2)));
              if (num5 == (uint) ushort.MaxValue)
                num2 += Vector128<byte>.Count;
              else
                goto label_22;
            }
            num2 = num4;
            num5 = (uint) Sse2.MoveMask(Sse2.CompareEqual(SpanHelpers.LoadVector128(ref first, num2), SpanHelpers.LoadVector128(ref second, num2)));
            if (num5 == (uint) ushort.MaxValue)
              goto label_35;
label_22:
            uint num6 = ~num5;
            IntPtr byteOffset = (IntPtr) ((int) (void*) num2 + BitOperations.TrailingZeroCount((int) num6));
            return Unsafe.AddByteOffset<byte>(ref first, byteOffset).CompareTo(Unsafe.AddByteOffset<byte>(ref second, byteOffset));
          }
        }
        else if (Vector.IsHardwareAccelerated && (UIntPtr) (void*) num3 > (UIntPtr) Vector<byte>.Count)
        {
          IntPtr num4 = num3 - Vector<byte>.Count;
          while ((void*) num4 > (void*) num2 && !(SpanHelpers.LoadVector(ref first, num2) != SpanHelpers.LoadVector(ref second, num2)))
            num2 += Vector<byte>.Count;
          goto label_34;
        }
        if ((UIntPtr) (void*) num3 > (UIntPtr) sizeof (UIntPtr))
        {
          IntPtr num4 = num3 - sizeof (UIntPtr);
          while ((void*) num4 > (void*) num2 && !(SpanHelpers.LoadUIntPtr(ref first, num2) != SpanHelpers.LoadUIntPtr(ref second, num2)))
            num2 += sizeof (UIntPtr);
        }
label_34:
        while ((void*) num1 > (void*) num2)
        {
          int num4 = Unsafe.AddByteOffset<byte>(ref first, num2).CompareTo(Unsafe.AddByteOffset<byte>(ref second, num2));
          if (num4 != 0)
            return num4;
          num2 += 1;
        }
      }
label_35:
      return firstLength - secondLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int LocateLastFoundByte(Vector<byte> match)
    {
      Vector<ulong> vector = Vector.AsVectorUInt64<byte>(match);
      ulong match1 = 0;
      int index;
      for (index = Vector<ulong>.Count - 1; index >= 0; --index)
      {
        match1 = vector[index];
        if (match1 != 0UL)
          break;
      }
      return index * 8 + SpanHelpers.LocateLastFoundByte(match1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int LocateFirstFoundByte(ulong match)
    {
      return Bmi1.X64.IsSupported ? (int) (Bmi1.X64.TrailingZeroCount(match) >> 3) : (int) ((match ^ match - 1UL) * 283686952306184UL >> 57);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int LocateLastFoundByte(ulong match)
    {
      return 7 - (BitOperations.LeadingZeroCount(match) >> 3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UIntPtr LoadUIntPtr(ref byte start, IntPtr offset)
    {
      return Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset<byte>(ref start, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector<byte> LoadVector(ref byte start, IntPtr offset)
    {
      return Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset<byte>(ref start, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> LoadVector128(ref byte start, IntPtr offset)
    {
      return Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.AddByteOffset<byte>(ref start, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<byte> LoadVector256(ref byte start, IntPtr offset)
    {
      return Unsafe.ReadUnaligned<Vector256<byte>>(ref Unsafe.AddByteOffset<byte>(ref start, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe IntPtr GetByteVectorSpanLength(IntPtr offset, int length)
    {
      return (IntPtr) (length - (int) (void*) offset & ~(Vector<byte>.Count - 1));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe IntPtr GetByteVector128SpanLength(IntPtr offset, int length)
    {
      return (IntPtr) (length - (int) (void*) offset & ~(Vector128<byte>.Count - 1));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe IntPtr GetByteVector256SpanLength(IntPtr offset, int length)
    {
      return (IntPtr) (length - (int) (void*) offset & ~(Vector256<byte>.Count - 1));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe IntPtr UnalignedCountVector(ref byte searchSpace)
    {
      return (IntPtr) (Vector<byte>.Count - ((int) Unsafe.AsPointer<byte>(ref searchSpace) & Vector<byte>.Count - 1) & Vector<byte>.Count - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe IntPtr UnalignedCountVector128(ref byte searchSpace)
    {
      return (IntPtr) (Vector128<byte>.Count - ((int) Unsafe.AsPointer<byte>(ref searchSpace) & Vector128<byte>.Count - 1) & Vector128<byte>.Count - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe IntPtr UnalignedCountVectorFromEnd(ref byte searchSpace, int length)
    {
      int num = (int) Unsafe.AsPointer<byte>(ref searchSpace) & Vector<byte>.Count - 1;
      return (IntPtr) ((length & Vector<byte>.Count - 1) + num & Vector<byte>.Count - 1);
    }

    public static int IndexOf(
      ref char searchSpace,
      int searchSpaceLength,
      ref char value,
      int valueLength)
    {
      if (valueLength == 0)
        return 0;
      char ch = value;
      ref char local = ref Unsafe.Add<char>(ref value, 1);
      int num1 = valueLength - 1;
      int length = searchSpaceLength - num1;
      int elementOffset = 0;
      while (length > 0)
      {
        int num2 = SpanHelpers.IndexOf(ref Unsafe.Add<char>(ref searchSpace, elementOffset), ch, length);
        if (num2 != -1)
        {
          int num3 = length - num2;
          int num4 = elementOffset + num2;
          if (num3 > 0)
          {
            if (SpanHelpers.SequenceEqual(ref Unsafe.As<char, byte>(ref Unsafe.Add<char>(ref searchSpace, num4 + 1)), ref Unsafe.As<char, byte>(ref local), (ulong) num1 * 2UL))
              return num4;
            length = num3 - 1;
            elementOffset = num4 + 1;
          }
          else
            break;
        }
        else
          break;
      }
      return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe int SequenceCompareTo(
      ref char first,
      int firstLength,
      ref char second,
      int secondLength)
    {
      int num1 = firstLength - secondLength;
      if (!Unsafe.AreSame<char>(ref first, ref second))
      {
        IntPtr num2 = (IntPtr) (firstLength < secondLength ? firstLength : secondLength);
        IntPtr elementOffset = (IntPtr) 0;
        if ((UIntPtr) (void*) num2 >= (UIntPtr) (sizeof (UIntPtr) / 2))
        {
          if (Vector.IsHardwareAccelerated && (UIntPtr) (void*) num2 >= (UIntPtr) Vector<ushort>.Count)
          {
            IntPtr num3 = num2 - Vector<ushort>.Count;
            while (!(Unsafe.ReadUnaligned<Vector<ushort>>(ref Unsafe.As<char, byte>(ref Unsafe.Add<char>(ref first, elementOffset))) != Unsafe.ReadUnaligned<Vector<ushort>>(ref Unsafe.As<char, byte>(ref Unsafe.Add<char>(ref second, elementOffset)))))
            {
              elementOffset += Vector<ushort>.Count;
              if ((void*) num3 < (void*) elementOffset)
                break;
            }
          }
          while ((void*) num2 >= (void*) (elementOffset + sizeof (UIntPtr) / 2) && !(Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.As<char, byte>(ref Unsafe.Add<char>(ref first, elementOffset))) != Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.As<char, byte>(ref Unsafe.Add<char>(ref second, elementOffset)))))
            elementOffset += sizeof (UIntPtr) / 2;
        }
        if (sizeof (UIntPtr) > 4 && (void*) num2 >= (void*) (elementOffset + 2) && Unsafe.ReadUnaligned<int>(ref Unsafe.As<char, byte>(ref Unsafe.Add<char>(ref first, elementOffset))) == Unsafe.ReadUnaligned<int>(ref Unsafe.As<char, byte>(ref Unsafe.Add<char>(ref second, elementOffset))))
          elementOffset += 2;
        while ((void*) elementOffset < (void*) num2)
        {
          int num3 = Unsafe.Add<char>(ref first, elementOffset).CompareTo(Unsafe.Add<char>(ref second, elementOffset));
          if (num3 != 0)
            return num3;
          elementOffset += 1;
        }
      }
      return num1;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe bool Contains(ref char searchSpace, char value, int length)
    {
      fixed (char* chPtr1 = &searchSpace)
      {
        char* chPtr2 = chPtr1;
        char* chPtr3 = chPtr2 + length;
        if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
          length = Vector<ushort>.Count - ((int) chPtr2 & Unsafe.SizeOf<Vector<ushort>>() - 1) / 2 & Vector<ushort>.Count - 1;
        while (true)
        {
          while (length >= 4)
          {
            length -= 4;
            if ((int) value != (int) *chPtr2 && (int) value != (int) chPtr2[1] && ((int) value != (int) chPtr2[2] && (int) value != (int) chPtr2[3]))
              chPtr2 += 4;
            else
              goto label_16;
          }
          while (length > 0)
          {
            --length;
            if ((int) value != (int) *chPtr2)
              ++chPtr2;
            else
              goto label_16;
          }
          if (Vector.IsHardwareAccelerated && chPtr2 < chPtr3)
          {
            length = (int) (chPtr3 - chPtr2 & (long) ~(Vector<ushort>.Count - 1));
            Vector<ushort> left = new Vector<ushort>((ushort) value);
            for (; length > 0; length -= Vector<ushort>.Count)
            {
              if (Vector<ushort>.Zero.Equals(Vector.Equals<ushort>(left, Unsafe.Read<Vector<ushort>>((void*) chPtr2))))
                chPtr2 += Vector<ushort>.Count;
              else
                goto label_16;
            }
            if (chPtr2 < chPtr3)
              length = (int) (chPtr3 - chPtr2);
            else
              break;
          }
          else
            break;
        }
        return false;
label_16:
        return true;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe int IndexOf(ref char searchSpace, char value, int length)
    {
      long num1 = 0;
      long num2 = (long) length;
      if (((int) Unsafe.AsPointer<char>(ref searchSpace) & 1) == 0)
      {
        if (Sse2.IsSupported)
        {
          if (length >= Vector128<ushort>.Count * 2)
            num2 = SpanHelpers.UnalignedCountVector128(ref searchSpace);
        }
        else if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
          num2 = SpanHelpers.UnalignedCountVector(ref searchSpace);
      }
      int num3;
      int num4;
      int num5;
      int num6;
      Vector<ushort> vector;
      while (true)
      {
        for (; num2 >= 4L; num2 -= 4L)
        {
          ref char local = ref SpanHelpers.Add(ref searchSpace, num1);
          if ((int) value != (int) local)
          {
            if ((int) value == (int) SpanHelpers.Add(ref local, 1L))
              return (int) (num1 + 1L);
            if ((int) value == (int) SpanHelpers.Add(ref local, 2L))
              return (int) (num1 + 2L);
            if ((int) value == (int) SpanHelpers.Add(ref local, 3L))
              return (int) (num1 + 3L);
            num1 += 4L;
          }
          else
            goto label_53;
        }
        for (; num2 > 0L; --num2)
        {
          if ((int) value != (int) SpanHelpers.Add(ref searchSpace, num1))
            ++num1;
          else
            goto label_53;
        }
        if (Avx2.IsSupported)
        {
          if (num1 < (long) length)
          {
            if (((long) Unsafe.AsPointer<char>(ref Unsafe.Add<char>(ref searchSpace, (IntPtr) num1)) & (long) (Vector256<byte>.Count - 1)) != 0L)
            {
              num3 = Sse2.MoveMask(Sse2.CompareEqual(Vector128.Create((ushort) value), SpanHelpers.LoadVector128(ref searchSpace, num1)).AsByte<ushort>());
              if (num3 == 0)
                num1 += (long) Vector128<ushort>.Count;
              else
                break;
            }
            long vector256SpanLength = SpanHelpers.GetCharVector256SpanLength(num1, (long) length);
            if (vector256SpanLength > 0L)
            {
              Vector256<ushort> left = Vector256.Create((ushort) value);
              do
              {
                Vector256<ushort> right = SpanHelpers.LoadVector256(ref searchSpace, num1);
                num4 = Avx2.MoveMask(Avx2.CompareEqual(left, right).AsByte<ushort>());
                if (num4 == 0)
                {
                  num1 += (long) Vector256<ushort>.Count;
                  vector256SpanLength -= (long) Vector256<ushort>.Count;
                }
                else
                  goto label_25;
              }
              while (vector256SpanLength > 0L);
            }
            if (SpanHelpers.GetCharVector128SpanLength(num1, (long) length) > 0L)
            {
              num5 = Sse2.MoveMask(Sse2.CompareEqual(Vector128.Create((ushort) value), SpanHelpers.LoadVector128(ref searchSpace, num1)).AsByte<ushort>());
              if (num5 == 0)
                num1 += (long) Vector128<ushort>.Count;
              else
                goto label_29;
            }
            if (num1 < (long) length)
              num2 = (long) length - num1;
            else
              goto label_49;
          }
          else
            goto label_49;
        }
        else if (Sse2.IsSupported)
        {
          if (num1 < (long) length)
          {
            long vector128SpanLength = SpanHelpers.GetCharVector128SpanLength(num1, (long) length);
            if (vector128SpanLength > 0L)
            {
              Vector128<ushort> left = Vector128.Create((ushort) value);
              do
              {
                Vector128<ushort> right = SpanHelpers.LoadVector128(ref searchSpace, num1);
                num6 = Sse2.MoveMask(Sse2.CompareEqual(left, right).AsByte<ushort>());
                if (num6 == 0)
                {
                  num1 += (long) Vector128<ushort>.Count;
                  vector128SpanLength -= (long) Vector128<ushort>.Count;
                }
                else
                  goto label_38;
              }
              while (vector128SpanLength > 0L);
            }
            if (num1 < (long) length)
              num2 = (long) length - num1;
            else
              goto label_49;
          }
          else
            goto label_49;
        }
        else if (Vector.IsHardwareAccelerated && num1 < (long) length)
        {
          long vectorSpanLength = SpanHelpers.GetCharVectorSpanLength(num1, (long) length);
          if (vectorSpanLength > 0L)
          {
            Vector<ushort> left = new Vector<ushort>((ushort) value);
            do
            {
              vector = Vector.Equals<ushort>(left, SpanHelpers.LoadVector(ref searchSpace, num1));
              if (Vector<ushort>.Zero.Equals(vector))
              {
                num1 += (long) Vector<ushort>.Count;
                vectorSpanLength -= (long) Vector<ushort>.Count;
              }
              else
                goto label_46;
            }
            while (vectorSpanLength > 0L);
          }
          if (num1 < (long) length)
            num2 = (long) length - num1;
          else
            goto label_49;
        }
        else
          goto label_49;
      }
      return (int) (num1 + (long) (BitOperations.TrailingZeroCount(num3) / 2));
label_25:
      return (int) (num1 + (long) (BitOperations.TrailingZeroCount(num4) / 2));
label_29:
      return (int) (num1 + (long) (BitOperations.TrailingZeroCount(num5) / 2));
label_38:
      return (int) (num1 + (long) (BitOperations.TrailingZeroCount(num6) / 2));
label_46:
      return (int) (num1 + (long) SpanHelpers.LocateFirstFoundChar(vector));
label_49:
      return -1;
label_53:
      return (int) num1;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe int IndexOfAny(
      ref char searchSpace,
      char value0,
      char value1,
      int length)
    {
      fixed (char* chPtr1 = &searchSpace)
      {
        char* chPtr2 = chPtr1;
        char* chPtr3 = chPtr2 + length;
        if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
          length = Vector<ushort>.Count - ((int) chPtr2 & Unsafe.SizeOf<Vector<ushort>>() - 1) / 2 & Vector<ushort>.Count - 1;
        while (true)
        {
          while (length >= 4)
          {
            length -= 4;
            if ((int) *chPtr2 != (int) value0 && (int) *chPtr2 != (int) value1)
            {
              if ((int) chPtr2[1] != (int) value0 && (int) chPtr2[1] != (int) value1)
              {
                if ((int) chPtr2[2] != (int) value0 && (int) chPtr2[2] != (int) value1)
                {
                  if ((int) chPtr2[3] != (int) value0 && (int) chPtr2[3] != (int) value1)
                  {
                    chPtr2 += 4;
                    continue;
                  }
                  ++chPtr2;
                }
                ++chPtr2;
              }
              ++chPtr2;
              goto label_23;
            }
            else
              goto label_23;
          }
          while (length > 0)
          {
            --length;
            if ((int) *chPtr2 != (int) value0 && (int) *chPtr2 != (int) value1)
              ++chPtr2;
            else
              goto label_23;
          }
          if (Vector.IsHardwareAccelerated && chPtr2 < chPtr3)
          {
            length = (int) (chPtr3 - chPtr2 & (long) ~(Vector<ushort>.Count - 1));
            Vector<ushort> right1 = new Vector<ushort>((ushort) value0);
            Vector<ushort> right2 = new Vector<ushort>((ushort) value1);
            for (; length > 0; length -= Vector<ushort>.Count)
            {
              Vector<ushort> left = Unsafe.Read<Vector<ushort>>((void*) chPtr2);
              Vector<ushort> vector = Vector.BitwiseOr<ushort>(Vector.Equals<ushort>(left, right1), Vector.Equals<ushort>(left, right2));
              if (!Vector<ushort>.Zero.Equals(vector))
                return (int) (chPtr2 - chPtr1) + SpanHelpers.LocateFirstFoundChar(vector);
              chPtr2 += Vector<ushort>.Count;
            }
            if (chPtr2 < chPtr3)
              length = (int) (chPtr3 - chPtr2);
            else
              break;
          }
          else
            break;
        }
        return -1;
label_23:
        return (int) (chPtr2 - chPtr1);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe int IndexOfAny(
      ref char searchSpace,
      char value0,
      char value1,
      char value2,
      int length)
    {
      fixed (char* chPtr1 = &searchSpace)
      {
        char* chPtr2 = chPtr1;
        char* chPtr3 = chPtr2 + length;
        if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
          length = Vector<ushort>.Count - ((int) chPtr2 & Unsafe.SizeOf<Vector<ushort>>() - 1) / 2 & Vector<ushort>.Count - 1;
        while (true)
        {
          while (length >= 4)
          {
            length -= 4;
            if ((int) *chPtr2 != (int) value0 && (int) *chPtr2 != (int) value1 && (int) *chPtr2 != (int) value2)
            {
              if ((int) chPtr2[1] != (int) value0 && (int) chPtr2[1] != (int) value1 && (int) chPtr2[1] != (int) value2)
              {
                if ((int) chPtr2[2] != (int) value0 && (int) chPtr2[2] != (int) value1 && (int) chPtr2[2] != (int) value2)
                {
                  if ((int) chPtr2[3] != (int) value0 && (int) chPtr2[3] != (int) value1 && (int) chPtr2[3] != (int) value2)
                  {
                    chPtr2 += 4;
                    continue;
                  }
                  ++chPtr2;
                }
                ++chPtr2;
              }
              ++chPtr2;
              goto label_23;
            }
            else
              goto label_23;
          }
          while (length > 0)
          {
            --length;
            if ((int) *chPtr2 != (int) value0 && (int) *chPtr2 != (int) value1 && (int) *chPtr2 != (int) value2)
              ++chPtr2;
            else
              goto label_23;
          }
          if (Vector.IsHardwareAccelerated && chPtr2 < chPtr3)
          {
            length = (int) (chPtr3 - chPtr2 & (long) ~(Vector<ushort>.Count - 1));
            Vector<ushort> right1 = new Vector<ushort>((ushort) value0);
            Vector<ushort> right2 = new Vector<ushort>((ushort) value1);
            Vector<ushort> right3 = new Vector<ushort>((ushort) value2);
            for (; length > 0; length -= Vector<ushort>.Count)
            {
              Vector<ushort> left = Unsafe.Read<Vector<ushort>>((void*) chPtr2);
              Vector<ushort> vector = Vector.BitwiseOr<ushort>(Vector.BitwiseOr<ushort>(Vector.Equals<ushort>(left, right1), Vector.Equals<ushort>(left, right2)), Vector.Equals<ushort>(left, right3));
              if (!Vector<ushort>.Zero.Equals(vector))
                return (int) (chPtr2 - chPtr1) + SpanHelpers.LocateFirstFoundChar(vector);
              chPtr2 += Vector<ushort>.Count;
            }
            if (chPtr2 < chPtr3)
              length = (int) (chPtr3 - chPtr2);
            else
              break;
          }
          else
            break;
        }
        return -1;
label_23:
        return (int) (chPtr2 - chPtr1);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe int IndexOfAny(
      ref char searchSpace,
      char value0,
      char value1,
      char value2,
      char value3,
      int length)
    {
      fixed (char* chPtr1 = &searchSpace)
      {
        char* chPtr2 = chPtr1;
        char* chPtr3 = chPtr2 + length;
        if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
          length = Vector<ushort>.Count - ((int) chPtr2 & Unsafe.SizeOf<Vector<ushort>>() - 1) / 2 & Vector<ushort>.Count - 1;
        while (true)
        {
          while (length >= 4)
          {
            length -= 4;
            if ((int) *chPtr2 != (int) value0 && (int) *chPtr2 != (int) value1 && ((int) *chPtr2 != (int) value2 && (int) *chPtr2 != (int) value3))
            {
              if ((int) chPtr2[1] != (int) value0 && (int) chPtr2[1] != (int) value1 && ((int) chPtr2[1] != (int) value2 && (int) chPtr2[1] != (int) value3))
              {
                if ((int) chPtr2[2] != (int) value0 && (int) chPtr2[2] != (int) value1 && ((int) chPtr2[2] != (int) value2 && (int) chPtr2[2] != (int) value3))
                {
                  if ((int) chPtr2[3] != (int) value0 && (int) chPtr2[3] != (int) value1 && ((int) chPtr2[3] != (int) value2 && (int) chPtr2[3] != (int) value3))
                  {
                    chPtr2 += 4;
                    continue;
                  }
                  ++chPtr2;
                }
                ++chPtr2;
              }
              ++chPtr2;
              goto label_23;
            }
            else
              goto label_23;
          }
          while (length > 0)
          {
            --length;
            if ((int) *chPtr2 != (int) value0 && (int) *chPtr2 != (int) value1 && ((int) *chPtr2 != (int) value2 && (int) *chPtr2 != (int) value3))
              ++chPtr2;
            else
              goto label_23;
          }
          if (Vector.IsHardwareAccelerated && chPtr2 < chPtr3)
          {
            length = (int) (chPtr3 - chPtr2 & (long) ~(Vector<ushort>.Count - 1));
            Vector<ushort> right1 = new Vector<ushort>((ushort) value0);
            Vector<ushort> right2 = new Vector<ushort>((ushort) value1);
            Vector<ushort> right3 = new Vector<ushort>((ushort) value2);
            Vector<ushort> right4 = new Vector<ushort>((ushort) value3);
            for (; length > 0; length -= Vector<ushort>.Count)
            {
              Vector<ushort> left = Unsafe.Read<Vector<ushort>>((void*) chPtr2);
              Vector<ushort> vector = Vector.BitwiseOr<ushort>(Vector.BitwiseOr<ushort>(Vector.BitwiseOr<ushort>(Vector.Equals<ushort>(left, right1), Vector.Equals<ushort>(left, right2)), Vector.Equals<ushort>(left, right3)), Vector.Equals<ushort>(left, right4));
              if (!Vector<ushort>.Zero.Equals(vector))
                return (int) (chPtr2 - chPtr1) + SpanHelpers.LocateFirstFoundChar(vector);
              chPtr2 += Vector<ushort>.Count;
            }
            if (chPtr2 < chPtr3)
              length = (int) (chPtr3 - chPtr2);
            else
              break;
          }
          else
            break;
        }
        return -1;
label_23:
        return (int) (chPtr2 - chPtr1);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe int IndexOfAny(
      ref char searchSpace,
      char value0,
      char value1,
      char value2,
      char value3,
      char value4,
      int length)
    {
      fixed (char* chPtr1 = &searchSpace)
      {
        char* chPtr2 = chPtr1;
        char* chPtr3 = chPtr2 + length;
        if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
          length = Vector<ushort>.Count - ((int) chPtr2 & Unsafe.SizeOf<Vector<ushort>>() - 1) / 2 & Vector<ushort>.Count - 1;
        while (true)
        {
          while (length >= 4)
          {
            length -= 4;
            if ((int) *chPtr2 != (int) value0 && (int) *chPtr2 != (int) value1 && ((int) *chPtr2 != (int) value2 && (int) *chPtr2 != (int) value3) && (int) *chPtr2 != (int) value4)
            {
              if ((int) chPtr2[1] != (int) value0 && (int) chPtr2[1] != (int) value1 && ((int) chPtr2[1] != (int) value2 && (int) chPtr2[1] != (int) value3) && (int) chPtr2[1] != (int) value4)
              {
                if ((int) chPtr2[2] != (int) value0 && (int) chPtr2[2] != (int) value1 && ((int) chPtr2[2] != (int) value2 && (int) chPtr2[2] != (int) value3) && (int) chPtr2[2] != (int) value4)
                {
                  if ((int) chPtr2[3] != (int) value0 && (int) chPtr2[3] != (int) value1 && ((int) chPtr2[3] != (int) value2 && (int) chPtr2[3] != (int) value3) && (int) chPtr2[3] != (int) value4)
                  {
                    chPtr2 += 4;
                    continue;
                  }
                  ++chPtr2;
                }
                ++chPtr2;
              }
              ++chPtr2;
              goto label_23;
            }
            else
              goto label_23;
          }
          while (length > 0)
          {
            --length;
            if ((int) *chPtr2 != (int) value0 && (int) *chPtr2 != (int) value1 && ((int) *chPtr2 != (int) value2 && (int) *chPtr2 != (int) value3) && (int) *chPtr2 != (int) value4)
              ++chPtr2;
            else
              goto label_23;
          }
          if (Vector.IsHardwareAccelerated && chPtr2 < chPtr3)
          {
            length = (int) (chPtr3 - chPtr2 & (long) ~(Vector<ushort>.Count - 1));
            Vector<ushort> right1 = new Vector<ushort>((ushort) value0);
            Vector<ushort> right2 = new Vector<ushort>((ushort) value1);
            Vector<ushort> right3 = new Vector<ushort>((ushort) value2);
            Vector<ushort> right4 = new Vector<ushort>((ushort) value3);
            Vector<ushort> right5 = new Vector<ushort>((ushort) value4);
            for (; length > 0; length -= Vector<ushort>.Count)
            {
              Vector<ushort> left = Unsafe.Read<Vector<ushort>>((void*) chPtr2);
              Vector<ushort> vector = Vector.BitwiseOr<ushort>(Vector.BitwiseOr<ushort>(Vector.BitwiseOr<ushort>(Vector.BitwiseOr<ushort>(Vector.Equals<ushort>(left, right1), Vector.Equals<ushort>(left, right2)), Vector.Equals<ushort>(left, right3)), Vector.Equals<ushort>(left, right4)), Vector.Equals<ushort>(left, right5));
              if (!Vector<ushort>.Zero.Equals(vector))
                return (int) (chPtr2 - chPtr1) + SpanHelpers.LocateFirstFoundChar(vector);
              chPtr2 += Vector<ushort>.Count;
            }
            if (chPtr2 < chPtr3)
              length = (int) (chPtr3 - chPtr2);
            else
              break;
          }
          else
            break;
        }
        return -1;
label_23:
        return (int) (chPtr2 - chPtr1);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe int LastIndexOf(ref char searchSpace, char value, int length)
    {
      fixed (char* chPtr1 = &searchSpace)
      {
        char* chPtr2 = chPtr1 + length;
        char* chPtr3 = chPtr1;
        if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
          length = ((int) chPtr2 & Unsafe.SizeOf<Vector<ushort>>() - 1) / 2;
        while (true)
        {
          while (length >= 4)
          {
            length -= 4;
            chPtr2 -= 4;
            if ((int) chPtr2[3] == (int) value)
              return (int) (chPtr2 - chPtr3) + 3;
            if ((int) chPtr2[2] == (int) value)
              return (int) (chPtr2 - chPtr3) + 2;
            if ((int) chPtr2[1] == (int) value)
              return (int) (chPtr2 - chPtr3) + 1;
            if ((int) *chPtr2 == (int) value)
              goto label_18;
          }
          while (length > 0)
          {
            --length;
            --chPtr2;
            if ((int) *chPtr2 == (int) value)
              goto label_18;
          }
          if (Vector.IsHardwareAccelerated && chPtr2 > chPtr3)
          {
            length = (int) (chPtr2 - chPtr3 & (long) ~(Vector<ushort>.Count - 1));
            Vector<ushort> left = new Vector<ushort>((ushort) value);
            for (; length > 0; length -= Vector<ushort>.Count)
            {
              char* chPtr4 = chPtr2 - Vector<ushort>.Count;
              Vector<ushort> vector = Vector.Equals<ushort>(left, Unsafe.Read<Vector<ushort>>((void*) chPtr4));
              if (!Vector<ushort>.Zero.Equals(vector))
                return (int) (chPtr4 - chPtr3) + SpanHelpers.LocateLastFoundChar(vector);
              chPtr2 -= Vector<ushort>.Count;
            }
            if (chPtr2 > chPtr3)
              length = (int) (chPtr2 - chPtr3);
            else
              break;
          }
          else
            break;
        }
        return -1;
label_18:
        return (int) (chPtr2 - chPtr3);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int LocateFirstFoundChar(Vector<ushort> match)
    {
      Vector<ulong> vector = Vector.AsVectorUInt64<ushort>(match);
      ulong match1 = 0;
      int index;
      for (index = 0; index < Vector<ulong>.Count; ++index)
      {
        match1 = vector[index];
        if (match1 != 0UL)
          break;
      }
      return index * 4 + SpanHelpers.LocateFirstFoundChar(match1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int LocateFirstFoundChar(ulong match)
    {
      return Bmi1.X64.IsSupported ? (int) (Bmi1.X64.TrailingZeroCount(match) >> 4) : (int) ((match ^ match - 1UL) * 4295098372UL >> 49);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int LocateLastFoundChar(Vector<ushort> match)
    {
      Vector<ulong> vector = Vector.AsVectorUInt64<ushort>(match);
      ulong match1 = 0;
      int index;
      for (index = Vector<ulong>.Count - 1; index >= 0; --index)
      {
        match1 = vector[index];
        if (match1 != 0UL)
          break;
      }
      return index * 4 + SpanHelpers.LocateLastFoundChar(match1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int LocateLastFoundChar(ulong match)
    {
      return 3 - (BitOperations.LeadingZeroCount(match) >> 4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref char Add(ref char source, long elementOffset)
    {
      return ref Unsafe.Add<char>(ref source, (IntPtr) elementOffset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector<ushort> LoadVector(ref char start, long offset)
    {
      return Unsafe.ReadUnaligned<Vector<ushort>>(ref Unsafe.As<char, byte>(ref Unsafe.Add<char>(ref start, (IntPtr) offset)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ushort> LoadVector128(ref char start, long offset)
    {
      return Unsafe.ReadUnaligned<Vector128<ushort>>(ref Unsafe.As<char, byte>(ref Unsafe.Add<char>(ref start, (IntPtr) offset)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<ushort> LoadVector256(ref char start, long offset)
    {
      return Unsafe.ReadUnaligned<Vector256<ushort>>(ref Unsafe.As<char, byte>(ref Unsafe.Add<char>(ref start, (IntPtr) offset)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetCharVectorSpanLength(long offset, long length)
    {
      return length - offset & (long) ~(Vector<ushort>.Count - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetCharVector128SpanLength(long offset, long length)
    {
      return length - offset & (long) ~(Vector128<ushort>.Count - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetCharVector256SpanLength(long offset, long length)
    {
      return length - offset & (long) ~(Vector256<ushort>.Count - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe long UnalignedCountVector(ref char searchSpace)
    {
      return (long) (uint) (-(int) Unsafe.AsPointer<char>(ref searchSpace) / 2) & (long) (Vector<ushort>.Count - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe long UnalignedCountVector128(ref char searchSpace)
    {
      return (long) (uint) (-(int) Unsafe.AsPointer<char>(ref searchSpace) / 2) & (long) (Vector128<ushort>.Count - 1);
    }

    public static int IndexOf<T>(
      ref T searchSpace,
      int searchSpaceLength,
      ref T value,
      int valueLength)
      where T : IEquatable<T>
    {
      if (valueLength == 0)
        return 0;
      T obj = value;
      ref T local = ref Unsafe.Add<T>(ref value, 1);
      int length1 = valueLength - 1;
      int elementOffset = 0;
      int num1;
      while (true)
      {
        int length2 = searchSpaceLength - elementOffset - length1;
        if (length2 > 0)
        {
          int num2 = SpanHelpers.IndexOf<T>(ref Unsafe.Add<T>(ref searchSpace, elementOffset), obj, length2);
          if (num2 != -1)
          {
            num1 = elementOffset + num2;
            if (!SpanHelpers.SequenceEqual<T>(ref Unsafe.Add<T>(ref searchSpace, num1 + 1), ref local, length1))
              elementOffset = num1 + 1;
            else
              break;
          }
          else
            goto label_8;
        }
        else
          goto label_8;
      }
      return num1;
label_8:
      return -1;
    }

    public static unsafe bool Contains<T>(ref T searchSpace, T value, int length) where T : IEquatable<T>
    {
      IntPtr elementOffset1 = (IntPtr) 0;
      if ((object) default (T) != null || (object) value != null)
      {
        while (length >= 8)
        {
          length -= 8;
          if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset1 + 0)) && !value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset1 + 1)) && (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset1 + 2)) && !value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset1 + 3))) && (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset1 + 4)) && !value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset1 + 5)) && (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset1 + 6)) && !value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset1 + 7)))))
            elementOffset1 += 8;
          else
            goto label_15;
        }
        if (length >= 4)
        {
          length -= 4;
          if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset1 + 0)) && !value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset1 + 1)) && (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset1 + 2)) && !value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset1 + 3))))
            elementOffset1 += 4;
          else
            goto label_15;
        }
        while (length > 0)
        {
          --length;
          if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset1)))
            elementOffset1 += 1;
          else
            goto label_15;
        }
      }
      else
      {
        byte* numPtr = (byte*) length;
        IntPtr elementOffset2 = (IntPtr) 0;
        while (elementOffset2.ToPointer() < numPtr)
        {
          if ((object) Unsafe.Add<T>(ref searchSpace, elementOffset2) != null)
            elementOffset2 += 1;
          else
            goto label_15;
        }
      }
      return false;
label_15:
      return true;
    }

    public static unsafe int IndexOf<T>(ref T searchSpace, T value, int length) where T : IEquatable<T>
    {
      IntPtr elementOffset = (IntPtr) 0;
      if ((object) default (T) != null || (object) value != null)
      {
        while (length >= 8)
        {
          length -= 8;
          if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset)))
          {
            if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 1)))
            {
              if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 2)))
              {
                if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 3)))
                {
                  if (value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 4)))
                    return (int) (void*) (elementOffset + 4);
                  if (value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 5)))
                    return (int) (void*) (elementOffset + 5);
                  if (value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 6)))
                    return (int) (void*) (elementOffset + 6);
                  if (value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 7)))
                    return (int) (void*) (elementOffset + 7);
                  elementOffset += 8;
                }
                else
                  goto label_28;
              }
              else
                goto label_27;
            }
            else
              goto label_26;
          }
          else
            goto label_25;
        }
        if (length >= 4)
        {
          length -= 4;
          if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset)))
          {
            if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 1)))
            {
              if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 2)))
              {
                if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 3)))
                  elementOffset += 4;
                else
                  goto label_28;
              }
              else
                goto label_27;
            }
            else
              goto label_26;
          }
          else
            goto label_25;
        }
        for (; length > 0; --length)
        {
          if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset)))
            elementOffset += 1;
          else
            goto label_25;
        }
        goto label_24;
label_26:
        return (int) (void*) (elementOffset + 1);
label_27:
        return (int) (void*) (elementOffset + 2);
label_28:
        return (int) (void*) (elementOffset + 3);
      }
      byte* numPtr = (byte*) length;
      elementOffset = (IntPtr) 0;
      while (elementOffset.ToPointer() < numPtr)
      {
        if ((object) Unsafe.Add<T>(ref searchSpace, elementOffset) != null)
          elementOffset += 1;
        else
          goto label_25;
      }
label_24:
      return -1;
label_25:
      return (int) (void*) elementOffset;
    }

    public static int IndexOfAny<T>(ref T searchSpace, T value0, T value1, int length) where T : IEquatable<T>
    {
      int elementOffset = 0;
      if ((object) default (T) != null || (object) value0 != null && (object) value1 != null)
      {
        for (; length - elementOffset >= 8; elementOffset += 8)
        {
          T other1 = Unsafe.Add<T>(ref searchSpace, elementOffset);
          if (!value0.Equals(other1) && !value1.Equals(other1))
          {
            T other2 = Unsafe.Add<T>(ref searchSpace, elementOffset + 1);
            if (!value0.Equals(other2) && !value1.Equals(other2))
            {
              T other3 = Unsafe.Add<T>(ref searchSpace, elementOffset + 2);
              if (!value0.Equals(other3) && !value1.Equals(other3))
              {
                T other4 = Unsafe.Add<T>(ref searchSpace, elementOffset + 3);
                if (!value0.Equals(other4) && !value1.Equals(other4))
                {
                  T other5 = Unsafe.Add<T>(ref searchSpace, elementOffset + 4);
                  if (value0.Equals(other5) || value1.Equals(other5))
                    return elementOffset + 4;
                  T other6 = Unsafe.Add<T>(ref searchSpace, elementOffset + 5);
                  if (value0.Equals(other6) || value1.Equals(other6))
                    return elementOffset + 5;
                  T other7 = Unsafe.Add<T>(ref searchSpace, elementOffset + 6);
                  if (value0.Equals(other7) || value1.Equals(other7))
                    return elementOffset + 6;
                  T other8 = Unsafe.Add<T>(ref searchSpace, elementOffset + 7);
                  if (value0.Equals(other8) || value1.Equals(other8))
                    return elementOffset + 7;
                }
                else
                  goto label_30;
              }
              else
                goto label_29;
            }
            else
              goto label_28;
          }
          else
            goto label_27;
        }
        if (length - elementOffset >= 4)
        {
          T other1 = Unsafe.Add<T>(ref searchSpace, elementOffset);
          if (!value0.Equals(other1) && !value1.Equals(other1))
          {
            T other2 = Unsafe.Add<T>(ref searchSpace, elementOffset + 1);
            if (!value0.Equals(other2) && !value1.Equals(other2))
            {
              T other3 = Unsafe.Add<T>(ref searchSpace, elementOffset + 2);
              if (!value0.Equals(other3) && !value1.Equals(other3))
              {
                T other4 = Unsafe.Add<T>(ref searchSpace, elementOffset + 3);
                if (!value0.Equals(other4) && !value1.Equals(other4))
                  elementOffset += 4;
                else
                  goto label_30;
              }
              else
                goto label_29;
            }
            else
              goto label_28;
          }
          else
            goto label_27;
        }
        for (; elementOffset < length; ++elementOffset)
        {
          T other = Unsafe.Add<T>(ref searchSpace, elementOffset);
          if (value0.Equals(other) || value1.Equals(other))
            goto label_27;
        }
        goto label_26;
label_28:
        return elementOffset + 1;
label_29:
        return elementOffset + 2;
label_30:
        return elementOffset + 3;
      }
      for (elementOffset = 0; elementOffset < length; ++elementOffset)
      {
        T obj = Unsafe.Add<T>(ref searchSpace, elementOffset);
        if ((object) obj == null)
        {
          if ((object) value0 == null || (object) value1 == null)
            goto label_27;
        }
        else if (obj.Equals(value0) || obj.Equals(value1))
          goto label_27;
      }
label_26:
      return -1;
label_27:
      return elementOffset;
    }

    public static int IndexOfAny<T>(ref T searchSpace, T value0, T value1, T value2, int length) where T : IEquatable<T>
    {
      int elementOffset = 0;
      if ((object) default (T) != null || (object) value0 != null && (object) value1 != null && (object) value2 != null)
      {
        for (; length - elementOffset >= 8; elementOffset += 8)
        {
          T other1 = Unsafe.Add<T>(ref searchSpace, elementOffset);
          if (!value0.Equals(other1) && !value1.Equals(other1) && !value2.Equals(other1))
          {
            T other2 = Unsafe.Add<T>(ref searchSpace, elementOffset + 1);
            if (!value0.Equals(other2) && !value1.Equals(other2) && !value2.Equals(other2))
            {
              T other3 = Unsafe.Add<T>(ref searchSpace, elementOffset + 2);
              if (!value0.Equals(other3) && !value1.Equals(other3) && !value2.Equals(other3))
              {
                T other4 = Unsafe.Add<T>(ref searchSpace, elementOffset + 3);
                if (!value0.Equals(other4) && !value1.Equals(other4) && !value2.Equals(other4))
                {
                  T other5 = Unsafe.Add<T>(ref searchSpace, elementOffset + 4);
                  if (value0.Equals(other5) || value1.Equals(other5) || value2.Equals(other5))
                    return elementOffset + 4;
                  T other6 = Unsafe.Add<T>(ref searchSpace, elementOffset + 5);
                  if (value0.Equals(other6) || value1.Equals(other6) || value2.Equals(other6))
                    return elementOffset + 5;
                  T other7 = Unsafe.Add<T>(ref searchSpace, elementOffset + 6);
                  if (value0.Equals(other7) || value1.Equals(other7) || value2.Equals(other7))
                    return elementOffset + 6;
                  T other8 = Unsafe.Add<T>(ref searchSpace, elementOffset + 7);
                  if (value0.Equals(other8) || value1.Equals(other8) || value2.Equals(other8))
                    return elementOffset + 7;
                }
                else
                  goto label_30;
              }
              else
                goto label_29;
            }
            else
              goto label_28;
          }
          else
            goto label_27;
        }
        if (length - elementOffset >= 4)
        {
          T other1 = Unsafe.Add<T>(ref searchSpace, elementOffset);
          if (!value0.Equals(other1) && !value1.Equals(other1) && !value2.Equals(other1))
          {
            T other2 = Unsafe.Add<T>(ref searchSpace, elementOffset + 1);
            if (!value0.Equals(other2) && !value1.Equals(other2) && !value2.Equals(other2))
            {
              T other3 = Unsafe.Add<T>(ref searchSpace, elementOffset + 2);
              if (!value0.Equals(other3) && !value1.Equals(other3) && !value2.Equals(other3))
              {
                T other4 = Unsafe.Add<T>(ref searchSpace, elementOffset + 3);
                if (!value0.Equals(other4) && !value1.Equals(other4) && !value2.Equals(other4))
                  elementOffset += 4;
                else
                  goto label_30;
              }
              else
                goto label_29;
            }
            else
              goto label_28;
          }
          else
            goto label_27;
        }
        for (; elementOffset < length; ++elementOffset)
        {
          T other = Unsafe.Add<T>(ref searchSpace, elementOffset);
          if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
            goto label_27;
        }
        goto label_26;
label_28:
        return elementOffset + 1;
label_29:
        return elementOffset + 2;
label_30:
        return elementOffset + 3;
      }
      for (elementOffset = 0; elementOffset < length; ++elementOffset)
      {
        T obj = Unsafe.Add<T>(ref searchSpace, elementOffset);
        if ((object) obj == null)
        {
          if ((object) value0 == null || (object) value1 == null || (object) value2 == null)
            goto label_27;
        }
        else if (obj.Equals(value0) || obj.Equals(value1) || obj.Equals(value2))
          goto label_27;
      }
label_26:
      return -1;
label_27:
      return elementOffset;
    }

    public static int IndexOfAny<T>(
      ref T searchSpace,
      int searchSpaceLength,
      ref T value,
      int valueLength)
      where T : IEquatable<T>
    {
      if (valueLength == 0)
        return -1;
      int num1 = -1;
      for (int elementOffset = 0; elementOffset < valueLength; ++elementOffset)
      {
        int num2 = SpanHelpers.IndexOf<T>(ref searchSpace, Unsafe.Add<T>(ref value, elementOffset), searchSpaceLength);
        if ((uint) num2 < (uint) num1)
        {
          num1 = num2;
          searchSpaceLength = num2;
          if (num1 == 0)
            break;
        }
      }
      return num1;
    }

    public static int LastIndexOf<T>(
      ref T searchSpace,
      int searchSpaceLength,
      ref T value,
      int valueLength)
      where T : IEquatable<T>
    {
      if (valueLength == 0)
        return 0;
      T obj = value;
      ref T local = ref Unsafe.Add<T>(ref value, 1);
      int length1 = valueLength - 1;
      int num1 = 0;
      int num2;
      while (true)
      {
        int length2 = searchSpaceLength - num1 - length1;
        if (length2 > 0)
        {
          num2 = SpanHelpers.LastIndexOf<T>(ref searchSpace, obj, length2);
          if (num2 != -1)
          {
            if (!SpanHelpers.SequenceEqual<T>(ref Unsafe.Add<T>(ref searchSpace, num2 + 1), ref local, length1))
              num1 += length2 - num2;
            else
              break;
          }
          else
            goto label_8;
        }
        else
          goto label_8;
      }
      return num2;
label_8:
      return -1;
    }

    public static int LastIndexOf<T>(ref T searchSpace, T value, int length) where T : IEquatable<T>
    {
      if ((object) default (T) != null || (object) value != null)
      {
        while (length >= 8)
        {
          length -= 8;
          if (value.Equals(Unsafe.Add<T>(ref searchSpace, length + 7)))
            return length + 7;
          if (value.Equals(Unsafe.Add<T>(ref searchSpace, length + 6)))
            return length + 6;
          if (value.Equals(Unsafe.Add<T>(ref searchSpace, length + 5)))
            return length + 5;
          if (value.Equals(Unsafe.Add<T>(ref searchSpace, length + 4)))
            return length + 4;
          if (!value.Equals(Unsafe.Add<T>(ref searchSpace, length + 3)))
          {
            if (!value.Equals(Unsafe.Add<T>(ref searchSpace, length + 2)))
            {
              if (!value.Equals(Unsafe.Add<T>(ref searchSpace, length + 1)))
              {
                if (value.Equals(Unsafe.Add<T>(ref searchSpace, length)))
                  goto label_22;
              }
              else
                goto label_23;
            }
            else
              goto label_24;
          }
          else
            goto label_25;
        }
        if (length >= 4)
        {
          length -= 4;
          if (!value.Equals(Unsafe.Add<T>(ref searchSpace, length + 3)))
          {
            if (!value.Equals(Unsafe.Add<T>(ref searchSpace, length + 2)))
            {
              if (!value.Equals(Unsafe.Add<T>(ref searchSpace, length + 1)))
              {
                if (value.Equals(Unsafe.Add<T>(ref searchSpace, length)))
                  goto label_22;
              }
              else
                goto label_23;
            }
            else
              goto label_24;
          }
          else
            goto label_25;
        }
        while (length > 0)
        {
          --length;
          if (value.Equals(Unsafe.Add<T>(ref searchSpace, length)))
            goto label_22;
        }
        goto label_21;
label_23:
        return length + 1;
label_24:
        return length + 2;
label_25:
        return length + 3;
      }
      for (--length; length >= 0; --length)
      {
        if ((object) Unsafe.Add<T>(ref searchSpace, length) == null)
          goto label_22;
      }
label_21:
      return -1;
label_22:
      return length;
    }

    public static int LastIndexOfAny<T>(ref T searchSpace, T value0, T value1, int length) where T : IEquatable<T>
    {
      if ((object) default (T) != null || (object) value0 != null && (object) value1 != null)
      {
        while (length >= 8)
        {
          length -= 8;
          T other1 = Unsafe.Add<T>(ref searchSpace, length + 7);
          if (value0.Equals(other1) || value1.Equals(other1))
            return length + 7;
          T other2 = Unsafe.Add<T>(ref searchSpace, length + 6);
          if (value0.Equals(other2) || value1.Equals(other2))
            return length + 6;
          T other3 = Unsafe.Add<T>(ref searchSpace, length + 5);
          if (value0.Equals(other3) || value1.Equals(other3))
            return length + 5;
          T other4 = Unsafe.Add<T>(ref searchSpace, length + 4);
          if (value0.Equals(other4) || value1.Equals(other4))
            return length + 4;
          T other5 = Unsafe.Add<T>(ref searchSpace, length + 3);
          if (!value0.Equals(other5) && !value1.Equals(other5))
          {
            T other6 = Unsafe.Add<T>(ref searchSpace, length + 2);
            if (!value0.Equals(other6) && !value1.Equals(other6))
            {
              T other7 = Unsafe.Add<T>(ref searchSpace, length + 1);
              if (!value0.Equals(other7) && !value1.Equals(other7))
              {
                T other8 = Unsafe.Add<T>(ref searchSpace, length);
                if (value0.Equals(other8) || value1.Equals(other8))
                  goto label_24;
              }
              else
                goto label_25;
            }
            else
              goto label_26;
          }
          else
            goto label_27;
        }
        if (length >= 4)
        {
          length -= 4;
          T other1 = Unsafe.Add<T>(ref searchSpace, length + 3);
          if (!value0.Equals(other1) && !value1.Equals(other1))
          {
            T other2 = Unsafe.Add<T>(ref searchSpace, length + 2);
            if (!value0.Equals(other2) && !value1.Equals(other2))
            {
              T other3 = Unsafe.Add<T>(ref searchSpace, length + 1);
              if (!value0.Equals(other3) && !value1.Equals(other3))
              {
                T other4 = Unsafe.Add<T>(ref searchSpace, length);
                if (value0.Equals(other4) || value1.Equals(other4))
                  goto label_24;
              }
              else
                goto label_25;
            }
            else
              goto label_26;
          }
          else
            goto label_27;
        }
        while (length > 0)
        {
          --length;
          T other = Unsafe.Add<T>(ref searchSpace, length);
          if (value0.Equals(other) || value1.Equals(other))
            goto label_24;
        }
        goto label_23;
label_25:
        return length + 1;
label_26:
        return length + 2;
label_27:
        return length + 3;
      }
      for (--length; length >= 0; --length)
      {
        T obj = Unsafe.Add<T>(ref searchSpace, length);
        if ((object) obj == null)
        {
          if ((object) value0 == null || (object) value1 == null)
            goto label_24;
        }
        else if (obj.Equals(value0) || obj.Equals(value1))
          goto label_24;
      }
label_23:
      return -1;
label_24:
      return length;
    }

    public static int LastIndexOfAny<T>(
      ref T searchSpace,
      T value0,
      T value1,
      T value2,
      int length)
      where T : IEquatable<T>
    {
      if ((object) default (T) != null || (object) value0 != null && (object) value1 != null)
      {
        while (length >= 8)
        {
          length -= 8;
          T other1 = Unsafe.Add<T>(ref searchSpace, length + 7);
          if (value0.Equals(other1) || value1.Equals(other1) || value2.Equals(other1))
            return length + 7;
          T other2 = Unsafe.Add<T>(ref searchSpace, length + 6);
          if (value0.Equals(other2) || value1.Equals(other2) || value2.Equals(other2))
            return length + 6;
          T other3 = Unsafe.Add<T>(ref searchSpace, length + 5);
          if (value0.Equals(other3) || value1.Equals(other3) || value2.Equals(other3))
            return length + 5;
          T other4 = Unsafe.Add<T>(ref searchSpace, length + 4);
          if (value0.Equals(other4) || value1.Equals(other4) || value2.Equals(other4))
            return length + 4;
          T other5 = Unsafe.Add<T>(ref searchSpace, length + 3);
          if (!value0.Equals(other5) && !value1.Equals(other5) && !value2.Equals(other5))
          {
            T other6 = Unsafe.Add<T>(ref searchSpace, length + 2);
            if (!value0.Equals(other6) && !value1.Equals(other6) && !value2.Equals(other6))
            {
              T other7 = Unsafe.Add<T>(ref searchSpace, length + 1);
              if (!value0.Equals(other7) && !value1.Equals(other7) && !value2.Equals(other7))
              {
                T other8 = Unsafe.Add<T>(ref searchSpace, length);
                if (value0.Equals(other8) || value1.Equals(other8) || value2.Equals(other8))
                  goto label_24;
              }
              else
                goto label_25;
            }
            else
              goto label_26;
          }
          else
            goto label_27;
        }
        if (length >= 4)
        {
          length -= 4;
          T other1 = Unsafe.Add<T>(ref searchSpace, length + 3);
          if (!value0.Equals(other1) && !value1.Equals(other1) && !value2.Equals(other1))
          {
            T other2 = Unsafe.Add<T>(ref searchSpace, length + 2);
            if (!value0.Equals(other2) && !value1.Equals(other2) && !value2.Equals(other2))
            {
              T other3 = Unsafe.Add<T>(ref searchSpace, length + 1);
              if (!value0.Equals(other3) && !value1.Equals(other3) && !value2.Equals(other3))
              {
                T other4 = Unsafe.Add<T>(ref searchSpace, length);
                if (value0.Equals(other4) || value1.Equals(other4) || value2.Equals(other4))
                  goto label_24;
              }
              else
                goto label_25;
            }
            else
              goto label_26;
          }
          else
            goto label_27;
        }
        while (length > 0)
        {
          --length;
          T other = Unsafe.Add<T>(ref searchSpace, length);
          if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
            goto label_24;
        }
        goto label_23;
label_25:
        return length + 1;
label_26:
        return length + 2;
label_27:
        return length + 3;
      }
      for (--length; length >= 0; --length)
      {
        T obj = Unsafe.Add<T>(ref searchSpace, length);
        if ((object) obj == null)
        {
          if ((object) value0 == null || (object) value1 == null || (object) value2 == null)
            goto label_24;
        }
        else if (obj.Equals(value0) || obj.Equals(value1) || obj.Equals(value2))
          goto label_24;
      }
label_23:
      return -1;
label_24:
      return length;
    }

    public static int LastIndexOfAny<T>(
      ref T searchSpace,
      int searchSpaceLength,
      ref T value,
      int valueLength)
      where T : IEquatable<T>
    {
      if (valueLength == 0)
        return -1;
      int num1 = -1;
      for (int elementOffset = 0; elementOffset < valueLength; ++elementOffset)
      {
        int num2 = SpanHelpers.LastIndexOf<T>(ref searchSpace, Unsafe.Add<T>(ref value, elementOffset), searchSpaceLength);
        if (num2 > num1)
          num1 = num2;
      }
      return num1;
    }

    public static bool SequenceEqual<T>(ref T first, ref T second, int length) where T : IEquatable<T>
    {
      if (!Unsafe.AreSame<T>(ref first, ref second))
      {
        IntPtr elementOffset = (IntPtr) 0;
        while (length >= 8)
        {
          length -= 8;
          T obj1 = Unsafe.Add<T>(ref first, elementOffset);
          T other1 = Unsafe.Add<T>(ref second, elementOffset);
          if (((object) obj1 != null ? (obj1.Equals(other1) ? 1 : 0) : ((object) other1 == null ? 1 : 0)) != 0)
          {
            T obj2 = Unsafe.Add<T>(ref first, elementOffset + 1);
            T other2 = Unsafe.Add<T>(ref second, elementOffset + 1);
            if (((object) obj2 != null ? (obj2.Equals(other2) ? 1 : 0) : ((object) other2 == null ? 1 : 0)) != 0)
            {
              T obj3 = Unsafe.Add<T>(ref first, elementOffset + 2);
              T other3 = Unsafe.Add<T>(ref second, elementOffset + 2);
              if (((object) obj3 != null ? (obj3.Equals(other3) ? 1 : 0) : ((object) other3 == null ? 1 : 0)) != 0)
              {
                T obj4 = Unsafe.Add<T>(ref first, elementOffset + 3);
                T other4 = Unsafe.Add<T>(ref second, elementOffset + 3);
                if (((object) obj4 != null ? (obj4.Equals(other4) ? 1 : 0) : ((object) other4 == null ? 1 : 0)) != 0)
                {
                  T obj5 = Unsafe.Add<T>(ref first, elementOffset + 4);
                  T other5 = Unsafe.Add<T>(ref second, elementOffset + 4);
                  if (((object) obj5 != null ? (obj5.Equals(other5) ? 1 : 0) : ((object) other5 == null ? 1 : 0)) != 0)
                  {
                    T obj6 = Unsafe.Add<T>(ref first, elementOffset + 5);
                    T other6 = Unsafe.Add<T>(ref second, elementOffset + 5);
                    if (((object) obj6 != null ? (obj6.Equals(other6) ? 1 : 0) : ((object) other6 == null ? 1 : 0)) != 0)
                    {
                      T obj7 = Unsafe.Add<T>(ref first, elementOffset + 6);
                      T other7 = Unsafe.Add<T>(ref second, elementOffset + 6);
                      if (((object) obj7 != null ? (obj7.Equals(other7) ? 1 : 0) : ((object) other7 == null ? 1 : 0)) != 0)
                      {
                        T obj8 = Unsafe.Add<T>(ref first, elementOffset + 7);
                        T other8 = Unsafe.Add<T>(ref second, elementOffset + 7);
                        if (((object) obj8 != null ? (obj8.Equals(other8) ? 1 : 0) : ((object) other8 == null ? 1 : 0)) != 0)
                          elementOffset += 8;
                        else
                          goto label_22;
                      }
                      else
                        goto label_22;
                    }
                    else
                      goto label_22;
                  }
                  else
                    goto label_22;
                }
                else
                  goto label_22;
              }
              else
                goto label_22;
            }
            else
              goto label_22;
          }
          else
            goto label_22;
        }
        if (length >= 4)
        {
          length -= 4;
          T obj1 = Unsafe.Add<T>(ref first, elementOffset);
          T other1 = Unsafe.Add<T>(ref second, elementOffset);
          if (((object) obj1 != null ? (obj1.Equals(other1) ? 1 : 0) : ((object) other1 == null ? 1 : 0)) != 0)
          {
            T obj2 = Unsafe.Add<T>(ref first, elementOffset + 1);
            T other2 = Unsafe.Add<T>(ref second, elementOffset + 1);
            if (((object) obj2 != null ? (obj2.Equals(other2) ? 1 : 0) : ((object) other2 == null ? 1 : 0)) != 0)
            {
              T obj3 = Unsafe.Add<T>(ref first, elementOffset + 2);
              T other3 = Unsafe.Add<T>(ref second, elementOffset + 2);
              if (((object) obj3 != null ? (obj3.Equals(other3) ? 1 : 0) : ((object) other3 == null ? 1 : 0)) != 0)
              {
                T obj4 = Unsafe.Add<T>(ref first, elementOffset + 3);
                T other4 = Unsafe.Add<T>(ref second, elementOffset + 3);
                if (((object) obj4 != null ? (obj4.Equals(other4) ? 1 : 0) : ((object) other4 == null ? 1 : 0)) != 0)
                  elementOffset += 4;
                else
                  goto label_22;
              }
              else
                goto label_22;
            }
            else
              goto label_22;
          }
          else
            goto label_22;
        }
        for (; length > 0; --length)
        {
          T obj = Unsafe.Add<T>(ref first, elementOffset);
          T other = Unsafe.Add<T>(ref second, elementOffset);
          if (((object) obj != null ? (obj.Equals(other) ? 1 : 0) : ((object) other == null ? 1 : 0)) != 0)
            elementOffset += 1;
          else
            goto label_22;
        }
        goto label_21;
label_22:
        return false;
      }
label_21:
      return true;
    }

    public static int SequenceCompareTo<T>(
      ref T first,
      int firstLength,
      ref T second,
      int secondLength)
      where T : IComparable<T>
    {
      int num1 = firstLength;
      if (num1 > secondLength)
        num1 = secondLength;
      for (int elementOffset = 0; elementOffset < num1; ++elementOffset)
      {
        T obj1 = Unsafe.Add<T>(ref second, elementOffset);
        ref T local1 = ref Unsafe.Add<T>(ref first, elementOffset);
        int num2;
        if ((object) default (T) == null)
        {
          T obj2 = local1;
          ref T local2 = ref obj2;
          if ((object) obj2 == null)
          {
            num2 = (object) obj1 == null ? 0 : -1;
            goto label_7;
          }
          else
            local1 = ref local2;
        }
        T other = obj1;
        num2 = local1.CompareTo(other);
label_7:
        int num3 = num2;
        if (num3 != 0)
          return num3;
      }
      return firstLength.CompareTo(secondLength);
    }

    internal readonly struct ComparerComparable<T, TComparer> : IComparable<T> where TComparer : IComparer<T>
    {
      private readonly T _value;
      private readonly TComparer _comparer;

      public ComparerComparable(T value, TComparer comparer)
      {
        this._value = value;
        this._comparer = comparer;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public int CompareTo(T other)
      {
        return this._comparer.Compare(this._value, other);
      }
    }
  }
}

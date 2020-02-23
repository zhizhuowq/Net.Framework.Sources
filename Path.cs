// Decompiled with JetBrains decompiler
// Type: System.IO.Path
// Assembly: System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// MVID: BBB3B884-123D-47EA-9CD1-5BED540D02AE
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Private.CoreLib.dll

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.IO
{
  public static class Path
  {
    public static readonly char DirectorySeparatorChar = '\\';
    public static readonly char AltDirectorySeparatorChar = '/';
    public static readonly char VolumeSeparatorChar = ':';
    public static readonly char PathSeparator = ';';
    [Nullable(1)]
    [Obsolete("Please use GetInvalidPathChars or GetInvalidFileNameChars instead.")]
    public static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

    [NullableContext(2)]
    [return: NotNullIfNotNull("path")]
    public static string ChangeExtension(string path, string extension)
    {
      if (path == null)
        return (string) null;
      int length = path.Length;
      if (length == 0)
        return string.Empty;
      for (int index = path.Length - 1; index >= 0; --index)
      {
        char c = path[index];
        if (c == '.')
        {
          length = index;
          break;
        }
        if (PathInternal.IsDirectorySeparator(c))
          break;
      }
      if (extension == null)
        return path.Substring(0, length);
      ReadOnlySpan<char> readOnlySpan = path.AsSpan(0, length);
      return !extension.StartsWith('.') ? readOnlySpan.ToString() + (ReadOnlySpan<char>) "." + (ReadOnlySpan<char>) extension : readOnlySpan.ToString() + (ReadOnlySpan<char>) extension;
    }

    [NullableContext(2)]
    public static string GetDirectoryName(string path)
    {
      if (path == null || PathInternal.IsEffectivelyEmpty(path.AsSpan()))
        return (string) null;
      int directoryNameOffset = Path.GetDirectoryNameOffset(path.AsSpan());
      return directoryNameOffset < 0 ? (string) null : PathInternal.NormalizeDirectorySeparators(path.Substring(0, directoryNameOffset));
    }

    public static ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path)
    {
      if (PathInternal.IsEffectivelyEmpty(path))
        return ReadOnlySpan<char>.Empty;
      int directoryNameOffset = Path.GetDirectoryNameOffset(path);
      return directoryNameOffset < 0 ? ReadOnlySpan<char>.Empty : path.Slice(0, directoryNameOffset);
    }

    private static int GetDirectoryNameOffset(ReadOnlySpan<char> path)
    {
      int rootLength = PathInternal.GetRootLength(path);
      int length = path.Length;
      if (length <= rootLength)
        return -1;
      do
        ;
      while (length > rootLength && !PathInternal.IsDirectorySeparator(path[--length]));
      while (length > rootLength && PathInternal.IsDirectorySeparator(path[length - 1]))
        --length;
      return length;
    }

    [NullableContext(2)]
    [return: NotNullIfNotNull("path")]
    public static string GetExtension(string path)
    {
      return path == null ? (string) null : Path.GetExtension(path.AsSpan()).ToString();
    }

    public static ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path)
    {
      int length = path.Length;
      for (int start = length - 1; start >= 0; --start)
      {
        char c = path[start];
        if (c == '.')
          return start != length - 1 ? path.Slice(start, length - start) : ReadOnlySpan<char>.Empty;
        if (PathInternal.IsDirectorySeparator(c))
          break;
      }
      return ReadOnlySpan<char>.Empty;
    }

    [NullableContext(2)]
    [return: NotNullIfNotNull("path")]
    public static string GetFileName(string path)
    {
      if (path == null)
        return (string) null;
      ReadOnlySpan<char> fileName = Path.GetFileName(path.AsSpan());
      return path.Length == fileName.Length ? path : fileName.ToString();
    }

    public static ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path)
    {
      int length1 = Path.GetPathRoot(path).Length;
      int length2 = path.Length;
      while (--length2 >= 0)
      {
        if (length2 < length1 || PathInternal.IsDirectorySeparator(path[length2]))
          return path.Slice(length2 + 1, path.Length - length2 - 1);
      }
      return path;
    }

    [NullableContext(2)]
    [return: NotNullIfNotNull("path")]
    public static string GetFileNameWithoutExtension(string path)
    {
      if (path == null)
        return (string) null;
      ReadOnlySpan<char> withoutExtension = Path.GetFileNameWithoutExtension(path.AsSpan());
      return path.Length == withoutExtension.Length ? path : withoutExtension.ToString();
    }

    public static ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path)
    {
      ReadOnlySpan<char> fileName = Path.GetFileName(path);
      int length = fileName.LastIndexOf<char>('.');
      return length != -1 ? fileName.Slice(0, length) : fileName;
    }

    [NullableContext(1)]
    public static unsafe string GetRandomFileName()
    {
      byte* buffer = stackalloc byte[8];
      Interop.GetRandomBytes(buffer, 8);
      return string.Create<IntPtr>(12, (IntPtr) (void*) buffer, (SpanAction<char, IntPtr>) ((span, key) => Path.Populate83FileNameFromRandomBytes((byte*) (void*) key, 8, span)));
    }

    [NullableContext(1)]
    public static bool IsPathFullyQualified(string path)
    {
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      return Path.IsPathFullyQualified(path.AsSpan());
    }

    public static bool IsPathFullyQualified(ReadOnlySpan<char> path)
    {
      return !PathInternal.IsPartiallyQualified(path);
    }

    [NullableContext(2)]
    public static bool HasExtension(string path)
    {
      return path != null && Path.HasExtension(path.AsSpan());
    }

    public static bool HasExtension(ReadOnlySpan<char> path)
    {
      for (int index = path.Length - 1; index >= 0; --index)
      {
        char c = path[index];
        if (c == '.')
          return index != path.Length - 1;
        if (PathInternal.IsDirectorySeparator(c))
          break;
      }
      return false;
    }

    [NullableContext(1)]
    public static string Combine(string path1, string path2)
    {
      if (path1 == null || path2 == null)
        throw new ArgumentNullException(path1 == null ? nameof (path1) : nameof (path2));
      return Path.CombineInternal(path1, path2);
    }

    [NullableContext(1)]
    public static string Combine(string path1, string path2, string path3)
    {
      if (path1 == null || path2 == null || path3 == null)
        throw new ArgumentNullException(path1 == null ? nameof (path1) : (path2 == null ? nameof (path2) : nameof (path3)));
      return Path.CombineInternal(path1, path2, path3);
    }

    [NullableContext(1)]
    public static string Combine(string path1, string path2, string path3, string path4)
    {
      if (path1 == null || path2 == null || (path3 == null || path4 == null))
        throw new ArgumentNullException(path1 == null ? nameof (path1) : (path2 == null ? nameof (path2) : (path3 == null ? nameof (path3) : nameof (path4))));
      return Path.CombineInternal(path1, path2, path3, path4);
    }

    [NullableContext(1)]
    public static unsafe string Combine(params string[] paths)
    {
      if (paths == null)
        throw new ArgumentNullException(nameof (paths));
      int capacity = 0;
      int num = 0;
      for (int index = 0; index < paths.Length; ++index)
      {
        if (paths[index] == null)
          throw new ArgumentNullException(nameof (paths));
        if (paths[index].Length != 0)
        {
          if (Path.IsPathRooted(paths[index]))
          {
            num = index;
            capacity = paths[index].Length;
          }
          else
            capacity += paths[index].Length;
          if (!PathInternal.IsDirectorySeparator(paths[index][paths[index].Length - 1]))
            ++capacity;
        }
      }
      // ISSUE: untyped stack allocation
      ValueStringBuilder valueStringBuilder = new ValueStringBuilder(new Span<char>((void*) __untypedstackalloc(new IntPtr(520)), 260));
      valueStringBuilder.EnsureCapacity(capacity);
      for (int index = num; index < paths.Length; ++index)
      {
        if (paths[index].Length != 0)
        {
          if (valueStringBuilder.Length == 0)
          {
            valueStringBuilder.Append(paths[index]);
          }
          else
          {
            if (!PathInternal.IsDirectorySeparator(valueStringBuilder[valueStringBuilder.Length - 1]))
              valueStringBuilder.Append('\\');
            valueStringBuilder.Append(paths[index]);
          }
        }
      }
      return valueStringBuilder.ToString();
    }

    [return: Nullable(1)]
    public static string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
    {
      if (path1.Length == 0)
        return path2.ToString();
      return path2.Length == 0 ? path1.ToString() : Path.JoinInternal(path1, path2);
    }

    [return: Nullable(1)]
    public static string Join(
      ReadOnlySpan<char> path1,
      ReadOnlySpan<char> path2,
      ReadOnlySpan<char> path3)
    {
      if (path1.Length == 0)
        return Path.Join(path2, path3);
      if (path2.Length == 0)
        return Path.Join(path1, path3);
      return path3.Length == 0 ? Path.Join(path1, path2) : Path.JoinInternal(path1, path2, path3);
    }

    [return: Nullable(1)]
    public static string Join(
      ReadOnlySpan<char> path1,
      ReadOnlySpan<char> path2,
      ReadOnlySpan<char> path3,
      ReadOnlySpan<char> path4)
    {
      if (path1.Length == 0)
        return Path.Join(path2, path3, path4);
      if (path2.Length == 0)
        return Path.Join(path1, path3, path4);
      if (path3.Length == 0)
        return Path.Join(path1, path2, path4);
      return path4.Length == 0 ? Path.Join(path1, path2, path3) : Path.JoinInternal(path1, path2, path3, path4);
    }

    [NullableContext(2)]
    [return: Nullable(1)]
    public static string Join(string path1, string path2)
    {
      return Path.Join(path1.AsSpan(), path2.AsSpan());
    }

    [NullableContext(2)]
    [return: Nullable(1)]
    public static string Join(string path1, string path2, string path3)
    {
      return Path.Join(path1.AsSpan(), path2.AsSpan(), path3.AsSpan());
    }

    [NullableContext(2)]
    [return: Nullable(1)]
    public static string Join(string path1, string path2, string path3, string path4)
    {
      return Path.Join(path1.AsSpan(), path2.AsSpan(), path3.AsSpan(), path4.AsSpan());
    }

    [NullableContext(1)]
    public static unsafe string Join([Nullable(new byte[] {1, 2})] params string[] paths)
    {
      if (paths == null)
        throw new ArgumentNullException(nameof (paths));
      if (paths.Length == 0)
        return string.Empty;
      int num = 0;
      foreach (string path in paths)
        num += path != null ? path.Length : 0;
      int capacity = num + (paths.Length - 1);
      // ISSUE: untyped stack allocation
      ValueStringBuilder valueStringBuilder = new ValueStringBuilder(new Span<char>((void*) __untypedstackalloc(new IntPtr(520)), 260));
      valueStringBuilder.EnsureCapacity(capacity);
      for (int index = 0; index < paths.Length; ++index)
      {
        string path = paths[index];
        if (path != null && path.Length != 0)
        {
          if (valueStringBuilder.Length == 0)
          {
            valueStringBuilder.Append(path);
          }
          else
          {
            if (!PathInternal.IsDirectorySeparator(valueStringBuilder[valueStringBuilder.Length - 1]) && !PathInternal.IsDirectorySeparator(path[0]))
              valueStringBuilder.Append('\\');
            valueStringBuilder.Append(path);
          }
        }
      }
      return valueStringBuilder.ToString();
    }

    public static bool TryJoin(
      ReadOnlySpan<char> path1,
      ReadOnlySpan<char> path2,
      Span<char> destination,
      out int charsWritten)
    {
      charsWritten = 0;
      if (path1.Length == 0 && path2.Length == 0)
        return true;
      if (path1.Length == 0 || path2.Length == 0)
      {
        ref ReadOnlySpan<char> local = ref (path1.Length == 0 ? ref path2 : ref path1);
        if (destination.Length < local.Length)
          return false;
        local.CopyTo(destination);
        charsWritten = local.Length;
        return true;
      }
      bool flag = !Path.EndsInDirectorySeparator(path1) && !PathInternal.StartsWithDirectorySeparator(path2);
      int num = path1.Length + path2.Length + (flag ? 1 : 0);
      if (destination.Length < num)
        return false;
      path1.CopyTo(destination);
      if (flag)
        destination[path1.Length] = Path.DirectorySeparatorChar;
      path2.CopyTo(destination.Slice(path1.Length + (flag ? 1 : 0)));
      charsWritten = num;
      return true;
    }

    public static bool TryJoin(
      ReadOnlySpan<char> path1,
      ReadOnlySpan<char> path2,
      ReadOnlySpan<char> path3,
      Span<char> destination,
      out int charsWritten)
    {
      charsWritten = 0;
      if (path1.Length == 0 && path2.Length == 0 && path3.Length == 0)
        return true;
      if (path1.Length == 0)
        return Path.TryJoin(path2, path3, destination, out charsWritten);
      if (path2.Length == 0)
        return Path.TryJoin(path1, path3, destination, out charsWritten);
      if (path3.Length == 0)
        return Path.TryJoin(path1, path2, destination, out charsWritten);
      int num1 = Path.EndsInDirectorySeparator(path1) || PathInternal.StartsWithDirectorySeparator(path2) ? 0 : 1;
      bool flag = !Path.EndsInDirectorySeparator(path2) && !PathInternal.StartsWithDirectorySeparator(path3);
      if (flag)
        ++num1;
      int num2 = path1.Length + path2.Length + path3.Length + num1;
      if (destination.Length < num2)
        return false;
      Path.TryJoin(path1, path2, destination, out charsWritten);
      if (flag)
        destination[charsWritten++] = Path.DirectorySeparatorChar;
      path3.CopyTo(destination.Slice(charsWritten));
      charsWritten += path3.Length;
      return true;
    }

    private static string CombineInternal(string first, string second)
    {
      if (string.IsNullOrEmpty(first))
        return second;
      if (string.IsNullOrEmpty(second))
        return first;
      return Path.IsPathRooted(second.AsSpan()) ? second : Path.JoinInternal(first.AsSpan(), second.AsSpan());
    }

    private static string CombineInternal(string first, string second, string third)
    {
      if (string.IsNullOrEmpty(first))
        return Path.CombineInternal(second, third);
      if (string.IsNullOrEmpty(second))
        return Path.CombineInternal(first, third);
      if (string.IsNullOrEmpty(third))
        return Path.CombineInternal(first, second);
      if (Path.IsPathRooted(third.AsSpan()))
        return third;
      return Path.IsPathRooted(second.AsSpan()) ? Path.CombineInternal(second, third) : Path.JoinInternal(first.AsSpan(), second.AsSpan(), third.AsSpan());
    }

    private static string CombineInternal(
      string first,
      string second,
      string third,
      string fourth)
    {
      if (string.IsNullOrEmpty(first))
        return Path.CombineInternal(second, third, fourth);
      if (string.IsNullOrEmpty(second))
        return Path.CombineInternal(first, third, fourth);
      if (string.IsNullOrEmpty(third))
        return Path.CombineInternal(first, second, fourth);
      if (string.IsNullOrEmpty(fourth))
        return Path.CombineInternal(first, second, third);
      if (Path.IsPathRooted(fourth.AsSpan()))
        return fourth;
      if (Path.IsPathRooted(third.AsSpan()))
        return Path.CombineInternal(third, fourth);
      return Path.IsPathRooted(second.AsSpan()) ? Path.CombineInternal(second, third, fourth) : Path.JoinInternal(first.AsSpan(), second.AsSpan(), third.AsSpan(), fourth.AsSpan());
    }

    private static unsafe string JoinInternal(ReadOnlySpan<char> first, ReadOnlySpan<char> second)
    {
      bool flag = PathInternal.IsDirectorySeparator(first[first.Length - 1]) || PathInternal.IsDirectorySeparator(second[0]);
      fixed (char* chPtr1 = &MemoryMarshal.GetReference<char>(first))
        fixed (char* chPtr2 = &MemoryMarshal.GetReference<char>(second))
          return string.Create<(IntPtr, int, IntPtr, int, bool)>(first.Length + second.Length + (flag ? 0 : 1), ((IntPtr) (void*) chPtr1, first.Length, (IntPtr) (void*) chPtr2, second.Length, flag), (SpanAction<char, (IntPtr, int, IntPtr, int, bool)>) ((destination, state) =>
          {
            new Span<char>((void*) state.First, state.FirstLength).CopyTo(destination);
            if (!state.HasSeparator)
              destination[state.FirstLength] = '\\';
            new Span<char>((void*) state.Second, state.SecondLength).CopyTo(destination.Slice(state.FirstLength + (state.HasSeparator ? 0 : 1)));
          }));
    }

    private static unsafe string JoinInternal(
      ReadOnlySpan<char> first,
      ReadOnlySpan<char> second,
      ReadOnlySpan<char> third)
    {
      bool flag1 = PathInternal.IsDirectorySeparator(first[first.Length - 1]) || PathInternal.IsDirectorySeparator(second[0]);
      bool flag2 = PathInternal.IsDirectorySeparator(second[second.Length - 1]) || PathInternal.IsDirectorySeparator(third[0]);
      fixed (char* chPtr1 = &MemoryMarshal.GetReference<char>(first))
        fixed (char* chPtr2 = &MemoryMarshal.GetReference<char>(second))
          fixed (char* chPtr3 = &MemoryMarshal.GetReference<char>(third))
            return string.Create<(IntPtr, int, IntPtr, int, IntPtr, int, bool, bool)>(first.Length + second.Length + third.Length + (flag1 ? 0 : 1) + (flag2 ? 0 : 1), ((IntPtr) (void*) chPtr1, first.Length, (IntPtr) (void*) chPtr2, second.Length, (IntPtr) (void*) chPtr3, third.Length, flag1, flag2), (SpanAction<char, (IntPtr, int, IntPtr, int, IntPtr, int, bool, bool)>) ((destination, state) =>
            {
              new Span<char>((void*) state.First, state.FirstLength).CopyTo(destination);
              if (!state.FirstHasSeparator)
                destination[state.FirstLength] = '\\';
              new Span<char>((void*) state.Second, state.SecondLength).CopyTo(destination.Slice(state.FirstLength + (state.FirstHasSeparator ? 0 : 1)));
              if (!state.ThirdHasSeparator)
                destination[destination.Length - state.ThirdLength - 1] = '\\';
              new Span<char>((void*) state.Third, state.ThirdLength).CopyTo(destination.Slice(destination.Length - state.ThirdLength));
            }));
    }

    private static unsafe string JoinInternal(
      ReadOnlySpan<char> first,
      ReadOnlySpan<char> second,
      ReadOnlySpan<char> third,
      ReadOnlySpan<char> fourth)
    {
      bool flag1 = PathInternal.IsDirectorySeparator(first[first.Length - 1]) || PathInternal.IsDirectorySeparator(second[0]);
      bool flag2 = PathInternal.IsDirectorySeparator(second[second.Length - 1]) || PathInternal.IsDirectorySeparator(third[0]);
      bool flag3 = PathInternal.IsDirectorySeparator(third[third.Length - 1]) || PathInternal.IsDirectorySeparator(fourth[0]);
      fixed (char* chPtr1 = &MemoryMarshal.GetReference<char>(first))
        fixed (char* chPtr2 = &MemoryMarshal.GetReference<char>(second))
          fixed (char* chPtr3 = &MemoryMarshal.GetReference<char>(third))
            fixed (char* chPtr4 = &MemoryMarshal.GetReference<char>(fourth))
              return string.Create<(IntPtr, int, IntPtr, int, IntPtr, int, IntPtr, int, bool, bool, bool)>(first.Length + second.Length + third.Length + fourth.Length + (flag1 ? 0 : 1) + (flag2 ? 0 : 1) + (flag3 ? 0 : 1), ((IntPtr) (void*) chPtr1, first.Length, (IntPtr) (void*) chPtr2, second.Length, (IntPtr) (void*) chPtr3, third.Length, (IntPtr) (void*) chPtr4, fourth.Length, flag1, flag2, flag3), (SpanAction<char, (IntPtr, int, IntPtr, int, IntPtr, int, IntPtr, int, bool, bool, bool)>) ((destination, state) =>
              {
                new Span<char>((void*) state.First, state.FirstLength).CopyTo(destination);
                if (!state.FirstHasSeparator)
                  destination[state.FirstLength] = '\\';
                new Span<char>((void*) state.Second, state.SecondLength).CopyTo(destination.Slice(state.FirstLength + (state.FirstHasSeparator ? 0 : 1)));
                if (!state.ThirdHasSeparator)
                  destination[state.FirstLength + state.SecondLength + (state.FirstHasSeparator ? 0 : 1)] = '\\';
                new Span<char>((void*) state.Third, state.ThirdLength).CopyTo(destination.Slice(state.FirstLength + state.SecondLength + (state.FirstHasSeparator ? 0 : 1) + (state.ThirdHasSeparator ? 0 : 1)));
                if (!state.FourthHasSeparator)
                  destination[destination.Length - state.FourthLength - 1] = '\\';
                new Span<char>((void*) state.Fourth, state.FourthLength).CopyTo(destination.Slice(destination.Length - state.FourthLength));
              }));
    }

    private static unsafe ReadOnlySpan<byte> Base32Char
    {
      get
      {
        return new ReadOnlySpan<byte>((void*) &\u003CPrivateImplementationDetails\u003E.A3EB15172CC7E6090A2EB32E6DC8C3BD30C39A02, 32);
      }
    }

    private static unsafe void Populate83FileNameFromRandomBytes(
      byte* bytes,
      int byteCount,
      Span<char> chars)
    {
      byte num1 = *bytes;
      byte num2 = bytes[1];
      byte num3 = bytes[2];
      byte num4 = bytes[3];
      byte num5 = bytes[4];
      chars[11] = (char) Path.Base32Char[(int) bytes[7] & 31];
      chars[0] = (char) Path.Base32Char[(int) num1 & 31];
      chars[1] = (char) Path.Base32Char[(int) num2 & 31];
      chars[2] = (char) Path.Base32Char[(int) num3 & 31];
      chars[3] = (char) Path.Base32Char[(int) num4 & 31];
      chars[4] = (char) Path.Base32Char[(int) num5 & 31];
      chars[5] = (char) Path.Base32Char[((int) num1 & 224) >> 5 | ((int) num4 & 96) >> 2];
      chars[6] = (char) Path.Base32Char[((int) num2 & 224) >> 5 | ((int) num5 & 96) >> 2];
      byte num6 = (byte) ((uint) num3 >> 5);
      if (((int) num4 & 128) != 0)
        num6 |= (byte) 8;
      if (((int) num5 & 128) != 0)
        num6 |= (byte) 16;
      chars[7] = (char) Path.Base32Char[(int) num6];
      chars[8] = '.';
      chars[9] = (char) Path.Base32Char[(int) bytes[5] & 31];
      chars[10] = (char) Path.Base32Char[(int) bytes[6] & 31];
    }

    [NullableContext(1)]
    public static string GetRelativePath(string relativeTo, string path)
    {
      return Path.GetRelativePath(relativeTo, path, Path.StringComparison);
    }

    private static string GetRelativePath(
      string relativeTo,
      string path,
      StringComparison comparisonType)
    {
      if (relativeTo == null)
        throw new ArgumentNullException(nameof (relativeTo));
      if (PathInternal.IsEffectivelyEmpty(relativeTo.AsSpan()))
        throw new ArgumentException(SR.Arg_PathEmpty, nameof (relativeTo));
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      if (PathInternal.IsEffectivelyEmpty(path.AsSpan()))
        throw new ArgumentException(SR.Arg_PathEmpty, nameof (path));
      relativeTo = Path.GetFullPath(relativeTo);
      path = Path.GetFullPath(path);
      if (!PathInternal.AreRootsEqual(relativeTo, path, comparisonType))
        return path;
      int commonPathLength = PathInternal.GetCommonPathLength(relativeTo, path, comparisonType == StringComparison.OrdinalIgnoreCase);
      if (commonPathLength == 0)
        return path;
      int length1 = relativeTo.Length;
      if (Path.EndsInDirectorySeparator(relativeTo.AsSpan()))
        --length1;
      bool flag = Path.EndsInDirectorySeparator(path.AsSpan());
      int length2 = path.Length;
      if (flag)
        --length2;
      if (length1 == length2 && commonPathLength >= length1)
        return ".";
      StringBuilder sb = StringBuilderCache.Acquire(Math.Max(relativeTo.Length, path.Length));
      if (commonPathLength < length1)
      {
        sb.Append("..");
        for (int index = commonPathLength + 1; index < length1; ++index)
        {
          if (PathInternal.IsDirectorySeparator(relativeTo[index]))
          {
            sb.Append(Path.DirectorySeparatorChar);
            sb.Append("..");
          }
        }
      }
      else if (PathInternal.IsDirectorySeparator(path[commonPathLength]))
        ++commonPathLength;
      int count = length2 - commonPathLength;
      if (flag)
        ++count;
      if (count > 0)
      {
        if (sb.Length > 0)
          sb.Append(Path.DirectorySeparatorChar);
        sb.Append(path, commonPathLength, count);
      }
      return StringBuilderCache.GetStringAndRelease(sb);
    }

    internal static StringComparison StringComparison
    {
      get
      {
        return !Path.IsCaseSensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
      }
    }

    [NullableContext(1)]
    public static string TrimEndingDirectorySeparator(string path)
    {
      return !Path.EndsInDirectorySeparator(path) || PathInternal.IsRoot(path.AsSpan()) ? path : path.Substring(0, path.Length - 1);
    }

    public static ReadOnlySpan<char> TrimEndingDirectorySeparator(
      ReadOnlySpan<char> path)
    {
      return !Path.EndsInDirectorySeparator(path) || PathInternal.IsRoot(path) ? path : path.Slice(0, path.Length - 1);
    }

    public static bool EndsInDirectorySeparator(ReadOnlySpan<char> path)
    {
      return path.Length > 0 && PathInternal.IsDirectorySeparator(path[path.Length - 1]);
    }

    [NullableContext(1)]
    public static bool EndsInDirectorySeparator(string path)
    {
      return path != null && path.Length > 0 && PathInternal.IsDirectorySeparator(path[path.Length - 1]);
    }

    [NullableContext(1)]
    public static char[] GetInvalidFileNameChars()
    {
      return new char[41]
      {
        '"',
        '<',
        '>',
        '|',
        char.MinValue,
        '\x0001',
        '\x0002',
        '\x0003',
        '\x0004',
        '\x0005',
        '\x0006',
        '\a',
        '\b',
        '\t',
        '\n',
        '\v',
        '\f',
        '\r',
        '\x000E',
        '\x000F',
        '\x0010',
        '\x0011',
        '\x0012',
        '\x0013',
        '\x0014',
        '\x0015',
        '\x0016',
        '\x0017',
        '\x0018',
        '\x0019',
        '\x001A',
        '\x001B',
        '\x001C',
        '\x001D',
        '\x001E',
        '\x001F',
        ':',
        '*',
        '?',
        '\\',
        '/'
      };
    }

    [NullableContext(1)]
    public static char[] GetInvalidPathChars()
    {
      return new char[33]
      {
        '|',
        char.MinValue,
        '\x0001',
        '\x0002',
        '\x0003',
        '\x0004',
        '\x0005',
        '\x0006',
        '\a',
        '\b',
        '\t',
        '\n',
        '\v',
        '\f',
        '\r',
        '\x000E',
        '\x000F',
        '\x0010',
        '\x0011',
        '\x0012',
        '\x0013',
        '\x0014',
        '\x0015',
        '\x0016',
        '\x0017',
        '\x0018',
        '\x0019',
        '\x001A',
        '\x001B',
        '\x001C',
        '\x001D',
        '\x001E',
        '\x001F'
      };
    }

    [NullableContext(1)]
    public static string GetFullPath(string path)
    {
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      if (PathInternal.IsEffectivelyEmpty(path.AsSpan()))
        throw new ArgumentException(SR.Arg_PathEmpty, nameof (path));
      if (path.Contains(char.MinValue))
        throw new ArgumentException(SR.Argument_InvalidPathChars, nameof (path));
      return PathInternal.IsExtended(path.AsSpan()) ? path : PathHelper.Normalize(path);
    }

    [NullableContext(1)]
    public static string GetFullPath(string path, string basePath)
    {
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      if (basePath == null)
        throw new ArgumentNullException(nameof (basePath));
      if (!Path.IsPathFullyQualified(basePath))
        throw new ArgumentException(SR.Arg_BasePathNotFullyQualified, nameof (basePath));
      if (basePath.Contains(char.MinValue) || path.Contains(char.MinValue))
        throw new ArgumentException(SR.Argument_InvalidPathChars);
      if (Path.IsPathFullyQualified(path))
        return Path.GetFullPath(path);
      if (PathInternal.IsEffectivelyEmpty(path.AsSpan()))
        return basePath;
      int length = path.Length;
      string str = length < 1 || !PathInternal.IsDirectorySeparator(path[0]) ? (length < 2 || !PathInternal.IsValidDriveChar(path[0]) || path[1] != ':' ? Path.JoinInternal(basePath.AsSpan(), path.AsSpan()) : (!Path.GetVolumeName(path.AsSpan()).EqualsOrdinal(Path.GetVolumeName(basePath.AsSpan())) ? (!PathInternal.IsDevice(basePath.AsSpan()) ? path.Insert(2, "\\") : (length == 2 ? Path.JoinInternal(basePath.AsSpan(0, 4), path.AsSpan(), "\\".AsSpan()) : Path.JoinInternal(basePath.AsSpan(0, 4), path.AsSpan(0, 2), "\\".AsSpan(), path.AsSpan(2)))) : Path.Join(basePath.AsSpan(), path.AsSpan(2)))) : Path.Join(Path.GetPathRoot(basePath.AsSpan()), path.AsSpan(1));
      return !PathInternal.IsDevice(str.AsSpan()) ? Path.GetFullPath(str) : PathInternal.RemoveRelativeSegments(str, PathInternal.GetRootLength(str.AsSpan()));
    }

    [NullableContext(1)]
    public static unsafe string GetTempPath()
    {
      // ISSUE: untyped stack allocation
      ValueStringBuilder valueStringBuilder = new ValueStringBuilder(new Span<char>((void*) __untypedstackalloc(new IntPtr(520)), 260));
      Path.GetTempPath(ref valueStringBuilder);
      string str = PathHelper.Normalize(ref valueStringBuilder);
      valueStringBuilder.Dispose();
      return str;
    }

    private static void GetTempPath(ref ValueStringBuilder builder)
    {
      uint tempPathW;
      while ((long) (tempPathW = Interop.Kernel32.GetTempPathW(builder.Capacity, ref builder.GetPinnableReference())) > (long) builder.Capacity)
        builder.EnsureCapacity(checked ((int) tempPathW));
      if (tempPathW == 0U)
        throw Win32Marshal.GetExceptionForLastWin32Error("");
      builder.Length = (int) tempPathW;
    }

    [NullableContext(1)]
    public static unsafe string GetTempFileName()
    {
      // ISSUE: untyped stack allocation
      ValueStringBuilder builder = new ValueStringBuilder(new Span<char>((void*) __untypedstackalloc(new IntPtr(520)), 260));
      Path.GetTempPath(ref builder);
      // ISSUE: untyped stack allocation
      ValueStringBuilder path = new ValueStringBuilder(new Span<char>((void*) __untypedstackalloc(new IntPtr(520)), 260));
      uint tempFileNameW = Interop.Kernel32.GetTempFileNameW(ref builder.GetPinnableReference(), "tmp", 0U, ref path.GetPinnableReference());
      builder.Dispose();
      if (tempFileNameW == 0U)
        throw Win32Marshal.GetExceptionForLastWin32Error("");
      path.Length = path.RawChars.IndexOf<char>(char.MinValue);
      string str = PathHelper.Normalize(ref path);
      path.Dispose();
      return str;
    }

    [NullableContext(2)]
    public static bool IsPathRooted(string path)
    {
      return path != null && Path.IsPathRooted(path.AsSpan());
    }

    public static bool IsPathRooted(ReadOnlySpan<char> path)
    {
      int length = path.Length;
      if (length >= 1 && PathInternal.IsDirectorySeparator(path[0]))
        return true;
      return length >= 2 && PathInternal.IsValidDriveChar(path[0]) && path[1] == ':';
    }

    [NullableContext(2)]
    public static string GetPathRoot(string path)
    {
      if (PathInternal.IsEffectivelyEmpty(path.AsSpan()))
        return (string) null;
      ReadOnlySpan<char> pathRoot = Path.GetPathRoot(path.AsSpan());
      return path.Length == pathRoot.Length ? PathInternal.NormalizeDirectorySeparators(path) : PathInternal.NormalizeDirectorySeparators(pathRoot.ToString());
    }

    public static ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path)
    {
      if (PathInternal.IsEffectivelyEmpty(path))
        return ReadOnlySpan<char>.Empty;
      int rootLength = PathInternal.GetRootLength(path);
      return rootLength > 0 ? path.Slice(0, rootLength) : ReadOnlySpan<char>.Empty;
    }

    internal static bool IsCaseSensitive
    {
      get
      {
        return false;
      }
    }

    internal static ReadOnlySpan<char> GetVolumeName(ReadOnlySpan<char> path)
    {
      ReadOnlySpan<char> pathRoot = Path.GetPathRoot(path);
      if (pathRoot.Length == 0)
        return pathRoot;
      int start = Path.GetUncRootLength(path);
      if (start == -1)
        start = !PathInternal.IsDevice(path) ? 0 : 4;
      ReadOnlySpan<char> path1 = pathRoot.Slice(start);
      return !Path.EndsInDirectorySeparator(path1) ? path1 : path1.Slice(0, path1.Length - 1);
    }

    internal static int GetUncRootLength(ReadOnlySpan<char> path)
    {
      bool flag = PathInternal.IsDevice(path);
      if (!flag && path.Slice(0, 2).EqualsOrdinal("\\\\".AsSpan()))
        return 2;
      return flag && path.Length >= 8 && (path.Slice(0, 8).EqualsOrdinal("\\\\?\\UNC\\".AsSpan()) || path.Slice(5, 4).EqualsOrdinal("UNC\\".AsSpan())) ? 8 : -1;
    }
  }
}

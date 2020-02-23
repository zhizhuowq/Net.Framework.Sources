// Decompiled with JetBrains decompiler
// Type: System.IO.PathInternal
// Assembly: System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// MVID: BBB3B884-123D-47EA-9CD1-5BED540D02AE
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Private.CoreLib.dll

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.IO
{
  internal static class PathInternal
  {
    internal static bool StartsWithDirectorySeparator(ReadOnlySpan<char> path)
    {
      return path.Length > 0 && PathInternal.IsDirectorySeparator(path[0]);
    }

    internal static bool IsRoot(ReadOnlySpan<char> path)
    {
      return path.Length == PathInternal.GetRootLength(path);
    }

    internal static int GetCommonPathLength(string first, string second, bool ignoreCase)
    {
      int index = PathInternal.EqualStartingCharacterCount(first, second, ignoreCase);
      if (index == 0 || index == first.Length && (index == second.Length || PathInternal.IsDirectorySeparator(second[index])) || index == second.Length && PathInternal.IsDirectorySeparator(first[index]))
        return index;
      while (index > 0 && !PathInternal.IsDirectorySeparator(first[index - 1]))
        --index;
      return index;
    }

    internal static unsafe int EqualStartingCharacterCount(
      string first,
      string second,
      bool ignoreCase)
    {
      if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second))
        return 0;
      int num1 = 0;
      IntPtr num2;
      if (first == null)
      {
        num2 = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = &first.GetPinnableReference())
          num2 = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) num2;
      IntPtr num3;
      if (second == null)
      {
        num3 = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr2 = &second.GetPinnableReference())
          num3 = (IntPtr) chPtr2;
      }
      char* chPtr3 = (char*) num3;
      char* chPtr4 = chPtr1;
      char* chPtr5 = chPtr3;
      char* chPtr6 = chPtr4 + first.Length;
      for (char* chPtr2 = chPtr5 + second.Length; chPtr4 != chPtr6 && chPtr5 != chPtr2 && ((int) *chPtr4 == (int) *chPtr5 || ignoreCase && (int) char.ToUpperInvariant(*chPtr4) == (int) char.ToUpperInvariant(*chPtr5)); ++chPtr5)
      {
        ++num1;
        ++chPtr4;
      }
      // ISSUE: fixed variable is out of scope
      // ISSUE: __unpin statement
      __unpin(chPtr2);
      // ISSUE: fixed variable is out of scope
      // ISSUE: __unpin statement
      __unpin(chPtr);
      return num1;
    }

    internal static bool AreRootsEqual(
      string first,
      string second,
      StringComparison comparisonType)
    {
      int rootLength1 = PathInternal.GetRootLength(first.AsSpan());
      int rootLength2 = PathInternal.GetRootLength(second.AsSpan());
      return rootLength1 == rootLength2 && string.Compare(first, 0, second, 0, rootLength1, comparisonType) == 0;
    }

    internal static unsafe string RemoveRelativeSegments(string path, int rootLength)
    {
      // ISSUE: untyped stack allocation
      ValueStringBuilder sb = new ValueStringBuilder(new Span<char>((void*) __untypedstackalloc(new IntPtr(520)), 260));
      if (PathInternal.RemoveRelativeSegments(path.AsSpan(), rootLength, ref sb))
        path = sb.ToString();
      sb.Dispose();
      return path;
    }

    internal static bool RemoveRelativeSegments(
      ReadOnlySpan<char> path,
      int rootLength,
      ref ValueStringBuilder sb)
    {
      bool flag = false;
      int length = rootLength;
      if (PathInternal.IsDirectorySeparator(path[length - 1]))
        --length;
      if (length > 0)
        sb.Append(path.Slice(0, length));
      for (int index1 = length; index1 < path.Length; ++index1)
      {
        char c = path[index1];
        if (PathInternal.IsDirectorySeparator(c) && index1 + 1 < path.Length)
        {
          if (!PathInternal.IsDirectorySeparator(path[index1 + 1]))
          {
            if ((index1 + 2 == path.Length || PathInternal.IsDirectorySeparator(path[index1 + 2])) && path[index1 + 1] == '.')
            {
              ++index1;
              continue;
            }
            if (index1 + 2 < path.Length && (index1 + 3 == path.Length || PathInternal.IsDirectorySeparator(path[index1 + 3])) && (path[index1 + 1] == '.' && path[index1 + 2] == '.'))
            {
              int index2;
              for (index2 = sb.Length - 1; index2 >= length; --index2)
              {
                if (PathInternal.IsDirectorySeparator(sb[index2]))
                {
                  sb.Length = index1 + 3 < path.Length || index2 != length ? index2 : index2 + 1;
                  break;
                }
              }
              if (index2 < length)
                sb.Length = length;
              index1 += 2;
              continue;
            }
          }
          else
            continue;
        }
        if (c != '\\' && c == '/')
        {
          c = '\\';
          flag = true;
        }
        sb.Append(c);
      }
      if (!flag && sb.Length == path.Length)
        return false;
      if (length != rootLength && sb.Length < rootLength)
        sb.Append(path[rootLength - 1]);
      return true;
    }

    internal static bool IsValidDriveChar(char value)
    {
      if (value >= 'A' && value <= 'Z')
        return true;
      return value >= 'a' && value <= 'z';
    }

    internal static bool EndsWithPeriodOrSpace(string path)
    {
      if (string.IsNullOrEmpty(path))
        return false;
      char ch = path[path.Length - 1];
      return ch == ' ' || ch == '.';
    }

    [return: NotNullIfNotNull("path")]
    internal static string EnsureExtendedPrefixIfNeeded(string path)
    {
      return path != null && (path.Length >= 260 || PathInternal.EndsWithPeriodOrSpace(path)) ? PathInternal.EnsureExtendedPrefix(path) : path;
    }

    internal static string EnsureExtendedPrefix(string path)
    {
      if (PathInternal.IsPartiallyQualified(path.AsSpan()) || PathInternal.IsDevice(path.AsSpan()))
        return path;
      return path.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase) ? path.Insert(2, "?\\UNC\\") : "\\\\?\\" + path;
    }

    internal static bool IsDevice(ReadOnlySpan<char> path)
    {
      if (PathInternal.IsExtended(path))
        return true;
      return path.Length >= 4 && PathInternal.IsDirectorySeparator(path[0]) && PathInternal.IsDirectorySeparator(path[1]) && (path[2] == '.' || path[2] == '?') && PathInternal.IsDirectorySeparator(path[3]);
    }

    internal static bool IsDeviceUNC(ReadOnlySpan<char> path)
    {
      return path.Length >= 8 && PathInternal.IsDevice(path) && (PathInternal.IsDirectorySeparator(path[7]) && path[4] == 'U') && path[5] == 'N' && path[6] == 'C';
    }

    internal static bool IsExtended(ReadOnlySpan<char> path)
    {
      return path.Length >= 4 && path[0] == '\\' && (path[1] == '\\' || path[1] == '?') && path[2] == '?' && path[3] == '\\';
    }

    internal static int GetRootLength(ReadOnlySpan<char> path)
    {
      int length = path.Length;
      int index = 0;
      bool flag1 = PathInternal.IsDevice(path);
      bool flag2 = flag1 && PathInternal.IsDeviceUNC(path);
      if (!flag1 | flag2 && length > 0 && PathInternal.IsDirectorySeparator(path[0]))
      {
        if (flag2 || length > 1 && PathInternal.IsDirectorySeparator(path[1]))
        {
          index = flag2 ? 8 : 2;
          int num = 2;
          while (index < length && (!PathInternal.IsDirectorySeparator(path[index]) || --num > 0))
            ++index;
        }
        else
          index = 1;
      }
      else if (flag1)
      {
        index = 4;
        while (index < length && !PathInternal.IsDirectorySeparator(path[index]))
          ++index;
        if (index < length && index > 4 && PathInternal.IsDirectorySeparator(path[index]))
          ++index;
      }
      else if (length >= 2 && path[1] == ':' && PathInternal.IsValidDriveChar(path[0]))
      {
        index = 2;
        if (length > 2 && PathInternal.IsDirectorySeparator(path[2]))
          ++index;
      }
      return index;
    }

    internal static bool IsPartiallyQualified(ReadOnlySpan<char> path)
    {
      if (path.Length < 2)
        return true;
      return PathInternal.IsDirectorySeparator(path[0]) ? path[1] != '?' && !PathInternal.IsDirectorySeparator(path[1]) : path.Length < 3 || path[1] != ':' || !PathInternal.IsDirectorySeparator(path[2]) || !PathInternal.IsValidDriveChar(path[0]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsDirectorySeparator(char c)
    {
      return c == '\\' || c == '/';
    }

    internal static unsafe string NormalizeDirectorySeparators(string path)
    {
      if (string.IsNullOrEmpty(path))
        return path;
      bool flag = true;
      for (int index = 0; index < path.Length; ++index)
      {
        char c = path[index];
        if (PathInternal.IsDirectorySeparator(c) && (c != '\\' || index > 0 && index + 1 < path.Length && PathInternal.IsDirectorySeparator(path[index + 1])))
        {
          flag = false;
          break;
        }
      }
      if (flag)
        return path;
      // ISSUE: untyped stack allocation
      ValueStringBuilder valueStringBuilder = new ValueStringBuilder(new Span<char>((void*) __untypedstackalloc(new IntPtr(520)), 260));
      int index1 = 0;
      if (PathInternal.IsDirectorySeparator(path[index1]))
      {
        ++index1;
        valueStringBuilder.Append('\\');
      }
      for (int index2 = index1; index2 < path.Length; ++index2)
      {
        char c = path[index2];
        if (PathInternal.IsDirectorySeparator(c))
        {
          if (index2 + 1 >= path.Length || !PathInternal.IsDirectorySeparator(path[index2 + 1]))
            c = '\\';
          else
            continue;
        }
        valueStringBuilder.Append(c);
      }
      return valueStringBuilder.ToString();
    }

    internal static bool IsEffectivelyEmpty(ReadOnlySpan<char> path)
    {
      if (path.IsEmpty)
        return true;
      ReadOnlySpan<char> readOnlySpan = path;
      for (int index = 0; index < readOnlySpan.Length; ++index)
      {
        if (readOnlySpan[index] != ' ')
          return false;
      }
      return true;
    }
  }
}

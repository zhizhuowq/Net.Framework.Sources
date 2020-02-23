// Decompiled with JetBrains decompiler
// Type: Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo
// Assembly: Microsoft.CSharp, Version=4.0.5.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: 614A787F-FB0A-409B-A86E-6A7884E2A37F
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\Microsoft.CSharp.dll

using System.ComponentModel;

namespace Microsoft.CSharp.RuntimeBinder
{
  /// <summary>表示有关特定于调用站点上的特定自变量的 C# 动态操作的信息。 此类的实例由 C# 编译器生成。</summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public sealed class CSharpArgumentInfo
  {
    internal static readonly CSharpArgumentInfo None = new CSharpArgumentInfo(CSharpArgumentInfoFlags.None, (string) null);

    internal CSharpArgumentInfoFlags Flags { get; }

    internal string Name { get; }

    private CSharpArgumentInfo(CSharpArgumentInfoFlags flags, string name)
    {
      this.Flags = flags;
      this.Name = name;
    }

    /// <summary>初始化 <see cref="T:Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo" /> 类的新实例。</summary>
    /// <param name="flags">参数的标志。</param>
    /// <param name="name">如果已指定参数名称，则为相应的名称；否则为空。</param>
    /// <returns>
    /// <see cref="T:Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo" /> 类的新实例。</returns>
    public static CSharpArgumentInfo Create(
      CSharpArgumentInfoFlags flags,
      string name)
    {
      return new CSharpArgumentInfo(flags, name);
    }

    internal bool UseCompileTimeType
    {
      get
      {
        return (uint) (this.Flags & CSharpArgumentInfoFlags.UseCompileTimeType) > 0U;
      }
    }

    internal bool LiteralConstant
    {
      get
      {
        return (uint) (this.Flags & CSharpArgumentInfoFlags.Constant) > 0U;
      }
    }

    internal bool NamedArgument
    {
      get
      {
        return (uint) (this.Flags & CSharpArgumentInfoFlags.NamedArgument) > 0U;
      }
    }

    internal bool IsByRefOrOut
    {
      get
      {
        return (uint) (this.Flags & (CSharpArgumentInfoFlags.IsRef | CSharpArgumentInfoFlags.IsOut)) > 0U;
      }
    }

    internal bool IsOut
    {
      get
      {
        return (uint) (this.Flags & CSharpArgumentInfoFlags.IsOut) > 0U;
      }
    }

    internal bool IsStaticType
    {
      get
      {
        return (uint) (this.Flags & CSharpArgumentInfoFlags.IsStaticType) > 0U;
      }
    }
  }
}

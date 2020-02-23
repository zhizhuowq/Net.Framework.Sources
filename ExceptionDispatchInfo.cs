// Decompiled with JetBrains decompiler
// Type: System.Runtime.ExceptionServices.ExceptionDispatchInfo
// Assembly: System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// MVID: BBB3B884-123D-47EA-9CD1-5BED540D02AE
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Private.CoreLib.dll

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Runtime.ExceptionServices
{
  [Nullable(0)]
  [NullableContext(1)]
  public sealed class ExceptionDispatchInfo
  {
    private readonly Exception _exception;
    private readonly Exception.DispatchState _dispatchState;

    private ExceptionDispatchInfo(Exception exception)
    {
      this._exception = exception;
      this._dispatchState = exception.CaptureDispatchState();
    }

    public static ExceptionDispatchInfo Capture(Exception source)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      return new ExceptionDispatchInfo(source);
    }

    public Exception SourceException
    {
      get
      {
        return this._exception;
      }
    }

    [StackTraceHidden]
    [DoesNotReturn]
    public void Throw()
    {
      this._exception.RestoreDispatchState(in this._dispatchState);
      throw this._exception;
    }

    [DoesNotReturn]
    public static void Throw(Exception source)
    {
      ExceptionDispatchInfo.Capture(source).Throw();
    }
  }
}

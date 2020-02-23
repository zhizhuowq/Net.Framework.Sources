// Decompiled with JetBrains decompiler
// Type: System.Threading.ManualResetEventSlim
// Assembly: System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// MVID: BBB3B884-123D-47EA-9CD1-5BED540D02AE
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Private.CoreLib.dll

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Threading
{
  [DebuggerDisplay("Set = {IsSet}")]
  public class ManualResetEventSlim : IDisposable
  {
    private static readonly Action<object> s_cancellationTokenCallback = new Action<object>(ManualResetEventSlim.CancellationTokenCallback);
    private volatile object m_lock;
    private volatile ManualResetEvent m_eventObj;
    private volatile int m_combinedState;

    [Nullable(1)]
    public WaitHandle WaitHandle
    {
      [NullableContext(1)] get
      {
        this.ThrowIfDisposed();
        if (this.m_eventObj == null)
          this.LazyInitializeEvent();
        return (WaitHandle) this.m_eventObj;
      }
    }

    public bool IsSet
    {
      get
      {
        return (uint) ManualResetEventSlim.ExtractStatePortion(this.m_combinedState, int.MinValue) > 0U;
      }
      private set
      {
        this.UpdateStateAtomically((value ? 1 : 0) << 31, int.MinValue);
      }
    }

    public int SpinCount
    {
      get
      {
        return ManualResetEventSlim.ExtractStatePortionAndShiftRight(this.m_combinedState, 1073217536, 19);
      }
      private set
      {
        this.m_combinedState = this.m_combinedState & -1073217537 | value << 19;
      }
    }

    private int Waiters
    {
      get
      {
        return ManualResetEventSlim.ExtractStatePortionAndShiftRight(this.m_combinedState, 524287, 0);
      }
      set
      {
        if (value >= 524287)
          throw new InvalidOperationException(SR.Format(SR.ManualResetEventSlim_ctor_TooManyWaiters, (object) 524287));
        this.UpdateStateAtomically(value, 524287);
      }
    }

    public ManualResetEventSlim()
      : this(false)
    {
    }

    public ManualResetEventSlim(bool initialState)
    {
      this.Initialize(initialState, SpinWait.SpinCountforSpinBeforeWait);
    }

    public ManualResetEventSlim(bool initialState, int spinCount)
    {
      if (spinCount < 0)
        throw new ArgumentOutOfRangeException(nameof (spinCount));
      if (spinCount > 2047)
        throw new ArgumentOutOfRangeException(nameof (spinCount), SR.Format(SR.ManualResetEventSlim_ctor_SpinCountOutOfRange, (object) 2047));
      this.Initialize(initialState, spinCount);
    }

    private void Initialize(bool initialState, int spinCount)
    {
      this.m_combinedState = initialState ? int.MinValue : 0;
      this.SpinCount = PlatformHelper.IsSingleProcessor ? 1 : spinCount;
    }

    private void EnsureLockObjectCreated()
    {
      if (this.m_lock != null)
        return;
      Interlocked.CompareExchange(ref this.m_lock, new object(), (object) null);
    }

    private bool LazyInitializeEvent()
    {
      bool isSet = this.IsSet;
      ManualResetEvent manualResetEvent = new ManualResetEvent(isSet);
      if (Interlocked.CompareExchange<ManualResetEvent>(ref this.m_eventObj, manualResetEvent, (ManualResetEvent) null) != null)
      {
        manualResetEvent.Dispose();
        return false;
      }
      if (this.IsSet != isSet)
      {
        lock (manualResetEvent)
        {
          if (this.m_eventObj == manualResetEvent)
            manualResetEvent.Set();
        }
      }
      return true;
    }

    public void Set()
    {
      this.Set(false);
    }

    private void Set(bool duringCancellation)
    {
      this.IsSet = true;
      if (this.Waiters > 0)
      {
        lock (this.m_lock)
          Monitor.PulseAll(this.m_lock);
      }
      ManualResetEvent eventObj = this.m_eventObj;
      if (eventObj == null || duringCancellation)
        return;
      lock (eventObj)
      {
        if (this.m_eventObj == null)
          return;
        this.m_eventObj.Set();
      }
    }

    public void Reset()
    {
      this.ThrowIfDisposed();
      if (this.m_eventObj != null)
        this.m_eventObj.Reset();
      this.IsSet = false;
    }

    public void Wait()
    {
      this.Wait(-1, new CancellationToken());
    }

    public void Wait(CancellationToken cancellationToken)
    {
      this.Wait(-1, cancellationToken);
    }

    public bool Wait(TimeSpan timeout)
    {
      long totalMilliseconds = (long) timeout.TotalMilliseconds;
      if (totalMilliseconds < -1L || totalMilliseconds > (long) int.MaxValue)
        throw new ArgumentOutOfRangeException(nameof (timeout));
      return this.Wait((int) totalMilliseconds, new CancellationToken());
    }

    public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
    {
      long totalMilliseconds = (long) timeout.TotalMilliseconds;
      if (totalMilliseconds < -1L || totalMilliseconds > (long) int.MaxValue)
        throw new ArgumentOutOfRangeException(nameof (timeout));
      return this.Wait((int) totalMilliseconds, cancellationToken);
    }

    public bool Wait(int millisecondsTimeout)
    {
      return this.Wait(millisecondsTimeout, new CancellationToken());
    }

    public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
    {
      this.ThrowIfDisposed();
      cancellationToken.ThrowIfCancellationRequested();
      if (millisecondsTimeout < -1)
        throw new ArgumentOutOfRangeException(nameof (millisecondsTimeout));
      if (!this.IsSet)
      {
        if (millisecondsTimeout == 0)
          return false;
        uint startTime = 0;
        bool flag = false;
        int millisecondsTimeout1 = millisecondsTimeout;
        if (millisecondsTimeout != -1)
        {
          startTime = TimeoutHelper.GetTime();
          flag = true;
        }
        int spinCount = this.SpinCount;
        SpinWait spinWait = new SpinWait();
        while (spinWait.Count < spinCount)
        {
          spinWait.SpinOnce(-1);
          if (this.IsSet)
            return true;
          if (spinWait.Count >= 100 && spinWait.Count % 10 == 0)
            cancellationToken.ThrowIfCancellationRequested();
        }
        this.EnsureLockObjectCreated();
        using (cancellationToken.UnsafeRegister(ManualResetEventSlim.s_cancellationTokenCallback, (object) this))
        {
          lock (this.m_lock)
          {
            while (!this.IsSet)
            {
              cancellationToken.ThrowIfCancellationRequested();
              if (flag)
              {
                millisecondsTimeout1 = TimeoutHelper.UpdateTimeOut(startTime, millisecondsTimeout);
                if (millisecondsTimeout1 <= 0)
                  return false;
              }
              ++this.Waiters;
              if (this.IsSet)
              {
                --this.Waiters;
                return true;
              }
              try
              {
                if (!Monitor.Wait(this.m_lock, millisecondsTimeout1))
                  return false;
              }
              finally
              {
                --this.Waiters;
              }
            }
          }
        }
      }
      return true;
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if ((this.m_combinedState & 1073741824) != 0)
        return;
      this.m_combinedState |= 1073741824;
      if (!disposing)
        return;
      ManualResetEvent eventObj = this.m_eventObj;
      if (eventObj == null)
        return;
      lock (eventObj)
      {
        eventObj.Dispose();
        this.m_eventObj = (ManualResetEvent) null;
      }
    }

    private void ThrowIfDisposed()
    {
      if ((this.m_combinedState & 1073741824) != 0)
        throw new ObjectDisposedException(SR.ManualResetEventSlim_Disposed);
    }

    private static void CancellationTokenCallback(object obj)
    {
      ManualResetEventSlim manualResetEventSlim = (ManualResetEventSlim) obj;
      lock (manualResetEventSlim.m_lock)
        Monitor.PulseAll(manualResetEventSlim.m_lock);
    }

    private void UpdateStateAtomically(int newBits, int updateBitsMask)
    {
      SpinWait spinWait = new SpinWait();
      while (true)
      {
        int combinedState = this.m_combinedState;
        if (Interlocked.CompareExchange(ref this.m_combinedState, combinedState & ~updateBitsMask | newBits, combinedState) != combinedState)
          spinWait.SpinOnce(-1);
        else
          break;
      }
    }

    private static int ExtractStatePortionAndShiftRight(
      int state,
      int mask,
      int rightBitShiftCount)
    {
      return (int) ((uint) (state & mask) >> rightBitShiftCount);
    }

    private static int ExtractStatePortion(int state, int mask)
    {
      return state & mask;
    }
  }
}

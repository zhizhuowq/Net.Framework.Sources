// Decompiled with JetBrains decompiler
// Type: System.Threading.Tasks.Task
// Assembly: System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// MVID: BBB3B884-123D-47EA-9CD1-5BED540D02AE
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Private.CoreLib.dll

using Internal.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.WindowsRuntime;

namespace System.Threading.Tasks
{
  [Nullable(0)]
  [NullableContext(1)]
  [DebuggerDisplay("Id = {Id}, Status = {Status}, Method = {DebuggerDisplayMethodDescription}")]
  [DebuggerTypeProxy(typeof (SystemThreadingTasks_TaskDebugView))]
  public class Task : IAsyncResult, IDisposable
  {
    private static readonly object s_taskCompletionSentinel = new object();
    private static readonly ContextCallback s_ecCallback = (ContextCallback) (obj => Unsafe.As<Task>(obj).InnerInvoke());
    [ThreadStatic]
    internal static Task t_currentTask;
    internal static int s_taskIdCounter;
    private volatile int m_taskId;
    internal Delegate m_action;
    internal object m_stateObject;
    internal TaskScheduler m_taskScheduler;
    internal volatile int m_stateFlags;
    private volatile object m_continuationObject;
    internal static bool s_asyncDebuggingEnabled;
    private static Dictionary<int, Task> s_currentActiveTasks;
    internal Task.ContingentProperties m_contingentProperties;

    [Nullable(2)]
    private Task ParentForDebugger
    {
      get
      {
        return this.m_contingentProperties?.m_parent;
      }
    }

    private int StateFlagsForDebugger
    {
      get
      {
        return this.m_stateFlags;
      }
    }

    internal static bool AddToActiveTasks(Task task)
    {
      LazyInitializer.EnsureInitialized<Dictionary<int, Task>>(ref Task.s_currentActiveTasks, (Func<Dictionary<int, Task>>) (() => new Dictionary<int, Task>()));
      int id = task.Id;
      lock (Task.s_currentActiveTasks)
        Task.s_currentActiveTasks[id] = task;
      return true;
    }

    internal static void RemoveFromActiveTasks(Task task)
    {
      if (Task.s_currentActiveTasks == null)
        return;
      int id = task.Id;
      lock (Task.s_currentActiveTasks)
        Task.s_currentActiveTasks.Remove(id);
    }

    internal Task(bool canceled, TaskCreationOptions creationOptions, CancellationToken ct)
    {
      int num = (int) creationOptions;
      if (canceled)
      {
        this.m_stateFlags = 5242880 | num;
        this.m_contingentProperties = new Task.ContingentProperties()
        {
          m_cancellationToken = ct,
          m_internalCancellationRequested = 1
        };
      }
      else
        this.m_stateFlags = 16777216 | num;
    }

    internal Task()
    {
      this.m_stateFlags = 33555456;
    }

    internal Task(object state, TaskCreationOptions creationOptions, bool promiseStyle)
    {
      if ((creationOptions & ~(TaskCreationOptions.AttachedToParent | TaskCreationOptions.RunContinuationsAsynchronously)) != TaskCreationOptions.None)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.creationOptions);
      if ((creationOptions & TaskCreationOptions.AttachedToParent) != TaskCreationOptions.None)
      {
        Task internalCurrent = Task.InternalCurrent;
        if (internalCurrent != null)
          this.EnsureContingentPropertiesInitializedUnsafe().m_parent = internalCurrent;
      }
      this.TaskConstructorCore((Delegate) null, state, new CancellationToken(), creationOptions, InternalTaskOptions.PromiseTask, (TaskScheduler) null);
    }

    public Task(Action action)
      : this((Delegate) action, (object) null, (Task) null, new CancellationToken(), TaskCreationOptions.None, InternalTaskOptions.None, (TaskScheduler) null)
    {
    }

    public Task(Action action, CancellationToken cancellationToken)
      : this((Delegate) action, (object) null, (Task) null, cancellationToken, TaskCreationOptions.None, InternalTaskOptions.None, (TaskScheduler) null)
    {
    }

    public Task(Action action, TaskCreationOptions creationOptions)
      : this((Delegate) action, (object) null, Task.InternalCurrentIfAttached(creationOptions), new CancellationToken(), creationOptions, InternalTaskOptions.None, (TaskScheduler) null)
    {
    }

    public Task(
      Action action,
      CancellationToken cancellationToken,
      TaskCreationOptions creationOptions)
      : this((Delegate) action, (object) null, Task.InternalCurrentIfAttached(creationOptions), cancellationToken, creationOptions, InternalTaskOptions.None, (TaskScheduler) null)
    {
    }

    [NullableContext(2)]
    public Task([Nullable(new byte[] {1, 2})] Action<object> action, object state)
      : this((Delegate) action, state, (Task) null, new CancellationToken(), TaskCreationOptions.None, InternalTaskOptions.None, (TaskScheduler) null)
    {
    }

    [NullableContext(2)]
    public Task([Nullable(new byte[] {1, 2})] Action<object> action, object state, CancellationToken cancellationToken)
      : this((Delegate) action, state, (Task) null, cancellationToken, TaskCreationOptions.None, InternalTaskOptions.None, (TaskScheduler) null)
    {
    }

    [NullableContext(2)]
    public Task([Nullable(new byte[] {1, 2})] Action<object> action, object state, TaskCreationOptions creationOptions)
      : this((Delegate) action, state, Task.InternalCurrentIfAttached(creationOptions), new CancellationToken(), creationOptions, InternalTaskOptions.None, (TaskScheduler) null)
    {
    }

    [NullableContext(2)]
    public Task(
      [Nullable(new byte[] {1, 2})] Action<object> action,
      object state,
      CancellationToken cancellationToken,
      TaskCreationOptions creationOptions)
      : this((Delegate) action, state, Task.InternalCurrentIfAttached(creationOptions), cancellationToken, creationOptions, InternalTaskOptions.None, (TaskScheduler) null)
    {
    }

    internal Task(
      Delegate action,
      object state,
      Task parent,
      CancellationToken cancellationToken,
      TaskCreationOptions creationOptions,
      InternalTaskOptions internalOptions,
      TaskScheduler scheduler)
    {
      if ((object) action == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);
      if (parent != null && (creationOptions & TaskCreationOptions.AttachedToParent) != TaskCreationOptions.None)
        this.EnsureContingentPropertiesInitializedUnsafe().m_parent = parent;
      this.TaskConstructorCore(action, state, cancellationToken, creationOptions, internalOptions, scheduler);
      this.CapturedContext = ExecutionContext.Capture();
    }

    internal void TaskConstructorCore(
      Delegate action,
      object state,
      CancellationToken cancellationToken,
      TaskCreationOptions creationOptions,
      InternalTaskOptions internalOptions,
      TaskScheduler scheduler)
    {
      this.m_action = action;
      this.m_stateObject = state;
      this.m_taskScheduler = scheduler;
      if ((creationOptions & ~(TaskCreationOptions.PreferFairness | TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent | TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler | TaskCreationOptions.RunContinuationsAsynchronously)) != TaskCreationOptions.None)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.creationOptions);
      int num = (int) (creationOptions | (TaskCreationOptions) internalOptions);
      this.m_stateFlags = (object) this.m_action == null || (internalOptions & InternalTaskOptions.ContinuationTask) != InternalTaskOptions.None ? num | 33554432 : num;
      Task.ContingentProperties contingentProperties = this.m_contingentProperties;
      if (contingentProperties != null)
      {
        Task parent = contingentProperties.m_parent;
        if (parent != null && (creationOptions & TaskCreationOptions.AttachedToParent) != TaskCreationOptions.None && (parent.CreationOptions & TaskCreationOptions.DenyChildAttach) == TaskCreationOptions.None)
          parent.AddNewChild();
      }
      if (!cancellationToken.CanBeCanceled)
        return;
      this.AssignCancellationToken(cancellationToken, (Task) null, (TaskContinuation) null);
    }

    private void AssignCancellationToken(
      CancellationToken cancellationToken,
      Task antecedent,
      TaskContinuation continuation)
    {
      Task.ContingentProperties contingentProperties = this.EnsureContingentPropertiesInitializedUnsafe();
      contingentProperties.m_cancellationToken = cancellationToken;
      try
      {
        if ((this.Options & (TaskCreationOptions) 13312) != TaskCreationOptions.None)
          return;
        if (cancellationToken.IsCancellationRequested)
        {
          this.InternalCancel(false);
        }
        else
        {
          CancellationTokenRegistration tokenRegistration = antecedent != null ? cancellationToken.UnsafeRegister((Action<object>) (t =>
          {
            Tuple<Task, Task, TaskContinuation> tuple = (Tuple<Task, Task, TaskContinuation>) t;
            Task task = tuple.Item1;
            tuple.Item2.RemoveContinuation((object) tuple.Item3);
            task.InternalCancel(false);
          }), (object) new Tuple<Task, Task, TaskContinuation>(this, antecedent, continuation)) : cancellationToken.UnsafeRegister((Action<object>) (t => ((Task) t).InternalCancel(false)), (object) this);
          contingentProperties.m_cancellationRegistration = new StrongBox<CancellationTokenRegistration>(tokenRegistration);
        }
      }
      catch
      {
        Task parent = this.m_contingentProperties?.m_parent;
        if (parent != null && (this.Options & TaskCreationOptions.AttachedToParent) != TaskCreationOptions.None && (parent.Options & TaskCreationOptions.DenyChildAttach) == TaskCreationOptions.None)
          parent.DisregardChild();
        throw;
      }
    }

    private string DebuggerDisplayMethodDescription
    {
      get
      {
        Delegate action = this.m_action;
        return ((object) action != null ? action.Method.ToString() : (string) null) ?? "{null}";
      }
    }

    internal TaskCreationOptions Options
    {
      get
      {
        return Task.OptionsMethod(this.m_stateFlags);
      }
    }

    internal static TaskCreationOptions OptionsMethod(int flags)
    {
      return (TaskCreationOptions) (flags & (int) ushort.MaxValue);
    }

    internal bool AtomicStateUpdate(int newBits, int illegalBits)
    {
      int stateFlags = this.m_stateFlags;
      if ((stateFlags & illegalBits) != 0)
        return false;
      return Interlocked.CompareExchange(ref this.m_stateFlags, stateFlags | newBits, stateFlags) == stateFlags || this.AtomicStateUpdateSlow(newBits, illegalBits);
    }

    private bool AtomicStateUpdateSlow(int newBits, int illegalBits)
    {
      int num;
      for (int comparand = this.m_stateFlags; (comparand & illegalBits) == 0; comparand = num)
      {
        num = Interlocked.CompareExchange(ref this.m_stateFlags, comparand | newBits, comparand);
        if (num == comparand)
          return true;
      }
      return false;
    }

    internal bool AtomicStateUpdate(int newBits, int illegalBits, ref int oldFlags)
    {
      for (int comparand = oldFlags = this.m_stateFlags; (comparand & illegalBits) == 0; comparand = oldFlags)
      {
        oldFlags = Interlocked.CompareExchange(ref this.m_stateFlags, comparand | newBits, comparand);
        if (oldFlags == comparand)
          return true;
      }
      return false;
    }

    internal void SetNotificationForWaitCompletion(bool enabled)
    {
      if (enabled)
      {
        this.AtomicStateUpdate(268435456, 90177536);
      }
      else
      {
        int comparand = this.m_stateFlags;
        while (true)
        {
          int num = Interlocked.CompareExchange(ref this.m_stateFlags, comparand & -268435457, comparand);
          if (num != comparand)
            comparand = num;
          else
            break;
        }
      }
    }

    internal bool NotifyDebuggerOfWaitCompletionIfNecessary()
    {
      if (!this.IsWaitNotificationEnabled || !this.ShouldNotifyDebuggerOfWaitCompletion)
        return false;
      this.NotifyDebuggerOfWaitCompletion();
      return true;
    }

    internal static bool AnyTaskRequiresNotifyDebuggerOfWaitCompletion(Task[] tasks)
    {
      foreach (Task task in tasks)
      {
        if (task != null && task.IsWaitNotificationEnabled && task.ShouldNotifyDebuggerOfWaitCompletion)
          return true;
      }
      return false;
    }

    internal bool IsWaitNotificationEnabledOrNotRanToCompletion
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)] get
      {
        return (this.m_stateFlags & 285212672) != 16777216;
      }
    }

    internal virtual bool ShouldNotifyDebuggerOfWaitCompletion
    {
      get
      {
        return this.IsWaitNotificationEnabled;
      }
    }

    internal bool IsWaitNotificationEnabled
    {
      get
      {
        return (uint) (this.m_stateFlags & 268435456) > 0U;
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private void NotifyDebuggerOfWaitCompletion()
    {
      this.SetNotificationForWaitCompletion(false);
    }

    internal bool MarkStarted()
    {
      return this.AtomicStateUpdate(65536, 4259840);
    }

    internal void FireTaskScheduledIfNeeded(TaskScheduler ts)
    {
      if ((this.m_stateFlags & 1073741824) != 0)
        return;
      this.m_stateFlags |= 1073741824;
      Task internalCurrent = Task.InternalCurrent;
      Task parent = this.m_contingentProperties?.m_parent;
      TplEventSource.Log.TaskScheduled(ts.Id, internalCurrent == null ? 0 : internalCurrent.Id, this.Id, parent == null ? 0 : parent.Id, (int) this.Options, 1);
    }

    internal void AddNewChild()
    {
      Task.ContingentProperties contingentProperties = this.EnsureContingentPropertiesInitialized();
      if (contingentProperties.m_completionCountdown == 1)
        ++contingentProperties.m_completionCountdown;
      else
        Interlocked.Increment(ref contingentProperties.m_completionCountdown);
    }

    internal void DisregardChild()
    {
      Interlocked.Decrement(ref this.EnsureContingentPropertiesInitialized().m_completionCountdown);
    }

    public void Start()
    {
      this.Start(TaskScheduler.Current);
    }

    public void Start(TaskScheduler scheduler)
    {
      int stateFlags = this.m_stateFlags;
      if (Task.IsCompletedMethod(stateFlags))
        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.Task_Start_TaskCompleted);
      if (scheduler == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.scheduler);
      TaskCreationOptions taskCreationOptions = Task.OptionsMethod(stateFlags);
      if ((taskCreationOptions & (TaskCreationOptions) 1024) != TaskCreationOptions.None)
        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.Task_Start_Promise);
      if ((taskCreationOptions & (TaskCreationOptions) 512) != TaskCreationOptions.None)
        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.Task_Start_ContinuationTask);
      if (Interlocked.CompareExchange<TaskScheduler>(ref this.m_taskScheduler, scheduler, (TaskScheduler) null) != null)
        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.Task_Start_AlreadyStarted);
      this.ScheduleAndStart(true);
    }

    public void RunSynchronously()
    {
      this.InternalRunSynchronously(TaskScheduler.Current, true);
    }

    public void RunSynchronously(TaskScheduler scheduler)
    {
      if (scheduler == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.scheduler);
      this.InternalRunSynchronously(scheduler, true);
    }

    internal void InternalRunSynchronously(TaskScheduler scheduler, bool waitForCompletion)
    {
      int stateFlags = this.m_stateFlags;
      TaskCreationOptions taskCreationOptions = Task.OptionsMethod(stateFlags);
      if ((taskCreationOptions & (TaskCreationOptions) 512) != TaskCreationOptions.None)
        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.Task_RunSynchronously_Continuation);
      if ((taskCreationOptions & (TaskCreationOptions) 1024) != TaskCreationOptions.None)
        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.Task_RunSynchronously_Promise);
      if (Task.IsCompletedMethod(stateFlags))
        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.Task_RunSynchronously_TaskCompleted);
      if (Interlocked.CompareExchange<TaskScheduler>(ref this.m_taskScheduler, scheduler, (TaskScheduler) null) != null)
        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.Task_RunSynchronously_AlreadyStarted);
      if (this.MarkStarted())
      {
        bool flag = false;
        try
        {
          if (!scheduler.TryRunInline(this, false))
          {
            scheduler.InternalQueueTask(this);
            flag = true;
          }
          if (!waitForCompletion || this.IsCompleted)
            return;
          this.SpinThenBlockingWait(-1, new CancellationToken());
        }
        catch (System.Exception ex)
        {
          if (!flag)
          {
            TaskSchedulerException schedulerException = new TaskSchedulerException(ex);
            this.AddException((object) schedulerException);
            this.Finish(false);
            this.m_contingentProperties.m_exceptionsHolder.MarkAsHandled(false);
            throw schedulerException;
          }
          throw;
        }
      }
      else
        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.Task_RunSynchronously_TaskCompleted);
    }

    internal static Task InternalStartNew(
      Task creatingTask,
      Delegate action,
      object state,
      CancellationToken cancellationToken,
      TaskScheduler scheduler,
      TaskCreationOptions options,
      InternalTaskOptions internalOptions)
    {
      if (scheduler == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.scheduler);
      Task task = new Task(action, state, creatingTask, cancellationToken, options, internalOptions | InternalTaskOptions.QueuedByRuntime, scheduler);
      task.ScheduleAndStart(false);
      return task;
    }

    internal static int NewId()
    {
      int TaskID;
      do
      {
        TaskID = Interlocked.Increment(ref Task.s_taskIdCounter);
      }
      while (TaskID == 0);
      if (TplEventSource.Log.IsEnabled())
        TplEventSource.Log.NewID(TaskID);
      return TaskID;
    }

    public int Id
    {
      get
      {
        if (this.m_taskId == 0)
          Interlocked.CompareExchange(ref this.m_taskId, Task.NewId(), 0);
        return this.m_taskId;
      }
    }

    public static int? CurrentId
    {
      get
      {
        return Task.InternalCurrent?.Id;
      }
    }

    [Nullable(2)]
    internal static Task InternalCurrent
    {
      get
      {
        return Task.t_currentTask;
      }
    }

    internal static Task InternalCurrentIfAttached(TaskCreationOptions creationOptions)
    {
      return (creationOptions & TaskCreationOptions.AttachedToParent) == TaskCreationOptions.None ? (Task) null : Task.InternalCurrent;
    }

    [Nullable(2)]
    public AggregateException Exception
    {
      [NullableContext(2)] get
      {
        AggregateException aggregateException = (AggregateException) null;
        if (this.IsFaulted)
          aggregateException = this.GetExceptions(false);
        return aggregateException;
      }
    }

    public TaskStatus Status
    {
      get
      {
        int stateFlags = this.m_stateFlags;
        return (stateFlags & 2097152) == 0 ? ((stateFlags & 4194304) == 0 ? ((stateFlags & 16777216) == 0 ? ((stateFlags & 8388608) == 0 ? ((stateFlags & 131072) == 0 ? ((stateFlags & 65536) == 0 ? ((stateFlags & 33554432) == 0 ? TaskStatus.Created : TaskStatus.WaitingForActivation) : TaskStatus.WaitingToRun) : TaskStatus.Running) : TaskStatus.WaitingForChildrenToComplete) : TaskStatus.RanToCompletion) : TaskStatus.Canceled) : TaskStatus.Faulted;
      }
    }

    public bool IsCanceled
    {
      get
      {
        return (this.m_stateFlags & 6291456) == 4194304;
      }
    }

    internal bool IsCancellationRequested
    {
      get
      {
        Task.ContingentProperties contingentProperties = Volatile.Read<Task.ContingentProperties>(ref this.m_contingentProperties);
        if (contingentProperties == null)
          return false;
        return contingentProperties.m_internalCancellationRequested == 1 || contingentProperties.m_cancellationToken.IsCancellationRequested;
      }
    }

    internal Task.ContingentProperties EnsureContingentPropertiesInitialized()
    {
      return LazyInitializer.EnsureInitialized<Task.ContingentProperties>(ref this.m_contingentProperties, (Func<Task.ContingentProperties>) (() => new Task.ContingentProperties()));
    }

    internal Task.ContingentProperties EnsureContingentPropertiesInitializedUnsafe()
    {
      return this.m_contingentProperties ?? (this.m_contingentProperties = new Task.ContingentProperties());
    }

    internal CancellationToken CancellationToken
    {
      get
      {
        Task.ContingentProperties contingentProperties = Volatile.Read<Task.ContingentProperties>(ref this.m_contingentProperties);
        return contingentProperties != null ? contingentProperties.m_cancellationToken : new CancellationToken();
      }
    }

    internal bool IsCancellationAcknowledged
    {
      get
      {
        return (uint) (this.m_stateFlags & 1048576) > 0U;
      }
    }

    public bool IsCompleted
    {
      get
      {
        return Task.IsCompletedMethod(this.m_stateFlags);
      }
    }

    private static bool IsCompletedMethod(int flags)
    {
      return (uint) (flags & 23068672) > 0U;
    }

    public bool IsCompletedSuccessfully
    {
      get
      {
        return (this.m_stateFlags & 23068672) == 16777216;
      }
    }

    public TaskCreationOptions CreationOptions
    {
      get
      {
        return this.Options & (TaskCreationOptions) -65281;
      }
    }

    WaitHandle IAsyncResult.AsyncWaitHandle
    {
      get
      {
        if ((uint) (this.m_stateFlags & 262144) > 0U)
          ThrowHelper.ThrowObjectDisposedException(ExceptionResource.Task_ThrowIfDisposed);
        return this.CompletedEvent.WaitHandle;
      }
    }

    [Nullable(2)]
    public object AsyncState
    {
      [NullableContext(2)] get
      {
        return this.m_stateObject;
      }
    }

    bool IAsyncResult.CompletedSynchronously
    {
      get
      {
        return false;
      }
    }

    [Nullable(2)]
    internal TaskScheduler ExecutingTaskScheduler
    {
      get
      {
        return this.m_taskScheduler;
      }
    }

    public static TaskFactory Factory { get; } = new TaskFactory();

    public static Task CompletedTask { get; } = new Task(false, (TaskCreationOptions) 16384, new CancellationToken());

    internal ManualResetEventSlim CompletedEvent
    {
      get
      {
        Task.ContingentProperties contingentProperties = this.EnsureContingentPropertiesInitialized();
        if (contingentProperties.m_completionEvent == null)
        {
          bool isCompleted = this.IsCompleted;
          ManualResetEventSlim manualResetEventSlim = new ManualResetEventSlim(isCompleted);
          if (Interlocked.CompareExchange<ManualResetEventSlim>(ref contingentProperties.m_completionEvent, manualResetEventSlim, (ManualResetEventSlim) null) != null)
            manualResetEventSlim.Dispose();
          else if (!isCompleted && this.IsCompleted)
            manualResetEventSlim.Set();
        }
        return contingentProperties.m_completionEvent;
      }
    }

    internal bool ExceptionRecorded
    {
      get
      {
        Task.ContingentProperties contingentProperties = Volatile.Read<Task.ContingentProperties>(ref this.m_contingentProperties);
        return contingentProperties != null && contingentProperties.m_exceptionsHolder != null && contingentProperties.m_exceptionsHolder.ContainsFaultList;
      }
    }

    public bool IsFaulted
    {
      get
      {
        return (uint) (this.m_stateFlags & 2097152) > 0U;
      }
    }

    [Nullable(2)]
    internal ExecutionContext CapturedContext
    {
      get
      {
        return (this.m_stateFlags & 536870912) == 536870912 ? (ExecutionContext) null : this.m_contingentProperties?.m_capturedContext ?? ExecutionContext.Default;
      }
      set
      {
        if (value == null)
        {
          this.m_stateFlags |= 536870912;
        }
        else
        {
          if (value == ExecutionContext.Default)
            return;
          this.EnsureContingentPropertiesInitializedUnsafe().m_capturedContext = value;
        }
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        if ((this.Options & (TaskCreationOptions) 16384) != TaskCreationOptions.None)
          return;
        if (!this.IsCompleted)
          ThrowHelper.ThrowInvalidOperationException(ExceptionResource.Task_Dispose_NotCompleted);
        Task.ContingentProperties contingentProperties = Volatile.Read<Task.ContingentProperties>(ref this.m_contingentProperties);
        if (contingentProperties != null)
        {
          ManualResetEventSlim completionEvent = contingentProperties.m_completionEvent;
          if (completionEvent != null)
          {
            contingentProperties.m_completionEvent = (ManualResetEventSlim) null;
            if (!completionEvent.IsSet)
              completionEvent.Set();
            completionEvent.Dispose();
          }
        }
      }
      this.m_stateFlags |= 262144;
    }

    internal void ScheduleAndStart(bool needsProtection)
    {
      if (needsProtection)
      {
        if (!this.MarkStarted())
          return;
      }
      else
        this.m_stateFlags |= 65536;
      if (Task.s_asyncDebuggingEnabled)
        Task.AddToActiveTasks(this);
      if (AsyncCausalityTracer.LoggingOn && (this.Options & (TaskCreationOptions) 512) == TaskCreationOptions.None)
        AsyncCausalityTracer.TraceOperationCreation(this, "Task: " + this.m_action.Method.Name);
      try
      {
        this.m_taskScheduler.InternalQueueTask(this);
      }
      catch (System.Exception ex)
      {
        TaskSchedulerException schedulerException = new TaskSchedulerException(ex);
        this.AddException((object) schedulerException);
        this.Finish(false);
        if ((this.Options & (TaskCreationOptions) 512) == TaskCreationOptions.None)
          this.m_contingentProperties.m_exceptionsHolder.MarkAsHandled(false);
        throw schedulerException;
      }
    }

    internal void AddException(object exceptionObject)
    {
      this.AddException(exceptionObject, false);
    }

    internal void AddException(object exceptionObject, bool representsCancellation)
    {
      Task.ContingentProperties contingentProperties = this.EnsureContingentPropertiesInitialized();
      if (contingentProperties.m_exceptionsHolder == null)
      {
        TaskExceptionHolder taskExceptionHolder = new TaskExceptionHolder(this);
        if (Interlocked.CompareExchange<TaskExceptionHolder>(ref contingentProperties.m_exceptionsHolder, taskExceptionHolder, (TaskExceptionHolder) null) != null)
          taskExceptionHolder.MarkAsHandled(false);
      }
      lock (contingentProperties)
        contingentProperties.m_exceptionsHolder.Add(exceptionObject, representsCancellation);
    }

    private AggregateException GetExceptions(bool includeTaskCanceledExceptions)
    {
      System.Exception includeThisException = (System.Exception) null;
      if (includeTaskCanceledExceptions && this.IsCanceled)
        includeThisException = (System.Exception) new TaskCanceledException(this);
      if (this.ExceptionRecorded)
        return this.m_contingentProperties.m_exceptionsHolder.CreateExceptionObject(false, includeThisException);
      if (includeThisException == null)
        return (AggregateException) null;
      return new AggregateException(new System.Exception[1]
      {
        includeThisException
      });
    }

    internal ReadOnlyCollection<ExceptionDispatchInfo> GetExceptionDispatchInfos()
    {
      return !this.IsFaulted || !this.ExceptionRecorded ? new ReadOnlyCollection<ExceptionDispatchInfo>((IList<ExceptionDispatchInfo>) new ExceptionDispatchInfo[0]) : this.m_contingentProperties.m_exceptionsHolder.GetExceptionDispatchInfos();
    }

    internal ExceptionDispatchInfo GetCancellationExceptionDispatchInfo()
    {
      return Volatile.Read<Task.ContingentProperties>(ref this.m_contingentProperties)?.m_exceptionsHolder?.GetCancellationExceptionDispatchInfo();
    }

    internal void ThrowIfExceptional(bool includeTaskCanceledExceptions)
    {
      System.Exception exceptions = (System.Exception) this.GetExceptions(includeTaskCanceledExceptions);
      if (exceptions != null)
      {
        this.UpdateExceptionObservedStatus();
        throw exceptions;
      }
    }

    internal static void ThrowAsync(System.Exception exception, SynchronizationContext targetContext)
    {
      ExceptionDispatchInfo exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
      if (targetContext != null)
      {
        try
        {
          targetContext.Post((SendOrPostCallback) (state => ((ExceptionDispatchInfo) state).Throw()), (object) exceptionDispatchInfo);
          return;
        }
        catch (System.Exception ex)
        {
          exceptionDispatchInfo = ExceptionDispatchInfo.Capture((System.Exception) new AggregateException(new System.Exception[2]
          {
            exception,
            ex
          }));
        }
      }
      if (WindowsRuntimeMarshal.ReportUnhandledError(exceptionDispatchInfo.SourceException))
        return;
      ThreadPool.QueueUserWorkItem((WaitCallback) (state => ((ExceptionDispatchInfo) state).Throw()), (object) exceptionDispatchInfo);
    }

    internal void UpdateExceptionObservedStatus()
    {
      Task parent = this.m_contingentProperties?.m_parent;
      if (parent == null || (this.Options & TaskCreationOptions.AttachedToParent) == TaskCreationOptions.None || ((parent.CreationOptions & TaskCreationOptions.DenyChildAttach) != TaskCreationOptions.None || Task.InternalCurrent != parent))
        return;
      this.m_stateFlags |= 524288;
    }

    internal bool IsExceptionObservedByParent
    {
      get
      {
        return (uint) (this.m_stateFlags & 524288) > 0U;
      }
    }

    internal bool IsDelegateInvoked
    {
      get
      {
        return (uint) (this.m_stateFlags & 131072) > 0U;
      }
    }

    internal void Finish(bool userDelegateExecute)
    {
      if (this.m_contingentProperties == null)
        this.FinishStageTwo();
      else
        this.FinishSlow(userDelegateExecute);
    }

    private void FinishSlow(bool userDelegateExecute)
    {
      if (!userDelegateExecute)
      {
        this.FinishStageTwo();
      }
      else
      {
        Task.ContingentProperties contingentProperties = this.m_contingentProperties;
        if (contingentProperties.m_completionCountdown == 1 || Interlocked.Decrement(ref contingentProperties.m_completionCountdown) == 0)
          this.FinishStageTwo();
        else
          this.AtomicStateUpdate(8388608, 23068672);
        List<Task> exceptionalChildren = contingentProperties.m_exceptionalChildren;
        if (exceptionalChildren == null)
          return;
        lock (exceptionalChildren)
          exceptionalChildren.RemoveAll((Predicate<Task>) (t => t.IsExceptionObservedByParent));
      }
    }

    private void FinishStageTwo()
    {
      Task.ContingentProperties props = Volatile.Read<Task.ContingentProperties>(ref this.m_contingentProperties);
      if (props != null)
        this.AddExceptionsFromChildren(props);
      int num;
      if (this.ExceptionRecorded)
      {
        num = 2097152;
        if (AsyncCausalityTracer.LoggingOn)
          AsyncCausalityTracer.TraceOperationCompletion(this, AsyncCausalityStatus.Error);
        if (Task.s_asyncDebuggingEnabled)
          Task.RemoveFromActiveTasks(this);
      }
      else if (this.IsCancellationRequested && this.IsCancellationAcknowledged)
      {
        num = 4194304;
        if (AsyncCausalityTracer.LoggingOn)
          AsyncCausalityTracer.TraceOperationCompletion(this, AsyncCausalityStatus.Canceled);
        if (Task.s_asyncDebuggingEnabled)
          Task.RemoveFromActiveTasks(this);
      }
      else
      {
        num = 16777216;
        if (AsyncCausalityTracer.LoggingOn)
          AsyncCausalityTracer.TraceOperationCompletion(this, AsyncCausalityStatus.Completed);
        if (Task.s_asyncDebuggingEnabled)
          Task.RemoveFromActiveTasks(this);
      }
      Interlocked.Exchange(ref this.m_stateFlags, this.m_stateFlags | num);
      Task.ContingentProperties contingentProperties = Volatile.Read<Task.ContingentProperties>(ref this.m_contingentProperties);
      if (contingentProperties != null)
      {
        contingentProperties.SetCompleted();
        contingentProperties.UnregisterCancellationCallback();
      }
      this.FinishStageThree();
    }

    internal void FinishStageThree()
    {
      this.m_action = (Delegate) null;
      Task.ContingentProperties contingentProperties = this.m_contingentProperties;
      if (contingentProperties != null)
      {
        contingentProperties.m_capturedContext = (ExecutionContext) null;
        this.NotifyParentIfPotentiallyAttachedTask();
      }
      this.FinishContinuations();
    }

    internal void NotifyParentIfPotentiallyAttachedTask()
    {
      Task parent = this.m_contingentProperties?.m_parent;
      if (parent == null || (parent.CreationOptions & TaskCreationOptions.DenyChildAttach) != TaskCreationOptions.None || (this.m_stateFlags & (int) ushort.MaxValue & 4) == 0)
        return;
      parent.ProcessChildCompletion(this);
    }

    internal void ProcessChildCompletion(Task childTask)
    {
      Task.ContingentProperties contingentProperties = Volatile.Read<Task.ContingentProperties>(ref this.m_contingentProperties);
      if (childTask.IsFaulted && !childTask.IsExceptionObservedByParent)
      {
        if (contingentProperties.m_exceptionalChildren == null)
          Interlocked.CompareExchange<List<Task>>(ref contingentProperties.m_exceptionalChildren, new List<Task>(), (List<Task>) null);
        List<Task> exceptionalChildren = contingentProperties.m_exceptionalChildren;
        if (exceptionalChildren != null)
        {
          lock (exceptionalChildren)
            exceptionalChildren.Add(childTask);
        }
      }
      if (Interlocked.Decrement(ref contingentProperties.m_completionCountdown) != 0)
        return;
      this.FinishStageTwo();
    }

    internal void AddExceptionsFromChildren(Task.ContingentProperties props)
    {
      List<Task> exceptionalChildren = props.m_exceptionalChildren;
      if (exceptionalChildren == null)
        return;
      lock (exceptionalChildren)
      {
        foreach (Task task in exceptionalChildren)
        {
          if (task.IsFaulted && !task.IsExceptionObservedByParent)
            this.AddException((object) Volatile.Read<Task.ContingentProperties>(ref task.m_contingentProperties).m_exceptionsHolder.CreateExceptionObject(false, (System.Exception) null));
        }
      }
      props.m_exceptionalChildren = (List<Task>) null;
    }

    internal bool ExecuteEntry()
    {
      int oldFlags = 0;
      if (!this.AtomicStateUpdate(131072, 23199744, ref oldFlags) && (oldFlags & 4194304) == 0)
        return false;
      if (!this.IsCancellationRequested & !this.IsCanceled)
        this.ExecuteWithThreadLocal(ref Task.t_currentTask, (Thread) null);
      else
        this.ExecuteEntryCancellationRequestedOrCanceled();
      return true;
    }

    internal virtual void ExecuteFromThreadPool(Thread threadPoolThread)
    {
      this.ExecuteEntryUnsafe(threadPoolThread);
    }

    internal void ExecuteEntryUnsafe(Thread threadPoolThread)
    {
      this.m_stateFlags |= 131072;
      if (!this.IsCancellationRequested & !this.IsCanceled)
        this.ExecuteWithThreadLocal(ref Task.t_currentTask, threadPoolThread);
      else
        this.ExecuteEntryCancellationRequestedOrCanceled();
    }

    internal void ExecuteEntryCancellationRequestedOrCanceled()
    {
      if (this.IsCanceled || (Interlocked.Exchange(ref this.m_stateFlags, this.m_stateFlags | 4194304) & 4194304) != 0)
        return;
      this.CancellationCleanupLogic();
    }

    private void ExecuteWithThreadLocal(ref Task currentTaskSlot, Thread threadPoolThread = null)
    {
      Task task = currentTaskSlot;
      TplEventSource log = TplEventSource.Log;
      Guid oldActivityThatWillContinue = new Guid();
      bool flag = log.IsEnabled();
      if (flag)
      {
        if (log.TasksSetActivityIds)
          EventSource.SetCurrentThreadActivityId(TplEventSource.CreateGuidForTaskID(this.Id), out oldActivityThatWillContinue);
        if (task != null)
          log.TaskStarted(task.m_taskScheduler.Id, task.Id, this.Id);
        else
          log.TaskStarted(TaskScheduler.Current.Id, 0, this.Id);
      }
      bool loggingOn = AsyncCausalityTracer.LoggingOn;
      if (loggingOn)
        AsyncCausalityTracer.TraceSynchronousWorkStart(this, CausalitySynchronousWork.Execution);
      try
      {
        currentTaskSlot = this;
        try
        {
          ExecutionContext capturedContext = this.CapturedContext;
          if (capturedContext == null)
            this.InnerInvoke();
          else if (threadPoolThread == null)
            ExecutionContext.RunInternal(capturedContext, Task.s_ecCallback, (object) this);
          else
            ExecutionContext.RunFromThreadPoolDispatchLoop(threadPoolThread, capturedContext, Task.s_ecCallback, (object) this);
        }
        catch (System.Exception ex)
        {
          this.HandleException(ex);
        }
        if (loggingOn)
          AsyncCausalityTracer.TraceSynchronousWorkCompletion(CausalitySynchronousWork.Execution);
        this.Finish(true);
      }
      finally
      {
        currentTaskSlot = task;
        if (flag)
        {
          if (task != null)
            log.TaskCompleted(task.m_taskScheduler.Id, task.Id, this.Id, this.IsFaulted);
          else
            log.TaskCompleted(TaskScheduler.Current.Id, 0, this.Id, this.IsFaulted);
          if (log.TasksSetActivityIds)
            EventSource.SetCurrentThreadActivityId(oldActivityThatWillContinue);
        }
      }
    }

    internal virtual void InnerInvoke()
    {
      if (this.m_action is Action action)
      {
        action();
      }
      else
      {
        if (!(this.m_action is Action<object> action))
          return;
        action(this.m_stateObject);
      }
    }

    private void HandleException(System.Exception unhandledException)
    {
      if (unhandledException is OperationCanceledException canceledException && this.IsCancellationRequested && this.m_contingentProperties.m_cancellationToken == canceledException.CancellationToken)
      {
        this.SetCancellationAcknowledged();
        this.AddException((object) canceledException, true);
      }
      else
        this.AddException((object) unhandledException);
    }

    public TaskAwaiter GetAwaiter()
    {
      return new TaskAwaiter(this);
    }

    public ConfiguredTaskAwaitable ConfigureAwait(
      bool continueOnCapturedContext)
    {
      return new ConfiguredTaskAwaitable(this, continueOnCapturedContext);
    }

    internal void SetContinuationForAwait(
      Action continuationAction,
      bool continueOnCapturedContext,
      bool flowExecutionContext)
    {
      TaskContinuation taskContinuation = (TaskContinuation) null;
      if (continueOnCapturedContext)
      {
        SynchronizationContext current = SynchronizationContext.Current;
        if (current != null && current.GetType() != typeof (SynchronizationContext))
        {
          taskContinuation = (TaskContinuation) new SynchronizationContextAwaitTaskContinuation(current, continuationAction, flowExecutionContext);
        }
        else
        {
          TaskScheduler internalCurrent = TaskScheduler.InternalCurrent;
          if (internalCurrent != null && internalCurrent != TaskScheduler.Default)
            taskContinuation = (TaskContinuation) new TaskSchedulerAwaitTaskContinuation(internalCurrent, continuationAction, flowExecutionContext);
        }
      }
      if (taskContinuation == null & flowExecutionContext)
        taskContinuation = (TaskContinuation) new AwaitTaskContinuation(continuationAction, true);
      if (taskContinuation != null)
      {
        if (this.AddTaskContinuation((object) taskContinuation, false))
          return;
        taskContinuation.Run(this, false);
      }
      else
      {
        if (this.AddTaskContinuation((object) continuationAction, false))
          return;
        AwaitTaskContinuation.UnsafeScheduleAction(continuationAction, this);
      }
    }

    internal void UnsafeSetContinuationForAwait(
      IAsyncStateMachineBox stateMachineBox,
      bool continueOnCapturedContext)
    {
      if (continueOnCapturedContext)
      {
        SynchronizationContext current = SynchronizationContext.Current;
        if (current != null && current.GetType() != typeof (SynchronizationContext))
        {
          SynchronizationContextAwaitTaskContinuation taskContinuation = new SynchronizationContextAwaitTaskContinuation(current, stateMachineBox.MoveNextAction, false);
          if (this.AddTaskContinuation((object) taskContinuation, false))
            return;
          taskContinuation.Run(this, false);
          return;
        }
        TaskScheduler internalCurrent = TaskScheduler.InternalCurrent;
        if (internalCurrent != null && internalCurrent != TaskScheduler.Default)
        {
          TaskSchedulerAwaitTaskContinuation taskContinuation = new TaskSchedulerAwaitTaskContinuation(internalCurrent, stateMachineBox.MoveNextAction, false);
          if (this.AddTaskContinuation((object) taskContinuation, false))
            return;
          taskContinuation.Run(this, false);
          return;
        }
      }
      if (this.AddTaskContinuation((object) stateMachineBox, false))
        return;
      ThreadPool.UnsafeQueueUserWorkItemInternal((object) stateMachineBox, true);
    }

    public static YieldAwaitable Yield()
    {
      return new YieldAwaitable();
    }

    public void Wait()
    {
      this.Wait(-1, new CancellationToken());
    }

    public bool Wait(TimeSpan timeout)
    {
      long totalMilliseconds = (long) timeout.TotalMilliseconds;
      if (totalMilliseconds < -1L || totalMilliseconds > (long) int.MaxValue)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.timeout);
      return this.Wait((int) totalMilliseconds, new CancellationToken());
    }

    public void Wait(CancellationToken cancellationToken)
    {
      this.Wait(-1, cancellationToken);
    }

    public bool Wait(int millisecondsTimeout)
    {
      return this.Wait(millisecondsTimeout, new CancellationToken());
    }

    public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
    {
      if (millisecondsTimeout < -1)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.millisecondsTimeout);
      if (!this.IsWaitNotificationEnabledOrNotRanToCompletion)
        return true;
      if (!this.InternalWait(millisecondsTimeout, cancellationToken))
        return false;
      if (this.IsWaitNotificationEnabledOrNotRanToCompletion)
      {
        this.NotifyDebuggerOfWaitCompletionIfNecessary();
        if (this.IsCanceled)
          cancellationToken.ThrowIfCancellationRequested();
        this.ThrowIfExceptional(true);
      }
      return true;
    }

    private bool WrappedTryRunInline()
    {
      if (this.m_taskScheduler == null)
        return false;
      try
      {
        return this.m_taskScheduler.TryRunInline(this, true);
      }
      catch (System.Exception ex)
      {
        throw new TaskSchedulerException(ex);
      }
    }

    [MethodImpl(MethodImplOptions.NoOptimization)]
    internal bool InternalWait(int millisecondsTimeout, CancellationToken cancellationToken)
    {
      return this.InternalWaitCore(millisecondsTimeout, cancellationToken);
    }

    private bool InternalWaitCore(int millisecondsTimeout, CancellationToken cancellationToken)
    {
      if (this.IsCompleted)
        return true;
      TplEventSource log = TplEventSource.Log;
      bool flag1 = log.IsEnabled();
      if (flag1)
      {
        Task internalCurrent = Task.InternalCurrent;
        log.TaskWaitBegin(internalCurrent != null ? internalCurrent.m_taskScheduler.Id : TaskScheduler.Default.Id, internalCurrent != null ? internalCurrent.Id : 0, this.Id, TplEventSource.TaskWaitBehavior.Synchronous, 0);
      }
      Debugger.NotifyOfCrossThreadDependency();
      bool flag2 = millisecondsTimeout == -1 && !cancellationToken.CanBeCanceled && (this.WrappedTryRunInline() && this.IsCompleted) || this.SpinThenBlockingWait(millisecondsTimeout, cancellationToken);
      if (flag1)
      {
        Task internalCurrent = Task.InternalCurrent;
        if (internalCurrent != null)
          log.TaskWaitEnd(internalCurrent.m_taskScheduler.Id, internalCurrent.Id, this.Id);
        else
          log.TaskWaitEnd(TaskScheduler.Default.Id, 0, this.Id);
        log.TaskWaitContinuationComplete(this.Id);
      }
      return flag2;
    }

    private bool SpinThenBlockingWait(int millisecondsTimeout, CancellationToken cancellationToken)
    {
      bool flag1 = millisecondsTimeout == -1;
      uint num1 = flag1 ? 0U : (uint) Environment.TickCount;
      bool flag2 = this.SpinWait(millisecondsTimeout);
      if (!flag2)
      {
        Task.SetOnInvokeMres setOnInvokeMres = new Task.SetOnInvokeMres();
        try
        {
          this.AddCompletionAction((ITaskCompletionAction) setOnInvokeMres, true);
          if (flag1)
          {
            flag2 = setOnInvokeMres.Wait(-1, cancellationToken);
          }
          else
          {
            uint num2 = (uint) Environment.TickCount - num1;
            if ((long) num2 < (long) millisecondsTimeout)
              flag2 = setOnInvokeMres.Wait((int) ((long) millisecondsTimeout - (long) num2), cancellationToken);
          }
        }
        finally
        {
          if (!this.IsCompleted)
            this.RemoveContinuation((object) setOnInvokeMres);
        }
      }
      return flag2;
    }

    private bool SpinWait(int millisecondsTimeout)
    {
      if (this.IsCompleted)
        return true;
      if (millisecondsTimeout == 0)
        return false;
      int countforSpinBeforeWait = SpinWait.SpinCountforSpinBeforeWait;
      SpinWait spinWait = new SpinWait();
      while (spinWait.Count < countforSpinBeforeWait)
      {
        spinWait.SpinOnce(-1);
        if (this.IsCompleted)
          return true;
      }
      return false;
    }

    internal bool InternalCancel(bool bCancelNonExecutingOnly)
    {
      bool flag1 = false;
      bool flag2 = false;
      TaskSchedulerException schedulerException = (TaskSchedulerException) null;
      if ((this.m_stateFlags & 65536) != 0)
      {
        TaskScheduler taskScheduler = this.m_taskScheduler;
        try
        {
          flag1 = taskScheduler != null && taskScheduler.TryDequeue(this);
        }
        catch (System.Exception ex)
        {
          schedulerException = new TaskSchedulerException(ex);
        }
        bool flag3 = taskScheduler != null && taskScheduler.RequiresAtomicStartTransition;
        if (!flag1 & bCancelNonExecutingOnly & flag3)
          flag2 = this.AtomicStateUpdate(4194304, 4325376);
      }
      if (!bCancelNonExecutingOnly | flag1 | flag2)
      {
        this.RecordInternalCancellationRequest();
        if (flag1)
          flag2 = this.AtomicStateUpdate(4194304, 4325376);
        else if (!flag2 && (this.m_stateFlags & 65536) == 0)
          flag2 = this.AtomicStateUpdate(4194304, 23265280);
        if (flag2)
          this.CancellationCleanupLogic();
      }
      if (schedulerException != null)
        throw schedulerException;
      return flag2;
    }

    internal void RecordInternalCancellationRequest()
    {
      this.EnsureContingentPropertiesInitialized().m_internalCancellationRequested = 1;
    }

    internal void RecordInternalCancellationRequest(CancellationToken tokenToRecord)
    {
      this.RecordInternalCancellationRequest();
      if (!(tokenToRecord != new CancellationToken()))
        return;
      this.m_contingentProperties.m_cancellationToken = tokenToRecord;
    }

    internal void RecordInternalCancellationRequest(
      CancellationToken tokenToRecord,
      object cancellationException)
    {
      this.RecordInternalCancellationRequest(tokenToRecord);
      if (cancellationException == null)
        return;
      this.AddException(cancellationException, true);
    }

    internal void CancellationCleanupLogic()
    {
      Interlocked.Exchange(ref this.m_stateFlags, this.m_stateFlags | 4194304);
      Task.ContingentProperties contingentProperties = Volatile.Read<Task.ContingentProperties>(ref this.m_contingentProperties);
      if (contingentProperties != null)
      {
        contingentProperties.SetCompleted();
        contingentProperties.UnregisterCancellationCallback();
      }
      if (AsyncCausalityTracer.LoggingOn)
        AsyncCausalityTracer.TraceOperationCompletion(this, AsyncCausalityStatus.Canceled);
      if (Task.s_asyncDebuggingEnabled)
        Task.RemoveFromActiveTasks(this);
      this.FinishStageThree();
    }

    private void SetCancellationAcknowledged()
    {
      this.m_stateFlags |= 1048576;
    }

    internal bool TrySetResult()
    {
      if (!this.AtomicStateUpdate(83886080, 90177536))
        return false;
      Task.ContingentProperties contingentProperties = this.m_contingentProperties;
      if (contingentProperties != null)
      {
        this.NotifyParentIfPotentiallyAttachedTask();
        contingentProperties.SetCompleted();
      }
      this.FinishContinuations();
      return true;
    }

    internal bool TrySetException(object exceptionObject)
    {
      bool flag = false;
      this.EnsureContingentPropertiesInitialized();
      if (this.AtomicStateUpdate(67108864, 90177536))
      {
        this.AddException(exceptionObject);
        this.Finish(false);
        flag = true;
      }
      return flag;
    }

    internal bool TrySetCanceled(CancellationToken tokenToRecord)
    {
      return this.TrySetCanceled(tokenToRecord, (object) null);
    }

    internal bool TrySetCanceled(CancellationToken tokenToRecord, object cancellationException)
    {
      bool flag = false;
      if (this.AtomicStateUpdate(67108864, 90177536))
      {
        this.RecordInternalCancellationRequest(tokenToRecord, cancellationException);
        this.CancellationCleanupLogic();
        flag = true;
      }
      return flag;
    }

    internal void FinishContinuations()
    {
      object continuationObject = Interlocked.Exchange(ref this.m_continuationObject, Task.s_taskCompletionSentinel);
      if (continuationObject == null)
        return;
      this.RunContinuations(continuationObject);
    }

    private void RunContinuations(object continuationObject)
    {
      TplEventSource tplEventSource = TplEventSource.Log;
      if (!tplEventSource.IsEnabled())
        tplEventSource = (TplEventSource) null;
      if (AsyncCausalityTracer.LoggingOn)
        AsyncCausalityTracer.TraceSynchronousWorkStart(this, CausalitySynchronousWork.CompletionNotification);
      bool flag1 = (this.m_stateFlags & 64) == 0 && RuntimeHelpers.TryEnsureSufficientExecutionStack();
      switch (continuationObject)
      {
        case IAsyncStateMachineBox box:
          AwaitTaskContinuation.RunOrScheduleAction(box, flag1);
          Task.LogFinishCompletionNotification();
          break;
        case Action action:
          AwaitTaskContinuation.RunOrScheduleAction(action, flag1);
          Task.LogFinishCompletionNotification();
          break;
        case TaskContinuation taskContinuation:
          taskContinuation.Run(this, flag1);
          Task.LogFinishCompletionNotification();
          break;
        case ITaskCompletionAction completionAction:
          this.RunOrQueueCompletionAction(completionAction, flag1);
          Task.LogFinishCompletionNotification();
          break;
        default:
          List<object> objectList = (List<object>) continuationObject;
          lock (objectList)
            ;
          int count = objectList.Count;
          if (flag1)
          {
            bool flag2 = false;
            for (int Index = 0; Index < count; ++Index)
            {
              object Object = objectList[Index];
              switch (Object)
              {
                case null:
                case ITaskCompletionAction _:
                  continue;
                case StandardTaskContinuation taskContinuation:
                  if ((taskContinuation.m_options & TaskContinuationOptions.ExecuteSynchronously) == TaskContinuationOptions.None)
                  {
                    objectList[Index] = (object) null;
                    tplEventSource?.RunningContinuationList(this.Id, Index, (object) taskContinuation);
                    taskContinuation.Run(this, false);
                    continue;
                  }
                  continue;
                default:
                  if (flag2)
                  {
                    objectList[Index] = (object) null;
                    tplEventSource?.RunningContinuationList(this.Id, Index, Object);
                    switch (Object)
                    {
                      case IAsyncStateMachineBox box:
                        AwaitTaskContinuation.RunOrScheduleAction(box, false);
                        break;
                      case Action action:
                        AwaitTaskContinuation.RunOrScheduleAction(action, false);
                        break;
                      default:
                        ((TaskContinuation) Object).Run(this, false);
                        break;
                    }
                  }
                  flag2 = true;
                  continue;
              }
            }
          }
          for (int Index = 0; Index < count; ++Index)
          {
            object Object = objectList[Index];
            if (Object != null)
            {
              objectList[Index] = (object) null;
              tplEventSource?.RunningContinuationList(this.Id, Index, Object);
              switch (Object)
              {
                case IAsyncStateMachineBox box:
                  AwaitTaskContinuation.RunOrScheduleAction(box, flag1);
                  continue;
                case Action action:
                  AwaitTaskContinuation.RunOrScheduleAction(action, flag1);
                  continue;
                case TaskContinuation taskContinuation:
                  taskContinuation.Run(this, flag1);
                  continue;
                default:
                  this.RunOrQueueCompletionAction((ITaskCompletionAction) Object, flag1);
                  continue;
              }
            }
          }
          Task.LogFinishCompletionNotification();
          break;
      }
    }

    private void RunOrQueueCompletionAction(
      ITaskCompletionAction completionAction,
      bool allowInlining)
    {
      if (allowInlining || !completionAction.InvokeMayRunArbitraryCode)
        completionAction.Invoke(this);
      else
        ThreadPool.UnsafeQueueUserWorkItemInternal((object) new CompletionActionInvoker(completionAction, this), true);
    }

    private static void LogFinishCompletionNotification()
    {
      if (!AsyncCausalityTracer.LoggingOn)
        return;
      AsyncCausalityTracer.TraceSynchronousWorkCompletion(CausalitySynchronousWork.CompletionNotification);
    }

    public Task ContinueWith(Action<Task> continuationAction)
    {
      return this.ContinueWith(continuationAction, TaskScheduler.Current, new CancellationToken(), TaskContinuationOptions.None);
    }

    public Task ContinueWith(
      Action<Task> continuationAction,
      CancellationToken cancellationToken)
    {
      return this.ContinueWith(continuationAction, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None);
    }

    public Task ContinueWith(Action<Task> continuationAction, TaskScheduler scheduler)
    {
      return this.ContinueWith(continuationAction, scheduler, new CancellationToken(), TaskContinuationOptions.None);
    }

    public Task ContinueWith(
      Action<Task> continuationAction,
      TaskContinuationOptions continuationOptions)
    {
      return this.ContinueWith(continuationAction, TaskScheduler.Current, new CancellationToken(), continuationOptions);
    }

    public Task ContinueWith(
      Action<Task> continuationAction,
      CancellationToken cancellationToken,
      TaskContinuationOptions continuationOptions,
      TaskScheduler scheduler)
    {
      return this.ContinueWith(continuationAction, scheduler, cancellationToken, continuationOptions);
    }

    private Task ContinueWith(
      Action<Task> continuationAction,
      TaskScheduler scheduler,
      CancellationToken cancellationToken,
      TaskContinuationOptions continuationOptions)
    {
      if (continuationAction == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationAction);
      if (scheduler == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.scheduler);
      TaskCreationOptions creationOptions;
      InternalTaskOptions internalOptions;
      Task.CreationOptionsFromContinuationOptions(continuationOptions, out creationOptions, out internalOptions);
      Task continuationTask = (Task) new ContinuationTaskFromTask(this, (Delegate) continuationAction, (object) null, creationOptions, internalOptions);
      this.ContinueWithCore(continuationTask, scheduler, cancellationToken, continuationOptions);
      return continuationTask;
    }

    public Task ContinueWith([Nullable(new byte[] {1, 1, 2})] Action<Task, object> continuationAction, [Nullable(2)] object state)
    {
      return this.ContinueWith(continuationAction, state, TaskScheduler.Current, new CancellationToken(), TaskContinuationOptions.None);
    }

    public Task ContinueWith(
      [Nullable(new byte[] {1, 1, 2})] Action<Task, object> continuationAction,
      [Nullable(2)] object state,
      CancellationToken cancellationToken)
    {
      return this.ContinueWith(continuationAction, state, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None);
    }

    public Task ContinueWith(
      [Nullable(new byte[] {1, 1, 2})] Action<Task, object> continuationAction,
      [Nullable(2)] object state,
      TaskScheduler scheduler)
    {
      return this.ContinueWith(continuationAction, state, scheduler, new CancellationToken(), TaskContinuationOptions.None);
    }

    public Task ContinueWith(
      [Nullable(new byte[] {1, 1, 2})] Action<Task, object> continuationAction,
      [Nullable(2)] object state,
      TaskContinuationOptions continuationOptions)
    {
      return this.ContinueWith(continuationAction, state, TaskScheduler.Current, new CancellationToken(), continuationOptions);
    }

    public Task ContinueWith(
      [Nullable(new byte[] {1, 1, 2})] Action<Task, object> continuationAction,
      [Nullable(2)] object state,
      CancellationToken cancellationToken,
      TaskContinuationOptions continuationOptions,
      TaskScheduler scheduler)
    {
      return this.ContinueWith(continuationAction, state, scheduler, cancellationToken, continuationOptions);
    }

    private Task ContinueWith(
      Action<Task, object> continuationAction,
      object state,
      TaskScheduler scheduler,
      CancellationToken cancellationToken,
      TaskContinuationOptions continuationOptions)
    {
      if (continuationAction == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationAction);
      if (scheduler == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.scheduler);
      TaskCreationOptions creationOptions;
      InternalTaskOptions internalOptions;
      Task.CreationOptionsFromContinuationOptions(continuationOptions, out creationOptions, out internalOptions);
      Task continuationTask = (Task) new ContinuationTaskFromTask(this, (Delegate) continuationAction, state, creationOptions, internalOptions);
      this.ContinueWithCore(continuationTask, scheduler, cancellationToken, continuationOptions);
      return continuationTask;
    }

    public Task<TResult> ContinueWith<[Nullable(2)] TResult>(
      Func<Task, TResult> continuationFunction)
    {
      return this.ContinueWith<TResult>(continuationFunction, TaskScheduler.Current, new CancellationToken(), TaskContinuationOptions.None);
    }

    public Task<TResult> ContinueWith<[Nullable(2)] TResult>(
      Func<Task, TResult> continuationFunction,
      CancellationToken cancellationToken)
    {
      return this.ContinueWith<TResult>(continuationFunction, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None);
    }

    public Task<TResult> ContinueWith<[Nullable(2)] TResult>(
      Func<Task, TResult> continuationFunction,
      TaskScheduler scheduler)
    {
      return this.ContinueWith<TResult>(continuationFunction, scheduler, new CancellationToken(), TaskContinuationOptions.None);
    }

    public Task<TResult> ContinueWith<[Nullable(2)] TResult>(
      Func<Task, TResult> continuationFunction,
      TaskContinuationOptions continuationOptions)
    {
      return this.ContinueWith<TResult>(continuationFunction, TaskScheduler.Current, new CancellationToken(), continuationOptions);
    }

    public Task<TResult> ContinueWith<[Nullable(2)] TResult>(
      Func<Task, TResult> continuationFunction,
      CancellationToken cancellationToken,
      TaskContinuationOptions continuationOptions,
      TaskScheduler scheduler)
    {
      return this.ContinueWith<TResult>(continuationFunction, scheduler, cancellationToken, continuationOptions);
    }

    private Task<TResult> ContinueWith<TResult>(
      Func<Task, TResult> continuationFunction,
      TaskScheduler scheduler,
      CancellationToken cancellationToken,
      TaskContinuationOptions continuationOptions)
    {
      if (continuationFunction == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);
      if (scheduler == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.scheduler);
      TaskCreationOptions creationOptions;
      InternalTaskOptions internalOptions;
      Task.CreationOptionsFromContinuationOptions(continuationOptions, out creationOptions, out internalOptions);
      Task<TResult> task = (Task<TResult>) new ContinuationResultTaskFromTask<TResult>(this, (Delegate) continuationFunction, (object) null, creationOptions, internalOptions);
      this.ContinueWithCore((Task) task, scheduler, cancellationToken, continuationOptions);
      return task;
    }

    [NullableContext(2)]
    [return: Nullable(1)]
    public Task<TResult> ContinueWith<TResult>(
      [Nullable(new byte[] {1, 1, 2, 1})] Func<Task, object, TResult> continuationFunction,
      object state)
    {
      return this.ContinueWith<TResult>(continuationFunction, state, TaskScheduler.Current, new CancellationToken(), TaskContinuationOptions.None);
    }

    [NullableContext(2)]
    [return: Nullable(1)]
    public Task<TResult> ContinueWith<TResult>(
      [Nullable(new byte[] {1, 1, 2, 1})] Func<Task, object, TResult> continuationFunction,
      object state,
      CancellationToken cancellationToken)
    {
      return this.ContinueWith<TResult>(continuationFunction, state, TaskScheduler.Current, cancellationToken, TaskContinuationOptions.None);
    }

    public Task<TResult> ContinueWith<[Nullable(2)] TResult>(
      [Nullable(new byte[] {1, 1, 2, 1})] Func<Task, object, TResult> continuationFunction,
      [Nullable(2)] object state,
      TaskScheduler scheduler)
    {
      return this.ContinueWith<TResult>(continuationFunction, state, scheduler, new CancellationToken(), TaskContinuationOptions.None);
    }

    [NullableContext(2)]
    [return: Nullable(1)]
    public Task<TResult> ContinueWith<TResult>(
      [Nullable(new byte[] {1, 1, 2, 1})] Func<Task, object, TResult> continuationFunction,
      object state,
      TaskContinuationOptions continuationOptions)
    {
      return this.ContinueWith<TResult>(continuationFunction, state, TaskScheduler.Current, new CancellationToken(), continuationOptions);
    }

    public Task<TResult> ContinueWith<[Nullable(2)] TResult>(
      [Nullable(new byte[] {1, 1, 2, 1})] Func<Task, object, TResult> continuationFunction,
      [Nullable(2)] object state,
      CancellationToken cancellationToken,
      TaskContinuationOptions continuationOptions,
      TaskScheduler scheduler)
    {
      return this.ContinueWith<TResult>(continuationFunction, state, scheduler, cancellationToken, continuationOptions);
    }

    private Task<TResult> ContinueWith<TResult>(
      Func<Task, object, TResult> continuationFunction,
      object state,
      TaskScheduler scheduler,
      CancellationToken cancellationToken,
      TaskContinuationOptions continuationOptions)
    {
      if (continuationFunction == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.continuationFunction);
      if (scheduler == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.scheduler);
      TaskCreationOptions creationOptions;
      InternalTaskOptions internalOptions;
      Task.CreationOptionsFromContinuationOptions(continuationOptions, out creationOptions, out internalOptions);
      Task<TResult> task = (Task<TResult>) new ContinuationResultTaskFromTask<TResult>(this, (Delegate) continuationFunction, state, creationOptions, internalOptions);
      this.ContinueWithCore((Task) task, scheduler, cancellationToken, continuationOptions);
      return task;
    }

    internal static void CreationOptionsFromContinuationOptions(
      TaskContinuationOptions continuationOptions,
      out TaskCreationOptions creationOptions,
      out InternalTaskOptions internalOptions)
    {
      if ((continuationOptions & (TaskContinuationOptions.LongRunning | TaskContinuationOptions.ExecuteSynchronously)) == (TaskContinuationOptions.LongRunning | TaskContinuationOptions.ExecuteSynchronously))
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.continuationOptions, ExceptionResource.Task_ContinueWith_ESandLR);
      if ((continuationOptions & ~(TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.PreferFairness | TaskContinuationOptions.LongRunning | TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.HideScheduler | TaskContinuationOptions.LazyCancellation | TaskContinuationOptions.RunContinuationsAsynchronously | TaskContinuationOptions.NotOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously)) != TaskContinuationOptions.None)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.continuationOptions);
      if ((continuationOptions & (TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.NotOnRanToCompletion)) == (TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.NotOnRanToCompletion))
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.continuationOptions, ExceptionResource.Task_ContinueWith_NotOnAnything);
      creationOptions = (TaskCreationOptions) (continuationOptions & (TaskContinuationOptions.PreferFairness | TaskContinuationOptions.LongRunning | TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.HideScheduler | TaskContinuationOptions.RunContinuationsAsynchronously));
      internalOptions = (continuationOptions & TaskContinuationOptions.LazyCancellation) != TaskContinuationOptions.None ? InternalTaskOptions.ContinuationTask | InternalTaskOptions.LazyCancellation : InternalTaskOptions.ContinuationTask;
    }

    internal void ContinueWithCore(
      Task continuationTask,
      TaskScheduler scheduler,
      CancellationToken cancellationToken,
      TaskContinuationOptions options)
    {
      TaskContinuation continuation = (TaskContinuation) new StandardTaskContinuation(continuationTask, options, scheduler);
      if (cancellationToken.CanBeCanceled)
      {
        if (this.IsCompleted || cancellationToken.IsCancellationRequested)
          continuationTask.AssignCancellationToken(cancellationToken, (Task) null, (TaskContinuation) null);
        else
          continuationTask.AssignCancellationToken(cancellationToken, this, continuation);
      }
      if (continuationTask.IsCompleted)
        return;
      if ((this.Options & (TaskCreationOptions) 1024) != TaskCreationOptions.None && !(this is ITaskCompletionAction))
      {
        TplEventSource log = TplEventSource.Log;
        if (log.IsEnabled())
          log.AwaitTaskContinuationScheduled(TaskScheduler.Current.Id, Task.CurrentId.GetValueOrDefault(), continuationTask.Id);
      }
      if (this.AddTaskContinuation((object) continuation, false))
        return;
      continuation.Run(this, true);
    }

    internal void AddCompletionAction(ITaskCompletionAction action)
    {
      this.AddCompletionAction(action, false);
    }

    internal void AddCompletionAction(ITaskCompletionAction action, bool addBeforeOthers)
    {
      if (this.AddTaskContinuation((object) action, addBeforeOthers))
        return;
      action.Invoke(this);
    }

    private bool AddTaskContinuationComplex(object tc, bool addBeforeOthers)
    {
      object continuationObject1 = this.m_continuationObject;
      if (continuationObject1 != Task.s_taskCompletionSentinel && !(continuationObject1 is List<object>))
        Interlocked.CompareExchange(ref this.m_continuationObject, (object) new List<object>()
        {
          continuationObject1
        }, continuationObject1);
      if (this.m_continuationObject is List<object> continuationObject2)
      {
        lock (continuationObject2)
        {
          if (this.m_continuationObject != Task.s_taskCompletionSentinel)
          {
            if (continuationObject2.Count == continuationObject2.Capacity)
              continuationObject2.RemoveAll((Predicate<object>) (l => l == null));
            if (addBeforeOthers)
              continuationObject2.Insert(0, tc);
            else
              continuationObject2.Add(tc);
            return true;
          }
        }
      }
      return false;
    }

    private bool AddTaskContinuation(object tc, bool addBeforeOthers)
    {
      if (this.IsCompleted)
        return false;
      return this.m_continuationObject == null && Interlocked.CompareExchange(ref this.m_continuationObject, tc, (object) null) == null || this.AddTaskContinuationComplex(tc, addBeforeOthers);
    }

    internal void RemoveContinuation(object continuationObject)
    {
      object continuationObject1 = this.m_continuationObject;
      if (continuationObject1 == Task.s_taskCompletionSentinel)
        return;
      if (!(continuationObject1 is List<object> objectList))
      {
        if (Interlocked.CompareExchange(ref this.m_continuationObject, (object) new List<object>(), continuationObject) == continuationObject)
          return;
        objectList = this.m_continuationObject as List<object>;
      }
      if (objectList == null)
        return;
      lock (objectList)
      {
        if (this.m_continuationObject == Task.s_taskCompletionSentinel)
          return;
        int index = objectList.IndexOf(continuationObject);
        if (index == -1)
          return;
        objectList[index] = (object) null;
      }
    }

    [MethodImpl(MethodImplOptions.NoOptimization)]
    public static void WaitAll(params Task[] tasks)
    {
      Task.WaitAllCore(tasks, -1, new CancellationToken());
    }

    [MethodImpl(MethodImplOptions.NoOptimization)]
    public static bool WaitAll(Task[] tasks, TimeSpan timeout)
    {
      long totalMilliseconds = (long) timeout.TotalMilliseconds;
      if (totalMilliseconds < -1L || totalMilliseconds > (long) int.MaxValue)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.timeout);
      return Task.WaitAllCore(tasks, (int) totalMilliseconds, new CancellationToken());
    }

    [MethodImpl(MethodImplOptions.NoOptimization)]
    public static bool WaitAll(Task[] tasks, int millisecondsTimeout)
    {
      return Task.WaitAllCore(tasks, millisecondsTimeout, new CancellationToken());
    }

    [MethodImpl(MethodImplOptions.NoOptimization)]
    public static void WaitAll(Task[] tasks, CancellationToken cancellationToken)
    {
      Task.WaitAllCore(tasks, -1, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.NoOptimization)]
    public static bool WaitAll(
      Task[] tasks,
      int millisecondsTimeout,
      CancellationToken cancellationToken)
    {
      return Task.WaitAllCore(tasks, millisecondsTimeout, cancellationToken);
    }

    private static bool WaitAllCore(
      Task[] tasks,
      int millisecondsTimeout,
      CancellationToken cancellationToken)
    {
      if (tasks == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.tasks);
      if (millisecondsTimeout < -1)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.millisecondsTimeout);
      cancellationToken.ThrowIfCancellationRequested();
      List<System.Exception> exceptions = (List<System.Exception>) null;
      List<Task> list1 = (List<Task>) null;
      List<Task> list2 = (List<Task>) null;
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = true;
      for (int index = tasks.Length - 1; index >= 0; --index)
      {
        Task task = tasks[index];
        if (task == null)
          ThrowHelper.ThrowArgumentException(ExceptionResource.Task_WaitMulti_NullTask, ExceptionArgument.tasks);
        bool flag4 = task.IsCompleted;
        if (!flag4)
        {
          if (millisecondsTimeout != -1 || cancellationToken.CanBeCanceled)
          {
            Task.AddToList<Task>(task, ref list1, tasks.Length);
          }
          else
          {
            flag4 = task.WrappedTryRunInline() && task.IsCompleted;
            if (!flag4)
              Task.AddToList<Task>(task, ref list1, tasks.Length);
          }
        }
        if (flag4)
        {
          if (task.IsFaulted)
            flag1 = true;
          else if (task.IsCanceled)
            flag2 = true;
          if (task.IsWaitNotificationEnabled)
            Task.AddToList<Task>(task, ref list2, 1);
        }
      }
      if (list1 != null)
      {
        flag3 = Task.WaitAllBlockingCore(list1, millisecondsTimeout, cancellationToken);
        if (flag3)
        {
          foreach (Task task in list1)
          {
            if (task.IsFaulted)
              flag1 = true;
            else if (task.IsCanceled)
              flag2 = true;
            if (task.IsWaitNotificationEnabled)
              Task.AddToList<Task>(task, ref list2, 1);
          }
        }
        GC.KeepAlive((object) tasks);
      }
      if (flag3 && list2 != null)
      {
        foreach (Task task in list2)
        {
          if (task.NotifyDebuggerOfWaitCompletionIfNecessary())
            break;
        }
      }
      if (flag3 && flag1 | flag2)
      {
        if (!flag1)
          cancellationToken.ThrowIfCancellationRequested();
        foreach (Task task in tasks)
          Task.AddExceptionsForCompletedTask(ref exceptions, task);
        ThrowHelper.ThrowAggregateException(exceptions);
      }
      return flag3;
    }

    private static void AddToList<T>(T item, ref List<T> list, int initSize)
    {
      if (list == null)
        list = new List<T>(initSize);
      list.Add(item);
    }

    private static bool WaitAllBlockingCore(
      List<Task> tasks,
      int millisecondsTimeout,
      CancellationToken cancellationToken)
    {
      bool flag = false;
      Task.SetOnCountdownMres setOnCountdownMres = new Task.SetOnCountdownMres(tasks.Count);
      try
      {
        foreach (Task task in tasks)
          task.AddCompletionAction((ITaskCompletionAction) setOnCountdownMres, true);
        flag = setOnCountdownMres.Wait(millisecondsTimeout, cancellationToken);
        return flag;
      }
      finally
      {
        if (!flag)
        {
          foreach (Task task in tasks)
          {
            if (!task.IsCompleted)
              task.RemoveContinuation((object) setOnCountdownMres);
          }
        }
      }
    }

    internal static void AddExceptionsForCompletedTask(ref List<System.Exception> exceptions, Task t)
    {
      AggregateException exceptions1 = t.GetExceptions(true);
      if (exceptions1 == null)
        return;
      t.UpdateExceptionObservedStatus();
      if (exceptions == null)
        exceptions = new List<System.Exception>(exceptions1.InnerExceptions.Count);
      exceptions.AddRange((IEnumerable<System.Exception>) exceptions1.InnerExceptions);
    }

    [MethodImpl(MethodImplOptions.NoOptimization)]
    public static int WaitAny(params Task[] tasks)
    {
      return Task.WaitAnyCore(tasks, -1, new CancellationToken());
    }

    [MethodImpl(MethodImplOptions.NoOptimization)]
    public static int WaitAny(Task[] tasks, TimeSpan timeout)
    {
      long totalMilliseconds = (long) timeout.TotalMilliseconds;
      if (totalMilliseconds < -1L || totalMilliseconds > (long) int.MaxValue)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.timeout);
      return Task.WaitAnyCore(tasks, (int) totalMilliseconds, new CancellationToken());
    }

    [MethodImpl(MethodImplOptions.NoOptimization)]
    public static int WaitAny(Task[] tasks, CancellationToken cancellationToken)
    {
      return Task.WaitAnyCore(tasks, -1, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.NoOptimization)]
    public static int WaitAny(Task[] tasks, int millisecondsTimeout)
    {
      return Task.WaitAnyCore(tasks, millisecondsTimeout, new CancellationToken());
    }

    [MethodImpl(MethodImplOptions.NoOptimization)]
    public static int WaitAny(
      Task[] tasks,
      int millisecondsTimeout,
      CancellationToken cancellationToken)
    {
      return Task.WaitAnyCore(tasks, millisecondsTimeout, cancellationToken);
    }

    private static int WaitAnyCore(
      Task[] tasks,
      int millisecondsTimeout,
      CancellationToken cancellationToken)
    {
      if (tasks == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.tasks);
      if (millisecondsTimeout < -1)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.millisecondsTimeout);
      cancellationToken.ThrowIfCancellationRequested();
      int num = -1;
      for (int index = 0; index < tasks.Length; ++index)
      {
        Task task = tasks[index];
        if (task == null)
          ThrowHelper.ThrowArgumentException(ExceptionResource.Task_WaitMulti_NullTask, ExceptionArgument.tasks);
        if (num == -1 && task.IsCompleted)
          num = index;
      }
      if (num == -1 && tasks.Length != 0)
      {
        Task<Task> continuation = TaskFactory.CommonCWAnyLogic((IList<Task>) tasks, true);
        if (continuation.Wait(millisecondsTimeout, cancellationToken))
          num = Array.IndexOf<Task>(tasks, continuation.Result);
        else
          TaskFactory.CommonCWAnyLogicCleanup(continuation);
      }
      GC.KeepAlive((object) tasks);
      return num;
    }

    public static Task<TResult> FromResult<[Nullable(2)] TResult>(TResult result)
    {
      return new Task<TResult>(result);
    }

    public static Task FromException(System.Exception exception)
    {
      if (exception == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.exception);
      Task task = new Task();
      task.TrySetException((object) exception);
      return task;
    }

    public static Task<TResult> FromException<[Nullable(2)] TResult>(System.Exception exception)
    {
      if (exception == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.exception);
      Task<TResult> task = new Task<TResult>();
      task.TrySetException((object) exception);
      return task;
    }

    public static Task FromCanceled(CancellationToken cancellationToken)
    {
      if (!cancellationToken.IsCancellationRequested)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.cancellationToken);
      return new Task(true, TaskCreationOptions.None, cancellationToken);
    }

    public static Task<TResult> FromCanceled<[Nullable(2)] TResult>(
      CancellationToken cancellationToken)
    {
      if (!cancellationToken.IsCancellationRequested)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.cancellationToken);
      return new Task<TResult>(true, default (TResult), TaskCreationOptions.None, cancellationToken);
    }

    internal static Task FromCanceled(OperationCanceledException exception)
    {
      Task task = new Task();
      task.TrySetCanceled(exception.CancellationToken, (object) exception);
      return task;
    }

    internal static Task<TResult> FromCanceled<TResult>(OperationCanceledException exception)
    {
      Task<TResult> task = new Task<TResult>();
      task.TrySetCanceled(exception.CancellationToken, (object) exception);
      return task;
    }

    public static Task Run(Action action)
    {
      return Task.InternalStartNew((Task) null, (Delegate) action, (object) null, new CancellationToken(), TaskScheduler.Default, TaskCreationOptions.DenyChildAttach, InternalTaskOptions.None);
    }

    public static Task Run(Action action, CancellationToken cancellationToken)
    {
      return Task.InternalStartNew((Task) null, (Delegate) action, (object) null, cancellationToken, TaskScheduler.Default, TaskCreationOptions.DenyChildAttach, InternalTaskOptions.None);
    }

    public static Task<TResult> Run<[Nullable(2)] TResult>(Func<TResult> function)
    {
      return Task<TResult>.StartNew((Task) null, function, new CancellationToken(), TaskCreationOptions.DenyChildAttach, InternalTaskOptions.None, TaskScheduler.Default);
    }

    public static Task<TResult> Run<[Nullable(2)] TResult>(
      Func<TResult> function,
      CancellationToken cancellationToken)
    {
      return Task<TResult>.StartNew((Task) null, function, cancellationToken, TaskCreationOptions.DenyChildAttach, InternalTaskOptions.None, TaskScheduler.Default);
    }

    public static Task Run([Nullable(new byte[] {1, 2})] Func<Task> function)
    {
      return Task.Run(function, new CancellationToken());
    }

    public static Task Run([Nullable(new byte[] {1, 2})] Func<Task> function, CancellationToken cancellationToken)
    {
      if (function == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.function);
      return cancellationToken.IsCancellationRequested ? Task.FromCanceled(cancellationToken) : (Task) new UnwrapPromise<VoidTaskResult>((Task) Task<Task>.Factory.StartNew(function, cancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default), true);
    }

    public static Task<TResult> Run<[Nullable(2)] TResult>([Nullable(new byte[] {1, 2, 1})] Func<Task<TResult>> function)
    {
      return Task.Run<TResult>(function, new CancellationToken());
    }

    public static Task<TResult> Run<[Nullable(2)] TResult>(
      [Nullable(new byte[] {1, 2, 1})] Func<Task<TResult>> function,
      CancellationToken cancellationToken)
    {
      if (function == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.function);
      return cancellationToken.IsCancellationRequested ? Task.FromCanceled<TResult>(cancellationToken) : (Task<TResult>) new UnwrapPromise<TResult>((Task) Task<Task<TResult>>.Factory.StartNew(function, cancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default), true);
    }

    public static Task Delay(TimeSpan delay)
    {
      return Task.Delay(delay, new CancellationToken());
    }

    public static Task Delay(TimeSpan delay, CancellationToken cancellationToken)
    {
      long totalMilliseconds = (long) delay.TotalMilliseconds;
      if (totalMilliseconds < -1L || totalMilliseconds > (long) int.MaxValue)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.delay, ExceptionResource.Task_Delay_InvalidDelay);
      return Task.Delay((int) totalMilliseconds, cancellationToken);
    }

    public static Task Delay(int millisecondsDelay)
    {
      return Task.Delay(millisecondsDelay, new CancellationToken());
    }

    public static Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
    {
      if (millisecondsDelay < -1)
        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.millisecondsDelay, ExceptionResource.Task_Delay_InvalidMillisecondsDelay);
      if (cancellationToken.IsCancellationRequested)
        return Task.FromCanceled(cancellationToken);
      if (millisecondsDelay == 0)
        return Task.CompletedTask;
      return !cancellationToken.CanBeCanceled ? (Task) new Task.DelayPromise(millisecondsDelay) : (Task) new Task.DelayPromiseWithCancellation(millisecondsDelay, cancellationToken);
    }

    public static Task WhenAll(IEnumerable<Task> tasks)
    {
      switch (tasks)
      {
        case Task[] taskArray:
          return Task.WhenAll(taskArray);
        case ICollection<Task> tasks2:
          int num = 0;
          Task[] tasks1 = new Task[tasks2.Count];
          foreach (Task task in tasks)
          {
            if (task == null)
              ThrowHelper.ThrowArgumentException(ExceptionResource.Task_MultiTaskContinuation_NullTask, ExceptionArgument.tasks);
            tasks1[num++] = task;
          }
          return Task.InternalWhenAll(tasks1);
        case null:
          ThrowHelper.ThrowArgumentNullException(ExceptionArgument.tasks);
          break;
      }
      List<Task> taskList = new List<Task>();
      foreach (Task task in tasks)
      {
        if (task == null)
          ThrowHelper.ThrowArgumentException(ExceptionResource.Task_MultiTaskContinuation_NullTask, ExceptionArgument.tasks);
        taskList.Add(task);
      }
      return Task.InternalWhenAll(taskList.ToArray());
    }

    public static Task WhenAll(params Task[] tasks)
    {
      if (tasks == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.tasks);
      int length = tasks.Length;
      if (length == 0)
        return Task.InternalWhenAll(tasks);
      Task[] tasks1 = new Task[length];
      for (int index = 0; index < length; ++index)
      {
        Task task = tasks[index];
        if (task == null)
          ThrowHelper.ThrowArgumentException(ExceptionResource.Task_MultiTaskContinuation_NullTask, ExceptionArgument.tasks);
        tasks1[index] = task;
      }
      return Task.InternalWhenAll(tasks1);
    }

    private static Task InternalWhenAll(Task[] tasks)
    {
      return tasks.Length != 0 ? (Task) new Task.WhenAllPromise(tasks) : Task.CompletedTask;
    }

    public static Task<TResult[]> WhenAll<[Nullable(2)] TResult>(
      IEnumerable<Task<TResult>> tasks)
    {
      switch (tasks)
      {
        case Task<TResult>[] taskArray:
          return Task.WhenAll<TResult>(taskArray);
        case ICollection<Task<TResult>> tasks2:
          int num = 0;
          Task<TResult>[] tasks1 = new Task<TResult>[tasks2.Count];
          foreach (Task<TResult> task in tasks)
          {
            if (task == null)
              ThrowHelper.ThrowArgumentException(ExceptionResource.Task_MultiTaskContinuation_NullTask, ExceptionArgument.tasks);
            tasks1[num++] = task;
          }
          return Task.InternalWhenAll<TResult>(tasks1);
        case null:
          ThrowHelper.ThrowArgumentNullException(ExceptionArgument.tasks);
          break;
      }
      List<Task<TResult>> taskList = new List<Task<TResult>>();
      foreach (Task<TResult> task in tasks)
      {
        if (task == null)
          ThrowHelper.ThrowArgumentException(ExceptionResource.Task_MultiTaskContinuation_NullTask, ExceptionArgument.tasks);
        taskList.Add(task);
      }
      return Task.InternalWhenAll<TResult>(taskList.ToArray());
    }

    public static Task<TResult[]> WhenAll<[Nullable(2)] TResult>(
      params Task<TResult>[] tasks)
    {
      if (tasks == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.tasks);
      int length = tasks.Length;
      if (length == 0)
        return Task.InternalWhenAll<TResult>(tasks);
      Task<TResult>[] tasks1 = new Task<TResult>[length];
      for (int index = 0; index < length; ++index)
      {
        Task<TResult> task = tasks[index];
        if (task == null)
          ThrowHelper.ThrowArgumentException(ExceptionResource.Task_MultiTaskContinuation_NullTask, ExceptionArgument.tasks);
        tasks1[index] = task;
      }
      return Task.InternalWhenAll<TResult>(tasks1);
    }

    private static Task<TResult[]> InternalWhenAll<TResult>(Task<TResult>[] tasks)
    {
      return tasks.Length != 0 ? (Task<TResult[]>) new Task.WhenAllPromise<TResult>(tasks) : new Task<TResult[]>(false, new TResult[0], TaskCreationOptions.None, new CancellationToken());
    }

    public static Task<Task> WhenAny(params Task[] tasks)
    {
      if (tasks == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.tasks);
      if (tasks.Length == 0)
        ThrowHelper.ThrowArgumentException(ExceptionResource.Task_MultiTaskContinuation_EmptyTaskList, ExceptionArgument.tasks);
      int length = tasks.Length;
      Task[] taskArray = new Task[length];
      for (int index = 0; index < length; ++index)
      {
        Task task = tasks[index];
        if (task == null)
          ThrowHelper.ThrowArgumentException(ExceptionResource.Task_MultiTaskContinuation_NullTask, ExceptionArgument.tasks);
        taskArray[index] = task;
      }
      return TaskFactory.CommonCWAnyLogic((IList<Task>) taskArray, false);
    }

    public static Task<Task> WhenAny(IEnumerable<Task> tasks)
    {
      if (tasks == null)
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.tasks);
      List<Task> taskList = new List<Task>();
      foreach (Task task in tasks)
      {
        if (task == null)
          ThrowHelper.ThrowArgumentException(ExceptionResource.Task_MultiTaskContinuation_NullTask, ExceptionArgument.tasks);
        taskList.Add(task);
      }
      if (taskList.Count == 0)
        ThrowHelper.ThrowArgumentException(ExceptionResource.Task_MultiTaskContinuation_EmptyTaskList, ExceptionArgument.tasks);
      return TaskFactory.CommonCWAnyLogic((IList<Task>) taskList, false);
    }

    public static Task<Task<TResult>> WhenAny<[Nullable(2)] TResult>(
      params Task<TResult>[] tasks)
    {
      return Task.WhenAny((Task[]) tasks).ContinueWith<Task<TResult>>(Task<TResult>.TaskWhenAnyCast.Value, new CancellationToken(), TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    public static Task<Task<TResult>> WhenAny<[Nullable(2)] TResult>(
      IEnumerable<Task<TResult>> tasks)
    {
      return Task.WhenAny((IEnumerable<Task>) tasks).ContinueWith<Task<TResult>>(Task<TResult>.TaskWhenAnyCast.Value, new CancellationToken(), TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    internal static Task<TResult> CreateUnwrapPromise<TResult>(Task outerTask, bool lookForOce)
    {
      return (Task<TResult>) new UnwrapPromise<TResult>(outerTask, lookForOce);
    }

    internal virtual Delegate[] GetDelegateContinuationsForDebugger()
    {
      return this.m_continuationObject != this ? Task.GetDelegatesFromContinuationObject(this.m_continuationObject) : (Delegate[]) null;
    }

    private static Delegate[] GetDelegatesFromContinuationObject(object continuationObject)
    {
      if (continuationObject != null)
      {
        if (continuationObject is Action action)
          return new Delegate[1]
          {
            (Delegate) AsyncMethodBuilderCore.TryGetStateMachineForDebugger(action)
          };
        if (continuationObject is TaskContinuation taskContinuation)
          return taskContinuation.GetDelegateContinuationsForDebugger();
        if (continuationObject is Task task)
        {
          Delegate[] continuationsForDebugger = task.GetDelegateContinuationsForDebugger();
          if (continuationsForDebugger != null)
            return continuationsForDebugger;
        }
        if (continuationObject is ITaskCompletionAction completionAction)
          return new Delegate[1]
          {
            (Delegate) new Action<Task>(completionAction.Invoke)
          };
        if (continuationObject is List<object> objectList)
        {
          List<Delegate> delegateList = new List<Delegate>();
          foreach (object continuationObject1 in objectList)
          {
            Delegate[] continuationObject2 = Task.GetDelegatesFromContinuationObject(continuationObject1);
            if (continuationObject2 != null)
            {
              foreach (Delegate @delegate in continuationObject2)
              {
                if ((object) @delegate != null)
                  delegateList.Add(@delegate);
              }
            }
          }
          return delegateList.ToArray();
        }
      }
      return (Delegate[]) null;
    }

    private static Task GetActiveTaskFromId(int taskId)
    {
      Task task = (Task) null;
      Task.s_currentActiveTasks?.TryGetValue(taskId, out task);
      return task;
    }

    internal class ContingentProperties
    {
      internal volatile int m_completionCountdown = 1;
      internal ExecutionContext m_capturedContext;
      internal volatile ManualResetEventSlim m_completionEvent;
      internal volatile TaskExceptionHolder m_exceptionsHolder;
      internal CancellationToken m_cancellationToken;
      internal StrongBox<CancellationTokenRegistration> m_cancellationRegistration;
      internal volatile int m_internalCancellationRequested;
      internal volatile List<Task> m_exceptionalChildren;
      internal Task m_parent;

      internal void SetCompleted()
      {
        this.m_completionEvent?.Set();
      }

      internal void UnregisterCancellationCallback()
      {
        if (this.m_cancellationRegistration == null)
          return;
        try
        {
          this.m_cancellationRegistration.Value.Dispose();
        }
        catch (ObjectDisposedException ex)
        {
        }
        this.m_cancellationRegistration = (StrongBox<CancellationTokenRegistration>) null;
      }
    }

    private sealed class SetOnInvokeMres : ManualResetEventSlim, ITaskCompletionAction
    {
      internal SetOnInvokeMres()
        : base(false, 0)
      {
      }

      public void Invoke(Task completingTask)
      {
        this.Set();
      }

      public bool InvokeMayRunArbitraryCode
      {
        get
        {
          return false;
        }
      }
    }

    private sealed class SetOnCountdownMres : ManualResetEventSlim, ITaskCompletionAction
    {
      private int _count;

      internal SetOnCountdownMres(int count)
      {
        this._count = count;
      }

      public void Invoke(Task completingTask)
      {
        if (Interlocked.Decrement(ref this._count) != 0)
          return;
        this.Set();
      }

      public bool InvokeMayRunArbitraryCode
      {
        get
        {
          return false;
        }
      }
    }

    private class DelayPromise : Task
    {
      private readonly TimerQueueTimer _timer;

      internal DelayPromise(int millisecondsDelay)
      {
        if (AsyncCausalityTracer.LoggingOn)
          AsyncCausalityTracer.TraceOperationCreation((Task) this, "Task.Delay");
        if (Task.s_asyncDebuggingEnabled)
          Task.AddToActiveTasks((Task) this);
        if (millisecondsDelay == -1)
          return;
        this._timer = new TimerQueueTimer((TimerCallback) (state => ((Task.DelayPromise) state).CompleteTimedOut()), (object) this, (uint) millisecondsDelay, uint.MaxValue, false);
        if (!this.IsCanceled)
          return;
        this._timer.Close();
      }

      private void CompleteTimedOut()
      {
        if (!this.TrySetResult())
          return;
        this.Cleanup();
        if (Task.s_asyncDebuggingEnabled)
          Task.RemoveFromActiveTasks((Task) this);
        if (!AsyncCausalityTracer.LoggingOn)
          return;
        AsyncCausalityTracer.TraceOperationCompletion((Task) this, AsyncCausalityStatus.Completed);
      }

      protected virtual void Cleanup()
      {
        this._timer?.Close();
      }
    }

    private sealed class DelayPromiseWithCancellation : Task.DelayPromise
    {
      private readonly CancellationToken _token;
      private readonly CancellationTokenRegistration _registration;

      internal DelayPromiseWithCancellation(int millisecondsDelay, CancellationToken token)
        : base(millisecondsDelay)
      {
        this._token = token;
        this._registration = token.UnsafeRegister((Action<object>) (state => ((Task.DelayPromiseWithCancellation) state).CompleteCanceled()), (object) this);
      }

      private void CompleteCanceled()
      {
        if (!this.TrySetCanceled(this._token))
          return;
        this.Cleanup();
      }

      protected override void Cleanup()
      {
        this._registration.Dispose();
        base.Cleanup();
      }
    }

    private sealed class WhenAllPromise : Task, ITaskCompletionAction
    {
      private readonly Task[] m_tasks;
      private int m_count;

      internal WhenAllPromise(Task[] tasks)
      {
        if (AsyncCausalityTracer.LoggingOn)
          AsyncCausalityTracer.TraceOperationCreation((Task) this, "Task.WhenAll");
        if (Task.s_asyncDebuggingEnabled)
          Task.AddToActiveTasks((Task) this);
        this.m_tasks = tasks;
        this.m_count = tasks.Length;
        foreach (Task task in tasks)
        {
          if (task.IsCompleted)
            this.Invoke(task);
          else
            task.AddCompletionAction((ITaskCompletionAction) this);
        }
      }

      public void Invoke(Task completedTask)
      {
        if (AsyncCausalityTracer.LoggingOn)
          AsyncCausalityTracer.TraceOperationRelation((Task) this, CausalityRelation.Join);
        if (Interlocked.Decrement(ref this.m_count) != 0)
          return;
        List<ExceptionDispatchInfo> exceptionDispatchInfoList = (List<ExceptionDispatchInfo>) null;
        Task task1 = (Task) null;
        for (int index = 0; index < this.m_tasks.Length; ++index)
        {
          Task task2 = this.m_tasks[index];
          if (task2.IsFaulted)
          {
            if (exceptionDispatchInfoList == null)
              exceptionDispatchInfoList = new List<ExceptionDispatchInfo>();
            exceptionDispatchInfoList.AddRange((IEnumerable<ExceptionDispatchInfo>) task2.GetExceptionDispatchInfos());
          }
          else if (task2.IsCanceled && task1 == null)
            task1 = task2;
          if (task2.IsWaitNotificationEnabled)
            this.SetNotificationForWaitCompletion(true);
          else
            this.m_tasks[index] = (Task) null;
        }
        if (exceptionDispatchInfoList != null)
          this.TrySetException((object) exceptionDispatchInfoList);
        else if (task1 != null)
        {
          this.TrySetCanceled(task1.CancellationToken, (object) task1.GetCancellationExceptionDispatchInfo());
        }
        else
        {
          if (AsyncCausalityTracer.LoggingOn)
            AsyncCausalityTracer.TraceOperationCompletion((Task) this, AsyncCausalityStatus.Completed);
          if (Task.s_asyncDebuggingEnabled)
            Task.RemoveFromActiveTasks((Task) this);
          this.TrySetResult();
        }
      }

      public bool InvokeMayRunArbitraryCode
      {
        get
        {
          return true;
        }
      }

      internal override bool ShouldNotifyDebuggerOfWaitCompletion
      {
        get
        {
          return base.ShouldNotifyDebuggerOfWaitCompletion && Task.AnyTaskRequiresNotifyDebuggerOfWaitCompletion(this.m_tasks);
        }
      }
    }

    private sealed class WhenAllPromise<T> : Task<T[]>, ITaskCompletionAction
    {
      private readonly Task<T>[] m_tasks;
      private int m_count;

      internal WhenAllPromise(Task<T>[] tasks)
      {
        this.m_tasks = tasks;
        this.m_count = tasks.Length;
        if (AsyncCausalityTracer.LoggingOn)
          AsyncCausalityTracer.TraceOperationCreation((Task) this, "Task.WhenAll");
        if (Task.s_asyncDebuggingEnabled)
          Task.AddToActiveTasks((Task) this);
        foreach (Task<T> task in tasks)
        {
          if (task.IsCompleted)
            this.Invoke((Task) task);
          else
            task.AddCompletionAction((ITaskCompletionAction) this);
        }
      }

      public void Invoke(Task ignored)
      {
        if (AsyncCausalityTracer.LoggingOn)
          AsyncCausalityTracer.TraceOperationRelation((Task) this, CausalityRelation.Join);
        if (Interlocked.Decrement(ref this.m_count) != 0)
          return;
        T[] result = new T[this.m_tasks.Length];
        List<ExceptionDispatchInfo> exceptionDispatchInfoList = (List<ExceptionDispatchInfo>) null;
        Task task1 = (Task) null;
        for (int index = 0; index < this.m_tasks.Length; ++index)
        {
          Task<T> task2 = this.m_tasks[index];
          if (task2.IsFaulted)
          {
            if (exceptionDispatchInfoList == null)
              exceptionDispatchInfoList = new List<ExceptionDispatchInfo>();
            exceptionDispatchInfoList.AddRange((IEnumerable<ExceptionDispatchInfo>) task2.GetExceptionDispatchInfos());
          }
          else if (task2.IsCanceled)
          {
            if (task1 == null)
              task1 = (Task) task2;
          }
          else
            result[index] = task2.GetResultCore(false);
          if (task2.IsWaitNotificationEnabled)
            this.SetNotificationForWaitCompletion(true);
          else
            this.m_tasks[index] = (Task<T>) null;
        }
        if (exceptionDispatchInfoList != null)
          this.TrySetException((object) exceptionDispatchInfoList);
        else if (task1 != null)
        {
          this.TrySetCanceled(task1.CancellationToken, (object) task1.GetCancellationExceptionDispatchInfo());
        }
        else
        {
          if (AsyncCausalityTracer.LoggingOn)
            AsyncCausalityTracer.TraceOperationCompletion((Task) this, AsyncCausalityStatus.Completed);
          if (Task.s_asyncDebuggingEnabled)
            Task.RemoveFromActiveTasks((Task) this);
          this.TrySetResult(result);
        }
      }

      public bool InvokeMayRunArbitraryCode
      {
        get
        {
          return true;
        }
      }

      internal override bool ShouldNotifyDebuggerOfWaitCompletion
      {
        get
        {
          return base.ShouldNotifyDebuggerOfWaitCompletion && Task.AnyTaskRequiresNotifyDebuggerOfWaitCompletion((Task[]) this.m_tasks);
        }
      }
    }
  }
}

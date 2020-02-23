// Decompiled with JetBrains decompiler
// Type: System.Diagnostics.Tracing.EventSource
// Assembly: System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// MVID: BBB3B884-123D-47EA-9CD1-5BED540D02AE
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Private.CoreLib.dll

using Microsoft.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Diagnostics.Tracing
{
  [NullableContext(2)]
  [Nullable(0)]
  public class EventSource : IDisposable
  {
    private readonly TraceLoggingEventHandleTable m_eventHandleTable = new TraceLoggingEventHandleTable();
    private static readonly bool m_EventSourcePreventRecursion;
    private string m_name;
    internal int m_id;
    private Guid m_guid;
    internal volatile EventSource.EventMetadata[] m_eventData;
    private volatile byte[] m_rawManifest;
    private EventHandler<EventCommandEventArgs> m_eventCommandExecuted;
    private EventSourceSettings m_config;
    private bool m_eventSourceDisposed;
    private bool m_eventSourceEnabled;
    internal EventLevel m_level;
    internal EventKeywords m_matchAnyKeyword;
    internal volatile EventDispatcher m_Dispatchers;
    private volatile EventSource.OverideEventProvider m_etwProvider;
    private volatile EventSource.OverideEventProvider m_eventPipeProvider;
    private bool m_completelyInited;
    private Exception m_constructionException;
    private byte m_outOfBandMessageCount;
    private EventCommandEventArgs m_deferredCommands;
    private string[] m_traits;
    internal static uint s_currentPid;
    [ThreadStatic]
    private static byte m_EventSourceExceptionRecurenceCount;
    [ThreadStatic]
    private static bool m_EventSourceInDecodeObject;
    internal volatile ulong[] m_channelData;
    private ActivityTracker m_activityTracker;
    private static byte[] namespaceBytes;
    private byte[] providerMetadata;

    [Nullable(1)]
    public string Name
    {
      [NullableContext(1)] get
      {
        return this.m_name;
      }
    }

    public Guid Guid
    {
      get
      {
        return this.m_guid;
      }
    }

    public bool IsEnabled()
    {
      return this.m_eventSourceEnabled;
    }

    public bool IsEnabled(EventLevel level, EventKeywords keywords)
    {
      return this.IsEnabled(level, keywords, EventChannel.None);
    }

    public bool IsEnabled(EventLevel level, EventKeywords keywords, EventChannel channel)
    {
      return this.m_eventSourceEnabled && this.IsEnabledCommon(this.m_eventSourceEnabled, this.m_level, this.m_matchAnyKeyword, level, keywords, channel);
    }

    public EventSourceSettings Settings
    {
      get
      {
        return this.m_config;
      }
    }

    [NullableContext(1)]
    public static Guid GetGuid(Type eventSourceType)
    {
      if (eventSourceType == (Type) null)
        throw new ArgumentNullException(nameof (eventSourceType));
      EventSourceAttribute customAttributeHelper = (EventSourceAttribute) EventSource.GetCustomAttributeHelper((MemberInfo) eventSourceType, typeof (EventSourceAttribute), EventManifestOptions.None);
      string name = eventSourceType.Name;
      if (customAttributeHelper != null)
      {
        if (customAttributeHelper.Guid != null)
        {
          Guid result = Guid.Empty;
          if (Guid.TryParse(customAttributeHelper.Guid, out result))
            return result;
        }
        if (customAttributeHelper.Name != null)
          name = customAttributeHelper.Name;
      }
      if (name == null)
        throw new ArgumentException(SR.Argument_InvalidTypeName, nameof (eventSourceType));
      return EventSource.GenerateGuidFromName(name.ToUpperInvariant());
    }

    [NullableContext(1)]
    public static string GetName(Type eventSourceType)
    {
      return EventSource.GetName(eventSourceType, EventManifestOptions.None);
    }

    public static string GenerateManifest(
      [Nullable(1)] Type eventSourceType,
      string assemblyPathToIncludeInManifest)
    {
      return EventSource.GenerateManifest(eventSourceType, assemblyPathToIncludeInManifest, EventManifestOptions.None);
    }

    public static string GenerateManifest(
      [Nullable(1)] Type eventSourceType,
      string assemblyPathToIncludeInManifest,
      EventManifestOptions flags)
    {
      if (eventSourceType == (Type) null)
        throw new ArgumentNullException(nameof (eventSourceType));
      byte[] manifestAndDescriptors = EventSource.CreateManifestAndDescriptors(eventSourceType, assemblyPathToIncludeInManifest, (EventSource) null, flags);
      return manifestAndDescriptors != null ? Encoding.UTF8.GetString(manifestAndDescriptors, 0, manifestAndDescriptors.Length) : (string) null;
    }

    [NullableContext(1)]
    public static IEnumerable<EventSource> GetSources()
    {
      List<EventSource> eventSourceList = new List<EventSource>();
      lock (EventListener.EventListenersLock)
      {
        foreach (WeakReference eventSource in EventListener.s_EventSources)
        {
          if (eventSource.Target is EventSource target && !target.IsDisposed)
            eventSourceList.Add(target);
        }
      }
      return (IEnumerable<EventSource>) eventSourceList;
    }

    [NullableContext(1)]
    public static void SendCommand(
      EventSource eventSource,
      EventCommand command,
      [Nullable(new byte[] {2, 1, 2})] IDictionary<string, string> commandArguments)
    {
      if (eventSource == null)
        throw new ArgumentNullException(nameof (eventSource));
      if (command <= EventCommand.Update && command != EventCommand.SendManifest)
        throw new ArgumentException(SR.EventSource_InvalidCommand, nameof (command));
      eventSource.SendCommand((EventListener) null, EventProviderType.ETW, 0, 0, command, true, EventLevel.LogAlways, EventKeywords.None, commandArguments);
    }

    public Exception ConstructionException
    {
      get
      {
        return this.m_constructionException;
      }
    }

    [NullableContext(1)]
    [return: Nullable(2)]
    public string GetTrait(string key)
    {
      if (this.m_traits != null)
      {
        for (int index = 0; index < this.m_traits.Length - 1; index += 2)
        {
          if (this.m_traits[index] == key)
            return this.m_traits[index + 1];
        }
      }
      return (string) null;
    }

    [NullableContext(1)]
    public override string ToString()
    {
      return SR.Format(SR.EventSource_ToString, (object) this.Name, (object) this.Guid);
    }

    [Nullable(new byte[] {2, 1})]
    public event EventHandler<EventCommandEventArgs> EventCommandExecuted
    {
      add
      {
        if (value == null)
          return;
        this.m_eventCommandExecuted += value;
        for (EventCommandEventArgs e = this.m_deferredCommands; e != null; e = e.nextCommand)
          value((object) this, e);
      }
      remove
      {
        this.m_eventCommandExecuted -= value;
      }
    }

    public static void SetCurrentThreadActivityId(Guid activityId)
    {
      if (TplEventSource.Log != null)
        TplEventSource.Log.SetActivityId(activityId);
      EventPipeInternal.EventActivityIdControl(2U, ref activityId);
      Interop.Advapi32.EventActivityIdControl(Interop.Advapi32.ActivityControl.EVENT_ACTIVITY_CTRL_SET_ID, ref activityId);
    }

    public static Guid CurrentThreadActivityId
    {
      get
      {
        Guid ActivityId = new Guid();
        Interop.Advapi32.EventActivityIdControl(Interop.Advapi32.ActivityControl.EVENT_ACTIVITY_CTRL_GET_ID, ref ActivityId);
        return ActivityId;
      }
    }

    public static void SetCurrentThreadActivityId(
      Guid activityId,
      out Guid oldActivityThatWillContinue)
    {
      oldActivityThatWillContinue = activityId;
      EventPipeInternal.EventActivityIdControl(2U, ref oldActivityThatWillContinue);
      Interop.Advapi32.EventActivityIdControl(Interop.Advapi32.ActivityControl.EVENT_ACTIVITY_CTRL_GET_SET_ID, ref oldActivityThatWillContinue);
      if (TplEventSource.Log == null)
        return;
      TplEventSource.Log.SetActivityId(activityId);
    }

    protected EventSource()
      : this(EventSourceSettings.EtwManifestEventFormat)
    {
    }

    protected EventSource(bool throwOnEventWriteErrors)
      : this((EventSourceSettings) (4 | (throwOnEventWriteErrors ? 1 : 0)))
    {
    }

    protected EventSource(EventSourceSettings settings)
      : this(settings, (string[]) null)
    {
    }

    protected EventSource(EventSourceSettings settings, [Nullable(new byte[] {2, 1})] params string[] traits)
    {
      this.m_config = this.ValidateSettings(settings);
      Guid eventSourceGuid;
      string eventSourceName;
      this.GetMetadata(out eventSourceGuid, out eventSourceName, out EventSource.EventMetadata[] _, out byte[] _);
      if (eventSourceGuid.Equals(Guid.Empty) || eventSourceName == null)
      {
        Type type = this.GetType();
        eventSourceGuid = EventSource.GetGuid(type);
        eventSourceName = EventSource.GetName(type);
      }
      this.Initialize(eventSourceGuid, eventSourceName, traits);
    }

    private unsafe void DefineEventPipeEvents()
    {
      if (this.SelfDescribingEvents)
        return;
      int length = this.m_eventData.Length;
      for (int index = 0; index < length; ++index)
      {
        uint eventId = (uint) this.m_eventData[index].Descriptor.EventId;
        if (eventId != 0U)
        {
          byte[] eventMetadata = EventPipeMetadataGenerator.Instance.GenerateEventMetadata(this.m_eventData[index]);
          uint metadataLength = eventMetadata != null ? (uint) eventMetadata.Length : 0U;
          string name = this.m_eventData[index].Name;
          long keywords = this.m_eventData[index].Descriptor.Keywords;
          uint version = (uint) this.m_eventData[index].Descriptor.Version;
          uint level = (uint) this.m_eventData[index].Descriptor.Level;
          fixed (byte* pMetadata = eventMetadata)
          {
            IntPtr num = this.m_eventPipeProvider.m_eventProvider.DefineEventHandle(eventId, name, keywords, version, level, pMetadata, metadataLength);
            this.m_eventData[index].EventHandle = num;
          }
        }
      }
    }

    internal virtual void GetMetadata(
      out Guid eventSourceGuid,
      out string eventSourceName,
      out EventSource.EventMetadata[] eventData,
      out byte[] manifestBytes)
    {
      eventSourceGuid = Guid.Empty;
      eventSourceName = (string) null;
      eventData = (EventSource.EventMetadata[]) null;
      manifestBytes = (byte[]) null;
    }

    [NullableContext(1)]
    protected virtual void OnEventCommand(EventCommandEventArgs command)
    {
    }

    protected unsafe void WriteEvent(int eventId)
    {
      this.WriteEventCore(eventId, 0, (EventSource.EventData*) null);
    }

    protected unsafe void WriteEvent(int eventId, int arg1)
    {
      if (!this.m_eventSourceEnabled)
        return;
      EventSource.EventData* data = stackalloc EventSource.EventData[1];
      data->DataPointer = (IntPtr) (void*) &arg1;
      data->Size = 4;
      data->Reserved = 0;
      this.WriteEventCore(eventId, 1, data);
    }

    protected unsafe void WriteEvent(int eventId, int arg1, int arg2)
    {
      if (!this.m_eventSourceEnabled)
        return;
      EventSource.EventData* data = stackalloc EventSource.EventData[2];
      data->DataPointer = (IntPtr) (void*) &arg1;
      data->Size = 4;
      data->Reserved = 0;
      data[1].DataPointer = (IntPtr) (void*) &arg2;
      data[1].Size = 4;
      data[1].Reserved = 0;
      this.WriteEventCore(eventId, 2, data);
    }

    protected unsafe void WriteEvent(int eventId, int arg1, int arg2, int arg3)
    {
      if (!this.m_eventSourceEnabled)
        return;
      EventSource.EventData* data = stackalloc EventSource.EventData[3];
      data->DataPointer = (IntPtr) (void*) &arg1;
      data->Size = 4;
      data->Reserved = 0;
      data[1].DataPointer = (IntPtr) (void*) &arg2;
      data[1].Size = 4;
      data[1].Reserved = 0;
      data[2].DataPointer = (IntPtr) (void*) &arg3;
      data[2].Size = 4;
      data[2].Reserved = 0;
      this.WriteEventCore(eventId, 3, data);
    }

    protected unsafe void WriteEvent(int eventId, long arg1)
    {
      if (!this.m_eventSourceEnabled)
        return;
      EventSource.EventData* data = stackalloc EventSource.EventData[1];
      data->DataPointer = (IntPtr) (void*) &arg1;
      data->Size = 8;
      data->Reserved = 0;
      this.WriteEventCore(eventId, 1, data);
    }

    protected unsafe void WriteEvent(int eventId, long arg1, long arg2)
    {
      if (!this.m_eventSourceEnabled)
        return;
      EventSource.EventData* data = stackalloc EventSource.EventData[2];
      data->DataPointer = (IntPtr) (void*) &arg1;
      data->Size = 8;
      data->Reserved = 0;
      data[1].DataPointer = (IntPtr) (void*) &arg2;
      data[1].Size = 8;
      data[1].Reserved = 0;
      this.WriteEventCore(eventId, 2, data);
    }

    protected unsafe void WriteEvent(int eventId, long arg1, long arg2, long arg3)
    {
      if (!this.m_eventSourceEnabled)
        return;
      EventSource.EventData* data = stackalloc EventSource.EventData[3];
      data->DataPointer = (IntPtr) (void*) &arg1;
      data->Size = 8;
      data->Reserved = 0;
      data[1].DataPointer = (IntPtr) (void*) &arg2;
      data[1].Size = 8;
      data[1].Reserved = 0;
      data[2].DataPointer = (IntPtr) (void*) &arg3;
      data[2].Size = 8;
      data[2].Reserved = 0;
      this.WriteEventCore(eventId, 3, data);
    }

    protected unsafe void WriteEvent(int eventId, string arg1)
    {
      if (!this.m_eventSourceEnabled)
        return;
      if (arg1 == null)
        arg1 = "";
      IntPtr num;
      if (arg1 == null)
      {
        num = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = &arg1.GetPinnableReference())
          num = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) num;
      EventSource.EventData* data = stackalloc EventSource.EventData[1];
      data->DataPointer = (IntPtr) (void*) chPtr1;
      data->Size = (arg1.Length + 1) * 2;
      data->Reserved = 0;
      this.WriteEventCore(eventId, 1, data);
      // ISSUE: fixed variable is out of scope
      // ISSUE: __unpin statement
      __unpin(chPtr);
    }

    protected unsafe void WriteEvent(int eventId, string arg1, string arg2)
    {
      if (!this.m_eventSourceEnabled)
        return;
      if (arg1 == null)
        arg1 = "";
      if (arg2 == null)
        arg2 = "";
      IntPtr num1;
      if (arg1 == null)
      {
        num1 = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = &arg1.GetPinnableReference())
          num1 = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) num1;
      IntPtr num2;
      if (arg2 == null)
      {
        num2 = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr2 = &arg2.GetPinnableReference())
          num2 = (IntPtr) chPtr2;
      }
      char* chPtr3 = (char*) num2;
      EventSource.EventData* data = stackalloc EventSource.EventData[2];
      data->DataPointer = (IntPtr) (void*) chPtr1;
      data->Size = (arg1.Length + 1) * 2;
      data->Reserved = 0;
      data[1].DataPointer = (IntPtr) (void*) chPtr3;
      data[1].Size = (arg2.Length + 1) * 2;
      data[1].Reserved = 0;
      this.WriteEventCore(eventId, 2, data);
      // ISSUE: fixed variable is out of scope
      // ISSUE: __unpin statement
      __unpin(chPtr2);
      // ISSUE: fixed variable is out of scope
      // ISSUE: __unpin statement
      __unpin(chPtr);
    }

    protected unsafe void WriteEvent(int eventId, string arg1, string arg2, string arg3)
    {
      if (!this.m_eventSourceEnabled)
        return;
      if (arg1 == null)
        arg1 = "";
      if (arg2 == null)
        arg2 = "";
      if (arg3 == null)
        arg3 = "";
      IntPtr num1;
      if (arg1 == null)
      {
        num1 = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = &arg1.GetPinnableReference())
          num1 = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) num1;
      IntPtr num2;
      if (arg2 == null)
      {
        num2 = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr2 = &arg2.GetPinnableReference())
          num2 = (IntPtr) chPtr2;
      }
      char* chPtr3 = (char*) num2;
      IntPtr num3;
      if (arg3 == null)
      {
        num3 = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr2 = &arg3.GetPinnableReference())
          num3 = (IntPtr) chPtr2;
      }
      char* chPtr4 = (char*) num3;
      EventSource.EventData* data = stackalloc EventSource.EventData[3];
      data->DataPointer = (IntPtr) (void*) chPtr1;
      data->Size = (arg1.Length + 1) * 2;
      data->Reserved = 0;
      data[1].DataPointer = (IntPtr) (void*) chPtr3;
      data[1].Size = (arg2.Length + 1) * 2;
      data[1].Reserved = 0;
      data[2].DataPointer = (IntPtr) (void*) chPtr4;
      data[2].Size = (arg3.Length + 1) * 2;
      data[2].Reserved = 0;
      this.WriteEventCore(eventId, 3, data);
      // ISSUE: fixed variable is out of scope
      // ISSUE: __unpin statement
      __unpin(chPtr2);
      // ISSUE: fixed variable is out of scope
      // ISSUE: __unpin statement
      __unpin(chPtr2);
      // ISSUE: fixed variable is out of scope
      // ISSUE: __unpin statement
      __unpin(chPtr);
    }

    protected unsafe void WriteEvent(int eventId, string arg1, int arg2)
    {
      if (!this.m_eventSourceEnabled)
        return;
      if (arg1 == null)
        arg1 = "";
      IntPtr num;
      if (arg1 == null)
      {
        num = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = &arg1.GetPinnableReference())
          num = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) num;
      EventSource.EventData* data = stackalloc EventSource.EventData[2];
      data->DataPointer = (IntPtr) (void*) chPtr1;
      data->Size = (arg1.Length + 1) * 2;
      data->Reserved = 0;
      data[1].DataPointer = (IntPtr) (void*) &arg2;
      data[1].Size = 4;
      data[1].Reserved = 0;
      this.WriteEventCore(eventId, 2, data);
      // ISSUE: fixed variable is out of scope
      // ISSUE: __unpin statement
      __unpin(chPtr);
    }

    protected unsafe void WriteEvent(int eventId, string arg1, int arg2, int arg3)
    {
      if (!this.m_eventSourceEnabled)
        return;
      if (arg1 == null)
        arg1 = "";
      IntPtr num;
      if (arg1 == null)
      {
        num = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = &arg1.GetPinnableReference())
          num = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) num;
      EventSource.EventData* data = stackalloc EventSource.EventData[3];
      data->DataPointer = (IntPtr) (void*) chPtr1;
      data->Size = (arg1.Length + 1) * 2;
      data->Reserved = 0;
      data[1].DataPointer = (IntPtr) (void*) &arg2;
      data[1].Size = 4;
      data[1].Reserved = 0;
      data[2].DataPointer = (IntPtr) (void*) &arg3;
      data[2].Size = 4;
      data[2].Reserved = 0;
      this.WriteEventCore(eventId, 3, data);
      // ISSUE: fixed variable is out of scope
      // ISSUE: __unpin statement
      __unpin(chPtr);
    }

    protected unsafe void WriteEvent(int eventId, string arg1, long arg2)
    {
      if (!this.m_eventSourceEnabled)
        return;
      if (arg1 == null)
        arg1 = "";
      IntPtr num;
      if (arg1 == null)
      {
        num = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = &arg1.GetPinnableReference())
          num = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) num;
      EventSource.EventData* data = stackalloc EventSource.EventData[2];
      data->DataPointer = (IntPtr) (void*) chPtr1;
      data->Size = (arg1.Length + 1) * 2;
      data->Reserved = 0;
      data[1].DataPointer = (IntPtr) (void*) &arg2;
      data[1].Size = 8;
      data[1].Reserved = 0;
      this.WriteEventCore(eventId, 2, data);
      // ISSUE: fixed variable is out of scope
      // ISSUE: __unpin statement
      __unpin(chPtr);
    }

    protected unsafe void WriteEvent(int eventId, long arg1, string arg2)
    {
      if (!this.m_eventSourceEnabled)
        return;
      if (arg2 == null)
        arg2 = "";
      IntPtr num;
      if (arg2 == null)
      {
        num = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = &arg2.GetPinnableReference())
          num = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) num;
      EventSource.EventData* data = stackalloc EventSource.EventData[2];
      data->DataPointer = (IntPtr) (void*) &arg1;
      data->Size = 8;
      data->Reserved = 0;
      data[1].DataPointer = (IntPtr) (void*) chPtr1;
      data[1].Size = (arg2.Length + 1) * 2;
      data[1].Reserved = 0;
      this.WriteEventCore(eventId, 2, data);
      // ISSUE: fixed variable is out of scope
      // ISSUE: __unpin statement
      __unpin(chPtr);
    }

    protected unsafe void WriteEvent(int eventId, int arg1, string arg2)
    {
      if (!this.m_eventSourceEnabled)
        return;
      if (arg2 == null)
        arg2 = "";
      IntPtr num;
      if (arg2 == null)
      {
        num = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = &arg2.GetPinnableReference())
          num = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) num;
      EventSource.EventData* data = stackalloc EventSource.EventData[2];
      data->DataPointer = (IntPtr) (void*) &arg1;
      data->Size = 4;
      data->Reserved = 0;
      data[1].DataPointer = (IntPtr) (void*) chPtr1;
      data[1].Size = (arg2.Length + 1) * 2;
      data[1].Reserved = 0;
      this.WriteEventCore(eventId, 2, data);
      // ISSUE: fixed variable is out of scope
      // ISSUE: __unpin statement
      __unpin(chPtr);
    }

    protected unsafe void WriteEvent(int eventId, byte[] arg1)
    {
      if (!this.m_eventSourceEnabled)
        return;
      EventSource.EventData* data = stackalloc EventSource.EventData[2];
      if (arg1 == null || arg1.Length == 0)
      {
        int num = 0;
        data->DataPointer = (IntPtr) (void*) &num;
        data->Size = 4;
        data->Reserved = 0;
        data[1].DataPointer = (IntPtr) (void*) &num;
        data[1].Size = 0;
        data[1].Reserved = 0;
        this.WriteEventCore(eventId, 2, data);
      }
      else
      {
        int length = arg1.Length;
        fixed (byte* numPtr = &arg1[0])
        {
          data->DataPointer = (IntPtr) (void*) &length;
          data->Size = 4;
          data->Reserved = 0;
          data[1].DataPointer = (IntPtr) (void*) numPtr;
          data[1].Size = length;
          data[1].Reserved = 0;
          this.WriteEventCore(eventId, 2, data);
        }
      }
    }

    protected unsafe void WriteEvent(int eventId, long arg1, byte[] arg2)
    {
      if (!this.m_eventSourceEnabled)
        return;
      EventSource.EventData* data = stackalloc EventSource.EventData[3];
      data->DataPointer = (IntPtr) (void*) &arg1;
      data->Size = 8;
      data->Reserved = 0;
      if (arg2 == null || arg2.Length == 0)
      {
        int num = 0;
        data[1].DataPointer = (IntPtr) (void*) &num;
        data[1].Size = 4;
        data[1].Reserved = 0;
        data[2].DataPointer = (IntPtr) (void*) &num;
        data[2].Size = 0;
        data[2].Reserved = 0;
        this.WriteEventCore(eventId, 3, data);
      }
      else
      {
        int length = arg2.Length;
        fixed (byte* numPtr = &arg2[0])
        {
          data[1].DataPointer = (IntPtr) (void*) &length;
          data[1].Size = 4;
          data[1].Reserved = 0;
          data[2].DataPointer = (IntPtr) (void*) numPtr;
          data[2].Size = length;
          data[2].Reserved = 0;
          this.WriteEventCore(eventId, 3, data);
        }
      }
    }

    [CLSCompliant(false)]
    [NullableContext(0)]
    protected unsafe void WriteEventCore(
      int eventId,
      int eventDataCount,
      EventSource.EventData* data)
    {
      this.WriteEventWithRelatedActivityIdCore(eventId, (Guid*) null, eventDataCount, data);
    }

    [CLSCompliant(false)]
    [NullableContext(0)]
    protected unsafe void WriteEventWithRelatedActivityIdCore(
      int eventId,
      Guid* relatedActivityId,
      int eventDataCount,
      EventSource.EventData* data)
    {
      if (!this.m_eventSourceEnabled)
        return;
      try
      {
        if ((IntPtr) relatedActivityId != IntPtr.Zero)
          this.ValidateEventOpcodeForTransfer(ref this.m_eventData[eventId], this.m_eventData[eventId].Name);
        EventOpcode opcode = (EventOpcode) this.m_eventData[eventId].Descriptor.Opcode;
        EventActivityOptions activityOptions = this.m_eventData[eventId].ActivityOptions;
        Guid* activityID = (Guid*) null;
        Guid empty1 = Guid.Empty;
        Guid empty2 = Guid.Empty;
        if (opcode != EventOpcode.Info && (IntPtr) relatedActivityId == IntPtr.Zero && (activityOptions & EventActivityOptions.Disable) == EventActivityOptions.None)
        {
          switch (opcode)
          {
            case EventOpcode.Start:
              this.m_activityTracker.OnStart(this.m_name, this.m_eventData[eventId].Name, this.m_eventData[eventId].Descriptor.Task, ref empty1, ref empty2, this.m_eventData[eventId].ActivityOptions);
              break;
            case EventOpcode.Stop:
              this.m_activityTracker.OnStop(this.m_name, this.m_eventData[eventId].Name, this.m_eventData[eventId].Descriptor.Task, ref empty1);
              break;
          }
          if (empty1 != Guid.Empty)
            activityID = &empty1;
          if (empty2 != Guid.Empty)
            relatedActivityId = &empty2;
        }
        if (this.m_eventData[eventId].EnabledForETW || this.m_eventData[eventId].EnabledForEventPipe)
        {
          if (!this.SelfDescribingEvents)
          {
            if (!this.m_etwProvider.WriteEvent(ref this.m_eventData[eventId].Descriptor, this.m_eventData[eventId].EventHandle, activityID, relatedActivityId, eventDataCount, (IntPtr) (void*) data))
              this.ThrowEventSourceException(this.m_eventData[eventId].Name, (Exception) null);
            if (!this.m_eventPipeProvider.WriteEvent(ref this.m_eventData[eventId].Descriptor, this.m_eventData[eventId].EventHandle, activityID, relatedActivityId, eventDataCount, (IntPtr) (void*) data))
              this.ThrowEventSourceException(this.m_eventData[eventId].Name, (Exception) null);
          }
          else
          {
            TraceLoggingEventTypes eventTypes = this.m_eventData[eventId].TraceLoggingEventTypes;
            if (eventTypes == null)
            {
              eventTypes = new TraceLoggingEventTypes(this.m_eventData[eventId].Name, this.m_eventData[eventId].Tags, this.m_eventData[eventId].Parameters);
              Interlocked.CompareExchange<TraceLoggingEventTypes>(ref this.m_eventData[eventId].TraceLoggingEventTypes, eventTypes, (TraceLoggingEventTypes) null);
            }
            EventSourceOptions options = new EventSourceOptions()
            {
              Keywords = (EventKeywords) this.m_eventData[eventId].Descriptor.Keywords,
              Level = (EventLevel) this.m_eventData[eventId].Descriptor.Level,
              Opcode = (EventOpcode) this.m_eventData[eventId].Descriptor.Opcode
            };
            this.WriteMultiMerge(this.m_eventData[eventId].Name, ref options, eventTypes, activityID, relatedActivityId, data);
          }
        }
        if (this.m_Dispatchers == null || !this.m_eventData[eventId].EnabledForAnyListener)
          return;
        this.WriteToAllListeners(eventId, activityID, relatedActivityId, eventDataCount, data);
      }
      catch (Exception ex)
      {
        if (ex is EventSourceException)
          throw;
        else
          this.ThrowEventSourceException(this.m_eventData[eventId].Name, ex);
      }
    }

    protected unsafe void WriteEvent(int eventId, [Nullable(new byte[] {1, 2})] params object[] args)
    {
      this.WriteEventVarargs(eventId, (Guid*) null, args);
    }

    protected unsafe void WriteEventWithRelatedActivityId(
      int eventId,
      Guid relatedActivityId,
      [Nullable(new byte[] {1, 2})] params object[] args)
    {
      this.WriteEventVarargs(eventId, &relatedActivityId, args);
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
        if (this.m_eventSourceEnabled)
        {
          try
          {
            this.SendManifest(this.m_rawManifest);
          }
          catch (Exception ex)
          {
          }
          this.m_eventSourceEnabled = false;
        }
        if (this.m_etwProvider != null)
        {
          this.m_etwProvider.Dispose();
          this.m_etwProvider = (EventSource.OverideEventProvider) null;
        }
        if (this.m_eventPipeProvider != null)
        {
          this.m_eventPipeProvider.Dispose();
          this.m_eventPipeProvider = (EventSource.OverideEventProvider) null;
        }
      }
      this.m_eventSourceEnabled = false;
      this.m_eventSourceDisposed = true;
    }

    ~EventSource()
    {
      this.Dispose(false);
    }

    private unsafe void WriteEventRaw(
      string eventName,
      ref EventDescriptor eventDescriptor,
      IntPtr eventHandle,
      Guid* activityID,
      Guid* relatedActivityID,
      int dataCount,
      IntPtr data)
    {
      if (this.m_etwProvider == null)
        this.ThrowEventSourceException(eventName, (Exception) null);
      else if (!this.m_etwProvider.WriteEventRaw(ref eventDescriptor, eventHandle, activityID, relatedActivityID, dataCount, data))
        this.ThrowEventSourceException(eventName, (Exception) null);
      if (this.m_eventPipeProvider == null)
      {
        this.ThrowEventSourceException(eventName, (Exception) null);
      }
      else
      {
        if (this.m_eventPipeProvider.WriteEventRaw(ref eventDescriptor, eventHandle, activityID, relatedActivityID, dataCount, data))
          return;
        this.ThrowEventSourceException(eventName, (Exception) null);
      }
    }

    internal EventSource(Guid eventSourceGuid, string eventSourceName)
      : this(eventSourceGuid, eventSourceName, EventSourceSettings.EtwManifestEventFormat, (string[]) null)
    {
    }

    internal EventSource(
      Guid eventSourceGuid,
      string eventSourceName,
      EventSourceSettings settings,
      string[] traits = null)
    {
      this.m_config = this.ValidateSettings(settings);
      this.Initialize(eventSourceGuid, eventSourceName, traits);
    }

    private void Initialize(Guid eventSourceGuid, string eventSourceName, string[] traits)
    {
      try
      {
        this.m_traits = traits;
        if (this.m_traits != null && this.m_traits.Length % 2 != 0)
          throw new ArgumentException(SR.EventSource_TraitEven, nameof (traits));
        if (eventSourceGuid == Guid.Empty)
          throw new ArgumentException(SR.EventSource_NeedGuid);
        if (eventSourceName == null)
          throw new ArgumentException(SR.EventSource_NeedName);
        this.m_name = eventSourceName;
        this.m_guid = eventSourceGuid;
        this.m_activityTracker = ActivityTracker.Instance;
        this.InitializeProviderMetadata();
        EventSource.OverideEventProvider overideEventProvider1 = new EventSource.OverideEventProvider(this, EventProviderType.ETW);
        overideEventProvider1.Register(this);
        EventSource.OverideEventProvider overideEventProvider2 = new EventSource.OverideEventProvider(this, EventProviderType.EventPipe);
        lock (EventListener.EventListenersLock)
          overideEventProvider2.Register(this);
        EventListener.AddEventSource(this);
        this.m_etwProvider = overideEventProvider1;
        if (this.Name != "System.Diagnostics.Eventing.FrameworkEventSource" || Environment.IsWindows8OrAbove)
        {
          GCHandle gcHandle = GCHandle.Alloc((object) this.providerMetadata, GCHandleType.Pinned);
          this.m_etwProvider.SetInformation(Interop.Advapi32.EVENT_INFO_CLASS.SetTraits, gcHandle.AddrOfPinnedObject(), (uint) this.providerMetadata.Length);
          gcHandle.Free();
        }
        this.m_eventPipeProvider = overideEventProvider2;
        this.m_completelyInited = true;
      }
      catch (Exception ex)
      {
        if (this.m_constructionException == null)
          this.m_constructionException = ex;
        this.ReportOutOfBandMessage("ERROR: Exception during construction of EventSource " + this.Name + ": " + ex.Message, true);
      }
      lock (EventListener.EventListenersLock)
      {
        for (EventCommandEventArgs commandArgs = this.m_deferredCommands; commandArgs != null; commandArgs = commandArgs.nextCommand)
          this.DoCommand(commandArgs);
      }
    }

    private static string GetName(Type eventSourceType, EventManifestOptions flags)
    {
      if (eventSourceType == (Type) null)
        throw new ArgumentNullException(nameof (eventSourceType));
      EventSourceAttribute customAttributeHelper = (EventSourceAttribute) EventSource.GetCustomAttributeHelper((MemberInfo) eventSourceType, typeof (EventSourceAttribute), flags);
      return customAttributeHelper != null && customAttributeHelper.Name != null ? customAttributeHelper.Name : eventSourceType.Name;
    }

    private static Guid GenerateGuidFromName(string name)
    {
      if (EventSource.namespaceBytes == null)
        EventSource.namespaceBytes = new byte[16]
        {
          (byte) 72,
          (byte) 44,
          (byte) 45,
          (byte) 178,
          (byte) 195,
          (byte) 144,
          (byte) 71,
          (byte) 200,
          (byte) 135,
          (byte) 248,
          (byte) 26,
          (byte) 21,
          (byte) 191,
          (byte) 193,
          (byte) 48,
          (byte) 251
        };
      byte[] bytes = Encoding.BigEndianUnicode.GetBytes(name);
      EventSource.Sha1ForNonSecretPurposes nonSecretPurposes = new EventSource.Sha1ForNonSecretPurposes();
      nonSecretPurposes.Start();
      nonSecretPurposes.Append(EventSource.namespaceBytes);
      nonSecretPurposes.Append(bytes);
      Array.Resize<byte>(ref bytes, 16);
      nonSecretPurposes.Finish(bytes);
      bytes[7] = (byte) ((int) bytes[7] & 15 | 80);
      return new Guid(bytes);
    }

    private unsafe object DecodeObject(
      int eventId,
      int parameterId,
      ref EventSource.EventData* data)
    {
      IntPtr dataPointer1 = data->DataPointer;
      ++data;
      Type type = this.GetDataType(this.m_eventData[eventId], parameterId);
      while (!(type == typeof (IntPtr)))
      {
        if (type == typeof (int))
          return (object) *(int*) (void*) dataPointer1;
        if (type == typeof (uint))
          return (object) *(uint*) (void*) dataPointer1;
        if (type == typeof (long))
          return (object) *(long*) (void*) dataPointer1;
        if (type == typeof (ulong))
          return (object) (ulong) *(long*) (void*) dataPointer1;
        if (type == typeof (byte))
          return (object) *(byte*) (void*) dataPointer1;
        if (type == typeof (sbyte))
          return (object) *(sbyte*) (void*) dataPointer1;
        if (type == typeof (short))
          return (object) *(short*) (void*) dataPointer1;
        if (type == typeof (ushort))
          return (object) *(ushort*) (void*) dataPointer1;
        if (type == typeof (float))
          return (object) *(float*) (void*) dataPointer1;
        if (type == typeof (double))
          return (object) *(double*) (void*) dataPointer1;
        if (type == typeof (Decimal))
          return (object) *(Decimal*) (void*) dataPointer1;
        if (type == typeof (bool))
          return *(int*) (void*) dataPointer1 == 1 ? (object) true : (object) false;
        if (type == typeof (Guid))
          return (object) *(Guid*) (void*) dataPointer1;
        if (type == typeof (char))
          return (object) (char) *(ushort*) (void*) dataPointer1;
        if (type == typeof (DateTime))
          return (object) DateTime.FromFileTimeUtc(*(long*) (void*) dataPointer1);
        if (type == typeof (byte[]))
        {
          int length = *(int*) (void*) dataPointer1;
          byte[] numArray = new byte[length];
          IntPtr dataPointer2 = data->DataPointer;
          ++data;
          for (int index = 0; index < length; ++index)
            numArray[index] = *(byte*) (void*) (dataPointer2 + index);
          return (object) numArray;
        }
        if (type == typeof (byte*))
          return (object) null;
        if (EventSource.m_EventSourcePreventRecursion && EventSource.m_EventSourceInDecodeObject)
          return (object) null;
        try
        {
          EventSource.m_EventSourceInDecodeObject = true;
          if (type.IsEnum())
          {
            type = Enum.GetUnderlyingType(type);
            if (Marshal.SizeOf(type) < 4)
              type = typeof (int);
          }
          else
            return dataPointer1 == IntPtr.Zero ? (object) null : (object) new string((char*) (void*) dataPointer1);
        }
        finally
        {
          EventSource.m_EventSourceInDecodeObject = false;
        }
      }
      return (object) *(IntPtr*) (void*) dataPointer1;
    }

    private EventDispatcher GetDispatcher(EventListener listener)
    {
      EventDispatcher eventDispatcher = this.m_Dispatchers;
      while (eventDispatcher != null && eventDispatcher.m_Listener != listener)
        eventDispatcher = eventDispatcher.m_Next;
      return eventDispatcher;
    }

    private unsafe void WriteEventVarargs(int eventId, Guid* childActivityID, object[] args)
    {
      if (!this.m_eventSourceEnabled)
        return;
      try
      {
        if ((IntPtr) childActivityID != IntPtr.Zero)
        {
          this.ValidateEventOpcodeForTransfer(ref this.m_eventData[eventId], this.m_eventData[eventId].Name);
          if (!this.m_eventData[eventId].HasRelatedActivityID)
            throw new ArgumentException(SR.EventSource_NoRelatedActivityId);
        }
        this.LogEventArgsMismatches(this.m_eventData[eventId].Parameters, args);
        Guid* activityID = (Guid*) null;
        Guid empty1 = Guid.Empty;
        Guid empty2 = Guid.Empty;
        EventOpcode opcode = (EventOpcode) this.m_eventData[eventId].Descriptor.Opcode;
        EventActivityOptions activityOptions = this.m_eventData[eventId].ActivityOptions;
        if ((IntPtr) childActivityID == IntPtr.Zero && (activityOptions & EventActivityOptions.Disable) == EventActivityOptions.None)
        {
          switch (opcode)
          {
            case EventOpcode.Start:
              this.m_activityTracker.OnStart(this.m_name, this.m_eventData[eventId].Name, this.m_eventData[eventId].Descriptor.Task, ref empty1, ref empty2, this.m_eventData[eventId].ActivityOptions);
              break;
            case EventOpcode.Stop:
              this.m_activityTracker.OnStop(this.m_name, this.m_eventData[eventId].Name, this.m_eventData[eventId].Descriptor.Task, ref empty1);
              break;
          }
          if (empty1 != Guid.Empty)
            activityID = &empty1;
          if (empty2 != Guid.Empty)
            childActivityID = &empty2;
        }
        if (this.m_eventData[eventId].EnabledForETW || this.m_eventData[eventId].EnabledForEventPipe)
        {
          if (!this.SelfDescribingEvents)
          {
            if (!this.m_etwProvider.WriteEvent(ref this.m_eventData[eventId].Descriptor, this.m_eventData[eventId].EventHandle, activityID, childActivityID, args))
              this.ThrowEventSourceException(this.m_eventData[eventId].Name, (Exception) null);
            if (!this.m_eventPipeProvider.WriteEvent(ref this.m_eventData[eventId].Descriptor, this.m_eventData[eventId].EventHandle, activityID, childActivityID, args))
              this.ThrowEventSourceException(this.m_eventData[eventId].Name, (Exception) null);
          }
          else
          {
            TraceLoggingEventTypes eventTypes = this.m_eventData[eventId].TraceLoggingEventTypes;
            if (eventTypes == null)
            {
              eventTypes = new TraceLoggingEventTypes(this.m_eventData[eventId].Name, EventTags.None, this.m_eventData[eventId].Parameters);
              Interlocked.CompareExchange<TraceLoggingEventTypes>(ref this.m_eventData[eventId].TraceLoggingEventTypes, eventTypes, (TraceLoggingEventTypes) null);
            }
            EventSourceOptions options = new EventSourceOptions()
            {
              Keywords = (EventKeywords) this.m_eventData[eventId].Descriptor.Keywords,
              Level = (EventLevel) this.m_eventData[eventId].Descriptor.Level,
              Opcode = (EventOpcode) this.m_eventData[eventId].Descriptor.Opcode
            };
            this.WriteMultiMerge(this.m_eventData[eventId].Name, ref options, eventTypes, activityID, childActivityID, args);
          }
        }
        if (this.m_Dispatchers == null || !this.m_eventData[eventId].EnabledForAnyListener)
          return;
        if (LocalAppContextSwitches.PreserveEventListnerObjectIdentity)
        {
          this.WriteToAllListeners(eventId, (uint*) null, (DateTime*) null, activityID, childActivityID, args);
        }
        else
        {
          object[] objArray = this.SerializeEventArgs(eventId, args);
          this.WriteToAllListeners(eventId, (uint*) null, (DateTime*) null, activityID, childActivityID, objArray);
        }
      }
      catch (Exception ex)
      {
        if (ex is EventSourceException)
          throw;
        else
          this.ThrowEventSourceException(this.m_eventData[eventId].Name, ex);
      }
    }

    private object[] SerializeEventArgs(int eventId, object[] args)
    {
      TraceLoggingEventTypes loggingEventTypes = this.m_eventData[eventId].TraceLoggingEventTypes;
      if (loggingEventTypes == null)
      {
        loggingEventTypes = new TraceLoggingEventTypes(this.m_eventData[eventId].Name, EventTags.None, this.m_eventData[eventId].Parameters);
        Interlocked.CompareExchange<TraceLoggingEventTypes>(ref this.m_eventData[eventId].TraceLoggingEventTypes, loggingEventTypes, (TraceLoggingEventTypes) null);
      }
      object[] objArray = new object[loggingEventTypes.typeInfos.Length];
      for (int index = 0; index < loggingEventTypes.typeInfos.Length; ++index)
        objArray[index] = loggingEventTypes.typeInfos[index].GetData(args[index]);
      return objArray;
    }

    private void LogEventArgsMismatches(ParameterInfo[] infos, object[] args)
    {
      bool flag = args.Length == infos.Length;
      for (int index = 0; flag && index < args.Length; ++index)
      {
        Type parameterType = infos[index].ParameterType;
        Type type = args[index]?.GetType();
        if (args[index] != null && !parameterType.IsAssignableFrom(type) || args[index] == null && (!parameterType.IsGenericType || !(parameterType.GetGenericTypeDefinition() == typeof (Nullable<>))) && parameterType.IsValueType)
        {
          flag = false;
          break;
        }
      }
      if (flag)
        return;
      Debugger.Log(0, (string) null, SR.EventSource_VarArgsParameterMismatch + "\r\n");
    }

    private unsafe void WriteToAllListeners(
      int eventId,
      Guid* activityID,
      Guid* childActivityID,
      int eventDataCount,
      EventSource.EventData* data)
    {
      int val1 = this.GetParameterCount(this.m_eventData[eventId]);
      int num = 0;
      for (int parameterId = 0; parameterId < val1; ++parameterId)
      {
        if (this.GetDataType(this.m_eventData[eventId], parameterId) == typeof (byte[]))
          num += 2;
        else
          ++num;
      }
      if (eventDataCount != num)
      {
        this.ReportOutOfBandMessage(SR.Format(SR.EventSource_EventParametersMismatch, (object) eventId, (object) eventDataCount, (object) val1), true);
        val1 = Math.Min(val1, eventDataCount);
      }
      object[] objArray = new object[val1];
      EventSource.EventData* data1 = data;
      for (int parameterId = 0; parameterId < val1; ++parameterId)
        objArray[parameterId] = this.DecodeObject(eventId, parameterId, ref data1);
      this.WriteToAllListeners(eventId, (uint*) null, (DateTime*) null, activityID, childActivityID, objArray);
    }

    internal unsafe void WriteToAllListeners(
      int eventId,
      uint* osThreadId,
      DateTime* timeStamp,
      Guid* activityID,
      Guid* childActivityID,
      params object[] args)
    {
      EventWrittenEventArgs eventCallbackArgs = new EventWrittenEventArgs(this);
      eventCallbackArgs.EventId = eventId;
      if ((IntPtr) osThreadId != IntPtr.Zero)
        eventCallbackArgs.OSThreadId = (long) (int) *osThreadId;
      if ((IntPtr) timeStamp != IntPtr.Zero)
        eventCallbackArgs.TimeStamp = *timeStamp;
      if ((IntPtr) activityID != IntPtr.Zero)
        eventCallbackArgs.ActivityId = *activityID;
      if ((IntPtr) childActivityID != IntPtr.Zero)
        eventCallbackArgs.RelatedActivityId = *childActivityID;
      eventCallbackArgs.EventName = this.m_eventData[eventId].Name;
      eventCallbackArgs.Message = this.m_eventData[eventId].Message;
      eventCallbackArgs.Payload = new ReadOnlyCollection<object>((IList<object>) args);
      this.DispatchToAllListeners(eventId, childActivityID, eventCallbackArgs);
    }

    private unsafe void DispatchToAllListeners(
      int eventId,
      Guid* childActivityID,
      EventWrittenEventArgs eventCallbackArgs)
    {
      Exception innerException = (Exception) null;
      for (EventDispatcher eventDispatcher = this.m_Dispatchers; eventDispatcher != null; eventDispatcher = eventDispatcher.m_Next)
      {
        if (eventId == -1 || eventDispatcher.m_EventEnabled[eventId])
        {
          try
          {
            eventDispatcher.m_Listener.OnEventWritten(eventCallbackArgs);
          }
          catch (Exception ex)
          {
            this.ReportOutOfBandMessage("ERROR: Exception during EventSource.OnEventWritten: " + ex.Message, false);
            innerException = ex;
          }
        }
      }
      if (innerException != null)
        throw new EventSourceException(innerException);
    }

    private unsafe void WriteEventString(EventLevel level, long keywords, string msgString)
    {
      if (this.m_etwProvider == null)
        return;
      string str = "EventSourceMessage";
      if (this.SelfDescribingEvents)
      {
        EventSourceOptions options = new EventSourceOptions()
        {
          Keywords = (EventKeywords) keywords,
          Level = level
        };
        var data = new{ message = msgString };
        TraceLoggingEventTypes eventTypes = new TraceLoggingEventTypes(str, EventTags.None, new Type[1]
        {
          data.GetType()
        });
        this.WriteMultiMergeInner(str, ref options, eventTypes, (Guid*) IntPtr.Zero, (Guid*) IntPtr.Zero, (object) data);
      }
      else
      {
        if (this.m_rawManifest == null && this.m_outOfBandMessageCount == (byte) 1)
        {
          ManifestBuilder manifestBuilder = new ManifestBuilder(this.Name, this.Guid, this.Name, (ResourceManager) null, EventManifestOptions.None);
          manifestBuilder.StartEvent(str, new EventAttribute(0)
          {
            Level = EventLevel.LogAlways,
            Task = (EventTask) 65534
          });
          manifestBuilder.AddEventParameter(typeof (string), "message");
          manifestBuilder.EndEvent();
          this.SendManifest(manifestBuilder.CreateManifest());
        }
        IntPtr num;
        if (msgString == null)
        {
          num = IntPtr.Zero;
        }
        else
        {
          fixed (char* chPtr = &msgString.GetPinnableReference())
            num = (IntPtr) chPtr;
        }
        char* chPtr1 = (char*) num;
        EventDescriptor eventDescriptor = new EventDescriptor(0, (byte) 0, (byte) 0, (byte) level, (byte) 0, 0, keywords);
        this.m_etwProvider.WriteEvent(ref eventDescriptor, IntPtr.Zero, (Guid*) null, (Guid*) null, 1, (IntPtr) (void*) &new EventProvider.EventData()
        {
          Ptr = (ulong) chPtr1,
          Size = (uint) (2 * (msgString.Length + 1)),
          Reserved = 0U
        });
        // ISSUE: fixed variable is out of scope
        // ISSUE: __unpin statement
        __unpin(chPtr);
      }
    }

    private void WriteStringToAllListeners(string eventName, string msg)
    {
      EventWrittenEventArgs eventData = new EventWrittenEventArgs(this);
      eventData.EventId = 0;
      eventData.Message = msg;
      eventData.Payload = new ReadOnlyCollection<object>((IList<object>) new List<object>()
      {
        (object) msg
      });
      eventData.PayloadNames = new ReadOnlyCollection<string>((IList<string>) new List<string>()
      {
        "message"
      });
      eventData.EventName = eventName;
      for (EventDispatcher eventDispatcher = this.m_Dispatchers; eventDispatcher != null; eventDispatcher = eventDispatcher.m_Next)
      {
        bool flag = false;
        if (eventDispatcher.m_EventEnabled == null)
        {
          flag = true;
        }
        else
        {
          for (int index = 0; index < eventDispatcher.m_EventEnabled.Length; ++index)
          {
            if (eventDispatcher.m_EventEnabled[index])
            {
              flag = true;
              break;
            }
          }
        }
        try
        {
          if (flag)
            eventDispatcher.m_Listener.OnEventWritten(eventData);
        }
        catch
        {
        }
      }
    }

    private bool IsEnabledByDefault(
      int eventNum,
      bool enable,
      EventLevel currentLevel,
      EventKeywords currentMatchAnyKeyword)
    {
      if (!enable)
        return false;
      EventLevel level = (EventLevel) this.m_eventData[eventNum].Descriptor.Level;
      EventKeywords eventKeywords = (EventKeywords) (this.m_eventData[eventNum].Descriptor.Keywords & ~(long) SessionMask.All.ToEventKeywords());
      EventChannel channel = (EventChannel) this.m_eventData[eventNum].Descriptor.Channel;
      return this.IsEnabledCommon(enable, currentLevel, currentMatchAnyKeyword, level, eventKeywords, channel);
    }

    private bool IsEnabledCommon(
      bool enabled,
      EventLevel currentLevel,
      EventKeywords currentMatchAnyKeyword,
      EventLevel eventLevel,
      EventKeywords eventKeywords,
      EventChannel eventChannel)
    {
      if (!enabled || currentLevel != EventLevel.LogAlways && currentLevel < eventLevel)
        return false;
      if (currentMatchAnyKeyword != EventKeywords.None && eventKeywords != EventKeywords.None)
      {
        if (eventChannel != EventChannel.None && this.m_channelData != null && (EventChannel) this.m_channelData.Length > eventChannel)
        {
          EventKeywords eventKeywords1 = (EventKeywords) this.m_channelData[(int) eventChannel] | eventKeywords;
          if (eventKeywords1 != EventKeywords.None && (eventKeywords1 & currentMatchAnyKeyword) == EventKeywords.None)
            return false;
        }
        else if ((eventKeywords & currentMatchAnyKeyword) == EventKeywords.None)
          return false;
      }
      return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ThrowEventSourceException(string eventName, Exception innerEx = null)
    {
      if (EventSource.m_EventSourceExceptionRecurenceCount > (byte) 0)
        return;
      try
      {
        ++EventSource.m_EventSourceExceptionRecurenceCount;
        string msg = "EventSourceException";
        if (eventName != null)
          msg = msg + " while processing event \"" + eventName + "\"";
        switch (EventProvider.GetLastWriteEventError())
        {
          case EventProvider.WriteEventErrorCode.NoFreeBuffers:
            this.ReportOutOfBandMessage(msg + ": " + SR.EventSource_NoFreeBuffers, true);
            if (!this.ThrowOnEventWriteErrors)
              break;
            throw new EventSourceException(SR.EventSource_NoFreeBuffers, innerEx);
          case EventProvider.WriteEventErrorCode.EventTooBig:
            this.ReportOutOfBandMessage(msg + ": " + SR.EventSource_EventTooBig, true);
            if (!this.ThrowOnEventWriteErrors)
              break;
            throw new EventSourceException(SR.EventSource_EventTooBig, innerEx);
          case EventProvider.WriteEventErrorCode.NullInput:
            this.ReportOutOfBandMessage(msg + ": " + SR.EventSource_NullInput, true);
            if (!this.ThrowOnEventWriteErrors)
              break;
            throw new EventSourceException(SR.EventSource_NullInput, innerEx);
          case EventProvider.WriteEventErrorCode.TooManyArgs:
            this.ReportOutOfBandMessage(msg + ": " + SR.EventSource_TooManyArgs, true);
            if (!this.ThrowOnEventWriteErrors)
              break;
            throw new EventSourceException(SR.EventSource_TooManyArgs, innerEx);
          default:
            if (innerEx != null)
            {
              innerEx = innerEx.GetBaseException();
              string[] strArray = new string[5]
              {
                msg,
                ": ",
                null,
                null,
                null
              };
              Type type = innerEx.GetType();
              strArray[2] = (object) type != null ? type.ToString() : (string) null;
              strArray[3] = ":";
              strArray[4] = innerEx.Message;
              this.ReportOutOfBandMessage(string.Concat(strArray), true);
            }
            else
              this.ReportOutOfBandMessage(msg, true);
            if (!this.ThrowOnEventWriteErrors)
              break;
            throw new EventSourceException(innerEx);
        }
      }
      finally
      {
        --EventSource.m_EventSourceExceptionRecurenceCount;
      }
    }

    private void ValidateEventOpcodeForTransfer(
      ref EventSource.EventMetadata eventData,
      string eventName)
    {
      if (eventData.Descriptor.Opcode == (byte) 9 || eventData.Descriptor.Opcode == (byte) 240 || eventData.Descriptor.Opcode == (byte) 1)
        return;
      this.ThrowEventSourceException(eventName, (Exception) null);
    }

    internal static EventOpcode GetOpcodeWithDefault(
      EventOpcode opcode,
      string eventName)
    {
      if (opcode == EventOpcode.Info && eventName != null)
      {
        if (eventName.EndsWith("Start", StringComparison.Ordinal))
          return EventOpcode.Start;
        if (eventName.EndsWith("Stop", StringComparison.Ordinal))
          return EventOpcode.Stop;
      }
      return opcode;
    }

    private int GetParameterCount(EventSource.EventMetadata eventData)
    {
      return eventData.Parameters.Length;
    }

    private Type GetDataType(EventSource.EventMetadata eventData, int parameterId)
    {
      return eventData.Parameters[parameterId].ParameterType;
    }

    internal void SendCommand(
      EventListener listener,
      EventProviderType eventProviderType,
      int perEventSourceSessionId,
      int etwSessionId,
      EventCommand command,
      bool enable,
      EventLevel level,
      EventKeywords matchAnyKeyword,
      IDictionary<string, string> commandArguments)
    {
      EventCommandEventArgs commandArgs = new EventCommandEventArgs(command, commandArguments, this, listener, eventProviderType, perEventSourceSessionId, etwSessionId, enable, level, matchAnyKeyword);
      lock (EventListener.EventListenersLock)
      {
        if (this.m_completelyInited)
        {
          this.m_deferredCommands = (EventCommandEventArgs) null;
          this.DoCommand(commandArgs);
        }
        else if (this.m_deferredCommands == null)
        {
          this.m_deferredCommands = commandArgs;
        }
        else
        {
          EventCommandEventArgs commandEventArgs = this.m_deferredCommands;
          while (commandEventArgs.nextCommand != null)
            commandEventArgs = commandEventArgs.nextCommand;
          commandEventArgs.nextCommand = commandArgs;
        }
      }
    }

    internal void DoCommand(EventCommandEventArgs commandArgs)
    {
      if (this.m_etwProvider == null || this.m_eventPipeProvider == null)
        return;
      this.m_outOfBandMessageCount = (byte) 0;
      bool flag1 = commandArgs.perEventSourceSessionId > 0 && commandArgs.perEventSourceSessionId <= 4;
      try
      {
        this.EnsureDescriptorsInitialized();
        commandArgs.dispatcher = this.GetDispatcher(commandArgs.listener);
        if (commandArgs.dispatcher == null && commandArgs.listener != null)
          throw new ArgumentException(SR.EventSource_ListenerNotFound);
        if (commandArgs.Arguments == null)
          commandArgs.Arguments = (IDictionary<string, string>) new Dictionary<string, string>();
        if (commandArgs.Command == EventCommand.Update)
        {
          for (int index = 0; index < this.m_eventData.Length; ++index)
            this.EnableEventForDispatcher(commandArgs.dispatcher, commandArgs.eventProviderType, index, this.IsEnabledByDefault(index, commandArgs.enable, commandArgs.level, commandArgs.matchAnyKeyword));
          if (commandArgs.enable)
          {
            if (!this.m_eventSourceEnabled)
            {
              this.m_level = commandArgs.level;
              this.m_matchAnyKeyword = commandArgs.matchAnyKeyword;
            }
            else
            {
              if (commandArgs.level > this.m_level)
                this.m_level = commandArgs.level;
              if (commandArgs.matchAnyKeyword == EventKeywords.None)
                this.m_matchAnyKeyword = EventKeywords.None;
              else if (this.m_matchAnyKeyword != EventKeywords.None)
                this.m_matchAnyKeyword |= commandArgs.matchAnyKeyword;
            }
          }
          bool flag2 = commandArgs.perEventSourceSessionId >= 0;
          if (commandArgs.perEventSourceSessionId == 0 && !commandArgs.enable)
            flag2 = false;
          if (commandArgs.listener == null)
          {
            if (!flag2)
              commandArgs.perEventSourceSessionId = -commandArgs.perEventSourceSessionId;
            --commandArgs.perEventSourceSessionId;
          }
          commandArgs.Command = flag2 ? EventCommand.Enable : EventCommand.Disable;
          if (flag2 && commandArgs.dispatcher == null && !this.SelfDescribingEvents)
            this.SendManifest(this.m_rawManifest);
          if (commandArgs.enable)
            this.m_eventSourceEnabled = true;
          this.OnEventCommand(commandArgs);
          EventHandler<EventCommandEventArgs> eventCommandExecuted = this.m_eventCommandExecuted;
          if (eventCommandExecuted != null)
            eventCommandExecuted((object) this, commandArgs);
          if (commandArgs.enable)
            return;
          for (int index = 0; index < this.m_eventData.Length; ++index)
          {
            bool flag3 = false;
            for (EventDispatcher eventDispatcher = this.m_Dispatchers; eventDispatcher != null; eventDispatcher = eventDispatcher.m_Next)
            {
              if (eventDispatcher.m_EventEnabled[index])
              {
                flag3 = true;
                break;
              }
            }
            this.m_eventData[index].EnabledForAnyListener = flag3;
          }
          if (this.AnyEventEnabled())
            return;
          this.m_level = EventLevel.LogAlways;
          this.m_matchAnyKeyword = EventKeywords.None;
          this.m_eventSourceEnabled = false;
        }
        else
        {
          this.OnEventCommand(commandArgs);
          EventHandler<EventCommandEventArgs> eventCommandExecuted = this.m_eventCommandExecuted;
          if (eventCommandExecuted == null)
            return;
          eventCommandExecuted((object) this, commandArgs);
        }
      }
      catch (Exception ex)
      {
        this.ReportOutOfBandMessage("ERROR: Exception in Command Processing for EventSource " + this.Name + ": " + ex.Message, true);
      }
    }

    internal bool EnableEventForDispatcher(
      EventDispatcher dispatcher,
      EventProviderType eventProviderType,
      int eventId,
      bool value)
    {
      if (dispatcher == null)
      {
        if (eventId >= this.m_eventData.Length)
          return false;
        if (this.m_etwProvider != null && eventProviderType == EventProviderType.ETW)
          this.m_eventData[eventId].EnabledForETW = value;
        if (this.m_eventPipeProvider != null && eventProviderType == EventProviderType.EventPipe)
          this.m_eventData[eventId].EnabledForEventPipe = value;
      }
      else
      {
        if (eventId >= dispatcher.m_EventEnabled.Length)
          return false;
        dispatcher.m_EventEnabled[eventId] = value;
        if (value)
          this.m_eventData[eventId].EnabledForAnyListener = true;
      }
      return true;
    }

    private bool AnyEventEnabled()
    {
      for (int index = 0; index < this.m_eventData.Length; ++index)
      {
        if (this.m_eventData[index].EnabledForETW || this.m_eventData[index].EnabledForAnyListener || this.m_eventData[index].EnabledForEventPipe)
          return true;
      }
      return false;
    }

    private bool IsDisposed
    {
      get
      {
        return this.m_eventSourceDisposed;
      }
    }

    private void EnsureDescriptorsInitialized()
    {
      if (this.m_eventData == null)
      {
        Guid eventSourceGuid = Guid.Empty;
        string eventSourceName = (string) null;
        EventSource.EventMetadata[] eventData = (EventSource.EventMetadata[]) null;
        byte[] manifestBytes = (byte[]) null;
        this.GetMetadata(out eventSourceGuid, out eventSourceName, out eventData, out manifestBytes);
        if (eventSourceGuid.Equals(Guid.Empty) || eventSourceName == null || (eventData == null || manifestBytes == null))
        {
          this.m_rawManifest = EventSource.CreateManifestAndDescriptors(this.GetType(), this.Name, this, EventManifestOptions.None);
        }
        else
        {
          this.m_name = eventSourceName;
          this.m_guid = eventSourceGuid;
          this.m_eventData = eventData;
          this.m_rawManifest = manifestBytes;
        }
        foreach (WeakReference eventSource in EventListener.s_EventSources)
        {
          if (eventSource.Target is EventSource target && target.Guid == this.m_guid && (!target.IsDisposed && target != this))
            throw new ArgumentException(SR.Format(SR.EventSource_EventSourceGuidInUse, (object) this.m_guid));
        }
        for (EventDispatcher eventDispatcher = this.m_Dispatchers; eventDispatcher != null; eventDispatcher = eventDispatcher.m_Next)
        {
          if (eventDispatcher.m_EventEnabled == null)
            eventDispatcher.m_EventEnabled = new bool[this.m_eventData.Length];
        }
        this.DefineEventPipeEvents();
      }
      if (EventSource.s_currentPid != 0U)
        return;
      EventSource.s_currentPid = Interop.GetCurrentProcessId();
    }

    private unsafe bool SendManifest(byte[] rawManifest)
    {
      bool flag = true;
      if (rawManifest == null)
        return false;
      fixed (byte* numPtr = rawManifest)
      {
        EventDescriptor eventDescriptor = new EventDescriptor(65534, (byte) 1, (byte) 0, (byte) 0, (byte) 254, 65534, 72057594037927935L);
        ManifestEnvelope manifestEnvelope = new ManifestEnvelope();
        manifestEnvelope.Format = ManifestEnvelope.ManifestFormats.SimpleXmlFormat;
        manifestEnvelope.MajorVersion = (byte) 1;
        manifestEnvelope.MinorVersion = (byte) 0;
        manifestEnvelope.Magic = (byte) 91;
        int length = rawManifest.Length;
        manifestEnvelope.ChunkNumber = (ushort) 0;
        EventProvider.EventData* eventDataPtr = stackalloc EventProvider.EventData[2];
        eventDataPtr->Ptr = (ulong) &manifestEnvelope;
        eventDataPtr->Size = (uint) sizeof (ManifestEnvelope);
        eventDataPtr->Reserved = 0U;
        eventDataPtr[1].Ptr = (ulong) numPtr;
        eventDataPtr[1].Reserved = 0U;
        int val2 = 65280;
label_3:
        manifestEnvelope.TotalChunks = (ushort) ((length + (val2 - 1)) / val2);
        while (length > 0)
        {
          eventDataPtr[1].Size = (uint) Math.Min(length, val2);
          if (this.m_etwProvider != null && !this.m_etwProvider.WriteEvent(ref eventDescriptor, IntPtr.Zero, (Guid*) null, (Guid*) null, 2, (IntPtr) (void*) eventDataPtr))
          {
            if (EventProvider.GetLastWriteEventError() == EventProvider.WriteEventErrorCode.EventTooBig && manifestEnvelope.ChunkNumber == (ushort) 0 && val2 > 256)
            {
              val2 /= 2;
              goto label_3;
            }
            else
            {
              flag = false;
              if (this.ThrowOnEventWriteErrors)
              {
                this.ThrowEventSourceException(nameof (SendManifest), (Exception) null);
                break;
              }
              break;
            }
          }
          else
          {
            length -= val2;
            eventDataPtr[1].Ptr += (ulong) (uint) val2;
            ++manifestEnvelope.ChunkNumber;
            if ((int) manifestEnvelope.ChunkNumber % 5 == 0)
              Thread.Sleep(15);
          }
        }
      }
      return flag;
    }

    internal static Attribute GetCustomAttributeHelper(
      MemberInfo member,
      Type attributeType,
      EventManifestOptions flags = EventManifestOptions.None)
    {
      if (!member.Module.Assembly.ReflectionOnly() && (flags & EventManifestOptions.AllowEventSourceOverride) == EventManifestOptions.None)
      {
        Attribute attribute = (Attribute) null;
        object[] customAttributes = member.GetCustomAttributes(attributeType, false);
        int index = 0;
        if (index < customAttributes.Length)
          attribute = (Attribute) customAttributes[index];
        return attribute;
      }
      string fullName = attributeType.FullName;
      foreach (CustomAttributeData customAttribute in (IEnumerable<CustomAttributeData>) CustomAttributeData.GetCustomAttributes(member))
      {
        if (EventSource.AttributeTypeNamesMatch(attributeType, customAttribute.Constructor.ReflectedType))
        {
          Attribute attribute = (Attribute) null;
          CustomAttributeTypedArgument attributeTypedArgument;
          if (customAttribute.ConstructorArguments.Count == 1)
          {
            Type type = attributeType;
            object[] objArray = new object[1];
            attributeTypedArgument = customAttribute.ConstructorArguments[0];
            objArray[0] = attributeTypedArgument.Value;
            attribute = (Attribute) Activator.CreateInstance(type, objArray);
          }
          else if (customAttribute.ConstructorArguments.Count == 0)
            attribute = (Attribute) Activator.CreateInstance(attributeType);
          if (attribute != null)
          {
            Type type = attribute.GetType();
            foreach (CustomAttributeNamedArgument namedArgument in (IEnumerable<CustomAttributeNamedArgument>) customAttribute.NamedArguments)
            {
              PropertyInfo property = type.GetProperty(namedArgument.MemberInfo.Name, BindingFlags.Instance | BindingFlags.Public);
              attributeTypedArgument = namedArgument.TypedValue;
              object obj = attributeTypedArgument.Value;
              if (property.PropertyType.IsEnum)
              {
                string str = obj.ToString();
                obj = Enum.Parse(property.PropertyType, str);
              }
              property.SetValue((object) attribute, obj, (object[]) null);
            }
            return attribute;
          }
        }
      }
      return (Attribute) null;
    }

    private static bool AttributeTypeNamesMatch(Type attributeType, Type reflectedAttributeType)
    {
      if (attributeType == reflectedAttributeType || string.Equals(attributeType.FullName, reflectedAttributeType.FullName, StringComparison.Ordinal))
        return true;
      return string.Equals(attributeType.Name, reflectedAttributeType.Name, StringComparison.Ordinal) && attributeType.Namespace.EndsWith("Diagnostics.Tracing", StringComparison.Ordinal) && reflectedAttributeType.Namespace.EndsWith("Diagnostics.Tracing", StringComparison.Ordinal);
    }

    private static Type GetEventSourceBaseType(
      Type eventSourceType,
      bool allowEventSourceOverride,
      bool reflectionOnly)
    {
      Type type = eventSourceType;
      if (type.BaseType() == (Type) null)
        return (Type) null;
      do
      {
        type = type.BaseType();
      }
      while (type != (Type) null && type.IsAbstract());
      if (type != (Type) null)
      {
        if (!allowEventSourceOverride)
        {
          if (reflectionOnly && type.FullName != typeof (EventSource).FullName || !reflectionOnly && type != typeof (EventSource))
            return (Type) null;
        }
        else if (type.Name != nameof (EventSource))
          return (Type) null;
      }
      return type;
    }

    private static byte[] CreateManifestAndDescriptors(
      Type eventSourceType,
      string eventSourceDllName,
      EventSource source,
      EventManifestOptions flags = EventManifestOptions.None)
    {
      ManifestBuilder manifest = (ManifestBuilder) null;
      bool flag1 = source == null || !source.SelfDescribingEvents;
      Exception innerException = (Exception) null;
      byte[] numArray = (byte[]) null;
      if (eventSourceType.IsAbstract() && (flags & EventManifestOptions.Strict) == EventManifestOptions.None)
        return (byte[]) null;
      try
      {
        MethodInfo[] methods = eventSourceType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        int eventId = 1;
        EventSource.EventMetadata[] eventData = (EventSource.EventMetadata[]) null;
        Dictionary<string, string> eventsByName = (Dictionary<string, string>) null;
        if (source != null || (flags & EventManifestOptions.Strict) != EventManifestOptions.None)
        {
          eventData = new EventSource.EventMetadata[methods.Length + 1];
          eventData[0].Name = "";
        }
        ResourceManager resources = (ResourceManager) null;
        EventSourceAttribute customAttributeHelper = (EventSourceAttribute) EventSource.GetCustomAttributeHelper((MemberInfo) eventSourceType, typeof (EventSourceAttribute), flags);
        if (customAttributeHelper != null && customAttributeHelper.LocalizationResources != null)
          resources = new ResourceManager(customAttributeHelper.LocalizationResources, eventSourceType.Assembly());
        manifest = new ManifestBuilder(EventSource.GetName(eventSourceType, flags), EventSource.GetGuid(eventSourceType), eventSourceDllName, resources, flags);
        manifest.StartEvent("EventSourceMessage", new EventAttribute(0)
        {
          Level = EventLevel.LogAlways,
          Task = (EventTask) 65534
        });
        manifest.AddEventParameter(typeof (string), "message");
        manifest.EndEvent();
        if ((flags & EventManifestOptions.Strict) != EventManifestOptions.None)
        {
          if (!(EventSource.GetEventSourceBaseType(eventSourceType, (uint) (flags & EventManifestOptions.AllowEventSourceOverride) > 0U, eventSourceType.Assembly().ReflectionOnly()) != (Type) null))
            manifest.ManifestError(SR.EventSource_TypeMustDeriveFromEventSource, false);
          if (!eventSourceType.IsAbstract() && !eventSourceType.IsSealed())
            manifest.ManifestError(SR.EventSource_TypeMustBeSealedOrAbstract, false);
        }
        string[] strArray = new string[3]
        {
          "Keywords",
          "Tasks",
          "Opcodes"
        };
        foreach (string str in strArray)
        {
          Type nestedType = eventSourceType.GetNestedType(str);
          if (nestedType != (Type) null)
          {
            if (eventSourceType.IsAbstract())
            {
              manifest.ManifestError(SR.Format(SR.EventSource_AbstractMustNotDeclareKTOC, (object) nestedType.Name), false);
            }
            else
            {
              foreach (FieldInfo field in nestedType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                EventSource.AddProviderEnumKind(manifest, field, str);
            }
          }
        }
        manifest.AddKeyword("Session3", 17592186044416UL);
        manifest.AddKeyword("Session2", 35184372088832UL);
        manifest.AddKeyword("Session1", 70368744177664UL);
        manifest.AddKeyword("Session0", 140737488355328UL);
        if (eventSourceType != typeof (EventSource))
        {
          for (int index1 = 0; index1 < methods.Length; ++index1)
          {
            MethodInfo method = methods[index1];
            ParameterInfo[] parameters = method.GetParameters();
            EventAttribute eventAttribute = (EventAttribute) EventSource.GetCustomAttributeHelper((MemberInfo) method, typeof (EventAttribute), flags);
            if (!method.IsStatic)
            {
              if (eventSourceType.IsAbstract())
              {
                if (eventAttribute != null)
                  manifest.ManifestError(SR.Format(SR.EventSource_AbstractMustNotDeclareEventMethods, (object) method.Name, (object) eventAttribute.EventId), false);
              }
              else
              {
                if (eventAttribute == null)
                {
                  if (!(method.ReturnType != typeof (void)) && !method.IsVirtual && EventSource.GetCustomAttributeHelper((MemberInfo) method, typeof (NonEventAttribute), flags) == null)
                    eventAttribute = new EventAttribute(eventId);
                  else
                    continue;
                }
                else if (eventAttribute.EventId <= 0)
                {
                  manifest.ManifestError(SR.Format(SR.EventSource_NeedPositiveId, (object) method.Name), true);
                  continue;
                }
                if (method.Name.LastIndexOf('.') >= 0)
                  manifest.ManifestError(SR.Format(SR.EventSource_EventMustNotBeExplicitImplementation, (object) method.Name, (object) eventAttribute.EventId), false);
                ++eventId;
                string name = method.Name;
                if (eventAttribute.Opcode == EventOpcode.Info)
                {
                  bool flag2 = eventAttribute.Task == EventTask.None;
                  if (flag2)
                    eventAttribute.Task = (EventTask) (65534 - eventAttribute.EventId);
                  if (!eventAttribute.IsOpcodeSet)
                    eventAttribute.Opcode = EventSource.GetOpcodeWithDefault(EventOpcode.Info, name);
                  if (flag2)
                  {
                    if (eventAttribute.Opcode == EventOpcode.Start)
                    {
                      string str = name.Substring(0, name.Length - "Start".Length);
                      if (string.Compare(name, 0, str, 0, str.Length) == 0 && string.Compare(name, str.Length, "Start", 0, Math.Max(name.Length - str.Length, "Start".Length)) == 0)
                        manifest.AddTask(str, (int) eventAttribute.Task);
                    }
                    else if (eventAttribute.Opcode == EventOpcode.Stop)
                    {
                      int index2 = eventAttribute.EventId - 1;
                      if (eventData != null && index2 < eventData.Length)
                      {
                        EventSource.EventMetadata eventMetadata = eventData[index2];
                        string strB = name.Substring(0, name.Length - "Stop".Length);
                        if (eventMetadata.Descriptor.Opcode == (byte) 1 && string.Compare(eventMetadata.Name, 0, strB, 0, strB.Length) == 0 && string.Compare(eventMetadata.Name, strB.Length, "Start", 0, Math.Max(eventMetadata.Name.Length - strB.Length, "Start".Length)) == 0)
                        {
                          eventAttribute.Task = (EventTask) eventMetadata.Descriptor.Task;
                          flag2 = false;
                        }
                      }
                      if (flag2 && (flags & EventManifestOptions.Strict) != EventManifestOptions.None)
                        throw new ArgumentException(SR.EventSource_StopsFollowStarts);
                    }
                  }
                }
                bool hasRelatedActivityID = EventSource.RemoveFirstArgIfRelatedActivityId(ref parameters);
                if (source == null || !source.SelfDescribingEvents)
                {
                  manifest.StartEvent(name, eventAttribute);
                  for (int index2 = 0; index2 < parameters.Length; ++index2)
                    manifest.AddEventParameter(parameters[index2].ParameterType, parameters[index2].Name);
                  manifest.EndEvent();
                }
                if (source != null || (flags & EventManifestOptions.Strict) != EventManifestOptions.None)
                {
                  EventSource.DebugCheckEvent(ref eventsByName, eventData, method, eventAttribute, manifest, flags);
                  if (eventAttribute.Channel != EventChannel.None)
                    eventAttribute.Keywords |= (EventKeywords) manifest.GetChannelKeyword(eventAttribute.Channel, (ulong) eventAttribute.Keywords);
                  string key = "event_" + name;
                  string localizedMessage = manifest.GetLocalizedMessage(key, CultureInfo.CurrentUICulture, false);
                  if (localizedMessage != null)
                    eventAttribute.Message = localizedMessage;
                  EventSource.AddEventDescriptor(ref eventData, name, eventAttribute, parameters, hasRelatedActivityID);
                }
              }
            }
          }
        }
        NameInfo.ReserveEventIDsBelow(eventId);
        if (source != null)
        {
          EventSource.TrimEventDescriptors(ref eventData);
          source.m_eventData = eventData;
          source.m_channelData = manifest.GetChannelData();
        }
        if (!eventSourceType.IsAbstract())
        {
          if (source != null)
          {
            if (source.SelfDescribingEvents)
              goto label_72;
          }
          flag1 = (flags & EventManifestOptions.OnlyIfNeededForRegistration) == EventManifestOptions.None || (uint) manifest.GetChannelData().Length > 0U;
          if (!flag1 && (flags & EventManifestOptions.Strict) == EventManifestOptions.None)
            return (byte[]) null;
          numArray = manifest.CreateManifest();
        }
      }
      catch (Exception ex)
      {
        if ((flags & EventManifestOptions.Strict) == EventManifestOptions.None)
          throw;
        else
          innerException = ex;
      }
label_72:
      if ((flags & EventManifestOptions.Strict) != EventManifestOptions.None && (manifest != null && manifest.Errors.Count > 0 || innerException != null))
      {
        string message = string.Empty;
        if (manifest != null && manifest.Errors.Count > 0)
        {
          bool flag2 = true;
          foreach (string error in (IEnumerable<string>) manifest.Errors)
          {
            if (!flag2)
              message += Environment.NewLine;
            flag2 = false;
            message += error;
          }
        }
        else
          message = "Unexpected error: " + innerException.Message;
        throw new ArgumentException(message, innerException);
      }
      return !flag1 ? (byte[]) null : numArray;
    }

    private static bool RemoveFirstArgIfRelatedActivityId(ref ParameterInfo[] args)
    {
      if (args.Length == 0 || !(args[0].ParameterType == typeof (Guid)) || !string.Equals(args[0].Name, "relatedActivityId", StringComparison.OrdinalIgnoreCase))
        return false;
      ParameterInfo[] parameterInfoArray = new ParameterInfo[args.Length - 1];
      Array.Copy((Array) args, 1, (Array) parameterInfoArray, 0, args.Length - 1);
      args = parameterInfoArray;
      return true;
    }

    private static void AddProviderEnumKind(
      ManifestBuilder manifest,
      FieldInfo staticField,
      string providerEnumKind)
    {
      bool flag = staticField.Module.Assembly.ReflectionOnly();
      Type fieldType = staticField.FieldType;
      if (!flag && fieldType == typeof (EventOpcode) || EventSource.AttributeTypeNamesMatch(fieldType, typeof (EventOpcode)))
      {
        if (!(providerEnumKind != "Opcodes"))
        {
          int rawConstantValue = (int) staticField.GetRawConstantValue();
          manifest.AddOpcode(staticField.Name, rawConstantValue);
          return;
        }
      }
      else if (!flag && fieldType == typeof (EventTask) || EventSource.AttributeTypeNamesMatch(fieldType, typeof (EventTask)))
      {
        if (!(providerEnumKind != "Tasks"))
        {
          int rawConstantValue = (int) staticField.GetRawConstantValue();
          manifest.AddTask(staticField.Name, rawConstantValue);
          return;
        }
      }
      else
      {
        if ((flag || !(fieldType == typeof (EventKeywords))) && !EventSource.AttributeTypeNamesMatch(fieldType, typeof (EventKeywords)))
          return;
        if (!(providerEnumKind != "Keywords"))
        {
          ulong rawConstantValue = (ulong) (long) staticField.GetRawConstantValue();
          manifest.AddKeyword(staticField.Name, rawConstantValue);
          return;
        }
      }
      manifest.ManifestError(SR.Format(SR.EventSource_EnumKindMismatch, (object) staticField.Name, (object) staticField.FieldType.Name, (object) providerEnumKind), false);
    }

    private static void AddEventDescriptor(
      [NotNull] ref EventSource.EventMetadata[] eventData,
      string eventName,
      EventAttribute eventAttribute,
      ParameterInfo[] eventParameters,
      bool hasRelatedActivityID)
    {
      if (eventData.Length <= eventAttribute.EventId)
      {
        EventSource.EventMetadata[] eventMetadataArray = new EventSource.EventMetadata[Math.Max(eventData.Length + 16, eventAttribute.EventId + 1)];
        Array.Copy((Array) eventData, 0, (Array) eventMetadataArray, 0, eventData.Length);
        eventData = eventMetadataArray;
      }
      eventData[eventAttribute.EventId].Descriptor = new EventDescriptor(eventAttribute.EventId, eventAttribute.Version, (byte) eventAttribute.Channel, (byte) eventAttribute.Level, (byte) eventAttribute.Opcode, (int) eventAttribute.Task, (long) (eventAttribute.Keywords | (EventKeywords) SessionMask.All.ToEventKeywords()));
      eventData[eventAttribute.EventId].Tags = eventAttribute.Tags;
      eventData[eventAttribute.EventId].Name = eventName;
      eventData[eventAttribute.EventId].Parameters = eventParameters;
      eventData[eventAttribute.EventId].Message = eventAttribute.Message;
      eventData[eventAttribute.EventId].ActivityOptions = eventAttribute.ActivityOptions;
      eventData[eventAttribute.EventId].HasRelatedActivityID = hasRelatedActivityID;
      eventData[eventAttribute.EventId].EventHandle = IntPtr.Zero;
    }

    private static void TrimEventDescriptors(ref EventSource.EventMetadata[] eventData)
    {
      int length = eventData.Length;
      while (0 < length)
      {
        --length;
        if (eventData[length].Descriptor.EventId != 0)
          break;
      }
      if (eventData.Length - length <= 2)
        return;
      EventSource.EventMetadata[] eventMetadataArray = new EventSource.EventMetadata[length + 1];
      Array.Copy((Array) eventData, 0, (Array) eventMetadataArray, 0, eventMetadataArray.Length);
      eventData = eventMetadataArray;
    }

    internal void AddListener(EventListener listener)
    {
      lock (EventListener.EventListenersLock)
      {
        bool[] eventEnabled = (bool[]) null;
        if (this.m_eventData != null)
          eventEnabled = new bool[this.m_eventData.Length];
        this.m_Dispatchers = new EventDispatcher(this.m_Dispatchers, eventEnabled, listener);
        listener.OnEventSourceCreated(this);
      }
    }

    private static void DebugCheckEvent(
      ref Dictionary<string, string> eventsByName,
      EventSource.EventMetadata[] eventData,
      MethodInfo method,
      EventAttribute eventAttribute,
      ManifestBuilder manifest,
      EventManifestOptions options)
    {
      int eventId = eventAttribute.EventId;
      string name = method.Name;
      int helperCallFirstArg = EventSource.GetHelperCallFirstArg(method);
      if (helperCallFirstArg >= 0 && eventId != helperCallFirstArg)
        manifest.ManifestError(SR.Format(SR.EventSource_MismatchIdToWriteEvent, (object) name, (object) eventId, (object) helperCallFirstArg), true);
      if (eventId < eventData.Length && eventData[eventId].Descriptor.EventId != 0)
        manifest.ManifestError(SR.Format(SR.EventSource_EventIdReused, (object) name, (object) eventId, (object) eventData[eventId].Name), true);
      for (int index = 0; index < eventData.Length; ++index)
      {
        if (eventData[index].Name != null && (EventTask) eventData[index].Descriptor.Task == eventAttribute.Task && (EventOpcode) eventData[index].Descriptor.Opcode == eventAttribute.Opcode)
        {
          manifest.ManifestError(SR.Format(SR.EventSource_TaskOpcodePairReused, (object) name, (object) eventId, (object) eventData[index].Name, (object) index), false);
          if ((options & EventManifestOptions.Strict) == EventManifestOptions.None)
            break;
        }
      }
      if (eventAttribute.Opcode != EventOpcode.Info)
      {
        bool flag = false;
        if (eventAttribute.Task == EventTask.None)
        {
          flag = true;
        }
        else
        {
          EventTask eventTask = (EventTask) (65534 - eventId);
          if (eventAttribute.Opcode != EventOpcode.Start && eventAttribute.Opcode != EventOpcode.Stop && eventAttribute.Task == eventTask)
            flag = true;
        }
        if (flag)
          manifest.ManifestError(SR.Format(SR.EventSource_EventMustHaveTaskIfNonDefaultOpcode, (object) name, (object) eventId), false);
      }
      if (eventsByName == null)
        eventsByName = new Dictionary<string, string>();
      if (eventsByName.ContainsKey(name))
        manifest.ManifestError(SR.Format(SR.EventSource_EventNameReused, (object) name), true);
      eventsByName[name] = name;
    }

    private static int GetHelperCallFirstArg(MethodInfo method)
    {
      byte[] ilAsByteArray = method.GetMethodBody().GetILAsByteArray();
      int num = -1;
      for (int index1 = 0; index1 < ilAsByteArray.Length; ++index1)
      {
        switch (ilAsByteArray[index1])
        {
          case 0:
          case 1:
          case 2:
          case 3:
          case 4:
          case 5:
          case 6:
          case 7:
          case 8:
          case 9:
          case 10:
          case 11:
          case 12:
          case 13:
          case 20:
          case 37:
          case 103:
          case 104:
          case 105:
          case 106:
          case 109:
          case 110:
          case 162:
            continue;
          case 14:
          case 16:
            ++index1;
            continue;
          case 21:
          case 22:
          case 23:
          case 24:
          case 25:
          case 26:
          case 27:
          case 28:
          case 29:
          case 30:
            if (index1 > 0 && ilAsByteArray[index1 - 1] == (byte) 2)
            {
              num = (int) ilAsByteArray[index1] - 22;
              continue;
            }
            continue;
          case 31:
            if (index1 > 0 && ilAsByteArray[index1 - 1] == (byte) 2)
              num = (int) ilAsByteArray[index1 + 1];
            ++index1;
            continue;
          case 32:
            index1 += 4;
            continue;
          case 40:
            index1 += 4;
            if (num >= 0)
            {
              for (int index2 = index1 + 1; index2 < ilAsByteArray.Length; ++index2)
              {
                if (ilAsByteArray[index2] == (byte) 42)
                  return num;
                if (ilAsByteArray[index2] != (byte) 0)
                  break;
              }
            }
            num = -1;
            continue;
          case 44:
          case 45:
            num = -1;
            ++index1;
            continue;
          case 57:
          case 58:
            num = -1;
            index1 += 4;
            continue;
          case 140:
          case 141:
            index1 += 4;
            continue;
          case 254:
            ++index1;
            if (index1 >= ilAsByteArray.Length || ilAsByteArray[index1] >= (byte) 6)
              break;
            continue;
        }
        return -1;
      }
      return -1;
    }

    internal void ReportOutOfBandMessage(string msg, bool flush)
    {
      try
      {
        Debugger.Log(0, (string) null, string.Format("EventSource Error: {0}{1}", (object) msg, (object) Environment.NewLine));
        if (this.m_outOfBandMessageCount < (byte) 15)
        {
          ++this.m_outOfBandMessageCount;
        }
        else
        {
          if (this.m_outOfBandMessageCount == (byte) 16)
            return;
          this.m_outOfBandMessageCount = (byte) 16;
          msg = "Reached message limit.   End of EventSource error messages.";
        }
        this.WriteEventString(EventLevel.LogAlways, -1L, msg);
        this.WriteStringToAllListeners("EventSourceMessage", msg);
      }
      catch (Exception ex)
      {
      }
    }

    private EventSourceSettings ValidateSettings(EventSourceSettings settings)
    {
      EventSourceSettings eventSourceSettings = EventSourceSettings.EtwManifestEventFormat | EventSourceSettings.EtwSelfDescribingEventFormat;
      if ((settings & eventSourceSettings) == eventSourceSettings)
        throw new ArgumentException(SR.EventSource_InvalidEventFormat, nameof (settings));
      if ((settings & eventSourceSettings) == EventSourceSettings.Default)
        settings |= EventSourceSettings.EtwSelfDescribingEventFormat;
      return settings;
    }

    private bool ThrowOnEventWriteErrors
    {
      get
      {
        return (uint) (this.m_config & EventSourceSettings.ThrowOnEventWriteErrors) > 0U;
      }
    }

    private bool SelfDescribingEvents
    {
      get
      {
        return (uint) (this.m_config & EventSourceSettings.EtwSelfDescribingEventFormat) > 0U;
      }
    }

    [NullableContext(1)]
    public EventSource(string eventSourceName)
      : this(eventSourceName, EventSourceSettings.EtwSelfDescribingEventFormat)
    {
    }

    [NullableContext(1)]
    public EventSource(string eventSourceName, EventSourceSettings config)
      : this(eventSourceName, config, (string[]) null)
    {
    }

    [NullableContext(1)]
    public EventSource(string eventSourceName, EventSourceSettings config, [Nullable(new byte[] {2, 1})] params string[] traits)
      : this(eventSourceName == null ? new Guid() : EventSource.GenerateGuidFromName(eventSourceName.ToUpperInvariant()), eventSourceName, config, traits)
    {
      if (eventSourceName == null)
        throw new ArgumentNullException(nameof (eventSourceName));
    }

    public unsafe void Write(string eventName)
    {
      if (!this.IsEnabled())
        return;
      EventSourceOptions options = new EventSourceOptions();
      this.WriteImpl(eventName, ref options, (object) null, (Guid*) null, (Guid*) null, SimpleEventTypes<EmptyStruct>.Instance);
    }

    public unsafe void Write(string eventName, EventSourceOptions options)
    {
      if (!this.IsEnabled())
        return;
      this.WriteImpl(eventName, ref options, (object) null, (Guid*) null, (Guid*) null, SimpleEventTypes<EmptyStruct>.Instance);
    }

    public unsafe void Write<T>(string eventName, [Nullable(1)] T data)
    {
      if (!this.IsEnabled())
        return;
      EventSourceOptions options = new EventSourceOptions();
      this.WriteImpl(eventName, ref options, (object) data, (Guid*) null, (Guid*) null, SimpleEventTypes<T>.Instance);
    }

    public unsafe void Write<T>(string eventName, EventSourceOptions options, [Nullable(1)] T data)
    {
      if (!this.IsEnabled())
        return;
      this.WriteImpl(eventName, ref options, (object) data, (Guid*) null, (Guid*) null, SimpleEventTypes<T>.Instance);
    }

    public unsafe void Write<T>(string eventName, ref EventSourceOptions options, [Nullable(1)] ref T data)
    {
      if (!this.IsEnabled())
        return;
      this.WriteImpl(eventName, ref options, (object) data, (Guid*) null, (Guid*) null, SimpleEventTypes<T>.Instance);
    }

    public unsafe void Write<T>(
      string eventName,
      ref EventSourceOptions options,
      ref Guid activityId,
      ref Guid relatedActivityId,
      [Nullable(1)] ref T data)
    {
      if (!this.IsEnabled())
        return;
      fixed (Guid* pActivityId = &activityId)
        fixed (Guid* guidPtr = &relatedActivityId)
          this.WriteImpl(eventName, ref options, (object) data, pActivityId, relatedActivityId == Guid.Empty ? (Guid*) null : guidPtr, SimpleEventTypes<T>.Instance);
    }

    private unsafe void WriteMultiMerge(
      string eventName,
      ref EventSourceOptions options,
      TraceLoggingEventTypes eventTypes,
      Guid* activityID,
      Guid* childActivityID,
      params object[] values)
    {
      if (!this.IsEnabled() || !this.IsEnabled(((int) options.valuesSet & 4) != 0 ? (EventLevel) options.level : (EventLevel) eventTypes.level, ((int) options.valuesSet & 1) != 0 ? options.keywords : eventTypes.keywords))
        return;
      this.WriteMultiMergeInner(eventName, ref options, eventTypes, activityID, childActivityID, values);
    }

    private unsafe void WriteMultiMergeInner(
      string eventName,
      ref EventSourceOptions options,
      TraceLoggingEventTypes eventTypes,
      Guid* activityID,
      Guid* childActivityID,
      params object[] values)
    {
      byte level = ((int) options.valuesSet & 4) != 0 ? options.level : eventTypes.level;
      byte opcode = ((int) options.valuesSet & 8) != 0 ? options.opcode : eventTypes.opcode;
      EventTags tags = ((int) options.valuesSet & 2) != 0 ? options.tags : eventTypes.Tags;
      EventKeywords eventKeywords = ((int) options.valuesSet & 1) != 0 ? options.keywords : eventTypes.keywords;
      NameInfo nameInfo = eventTypes.GetNameInfo(eventName ?? eventTypes.Name, tags);
      if (nameInfo == null)
        return;
      EventDescriptor eventDescriptor = new EventDescriptor(nameInfo.identity, level, opcode, (long) eventKeywords);
      IntPtr eventHandle = nameInfo.GetOrCreateEventHandle((EventProvider) this.m_eventPipeProvider, this.m_eventHandleTable, eventDescriptor, eventTypes);
      int pinCount = eventTypes.pinCount;
      byte* scratch = stackalloc byte[eventTypes.scratchSize];
      EventSource.EventData* eventDataPtr = stackalloc EventSource.EventData[eventTypes.dataCount + 3];
      for (int index = 0; index < eventTypes.dataCount + 3; ++index)
        eventDataPtr[index] = new EventSource.EventData();
      GCHandle* gcHandlePtr = stackalloc GCHandle[pinCount];
      for (int index = 0; index < pinCount; ++index)
        gcHandlePtr[index] = new GCHandle();
      fixed (byte* pointer1 = this.providerMetadata)
        fixed (byte* pointer2 = nameInfo.nameMetadata)
          fixed (byte* pointer3 = eventTypes.typeMetadata)
          {
            eventDataPtr->SetMetadata(pointer1, this.providerMetadata.Length, 2);
            eventDataPtr[1].SetMetadata(pointer2, nameInfo.nameMetadata.Length, 1);
            eventDataPtr[2].SetMetadata(pointer3, eventTypes.typeMetadata.Length, 1);
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
              DataCollector.ThreadInstance.Enable(scratch, eventTypes.scratchSize, eventDataPtr + 3, eventTypes.dataCount, gcHandlePtr, pinCount);
              for (int index = 0; index < eventTypes.typeInfos.Length; ++index)
              {
                TraceLoggingTypeInfo typeInfo = eventTypes.typeInfos[index];
                typeInfo.WriteData(TraceLoggingDataCollector.Instance, typeInfo.PropertyValueFactory(values[index]));
              }
              this.WriteEventRaw(eventName, ref eventDescriptor, eventHandle, activityID, childActivityID, (int) (DataCollector.ThreadInstance.Finish() - eventDataPtr), (IntPtr) (void*) eventDataPtr);
            }
            finally
            {
              this.WriteCleanup(gcHandlePtr, pinCount);
            }
          }
    }

    internal unsafe void WriteMultiMerge(
      string eventName,
      ref EventSourceOptions options,
      TraceLoggingEventTypes eventTypes,
      Guid* activityID,
      Guid* childActivityID,
      EventSource.EventData* data)
    {
      if (!this.IsEnabled())
        return;
      fixed (EventSourceOptions* eventSourceOptionsPtr = &options)
      {
        EventDescriptor descriptor;
        NameInfo nameInfo = this.UpdateDescriptor(eventName, eventTypes, ref options, out descriptor);
        if (nameInfo == null)
          return;
        IntPtr eventHandle = nameInfo.GetOrCreateEventHandle((EventProvider) this.m_eventPipeProvider, this.m_eventHandleTable, descriptor, eventTypes);
        int num = eventTypes.dataCount + eventTypes.typeInfos.Length * 2 + 3;
        EventSource.EventData* eventDataPtr = stackalloc EventSource.EventData[num];
        for (int index = 0; index < num; ++index)
          eventDataPtr[index] = new EventSource.EventData();
        fixed (byte* pointer1 = this.providerMetadata)
          fixed (byte* pointer2 = nameInfo.nameMetadata)
            fixed (byte* pointer3 = eventTypes.typeMetadata)
            {
              eventDataPtr->SetMetadata(pointer1, this.providerMetadata.Length, 2);
              eventDataPtr[1].SetMetadata(pointer2, nameInfo.nameMetadata.Length, 1);
              eventDataPtr[2].SetMetadata(pointer3, eventTypes.typeMetadata.Length, 1);
              int dataCount = 3;
              for (int index = 0; index < eventTypes.typeInfos.Length; ++index)
              {
                eventDataPtr[dataCount].m_Ptr = data[index].m_Ptr;
                eventDataPtr[dataCount].m_Size = data[index].m_Size;
                if (data[index].m_Size == 4 && eventTypes.typeInfos[index].DataType == typeof (bool))
                  eventDataPtr[dataCount].m_Size = 1;
                ++dataCount;
              }
              this.WriteEventRaw(eventName, ref descriptor, eventHandle, activityID, childActivityID, dataCount, (IntPtr) (void*) eventDataPtr);
            }
      }
    }

    private unsafe void WriteImpl(
      string eventName,
      ref EventSourceOptions options,
      object data,
      Guid* pActivityId,
      Guid* pRelatedActivityId,
      TraceLoggingEventTypes eventTypes)
    {
      try
      {
        fixed (EventSourceOptions* eventSourceOptionsPtr = &options)
        {
          options.Opcode = options.IsOpcodeSet ? options.Opcode : EventSource.GetOpcodeWithDefault(options.Opcode, eventName);
          EventDescriptor descriptor;
          NameInfo nameInfo = this.UpdateDescriptor(eventName, eventTypes, ref options, out descriptor);
          if (nameInfo == null)
            return;
          IntPtr eventHandle = nameInfo.GetOrCreateEventHandle((EventProvider) this.m_eventPipeProvider, this.m_eventHandleTable, descriptor, eventTypes);
          int pinCount = eventTypes.pinCount;
          byte* scratch = stackalloc byte[eventTypes.scratchSize];
          EventSource.EventData* eventDataPtr = stackalloc EventSource.EventData[eventTypes.dataCount + 3];
          for (int index = 0; index < eventTypes.dataCount + 3; ++index)
            eventDataPtr[index] = new EventSource.EventData();
          GCHandle* gcHandlePtr = stackalloc GCHandle[pinCount];
          for (int index = 0; index < pinCount; ++index)
            gcHandlePtr[index] = new GCHandle();
          fixed (byte* pointer1 = this.providerMetadata)
            fixed (byte* pointer2 = nameInfo.nameMetadata)
              fixed (byte* pointer3 = eventTypes.typeMetadata)
              {
                eventDataPtr->SetMetadata(pointer1, this.providerMetadata.Length, 2);
                eventDataPtr[1].SetMetadata(pointer2, nameInfo.nameMetadata.Length, 1);
                eventDataPtr[2].SetMetadata(pointer3, eventTypes.typeMetadata.Length, 1);
                RuntimeHelpers.PrepareConstrainedRegions();
                EventOpcode opcode = (EventOpcode) descriptor.Opcode;
                Guid empty1 = Guid.Empty;
                Guid empty2 = Guid.Empty;
                if ((IntPtr) pActivityId == IntPtr.Zero && (IntPtr) pRelatedActivityId == IntPtr.Zero && (options.ActivityOptions & EventActivityOptions.Disable) == EventActivityOptions.None)
                {
                  switch (opcode)
                  {
                    case EventOpcode.Start:
                      this.m_activityTracker.OnStart(this.m_name, eventName, 0, ref empty1, ref empty2, options.ActivityOptions);
                      break;
                    case EventOpcode.Stop:
                      this.m_activityTracker.OnStop(this.m_name, eventName, 0, ref empty1);
                      break;
                  }
                  if (empty1 != Guid.Empty)
                    pActivityId = &empty1;
                  if (empty2 != Guid.Empty)
                    pRelatedActivityId = &empty2;
                }
                try
                {
                  DataCollector.ThreadInstance.Enable(scratch, eventTypes.scratchSize, eventDataPtr + 3, eventTypes.dataCount, gcHandlePtr, pinCount);
                  TraceLoggingTypeInfo typeInfo = eventTypes.typeInfos[0];
                  typeInfo.WriteData(TraceLoggingDataCollector.Instance, typeInfo.PropertyValueFactory(data));
                  this.WriteEventRaw(eventName, ref descriptor, eventHandle, pActivityId, pRelatedActivityId, (int) (DataCollector.ThreadInstance.Finish() - eventDataPtr), (IntPtr) (void*) eventDataPtr);
                  if (this.m_Dispatchers == null)
                    return;
                  EventPayload data1 = (EventPayload) eventTypes.typeInfos[0].GetData(data);
                  this.WriteToAllListeners(eventName, ref descriptor, nameInfo.tags, pActivityId, pRelatedActivityId, data1);
                }
                catch (Exception ex)
                {
                  if (ex is EventSourceException)
                    throw;
                  else
                    this.ThrowEventSourceException(eventName, ex);
                }
                finally
                {
                  this.WriteCleanup(gcHandlePtr, pinCount);
                }
              }
        }
      }
      catch (Exception ex)
      {
        if (ex is EventSourceException)
          throw;
        else
          this.ThrowEventSourceException(eventName, ex);
      }
    }

    private unsafe void WriteToAllListeners(
      string eventName,
      ref EventDescriptor eventDescriptor,
      EventTags tags,
      Guid* pActivityId,
      Guid* pChildActivityId,
      EventPayload payload)
    {
      EventWrittenEventArgs eventCallbackArgs = new EventWrittenEventArgs(this);
      eventCallbackArgs.EventName = eventName;
      eventCallbackArgs.m_level = (EventLevel) eventDescriptor.Level;
      eventCallbackArgs.m_keywords = (EventKeywords) eventDescriptor.Keywords;
      eventCallbackArgs.m_opcode = (EventOpcode) eventDescriptor.Opcode;
      eventCallbackArgs.m_tags = tags;
      eventCallbackArgs.EventId = -1;
      if ((IntPtr) pActivityId != IntPtr.Zero)
        eventCallbackArgs.ActivityId = *pActivityId;
      if ((IntPtr) pChildActivityId != IntPtr.Zero)
        eventCallbackArgs.RelatedActivityId = *pChildActivityId;
      if (payload != null)
      {
        eventCallbackArgs.Payload = new ReadOnlyCollection<object>((IList<object>) payload.Values);
        eventCallbackArgs.PayloadNames = new ReadOnlyCollection<string>((IList<string>) payload.Keys);
      }
      this.DispatchToAllListeners(-1, pActivityId, eventCallbackArgs);
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [NonEvent]
    private unsafe void WriteCleanup(GCHandle* pPins, int cPins)
    {
      DataCollector.ThreadInstance.Disable();
      for (int index = 0; index < cPins; ++index)
      {
        if (pPins[index].IsAllocated)
          pPins[index].Free();
      }
    }

    private void InitializeProviderMetadata()
    {
      if (this.m_traits != null)
      {
        List<byte> metaData = new List<byte>(100);
        for (int index = 0; index < this.m_traits.Length - 1; index += 2)
        {
          if (this.m_traits[index].StartsWith("ETW_", StringComparison.Ordinal))
          {
            string s = this.m_traits[index].Substring(4);
            byte result;
            if (!byte.TryParse(s, out result))
            {
              if (!(s == "GROUP"))
                throw new ArgumentException(SR.Format(SR.EventSource_UnknownEtwTrait, (object) s), "traits");
              result = (byte) 1;
            }
            string trait = this.m_traits[index + 1];
            int count = metaData.Count;
            metaData.Add((byte) 0);
            metaData.Add((byte) 0);
            metaData.Add(result);
            int num = EventSource.AddValueToMetaData(metaData, trait) + 3;
            metaData[count] = (byte) num;
            metaData[count + 1] = (byte) (num >> 8);
          }
        }
        this.providerMetadata = Statics.MetadataForString(this.Name, 0, metaData.Count, 0);
        int num1 = this.providerMetadata.Length - metaData.Count;
        foreach (byte num2 in metaData)
          this.providerMetadata[num1++] = num2;
      }
      else
        this.providerMetadata = Statics.MetadataForString(this.Name, 0, 0, 0);
    }

    private static int AddValueToMetaData(List<byte> metaData, string value)
    {
      if (value.Length == 0)
        return 0;
      int count = metaData.Count;
      char ch = value[0];
      switch (ch)
      {
        case '#':
          for (int index = 1; index < value.Length; ++index)
          {
            if (value[index] != ' ')
            {
              if (index + 1 >= value.Length)
                throw new ArgumentException(SR.EventSource_EvenHexDigits, "traits");
              metaData.Add((byte) (EventSource.HexDigit(value[index]) * 16 + EventSource.HexDigit(value[index + 1])));
              ++index;
            }
          }
          break;
        case '@':
          metaData.AddRange((IEnumerable<byte>) Encoding.UTF8.GetBytes(value.Substring(1)));
          break;
        case '{':
          metaData.AddRange((IEnumerable<byte>) new Guid(value).ToByteArray());
          break;
        default:
          if ('A' > ch && ' ' != ch)
            throw new ArgumentException(SR.Format(SR.EventSource_IllegalValue, (object) value), "traits");
          metaData.AddRange((IEnumerable<byte>) Encoding.UTF8.GetBytes(value));
          break;
      }
      return metaData.Count - count;
    }

    private static int HexDigit(char c)
    {
      if ('0' <= c && c <= '9')
        return (int) c - 48;
      if ('a' <= c)
        c -= ' ';
      if ('A' <= c && c <= 'F')
        return (int) c - 65 + 10;
      throw new ArgumentException(SR.Format(SR.EventSource_BadHexDigit, (object) c), "traits");
    }

    private NameInfo UpdateDescriptor(
      string name,
      TraceLoggingEventTypes eventInfo,
      ref EventSourceOptions options,
      out EventDescriptor descriptor)
    {
      NameInfo nameInfo = (NameInfo) null;
      int traceloggingId = 0;
      byte level = ((int) options.valuesSet & 4) != 0 ? options.level : eventInfo.level;
      byte opcode = ((int) options.valuesSet & 8) != 0 ? options.opcode : eventInfo.opcode;
      EventTags tags = ((int) options.valuesSet & 2) != 0 ? options.tags : eventInfo.Tags;
      EventKeywords keywords = ((int) options.valuesSet & 1) != 0 ? options.keywords : eventInfo.keywords;
      if (this.IsEnabled((EventLevel) level, keywords))
      {
        nameInfo = eventInfo.GetNameInfo(name ?? eventInfo.Name, tags);
        traceloggingId = nameInfo.identity;
      }
      descriptor = new EventDescriptor(traceloggingId, level, opcode, (long) keywords);
      return nameInfo;
    }

    [NullableContext(0)]
    protected internal struct EventData
    {
      internal ulong m_Ptr;
      internal int m_Size;
      internal int m_Reserved;

      public unsafe IntPtr DataPointer
      {
        get
        {
          return (IntPtr) (void*) this.m_Ptr;
        }
        set
        {
          this.m_Ptr = (ulong) (void*) value;
        }
      }

      public int Size
      {
        get
        {
          return this.m_Size;
        }
        set
        {
          this.m_Size = value;
        }
      }

      internal int Reserved
      {
        set
        {
          this.m_Reserved = value;
        }
      }

      internal unsafe void SetMetadata(byte* pointer, int size, int reserved)
      {
        this.m_Ptr = (ulong) pointer;
        this.m_Size = size;
        this.m_Reserved = reserved;
      }
    }

    private struct Sha1ForNonSecretPurposes
    {
      private long length;
      private uint[] w;
      private int pos;

      public void Start()
      {
        if (this.w == null)
          this.w = new uint[85];
        this.length = 0L;
        this.pos = 0;
        this.w[80] = 1732584193U;
        this.w[81] = 4023233417U;
        this.w[82] = 2562383102U;
        this.w[83] = 271733878U;
        this.w[84] = 3285377520U;
      }

      public void Append(byte input)
      {
        this.w[this.pos / 4] = this.w[this.pos / 4] << 8 | (uint) input;
        if (64 != ++this.pos)
          return;
        this.Drain();
      }

      public void Append(byte[] input)
      {
        foreach (byte input1 in input)
          this.Append(input1);
      }

      public void Finish(byte[] output)
      {
        long num1 = this.length + (long) (8 * this.pos);
        this.Append((byte) 128);
        while (this.pos != 56)
          this.Append((byte) 0);
        this.Append((byte) (num1 >> 56));
        this.Append((byte) (num1 >> 48));
        this.Append((byte) (num1 >> 40));
        this.Append((byte) (num1 >> 32));
        this.Append((byte) (num1 >> 24));
        this.Append((byte) (num1 >> 16));
        this.Append((byte) (num1 >> 8));
        this.Append((byte) num1);
        int num2 = output.Length < 20 ? output.Length : 20;
        for (int index = 0; index != num2; ++index)
        {
          uint num3 = this.w[80 + index / 4];
          output[index] = (byte) (num3 >> 24);
          this.w[80 + index / 4] = num3 << 8;
        }
      }

      private void Drain()
      {
        for (int index = 16; index != 80; ++index)
          this.w[index] = BitOperations.RotateLeft(this.w[index - 3] ^ this.w[index - 8] ^ this.w[index - 14] ^ this.w[index - 16], 1);
        uint num1 = this.w[80];
        uint num2 = this.w[81];
        uint num3 = this.w[82];
        uint num4 = this.w[83];
        uint num5 = this.w[84];
        for (int index = 0; index != 20; ++index)
        {
          uint num6 = (uint) ((int) num2 & (int) num3 | ~(int) num2 & (int) num4);
          uint num7 = (uint) ((int) BitOperations.RotateLeft(num1, 5) + (int) num6 + (int) num5 + 1518500249) + this.w[index];
          num5 = num4;
          num4 = num3;
          num3 = BitOperations.RotateLeft(num2, 30);
          num2 = num1;
          num1 = num7;
        }
        for (int index = 20; index != 40; ++index)
        {
          uint num6 = num2 ^ num3 ^ num4;
          uint num7 = (uint) ((int) BitOperations.RotateLeft(num1, 5) + (int) num6 + (int) num5 + 1859775393) + this.w[index];
          num5 = num4;
          num4 = num3;
          num3 = BitOperations.RotateLeft(num2, 30);
          num2 = num1;
          num1 = num7;
        }
        for (int index = 40; index != 60; ++index)
        {
          uint num6 = (uint) ((int) num2 & (int) num3 | (int) num2 & (int) num4 | (int) num3 & (int) num4);
          uint num7 = (uint) ((int) BitOperations.RotateLeft(num1, 5) + (int) num6 + (int) num5 - 1894007588) + this.w[index];
          num5 = num4;
          num4 = num3;
          num3 = BitOperations.RotateLeft(num2, 30);
          num2 = num1;
          num1 = num7;
        }
        for (int index = 60; index != 80; ++index)
        {
          uint num6 = num2 ^ num3 ^ num4;
          uint num7 = (uint) ((int) BitOperations.RotateLeft(num1, 5) + (int) num6 + (int) num5 - 899497514) + this.w[index];
          num5 = num4;
          num4 = num3;
          num3 = BitOperations.RotateLeft(num2, 30);
          num2 = num1;
          num1 = num7;
        }
        this.w[80] += num1;
        this.w[81] += num2;
        this.w[82] += num3;
        this.w[83] += num4;
        this.w[84] += num5;
        this.length += 512L;
        this.pos = 0;
      }
    }

    private class OverideEventProvider : EventProvider
    {
      private EventSource m_eventSource;
      private EventProviderType m_eventProviderType;

      public OverideEventProvider(EventSource eventSource, EventProviderType providerType)
        : base(providerType)
      {
        this.m_eventSource = eventSource;
        this.m_eventProviderType = providerType;
      }

      protected override void OnControllerCommand(
        ControllerCommand command,
        IDictionary<string, string> arguments,
        int perEventSourceSessionId,
        int etwSessionId)
      {
        this.m_eventSource.SendCommand((EventListener) null, this.m_eventProviderType, perEventSourceSessionId, etwSessionId, (EventCommand) command, this.IsEnabled(), this.Level, this.MatchAnyKeyword, arguments);
      }
    }

    internal struct EventMetadata
    {
      public EventDescriptor Descriptor;
      public IntPtr EventHandle;
      public EventTags Tags;
      public bool EnabledForAnyListener;
      public bool EnabledForETW;
      public bool EnabledForEventPipe;
      public bool HasRelatedActivityID;
      public byte TriggersActivityTracking;
      public string Name;
      public string Message;
      public ParameterInfo[] Parameters;
      public TraceLoggingEventTypes TraceLoggingEventTypes;
      public EventActivityOptions ActivityOptions;
    }
  }
}

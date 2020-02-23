// Decompiled with JetBrains decompiler
// Type: System.IO.FileStream
// Assembly: System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// MVID: BBB3B884-123D-47EA-9CD1-5BED540D02AE
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Private.CoreLib.dll

using Microsoft.Win32.SafeHandles;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
  [NullableContext(1)]
  [Nullable(0)]
  public class FileStream : Stream
  {
    private static int s_cachedSerializationSwitch = 0;
    private static readonly IOCompletionCallback s_ioCallback = new IOCompletionCallback(FileStream.FileStreamCompletionSource.IOCallback);
    private Task _activeBufferOperation = Task.CompletedTask;
    private byte[] _buffer;
    private int _bufferLength;
    private readonly SafeFileHandle _fileHandle;
    private readonly FileAccess _access;
    private readonly string _path;
    private int _readPos;
    private int _readLength;
    private int _writePos;
    private readonly bool _useAsyncIO;
    private Task<int> _lastSynchronouslyCompletedTask;
    private long _filePosition;
    private bool _exposedHandle;
    private bool _canSeek;
    private bool _isPipe;
    private long _appendStart;
    private PreAllocatedOverlapped _preallocatedOverlapped;
    private FileStream.FileStreamCompletionSource _currentOverlappedOwner;

    [Obsolete("This constructor has been deprecated.  Please use new FileStream(SafeFileHandle handle, FileAccess access) instead.  https://go.microsoft.com/fwlink/?linkid=14202")]
    public FileStream(IntPtr handle, FileAccess access)
      : this(handle, access, true, 4096, false)
    {
    }

    [Obsolete("This constructor has been deprecated.  Please use new FileStream(SafeFileHandle handle, FileAccess access) instead, and optionally make a new SafeFileHandle with ownsHandle=false if needed.  https://go.microsoft.com/fwlink/?linkid=14202")]
    public FileStream(IntPtr handle, FileAccess access, bool ownsHandle)
      : this(handle, access, ownsHandle, 4096, false)
    {
    }

    [Obsolete("This constructor has been deprecated.  Please use new FileStream(SafeFileHandle handle, FileAccess access, int bufferSize) instead, and optionally make a new SafeFileHandle with ownsHandle=false if needed.  https://go.microsoft.com/fwlink/?linkid=14202")]
    public FileStream(IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize)
      : this(handle, access, ownsHandle, bufferSize, false)
    {
    }

    [Obsolete("This constructor has been deprecated.  Please use new FileStream(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync) instead, and optionally make a new SafeFileHandle with ownsHandle=false if needed.  https://go.microsoft.com/fwlink/?linkid=14202")]
    public FileStream(
      IntPtr handle,
      FileAccess access,
      bool ownsHandle,
      int bufferSize,
      bool isAsync)
    {
      SafeFileHandle handle1 = new SafeFileHandle(handle, ownsHandle);
      try
      {
        this.ValidateAndInitFromHandle(handle1, access, bufferSize, isAsync);
      }
      catch
      {
        GC.SuppressFinalize((object) handle1);
        throw;
      }
      this._access = access;
      this._useAsyncIO = isAsync;
      this._fileHandle = handle1;
    }

    public FileStream(SafeFileHandle handle, FileAccess access)
      : this(handle, access, 4096)
    {
    }

    public FileStream(SafeFileHandle handle, FileAccess access, int bufferSize)
      : this(handle, access, bufferSize, FileStream.GetDefaultIsAsync(handle))
    {
    }

    private void ValidateAndInitFromHandle(
      SafeFileHandle handle,
      FileAccess access,
      int bufferSize,
      bool isAsync)
    {
      if (handle.IsInvalid)
        throw new ArgumentException(SR.Arg_InvalidHandle, nameof (handle));
      if (access < FileAccess.Read || access > FileAccess.ReadWrite)
        throw new ArgumentOutOfRangeException(nameof (access), SR.ArgumentOutOfRange_Enum);
      if (bufferSize <= 0)
        throw new ArgumentOutOfRangeException(nameof (bufferSize), SR.ArgumentOutOfRange_NeedPosNum);
      if (handle.IsClosed)
        throw new ObjectDisposedException(SR.ObjectDisposed_FileClosed);
      bool? isAsync1 = handle.IsAsync;
      if (isAsync1.HasValue)
      {
        int num1 = isAsync ? 1 : 0;
        isAsync1 = handle.IsAsync;
        int num2 = isAsync1.GetValueOrDefault() ? 1 : 0;
        if (num1 != num2)
          throw new ArgumentException(SR.Arg_HandleNotAsync, nameof (handle));
      }
      this._exposedHandle = true;
      this._bufferLength = bufferSize;
      this.InitFromHandle(handle, access, isAsync);
    }

    public FileStream(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync)
    {
      this.ValidateAndInitFromHandle(handle, access, bufferSize, isAsync);
      this._access = access;
      this._useAsyncIO = isAsync;
      this._fileHandle = handle;
    }

    public FileStream(string path, FileMode mode)
      : this(path, mode, mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite, FileShare.Read, 4096, false)
    {
    }

    public FileStream(string path, FileMode mode, FileAccess access)
      : this(path, mode, access, FileShare.Read, 4096, false)
    {
    }

    public FileStream(string path, FileMode mode, FileAccess access, FileShare share)
      : this(path, mode, access, share, 4096, false)
    {
    }

    public FileStream(
      string path,
      FileMode mode,
      FileAccess access,
      FileShare share,
      int bufferSize)
      : this(path, mode, access, share, bufferSize, false)
    {
    }

    public FileStream(
      string path,
      FileMode mode,
      FileAccess access,
      FileShare share,
      int bufferSize,
      bool useAsync)
      : this(path, mode, access, share, bufferSize, useAsync ? FileOptions.Asynchronous : FileOptions.None)
    {
    }

    public FileStream(
      string path,
      FileMode mode,
      FileAccess access,
      FileShare share,
      int bufferSize,
      FileOptions options)
    {
      switch (path)
      {
        case "":
          throw new ArgumentException(SR.Argument_EmptyPath, nameof (path));
        case null:
          throw new ArgumentNullException(nameof (path), SR.ArgumentNull_Path);
        default:
          FileShare fileShare = share & ~FileShare.Inheritable;
          string paramName = (string) null;
          if (mode < FileMode.CreateNew || mode > FileMode.Append)
            paramName = nameof (mode);
          else if (access < FileAccess.Read || access > FileAccess.ReadWrite)
            paramName = nameof (access);
          else if (fileShare < FileShare.None || fileShare > (FileShare.ReadWrite | FileShare.Delete))
            paramName = nameof (share);
          if (paramName != null)
            throw new ArgumentOutOfRangeException(paramName, SR.ArgumentOutOfRange_Enum);
          if (options != FileOptions.None && (options & (FileOptions) 67092479) != FileOptions.None)
            throw new ArgumentOutOfRangeException(nameof (options), SR.ArgumentOutOfRange_Enum);
          if (bufferSize <= 0)
            throw new ArgumentOutOfRangeException(nameof (bufferSize), SR.ArgumentOutOfRange_NeedPosNum);
          if ((access & FileAccess.Write) == (FileAccess) 0 && (mode == FileMode.Truncate || mode == FileMode.CreateNew || (mode == FileMode.Create || mode == FileMode.Append)))
            throw new ArgumentException(SR.Format(SR.Argument_InvalidFileModeAndAccessCombo, (object) mode, (object) access), nameof (access));
          if ((access & FileAccess.Read) != (FileAccess) 0 && mode == FileMode.Append)
            throw new ArgumentException(SR.Argument_InvalidAppendMode, nameof (access));
          this._path = Path.GetFullPath(path);
          this._access = access;
          this._bufferLength = bufferSize;
          if ((options & FileOptions.Asynchronous) != FileOptions.None)
            this._useAsyncIO = true;
          if ((access & FileAccess.Write) == FileAccess.Write)
            SerializationInfo.ThrowIfDeserializationInProgress("AllowFileWrites", ref FileStream.s_cachedSerializationSwitch);
          this._fileHandle = this.OpenHandle(mode, share, options);
          try
          {
            this.Init(mode, share, path);
            break;
          }
          catch
          {
            this._fileHandle.Dispose();
            this._fileHandle = (SafeFileHandle) null;
            throw;
          }
      }
    }

    [Obsolete("This property has been deprecated.  Please use FileStream's SafeFileHandle property instead.  https://go.microsoft.com/fwlink/?linkid=14202")]
    public virtual IntPtr Handle
    {
      get
      {
        return this.SafeFileHandle.DangerousGetHandle();
      }
    }

    public virtual void Lock(long position, long length)
    {
      if (position < 0L || length < 0L)
        throw new ArgumentOutOfRangeException(position < 0L ? nameof (position) : nameof (length), SR.ArgumentOutOfRange_NeedNonNegNum);
      if (this._fileHandle.IsClosed)
        throw Error.GetFileNotOpen();
      this.LockInternal(position, length);
    }

    public virtual void Unlock(long position, long length)
    {
      if (position < 0L || length < 0L)
        throw new ArgumentOutOfRangeException(position < 0L ? nameof (position) : nameof (length), SR.ArgumentOutOfRange_NeedNonNegNum);
      if (this._fileHandle.IsClosed)
        throw Error.GetFileNotOpen();
      this.UnlockInternal(position, length);
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
      return this.GetType() != typeof (FileStream) ? base.FlushAsync(cancellationToken) : this.FlushAsyncInternal(cancellationToken);
    }

    public override int Read(byte[] array, int offset, int count)
    {
      this.ValidateReadWriteArgs(array, offset, count);
      return !this._useAsyncIO ? this.ReadSpan(new Span<byte>(array, offset, count)) : this.ReadAsyncTask(array, offset, count, CancellationToken.None).GetAwaiter().GetResult();
    }

    [NullableContext(0)]
    public override int Read(Span<byte> buffer)
    {
      if (!(this.GetType() == typeof (FileStream)) || this._useAsyncIO)
        return base.Read(buffer);
      if (this._fileHandle.IsClosed)
        throw Error.GetFileNotOpen();
      return this.ReadSpan(buffer);
    }

    public override Task<int> ReadAsync(
      byte[] buffer,
      int offset,
      int count,
      CancellationToken cancellationToken)
    {
      if (buffer == null)
        throw new ArgumentNullException(nameof (buffer), SR.ArgumentNull_Buffer);
      if (offset < 0)
        throw new ArgumentOutOfRangeException(nameof (offset), SR.ArgumentOutOfRange_NeedNonNegNum);
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_NeedNonNegNum);
      if (buffer.Length - offset < count)
        throw new ArgumentException(SR.Argument_InvalidOffLen);
      if (this.GetType() != typeof (FileStream) || !this._useAsyncIO)
        return base.ReadAsync(buffer, offset, count, cancellationToken);
      if (cancellationToken.IsCancellationRequested)
        return Task.FromCanceled<int>(cancellationToken);
      if (this.IsClosed)
        throw Error.GetFileNotOpen();
      return this.ReadAsyncTask(buffer, offset, count, cancellationToken);
    }

    [NullableContext(0)]
    public override ValueTask<int> ReadAsync(
      Memory<byte> buffer,
      CancellationToken cancellationToken = default (CancellationToken))
    {
      if (!this._useAsyncIO || this.GetType() != typeof (FileStream))
        return base.ReadAsync(buffer, cancellationToken);
      if (cancellationToken.IsCancellationRequested)
        return new ValueTask<int>(Task.FromCanceled<int>(cancellationToken));
      if (this.IsClosed)
        throw Error.GetFileNotOpen();
      int synchronousResult;
      Task<int> task = this.ReadAsyncInternal(buffer, cancellationToken, out synchronousResult);
      return task == null ? new ValueTask<int>(synchronousResult) : new ValueTask<int>(task);
    }

    private Task<int> ReadAsyncTask(
      byte[] array,
      int offset,
      int count,
      CancellationToken cancellationToken)
    {
      int synchronousResult;
      Task<int> task = this.ReadAsyncInternal(new Memory<byte>(array, offset, count), cancellationToken, out synchronousResult);
      if (task == null)
      {
        task = this._lastSynchronouslyCompletedTask;
        if (task == null || task.Result != synchronousResult)
          this._lastSynchronouslyCompletedTask = task = Task.FromResult<int>(synchronousResult);
      }
      return task;
    }

    public override void Write(byte[] array, int offset, int count)
    {
      this.ValidateReadWriteArgs(array, offset, count);
      if (this._useAsyncIO)
        this.WriteAsyncInternal(new ReadOnlyMemory<byte>(array, offset, count), CancellationToken.None).GetAwaiter().GetResult();
      else
        this.WriteSpan(new ReadOnlySpan<byte>(array, offset, count));
    }

    [NullableContext(0)]
    public override void Write(ReadOnlySpan<byte> buffer)
    {
      if (this.GetType() == typeof (FileStream) && !this._useAsyncIO)
      {
        if (this._fileHandle.IsClosed)
          throw Error.GetFileNotOpen();
        this.WriteSpan(buffer);
      }
      else
        base.Write(buffer);
    }

    public override Task WriteAsync(
      byte[] buffer,
      int offset,
      int count,
      CancellationToken cancellationToken)
    {
      if (buffer == null)
        throw new ArgumentNullException(nameof (buffer), SR.ArgumentNull_Buffer);
      if (offset < 0)
        throw new ArgumentOutOfRangeException(nameof (offset), SR.ArgumentOutOfRange_NeedNonNegNum);
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_NeedNonNegNum);
      if (buffer.Length - offset < count)
        throw new ArgumentException(SR.Argument_InvalidOffLen);
      if (!this._useAsyncIO || this.GetType() != typeof (FileStream))
        return base.WriteAsync(buffer, offset, count, cancellationToken);
      if (cancellationToken.IsCancellationRequested)
        return Task.FromCanceled(cancellationToken);
      if (this.IsClosed)
        throw Error.GetFileNotOpen();
      return this.WriteAsyncInternal(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken).AsTask();
    }

    [NullableContext(0)]
    public override ValueTask WriteAsync(
      ReadOnlyMemory<byte> buffer,
      CancellationToken cancellationToken = default (CancellationToken))
    {
      if (!this._useAsyncIO || this.GetType() != typeof (FileStream))
        return base.WriteAsync(buffer, cancellationToken);
      if (cancellationToken.IsCancellationRequested)
        return new ValueTask((Task) Task.FromCanceled<int>(cancellationToken));
      if (this.IsClosed)
        throw Error.GetFileNotOpen();
      return this.WriteAsyncInternal(buffer, cancellationToken);
    }

    public override void Flush()
    {
      this.Flush(false);
    }

    public virtual void Flush(bool flushToDisk)
    {
      if (this.IsClosed)
        throw Error.GetFileNotOpen();
      this.FlushInternalBuffer();
      if (!flushToDisk || !this.CanWrite)
        return;
      this.FlushOSBuffer();
    }

    public override bool CanRead
    {
      get
      {
        return !this._fileHandle.IsClosed && (uint) (this._access & FileAccess.Read) > 0U;
      }
    }

    public override bool CanWrite
    {
      get
      {
        return !this._fileHandle.IsClosed && (uint) (this._access & FileAccess.Write) > 0U;
      }
    }

    private void ValidateReadWriteArgs(byte[] array, int offset, int count)
    {
      if (array == null)
        throw new ArgumentNullException(nameof (array), SR.ArgumentNull_Buffer);
      if (offset < 0)
        throw new ArgumentOutOfRangeException(nameof (offset), SR.ArgumentOutOfRange_NeedNonNegNum);
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count), SR.ArgumentOutOfRange_NeedNonNegNum);
      if (array.Length - offset < count)
        throw new ArgumentException(SR.Argument_InvalidOffLen);
      if (this._fileHandle.IsClosed)
        throw Error.GetFileNotOpen();
    }

    public override void SetLength(long value)
    {
      if (value < 0L)
        throw new ArgumentOutOfRangeException(nameof (value), SR.ArgumentOutOfRange_NeedNonNegNum);
      if (this._fileHandle.IsClosed)
        throw Error.GetFileNotOpen();
      if (!this.CanSeek)
        throw Error.GetSeekNotSupported();
      if (!this.CanWrite)
        throw Error.GetWriteNotSupported();
      this.SetLengthInternal(value);
    }

    public virtual SafeFileHandle SafeFileHandle
    {
      get
      {
        this.Flush();
        this._exposedHandle = true;
        return this._fileHandle;
      }
    }

    public virtual string Name
    {
      get
      {
        return this._path ?? SR.IO_UnknownFileName;
      }
    }

    public virtual bool IsAsync
    {
      get
      {
        return this._useAsyncIO;
      }
    }

    public override long Length
    {
      get
      {
        if (this._fileHandle.IsClosed)
          throw Error.GetFileNotOpen();
        if (!this.CanSeek)
          throw Error.GetSeekNotSupported();
        return this.GetLengthInternal();
      }
    }

    private void VerifyOSHandlePosition()
    {
      if (!this._exposedHandle || !this.CanSeek || this._filePosition == this.SeekCore(this._fileHandle, 0L, SeekOrigin.Current, false))
        return;
      this._readPos = this._readLength = 0;
      if (this._writePos > 0)
      {
        this._writePos = 0;
        throw new IOException(SR.IO_FileStreamHandlePosition);
      }
    }

    private void PrepareForReading()
    {
      if (this._fileHandle.IsClosed)
        throw Error.GetFileNotOpen();
      if (this._readLength == 0 && !this.CanRead)
        throw Error.GetReadNotSupported();
    }

    public override long Position
    {
      get
      {
        if (this._fileHandle.IsClosed)
          throw Error.GetFileNotOpen();
        if (!this.CanSeek)
          throw Error.GetSeekNotSupported();
        this.VerifyOSHandlePosition();
        return this._filePosition - (long) this._readLength + (long) this._readPos + (long) this._writePos;
      }
      set
      {
        if (value < 0L)
          throw new ArgumentOutOfRangeException(nameof (value), SR.ArgumentOutOfRange_NeedNonNegNum);
        this.Seek(value, SeekOrigin.Begin);
      }
    }

    internal virtual bool IsClosed
    {
      get
      {
        return this._fileHandle.IsClosed;
      }
    }

    private static bool IsIoRelatedException(Exception e)
    {
      switch (e)
      {
        case IOException _:
        case UnauthorizedAccessException _:
        case NotSupportedException _:
          return true;
        case ArgumentException _:
          return !(e is ArgumentNullException);
        default:
          return false;
      }
    }

    private byte[] GetBuffer()
    {
      if (this._buffer == null)
      {
        this._buffer = new byte[this._bufferLength];
        this.OnBufferAllocated();
      }
      return this._buffer;
    }

    private void OnBufferAllocated()
    {
      if (!this._useAsyncIO)
        return;
      this._preallocatedOverlapped = new PreAllocatedOverlapped(FileStream.s_ioCallback, (object) this, (object) this._buffer);
    }

    private void FlushInternalBuffer()
    {
      if (this._writePos > 0)
      {
        this.FlushWriteBuffer(false);
      }
      else
      {
        if (this._readPos >= this._readLength || !this.CanSeek)
          return;
        this.FlushReadBuffer();
      }
    }

    private void FlushReadBuffer()
    {
      int num = this._readPos - this._readLength;
      if (num != 0)
        this.SeekCore(this._fileHandle, (long) num, SeekOrigin.Current, false);
      this._readPos = this._readLength = 0;
    }

    public override int ReadByte()
    {
      this.PrepareForReading();
      byte[] buffer = this.GetBuffer();
      if (this._readPos == this._readLength)
      {
        this.FlushWriteBuffer(false);
        this._readLength = this.FillReadBufferForReadByte();
        this._readPos = 0;
        if (this._readLength == 0)
          return -1;
      }
      return (int) buffer[this._readPos++];
    }

    public override void WriteByte(byte value)
    {
      this.PrepareForWriting();
      if (this._writePos == this._bufferLength)
        this.FlushWriteBufferForWriteByte();
      this.GetBuffer()[this._writePos++] = value;
    }

    private void PrepareForWriting()
    {
      if (this._fileHandle.IsClosed)
        throw Error.GetFileNotOpen();
      if (this._writePos != 0)
        return;
      if (!this.CanWrite)
        throw Error.GetWriteNotSupported();
      this.FlushReadBuffer();
    }

    ~FileStream()
    {
      this.Dispose(false);
    }

    public override IAsyncResult BeginRead(
      byte[] array,
      int offset,
      int numBytes,
      AsyncCallback callback,
      [Nullable(2)] object state)
    {
      if (array == null)
        throw new ArgumentNullException(nameof (array));
      if (offset < 0)
        throw new ArgumentOutOfRangeException(nameof (offset), SR.ArgumentOutOfRange_NeedNonNegNum);
      if (numBytes < 0)
        throw new ArgumentOutOfRangeException(nameof (numBytes), SR.ArgumentOutOfRange_NeedNonNegNum);
      if (array.Length - offset < numBytes)
        throw new ArgumentException(SR.Argument_InvalidOffLen);
      if (this.IsClosed)
        throw new ObjectDisposedException(SR.ObjectDisposed_FileClosed);
      if (!this.CanRead)
        throw new NotSupportedException(SR.NotSupported_UnreadableStream);
      return !this.IsAsync ? base.BeginRead(array, offset, numBytes, callback, state) : TaskToApm.Begin((Task) this.ReadAsyncTask(array, offset, numBytes, CancellationToken.None), callback, state);
    }

    public override IAsyncResult BeginWrite(
      byte[] array,
      int offset,
      int numBytes,
      AsyncCallback callback,
      [Nullable(2)] object state)
    {
      if (array == null)
        throw new ArgumentNullException(nameof (array));
      if (offset < 0)
        throw new ArgumentOutOfRangeException(nameof (offset), SR.ArgumentOutOfRange_NeedNonNegNum);
      if (numBytes < 0)
        throw new ArgumentOutOfRangeException(nameof (numBytes), SR.ArgumentOutOfRange_NeedNonNegNum);
      if (array.Length - offset < numBytes)
        throw new ArgumentException(SR.Argument_InvalidOffLen);
      if (this.IsClosed)
        throw new ObjectDisposedException(SR.ObjectDisposed_FileClosed);
      if (!this.CanWrite)
        throw new NotSupportedException(SR.NotSupported_UnwritableStream);
      return !this.IsAsync ? base.BeginWrite(array, offset, numBytes, callback, state) : TaskToApm.Begin(this.WriteAsyncInternal(new ReadOnlyMemory<byte>(array, offset, numBytes), CancellationToken.None).AsTask(), callback, state);
    }

    public override int EndRead(IAsyncResult asyncResult)
    {
      if (asyncResult == null)
        throw new ArgumentNullException(nameof (asyncResult));
      return !this.IsAsync ? base.EndRead(asyncResult) : TaskToApm.End<int>(asyncResult);
    }

    public override void EndWrite(IAsyncResult asyncResult)
    {
      if (asyncResult == null)
        throw new ArgumentNullException(nameof (asyncResult));
      if (!this.IsAsync)
        base.EndWrite(asyncResult);
      else
        TaskToApm.End(asyncResult);
    }

    private void Init(FileMode mode, FileShare share, string originalPath)
    {
      if (!PathInternal.IsExtended((ReadOnlySpan<char>) originalPath))
      {
        int num;
        switch (Interop.Kernel32.GetFileType((SafeHandle) this._fileHandle))
        {
          case 0:
            num = Marshal.GetLastWin32Error();
            break;
          case 1:
            goto label_7;
          default:
            num = 0;
            break;
        }
        int errorCode = num;
        this._fileHandle.Dispose();
        if (errorCode != 0)
          throw Win32Marshal.GetExceptionForWin32Error(errorCode, "");
        throw new NotSupportedException(SR.NotSupported_FileStreamOnNonFiles);
      }
label_7:
      if (this._useAsyncIO)
      {
        try
        {
          this._fileHandle.ThreadPoolBinding = ThreadPoolBoundHandle.BindHandle((SafeHandle) this._fileHandle);
        }
        catch (ArgumentException ex)
        {
          throw new IOException(SR.IO_BindHandleFailed, (Exception) ex);
        }
        finally
        {
          if (this._fileHandle.ThreadPoolBinding == null)
            this._fileHandle.Dispose();
        }
      }
      this._canSeek = true;
      if (mode == FileMode.Append)
        this._appendStart = this.SeekCore(this._fileHandle, 0L, SeekOrigin.End, false);
      else
        this._appendStart = -1L;
    }

    private void InitFromHandle(SafeFileHandle handle, FileAccess access, bool useAsyncIO)
    {
      this.InitFromHandleImpl(handle, access, useAsyncIO);
    }

    private void InitFromHandleImpl(SafeFileHandle handle, FileAccess access, bool useAsyncIO)
    {
      int fileType = Interop.Kernel32.GetFileType((SafeHandle) handle);
      this._canSeek = fileType == 1;
      this._isPipe = fileType == 3;
      if (useAsyncIO)
      {
        if (!handle.IsAsync.GetValueOrDefault())
        {
          try
          {
            handle.ThreadPoolBinding = ThreadPoolBoundHandle.BindHandle((SafeHandle) handle);
            goto label_6;
          }
          catch (Exception ex)
          {
            throw new ArgumentException(SR.Arg_HandleNotAsync, nameof (handle), ex);
          }
        }
      }
      if (!useAsyncIO)
        FileStream.VerifyHandleIsSync(handle, fileType, access);
label_6:
      if (this._canSeek)
        this.SeekCore(handle, 0L, SeekOrigin.Current, false);
      else
        this._filePosition = 0L;
    }

    private static Interop.Kernel32.SECURITY_ATTRIBUTES GetSecAttrs(FileShare share)
    {
      Interop.Kernel32.SECURITY_ATTRIBUTES securityAttributes = new Interop.Kernel32.SECURITY_ATTRIBUTES();
      if ((share & FileShare.Inheritable) != FileShare.None)
        securityAttributes = new Interop.Kernel32.SECURITY_ATTRIBUTES()
        {
          nLength = (uint) sizeof (Interop.Kernel32.SECURITY_ATTRIBUTES),
          bInheritHandle = Interop.BOOL.TRUE
        };
      return securityAttributes;
    }

    private bool HasActiveBufferOperation
    {
      get
      {
        return !this._activeBufferOperation.IsCompleted;
      }
    }

    public override bool CanSeek
    {
      get
      {
        return this._canSeek;
      }
    }

    private long GetLengthInternal()
    {
      Interop.Kernel32.FILE_STANDARD_INFO lpFileInformation = new Interop.Kernel32.FILE_STANDARD_INFO();
      if (!Interop.Kernel32.GetFileInformationByHandleEx(this._fileHandle, Interop.Kernel32.FILE_INFO_BY_HANDLE_CLASS.FileStandardInfo, out lpFileInformation, (uint) sizeof (Interop.Kernel32.FILE_STANDARD_INFO)))
        throw Win32Marshal.GetExceptionForLastWin32Error(this._path);
      long num = lpFileInformation.EndOfFile;
      if (this._writePos > 0 && this._filePosition + (long) this._writePos > num)
        num = (long) this._writePos + this._filePosition;
      return num;
    }

    protected override void Dispose(bool disposing)
    {
      try
      {
        if (this._fileHandle == null || this._fileHandle.IsClosed)
          return;
        if (this._writePos <= 0)
          return;
        try
        {
          this.FlushWriteBuffer(!disposing);
        }
        catch (Exception ex) when (FileStream.IsIoRelatedException(ex) && !disposing)
        {
        }
      }
      finally
      {
        if (this._fileHandle != null && !this._fileHandle.IsClosed)
        {
          this._fileHandle.ThreadPoolBinding?.Dispose();
          this._fileHandle.Dispose();
        }
        this._preallocatedOverlapped?.Dispose();
        this._canSeek = false;
      }
    }

    public override ValueTask DisposeAsync()
    {
      return !(this.GetType() == typeof (FileStream)) ? base.DisposeAsync() : this.DisposeAsyncCore();
    }

    private async ValueTask DisposeAsyncCore()
    {
      FileStream fileStream = this;
      try
      {
        if (fileStream._fileHandle == null || fileStream._fileHandle.IsClosed || fileStream._writePos <= 0)
          return;
        await fileStream.FlushAsyncInternal(new CancellationToken()).ConfigureAwait(false);
      }
      finally
      {
        if (fileStream._fileHandle != null && !fileStream._fileHandle.IsClosed)
        {
          fileStream._fileHandle.ThreadPoolBinding?.Dispose();
          fileStream._fileHandle.Dispose();
        }
        fileStream._preallocatedOverlapped?.Dispose();
        fileStream._canSeek = false;
        GC.SuppressFinalize((object) fileStream);
      }
    }

    private void FlushOSBuffer()
    {
      if (!Interop.Kernel32.FlushFileBuffers((SafeHandle) this._fileHandle))
        throw Win32Marshal.GetExceptionForLastWin32Error(this._path);
    }

    private Task FlushWriteAsync(CancellationToken cancellationToken)
    {
      if (this._writePos == 0)
        return Task.CompletedTask;
      Task task1 = this.WriteAsyncInternalCore(new ReadOnlyMemory<byte>(this.GetBuffer(), 0, this._writePos), cancellationToken);
      this._writePos = 0;
      Task task2;
      if (!this.HasActiveBufferOperation)
        task2 = task1;
      else
        task2 = Task.WhenAll(this._activeBufferOperation, task1);
      this._activeBufferOperation = task2;
      return task1;
    }

    private void FlushWriteBufferForWriteByte()
    {
      this.FlushWriteBuffer(false);
    }

    private void FlushWriteBuffer(bool calledFromFinalizer = false)
    {
      if (this._writePos == 0)
        return;
      if (this._useAsyncIO)
      {
        Task task = this.FlushWriteAsync(CancellationToken.None);
        if (!calledFromFinalizer)
          task.GetAwaiter().GetResult();
      }
      else
        this.WriteCore(new ReadOnlySpan<byte>(this.GetBuffer(), 0, this._writePos));
      this._writePos = 0;
    }

    private void SetLengthInternal(long value)
    {
      if (this._writePos > 0)
        this.FlushWriteBuffer(false);
      else if (this._readPos < this._readLength)
        this.FlushReadBuffer();
      this._readPos = 0;
      this._readLength = 0;
      if (this._appendStart != -1L && value < this._appendStart)
        throw new IOException(SR.IO_SetLengthAppendTruncate);
      this.SetLengthCore(value);
    }

    private void SetLengthCore(long value)
    {
      long filePosition = this._filePosition;
      this.VerifyOSHandlePosition();
      if (this._filePosition != value)
        this.SeekCore(this._fileHandle, value, SeekOrigin.Begin, false);
      if (!Interop.Kernel32.SetEndOfFile(this._fileHandle))
      {
        int lastWin32Error = Marshal.GetLastWin32Error();
        if (lastWin32Error == 87)
          throw new ArgumentOutOfRangeException(nameof (value), SR.ArgumentOutOfRange_FileLengthTooBig);
        throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error, this._path);
      }
      if (filePosition == value)
        return;
      if (filePosition < value)
        this.SeekCore(this._fileHandle, filePosition, SeekOrigin.Begin, false);
      else
        this.SeekCore(this._fileHandle, 0L, SeekOrigin.End, false);
    }

    private FileStream.FileStreamCompletionSource CompareExchangeCurrentOverlappedOwner(
      FileStream.FileStreamCompletionSource newSource,
      FileStream.FileStreamCompletionSource existingSource)
    {
      return Interlocked.CompareExchange<FileStream.FileStreamCompletionSource>(ref this._currentOverlappedOwner, newSource, existingSource);
    }

    private int ReadSpan(Span<byte> destination)
    {
      bool flag = false;
      int num1 = this._readLength - this._readPos;
      if (num1 == 0)
      {
        if (!this.CanRead)
          throw Error.GetReadNotSupported();
        if (this._writePos > 0)
          this.FlushWriteBuffer(false);
        if (!this.CanSeek || destination.Length >= this._bufferLength)
        {
          int num2 = this.ReadNative(destination);
          this._readPos = 0;
          this._readLength = 0;
          return num2;
        }
        num1 = this.ReadNative((Span<byte>) this.GetBuffer());
        if (num1 == 0)
          return 0;
        flag = num1 < this._bufferLength;
        this._readPos = 0;
        this._readLength = num1;
      }
      if (num1 > destination.Length)
        num1 = destination.Length;
      new ReadOnlySpan<byte>(this.GetBuffer(), this._readPos, num1).CopyTo(destination);
      this._readPos += num1;
      if (!this._isPipe && num1 < destination.Length && !flag)
      {
        int num2 = this.ReadNative(destination.Slice(num1));
        num1 += num2;
        this._readPos = 0;
        this._readLength = 0;
      }
      return num1;
    }

    private int FillReadBufferForReadByte()
    {
      return !this._useAsyncIO ? this.ReadNative((Span<byte>) this._buffer) : this.ReadNativeAsync(new Memory<byte>(this._buffer), 0, CancellationToken.None).GetAwaiter().GetResult();
    }

    private unsafe int ReadNative(Span<byte> buffer)
    {
      this.VerifyOSHandlePosition();
      int errorCode;
      int num = this.ReadFileNative(this._fileHandle, buffer, (NativeOverlapped*) null, out errorCode);
      if (num == -1)
      {
        if (errorCode == 109)
        {
          num = 0;
        }
        else
        {
          if (errorCode == 87)
            throw new ArgumentException(SR.Arg_HandleNotSync, "_fileHandle");
          throw Win32Marshal.GetExceptionForWin32Error(errorCode, this._path);
        }
      }
      this._filePosition += (long) num;
      return num;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      if (origin < SeekOrigin.Begin || origin > SeekOrigin.End)
        throw new ArgumentException(SR.Argument_InvalidSeekOrigin, nameof (origin));
      if (this._fileHandle.IsClosed)
        throw Error.GetFileNotOpen();
      if (!this.CanSeek)
        throw Error.GetSeekNotSupported();
      if (this._writePos > 0)
        this.FlushWriteBuffer(false);
      else if (origin == SeekOrigin.Current)
        offset -= (long) (this._readLength - this._readPos);
      this._readPos = this._readLength = 0;
      this.VerifyOSHandlePosition();
      long offset1 = this._filePosition + (long) (this._readPos - this._readLength);
      long num1 = this.SeekCore(this._fileHandle, offset, origin, false);
      if (this._appendStart != -1L && num1 < this._appendStart)
      {
        this.SeekCore(this._fileHandle, offset1, SeekOrigin.Begin, false);
        throw new IOException(SR.IO_SeekAppendOverwrite);
      }
      if (this._readLength > 0)
      {
        if (offset1 == num1)
        {
          if (this._readPos > 0)
          {
            Buffer.BlockCopy((Array) this.GetBuffer(), this._readPos, (Array) this.GetBuffer(), 0, this._readLength - this._readPos);
            this._readLength -= this._readPos;
            this._readPos = 0;
          }
          if (this._readLength > 0)
            this.SeekCore(this._fileHandle, (long) this._readLength, SeekOrigin.Current, false);
        }
        else if (offset1 - (long) this._readPos < num1 && num1 < offset1 + (long) this._readLength - (long) this._readPos)
        {
          int num2 = (int) (num1 - offset1);
          Buffer.BlockCopy((Array) this.GetBuffer(), this._readPos + num2, (Array) this.GetBuffer(), 0, this._readLength - (this._readPos + num2));
          this._readLength -= this._readPos + num2;
          this._readPos = 0;
          if (this._readLength > 0)
            this.SeekCore(this._fileHandle, (long) this._readLength, SeekOrigin.Current, false);
        }
        else
        {
          this._readPos = 0;
          this._readLength = 0;
        }
      }
      return num1;
    }

    private long SeekCore(
      SafeFileHandle fileHandle,
      long offset,
      SeekOrigin origin,
      bool closeInvalidHandle = false)
    {
      long lpNewFilePointer;
      if (!Interop.Kernel32.SetFilePointerEx(fileHandle, offset, out lpNewFilePointer, (uint) origin))
      {
        if (closeInvalidHandle)
          throw Win32Marshal.GetExceptionForWin32Error(this.GetLastWin32ErrorAndDisposeHandleIfInvalid(), this._path);
        throw Win32Marshal.GetExceptionForLastWin32Error(this._path);
      }
      this._filePosition = lpNewFilePointer;
      return lpNewFilePointer;
    }

    private void WriteSpan(ReadOnlySpan<byte> source)
    {
      if (this._writePos == 0)
      {
        if (!this.CanWrite)
          throw Error.GetWriteNotSupported();
        if (this._readPos < this._readLength)
          this.FlushReadBuffer();
        this._readPos = 0;
        this._readLength = 0;
      }
      if (this._writePos > 0)
      {
        int num = this._bufferLength - this._writePos;
        if (num > 0)
        {
          if (num >= source.Length)
          {
            source.CopyTo(this.GetBuffer().AsSpan<byte>(this._writePos));
            this._writePos += source.Length;
            return;
          }
          source.Slice(0, num).CopyTo(this.GetBuffer().AsSpan<byte>(this._writePos));
          this._writePos += num;
          source = source.Slice(num);
        }
        this.WriteCore(new ReadOnlySpan<byte>(this.GetBuffer(), 0, this._writePos));
        this._writePos = 0;
      }
      if (source.Length >= this._bufferLength)
      {
        this.WriteCore(source);
      }
      else
      {
        if (source.Length == 0)
          return;
        source.CopyTo(this.GetBuffer().AsSpan<byte>(this._writePos));
        this._writePos = source.Length;
      }
    }

    private unsafe void WriteCore(ReadOnlySpan<byte> source)
    {
      this.VerifyOSHandlePosition();
      int errorCode = 0;
      int num = this.WriteFileNative(this._fileHandle, source, (NativeOverlapped*) null, out errorCode);
      if (num == -1)
      {
        if (errorCode == 232)
        {
          num = 0;
        }
        else
        {
          if (errorCode == 87)
            throw new IOException(SR.IO_FileTooLongOrHandleNotSync);
          throw Win32Marshal.GetExceptionForWin32Error(errorCode, this._path);
        }
      }
      this._filePosition += (long) num;
    }

    private Task<int> ReadAsyncInternal(
      Memory<byte> destination,
      CancellationToken cancellationToken,
      out int synchronousResult)
    {
      if (!this.CanRead)
        throw Error.GetReadNotSupported();
      if (this._isPipe)
      {
        if (this._readPos < this._readLength)
        {
          int length = Math.Min(this._readLength - this._readPos, destination.Length);
          new Span<byte>(this.GetBuffer(), this._readPos, length).CopyTo(destination.Span);
          this._readPos += length;
          synchronousResult = length;
          return (Task<int>) null;
        }
        synchronousResult = 0;
        return this.ReadNativeAsync(destination, 0, cancellationToken);
      }
      if (this._writePos > 0)
        this.FlushWriteBuffer(false);
      if (this._readPos == this._readLength)
      {
        if (destination.Length < this._bufferLength)
        {
          this._readLength = this.ReadNativeAsync(new Memory<byte>(this.GetBuffer()), 0, cancellationToken).GetAwaiter().GetResult();
          int length = Math.Min(this._readLength, destination.Length);
          new Span<byte>(this.GetBuffer(), 0, length).CopyTo(destination.Span);
          this._readPos = length;
          synchronousResult = length;
          return (Task<int>) null;
        }
        this._readPos = 0;
        this._readLength = 0;
        synchronousResult = 0;
        return this.ReadNativeAsync(destination, 0, cancellationToken);
      }
      int num = Math.Min(this._readLength - this._readPos, destination.Length);
      new Span<byte>(this.GetBuffer(), this._readPos, num).CopyTo(destination.Span);
      this._readPos += num;
      if (num == destination.Length)
      {
        synchronousResult = num;
        return (Task<int>) null;
      }
      this._readPos = 0;
      this._readLength = 0;
      synchronousResult = 0;
      return this.ReadNativeAsync(destination.Slice(num), num, cancellationToken);
    }

    private unsafe Task<int> ReadNativeAsync(
      Memory<byte> destination,
      int numBufferedBytesRead,
      CancellationToken cancellationToken)
    {
      FileStream.FileStreamCompletionSource completionSource = FileStream.FileStreamCompletionSource.Create(this, numBufferedBytesRead, (ReadOnlyMemory<byte>) destination);
      NativeOverlapped* overlapped = completionSource.Overlapped;
      if (this.CanSeek)
      {
        long length = this.Length;
        this.VerifyOSHandlePosition();
        if (this._filePosition + (long) destination.Length > length)
          destination = this._filePosition > length ? new Memory<byte>() : destination.Slice(0, (int) (length - this._filePosition));
        overlapped->OffsetLow = (int) this._filePosition;
        overlapped->OffsetHigh = (int) (this._filePosition >> 32);
        this.SeekCore(this._fileHandle, (long) destination.Length, SeekOrigin.Current, false);
      }
      int errorCode = 0;
      if (this.ReadFileNative(this._fileHandle, destination.Span, overlapped, out errorCode) == -1)
      {
        switch (errorCode)
        {
          case 109:
            overlapped->InternalLow = IntPtr.Zero;
            completionSource.SetCompletedSynchronously(0);
            break;
          case 997:
            if (cancellationToken.CanBeCanceled)
            {
              completionSource.RegisterForCancellation(cancellationToken);
              break;
            }
            break;
          default:
            if (!this._fileHandle.IsClosed && this.CanSeek)
              this.SeekCore(this._fileHandle, 0L, SeekOrigin.Current, false);
            completionSource.ReleaseNativeResource();
            if (errorCode == 38)
              throw Error.GetEndOfFile();
            throw Win32Marshal.GetExceptionForWin32Error(errorCode, this._path);
        }
      }
      return completionSource.Task;
    }

    private ValueTask WriteAsyncInternal(
      ReadOnlyMemory<byte> source,
      CancellationToken cancellationToken)
    {
      if (!this.CanWrite)
        throw Error.GetWriteNotSupported();
      bool flag = false;
      if (!this._isPipe)
      {
        if (this._writePos == 0)
        {
          if (this._readPos < this._readLength)
            this.FlushReadBuffer();
          this._readPos = 0;
          this._readLength = 0;
        }
        int num = this._bufferLength - this._writePos;
        if (source.Length < this._bufferLength && !this.HasActiveBufferOperation && source.Length <= num)
        {
          source.Span.CopyTo(new Span<byte>(this.GetBuffer(), this._writePos, source.Length));
          this._writePos += source.Length;
          flag = true;
          if (source.Length != num)
            return new ValueTask();
        }
      }
      Task task1 = (Task) null;
      if (this._writePos > 0)
      {
        task1 = this.FlushWriteAsync(cancellationToken);
        if (flag || task1.IsFaulted || task1.IsCanceled)
          return new ValueTask(task1);
      }
      Task task2 = this.WriteAsyncInternalCore(source, cancellationToken);
      Task task3;
      if (task1 != null && task1.Status != TaskStatus.RanToCompletion)
      {
        if (task2.Status != TaskStatus.RanToCompletion)
          task3 = Task.WhenAll(task1, task2);
        else
          task3 = task1;
      }
      else
        task3 = task2;
      return new ValueTask(task3);
    }

    private unsafe Task WriteAsyncInternalCore(
      ReadOnlyMemory<byte> source,
      CancellationToken cancellationToken)
    {
      FileStream.FileStreamCompletionSource completionSource = FileStream.FileStreamCompletionSource.Create(this, 0, source);
      NativeOverlapped* overlapped = completionSource.Overlapped;
      if (this.CanSeek)
      {
        long length = this.Length;
        this.VerifyOSHandlePosition();
        if (this._filePosition + (long) source.Length > length)
          this.SetLengthCore(this._filePosition + (long) source.Length);
        overlapped->OffsetLow = (int) this._filePosition;
        overlapped->OffsetHigh = (int) (this._filePosition >> 32);
        this.SeekCore(this._fileHandle, (long) source.Length, SeekOrigin.Current, false);
      }
      int errorCode = 0;
      if (this.WriteFileNative(this._fileHandle, source.Span, overlapped, out errorCode) == -1)
      {
        switch (errorCode)
        {
          case 232:
            completionSource.SetCompletedSynchronously(0);
            return Task.CompletedTask;
          case 997:
            if (cancellationToken.CanBeCanceled)
            {
              completionSource.RegisterForCancellation(cancellationToken);
              break;
            }
            break;
          default:
            if (!this._fileHandle.IsClosed && this.CanSeek)
              this.SeekCore(this._fileHandle, 0L, SeekOrigin.Current, false);
            completionSource.ReleaseNativeResource();
            if (errorCode == 38)
              throw Error.GetEndOfFile();
            throw Win32Marshal.GetExceptionForWin32Error(errorCode, this._path);
        }
      }
      return (Task) completionSource.Task;
    }

    private unsafe int ReadFileNative(
      SafeFileHandle handle,
      Span<byte> bytes,
      NativeOverlapped* overlapped,
      out int errorCode)
    {
      int numBytesRead = 0;
      int num;
      fixed (byte* bytes1 = &MemoryMarshal.GetReference<byte>(bytes))
        num = this._useAsyncIO ? Interop.Kernel32.ReadFile((SafeHandle) handle, bytes1, bytes.Length, IntPtr.Zero, overlapped) : Interop.Kernel32.ReadFile((SafeHandle) handle, bytes1, bytes.Length, out numBytesRead, IntPtr.Zero);
      if (num == 0)
      {
        errorCode = this.GetLastWin32ErrorAndDisposeHandleIfInvalid();
        return -1;
      }
      errorCode = 0;
      return numBytesRead;
    }

    private unsafe int WriteFileNative(
      SafeFileHandle handle,
      ReadOnlySpan<byte> buffer,
      NativeOverlapped* overlapped,
      out int errorCode)
    {
      int numBytesWritten = 0;
      int num;
      fixed (byte* bytes = &MemoryMarshal.GetReference<byte>(buffer))
        num = this._useAsyncIO ? Interop.Kernel32.WriteFile((SafeHandle) handle, bytes, buffer.Length, IntPtr.Zero, overlapped) : Interop.Kernel32.WriteFile((SafeHandle) handle, bytes, buffer.Length, out numBytesWritten, IntPtr.Zero);
      if (num == 0)
      {
        errorCode = this.GetLastWin32ErrorAndDisposeHandleIfInvalid();
        return -1;
      }
      errorCode = 0;
      return numBytesWritten;
    }

    private int GetLastWin32ErrorAndDisposeHandleIfInvalid()
    {
      int lastWin32Error = Marshal.GetLastWin32Error();
      if (lastWin32Error == 6)
        this._fileHandle.Dispose();
      return lastWin32Error;
    }

    public override Task CopyToAsync(
      Stream destination,
      int bufferSize,
      CancellationToken cancellationToken)
    {
      if (!this._useAsyncIO || this.GetType() != typeof (FileStream))
        return base.CopyToAsync(destination, bufferSize, cancellationToken);
      StreamHelpers.ValidateCopyToArgs((Stream) this, destination, bufferSize);
      if (cancellationToken.IsCancellationRequested)
        return (Task) Task.FromCanceled<int>(cancellationToken);
      if (this._fileHandle.IsClosed)
        throw Error.GetFileNotOpen();
      return this.AsyncModeCopyToAsync(destination, bufferSize, cancellationToken);
    }

    private unsafe async Task AsyncModeCopyToAsync(
      Stream destination,
      int bufferSize,
      CancellationToken cancellationToken)
    {
      FileStream fileStream = this;
      if (fileStream._writePos > 0)
        await fileStream.FlushWriteAsync(cancellationToken).ConfigureAwait(false);
      if (fileStream.GetBuffer() != null)
      {
        int length = fileStream._readLength - fileStream._readPos;
        if (length > 0)
        {
          await destination.WriteAsync(new ReadOnlyMemory<byte>(fileStream.GetBuffer(), fileStream._readPos, length), cancellationToken).ConfigureAwait(false);
          fileStream._readPos = fileStream._readLength = 0;
        }
      }
      FileStream.AsyncCopyToAwaitable readAwaitable = new FileStream.AsyncCopyToAwaitable(fileStream);
      bool canSeek = fileStream.CanSeek;
      if (canSeek)
      {
        fileStream.VerifyOSHandlePosition();
        readAwaitable._position = fileStream._filePosition;
      }
      byte[] copyBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
      PreAllocatedOverlapped awaitableOverlapped = new PreAllocatedOverlapped(FileStream.AsyncCopyToAwaitable.s_callback, (object) readAwaitable, (object) copyBuffer);
      CancellationTokenRegistration cancellationReg = new CancellationTokenRegistration();
      try
      {
        if (cancellationToken.CanBeCanceled)
          cancellationReg = cancellationToken.UnsafeRegister((Action<object>) (s =>
          {
            FileStream.AsyncCopyToAwaitable asyncCopyToAwaitable = (FileStream.AsyncCopyToAwaitable) s;
            lock (asyncCopyToAwaitable.CancellationLock)
            {
              if ((IntPtr) asyncCopyToAwaitable._nativeOverlapped == IntPtr.Zero)
                return;
              Interop.Kernel32.CancelIoEx((SafeHandle) asyncCopyToAwaitable._fileStream._fileHandle, asyncCopyToAwaitable._nativeOverlapped);
            }
          }), (object) readAwaitable);
        while (true)
        {
          cancellationToken.ThrowIfCancellationRequested();
          readAwaitable.ResetForNextOperation();
          try
          {
            readAwaitable._nativeOverlapped = fileStream._fileHandle.ThreadPoolBinding.AllocateNativeOverlapped(awaitableOverlapped);
            if (canSeek)
            {
              readAwaitable._nativeOverlapped->OffsetLow = (int) readAwaitable._position;
              readAwaitable._nativeOverlapped->OffsetHigh = (int) (readAwaitable._position >> 32);
            }
            int errorCode;
            if (fileStream.ReadFileNative(fileStream._fileHandle, (Span<byte>) copyBuffer, readAwaitable._nativeOverlapped, out errorCode) < 0)
            {
              if (errorCode != 38 && errorCode != 109)
              {
                if (errorCode != 997)
                  throw Win32Marshal.GetExceptionForWin32Error(errorCode, fileStream._path);
              }
              else
                readAwaitable.MarkCompleted();
            }
            await readAwaitable;
            switch (readAwaitable._errorCode)
            {
              case 0:
              case 38:
              case 109:
                int numBytes = (int) readAwaitable._numBytes;
                if (numBytes != 0)
                {
                  if (canSeek)
                  {
                    readAwaitable._position += (long) numBytes;
                    break;
                  }
                  break;
                }
                goto label_38;
              case 995:
                throw new OperationCanceledException(cancellationToken.IsCancellationRequested ? cancellationToken : new CancellationToken(true));
              default:
                throw Win32Marshal.GetExceptionForWin32Error((int) readAwaitable._errorCode, fileStream._path);
            }
          }
          finally
          {
            NativeOverlapped* nativeOverlapped;
            lock (readAwaitable.CancellationLock)
            {
              nativeOverlapped = readAwaitable._nativeOverlapped;
              readAwaitable._nativeOverlapped = (NativeOverlapped*) null;
            }
            if ((IntPtr) nativeOverlapped != IntPtr.Zero)
              fileStream._fileHandle.ThreadPoolBinding.FreeNativeOverlapped(nativeOverlapped);
          }
          await destination.WriteAsync(new ReadOnlyMemory<byte>(copyBuffer, 0, (int) readAwaitable._numBytes), cancellationToken).ConfigureAwait(false);
        }
      }
      finally
      {
        cancellationReg.Dispose();
        awaitableOverlapped.Dispose();
        ArrayPool<byte>.Shared.Return(copyBuffer, false);
        if (!fileStream._fileHandle.IsClosed && fileStream.CanSeek)
          fileStream.SeekCore(fileStream._fileHandle, 0L, SeekOrigin.End, false);
      }
label_38:;
    }

    private Task FlushAsyncInternal(CancellationToken cancellationToken)
    {
      if (cancellationToken.IsCancellationRequested)
        return Task.FromCanceled(cancellationToken);
      if (this._fileHandle.IsClosed)
        throw Error.GetFileNotOpen();
      try
      {
        this.FlushInternalBuffer();
      }
      catch (Exception ex)
      {
        return Task.FromException(ex);
      }
      return Task.CompletedTask;
    }

    private void LockInternal(long position, long length)
    {
      if (!Interop.Kernel32.LockFile(this._fileHandle, (int) position, (int) (position >> 32), (int) length, (int) (length >> 32)))
        throw Win32Marshal.GetExceptionForLastWin32Error(this._path);
    }

    private void UnlockInternal(long position, long length)
    {
      if (!Interop.Kernel32.UnlockFile(this._fileHandle, (int) position, (int) (position >> 32), (int) length, (int) (length >> 32)))
        throw Win32Marshal.GetExceptionForLastWin32Error(this._path);
    }

    private SafeFileHandle ValidateFileHandle(SafeFileHandle fileHandle)
    {
      if (fileHandle.IsInvalid)
      {
        int errorCode = Marshal.GetLastWin32Error();
        if (errorCode == 3 && this._path.Length == PathInternal.GetRootLength((ReadOnlySpan<char>) this._path))
          errorCode = 5;
        throw Win32Marshal.GetExceptionForWin32Error(errorCode, this._path);
      }
      fileHandle.IsAsync = new bool?(this._useAsyncIO);
      return fileHandle;
    }

    private SafeFileHandle OpenHandle(
      FileMode mode,
      FileShare share,
      FileOptions options)
    {
      return this.CreateFileOpenHandle(mode, share, options);
    }

    private SafeFileHandle CreateFileOpenHandle(
      FileMode mode,
      FileShare share,
      FileOptions options)
    {
      Interop.Kernel32.SECURITY_ATTRIBUTES secAttrs = FileStream.GetSecAttrs(share);
      int dwDesiredAccess = ((this._access & FileAccess.Read) == FileAccess.Read ? int.MinValue : 0) | ((this._access & FileAccess.Write) == FileAccess.Write ? 1073741824 : 0);
      share &= ~FileShare.Inheritable;
      if (mode == FileMode.Append)
        mode = FileMode.OpenOrCreate;
      int dwFlagsAndAttributes = (int) (options | (FileOptions) 1048576);
      using (DisableMediaInsertionPrompt.Create())
        return this.ValidateFileHandle(Interop.Kernel32.CreateFile(this._path, dwDesiredAccess, share, ref secAttrs, mode, dwFlagsAndAttributes, IntPtr.Zero));
    }

    private static bool GetDefaultIsAsync(SafeFileHandle handle)
    {
      bool? isAsync = handle.IsAsync;
      if (isAsync.HasValue)
        return isAsync.GetValueOrDefault();
      bool? nullable = FileStream.IsHandleSynchronous(handle, true);
      return (nullable.HasValue ? new bool?(!nullable.GetValueOrDefault()) : new bool?()).GetValueOrDefault();
    }

    private static unsafe bool? IsHandleSynchronous(SafeFileHandle fileHandle, bool ignoreInvalid)
    {
      if (fileHandle.IsInvalid)
        return new bool?();
      uint num;
      switch (Interop.NtDll.NtQueryInformationFile(fileHandle, out Interop.NtDll.IO_STATUS_BLOCK _, (void*) &num, 4U, 16U))
      {
        case -1073741816:
          if (!ignoreInvalid)
            throw Win32Marshal.GetExceptionForWin32Error(6, "");
          return new bool?();
        case 0:
          return new bool?((num & 48U) > 0U);
        default:
          return new bool?();
      }
    }

    private static void VerifyHandleIsSync(SafeFileHandle handle, int fileType, FileAccess access)
    {
      if (!handle.IsAsync.HasValue)
        return;
      bool? nullable = FileStream.IsHandleSynchronous(handle, false);
      if ((nullable.HasValue ? (nullable.GetValueOrDefault() ? 1 : 0) : 1) == 0)
        throw new ArgumentException(SR.Arg_HandleNotSync, nameof (handle));
    }

    private sealed class AsyncCopyToAwaitable : ICriticalNotifyCompletion, INotifyCompletion
    {
      private static readonly Action s_sentinel = (Action) (() => {});
      internal static readonly IOCompletionCallback s_callback = new IOCompletionCallback(FileStream.AsyncCopyToAwaitable.IOCallback);
      internal readonly FileStream _fileStream;
      internal long _position;
      internal unsafe NativeOverlapped* _nativeOverlapped;
      internal Action _continuation;
      internal uint _errorCode;
      internal uint _numBytes;

      internal object CancellationLock
      {
        get
        {
          return (object) this;
        }
      }

      internal AsyncCopyToAwaitable(FileStream fileStream)
      {
        this._fileStream = fileStream;
      }

      internal void ResetForNextOperation()
      {
        this._continuation = (Action) null;
        this._errorCode = 0U;
        this._numBytes = 0U;
      }

      internal static unsafe void IOCallback(
        uint errorCode,
        uint numBytes,
        NativeOverlapped* pOVERLAP)
      {
        FileStream.AsyncCopyToAwaitable nativeOverlappedState = (FileStream.AsyncCopyToAwaitable) ThreadPoolBoundHandle.GetNativeOverlappedState(pOVERLAP);
        nativeOverlappedState._errorCode = errorCode;
        nativeOverlappedState._numBytes = numBytes;
        Action action = nativeOverlappedState._continuation ?? Interlocked.CompareExchange<Action>(ref nativeOverlappedState._continuation, FileStream.AsyncCopyToAwaitable.s_sentinel, (Action) null);
        if (action == null)
          return;
        action();
      }

      internal void MarkCompleted()
      {
        this._continuation = FileStream.AsyncCopyToAwaitable.s_sentinel;
      }

      public FileStream.AsyncCopyToAwaitable GetAwaiter()
      {
        return this;
      }

      public bool IsCompleted
      {
        get
        {
          return this._continuation == FileStream.AsyncCopyToAwaitable.s_sentinel;
        }
      }

      public void GetResult()
      {
      }

      public void OnCompleted(Action continuation)
      {
        this.UnsafeOnCompleted(continuation);
      }

      public void UnsafeOnCompleted(Action continuation)
      {
        if (this._continuation != FileStream.AsyncCopyToAwaitable.s_sentinel && Interlocked.CompareExchange<Action>(ref this._continuation, continuation, (Action) null) == null)
          return;
        Task.Run(continuation);
      }
    }

    private class FileStreamCompletionSource : TaskCompletionSource<int>
    {
      private static Action<object> s_cancelCallback;
      private readonly FileStream _stream;
      private readonly int _numBufferedBytes;
      private CancellationTokenRegistration _cancellationRegistration;
      private unsafe NativeOverlapped* _overlapped;
      private long _result;

      protected unsafe FileStreamCompletionSource(
        FileStream stream,
        int numBufferedBytes,
        byte[] bytes)
        : base(TaskCreationOptions.RunContinuationsAsynchronously)
      {
        this._numBufferedBytes = numBufferedBytes;
        this._stream = stream;
        this._result = 0L;
        this._overlapped = bytes == null || this._stream.CompareExchangeCurrentOverlappedOwner(this, (FileStream.FileStreamCompletionSource) null) != null ? this._stream._fileHandle.ThreadPoolBinding.AllocateNativeOverlapped(FileStream.s_ioCallback, (object) this, (object) bytes) : this._stream._fileHandle.ThreadPoolBinding.AllocateNativeOverlapped(this._stream._preallocatedOverlapped);
      }

      internal unsafe NativeOverlapped* Overlapped
      {
        get
        {
          return this._overlapped;
        }
      }

      public void SetCompletedSynchronously(int numBytes)
      {
        this.ReleaseNativeResource();
        this.TrySetResult(numBytes + this._numBufferedBytes);
      }

      public unsafe void RegisterForCancellation(CancellationToken cancellationToken)
      {
        if ((IntPtr) this._overlapped == IntPtr.Zero)
          return;
        Action<object> callback = FileStream.FileStreamCompletionSource.s_cancelCallback;
        if (callback == null)
          FileStream.FileStreamCompletionSource.s_cancelCallback = callback = new Action<object>(FileStream.FileStreamCompletionSource.Cancel);
        long num = Interlocked.CompareExchange(ref this._result, 17179869184L, 0L);
        switch (num)
        {
          case 0:
            this._cancellationRegistration = cancellationToken.UnsafeRegister(callback, (object) this);
            num = Interlocked.Exchange(ref this._result, 0L);
            goto case 34359738368;
          case 34359738368:
            if (num == 0L || num == 34359738368L || num == 17179869184L)
              break;
            this.CompleteCallback((ulong) num);
            break;
          default:
            num = Interlocked.Exchange(ref this._result, 0L);
            goto case 34359738368;
        }
      }

      internal virtual unsafe void ReleaseNativeResource()
      {
        this._cancellationRegistration.Dispose();
        if ((IntPtr) this._overlapped != IntPtr.Zero)
        {
          this._stream._fileHandle.ThreadPoolBinding.FreeNativeOverlapped(this._overlapped);
          this._overlapped = (NativeOverlapped*) null;
        }
        this._stream.CompareExchangeCurrentOverlappedOwner((FileStream.FileStreamCompletionSource) null, this);
      }

      internal static unsafe void IOCallback(
        uint errorCode,
        uint numBytes,
        NativeOverlapped* pOverlapped)
      {
        object nativeOverlappedState = ThreadPoolBoundHandle.GetNativeOverlappedState(pOverlapped);
        FileStream.FileStreamCompletionSource completionSource = nativeOverlappedState is FileStream fileStream ? fileStream._currentOverlappedOwner : (FileStream.FileStreamCompletionSource) nativeOverlappedState;
        ulong packedResult = errorCode == 0U || errorCode == 109U || errorCode == 232U ? 4294967296UL | (ulong) numBytes : 8589934592UL | (ulong) errorCode;
        if (Interlocked.Exchange(ref completionSource._result, (long) packedResult) != 0L || Interlocked.Exchange(ref completionSource._result, 34359738368L) == 0L)
          return;
        completionSource.CompleteCallback(packedResult);
      }

      private void CompleteCallback(ulong packedResult)
      {
        CancellationToken token = this._cancellationRegistration.Token;
        this.ReleaseNativeResource();
        if (((long) packedResult & -4294967296L) == 8589934592L)
        {
          int errorCode = (int) ((long) packedResult & (long) uint.MaxValue);
          if (errorCode == 995)
            this.TrySetCanceled(token.IsCancellationRequested ? token : new CancellationToken(true));
          else
            this.TrySetException(Win32Marshal.GetExceptionForWin32Error(errorCode, ""));
        }
        else
          this.TrySetResult((int) ((long) packedResult & (long) uint.MaxValue) + this._numBufferedBytes);
      }

      private static unsafe void Cancel(object state)
      {
        FileStream.FileStreamCompletionSource completionSource = (FileStream.FileStreamCompletionSource) state;
        if (completionSource._stream._fileHandle.IsInvalid || Interop.Kernel32.CancelIoEx((SafeHandle) completionSource._stream._fileHandle, completionSource._overlapped))
          return;
        int lastWin32Error = Marshal.GetLastWin32Error();
        if (lastWin32Error != 1168)
          throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error, "");
      }

      public static FileStream.FileStreamCompletionSource Create(
        FileStream stream,
        int numBufferedBytesRead,
        ReadOnlyMemory<byte> memory)
      {
        ArraySegment<byte> segment;
        return !MemoryMarshal.TryGetArray<byte>(memory, out segment) || segment.Array != stream._buffer ? (FileStream.FileStreamCompletionSource) new FileStream.MemoryFileStreamCompletionSource(stream, numBufferedBytesRead, memory) : new FileStream.FileStreamCompletionSource(stream, numBufferedBytesRead, segment.Array);
      }
    }

    private sealed class MemoryFileStreamCompletionSource : FileStream.FileStreamCompletionSource
    {
      private MemoryHandle _handle;

      internal MemoryFileStreamCompletionSource(
        FileStream stream,
        int numBufferedBytes,
        ReadOnlyMemory<byte> memory)
        : base(stream, numBufferedBytes, (byte[]) null)
      {
        this._handle = memory.Pin();
      }

      internal override void ReleaseNativeResource()
      {
        this._handle.Dispose();
        base.ReleaseNativeResource();
      }
    }
  }
}

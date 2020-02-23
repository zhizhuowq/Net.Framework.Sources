// Decompiled with JetBrains decompiler
// Type: System.Net.Http.HttpContent
// Assembly: System.Net.Http, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: D8D12B97-8555-48F6-96B3-BBC6BF89FCBA
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Net.Http.dll

using System.Buffers;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http
{
  /// <summary>表示 HTTP 实体正文和内容标头的基类。</summary>
  public abstract class HttpContent : IDisposable
  {
    internal static readonly Encoding DefaultStringEncoding = Encoding.UTF8;
    private HttpContentHeaders _headers;
    private MemoryStream _bufferedContent;
    private object _contentReadStream;
    private bool _disposed;
    private bool _canCalculateLength;

    /// <summary>获取 RFC 2616 中定义的 HTTP 内容标头。</summary>
    /// <returns>RFC 2616 中定义的内容标头。</returns>
    public HttpContentHeaders Headers
    {
      get
      {
        if (this._headers == null)
          this._headers = new HttpContentHeaders(this);
        return this._headers;
      }
    }

    private bool IsBuffered
    {
      get
      {
        return this._bufferedContent != null;
      }
    }

    internal void SetBuffer(byte[] buffer, int offset, int count)
    {
      this._bufferedContent = new MemoryStream(buffer, offset, count, false, true);
    }

    internal bool TryGetBuffer(out ArraySegment<byte> buffer)
    {
      if (this._bufferedContent != null)
        return this._bufferedContent.TryGetBuffer(out buffer);
      buffer = new ArraySegment<byte>();
      return false;
    }

    /// <summary>初始化 <see cref="T:System.Net.Http.HttpContent" /> 类的新实例。</summary>
    protected HttpContent()
    {
      if (NetEventSource.IsEnabled)
        NetEventSource.Enter((object) this, (FormattableString) null, ".ctor");
      this._canCalculateLength = true;
      if (!NetEventSource.IsEnabled)
        return;
      NetEventSource.Exit((object) this, (FormattableString) null, ".ctor");
    }

    /// <summary>将 HTTP 内容序列化到字符串，此为异步操作。</summary>
    /// <returns>表示异步操作的任务对象。</returns>
    public Task<string> ReadAsStringAsync()
    {
      this.CheckDisposed();
      return HttpContent.WaitAndReturnAsync<HttpContent, string>(this.LoadIntoBufferAsync(), this, (Func<HttpContent, string>) (s => s.ReadBufferedContentAsString()));
    }

    private string ReadBufferedContentAsString()
    {
      if (this._bufferedContent.Length == 0L)
        return string.Empty;
      ArraySegment<byte> buffer;
      if (!this.TryGetBuffer(out buffer))
        buffer = new ArraySegment<byte>(this._bufferedContent.ToArray());
      return HttpContent.ReadBufferAsString(buffer, this.Headers);
    }

    internal static string ReadBufferAsString(ArraySegment<byte> buffer, HttpContentHeaders headers)
    {
      Encoding encoding = (Encoding) null;
      int preambleLength = -1;
      string charSet = headers.ContentType?.CharSet;
      if (charSet != null)
      {
        try
        {
          encoding = charSet.Length <= 2 || charSet[0] != '"' || charSet[charSet.Length - 1] != '"' ? Encoding.GetEncoding(charSet) : Encoding.GetEncoding(charSet.Substring(1, charSet.Length - 2));
          preambleLength = HttpContent.GetPreambleLength(buffer, encoding);
        }
        catch (ArgumentException ex)
        {
          throw new InvalidOperationException(SR.net_http_content_invalid_charset, (Exception) ex);
        }
      }
      if (encoding == null && !HttpContent.TryDetectEncoding(buffer, out encoding, out preambleLength))
      {
        encoding = HttpContent.DefaultStringEncoding;
        preambleLength = 0;
      }
      return encoding.GetString(buffer.Array, buffer.Offset + preambleLength, buffer.Count - preambleLength);
    }

    /// <summary>将 HTTP 内容序列化到字节数组，此为异步操作。</summary>
    /// <returns>表示异步操作的任务对象。</returns>
    public Task<byte[]> ReadAsByteArrayAsync()
    {
      this.CheckDisposed();
      return HttpContent.WaitAndReturnAsync<HttpContent, byte[]>(this.LoadIntoBufferAsync(), this, (Func<HttpContent, byte[]>) (s => s.ReadBufferedContentAsByteArray()));
    }

    internal byte[] ReadBufferedContentAsByteArray()
    {
      return this._bufferedContent.ToArray();
    }

    /// <summary>将 HTTP 内容序列化并返回将内容表示为异步操作的流。</summary>
    /// <returns>表示异步操作的任务对象。</returns>
    public Task<Stream> ReadAsStreamAsync()
    {
      this.CheckDisposed();
      if (this._contentReadStream == null)
      {
        ArraySegment<byte> buffer;
        Task<Stream> task = this.TryGetBuffer(out buffer) ? Task.FromResult<Stream>((Stream) new MemoryStream(buffer.Array, buffer.Offset, buffer.Count, false)) : this.CreateContentReadStreamAsync();
        this._contentReadStream = (object) task;
        return task;
      }
      if (this._contentReadStream is Task<Stream> contentReadStream)
        return contentReadStream;
      Task<Stream> task1 = Task.FromResult<Stream>((Stream) this._contentReadStream);
      this._contentReadStream = (object) task1;
      return task1;
    }

    internal Stream TryReadAsStream()
    {
      this.CheckDisposed();
      if (this._contentReadStream == null)
      {
        ArraySegment<byte> buffer;
        Stream stream = this.TryGetBuffer(out buffer) ? (Stream) new MemoryStream(buffer.Array, buffer.Offset, buffer.Count, false) : this.TryCreateContentReadStream();
        this._contentReadStream = (object) stream;
        return stream;
      }
      if (this._contentReadStream is Stream contentReadStream)
        return contentReadStream;
      Task<Stream> contentReadStream1 = (Task<Stream>) this._contentReadStream;
      return contentReadStream1.Status != TaskStatus.RanToCompletion ? (Stream) null : contentReadStream1.Result;
    }

    /// <summary>将 HTTP 内容序列化到流，此为异步操作。</summary>
    /// <param name="stream">目标流。</param>
    /// <param name="context">有关传输的信息（例如信道绑定令牌）。 此参数可以为 <see langword="null" />。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    protected abstract Task SerializeToStreamAsync(Stream stream, TransportContext context);

    internal virtual Task SerializeToStreamAsync(
      Stream stream,
      TransportContext context,
      CancellationToken cancellationToken)
    {
      return this.SerializeToStreamAsync(stream, context);
    }

    internal virtual bool AllowDuplex
    {
      get
      {
        return true;
      }
    }

    /// <summary>将 HTTP 内容序列化为字节流，并将其复制到作为 <paramref name="stream" /> 参数提供的流对象。</summary>
    /// <param name="stream">目标流。</param>
    /// <param name="context">有关传输的信息（例如信道绑定令牌）。 此参数可以为 <see langword="null" />。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    public Task CopyToAsync(Stream stream, TransportContext context)
    {
      return this.CopyToAsync(stream, context, CancellationToken.None);
    }

    internal Task CopyToAsync(
      Stream stream,
      TransportContext context,
      CancellationToken cancellationToken)
    {
      this.CheckDisposed();
      if (stream == null)
        throw new ArgumentNullException(nameof (stream));
      try
      {
        ArraySegment<byte> buffer;
        if (this.TryGetBuffer(out buffer))
          return HttpContent.CopyToAsyncCore(stream.WriteAsync(new ReadOnlyMemory<byte>(buffer.Array, buffer.Offset, buffer.Count), cancellationToken));
        Task streamAsync = this.SerializeToStreamAsync(stream, context, cancellationToken);
        this.CheckTaskNotNull(streamAsync);
        return HttpContent.CopyToAsyncCore(new ValueTask(streamAsync));
      }
      catch (Exception ex) when (HttpContent.StreamCopyExceptionNeedsWrapping(ex))
      {
        return Task.FromException(HttpContent.GetStreamCopyException(ex));
      }
    }

    private static async Task CopyToAsyncCore(ValueTask copyTask)
    {
      try
      {
        await copyTask.ConfigureAwait(false);
      }
      catch (Exception ex) when (HttpContent.StreamCopyExceptionNeedsWrapping(ex))
      {
        throw HttpContent.WrapStreamCopyException(ex);
      }
    }

    /// <summary>将 HTTP 内容序列化为字节流，并将其复制到作为 <paramref name="stream" /> 参数提供的流对象。</summary>
    /// <param name="stream">目标流。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    public Task CopyToAsync(Stream stream)
    {
      return this.CopyToAsync(stream, (TransportContext) null);
    }

    /// <summary>以异步操作方式将 HTTP 内容序列化到内存缓冲区。</summary>
    /// <returns>表示异步操作的任务对象。</returns>
    public Task LoadIntoBufferAsync()
    {
      return this.LoadIntoBufferAsync((long) int.MaxValue);
    }

    /// <summary>以异步操作方式将 HTTP 内容序列化到内存缓冲区。</summary>
    /// <param name="maxBufferSize">要使用的缓冲区最大大小。（以字节为单位）。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    public Task LoadIntoBufferAsync(long maxBufferSize)
    {
      return this.LoadIntoBufferAsync(maxBufferSize, CancellationToken.None);
    }

    internal Task LoadIntoBufferAsync(long maxBufferSize, CancellationToken cancellationToken)
    {
      this.CheckDisposed();
      if (maxBufferSize > (long) int.MaxValue)
        throw new ArgumentOutOfRangeException(nameof (maxBufferSize), (object) maxBufferSize, SR.Format((IFormatProvider) CultureInfo.InvariantCulture, SR.net_http_content_buffersize_limit, (object) int.MaxValue));
      if (this.IsBuffered)
        return Task.CompletedTask;
      Exception error = (Exception) null;
      MemoryStream memoryStream = this.CreateMemoryStream(maxBufferSize, out error);
      if (memoryStream == null)
        return Task.FromException(error);
      try
      {
        Task streamAsync = this.SerializeToStreamAsync((Stream) memoryStream, (TransportContext) null, cancellationToken);
        this.CheckTaskNotNull(streamAsync);
        return this.LoadIntoBufferAsyncCore(streamAsync, memoryStream);
      }
      catch (Exception ex) when (HttpContent.StreamCopyExceptionNeedsWrapping(ex))
      {
        return Task.FromException(HttpContent.GetStreamCopyException(ex));
      }
    }

    private async Task LoadIntoBufferAsyncCore(
      Task serializeToStreamTask,
      MemoryStream tempBuffer)
    {
      HttpContent httpContent = this;
      try
      {
        await serializeToStreamTask.ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        tempBuffer.Dispose();
        Exception streamCopyException = HttpContent.GetStreamCopyException(ex);
        if (streamCopyException != ex)
          throw streamCopyException;
        throw;
      }
      try
      {
        tempBuffer.Seek(0L, SeekOrigin.Begin);
        httpContent._bufferedContent = tempBuffer;
      }
      catch (Exception ex)
      {
        if (NetEventSource.IsEnabled)
          NetEventSource.Error((object) httpContent, (object) ex, nameof (LoadIntoBufferAsyncCore));
        throw;
      }
    }

    /// <summary>采用异步操作将 HTTP 内容序列化到内存流。</summary>
    /// <returns>表示异步操作的任务对象。</returns>
    protected virtual Task<Stream> CreateContentReadStreamAsync()
    {
      return HttpContent.WaitAndReturnAsync<HttpContent, Stream>(this.LoadIntoBufferAsync(), this, (Func<HttpContent, Stream>) (s => (Stream) s._bufferedContent));
    }

    internal virtual Stream TryCreateContentReadStream()
    {
      return (Stream) null;
    }

    /// <summary>确定 HTTP 内容是否具有有效的长度（以字节为单位）。</summary>
    /// <param name="length">HTTP 内容的长度（以字节为单位）。</param>
    /// <returns>如果 <paramref name="length" /> 是有效长度，则为 <see langword="true" />；否则为 <see langword="false" />。</returns>
    protected internal abstract bool TryComputeLength(out long length);

    internal long? GetComputedOrBufferLength()
    {
      this.CheckDisposed();
      if (this.IsBuffered)
        return new long?(this._bufferedContent.Length);
      if (this._canCalculateLength)
      {
        long length = 0;
        if (this.TryComputeLength(out length))
          return new long?(length);
        this._canCalculateLength = false;
      }
      return new long?();
    }

    private MemoryStream CreateMemoryStream(long maxBufferSize, out Exception error)
    {
      error = (Exception) null;
      long? contentLength = this.Headers.ContentLength;
      if (!contentLength.HasValue)
        return (MemoryStream) new HttpContent.LimitMemoryStream((int) maxBufferSize, 0);
      long? nullable = contentLength;
      long num = maxBufferSize;
      if (!(nullable.GetValueOrDefault() > num & nullable.HasValue))
        return (MemoryStream) new HttpContent.LimitMemoryStream((int) maxBufferSize, (int) contentLength.Value);
      error = (Exception) new HttpRequestException(SR.Format((IFormatProvider) CultureInfo.InvariantCulture, SR.net_http_content_buffersize_exceeded, (object) maxBufferSize));
      return (MemoryStream) null;
    }

    /// <summary>释放由 <see cref="T:System.Net.Http.HttpContent" /> 使用的非托管资源，并可根据需要释放托管资源。</summary>
    /// <param name="disposing">如果释放托管资源和非托管资源，则为 <see langword="true" />；如果仅释放非托管资源，则为 <see langword="false" />。</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!disposing || this._disposed)
        return;
      this._disposed = true;
      if (this._contentReadStream != null)
      {
        if (!(this._contentReadStream is Stream stream))
          stream = !(this._contentReadStream is Task<Stream> contentReadStream) || contentReadStream.Status != TaskStatus.RanToCompletion ? (Stream) null : contentReadStream.Result;
        stream?.Dispose();
        this._contentReadStream = (object) null;
      }
      if (!this.IsBuffered)
        return;
      this._bufferedContent.Dispose();
    }

    /// <summary>释放由 <see cref="T:System.Net.Http.HttpContent" /> 使用的非托管资源和托管资源。</summary>
    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void CheckDisposed()
    {
      if (this._disposed)
        throw new ObjectDisposedException(this.GetType().ToString());
    }

    private void CheckTaskNotNull(Task task)
    {
      if (task == null)
      {
        InvalidOperationException operationException = new InvalidOperationException(SR.net_http_content_no_task_returned);
        if (NetEventSource.IsEnabled)
          NetEventSource.Error((object) this, (object) operationException, nameof (CheckTaskNotNull));
        throw operationException;
      }
    }

    internal static bool StreamCopyExceptionNeedsWrapping(Exception e)
    {
      return e is IOException || e is ObjectDisposedException;
    }

    private static Exception GetStreamCopyException(Exception originalException)
    {
      return !HttpContent.StreamCopyExceptionNeedsWrapping(originalException) ? originalException : HttpContent.WrapStreamCopyException(originalException);
    }

    internal static Exception WrapStreamCopyException(Exception e)
    {
      return (Exception) new HttpRequestException(SR.net_http_content_stream_copy_error, e);
    }

    private static int GetPreambleLength(ArraySegment<byte> buffer, Encoding encoding)
    {
      byte[] array = buffer.Array;
      int offset = buffer.Offset;
      int count = buffer.Count;
      switch (encoding.CodePage)
      {
        case 1200:
          return count < 2 || array[offset] != byte.MaxValue || array[offset + 1] != (byte) 254 ? 0 : 2;
        case 1201:
          return count < 2 || array[offset] != (byte) 254 || array[offset + 1] != byte.MaxValue ? 0 : 2;
        case 12000:
          return count < 4 || array[offset] != byte.MaxValue || (array[offset + 1] != (byte) 254 || array[offset + 2] != (byte) 0) || array[offset + 3] != (byte) 0 ? 0 : 4;
        case 65001:
          return count < 3 || array[offset] != (byte) 239 || (array[offset + 1] != (byte) 187 || array[offset + 2] != (byte) 191) ? 0 : 3;
        default:
          byte[] preamble = encoding.GetPreamble();
          return !HttpContent.BufferHasPrefix(buffer, preamble) ? 0 : preamble.Length;
      }
    }

    private static bool TryDetectEncoding(
      ArraySegment<byte> buffer,
      out Encoding encoding,
      out int preambleLength)
    {
      byte[] array = buffer.Array;
      int offset = buffer.Offset;
      int count = buffer.Count;
      if (count >= 2)
      {
        switch ((int) array[offset] << 8 | (int) array[offset + 1])
        {
          case 61371:
            if (count >= 3 && array[offset + 2] == (byte) 191)
            {
              encoding = Encoding.UTF8;
              preambleLength = 3;
              return true;
            }
            break;
          case 65279:
            encoding = Encoding.BigEndianUnicode;
            preambleLength = 2;
            return true;
          case 65534:
            if (count >= 4 && array[offset + 2] == (byte) 0 && array[offset + 3] == (byte) 0)
            {
              encoding = Encoding.UTF32;
              preambleLength = 4;
            }
            else
            {
              encoding = Encoding.Unicode;
              preambleLength = 2;
            }
            return true;
        }
      }
      encoding = (Encoding) null;
      preambleLength = 0;
      return false;
    }

    private static bool BufferHasPrefix(ArraySegment<byte> buffer, byte[] prefix)
    {
      byte[] array = buffer.Array;
      if (prefix == null || array == null || (prefix.Length > buffer.Count || prefix.Length == 0))
        return false;
      int index = 0;
      int offset = buffer.Offset;
      while (index < prefix.Length)
      {
        if ((int) prefix[index] != (int) array[offset])
          return false;
        ++index;
        ++offset;
      }
      return true;
    }

    private static async Task<TResult> WaitAndReturnAsync<TState, TResult>(
      Task waitTask,
      TState state,
      Func<TState, TResult> returnFunc)
    {
      await waitTask.ConfigureAwait(false);
      return returnFunc(state);
    }

    private static Exception CreateOverCapacityException(int maxBufferSize)
    {
      return (Exception) new HttpRequestException(SR.Format(SR.net_http_content_buffersize_exceeded, (object) maxBufferSize));
    }

    internal sealed class LimitMemoryStream : MemoryStream
    {
      private readonly int _maxSize;

      public LimitMemoryStream(int maxSize, int capacity)
        : base(capacity)
      {
        this._maxSize = maxSize;
      }

      public byte[] GetSizedBuffer()
      {
        ArraySegment<byte> buffer;
        return !this.TryGetBuffer(out buffer) || buffer.Offset != 0 || buffer.Count != buffer.Array.Length ? this.ToArray() : buffer.Array;
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
        this.CheckSize(count);
        base.Write(buffer, offset, count);
      }

      public override void WriteByte(byte value)
      {
        this.CheckSize(1);
        base.WriteByte(value);
      }

      public override Task WriteAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken)
      {
        this.CheckSize(count);
        return base.WriteAsync(buffer, offset, count, cancellationToken);
      }

      public override ValueTask WriteAsync(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken)
      {
        this.CheckSize(buffer.Length);
        return base.WriteAsync(buffer, cancellationToken);
      }

      public override IAsyncResult BeginWrite(
        byte[] buffer,
        int offset,
        int count,
        AsyncCallback callback,
        object state)
      {
        this.CheckSize(count);
        return base.BeginWrite(buffer, offset, count, callback, state);
      }

      public override void EndWrite(IAsyncResult asyncResult)
      {
        base.EndWrite(asyncResult);
      }

      public override Task CopyToAsync(
        Stream destination,
        int bufferSize,
        CancellationToken cancellationToken)
      {
        ArraySegment<byte> buffer;
        if (!this.TryGetBuffer(out buffer))
          return base.CopyToAsync(destination, bufferSize, cancellationToken);
        StreamHelpers.ValidateCopyToArgs((Stream) this, destination, bufferSize);
        long position = this.Position;
        long length = this.Length;
        this.Position = length;
        long num = length - position;
        return destination.WriteAsync(buffer.Array, (int) ((long) buffer.Offset + position), (int) num, cancellationToken);
      }

      private void CheckSize(int countToAdd)
      {
        if ((long) this._maxSize - this.Length < (long) countToAdd)
          throw HttpContent.CreateOverCapacityException(this._maxSize);
      }
    }

    internal sealed class LimitArrayPoolWriteStream : Stream
    {
      private readonly int _maxBufferSize;
      private byte[] _buffer;
      private int _length;

      public LimitArrayPoolWriteStream(int maxBufferSize)
        : this(maxBufferSize, 256L)
      {
      }

      public LimitArrayPoolWriteStream(int maxBufferSize, long capacity)
      {
        if (capacity < 256L)
          capacity = 256L;
        else if (capacity > (long) maxBufferSize)
          throw HttpContent.CreateOverCapacityException(maxBufferSize);
        this._maxBufferSize = maxBufferSize;
        this._buffer = ArrayPool<byte>.Shared.Rent((int) capacity);
      }

      protected override void Dispose(bool disposing)
      {
        ArrayPool<byte>.Shared.Return(this._buffer, false);
        this._buffer = (byte[]) null;
        base.Dispose(disposing);
      }

      public ArraySegment<byte> GetBuffer()
      {
        return new ArraySegment<byte>(this._buffer, 0, this._length);
      }

      public byte[] ToArray()
      {
        byte[] numArray = new byte[this._length];
        Buffer.BlockCopy((Array) this._buffer, 0, (Array) numArray, 0, this._length);
        return numArray;
      }

      private void EnsureCapacity(int value)
      {
        if ((uint) value > (uint) this._maxBufferSize)
          throw HttpContent.CreateOverCapacityException(this._maxBufferSize);
        if (value <= this._buffer.Length)
          return;
        this.Grow(value);
      }

      private void Grow(int value)
      {
        byte[] buffer = this._buffer;
        this._buffer = (byte[]) null;
        uint num = (uint) (2 * buffer.Length);
        byte[] numArray = ArrayPool<byte>.Shared.Rent(num > 2147483591U ? (value > 2147483591 ? value : 2147483591) : Math.Max(value, (int) num));
        Buffer.BlockCopy((Array) buffer, 0, (Array) numArray, 0, this._length);
        ArrayPool<byte>.Shared.Return(buffer, false);
        this._buffer = numArray;
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
        this.EnsureCapacity(this._length + count);
        Buffer.BlockCopy((Array) buffer, offset, (Array) this._buffer, this._length, count);
        this._length += count;
      }

      public override void Write(ReadOnlySpan<byte> buffer)
      {
        this.EnsureCapacity(this._length + buffer.Length);
        buffer.CopyTo(new Span<byte>(this._buffer, this._length, buffer.Length));
        this._length += buffer.Length;
      }

      public override Task WriteAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken)
      {
        this.Write(buffer, offset, count);
        return Task.CompletedTask;
      }

      public override ValueTask WriteAsync(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken = default (CancellationToken))
      {
        this.Write(buffer.Span);
        return new ValueTask();
      }

      public override IAsyncResult BeginWrite(
        byte[] buffer,
        int offset,
        int count,
        AsyncCallback asyncCallback,
        object asyncState)
      {
        return TaskToApm.Begin(this.WriteAsync(buffer, offset, count, CancellationToken.None), asyncCallback, asyncState);
      }

      public override void EndWrite(IAsyncResult asyncResult)
      {
        TaskToApm.End(asyncResult);
      }

      public override void WriteByte(byte value)
      {
        int num = this._length + 1;
        this.EnsureCapacity(num);
        this._buffer[this._length] = value;
        this._length = num;
      }

      public override void Flush()
      {
      }

      public override Task FlushAsync(CancellationToken cancellationToken)
      {
        return Task.CompletedTask;
      }

      public override long Length
      {
        get
        {
          return (long) this._length;
        }
      }

      public override bool CanWrite
      {
        get
        {
          return true;
        }
      }

      public override bool CanRead
      {
        get
        {
          return false;
        }
      }

      public override bool CanSeek
      {
        get
        {
          return false;
        }
      }

      public override long Position
      {
        get
        {
          throw new NotSupportedException();
        }
        set
        {
          throw new NotSupportedException();
        }
      }

      public override int Read(byte[] buffer, int offset, int count)
      {
        throw new NotSupportedException();
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
        throw new NotSupportedException();
      }

      public override void SetLength(long value)
      {
        throw new NotSupportedException();
      }
    }
  }
}

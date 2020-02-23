// Decompiled with JetBrains decompiler
// Type: System.Net.Http.StreamContent
// Assembly: System.Net.Http, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: D8D12B97-8555-48F6-96B3-BBC6BF89FCBA
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Net.Http.dll

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http
{
  /// <summary>提供基于流的 HTTP 内容。</summary>
  public class StreamContent : HttpContent
  {
    private Stream _content;
    private int _bufferSize;
    private bool _contentConsumed;
    private long _start;

    /// <summary>创建 <see cref="T:System.Net.Http.StreamContent" /> 类的新实例。</summary>
    /// <param name="content">用于初始化 <see cref="T:System.Net.Http.StreamContent" /> 的内容。</param>
    public StreamContent(Stream content)
    {
      if (content == null)
        throw new ArgumentNullException(nameof (content));
      this.InitializeContent(content, 0);
    }

    /// <summary>创建 <see cref="T:System.Net.Http.StreamContent" /> 类的新实例。</summary>
    /// <param name="content">用于初始化 <see cref="T:System.Net.Http.StreamContent" /> 的内容。</param>
    /// <param name="bufferSize">
    /// <see cref="T:System.Net.Http.StreamContent" /> 的缓冲区的大小（以字节为单位）。</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="content" /> 是 <see langword="null" />。</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="bufferSize" /> 小于或等于零。</exception>
    public StreamContent(Stream content, int bufferSize)
    {
      if (content == null)
        throw new ArgumentNullException(nameof (content));
      if (bufferSize <= 0)
        throw new ArgumentOutOfRangeException(nameof (bufferSize));
      this.InitializeContent(content, bufferSize);
    }

    private void InitializeContent(Stream content, int bufferSize)
    {
      this._content = content;
      this._bufferSize = bufferSize;
      if (content.CanSeek)
        this._start = content.Position;
      if (!NetEventSource.IsEnabled)
        return;
      NetEventSource.Associate((object) this, (object) content, nameof (InitializeContent));
    }

    /// <summary>将 HTTP 内容序列化到流，此为异步操作。</summary>
    /// <param name="stream">目标流。</param>
    /// <param name="context">有关传输的信息（例如信道绑定令牌）。 此参数可以为 <see langword="null" />。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
    {
      return this.SerializeToStreamAsyncCore(stream, new CancellationToken());
    }

    internal override Task SerializeToStreamAsync(
      Stream stream,
      TransportContext context,
      CancellationToken cancellationToken)
    {
      return !(this.GetType() == typeof (StreamContent)) ? base.SerializeToStreamAsync(stream, context, cancellationToken) : this.SerializeToStreamAsyncCore(stream, cancellationToken);
    }

    private Task SerializeToStreamAsyncCore(Stream stream, CancellationToken cancellationToken)
    {
      this.PrepareContent();
      return StreamToStreamCopy.CopyAsync(this._content, stream, this._bufferSize, !this._content.CanSeek, cancellationToken);
    }

    /// <summary>确定流内容是否具有有效的长度（以字节为单位）。</summary>
    /// <param name="length">用字节表示的流内容长度。</param>
    /// <returns>如果 <paramref name="length" /> 是有效长度，则为 <see langword="true" />；否则为 <see langword="false" />。</returns>
    protected internal override bool TryComputeLength(out long length)
    {
      if (this._content.CanSeek)
      {
        length = this._content.Length - this._start;
        return true;
      }
      length = 0L;
      return false;
    }

    /// <summary>释放由 <see cref="T:System.Net.Http.StreamContent" /> 使用的非托管资源，并可根据需要释放托管资源。</summary>
    /// <param name="disposing">如果释放托管资源和非托管资源，则为 <see langword="true" />；如果仅释放非托管资源，则为 <see langword="false" />。</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
        this._content.Dispose();
      base.Dispose(disposing);
    }

    /// <summary>作为一个异步操作，将 HTTP 流内容写入内存流。</summary>
    /// <returns>表示异步操作的任务对象。</returns>
    protected override Task<Stream> CreateContentReadStreamAsync()
    {
      return Task.FromResult<Stream>((Stream) new StreamContent.ReadOnlyStream(this._content));
    }

    internal override Stream TryCreateContentReadStream()
    {
      return !(this.GetType() == typeof (StreamContent)) ? (Stream) null : (Stream) new StreamContent.ReadOnlyStream(this._content);
    }

    internal override bool AllowDuplex
    {
      get
      {
        return false;
      }
    }

    private void PrepareContent()
    {
      if (this._contentConsumed)
      {
        if (!this._content.CanSeek)
          throw new InvalidOperationException(SR.net_http_content_stream_already_read);
        this._content.Position = this._start;
      }
      this._contentConsumed = true;
    }

    private sealed class ReadOnlyStream : DelegatingStream
    {
      public override bool CanWrite
      {
        get
        {
          return false;
        }
      }

      public override int WriteTimeout
      {
        get
        {
          throw new NotSupportedException(SR.net_http_content_readonly_stream);
        }
        set
        {
          throw new NotSupportedException(SR.net_http_content_readonly_stream);
        }
      }

      public ReadOnlyStream(Stream innerStream)
        : base(innerStream)
      {
      }

      public override void Flush()
      {
        throw new NotSupportedException(SR.net_http_content_readonly_stream);
      }

      public override Task FlushAsync(CancellationToken cancellationToken)
      {
        throw new NotSupportedException(SR.net_http_content_readonly_stream);
      }

      public override void SetLength(long value)
      {
        throw new NotSupportedException(SR.net_http_content_readonly_stream);
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
        throw new NotSupportedException(SR.net_http_content_readonly_stream);
      }

      public override void Write(ReadOnlySpan<byte> buffer)
      {
        throw new NotSupportedException(SR.net_http_content_readonly_stream);
      }

      public override void WriteByte(byte value)
      {
        throw new NotSupportedException(SR.net_http_content_readonly_stream);
      }

      public override Task WriteAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken)
      {
        throw new NotSupportedException(SR.net_http_content_readonly_stream);
      }

      public override ValueTask WriteAsync(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken = default (CancellationToken))
      {
        throw new NotSupportedException(SR.net_http_content_readonly_stream);
      }
    }
  }
}

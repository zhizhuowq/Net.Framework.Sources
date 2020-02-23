// Decompiled with JetBrains decompiler
// Type: System.Net.Http.MultipartContent
// Assembly: System.Net.Http, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: D8D12B97-8555-48F6-96B3-BBC6BF89FCBA
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Net.Http.dll

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http
{
  /// <summary>提供 <see cref="T:System.Net.Http.HttpContent" /> 对象的集合，其可通过使用多部分/* 内容类型规范序列化。</summary>
  public class MultipartContent : HttpContent, IEnumerable<HttpContent>, IEnumerable
  {
    private static readonly int s_crlfLength = MultipartContent.GetEncodedLength("\r\n");
    private static readonly int s_dashDashLength = MultipartContent.GetEncodedLength("--");
    private static readonly int s_colonSpaceLength = MultipartContent.GetEncodedLength(": ");
    private static readonly int s_commaSpaceLength = MultipartContent.GetEncodedLength(", ");
    private readonly List<HttpContent> _nestedContent;
    private readonly string _boundary;

    /// <summary>创建 <see cref="T:System.Net.Http.MultipartContent" /> 类的新实例。</summary>
    public MultipartContent()
      : this("mixed", MultipartContent.GetDefaultBoundary())
    {
    }

    /// <summary>创建 <see cref="T:System.Net.Http.MultipartContent" /> 类的新实例。</summary>
    /// <param name="subtype">多部分内容的子类型。</param>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="subtype" /> 为 <see langword="null" /> 或者只包含空格字符。</exception>
    public MultipartContent(string subtype)
      : this(subtype, MultipartContent.GetDefaultBoundary())
    {
    }

    /// <summary>创建 <see cref="T:System.Net.Http.MultipartContent" /> 类的新实例。</summary>
    /// <param name="subtype">多部分内容的子类型。</param>
    /// <param name="boundary">多部分内容的边界字符串。</param>
    /// <exception cref="T:System.ArgumentException">
    ///         <paramref name="subtype" /> 是 <see langword="null" /> 或空字符串。
    /// <paramref name="boundary" /> 为 <see langword="null" /> 或者只包含空格字符。
    /// 
    /// 或 -
    /// <paramref name="boundary" /> 以空格字符结尾。</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="boundary" /> 的长度大于 70。</exception>
    public MultipartContent(string subtype, string boundary)
    {
      if (string.IsNullOrWhiteSpace(subtype))
        throw new ArgumentException(SR.net_http_argument_empty_string, nameof (subtype));
      MultipartContent.ValidateBoundary(boundary);
      this._boundary = boundary;
      string str = boundary;
      if (!str.StartsWith("\"", StringComparison.Ordinal))
        str = "\"" + str + "\"";
      this.Headers.ContentType = new MediaTypeHeaderValue("multipart/" + subtype)
      {
        Parameters = {
          new NameValueHeaderValue(nameof (boundary), str)
        }
      };
      this._nestedContent = new List<HttpContent>();
    }

    private static void ValidateBoundary(string boundary)
    {
      if (string.IsNullOrWhiteSpace(boundary))
        throw new ArgumentException(SR.net_http_argument_empty_string, nameof (boundary));
      if (boundary.Length > 70)
        throw new ArgumentOutOfRangeException(nameof (boundary), (object) boundary, SR.Format((IFormatProvider) CultureInfo.InvariantCulture, SR.net_http_content_field_too_long, (object) 70));
      if (boundary.EndsWith(" ", StringComparison.Ordinal))
        throw new ArgumentException(SR.Format((IFormatProvider) CultureInfo.InvariantCulture, SR.net_http_headers_invalid_value, (object) boundary), nameof (boundary));
      foreach (char ch in boundary)
      {
        if (('0' > ch || ch > '9') && ('a' > ch || ch > 'z') && (('A' > ch || ch > 'Z') && !"'()+_,-./:=? ".Contains(ch)))
          throw new ArgumentException(SR.Format((IFormatProvider) CultureInfo.InvariantCulture, SR.net_http_headers_invalid_value, (object) boundary), nameof (boundary));
      }
    }

    private static string GetDefaultBoundary()
    {
      return Guid.NewGuid().ToString();
    }

    /// <summary>添加多部分 HTTP 内容到 <see cref="T:System.Net.Http.HttpContent" /> 对象的集合，其可通过使用多部分/* 内容类型规范获取序列化。</summary>
    /// <param name="content">要添加到集合中的 HTTP 内容。</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="content" /> 是 <see langword="null" />。</exception>
    public virtual void Add(HttpContent content)
    {
      if (content == null)
        throw new ArgumentNullException(nameof (content));
      this._nestedContent.Add(content);
    }

    /// <summary>释放由 <see cref="T:System.Net.Http.MultipartContent" /> 使用的非托管资源，并可根据需要释放托管资源。</summary>
    /// <param name="disposing">如果释放托管资源和非托管资源，则为 <see langword="true" />；如果仅释放非托管资源，则为 <see langword="false" />。</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        foreach (HttpContent httpContent in this._nestedContent)
          httpContent.Dispose();
        this._nestedContent.Clear();
      }
      base.Dispose(disposing);
    }

    /// <summary>返回循环访问 <see cref="T:System.Net.Http.HttpContent" /> 对象集合的枚举器，该对象集合使用多部分/* 内容类型规范进行序列化。</summary>
    /// <returns>一个可用于循环访问集合的对象。</returns>
    public IEnumerator<HttpContent> GetEnumerator()
    {
      return (IEnumerator<HttpContent>) this._nestedContent.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) this._nestedContent.GetEnumerator();
    }

    /// <summary>以异步操作方式，将多部分 HTTP 内容序列化到流。</summary>
    /// <param name="stream">目标流。</param>
    /// <param name="context">有关传输的信息（例如信道绑定令牌）。 此参数可以为 <see langword="null" />。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
    {
      return this.SerializeToStreamAsyncCore(stream, context, new CancellationToken());
    }

    internal override Task SerializeToStreamAsync(
      Stream stream,
      TransportContext context,
      CancellationToken cancellationToken)
    {
      return !(this.GetType() == typeof (MultipartContent)) ? base.SerializeToStreamAsync(stream, context, cancellationToken) : this.SerializeToStreamAsyncCore(stream, context, cancellationToken);
    }

    private protected async Task SerializeToStreamAsyncCore(
      Stream stream,
      TransportContext context,
      CancellationToken cancellationToken)
    {
      MultipartContent multipartContent = this;
      try
      {
        ConfiguredValueTaskAwaitable valueTaskAwaitable = MultipartContent.EncodeStringToStreamAsync(stream, "--" + multipartContent._boundary + "\r\n", cancellationToken).ConfigureAwait(false);
        await valueTaskAwaitable;
        StringBuilder output = new StringBuilder();
        for (int contentIndex = 0; contentIndex < multipartContent._nestedContent.Count; ++contentIndex)
        {
          HttpContent content = multipartContent._nestedContent[contentIndex];
          valueTaskAwaitable = MultipartContent.EncodeStringToStreamAsync(stream, multipartContent.SerializeHeadersToString(output, contentIndex, content), cancellationToken).ConfigureAwait(false);
          await valueTaskAwaitable;
          await content.CopyToAsync(stream, context, cancellationToken).ConfigureAwait(false);
          content = (HttpContent) null;
        }
        valueTaskAwaitable = MultipartContent.EncodeStringToStreamAsync(stream, "\r\n--" + multipartContent._boundary + "--\r\n", cancellationToken).ConfigureAwait(false);
        await valueTaskAwaitable;
        output = (StringBuilder) null;
      }
      catch (Exception ex)
      {
        if (NetEventSource.IsEnabled)
          NetEventSource.Error((object) multipartContent, (object) ex, nameof (SerializeToStreamAsyncCore));
        throw;
      }
    }

    protected override async Task<Stream> CreateContentReadStreamAsync()
    {
      MultipartContent multipartContent = this;
      try
      {
        Stream[] streams = new Stream[2 + multipartContent._nestedContent.Count * 2];
        StringBuilder scratch = new StringBuilder();
        int streamIndex = 0;
        streams[streamIndex++] = MultipartContent.EncodeStringToNewStream("--" + multipartContent._boundary + "\r\n");
        for (int contentIndex = 0; contentIndex < multipartContent._nestedContent.Count; ++contentIndex)
        {
          HttpContent content = multipartContent._nestedContent[contentIndex];
          streams[streamIndex++] = MultipartContent.EncodeStringToNewStream(multipartContent.SerializeHeadersToString(scratch, contentIndex, content));
          ConfiguredTaskAwaitable<Stream> configuredTaskAwaitable = content.ReadAsStreamAsync().ConfigureAwait(false);
          Stream stream = await configuredTaskAwaitable ?? (Stream) new MemoryStream();
          if (!stream.CanSeek)
          {
            // ISSUE: reference to a compiler-generated method
            configuredTaskAwaitable = multipartContent.\u003C\u003En__0().ConfigureAwait(false);
            return await configuredTaskAwaitable;
          }
          streams[streamIndex++] = stream;
        }
        streams[streamIndex] = MultipartContent.EncodeStringToNewStream("\r\n--" + multipartContent._boundary + "--\r\n");
        return (Stream) new MultipartContent.ContentReadStream(streams);
      }
      catch (Exception ex)
      {
        if (NetEventSource.IsEnabled)
          NetEventSource.Error((object) multipartContent, (object) ex, nameof (CreateContentReadStreamAsync));
        throw;
      }
    }

    private string SerializeHeadersToString(
      StringBuilder scratch,
      int contentIndex,
      HttpContent content)
    {
      scratch.Clear();
      if (contentIndex != 0)
      {
        scratch.Append("\r\n--");
        scratch.Append(this._boundary);
        scratch.Append("\r\n");
      }
      foreach (KeyValuePair<string, IEnumerable<string>> header in (HttpHeaders) content.Headers)
      {
        scratch.Append(header.Key);
        scratch.Append(": ");
        string str1 = string.Empty;
        foreach (string str2 in header.Value)
        {
          scratch.Append(str1);
          scratch.Append(str2);
          str1 = ", ";
        }
        scratch.Append("\r\n");
      }
      scratch.Append("\r\n");
      return scratch.ToString();
    }

    private static ValueTask EncodeStringToStreamAsync(
      Stream stream,
      string input,
      CancellationToken cancellationToken)
    {
      byte[] bytes = HttpRuleParser.DefaultHttpEncoding.GetBytes(input);
      return stream.WriteAsync(new ReadOnlyMemory<byte>(bytes), cancellationToken);
    }

    private static Stream EncodeStringToNewStream(string input)
    {
      return (Stream) new MemoryStream(HttpRuleParser.DefaultHttpEncoding.GetBytes(input), false);
    }

    internal override bool AllowDuplex
    {
      get
      {
        return false;
      }
    }

    /// <summary>确定 HTTP 多部分内容的长度是否有效（以字节为单位）。</summary>
    /// <param name="length">HTTP 内容的长度（以字节为单位）。</param>
    /// <returns>如果 <paramref name="length" /> 是有效长度，则为 <see langword="true" />；否则为 <see langword="false" />。</returns>
    protected internal override bool TryComputeLength(out long length)
    {
      int encodedLength = MultipartContent.GetEncodedLength(this._boundary);
      long num1 = 0;
      long num2 = (long) (MultipartContent.s_crlfLength + MultipartContent.s_dashDashLength + encodedLength + MultipartContent.s_crlfLength);
      long num3 = num1 + (long) (MultipartContent.s_dashDashLength + encodedLength + MultipartContent.s_crlfLength);
      bool flag = true;
      foreach (HttpContent httpContent in this._nestedContent)
      {
        if (flag)
          flag = false;
        else
          num3 += num2;
        foreach (KeyValuePair<string, IEnumerable<string>> header in (HttpHeaders) httpContent.Headers)
        {
          num3 += (long) (MultipartContent.GetEncodedLength(header.Key) + MultipartContent.s_colonSpaceLength);
          int num4 = 0;
          foreach (string input in header.Value)
          {
            num3 += (long) MultipartContent.GetEncodedLength(input);
            ++num4;
          }
          if (num4 > 1)
            num3 += (long) ((num4 - 1) * MultipartContent.s_commaSpaceLength);
          num3 += (long) MultipartContent.s_crlfLength;
        }
        num3 += (long) MultipartContent.s_crlfLength;
        long length1 = 0;
        if (!httpContent.TryComputeLength(out length1))
        {
          length = 0L;
          return false;
        }
        num3 += length1;
      }
      long num5 = num3 + (long) (MultipartContent.s_crlfLength + MultipartContent.s_dashDashLength + encodedLength + MultipartContent.s_dashDashLength + MultipartContent.s_crlfLength);
      length = num5;
      return true;
    }

    private static int GetEncodedLength(string input)
    {
      return HttpRuleParser.DefaultHttpEncoding.GetByteCount(input);
    }

    private sealed class ContentReadStream : Stream
    {
      private readonly Stream[] _streams;
      private readonly long _length;
      private int _next;
      private Stream _current;
      private long _position;

      internal ContentReadStream(Stream[] streams)
      {
        this._streams = streams;
        foreach (Stream stream in streams)
          this._length += stream.Length;
      }

      protected override void Dispose(bool disposing)
      {
        if (!disposing)
          return;
        foreach (Stream stream in this._streams)
          stream.Dispose();
      }

      public override async ValueTask DisposeAsync()
      {
        Stream[] streamArray = this._streams;
        for (int index = 0; index < streamArray.Length; ++index)
          await streamArray[index].DisposeAsync().ConfigureAwait(false);
        streamArray = (Stream[]) null;
      }

      public override bool CanRead
      {
        get
        {
          return true;
        }
      }

      public override bool CanSeek
      {
        get
        {
          return true;
        }
      }

      public override bool CanWrite
      {
        get
        {
          return false;
        }
      }

      public override int Read(byte[] buffer, int offset, int count)
      {
        MultipartContent.ContentReadStream.ValidateReadArgs(buffer, offset, count);
        if (count == 0)
          return 0;
        int num;
        while (true)
        {
          if (this._current != null)
          {
            num = this._current.Read(buffer, offset, count);
            if (num == 0)
              this._current = (Stream) null;
            else
              break;
          }
          if (this._next < this._streams.Length)
            this._current = this._streams[this._next++];
          else
            goto label_7;
        }
        this._position += (long) num;
        return num;
label_7:
        return 0;
      }

      public override int Read(Span<byte> buffer)
      {
        if (buffer.Length == 0)
          return 0;
        int num;
        while (true)
        {
          if (this._current != null)
          {
            num = this._current.Read(buffer);
            if (num == 0)
              this._current = (Stream) null;
            else
              break;
          }
          if (this._next < this._streams.Length)
            this._current = this._streams[this._next++];
          else
            goto label_7;
        }
        this._position += (long) num;
        return num;
label_7:
        return 0;
      }

      public override Task<int> ReadAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken)
      {
        MultipartContent.ContentReadStream.ValidateReadArgs(buffer, offset, count);
        return this.ReadAsyncPrivate(new Memory<byte>(buffer, offset, count), cancellationToken).AsTask();
      }

      public override ValueTask<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default (CancellationToken))
      {
        return this.ReadAsyncPrivate(buffer, cancellationToken);
      }

      public override IAsyncResult BeginRead(
        byte[] array,
        int offset,
        int count,
        AsyncCallback asyncCallback,
        object asyncState)
      {
        return TaskToApm.Begin((Task) this.ReadAsync(array, offset, count, CancellationToken.None), asyncCallback, asyncState);
      }

      public override int EndRead(IAsyncResult asyncResult)
      {
        return TaskToApm.End<int>(asyncResult);
      }

      public async ValueTask<int> ReadAsyncPrivate(
        Memory<byte> buffer,
        CancellationToken cancellationToken)
      {
        MultipartContent.ContentReadStream contentReadStream1 = this;
        if (buffer.Length == 0)
          return 0;
        int num1;
        while (true)
        {
          if (contentReadStream1._current != null)
          {
            num1 = await contentReadStream1._current.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (num1 == 0)
              contentReadStream1._current = (Stream) null;
            else
              break;
          }
          if (contentReadStream1._next < contentReadStream1._streams.Length)
          {
            MultipartContent.ContentReadStream contentReadStream2 = contentReadStream1;
            Stream[] streams = contentReadStream1._streams;
            MultipartContent.ContentReadStream contentReadStream3 = contentReadStream1;
            int next = contentReadStream1._next;
            int num2 = next + 1;
            contentReadStream3._next = num2;
            int index = next;
            Stream stream = streams[index];
            contentReadStream2._current = stream;
          }
          else
            goto label_7;
        }
        contentReadStream1._position += (long) num1;
        return num1;
label_7:
        return 0;
      }

      public override long Position
      {
        get
        {
          return this._position;
        }
        set
        {
          if (value < 0L)
            throw new ArgumentOutOfRangeException(nameof (value));
          long num = 0;
          for (int index1 = 0; index1 < this._streams.Length; ++index1)
          {
            Stream stream = this._streams[index1];
            long length = stream.Length;
            if (value < num + length)
            {
              this._current = stream;
              int index2 = index1 + 1;
              this._next = index2;
              stream.Position = value - num;
              for (; index2 < this._streams.Length; ++index2)
                this._streams[index2].Position = 0L;
              this._position = value;
              return;
            }
            num += length;
          }
          this._current = (Stream) null;
          this._next = this._streams.Length;
          this._position = value;
        }
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
        switch (origin)
        {
          case SeekOrigin.Begin:
            this.Position = offset;
            break;
          case SeekOrigin.Current:
            this.Position += offset;
            break;
          case SeekOrigin.End:
            this.Position = this._length + offset;
            break;
          default:
            throw new ArgumentOutOfRangeException(nameof (origin));
        }
        return this.Position;
      }

      public override long Length
      {
        get
        {
          return this._length;
        }
      }

      private static void ValidateReadArgs(byte[] buffer, int offset, int count)
      {
        if (buffer == null)
          throw new ArgumentNullException(nameof (buffer));
        if (offset < 0)
          throw new ArgumentOutOfRangeException(nameof (offset));
        if (count < 0)
          throw new ArgumentOutOfRangeException(nameof (count));
        if (offset > buffer.Length - count)
          throw new ArgumentException(SR.net_http_buffer_insufficient_length, nameof (buffer));
      }

      public override void Flush()
      {
      }

      public override void SetLength(long value)
      {
        throw new NotSupportedException();
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
        throw new NotSupportedException();
      }

      public override void Write(ReadOnlySpan<byte> buffer)
      {
        throw new NotSupportedException();
      }

      public override Task WriteAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken)
      {
        throw new NotSupportedException();
      }

      public override ValueTask WriteAsync(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken = default (CancellationToken))
      {
        throw new NotSupportedException();
      }
    }
  }
}

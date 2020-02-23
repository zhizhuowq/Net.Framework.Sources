// Decompiled with JetBrains decompiler
// Type: System.Net.Http.HttpClient
// Assembly: System.Net.Http, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: D8D12B97-8555-48F6-96B3-BBC6BF89FCBA
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Net.Http.dll

using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http
{
  /// <summary>提供基本类，用于发送 HTTP 请求和接收来自通过 URI 确认的资源的 HTTP 响应。</summary>
  public class HttpClient : HttpMessageInvoker
  {
    private static readonly TimeSpan s_defaultTimeout = TimeSpan.FromSeconds(100.0);
    private static readonly TimeSpan s_maxTimeout = TimeSpan.FromMilliseconds((double) int.MaxValue);
    private static readonly TimeSpan s_infiniteTimeout = System.Threading.Timeout.InfiniteTimeSpan;
    private Version _defaultRequestVersion = HttpUtilities.DefaultRequestVersion;
    private static IWebProxy s_defaultProxy;
    private volatile bool _operationStarted;
    private volatile bool _disposed;
    private CancellationTokenSource _pendingRequestsCts;
    private HttpRequestHeaders _defaultRequestHeaders;
    private Uri _baseAddress;
    private TimeSpan _timeout;
    private int _maxResponseContentBufferSize;

    public static IWebProxy DefaultProxy
    {
      get
      {
        return LazyInitializer.EnsureInitialized<IWebProxy>(ref HttpClient.s_defaultProxy, (Func<IWebProxy>) (() => SystemProxyInfo.Proxy));
      }
      set
      {
        IWebProxy webProxy = value;
        if (webProxy == null)
          throw new ArgumentNullException(nameof (value));
        HttpClient.s_defaultProxy = webProxy;
      }
    }

    /// <summary>获取与每个请求一起发送的标题。</summary>
    /// <returns>应与每一个请求一起发送的标题。</returns>
    public HttpRequestHeaders DefaultRequestHeaders
    {
      get
      {
        if (this._defaultRequestHeaders == null)
          this._defaultRequestHeaders = new HttpRequestHeaders();
        return this._defaultRequestHeaders;
      }
    }

    public Version DefaultRequestVersion
    {
      get
      {
        return this._defaultRequestVersion;
      }
      set
      {
        this.CheckDisposedOrStarted();
        Version version = value;
        if ((object) version == null)
          throw new ArgumentNullException(nameof (value));
        this._defaultRequestVersion = version;
      }
    }

    /// <summary>获取或设置发送请求时使用的 Internet 资源的统一资源标识符 (URI) 的基址。</summary>
    /// <returns>发送请求时使用的 Internet 资源的统一资源标识符 (URI) 的基址。</returns>
    public Uri BaseAddress
    {
      get
      {
        return this._baseAddress;
      }
      set
      {
        HttpClient.CheckBaseAddress(value, nameof (value));
        this.CheckDisposedOrStarted();
        if (NetEventSource.IsEnabled)
          NetEventSource.UriBaseAddress((object) this, value);
        this._baseAddress = value;
      }
    }

    /// <summary>获取或设置请求超时前等待的时间跨度。</summary>
    /// <returns>请求超时前等待的时间跨度。</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">指定的超时值小于或等于零，而不是 <see cref="F:System.Threading.Timeout.InfiniteTimeSpan" />。</exception>
    /// <exception cref="T:System.InvalidOperationException">已在当前实例上启动了一个操作。</exception>
    /// <exception cref="T:System.ObjectDisposedException">已释放当前实例。</exception>
    public TimeSpan Timeout
    {
      get
      {
        return this._timeout;
      }
      set
      {
        if (value != HttpClient.s_infiniteTimeout && (value <= TimeSpan.Zero || value > HttpClient.s_maxTimeout))
          throw new ArgumentOutOfRangeException(nameof (value));
        this.CheckDisposedOrStarted();
        this._timeout = value;
      }
    }

    /// <summary>获取或设置读取响应内容时要缓冲的最大字节数。</summary>
    /// <returns>当读取响应内容时缓冲区的最大字节数。 此属性的默认值为 2 GB。</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">指定大小小于或等于零。</exception>
    /// <exception cref="T:System.InvalidOperationException">已在当前实例上启动了一个操作。</exception>
    /// <exception cref="T:System.ObjectDisposedException">已释放当前实例。</exception>
    public long MaxResponseContentBufferSize
    {
      get
      {
        return (long) this._maxResponseContentBufferSize;
      }
      set
      {
        if (value <= 0L)
          throw new ArgumentOutOfRangeException(nameof (value));
        if (value > (long) int.MaxValue)
          throw new ArgumentOutOfRangeException(nameof (value), (object) value, SR.Format((IFormatProvider) CultureInfo.InvariantCulture, SR.net_http_content_buffersize_limit, (object) int.MaxValue));
        this.CheckDisposedOrStarted();
        this._maxResponseContentBufferSize = (int) value;
      }
    }

    /// <summary>初始化 <see cref="T:System.Net.Http.HttpClient" /> 类的新实例。</summary>
    public HttpClient()
      : this((HttpMessageHandler) new HttpClientHandler())
    {
    }

    /// <summary>用特定的处理程序初始化 <see cref="T:System.Net.Http.HttpClient" /> 类的新实例。</summary>
    /// <param name="handler">要用于发送请求的 HTTP 处理程序堆栈。</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="handler" /> 为 <see langword="null" />。</exception>
    public HttpClient(HttpMessageHandler handler)
      : this(handler, true)
    {
    }

    /// <summary>用特定的处理程序初始化 <see cref="T:System.Net.Http.HttpClient" /> 类的新实例。</summary>
    /// <param name="handler">负责处理 HTTP 响应消息的 <see cref="T:System.Net.Http.HttpMessageHandler" />。</param>
    /// <param name="disposeHandler">如果内部处理程序应由 HttpClient.Dispose 处置，则为 <see langword="true" />，如果希望重新使用内部处理程序，则为 <see langword="false" />。</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="handler" /> 为 <see langword="null" />。</exception>
    public HttpClient(HttpMessageHandler handler, bool disposeHandler)
      : base(handler, disposeHandler)
    {
      this._timeout = HttpClient.s_defaultTimeout;
      this._maxResponseContentBufferSize = int.MaxValue;
      this._pendingRequestsCts = new CancellationTokenSource();
    }

    /// <summary>将 GET 请求发送到指定 URI 并在异步操作中以字符串的形式返回响应正文。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<string> GetStringAsync(string requestUri)
    {
      return this.GetStringAsync(this.CreateUri(requestUri));
    }

    /// <summary>将 GET 请求发送到指定 URI 并在异步操作中以字符串的形式返回响应正文。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<string> GetStringAsync(Uri requestUri)
    {
      return this.GetStringAsyncCore(this.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead));
    }

    private async Task<string> GetStringAsyncCore(Task<HttpResponseMessage> getTask)
    {
      using (HttpResponseMessage responseMessage = await getTask.ConfigureAwait(false))
      {
        responseMessage.EnsureSuccessStatusCode();
        HttpContent content = responseMessage.Content;
        if (content != null)
        {
          HttpContentHeaders headers = content.Headers;
          Stream stream = content.TryReadAsStream();
          if (stream == null)
            stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
          using (Stream responseStream = stream)
          {
            using (HttpContent.LimitArrayPoolWriteStream buffer = new HttpContent.LimitArrayPoolWriteStream(this._maxResponseContentBufferSize, (long) (int) headers.ContentLength.GetValueOrDefault()))
            {
              try
              {
                await responseStream.CopyToAsync((Stream) buffer).ConfigureAwait(false);
              }
              catch (Exception ex) when (HttpContent.StreamCopyExceptionNeedsWrapping(ex))
              {
                throw HttpContent.WrapStreamCopyException(ex);
              }
              if (buffer.Length > 0L)
                return HttpContent.ReadBufferAsString(buffer.GetBuffer(), headers);
            }
          }
          headers = (HttpContentHeaders) null;
        }
        return string.Empty;
      }
    }

    /// <summary>将 GET 请求发送到指定 URI 并在异步操作中以字节数组的形式返回响应正文。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<byte[]> GetByteArrayAsync(string requestUri)
    {
      return this.GetByteArrayAsync(this.CreateUri(requestUri));
    }

    /// <summary>将 GET 请求发送到指定 URI 并在异步操作中以字节数组的形式返回响应正文。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<byte[]> GetByteArrayAsync(Uri requestUri)
    {
      return this.GetByteArrayAsyncCore(this.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead));
    }

    private async Task<byte[]> GetByteArrayAsyncCore(Task<HttpResponseMessage> getTask)
    {
      using (HttpResponseMessage responseMessage = await getTask.ConfigureAwait(false))
      {
        responseMessage.EnsureSuccessStatusCode();
        HttpContent content = responseMessage.Content;
        if (content != null)
        {
          HttpContentHeaders headers = content.Headers;
          Stream stream = content.TryReadAsStream();
          if (stream == null)
            stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
          using (Stream responseStream = stream)
          {
            long? contentLength = headers.ContentLength;
            Stream buffer;
            if (contentLength.HasValue)
            {
              buffer = (Stream) new HttpContent.LimitMemoryStream(this._maxResponseContentBufferSize, (int) contentLength.GetValueOrDefault());
              try
              {
                await responseStream.CopyToAsync(buffer).ConfigureAwait(false);
              }
              catch (Exception ex) when (HttpContent.StreamCopyExceptionNeedsWrapping(ex))
              {
                throw HttpContent.WrapStreamCopyException(ex);
              }
              if (buffer.Length > 0L)
                return ((HttpContent.LimitMemoryStream) buffer).GetSizedBuffer();
            }
            else
            {
              buffer = (Stream) new HttpContent.LimitArrayPoolWriteStream(this._maxResponseContentBufferSize);
              try
              {
                try
                {
                  await responseStream.CopyToAsync(buffer).ConfigureAwait(false);
                }
                catch (Exception ex) when (HttpContent.StreamCopyExceptionNeedsWrapping(ex))
                {
                  throw HttpContent.WrapStreamCopyException(ex);
                }
                if (buffer.Length > 0L)
                  return ((HttpContent.LimitArrayPoolWriteStream) buffer).ToArray();
              }
              finally
              {
                buffer.Dispose();
              }
            }
            buffer = (Stream) null;
          }
          headers = (HttpContentHeaders) null;
        }
        return Array.Empty<byte>();
      }
    }

    /// <summary>将 GET 请求发送到指定 URI 并在异步操作中以流的形式返回响应正文。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<Stream> GetStreamAsync(string requestUri)
    {
      return this.GetStreamAsync(this.CreateUri(requestUri));
    }

    /// <summary>将 GET 请求发送到指定 URI 并在异步操作中以流的形式返回响应正文。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<Stream> GetStreamAsync(Uri requestUri)
    {
      return this.FinishGetStreamAsync(this.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead));
    }

    private async Task<Stream> FinishGetStreamAsync(Task<HttpResponseMessage> getTask)
    {
      HttpResponseMessage httpResponseMessage = await getTask.ConfigureAwait(false);
      httpResponseMessage.EnsureSuccessStatusCode();
      HttpContent content = httpResponseMessage.Content;
      Stream stream1;
      if (content != null)
      {
        Stream stream2 = content.TryReadAsStream();
        if (stream2 == null)
          stream2 = await content.ReadAsStreamAsync().ConfigureAwait(false);
        stream1 = stream2;
      }
      else
        stream1 = Stream.Null;
      return stream1;
    }

    /// <summary>以异步操作将 GET 请求发送给指定 URI。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> GetAsync(string requestUri)
    {
      return this.GetAsync(this.CreateUri(requestUri));
    }

    /// <summary>以异步操作将 GET 请求发送给指定 URI。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> GetAsync(Uri requestUri)
    {
      return this.GetAsync(requestUri, HttpCompletionOption.ResponseContentRead);
    }

    /// <summary>用以异步操作的 HTTP 完成选项发送 GET 请求到指定的 URI。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <param name="completionOption">指示操作应视为已完成的时间的 HTTP 完成选项值。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> GetAsync(
      string requestUri,
      HttpCompletionOption completionOption)
    {
      return this.GetAsync(this.CreateUri(requestUri), completionOption);
    }

    /// <summary>用以异步操作的 HTTP 完成选项发送 GET 请求到指定的 URI。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <param name="completionOption">指示操作应视为已完成的时间的 HTTP 完成选项值。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> GetAsync(
      Uri requestUri,
      HttpCompletionOption completionOption)
    {
      return this.GetAsync(requestUri, completionOption, CancellationToken.None);
    }

    /// <summary>用以异步操作的取消标记发送 GET 请求到指定的 URI。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <param name="cancellationToken">可由其他对象或线程用以接收取消通知的取消标记。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> GetAsync(
      string requestUri,
      CancellationToken cancellationToken)
    {
      return this.GetAsync(this.CreateUri(requestUri), cancellationToken);
    }

    /// <summary>用以异步操作的取消标记发送 GET 请求到指定的 URI。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <param name="cancellationToken">可由其他对象或线程用以接收取消通知的取消标记。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> GetAsync(
      Uri requestUri,
      CancellationToken cancellationToken)
    {
      return this.GetAsync(requestUri, HttpCompletionOption.ResponseContentRead, cancellationToken);
    }

    /// <summary>用以异步操作的 HTTP 完成选项和取消标记发送 GET 请求到指定的 URI。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <param name="completionOption">指示操作应视为已完成的时间的 HTTP 完成选项值。</param>
    /// <param name="cancellationToken">可由其他对象或线程用以接收取消通知的取消标记。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> GetAsync(
      string requestUri,
      HttpCompletionOption completionOption,
      CancellationToken cancellationToken)
    {
      return this.GetAsync(this.CreateUri(requestUri), completionOption, cancellationToken);
    }

    /// <summary>用以异步操作的 HTTP 完成选项和取消标记发送 GET 请求到指定的 URI。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <param name="completionOption">指示操作应视为已完成的时间的 HTTP 完成选项值。</param>
    /// <param name="cancellationToken">可由其他对象或线程用以接收取消通知的取消标记。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> GetAsync(
      Uri requestUri,
      HttpCompletionOption completionOption,
      CancellationToken cancellationToken)
    {
      return this.SendAsync(this.CreateRequestMessage(HttpMethod.Get, requestUri), completionOption, cancellationToken);
    }

    /// <summary>以异步操作将 POST 请求发送给指定 URI。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <param name="content">发送到服务器的 HTTP 请求内容。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> PostAsync(
      string requestUri,
      HttpContent content)
    {
      return this.PostAsync(this.CreateUri(requestUri), content);
    }

    /// <summary>以异步操作将 POST 请求发送给指定 URI。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <param name="content">发送到服务器的 HTTP 请求内容。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> PostAsync(
      Uri requestUri,
      HttpContent content)
    {
      return this.PostAsync(requestUri, content, CancellationToken.None);
    }

    /// <summary>用以异步操作的取消标记发送 POST 请求。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <param name="content">发送到服务器的 HTTP 请求内容。</param>
    /// <param name="cancellationToken">可由其他对象或线程用以接收取消通知的取消标记。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> PostAsync(
      string requestUri,
      HttpContent content,
      CancellationToken cancellationToken)
    {
      return this.PostAsync(this.CreateUri(requestUri), content, cancellationToken);
    }

    /// <summary>用以异步操作的取消标记发送 POST 请求。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <param name="content">发送到服务器的 HTTP 请求内容。</param>
    /// <param name="cancellationToken">可由其他对象或线程用以接收取消通知的取消标记。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> PostAsync(
      Uri requestUri,
      HttpContent content,
      CancellationToken cancellationToken)
    {
      HttpRequestMessage requestMessage = this.CreateRequestMessage(HttpMethod.Post, requestUri);
      requestMessage.Content = content;
      return this.SendAsync(requestMessage, cancellationToken);
    }

    /// <summary>以异步操作将 PUT 请求发送给指定 URI。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <param name="content">发送到服务器的 HTTP 请求内容。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> PutAsync(
      string requestUri,
      HttpContent content)
    {
      return this.PutAsync(this.CreateUri(requestUri), content);
    }

    /// <summary>以异步操作将 PUT 请求发送给指定 URI。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <param name="content">发送到服务器的 HTTP 请求内容。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> PutAsync(
      Uri requestUri,
      HttpContent content)
    {
      return this.PutAsync(requestUri, content, CancellationToken.None);
    }

    /// <summary>用以异步操作的取消标记发送 PUT 请求。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <param name="content">发送到服务器的 HTTP 请求内容。</param>
    /// <param name="cancellationToken">可由其他对象或线程用以接收取消通知的取消标记。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> PutAsync(
      string requestUri,
      HttpContent content,
      CancellationToken cancellationToken)
    {
      return this.PutAsync(this.CreateUri(requestUri), content, cancellationToken);
    }

    /// <summary>用以异步操作的取消标记发送 PUT 请求。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <param name="content">发送到服务器的 HTTP 请求内容。</param>
    /// <param name="cancellationToken">可由其他对象或线程用以接收取消通知的取消标记。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> PutAsync(
      Uri requestUri,
      HttpContent content,
      CancellationToken cancellationToken)
    {
      HttpRequestMessage requestMessage = this.CreateRequestMessage(HttpMethod.Put, requestUri);
      requestMessage.Content = content;
      return this.SendAsync(requestMessage, cancellationToken);
    }

    public Task<HttpResponseMessage> PatchAsync(
      string requestUri,
      HttpContent content)
    {
      return this.PatchAsync(this.CreateUri(requestUri), content);
    }

    public Task<HttpResponseMessage> PatchAsync(
      Uri requestUri,
      HttpContent content)
    {
      return this.PatchAsync(requestUri, content, CancellationToken.None);
    }

    public Task<HttpResponseMessage> PatchAsync(
      string requestUri,
      HttpContent content,
      CancellationToken cancellationToken)
    {
      return this.PatchAsync(this.CreateUri(requestUri), content, cancellationToken);
    }

    public Task<HttpResponseMessage> PatchAsync(
      Uri requestUri,
      HttpContent content,
      CancellationToken cancellationToken)
    {
      HttpRequestMessage requestMessage = this.CreateRequestMessage(HttpMethod.Patch, requestUri);
      requestMessage.Content = content;
      return this.SendAsync(requestMessage, cancellationToken);
    }

    /// <summary>以异步操作将 DELETE 请求发送给指定 URI。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.InvalidOperationException">请求消息已由 <see cref="T:System.Net.Http.HttpClient" /> 实例发送。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> DeleteAsync(string requestUri)
    {
      return this.DeleteAsync(this.CreateUri(requestUri));
    }

    /// <summary>以异步操作将 DELETE 请求发送给指定 URI。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.InvalidOperationException">请求消息已由 <see cref="T:System.Net.Http.HttpClient" /> 实例发送。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> DeleteAsync(Uri requestUri)
    {
      return this.DeleteAsync(requestUri, CancellationToken.None);
    }

    /// <summary>用以异步操作的取消标记发送 DELETE 请求到指定的 URI。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <param name="cancellationToken">可由其他对象或线程用以接收取消通知的取消标记。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.InvalidOperationException">请求消息已由 <see cref="T:System.Net.Http.HttpClient" /> 实例发送。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> DeleteAsync(
      string requestUri,
      CancellationToken cancellationToken)
    {
      return this.DeleteAsync(this.CreateUri(requestUri), cancellationToken);
    }

    /// <summary>用以异步操作的取消标记发送 DELETE 请求到指定的 URI。</summary>
    /// <param name="requestUri">请求发送到的 URI。</param>
    /// <param name="cancellationToken">可由其他对象或线程用以接收取消通知的取消标记。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="requestUri" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.InvalidOperationException">请求消息已由 <see cref="T:System.Net.Http.HttpClient" /> 实例发送。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> DeleteAsync(
      Uri requestUri,
      CancellationToken cancellationToken)
    {
      return this.SendAsync(this.CreateRequestMessage(HttpMethod.Delete, requestUri), cancellationToken);
    }

    /// <summary>以异步操作发送 HTTP 请求。</summary>
    /// <param name="request">要发送的 HTTP 请求消息。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="request" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.InvalidOperationException">请求消息已由 <see cref="T:System.Net.Http.HttpClient" /> 实例发送。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
      return this.SendAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
    }

    /// <summary>以异步操作发送 HTTP 请求。</summary>
    /// <param name="request">要发送的 HTTP 请求消息。</param>
    /// <param name="cancellationToken">取消操作的取消标记。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="request" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.InvalidOperationException">请求消息已由 <see cref="T:System.Net.Http.HttpClient" /> 实例发送。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public override Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request,
      CancellationToken cancellationToken)
    {
      return this.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
    }

    /// <summary>以异步操作发送 HTTP 请求。</summary>
    /// <param name="request">要发送的 HTTP 请求消息。</param>
    /// <param name="completionOption">操作应完成时（在响应可利用或在读取整个响应内容之后）。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="request" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.InvalidOperationException">请求消息已由 <see cref="T:System.Net.Http.HttpClient" /> 实例发送。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request,
      HttpCompletionOption completionOption)
    {
      return this.SendAsync(request, completionOption, CancellationToken.None);
    }

    /// <summary>以异步操作发送 HTTP 请求。</summary>
    /// <param name="request">要发送的 HTTP 请求消息。</param>
    /// <param name="completionOption">操作应完成时（在响应可利用或在读取整个响应内容之后）。</param>
    /// <param name="cancellationToken">取消操作的取消标记。</param>
    /// <returns>表示异步操作的任务对象。</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="request" /> 为 <see langword="null" />。</exception>
    /// <exception cref="T:System.InvalidOperationException">请求消息已由 <see cref="T:System.Net.Http.HttpClient" /> 实例发送。</exception>
    /// <exception cref="T:System.Net.Http.HttpRequestException">由于基础问题（如网络连接性、DNS 失败、服务器证书验证或超时），请求失败。</exception>
    public Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request,
      HttpCompletionOption completionOption,
      CancellationToken cancellationToken)
    {
      if (request == null)
        throw new ArgumentNullException(nameof (request));
      this.CheckDisposed();
      HttpClient.CheckRequestMessage(request);
      this.SetOperationStarted();
      this.PrepareRequestMessage(request);
      bool flag = this._timeout != HttpClient.s_infiniteTimeout;
      bool disposeCts;
      CancellationTokenSource cts;
      if (flag || cancellationToken.CanBeCanceled)
      {
        disposeCts = true;
        cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this._pendingRequestsCts.Token);
        if (flag)
          cts.CancelAfter(this._timeout);
      }
      else
      {
        disposeCts = false;
        cts = this._pendingRequestsCts;
      }
      Task<HttpResponseMessage> sendTask;
      try
      {
        sendTask = base.SendAsync(request, cts.Token);
      }
      catch
      {
        this.HandleFinishSendAsyncCleanup(cts, disposeCts);
        throw;
      }
      return completionOption != HttpCompletionOption.ResponseContentRead || string.Equals(request.Method.Method, "HEAD", StringComparison.OrdinalIgnoreCase) ? this.FinishSendAsyncUnbuffered(sendTask, request, cts, disposeCts) : this.FinishSendAsyncBuffered(sendTask, request, cts, disposeCts);
    }

    private async Task<HttpResponseMessage> FinishSendAsyncBuffered(
      Task<HttpResponseMessage> sendTask,
      HttpRequestMessage request,
      CancellationTokenSource cts,
      bool disposeCts)
    {
      HttpClient httpClient = this;
      HttpResponseMessage response = (HttpResponseMessage) null;
      HttpResponseMessage httpResponseMessage;
      try
      {
        response = await sendTask.ConfigureAwait(false);
        if (response == null)
          throw new InvalidOperationException(SR.net_http_handler_noresponse);
        if (response.Content != null)
          await response.Content.LoadIntoBufferAsync((long) httpClient._maxResponseContentBufferSize, cts.Token).ConfigureAwait(false);
        if (NetEventSource.IsEnabled)
          NetEventSource.ClientSendCompleted(httpClient, response, request);
        httpResponseMessage = response;
      }
      catch (Exception ex)
      {
        response?.Dispose();
        httpClient.HandleFinishSendAsyncError(ex, cts);
        throw;
      }
      finally
      {
        httpClient.HandleFinishSendAsyncCleanup(cts, disposeCts);
      }
      return httpResponseMessage;
    }

    private async Task<HttpResponseMessage> FinishSendAsyncUnbuffered(
      Task<HttpResponseMessage> sendTask,
      HttpRequestMessage request,
      CancellationTokenSource cts,
      bool disposeCts)
    {
      HttpClient httpClient = this;
      HttpResponseMessage httpResponseMessage;
      try
      {
        HttpResponseMessage response = await sendTask.ConfigureAwait(false);
        if (response == null)
          throw new InvalidOperationException(SR.net_http_handler_noresponse);
        if (NetEventSource.IsEnabled)
          NetEventSource.ClientSendCompleted(httpClient, response, request);
        httpResponseMessage = response;
      }
      catch (Exception ex)
      {
        httpClient.HandleFinishSendAsyncError(ex, cts);
        throw;
      }
      finally
      {
        httpClient.HandleFinishSendAsyncCleanup(cts, disposeCts);
      }
      return httpResponseMessage;
    }

    private void HandleFinishSendAsyncError(Exception e, CancellationTokenSource cts)
    {
      if (NetEventSource.IsEnabled)
        NetEventSource.Error((object) this, (object) e, nameof (HandleFinishSendAsyncError));
      if (cts.IsCancellationRequested && e is HttpRequestException)
      {
        if (NetEventSource.IsEnabled)
          NetEventSource.Error((object) this, (object) "Canceled", nameof (HandleFinishSendAsyncError));
        throw new OperationCanceledException(cts.Token);
      }
    }

    private void HandleFinishSendAsyncCleanup(CancellationTokenSource cts, bool disposeCts)
    {
      if (!disposeCts)
        return;
      cts.Dispose();
    }

    /// <summary>取消该实例所有挂起的请求。</summary>
    public void CancelPendingRequests()
    {
      this.CheckDisposed();
      if (NetEventSource.IsEnabled)
        NetEventSource.Enter((object) this, (FormattableString) null, nameof (CancelPendingRequests));
      CancellationTokenSource cancellationTokenSource = Interlocked.Exchange<CancellationTokenSource>(ref this._pendingRequestsCts, new CancellationTokenSource());
      cancellationTokenSource.Cancel();
      cancellationTokenSource.Dispose();
      if (!NetEventSource.IsEnabled)
        return;
      NetEventSource.Exit((object) this, (FormattableString) null, nameof (CancelPendingRequests));
    }

    /// <summary>释放由 <see cref="T:System.Net.Http.HttpClient" /> 使用的非托管资源，并可根据需要释放托管资源。</summary>
    /// <param name="disposing">如果释放托管资源和非托管资源，则为 <see langword="true" />；如果仅释放非托管资源，则为 <see langword="false" />。</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && !this._disposed)
      {
        this._disposed = true;
        this._pendingRequestsCts.Cancel();
        this._pendingRequestsCts.Dispose();
      }
      base.Dispose(disposing);
    }

    private void SetOperationStarted()
    {
      if (this._operationStarted)
        return;
      this._operationStarted = true;
    }

    private void CheckDisposedOrStarted()
    {
      this.CheckDisposed();
      if (this._operationStarted)
        throw new InvalidOperationException(SR.net_http_operation_started);
    }

    private void CheckDisposed()
    {
      if (this._disposed)
        throw new ObjectDisposedException(this.GetType().ToString());
    }

    private static void CheckRequestMessage(HttpRequestMessage request)
    {
      if (!request.MarkAsSent())
        throw new InvalidOperationException(SR.net_http_client_request_already_sent);
    }

    private void PrepareRequestMessage(HttpRequestMessage request)
    {
      Uri uri = (Uri) null;
      if (request.RequestUri == (Uri) null && this._baseAddress == (Uri) null)
        throw new InvalidOperationException(SR.net_http_client_invalid_requesturi);
      if (request.RequestUri == (Uri) null)
        uri = this._baseAddress;
      else if (!request.RequestUri.IsAbsoluteUri)
      {
        if (this._baseAddress == (Uri) null)
          throw new InvalidOperationException(SR.net_http_client_invalid_requesturi);
        uri = new Uri(this._baseAddress, request.RequestUri);
      }
      if (uri != (Uri) null)
        request.RequestUri = uri;
      if (this._defaultRequestHeaders == null)
        return;
      request.Headers.AddHeaders((HttpHeaders) this._defaultRequestHeaders);
    }

    private static void CheckBaseAddress(Uri baseAddress, string parameterName)
    {
      if (baseAddress == (Uri) null)
        return;
      if (!baseAddress.IsAbsoluteUri)
        throw new ArgumentException(SR.net_http_client_absolute_baseaddress_required, parameterName);
      if (!HttpUtilities.IsHttpUri(baseAddress))
        throw new ArgumentException(SR.net_http_client_http_baseaddress_required, parameterName);
    }

    private Uri CreateUri(string uri)
    {
      return !string.IsNullOrEmpty(uri) ? new Uri(uri, UriKind.RelativeOrAbsolute) : (Uri) null;
    }

    private HttpRequestMessage CreateRequestMessage(HttpMethod method, Uri uri)
    {
      return new HttpRequestMessage(method, uri)
      {
        Version = this._defaultRequestVersion
      };
    }
  }
}

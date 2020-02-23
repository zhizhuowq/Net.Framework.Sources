// Decompiled with JetBrains decompiler
// Type: System.Net.Http.MultipartFormDataContent
// Assembly: System.Net.Http, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: D8D12B97-8555-48F6-96B3-BBC6BF89FCBA
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Net.Http.dll

using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http
{
  /// <summary>为使用 multipart/form-data MIME 类型进行编码的内容提供容器。</summary>
  public class MultipartFormDataContent : MultipartContent
  {
    /// <summary>创建 <see cref="T:System.Net.Http.MultipartFormDataContent" /> 类的新实例。</summary>
    public MultipartFormDataContent()
      : base("form-data")
    {
    }

    /// <summary>创建 <see cref="T:System.Net.Http.MultipartFormDataContent" /> 类的新实例。</summary>
    /// <param name="boundary">多部分窗体数据内容的边界字符串。</param>
    /// <exception cref="T:System.ArgumentException">
    ///         <paramref name="boundary" /> 为 <see langword="null" /> 或者只包含空格字符。
    /// 或
    /// <paramref name="boundary" /> 以空格字符结尾。</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="boundary" /> 的长度大于 70。</exception>
    public MultipartFormDataContent(string boundary)
      : base("form-data", boundary)
    {
    }

    /// <summary>向序列化为多部/窗体数据 MIME 类型的 <see cref="T:System.Net.Http.HttpContent" /> 对象集合添加 HTTP 内容。</summary>
    /// <param name="content">要添加到集合中的 HTTP 内容。</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="content" /> 是 <see langword="null" />。</exception>
    public override void Add(HttpContent content)
    {
      if (content == null)
        throw new ArgumentNullException(nameof (content));
      if (content.Headers.ContentDisposition == null)
        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
      base.Add(content);
    }

    /// <summary>向序列化为多部/窗体数据 MIME 类型的 <see cref="T:System.Net.Http.HttpContent" /> 对象集合添加 HTTP 内容。</summary>
    /// <param name="content">要添加到集合中的 HTTP 内容。</param>
    /// <param name="name">要添加的 HTTP 内容的名称。</param>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="name" /> 为 <see langword="null" /> 或者只包含空格字符。</exception>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="content" /> 是 <see langword="null" />。</exception>
    public void Add(HttpContent content, string name)
    {
      if (content == null)
        throw new ArgumentNullException(nameof (content));
      if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException(SR.net_http_argument_empty_string, nameof (name));
      this.AddInternal(content, name, (string) null);
    }

    /// <summary>向序列化为多部/窗体数据 MIME 类型的 <see cref="T:System.Net.Http.HttpContent" /> 对象集合添加 HTTP 内容。</summary>
    /// <param name="content">要添加到集合中的 HTTP 内容。</param>
    /// <param name="name">要添加的 HTTP 内容的名称。</param>
    /// <param name="fileName">要添加到集合中的 HTTP 内容的文件名。</param>
    /// <exception cref="T:System.ArgumentException">
    ///         <paramref name="name" /> 为 <see langword="null" /> 或者只包含空格字符。
    /// 或
    /// <paramref name="fileName" /> 为 <see langword="null" /> 或者只包含空格字符。</exception>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="content" /> 是 <see langword="null" />。</exception>
    public void Add(HttpContent content, string name, string fileName)
    {
      if (content == null)
        throw new ArgumentNullException(nameof (content));
      if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException(SR.net_http_argument_empty_string, nameof (name));
      if (string.IsNullOrWhiteSpace(fileName))
        throw new ArgumentException(SR.net_http_argument_empty_string, nameof (fileName));
      this.AddInternal(content, name, fileName);
    }

    private void AddInternal(HttpContent content, string name, string fileName)
    {
      if (content.Headers.ContentDisposition == null)
        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
          Name = name,
          FileName = fileName,
          FileNameStar = fileName
        };
      base.Add(content);
    }

    internal override Task SerializeToStreamAsync(
      Stream stream,
      TransportContext context,
      CancellationToken cancellationToken)
    {
      return !(this.GetType() == typeof (MultipartFormDataContent)) ? base.SerializeToStreamAsync(stream, context, cancellationToken) : this.SerializeToStreamAsyncCore(stream, context, cancellationToken);
    }
  }
}

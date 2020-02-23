// Decompiled with JetBrains decompiler
// Type: System.Net.Http.Headers.HttpContentHeaders
// Assembly: System.Net.Http, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: D8D12B97-8555-48F6-96B3-BBC6BF89FCBA
// Assembly location: C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.1\System.Net.Http.dll

using System.Collections.Generic;

namespace System.Net.Http.Headers
{
  /// <summary>表示 RFC 2616 中定义的“内容标头”的集合。</summary>
  public sealed class HttpContentHeaders : HttpHeaders
  {
    private readonly HttpContent _parent;
    private bool _contentLengthSet;
    private HttpHeaderValueCollection<string> _allow;
    private HttpHeaderValueCollection<string> _contentEncoding;
    private HttpHeaderValueCollection<string> _contentLanguage;

    /// <summary>获取 HTTP 响应上的 <see langword="Allow" /> 内容标头值。</summary>
    /// <returns>HTTP 响应上的 <see langword="Allow" /> 标头的值。</returns>
    public ICollection<string> Allow
    {
      get
      {
        if (this._allow == null)
          this._allow = new HttpHeaderValueCollection<string>(KnownHeaders.Allow.Descriptor, (HttpHeaders) this, HeaderUtilities.TokenValidator);
        return (ICollection<string>) this._allow;
      }
    }

    /// <summary>获取 HTTP 响应上的 <see langword="Content-Disposition" /> 内容标头值。</summary>
    /// <returns>HTTP 响应上的 <see langword="Content-Disposition" /> 内容标头值。</returns>
    public ContentDispositionHeaderValue ContentDisposition
    {
      get
      {
        return (ContentDispositionHeaderValue) this.GetParsedValues(KnownHeaders.ContentDisposition.Descriptor);
      }
      set
      {
        this.SetOrRemoveParsedValue(KnownHeaders.ContentDisposition.Descriptor, (object) value);
      }
    }

    /// <summary>获取 HTTP 响应上的 <see langword="Content-Encoding" /> 内容标头值。</summary>
    /// <returns>HTTP 响应上的 <see langword="Content-Encoding" /> 内容标头值。</returns>
    public ICollection<string> ContentEncoding
    {
      get
      {
        if (this._contentEncoding == null)
          this._contentEncoding = new HttpHeaderValueCollection<string>(KnownHeaders.ContentEncoding.Descriptor, (HttpHeaders) this, HeaderUtilities.TokenValidator);
        return (ICollection<string>) this._contentEncoding;
      }
    }

    /// <summary>获取 HTTP 响应上的 <see langword="Content-Language" /> 内容标头值。</summary>
    /// <returns>HTTP 响应上的 <see langword="Content-Language" /> 内容标头值。</returns>
    public ICollection<string> ContentLanguage
    {
      get
      {
        if (this._contentLanguage == null)
          this._contentLanguage = new HttpHeaderValueCollection<string>(KnownHeaders.ContentLanguage.Descriptor, (HttpHeaders) this, HeaderUtilities.TokenValidator);
        return (ICollection<string>) this._contentLanguage;
      }
    }

    /// <summary>获取或设置 HTTP 响应上的 <see langword="Content-Length" /> 内容标头值。</summary>
    /// <returns>HTTP 响应上的 <see langword="Content-Length" /> 内容标头值。</returns>
    public long? ContentLength
    {
      get
      {
        object parsedValues = this.GetParsedValues(KnownHeaders.ContentLength.Descriptor);
        if (!this._contentLengthSet && parsedValues == null)
        {
          long? computedOrBufferLength = this._parent.GetComputedOrBufferLength();
          if (computedOrBufferLength.HasValue)
            this.SetParsedValue(KnownHeaders.ContentLength.Descriptor, (object) computedOrBufferLength.Value);
          return computedOrBufferLength;
        }
        return parsedValues == null ? new long?() : new long?((long) parsedValues);
      }
      set
      {
        this.SetOrRemoveParsedValue(KnownHeaders.ContentLength.Descriptor, (object) value);
        this._contentLengthSet = true;
      }
    }

    /// <summary>获取或设置 HTTP 响应上的 <see langword="Content-Location" /> 内容标头值。</summary>
    /// <returns>HTTP 响应上的 <see langword="Content-Location" /> 内容标头值。</returns>
    public Uri ContentLocation
    {
      get
      {
        return (Uri) this.GetParsedValues(KnownHeaders.ContentLocation.Descriptor);
      }
      set
      {
        this.SetOrRemoveParsedValue(KnownHeaders.ContentLocation.Descriptor, (object) value);
      }
    }

    /// <summary>获取或设置 HTTP 响应上的 <see langword="Content-MD5" /> 内容标头值。</summary>
    /// <returns>HTTP 响应上的 <see langword="Content-MD5" /> 内容标头值。</returns>
    public byte[] ContentMD5
    {
      get
      {
        return (byte[]) this.GetParsedValues(KnownHeaders.ContentMD5.Descriptor);
      }
      set
      {
        this.SetOrRemoveParsedValue(KnownHeaders.ContentMD5.Descriptor, (object) value);
      }
    }

    /// <summary>获取或设置 HTTP 响应上的 <see langword="Content-Range" /> 内容标头值。</summary>
    /// <returns>HTTP 响应上的 <see langword="Content-Range" /> 内容标头值。</returns>
    public ContentRangeHeaderValue ContentRange
    {
      get
      {
        return (ContentRangeHeaderValue) this.GetParsedValues(KnownHeaders.ContentRange.Descriptor);
      }
      set
      {
        this.SetOrRemoveParsedValue(KnownHeaders.ContentRange.Descriptor, (object) value);
      }
    }

    /// <summary>获取或设置 HTTP 响应上的 <see langword="Content-Type" /> 内容标头值。</summary>
    /// <returns>HTTP 响应上的 <see langword="Content-Type" /> 内容标头值。</returns>
    public MediaTypeHeaderValue ContentType
    {
      get
      {
        return (MediaTypeHeaderValue) this.GetParsedValues(KnownHeaders.ContentType.Descriptor);
      }
      set
      {
        this.SetOrRemoveParsedValue(KnownHeaders.ContentType.Descriptor, (object) value);
      }
    }

    /// <summary>获取或设置 HTTP 响应上的 <see langword="Expires" /> 内容标头值。</summary>
    /// <returns>HTTP 响应上的 <see langword="Expires" /> 内容标头值。</returns>
    public DateTimeOffset? Expires
    {
      get
      {
        return HeaderUtilities.GetDateTimeOffsetValue(KnownHeaders.Expires.Descriptor, (HttpHeaders) this, new DateTimeOffset?(DateTimeOffset.MinValue));
      }
      set
      {
        this.SetOrRemoveParsedValue(KnownHeaders.Expires.Descriptor, (object) value);
      }
    }

    /// <summary>获取或设置 HTTP 响应上的 <see langword="Last-Modified" /> 内容标头值。</summary>
    /// <returns>HTTP 响应上的 <see langword="Last-Modified" /> 内容标头值。</returns>
    public DateTimeOffset? LastModified
    {
      get
      {
        return HeaderUtilities.GetDateTimeOffsetValue(KnownHeaders.LastModified.Descriptor, (HttpHeaders) this, new DateTimeOffset?());
      }
      set
      {
        this.SetOrRemoveParsedValue(KnownHeaders.LastModified.Descriptor, (object) value);
      }
    }

    internal HttpContentHeaders(HttpContent parent)
      : base(HttpHeaderType.Content | HttpHeaderType.Custom, HttpHeaderType.None)
    {
      this._parent = parent;
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.JsonConvert
// Assembly: Newtonsoft.Json, Version=12.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed
// MVID: 20735148-1523-4917-A5E8-F91B0B239405
// Assembly location: C:\Users\WangQiang\.nuget\packages\newtonsoft.json\12.0.3\lib\netstandard2.0\Newtonsoft.Json.dll

using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Provides methods for converting between .NET types and JSON types.
  /// </summary>
  /// <example>
  ///   <code lang="cs" source="..\Src\Newtonsoft.Json.Tests\Documentation\SerializationTests.cs" region="SerializeObject" title="Serializing and Deserializing JSON with JsonConvert" />
  /// </example>
  [NullableContext(1)]
  [Nullable(0)]
  public static class JsonConvert
  {
    /// <summary>
    /// Represents JavaScript's boolean value <c>true</c> as a string. This field is read-only.
    /// </summary>
    public static readonly string True = "true";
    /// <summary>
    /// Represents JavaScript's boolean value <c>false</c> as a string. This field is read-only.
    /// </summary>
    public static readonly string False = "false";
    /// <summary>
    /// Represents JavaScript's <c>null</c> as a string. This field is read-only.
    /// </summary>
    public static readonly string Null = "null";
    /// <summary>
    /// Represents JavaScript's <c>undefined</c> as a string. This field is read-only.
    /// </summary>
    public static readonly string Undefined = "undefined";
    /// <summary>
    /// Represents JavaScript's positive infinity as a string. This field is read-only.
    /// </summary>
    public static readonly string PositiveInfinity = "Infinity";
    /// <summary>
    /// Represents JavaScript's negative infinity as a string. This field is read-only.
    /// </summary>
    public static readonly string NegativeInfinity = "-Infinity";
    /// <summary>
    /// Represents JavaScript's <c>NaN</c> as a string. This field is read-only.
    /// </summary>
    public static readonly string NaN = nameof (NaN);

    /// <summary>
    /// Gets or sets a function that creates default <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
    /// Default settings are automatically used by serialization methods on <see cref="T:Newtonsoft.Json.JsonConvert" />,
    /// and <see cref="M:Newtonsoft.Json.Linq.JToken.ToObject``1" /> and <see cref="M:Newtonsoft.Json.Linq.JToken.FromObject(System.Object)" /> on <see cref="T:Newtonsoft.Json.Linq.JToken" />.
    /// To serialize without using any default settings create a <see cref="T:Newtonsoft.Json.JsonSerializer" /> with
    /// <see cref="M:Newtonsoft.Json.JsonSerializer.Create" />.
    /// </summary>
    [Nullable(new byte[] {2, 1})]
    public static Func<JsonSerializerSettings> DefaultSettings { [return: Nullable(new byte[] {2, 1})] get; [param: Nullable(new byte[] {2, 1})] set; }

    /// <summary>
    /// Converts the <see cref="T:System.DateTime" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.DateTime" />.</returns>
    public static string ToString(DateTime value)
    {
      return JsonConvert.ToString(value, DateFormatHandling.IsoDateFormat, DateTimeZoneHandling.RoundtripKind);
    }

    /// <summary>
    /// Converts the <see cref="T:System.DateTime" /> to its JSON string representation using the <see cref="T:Newtonsoft.Json.DateFormatHandling" /> specified.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="format">The format the date will be converted to.</param>
    /// <param name="timeZoneHandling">The time zone handling when the date is converted to a string.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.DateTime" />.</returns>
    public static string ToString(
      DateTime value,
      DateFormatHandling format,
      DateTimeZoneHandling timeZoneHandling)
    {
      DateTime dateTime = DateTimeUtils.EnsureDateTime(value, timeZoneHandling);
      using (StringWriter stringWriter = StringUtils.CreateStringWriter(64))
      {
        stringWriter.Write('"');
        DateTimeUtils.WriteDateTimeString((TextWriter) stringWriter, dateTime, format, (string) null, CultureInfo.InvariantCulture);
        stringWriter.Write('"');
        return stringWriter.ToString();
      }
    }

    /// <summary>
    /// Converts the <see cref="T:System.DateTimeOffset" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.DateTimeOffset" />.</returns>
    public static string ToString(DateTimeOffset value)
    {
      return JsonConvert.ToString(value, DateFormatHandling.IsoDateFormat);
    }

    /// <summary>
    /// Converts the <see cref="T:System.DateTimeOffset" /> to its JSON string representation using the <see cref="T:Newtonsoft.Json.DateFormatHandling" /> specified.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="format">The format the date will be converted to.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.DateTimeOffset" />.</returns>
    public static string ToString(DateTimeOffset value, DateFormatHandling format)
    {
      using (StringWriter stringWriter = StringUtils.CreateStringWriter(64))
      {
        stringWriter.Write('"');
        DateTimeUtils.WriteDateTimeOffsetString((TextWriter) stringWriter, value, format, (string) null, CultureInfo.InvariantCulture);
        stringWriter.Write('"');
        return stringWriter.ToString();
      }
    }

    /// <summary>
    /// Converts the <see cref="T:System.Boolean" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.Boolean" />.</returns>
    public static string ToString(bool value)
    {
      return !value ? JsonConvert.False : JsonConvert.True;
    }

    /// <summary>
    /// Converts the <see cref="T:System.Char" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.Char" />.</returns>
    public static string ToString(char value)
    {
      return JsonConvert.ToString(char.ToString(value));
    }

    /// <summary>
    /// Converts the <see cref="T:System.Enum" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.Enum" />.</returns>
    public static string ToString(Enum value)
    {
      return value.ToString("D");
    }

    /// <summary>
    /// Converts the <see cref="T:System.Int32" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.Int32" />.</returns>
    public static string ToString(int value)
    {
      return value.ToString((string) null, (IFormatProvider) CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="T:System.Int16" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.Int16" />.</returns>
    public static string ToString(short value)
    {
      return value.ToString((string) null, (IFormatProvider) CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="T:System.UInt16" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.UInt16" />.</returns>
    [CLSCompliant(false)]
    public static string ToString(ushort value)
    {
      return value.ToString((string) null, (IFormatProvider) CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="T:System.UInt32" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.UInt32" />.</returns>
    [CLSCompliant(false)]
    public static string ToString(uint value)
    {
      return value.ToString((string) null, (IFormatProvider) CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="T:System.Int64" />  to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.Int64" />.</returns>
    public static string ToString(long value)
    {
      return value.ToString((string) null, (IFormatProvider) CultureInfo.InvariantCulture);
    }

    private static string ToStringInternal(BigInteger value)
    {
      return value.ToString((string) null, (IFormatProvider) CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="T:System.UInt64" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.UInt64" />.</returns>
    [CLSCompliant(false)]
    public static string ToString(ulong value)
    {
      return value.ToString((string) null, (IFormatProvider) CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="T:System.Single" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.Single" />.</returns>
    public static string ToString(float value)
    {
      return JsonConvert.EnsureDecimalPlace((double) value, value.ToString("R", (IFormatProvider) CultureInfo.InvariantCulture));
    }

    internal static string ToString(
      float value,
      FloatFormatHandling floatFormatHandling,
      char quoteChar,
      bool nullable)
    {
      return JsonConvert.EnsureFloatFormat((double) value, JsonConvert.EnsureDecimalPlace((double) value, value.ToString("R", (IFormatProvider) CultureInfo.InvariantCulture)), floatFormatHandling, quoteChar, nullable);
    }

    private static string EnsureFloatFormat(
      double value,
      string text,
      FloatFormatHandling floatFormatHandling,
      char quoteChar,
      bool nullable)
    {
      if (floatFormatHandling == FloatFormatHandling.Symbol || !double.IsInfinity(value) && !double.IsNaN(value))
        return text;
      if (floatFormatHandling != FloatFormatHandling.DefaultValue)
        return quoteChar.ToString() + text + quoteChar.ToString();
      return nullable ? JsonConvert.Null : "0.0";
    }

    /// <summary>
    /// Converts the <see cref="T:System.Double" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.Double" />.</returns>
    public static string ToString(double value)
    {
      return JsonConvert.EnsureDecimalPlace(value, value.ToString("R", (IFormatProvider) CultureInfo.InvariantCulture));
    }

    internal static string ToString(
      double value,
      FloatFormatHandling floatFormatHandling,
      char quoteChar,
      bool nullable)
    {
      return JsonConvert.EnsureFloatFormat(value, JsonConvert.EnsureDecimalPlace(value, value.ToString("R", (IFormatProvider) CultureInfo.InvariantCulture)), floatFormatHandling, quoteChar, nullable);
    }

    private static string EnsureDecimalPlace(double value, string text)
    {
      return double.IsNaN(value) || double.IsInfinity(value) || (text.IndexOf('.') != -1 || text.IndexOf('E') != -1) || text.IndexOf('e') != -1 ? text : text + ".0";
    }

    private static string EnsureDecimalPlace(string text)
    {
      return text.IndexOf('.') != -1 ? text : text + ".0";
    }

    /// <summary>
    /// Converts the <see cref="T:System.Byte" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.Byte" />.</returns>
    public static string ToString(byte value)
    {
      return value.ToString((string) null, (IFormatProvider) CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="T:System.SByte" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.SByte" />.</returns>
    [CLSCompliant(false)]
    public static string ToString(sbyte value)
    {
      return value.ToString((string) null, (IFormatProvider) CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="T:System.Decimal" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.Decimal" />.</returns>
    public static string ToString(Decimal value)
    {
      return JsonConvert.EnsureDecimalPlace(value.ToString((string) null, (IFormatProvider) CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Converts the <see cref="T:System.Guid" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.Guid" />.</returns>
    public static string ToString(Guid value)
    {
      return JsonConvert.ToString(value, '"');
    }

    internal static string ToString(Guid value, char quoteChar)
    {
      string str1 = value.ToString("D", (IFormatProvider) CultureInfo.InvariantCulture);
      string str2 = quoteChar.ToString((IFormatProvider) CultureInfo.InvariantCulture);
      return str2 + str1 + str2;
    }

    /// <summary>
    /// Converts the <see cref="T:System.TimeSpan" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.TimeSpan" />.</returns>
    public static string ToString(TimeSpan value)
    {
      return JsonConvert.ToString(value, '"');
    }

    internal static string ToString(TimeSpan value, char quoteChar)
    {
      return JsonConvert.ToString(value.ToString(), quoteChar);
    }

    /// <summary>
    /// Converts the <see cref="T:System.Uri" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.Uri" />.</returns>
    public static string ToString([Nullable(2)] Uri value)
    {
      return value == (Uri) null ? JsonConvert.Null : JsonConvert.ToString(value, '"');
    }

    internal static string ToString(Uri value, char quoteChar)
    {
      return JsonConvert.ToString(value.OriginalString, quoteChar);
    }

    /// <summary>
    /// Converts the <see cref="T:System.String" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.String" />.</returns>
    public static string ToString([Nullable(2)] string value)
    {
      return JsonConvert.ToString(value, '"');
    }

    /// <summary>
    /// Converts the <see cref="T:System.String" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="delimiter">The string delimiter character.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.String" />.</returns>
    public static string ToString([Nullable(2)] string value, char delimiter)
    {
      return JsonConvert.ToString(value, delimiter, StringEscapeHandling.Default);
    }

    /// <summary>
    /// Converts the <see cref="T:System.String" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="delimiter">The string delimiter character.</param>
    /// <param name="stringEscapeHandling">The string escape handling.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.String" />.</returns>
    public static string ToString(
      [Nullable(2)] string value,
      char delimiter,
      StringEscapeHandling stringEscapeHandling)
    {
      if (delimiter != '"' && delimiter != '\'')
        throw new ArgumentException("Delimiter must be a single or double quote.", nameof (delimiter));
      return JavaScriptUtils.ToEscapedJavaScriptString(value, delimiter, true, stringEscapeHandling);
    }

    /// <summary>
    /// Converts the <see cref="T:System.Object" /> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="T:System.Object" />.</returns>
    public static string ToString([Nullable(2)] object value)
    {
      if (value == null)
        return JsonConvert.Null;
      switch (ConvertUtils.GetTypeCode(value.GetType()))
      {
        case PrimitiveTypeCode.Char:
          return JsonConvert.ToString((char) value);
        case PrimitiveTypeCode.Boolean:
          return JsonConvert.ToString((bool) value);
        case PrimitiveTypeCode.SByte:
          return JsonConvert.ToString((sbyte) value);
        case PrimitiveTypeCode.Int16:
          return JsonConvert.ToString((short) value);
        case PrimitiveTypeCode.UInt16:
          return JsonConvert.ToString((ushort) value);
        case PrimitiveTypeCode.Int32:
          return JsonConvert.ToString((int) value);
        case PrimitiveTypeCode.Byte:
          return JsonConvert.ToString((byte) value);
        case PrimitiveTypeCode.UInt32:
          return JsonConvert.ToString((uint) value);
        case PrimitiveTypeCode.Int64:
          return JsonConvert.ToString((long) value);
        case PrimitiveTypeCode.UInt64:
          return JsonConvert.ToString((ulong) value);
        case PrimitiveTypeCode.Single:
          return JsonConvert.ToString((float) value);
        case PrimitiveTypeCode.Double:
          return JsonConvert.ToString((double) value);
        case PrimitiveTypeCode.DateTime:
          return JsonConvert.ToString((DateTime) value);
        case PrimitiveTypeCode.DateTimeOffset:
          return JsonConvert.ToString((DateTimeOffset) value);
        case PrimitiveTypeCode.Decimal:
          return JsonConvert.ToString((Decimal) value);
        case PrimitiveTypeCode.Guid:
          return JsonConvert.ToString((Guid) value);
        case PrimitiveTypeCode.TimeSpan:
          return JsonConvert.ToString((TimeSpan) value);
        case PrimitiveTypeCode.BigInteger:
          return JsonConvert.ToStringInternal((BigInteger) value);
        case PrimitiveTypeCode.Uri:
          return JsonConvert.ToString((Uri) value);
        case PrimitiveTypeCode.String:
          return JsonConvert.ToString((string) value);
        case PrimitiveTypeCode.DBNull:
          return JsonConvert.Null;
        default:
          throw new ArgumentException("Unsupported type: {0}. Use the JsonSerializer class to get the object's JSON representation.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) value.GetType()));
      }
    }

    /// <summary>Serializes the specified object to a JSON string.</summary>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    [DebuggerStepThrough]
    public static string SerializeObject([Nullable(2)] object value)
    {
      return JsonConvert.SerializeObject(value, (Type) null, (JsonSerializerSettings) null);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using formatting.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="formatting">Indicates how the output should be formatted.</param>
    /// <returns>A JSON string representation of the object.</returns>
    [DebuggerStepThrough]
    public static string SerializeObject([Nullable(2)] object value, Formatting formatting)
    {
      return JsonConvert.SerializeObject(value, formatting, (JsonSerializerSettings) null);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using a collection of <see cref="T:Newtonsoft.Json.JsonConverter" />.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="converters">A collection of converters used while serializing.</param>
    /// <returns>A JSON string representation of the object.</returns>
    [DebuggerStepThrough]
    public static string SerializeObject([Nullable(2)] object value, params JsonConverter[] converters)
    {
      JsonSerializerSettings serializerSettings;
      if (converters == null || converters.Length == 0)
        serializerSettings = (JsonSerializerSettings) null;
      else
        serializerSettings = new JsonSerializerSettings()
        {
          Converters = (IList<JsonConverter>) converters
        };
      JsonSerializerSettings settings = serializerSettings;
      return JsonConvert.SerializeObject(value, (Type) null, settings);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using formatting and a collection of <see cref="T:Newtonsoft.Json.JsonConverter" />.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="formatting">Indicates how the output should be formatted.</param>
    /// <param name="converters">A collection of converters used while serializing.</param>
    /// <returns>A JSON string representation of the object.</returns>
    [DebuggerStepThrough]
    public static string SerializeObject(
      [Nullable(2)] object value,
      Formatting formatting,
      params JsonConverter[] converters)
    {
      JsonSerializerSettings serializerSettings;
      if (converters == null || converters.Length == 0)
        serializerSettings = (JsonSerializerSettings) null;
      else
        serializerSettings = new JsonSerializerSettings()
        {
          Converters = (IList<JsonConverter>) converters
        };
      JsonSerializerSettings settings = serializerSettings;
      return JsonConvert.SerializeObject(value, (Type) null, formatting, settings);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="settings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to serialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.</param>
    /// <returns>A JSON string representation of the object.</returns>
    [DebuggerStepThrough]
    public static string SerializeObject([Nullable(2)] object value, JsonSerializerSettings settings)
    {
      return JsonConvert.SerializeObject(value, (Type) null, settings);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using a type, formatting and <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="settings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to serialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.</param>
    /// <param name="type">
    /// The type of the value being serialized.
    /// This parameter is used when <see cref="P:Newtonsoft.Json.JsonSerializer.TypeNameHandling" /> is <see cref="F:Newtonsoft.Json.TypeNameHandling.Auto" /> to write out the type name if the type of the value does not match.
    /// Specifying the type is optional.
    /// </param>
    /// <returns>A JSON string representation of the object.</returns>
    [NullableContext(2)]
    [DebuggerStepThrough]
    [return: Nullable(1)]
    public static string SerializeObject(object value, Type type, JsonSerializerSettings settings)
    {
      JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
      return JsonConvert.SerializeObjectInternal(value, type, jsonSerializer);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using formatting and <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="formatting">Indicates how the output should be formatted.</param>
    /// <param name="settings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to serialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.</param>
    /// <returns>A JSON string representation of the object.</returns>
    [NullableContext(2)]
    [DebuggerStepThrough]
    [return: Nullable(1)]
    public static string SerializeObject(
      object value,
      Formatting formatting,
      JsonSerializerSettings settings)
    {
      return JsonConvert.SerializeObject(value, (Type) null, formatting, settings);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using a type, formatting and <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="formatting">Indicates how the output should be formatted.</param>
    /// <param name="settings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to serialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.</param>
    /// <param name="type">
    /// The type of the value being serialized.
    /// This parameter is used when <see cref="P:Newtonsoft.Json.JsonSerializer.TypeNameHandling" /> is <see cref="F:Newtonsoft.Json.TypeNameHandling.Auto" /> to write out the type name if the type of the value does not match.
    /// Specifying the type is optional.
    /// </param>
    /// <returns>A JSON string representation of the object.</returns>
    [NullableContext(2)]
    [DebuggerStepThrough]
    [return: Nullable(1)]
    public static string SerializeObject(
      object value,
      Type type,
      Formatting formatting,
      JsonSerializerSettings settings)
    {
      JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
      jsonSerializer.Formatting = formatting;
      return JsonConvert.SerializeObjectInternal(value, type, jsonSerializer);
    }

    private static string SerializeObjectInternal(
      [Nullable(2)] object value,
      [Nullable(2)] Type type,
      JsonSerializer jsonSerializer)
    {
      StringWriter stringWriter = new StringWriter(new StringBuilder(256), (IFormatProvider) CultureInfo.InvariantCulture);
      using (JsonTextWriter jsonTextWriter = new JsonTextWriter((TextWriter) stringWriter))
      {
        jsonTextWriter.Formatting = jsonSerializer.Formatting;
        jsonSerializer.Serialize((JsonWriter) jsonTextWriter, value, type);
      }
      return stringWriter.ToString();
    }

    /// <summary>Deserializes the JSON to a .NET object.</summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <returns>The deserialized object from the JSON string.</returns>
    [DebuggerStepThrough]
    [return: Nullable(2)]
    public static object DeserializeObject(string value)
    {
      return JsonConvert.DeserializeObject(value, (Type) null, (JsonSerializerSettings) null);
    }

    /// <summary>
    /// Deserializes the JSON to a .NET object using <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="settings">
    /// The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to deserialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.
    /// </param>
    /// <returns>The deserialized object from the JSON string.</returns>
    [DebuggerStepThrough]
    [return: Nullable(2)]
    public static object DeserializeObject(string value, JsonSerializerSettings settings)
    {
      return JsonConvert.DeserializeObject(value, (Type) null, settings);
    }

    /// <summary>Deserializes the JSON to the specified .NET type.</summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="type">The <see cref="T:System.Type" /> of object being deserialized.</param>
    /// <returns>The deserialized object from the JSON string.</returns>
    [DebuggerStepThrough]
    [return: Nullable(2)]
    public static object DeserializeObject(string value, Type type)
    {
      return JsonConvert.DeserializeObject(value, type, (JsonSerializerSettings) null);
    }

    /// <summary>Deserializes the JSON to the specified .NET type.</summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <returns>The deserialized object from the JSON string.</returns>
    [DebuggerStepThrough]
    public static T DeserializeObject<[Nullable(2)] T>(string value)
    {
      return JsonConvert.DeserializeObject<T>(value, (JsonSerializerSettings) null);
    }

    /// <summary>Deserializes the JSON to the given anonymous type.</summary>
    /// <typeparam name="T">
    /// The anonymous type to deserialize to. This can't be specified
    /// traditionally and must be inferred from the anonymous type passed
    /// as a parameter.
    /// </typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="anonymousTypeObject">The anonymous type object.</param>
    /// <returns>The deserialized anonymous type from the JSON string.</returns>
    [DebuggerStepThrough]
    public static T DeserializeAnonymousType<[Nullable(2)] T>(
      string value,
      T anonymousTypeObject)
    {
      return JsonConvert.DeserializeObject<T>(value);
    }

    /// <summary>
    /// Deserializes the JSON to the given anonymous type using <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
    /// </summary>
    /// <typeparam name="T">
    /// The anonymous type to deserialize to. This can't be specified
    /// traditionally and must be inferred from the anonymous type passed
    /// as a parameter.
    /// </typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="anonymousTypeObject">The anonymous type object.</param>
    /// <param name="settings">
    /// The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to deserialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.
    /// </param>
    /// <returns>The deserialized anonymous type from the JSON string.</returns>
    [DebuggerStepThrough]
    public static T DeserializeAnonymousType<[Nullable(2)] T>(
      string value,
      T anonymousTypeObject,
      JsonSerializerSettings settings)
    {
      return JsonConvert.DeserializeObject<T>(value, settings);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type using a collection of <see cref="T:Newtonsoft.Json.JsonConverter" />.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="converters">Converters to use while deserializing.</param>
    /// <returns>The deserialized object from the JSON string.</returns>
    [DebuggerStepThrough]
    [return: MaybeNull]
    public static T DeserializeObject<[Nullable(2)] T>(
      string value,
      params JsonConverter[] converters)
    {
      return (T) JsonConvert.DeserializeObject(value, typeof (T), converters);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type using <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The object to deserialize.</param>
    /// <param name="settings">
    /// The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to deserialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.
    /// </param>
    /// <returns>The deserialized object from the JSON string.</returns>
    [DebuggerStepThrough]
    [return: MaybeNull]
    public static T DeserializeObject<[Nullable(2)] T>(
      string value,
      [Nullable(2)] JsonSerializerSettings settings)
    {
      return (T) JsonConvert.DeserializeObject(value, typeof (T), settings);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type using a collection of <see cref="T:Newtonsoft.Json.JsonConverter" />.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="type">The type of the object to deserialize.</param>
    /// <param name="converters">Converters to use while deserializing.</param>
    /// <returns>The deserialized object from the JSON string.</returns>
    [DebuggerStepThrough]
    [return: Nullable(2)]
    public static object DeserializeObject(
      string value,
      Type type,
      params JsonConverter[] converters)
    {
      JsonSerializerSettings serializerSettings;
      if (converters == null || converters.Length == 0)
        serializerSettings = (JsonSerializerSettings) null;
      else
        serializerSettings = new JsonSerializerSettings()
        {
          Converters = (IList<JsonConverter>) converters
        };
      JsonSerializerSettings settings = serializerSettings;
      return JsonConvert.DeserializeObject(value, type, settings);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type using <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="type">The type of the object to deserialize to.</param>
    /// <param name="settings">
    /// The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to deserialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.
    /// </param>
    /// <returns>The deserialized object from the JSON string.</returns>
    [NullableContext(2)]
    public static object DeserializeObject(
      [Nullable(1)] string value,
      Type type,
      JsonSerializerSettings settings)
    {
      ValidationUtils.ArgumentNotNull((object) value, nameof (value));
      JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
      if (!jsonSerializer.IsCheckAdditionalContentSet())
        jsonSerializer.CheckAdditionalContent = true;
      using (JsonTextReader jsonTextReader = new JsonTextReader((TextReader) new StringReader(value)))
        return jsonSerializer.Deserialize((JsonReader) jsonTextReader, type);
    }

    /// <summary>
    /// Populates the object with values from the JSON string.
    /// </summary>
    /// <param name="value">The JSON to populate values from.</param>
    /// <param name="target">The target object to populate values onto.</param>
    [DebuggerStepThrough]
    public static void PopulateObject(string value, object target)
    {
      JsonConvert.PopulateObject(value, target, (JsonSerializerSettings) null);
    }

    /// <summary>
    /// Populates the object with values from the JSON string using <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
    /// </summary>
    /// <param name="value">The JSON to populate values from.</param>
    /// <param name="target">The target object to populate values onto.</param>
    /// <param name="settings">
    /// The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to deserialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.
    /// </param>
    public static void PopulateObject(string value, object target, [Nullable(2)] JsonSerializerSettings settings)
    {
      using (JsonReader reader = (JsonReader) new JsonTextReader((TextReader) new StringReader(value)))
      {
        JsonSerializer.CreateDefault(settings).Populate(reader, target);
        if (settings == null || !settings.CheckAdditionalContent)
          return;
        while (reader.Read())
        {
          if (reader.TokenType != JsonToken.Comment)
            throw JsonSerializationException.Create(reader, "Additional text found in JSON string after finishing deserializing object.");
        }
      }
    }

    /// <summary>
    /// Serializes the <see cref="T:System.Xml.XmlNode" /> to a JSON string.
    /// </summary>
    /// <param name="node">The node to serialize.</param>
    /// <returns>A JSON string of the <see cref="T:System.Xml.XmlNode" />.</returns>
    public static string SerializeXmlNode([Nullable(2)] XmlNode node)
    {
      return JsonConvert.SerializeXmlNode(node, Formatting.None);
    }

    /// <summary>
    /// Serializes the <see cref="T:System.Xml.XmlNode" /> to a JSON string using formatting.
    /// </summary>
    /// <param name="node">The node to serialize.</param>
    /// <param name="formatting">Indicates how the output should be formatted.</param>
    /// <returns>A JSON string of the <see cref="T:System.Xml.XmlNode" />.</returns>
    public static string SerializeXmlNode([Nullable(2)] XmlNode node, Formatting formatting)
    {
      XmlNodeConverter xmlNodeConverter = new XmlNodeConverter();
      return JsonConvert.SerializeObject((object) node, formatting, (JsonConverter) xmlNodeConverter);
    }

    /// <summary>
    /// Serializes the <see cref="T:System.Xml.XmlNode" /> to a JSON string using formatting and omits the root object if <paramref name="omitRootObject" /> is <c>true</c>.
    /// </summary>
    /// <param name="node">The node to serialize.</param>
    /// <param name="formatting">Indicates how the output should be formatted.</param>
    /// <param name="omitRootObject">Omits writing the root object.</param>
    /// <returns>A JSON string of the <see cref="T:System.Xml.XmlNode" />.</returns>
    public static string SerializeXmlNode([Nullable(2)] XmlNode node, Formatting formatting, bool omitRootObject)
    {
      XmlNodeConverter xmlNodeConverter = new XmlNodeConverter()
      {
        OmitRootObject = omitRootObject
      };
      return JsonConvert.SerializeObject((object) node, formatting, (JsonConverter) xmlNodeConverter);
    }

    /// <summary>
    /// Deserializes the <see cref="T:System.Xml.XmlNode" /> from a JSON string.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <returns>The deserialized <see cref="T:System.Xml.XmlNode" />.</returns>
    [return: Nullable(2)]
    public static XmlDocument DeserializeXmlNode(string value)
    {
      return JsonConvert.DeserializeXmlNode(value, (string) null);
    }

    /// <summary>
    /// Deserializes the <see cref="T:System.Xml.XmlNode" /> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName" />.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <returns>The deserialized <see cref="T:System.Xml.XmlNode" />.</returns>
    [NullableContext(2)]
    public static XmlDocument DeserializeXmlNode(
      [Nullable(1)] string value,
      string deserializeRootElementName)
    {
      return JsonConvert.DeserializeXmlNode(value, deserializeRootElementName, false);
    }

    /// <summary>
    /// Deserializes the <see cref="T:System.Xml.XmlNode" /> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName" />
    /// and writes a Json.NET array attribute for collections.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <param name="writeArrayAttribute">
    /// A value to indicate whether to write the Json.NET array attribute.
    /// This attribute helps preserve arrays when converting the written XML back to JSON.
    /// </param>
    /// <returns>The deserialized <see cref="T:System.Xml.XmlNode" />.</returns>
    [NullableContext(2)]
    public static XmlDocument DeserializeXmlNode(
      [Nullable(1)] string value,
      string deserializeRootElementName,
      bool writeArrayAttribute)
    {
      return JsonConvert.DeserializeXmlNode(value, deserializeRootElementName, writeArrayAttribute, false);
    }

    /// <summary>
    /// Deserializes the <see cref="T:System.Xml.XmlNode" /> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName" />,
    /// writes a Json.NET array attribute for collections, and encodes special characters.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <param name="writeArrayAttribute">
    /// A value to indicate whether to write the Json.NET array attribute.
    /// This attribute helps preserve arrays when converting the written XML back to JSON.
    /// </param>
    /// <param name="encodeSpecialCharacters">
    /// A value to indicate whether to encode special characters when converting JSON to XML.
    /// If <c>true</c>, special characters like ':', '@', '?', '#' and '$' in JSON property names aren't used to specify
    /// XML namespaces, attributes or processing directives. Instead special characters are encoded and written
    /// as part of the XML element name.
    /// </param>
    /// <returns>The deserialized <see cref="T:System.Xml.XmlNode" />.</returns>
    [NullableContext(2)]
    public static XmlDocument DeserializeXmlNode(
      [Nullable(1)] string value,
      string deserializeRootElementName,
      bool writeArrayAttribute,
      bool encodeSpecialCharacters)
    {
      return (XmlDocument) JsonConvert.DeserializeObject(value, typeof (XmlDocument), (JsonConverter) new XmlNodeConverter()
      {
        DeserializeRootElementName = deserializeRootElementName,
        WriteArrayAttribute = writeArrayAttribute,
        EncodeSpecialCharacters = encodeSpecialCharacters
      });
    }

    /// <summary>
    /// Serializes the <see cref="T:System.Xml.Linq.XNode" /> to a JSON string.
    /// </summary>
    /// <param name="node">The node to convert to JSON.</param>
    /// <returns>A JSON string of the <see cref="T:System.Xml.Linq.XNode" />.</returns>
    public static string SerializeXNode([Nullable(2)] XObject node)
    {
      return JsonConvert.SerializeXNode(node, Formatting.None);
    }

    /// <summary>
    /// Serializes the <see cref="T:System.Xml.Linq.XNode" /> to a JSON string using formatting.
    /// </summary>
    /// <param name="node">The node to convert to JSON.</param>
    /// <param name="formatting">Indicates how the output should be formatted.</param>
    /// <returns>A JSON string of the <see cref="T:System.Xml.Linq.XNode" />.</returns>
    public static string SerializeXNode([Nullable(2)] XObject node, Formatting formatting)
    {
      return JsonConvert.SerializeXNode(node, formatting, false);
    }

    /// <summary>
    /// Serializes the <see cref="T:System.Xml.Linq.XNode" /> to a JSON string using formatting and omits the root object if <paramref name="omitRootObject" /> is <c>true</c>.
    /// </summary>
    /// <param name="node">The node to serialize.</param>
    /// <param name="formatting">Indicates how the output should be formatted.</param>
    /// <param name="omitRootObject">Omits writing the root object.</param>
    /// <returns>A JSON string of the <see cref="T:System.Xml.Linq.XNode" />.</returns>
    public static string SerializeXNode([Nullable(2)] XObject node, Formatting formatting, bool omitRootObject)
    {
      XmlNodeConverter xmlNodeConverter = new XmlNodeConverter()
      {
        OmitRootObject = omitRootObject
      };
      return JsonConvert.SerializeObject((object) node, formatting, (JsonConverter) xmlNodeConverter);
    }

    /// <summary>
    /// Deserializes the <see cref="T:System.Xml.Linq.XNode" /> from a JSON string.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <returns>The deserialized <see cref="T:System.Xml.Linq.XNode" />.</returns>
    [return: Nullable(2)]
    public static XDocument DeserializeXNode(string value)
    {
      return JsonConvert.DeserializeXNode(value, (string) null);
    }

    /// <summary>
    /// Deserializes the <see cref="T:System.Xml.Linq.XNode" /> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName" />.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <returns>The deserialized <see cref="T:System.Xml.Linq.XNode" />.</returns>
    [NullableContext(2)]
    public static XDocument DeserializeXNode(
      [Nullable(1)] string value,
      string deserializeRootElementName)
    {
      return JsonConvert.DeserializeXNode(value, deserializeRootElementName, false);
    }

    /// <summary>
    /// Deserializes the <see cref="T:System.Xml.Linq.XNode" /> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName" />
    /// and writes a Json.NET array attribute for collections.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <param name="writeArrayAttribute">
    /// A value to indicate whether to write the Json.NET array attribute.
    /// This attribute helps preserve arrays when converting the written XML back to JSON.
    /// </param>
    /// <returns>The deserialized <see cref="T:System.Xml.Linq.XNode" />.</returns>
    [NullableContext(2)]
    public static XDocument DeserializeXNode(
      [Nullable(1)] string value,
      string deserializeRootElementName,
      bool writeArrayAttribute)
    {
      return JsonConvert.DeserializeXNode(value, deserializeRootElementName, writeArrayAttribute, false);
    }

    /// <summary>
    /// Deserializes the <see cref="T:System.Xml.Linq.XNode" /> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName" />,
    /// writes a Json.NET array attribute for collections, and encodes special characters.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <param name="writeArrayAttribute">
    /// A value to indicate whether to write the Json.NET array attribute.
    /// This attribute helps preserve arrays when converting the written XML back to JSON.
    /// </param>
    /// <param name="encodeSpecialCharacters">
    /// A value to indicate whether to encode special characters when converting JSON to XML.
    /// If <c>true</c>, special characters like ':', '@', '?', '#' and '$' in JSON property names aren't used to specify
    /// XML namespaces, attributes or processing directives. Instead special characters are encoded and written
    /// as part of the XML element name.
    /// </param>
    /// <returns>The deserialized <see cref="T:System.Xml.Linq.XNode" />.</returns>
    [NullableContext(2)]
    public static XDocument DeserializeXNode(
      [Nullable(1)] string value,
      string deserializeRootElementName,
      bool writeArrayAttribute,
      bool encodeSpecialCharacters)
    {
      return (XDocument) JsonConvert.DeserializeObject(value, typeof (XDocument), (JsonConverter) new XmlNodeConverter()
      {
        DeserializeRootElementName = deserializeRootElementName,
        WriteArrayAttribute = writeArrayAttribute,
        EncodeSpecialCharacters = encodeSpecialCharacters
      });
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Linq.JObject
// Assembly: Newtonsoft.Json, Version=12.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed
// MVID: 20735148-1523-4917-A5E8-F91B0B239405
// Assembly location: C:\Users\WangQiang\.nuget\packages\newtonsoft.json\12.0.3\lib\netstandard2.0\Newtonsoft.Json.dll

using Newtonsoft.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Linq
{
  /// <summary>Represents a JSON object.</summary>
  /// <example>
  ///   <code lang="cs" source="..\Src\Newtonsoft.Json.Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonCreateParse" title="Parsing a JSON Object from Text" />
  /// </example>
  [NullableContext(1)]
  [Nullable(0)]
  public class JObject : JContainer, IDictionary<string, JToken>, ICollection<KeyValuePair<string, JToken>>, IEnumerable<KeyValuePair<string, JToken>>, IEnumerable, INotifyPropertyChanged, ICustomTypeDescriptor, INotifyPropertyChanging
  {
    private readonly JPropertyKeyedCollection _properties = new JPropertyKeyedCollection();

    /// <summary>
    /// Writes this token to a <see cref="T:Newtonsoft.Json.JsonWriter" /> asynchronously.
    /// </summary>
    /// <param name="writer">A <see cref="T:Newtonsoft.Json.JsonWriter" /> into which this method will write.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <param name="converters">A collection of <see cref="T:Newtonsoft.Json.JsonConverter" /> which will be used when writing the token.</param>
    /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous write operation.</returns>
    public override Task WriteToAsync(
      JsonWriter writer,
      CancellationToken cancellationToken,
      params JsonConverter[] converters)
    {
      Task task = writer.WriteStartObjectAsync(cancellationToken);
      if (!task.IsCompletedSucessfully())
        return AwaitProperties(task, 0, writer, cancellationToken, converters);
      for (int index = 0; index < this._properties.Count; ++index)
      {
        Task async = this._properties[index].WriteToAsync(writer, cancellationToken, converters);
        if (!async.IsCompletedSucessfully())
          return AwaitProperties(async, index + 1, writer, cancellationToken, converters);
      }
      return writer.WriteEndObjectAsync(cancellationToken);

      async Task AwaitProperties(
        Task task,
        int i,
        JsonWriter Writer,
        CancellationToken CancellationToken,
        JsonConverter[] Converters)
      {
        ConfiguredTaskAwaitable configuredTaskAwaitable = task.ConfigureAwait(false);
        await configuredTaskAwaitable;
        for (; i < this._properties.Count; ++i)
        {
          configuredTaskAwaitable = this._properties[i].WriteToAsync(Writer, CancellationToken, Converters).ConfigureAwait(false);
          await configuredTaskAwaitable;
        }
        configuredTaskAwaitable = Writer.WriteEndObjectAsync(CancellationToken).ConfigureAwait(false);
        await configuredTaskAwaitable;
      }
    }

    /// <summary>
    /// Asynchronously loads a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
    /// </summary>
    /// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JObject" />.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
    /// <returns>
    /// A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous load. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
    /// property returns a <see cref="T:Newtonsoft.Json.Linq.JObject" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
    public static Task<JObject> LoadAsync(
      JsonReader reader,
      CancellationToken cancellationToken = default (CancellationToken))
    {
      return JObject.LoadAsync(reader, (JsonLoadSettings) null, cancellationToken);
    }

    /// <summary>
    /// Asynchronously loads a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
    /// </summary>
    /// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JObject" />.</param>
    /// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonLoadSettings" /> used to load the JSON.
    /// If this is <c>null</c>, default load settings will be used.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
    /// <returns>
    /// A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous load. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
    /// property returns a <see cref="T:Newtonsoft.Json.Linq.JObject" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
    public static async Task<JObject> LoadAsync(
      JsonReader reader,
      [Nullable(2)] JsonLoadSettings settings,
      CancellationToken cancellationToken = default (CancellationToken))
    {
      ValidationUtils.ArgumentNotNull((object) reader, nameof (reader));
      ConfiguredTaskAwaitable<bool> configuredTaskAwaitable;
      if (reader.TokenType == JsonToken.None)
      {
        configuredTaskAwaitable = reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        if (!await configuredTaskAwaitable)
          throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader.");
      }
      configuredTaskAwaitable = reader.MoveToContentAsync(cancellationToken).ConfigureAwait(false);
      int num = await configuredTaskAwaitable ? 1 : 0;
      if (reader.TokenType != JsonToken.StartObject)
        throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader. Current JsonReader item is not an object: {0}".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) reader.TokenType));
      JObject o = new JObject();
      o.SetLineInfo(reader as IJsonLineInfo, settings);
      await o.ReadTokenFromAsync(reader, settings, cancellationToken).ConfigureAwait(false);
      return o;
    }

    /// <summary>Gets the container's children tokens.</summary>
    /// <value>The container's children tokens.</value>
    protected override IList<JToken> ChildrenTokens
    {
      get
      {
        return (IList<JToken>) this._properties;
      }
    }

    /// <summary>Occurs when a property value changes.</summary>
    [Nullable(2)]
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>Occurs when a property value is changing.</summary>
    [Nullable(2)]
    public event PropertyChangingEventHandler PropertyChanging;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JObject" /> class.
    /// </summary>
    public JObject()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JObject" /> class from another <see cref="T:Newtonsoft.Json.Linq.JObject" /> object.
    /// </summary>
    /// <param name="other">A <see cref="T:Newtonsoft.Json.Linq.JObject" /> object to copy from.</param>
    public JObject(JObject other)
      : base((JContainer) other)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JObject" /> class with the specified content.
    /// </summary>
    /// <param name="content">The contents of the object.</param>
    public JObject(params object[] content)
      : this((object) content)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JObject" /> class with the specified content.
    /// </summary>
    /// <param name="content">The contents of the object.</param>
    public JObject(object content)
    {
      this.Add(content);
    }

    internal override bool DeepEquals(JToken node)
    {
      return node is JObject jobject && this._properties.Compare(jobject._properties);
    }

    [NullableContext(2)]
    internal override int IndexOfItem(JToken item)
    {
      return item == null ? -1 : this._properties.IndexOfReference(item);
    }

    [NullableContext(2)]
    internal override void InsertItem(int index, JToken item, bool skipParentCheck)
    {
      if (item != null && item.Type == JTokenType.Comment)
        return;
      base.InsertItem(index, item, skipParentCheck);
    }

    internal override void ValidateToken(JToken o, [Nullable(2)] JToken existing)
    {
      ValidationUtils.ArgumentNotNull((object) o, nameof (o));
      if (o.Type != JTokenType.Property)
        throw new ArgumentException("Can not add {0} to {1}.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) o.GetType(), (object) this.GetType()));
      JProperty jproperty1 = (JProperty) o;
      if (existing != null)
      {
        JProperty jproperty2 = (JProperty) existing;
        if (jproperty1.Name == jproperty2.Name)
          return;
      }
      if (this._properties.TryGetValue(jproperty1.Name, out existing))
        throw new ArgumentException("Can not add property {0} to {1}. Property with the same name already exists on object.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) jproperty1.Name, (object) this.GetType()));
    }

    internal override void MergeItem(object content, [Nullable(2)] JsonMergeSettings settings)
    {
      if (!(content is JObject jobject))
        return;
      foreach (KeyValuePair<string, JToken> keyValuePair in jobject)
      {
        JProperty jproperty = this.Property(keyValuePair.Key, settings != null ? settings.PropertyNameComparison : StringComparison.Ordinal);
        if (jproperty == null)
          this.Add(keyValuePair.Key, keyValuePair.Value);
        else if (keyValuePair.Value != null)
        {
          if (!(jproperty.Value is JContainer jcontainer) || jcontainer.Type != keyValuePair.Value.Type)
          {
            if (!JObject.IsNull(keyValuePair.Value) || settings != null && settings.MergeNullValueHandling == MergeNullValueHandling.Merge)
              jproperty.Value = keyValuePair.Value;
          }
          else
            jcontainer.Merge((object) keyValuePair.Value, settings);
        }
      }
    }

    private static bool IsNull(JToken token)
    {
      return token.Type == JTokenType.Null || token is JValue jvalue && jvalue.Value == null;
    }

    internal void InternalPropertyChanged(JProperty childProperty)
    {
      this.OnPropertyChanged(childProperty.Name);
      if (this._listChanged != null)
        this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, this.IndexOfItem((JToken) childProperty)));
      if (this._collectionChanged == null)
        return;
      this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (IList) childProperty, (IList) childProperty, this.IndexOfItem((JToken) childProperty)));
    }

    internal void InternalPropertyChanging(JProperty childProperty)
    {
      this.OnPropertyChanging(childProperty.Name);
    }

    internal override JToken CloneToken()
    {
      return (JToken) new JObject(this);
    }

    /// <summary>
    /// Gets the node type for this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
    /// </summary>
    /// <value>The type.</value>
    public override JTokenType Type
    {
      get
      {
        return JTokenType.Object;
      }
    }

    /// <summary>
    /// Gets an <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JProperty" /> of this object's properties.
    /// </summary>
    /// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JProperty" /> of this object's properties.</returns>
    public IEnumerable<JProperty> Properties()
    {
      return this._properties.Cast<JProperty>();
    }

    /// <summary>
    /// Gets a <see cref="T:Newtonsoft.Json.Linq.JProperty" /> with the specified name.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>A <see cref="T:Newtonsoft.Json.Linq.JProperty" /> with the specified name or <c>null</c>.</returns>
    [return: Nullable(2)]
    public JProperty Property(string name)
    {
      return this.Property(name, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the <see cref="T:Newtonsoft.Json.Linq.JProperty" /> with the specified name.
    /// The exact name will be searched for first and if no matching property is found then
    /// the <see cref="T:System.StringComparison" /> will be used to match a property.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
    /// <returns>A <see cref="T:Newtonsoft.Json.Linq.JProperty" /> matched with the specified name or <c>null</c>.</returns>
    [return: Nullable(2)]
    public JProperty Property(string name, StringComparison comparison)
    {
      if (name == null)
        return (JProperty) null;
      JToken jtoken;
      if (this._properties.TryGetValue(name, out jtoken))
        return (JProperty) jtoken;
      if (comparison != StringComparison.Ordinal)
      {
        for (int index = 0; index < this._properties.Count; ++index)
        {
          JProperty property = (JProperty) this._properties[index];
          if (string.Equals(property.Name, name, comparison))
            return property;
        }
      }
      return (JProperty) null;
    }

    /// <summary>
    /// Gets a <see cref="T:Newtonsoft.Json.Linq.JEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JToken" /> of this object's property values.
    /// </summary>
    /// <returns>A <see cref="T:Newtonsoft.Json.Linq.JEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JToken" /> of this object's property values.</returns>
    [return: Nullable(new byte[] {0, 1})]
    public JEnumerable<JToken> PropertyValues()
    {
      return new JEnumerable<JToken>(this.Properties().Select<JProperty, JToken>((Func<JProperty, JToken>) (p => p.Value)));
    }

    /// <summary>
    /// Gets the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified key.
    /// </summary>
    /// <value>The <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified key.</value>
    [Nullable(2)]
    public override JToken this[object key]
    {
      [return: Nullable(2)] get
      {
        ValidationUtils.ArgumentNotNull(key, nameof (key));
        if (!(key is string index))
          throw new ArgumentException("Accessed JObject values with invalid key value: {0}. Object property name expected.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) MiscellaneousUtils.ToString(key)));
        return this[index];
      }
      [param: Nullable(2)] set
      {
        ValidationUtils.ArgumentNotNull(key, nameof (key));
        if (!(key is string index))
          throw new ArgumentException("Set JObject values with invalid key value: {0}. Object property name expected.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) MiscellaneousUtils.ToString(key)));
        this[index] = value;
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified property name.
    /// </summary>
    /// <value></value>
    [Nullable(2)]
    public JToken this[string propertyName]
    {
      [return: Nullable(2)] get
      {
        ValidationUtils.ArgumentNotNull((object) propertyName, nameof (propertyName));
        return this.Property(propertyName, StringComparison.Ordinal)?.Value;
      }
      [param: Nullable(2)] set
      {
        JProperty jproperty = this.Property(propertyName, StringComparison.Ordinal);
        if (jproperty != null)
        {
          jproperty.Value = value;
        }
        else
        {
          this.OnPropertyChanging(propertyName);
          this.Add(propertyName, value);
          this.OnPropertyChanged(propertyName);
        }
      }
    }

    /// <summary>
    /// Loads a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
    /// </summary>
    /// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JObject" />.</param>
    /// <returns>A <see cref="T:Newtonsoft.Json.Linq.JObject" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
    /// <exception cref="T:Newtonsoft.Json.JsonReaderException">
    ///     <paramref name="reader" /> is not valid JSON.
    /// </exception>
    public static JObject Load(JsonReader reader)
    {
      return JObject.Load(reader, (JsonLoadSettings) null);
    }

    /// <summary>
    /// Loads a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
    /// </summary>
    /// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JObject" />.</param>
    /// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonLoadSettings" /> used to load the JSON.
    /// If this is <c>null</c>, default load settings will be used.</param>
    /// <returns>A <see cref="T:Newtonsoft.Json.Linq.JObject" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
    /// <exception cref="T:Newtonsoft.Json.JsonReaderException">
    ///     <paramref name="reader" /> is not valid JSON.
    /// </exception>
    public static JObject Load(JsonReader reader, [Nullable(2)] JsonLoadSettings settings)
    {
      ValidationUtils.ArgumentNotNull((object) reader, nameof (reader));
      if (reader.TokenType == JsonToken.None && !reader.Read())
        throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader.");
      reader.MoveToContent();
      if (reader.TokenType != JsonToken.StartObject)
        throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader. Current JsonReader item is not an object: {0}".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) reader.TokenType));
      JObject jobject = new JObject();
      jobject.SetLineInfo(reader as IJsonLineInfo, settings);
      jobject.ReadTokenFrom(reader, settings);
      return jobject;
    }

    /// <summary>
    /// Load a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from a string that contains JSON.
    /// </summary>
    /// <param name="json">A <see cref="T:System.String" /> that contains JSON.</param>
    /// <returns>A <see cref="T:Newtonsoft.Json.Linq.JObject" /> populated from the string that contains JSON.</returns>
    /// <exception cref="T:Newtonsoft.Json.JsonReaderException">
    ///     <paramref name="json" /> is not valid JSON.
    /// </exception>
    /// <example>
    ///   <code lang="cs" source="..\Src\Newtonsoft.Json.Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonCreateParse" title="Parsing a JSON Object from Text" />
    /// </example>
    public static JObject Parse(string json)
    {
      return JObject.Parse(json, (JsonLoadSettings) null);
    }

    /// <summary>
    /// Load a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from a string that contains JSON.
    /// </summary>
    /// <param name="json">A <see cref="T:System.String" /> that contains JSON.</param>
    /// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonLoadSettings" /> used to load the JSON.
    /// If this is <c>null</c>, default load settings will be used.</param>
    /// <returns>A <see cref="T:Newtonsoft.Json.Linq.JObject" /> populated from the string that contains JSON.</returns>
    /// <exception cref="T:Newtonsoft.Json.JsonReaderException">
    ///     <paramref name="json" /> is not valid JSON.
    /// </exception>
    /// <example>
    ///   <code lang="cs" source="..\Src\Newtonsoft.Json.Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonCreateParse" title="Parsing a JSON Object from Text" />
    /// </example>
    public static JObject Parse(string json, [Nullable(2)] JsonLoadSettings settings)
    {
      using (JsonReader reader = (JsonReader) new JsonTextReader((TextReader) new StringReader(json)))
      {
        JObject jobject = JObject.Load(reader, settings);
        do
          ;
        while (reader.Read());
        return jobject;
      }
    }

    /// <summary>
    /// Creates a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from an object.
    /// </summary>
    /// <param name="o">The object that will be used to create <see cref="T:Newtonsoft.Json.Linq.JObject" />.</param>
    /// <returns>A <see cref="T:Newtonsoft.Json.Linq.JObject" /> with the values of the specified object.</returns>
    public static JObject FromObject(object o)
    {
      return JObject.FromObject(o, JsonSerializer.CreateDefault());
    }

    /// <summary>
    /// Creates a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from an object.
    /// </summary>
    /// <param name="o">The object that will be used to create <see cref="T:Newtonsoft.Json.Linq.JObject" />.</param>
    /// <param name="jsonSerializer">The <see cref="T:Newtonsoft.Json.JsonSerializer" /> that will be used to read the object.</param>
    /// <returns>A <see cref="T:Newtonsoft.Json.Linq.JObject" /> with the values of the specified object.</returns>
    public static JObject FromObject(object o, JsonSerializer jsonSerializer)
    {
      JToken jtoken = JToken.FromObjectInternal(o, jsonSerializer);
      if (jtoken.Type != JTokenType.Object)
        throw new ArgumentException("Object serialized to {0}. JObject instance expected.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) jtoken.Type));
      return (JObject) jtoken;
    }

    /// <summary>
    /// Writes this token to a <see cref="T:Newtonsoft.Json.JsonWriter" />.
    /// </summary>
    /// <param name="writer">A <see cref="T:Newtonsoft.Json.JsonWriter" /> into which this method will write.</param>
    /// <param name="converters">A collection of <see cref="T:Newtonsoft.Json.JsonConverter" /> which will be used when writing the token.</param>
    public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
    {
      writer.WriteStartObject();
      for (int index = 0; index < this._properties.Count; ++index)
        this._properties[index].WriteTo(writer, converters);
      writer.WriteEndObject();
    }

    /// <summary>
    /// Gets the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified property name.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>The <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified property name.</returns>
    [NullableContext(2)]
    public JToken GetValue(string propertyName)
    {
      return this.GetValue(propertyName, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified property name.
    /// The exact property name will be searched for first and if no matching property is found then
    /// the <see cref="T:System.StringComparison" /> will be used to match a property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
    /// <returns>The <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified property name.</returns>
    [NullableContext(2)]
    public JToken GetValue(string propertyName, StringComparison comparison)
    {
      if (propertyName == null)
        return (JToken) null;
      return this.Property(propertyName, comparison)?.Value;
    }

    /// <summary>
    /// Tries to get the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified property name.
    /// The exact property name will be searched for first and if no matching property is found then
    /// the <see cref="T:System.StringComparison" /> will be used to match a property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">The value.</param>
    /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
    /// <returns><c>true</c> if a value was successfully retrieved; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(string propertyName, StringComparison comparison, [Nullable(2), NotNullWhen(true)] out JToken value)
    {
      value = this.GetValue(propertyName, comparison);
      return value != null;
    }

    /// <summary>Adds the specified property name.</summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">The value.</param>
    public void Add(string propertyName, [Nullable(2)] JToken value)
    {
      this.Add((object) new JProperty(propertyName, (object) value));
    }

    /// <summary>
    /// Determines whether the JSON object has the specified property name.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns><c>true</c> if the JSON object has the specified property name; otherwise, <c>false</c>.</returns>
    public bool ContainsKey(string propertyName)
    {
      ValidationUtils.ArgumentNotNull((object) propertyName, nameof (propertyName));
      return this._properties.Contains(propertyName);
    }

    ICollection<string> IDictionary<string, JToken>.Keys
    {
      get
      {
        return this._properties.Keys;
      }
    }

    /// <summary>Removes the property with the specified name.</summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns><c>true</c> if item was successfully removed; otherwise, <c>false</c>.</returns>
    public bool Remove(string propertyName)
    {
      JProperty jproperty = this.Property(propertyName, StringComparison.Ordinal);
      if (jproperty == null)
        return false;
      jproperty.Remove();
      return true;
    }

    /// <summary>
    /// Tries to get the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified property name.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> if a value was successfully retrieved; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(string propertyName, [Nullable(2), NotNullWhen(true)] out JToken value)
    {
      JProperty jproperty = this.Property(propertyName, StringComparison.Ordinal);
      if (jproperty == null)
      {
        value = (JToken) null;
        return false;
      }
      value = jproperty.Value;
      return true;
    }

    [Nullable(new byte[] {1, 2})]
    ICollection<JToken> IDictionary<string, JToken>.Values
    {
      [return: Nullable(new byte[] {1, 2})] get
      {
        throw new NotImplementedException();
      }
    }

    void ICollection<KeyValuePair<string, JToken>>.Add(
      [Nullable(new byte[] {0, 1, 2})] KeyValuePair<string, JToken> item)
    {
      this.Add((object) new JProperty(item.Key, (object) item.Value));
    }

    void ICollection<KeyValuePair<string, JToken>>.Clear()
    {
      this.RemoveAll();
    }

    bool ICollection<KeyValuePair<string, JToken>>.Contains(
      [Nullable(new byte[] {0, 1, 2})] KeyValuePair<string, JToken> item)
    {
      JProperty jproperty = this.Property(item.Key, StringComparison.Ordinal);
      return jproperty != null && jproperty.Value == item.Value;
    }

    void ICollection<KeyValuePair<string, JToken>>.CopyTo(
      [Nullable(new byte[] {1, 0, 1, 2})] KeyValuePair<string, JToken>[] array,
      int arrayIndex)
    {
      if (array == null)
        throw new ArgumentNullException(nameof (array));
      if (arrayIndex < 0)
        throw new ArgumentOutOfRangeException(nameof (arrayIndex), "arrayIndex is less than 0.");
      if (arrayIndex >= array.Length && arrayIndex != 0)
        throw new ArgumentException("arrayIndex is equal to or greater than the length of array.");
      if (this.Count > array.Length - arrayIndex)
        throw new ArgumentException("The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");
      int num = 0;
      foreach (JProperty property in (Collection<JToken>) this._properties)
      {
        array[arrayIndex + num] = new KeyValuePair<string, JToken>(property.Name, property.Value);
        ++num;
      }
    }

    bool ICollection<KeyValuePair<string, JToken>>.IsReadOnly
    {
      get
      {
        return false;
      }
    }

    bool ICollection<KeyValuePair<string, JToken>>.Remove(
      [Nullable(new byte[] {0, 1, 2})] KeyValuePair<string, JToken> item)
    {
      if (!((ICollection<KeyValuePair<string, JToken>>) this).Contains(item))
        return false;
      this.Remove(item.Key);
      return true;
    }

    internal override int GetDeepHashCode()
    {
      return this.ContentsHashCode();
    }

    /// <summary>
    /// Returns an enumerator that can be used to iterate through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
    /// </returns>
    [return: Nullable(new byte[] {1, 0, 1, 2})]
    public IEnumerator<KeyValuePair<string, JToken>> GetEnumerator()
    {
      foreach (JProperty property in (Collection<JToken>) this._properties)
        yield return new KeyValuePair<string, JToken>(property.Name, property.Value);
    }

    /// <summary>
    /// Raises the <see cref="E:Newtonsoft.Json.Linq.JObject.PropertyChanged" /> event with the provided arguments.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Raises the <see cref="E:Newtonsoft.Json.Linq.JObject.PropertyChanging" /> event with the provided arguments.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    protected virtual void OnPropertyChanging(string propertyName)
    {
      PropertyChangingEventHandler propertyChanging = this.PropertyChanging;
      if (propertyChanging == null)
        return;
      propertyChanging((object) this, new PropertyChangingEventArgs(propertyName));
    }

    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
    {
      return ((ICustomTypeDescriptor) this).GetProperties((Attribute[]) null);
    }

    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(
      Attribute[] attributes)
    {
      PropertyDescriptor[] properties = new PropertyDescriptor[this.Count];
      int index = 0;
      foreach (KeyValuePair<string, JToken> keyValuePair in this)
      {
        properties[index] = (PropertyDescriptor) new JPropertyDescriptor(keyValuePair.Key);
        ++index;
      }
      return new PropertyDescriptorCollection(properties);
    }

    AttributeCollection ICustomTypeDescriptor.GetAttributes()
    {
      return AttributeCollection.Empty;
    }

    [NullableContext(2)]
    string ICustomTypeDescriptor.GetClassName()
    {
      return (string) null;
    }

    [NullableContext(2)]
    string ICustomTypeDescriptor.GetComponentName()
    {
      return (string) null;
    }

    TypeConverter ICustomTypeDescriptor.GetConverter()
    {
      return new TypeConverter();
    }

    [NullableContext(2)]
    EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
    {
      return (EventDescriptor) null;
    }

    [NullableContext(2)]
    PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
    {
      return (PropertyDescriptor) null;
    }

    [return: Nullable(2)]
    object ICustomTypeDescriptor.GetEditor(System.Type editorBaseType)
    {
      return (object) null;
    }

    EventDescriptorCollection ICustomTypeDescriptor.GetEvents(
      Attribute[] attributes)
    {
      return EventDescriptorCollection.Empty;
    }

    EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
    {
      return EventDescriptorCollection.Empty;
    }

    [return: Nullable(2)]
    object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
    {
      return pd is JPropertyDescriptor ? (object) this : (object) null;
    }

    /// <summary>
    /// Returns the <see cref="T:System.Dynamic.DynamicMetaObject" /> responsible for binding operations performed on this object.
    /// </summary>
    /// <param name="parameter">The expression tree representation of the runtime value.</param>
    /// <returns>
    /// The <see cref="T:System.Dynamic.DynamicMetaObject" /> to bind this object.
    /// </returns>
    protected override DynamicMetaObject GetMetaObject(Expression parameter)
    {
      return (DynamicMetaObject) new DynamicProxyMetaObject<JObject>(parameter, this, (DynamicProxy<JObject>) new JObject.JObjectDynamicProxy());
    }

    [Nullable(new byte[] {0, 1})]
    private class JObjectDynamicProxy : DynamicProxy<JObject>
    {
      public override bool TryGetMember(
        JObject instance,
        GetMemberBinder binder,
        [Nullable(2)] out object result)
      {
        result = (object) instance[binder.Name];
        return true;
      }

      public override bool TrySetMember(JObject instance, SetMemberBinder binder, object value)
      {
        if (!(value is JToken jtoken))
          jtoken = (JToken) new JValue(value);
        instance[binder.Name] = jtoken;
        return true;
      }

      public override IEnumerable<string> GetDynamicMemberNames(JObject instance)
      {
        return instance.Properties().Select<JProperty, string>((Func<JProperty, string>) (p => p.Name));
      }
    }
  }
}

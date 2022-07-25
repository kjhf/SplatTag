using NLog;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  public abstract class SingleValueHandler<T> :
    BaseHandler,
    IMatchable<T>,
    IMergable<T>
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly FilterOptions nameOption;

    /// <summary>
    /// Construct the SingleValueHandler with the context of the value to serialize.
    /// </summary>
    /// <param name="nameOption"></param>
    protected SingleValueHandler(FilterOptions? nameOption = null, T? initialValue = default)
    {
      this.nameOption = nameOption ?? FilterOptions.None;
      this.Value = initialValue;
    }

    /// <inheritdoc/>
    public override bool HasDataToSerialize => Value != null && !Value.Equals(default(T));

    protected internal T? Value { get; set; }

    public FilterOptions MatchWithReason(SingleValueHandler<T> other) => Value?.Equals(other.Value) == true ? nameOption : FilterOptions.None;

    public override FilterOptions MatchWithReason(BaseHandler other) => MatchWithReason((SingleValueHandler<T>)other);

    public FilterOptions MatchWithReason(T? other)
    {
      if (Value?.Equals(other) == true)
      {
        return nameOption;
      }
      return FilterOptions.None;
    }

    public override void Merge(ISelfMergable other)
    {
      if (other is SingleValueHandler<T> handler)
      {
        Merge(handler.Value);
      }
    }

    public virtual void Merge(T? other)
    {
      this.Value = other;
    }

    protected virtual void DeserializeSingleValue(SerializationInfo info, StreamingContext context)
    {
      Value = info.GetValueOrDefault(SerializedHandlerName, default(T));
    }

    protected virtual object SerializeSingleValue()
    {
      Dictionary<string, object> result = new();

      if (HasDataToSerialize)
      {
        result.Add(SerializedHandlerName, Value!);
      }
      return result;
    }

    public override object ToSerializedObject()
    {
      return SerializeSingleValue();
    }

    // public so derived classes can implement ISerializable
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (HasDataToSerialize)
      {
        info.AddValue(SerializedHandlerName, Value!);
      }
    }
  }
}
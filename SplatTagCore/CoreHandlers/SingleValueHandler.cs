using NLog;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  public abstract class SingleValueHandler<T> : BaseHandler<T?>
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

    public abstract string SerializedName { get; }
    protected internal T? Value { get; set; }

    public override FilterOptions MatchWithReason(IMatchable other)
    {
      if (other is SingleValueHandler<T> handler && Value?.Equals(handler.Value) == true)
      {
        return nameOption;
      }
      return FilterOptions.None;
    }

    public override FilterOptions MatchWithReason(T? other)
    {
      if (Value?.Equals(other) == true)
      {
        return nameOption;
      }
      return FilterOptions.None;
    }

    public override void Merge(IMergable other)
    {
      if (other is SingleValueHandler<T> handler)
      {
        Merge(handler.Value);
      }
    }

    public override void Merge(T? other)
    {
      this.Value = other;
    }

    protected virtual void DeserializeSingleValue(SerializationInfo info, StreamingContext context)
    {
      Value = info.GetValueOrDefault(SerializedName, default(T));
    }

    protected virtual void SerializeSingleValue(SerializationInfo info, StreamingContext context)
    {
      if (HasDataToSerialize)
      {
        info.AddValue(SerializedName, Value);
      }
    }
  }
}
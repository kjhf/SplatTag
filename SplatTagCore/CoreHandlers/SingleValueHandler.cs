using NLog;
using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public abstract class SingleValueHandler<T> : BaseHandler<T?>, ISerializable
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly FilterOptions nameOption;

    /// <summary>
    /// Construct the SingleValueHandler with the context of the value to serialize.
    /// </summary>
    /// <param name="nameOption"></param>
    protected SingleValueHandler(FilterOptions? nameOption, T? initialValue = default)
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

    #region Serialization

    protected virtual void DeserialieSingleValue(SerializationInfo info, StreamingContext context)
    {
      Value = info.GetValueOrDefault(SerializedName, default(T));
    }

    protected virtual void SerialieSingleValue(SerializationInfo info, StreamingContext context)
    {
      if (HasDataToSerialize)
      {
        info.AddValue(SerializedName, Value);
      }
    }

    // Deserialize
    protected SingleValueHandler(SerializationInfo info, StreamingContext context)
    {
      DeserialieSingleValue(info, context);
    }

    // Serialize
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      SerialieSingleValue(info, context);
    }

    #endregion Serialization
  }
}
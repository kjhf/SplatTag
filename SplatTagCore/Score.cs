using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public record Score : ICoreObject
  {
    /// <summary>
    /// Empty score.
    /// </summary>
    public static readonly Score Empty = new();

    public Score(IList<int>? points = null)
    {
      this.Points = points ?? Array.Empty<int>();
    }

    [JsonIgnore]
    /// <summary>
    /// Displayable description of the score, e.g. 3-2
    /// </summary>
    public string Description => string.Join("-", Points);

    [JsonIgnore]
    /// <summary>
    /// Sum the points total to find the number of games played.
    /// </summary>
    public int GamesPlayed => Points.Sum();

    [JsonProperty]
    /// <summary>
    /// Score points, indexed by team.
    /// </summary>
    public IList<int> Points { get; }

    public override string ToString() => $"Score: {Description}";

    public bool Equals(ICoreObject other) => Equals(other as Score);

    public string GetDisplayValue() => Description;

    #region Serialization

    // Deserialize
    protected Score(SerializationInfo info, StreamingContext context)
      : base()
    {
      Points = info.GetValueOrDefault(nameof(Points), Array.Empty<int>());
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (Points.Count > 0)
      {
        info.AddValue(nameof(Points), Points);
      }
    }

    #endregion Serialization
  }
}
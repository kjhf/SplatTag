using System;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public record TeamId : ICoreObject, IEquatable<TeamId>, IEquatable<Guid>, ISerializable
  {
    public TeamId(Guid id)
    {
      Id = id;
    }

    public Guid Id { get; }

    public bool Equals(Guid other) => Id.Equals(other);

    public Team GetTeam(ITeamResolver resolver) => resolver.GetTeamById(Id);

    public static implicit operator Guid(TeamId? t) => t?.Id ?? Guid.Empty;
    public static explicit operator TeamId(Guid t) => new(t);

    public override string ToString() => $"Team: {Id}";

    #region Serialization

    // Deserialize
    protected TeamId(SerializationInfo info, StreamingContext context)
    {
      Id = (Guid)info.GetValue("Id", typeof(Guid));
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Id", Id);
    }

    public string GetDisplayValue() => Id.ToString();

    public bool Equals(ICoreObject other) => Equals(other as TeamId);

    #endregion Serialization
  }
}
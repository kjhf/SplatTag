using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public record WeaponsContainer : ICoreObject, ISerializable
  {
    public const string SerializationName = "Weps";
    private string[]? weapons;
    public IList<string> Weapons => weapons ??= Array.Empty<string>();

    public WeaponsContainer(IEnumerable<string>? weapons = null)
    {
      this.weapons = weapons?.ToArray();
    }

    public static explicit operator WeaponsContainer(List<string> t) => new(t);

    public override string ToString() => $"Weapons: {weapons?.Length ?? 0}";

    #region Serialization

    // Deserialize
    protected WeaponsContainer(SerializationInfo info, StreamingContext context)
    {
      weapons = (string[])info.GetValue(SerializationName, typeof(string[]));
    }

    // Serialize
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (weapons?.Length > 0)
      {
        info.AddValue(SerializationName, weapons);
      }
    }

    public string GetDisplayValue() => ToString();

    public bool Equals(ICoreObject other) => Equals(other as WeaponsContainer);

    #endregion Serialization
  }
}
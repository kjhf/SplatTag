using NLog;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace SplatTagCore
{
  [Serializable]
  public class DiscordHandler : BaseHandlerCollectionSourced<DiscordHandler>, ISerializable
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private const string DiscordIdsSerialization = "Ids";
    private const string DiscordUsernameSerialization = "Usernames";

    /// <summary>Parameterless constructor</summary>
    /// <remarks>Required for serialization - do not delete.</remarks>
    public DiscordHandler()
      : base()
    {
    }

    protected override void InitialiseHandlers()
    {
      handlers.Clear();
      handlers.Add(DiscordIdsSerialization, new NamesHandler<Name>(FilterOptions.DiscordId, DiscordIdsSerialization));
      handlers.Add(DiscordUsernameSerialization, new NamesHandler<Name>(FilterOptions.DiscordName, DiscordUsernameSerialization));
    }

    public static readonly Regex DISCORD_NAME_REGEX = new(@"\(?.*#[0-9]{4}\)?", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

    /// <summary>
    /// The Discord ids
    /// </summary>
    public IReadOnlyCollection<Name> Ids => IdsHandler.GetItemsUnordered();

    /// <summary>
    /// The Discord ids
    /// </summary>
    public NamesHandler<Name> IdsHandler => (NamesHandler<Name>)this[DiscordIdsSerialization];

    /// <summary>
    /// The Discord usernames
    /// </summary>
    public IReadOnlyCollection<Name> Usernames => UsernamesHandler.GetItemsUnordered();

    /// <summary>
    /// The Discord usernames
    /// </summary>
    public NamesHandler<Name> UsernamesHandler => (NamesHandler<Name>)this[DiscordUsernameSerialization];

    /// <summary>
    /// Add a new Discord id to the front of this profile
    /// </summary>
    public void AddId(string slug, Source source)
      => IdsHandler.Add(new Name(slug, source));

    /// <summary>
    /// Add Discord ids to this Discord profile
    /// </summary>
    /// <param name="ids"></param>
    public void AddIds(IEnumerable<Name> ids)
      => IdsHandler.Add(ids);

    /// <summary>
    /// Add a new Discord name to the front of this profile
    /// </summary>
    /// <param name="ids"></param>
    public void AddUsername(string username, Source source)
      => UsernamesHandler.Add(new Name(username, source));

    /// <summary>
    /// Add Discord usernames to this Discord profile
    /// </summary>
    public void AddUsernames(IEnumerable<Name> incoming)
      => UsernamesHandler.Add(incoming);

    /// <summary>
    /// Return if this Discord matches another by persistent data (ids).
    /// </summary>
    public bool MatchPersistent(DiscordHandler other)
      => IdsHandler.Match(other.IdsHandler);

    /// <summary>
    /// Return if this Discord matches by username
    /// </summary>
    public bool MatchUsernames(DiscordHandler other)
      => UsernamesHandler.Match(other.UsernamesHandler);

    #region Serialization

    // Deserialize
    protected DiscordHandler(SerializationInfo info, StreamingContext context)
      : base()
    {
      DeserializeHandlers(info, context);
    }

    // Serialize
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      SerializeHandlers(info, context);
    }

    #endregion Serialization
  }
}
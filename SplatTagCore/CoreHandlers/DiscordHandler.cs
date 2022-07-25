using NLog;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace SplatTagCore
{
  [Serializable]
  public class DiscordHandler :
    BaseHandlerCollectionSourced,
    ISerializable
  {
    public const string SerializationName = "Dis";
    public static readonly Regex DISCORD_NAME_REGEX = new(@"\(?.*#[0-9]{4}\)?", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public override string SerializedHandlerName => SerializationName;

    /// <summary>Parameterless constructor</summary>
    /// <remarks>Required for serialization - do not delete.</remarks>
    public DiscordHandler()
    {
    }

    /// <summary>
    /// The Discord ids
    /// </summary>
    public IReadOnlyCollection<Name> Ids => IdsHandlerNoCreate?.GetItemsUnordered() ?? Array.Empty<Name>();

    /// <summary>
    /// The Discord ids in recent source order
    /// </summary>
    public IReadOnlyList<Name> IdsOrdered => IdsHandlerNoCreate?.GetItemsOrdered() ?? Array.Empty<Name>();

    /// <summary>
    /// The Discord usernames
    /// </summary>
    public IReadOnlyCollection<Name> Usernames => UsernamesHandlerNoCreate?.GetItemsUnordered() ?? Array.Empty<Name>();

    /// <summary>
    /// The Discord usernames in recent source order
    /// </summary>
    public IReadOnlyList<Name> UsernamesOrdered => UsernamesHandlerNoCreate?.GetItemsOrdered() ?? Array.Empty<Name>();

    protected override IReadOnlyDictionary<string, (Type, Func<BaseHandler>)> SupportedHandlers => new Dictionary<string, (Type, Func<BaseHandler>)>
    {
      { DiscordIdsHandler.SerializationName, (typeof(DiscordIdsHandler), () => new DiscordIdsHandler()) },
      { DiscordUsernamesHandler.SerializationName, (typeof(DiscordUsernamesHandler), () => new DiscordUsernamesHandler()) }
    };

    /// <summary>
    /// The handler for Discord ids
    /// </summary>
    private DiscordIdsHandler IdsHandlerWithCreate => GetHandler<DiscordIdsHandler>(DiscordIdsHandler.SerializationName);

    /// <summary>
    /// The handler for Discord usernames
    /// </summary>
    private DiscordUsernamesHandler UsernamesHandlerWithCreate => GetHandler<DiscordUsernamesHandler>(DiscordUsernamesHandler.SerializationName);

    /// <summary>
    /// The handler for Discord ids
    /// </summary>
    private DiscordIdsHandler? IdsHandlerNoCreate => GetHandlerNoCreate<DiscordIdsHandler>(DiscordIdsHandler.SerializationName);

    /// <summary>
    /// The handler for Discord usernames
    /// </summary>
    private DiscordUsernamesHandler? UsernamesHandlerNoCreate => GetHandlerNoCreate<DiscordUsernamesHandler>(DiscordUsernamesHandler.SerializationName);

    /// <summary>
    /// Add a new Discord id to the front of this profile
    /// </summary>
    public void AddId(string slug, Source source)
      => IdsHandlerWithCreate.Add(new Name(slug, source));

    /// <summary>
    /// Add Discord ids to this Discord profile
    /// </summary>
    /// <param name="ids"></param>
    public void AddIds(IEnumerable<Name> ids)
      => IdsHandlerWithCreate.Add(ids);

    /// <summary>
    /// Add a new Discord name to the front of this profile
    /// </summary>
    /// <param name="ids"></param>
    public void AddUsername(string username, Source source)
      => UsernamesHandlerWithCreate.Add(new Name(username, source));

    /// <summary>
    /// Add Discord usernames to this Discord profile
    /// </summary>
    public void AddUsernames(IEnumerable<Name> incoming)
      => UsernamesHandlerWithCreate.Add(incoming);

    /// <summary>
    /// Return if this Discord matches another by persistent data (ids).
    /// </summary>
    public bool MatchPersistent(DiscordHandler other)
      => MatchByHandlerName(DiscordIdsHandler.SerializationName, other);

    /// <summary>
    /// Return if this Discord matches by username
    /// </summary>
    public bool MatchUsernames(DiscordHandler other)
      => MatchByHandlerName(DiscordUsernamesHandler.SerializationName, other);

    #region Serialization

    // Deserialize
    protected DiscordHandler(SerializationInfo info, StreamingContext context)
    {
      base.DeserializeHandlers(info, context);
    }

    /// <summary>Serialize</summary>
    /// <remarks>Handled in <see cref="BaseHandlerCollectionSourced.GetObjectData(SerializationInfo, StreamingContext)"/>.</remarks>

    #endregion Serialization
  }
}
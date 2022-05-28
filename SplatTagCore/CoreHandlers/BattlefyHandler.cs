using NLog;
using SplatTagCore.Social;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SplatTagCore
{
  [Serializable]
  public class BattlefyHandler : BaseHandlerCollectionSourced<BattlefyHandler>, ISerializable
  {
    public const string SerializationName = "Bfy";
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public BattlefyHandler()
    {
    }

    /// <summary>
    /// Combination of Battlefy usernames, slugs and ids
    /// </summary>
    public IReadOnlyCollection<Name> AllNames => new List<Name>(Usernames.Concat(Slugs).Concat(PersistentIds).Distinct());

    /// <summary>
    /// The persistent Battlefy ids
    /// </summary>
    public IReadOnlyCollection<Name> PersistentIds => PersistentIdsHandlerNoCreate?.GetItemsUnordered() ?? Array.Empty<Name>();

    /// <summary>
    /// The persistent Battlefy ids in recent source order
    /// </summary>
    public IReadOnlyList<Name> PersistentIdsOrdered => PersistentIdsHandlerNoCreate?.GetItemsOrdered() ?? Array.Empty<Name>();

    /// <summary>
    /// The persistent Battlefy slugs
    /// </summary>
    public IReadOnlyCollection<BattlefyUserSocial> Slugs => SlugsHandlerNoCreate?.GetItemsUnordered() ?? Array.Empty<BattlefyUserSocial>();

    /// <summary>
    /// The persistent Battlefy slugs in recent source order
    /// </summary>
    public IReadOnlyList<BattlefyUserSocial> SlugsOrdered => SlugsHandlerNoCreate?.GetItemsOrdered() ?? Array.Empty<BattlefyUserSocial>();

    /// <summary>
    /// The Battlefy usernames
    /// </summary>
    public IReadOnlyCollection<Name> Usernames => UsernamesHandlerNoCreate?.GetItemsUnordered() ?? Array.Empty<Name>();

    /// <summary>
    /// The Battlefy usernames in recent source order
    /// </summary>
    public IReadOnlyList<Name> UsernamesOrdered => UsernamesHandlerNoCreate?.GetItemsOrdered() ?? Array.Empty<Name>();

    protected override IReadOnlyDictionary<string, (Type, Func<BaseHandler>)> SupportedHandlers => new Dictionary<string, (Type, Func<BaseHandler>)>
    {
      { BattlefyUsernamesHandler.SerializationName, (typeof(BattlefyUsernamesHandler), () => new BattlefyUsernamesHandler()) },
      { BattlefySlugsHandler.SerializationName, (typeof(BattlefySlugsHandler), () => new BattlefySlugsHandler()) },
      { BattlefyIdsHandler.SerializationName, (typeof(BattlefyIdsHandler), () => new BattlefyIdsHandler()) }
    };

    /// <summary>
    /// The handler for Battlefy persistent ids
    /// </summary>
    private BattlefyIdsHandler PersistentIdsHandlerWithCreate => GetHandler<BattlefyIdsHandler>(BattlefyIdsHandler.SerializationName);

    /// <summary>
    /// The handler for Battlefy persistent ids
    /// </summary>
    private BattlefyIdsHandler? PersistentIdsHandlerNoCreate => GetHandlerNoCreate<BattlefyIdsHandler>(BattlefyIdsHandler.SerializationName);

    /// <summary>
    /// The handler for persistent Battlefy slugs
    /// </summary>
    private BattlefySlugsHandler SlugsHandlerWithCreate => GetHandler<BattlefySlugsHandler>(BattlefySlugsHandler.SerializationName);

    /// <summary>
    /// The handler for persistent Battlefy slugs
    /// </summary>
    private BattlefySlugsHandler? SlugsHandlerNoCreate => GetHandlerNoCreate<BattlefySlugsHandler>(BattlefySlugsHandler.SerializationName);

    /// <summary>
    /// The handler for Battlefy usernames
    /// </summary>
    private BattlefyUsernamesHandler UsernamesHandlerWithCreate => GetHandler<BattlefyUsernamesHandler>(BattlefyUsernamesHandler.SerializationName);

    /// <summary>
    /// The handler for Battlefy usernames
    /// </summary>
    private BattlefyUsernamesHandler? UsernamesHandlerNoCreate => GetHandlerNoCreate<BattlefyUsernamesHandler>(BattlefyUsernamesHandler.SerializationName);

    /// <summary>
    /// Add a new Battlefy persistent id to the Battlefy profile
    /// </summary>
    public void AddPersistentId(string persistentId, Source source)
      => PersistentIdsHandlerWithCreate.Add(new Name(persistentId, source));

    /// <summary>
    /// Add new Battlefy persistent ids to the Battlefy profile
    /// </summary>
    public void AddPersistentIds(IEnumerable<Name> incoming)
      => PersistentIdsHandlerWithCreate.Add(incoming);

    /// <summary>
    /// Add a new Battlefy slug to the Battlefy profile
    /// </summary>
    public void AddSlug(string slug, Source source)
      => SlugsHandlerWithCreate.Add(new BattlefyUserSocial(slug, source));

    /// <summary>
    /// Add new Battlefy slugs to the Battlefy profile
    /// </summary>
    public void AddSlugs(IEnumerable<BattlefyUserSocial> incoming)
      => SlugsHandlerWithCreate.Add(incoming);

    /// <summary>
    /// Add a new Battlefy username to the Battlefy profile
    /// </summary>
    public void AddUsername(string username, Source source)
      => UsernamesHandlerWithCreate.Add(new Name(username, source));

    /// <summary>
    /// Add new Battlefy usernames to the Battlefy profile
    /// </summary>
    public void AddUsernames(IEnumerable<Name> incoming)
      => UsernamesHandlerWithCreate.Add(incoming);

    /// <summary>
    /// Return if this Battlefy matches another by persistent data (ids).
    /// </summary>
    public bool MatchPersistent(BattlefyHandler? other)
      => MatchByHandlerName(BattlefyIdsHandler.SerializationName, other);

    /// <summary>
    /// Return if this Battlefy matches another by slugs.
    /// </summary>
    public bool MatchSlugs(BattlefyHandler? other)
      => MatchByHandlerName(BattlefySlugsHandler.SerializationName, other);

    /// <summary>
    /// Return if this Battlefy matches another by usernames.
    /// </summary>
    public bool MatchUsernames(BattlefyHandler? other)
      => MatchByHandlerName(BattlefyUsernamesHandler.SerializationName, other);

    #region Serialization

    // Deserialize
    protected BattlefyHandler(SerializationInfo info, StreamingContext context)
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
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
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private const string BattlefyUsernameSerialization = "Usernames";
    private const string BattlefySlugsSerialization = "Slugs";
    private const string BattlefyPersistentIdsSerialization = "PersistentIds";

    public BattlefyHandler()
      : base()
    {
    }

    protected override void InitialiseHandlers()
    {
      handlers.Clear();
      handlers.Add(BattlefyUsernameSerialization, new NamesHandler<Name>(FilterOptions.BattlefyUsername, BattlefyUsernameSerialization));
      handlers.Add(BattlefySlugsSerialization, new NamesHandler<BattlefyUserSocial>(FilterOptions.BattlefySlugs, BattlefySlugsSerialization));
      handlers.Add(BattlefyPersistentIdsSerialization, new NamesHandler<Name>(FilterOptions.BattlefyPersistentIds, BattlefyPersistentIdsSerialization));
    }

    /// <summary>
    /// The persistent Battlefy slugs
    /// </summary>
    public IReadOnlyCollection<BattlefyUserSocial> Slugs => SlugsHandler.GetItemsUnordered();

    /// <summary>
    /// The persistent Battlefy slugs
    /// </summary>
    public NamesHandler<BattlefyUserSocial> SlugsHandler => (NamesHandler<BattlefyUserSocial>)this[BattlefySlugsSerialization];

    /// <summary>
    /// The Battlefy usernames
    /// </summary>
    public IReadOnlyCollection<Name> Usernames => UsernamesHandler.GetItemsUnordered();

    /// <summary>
    /// The Battlefy usernames
    /// </summary>
    public NamesHandler<Name> UsernamesHandler => (NamesHandler<Name>)this[BattlefyUsernameSerialization];

    /// <summary>
    /// The persistent Battlefy ids
    /// </summary>
    public IReadOnlyCollection<Name> PersistentIds => PersistentIdsHandler.GetItemsUnordered();

    /// <summary>
    /// The Battlefy persistent ids
    /// </summary>
    public NamesHandler<Name> PersistentIdsHandler => (NamesHandler<Name>)this[BattlefyPersistentIdsSerialization];

    /// <summary>
    /// Combination of Battlefy slugs and ids
    /// </summary>
    public IReadOnlyCollection<Name> AllNames => new List<Name>(Usernames.Concat(Slugs).Concat(PersistentIds).Distinct());

    /// <summary>
    /// Add a new Battlefy slug to the Battlefy profile
    /// </summary>
    public void AddSlug(string slug, Source source)
      => SlugsHandler.Add(new BattlefyUserSocial(slug, source));

    /// <summary>
    /// Add new Battlefy slugs to the Battlefy profile
    /// </summary>
    public void AddSlugs(IEnumerable<BattlefyUserSocial> incoming)
      => SlugsHandler.Add(incoming);

    /// <summary>
    /// Add a new Battlefy username to the Battlefy profile
    /// </summary>
    public void AddUsername(string username, Source source)
      => UsernamesHandler.Add(new Name(username, source));

    /// <summary>
    /// Add new Battlefy usernames to the Battlefy profile
    /// </summary>
    public void AddUsernames(IEnumerable<Name> incoming)
      => UsernamesHandler.Add(incoming);

    /// <summary>
    /// Add a new Battlefy persistent id to the Battlefy profile
    /// </summary>
    public void AddPersistentId(string persistentId, Source source)
      => PersistentIdsHandler.Add(new Name(persistentId, source));

    /// <summary>
    /// Add new Battlefy persistent ids to the Battlefy profile
    /// </summary>
    public void AddPersistentIds(IEnumerable<Name> incoming)
      => PersistentIdsHandler.Add(incoming);

    /// <summary>
    /// Return if this Battlefy matches another by slugs.
    /// </summary>
    public bool MatchSlugs(BattlefyHandler other)
      => SlugsHandler.Match(other.SlugsHandler);

    /// <summary>
    /// Return if this Battlefy matches another by usernames.
    /// </summary>
    public bool MatchUsernames(BattlefyHandler other)
      => UsernamesHandler.Match(other.UsernamesHandler);

    /// <summary>
    /// Return if this Battlefy matches another by persistent data (ids).
    /// </summary>
    public bool MatchPersistent(BattlefyHandler other)
      => PersistentIdsHandler.Match(other.PersistentIdsHandler);

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
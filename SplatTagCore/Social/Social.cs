using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SplatTagCore.Social
{
  [Serializable]
  public abstract class Social : Name, ISerializable
  {
    /// <summary>
    /// URL of the website as a base to prepend the handle of the social.
    /// </summary>
    protected readonly string socialBaseAddress;

    /// <summary>
    /// Construct a Social account based on the name and the source
    /// </summary>
    public Social(string handle, Source source, string socialBaseAddress)
      : base(source)
    {
      this.socialBaseAddress = socialBaseAddress;
      base.Value = ProcessHandle(handle);
    }

    /// <summary>
    /// Construct a Social account based on the name and the source
    /// </summary>
    /// <remarks>
    /// This constructor is used by <see cref="Activator"/>.
    /// </remarks>
    public Social(string handle, IEnumerable<Source> sources, string socialBaseAddress)
      : base(sources)
    {
      this.socialBaseAddress = socialBaseAddress;
      base.Value = ProcessHandle(handle);
    }

    /// <summary>
    /// Get the name or id (Handle) of the Social account
    /// </summary>
    public virtual string Handle => Value;

    /// <summary>
    /// Get the Uri to the Social account
    /// </summary>
    public virtual Uri? Uri
    {
      get
      {
        if (Handle == null)
        {
          return null;
        }
        else
        {
          Uri.TryCreate($"https://{socialBaseAddress}/{Handle}", UriKind.RelativeOrAbsolute, out Uri result);
          return result;
        }
      }
    }

    protected virtual string ProcessHandle(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        return value;
      }
      else
      {
        if (value.Contains(socialBaseAddress))
        {
          value = value.Substring(value.IndexOf(socialBaseAddress) + socialBaseAddress.Length);
        }

        return value.TrimStart('/', '@');
      }
    }

    #region Serialization

    // Deserialize
    protected Social(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      this.socialBaseAddress = info.GetString("SocialBaseAddress");
    }

    // Serialize
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("SocialBaseAddress", this.socialBaseAddress);
    }

    #endregion Serialization
  }
}
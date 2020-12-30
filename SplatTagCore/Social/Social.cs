using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SplatTagCore.Social
{
  public abstract class Social : Name
  {
    /// <summary>
    /// URL of the website as a base to prepend the handle of the social.
    /// </summary>
    protected readonly string socialBaseAddress;

    /// <summary>
    /// Construct a Social account based on the name and the source
    /// </summary>
    protected Social(string handle, Source source, string socialBaseAddress)
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
    protected Social(string handle, IEnumerable<Source> sources, string socialBaseAddress)
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
        if (Value == null || socialBaseAddress == null)
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

    protected virtual string ProcessHandle(string newHandle)
    {
      if (string.IsNullOrWhiteSpace(newHandle))
      {
        return newHandle;
      }
      else
      {
        if (newHandle.Contains(socialBaseAddress))
        {
          newHandle = newHandle.Substring(newHandle.IndexOf(socialBaseAddress) + socialBaseAddress.Length);
        }

        return newHandle.TrimStart('/', '@');
      }
    }

    public override string ToString()
    {
      return Uri?.AbsoluteUri ?? Value ?? base.ToString();
    }

    #region Serialization

    // Deserialized constructor
    protected Social(SerializationInfo info, StreamingContext context, string socialBaseAddress)
      : base(info, context)
    {
      this.socialBaseAddress = socialBaseAddress;
    }

    #endregion Serialization
  }
}
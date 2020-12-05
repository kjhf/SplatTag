using System;
using System.Collections.Generic;

namespace SplatTagCore.Social
{
  public abstract class Social : Name
  {
    /// <summary>
    /// Construct a Social account based on the name and the source
    /// </summary>
    protected Social(string handle, Source source)
      : base(handle, source)
    {
      base.Value = ProcessHandle(handle);
    }

    /// <summary>
    /// Construct a Social account based on the name and the source
    /// </summary>
    /// <remarks>
    /// This constructor is used by <see cref="Activator"/>.
    /// </remarks>
    protected Social(string handle, IEnumerable<Source> sources)
      : base(handle, sources)
    {
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
          Uri.TryCreate($"https://{SocialBaseAddress}/{Handle}", UriKind.RelativeOrAbsolute, out Uri result);
          return result;
        }
      }
    }

    /// <summary>
    /// URL of the website as a base to prepend the handle of the social.
    /// </summary>
    protected abstract string SocialBaseAddress { get; }

    protected virtual string ProcessHandle(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        return value;
      }
      else
      {
        if (value.Contains(SocialBaseAddress))
        {
          value = value.Substring(value.IndexOf(SocialBaseAddress) + SocialBaseAddress.Length);
        }

        return value.TrimStart('/', '@');
      }
    }
  }
}
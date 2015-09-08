﻿#region Copyright (C) 2007-2012 Team MediaPortal

/*
    Copyright (C) 2007-2012 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using MediaPortal.Common.MediaManagement;
using MediaPortal.Extensions.MediaServer.DIDL;
using MediaPortal.Extensions.MediaServer.DLNA;
using MediaPortal.Extensions.MediaServer.Filters;
using MediaPortal.Extensions.MediaServer.Objects.Basic;
using MediaPortal.Extensions.MediaServer.Objects.MediaLibrary;
using MediaPortal.Extensions.MediaServer.Protocols;
using MediaPortal.Plugins.Transcoding.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MediaPortal.Extensions.MediaServer.Profiles
{
  #region Profile

  public class VideoInfo
  {
    public VideoContainer VideoContainerType = VideoContainer.Unknown;
    public VideoCodec VideoCodecType = VideoCodec.Unknown;
    public AudioCodec AudioCodecType = AudioCodec.Unknown;
    public H264Profile H264EncodingProfileType = H264Profile.Unknown;
    public PixelFormat PixelFormatType = PixelFormat.Unknown;
    public QualityMode QualityType = QualityMode.Default;
    public H264Preset H264TargetPresetType = H264Preset.Default;
    public string BrandExclusion = null;
    public float AspectRatio = 0;
    public float H264LevelMinimum = 0;
    public long MaxVideoBitrate = 0;
    public int MaxVideoHeight = 0;
    public long AudioBitrate = 0;
    public long AudioFrequency = 0;
    public bool? AudioMultiChannel = null;
    public string FourCC = null;
    public bool? SquarePixels = null;
    public bool ForceVideoTranscoding = false;
    public bool ForceStereo = false;
    public bool ForceInheritance = false;
    public string Movflags = null;

    public bool Matches(MetadataContainer info, int audioStreamIndex, H264LevelCheck h264LevelCheckType)
    {
      bool bPass = true;
      bPass &= (VideoContainerType == VideoContainer.Unknown || VideoContainerType == info.Metadata.VideoContainerType);
      bPass &= (VideoCodecType == VideoCodec.Unknown || VideoCodecType == info.Video.Codec);
      bPass &= (AudioCodecType == AudioCodec.Unknown || AudioCodecType == info.Audio[audioStreamIndex].Codec);
      if(SquarePixels == true )
      {
        bPass &= (info.Video.HasSquarePixels == true);
      }
      else if (SquarePixels == false)
      {
        bPass &= (info.Video.HasSquarePixels == false);
      }
      bPass &= (PixelFormatType == PixelFormat.Unknown || PixelFormatType == info.Video.PixelFormatType);
      if (AudioMultiChannel == true)
      {
        bPass &= (info.Audio[audioStreamIndex].Channels == 0 || info.Audio[audioStreamIndex].Channels > 2);
      }
      else if (AudioMultiChannel == false)
      {
        bPass &= (info.Audio[audioStreamIndex].Channels <= 2);
      }

      List<string> brandExclusions = new List<string>();
      if (BrandExclusion != null)
      {
        brandExclusions.AddRange(BrandExclusion.Split(','));
      }
      bPass &= (BrandExclusion == null || (info.Metadata.MajorBrand != null && !brandExclusions.Contains(info.Metadata.MajorBrand)));

      List<string> fourcc = new List<string>();
      if (FourCC != null)
      {
        fourcc.AddRange(FourCC.Split(','));
      }
      bPass &= (FourCC == null || (info.Video.FourCC != null && fourcc.Contains(info.Video.FourCC)));

      if (info.Video.Codec == VideoCodec.H264)
      {
        if (H264EncodingProfileType != H264Profile.Unknown)
        {
          bPass &= (info.Video.H264ProfileType != H264Profile.Unknown && H264EncodingProfileType == info.Video.H264ProfileType);
          if (H264LevelMinimum > 0)
          {
            float videoH264Level = 0;
            if (h264LevelCheckType == H264LevelCheck.RefFramesLevel)
            {
              videoH264Level = info.Video.H264RefLevel;
            }
            else if (h264LevelCheckType == H264LevelCheck.HeaderLevel)
            {
              videoH264Level = info.Video.H264HeaderLevel;
            }
            else
            {
              if (info.Video.H264HeaderLevel <= 0)
              {
                videoH264Level = info.Video.H264RefLevel;
              }
              if (info.Video.H264RefLevel <= 0)
              {
                videoH264Level = info.Video.H264HeaderLevel;
              }
              if (info.Video.H264HeaderLevel > info.Video.H264RefLevel)
              {
                videoH264Level = info.Video.H264HeaderLevel;
              }
              else
              {
                videoH264Level = info.Video.H264RefLevel;
              }
            }
            bPass &= (videoH264Level > 0 && videoH264Level >= H264LevelMinimum);
          }
        }
      }

      return bPass;
    }

    public bool Matches(VideoInfo videoItem)
    {
      return VideoContainerType == videoItem.VideoContainerType &&
        VideoCodecType == videoItem.VideoCodecType &&
        AudioCodecType == videoItem.AudioCodecType &&
        H264EncodingProfileType == videoItem.H264EncodingProfileType &&
        BrandExclusion == videoItem.BrandExclusion &&
        AspectRatio == videoItem.AspectRatio &&
        H264LevelMinimum == videoItem.H264LevelMinimum &&
        MaxVideoBitrate == videoItem.MaxVideoBitrate &&
        MaxVideoHeight == videoItem.MaxVideoHeight &&
        AudioBitrate == videoItem.AudioBitrate &&
        AudioFrequency == videoItem.AudioFrequency &&
        SquarePixels == videoItem.SquarePixels &&
        FourCC == videoItem.FourCC &&
        Movflags == videoItem.Movflags &&
        ForceVideoTranscoding == videoItem.ForceVideoTranscoding &&
        ForceStereo == videoItem.ForceStereo;
    }
  }

  public class AudioInfo
  {
    public AudioContainer AudioContainerType = AudioContainer.Unknown;
    public long Bitrate = 0;
    public long Frequency = 0;
    public bool ForceStereo = false;
    public bool ForceInheritance = false;

    public bool Matches(MetadataContainer info, int audioStreamIndex)
    {
      bool bPass = true;
      bPass &= (AudioContainerType == AudioContainer.Unknown || AudioContainerType == info.Metadata.AudioContainerType);
      bPass &= (Bitrate == 0 || Bitrate >= info.Audio[audioStreamIndex].Bitrate);
      bPass &= (Frequency == 0 || Frequency >= info.Audio[audioStreamIndex].Frequency);

      return bPass;
    }

    public bool Matches(AudioInfo audioItem)
    {
      return AudioContainerType == audioItem.AudioContainerType &&
        Bitrate == audioItem.Bitrate &&
        Frequency == audioItem.Frequency &&
        ForceStereo == audioItem.ForceStereo;
    }
  }

  public class ImageInfo
  {
    public ImageContainer ImageContainerType = ImageContainer.Unknown;
    public PixelFormat PixelFormatType = PixelFormat.Unknown;
    public QualityMode QualityType = QualityMode.Default;
    public bool ForceInheritance = false;

    public bool Matches(MetadataContainer info)
    {
      bool bPass = true;
      bPass &= (ImageContainerType == ImageContainer.Unknown || ImageContainerType == info.Metadata.ImageContainerType);
      bPass &= (PixelFormatType == PixelFormat.Unknown || PixelFormatType == info.Image.PixelFormatType);

      return bPass;
    }

    public bool Matches(ImageInfo imageItem)
    {
      return ImageContainerType == imageItem.ImageContainerType &&
        PixelFormatType == imageItem.PixelFormatType &&
        QualityType == imageItem.QualityType;
    }
  }

  public abstract class MediaTranscoding
  {
    public string TranscoderBinPath; //Overrides transcoder binary
    public string TranscoderArguments; //Overrides transcoder binary arguments
  }

  public class AudioTranscodingTarget : MediaTranscoding
  {
    public AudioInfo Target = null;
    public List<AudioInfo> Sources = null;
  }

  public class VideoTranscodingTarget : MediaTranscoding
  {
    public VideoInfo Target = null;
    public List<VideoInfo> Sources = null;
  }

  public class ImageTranscodingTarget : MediaTranscoding
  {
    public ImageInfo Target = null;
    public List<ImageInfo> Sources = null;
  }

  public class MediaMimeMapping
  {
    public string MIME = null;
    public string MIMEName = null;
    public string MappedMediaFormat = null;
  }

  public class UpnpDeviceInformation
  {
    public MediaServerUpnPDeviceInformation DeviceInformation = new MediaServerUpnPDeviceInformation();
    public string AdditionalElements = null;
  }

  #endregion

  #region Client settings

  public enum H264LevelCheck
  {
    Any,
    RefFramesLevel,
    HeaderLevel
  }

  public enum ThumbnailDelivery
  {
    None,
    All,
    Resource,
    AlbumArt,
    Icon
  }

  public enum MetadataDelivery
  {
    All,
    Required
  }

  public class ThumbnailSettings
  {
    public int MaxWidth = 160;
    public int MaxHeight = 160;
    public ThumbnailDelivery Delivery = ThumbnailDelivery.All;
  }

  public class VideoSettings
  {
    public H264LevelCheck H264LevelCheckMethod = H264LevelCheck.Any;
    public int MaxHeight = 1080;
    public QualityMode Quality = QualityMode.Default;
    public int QualityFactor = 3;
    public int H264QualityFactor = 25;
    public H264Preset H264TargetPreset = H264Preset.Default;
    public H264Profile H264TargetProfile = H264Profile.Baseline;
    public float H264Level = 3.0F;
    public Coder CoderType = Coder.Default;
  }

  public class ImageSettings
  {
    public int MaxWidth = 4096;
    public int MaxHeight = 4096;
    public bool AutoRotate = false;
    public QualityMode Quality = QualityMode.Default;
    public Coder CoderType = Coder.Default;
  }

  public class AudioSettings
  {
    public bool DefaultStereo = true;
    public int DefaultBitrate = 192;
    public Coder CoderType = Coder.Default;
  }

  public class ProfileSubtitle
  {
    public SubtitleCodec Format = SubtitleCodec.Srt;
    public string Mime = "text/srt";
  }

  public class SubtitleSettings
  {
    public SubtitleSupport SubtitleMode = SubtitleSupport.None;
    public List<ProfileSubtitle> SubtitlesSupported = new List<ProfileSubtitle>();
  }

  public class CommunicationSettings
  {
    public bool AllowChunckedTransfer = true;
    public int DefaultBufferSize = 1500;
    public long InitialBufferSize = 0;
  }

  public class MetadataSettings
  {
    public MetadataDelivery Delivery = MetadataDelivery.All;
  }

  public class ProfileSettings
  {
    public ThumbnailSettings Thumbnails = new ThumbnailSettings();
    public VideoSettings Video = new VideoSettings();
    public ImageSettings Images = new ImageSettings();
    public AudioSettings Audio = new AudioSettings();
    public SubtitleSettings Subtitles = new SubtitleSettings();
    public CommunicationSettings Communication = new CommunicationSettings();
    public MetadataSettings Metadata = new MetadataSettings();
    public BasicContainer RootContainer = null;
  }

  public class TranscodingSetup
  {
    public List<VideoTranscodingTarget> Video = new List<VideoTranscodingTarget>();
    public List<AudioTranscodingTarget> Audio = new List<AudioTranscodingTarget>();
    public List<ImageTranscodingTarget> Images = new List<ImageTranscodingTarget>();
  }

  public class Detection
  {
    public Dictionary<string, string> HttpHeaders = new Dictionary<string, string>();
    public UpnpSearch UPnPSearch = new UpnpSearch();
  }

  public class UpnpSearch
  {
    public UpnpSearch()
    {
      FriendlyName = null;
      ModelName = null;
      ModelNumber = null;
      ProductNumber = null;
      Server = null;
      Manufacturer = null;
    }

    public string FriendlyName { get; set; }
    public string ModelName { get; set; }
    public string ModelNumber { get; set; }
    public string ProductNumber { get; set; }
    public string Server { get; set; }
    public string Manufacturer { get; set; }

    public int Count()
    {
      PropertyInfo[] properties = typeof(UpnpSearch).GetProperties();
      return properties.Count(property => property.GetValue(this) != null);
    }
  }

  public class EndPointSettings
  {
    public EndPointProfile Profile = null;
    public string PreferredSubtitleLanguages = null;
    public string DefaultSubtitleEncodings = null;
    public string PreferredAudioLanguages = null;
    public bool EstimateTransodedSize = true;
    public BasicContainer RootContainer = null;
    public Dictionary<Guid, DlnaMediaItem> DlnaMediaItems = new Dictionary<Guid, DlnaMediaItem>();

    public DlnaMediaItem GetDlnaItem(MediaItem item)
    {
      DlnaMediaItem dlnaItem;
      if (DlnaMediaItems.TryGetValue(item.MediaItemId, out dlnaItem))
        return dlnaItem;

      dlnaItem = new DlnaMediaItem(item, this);
      DlnaMediaItems.Add(item.MediaItemId, dlnaItem);
      return dlnaItem;
    }

    public void InitialiseContainerTree()
    {
      if (Profile == null) return;

      RootContainer = new BasicContainer("0", this) { Title = "MediaPortal Media Library" };

      var audioContainer = new BasicContainer("A", this) { Title = "Audio" };
      audioContainer.Add(new MediaLibraryShareContainer("AAS", this, "Audio") { Title = "Shares" });
      audioContainer.Add(new MediaLibraryAlbumContainer("AA", this) { Title = "Albums" });
      audioContainer.Add(new MediaLibraryGenreContainer("AG", this) { Title = "Genres" });
      RootContainer.Add(audioContainer);

      var pictureContainer = new BasicContainer("I", this) { Title = "Images" };
      pictureContainer.Add(new MediaLibraryShareContainer("IIS", this, "Image") { Title = "Shares" });
      RootContainer.Add(pictureContainer);

      var videoContainer = new BasicContainer("V", this) { Title = "Video" };
      videoContainer.Add(new MediaLibraryShareContainer("VVS", this, "Video") { Title = "Shares" });
      videoContainer.Add(new MediaLibraryShareContainer("VMS", this, "Movie") { Title = "Movie Shares" });
      videoContainer.Add(new MediaLibraryShareContainer("VSS", this, "Series") { Title = "Series Shares" });
      videoContainer.Add(new MediaLibraryGenreContainer("VG", this) { Title = "Genres" });
      RootContainer.Add(videoContainer);

      RootContainer.Add(new MediaLibraryShareContainer("M", this) { Title = "Shares" });
    }
  }

  public class VideoMatch
  {
    public VideoInfo MatchedVideoSource;
    public int MatchedAudioStream;
  }

  public class AudioMatch
  {
    public AudioInfo MatchedAudioSource;
    public int MatchedAudioStream;
  }

  public class ImageMatch
  {
    public ImageInfo MatchedImageSource;
  }

  public class EndPointProfile
  {
    public bool Active = false;
    public string ID = "";
    public string Name = "?";
    public UpnpDeviceInformation UpnpDevice = new UpnpDeviceInformation();
    public GenericDidlMessageBuilder.ContentBuilder DirectoryContentBuilder = GenericDidlMessageBuilder.ContentBuilder.GenericContentBuilder;
    public GenericAccessProtocol.ResourceAccessProtocol ResourceAccessHandler = GenericAccessProtocol.ResourceAccessProtocol.GenericAccessProtocol;
    public GenericContentDirectoryFilter.ContentFilter DirectoryContentFilter = GenericContentDirectoryFilter.ContentFilter.GenericContentFilter;
    public ProtocolInfoFormat ProtocolInfo = ProtocolInfoFormat.DLNA;
    public ProfileSettings Settings = new ProfileSettings();
    public TranscodingSetup MediaTranscoding = new TranscodingSetup();
    public Dictionary<string, MediaMimeMapping> MediaMimeMap = new Dictionary<string, MediaMimeMapping>();
    public List<Detection> Detections = new List<Detection>();

    public override string ToString()
    {
      return ID + " - " + Name;
    }

    #region Transcode Matching

    public VideoTranscodingTarget GetMatchingVideoTranscoding(MetadataContainer info, EndPointSettings settings, out VideoMatch matchedSource)
    {
      int iMatchedAudioStream = 0;
      if (string.IsNullOrEmpty(settings.PreferredAudioLanguages) == false)
      {
        List<string> valuesLangs = new List<string>(settings.PreferredAudioLanguages.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
        int currentPriority = -1;
        for (int iAudio = 0; iAudio < info.Audio.Count; iAudio++)
        {
          for (int iPriority = 0; iPriority < valuesLangs.Count; iPriority++)
          {
            if (valuesLangs[iPriority].Equals(info.Audio[iAudio].Language, StringComparison.InvariantCultureIgnoreCase) == true)
            {
              if (currentPriority == -1 || iPriority < currentPriority)
              {
                currentPriority = iPriority;
                iMatchedAudioStream = iAudio;
              }
            }
          }
        }
      }

      foreach (VideoTranscodingTarget tDef in MediaTranscoding.Video)
      {
        foreach (VideoInfo src in tDef.Sources)
        {
          if (src.Matches(info, iMatchedAudioStream, Settings.Video.H264LevelCheckMethod))
          {
            matchedSource = new VideoMatch();
            matchedSource.MatchedAudioStream = iMatchedAudioStream;
            matchedSource.MatchedVideoSource = src;
            return tDef;
          }
        }
      }
      matchedSource = null;
      return null;
    }

    public AudioTranscodingTarget GetMatchingAudioTranscoding(MetadataContainer info, out AudioMatch matchedSource)
    {
      int iMatchedAudioStream = 0;
      foreach (AudioTranscodingTarget tDef in MediaTranscoding.Audio)
      {
        foreach (AudioInfo src in tDef.Sources)
        {
          if (src.Matches(info, iMatchedAudioStream))
          {
            matchedSource = new AudioMatch();
            matchedSource.MatchedAudioStream = iMatchedAudioStream;
            matchedSource.MatchedAudioSource = src;
            return tDef;
          }
        }
      }
      matchedSource = null;
      return null;
    }

    public ImageTranscodingTarget GetMatchingImageTranscoding(MetadataContainer info, out ImageMatch matchedSource)
    {
      foreach (ImageTranscodingTarget tDef in MediaTranscoding.Images)
      {
        foreach (ImageInfo src in tDef.Sources)
        {
          if (src.Matches(info))
          {
            matchedSource = new ImageMatch();
            matchedSource.MatchedImageSource = src;
            return tDef;
          }
        }
      }
      if(info.Image.Height > Settings.Images.MaxHeight ||
        info.Image.Width > Settings.Images.MaxWidth ||
        (info.Image.Orientation > 1 && Settings.Images.AutoRotate == true))
      {
        matchedSource = new ImageMatch();
        matchedSource.MatchedImageSource = new ImageInfo();

        ImageTranscodingTarget tDef = new ImageTranscodingTarget();
        tDef.Target = new ImageInfo();
        tDef.Target.ImageContainerType = info.Metadata.ImageContainerType;
        tDef.Target.PixelFormatType = info.Image.PixelFormatType;
        tDef.Target.QualityType = Settings.Images.Quality;
        return tDef;
      }
      matchedSource = null;
      return null;
    }
  }

  #endregion

  #endregion
}

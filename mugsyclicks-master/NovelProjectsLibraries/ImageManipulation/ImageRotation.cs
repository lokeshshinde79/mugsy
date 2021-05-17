using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Web;
using System.Net;

namespace NovelProjects.ImageManipulation
{
  public class ImageRotation : IDisposable
  {
    #region Class Variables
    public static byte JPEG = 0;
    public static byte PNG = 1;

    private Stream imageStream;
    private string fileOutputName;
    private bool success;
    private ImageOrientation _ImageOrientation;
    private long defaultQuality = 90;

    // Track whether Dispose has been called.
    private bool disposed = false;
    #endregion

    #region Getter and Setter Methods
    public Stream ImageStream
    {
      get { return imageStream; }
      set { imageStream = value; }
    }

    public bool Success
    {
      get { return success; }
    }

    public string OutputFileName
    {
      get { return fileOutputName; }
    }

    public ImageOrientation ImageOrientation
    {
      get { return _ImageOrientation; }
    }
    #endregion

    #region Constructor
    public ImageRotation(String filepath) : this(new FileStream(filepath, FileMode.Open, FileAccess.Read))
    {
    }

    public ImageRotation(Stream s)
    {
      imageStream = s;
      SetImageOrientation();
    }
    #endregion

    #region AutoRotate
    #region Save to file
    public void AutoRotate(string path, string filename, ImageOrientation orientation)
    {
      AutoRotate(path, filename, orientation, ImageRotation.JPEG);
    }

    public void AutoRotate(string path, string filename, ImageOrientation orientation, byte imageFormat)
    {
      AutoRotate(path, filename, orientation, imageFormat, defaultQuality);
    }

    public void AutoRotate(string path, string filename, ImageOrientation orientation, byte imageFormat, long quality)
    {
      string ext = "";

      switch (imageFormat)
      {
        case 0:
          ext = "jpeg";
          break;

        case 1:
          ext = "png";
          break;

        default:
          ext = "jpeg";
          break;
      }

      //-- set output file name --//
      fileOutputName = filename + "." + ext;

      string fullpath = path + fileOutputName;
      Image sampled = RotateImageStream(orientation, imageFormat);

      //-- save thumbnail --//
      ImageCodecInfo cinfo = GetEncoderInfo(imageFormat);
      EncoderParameters eparams = GetEncoderParameters(quality);
      sampled.Save(fullpath, cinfo, eparams);
    }
    #endregion

    #region To Byte Array
    public byte[] AutoRotateByteArray(ImageOrientation orientation)
    {
      return AutoRotateByteArray(orientation, ImageRotation.JPEG);
    }

    public byte[] AutoRotateByteArray(ImageOrientation orientation, byte imageFormat)
    {
      return AutoRotateByteArray(orientation, imageFormat, defaultQuality);
    }

    public byte[] AutoRotateByteArray(ImageOrientation orientation, byte imageFormat, long quality)
    {
      byte[] retval = null;

      MemoryStream ms = new MemoryStream();
      ImageCodecInfo cinfo = GetEncoderInfo(imageFormat);
      EncoderParameters eparams = GetEncoderParameters(quality);

      Image sampled = RotateImageStream(orientation, imageFormat);
      sampled.Save(ms, cinfo, eparams);
      retval = ms.ToArray();

      return retval;
    }
    #endregion
    #endregion

    #region AutoRotate
    #region Save to file
    public void Rotate(string path, string filename, RotateFlipType rotateType)
    {
      Rotate(path, filename, rotateType, ImageRotation.JPEG);
    }

    public void Rotate(string path, string filename, RotateFlipType rotateType, byte imageFormat)
    {
      Rotate(path, filename, rotateType, imageFormat, defaultQuality);
    }

    public void Rotate(string path, string filename, RotateFlipType rotateType, byte imageFormat, long quality)
    {
      string ext = "";

      switch (imageFormat)
      {
        case 0:
          ext = "jpeg";
          break;

        case 1:
          ext = "png";
          break;

        default:
          ext = "jpeg";
          break;
      }

      //-- set output file name --//
      fileOutputName = filename + "." + ext;

      string fullpath = path + fileOutputName;
      Image sampled = RotateImageStream(rotateType, imageFormat);

      //-- save thumbnail --//
      ImageCodecInfo cinfo = GetEncoderInfo(imageFormat);
      EncoderParameters eparams = GetEncoderParameters(quality);
      sampled.Save(fullpath, cinfo, eparams);
    }
    #endregion

    #region To Byte Array
    public byte[] RotateByteArray(RotateFlipType rotateType)
    {
      return RotateByteArray(rotateType, ImageRotation.JPEG);
    }

    public byte[] RotateByteArray(RotateFlipType rotateType, byte imageFormat)
    {
      return RotateByteArray(rotateType, imageFormat, defaultQuality);
    }

    public byte[] RotateByteArray(RotateFlipType rotateType, byte imageFormat, long quality)
    {
      byte[] retval = null;

      MemoryStream ms = new MemoryStream();
      ImageCodecInfo cinfo = GetEncoderInfo(imageFormat);
      EncoderParameters eparams = GetEncoderParameters(quality);

      Image sampled = RotateImageStream(rotateType, imageFormat);
      sampled.Save(ms, cinfo, eparams);
      retval = ms.ToArray();

      return retval;
    }
    #endregion
    #endregion

    #region RotateImageStream
    private Image RotateImageStream(ImageOrientation orientation, byte imageFormat)
    {
      RotateFlipType flipType = RotateFlipType.RotateNoneFlipNone;

      if (_ImageOrientation != orientation)
        flipType = RotateFlipType.Rotate90FlipNone;

      return RotateImageStream(flipType, imageFormat);
    }

    private Image RotateImageStream(RotateFlipType rotateType, byte imageFormat)
    {
      //-- default --//
      PixelFormat pformat = PixelFormat.Undefined;

      if (imageFormat == ThumbnailResampler.PNG)
        pformat = PixelFormat.Format32bppArgb;
      else
        pformat = PixelFormat.Format32bppRgb;

      return RotateImageStream(rotateType, imageFormat, pformat, InterpolationMode.HighQualityBicubic);
    }

    private Image RotateImageStream(RotateFlipType rotateType, byte imageFormat, PixelFormat pixelFormat, InterpolationMode interpolationMode)
    {
      Image original = null;

      try
      {
        original = Image.FromStream(imageStream);
        original.RotateFlip(rotateType);

        success = true;
      }
      catch (Exception ex)
      {
        //-- dispose of resources --//
        if (original != null) original.Dispose();

        success = false;
      }

      return original;
    }
    #endregion

    #region SetImageOrientation
    private void SetImageOrientation()
    {
      Image original = null;

      try
      {
        original = Image.FromStream(imageStream);

        float originalWidth = (float)original.Width;
        float originalHeight = (float)original.Height;
        float aspectRatio = originalWidth / originalHeight;

        if (aspectRatio > 1)
        {
          this._ImageOrientation = ImageOrientation.Horizontal;
        }
        else if (aspectRatio == 1)
        {
          this._ImageOrientation = ImageOrientation.Square;
        }
        else
        {
          this._ImageOrientation = ImageOrientation.Vertical;
        }

        original.Dispose();
      }
      catch (Exception ex)
      {
        if (original != null) original.Dispose();
      }
    }
    #endregion

    #region MIME and Encoding
    private string GetMimeType(string ext)
    {
      string retval = "";

      switch (ext)
      {
        case "png":
          retval = "image/png";
          break;

        //case ".gif":
        //  retval = "image/gif";
        //  break;

        default:
          retval = "image/jpeg";
          break;
      }

      return retval;
    }

    private string GetExtensionFromMime(string mime)
    {
      string retval = "";

      switch (mime)
      {
        case "image/png":
          retval = "png";
          break;

        default:
          retval = "jpeg";
          break;
      }

      return retval;
    }

    private ImageCodecInfo GetEncoderInfo(byte imageFormat)
    {
      string ext = "";
      switch (imageFormat)
      {
        case 0:
          ext = "jpeg";
          break;

        case 1:
          ext = "png";
          break;

        default:
          ext = "jpeg";
          break;
      }
      return GetEncoderInfo(GetMimeType(ext));
    }

    private ImageCodecInfo GetEncoderInfo(string mimeType)
    {
      ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
      foreach (ImageCodecInfo codec in codecs)
      {
        if (codec.MimeType == mimeType)
        {
          return codec;
        }
      }
      return null;
    }

    private EncoderParameters GetEncoderParameters(long quality)
    {
      EncoderParameters eparams = new EncoderParameters(1);
      EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
      eparams.Param[0] = encoderParam;

      return eparams;
    }
    #endregion

    #region Dispose Methods (IDispose)
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
      if (!this.disposed)
      {
        if (disposing)
        {
          imageStream.Dispose();
        }

        // Note disposing has been done.
        disposed = true;
      }
    }
    #endregion
  }
}

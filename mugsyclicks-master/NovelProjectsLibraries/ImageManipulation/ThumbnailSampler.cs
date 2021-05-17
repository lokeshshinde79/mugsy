#region

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

#endregion

namespace NovelProjects.ImageManipulation
{
  /*
   *  Class: ThumbnailResampler
   *  Author: Josh Smith, Nathan Wilkinson
   *  Company: NovelProjects, Inc.
   */

  public enum ImageOrientation
  {
    Horizontal = 0,
    Square = 1,
    Vertical = 2
  }

  public class ThumbnailResampler : IDisposable
  {
    #region Class Variables

    //-- PUBLIC --//
    public static byte DOWN;
    public static byte FAN = 1;

    //-- CONSTANT --//
    public const byte JPEG = 0;
    public const byte PNG = 1;

    //-- PRIVATE --//
    private ImageOrientation _ImageOrientation;
    private long defaultQuality = 90;

    // Track whether Dispose has been called.
    private bool disposed;
    private string fileOutputName;
    private int finalHeight;
    private int finalWidth;
    private Stream imageStream;
    private bool success;

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

    public int ThumbnailWidth
    {
      get { return finalWidth; }
    }

    public int ThumbnailHeight
    {
      get { return finalHeight; }
    }

    public ImageOrientation ImageOrientation
    {
      get { return _ImageOrientation; }
    }

    #endregion

    #region Constructors

    public ThumbnailResampler(Stream s)
    {
      imageStream = s;
      SetImageOrientation();
    }

    #endregion

    #region Save Methods

    public void Save(string path, string filename)
    {
      //-- set output file name --//
      fileOutputName = filename + ".jpeg";

      string fullpath = path + fileOutputName;

      Image original = Image.FromStream(imageStream);

      //-- save photo --//
      original.Save(fullpath);
    }

    public void Save(string path, string filename, int maxWidth)
    {
      Save(path, filename, JPEG, maxWidth, Int32.MaxValue, defaultQuality);
    }

    public void Save(string path, string filename, byte imageFormat, int maxWidth)
    {
      Save(path, filename, imageFormat, maxWidth, Int32.MaxValue, defaultQuality);
    }

    public void Save(string path, string filename, int maxWidth, int maxHeight)
    {
      Save(path, filename, JPEG, maxWidth, maxHeight, defaultQuality);
    }

    public void Save(string path, string filename, byte imageFormat, int maxWidth, int maxHeight)
    {
      Save(path, filename, imageFormat, maxWidth, maxHeight, defaultQuality);
    }

    public void Save(string path, string filename, int maxWidth, long quality)
    {
      Save(path, filename, JPEG, maxWidth, Int32.MaxValue, quality);
    }

    public void Save(string path, string filename, int maxWidth, int maxHeight, long quality)
    {
      Save(path, filename, JPEG, maxWidth, maxHeight, quality);
    }

    public void Save(string path, string filename, byte imageFormat, int maxWidth, int maxHeight, long quality)
    {
      int[] dim = GetMaxImageDimentions(maxWidth, maxHeight);
      if (dim == null)
      {
        return;
      }

      string ext;

      switch (imageFormat)
      {
        case JPEG:
          ext = "jpeg";
          break;

        case PNG:
          ext = "png";
          break;

        default:
          ext = "jpeg";
          break;
      }

      //-- set output file name --//
      fileOutputName = filename + "." + ext;

      string fullpath = path + fileOutputName;

      try
      {
        using (Image sampled = ResampleImage(imageFormat, dim[0], dim[1]))
        {
          ImageCodecInfo cinfo = GetEncoderInfo(imageFormat);
          EncoderParameters eparams = GetEncoderParameters(quality);
          sampled.Save(fullpath, cinfo, eparams);
        }
      }
      catch (ArgumentException aex)
      {
        throw new Exception("Error saving thumbnail Image!", aex);
      }
    }

    #endregion

    #region GetByteArray

    public byte[] GetByteArray()
    {
      byte[] retval = null;
      MemoryStream ms = new MemoryStream();
      ImageCodecInfo cinfo = GetEncoderInfo(JPEG);
      EncoderParameters eparams = GetEncoderParameters(defaultQuality);

      Image original = Image.FromStream(imageStream);

      original.Save(ms, cinfo, eparams);
      retval = ms.ToArray();

      return retval;
    }

    public byte[] GetByteArray(int maxWidth)
    {
      return GetByteArray(JPEG, maxWidth);
    }

    public byte[] GetByteArray(byte imageFormat, int maxWidth)
    {
      return GetByteArray(imageFormat, maxWidth, Int32.MaxValue);
    }

    public byte[] GetByteArray(int maxWidth, int maxHeight)
    {
      return GetByteArray(JPEG, maxWidth, maxHeight);
    }

    public byte[] GetByteArray(byte imageFormat, int maxWidth, int maxHeight)
    {
      return GetByteArray(imageFormat, maxWidth, maxHeight, defaultQuality);
    }

    public byte[] GetByteArray(int maxWidth, long quality)
    {
      return GetByteArray(JPEG, maxWidth, Int32.MaxValue, quality);
    }

    public byte[] GetByteArray(int maxWidth, int maxHeight, long quality)
    {
      return GetByteArray(JPEG, maxWidth, Int32.MaxValue, quality);
    }

    public byte[] GetByteArray(byte imageFormat, int maxWidth, int maxHeight, long quality)
    {
      int[] dim = GetMaxImageDimentions(maxWidth, maxHeight);
      byte[] retval = null;

      if (dim == null)
      {
        return null;
      }

      try
      {
        using (Image sampled = ResampleImage(imageFormat, dim[0], dim[1]))
        {
          ImageCodecInfo cinfo = GetEncoderInfo(imageFormat);
          EncoderParameters eparams = GetEncoderParameters(quality);

          using (MemoryStream ms = new MemoryStream())
          {
            sampled.Save(ms, cinfo, eparams);

            retval = ms.ToArray();
          }
        }
      }
      catch (ArgumentException aex)
      {
        throw new Exception("Error saving thumbnail Image!", aex);
      }

      return retval;
    }

    #endregion

    #region SaveWithWatermark Methods

    public void SaveWithWatermark(string path, string filename, string watermarkpath, int maxWidth)
    {
      SaveWithWatermark(path, filename, watermarkpath, JPEG, maxWidth);
    }

    public void SaveWithWatermark(string path, string filename, string watermarkpath, byte imageFormat, int maxWidth)
    {
      SaveWithWatermark(path, filename, watermarkpath, imageFormat, maxWidth, Int32.MaxValue);
    }

    public void SaveWithWatermark(string path, string filename, string watermarkpath, int maxWidth, int maxHeight)
    {
      SaveWithWatermark(path, filename, watermarkpath, JPEG, maxWidth, maxHeight);
    }

    public void SaveWithWatermark(string path, string filename, string watermarkpath, byte imageFormat, int maxWidth,
                                  int maxHeight)
    {
      SaveWithWatermark(path, filename, watermarkpath, imageFormat, maxWidth, maxHeight, defaultQuality);
    }

    public void SaveWithWatermark(string path, string filename, string watermarkpath, int maxWidth, long quality)
    {
      SaveWithWatermark(path, filename, watermarkpath, JPEG, maxWidth, Int32.MaxValue, quality);
    }

    public void SaveWithWatermark(string path, string filename, string watermarkpath, int maxWidth, int maxHeight,
                                  long quality)
    {
      SaveWithWatermark(path, filename, watermarkpath, JPEG, maxWidth, Int32.MaxValue, quality);
    }

    public void SaveWithWatermark(string path, string filename, string watermarkpath, byte imageFormat, int maxWidth,
                                  int maxHeight, long quality)
    {
      int[] dim = GetMaxImageDimentions(maxWidth, maxHeight);

      if (dim == null)
      {
        return;
      }

      
      string ext;

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


      try
      {
        using (Image sampled = ResampleImageWithWatermark(imageFormat, dim[0], dim[1], watermarkpath))
        {
          ImageCodecInfo cinfo = GetEncoderInfo(imageFormat);
          EncoderParameters eparams = GetEncoderParameters(quality);
          sampled.Save(fullpath, cinfo, eparams);
        }
      }
      catch (ArgumentException aex)
      {
        throw new Exception("Error saving thumbnail Image!", aex);
      }
    }

    #endregion

    #region ByteArrayWithWatermark Methods

    public byte[] ByteArrayWithWatermark(string watermarkpath, int maxWidth)
    {
      return ByteArrayWithWatermark(watermarkpath, JPEG, maxWidth);
    }

    public byte[] ByteArrayWithWatermark(string watermarkpath, byte imageFormat, int maxWidth)
    {
      return ByteArrayWithWatermark(watermarkpath, imageFormat, maxWidth, Int32.MaxValue);
    }

    public byte[] ByteArrayWithWatermark(string watermarkpath, int maxWidth, int maxHeight)
    {
      return ByteArrayWithWatermark(watermarkpath, JPEG, maxWidth, maxHeight);
    }

    public byte[] ByteArrayWithWatermark(string watermarkpath, byte imageFormat, int maxWidth, int maxHeight)
    {
      return ByteArrayWithWatermark(watermarkpath, imageFormat, maxWidth, maxHeight, defaultQuality);
    }

    public byte[] ByteArrayWithWatermark(string watermarkpath, int maxWidth, long quality)
    {
      return ByteArrayWithWatermark(watermarkpath, JPEG, maxWidth, Int32.MaxValue, quality);
    }

    public byte[] ByteArrayWithWatermark(string watermarkpath, int maxWidth, int maxHeight, long quality)
    {
      return ByteArrayWithWatermark(watermarkpath, JPEG, maxWidth, Int32.MaxValue, quality);
    }

    public byte[] ByteArrayWithWatermark(string watermarkpath, byte imageFormat, int maxWidth, int maxHeight,
                                         long quality)
    {
      int[] dim = GetMaxImageDimentions(maxWidth, maxHeight);
      byte[] retval = null;

      if (dim == null)
      {
        return null;
      }

      try
      {
        using (Image sampled = ResampleImageWithWatermark(imageFormat, dim[0], dim[1], watermarkpath))
        {
          ImageCodecInfo cinfo = GetEncoderInfo(imageFormat);
          EncoderParameters eparams = GetEncoderParameters(quality);

          using (MemoryStream ms = new MemoryStream())
          {
            sampled.Save(ms, cinfo, eparams);

            retval = ms.ToArray();
          }
        }
      }
      catch (ArgumentException aex)
      {
        throw new Exception("Error saving thumbnail Image!", aex);
      }

      return retval;
    }

    #endregion

    #region SaveBeforeCrop Methods

    public void SaveBeforeCrop(string path, string filename, int minWidth)
    {
      SaveBeforeCrop(path, filename, JPEG, minWidth);
    }

    public void SaveBeforeCrop(string path, string filename, byte imageFormat, int minWidth)
    {
      SaveBeforeCrop(path, filename, imageFormat, minWidth, Int32.MaxValue);
    }

    public void SaveBeforeCrop(string path, string filename, int minWidth, int minHeight)
    {
      SaveBeforeCrop(path, filename, JPEG, minWidth, minHeight);
    }

    public void SaveBeforeCrop(string path, string filename, byte imageFormat, int minWidth, int minHeight)
    {
      SaveBeforeCrop(path, filename, imageFormat, minWidth, minHeight, defaultQuality);
    }

    public void SaveBeforeCrop(string path, string filename, int minWidth, long quality)
    {
      SaveBeforeCrop(path, filename, JPEG, minWidth, Int32.MaxValue, quality);
    }

    public void SaveBeforeCrop(string path, string filename, int minWidth, int minHeight, long quality)
    {
      SaveBeforeCrop(path, filename, JPEG, minWidth, minHeight, quality);
    }

    public void SaveBeforeCrop(string path, string filename, byte imageFormat, int minWidth, int minHeight, long quality)
    {
      int[] dim = GetMinImageDimentions(minWidth, minHeight);

      if (dim == null)
      {
        return;
      }

      string ext;

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

      try
      {
        using (Image sampled = ResampleImage(imageFormat, dim[0], dim[1]))
        {
          ImageCodecInfo cinfo = GetEncoderInfo(imageFormat);
          EncoderParameters eparams = GetEncoderParameters(quality);
          sampled.Save(fullpath, cinfo, eparams);
        }
      }
      catch (ArgumentException aex)
      {
        throw new Exception("Error saving thumbnail Image!", aex);
      }
    }

    #endregion

    #region SaveAndAutoCrop Methods

    public void SaveAndAutoCrop(string path, string filename, int minWidth, int minHeight)
    {
      SaveAndAutoCrop(path, filename, JPEG, minWidth, minHeight);
    }

    public void SaveAndAutoCrop(string path, string filename, byte imageFormat, int minWidth, int minHeight)
    {
      SaveAndAutoCrop(path, filename, imageFormat, minWidth, minHeight, defaultQuality);
    }

    public void SaveAndAutoCrop(string path, string filename, int minWidth, int minHeight, long quality)
    {
      SaveAndAutoCrop(path, filename, JPEG, minWidth, minHeight, quality);
    }

    public void SaveAndAutoCrop(string path, string filename, byte imageFormat, int minWidth, int minHeight,
                                long quality)
    {
      int[] dim = GetMinImageDimentions(minWidth, minHeight);
      if (dim == null)
      {
        return;
      }


      string ext;

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



      try
      {
        using (Image sampled = ResampleImage(imageFormat, dim[0], dim[1]))
        {
          using (MemoryStream streamToCrop = new MemoryStream())
          {
            ImageCodecInfo cinfo = GetEncoderInfo(imageFormat);
            EncoderParameters eparams = GetEncoderParameters(quality);
            sampled.Save(streamToCrop, cinfo, eparams);




            int[] br = new int[2];

            int[] tl = CalculateTopLeftAutoCrop(streamToCrop, minWidth, minHeight);
            br[0] = tl[0] + minWidth;
            br[1] = tl[1] + minHeight;

            Crop(streamToCrop, path, filename, tl, br);

          }
        }
      }
      catch (ArgumentException aex)
      {
        throw new Exception("Error saving thumbnail Image!", aex);
      }
    }

    #endregion

    #region Crop Methods

    public void Crop(String outputPath, String filename, int[] topleftXY, int[] bottomrightXY)
    {
      Crop(imageStream, outputPath, filename, topleftXY, bottomrightXY, JPEG, defaultQuality);
    }

    public void Crop(Stream stream, String outputPath, String filename, int[] topleftXY, int[] bottomrightXY)
    {
      Crop(stream, outputPath, filename, topleftXY, bottomrightXY, JPEG, defaultQuality);
    }

    public void Crop(String outputPath, String filename, int[] topleftXY, int[] bottomrightXY, long quality)
    {
      Crop(imageStream, outputPath, filename, topleftXY, bottomrightXY, JPEG, quality);
    }

    public void Crop(Stream stream, String outputPath, String filename, int[] topleftXY, int[] bottomrightXY, byte imageFormat,
                     long quality)
    {

      string ext;

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


      string fullpath = outputPath + fileOutputName;


      try
      {
        using (Image sampled = CropImage(stream, imageFormat, topleftXY, bottomrightXY))
        {
          ImageCodecInfo cinfo = GetEncoderInfo(imageFormat);
          EncoderParameters eparams = GetEncoderParameters(quality);
          sampled.Save(fullpath, cinfo, eparams);
        }
      }
      catch (ArgumentException aex)
      {
        throw new Exception("Error saving thumbnail Image!", aex);
      }
    }

    #endregion

    #region Resampling Method

    private Image ResampleImage(byte imageFormat, int width, int height)
    {
      //-- default --//
      PixelFormat pformat = PixelFormat.Format24bppRgb;

      if (imageFormat == PNG)
        pformat = PixelFormat.Format32bppArgb;

      return ResampleImage(width, height, pformat);
    }

    private Image ResampleImage(int width, int height, PixelFormat pixelFormat)
    {
      Bitmap thumb = null;

      try
      {
        using (Image original = Image.FromStream(imageStream))
        {
          thumb = new Bitmap(width, height, pixelFormat);
          thumb.SetResolution(original.HorizontalResolution, original.VerticalResolution);

          using (Graphics g = Graphics.FromImage(thumb))
          {
            /*
             * InterpolationMode.HighQualityBicubic produces the best quality thumbnail BUT
             *   (and it's a big one) it will add a slight border the image.
             *   on a full color photo it's not noticable, but an image on a white background (like a product shot) you'll see it.
             *   This is not the case for a PNG
             */

            g.CompositingMode = CompositingMode.SourceCopy;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle newSize = new Rectangle(0, 0, width, height);
            Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);

            g.DrawImage(original, newSize, rect, GraphicsUnit.Pixel);

            //-- set width, height --//
            finalWidth = width;
            finalHeight = height;
          }
        }

        #region old
        //original = Image.FromStream(imageStream);

        //thumb = new Bitmap(width, height, pixelFormat);
        //thumb.SetResolution(original.HorizontalResolution, original.VerticalResolution);


        ///*
        // * InterpolationMode.HighQualityBicubic produces the best quality thumbnail BUT
        // *   (and it's a big one) it will add a slight border the image.
        // *   on a full color photo it's not noticable, but an image on a white background (like a product shot) you'll see it.
        // *   This is not the case for a PNG
        // */

        //g = Graphics.FromImage(thumb);
        //g.CompositingMode = CompositingMode.SourceCopy;
        //g.CompositingQuality = CompositingQuality.HighQuality;
        //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //g.SmoothingMode = SmoothingMode.HighQuality;
        //g.PixelOffsetMode = PixelOffsetMode.HighQuality;


        //Rectangle newSize = new Rectangle(0, 0, width, height);
        //Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);

        //g.DrawImage(original, newSize, rect, GraphicsUnit.Pixel);

        ////-- set width, height --//
        //finalWidth = width;
        //finalHeight = height;

        ////-- dispose of resources --//
        //g.Dispose();
        //thumb.Dispose();
        //original.Dispose();
        #endregion

        success = true;
      }
      catch (ArgumentException aex)
      {
        success = false;
        throw new Exception("Error resampling Image to create the thumbnail Image!", aex);
      }

      return thumb;
    }

    #endregion

    #region Resampling With Watermark Method

    private Image ResampleImageWithWatermark(byte imageFormat, int width, int height, string watermarkUrl)
    {
      //-- default --//
      PixelFormat pformat = PixelFormat.Format24bppRgb;

      if (imageFormat == PNG)
        pformat = PixelFormat.Format32bppArgb;

      return ResampleImageWithWatermark(width, height, pformat, watermarkUrl);
    }

    private Image ResampleImageWithWatermark(int width, int height, PixelFormat pixelFormat, String watermarkUrl)
    {
      Bitmap thumb = null;

      try
      {
        using (Image original = Image.FromStream(imageStream))
        using (Bitmap watermark = GetScaledWatermark(watermarkUrl, width, height))
        {
          thumb = new Bitmap(width, height, pixelFormat);
          thumb.SetResolution(original.HorizontalResolution, original.VerticalResolution);

          using (Graphics g = Graphics.FromImage(thumb))
          {
            /*
             * InterpolationMode.HighQualityBicubic produces the best quality thumbnail BUT
             *   (and it's a big one) it will add a slight border the image.
             *   on a full color photo it's not noticable, but an image on a white background (like a product shot) you'll see it.
             *   This is not the case for a PNG
             */

            g.CompositingMode = CompositingMode.SourceCopy;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle newSize = new Rectangle(0, 0, width, height);
            Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);

            g.DrawImage(original, newSize, rect, GraphicsUnit.Pixel);

            #region Draw Watermark
            /*
             * do this to set the size of the watermark to fit the graphic
             */
            rect = new Rectangle(0, 0, watermark.Width, watermark.Height);

            /*
             * use CompositingMode.SourceOver so that it will draw the watermark on top of the current graphic (with transparency and all)
             * without setting it to SourceOver, it will overwrite your graphic
             */
            g.CompositingMode = CompositingMode.SourceOver;
            g.DrawImage(watermark, newSize, rect, GraphicsUnit.Pixel);
            #endregion

            //-- set width, height --//
            finalWidth = width;
            finalHeight = height;
          }
        }

        #region old
        //original = Image.FromStream(imageStream);
        //watermark = GetScaledWatermark(watermarkUrl, width, height);
        //thumb = new Bitmap(width, height, pixelFormat);
        //thumb.SetResolution(original.HorizontalResolution, original.VerticalResolution);

        //g = Graphics.FromImage(thumb);
        //g.CompositingMode = CompositingMode.SourceCopy;
        //g.CompositingQuality = CompositingQuality.HighQuality;
        //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //g.SmoothingMode = SmoothingMode.HighQuality;
        //g.PixelOffsetMode = PixelOffsetMode.HighQuality;


        //Rectangle newSize = new Rectangle(0, 0, width, height);
        //Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);

        //g.DrawImage(original, newSize, rect, GraphicsUnit.Pixel);

        //#region Draw Watermark
        ///*
        // * do this to set the size of the watermark to fit the graphic
        // */
        //rect = new Rectangle(0, 0, watermark.Width, watermark.Height);

        ///*
        // * use CompositingMode.SourceOver so that it will draw the watermark on top of the current graphic (with transparency and all)
        // * without setting it to SourceOver, it will overwrite your graphic
        // */
        //g.CompositingMode = CompositingMode.SourceOver;
        //g.DrawImage(watermark, newSize, rect, GraphicsUnit.Pixel);
        //#endregion

        ////-- set width, height --//
        //finalWidth = width;
        //finalHeight = height;

        ////-- dispose of resources --//
        //g.Dispose();
        //original.Dispose();
        //watermark.Dispose();
        #endregion

        success = true;
      }
      catch (ArgumentException aex)
      {
        success = false;
        throw new Exception("Error resampling Image with watermark to create the thumbnail Image!", aex);
      }
      //catch (Exception)
      //{
      //  //-- dispose of resources --//
      //  if (g != null) g.Dispose();
      //  if (original != null) original.Dispose();
      //  if (thumb != null) thumb.Dispose();
      //  if (watermark != null) watermark.Dispose();

      //  success = false;
      //}

      return thumb;

      #region old code
      //Image original = null;
      //Image watermark = null;
      //Image thumb = null;
      //Graphics g = null;

      //try
      //{
      //  original = Image.FromStream(imageStream);
      //  thumb = new Bitmap(width, height, pixelFormat);
      //  watermark = GetScaledWatermark(watermarkUrl, width, height);

      //  g = Graphics.FromImage(thumb);
      //  g.CompositingQuality = CompositingQuality.Default;
      //  g.SmoothingMode = SmoothingMode.HighQuality;
      //  g.InterpolationMode = InterpolationMode.HighQualityBicubic;

      //  Rectangle rect = new Rectangle(0, 0, width, height);
      //  g.DrawImage(original, rect, 0, 0, original.Width, original.Height, GraphicsUnit.Pixel);
      //  g.DrawImage(watermark, rect, 0, 0, watermark.Width, watermark.Height, GraphicsUnit.Pixel);

      //  ////-- set width, height --//
      //  finalWidth = width;
      //  finalHeight = height;

      //  ////-- dispose of resources --//
      //  g.Dispose();
      //  original.Dispose();
      //  watermark.Dispose();

      //  success = true;
      //}
      //catch (Exception)
      //{
      //  //-- dispose of resources --//
      //  if (g != null) g.Dispose();
      //  if (original != null) original.Dispose();
      //  if (thumb != null) thumb.Dispose();
      //  if (watermark != null) watermark.Dispose();

      //  success = false;
      //}

      //return thumb;
      #endregion
    }

    #endregion

    #region Save With Reflection

    public void SaveWithReflection(string path, string filename, int maxWidth)
    {
      SaveWithReflection(path, filename, maxWidth, Int32.MaxValue, DOWN);
    }

    public void SaveWithReflection(string path, string filename, int maxWidth, byte reflectionType)
    {
      SaveWithReflection(path, filename, maxWidth, Int32.MaxValue, reflectionType);
    }

    public void SaveWithReflection(string path, string filename, int maxWidth, int maxHeight)
    {
      SaveWithReflection(path, filename, maxWidth, maxHeight, DOWN);
    }

    public void SaveWithReflection(string path, string filename, int maxWidth, int maxHeight, byte reflectionType)
    {
      Image original = null;

      try
      {
        original = Image.FromStream(imageStream);
        float originalWidth = original.Width;
        float originalHeight = original.Height;
        float aspectRatio = originalWidth/originalHeight;

        //-- reducedHeight is if the width is the determining scale value, this would be the height --//
        int reducedHeight = (int) Math.Ceiling(maxWidth/aspectRatio);
        //-- reducedHeight is if the height is the determining scale value, this would be the width --//
        int reducedWidth = (int) Math.Ceiling(maxHeight*aspectRatio);

        //int widthSpread = maxWidth - reducedWidth;
        int heightSpread = maxHeight - reducedHeight;

        //-- width is default determinate --//
        int newWidth = heightSpread >= 0 ? maxWidth : reducedWidth;
        int newHeight = heightSpread >= 0 ? reducedHeight : maxHeight;

        original.Dispose();

        SaveResampledImageWithReflection(path, filename, newWidth, newHeight, reflectionType);
      }
      catch (Exception)
      {
        if (original != null) original.Dispose();
      }
    }

    private void SaveResampledImageWithReflection(string path, string filename, int width, int height, byte reflectionType)
    {
      Image original = null;
      Image roriginal = null;
      Image thumb = null;
      Graphics g = null;
      fileOutputName = filename + ".png";
      string fullpath = path + fileOutputName;
      int padding = 0;
      int rwidth = Int32.MaxValue;
      int rheight = Int32.MaxValue;

      if (reflectionType == DOWN)
      {
        rwidth = width;
        rheight = height + 40;
      }
      else if (reflectionType == FAN)
      {
        //-- make width and height 40px bigger --//
        padding = 40;
        rwidth = (int) Math.Ceiling(width + (padding*2f));
        rheight = (int) Math.Ceiling(height + 40f);
      }

      try
      {
        thumb = new Bitmap(rwidth, rheight, PixelFormat.Format32bppArgb);

        g = Graphics.FromImage(thumb);
        g.CompositingQuality = CompositingQuality.Default;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        original = Image.FromStream(imageStream);
        Rectangle rect = new Rectangle(padding, 0, width, height);
        g.DrawImage(original, rect, 0, 0, original.Width, original.Height, GraphicsUnit.Pixel);

        roriginal = Image.FromStream(imageStream);
        roriginal.RotateFlip(RotateFlipType.Rotate180FlipX);

        for (int i = 1; i < (rheight - height); i++)
        {
          double lwidth = (rwidth - width) > 0 ? 80f/(rwidth - width) : 0;
          int rectx = (int) Math.Ceiling(padding - (i*lwidth));
          int rectw = (int) Math.Ceiling(width + (2*(lwidth*i)));

          //Rectangle srcrect = new Rectangle(0, (i * 4 - 1), roriginal.Width, 1);
          Rectangle destrect = new Rectangle(rectx, height + (i - 1), rectw, 1);

          //Point ul = new Point(rectx, (i - 1) + height);
          //Point ur = new Point(2 + rectw, (i - 1) + height);
          //Point ll = new Point(rectx, i + height);
          //Point[] destPara1 = { ul, ur, ll };

          float srcx = 0f;
          float srcy = i*6f;
          float srcwidth = roriginal.Width*1f;
          float srcheight = 1f;

          ColorMatrix matrix = new ColorMatrix();
          float opacity = (1 - (i/40f))*.6f;
          //float opacity = (1f / (i + 1)) - .025f;
          matrix.Matrix33 = opacity >= 0 ? opacity : 0;
          ImageAttributes attr = new ImageAttributes();
          attr.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

          g.DrawImage(roriginal, destrect, srcx, srcy, srcwidth, srcheight, GraphicsUnit.Pixel, attr);
        }

        //-- set width, height --//
        finalWidth = width;
        finalHeight = height;

        //-- save thumbnail --//
        ImageCodecInfo cinfo = GetEncoderInfo("image/png");
        EncoderParameters eparams = new EncoderParameters(1);
        EncoderParameter encoderParam = new EncoderParameter(Encoder.Quality, (long) 100);
        eparams.Param[0] = encoderParam;

        thumb.Save(fullpath, cinfo, eparams);

        //-- dispose of resources --//
        g.Dispose();
        original.Dispose();
        roriginal.Dispose();
        thumb.Dispose();

        success = true;
      }
      catch (Exception)
      {
        //HttpContext.Current.Response.Write(ex.Message);

        //-- dispose of resources --//
        if (g != null) g.Dispose();
        if (original != null) original.Dispose();
        if (roriginal != null) roriginal.Dispose();
        if (thumb != null) thumb.Dispose();

        success = false;
      }
    }

    #endregion

    #region Crop

    private Image CropImage(Stream stream, byte imageFormat, int[] topleftXY, int[] bottomrightXY)
    {
      PixelFormat pformat = PixelFormat.Format24bppRgb;

      if (imageFormat == PNG)
        pformat = PixelFormat.Format32bppArgb;

      return CropImage(stream, topleftXY, bottomrightXY, pformat);
    }

    private Image CropImage(Stream stream, int[] topleftXY, int[] bottomrightXY, PixelFormat pixelFormat)
    {
      Bitmap thumb = null;


      int topleftx = topleftXY[0];
      int toplefty = topleftXY[1];
      int width = bottomrightXY[0] - topleftXY[0];
      int height = bottomrightXY[1] - topleftXY[1];

      try
      {
        using (Image original = Image.FromStream(stream))
        {
          thumb = new Bitmap(width, height, pixelFormat);
          thumb.SetResolution(original.HorizontalResolution, original.VerticalResolution);

          using (Graphics g = Graphics.FromImage(thumb))
          {
            /*
             * InterpolationMode.HighQualityBicubic produces the best quality thumbnail BUT
             *   (and it's a big one) it will add a slight border the image.
             *   on a full color photo it's not noticable, but an image on a white background (like a product shot) you'll see it.
             *   This is not the case for a PNG
             */

            g.CompositingMode = CompositingMode.SourceCopy;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle newSize = new Rectangle(0, 0, width, height);
            Rectangle rect = new Rectangle(topleftx, toplefty, width, height);

            g.DrawImage(original, newSize, rect, GraphicsUnit.Pixel);
            //g.DrawImage(original, rect, topleftx, toplefty, width, (float)height, GraphicsUnit.Pixel);

            //-- set width, height --//
            finalWidth = width;
            finalHeight = height;
          }
        }

        #region old
        //  int topleftx = topleftXY[0];
        //  int toplefty = topleftXY[1];
        //  int width = bottomrightXY[0] - topleftXY[0];
        //  int height = bottomrightXY[1] - topleftXY[1];

        //  original = Image.FromStream(stream);
        //  thumb = new Bitmap(width, height, pixelFormat);

        //  g = Graphics.FromImage(thumb);
        //  g.CompositingQuality = CompositingQuality.Default;
        //  g.SmoothingMode = SmoothingMode.HighQuality;
        //  g.InterpolationMode = interpolationMode;

        //  //-- the -1 is there on purpose, otherwise there is a dark line on the top and left--//
        //  Rectangle rect = new Rectangle(0, 0, width, height);
        //  //g.DrawImage(original, rect, topleftXY[0], topleftXY[1], width, height, GraphicsUnit.Pixel);
        //  g.DrawImage(original, rect, topleftx, toplefty, width, (float) height, GraphicsUnit.Pixel);

        //  //-- set width, height --//
        //  finalWidth = width;
        //  finalHeight = height;

        //  //-- dispose of resources --//
        //  g.Dispose();
        //  original.Dispose();

        //  success = true;
        #endregion

        success = true;
      }
      catch (ArgumentException aex)
      {
        success = false;
        throw new Exception("Error cropping Image!", aex);
      }

      return thumb;
    }

    #endregion

    #region Resampling Util Methods

    #region GetMinImageDimensions

    private int[] GetMinImageDimentions(int maxWidth, int maxHeight)
    {
      int[] retval = null;
      Image original = null;

      try
      {
        original = Image.FromStream(imageStream);
        retval = GetMinImageDimentions(original, maxWidth, maxHeight);
        original.Dispose();
      }
      catch (Exception)
      {
        if (original != null) original.Dispose();
      }

      return retval;
    }

    private int[] GetMinImageDimentions(Image original, int maxWidth, int maxHeight)
    {
      int[] retval;

      float originalWidth = original.Width;
      float originalHeight = original.Height;
      float aspectRatio = originalWidth/originalHeight;

      //-- reducedHeight is if the width is the determining scale value, this would be the height --//
      int reducedHeight = (int) Math.Ceiling(maxWidth/aspectRatio);
      //-- reducedHeight is if the height is the determining scale value, this would be the width --//
      int reducedWidth = (int) Math.Ceiling(maxHeight*aspectRatio);

      //int widthSpread = maxWidth - reducedWidth;
      int heightSpread = maxHeight - reducedHeight;

      //-- width is default determinate --//
      int newWidth = heightSpread >= 0 ? reducedWidth : maxWidth;
      int newHeight = heightSpread >= 0 ? maxHeight : reducedHeight;

      if (newWidth > original.Width && newHeight > original.Height)
      {
        newWidth = original.Width;
        newHeight = original.Height;
      }

      retval = new[] {newWidth, newHeight};

      return retval;
    }

    #endregion

    #region GetMaxImageDimensions

    private int[] GetMaxImageDimentions(int maxWidth, int maxHeight)
    {
      int[] retval = null;
      Image original = null;

      try
      {
        original = Image.FromStream(imageStream);
        retval = GetMaxImageDimentions(original, maxWidth, maxHeight);
        original.Dispose();
      }
      catch (Exception)
      {
        if (original != null) original.Dispose();
      }

      return retval;
    }

    private int[] GetMaxImageDimentions(Image original, int maxWidth, int maxHeight)
    {
      int[] retval;

      float originalWidth = original.Width;
      float originalHeight = original.Height;
      float aspectRatio = originalWidth/originalHeight;

      //-- reducedHeight is if the width is the determining scale value, this would be the height --//
      int reducedHeight = (int) Math.Ceiling(maxWidth/aspectRatio);
      //-- reducedHeight is if the height is the determining scale value, this would be the width --//
      int reducedWidth = (int) Math.Ceiling(maxHeight*aspectRatio);

      //int widthSpread = maxWidth - reducedWidth;
      int heightSpread = maxHeight - reducedHeight;

      //-- width is default determinate --//
      int newWidth = heightSpread >= 0 ? maxWidth : reducedWidth;
      int newHeight = heightSpread >= 0 ? reducedHeight : maxHeight;

      if (newWidth > original.Width && newHeight > original.Height)
      {
        newWidth = original.Width;
        newHeight = original.Height;
      }

      retval = new[] {newWidth, newHeight};

      return retval;
    }

    #endregion

    #region CalculateTopLeftAutoCrop

    private int[] CalculateTopLeftAutoCrop(int width, int height)
    {
      return CalculateTopLeftAutoCrop(imageStream, width, height);
    }

    private int[] CalculateTopLeftAutoCrop(Stream stream, int width, int height)
    {
      int[] retval = new int[2];
      Image original = null;

      try
      {
        original = Image.FromStream(stream);

        if (original.Width > width)
          retval[0] = (original.Width - width)/2;
        else
          retval[0] = 0;

        if (original.Height > height)
          retval[1] = (original.Height - height)/2;
        else
          retval[1] = 0;

        original.Dispose();
      }
      catch (Exception)
      {
        if (original != null) original.Dispose();
      }

      return retval;
    }

    #endregion

    #region GetScaledWatermark

    private Bitmap GetScaledWatermark(string watermarkUrl, int maxWidth, int maxHeight)
    {
      Image watermark = null;
      Bitmap retval = null;
      Graphics g = null;

      try
      {
        /*
         * I think this is bad design to do a http request to get the thumbnail
         */
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(watermarkUrl);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        Stream resStream = response.GetResponseStream();

        watermark = Image.FromStream(resStream);


        int[] watermarkDim = GetMaxImageDimentions(watermark, maxWidth, maxHeight);
        int xoffset = (maxWidth - watermarkDim[0]) / 2;

        retval = new Bitmap(maxWidth, maxHeight, PixelFormat.Format32bppArgb);
        retval.SetResolution(watermark.HorizontalResolution, watermark.VerticalResolution);  // maybe we don't need this??

        g = Graphics.FromImage(retval);
        g.CompositingMode = CompositingMode.SourceCopy;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;


        Rectangle newSize = new Rectangle(0, 0, watermark.Width, watermark.Height);
        Rectangle rect = new Rectangle(xoffset, 0, watermarkDim[0], watermarkDim[1]);

        g.DrawImage(watermark, rect, newSize, GraphicsUnit.Pixel);

        //-- dispose of resources --//
        g.Dispose();
        watermark.Dispose();

        success = true;
      }
      catch (Exception)
      {
        //-- dispose of resources --//
        if (g != null) g.Dispose();
        if (watermark != null) watermark.Dispose();
        if (retval != null) retval.Dispose();

        success = false;
      }

      return retval;
      

      #region old code
      //Image watermark = null;
      //Image retval = null;
      //Graphics g = null;

      //try
      //{
      //  HttpWebRequest request = (HttpWebRequest)WebRequest.Create(watermarkUrl);
      //  HttpWebResponse response = (HttpWebResponse)request.GetResponse();
      //  Stream resStream = response.GetResponseStream();

      //  watermark = Image.FromStream(resStream);

      //  int[] watermarkDim = GetMaxImageDimentions(watermark, maxWidth, maxHeight);
      //  //int width = watermarkDim[0] <= maxWidth ? watermarkDim[0] : maxWidth;
      //  //int height = watermarkDim[1] <= maxHeight ? watermarkDim[1] : maxHeight;
      //  //int width = maxWidth - 10;
      //  //int height = maxHeight - 10;
      //  int xoffset = (maxWidth - watermarkDim[0]) / 2;

      //  retval = new Bitmap(maxWidth, maxHeight, PixelFormat.Format32bppArgb);

      //  g = Graphics.FromImage(retval);
      //  g.CompositingQuality = CompositingQuality.Default;
      //  g.SmoothingMode = SmoothingMode.HighQuality;
      //  g.InterpolationMode = InterpolationMode.HighQualityBicubic;
      //  Rectangle rect = new Rectangle(xoffset, 0, watermarkDim[0], watermarkDim[1]);
      //  g.DrawImage(watermark, rect, 0, 0, watermark.Width, watermark.Height, GraphicsUnit.Pixel);

      //  //-- dispose of resources --//
      //  g.Dispose();
      //  watermark.Dispose();

      //  success = true;
      //}
      //catch (Exception)
      //{
      //  //-- dispose of resources --//
      //  if (g != null) g.Dispose();
      //  if (watermark != null) watermark.Dispose();
      //  if (retval != null) retval.Dispose();

      //  success = false;
      //}

      //return retval;
      #endregion
    }

    #endregion

    #region SetImageOrientation

    private void SetImageOrientation()
    {
      Image original = null;

      try
      {
        original = Image.FromStream(imageStream);

        float originalWidth = original.Width;
        float originalHeight = original.Height;
        float aspectRatio = originalWidth/originalHeight;

        if (aspectRatio < 1)
        {
          _ImageOrientation = ImageOrientation.Horizontal;
        }
        else if (aspectRatio == 1)
        {
          _ImageOrientation = ImageOrientation.Square;
        }
        else
        {
          _ImageOrientation = ImageOrientation.Vertical;
        }

        original.Dispose();
      }
      catch (Exception)
      {
        if (original != null) original.Dispose();
      }
    }

    #endregion

    #region MIME and Encoding

    private string GetMimeType(string ext)
    {
      string retval;

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
      string retval;

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
      string ext;
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
      EncoderParameter encoderParam = new EncoderParameter(Encoder.Quality, quality);
      eparams.Param[0] = encoderParam;

      return eparams;
    }

    #endregion

    #endregion

    #region Dispose Methods (IDispose)

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
      if (!disposed)
      {
        if (disposing)
        {
          imageStream.Dispose();
        }

        disposed = true;
      }
    }

    #endregion
  }
}
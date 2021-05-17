using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace NovelProjects.ImageManipulation
{
	public class ImageMasker
	{
		#region Apply Mask
		/// <summary>
		/// Takes in an image and a mask and puts the mask on top of the image.
		/// </summary>
		/// <param name="i">Image to draw</param>
		/// <param name="m">Mask to overlay on image</param>
		/// <param name="dest">Location to draw the image in the mask</param>
		/// <param name="bgColor">Canvas background color</param>
		/// <returns>Stream of the masked image</returns>
		public static Stream CreateMask(Stream i, Stream m, RectangleF dest, Color bgColor)
		{
			Image image = Image.FromStream(i);
			Image mask = Image.FromStream(m);

		  return CreateMask(image, mask, dest, bgColor);
      //MemoryStream s = new MemoryStream();

      //try
      //{
      //  using (Image canvas = new Bitmap(mask.Width, mask.Height))
      //  {
      //    // Initialize Graphics object to work on the canvas
      //    using (Graphics g = Graphics.FromImage(canvas))
      //    {
      //      g.Clear(bgColor);

      //      g.DrawImage(image, dest, new RectangleF(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
      //      g.DrawImage(mask, new RectangleF(0, 0, mask.Width, mask.Height), new RectangleF(0, 0, mask.Width, mask.Height), GraphicsUnit.Pixel);

      //      canvas.Save(s, ImageFormat.Jpeg);
      //    }
      //  }
      //}
      //catch { throw new Exception("Error"); }
      //finally
      //{
      //  image.Dispose();
      //  mask.Dispose();
      //}

      //return s;
		}


    /// <summary>
    /// Takes in an image and a mask and puts the mask on top of the image.
    /// </summary>
    /// <param name="image">Image to draw</param>
    /// <param name="mask">Mask to overlay on image</param>
    /// <param name="dest">Location to draw the image in the mask</param>
    /// <param name="bgColor">Canvas background color</param>
    /// <returns>Stream of the masked image</returns>
    public static MemoryStream CreateMask(Image image, Image mask, RectangleF dest, Color bgColor)
    {
      //Image image = Image.FromStream(i);
      //Image mask = Image.FromStream(m);
      MemoryStream s = new MemoryStream();

      try
      {
        using (Image canvas = new Bitmap(mask.Width, mask.Height))
        {
          // Initialize Graphics object to work on the canvas
          using (Graphics g = Graphics.FromImage(canvas))
          {
            g.Clear(bgColor);

            g.DrawImage(image, dest, new RectangleF(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            g.DrawImage(mask, new RectangleF(0, 0, mask.Width, mask.Height), new RectangleF(0, 0, mask.Width, mask.Height), GraphicsUnit.Pixel);

            canvas.Save(s, ImageFormat.Jpeg);
          }
        }
      }
      catch { throw new Exception("Error"); }
      finally
      {
        image.Dispose();
        mask.Dispose();
      }

      return s;
    }
		#endregion
		
    /// <summary>
    /// Combines the two Images, by positioning the second on top of the first
    /// </summary>
    /// <param name="baseImage"></param>
    /// <param name="maskImage"></param>
    /// <param name="left"></param>
    /// <param name="top"></param>
    /// <returns>Returns a PNG</returns>
    public static MemoryStream MaskImage(Image baseImage, Image maskImage, int left, int top)
    {
      Image newImage = (Image)baseImage.Clone();
      Graphics combineImage = Graphics.FromImage(newImage);
      combineImage.DrawImage(maskImage, left, top);

      MemoryStream s = new MemoryStream();
      newImage.Save(s, ImageFormat.Png);

      //Image returnImage = Image.FromStream(s);
      //return returnImage;

      return s;
    }

    /// <summary>
    /// Combines the two Images, by positioning the second on top of the first
    /// </summary>
    /// <param name="baseImage"></param>
    /// <param name="maskImage"></param>
    /// <param name="left"></param>
    /// <param name="top"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns>Returns a PNG</returns>
    public static MemoryStream MaskImage(Image baseImage, Image maskImage, int left, int top, int width, int height)
    {
      return MaskImage(baseImage, maskImage, new Rectangle(left, top, width, height));
    }

    /// <summary>
    /// Combines the two Images, by positioning the second on top of the first
    /// </summary>
    /// <param name="baseImage"></param>
    /// <param name="maskImage"></param>
    /// <param name="rectangle"></param>
    /// <returns>Returns a PNG</returns>
    public static MemoryStream MaskImage(Image baseImage, Image maskImage, Rectangle rectangle)
    {
      Image newImage = (Image)baseImage.Clone();
      Graphics combineImage = Graphics.FromImage(newImage);
      combineImage.DrawImage(maskImage, rectangle);

      MemoryStream s = new MemoryStream();
      newImage.Save(s, ImageFormat.Png);

      //Image returnImage = Image.FromStream(s);
      //return returnImage;

      return s;
    }
	}
}
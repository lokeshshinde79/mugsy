using System;
using System.Collections;
using System.IO;
using System.Web;

namespace NovelProjects.Web
{

  /// <summary>
  /// This object is for passing and returning 
  /// </summary>
  public class DownloadObject
  {
    public Byte[] FileData { get; set; }
    public String FileName { get; set; }
    public String Extension { get; set; }

    /// <summary>
    /// DownloadObject
    /// </summary>
    public DownloadObject()
    {

    }

    /// <summary>
    /// DownloadObject
    /// </summary>
    /// <param name="FileData">Byte Array</param>
    /// <param name="Extension">File extension, ie .zip, .pdf etc</param>
    public DownloadObject(Byte[] fileData, String extension)
    {
      FileData = fileData;
      Extension = extension;
    }

    /// <summary>
    /// DownloadObject
    /// </summary>
    /// <param name="FileData">Byte Array</param>
    /// <param name="Extension">File extension, ie .zip, .pdf etc</param>
    /// <param name="FileName">File name. Does not need to contain the extension</param>
    public DownloadObject(Byte[] FileData, String Extension, String FileName)
    {
      this.FileData = FileData;
      this.Extension = Extension;
      this.FileName = FileName;
    }

    public void StreamDownload()
    {
			FileUtils.StreamDownload(FileData, Extension, FileName);
    }

    public void StreamDownload(HttpContext Context)
    {
			FileUtils.StreamDownload(FileData, Extension, FileName, Context);
    }

    public void PromptDownload()
    {
      FileUtils.PromptDownload(FileData, Extension, FileName);
    }

    public void PromptDownload(HttpContext Context)
    {
      FileUtils.PromptDownload(FileData, Extension, FileName, Context);
    }
  }

  /// <summary>
  /// Methods for prompting and streaming file downloads.
  /// </summary>
  public static class FileUtils
  {

    /// <summary>
    /// Reads data from a stream until the end is reached. The
    /// data is returned as a byte array. An IOException is
    /// thrown if any of the underlying IO calls fail.
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    public static Byte[] ReadFully(Stream stream)
    {
      return ReadFully(stream, null);
    }

    /// <summary>
    /// Reads data from a stream until the end is reached. The
    /// data is returned as a byte array. An IOException is
    /// thrown if any of the underlying IO calls fail.
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="initialBufferLength">The initial buffer length</param>
    public static Byte[] ReadFully(Stream stream, long initialBufferLength)
    {
      return ReadFully(stream, (int?) initialBufferLength);
    }

    /// <summary>
    /// Reads data from a stream until the end is reached. The
    /// data is returned as a byte array. An IOException is
    /// thrown if any of the underlying IO calls fail.
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="initialBufferLength">The initial buffer length</param>
    public static Byte[] ReadFully(Stream stream, int initialBufferLength)
    {
			return ReadFully(stream, (int?)initialBufferLength);
    }

    /// <summary>
    /// Reads data from a stream until the end is reached. The
    /// data is returned as a byte array. An IOException is
    /// thrown if any of the underlying IO calls fail.
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="initialBufferLength">The initial buffer length</param>
    private static Byte[] ReadFully(Stream stream, int? initialBufferLength)
    {
      /*
       * src: http://www.yoda.arachsys.com/csharp/readbinary.html
       */

      // If we've been passed an unhelpful initial length, just
      // use 32K.
      if (!initialBufferLength.HasValue || initialBufferLength < 1)
      {
        initialBufferLength = 32768;
      }

      byte[] buffer = new byte[initialBufferLength.Value];
      int read = 0;

      int chunk;
      while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
      {
        read += chunk;

        // If we've reached the end of our buffer, check to see if there's
        // any more information
        if (read == buffer.Length)
        {
          int nextByte = stream.ReadByte();

          // End of stream? If so, we're done
          if (nextByte == -1)
          {
            return buffer;
          }

          // Nope. Resize the buffer, put in the byte we've just
          // read, and continue
          byte[] newBuffer = new byte[buffer.Length * 2];
          Array.Copy(buffer, newBuffer, buffer.Length);
          newBuffer[read] = (byte)nextByte;
          buffer = newBuffer;
          read++;
        }
      }
      // Buffer is now too big. Shrink it.
      byte[] ret = new byte[read];
      Array.Copy(buffer, ret, read);
      return ret;
    }



		#region File Prompt and Stream Download Methods

		/// <summary>
		/// This Streams the Download of a given ByteArray
		/// </summary>
		/// <param name="fileData">Byte Array</param>
		/// <param name="extension">File extension, ie .zip, .pdf etc</param>
		public static void StreamDownload(Byte[] fileData, String extension)
		{
			StreamDownload(fileData, extension, String.Empty, HttpContext.Current);
		}
    
    /// <summary>
    /// This Streams the Download of a given ByteArray
    /// </summary>
    /// <param name="fileData">Byte Array</param>
		/// <param name="extension">File extension, ie .zip, .pdf etc</param>
		/// <param name="fileName">Name of the file, does not need to contain the file extension.</param>
		public static void StreamDownload(Byte[] fileData, String extension, String fileName)
    {
			StreamDownload(fileData, extension, fileName, HttpContext.Current);
    }

    /// <summary>
    /// This Streams the Download of a given ByteArray
    /// </summary>
    /// <param name="fileData">Byte Array</param>
		/// <param name="extension">File extension, ie .zip, .pdf etc</param>
		/// <param name="context">The current web request.</param>
		public static void StreamDownload(Byte[] fileData, String extension, HttpContext context)
		{
			StreamDownload(fileData, extension, String.Empty, HttpContext.Current);
		}

    /// <summary>
    /// This Streams the Download of a given ByteArray
    /// </summary>
    /// <param name="fileData">Byte Array</param>
		/// <param name="extension">File extension, ie .zip, .pdf etc</param>
		/// <param name="fileName">Name of the file, does not need to contain the file extension.</param>
		/// <param name="context">The current web request.</param>
		public static void StreamDownload(Byte[] fileData, String extension, String fileName, HttpContext context)
    {
      //HttpContext.Current.Response.Clear();
      //HttpContext.Current.Response.ContentType = GetContentType(extension);
      //HttpContext.Current.Response.OutputStream.Write(fileData, 0, fileData.Length);
      //HttpContext.Current.Response.End();



      /*
       * Updated Code 7/28/09 by Josh
       */

      // Offset for writing the byte array
      int offset = 0;

      // Length of the file/array:
      int length;

      // Total bytes to read:
      long dataToRead;

      try
      {
        // Total bytes to read:
        dataToRead = fileData.Length;

        length = dataToRead > 10000 ? 10000 : (int)dataToRead;

        //Current.Response.Clear(); // commented out b/c it seemed to be causing issues
        context.Response.ContentType = GetContentType(extension);
				context.Response.AddHeader("Content-Disposition", "filename=\"" + fileName + "\"");
        context.Response.AddHeader("Content-Length", fileData.Length.ToString());

        // Read the bytes.
        while (dataToRead > 0)
        {
          // Verify that the client is connected.
          if (context.Response.IsClientConnected)
          {
            // Write the data to the current output stream.
            context.Response.OutputStream.Write(fileData, offset, length);

            // Flush the data to the HTML output.
            context.Response.Flush();

            offset += length;
            dataToRead = dataToRead - length;
            length = dataToRead > 10000 ? 10000 : (int)dataToRead;
          }
          else
          {
            //prevent infinite loop if user disconnects
            dataToRead = -1;
          }
        }
      }
      catch (Exception ex)
      {
        // Trap the error, if any.
        context.Response.Write("Error : " + ex.Message);
      }
      finally
      {
        context.Response.Close();
        //Current.Response.End(); // commented out and used the line above instead
      }
    }
    
    /// <summary>
    /// Prompts the download of the File. The fileName does not need to contain the file extension.
    /// </summary>
    /// <param name="filePath">The fully qualified name of the file, or the relative file name.</param>
    /// <param name="fileName">Name of the file, does not need to contain the file extension.</param>
    public static void PromptDownload(String filePath, String fileName)
    {
      string ext = Path.GetExtension(filePath);

      PromptDownload(filePath, ext, fileName);
    }

    /// <summary>
    /// Prompts the download of the File. The fileName does not need to contain the extension.
    /// </summary>
    /// <param name="filePath">The fully qualified name of the file, or the relative file name.</param>
    /// <param name="extension">The file extension, ie .zip, .pdf etc.</param>
    /// <param name="fileName">Name of the file, does not need to contain the file extension.</param>
    public static void PromptDownload(String filePath, String extension, String fileName)
    {
      byte[] fileData = File.ReadAllBytes(filePath);

      PromptDownload(fileData, extension, fileName, HttpContext.Current);
		}

		/// <summary>
		/// This Prompts for the Download of a given ByteArray.  The FileName does not need to contain the Extension.
		/// </summary>
		/// <param name="fileData">Byte Array</param>
		/// <param name="extension">File extension, ie .zip, .pdf etc</param>
		/// <param name="fileName">The FileName does not need to contain the Extension</param>
		public static void PromptDownload(Byte[] fileData, String extension, String fileName)
		{
      PromptDownload(fileData, extension, fileName, HttpContext.Current);
		}

    [Obsolete("This method has been replaced with PromptDownload(Byte[] FileData, String Extension, String FileName, HttpContext Context)")]
    public static void PromptDownload(HttpContext context, Byte[] fileData, String extension, String fileName)
    {
      PromptDownload(fileData, extension, fileName, context);
    }

  	/// <summary>
    /// This Prompts for the Download of a given ByteArray.  The fileName does not need to contain the extension.
		/// </summary>
    /// <param name="fileData">Byte Array</param>
    /// <param name="extension">File extension, ie .zip, .pdf etc</param>
    /// <param name="fileName">The FileName does not need to contain the extension</param>
    /// <param name="context">The current web request.</param>
    public static void PromptDownload(Byte[] fileData, String extension, String fileName, HttpContext context)
    {
      // added by Josh 10/20/08
      if (!fileName.Contains(extension))
      {
        fileName += extension;
      }


      /*
       * Updated Code 7/28/09 by Josh
       */
      
      // Offset for writing the byte array
      int offset = 0;

      // Length of the file/array:
      int length;

      // Total bytes to read:
      long dataToRead;

      try
      {
        // Total bytes to read:
        dataToRead = fileData.Length;

        length = dataToRead > 10000 ? 10000 : (int)dataToRead;

        //Current.Response.Clear(); // commented out b/c it seemed to be causing issues
        context.Response.ContentType = GetContentType(extension);
        context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
        context.Response.AddHeader("Content-Length", fileData.Length.ToString());

        // Read the bytes.
        while (dataToRead > 0)
        {
          // Verify that the client is connected.
          if (context.Response.IsClientConnected)
          {
            // Write the data to the current output stream.
            context.Response.OutputStream.Write(fileData, offset, length);

            // Flush the data to the HTML output.
            context.Response.Flush();

            offset += length;
            dataToRead = dataToRead - length;
            length = dataToRead > 10000 ? 10000 : (int)dataToRead;
          }
          else
          {
            //prevent infinite loop if user disconnects
            dataToRead = -1;
          }
        }
      }
      catch (Exception ex)
      {
        // Trap the error, if any.
        context.Response.Write("Error : " + ex.Message);
      }
      finally
      {
        context.Response.Close();
        //Current.Response.End(); // commented out and used the line above instead
      }
    }
    #endregion

    #region Get content type for file, given its extenstion
    /// <summary>
    /// Gets the type of the content.
    /// </summary>
    /// <param name="extension">The file extension.</param>
    /// <returns></returns>
    public static string GetContentType(string extension)
    {
      #region Create file extension lookup table
      Hashtable lookup = new Hashtable();
      lookup[".323"] = "text/h323";
      lookup[".acx"] = "application/internet-property-stream";
      lookup[".ai"] = "application/postscript";
      lookup[".aif"] = "audio/x-aiff";
      lookup[".aifc"] = "audio/x-aiff";
      lookup[".aiff"] = "audio/x-aiff";
      lookup[".asf"] = "video/x-ms-asf";
      lookup[".asr"] = "video/x-ms-asf";
      lookup[".asx"] = "video/x-ms-asf";
      lookup[".au"] = "audio/basic";
      lookup[".avi"] = "video/x-msvideo";
      lookup[".axs"] = "application/olescript";
      lookup[".bas"] = "text/plain";
      lookup[".bcpio"] = "application/x-bcpio";
      lookup[".bin"] = "application/octet-stream";
      lookup[".bmp"] = "image/bmp";
      lookup[".c"] = "text/plain";
      lookup[".cat"] = "application/vnd.ms-pkiseccat";
      lookup[".cdf"] = "application/x-cdf";
      lookup[".cer"] = "application/x-x509-ca-cert";
      lookup[".class"] = "application/octet-stream";
      lookup[".clp"] = "application/x-msclip";
      lookup[".cmx"] = "image/x-cmx";
      lookup[".cod"] = "image/cis-cod";
      lookup[".cpio"] = "application/x-cpio";
      lookup[".crd"] = "application/x-mscardfile";
      lookup[".crl"] = "application/pkix-crl";
      lookup[".crt"] = "application/x-x509-ca-cert";
      lookup[".csh"] = "application/x-csh";
      lookup[".css"] = "text/css";
      lookup[".db"] = "application/octet-stream";
      lookup[".dcr"] = "application/x-director";
      lookup[".der"] = "application/x-x509-ca-cert";
      lookup[".dir"] = "application/x-director";
      lookup[".dll"] = "application/x-msdownload";
      lookup[".dms"] = "application/octet-stream";
      lookup[".doc"] = "application/msword";
      lookup[".dot"] = "application/msword";
      lookup[".dvi"] = "application/x-dvi";
      lookup[".dwg"] = "application/acad";
      lookup[".dxr"] = "application/x-director";
      lookup[".eps"] = "application/postscript";
      lookup[".etx"] = "text/x-setext";
      lookup[".evy"] = "application/envoy";
      lookup[".exe"] = "application/octet-stream";
      lookup[".fif"] = "application/fractals";
      lookup[".fla"] = "application/octet-stream";
      lookup[".flr"] = "x-world/x-vrml";
      lookup[".fnm"] = "application/fanniemae";
      lookup[".gif"] = "image/gif";
      lookup[".gtar"] = "application/x-gtar";
      lookup[".gz"] = "application/x-gzip";
      lookup[".h"] = "text/plain";
      lookup[".hdf"] = "application/x-hdf";
      lookup[".hlp"] = "application/winhlp";
      lookup[".hqx"] = "application/mac-binhex40";
      lookup[".hta"] = "application/hta";
      lookup[".htc"] = "text/x-component";
      lookup[".htm"] = "text/html";
      lookup[".html"] = "text/html";
      lookup[".htt"] = "text/webviewhtml";
      lookup[".ico"] = "image/x-icon";
      lookup[".ief"] = "image/ief";
      lookup[".iii"] = "application/x-iphone";
      lookup[".ins"] = "application/x-internet-signup";
      lookup[".isp"] = "application/x-internet-signup";
      lookup[".jfif"] = "image/pipeg";
      lookup[".jpe"] = "image/jpeg";
      lookup[".jpeg"] = "image/jpeg";
      lookup[".jpg"] = "image/jpeg";
      lookup[".js"] = "application/x-javascript";
      lookup[".latex"] = "application/x-latex";
      lookup[".ldf"] = "application/octet-stream";
      lookup[".lha"] = "application/octet-stream";
      lookup[".lsf"] = "video/x-la-asf";
      lookup[".lsx"] = "video/x-la-asf";
      lookup[".lzh"] = "application/octet-stream";
      lookup[".m13"] = "application/x-msmediaview";
      lookup[".m14"] = "application/x-msmediaview";
      lookup[".m3u"] = "audio/x-mpegurl";
      lookup[".man"] = "application/x-troff-man";
      lookup[".mdb"] = "application/x-msaccess";
      lookup[".mdf"] = "application/octet-stream";
      lookup[".me"] = "application/x-troff-me";
      lookup[".mht"] = "message/rfc822";
      lookup[".mhtml"] = "message/rfc822";
      lookup[".mid"] = "audio/mid";
      lookup[".mny"] = "application/x-msmoney";
      lookup[".mov"] = "video/quicktime";
      lookup[".movie"] = "video/x-sgi-movie";
      lookup[".mp2"] = "video/mpeg";
      lookup[".mp3"] = "audio/mpeg";
      lookup[".mp4"] = "video/mp4";
      lookup[".mpa"] = "video/mpeg";
      lookup[".mpe"] = "video/mpeg";
      lookup[".mpeg"] = "video/mpeg";
      lookup[".mpg"] = "video/mpeg";
      lookup[".mpp"] = "application/vnd.ms-project";
      lookup[".mpv2"] = "video/mpeg";
      lookup[".ms"] = "application/x-troff-ms";
      lookup[".mvb"] = "application/x-msmediaview";
      lookup[".nws"] = "message/rfc822";
      lookup[".oda"] = "application/oda";
      lookup[".p10"] = "application/pkcs10";
      lookup[".p12"] = "application/x-pkcs12";
      lookup[".p7b"] = "application/x-pkcs7-certificates";
      lookup[".p7c"] = "application/x-pkcs7-mime";
      lookup[".p7m"] = "application/x-pkcs7-mime";
      lookup[".p7r"] = "application/x-pkcs7-certreqresp";
      lookup[".p7s"] = "application/x-pkcs7-signature";
      lookup[".pbm"] = "image/x-portable-bitmap";
      lookup[".pdf"] = "application/pdf";
      lookup[".pfx"] = "application/x-pkcs12";
      lookup[".pgm"] = "image/x-portable-graymap";
      lookup[".pko"] = "application/ynd.ms-pkipko";
      lookup[".pma"] = "application/x-perfmon";
      lookup[".pmc"] = "application/x-perfmon";
      lookup[".pml"] = "application/x-perfmon";
      lookup[".pmr"] = "application/x-perfmon";
      lookup[".pmw"] = "application/x-perfmon";
      lookup[".png"] = "image/png";
      lookup[".pnm"] = "image/x-portable-anymap";
      lookup[".pot,"] = "application/vnd.ms-powerpoint";
      lookup[".ppm"] = "image/x-portable-pixmap";
      lookup[".pps"] = "application/vnd.ms-powerpoint";
      lookup[".ppt"] = "application/vnd.ms-powerpoint";
      lookup[".prf"] = "application/pics-rules";
      lookup[".ps"] = "application/postscript";
      lookup[".pst"] = "application/octet-stream";
      lookup[".psd"] = "application/photoshop";
      lookup[".pub"] = "application/x-mspublisher";
      lookup[".qbb"] = "application/octet-stream";
      lookup[".qbw"] = "application/octet-stream";
      lookup[".qt"] = "video/quicktime";
      lookup[".ra"] = "audio/x-pn-realaudio";
      lookup[".ram"] = "audio/x-pn-realaudio";
      lookup[".ras"] = "image/x-cmu-raster";
      lookup[".rgb"] = "image/x-rgb";
      lookup[".rmi"] = "audio/mid";
      lookup[".roff"] = "application/x-troff";
      lookup[".rtf"] = "application/rtf";
      lookup[".rtx"] = "text/richtext";
      lookup[".scd"] = "application/x-msschedule";
      lookup[".sct"] = "text/scriptlet";
      lookup[".setpay"] = "application/set-payment-initiation";
      lookup[".setreg"] = "application/set-registration-initiation";
      lookup[".sh"] = "application/x-sh";
      lookup[".shar"] = "application/x-shar";
      lookup[".sit"] = "application/x-stuffit";
      lookup[".snd"] = "audio/basic";
      lookup[".spc"] = "application/x-pkcs7-certificates";
      lookup[".spl"] = "application/futuresplash";
      lookup[".src"] = "application/x-wais-source";
      lookup[".sst"] = "application/vnd.ms-pkicertstore";
      lookup[".stl"] = "application/vnd.ms-pkistl";
      lookup[".stm"] = "text/html";
      lookup[".sv4cpio"] = "application/x-sv4cpio";
      lookup[".sv4crc"] = "application/x-sv4crc";
      lookup[".swf"] = "application/x-shockwave-flash";
      lookup[".t"] = "application/x-troff";
      lookup[".tar"] = "application/x-tar";
      lookup[".tcl"] = "application/x-tcl";
      lookup[".tex"] = "application/x-tex";
      lookup[".texi"] = "application/x-texinfo";
      lookup[".texinfo"] = "application/x-texinfo";
      lookup[".tgz"] = "application/x-compressed";
      lookup[".tif"] = "image/tiff";
      lookup[".tiff"] = "image/tiff";
      lookup[".tr"] = "application/x-troff";
      lookup[".trm"] = "application/x-msterminal";
      lookup[".tsv"] = "text/tab-separated-values";
      lookup[".txt"] = "text/plain";
      lookup[".uls"] = "text/iuls";
      lookup[".ustar"] = "application/x-ustar";
      lookup[".vcf"] = "text/x-vcard";
      lookup[".vrml"] = "x-world/x-vrml";
      lookup[".wav"] = "audio/x-wav";
      lookup[".wcm"] = "application/vnd.ms-works";
      lookup[".wdb"] = "application/vnd.ms-works";
      lookup[".wks"] = "application/vnd.ms-works";
      lookup[".wmf"] = "application/x-msmetafile";
      lookup[".wp"] = "application/wordperfect";
      lookup[".wps"] = "application/vnd.ms-works";
      lookup[".wri"] = "application/x-mswrite";
      lookup[".wrl"] = "x-world/x-vrml";
      lookup[".wrz"] = "x-world/x-vrml";
      lookup[".xaf"] = "x-world/x-vrml";
      lookup[".xbm"] = "image/x-xbitmap";
      lookup[".xla"] = "application/vnd.ms-excel";
      lookup[".xlc"] = "application/vnd.ms-excel";
      lookup[".xlm"] = "application/vnd.ms-excel";
      lookup[".xls"] = "application/vnd.ms-excel";
      lookup[".xlt"] = "application/vnd.ms-excel";
      lookup[".xlw"] = "application/vnd.ms-excel";
      lookup[".xof"] = "x-world/x-vrml";
      lookup[".xpm"] = "image/x-xpixmap";
      lookup[".xwd"] = "image/x-xwindowdump";
      lookup[".z"] = "application/x-compress";
      lookup[".zip"] = "application/zip";
      #endregion

      if (lookup.ContainsKey(extension.ToLower())) return lookup[extension.ToLower()].ToString();
      else return "application/octet-stream";
    }
    #endregion   
  }
}

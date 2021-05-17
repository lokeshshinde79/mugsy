using System;
using System.IO;
using System.Security.Cryptography;
using NovelProjects.AESPrivateKey;

namespace NovelProjects.Encryption
{
  /// <summary>
  /// A simple AES encryption/Decryption class
  /// </summary>
  public class SimpleAES
	{
		Byte[] Key, Vector;
		private ICryptoTransform EncryptorTransform, DecryptorTransform;
		private System.Text.UTF8Encoding UTFEncoder;

		/// <summary>
		/// Initializes the simple AES class.
		/// </summary>
		public SimpleAES()
		{
      PrivateKey key = new PrivateKey();
      this.Key = key.GetKey();
			this.Vector = key.GetVector();
			//This is our encryption method
			RijndaelManaged rm = new RijndaelManaged();

			//Create an encryptor and a decryptor using our encryption method, key, and vector.
			EncryptorTransform = rm.CreateEncryptor(this.Key, this.Vector);
			DecryptorTransform = rm.CreateDecryptor(this.Key, this.Vector);

			//Used to translate bytes to text and vice versa
			UTFEncoder = new System.Text.UTF8Encoding();
		}

    /// <summary>
    /// Initializes the simple AES class.
    /// </summary>
    public SimpleAES(Byte[] Vector)
    {
      PrivateKey key = new PrivateKey();
      this.Key = key.GetKey();
      this.Vector = Vector;
      //This is our encryption method
      RijndaelManaged rm = new RijndaelManaged();

      //Create an encryptor and a decryptor using our encryption method, key, and vector.
      EncryptorTransform = rm.CreateEncryptor(this.Key, this.Vector);
      DecryptorTransform = rm.CreateDecryptor(this.Key, this.Vector);

      //Used to translate bytes to text and vice versa
      UTFEncoder = new System.Text.UTF8Encoding();
    }

    /// <summary>
    /// Initializes the simple AES class.
    /// </summary>
    /// <param name="Key">The encryption key.</param>
    /// <param name="Vector">The IV.</param>
    public SimpleAES(Byte[] Key, Byte[] Vector)
    {
      this.Key = Key;
      this.Vector = Vector;
      //This is our encryption method
      RijndaelManaged rm = new RijndaelManaged();

      //Create an encryptor and a decryptor using our encryption method, key, and vector.
      EncryptorTransform = rm.CreateEncryptor(this.Key, this.Vector);
      DecryptorTransform = rm.CreateDecryptor(this.Key, this.Vector);

      //Used to translate bytes to text and vice versa
      UTFEncoder = new System.Text.UTF8Encoding();
    }

		/// <summary>
		/// Generates an encryption key.
		/// </summary>
		/// <returns>byte[] encryption key. Store it some place safe.</returns>
		static public byte[] GenerateEncryptionKey()
		{
			//Generate a Key.
			RijndaelManaged rm = new RijndaelManaged();
			rm.GenerateKey();
			return rm.Key;
		}

		/// <summary>
		/// Generates a unique encryption vector
		/// </summary>
		/// <returns></returns>
		static public byte[] GenerateEncryptionVector()
		{
			//Generate a Vector
			RijndaelManaged rm = new RijndaelManaged();
			rm.GenerateIV();
			return rm.IV;
		}

		/// <summary>
		/// Encrypt some text and return an encrypted byte array.
		/// </summary>
		/// <param name="TextValue">The Text to encrypt</param>
		/// <returns>byte[] of the encrypted value</returns>
		public byte[] Encrypt(string TextValue)
		{
			//Translates our text value into a byte array.
			Byte[] bytes = UTFEncoder.GetBytes(TextValue);

			//Used to stream the data in and out of the CryptoStream.
			MemoryStream memoryStream = new MemoryStream();

			/*
			 * We will have to write the unencrypted bytes to the stream,
			 * then read the encrypted result back from the stream.
			 */
			#region Write the decrypted value to the encryption stream
			CryptoStream cs = new CryptoStream(memoryStream, EncryptorTransform, CryptoStreamMode.Write);
			cs.Write(bytes,0,bytes.Length);
			cs.FlushFinalBlock();
			#endregion

			#region Read encrypted value back out of the stream
			memoryStream.Position=0;
			byte[] encrypted = new byte[memoryStream.Length];
			memoryStream.Read(encrypted,0,encrypted.Length);
			#endregion

			//Clean up.
			cs.Close();
			memoryStream.Close();

			return encrypted;
		}

		public string Decrypt(byte[] EncryptedValue)
		{
			#region Write the encrypted value to the decryption stream
			MemoryStream encryptedStream = new MemoryStream();
			CryptoStream decryptStream = new CryptoStream(encryptedStream, DecryptorTransform, CryptoStreamMode.Write);
			decryptStream.Write(EncryptedValue, 0, EncryptedValue.Length);
			decryptStream.FlushFinalBlock();
			#endregion

			#region Read the decrypted value from the stream.
			encryptedStream.Position=0;
			Byte[] decryptedBytes = new Byte[encryptedStream.Length];
			encryptedStream.Read(decryptedBytes,0,decryptedBytes.Length);
			encryptedStream.Close();
			#endregion
			
      return UTFEncoder.GetString(decryptedBytes);
		}
	}
}

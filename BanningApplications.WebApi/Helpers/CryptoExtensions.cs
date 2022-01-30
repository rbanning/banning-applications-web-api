using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BanningApplications.WebApi.Helpers
{
	public static class CryptoExtensions
    {
		private const int SALT_SIZE = 128 / 8;
		private const int KEYSIZE = 256;
		private const string DEFAULT_SALT = "bannapps";

		// This constant string is used as a "salt" value for the PasswordDeriveBytes function calls.
		// This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
		// 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
		private static readonly byte[] initVectorBytes = Encoding.ASCII.GetBytes("BpyAWhnp8NeL6NW4");

		private static string DefaultKey
		{
			get { return "Ghpp5AkyK5gfzysa9Yf7dakKie"; }
		}

		public static string Encrypt(this string text, string key = null)
		{
			if (string.IsNullOrEmpty(text)) { return string.Empty; }

			key = string.IsNullOrEmpty(key) ? DefaultKey : key;

			byte[] plainTextBytes = Encoding.UTF8.GetBytes(text);
			using (PasswordDeriveBytes password = new PasswordDeriveBytes(key, null))
			{
				byte[] keyBytes = password.GetBytes(KEYSIZE / 8);
				using (RijndaelManaged symmetricKey = new RijndaelManaged())
				{
					symmetricKey.Mode = CipherMode.CBC;
					using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes))
					{
						using (MemoryStream memoryStream = new MemoryStream())
						{
							using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
							{
								cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
								cryptoStream.FlushFinalBlock();
								byte[] cipherTextBytes = memoryStream.ToArray();
								return Convert.ToBase64String(cipherTextBytes);
							}
						}
					}
				}
			}
		}

		public static string Decrypt(this string text, string key = null)
		{
			if (string.IsNullOrEmpty(text)) { return string.Empty; }

			key = string.IsNullOrEmpty(key) ? DefaultKey : key;

			byte[] cipherTextBytes = Convert.FromBase64String(text);
			using (PasswordDeriveBytes password = new PasswordDeriveBytes(key, null))
			{
				byte[] keyBytes = password.GetBytes(KEYSIZE / 8);
				using (RijndaelManaged symmetricKey = new RijndaelManaged())
				{
					symmetricKey.Mode = CipherMode.CBC;
					using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes))
					{
						using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
						{
							using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
							{
								byte[] plainTextBytes = new byte[cipherTextBytes.Length];
								int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
								return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
							}
						}
					}
				}
			}

		}


		public static string Hash(this string text, string salt = DEFAULT_SALT)
		{
			// generate a 128-bit salt using a secure PRNG
			byte[] saltBytes = new byte[SALT_SIZE];
			saltBytes = Encoding.UTF8.GetBytes(CreateHashSalt(salt));


			// derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
			return Convert.ToBase64String(KeyDerivation.Pbkdf2(
				password: text,
				salt: saltBytes,
				prf: KeyDerivationPrf.HMACSHA1,
				iterationCount: 10000,
				numBytesRequested: 256 / 8));

		}

		public static string CreateHashSalt(string salt = null)
		{

			// generate a 128-bit salt using a secure PRNG
			byte[] saltBytes = new byte[SALT_SIZE];
			if (!string.IsNullOrEmpty(salt))
			{
				while (Encoding.Unicode.GetByteCount(salt) < SALT_SIZE)
				{
					salt += "a";	//pad with a character
				}
				saltBytes = Encoding.UTF8.GetBytes(salt);
			}
			else
			{
				using (var rng = RandomNumberGenerator.Create())
				{
					rng.GetBytes(saltBytes);
				}
			}

			return Convert.ToBase64String(saltBytes);

		}



		#region >>> SYMMETRIC Encryption <<<

		public static class SymmetricEncryption
		{
			public static (string key, string iv) InitSymmetricEncryptionKey_IV()
			{
				var key = GetEncodedRandomString(32); // 256
				Aes cipher = CreateCipher(key);
				var iv = Convert.ToBase64String(cipher.IV);
				return (key, iv);
			}

			public static string Encrypt(string text, string key, string iv)
			{
				Aes cipher = CreateCipher(key);
				cipher.IV = Convert.FromBase64String(iv);
 
				ICryptoTransform cryptTransform = cipher.CreateEncryptor();
				byte[] plaintext = Encoding.UTF8.GetBytes(text);
				byte[] cipherText = cryptTransform.TransformFinalBlock(plaintext, 0, plaintext.Length);
 
				return Convert.ToBase64String(cipherText);
			}

			public static string Decrypt(string encryptedText, string key, string iv)
			{
				Aes cipher = CreateCipher(key);
				cipher.IV = Convert.FromBase64String(iv);
 
				ICryptoTransform cryptTransform = cipher.CreateDecryptor();
				byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
				byte[] plainBytes = cryptTransform.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
 
				return Encoding.UTF8.GetString(plainBytes);
			}
 
			private static string GetEncodedRandomString(int length)
			{
				var base64 = Convert.ToBase64String(GenerateRandomBytes(length));
				return base64;
			}
 
			private static Aes CreateCipher(string keyBase64)
			{
				// Default values: key size 256, Padding PKC27
				Aes cipher = Aes.Create();
				cipher.Mode = CipherMode.CBC;  // Ensure the integrity of the cipher text if using CBC
 
				cipher.Padding = PaddingMode.ISO10126;
				cipher.Key = Convert.FromBase64String(keyBase64);
 
				return cipher;
			}
 
			private static byte[] GenerateRandomBytes(int length)
			{
				var byteArray = new byte[length];
				RandomNumberGenerator.Fill(byteArray);
				return byteArray;
			}
		}
		

		#endregion
	}
}

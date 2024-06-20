using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace SMGI.Common
{
    class RegInfo
    {
        byte[] pkey = { 213, 211, 193, 121, 86, 186, 115, 227, 182, 86, 11, 14, 129, 33, 112, 95, 163, 100, 65, 195, 17, 96, 1, 116, 213, 39, 148, 8, 203, 100, 30, 11, 115, 142, 206, 250, 143, 83, 172, 34, 118, 213, 47, 93, 233, 182, 8, 49, 55, 201, 191, 167, 181, 50, 42, 26, 127, 149, 95, 93, 20, 209, 50, 51, 152, 32, 101, 93, 141, 151, 44, 122, 131, 0, 117, 63, 95, 121, 173, 22, 242, 249, 99, 74, 42, 242, 130, 178, 91, 63, 61, 217, 197, 135, 241, 14, 158, 33, 111, 211, 226, 20, 118, 89, 101, 85, 55, 128, 203, 207, 114, 7, 34, 103, 6, 232, 9, 167, 106, 165, 240, 185, 47, 33, 46, 102, 165, 169 };
        internal byte[] RSADecrypt(byte[] DataToDecrypt)
        {
            try
            {
                byte[] decryptedData;
                //Create a new instance of RSACryptoServiceProvider.

                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Import the RSA Key information. This needs

                    //to include the private key information.
                    byte[] Exponent = { 1, 0, 1 };
                    //Create a new instance of RSAParameters.

                    RSAParameters RSAKeyInfo = new RSAParameters();

                    //Set RSAKeyInfo to the public key values. 

                    RSAKeyInfo.Modulus = pkey;
                    RSAKeyInfo.Exponent = Exponent;

                    RSA.ImportParameters(RSAKeyInfo);

                    //Decrypt the passed byte array and specify OAEP padding.  

                    //OAEP padding is only available on Microsoft Windows XP or

                    //later.  

                    decryptedData = RSA.Decrypt(DataToDecrypt,false);
                }
                return decryptedData;
            }
            //Catch and display a CryptographicException  

            //to the console.

            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }

        }
        
    }
    
}

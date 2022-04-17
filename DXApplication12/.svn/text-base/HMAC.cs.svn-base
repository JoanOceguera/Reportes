/********************PSeudocode*****************************************
 ***********************************************************************
 function hmac (key, message)
    if (length(key) > blocksize) then
        key = hash(key) // keys longer than blocksize are shortened
    end if
    if (length(key) < blocksize) then
        key = key ∥ [0x00 * (blocksize - length(key))] // keys shorter than blocksize are zero-padded ('∥' is concatenation) 
    end if
   
    o_key_pad = [0x5c * blocksize] ⊕ key // Where blocksize is that of the underlying hash function
    i_key_pad = [0x36 * blocksize] ⊕ key // Where ⊕ is exclusive or (XOR)
   
    return hash(o_key_pad ∥ hash(i_key_pad ∥ message)) // Where '∥' is concatenation
 end function
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace HMAC
{
    class HMACAlgorithm
    {
        private ushort blocklenght = 16;//longitud bloke en bytes

        private HashAlgorithm hash;

        public HashAlgorithm Hash
        {
            get { return hash; }
            set { hash = value; }
        }               
        public ushort Blocklenght
        {
            get { return blocklenght; }
            set { blocklenght = value; }
        }

        public HMACAlgorithm()
        {
            this.Hash = new SHA512Managed();
        }

        public byte[] HMAC(String key, String message)
        {
            byte[] keyb = Encoding.ASCII.GetBytes(key);
            byte[] messageb = Encoding.ASCII.GetBytes(message);
            
            if (keyb.Length > this.blocklenght)//si la longitud de la clave es mayor ke la longitud del bloke, recorto la llave a la longitud del bloke 128 b ke da como salida MD5
            {
                byte[] keyH = this.HASH_FromText(key);
                keyb = this.BytesComplete(keyH, 0x00, this.blocklenght - keyH.Length); //se lleva la llave a la longitud del bloke rellenando con 0
            }
            else
                if (keyb.Length < this.blocklenght)//si la longitud de la clave es menor ke la longitud del bloke, completo con 0x00
                    keyb = this.BytesComplete(keyb, 0x00, this.blocklenght - keyb.Length); //relleno la llave con 0 hasta llevarla al tamanno del bloke
            

            byte[] opad = this.XOR(this.BytesComplete(new byte[0] ,0x5c,this.blocklenght),keyb);
            byte[] ipad = this.XOR(this.BytesComplete(new byte[0], 0x36, this.blocklenght),keyb);

            
            return this.HASH_FromBytes(opad.Concat(this.HASH_FromBytes(ipad.Concat(messageb).ToArray())).ToArray()); //hash(o_key_pad ∥ hash(i_key_pad ∥ message))
        }

        public byte[] HASH_FromText(String text)
        {            
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(text);
            byte[] hashBytes = this.Hash.ComputeHash(inputBytes);

            return hashBytes;
        }
        public byte[] HASH_FromBytes(byte[] textByteArray)
        {
            byte[] hashBytes = this.Hash.ComputeHash(textByteArray.ToArray());

            return hashBytes;
        }
        

        public byte[] BytesComplete(byte[] toComplete, byte completer, int numberToComplete)
        {
            byte[] arrayToComplete = toComplete;
            for (int i = 0; i < numberToComplete; i++)
            {
                byte[] bconcat = new byte[1];
                bconcat[0] =  completer ;
                arrayToComplete = arrayToComplete.Concat(bconcat).ToArray();
            }
            return arrayToComplete;
        }

        public byte[] XOR(byte[] array1, byte[] array2)
        {
            byte[] arrtemp = new byte[array1.Length];

            for (int i = 0; i < array1.Length; i++)
            {
                arrtemp[i] = (byte)(array1[i] ^ array2[i]);
            }
            return arrtemp;
        }
        public static String ToHex(byte[] hmacresult)
        {
            String hmacreturn = String.Empty;
            for (int i = 0; i < hmacresult.Length; i++)
            {
                hmacreturn += Convert.ToString(hmacresult[i], 16);
            }
            return hmacreturn;
        }
    }
}

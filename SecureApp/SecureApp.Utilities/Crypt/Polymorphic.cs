using System;

namespace SecureAppUtil.Crypt
{    
    public class Polymorphic
    {
        public string Encrypt(string input, string key)
        {
            Random r = new Random();
            int kc = 0;
            char[] text = input.ToCharArray();
            char[] keyarr = key.ToCharArray();
            char[] finVal = new char[input.Length + 1];
            int rnd = r.Next(100, 220);
            for (int index = 0; index < input.Length; index++)
            {
                if (kc >= keyarr.Length)
                    kc = 0;

                int ptVal = text[index];
                int kVal = keyarr[kc];
                int ciVal = ptVal + kVal + rnd;
                finVal[index] = Convert.ToChar(ciVal);
                kc++;
            }
            finVal[input.Length] = (char)rnd;
            string retStr = new string(finVal);
            return retStr;

        }

        public string Decrypt(string input, string key)
        {
            char[] text = input.ToCharArray();
            char[] keyarr = key.ToCharArray();
            char[] finVal = new char[input.Length - 1];
            int rndKVal = text[input.Length - 1];
            text[input.Length - 1] = '\0';
            int kc = 0;
            for (int index = 0; index < input.Length; index++)
            {
                if (index >= input.Length - 1)
                    continue;
                if (kc >= keyarr.Length)
                    kc = 0;
                int ciVal = text[index];
                int kVal = keyarr[kc];
                int ptVal = ciVal - rndKVal - kVal;
                finVal[index] = Convert.ToChar(ptVal);
                kc++;
            }
            string retStr = new string(finVal);
            return retStr;
        }
    }
}
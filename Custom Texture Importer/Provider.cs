using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Custom_Texture_Importer
{
    public class Provider
    {
        public DefaultFileProvider _provider;

        public Provider()
        {
            _provider = new(FortniteUtil.PakPath, SearchOption.TopDirectoryOnly, false, new VersionContainer(EGame.GAME_UE5_LATEST));
            _provider.Initialize();

            var httpClient = new HttpClient();
            var req = httpClient.GetAsync("https://fortnite-api.com/v2/aes");
            req.Wait();
            if (req.Result.StatusCode == HttpStatusCode.OK)
            {
                var content = req.Result.Content.ReadAsStringAsync();
                content.Wait();
                var aes = JsonConvert.DeserializeObject<AES>(content.Result).Data;

                var keys = new List<KeyValuePair<FGuid, FAesKey>>();
                if (aes.MainKey != null)
                    keys.Add(new(new FGuid(), new FAesKey(aes.MainKey)));
                keys.AddRange(from x in aes.DynamicKeys
                              select new KeyValuePair<FGuid, FAesKey>(new FGuid(x.PakGuid), new FAesKey(x.Key)));
                _provider.SubmitKeys(keys);
                _provider.LoadMappings();
            }
            else
            {
                Console.WriteLine("ERR | AES REQUEST ENDED WITH STATUS CODE " + req.Result.StatusCode);
            }
        }
      

        public static bool SwapNormally(byte[] search, byte[] replace, ref byte[] array)
        {
            try
            {
                var arr = new List<byte>(array);
                var index = IndexOfSequence(array, search);

                if (replace.Length < search.Length)
                    Array.Resize(ref replace, search.Length);

                arr.RemoveRange(index, search.Length);
                arr.InsertRange(index, replace);
                array = arr.ToArray();
            }
            catch
            {
                return false;
            }

            return true;
        }

        //Originally: https://stackoverflow.com/a/332667/12897035
        public static int IndexOfSequence(byte[] buffer, byte[] pattern)
        {
            int i = Array.IndexOf(buffer, pattern[0], 0);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual(pattern))
                    return i;
                i = Array.IndexOf(buffer, pattern[0], i + 1);
            }

            return -1;
        }
    }
}

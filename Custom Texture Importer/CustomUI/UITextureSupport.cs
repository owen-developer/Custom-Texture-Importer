using CUE4Parse;
using CUE4Parse.FileProvider;
using Custom_Texture_Importer.Models;
using Custom_Texture_Importer.Utils.Libs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Custom_Texture_Importer.CustomUI
{
    public class UITextureSupport
    {
        private DefaultFileProvider Provider;

        public UITextureSupport(string customPaks)
        {
            Provider = new FileProvider(customPaks).Provider;
        }

        public byte[] PrepImport(string uasset) =>
            Provider.SaveAsset(uasset);
    }
}
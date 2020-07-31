using CSRDemo;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFShop
{
    public class FormINFO
    {
        public enum FormType
        {
            Simple, SimpleIMG, Custom, Model
        }
        public enum FormTag
        {
            Main, sellMain, recycleMain, preferenceMain, confirmSell, confirmRecycle, confirmedSell, confirmedRecycle
        }
        public FormINFO(string _playername, FormType _type, FormTag _tag)
        {
            playername = _playername;
            Type = _type;
            Tag = _tag; 
            playeruuid = Program.GetUUID(_playername);
 
        }
        public FormType Type;
        public FormTag Tag;
        public uint id;
        public string playeruuid, playername, title;
        public JToken content, domain;
        public JArray buttons,domain_source;
    }
}

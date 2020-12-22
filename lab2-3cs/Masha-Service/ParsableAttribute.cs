using System;

namespace M_Service
{
    public class ParsableAttribute : Attribute
    {
        public string AliasAlice { get; set; }

        public ParsableAttribute()
        {
            AliasAlice = "add";
        }

        public ParsableAttribute(string val)
        {
            AliasAlice = val;
        }
    }
}

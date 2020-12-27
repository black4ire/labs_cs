using System.Configuration;

using Config_Provider;
namespace STW_Service
{
    [Parsable("cryptoOptions")]
    class CryptoOptions
    {
        [Parsable("key")]
        public string Key { get; set; } = ConfigurationManager.AppSettings["CryptoKey"];
        [Parsable("initVector")]
        public string IV { get; set; } = ConfigurationManager.AppSettings["CryptoIV"];
        public CryptoOptions() { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace APBWatcher
{
    public class WatcherConfig
    {
        [YamlMember(Alias="apb_accounts")]
        public List<Dictionary<string, string>> ApbAccounts { get; set; }
        [YamlMember(Alias = "influx_host")]
        public string InfluxHost { get; set; }
        [YamlMember(Alias = "influx_username")]
        public string InfluxUsername { get; set; }
        [YamlMember(Alias = "influx_password")]
        public string InfluxPassword { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clock
{
    [Serializable]
    public class Alarm
    {
        public TimeSpan Time { get; set; }
        public string SoundPath { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "Будильник";

        public override string ToString()
            => $"{Time:h\\:mm} — {Name} {(Enabled ? "Enabled" : "Disabled")}";
    }
}

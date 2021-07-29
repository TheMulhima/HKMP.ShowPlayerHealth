using System;
using System.Collections.Generic;
using Modding;

namespace PlayerHealth
{
    [Serializable]
    public class Settings
    {
        public List<string> AllPlayers = new List<string>();
        public (string, string) DisplayedPlayers;
    }

    public class GlobalModSettings:ModSettings
    {
        public string Save_Data_To_File = "Alpha1";
        public string Load_Data_From_File = "Alpha2";
        public string Remove_Text = "Alpha0";
    }
}
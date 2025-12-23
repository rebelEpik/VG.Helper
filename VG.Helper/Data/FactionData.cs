using System.Text.Json.Serialization;

namespace VG.Helper.Data
{
    public class FactionData
    {
        [JsonPropertyName("f1")]
        public string Faction1 { get; set; }
        [JsonPropertyName("f2")]
        public string Faction2 { get; set; }
        [JsonPropertyName("reputation")]
        public int Reputation { get; set; }
        private static readonly Dictionary<string, string> FactionMap = new()
        {
            { "Red",             "Kolyatov Collective" },
            { "Blue",            "Stellar Industries" },
            { "Gold",            "Luminate Combine" },
            { "SalvageGuild",   "Steel Vultures" },
            { "BountyGuild",    "Orsanon Security" },
            { "IndustrialGuild", "Forge Industries" },
            { "MiningGuild", "Mindus Holdings" },
            {"TradingGuild", "Intertrade Network" },
            {"PoliceGuild", "Canisec" },
            {"Puppeteers" , "Your Employer" },
            {"Smugglers" , "Void Drifters" },
            {"Marauders", "Corsair Syndicate" },
            {"Darkspacers", "Darkspace Compact" }
        };
        public string Faction1Name => MapFaction(Faction1);
        public string Faction2Name => MapFaction(Faction2);
        private static string MapFaction(string factionId)
        {
            if (string.IsNullOrWhiteSpace(factionId))
                return factionId ?? string.Empty;

            return FactionMap.TryGetValue(factionId, out var name) ? name : factionId;
        }
        public override string ToString()
        {
            string factionName = Faction1Name == "Player" ? Faction2Name : Faction1Name;
            int adjustedReputation = Reputation;
            if (Reputation < 0)
            {
                int positiveValue = Math.Abs(Reputation);
                adjustedReputation = -(15000 - positiveValue);
            }
            return $"{factionName}: {adjustedReputation}";
        }
    }
}

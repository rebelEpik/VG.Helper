namespace VG.Helper.Data
{
    public class Commander
    {
        public required string firstName { get; set; }
        public required string lastName { get; set; }
        public string Name => $"{firstName} {lastName}";
        public required string callsign { get; set; }
        public required string icon { get; set; }
        public required int experience { get; set; }
        public required int level { get; set; }
        public required int bonusSkillPoints { get; set; }
        public string? credits { get; set; }
        public List<FactionData> reputation { get; set; } = new List<FactionData>();
    }
}

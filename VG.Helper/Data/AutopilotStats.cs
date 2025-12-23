namespace VG.Helper.Data
{
    public class AutopilotStats
    {
        public required string shipName { get; set; }
        public required double startTime { get; set; }
        public required double endTime { get; set; }
        public List<Stats> stats { get; set; } = new List<Stats>();
    }
}

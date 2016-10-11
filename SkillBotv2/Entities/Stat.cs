namespace SkillBotv2.Entities
{
    class Stat
    {
        public int Rank { get; set; }
        public int Level { get; set; }
        public long Exp { get; set; }

        public static Stat CreateFromCSV(string[] parts)
        {
            return new Stat
            {
                Rank = int.Parse(parts[0]),
                Level = int.Parse(parts[1]),
                Exp = long.Parse(parts[2])
            };
        }
    }
}
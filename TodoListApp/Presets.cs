namespace TodoListApp;

public record NamedStyle(string Name, DeadlineColorSettings Style);

public static class BuiltInPresets
{
    public static readonly IReadOnlyList<NamedStyle> All =
    [
        P("Classic", 14, false, "228B22", "8B0000"),
        P("Traffic Light", 14, true, "2ECC40", "FFDC00", "FF4136"),
        P("Sunset", 14, false, "FFD700", "FF8C00", "DC143C", "4B0082"),
        P("Blocky Grass", 14, true, "7CFC00", "5DBB2A", "3F8F1F", "8B7355", "5C4033"),
        P("Ocean", 14, false, "00CED1", "1E90FF", "00008B", "191970"),
        P("Lava", 10, false, "FFFF66", "FF9900", "FF3300", "8B0000", "1A0000"),
        P("Forest Fire", 14, true, "228B22", "9ACD32", "FFA500", "FF4500", "8B0000"),
        P("Candy", 14, false, "FFB6C1", "FF69B4", "DA70D6", "8A2BE2"),
        P("Ice", 14, false, "E0FFFF", "87CEFA", "4682B4", "191970"),
        P("Neon", 10, true, "39FF14", "FFFF00", "FF6EC7", "FF073A"),
        P("Autumn", 21, false, "9ACD32", "DAA520", "D2691E", "8B2500"),
        P("Spring", 14, false, "98FB98", "F0E68C", "FFB6C1"),
        P("Desert", 14, false, "F4A460", "CD853F", "A0522D", "5C3317"),
        P("Twilight", 14, false, "87CEEB", "9370DB", "483D8B", "0B0B3B"),
        P("Berry", 14, false, "6A5ACD", "9932CC", "C71585", "8B0000"),
        P("Mint Chocolate", 14, false, "98FF98", "3EB489", "7B3F00", "3B1F0B"),
        P("PICO-8 Heat", 7, true, "00E436", "FFEC27", "FFA300", "FF004D", "7E2553"),
        P("Game Boy", 14, true, "9BBC0F", "8BAC0F", "306230", "0F380F"),
        P("Monochrome", 14, false, "D3D3D3", "808080", "303030", "000000"),
        P("Sepia", 14, false, "F5DEB3", "D2B48C", "8B5A2B", "3E2723"),
        P("Rose Gold", 14, false, "FADADD", "E6A57E", "B76E79", "6D2E46"),
        P("Toxic", 10, false, "ADFF2F", "32CD32", "6A0DAD", "2E003E"),
        P("Cherry Blossom", 14, false, "FFF0F5", "FFB7C5", "FF69B4", "C71585"),
        P("Emerald", 14, false, "50C878", "009B77", "006400", "013220"),
        P("Sky", 14, false, "87CEFA", "00BFFF", "1E90FF", "0000CD"),
        P("Gold Rush", 14, false, "FFF8DC", "FFD700", "DAA520", "8B6508"),
        P("Magma", 14, false, "FCFDBF", "FEC287", "FB8761", "DE4968", "8C2981", "3B0F70"),
        P("Viridis", 21, false, "FDE725", "5EC962", "21908C", "3B528B", "440154"),
        P("Plasma", 14, false, "0D0887", "7E03A8", "CC4678", "F89441", "F0F921"),
        P("Inferno", 14, false, "FCFFA4", "F98C0A", "BB3754", "420A68", "000004"),
        P("Coral Reef", 14, false, "40E0D0", "FF7F50", "DC143C"),
        P("Pastel", 14, true, "B5EAD7", "E2F0CB", "FFDAC1", "FFB7B2", "FF9AA2"),
        P("Rainbow", 14, true, "0000FF", "00FF00", "FFFF00", "FFA500", "FF0000"),
        P("Storm", 14, false, "B0C4DE", "778899", "2F4F4F", "000000"),
        P("Swamp", 14, false, "6B8E23", "556B2F", "6B4423", "2E1A0E"),
        P("Jungle", 14, true, "00A86B", "228B22", "006400", "8B4513", "4B0000"),
        P("Blocky Sunset", 14, true, "FFE066", "FFA94D", "FF6B6B", "C2255C", "5F3DC4"),
        P("Blocky Ocean", 14, true, "66D9E8", "3BC9DB", "1C7ED6", "1864AB", "0B2545"),
        P("Blocky Fire", 14, true, "FFE066", "FFA500", "FF4500", "B22222", "4A0000"),
        P("Blocky Ice", 14, true, "E7F5FF", "A5D8FF", "4DABF7", "1C7ED6", "0B3D91"),
        P("Blocky Candy", 14, true, "FFD6E8", "FFA8D2", "FF6EB4", "D63384", "8E1B5C"),
        P("Blocky Sand", 14, true, "FFF3BF", "FFD43B", "E8A33D", "B8742A", "6F4518"),
        P("Blocky Night", 14, true, "5C7CFA", "4263EB", "3B5BDB", "1C1C5E", "0B0B2B"),
        P("Calm or Urgent", 7, true, "2B8A3E", "E03131"),
        P("Quick Week", 7, false, "40C057", "FAB005", "FA5252"),
        P("Month Planner", 30, false, "2F9E44", "94D82D", "FCC419", "FF922B", "F03E3E"),
        P("Mono Blue", 14, false, "DBE4FF", "91A7FF", "4C6EF5", "364FC7", "1B2A7A"),
        P("Mono Green", 14, false, "D3F9D8", "8CE99A", "40C057", "2B8A3E", "0B3D1A"),
        P("Mono Red", 14, false, "FFE3E3", "FFA8A8", "FA5252", "C92A2A", "5C0000"),
        P("Mono Purple", 14, false, "F3D9FA", "DA77F2", "AE3EC9", "862E9C", "3B0F4F"),
    ];

    private static NamedStyle P(string name, int days, bool blocky, params string[] hex)
    {
        var colors = hex.Select(h => Color.ParseColor("#" + h).ToArgb()).ToList();
        return new NamedStyle(name, new DeadlineColorSettings
            { Days = days, Colors = colors, Steps = blocky ? colors.Count : 0 });
    }
}
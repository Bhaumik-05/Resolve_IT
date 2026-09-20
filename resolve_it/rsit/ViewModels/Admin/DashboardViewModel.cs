using rsit.Repositories.Projections;

namespace rsit.ViewModels.Admin
{
    /// <summary>FR-20 admin dashboard.</summary>
    public class AdminDashboardViewModel
    {
        public DashboardData Data { get; set; } = new();

        public int TrendMax => Data.Trend.Count == 0 ? 0 : Data.Trend.Max(t => t.Count);
    }
}

namespace rsit.ViewModels.Admin
{
    /// <summary>Model for the _BarList partial.</summary>
    public class BarListViewModel
    {
        public BarListViewModel(List<LabelCount> items, string tone = "")
        {
            Items = items;
            Tone = tone;
        }

        public List<LabelCount> Items { get; }

        /// <summary>"", "blue", "amber", "purple" or "navy" (CSS bar-* class).</summary>
        public string Tone { get; }
    }
}

using System;
using System.Collections.Generic;

namespace ClassroomManagement.Models
{
    public class Poll
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public List<PollOption> Options { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class PollOption
    {
        public int Id { get; set; } // 1, 2, 3... relative to Poll
        public string Text { get; set; } = string.Empty;
    }

    public class PollVote
    {
        public int PollId { get; set; }
        public int OptionId { get; set; }
        public string StudentMachineId { get; set; } = string.Empty;
    }

    public class PollResultUpdate
    {
        public int PollId { get; set; }
        public Dictionary<int, int> VoteCounts { get; set; } = new(); // OptionId -> Count
        public int TotalVotes { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace ClassroomManagement.Models
{
    public class AssignmentSubmission
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SessionId { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string Note { get; set; }
        public DateTime SubmittedAt { get; set; }
        public List<SubmittedFile> Files { get; set; } = new List<SubmittedFile>();
    }

    public class SubmittedFile
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string LocalPath { get; set; } // Path on teacher's machine
        public byte[]? Data { get; set; } // Content for transfer (not saved to DB)
    }
}

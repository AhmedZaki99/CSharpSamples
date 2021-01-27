using System;

namespace AsyncLibrary
{
    public class ProgressReport
    {
        public float Percentage { get; set; } = 0;
        public string FileName { get; set; }

        public ProgressReport(float percentage, string fileName) => (Percentage, FileName) = (percentage, fileName);
    }
}

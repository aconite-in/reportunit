﻿namespace ReportUnit.Layer
{
    using System.Collections.Generic;

    /// <summary>
    /// Information for a test fixture
    /// </summary>
    internal class TestSuite
    {
        public TestSuite()
        {
            Tests = new List<Test>();
            this.Status = Status.Unknown;
        }

        public string Name { get; set; }

        public Status Status { get; set; }

		public string failedCount { get; set; } 

		public string Set { get; set; }

        public string FeatureOwner { get; set; }

        /// <summary>
        /// Error messages, description, etc
        /// </summary>
        public string StatusMessage { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        /// <summary>
        /// How long the test fixture took to run (in milliseconds)
        /// </summary>
        public double Duration { get; set; }

        public List<Test> Tests { get; private set; }
    }
}

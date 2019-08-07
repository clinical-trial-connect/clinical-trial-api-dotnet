using System.Collections.Generic;

namespace TestingGraphql
{
    public class ClinicalTrialFilter : ClinicalFilter
    {
        public List<string> Conditions { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int TravelRadius { get; set; }
        public string Gender { get; set; }
        public int FromAge { get; set; }
        public int ToAge { get; set; }
        public string Phase { get; set; }
        public string StudyType { get; set; }
        public string ZipCode { get; set; }
    }
}

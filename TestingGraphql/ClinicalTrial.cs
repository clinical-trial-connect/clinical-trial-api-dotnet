using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestingGraphql
{
    public class ClinicalTrial
    {
        public string Cursor { get; set; }
        public string Title { get; set; }
        public List<string> Conditions { get; set; }
        public string Phase { get; set; }
        public string Gender { get; set; }
        public int MaximumAge { get; set; }
        public int MinimumAge { get; set; }
        public string Description { get; set; }
        public string Criteria { get; set; }
        public string AgencyClass { get; set; }
        public string DetailedDescription { get; set; }
        public string HasExpandedAccess { get; set; }
        public string HealthyVolunteers { get; set; }
        public string IsFdaRegulated { get; set; }
        public List<string> Keywords { get; set; }
        public string LinkDescription { get; set; }
        public string LinkUrl { get; set; }
        public string NctId { get; set; }
        public string OfficialTitle { get; set; }
        public string OverallContactEmail { get; set; }
        public string OverallContactName { get; set; }
        public string OverallContactPhone { get; set; }
        public string OverallOfficialAffilitation { get; set; }
        public string OverallOfficialName { get; set; }
        public string OverallOfficialRole { get; set; }
        public string OverallStatus { get; set; }
        public string Sponsor { get; set; }
        public string StudyType { get; set; }
        public List<ClinicalTrialSite> Sites { get; set; }
    }
}

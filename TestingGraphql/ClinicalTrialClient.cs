using GraphQL.Client;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestingGraphql
{
    public class ClinicalTrialClient
    {
        private readonly string TrialObject = "trials";
        private readonly string DefaultTrialProperties = "agencyClass conditions countries criteria description detailedDescription gender hasExpandedAccess healthyVolunteers isFdaRegulated keywords linkDescription linkUrl maximumAge minimumAge nctId officialTitle overallContactEmail overallContactName overallContactPhone overallOfficialAffilitation overallOfficialName overallOfficialRole overallStatus phase sites{ city contactEmail contactName contactPhone contactPhoneExt country facility latitude longitude state status url zipCode } sponsor studyType title";

        private readonly string SponsorObject = "sponsors";
        private readonly string DefaultSponsorProperties = "name";

        private readonly string ConditionsObject = "conditions:__type(name: \"Condition\")";
        private readonly string DefaultConditionsProperties = "name";

        private readonly string DefaultPageInfo = "pageInfo { endCursor hasNextPage hasPreviousPage startCursor }";

        private GraphQLClient _Client;

        public ClinicalTrialClient(string endpoint, string token)
        {
            _Client = new GraphQLClient(endpoint);
            _Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        public async Task<List<ClinicalCondition>> GetClinicalConditions(string search)
        {
            GraphQLRequest graphQLRequest = GetGraphQLRequest(GetQuery(ConditionsObject, DefaultConditionsProperties, true));
            var result = await _Client.PostAsync(graphQLRequest);
            return MapResultToConditionObject(result, search);
        }

        private List<ClinicalCondition> MapResultToConditionObject(GraphQLResponse result, string search)
        {
            List<ClinicalCondition> conditions = new List<ClinicalCondition>();
            foreach (var edge in result.Data.conditions.enumValues)
            {
                if (((string)edge.name).ToLower().Contains(search.ToLower()))
                {
                    conditions.Add(new ClinicalCondition()
                    {
                        Name = edge.name,
                        Description = ((string)edge.name).Replace("_", " ")
                    });
                }
            }
            return conditions;
        }

        public async Task<ClinicalTrialResult<ClinicalSponsor>> GetClinicalSponsors(ClinicalFilter filter)
        {
            GraphQLRequest graphQLRequest = GetGraphQLRequest(GetQuery(SponsorObject, DefaultSponsorProperties, false, filter.first, filter.after));
            var result = await _Client.PostAsync(graphQLRequest);
            ClinicalTrialResult<ClinicalSponsor> clinicalTrialResult = GetClinicalSponsorResultInstance(result);

            MapResultToClinicalSponsorObject(result, clinicalTrialResult);

            return clinicalTrialResult;
        }

        private void MapResultToClinicalSponsorObject(GraphQL.Common.Response.GraphQLResponse result, ClinicalTrialResult<ClinicalSponsor> clinicalTrialResult)
        {
            foreach (var edge in result.Data.sponsors.edges)
            {
                clinicalTrialResult.Items.Add(new ClinicalSponsor() { Name = edge.node.name });
            }
        }

        public async Task<ClinicalTrialResult<ClinicalTrial>> GetClinicalTrials(ClinicalTrialFilter filter)
        {
            GraphQLRequest graphQLRequest = GetGraphQLRequest(GetQueryForClinicalTrial(filter));
            var result = await _Client.PostAsync(graphQLRequest);
            ClinicalTrialResult<ClinicalTrial> clinicalTrialResult = GetClinicalTrialResultInstance(result);

            MapResultToClinicalTrialObject(result, clinicalTrialResult);

            return clinicalTrialResult;
        }

        public string GetQueryForClinicalTrial(ClinicalTrialFilter filter)
        {
            string query = GetQuery(TrialObject, DefaultTrialProperties, false, filter.first, filter.after);
            if (filter.Conditions != null && filter.Conditions.Any())
            {
                bool hasPagination = filter.first > 0 || !string.IsNullOrEmpty(filter.after);
                string conditions = string.Join(",", filter.Conditions);
                string endOfLine = hasPagination ? "," : ")";
                string baseFilters = null;
                if (filter.Longitude != null)
                {
                    string latitude = AddFilterToBase("", "latitude", filter.Latitude.ToString(), true);
                    string longitude = AddFilterToBase("", "latitude", filter.Latitude.ToString(), true);
                    string coordinates = " coordinates: {" + latitude + ", " + longitude + "}";
                    baseFilters = coordinates;
                }
                baseFilters = AddFilterToBase(baseFilters, "studyType", filter.StudyType, true);
                baseFilters = AddFilterToBase(baseFilters, "zipCode", filter.ZipCode);
                baseFilters = AddFilterToBase(baseFilters, "phase", filter.Phase, true);

                if (filter.FromAge > 0)
                {
                    if (filter.ToAge > 0)
                    {
                        baseFilters = AddFilterToBase(baseFilters, "age", filter.FromAge.ToString() + ":" + filter.ToAge.ToString());
                    }
                    else
                    {
                        baseFilters = AddFilterToBase(baseFilters, "age", filter.FromAge.ToString(), true);
                    }
                }

                baseFilters = AddFilterToBase(baseFilters, "gender", filter.Gender, true);
                baseFilters = AddFilterToBase(baseFilters, "travelRadius", filter.TravelRadius > 0 ? filter.TravelRadius.ToString() : "", true);
                string baseMatches = "baseMatches(conditions: [" + conditions + "], baseFilters: { " + baseFilters + " }" + endOfLine + "";
                string startOfLine = hasPagination ? "(" : "";
                query = query.Replace(TrialObject + startOfLine, baseMatches);
            }
            return query;
        }

        private string AddFilterToBase(string currentFilter, string filterName, string filterValue, bool isEnum = false)
        {
            if (string.IsNullOrEmpty(filterValue))
                return currentFilter;

            string startOfLine = string.IsNullOrEmpty(currentFilter) ? "" : ", ";
            filterValue = isEnum ? filterValue : "\"" + filterValue + "\"";
            return currentFilter + startOfLine + filterName + ": " + filterValue;
        }

        private void MapResultToClinicalTrialObject(GraphQL.Common.Response.GraphQLResponse result, ClinicalTrialResult<ClinicalTrial> clinicalTrialResult)
        {
            var data = result.Data.trials != null ? result.Data.trials : result.Data.baseMatches;
            foreach (var edge in data.edges)
            {
                var clinicalTrial = new ClinicalTrial()
                {
                    Cursor = edge.cursor,
                    Title = edge.node.title,
                    Phase = edge.node.phase,
                    Gender = edge.node.gender,
                    MaximumAge = edge.node.maximumAge,
                    MinimumAge = edge.node.minimumAge,
                    Description = edge.node.description,
                    Criteria = edge.node.criteria,
                    Conditions = GetListFromTextArray(edge.node.conditions.Value),
                    AgencyClass = edge.node.agencyClass,
                    DetailedDescription = edge.node.detailedDescription,
                    HasExpandedAccess = edge.node.hasExpandedAccess,
                    HealthyVolunteers = edge.node.healthyVolunteers,
                    IsFdaRegulated = edge.node.isFdaRegulated,
                    Keywords = GetListFromTextArray(edge.node.keywords.Value),
                    LinkDescription = edge.node.linkDescription,
                    LinkUrl = edge.node.linkUrl,
                    NctId = edge.node.nctId,
                    OfficialTitle = edge.node.officialTitle,
                    OverallContactEmail = edge.node.overallContactEmail,
                    OverallContactName = edge.node.overallContactName,
                    OverallContactPhone = edge.node.overallContactPhone,
                    OverallOfficialAffilitation = edge.node.overallOfficialAffilitation,
                    OverallOfficialName = edge.node.overallOfficialName,
                    OverallOfficialRole = edge.node.overallOfficialRole,
                    OverallStatus = edge.node.overallStatus,
                    Sponsor = edge.node.sponsor,
                    StudyType = edge.node.studyType,
                    Sites = new List<ClinicalTrialSite>()
                };

                int i = 0;
                foreach (var site in edge.node.sites)
                {
                    clinicalTrial.Sites.Add(new ClinicalTrialSite()
                    {
                        Key = i,
                        City = site.city,
                        ContactEmail = site.contactEmail,
                        ContactName = site.contactName,
                        ContactPhone = site.contactPhone,
                        ContactPhoneExt = site.contactPhoneExt,
                        Country = site.country,
                        Facility = site.facility,
                        Latitude = site.latitude,
                        Longitude = site.longitude,
                        State = site.state,
                        Status = site.status,
                        Url = site.url,
                        ZipCode = site.zipCode,
                    });

                    i++;
                }

                clinicalTrialResult.Items.Add(clinicalTrial);
            }
        }

        private List<string> GetListFromTextArray(string textArray)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(textArray).OfType<string>().ToList();
        }

        private static ClinicalTrialResult<ClinicalSponsor> GetClinicalSponsorResultInstance(GraphQL.Common.Response.GraphQLResponse result)
        {
            return new ClinicalTrialResult<ClinicalSponsor>()
            {
                HasNextPage = result.Data.sponsors.pageInfo.hasNextPage,
                HasPreviousPage = result.Data.sponsors.pageInfo.hasPreviousPage,
                EndCursor = result.Data.sponsors.pageInfo.endCursor,
                StartCursor = result.Data.sponsors.pageInfo.startCursor,
                TotalCount = result.Data.sponsors.totalCount,
                Items = new List<ClinicalSponsor>()
            };
        }

        private static ClinicalTrialResult<ClinicalTrial> GetClinicalTrialResultInstance(GraphQL.Common.Response.GraphQLResponse result)
        {
            var data = result.Data.trials != null ? result.Data.trials : result.Data.baseMatches;
            return new ClinicalTrialResult<ClinicalTrial>()
            {
                HasNextPage = data.pageInfo.hasNextPage,
                HasPreviousPage = data.pageInfo.hasPreviousPage,
                EndCursor = data.pageInfo.endCursor,
                StartCursor = data.pageInfo.startCursor,
                TotalCount = data.totalCount,
                Items = new List<ClinicalTrial>()
            };
        }

        private string GetQuery(string objetName, string properties, bool isEnum = false, int first = 0, string after = null)
        {

            if (isEnum)
            {
                return "{ " + objetName + " { enumValues { name } } }";
            }
            else
            {
                string paginateText = "";
                if (first > 0)
                {
                    string afterText = string.IsNullOrEmpty(after) ? "" : ", after: \"" + after + "\"";
                    paginateText = "(first: " + first + afterText + ")";
                }

                return "{ " + objetName + paginateText + " { " + DefaultPageInfo + " totalCount edges { cursor node { " + properties + " } } } }";
            }
        }

        private GraphQLRequest GetGraphQLRequest(string query)
        {
            return new GraphQLRequest
            {
                Query = query
            };
        }
    }
}

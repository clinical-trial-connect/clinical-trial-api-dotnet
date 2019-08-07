using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Client;
using GraphQL.Common.Request;
using Microsoft.AspNetCore.Mvc;

namespace TestingGraphql.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        async public Task<dynamic> Get(string title)
        {
            var client = new ClinicalTrialClient("https://clinicaltrialconnect.dev/graphql", "fC4tJvGeePYsyRs3VnWU8Evd");

            return await client.GetClinicalTrials(new ClinicalTrialFilter()
            {
                Conditions = new List<string>() {
                    "ABDOMINAL_CANCER",
                    "ABDOMINAL_INJURY",
                    "ABDOMINAL_NEOPLASMS",
                    "GLIOBLASTOMA"
                },
                ZipCode = "10018",
                TravelRadius = 1,
                StudyType = "INTERVENTIONAL"
            });
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

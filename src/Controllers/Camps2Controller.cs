using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    public class Camps2Controller : ControllerBase
    {
        private readonly ICampRepository _campRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        // after creating a CampProfile.cs for the mapping add the mapper to this class through Dependency Injection
        public Camps2Controller(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _campRepository = campRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator; // generates URI
        }


        /*
         * public async Task<IActionResult> Get()
         * You can also specify the return type of the method. It will automatically return a 200 when the return type is correct
         */
        [HttpGet] // get on the same route api/[controller]
        public async Task<IActionResult> Get(bool includeTalks = false)
        {
            try
            {
                var results = await _campRepository.GetAllCampsAsync(includeTalks);
                var result = new
                {
                    Count = results.Count(),
                    Results = _mapper.Map<CampModel[]>(results)
                };

                return Ok(result);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        /*
         * Get an individual Camp specified by the surrogate key
         */
        [HttpGet("{moniker}")] // everything inside this get is an extension of the route api/[controller]/{moniker}
        public async Task<ActionResult<CampModel>> GetByMoniker(string moniker)
        {
            try
            {
                var result = await _campRepository.GetCampAsync(moniker);

                if (result == null) return NotFound("Item not found.");

                return _mapper.Map<CampModel>(result);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

        }


        /*
         * You can make multiple search with different search parameters
         */
        [HttpGet("search")] // api/[controller]/search?theDate=...&inlcudeTalks=true
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await _campRepository.GetAllCampsByEventDate(theDate, includeTalks);

                if (!results.Any()) return NotFound();

                return _mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

        }


        /*
         * You can use the attribute [FromBody] to let the controller now the data is going to be send from a body (json post)
         * Or you can add the attribute [apicontroller] Above the class. This is called modelbinding. Information that isn't a property inside the model class will be lost.
         */
        [HttpPost()] // POST on api/[controller]
        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                var existingCamp = await _campRepository.GetCampAsync(model.Moniker); // You can put a check inside the database to avoid duplicates but it is also good to check in the API

                if (existingCamp != null)
                {
                    return BadRequest("Moniker already exists.");
                }

                // Generate the URI
                var locationURI = _linkGenerator.GetPathByAction("GetByMoniker", // refers to the action get on line 58, where we get a camp by moniker
                    "Camps", // refers to the controller name
                    new { moniker = model.Moniker }); // new object of any route values, you can include more than one inputs

                if (string.IsNullOrEmpty(locationURI))
                {
                    return BadRequest("Could not use current moniker");
                }

                // Create a new camp
                var camp = _mapper.Map<Camp>(model);
                _campRepository.Add(camp);
                if (await _campRepository.SaveChangesAsync())
                {
                    return Created(locationURI, _mapper.Map<CampModel>(camp)); // include the location of the new created object
                }

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
            return BadRequest();
        }


        [HttpPut("{moniker}")] // api/[controller]/{moniker} identical to the get but uses put
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel model)
        {
            try
            {
                // Get old object before applying the changes
                var oldCamp = await _campRepository.GetCampAsync(moniker);
                if (oldCamp == null)
                {
                    NotFound($"Could not find camp with moniker: {moniker}");
                }

                _mapper.Map(model, oldCamp); // sends model data to oldCamp data

                if (await _campRepository.SaveChangesAsync())
                {
                    return _mapper.Map<CampModel>(oldCamp); // old camp is updated and return model back
                }

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest("Can't update item because no changes"); // if you try to update twice without changes it will give a bad request because if statement returns false
        }


        [HttpDelete("{moniker}")] // api/[controller]/{moniker} identical to the get but uses delete
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                // Get old object before applying the changes
                var oldCamp = await _campRepository.GetCampAsync(moniker);
                if (oldCamp == null)
                {
                    return NotFound($"Could not find camp with moniker: {moniker}");
                }

                _campRepository.Delete(oldCamp); // business rules: delete if there are no talkers etc./ delete talkers first 

                if (await _campRepository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest("Failed to delete");
        }

    }
}

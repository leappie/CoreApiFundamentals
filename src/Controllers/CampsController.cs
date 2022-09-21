using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _campRepository;
        private readonly IMapper _mapper;

        // after creating a CampProfile.cs for the mapping add the mapper to this class through Dependency Injection
        public CampsController(ICampRepository campRepository, IMapper mapper) 
        {
            _campRepository = campRepository;
            _mapper = mapper;
        }



        /*
         * public async Task<IActionResult> Get()
         * You can also specify the return type of the method. It will automatically return a 200 when the return type is correct
         * 
         */
        [HttpGet] // get on the same route api/[controller]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            try
            {
                var results = await _campRepository.GetAllCampsAsync(includeTalks);

                // Map the results to the desired model
                CampModel[] models = _mapper.Map<CampModel[]>(results);
                return Ok(models);
            }
            catch(Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        /// <summary>
        /// Get an individual Camp specified by the surrogate key
        /// </summary>
        /// <param name="moniker"></param>
        /// <returns></returns>
        [HttpGet("{moniker}")] // everything inside this get is an extension of the route api/[controller]/{moniker}
        public async Task<ActionResult<CampModel>> Get(string moniker)
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

    }
}

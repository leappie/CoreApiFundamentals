using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Migrations;
using CoreCodeCamp.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {

        private readonly ICampRepository _campRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CampsController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _campRepository = campRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        
        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false) // instead of IActionResult you can use ActionResult with the return type. In this case you dont need to return Ok(..)
        {
            try 
            {
                var results = await _campRepository.GetAllCampsAsync(includeTalks);
                CampModel[] models = _mapper.Map<CampModel[]>(results);

                //return Ok(models);
                return models;

            } catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        // GET api/<CampsController>/ATL2018
        [HttpGet("{moniker}")]
        public async Task<IActionResult> Get(string moniker)
        {
            try
            {
                var result = await _campRepository.GetCampAsync(moniker);

                if (result == null)
                {
                    return NotFound();
                }

                CampModel model = _mapper.Map<CampModel>(result); // Mapper maps a single object to another single object and an array or list to another array or list

                return Ok(model);
            } 
            catch
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");

            }
        }

        // ../api/[controller]/search?date=2018-10-18
        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime date, bool includeTalks = false) // de query parameters worden bepaald door de method parameters
        {
            try
            {
                var result = await _campRepository.GetAllCampsByEventDate(date, includeTalks); 

                if (!result.Any())
                {
                    return NotFound();
                }

                CampModel[] campModel = _mapper.Map<CampModel[]>(result); // Mapper maps a single object to another single object and an array or list to another array or list

                return Ok(campModel);
            }
            catch
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");

            }

        }

        // POST api/<CampsController>
        [HttpPost]
        public async Task<ActionResult<CampModel>> Post(CampModel model) // model binding if the post data is not in the model it is not accepted
        {
            try
            {
                var location = _linkGenerator.GetPathByAction("Get", "Camps", new { moniker = model.Moniker }); // generate link for moniker
                
                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker");
                }

                var camp = _mapper.Map<Camp>(model);
                _campRepository.Add<Camp>(camp);
                Console.WriteLine(camp);

                if(await _campRepository.SaveChangesAsync())
                {
                    Console.WriteLine("Inside await");
                    return Created($"/api/camps/{camp.Moniker}", _mapper.Map<CampModel>(camp));
                }
            }
            catch
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");

            }

            return BadRequest();
        }

        // PUT api/<CampsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CampsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

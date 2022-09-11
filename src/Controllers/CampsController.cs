using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Migrations;
using CoreCodeCamp.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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

        public CampsController(ICampRepository campRepository, IMapper mapper)
        {
            _campRepository = campRepository;
            _mapper = mapper;
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

        // POST api/<CampsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
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

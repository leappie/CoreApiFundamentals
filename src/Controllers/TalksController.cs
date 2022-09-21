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
    [Route("api/camps/{moniker}/talks")]// it is related to another route
    [ApiController]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _campRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public TalksController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this._campRepository = campRepository;
            this._mapper = mapper;
            this._linkGenerator = linkGenerator;
        }


        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> GetAllTalks(string moniker) // note the moniker is already part of the route
        {
            try
            {
                // Check if moniker exits
                var existingCamp = await _campRepository.GetCampAsync(moniker);

                if (existingCamp == null) return NotFound("Moniker not found");

                var talks = await _campRepository.GetTalksByMonikerAsync(moniker);
                return _mapper.Map<TalkModel[]>(talks);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }


        [HttpGet("{id:int}")] // if id is no int 404 error automatically
        public async Task<ActionResult<TalkModel>> GetIndividualTalk(string moniker, int id) // note the moniker is already part of the route
        {
            try
            {
                // Check if moniker exits
                var existingCamp = await _campRepository.GetCampAsync(moniker);

                if (existingCamp == null) return NotFound("Camp does not exist");

                var talk = await _campRepository.GetTalkByMonikerAsync(moniker, id, true);
                return _mapper.Map<TalkModel>(talk); // if id not found 204 error no content
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }


        [HttpPost]
        public async Task<ActionResult<TalkModel>> PostTalk(string moniker, TalkModel model) // note the moniker is already part of the route
        {
            try
            {
                // Check if moniker exits
                var existingCamp = await _campRepository.GetCampAsync(moniker);
                if (existingCamp == null) return BadRequest("Camp does not exist");

                // Get talk from model
                var talk = _mapper.Map<Talk>(model);
                talk.Camp = existingCamp;

                if (model.Speaker == null) return BadRequest("Speaker ID is required");
                var speaker = await _campRepository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null) return BadRequest("Speaker could not be found");
                talk.Speaker = speaker;

                _campRepository.Add(talk);

                // Get created URI
                var locationURI = _linkGenerator.GetPathByAction(
                    "GetIndividualTalk",
                    "Talks",
                    new { moniker = moniker, id = talk.TalkId });

                if (await _campRepository.SaveChangesAsync())
                {
                    return Created(locationURI, _mapper.Map<TalkModel>(talk));
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest("Error creating talk");
        }

    }
}

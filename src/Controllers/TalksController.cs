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
                if (existingCamp == null) return NotFound("Camp does not exist");

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

                var talk = await _campRepository.GetTalkByMonikerAsync(moniker, id, true); // true -> get speakers too
                if (talk == null) return NotFound("Talk not found");
                return _mapper.Map<TalkModel>(talk); // if id not found 204 error no content
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }


        [HttpPost] // you are posting to the collection of talks so you dont need an id here
        public async Task<ActionResult<TalkModel>> PostTalk(string moniker, TalkModel model) // note the moniker is already part of the route
        {
            try
            {
                // Get Camp
                var existingCamp = await _campRepository.GetCampAsync(moniker);
                if (existingCamp == null) return BadRequest("Camp does not exist");

                // Get talk from model
                var talk = _mapper.Map<Talk>(model);
                talk.Camp = existingCamp; // camp must exist before posting something to it

                if (model.Speaker == null) return BadRequest("Speaker ID is required");
                var speaker = await _campRepository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null) return BadRequest("Speaker could not be found");
                talk.Speaker = speaker;

                _campRepository.Add(talk);

                if (await _campRepository.SaveChangesAsync())
                {
                    // Get created URI
                    var locationURI = _linkGenerator.GetPathByAction( // inside if because TalkId is updated by now
                        "GetIndividualTalk",
                        "Talks",
                        new { moniker = moniker, id = talk.TalkId });

                    return Created(locationURI, _mapper.Map<TalkModel>(talk));
                }
                else
                {
                    return BadRequest("Error creating talk");

                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        // TODO
        [HttpPut("{id:int}")] // update on "api/camps/{moniker}/talks/{id}"
        public async Task<ActionResult<TalkModel>> UpdateTalk(string moniker, int id, TalkModel model)
        {
            try
            {
                // Check if talk exits
                var existingTalk = await _campRepository.GetTalkByMonikerAsync(moniker, id, true);
                if (existingTalk == null) return NotFound("Talk not found");
                _mapper.Map(model, existingTalk); // map your changes from talkmodel to talk 

                if (model.Speaker != null) // update the speaker if it is included
                {
                    var speaker = await _campRepository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null)
                    {
                        existingTalk.Speaker = speaker;
                    }
                }

                if (await _campRepository.SaveChangesAsync()) // Throws exception when TalkID is not included.Because we have defined TalkID in TalkModel. It wil asign TalkID 0 to existingTalk. You can avoid this by ignorint this mapping or removing TalkID from TalkModel
                {
                    return _mapper.Map<TalkModel>(existingTalk);
                }
                else
                {
                    return BadRequest("Failed update");
                }
                throw new NotImplementedException();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult<TalkModel>> DeleteTalk(string moniker, int id)
        {
            try
            {
                // Check if talk exits 
                var existingTalk = await _campRepository.GetTalkByMonikerAsync(moniker, id);
                if (existingTalk == null) return BadRequest("Talk not found");

                _campRepository.Delete(existingTalk);

                if (await _campRepository.SaveChangesAsync())
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Delete failed");
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VideoPlayer.Models;

namespace VideoPlayer.Controllers
{
    public class VideoController : Controller
    {
        public VideoController()
        {
            _pool = BotPool.Instance;
        }
        private readonly BotPool _pool;
        private readonly string[] Whitelist = { "mdalstrom", "untovvn" };

        [Route("Video/{name}")]
        public ActionResult Index(string name)
        {
            _pool.TryAdd(name);
            return View();
        }

        [Route("Video/All/{name}")]
        public ActionResult All(string name)
        {
            _pool.TryAdd(name);
            return new ObjectResult(new JObject(_pool.GetBot(name).VideoRequests.ToArray()));
        }
    }
}
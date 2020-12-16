using ProjectArt.MVCPattern.Attributes;
using Controller = ProjectArt.MVCPattern.Controller;
using IActionResult = ProjectArt.MVCPattern.IActionResult;

namespace BRD_Sport_Sem.Controllers
{
    [Controller("Disciplines")]
    public class DisciplinesController : Controller
    {
        // GET
        [Action("~/DisciplinesList", Method = MethodType.GET)]
        public IActionResult Index()
        {
            return View("DisciplinesList");
        }
        [Action("~/DisciplinesList/AlcoChess", Method = MethodType.GET)]
        public IActionResult GetAlcoChess()
        {
            return View("AlcoChess");
        }
        [Action("~/DisciplinesList/BeerMarathon", Method = MethodType.GET)]
        public IActionResult GetBeerMarathon()
        {
            return View("BeerMarathon");
        }
        [Action("~/DisciplinesList/BeerMile", Method = MethodType.GET)]
        public IActionResult GetBeerMile()
        {
            return View("BeerMile");
        }
        [Action("~/DisciplinesList/BeerPong", Method = MethodType.GET)]
        public IActionResult GetBeerPong()
        {
            return View("BeerPong");
        }
        [Action("~/DisciplinesList/HundredLiters", Method = MethodType.GET)]
        public IActionResult GetHundredLiters()
        {
            return View("HundredLiters");
        }
        [Action("~/DisciplinesList/Literball", Method = MethodType.GET)]
        public IActionResult GetLiterball()
        {
            return View("Literball");
        }
        [Action("~/DisciplinesList/WomenGlue", Method = MethodType.GET)]
        public IActionResult GetWomenGlue()
        {
            return View("WomenGlue");
        }
    }
}

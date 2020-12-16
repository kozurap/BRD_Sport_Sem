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
    }
}

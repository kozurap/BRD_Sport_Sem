using ProjectArt.MVCPattern.Attributes;
using Controller = ProjectArt.MVCPattern.Controller;
using IActionResult = ProjectArt.MVCPattern.IActionResult;

namespace BRD_Sport_Sem.Controllers
{
    [Controller("Index")]    
    public class IndexController : Controller
    {
        // GET
        [Action(Pattern = "", IsControllerRelatedPath = false)]
        public IActionResult Index()
        {
            return View("Index");
        }
    }
}
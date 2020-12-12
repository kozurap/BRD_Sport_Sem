﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ProjectArt.MVCPattern
{
    public delegate IActionResult ActionDelegate();
    
    public interface IActionResult
    {
        Task ExecuteResult(Controller controller);
    }
}
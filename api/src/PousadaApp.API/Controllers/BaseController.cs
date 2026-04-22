using Microsoft.AspNetCore.Mvc;

namespace PousadaApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected int GetEmpresaId() => int.Parse(User.FindFirst("empresaId")!.Value);
}

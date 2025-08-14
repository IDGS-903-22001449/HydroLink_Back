using HydroLink.Dtos;
using HydroLink.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly IEmailService _emailService;

    public ContactController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendContactForm([FromBody] ContactFormDto contactForm)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        string body = $@"
            Nombre: {contactForm.Name}
            Email: {contactForm.Email}
            Asunto: {contactForm.Subject}
            Mensaje:
            {contactForm.Message}
        ";

        string destinationEmail = "ajkevin55@gmail.com";

        try
        {
            await _emailService.SendEmailAsync(destinationEmail, contactForm.Subject, body);
            return Ok(new { message = "Mensaje enviado correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error al enviar el mensaje.", details = ex.Message });
        }
    }
}

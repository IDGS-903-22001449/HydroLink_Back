using HydroLink.Data;
using HydroLink.Dtos;
using HydroLink.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestSharp;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HydroLink.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AccountController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new AppUser
            {
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                UserName = registerDto.Email
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            if (registerDto.Roles is null || registerDto.Roles.Count == 0)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                foreach (var role in registerDto.Roles)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }

            try
            {
                var existeCliente = await _context.Persona.OfType<Cliente>()
                    .AnyAsync(c => c.Email == registerDto.Email);

                if (!existeCliente)
                {
                    var nombreCompleto = registerDto.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var nombre = nombreCompleto.Length > 0 ? nombreCompleto[0] : registerDto.FullName;
                    var apellido = nombreCompleto.Length > 1 ? string.Join(" ", nombreCompleto.Skip(1)) : "";
                    
                    var cliente = new Cliente
                    {
                        Nombre = nombre,
                        Apellido = apellido,
                        Email = registerDto.Email,
                        Telefono = registerDto.Telefono ?? "",
                        Direccion = registerDto.Direccion ?? "",
                        Empresa = registerDto.Empresa,
                        FechaRegistro = DateTime.UtcNow,
                        Activo = true
                    };

                    _context.Persona.Add(cliente);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear cliente: {ex.Message}");
            }

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Account and Client Created Successfully!"
            });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User not found with this email"
                });
            }

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!result)
            {
                return Unauthorized(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Invalid Password"
                });
            }

            var token = GenerateToken(user);
            var refreshToken = GenerateRefreshToken();
            _ = int.TryParse(_configuration.GetSection("JWTSetting").GetSection("RefreshTokenValidityIn").Value!, out int RefreshTokenValidityIn);
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(RefreshTokenValidityIn);
            await _userManager.UpdateAsync(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                IsSuccess = true,
                Message = "Login Success",
                RefreshToken = refreshToken
            });
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken(TokenDto tokenDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var principal = GetPrincipalFromExpiredToken(tokenDto.Token);
            var user = await _userManager.FindByEmailAsync(tokenDto.Email);

            if (principal == null || user is null || user.RefreshToken != tokenDto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Invalid request"
                });

            var newJwtToken = GenerateToken(user);
            var newrRefreshToken = GenerateRefreshToken();
            _ = int.TryParse(_configuration.GetSection("JWTSetting").GetSection("RefreshTokenValidityIn").Value!, out int RefreshTokenValidityIn);
            user.RefreshToken = newrRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(RefreshTokenValidityIn);

            await _userManager.UpdateAsync(user);
            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Token = newJwtToken,
                RefreshToken = newrRefreshToken,
                Message = "Refreshed token successfully"
            });
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user is null)
            {
                return Ok(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "No hay un usuario con este Email"
                });
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"http://localhost:4200/reset-password?email={user.Email}&token={WebUtility.UrlEncode(token)}";

            //using RestSharp;

            //var request = new RestRequest();
            //request.AddHeader("Authorization", "Bearer <YOUR_API_TOKEN>");
            //request.AddHeader("Content-Type", "application/json");
            //request.AddParameter("application/json", "{\"from\":{\"email\":\"hello@demomailtrap.co\",\"name\":\"Mailtrap Test\"},\"to\":[{\"email\":\"kevinfranciscoaj3h@gmail.com\"}],\"template_uuid\":\"8aee3ee9-0065-44ee-9d83-a8db94a1e0fa\",\"template_variables\":{\"user_email\":\"Test_User_email\",\"pass_reset_link\":\"Test_Pass_reset_link\"}}", ParameterType.RequestBody);
            //var response = client.Post(request);
            //System.Console.WriteLine(response.Content);

            var client = new RestClient("https://send.api.mailtrap.io/api/send");
            var request = new RestRequest
            {
                Method = Method.Post,
                RequestFormat = DataFormat.Json
            };

            request.AddHeader("Authorization", "Bearer d0c5686d8530f48212af0d431ddfc489");
            request.AddJsonBody(new
            {
                from = new { email = "hello@demomailtrap.co" },
                to = new[] { new { email = user.Email } },
                template_uuid = "8aee3ee9-0065-44ee-9d83-a8db94a1e0fa",
                template_variables = new { user_email = user.Email, pass_reset_link = resetLink }

            });

            var response = client.Execute(request);
            if (response.IsSuccessful)
            {
                return Ok(new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Se envió un correo electrónico con un enlace para restablecer la contraseña. Por favor, revise su correo electrónico."
                });
            }
            else
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = response.Content!.ToString()
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user is null)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User does not exist with this email"
                });
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);

            if (result.Succeeded)
            {
                return Ok(new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Password reset Successfully"
                });
            }
            return BadRequest(new AuthResponseDto
            {
                IsSuccess = false,
                Message = result.Errors.FirstOrDefault()?.Description
            });
        }

        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(changePasswordDto.Email);
            if (user is null)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User does not exist with this email"
                });
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

            if (result.Succeeded)
            {
                return Ok(new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Password changed successfully"
                });
            }

            return BadRequest(new AuthResponseDto
            {
                IsSuccess = false,
                Message = result.Errors.FirstOrDefault()?.Description
            });
        }

        [HttpGet("detail")]
        public async Task<ActionResult<UserDetailDto>> GetUserDetail()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId!);

            if (user == null)
            {
                return NotFound(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            return Ok(new UserDetailDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = (await _userManager.GetRolesAsync(user)).ToArray(),
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                AccessFailedCount = user.AccessFailedCount
            });
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetUserProfile()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId!);

            if (user == null)
            {
                return NotFound(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            var cliente = await _context.Persona.OfType<Cliente>()
                .FirstOrDefaultAsync(c => c.Email == user.Email);

            var userProfile = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = (await _userManager.GetRolesAsync(user)).ToArray(),
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                AccessFailedCount = user.AccessFailedCount
            };

            if (cliente != null)
            {
                userProfile.ClienteId = cliente.Id;
                userProfile.Nombre = cliente.Nombre;
                userProfile.Apellido = cliente.Apellido;
                userProfile.Telefono = cliente.Telefono;
                userProfile.Direccion = cliente.Direccion;
                userProfile.Empresa = cliente.Empresa;
                userProfile.TipoPersona = cliente.TipoPersona;
                userProfile.FechaRegistro = cliente.FechaRegistro;
                userProfile.Activo = cliente.Activo;
            }

            return Ok(userProfile);
        }

        [HttpPut("profile")]
        public async Task<ActionResult<AuthResponseDto>> UpdateUserProfile(UpdateProfileDto updateProfileDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId!);

            if (user == null)
            {
                return NotFound(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            try
            {
                user.FullName = updateProfileDto.FullName;
                user.PhoneNumber = updateProfileDto.PhoneNumber;
                
                var updateUserResult = await _userManager.UpdateAsync(user);
                if (!updateUserResult.Succeeded)
                {
                    return BadRequest(new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Error updating user information: " + string.Join(", ", updateUserResult.Errors.Select(e => e.Description))
                    });
                }

                var cliente = await _context.Persona.OfType<Cliente>()
                    .FirstOrDefaultAsync(c => c.Email == user.Email);

                if (cliente != null)
                {
                    cliente.Nombre = updateProfileDto.Nombre;
                    cliente.Apellido = updateProfileDto.Apellido;
                    cliente.Telefono = updateProfileDto.Telefono ?? "";
                    cliente.Direccion = updateProfileDto.Direccion ?? "";
                    cliente.Empresa = updateProfileDto.Empresa;
                    
                    _context.Persona.Update(cliente);
                }
                else
                {
                    cliente = new Cliente
                    {
                        Nombre = updateProfileDto.Nombre,
                        Apellido = updateProfileDto.Apellido,
                        Email = user.Email!,
                        Telefono = updateProfileDto.Telefono ?? "",
                        Direccion = updateProfileDto.Direccion ?? "",
                        Empresa = updateProfileDto.Empresa,
                        FechaRegistro = DateTime.UtcNow,
                        Activo = true
                    };
                    
                    _context.Persona.Add(cliente);
                }

                await _context.SaveChangesAsync();

                return Ok(new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Profile updated successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Error updating profile: " + ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetUsers()
        {
            var userList = await _userManager.Users.ToListAsync();
            var userDetails = new List<UserDetailDto>();

            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDetails.Add(new UserDetailDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Roles = roles.ToArray()
                });
            }

            return Ok(userDetails);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JWTSetting").GetSection("securityKey").Value!)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        private string GenerateToken(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JWTSetting").GetSection("securityKey").Value!);
            var roles = _userManager.GetRolesAsync(user).Result;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Name, user.FullName ?? ""),
                new Claim(JwtRegisteredClaimNames.NameId, user.Id ?? ""),
                new Claim(JwtRegisteredClaimNames.Aud, _configuration["JWTSetting:ValidAudience"]),
                new Claim(JwtRegisteredClaimNames.Iss, _configuration["JWTSetting:ValidIssuer"])
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Shop.Core.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Shop.Application.Interfaces.Auth;
using Microsoft.Extensions.Logging;

namespace Shop.Infastracture
{
    public class JWTProvider : IJWTProvider
    {
        private readonly JwtOptions _options;
        private readonly ILogger<JWTProvider> _logger;
        
        public JWTProvider(IOptions<JwtOptions> options, ILogger<JWTProvider> logger)
        {
            _options = options.Value;
            _logger = logger;
        }
        
        public string GenerateToken(User user)
        {
            Claim[] claims = [new("userId", user.Id.ToString())];

            var signingCredential = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: signingCredential,
                expires: DateTime.UtcNow.AddHours(_options.ExpiresHours));

            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenValue;
        }
    }
}

using Esferas.Data;
using Esferas.Models;
using Esferas.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Esferas.Services
{
    public class LinkUnicoService
    {
        private readonly ApplicationDbContext _context;

        public LinkUnicoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerarLinkEmpresaAsync(int encuestaId)
        {
            var existente = await _context.LinksUnicos
                .FirstOrDefaultAsync(l => l.EncuestaId == encuestaId && l.EsLinkEmpresa);

            if (existente != null)
                return $"/empresa/r/{existente.Token}";

            var nuevo = new LinkUnico
            {
                EncuestaId = encuestaId,
                Token = Guid.NewGuid(),
                FechaCreacion = DateTime.UtcNow,
                EsLinkEmpresa = true
            };

            _context.LinksUnicos.Add(nuevo);
            await _context.SaveChangesAsync();

            return $"/empresa/r/{nuevo.Token}";
        }
    }
}

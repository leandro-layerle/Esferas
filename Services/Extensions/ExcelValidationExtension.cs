using ClosedXML.Excel;
using Esferas.Models.DTOs;
using Esferas.Models.Entities;
using Esferas.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Esferas.Services.Extensions
{
    public static class ExcelValidationExtension
    {
        public static (string Valor, ImportLogDto log) LeerTextoConLog(this IXLCell celda, int fila, string campo)
        {
            var valor = celda?.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(valor))
            {
                return (null, new ImportLogDto
                {
                    Fila = fila,
                    Mensaje = $"Campo '{campo}' vacio",
                    EsExito = false
                });
            }

            return (valor, null);
        }

        public static (int? Valor, ImportLogDto Log) LeerEnteroConLog(this IXLCell celda, int fila, string campo)
        {
            var texto = celda?.GetString()?.Trim();
            if (!int.TryParse(texto, out var valor))
            {
                return (null, new ImportLogDto
                {
                    Fila = fila,
                    Mensaje = $"Campo '{campo}' inválido: '{texto}'",
                    EsExito = false
                });
            }

            return (valor, null);
        }

        public static (bool Valor, ImportLogDto Log) LeerBoolConLog(this IXLCell celda, int fila, string campo)
        {
            var texto = celda?.GetString()?.Trim().ToLower();
            if (texto == "true") return (true, null);
            if (texto == "false") return (false, null);

            return (false, new ImportLogDto
            {
                Fila = fila,
                Mensaje = $"Campo '{campo}' inválido (debe ser true o false): '{texto}'",
                EsExito = false
            });
        }

        public static (Categoria Valor, ImportLogDto Log) BuscarCategoriaConLog(this DbContext context, string nombre, TipoCategoria tipo, int fila, string campo)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return (null, null);

            var categoria = context.Set<Categoria>().FirstOrDefault(c => c.Nombre == nombre && c.Tipo == tipo);
            if (categoria == null)
            {
                return (null, new ImportLogDto
                {
                    Fila = fila,
                    Mensaje = $"Categoría '{campo}' no encontrada: '{nombre}'",
                    EsExito = false
                });
            }

            return (categoria, null);
        }

    }
}

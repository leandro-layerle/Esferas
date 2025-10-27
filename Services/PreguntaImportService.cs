namespace Esferas.Services
{
    using Esferas.Data;
    using Esferas.Models.DTOs;
    using Esferas.Models.Entities;
    using Esferas.Models.Enums;
    using Esferas.Services.Extensions;
    using ClosedXML.Excel;


    public class PreguntaImportService
    {
        private readonly ApplicationDbContext _context;

        public PreguntaImportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<ImportLogDto> Logs, bool FueImportacionExitosa)> ImportarDesdeExcelAsync(Stream archivoStream, int encuestaId)
        {

            var logs = new List<ImportLogDto>();
            var preguntasValidas = new List<Pregunta>();

            using var workbook = new XLWorkbook(archivoStream);
            var hoja = workbook.Worksheets.First();
            var totalFilas = hoja.LastRowUsed()
                                 .RowNumber();

            try
            {
                for (int fila = 2; fila <= totalFilas; fila++)
                {
                    var filaLogs = new List<ImportLogDto>();
                    var row = hoja.Row(fila);

                    var (texto, logTexto) = row.Cell(1).LeerTextoConLog(fila, "Texto");
                    if (logTexto != null) filaLogs.Add(logTexto);

                    var (tipoInt, logTipo) = row.Cell(2).LeerEnteroConLog(fila, "TipoRespuesta");
                    if (logTipo != null || tipoInt is null || !Enum.IsDefined(typeof(TipoRespuesta), tipoInt))
                    {
                        filaLogs.Add(logTipo ?? new ImportLogDto
                        {
                            Fila = fila,
                            Mensaje = "TipoRespuesta inválido o fuera de rango",
                            EsExito = false
                        });
                    }

                    var (catPrim, logCatPrim) = _context.BuscarCategoriaConLog(row.Cell(3).GetString(), TipoCategoria.Primaria, fila, "Categoría Primaria");
                    if (logCatPrim != null) filaLogs.Add(logCatPrim);

                    var (catSec, logCatSec) = _context.BuscarCategoriaConLog(row.Cell(4).GetString(), TipoCategoria.Secundaria, fila, "Categoría Secundaria");
                    if (logCatSec != null) filaLogs.Add(logCatSec);

                    var (catTer, logCatTer) = _context.BuscarCategoriaConLog(row.Cell(5).GetString(), TipoCategoria.Terciaria, fila, "Categoría Terciaria");
                    if (logCatTer != null) filaLogs.Add(logCatTer);

                    var (esPersonalizable, logPers) = row.Cell(6).LeerBoolConLog(fila, "Es Personalizable");
                    if (logPers != null) filaLogs.Add(logPers);

                    var (esRelevante, logRelev) = row.Cell(7).LeerBoolConLog(fila, "Es Relevante Para Semaforo");
                    if (logRelev != null) filaLogs.Add(logRelev);

                    // Si hubo errores en esta fila, loguear todos y continuar con la siguiente
                    if (filaLogs.Any())
                    {
                        logs.AddRange(filaLogs);
                        continue;
                    }

                    // Si todo está bien, construir la entidad y loguear éxito
                    var pregunta = new Pregunta
                    {
                        EncuestaId = encuestaId,
                        Texto = texto,
                        TipoRespuesta = (TipoRespuesta)tipoInt.Value,
                        CategoriaPrimariaId = catPrim.Id,
                        CategoriaSecundariaId = (int)catSec?.Id,
                        CategoriaTerciariaId = catTer?.Id,
                        EsPersonalizable = esPersonalizable,
                        EsRelevanteParaSemaforo = esRelevante
                    };

                    preguntasValidas.Add(pregunta);

                    logs.Add(new ImportLogDto
                    {
                        Fila = fila,
                        Mensaje = "Importación exitosa",
                        EsExito = true
                    });
                }

                _context.Preguntas.AddRange(preguntasValidas);
                await _context.SaveChangesAsync();

                return (logs, !logs.Any(l => !l.EsExito));

            }
            catch (Exception ex)
            {
                logs.Add(new ImportLogDto { Fila= 0,  Mensaje= ex.Message, EsExito=false });
                return (logs, false);
            }
        }
    }
}

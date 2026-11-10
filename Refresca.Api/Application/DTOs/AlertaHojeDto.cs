namespace Refresca.Api.Application.DTOs;

/// <summary>Contrato com o app: GET /api/alertas/hoje.</summary>
/// <param name="Data">Dia do alerta (AAAA-MM-DD, horário de Brasília).</param>
/// <param name="Perigoso">true quando o dia deve ser de calor perigoso.</param>
/// <param name="Probabilidade">Probabilidade dada pelo modelo de ML; null quando o alerta veio só dos totens.</param>
/// <param name="Mensagem">Texto mostrado ao trabalhador.</param>
/// <param name="Fonte">De onde veio o alerta (serviço de previsão ou totens).</param>
public sealed record AlertaHojeDto(
    string Data, bool Perigoso, double? Probabilidade, string Mensagem, string Fonte);

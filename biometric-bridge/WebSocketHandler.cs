// =============================================================================
// WebSocketHandler — Protocolo de comunicação entre Frontend e Bridge
// =============================================================================
// Mensagens JSON enviadas pelo frontend:
//
//   { "command": "status" }
//   → Resposta: { "type": "status", "sdkAvailable": bool, "readerConnected": bool }
//
//   { "command": "capture_enroll" }
//   → Progresso: { "type": "progress", "message": "Amostra 1 de 4..." }
//   → Resultado: { "type": "enroll_complete", "templateBase64": "..." }
//
//   { "command": "capture_verify" }
//   → Progresso: { "type": "progress", "message": "Aguardando..." }
//   → Resultado: { "type": "verify_complete", "sampleBase64": "..." }
//
//   { "command": "cancel" }
//   → Para qualquer captura em andamento
// =============================================================================

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace EpiManagement.BiometricBridge;

public class WebSocketHandler
{
    private readonly WebSocket _ws;
    private readonly BiometricManager _manager;
    private CancellationTokenSource? _operationCts;

    public WebSocketHandler(WebSocket ws, BiometricManager manager)
    {
        _ws = ws;
        _manager = manager;
    }

    public async Task HandleAsync(CancellationToken ct)
    {
        var buffer = new byte[4096];

        try
        {
            while (_ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Fechando", ct);
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                await ProcessMessageAsync(message, ct);
            }
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException) { }
    }

    private async Task ProcessMessageAsync(string json, CancellationToken ct)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var command = doc.RootElement.GetProperty("command").GetString();

            switch (command)
            {
                // ----------------------------------------------------------
                // STATUS: retorna estado do SDK e leitor
                // ----------------------------------------------------------
                case "status":
                    await SendAsync(new
                    {
                        type = "status",
                        sdkAvailable = BiometricManager.IsSdkAvailable,
                        readerConnected = BiometricManager.IsReaderConnected
                    }, ct);
                    break;

                // ----------------------------------------------------------
                // CAPTURE_ENROLL: cadastro biométrico (4 amostras → template)
                // Usado na tela de cadastro de funcionário (RH)
                // ----------------------------------------------------------
                case "capture_enroll":
                    _operationCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    try
                    {
                        var templateBase64 = await _manager.CaptureEnrollmentAsync(
                            progress => _ = SendAsync(new { type = "progress", message = progress }, ct),
                            _operationCts.Token
                        );

                        await SendAsync(new
                        {
                            type = "enroll_complete",
                            templateBase64,
                            message = "Template biométrico gerado com sucesso!"
                        }, ct);
                    }
                    catch (OperationCanceledException)
                    {
                        await SendAsync(new { type = "cancelled", message = "Operação cancelada" }, ct);
                    }
                    finally
                    {
                        _operationCts?.Dispose();
                        _operationCts = null;
                    }
                    break;

                // ----------------------------------------------------------
                // CAPTURE_VERIFY: captura para identificação/assinatura
                // Usado na tela de Entrega de EPI (Almoxarifado)
                // ----------------------------------------------------------
                case "capture_verify":
                    _operationCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    try
                    {
                        var sampleBase64 = await _manager.CaptureVerificationAsync(
                            progress => _ = SendAsync(new { type = "progress", message = progress }, ct),
                            _operationCts.Token
                        );

                        await SendAsync(new
                        {
                            type = "verify_complete",
                            sampleBase64,
                            message = "Digital capturada com sucesso!"
                        }, ct);
                    }
                    catch (OperationCanceledException)
                    {
                        await SendAsync(new { type = "cancelled", message = "Operação cancelada" }, ct);
                    }
                    finally
                    {
                        _operationCts?.Dispose();
                        _operationCts = null;
                    }
                    break;

                // ----------------------------------------------------------
                // CANCEL: para a captura em andamento
                // ----------------------------------------------------------
                case "cancel":
                    _manager.StopCapture();
                    _operationCts?.Cancel();
                    await SendAsync(new { type = "cancelled", message = "Operação cancelada" }, ct);
                    break;

                default:
                    await SendAsync(new { type = "error", message = $"Comando desconhecido: {command}" }, ct);
                    break;
            }
        }
        catch (Exception ex)
        {
            await SendAsync(new { type = "error", message = ex.Message }, ct);
        }
    }

    private async Task SendAsync(object data, CancellationToken ct)
    {
        if (_ws.State != WebSocketState.Open) return;
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var bytes = Encoding.UTF8.GetBytes(json);
        await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, ct);
    }
}

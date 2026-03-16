// =============================================================================
// EPI Management — DigitalPersona Biometric Bridge
// =============================================================================
// Este serviço deve rodar na máquina Windows onde o leitor DigitalPersona
// está conectado via USB.
//
// PRÉ-REQUISITOS:
//   1. Instalar o DigitalPersona One Touch for Windows SDK
//      (arquivo: Digital-Persona-SDK-master/SDK/Setup.exe)
//   2. Conectar o leitor U.are.U 4000B ou 4500 via USB
//   3. Copiar DPFPLib.dll para a pasta deste projeto e descomentar
//      a referência no .csproj
//   4. Rodar: dotnet run
//      O bridge ficará escutando em http://localhost:7001
//
// O frontend React se conecta via WebSocket em ws://localhost:7001/ws
// =============================================================================

using EpiManagement.BiometricBridge;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BiometricManager>();

// CORS: permite conexão do frontend em qualquer porta local
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Bridge escuta apenas localmente (segurança)
builder.WebHost.UseUrls("http://localhost:7001");

var app = builder.Build();

app.UseCors();

// Habilita WebSocket com keep-alive de 30 segundos
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30),
});

// Endpoint principal WebSocket
app.Map("/ws", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    var ws = await context.WebSockets.AcceptWebSocketAsync();
    var manager = context.RequestServices.GetRequiredService<BiometricManager>();
    var handler = new WebSocketHandler(ws, manager);
    await handler.HandleAsync(context.RequestAborted);
});

// Endpoint de status (health check do frontend)
app.MapGet("/status", () => new
{
    status = "online",
    sdkAvailable = BiometricManager.IsSdkAvailable,
    readerConnected = BiometricManager.IsReaderConnected,
    version = "1.0.0"
});

Console.WriteLine("=== EPI Management — Biometric Bridge ===");
Console.WriteLine("Escutando em: ws://localhost:7001/ws");
Console.WriteLine($"SDK DigitalPersona: {(BiometricManager.IsSdkAvailable ? "OK" : "NÃO ENCONTRADO")}");
Console.WriteLine($"Leitor conectado: {(BiometricManager.IsReaderConnected ? "SIM" : "NÃO")}");
Console.WriteLine("=========================================");

await app.RunAsync();

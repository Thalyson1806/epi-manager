# EPI Management — Biometric Bridge

Serviço Windows que faz ponte entre o leitor DigitalPersona e o sistema web.

## Como usar

1. Instale o SDK: `Digital-Persona-SDK-master/SDK/Setup.exe`
2. Conecte o leitor U.are.U via USB
3. Copie `DPFPLib.dll` da instalação do SDK para esta pasta
4. Descomente as referências no `.csproj` e no `BiometricManager.cs`
5. Execute: `dotnet run`

O bridge fica em `ws://localhost:7001/ws`

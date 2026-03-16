// =============================================================================
// BiometricManager — Gerencia o ciclo de vida do SDK DigitalPersona
// =============================================================================
// Esta classe encapsula todas as operações com o SDK.
//
// REFERÊNCIA SDK: One Touch for Windows SDK .NET Developer Guide.pdf
//   Namespaces principais:
//     DPFP                        — Classes base (Template, Sample, etc.)
//     DPFP.Capture                — Captura de impressão digital
//     DPFP.Processing             — Extração de features e enrollment
//     DPFP.Verification           — Verificação 1:1
//
// FLUXO DE ENROLLMENT (cadastro de template):
//   1. Criar DPFP.Capture.Capture()
//   2. Registrar evento OnSampleArrived
//   3. Chamar StartCapture() — aguardar 4 amostras
//   4. Para cada amostra: ExtractFeatures(sample, DataPurpose.Enrollment)
//   5. Enrollment.AddFeatures(featureSet) — após 4 features: Enrollment.Template
//   6. Serializar: template.Serialize() → byte[]
//
// FLUXO DE VERIFICAÇÃO (identificação 1:N):
//   1. Capturar 1 amostra
//   2. ExtractFeatures(sample, DataPurpose.Verification) → featureSet
//   3. Para cada funcionário com template:
//      a. DeSerialize(storedBytes) → template
//      b. Verifier.Verify(featureSet, template, ref result)
//      c. if result.Verified → funcionário identificado
// =============================================================================

using System.Text.Json;

namespace EpiManagement.BiometricBridge;

public class BiometricManager
{
    // ==========================================================================
    // PROPRIEDADES ESTÁTICAS — status do SDK e leitor
    // ==========================================================================

    /// <summary>
    /// Indica se o assembly DPFPLib.dll foi carregado com sucesso.
    /// Será false se o SDK não estiver instalado.
    /// </summary>
    public static bool IsSdkAvailable { get; private set; }

    /// <summary>
    /// Indica se o leitor U.are.U está conectado e respondendo.
    /// </summary>
    public static bool IsReaderConnected { get; private set; }

    // ==========================================================================
    // OBJETOS DO SDK — descomente após referenciar DPFPLib.dll
    // ==========================================================================

    // private DPFP.Capture.Capture? _capture;
    // private DPFP.Processing.Enrollment? _enrollment;
    // private DPFP.Verification.Verification? _verifier;

    // Callback chamado quando uma amostra chega do leitor
    private Action<byte[]>? _onSampleCaptured;
    private Action<string>? _onError;
    private CancellationTokenSource? _captureCts;

    static BiometricManager()
    {
        // Tenta inicializar o SDK na carga estática
        // Se a DLL não estiver disponível, IsSdkAvailable ficará false
        try
        {
            // TODO: Descomentar após adicionar DPFPLib.dll ao projeto
            // var testCapture = new DPFP.Capture.Capture();
            // testCapture.Dispose();
            // IsSdkAvailable = true;

            // SIMULAÇÃO: remover quando SDK real for integrado
            IsSdkAvailable = false;
            IsReaderConnected = false;
        }
        catch (Exception ex)
        {
            IsSdkAvailable = false;
            Console.WriteLine($"[SDK] Erro ao inicializar DigitalPersona SDK: {ex.Message}");
            Console.WriteLine("[SDK] Verifique se o SDK está instalado (SDK/Setup.exe)");
        }
    }

    // ==========================================================================
    // MÉTODO: CaptureEnrollmentAsync
    // Captura múltiplas amostras e retorna o template serializado
    // ==========================================================================

    /// <summary>
    /// Realiza o cadastro biométrico completo.
    /// O SDK precisa de 4 amostras para gerar um template de qualidade.
    /// Retorna o template como base64 para ser salvo no banco via API.
    /// </summary>
    /// <param name="onProgress">Callback: informa progresso (ex: "2 de 4 amostras")</param>
    /// <param name="ct">CancellationToken para cancelar a operação</param>
    /// <returns>Template serializado em base64</returns>
    public async Task<string> CaptureEnrollmentAsync(
        Action<string> onProgress,
        CancellationToken ct = default)
    {
        // ======================================================================
        // IMPLEMENTAÇÃO COM SDK REAL — descomente este bloco
        // ======================================================================
        /*
        _enrollment = new DPFP.Processing.Enrollment();
        var tcs = new TaskCompletionSource<string>();
        int samplesCollected = 0;
        const int samplesRequired = 4;

        _capture = new DPFP.Capture.Capture();

        // Evento disparado a cada nova leitura do dedo
        _capture.EventHandler = new CaptureEventHandler(sample =>
        {
            try
            {
                // Extrai features para enrollment
                var extractor = new DPFP.Processing.FeatureExtraction();
                var feedback = new DPFP.Capture.CaptureFeedback();
                extractor.CreateFeatureSet(sample, DPFP.Processing.DataPurpose.Enrollment, ref feedback, out var featureSet);

                if (feedback == DPFP.Capture.CaptureFeedback.Good)
                {
                    _enrollment.AddFeatures(featureSet);
                    samplesCollected++;
                    onProgress($"Amostra {samplesCollected} de {samplesRequired} capturada. Retire e coloque o dedo novamente.");

                    if (samplesCollected >= samplesRequired && _enrollment.TemplateStatus == DPFP.Processing.Enrollment.Status.Ready)
                    {
                        _capture.StopCapture();
                        var templateBytes = _enrollment.Template.Serialize();
                        tcs.SetResult(Convert.ToBase64String(templateBytes));
                    }
                }
                else
                {
                    onProgress($"Leitura ruim ({feedback}). Tente novamente.");
                }
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        _capture.StartCapture();

        using var reg = ct.Register(() =>
        {
            _capture?.StopCapture();
            tcs.TrySetCanceled();
        });

        return await tcs.Task;
        */

        // ======================================================================
        // SIMULAÇÃO — remover quando SDK real for integrado
        // ======================================================================
        onProgress("Simulação: Coloque o dedo no leitor (1/4)");
        await Task.Delay(1500, ct);
        onProgress("Simulação: Retire e coloque novamente (2/4)");
        await Task.Delay(1500, ct);
        onProgress("Simulação: Mais uma vez (3/4)");
        await Task.Delay(1500, ct);
        onProgress("Simulação: Última amostra (4/4)");
        await Task.Delay(1500, ct);

        // Retorna template falso para teste
        var fakeTemplate = new byte[512];
        new Random().NextBytes(fakeTemplate);
        return Convert.ToBase64String(fakeTemplate);
    }

    // ==========================================================================
    // MÉTODO: CaptureVerificationAsync
    // Captura 1 amostra para identificação/assinatura
    // ==========================================================================

    /// <summary>
    /// Captura uma única amostra do leitor para verificação ou assinatura.
    /// Retorna a feature set em base64 para ser enviada ao backend para identificação 1:N.
    /// </summary>
    public async Task<string> CaptureVerificationAsync(
        Action<string> onProgress,
        CancellationToken ct = default)
    {
        // ======================================================================
        // IMPLEMENTAÇÃO COM SDK REAL — descomente este bloco
        // ======================================================================
        /*
        var tcs = new TaskCompletionSource<string>();

        _capture = new DPFP.Capture.Capture();
        _capture.EventHandler = new CaptureEventHandler(sample =>
        {
            try
            {
                var extractor = new DPFP.Processing.FeatureExtraction();
                var feedback = new DPFP.Capture.CaptureFeedback();
                extractor.CreateFeatureSet(sample, DPFP.Processing.DataPurpose.Verification, ref feedback, out var featureSet);

                if (feedback == DPFP.Capture.CaptureFeedback.Good)
                {
                    _capture.StopCapture();
                    // Serializa a feature set para envio ao backend
                    var featureBytes = featureSet.Serialize();
                    tcs.SetResult(Convert.ToBase64String(featureBytes));
                }
                else
                {
                    onProgress($"Leitura ruim ({feedback}). Tente novamente.");
                }
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        _capture.StartCapture();

        using var reg = ct.Register(() =>
        {
            _capture?.StopCapture();
            tcs.TrySetCanceled();
        });

        return await tcs.Task;
        */

        // SIMULAÇÃO
        onProgress("Aguardando leitura do dedo...");
        await Task.Delay(2000, ct);
        var fakeFeature = new byte[256];
        new Random().NextBytes(fakeFeature);
        return Convert.ToBase64String(fakeFeature);
    }

    public void StopCapture()
    {
        // _capture?.StopCapture();
        _captureCts?.Cancel();
    }
}

// =============================================================================
// CaptureEventHandler — Adaptador para o evento do SDK
// =============================================================================
// O SDK DigitalPersona usa um padrão de event handler com interface específica.
// Esta classe adapta um delegate para a interface esperada pelo SDK.
//
// USO NO SDK:
//   capture.EventHandler = new CaptureEventHandler(sample => { ... });
// =============================================================================

// TODO: Descomentar quando DPFPLib.dll estiver referenciado
/*
public class CaptureEventHandler : DPFP.Capture.EventHandlerBase
{
    private readonly Action<DPFP.Sample> _onSample;

    public CaptureEventHandler(Action<DPFP.Sample> onSample)
    {
        _onSample = onSample;
    }

    // Chamado quando o leitor captura uma amostra
    public override void OnSampleArrived(object Capture, string ReaderSerialNumber, DPFP.Sample Sample)
    {
        _onSample(Sample);
    }

    // Chamado quando o leitor é conectado
    public override void OnReaderConnect(object Capture, string ReaderSerialNumber)
    {
        Console.WriteLine($"[SDK] Leitor conectado: {ReaderSerialNumber}");
        BiometricManager.IsReaderConnected = true;
    }

    // Chamado quando o leitor é desconectado
    public override void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
    {
        Console.WriteLine($"[SDK] Leitor desconectado: {ReaderSerialNumber}");
        BiometricManager.IsReaderConnected = false;
    }

    // Chamado quando o dedo é colocado no leitor
    public override void OnFingerTouch(object Capture, string ReaderSerialNumber) { }

    // Chamado quando o dedo é retirado do leitor
    public override void OnFingerGone(object Capture, string ReaderSerialNumber) { }
}
*/

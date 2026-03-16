namespace EpiManagement.Domain.Interfaces;

public interface IBiometricService
{
    Task<byte[]?> CaptureTemplateAsync(CancellationToken ct = default);
    Task<(bool matched, Guid? employeeId)> IdentifyAsync(byte[] capturedSample, CancellationToken ct = default);
    Task<bool> VerifyAsync(byte[] storedTemplate, byte[] capturedSample, CancellationToken ct = default);
}

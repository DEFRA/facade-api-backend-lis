namespace LisFacadeApiBackend.Facade.Services.Cattle.Translators;

public interface ITranslator<TInboundRequest, TInboundResponse>
{
    Task<TInboundResponse> Translate(TInboundRequest inboundRequest, CancellationToken cancellationToken);
}

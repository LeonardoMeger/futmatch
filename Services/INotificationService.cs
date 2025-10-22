using FutMatchApp.Models;
using FutMatchApp.Models.Enums;

namespace FutMatchApp.Services
{
    public interface INotificationService
    {
        Task CriarNotificacaoDesafioEnviado(int userId, int reservationId, string timeDesafiado);
        Task CriarNotificacaoDesafioRecebido(int userId, int reservationId, string timeDesafiante);
        Task CriarNotificacaoDesafioAceito(int userDesafianteId, int userDesafiadoId, int reservationId);
        Task CriarNotificacaoDesafioRejeitado(int userDesafianteId, string timeQueRejeitou);
        Task CriarNotificacaoPartidaConfirmada(int userId, int reservationId);
        Task CriarNotificacaoResultadoPendente(int userId, int reservationId, string meuTime, string timeAdversario, int meusGols, int golsAdversario);
        Task CriarNotificacaoResultadoConfirmado(int userId, int reservationId, string time1, string time2, int gols1, int gols2);
        Task CriarNotificacaoConflito(int userId, int reservationId, string meuTime, string timeAdversario);
        Task CriarNotificacaoResultado(int userId, int reservationId, string myTeamName, string opponentTeamName, string resultado, string placar);
        Task CriarNotificacaoResultadoPartida(int userId, int reservationId, string resultado);
        Task CriarLembretePartida(int userId, int reservationId);
        Task<List<Notification>> BuscarNotificacoesUsuario(int userId, bool apenasNaoLidas = false);
        Task MarcarComoLida(int notificationId, int userId);
        Task MarcarTodasComoLidas(int userId);
        Task<int> ContarNotificacoesNaoLidas(int userId);
        Task<bool> AceitarDesafio(int notificationId, int userId);
        Task<bool> RejeitarDesafio(int notificationId, int userId);
        Task<bool> ConfirmarResultado(int notificationId, int userId, int meusGols, int golsAdversario);
        Task<bool> ContestarResultado(int notificationId, int userId, int meusGols, int golsAdversario, string motivo);
        Task<bool> ResolverConflito(int reservationId, int adminUserId, int golsTime1Final, int golsTime2Final, string observacoes);
        Task<List<Reservation>> BuscarConflitosResultado();
        Task<bool> PodeInformarResultado(int reservationId, int userId);
        Task<object> BuscarHistoricoResultados(int reservationId);
    }
}

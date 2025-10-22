using FutMatchApp.Data;
using FutMatchApp.Models.Enums;
using FutMatchApp.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using FutMatchApp.Models.Enums;

namespace FutMatchApp.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CriarNotificacaoDesafioEnviado(int userId, int reservationId, string timeDesafiado)
        {
            var notification = new Notification
            {
                UserId = userId,
                ReservationId = reservationId,
                Tipo = TipoNotificacao.DesafioEnviado,
                Titulo = "Desafio Enviado",
                Mensagem = $"Você enviou um desafio para {timeDesafiado}",
                DataCriacao = DateTime.Now,
                Lida = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CriarNotificacaoDesafioRecebido(int userId, int reservationId, string timeDesafiante)
        {
            var notification = new Notification
            {
                UserId = userId,
                ReservationId = reservationId,
                Tipo = TipoNotificacao.DesafioRecebido,
                Titulo = "Novo Desafio Recebido!",
                Mensagem = $"{timeDesafiante} quer jogar contra seu time. Aceitar o desafio?",
                DataCriacao = DateTime.Now,
                Lida = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CriarNotificacaoDesafioAceito(int userDesafianteId, int userDesafiadoId, int reservationId)
        {
            var notification1 = new Notification
            {
                UserId = userDesafianteId,
                ReservationId = reservationId,
                Tipo = TipoNotificacao.DesafioAceito,
                Titulo = "Desafio Aceito!",
                Mensagem = "Seu desafio foi aceito! A partida está confirmada.",
                DataCriacao = DateTime.Now,
                Lida = false
            };
            var notification2 = new Notification
            {
                UserId = userDesafiadoId,
                ReservationId = reservationId,
                Tipo = TipoNotificacao.PartidaConfirmada,
                Titulo = "Partida Confirmada",
                Mensagem = "Você aceitou o desafio. A partida está confirmada!",
                DataCriacao = DateTime.Now,
                Lida = false
            };

            _context.Notifications.AddRange(notification1, notification2);
            await _context.SaveChangesAsync();
        }

        public async Task CriarNotificacaoDesafioRejeitado(int userDesafianteId, string timeQueRejeitou)
        {
            var notification = new Notification
            {
                UserId = userDesafianteId,
                ReservationId = null,
                Tipo = TipoNotificacao.DesafioRejeitado,
                Titulo = "Desafio Rejeitado",
                Mensagem = $"{timeQueRejeitou} rejeitou seu desafio.",
                DataCriacao = DateTime.Now,
                Lida = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CriarNotificacaoPartidaConfirmada(int userId, int reservationId)
        {
            var notification = new Notification
            {
                UserId = userId,
                ReservationId = reservationId,
                Tipo = TipoNotificacao.PartidaConfirmada,
                Titulo = "Partida Confirmada",
                Mensagem = "Sua partida foi confirmada e está agendada!",
                DataCriacao = DateTime.Now,
                Lida = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
        public async Task CriarNotificacaoResultadoPendente(int userId, int reservationId, string meuTime, string timeAdversario, int meusGols, int golsAdversario)
        {
            var notification = new Notification
            {
                UserId = userId,
                ReservationId = reservationId,
                Tipo = TipoNotificacao.ResultadoPendente,
                Titulo = "Confirme o Resultado da Partida",
                Mensagem = $"{meuTime} informou o resultado: {timeAdversario} {golsAdversario} x {meusGols} {meuTime}. Confirme se está correto.",
                DataCriacao = DateTime.Now,
                Lida = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CriarNotificacaoResultadoConfirmado(int userId, int reservationId, string time1, string time2, int gols1, int gols2)
        {
            var resultadoTexto = gols1 > gols2 ? "Vitória" : gols1 < gols2 ? "Derrota" : "Empate";

            var notification = new Notification
            {
                UserId = userId,
                ReservationId = reservationId,
                Tipo = TipoNotificacao.ResultadoConfirmado,
                Titulo = $"Resultado Confirmado - {resultadoTexto}",
                Mensagem = $"Resultado confirmado por ambos os times: {time1} {gols1} x {gols2} {time2}",
                DataCriacao = DateTime.Now,
                Lida = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CriarNotificacaoConflito(int userId, int reservationId, string meuTime, string timeAdversario)
        {
            var notification = new Notification
            {
                UserId = userId,
                ReservationId = reservationId,
                Tipo = TipoNotificacao.ResultadoConflito,
                Titulo = "Conflito no Resultado",
                Mensagem = $"Há divergência nos resultados informados entre {meuTime} e {timeAdversario}. Um administrador irá resolver.",
                DataCriacao = DateTime.Now,
                Lida = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
        public async Task CriarNotificacaoResultado(int userId, int reservationId, string myTeamName, string opponentTeamName, string resultado, string placar)
        {
            var notification = new Notification
            {
                UserId = userId,
                ReservationId = reservationId,
                Tipo = TipoNotificacao.ResultadoPartida,
                Titulo = "Resultado da Partida Informado",
                Mensagem = $"{myTeamName} informou o resultado da partida: {placar}. Resultado para {opponentTeamName}: {resultado}",
                DataCriacao = DateTime.Now,
                Lida = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CriarNotificacaoResultadoPartida(int userId, int reservationId, string resultado)
        {
            var notification = new Notification
            {
                UserId = userId,
                ReservationId = reservationId,
                Tipo = TipoNotificacao.ResultadoPartida,
                Titulo = "Resultado da Partida",
                Mensagem = resultado,
                DataCriacao = DateTime.Now,
                Lida = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CriarLembretePartida(int userId, int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Court)
                .Include(r => r.OpponentTeam)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation != null)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    ReservationId = reservationId,
                    Tipo = TipoNotificacao.LembretePartida,
                    Titulo = "Lembrete de Partida",
                    Mensagem = $"Sua partida contra {reservation.OpponentTeam?.Nome} está próxima! Local: {reservation.Court.Nome}, Horário: {reservation.DataHora:dd/MM HH:mm}",
                    DataCriacao = DateTime.Now,
                    Lida = false
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<Notification>> BuscarNotificacoesUsuario(int userId, bool apenasNaoLidas = false)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId);

            if (apenasNaoLidas)
            {
                query = query.Where(n => !n.Lida);
            }

            return await query
                .OrderByDescending(n => n.DataCriacao)
                .Take(50) 
                .ToListAsync();
        }

        public async Task MarcarComoLida(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null && !notification.Lida)
            {
                notification.Lida = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarcarTodasComoLidas(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.Lida)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.Lida = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> ContarNotificacoesNaoLidas(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.Lida);
        }
        public async Task<bool> AceitarDesafio(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .Include(n => n.Reservation)
                .ThenInclude(r => r.Team)
                .Include(n => n.Reservation)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(n => n.Id == notificationId &&
                                       n.UserId == userId &&
                                       n.Tipo == TipoNotificacao.DesafioRecebido);

            if (notification?.Reservation == null)
                return false;

            var user = await _context.Users
                .Include(u => u.SelectedTeam)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.SelectedTeam == null)
                return false;

            notification.Reservation.OpponentUserId = userId;
            notification.Reservation.OpponentTeamId = user.SelectedTeam.Id;
            notification.Reservation.Status = StatusReservation.Confirmada;

            notification.Lida = true;

            await CriarNotificacaoDesafioAceito(
                notification.Reservation.UserId,
                userId,
                notification.Reservation.Id
            );

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejeitarDesafio(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .Include(n => n.Reservation)
                .ThenInclude(r => r.Team)
                .FirstOrDefaultAsync(n => n.Id == notificationId &&
                                       n.UserId == userId &&
                                       n.Tipo == TipoNotificacao.DesafioRecebido);

            if (notification?.Reservation == null)
                return false;

            var user = await _context.Users
                .Include(u => u.SelectedTeam)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.SelectedTeam == null)
                return false;
           notification.Reservation.Status = StatusReservation.Cancelada;

            notification.Lida = true;

            await CriarNotificacaoDesafioRejeitado(
                notification.Reservation.UserId,
                user.SelectedTeam.Nome
            );

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ConfirmarResultado(int notificationId, int userId, int meusGols, int golsAdversario)
        {
            var notification = await _context.Notifications
                .Include(n => n.Reservation)
                .ThenInclude(r => r.Team)
                .Include(n => n.Reservation)
                .ThenInclude(r => r.OpponentTeam)
                .FirstOrDefaultAsync(n => n.Id == notificationId &&
                                       n.UserId == userId &&
                                       n.Tipo == TipoNotificacao.ResultadoPendente);

            if (notification?.Reservation == null)
                return false;

            var user = await _context.Users
                .Include(u => u.SelectedTeam)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.SelectedTeam == null)
                return false;

            var reservation = notification.Reservation;
            var isTeam1 = reservation.TeamId == user.SelectedTeam.Id;

            if (isTeam1)
            {
                reservation.ResultadoInformadoTime1 = true;
                reservation.DataResultadoTime1 = DateTime.Now;
                reservation.GolsTime1_InformadoPeloTime1 = meusGols;
                reservation.GolsTime2_InformadoPeloTime1 = golsAdversario;
            }
            else
            {
                reservation.ResultadoInformadoTime2 = true;
                reservation.DataResultadoTime2 = DateTime.Now;
                reservation.GolsTime1_InformadoPeloTime2 = golsAdversario;
                reservation.GolsTime2_InformadoPeloTime2 = meusGols;
            }

            if (reservation.ResultadoInformadoTime1 && reservation.ResultadoInformadoTime2)
            {
                var gols1_PorTime1 = reservation.GolsTime1_InformadoPeloTime1 ?? 0;
                var gols2_PorTime1 = reservation.GolsTime2_InformadoPeloTime1 ?? 0;
                var gols1_PorTime2 = reservation.GolsTime1_InformadoPeloTime2 ?? 0;
                var gols2_PorTime2 = reservation.GolsTime2_InformadoPeloTime2 ?? 0;

                if (gols1_PorTime1 == gols1_PorTime2 && gols2_PorTime1 == gols2_PorTime2)
                {
                    reservation.GolsTime1Final = gols1_PorTime1;
                    reservation.GolsTime2Final = gols2_PorTime1;
                    reservation.StatusResultado = StatusResultado.Confirmado;
                    reservation.Status = StatusReservation.Finalizada;
                    reservation.DataResultadoFinal = DateTime.Now;

                    
                    await CriarNotificacaoResultadoConfirmado(
                        reservation.UserId,
                        reservation.Id,
                        reservation.Team.Nome,
                        reservation.OpponentTeam.Nome,
                        gols1_PorTime1,
                        gols2_PorTime1
                    );

                    if (reservation.OpponentUserId.HasValue)
                    {
                        await CriarNotificacaoResultadoConfirmado(
                            reservation.OpponentUserId.Value,
                            reservation.Id,
                            reservation.Team.Nome,
                            reservation.OpponentTeam.Nome,
                            gols1_PorTime1,
                            gols2_PorTime1
                        );
                    }
                }
                else
                {
                    reservation.StatusResultado = StatusResultado.Conflito;

                    await CriarNotificacaoConflito(
                        reservation.UserId,
                        reservation.Id,
                        reservation.Team.Nome,
                        reservation.OpponentTeam.Nome
                    );

                    if (reservation.OpponentUserId.HasValue)
                    {
                        await CriarNotificacaoConflito(
                            reservation.OpponentUserId.Value,
                            reservation.Id,
                            reservation.OpponentTeam.Nome,
                            reservation.Team.Nome
                        );
                    }
                }
            }

            notification.Lida = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ContestarResultado(int notificationId, int userId, int meusGols, int golsAdversario, string motivo)
        {
            return await ConfirmarResultado(notificationId, userId, meusGols, golsAdversario);
        }

        public async Task<bool> ResolverConflito(int reservationId, int adminUserId, int golsTime1Final, int golsTime2Final, string observacoes)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Team)
                .Include(r => r.OpponentTeam)
                .FirstOrDefaultAsync(r => r.Id == reservationId &&
                                       r.StatusResultado == StatusResultado.Conflito);

            if (reservation == null)
                return false;

            reservation.GolsTime1Final = golsTime1Final;
            reservation.GolsTime2Final = golsTime2Final;
            reservation.StatusResultado = StatusResultado.Resolvido;
            reservation.DataResultadoFinal = DateTime.Now;
            reservation.Status = StatusReservation.Finalizada;

            if (golsTime1Final > golsTime2Final)
            {
                reservation.ResultadoTime1Final = ResultadoPartida.Vitoria;
                reservation.ResultadoTime2Final = ResultadoPartida.Derrota;
            }
            else if (golsTime1Final < golsTime2Final)
            {
                reservation.ResultadoTime1Final = ResultadoPartida.Derrota;
                reservation.ResultadoTime2Final = ResultadoPartida.Vitoria;
            }
            else
            {
                reservation.ResultadoTime1Final = ResultadoPartida.Empate;
                reservation.ResultadoTime2Final = ResultadoPartida.Empate;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Reservation>> BuscarConflitosResultado()
        {
            return await _context.Reservations
                .Include(r => r.Court)
                .Include(r => r.Team)
                .Include(r => r.OpponentTeam)
                .Where(r => r.StatusResultado == StatusResultado.Conflito)
                .OrderBy(r => r.DataHora)
                .ToListAsync();
        }

        public async Task<bool> PodeInformarResultado(int reservationId, int userId)
        {
            var user = await _context.Users
                .Include(u => u.SelectedTeam)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.SelectedTeam == null)
                return false;

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId &&
                                       (r.TeamId == user.SelectedTeam.Id || r.OpponentTeamId == user.SelectedTeam.Id) &&
                                       r.Status == StatusReservation.Confirmada);

            if (reservation == null)
                return false;

            var isTeam1 = reservation.TeamId == user.SelectedTeam.Id;
            var jaInformou = isTeam1 ? reservation.ResultadoInformadoTime1 : reservation.ResultadoInformadoTime2;

            return !jaInformou && DateTime.Now >= reservation.DataHora.AddHours(reservation.DuracaoHoras);
        }

        public async Task<object> BuscarHistoricoResultados(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Team)
                .Include(r => r.OpponentTeam)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                return null;

            var resultadoTime1 = new
            {
                informado = reservation.ResultadoInformadoTime1,
                data = reservation.ResultadoInformadoTime1 ? reservation.DataResultadoTime1 : (DateTime?)null,
                golsTime1 = reservation.ResultadoInformadoTime1 ? reservation.GolsTime1_InformadoPeloTime1 : (int?)null,
                golsTime2 = reservation.ResultadoInformadoTime1 ? reservation.GolsTime2_InformadoPeloTime1 : (int?)null
            };

            var resultadoTime2 = new
            {
                informado = reservation.ResultadoInformadoTime2,
                data = reservation.ResultadoInformadoTime2 ? reservation.DataResultadoTime2 : (DateTime?)null,
                golsTime1 = reservation.ResultadoInformadoTime2 ? reservation.GolsTime1_InformadoPeloTime2 : (int?)null,
                golsTime2 = reservation.ResultadoInformadoTime2 ? reservation.GolsTime2_InformadoPeloTime2 : (int?)null
            };

            var temResultadoFinal = reservation.StatusResultado == StatusResultado.Confirmado ||
                                   reservation.StatusResultado == StatusResultado.Resolvido;

            var resultadoFinal = temResultadoFinal ? new
            {
                golsTime1 = reservation.GolsTime1Final,
                golsTime2 = reservation.GolsTime2Final,
                dataConfirmacao = reservation.DataResultadoFinal,
                statusFinal = reservation.StatusResultado.ToString()
            } : null;

            return new
            {
                reservationId = reservation.Id,
                time1 = reservation.Team.Nome,
                time2 = reservation.OpponentTeam?.Nome ?? "Time não definido",
                statusResultado = reservation.StatusResultado.ToString(),
                dataPartida = reservation.DataHora.ToString("dd/MM/yyyy HH:mm"),
                quadra = reservation.Court?.Nome ?? "Quadra não definida",
                resultadoTime1 = resultadoTime1,
                resultadoTime2 = resultadoTime2,
                resultadoFinal = resultadoFinal,
                temConflito = reservation.StatusResultado == StatusResultado.Conflito,
                partidaFinalizada = reservation.Status == StatusReservation.Finalizada
            };
        }
    }
}

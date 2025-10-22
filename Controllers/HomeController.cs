using FutMatchApp.Data;
using FutMatchApp.Models;
using FutMatchApp.Models.Enums;
using FutMatchApp.Models.ViewModels;
using FutMatchApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace FutMatchApp.Controllers
{

    public class HomeController : BaseController
    {
        private readonly IGooglePlacesService _googlePlacesService;
        private readonly INotificationService _notificationService;

        public HomeController(
            ApplicationDbContext context,
            IGooglePlacesService googlePlacesService,
            INotificationService notificationService)
            : base(context) 
        {
            _googlePlacesService = googlePlacesService;
            _notificationService = notificationService;
        }
        public async Task<IActionResult> Index(string cidade, string bairro, int? quadraId, DateTime? dataInicio, DateTime? dataFim)
        {
            var viewModel = new HomeViewModel();

            if (User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                var googleCourts = new List<Court>();

                if (!string.IsNullOrEmpty(cidade))
                {
                    try
                    {
                        var searchLocation = cidade;
                        if (!string.IsNullOrEmpty(bairro))
                        {
                            searchLocation = $"{bairro}, {cidade}";
                        }

                        Console.WriteLine($"Buscando quadras em: {searchLocation}");
                        googleCourts = await _googlePlacesService.SearchFootballCourtsAsync(searchLocation);
                        Console.WriteLine($"Encontradas {googleCourts.Count} quadras");

                        if (dataInicio.HasValue || dataFim.HasValue)
                        {
                            var dataIni = dataInicio ?? DateTime.Now.Date;
                            var dataFin = dataFim ?? dataInicio ?? DateTime.Now.Date;

                            if (dataFin < dataIni)
                            {
                                dataFin = dataIni;
                            }

                            googleCourts = await FiltrarQuadrasPorDisponibilidade(googleCourts, dataIni, dataFin);

                            TempData["Info"] = $"Buscando quadras disponíveis entre {dataIni:dd/MM/yyyy} e {dataFin:dd/MM/yyyy} em {searchLocation}";
                        }

                        if (googleCourts.Any())
                        {
                            var periodo = "";
                            if (dataInicio.HasValue || dataFim.HasValue)
                            {
                                var dataIni = dataInicio ?? DateTime.Now.Date;
                                var dataFin = dataFim ?? dataInicio ?? DateTime.Now.Date;
                                periodo = dataIni == dataFin ?
                                    $" para {dataIni:dd/MM/yyyy}" :
                                    $" entre {dataIni:dd/MM/yyyy} e {dataFin:dd/MM/yyyy}";
                            }
                            TempData["Success"] = $"Encontradas {googleCourts.Count} quadras em {searchLocation}{periodo}";
                        }
                        else
                        {
                            TempData["Info"] = $"Nenhuma quadra encontrada em {searchLocation} para o período selecionado. Tente outra localização ou período.";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao buscar quadras: {ex.Message}");
                        TempData["Error"] = "Erro ao buscar quadras. Tente novamente.";
                    }
                }
                else if (user != null && !string.IsNullOrEmpty(user.Cidade))
                {
                    try
                    {
                        var userLocation = $"{user.Cidade}, {user.Estado}";
                        googleCourts = await _googlePlacesService.SearchFootballCourtsAsync(userLocation);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao buscar quadras da cidade do usuário: {ex.Message}");
                    }
                }

                viewModel = new HomeViewModel
                {
                    User = user,
                    UserLocation = user?.Cidade,
                    GoogleCourts = googleCourts,
                    SelectedCidade = cidade,
                    SelectedBairro = bairro,
                    SelectedQuadraId = quadraId,
                    DataInicio = dataInicio ?? DateTime.Now.Date,
                    DataFim = dataFim ?? dataInicio ?? DateTime.Now.Date
                };
            }

            viewModel.Courts = await _context.Courts.Where(c => c.Ativa).ToListAsync();

            return View(viewModel);
        }

        private async Task<List<Court>> FiltrarQuadrasPorDisponibilidade(List<Court> courts, DateTime dataInicio, DateTime dataFim)
        {
            var courtsDisponiveis = new List<Court>();

            foreach (var court in courts)
            {
                var temDisponibilidade = await VerificarDisponibilidadeNoPeriodo(court.GooglePlaceId, dataInicio, dataFim);

                if (temDisponibilidade)
                {
                    courtsDisponiveis.Add(court);
                }
            }

            return courtsDisponiveis;
        }

        private async Task<bool> VerificarDisponibilidadeNoPeriodo(string? googlePlaceId, DateTime dataInicio, DateTime dataFim)
        {
            if (string.IsNullOrEmpty(googlePlaceId))
            {
                return true;
            }

            var reservasNoPeriodo = await _context.Reservations
                .Include(r => r.Court)
                .Where(r => r.Court.GooglePlaceId == googlePlaceId &&
                           r.Status != StatusReservation.Cancelada &&
                           r.DataHora.Date >= dataInicio.Date &&
                           r.DataHora.Date <= dataFim.Date)
                .ToListAsync();

            if (!reservasNoPeriodo.Any())
            {
                return true;
            }

            var diasNoPeriodo = (dataFim - dataInicio).Days + 1;
            var horariosDisponiveis = 15; 
            var totalHorariosNoPeriodo = diasNoPeriodo * horariosDisponiveis;
            var horariosReservados = reservasNoPeriodo.Sum(r => r.DuracaoHoras);

            return horariosReservados < totalHorariosNoPeriodo;
        }

        [HttpPost]
        public async Task<IActionResult> QuickSearch(string cidade, string bairro, DateTime? dataInicio, DateTime? dataFim)
        {
            if (string.IsNullOrEmpty(cidade))
            {
                TempData["Error"] = "Digite uma cidade para buscar.";
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index", new
            {
                cidade = cidade,
                bairro = bairro,
                dataInicio = dataInicio,
                dataFim = dataFim
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetQuadras(string cidade, string bairro, DateTime? dataInicio, DateTime? dataFim)
        {
            if (string.IsNullOrEmpty(cidade))
            {
                return Json(new { success = false, message = "Cidade é obrigatória" });
            }

            try
            {
                var searchLocation = cidade;
                if (!string.IsNullOrEmpty(bairro))
                {
                    searchLocation = $"{bairro}, {cidade}";
                }

                var quadras = await _googlePlacesService.SearchFootballCourtsAsync(searchLocation);

                if (dataInicio.HasValue || dataFim.HasValue)
                {
                    var dataIni = dataInicio ?? DateTime.Now.Date;
                    var dataFin = dataFim ?? dataInicio ?? DateTime.Now.Date;

                    if (dataFin < dataIni)
                    {
                        dataFin = dataIni;
                    }

                    quadras = await FiltrarQuadrasPorDisponibilidade(quadras, dataIni, dataFin);
                }

                var result = quadras.Select(q => new
                {
                    id = q.GooglePlaceId,
                    nome = q.Nome,
                    localizacao = q.Localizacao,
                    preco = q.PrecoPorHora,
                    rating = q.GoogleRating,
                    latitude = q.Latitude,
                    longitude = q.Longitude
                }).ToList();

                return Json(new { success = true, quadras = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar quadras: {ex.Message}");
                return Json(new { success = false, message = "Erro ao buscar quadras. Tente novamente." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReserveGoogleCourt(string googlePlaceId, string courtName, string courtLocation, DateTime dataHora, int duracaoHoras = 1, string? observacoes = null)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Usuário não autenticado." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users
                .Include(u => u.SelectedTeam)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.SelectedTeam == null)
            {
                return Json(new { success = false, message = "Selecione um time no seu perfil antes de fazer uma reserva." });
            }

            var virtualCourt = await _context.Courts.FirstOrDefaultAsync(c => c.GooglePlaceId == googlePlaceId);

            if (virtualCourt == null)
            {
                virtualCourt = new Court
                {
                    Nome = courtName,
                    Localizacao = courtLocation,
                    GooglePlaceId = googlePlaceId,
                    PrecoPorHora = 80.00m,
                    Ativa = true,
                    IsFromGoogle = true
                };

                _context.Courts.Add(virtualCourt);
                await _context.SaveChangesAsync();
            }

            var dataFim = dataHora.AddHours(duracaoHoras);
            var conflictingReservation = await _context.Reservations
                .Where(r => r.CourtId == virtualCourt.Id &&
                           r.Status != StatusReservation.Cancelada &&
                           ((r.DataHora <= dataHora && r.DataHora.AddHours(r.DuracaoHoras) > dataHora) ||
                            (r.DataHora < dataFim && r.DataHora >= dataHora)))
                .FirstOrDefaultAsync();

            if (conflictingReservation != null)
            {
                return Json(new { success = false, message = "Este horário já está reservado." });
            }

            var reservation = new Reservation
            {
                UserId = userId,
                TeamId = user.SelectedTeam.Id,
                CourtId = virtualCourt.Id,
                DataHora = dataHora,
                DuracaoHoras = duracaoHoras,
                Observacoes = observacoes,
                Status = StatusReservation.Pendente
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Horário agendado com sucesso para {courtName}! Outros times já podem enviar desafios para jogar contra vocês.",
                title = "Horário Agendado!"
            });
        }

        [HttpPost]
        public async Task<IActionResult> AcceptGoogleCourtChallenge(int reservationId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Usuário não autenticado." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users
                .Include(u => u.SelectedTeam)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.SelectedTeam == null)
            {
                return Json(new { success = false, message = "Selecione um time no seu perfil." });
            }

            var reservation = await _context.Reservations
                .Include(r => r.Court)
                .Include(r => r.Team)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == reservationId &&
                                        r.Status == StatusReservation.Pendente &&
                                        r.OpponentTeamId == null);

            if (reservation == null)
            {
                return Json(new { success = false, message = "Desafio não encontrado ou não disponível." });
            }

            if (reservation.Team == null)
            {
                return Json(new { success = false, message = "Erro ao carregar informações do time. Tente novamente." });
            }

            if (reservation.TeamId == user.SelectedTeam.Id)
            {
                return Json(new { success = false, message = "Você não pode desafiar seu próprio time." });
            }

            try
            {
                await _notificationService.CriarNotificacaoDesafioEnviado(
                    userId,
                    reservationId,
                    reservation.Team.Nome
                );

                await _notificationService.CriarNotificacaoDesafioRecebido(
                    reservation.UserId,
                    reservationId,
                    user.SelectedTeam.Nome
                );

                reservation.OpponentUserId = userId;
                reservation.OpponentTeamId = user.SelectedTeam.Id;
                reservation.Status = StatusReservation.Confirmada;

                await _context.SaveChangesAsync();

                await _notificationService.CriarNotificacaoDesafioAceito(
                    userId, 
                    reservation.UserId, 
                    reservationId
                );

                return Json(new
                {
                    success = true,
                    message = $"Desafio aceito! Partida confirmada contra {reservation.Team.Nome}. Ambos os times foram notificados.",
                    title = "Desafio Aceito!"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar desafio aceito: {ex.Message}");
                return Json(new { success = false, message = "Erro ao confirmar desafio. Tente novamente." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Usuário não autenticado." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var notifications = await _notificationService.BuscarNotificacoesUsuario(userId);
                var unreadCount = await _notificationService.ContarNotificacoesNaoLidas(userId);

                var result = notifications.Select(n => new
                {
                    id = n.Id,
                    titulo = n.Titulo,
                    mensagem = n.Mensagem,
                    tipo = n.Tipo.ToString(),
                    lida = n.Lida,
                    dataCriacao = n.DataCriacao.ToString("dd/MM HH:mm"),
                    reservationId = n.ReservationId
                }).ToList();

                return Json(new
                {
                    success = true,
                    notifications = result,
                    unreadCount = unreadCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar notificações: {ex.Message}");
                return Json(new { success = false, message = "Erro ao carregar notificações." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Usuário não autenticado." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                await _notificationService.MarcarComoLida(notificationId, userId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao marcar notificação como lida: {ex.Message}");
                return Json(new { success = false, message = "Erro ao marcar notificação." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Usuário não autenticado." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                await _notificationService.MarcarTodasComoLidas(userId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao marcar todas as notificações como lidas: {ex.Message}");
                return Json(new { success = false, message = "Erro ao marcar notificações." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TestGoogleAdvanced(string location)
        {
            try
            {
                var courts = await _googlePlacesService.SearchFootballCourtsAsync(location, 20000);

                return Json(new
                {
                    success = true,
                    count = courts.Count,
                    location = location,
                    courts = courts.Select(c => new
                    {
                        c.Nome,
                        c.Localizacao,
                        c.GooglePlaceId,
                        c.GoogleRating,
                        c.PrecoPorHora,
                        c.Descricao
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    location = location
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCourtDetails(string placeId, string name, string location, decimal price, double? lat, double? lng, decimal? rating)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users
                .Include(u => u.SelectedTeam)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.SelectedTeam == null)
            {
                TempData["Error"] = "Selecione um time no seu perfil antes de visualizar detalhes da quadra.";
                return RedirectToAction("Index", "Profile");
            }

            var court = new Court
            {
                GooglePlaceId = placeId,
                Nome = name,
                Localizacao = location,
                PrecoPorHora = price,
                Latitude = lat,
                Longitude = lng,
                GoogleRating = rating,
                IsFromGoogle = true,
                Ativa = true,
                Descricao = $"⭐ {rating?.ToString("F1") ?? "N/A"} • Quadra encontrada no Google Maps"
            };

            var dates = new List<DateTime>();
            for (int i = 0; i < 7; i++)
            {
                dates.Add(DateTime.Now.Date.AddDays(i));
            }

            var timeSlots = new List<string>
            {
                "06:00", "07:00", "08:00", "09:00", "10:00", "11:00",
                "12:00", "13:00", "14:00", "15:00", "16:00", "17:00",
                "18:00", "19:00", "20:00", "21:00", "22:00"
            };

            var allReservations = await _context.Reservations
                .Include(r => r.Team)           
                .Include(r => r.User)           
                .Include(r => r.OpponentTeam)   
                .Include(r => r.OpponentUser)   
                .Include(r => r.Court)         
                .Where(r => r.Court.GooglePlaceId == placeId &&
                           r.Status != StatusReservation.Cancelada &&
                           r.DataHora >= DateTime.Now.Date &&
                           r.DataHora < DateTime.Now.Date.AddDays(7))
                .OrderBy(r => r.DataHora)
                .ToListAsync();

            var viewModel = new GoogleCourtDetailsViewModel
            {
                Court = court,
                AvailableDates = dates,
                TimeSlots = timeSlots,
                AllReservations = allReservations,
                UserTeam = user.SelectedTeam,
                CurrentUserId = userId
            };

            return View(viewModel);
        }

        public async Task<IActionResult> CourtDetails(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var court = await _context.Courts.FirstOrDefaultAsync(c => c.Id == id && c.Ativa);
            if (court == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users
                .Include(u => u.SelectedTeam)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.SelectedTeam == null)
            {
                TempData["Error"] = "Selecione um time no seu perfil antes de fazer uma reserva.";
                return RedirectToAction("Index", "Profile");
            }
            var dates = new List<DateTime>();
            for (int i = 0; i < 7; i++)
            {
                dates.Add(DateTime.Now.Date.AddDays(i));
            }

            var timeSlots = new List<string>
            {
                "06:00", "07:00", "08:00", "09:00", "10:00", "11:00",
                "12:00", "13:00", "14:00", "15:00", "16:00", "17:00",
                "18:00", "19:00", "20:00", "21:00", "22:00"
            };

            var existingReservations = await _context.Reservations
                .Where(r => r.CourtId == id &&
                           r.Status != StatusReservation.Cancelada &&
                           r.DataHora >= DateTime.Now.Date &&
                           r.DataHora < DateTime.Now.Date.AddDays(7))
                .Select(r => new { r.DataHora, r.DuracaoHoras })
                .ToListAsync();

            var viewModel = new CourtDetailsViewModel
            {
                Court = court,
                AvailableDates = dates,
                TimeSlots = timeSlots,
                ExistingReservations = existingReservations.ToDictionary(
                    r => r.DataHora.Date,
                    r => r.DataHora.TimeOfDay
                ),
                UserTeam = user.SelectedTeam
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> QuickReservation(int courtId, DateTime dataHora, int duracaoHoras = 1, string? observacoes = null)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users
                .Include(u => u.SelectedTeam)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.SelectedTeam == null)
            {
                TempData["Error"] = "Selecione um time no seu perfil antes de fazer uma reserva.";
                return RedirectToAction("Index", "Profile");
            }

            var dataFim = dataHora.AddHours(duracaoHoras);
            var conflictingReservation = await _context.Reservations
                .Where(r => r.CourtId == courtId &&
                           r.Status != StatusReservation.Cancelada &&
                           ((r.DataHora <= dataHora && r.DataHora.AddHours(r.DuracaoHoras) > dataHora) ||
                            (r.DataHora < dataFim && r.DataHora >= dataHora)))
                .FirstOrDefaultAsync();

            if (conflictingReservation != null)
            {
                TempData["Error"] = "Este horário já está reservado.";
                return RedirectToAction("Index");
            }

            var reservation = new Reservation
            {
                UserId = userId,
                TeamId = user.SelectedTeam.Id,
                CourtId = courtId,
                DataHora = dataHora,
                DuracaoHoras = duracaoHoras,
                Observacoes = observacoes,
                Status = StatusReservation.Pendente
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Reserva criada com sucesso! Aguardando outro time aceitar o desafio.";
            return RedirectToAction("Index", "Reservation");
        }

        private async Task<User?> GetCurrentUserWithTeamAsync()
        {
            if (!User.Identity.IsAuthenticated)
                return null;

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            return await _context.Users
                .Include(u => u.SelectedTeam)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
        public override ViewResult View(object? model)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var currentUser = _context.Users
                    .Include(u => u.SelectedTeam)
                    .FirstOrDefault(u => u.Id == userId);

                ViewBag.CurrentUser = currentUser;
                ViewBag.CurrentTeam = currentUser?.SelectedTeam;
            }

            return base.View(model);
        }

        public override ViewResult View(string? viewName, object? model)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var currentUser = _context.Users
                    .Include(u => u.SelectedTeam)
                    .FirstOrDefault(u => u.Id == userId);

                ViewBag.CurrentUser = currentUser;
                ViewBag.CurrentTeam = currentUser?.SelectedTeam;
            }

            return base.View(viewName, model);
        }

        [HttpGet]
        public async Task<IActionResult> GetTeamInfo(int teamId)
        {
            try
            {
                var team = await _context.Teams
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == teamId);

                if (team == null)
                {
                    return Json(new { success = false, message = "Time não encontrado." });
                }

                var teamMatches = await _context.Reservations
                    .Include(r => r.Team)
                    .Include(r => r.OpponentTeam)
                    .Where(r => (r.TeamId == teamId || r.OpponentTeamId == teamId) &&
                               r.Status == StatusReservation.Finalizada &&
                               r.ResultadoConfirmado)
                    .ToListAsync();

                int victories = 0, defeats = 0, draws = 0, goalsScored = 0, goalsConceded = 0;

                foreach (var match in teamMatches)
                {
                    bool isTeam1 = match.TeamId == teamId;
                    var myResult = isTeam1 ? match.ResultadoTime1 : match.ResultadoTime2;
                    var myGoals = isTeam1 ? match.GolsTime1 : match.GolsTime2;
                    var opponentGoals = isTeam1 ? match.GolsTime2 : match.GolsTime1;

                    switch (myResult)
                    {
                        case ResultadoPartida.Vitoria:
                            victories++;
                            break;
                        case ResultadoPartida.Derrota:
                            defeats++;
                            break;
                        case ResultadoPartida.Empate:
                            draws++;
                            break;
                    }

                    goalsScored += myGoals ?? 0;
                    goalsConceded += opponentGoals ?? 0;
                }

                var totalMatches = teamMatches.Count;
                var winRate = totalMatches > 0 ? Math.Round((double)victories / totalMatches * 100, 1) : 0;

                var upcomingMatches = await _context.Reservations
                    .Include(r => r.Court)
                    .Include(r => r.OpponentTeam)
                    .Where(r => (r.TeamId == teamId || r.OpponentTeamId == teamId) &&
                               r.Status == StatusReservation.Confirmada &&
                               r.DataHora >= DateTime.Now)
                    .OrderBy(r => r.DataHora)
                    .Take(3)
                    .Select(r => new
                    {
                        dataHora = r.DataHora.ToString("dd/MM HH:mm"),
                        quadra = r.Court.Nome,
                        oponente = r.TeamId == teamId ? r.OpponentTeam.Nome : r.Team.Nome
                    })
                    .ToListAsync();

                var teamInfo = new
                {
                    success = true,
                    team = new
                    {
                        id = team.Id,
                        nome = team.Nome,
                        descricao = team.Descricao ?? "Nenhuma descrição disponível",
                        fotoUrl = team.FotoUrl,
                        corPrimaria = team.CorPrimaria ?? "#007bff",
                        corSecundaria = team.CorSecundaria,
                        faixaIdade = team.FaixaIdadeDisplay,
                        criador = team.User.Nome,
                        dataCriacao = team.DataCriacao.ToString("dd/MM/yyyy")
                    },
                    stats = new
                    {
                        totalPartidas = totalMatches,
                        vitorias = victories,
                        derrotas = defeats,
                        empates = draws,
                        golsFeitos = goalsScored,
                        golsSofridos = goalsConceded,
                        saldoGols = goalsScored - goalsConceded,
                        taxaVitoria = winRate,
                        performance = winRate >= 60 ? "Excelente" : winRate >= 40 ? "Boa" : "Regular"
                    },
                    proximasPartidas = upcomingMatches
                };

                return Json(teamInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar informações do time: {ex.Message}");
                return Json(new { success = false, message = "Erro ao carregar informações do time." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckPendingMatchResults()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Usuário não autenticado." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users
                .Include(u => u.SelectedTeam)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.SelectedTeam == null)
            {
                return Json(new { success = false, message = "Nenhum time selecionado." });
            }

            var now = DateTime.Now;
            var pendingMatch = await _context.Reservations
                .Include(r => r.Court)
                .Include(r => r.Team)
                .Include(r => r.OpponentTeam)
                .Where(r => (r.TeamId == user.SelectedTeam.Id || r.OpponentTeamId == user.SelectedTeam.Id) &&
                           r.Status == StatusReservation.Confirmada &&
                           r.DataHora.AddHours(r.DuracaoHoras) <= now &&
                           ((r.TeamId == user.SelectedTeam.Id && !r.ResultadoInformadoTime1) || 
                            (r.OpponentTeamId == user.SelectedTeam.Id && !r.ResultadoInformadoTime2))) 
                .OrderBy(r => r.DataHora)
                .FirstOrDefaultAsync();

            if (pendingMatch == null)
            {
                return Json(new { success = false, message = "Nenhuma partida pendente de resultado." });
            }

            var isTeam1 = pendingMatch.TeamId == user.SelectedTeam.Id;
            var myTeam = isTeam1 ? pendingMatch.Team : pendingMatch.OpponentTeam;
            var opponentTeam = isTeam1 ? pendingMatch.OpponentTeam : pendingMatch.Team;

            var otherTeamInformed = isTeam1 ? pendingMatch.ResultadoInformadoTime2 : pendingMatch.ResultadoInformadoTime1;
            var otherTeamResult = "";

            if (otherTeamInformed)
            {
                var otherMyGoals = isTeam1 ?
                    (pendingMatch.GolsTime1_InformadoPeloTime2 ?? 0) :
                    (pendingMatch.GolsTime2_InformadoPeloTime1 ?? 0);
                var otherOpponentGoals = isTeam1 ?
                    (pendingMatch.GolsTime2_InformadoPeloTime2 ?? 0) :
                    (pendingMatch.GolsTime1_InformadoPeloTime1 ?? 0);

                otherTeamResult = $"{otherOpponentGoals}x{otherMyGoals}";
            }

            return Json(new
            {
                success = true,
                match = new
                {
                    id = pendingMatch.Id,
                    dataHora = pendingMatch.DataHora.ToString("dd/MM/yyyy HH:mm"),
                    quadra = pendingMatch.Court.Nome,
                    meuTime = new { id = myTeam.Id, nome = myTeam.Nome },
                    timeAdversario = new { id = opponentTeam.Id, nome = opponentTeam.Nome },
                    duracao = pendingMatch.DuracaoHoras,
                    isTeam1 = isTeam1,
                    outroTimeInformou = otherTeamInformed,
                    resultadoOutroTime = otherTeamResult
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitMatchResult(int reservationId, int myGoals, int opponentGoals)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Usuário não autenticado." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users
                .Include(u => u.SelectedTeam)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.SelectedTeam == null)
            {
                return Json(new { success = false, message = "Nenhum time selecionado." });
            }

            if (myGoals < 0 || opponentGoals < 0)
            {
                return Json(new { success = false, message = "O número de gols não pode ser negativo." });
            }

            if (myGoals > 50 || opponentGoals > 50)
            {
                return Json(new { success = false, message = "Número de gols muito alto. Máximo permitido: 50." });
            }

            var reservation = await _context.Reservations
                .Include(r => r.Team)
                .Include(r => r.OpponentTeam)
                .Include(r => r.Court)
                .FirstOrDefaultAsync(r => r.Id == reservationId &&
                                        (r.TeamId == user.SelectedTeam.Id || r.OpponentTeamId == user.SelectedTeam.Id) &&
                                        r.Status == StatusReservation.Confirmada);

            if (reservation == null)
            {
                return Json(new { success = false, message = "Partida não encontrada." });
            }

            var matchEndTime = reservation.DataHora.AddHours(reservation.DuracaoHoras);
            if (DateTime.Now < matchEndTime)
            {
                return Json(new { success = false, message = "A partida ainda não terminou." });
            }

            var isTeam1 = reservation.TeamId == user.SelectedTeam.Id;

            if ((isTeam1 && reservation.ResultadoInformadoTime1) ||
                (!isTeam1 && reservation.ResultadoInformadoTime2))
            {
                return Json(new { success = false, message = "Seu time já informou o resultado desta partida." });
            }

            try
            {
                if (isTeam1)
                {
                    reservation.ResultadoInformadoTime1 = true;
                    reservation.DataResultadoTime1 = DateTime.Now;
                    reservation.GolsTime1_InformadoPeloTime1 = myGoals;
                    reservation.GolsTime2_InformadoPeloTime1 = opponentGoals;
                }
                else
                {
                    reservation.ResultadoInformadoTime2 = true;
                    reservation.DataResultadoTime2 = DateTime.Now;
                    reservation.GolsTime1_InformadoPeloTime2 = opponentGoals;
                    reservation.GolsTime2_InformadoPeloTime2 = myGoals;
                }

                if (reservation.ResultadoInformadoTime1 && reservation.ResultadoInformadoTime2)
                {
                    var golsTime1_PorTime1 = reservation.GolsTime1_InformadoPeloTime1 ?? 0;
                    var golsTime2_PorTime1 = reservation.GolsTime2_InformadoPeloTime1 ?? 0;
                    var golsTime1_PorTime2 = reservation.GolsTime1_InformadoPeloTime2 ?? 0;
                    var golsTime2_PorTime2 = reservation.GolsTime2_InformadoPeloTime2 ?? 0;

                    if (golsTime1_PorTime1 == golsTime1_PorTime2 && golsTime2_PorTime1 == golsTime2_PorTime2)
                    {
                        await ConfirmarResultadoFinal(reservation, golsTime1_PorTime1, golsTime2_PorTime1);
                    }
                    else
                    {
                        reservation.StatusResultado = StatusResultado.Conflito;
                    }
                }
                else
                {
                    reservation.StatusResultado = isTeam1 ? StatusResultado.ParcialTime1 : StatusResultado.ParcialTime2;
                }

                await _context.SaveChangesAsync();

                var otherUserId = isTeam1 ? reservation.OpponentUserId : reservation.UserId;
                var otherTeam = isTeam1 ? reservation.OpponentTeam : reservation.Team;
                var myTeam = isTeam1 ? reservation.Team : reservation.OpponentTeam;

                if (otherUserId.HasValue)
                {
                    if (reservation.StatusResultado == StatusResultado.Confirmado)
                    {
                        await _notificationService.CriarNotificacaoResultadoConfirmado(
                            otherUserId.Value, reservationId, myTeam.Nome, otherTeam.Nome, myGoals, opponentGoals);
                    }
                    else if (reservation.StatusResultado == StatusResultado.Conflito)
                    {
                        await _notificationService.CriarNotificacaoConflito(
                            otherUserId.Value, reservationId, myTeam.Nome, otherTeam.Nome);
                    }
                    else
                    {
                        await _notificationService.CriarNotificacaoResultadoPendente(
                            otherUserId.Value, reservationId, myTeam.Nome, otherTeam.Nome, myGoals, opponentGoals);
                    }
                }

                var message = GetResultMessage(reservation.StatusResultado, myGoals, opponentGoals);

                return Json(new
                {
                    success = true,
                    message = message.message,
                    status = reservation.StatusResultado.ToString(),
                    conflito = reservation.StatusResultado == StatusResultado.Conflito,
                    confirmado = reservation.StatusResultado == StatusResultado.Confirmado,
                    result = new
                    {
                        myGoals = myGoals,
                        opponentGoals = opponentGoals,
                        status = reservation.StatusResultado.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar resultado da partida: {ex.Message}");
                return Json(new { success = false, message = "Erro ao salvar o resultado. Tente novamente." });
            }
        }

        private async Task ConfirmarResultadoFinal(Reservation reservation, int golsTime1, int golsTime2)
        {
            reservation.GolsTime1Final = golsTime1;
            reservation.GolsTime2Final = golsTime2;
            reservation.DataResultadoFinal = DateTime.Now;
            reservation.StatusResultado = StatusResultado.Confirmado;
            reservation.Status = StatusReservation.Finalizada;

            if (golsTime1 > golsTime2)
            {
                reservation.ResultadoTime1Final = ResultadoPartida.Vitoria;
                reservation.ResultadoTime2Final = ResultadoPartida.Derrota;
            }
            else if (golsTime1 < golsTime2)
            {
                reservation.ResultadoTime1Final = ResultadoPartida.Derrota;
                reservation.ResultadoTime2Final = ResultadoPartida.Vitoria;
            }
            else
            {
                reservation.ResultadoTime1Final = ResultadoPartida.Empate;
                reservation.ResultadoTime2Final = ResultadoPartida.Empate;
            }
        }

        private (string message, string type) GetResultMessage(StatusResultado status, int myGoals, int opponentGoals)
        {
            return status switch
            {
                StatusResultado.ParcialTime1 or StatusResultado.ParcialTime2 =>
                    ("Resultado salvo! Aguardando confirmação do time adversário.", "info"),
                StatusResultado.Confirmado =>
                    myGoals > opponentGoals ? ("Parabéns pela vitória! Resultado confirmado por ambos os times.", "success") :
                    myGoals < opponentGoals ? ("Resultado confirmado por ambos os times. Que venha a próxima!", "info") :
                    ("Empate confirmado por ambos os times!", "info"),
                StatusResultado.Conflito =>
                    ("Há divergência nos resultados informados pelos times. Um administrador irá resolver.", "warning"),
                _ => ("Resultado salvo com sucesso!", "success")
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetResultConflicts()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Não autorizado" });
            }

            var conflicts = await _context.Reservations
                .Include(r => r.Court)
                .Include(r => r.Team)
                .Include(r => r.OpponentTeam)
                .Where(r => r.StatusResultado == StatusResultado.Conflito)
                .Select(r => new
                {
                    id = r.Id,
                    dataHora = r.DataHora.ToString("dd/MM/yyyy HH:mm"),
                    quadra = r.Court.Nome,
                    time1 = r.Team.Nome,
                    time2 = r.OpponentTeam.Nome,
                    resultadoTime1 = $"{r.GolsTime1_InformadoPeloTime1}x{r.GolsTime2_InformadoPeloTime1}",
                    resultadoTime2 = $"{r.GolsTime1_InformadoPeloTime2}x{r.GolsTime2_InformadoPeloTime2}",
                    dataTime1 = r.DataResultadoTime1,
                    dataTime2 = r.DataResultadoTime2
                })
                .ToListAsync();

            return Json(new { success = true, conflicts });
        }
    }
}
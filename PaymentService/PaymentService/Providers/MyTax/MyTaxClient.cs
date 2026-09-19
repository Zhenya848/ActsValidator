using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PaymentService.Options;
using PaymentService.Providers.MyTax.Stores;

namespace PaymentService.Providers.MyTax
{
    // ============================================================
    // ВНИМАНИЕ: это НЕОФИЦИАЛЬНЫЙ API lknpd.nalog.ru ("Мой налог").
    // ФНС его не документирует и не поддерживает публично — он может
    // измениться в любой момент без предупреждения. Используйте на
    // свой риск и не публикуйте пароль/токены самозанятого в логах.
    // ============================================================
 
    internal sealed class DeviceInfo
    {
        [JsonPropertyName("sourceDeviceId")]
        public string SourceDeviceId { get; set; } = default!;
 
        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; } = "WEB";
 
        [JsonPropertyName("appVersion")]
        public string AppVersion { get; set; } = "1.0.0";
 
        [JsonPropertyName("metaDetails")]
        public MetaDetails MetaDetails { get; set; } = new();
    }
 
    internal sealed class MetaDetails
    {
        [JsonPropertyName("browser")]
        public string Browser { get; set; } = "";
 
        [JsonPropertyName("browserVersion")]
        public string BrowserVersion { get; set; } = "";
 
        [JsonPropertyName("os")]
        public string Os { get; set; } = "web";
    }
 
    internal sealed class AuthByPasswordRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = default!;
 
        [JsonPropertyName("password")]
        public string Password { get; set; } = default!;
 
        [JsonPropertyName("deviceInfo")]
        public DeviceInfo DeviceInfo { get; set; } = default!;
    }
 
    internal sealed class RefreshTokenRequest
    {
        [JsonPropertyName("deviceInfo")]
        public DeviceInfo DeviceInfo { get; set; } = default!;
 
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = default!;
    }
 
    internal sealed class AuthResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = default!;
 
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = default!;
 
        // Дата истечения токена, присылается сервисом (ISO 8601)
        [JsonPropertyName("tokenExpireIn")]
        public DateTimeOffset TokenExpireIn { get; set; }
    }
 
    // ===== DTO: чек (income) =====
 
    public sealed class ReceiptItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
 
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
 
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 1;
    }
 
    internal sealed class ClientInfo
    {
        [JsonPropertyName("contactPhone")]
        public string? ContactPhone { get; set; }
 
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }
 
        [JsonPropertyName("inn")]
        public string? Inn { get; set; }
 
        // FROM_INDIVIDUAL — физлицо, FROM_LEGAL_ENTITY — юрлицо/ИП, FROM_FOREIGN_AGENCY — иностранная организация
        [JsonPropertyName("incomeType")]
        public string IncomeType { get; set; } = "FROM_INDIVIDUAL";
    }
 
    internal sealed class CreateIncomeRequest
    {
        [JsonPropertyName("paymentType")]
        public string PaymentType { get; set; } = "CASH"; // CASH или ACCOUNT (безнал)
 
        [JsonPropertyName("ignoreMaxTotalIncomeRestriction")]
        public bool IgnoreMaxTotalIncomeRestriction { get; set; } = false;
 
        [JsonPropertyName("client")]
        public ClientInfo Client { get; set; } = new();
 
        [JsonPropertyName("requestTime")]
        public string RequestTime { get; set; } = default!;
 
        [JsonPropertyName("operationTime")]
        public string OperationTime { get; set; } = default!;
 
        [JsonPropertyName("services")]
        public ReceiptItem[] Services { get; set; } = Array.Empty<ReceiptItem>();
 
        [JsonPropertyName("totalAmount")]
        public string TotalAmount { get; set; } = default!;
    }
 
    public sealed class CreateIncomeResult
    {
        [JsonPropertyName("approvedReceiptUuid")]
        public string ReceiptUuid { get; set; } = default!;
    }
 
    internal sealed class CancelIncomeRequest
    {
        [JsonPropertyName("comment")]
        public string Comment { get; set; } = default!;
 
        [JsonPropertyName("receiptUuid")]
        public string ReceiptUuid { get; set; } = default!;
 
        [JsonPropertyName("requestTime")]
        public string RequestTime { get; set; } = default!;
 
        [JsonPropertyName("operationTime")]
        public string OperationTime { get; set; } = default!;
    }
    
    internal sealed class SmsChallengeRequest
    {
        [JsonPropertyName("phone")]
        public string Phone { get; set; } = default!;
 
        [JsonPropertyName("requireTpToBeActive")]
        public bool RequireTpToBeActive { get; set; } = true;
    }
 
    internal sealed class SmsChallengeResponse
    {
        [JsonPropertyName("challengeToken")]
        public string ChallengeToken { get; set; } = default!;
 
        [JsonPropertyName("expireDate")]
        public DateTimeOffset ExpireDate { get; set; }
        // Код из SMS живёт ~2 минуты — новый запросить можно только
        // после истечения предыдущего или успешной авторизации по нему.
    }
 
    internal sealed class AuthByPhoneRequest
    {
        [JsonPropertyName("phone")]
        public string Phone { get; set; } = default!;
 
        [JsonPropertyName("challengeToken")]
        public string ChallengeToken { get; set; } = default!;
 
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;
 
        [JsonPropertyName("deviceInfo")]
        public DeviceInfo DeviceInfo { get; set; } = default!;
    }
 
    // ===== Клиент =====
 
    public interface IMoyNalogClient
    {
        Task<CreateIncomeResult> CreateReceiptAsync(
            ReceiptItem[] items,
            DateTimeOffset operationTime,
            CancellationToken cancellationToken = default);
 
        Task CancelReceiptAsync(
            string receiptUuid,
            string comment,
            CancellationToken cancellationToken = default);
 
        string GetPrintUrl(string receiptUuid);
    }
    
    public interface IMoyNalogBootstrap
    {
        // Шаг 1: запросить SMS-код на телефон, привязанный к аккаунту.
        // Возвращает challengeToken, который нужно передать на шаге 2.
        // Новый код можно запросить только после истечения предыдущего
        // (~2 минуты) или успешной авторизации по нему.
        Task<string> RequestSmsCodeAsync(string phone, CancellationToken cancellationToken = default);
 
        // Шаг 2: код из SMS вводит человек один раз — сюда его и передаём.
        // При успехе токены сохраняются в NalogTokenCache (и, если настроен,
        // в персистентном хранилище) — дальше сервис работает автономно
        // через refreshToken до его истечения примерно через год.
        Task AuthenticateByPhoneAsync(
            string phone,
            string challengeToken,
            string smsCode,
            CancellationToken cancellationToken = default);
    }
 
    public sealed class MoyNalogClient : IMoyNalogClient, IMoyNalogBootstrap
    {
        private const string BaseUrl = "https://lknpd.nalog.ru/api/v1";
 
        private readonly HttpClient _httpClient;
        private readonly TaxAuthOptions _options;
        
        private readonly TaxTokenCache _tokenCache;
 
        public MoyNalogClient(HttpClient httpClient, IOptions<TaxAuthOptions> options, TaxTokenCache tokenCache)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _tokenCache = tokenCache;
        }
 
        private DeviceInfo BuildDeviceInfo() => new()
        {
            SourceDeviceId = _options.SourceDeviceId
        };
 
        private static string ToIso(DateTimeOffset dt) =>
            dt.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
 
        // Гарантирует наличие валидного access_token в TaxTokenCache,
        // при необходимости авторизуется заново или обновляет токен по refreshToken.
        // Сам MoyNalogClient — Transient, но токен переживает его пересоздание,
        // т.к. лежит в Singleton-кэше, а не в полях этого класса.
        private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
        {
            await _tokenCache.EnsureLoadedFromStoreAsync(cancellationToken);
            
            if (_tokenCache.HasValidAccessToken)
            {
                ApplyBearerHeader(_tokenCache.AccessToken!);
                return;
            }
 
            using var releaser = await _tokenCache.LockAsync(cancellationToken);
 
            // Двойная проверка — вдруг другой вызов уже обновил токен, пока мы ждали лок
            if (_tokenCache.HasValidAccessToken)
            {
                ApplyBearerHeader(_tokenCache.AccessToken!);
                return;
            }
 
            AuthResponse auth;
 
            if (_tokenCache.RefreshToken is not null &&
                DateTimeOffset.UtcNow < _tokenCache.RefreshTokenExpiresAt)
            {
                var refreshBody = new RefreshTokenRequest
                {
                    DeviceInfo = BuildDeviceInfo(),
                    RefreshToken = _tokenCache.RefreshToken
                };
 
                var refreshResponse = await _httpClient.PostAsJsonAsync(
                    $"{BaseUrl}/auth/token", refreshBody, cancellationToken);
 
                if (refreshResponse.IsSuccessStatusCode)
                {
                    auth = (await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>(
                        cancellationToken: cancellationToken))!;
 
                    await ApplyAuthAsync(auth, cancellationToken);
                    return;
                }
                // Если refresh не сработал (истёк/невалиден) — падаем в обычную авторизацию ниже
            }
 
            var authBody = new AuthByPasswordRequest
            {
                Username = _options.Inn,
                Password = _options.Password,
                DeviceInfo = BuildDeviceInfo()
            };
 
            using var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/auth/lkfl", authBody, cancellationToken);
 
            response.EnsureSuccessStatusCode();
 
            auth = (await response.Content.ReadFromJsonAsync<AuthResponse>(
                cancellationToken: cancellationToken))!;
            
            await ApplyAuthAsync(auth, cancellationToken, isFreshPasswordLogin: true);
        }
 
        private async Task ApplyAuthAsync(
            AuthResponse auth,
            CancellationToken ct,
            bool isFreshPasswordLogin = false)
        {
            var refreshExpiresAt = isFreshPasswordLogin
                ? DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenExpiredInDays)
                : _tokenCache.RefreshTokenExpiresAt;
 
            await _tokenCache.SetAsync(
                auth.Token,
                auth.RefreshToken,
                auth.TokenExpireIn,
                refreshExpiresAt,
                ct);
 
            ApplyBearerHeader(auth.Token);
        }
 
        private void ApplyBearerHeader(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
        }
 
        public async Task<CreateIncomeResult> CreateReceiptAsync(
            ReceiptItem[] items,
            DateTimeOffset operationTime,
            CancellationToken cancellationToken = default)
        {
            if (items is null || items.Length == 0)
                throw new ArgumentException("Нужна хотя бы одна позиция чека", nameof(items));
 
            await EnsureAuthenticatedAsync(cancellationToken);
 
            decimal total = 0m;
            foreach (var item in items)
                total += item.Amount * item.Quantity;
 
            var now = DateTimeOffset.Now;
 
            var request = new CreateIncomeRequest
            {
                PaymentType = "CASH",
                Client = new ClientInfo { IncomeType = "FROM_INDIVIDUAL" },
                RequestTime = ToIso(now),
                OperationTime = ToIso(operationTime),
                Services = items,
                TotalAmount = total.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
            };
 
            using var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/income", request, cancellationToken);
 
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Не удалось создать чек. HTTP {(int)response.StatusCode}: {body}");
            }
 
            var result = await response.Content.ReadFromJsonAsync<CreateIncomeResult>(
                cancellationToken: cancellationToken);
 
            return result ?? throw new InvalidOperationException("Пустой ответ при создании чека.");
        }
 
        public async Task CancelReceiptAsync(
            string receiptUuid,
            string comment,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
 
            var now = DateTimeOffset.Now;
 
            var request = new CancelIncomeRequest
            {
                Comment = comment,
                ReceiptUuid = receiptUuid,
                RequestTime = ToIso(now),
                OperationTime = ToIso(now)
            };
 
            using var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/cancel", request, cancellationToken);
 
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Не удалось отменить чек. HTTP {(int)response.StatusCode}: {body}");
            }
        }
 
        public string GetPrintUrl(string receiptUuid) =>
            $"https://lknpd.nalog.ru/api/v1/receipt/{_options.Inn}/{receiptUuid}/print";

        public async Task<string> RequestSmsCodeAsync(string phone, CancellationToken cancellationToken = default)
        {
            var request = new SmsChallengeRequest { Phone = phone };
 
            using var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/auth/challenge/sms/start", request, cancellationToken);
 
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Не удалось запросить SMS-код. HTTP {(int)response.StatusCode}: {body}");
            }
 
            var result = await response.Content.ReadFromJsonAsync<SmsChallengeResponse>(
                cancellationToken: cancellationToken);
 
            return result?.ChallengeToken
                   ?? throw new InvalidOperationException("Пустой ответ при запросе SMS-кода.");
        }

        public async Task AuthenticateByPhoneAsync(string phone, string challengeToken, string smsCode,
            CancellationToken cancellationToken = default)
        {
            var request = new AuthByPhoneRequest
            {
                Phone = phone,
                ChallengeToken = challengeToken,
                Code = smsCode,
                DeviceInfo = BuildDeviceInfo()
            };
 
            using var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/auth/challenge/sms/verify", request, cancellationToken);
 
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Не удалось авторизоваться по SMS-коду. HTTP {(int)response.StatusCode}: {body}");
            }
 
            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(
                cancellationToken: cancellationToken);
 
            if (auth is null)
                throw new InvalidOperationException("Пустой ответ при авторизации по SMS.");
 
            // Как и при логине паролем — refreshToken свежий, считаем его
            // сроком жизни ~1 год от текущего момента.
            await ApplyAuthAsync(auth, cancellationToken, isFreshPasswordLogin: true);
        }
    }
}
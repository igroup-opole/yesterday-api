using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using IdentityModel.Client;
using IdentityServer.Core.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using YesterdayApi.Core.Identity.Web;
using YesterdayApi.Core.Users.Web;
using YesterdayApi.Utilities.Exceptions;

namespace YesterdayApi.Core.Identity
{
    public class IdentityService : IIdentityService
    {
        private const string DiscoveryUrl = "http://localhost:5000";
        private readonly IUserIdentityService _userIdentityService;
        private readonly ITokenDecoder _tokenDecoder;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

        public IdentityService(IUserIdentityService userIdentityService, ITokenDecoder tokenDecoder, IMapper mapper, IConfiguration configuration)
        {
            _userIdentityService = userIdentityService;
            _tokenDecoder = tokenDecoder;
            _mapper = mapper;
            _configuration = configuration;
            _client = new HttpClient();
        }

        public async Task<string> GetAccessToken(UserIdentityRequest userIdentityRequest)
        {
            var disco = await GetDiscoveryDocument();

            var tokenResponse = await RequestPasswordToken(disco.TokenEndpoint, userIdentityRequest);

            if (tokenResponse.IsError)
            {
                throw new BadRequestException("Error", tokenResponse.Error);
            }

            await _userIdentityService.UpdateUserRefreshToken(userIdentityRequest.UserName, tokenResponse.RefreshToken);
            return tokenResponse.AccessToken;
        }

        public Task<string> RefreshAccessToken(string authHeader)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> RegisterUser(UserRegistrationRequest userRegistrationRequest)
        {
            throw new NotImplementedException();
        }

        public Task SetAdmin(string userName)
        {
            throw new NotImplementedException();
        }

        private async Task<DiscoveryResponse> GetDiscoveryDocument()
        {
            var disco = await _client.GetDiscoveryDocumentAsync(DiscoveryUrl);
            if (disco.IsError)
            {
                throw new InternalServerErrorException("Error", "Encountered problems in the process of getting the discovery document.");
            }

            return disco;
        }

        private async Task<TokenResponse> RequestPasswordToken(string tokenEndpoint, UserIdentityRequest userRequest)
        {
            var tokenResponse = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest()
            {
                Address = tokenEndpoint,
                ClientId = _configuration.GetSection("IdentityConfig:ClientId").Value,
                ClientSecret = _configuration.GetSection("IdentityConfig:ClientSecret").Value,
                Scope = _configuration.GetSection("IdentityConfig:Scope").Value,
                UserName = userRequest.UserName,
                Password = userRequest.Password,
            });

            return tokenResponse;
        }
    }
}

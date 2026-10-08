using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using ViaCepLogger.Api.Controllers;
using ViaCepLogger.Api.Models;
using ViaCepLogger.Api.Services;
using Xunit;

namespace ViaCepLogger.Tests;

public class ViaCepContractAndUnitTests
{
    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }

    private static (CepController Controller, ViaCepService Service) CreateSut(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler(responder));
        var service = new ViaCepService(httpClient, NullLogger<ViaCepService>.Instance);
        var controller = new CepController(service, NullLogger<CepController>.Instance);
        return (controller, service);
    }

    [Fact]
    public async Task GetAddress_WhenCepIsValid_ReturnsOkWithData()
    {
        var json = "{\"cep\":\"01001-000\",\"logradouro\":\"Praça da Sé\",\"localidade\":\"São Paulo\",\"uf\":\"SP\"}";
        var (controller, _) = CreateSut(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });

        var result = await controller.GetAddress("01001000");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        var response = Assert.IsType<ViaCepResponse>(okResult.Value);
        Assert.Equal("01001-000", response.Cep);
        Assert.Equal("São Paulo", response.Localidade);
        Assert.Equal("SP", response.Uf);
    }

    [Fact]
    public async Task GetAddress_WhenCepNotFoundWithBooleanError_ReturnsNotFound()
    {
        var json = "{\"erro\":true}";
        var (controller, _) = CreateSut(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });

        var result = await controller.GetAddress("01001000");

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetAddress_WhenCepNotFoundWithStringError_ReturnsNotFound()
    {
        var json = "{\"erro\":\"true\"}";
        var (controller, _) = CreateSut(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });

        var result = await controller.GetAddress("01001000");

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetAddress_WhenUpstreamReturnsServiceUnavailable_ReturnsBadGateway()
    {
        var (controller, _) = CreateSut(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

        var result = await controller.GetAddress("01001000");

        var objResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(502, objResult.StatusCode);
    }

    [Fact]
    public async Task GetAddress_WhenUpstreamReturnsInvalidJson_ReturnsBadGateway()
    {
        var (controller, _) = CreateSut(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{invalid_payload: true")
        });

        var result = await controller.GetAddress("01001000");

        var objResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(502, objResult.StatusCode);
    }

    [Fact]
    public async Task GetAddress_WhenUpstreamTimesOut_ReturnsGatewayTimeout()
    {
        var (controller, _) = CreateSut(_ => throw new TaskCanceledException("Simulated timeout"));

        var result = await controller.GetAddress("01001000");

        var objResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(504, objResult.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("123456789")]
    [InlineData("abcdefgh")]
    public async Task GetAddress_WhenCepIsInvalidOrEmpty_ReturnsBadRequest(string? invalidCep)
    {
        var (controller, _) = CreateSut(_ => new HttpResponseMessage(HttpStatusCode.OK));

        var result = await controller.GetAddress(invalidCep);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }
}

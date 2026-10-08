using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Api.Controllers;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ApiVersioning.Tests;

public class ApiVersioningUnitAndContractTests
{
    [Fact]
    public async Task CustomerController_V1_ReturnsExpectedContractAndDocumentString()
    {
        var controller = new CustomerController();
        var actionResult = await controller.GetCustomerByIdAsync(42);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okResult.StatusCode);

        var customer = Assert.IsType<CustomerModel>(okResult.Value);
        Assert.Equal(42, customer.Id);
        Assert.Equal("123.456.789-00", customer.Document);
        Assert.Equal("John Doe", customer.Name);
        Assert.Equal("john.doe@gmail.com", customer.Email);
        Assert.Equal("+1-202-555-0143", customer.PhoneNumber);
    }

    [Fact]
    public async Task CustomerController_V2_ReturnsExpectedContractAndDocumentNumber()
    {
        var controller = new CustomerController();
        var actionResult = await controller.GetCustomerByIdV2Async(99);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okResult.StatusCode);

        var customer = Assert.IsType<CustomerModelV2>(okResult.Value);
        Assert.Equal(99, customer.Id);
        Assert.Equal(12345678900L, customer.Document);
        Assert.Equal("John Doe", customer.Name);
        Assert.Equal("john.doe@gmail.com", customer.Email);
        Assert.Equal("+1-202-555-0143", customer.PhoneNumber);
    }

    [Fact]
    public void CustomerController_DeclaresApiVersionsAndDeprecation()
    {
        var type = typeof(CustomerController);
        var apiVersionAttrs = type.GetCustomAttributes<ApiVersionAttribute>().ToList();

        Assert.Contains(apiVersionAttrs, a => a.Versions.Any(v => v.ToString() == "1.0") && a.Deprecated);
        Assert.Contains(apiVersionAttrs, a => a.Versions.Any(v => v.ToString() == "2.0") && !a.Deprecated);

        var v1Method = type.GetMethod(nameof(CustomerController.GetCustomerByIdAsync));
        Assert.NotNull(v1Method);
        Assert.True(v1Method.IsDefined(typeof(ObsoleteAttribute), inherit: false));
    }

    [Fact]
    public async Task UserController_V1_GetUsersAsync_ReturnsAllPredefinedUsers()
    {
        var controller = new UserController();
        var actionResult = await controller.GetUsersAsync();

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okResult.StatusCode);

        var users = Assert.IsAssignableFrom<IEnumerable<UserModel>>(okResult.Value).ToList();
        Assert.Equal(3, users.Count);
        Assert.Contains(users, u => u.Id == 1 && u.UserName == "alice.smith");
    }

    [Fact]
    public async Task UserController_V2_GetUsersV2Async_ReturnsAllPredefinedUsers()
    {
        var controller = new UserController();
        var actionResult = await controller.GetUsersV2Async();

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okResult.StatusCode);

        var users = Assert.IsAssignableFrom<IEnumerable<UserModel>>(okResult.Value).ToList();
        Assert.Equal(3, users.Count);
        Assert.Contains(users, u => u.Id == 2 && u.UserName == "bob.johnson");
    }

    [Fact]
    public async Task UserController_V1_GetUserByIdAsync_WhenFound_ReturnsUser()
    {
        var controller = new UserController();
        var actionResult = await controller.GetUserByIdAsync(1);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okResult.StatusCode);

        var user = Assert.IsType<UserModel>(okResult.Value);
        Assert.Equal(1, user.Id);
        Assert.Equal("Alice Smith", user.Name);
    }

    [Fact]
    public async Task UserController_V1_GetUserByIdAsync_WhenNotFound_ReturnsNotFound()
    {
        var controller = new UserController();
        var actionResult = await controller.GetUserByIdAsync(999);

        var notFoundResult = Assert.IsType<NotFoundResult>(actionResult.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task UserController_V2_GetUserById2Async_WhenFound_ReturnsUser()
    {
        var controller = new UserController();
        var actionResult = await controller.GetUserById2Async(3);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okResult.StatusCode);

        var user = Assert.IsType<UserModel>(okResult.Value);
        Assert.Equal(3, user.Id);
        Assert.Equal("Charlie Brown", user.Name);
    }

    [Fact]
    public async Task UserController_V2_GetUserById2Async_WhenNotFound_ReturnsNotFound()
    {
        var controller = new UserController();
        var actionResult = await controller.GetUserById2Async(999);

        var notFoundResult = Assert.IsType<NotFoundResult>(actionResult.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public void UserController_DeclaresApiVersionsAndDeprecation()
    {
        var type = typeof(UserController);
        var apiVersionAttrs = type.GetCustomAttributes<ApiVersionAttribute>().ToList();

        Assert.Contains(apiVersionAttrs, a => a.Versions.Any(v => v.ToString() == "1.0") && a.Deprecated);
        Assert.Contains(apiVersionAttrs, a => a.Versions.Any(v => v.ToString() == "2.0") && !a.Deprecated);

        var v1Method = type.GetMethod(nameof(UserController.GetUserByIdAsync));
        Assert.NotNull(v1Method);
        Assert.True(v1Method.IsDefined(typeof(ObsoleteAttribute), inherit: false));
    }
}

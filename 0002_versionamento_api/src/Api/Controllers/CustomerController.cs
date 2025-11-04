using Api.Models;

using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Tags("Customers")]
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
[Route("api")]
[ApiController]
public class CustomerController : ControllerBase
{
    [HttpGet("v{version:apiVersion}/customers/{id}")]
    [MapToApiVersion("1.0")]
    [Obsolete("This API version is deprecated. Please use version 2.0.")]
    [ProducesResponseType(typeof(CustomerModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerModel>> GetCustomerByIdAsync([FromRoute] int id)
    {
        var customer = new CustomerModel
        (
            Id: id,
            Document: "123.456.789-00",
            Name: "John Doe",
            Email: "john.doe@gmail.com",
            PhoneNumber: "+1-202-555-0143"
        );

        return await Task.FromResult(Ok(customer));
    }

    [HttpGet("v{version:apiVersion}/customers/{id}")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(CustomerModelV2), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerModelV2>> GetCustomerByIdV2Async([FromRoute] int id)
    {
        var customer = new CustomerModelV2
        (
            Id: id,
            Document: 12345678900,
            Name: "John Doe",
            Email: "john.doe@gmail.com",
            PhoneNumber: "+1-202-555-0143"
        );

        return await Task.FromResult(Ok(customer));
    }
}

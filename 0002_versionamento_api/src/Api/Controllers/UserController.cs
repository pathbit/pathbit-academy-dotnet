using Api.Models;

using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Tags("Users")]
[Route("api")]
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
[ApiController]
public class UserController : ControllerBase
{
    private static readonly List<UserModel> _users = new()
    {
        new (1, "Alice Smith", "alice.smith", "alice.smith@example.com"),
        new (2, "Bob Johnson", "bob.johnson", "bob.johnson@example.com"),
        new (3, "Charlie Brown", "charlie.brown", "charlie.brown@example.com")
    };

    [HttpGet("v{version:apiVersion}/users")]
    [MapToApiVersion("1.0")]
    [Obsolete("This API version is deprecated. Please use version 2.0.")]
    [ProducesResponseType(typeof(IEnumerable<UserModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserModel>>> GetUsersAsync()
    {
        return await Task.FromResult(Ok(_users));
    }

    [HttpGet("v{version:apiVersion}/users")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(IEnumerable<UserModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserModel>>> GetUsersV2Async()
    {
        return await Task.FromResult(Ok(_users));
    }

    [HttpGet("v{version:apiVersion}/users/{id}")]
    [MapToApiVersion("1.0")]
    [Obsolete("This API version is deprecated. Please use version 2.0.")]
    [ProducesResponseType(typeof(UserModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserModel>> GetUserByIdAsync([FromRoute] int id)
    {
        var user = await Task.FromResult(_users.Find(user => user.Id == id));

        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet("v{version:apiVersion}/users/id/{id}")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(UserModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserModel>> GetUserById2Async([FromRoute] int id)
    {
        var user = await Task.FromResult(_users.Find(user => user.Id == id));

        return user is null ? NotFound() : Ok(user);
    }
}

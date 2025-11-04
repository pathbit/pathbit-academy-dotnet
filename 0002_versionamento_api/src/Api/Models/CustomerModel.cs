namespace Api.Models;

public record CustomerModel(int Id, string Document, string Name, string Email, string PhoneNumber);
public record CustomerModelV2(int Id, long Document, string Name, string Email, string PhoneNumber);

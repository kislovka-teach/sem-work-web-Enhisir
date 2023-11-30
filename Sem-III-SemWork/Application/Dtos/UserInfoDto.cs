using Database.Entities;

namespace Application.Dtos;

public record UserInfoDto(
    string Login, 
    string Name, 
    City City);
using AspNetExample.Domain.Dtos;
using AspNetExample.Domain.Entities;

namespace AspNetExample.Domain.Extensions;

public static class WardExtensions
{
    public static WardDto ToDto(this Ward ward)
    {
        return new WardDto
        {
            Id = ward.Id,
            Name = ward.Name,
            Places = ward.Places,
            DepartmentName = ward.Department.Name
        };
    }
}
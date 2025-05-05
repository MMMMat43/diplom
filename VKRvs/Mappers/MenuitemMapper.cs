using VKRvs.DTO;
using VKRvs.Models;

namespace VKRvs.Mappers
{
    public static class MenuitemMapper
    {
        public static MenuItemDto ToDto(this Menuitem menuItem)
        {
            return new MenuItemDto
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Price = menuItem.Price,
                Description = menuItem.Description
            };
        }

        public static Menuitem FromDto(this MenuItemDto dto)
        {
            return new Menuitem
            {
                Id = dto.Id,
                Name = dto.Name,
                Price = dto.Price,
                Description = dto.Description
            };
        }
    }

}

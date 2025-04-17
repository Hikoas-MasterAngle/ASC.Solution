using System.Collections.Generic;

namespace ASC.WEB.Models
{
    public class NavigationMenu
    {
        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>(); // ✅ Thêm danh sách menu
    }

    public class MenuItem
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Link { get; set; } = "#";
        public string MaterialIcon { get; set; } = "menu";
        public List<string> UserRoles { get; set; } = new List<string>();
        public bool IsNested { get; set; } = false;
        public List<MenuItem> NestedItems { get; set; } = new List<MenuItem>();
        public int Sequence { get; set; } = 0;
    }
}

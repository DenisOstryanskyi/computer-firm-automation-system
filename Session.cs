namespace CFAS
{
    /// <summary>
    /// Зберігає сесію користувача, що успішно увійшов у систему.
    /// </summary>
    public static class Session
    {
        public static int UserId { get; set; }
        public static string Login { get; set; }
        public static string FullName { get; set; }
        public static string RoleName { get; set; }
    }
}
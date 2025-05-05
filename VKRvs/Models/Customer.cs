using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VKRvs.Models;

public partial class Customer
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Имя обязательно для заполнения.")]
    [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов.")]
    public string? FirstName { get; set; }

    [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов.")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Email обязателен для заполнения.")]
    [EmailAddress(ErrorMessage = "Некорректный формат email.")]
    public string Email { get; set; } = null!;

    [Phone(ErrorMessage = "Некорректный формат номера телефона.")]
    [StringLength(20, ErrorMessage = "Номер телефона не должен превышать 20 символов.")]
    public string? PhoneNumber { get; set; }

    [StringLength(200, ErrorMessage = "Адрес не должен превышать 200 символов.")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Пароль обязателен.")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Пароль должен быть длиной от 6 до 255 символов.")]
    public string? PasswordHash { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Количество бонусных баллов не может быть отрицательным.")]
    public int? LoyaltyPoints { get; set; }

    public virtual ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Orderitem> Orderitems { get; set; } = new List<Orderitem>();
}

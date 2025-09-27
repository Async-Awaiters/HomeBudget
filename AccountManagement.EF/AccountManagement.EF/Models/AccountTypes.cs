using System.Text.Json.Serialization;

namespace AccountManagement.EF.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountTypes
{
    Cash,
    DebitCard,
    CreditCard,
    SavingAccount,
    TakenDebt,
    IssuedLoan,
    Other
}

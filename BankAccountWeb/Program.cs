using BankAccountWeb;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

var sync = new object();
var account = new BankAccount(100m);
app.MapGet("/api/balance", () => { lock(sync) return Results.Ok(new { balance = account.Balance }); });
app.MapPost("/api/reset", () => { lock(sync) { account = new BankAccount(100m); return Results.Ok(new { balance = account.Balance }); } });
app.MapPost("/api/deposit", (AmountRequest request) => Execute(() => account.Deposit(request.Amount)));
app.MapPost("/api/withdraw", (AmountRequest request) => Execute(() => account.Withdraw(request.Amount)));
IResult Execute(Action action)
{
    lock(sync)
    {
        try { action(); return Results.Ok(new { balance = account.Balance }); }
        catch (ArgumentOutOfRangeException) { return Results.BadRequest(new { error = "Amount must be positive." }); }
        catch (InvalidOperationException) { return Results.BadRequest(new { error = "Insufficient funds." }); }
    }
}
app.Run();
record AmountRequest(decimal Amount);

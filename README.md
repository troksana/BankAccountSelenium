# BankAccountSelenium — C# / ASP.NET Core 8 / Selenium / MSTest

Ta wersja zachowuje aplikację BankAccountWeb z projektu Playwright, ale zastępuje testy Playwright testami Selenium WebDriver.

## Wymagania
- Visual Studio 2022 z obsługą .NET 8 lub .NET 8 SDK
- Google Chrome
- Internet przy pierwszym uruchomieniu Selenium Manager, aby pobrać odpowiedni ChromeDriver (jeśli nie jest dostępny lokalnie)

## Uruchomienie
1. Otwórz `BankAccountSelenium.sln` w Visual Studio.
2. Wykonaj Build → Rebuild Solution (NuGet Restore).
3. Zamknij ręcznie uruchomione aplikacje na porcie 5187.
4. Test → Test Explorer → Run All Tests.

Testy uruchamiają serwer aplikacji samodzielnie, resetują saldo do 100 PLN przed każdym testem i otwierają Chrome. Nie trzeba uruchamiać `playwright.ps1` ani instalować Playwright.

Alternatywnie: `dotnet test BankAccountSelenium.sln`.
Tylko smoke: `dotnet test BankAccountSelenium.sln --filter "TestCategory=Smoke"`.

## Testy
- Smoke: otwarcie aplikacji, wpłata 50 PLN
- Functional: wypłata 40 PLN
- Negative: wypłata 150 PLN, wpłata 0 lub -5 PLN
- Regression: zachowanie salda po odświeżeniu

Aby ukryć okno Chrome, odkomentuj `options.AddArgument("--headless=new");` w `SetUp()`.

**Uwaga:** aplikacja demonstracyjna przechowuje saldo wyłącznie w pamięci serwera. Nie jest to rzeczywisty system bankowy. Testy nie zostały uruchomione w środowisku generowania projektu.

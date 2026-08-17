# Settlement Fee — legacy code, staged for the take-home

## Repo layout

```
SettlementFee.sln
db/
  Setup/00_CreateDatabase.sql       creates SettlementFeeDb
  Setup/Setup-LocalDb.ps1           one-shot: create DB + seed + deploy proc
  Seed/CurrencyFeeFloor.sql         reference table + rows
  Procedures/usp_CalculateSettlementFee.sql
  Run-Fee.ps1                       call the proc with arbitrary inputs
src/
  Legacy.Quotes/                    the C# preview API (unchanged from appendix)
```

## Getting it running in Visual Studio 2026

You need the **SQL Server LocalDB** component. If you installed VS2026 with the
"ASP.NET and web development" or ".NET desktop development" workload it's already
there. To check, open a terminal and run `sqllocaldb info` — if that fails, open
**Visual Studio Installer → Modify** and tick "SQL Server Express LocalDB" under
Individual Components, or grab the standalone installer from Microsoft.

1. **Stand up the database** — from a normal PowerShell prompt at the repo root:

   ```powershell
   .\db\Setup\Setup-LocalDb.ps1
   ```

   This creates `SettlementFeeDb` on `(localdb)\MSSQLLocalDB`, seeds
   `CurrencyFeeFloor`, and deploys the stored procedure. It's idempotent, so re-run
   it any time you edit the `.sql` files.

2. **Open the solution** — double-click `SettlementFee.sln`, or `File → Open →
   Project/Solution` in VS2026.

3. **Run the API** — hit F5 (or Ctrl+F5 for no debugger). `launchSettings.json`
   already points `LEGACY_DB` at the LocalDB instance from step 1, so there's
   nothing else to configure. It'll listen on `http://localhost:5080`.

4. **Hit the preview endpoint** — either from the browser or PowerShell:

   ```powershell
   Invoke-RestMethod "http://localhost:5080/api/quote/preview?amount=12500&currency=GBP&tier=PARTNER&discountPct=0.10&expedited=false&bookedAtUtc=2018-06-01"
   ```

5. **Query the stored procedure directly** — this is your ground truth. Use it
   heavily to generate expected values for the rules-module tests:

   ```powershell
   .\db\Run-Fee.ps1 -Amount 12500 -Currency GBP -Tier PARTNER -DiscountPct 0.10 -BookedAtUtc 2018-06-01
   ```

If `sqlcmd` isn't on your PATH, it ships with SSMS and with the "SQL Server Data
Tools" component in the VS Installer, or install the standalone
"Microsoft Command Line Utilities for SQL Server."

### If LocalDB fights you

You don't need LocalDB specifically — any reachable SQL Server instance works.
Point `LEGACY_DB` (in `launchSettings.json` or as an environment variable) and the
`-S` argument in the two PowerShell scripts at whatever instance you're using
(e.g. a full SQL Server Express install, or a `mcr.microsoft.com/mssql/server`
Docker container if you'd rather not touch LocalDB), then run the same three
`.sql` files against it in order: `00_CreateDatabase.sql`,
`CurrencyFeeFloor.sql`, `usp_CalculateSettlementFee.sql`.

---

## What I found

*(fill in — differences between the three implementations, what you're treating
as ground truth and why, bugs vs. deliberate behaviour, what you'd ask and who.)*

## Rules module

*(fill in — pure module + tests live here, e.g. `src/FeeRules/`)*

## How I'd prove the two agree

*(fill in — shadow-run approach, comparison record fields, what "safe to cut
over" looks like.)*

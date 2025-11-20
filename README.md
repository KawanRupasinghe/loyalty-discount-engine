# Loyalty Discount Engine (.NET + NUnit)

A small, testable **.NET 9** rules engine that applies **one** discount per order (no stacking).  
**Policy:** Highest discount wins; **tie-break** = `Loyalty > Coupon > Black Friday > Regular`.  
**Rates:** Loyalty **20%**, Regular **10%**, Black Friday **5%**, Coupon **10%** or **25%** (max cap **25%**).  
**Price calc:** `final = total * (1 - pct)` (rounded to **2dp**, AwayFromZero).

---

## 🧰 Tech Stack

- C# / .NET 9
- NUnit (+ Coverlet for coverage)
- (Optional) Spectre.Console for a nicer CLI dashboard

---

## 🚀 Quick Start (Clone & Run)

```bash
# 1) Clone
git clone https://github.com/KawanRupasinghe/loyalty-discount-engine.git
cd loyalty-discount-engine

# 2) Restore & build
dotnet restore
dotnet build

# 3) System Scenarios

#Loyalty member, total 1200
dotnet run --project src/LoyaltyDiscount.Cli -- --loyalty --total 1200

#Regular customer + 25% coupon, total 250
dotnet run --project src/LoyaltyDiscount.Cli -- --regular --coupon 25 --total 250

#Coupon 10% + Black Friday, total 99.99
dotnet run --project src/LoyaltyDiscount.Cli -- --coupon 10 --bf --total 99.99

#Run the built-in demo suite
dotnet run --project src/LoyaltyDiscount.Cli -- --demo

# 4) Run tests
dotnet test tests/LoyaltyDiscount.Tests
```

---

## 📁 Project Structure

```bash
.
├─ src/
│  ├─ LoyaltyDiscount/                 # Class Library (business rules)
│  │  ├─ LoyaltyDiscount.cs
│  │  └─ LoyaltyDiscount.csproj
│  └─ LoyaltyDiscount.Cli/             # Console app (friendly runner)
│     ├─ Program.cs
│     └─ LoyaltyDiscount.Cli.csproj
├─ tests/
│  └─ LoyaltyDiscount.Tests/           # NUnit tests
│     ├─ DecisionTableTests.cs
│     ├─ DiscountSelectorTests.cs
│     ├─ RoundingAndValidationTests.cs
│     ├─ TieBreakAndPolicyTests.cs
│     └─ LoyaltyDiscount.Tests.csproj
├─ .github/
│  └─ workflows/
│     ├─ ci.yml
│     └─ dotnet-tests.yml
├─ .vscode/
│  ├─ tasks.json
│  └─ extensions.json
├─ azure-pipelines.yml
├─ LoyaltyDiscount.sln
└─ README.md
```


---

## 🧪 Tests & results (TRX)

You can generate a Visual Studio TRX test results file locally and in CI. This is helpful for sharing results, attaching them to PRs, or viewing detailed run logs.

### Run locally and create TRX

```bash
# From the repo root
dotnet test LoyaltyDiscount.sln \
   --logger "trx;LogFileName=TestResults.trx" \
   --results-directory "./TestResults"
```

- Output path: `TestResults/TestResults.trx`
- Open the file in VS Code (it’s XML), or use any TRX viewer extension if you prefer a richer UI.

### VS Code one-click task

This repo includes a task to generate TRX without typing commands:

- Terminal → Run Task → `test:trx`

Under the hood this runs `dotnet test` with the TRX logger and writes to `./TestResults`.

### View TRX in VS Code

- Recommended extension: TRX File Viewer (see below to install)
- After installing, right-click a `.trx` file → "Open With..." → select "TRX File Viewer"
- Or just click the `.trx` file; the extension may register itself as the default editor

Recommended workspace extension is declared in `.vscode/extensions.json`, so VS Code should prompt you to install it on open.


### GitHub Actions (CI)

There are two workflows you may use in this repo:

1) Multi-OS matrix (file: `.github/workflows/ci.yml`)
   - Runs on: `ubuntu-latest`, `windows-latest`, `macos-latest`
   - Uploads per-OS artifacts named:
     - `test-results-ubuntu-latest`
     - `test-results-windows-latest`
     - `test-results-macos-latest`
   - Each artifact contains one or more TRX files under `**/TestResults/*.trx`.

2) Single-OS workflow (file: `.github/workflows/dotnet-tests.yml`)
   - Runs on: `ubuntu-latest`
   - Uploads a single artifact named: `test-results-trx`
   - TRX path: `TestResults/TestResults.trx`

Where to view:

- Pull Requests → Checks → “.NET Tests” (or “CI”) → Test results summary (published via the workflow)
- Actions → latest run → Artifacts → download the artifact for your OS (or `test-results-trx`)

### Azure DevOps (Pipeline)

Pipeline definition: `azure-pipelines.yml`

How to enable:

1) In Azure DevOps → Pipelines → New pipeline → point to `azure-pipelines.yml`
2) Save & run (pipeline triggers on pushes to `main`)

Where to view:

- Pipeline run → “Tests” tab (TRX is published via `PublishTestResults@2`)
- Artifacts (if you additionally choose to publish files there)


### Troubleshooting

- No TRX file? Ensure the `--logger trx` and `--results-directory` arguments are present and the folder exists/writable.
- CI job shows no tests? Confirm test project targets the same .NET SDK that the runner installs (this repo uses .NET 9).
- PR “Checks” don’t show test details on forks? Some GitHub permission settings can limit write access to Checks for forked PRs; artifacts still upload.



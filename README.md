# CertPrep

CertPrep is a local-first desktop practice app for certification study. It provides short Study sessions with immediate answer checks and explanations, Exam Simulation without per-question feedback, mixed sessions across multiple exams, missed-question retry, resumable sessions, and objective-level mastery and readiness. The domain model is provider-neutral, so a Microsoft skills-measured area and a CCNA topic are both ordinary exam objectives.

The embedded catalog contains 603 original questions across AZ-104, AZ-700, AZ-900, MD-102, MS-102, SC-200, SC-300, SC-401, and SC-900. The larger banks focus on Azure, Entra, endpoint, Microsoft 365, security operations, and information-protection administration. Questions follow the official Microsoft skills-measured areas and link to supporting Microsoft Learn documentation. They include operational scenarios, balanced True/False items, explicit multi-select answer counts, four-to-six-choice questions, and deployment or response-order exercises. They are not exam dumps and should not be treated as complete replacements for hands-on work or Microsoft Learn training.

CertPrep is an independent project and is not affiliated with, endorsed by, or sponsored by Microsoft. Microsoft, Azure, Microsoft 365, Microsoft Entra, and related names are trademarks of Microsoft Corporation.

## Run it

Requires the .NET 10 SDK.

```powershell
dotnet tool restore
dotnet restore CertPrep.sln
dotnet run --project src/CertPrep/CertPrep.csproj
```

Release builds create and migrate `%LOCALAPPDATA%\CertPrep\certprep.db`, then merge the embedded starter catalog by stable content key. Debug builds use `%LOCALAPPDATA%\CertPrep\Dev\certprep.db`, so development does not overwrite an installed profile. Before an existing database is migrated, the app creates a valid SQLite backup under `backups` and retains the newest three. Delete these files only when you intentionally want a fresh local profile.

The desktop shell opens at `1180 × 760` and supports a minimum size of `920 × 640`. Session lengths are `5`, `10`, `20`, `40`, or every available question when the bank is smaller. Appearance defaults to `System`; `Light` and graphite-black `Dark` overrides are persisted locally. `Roast mode` also persists locally and defaults on; it changes rank names and result captions without changing progression. Turn it off on the Dashboard for professional copy.

## Structure

```text
src/CertPrep/
  Features/                 Vertical product slices
    Dashboard/
    ExamCatalog/
      Importing/             Validated package loading and catalog merge
    Practice/
    Progress/
    Rewards/
    Results/
  Infrastructure/Persistence/
  Infrastructure/Settings/
  Content/                   Embedded starter package and per-exam banks
    Banks/                   One independently versioned JSON object per exam
  Shared/
  Shell/
tests/CertPrep.Tests/
  Scenarios/                Persistence and simulated-session tests
  Views/                    Real XAML workflow and snapshot test
```

Views own layout. ViewModels own UI state and commands. The practice service owns session rules. Concrete repositories own EF Core queries. SQLite contexts come from a pooled factory, so no long-lived `DbContext` leaks into the UI.

Questions, choices, explanations, and source references are copied into each session. Historical results therefore remain stable if the question bank changes later.

The production bank rules are documented in `src/CertPrep/Content/README.md`. Objective weights, study-guide dates, source families, and known upcoming exam updates are recorded in `docs/content-sources/`.

In-progress sessions and draft answer selections are stored in SQLite. `Save for later`, ordinary navigation, and application shutdown flush the active draft; the Dashboard then offers `Resume` or `End session`. Installed-app updates keep the same release data directory, and EF Core migrations upgrade that profile in place after taking a backup.

The header `Import` action accepts a compatible CertPrep `.db`, `.sqlite`, or `.sqlite3` file. It reads the source in read-only mode, validates the catalog tables and question rules, then merges exams, objectives, questions, and choices in one target transaction. Stable content keys update matching questions; new keys add questions. Practice sessions, progress, source integer IDs, and unrelated tables are never imported.

Mixed practice balances questions across the selected exams, randomizes their final order, and never exceeds the length selected in setup. Each question keeps its original exam attribution, so mixed results still improve the correct exam and objective progress.

Study mode labels the action `Check answer` and immediately reveals correct/incorrect choices, the explanation, and the source. Exam Simulation holds that feedback until completion, where missed questions show the submitted answer, correct answer, explanation, and source. `Retry missed` turns those misses into a new Study session.

Objective mastery is derived from immutable session history rather than a mutable counter. The tiers are `Unseen`, `Learning`, `Reliable`, and `Mastered`. Readiness combines recent weighted accuracy, active-question coverage, and evidence volume. The exam score is mostly the question-weighted objective average, with the weakest objective contributing 25%, so one weak category remains visible. A wrong answer lowers readiness without deleting an already-earned mastery tier; that objective is instead marked for review.

The XP ledger is append-only and idempotent. A completed session awards 20 XP; correct answers award 10, 12, or 15 XP by difficulty; a recent repeat is reduced to 20%; recovering a previously missed question awards 15 XP; objective promotions award 10, 50, or 100 XP; and the first qualifying Boss Exam clear awards 250 XP. Level 1 needs 200 XP and each following level needs 50 XP more than the previous one. Startup reconciliation safely repairs rewards for a completed session if the app closed between completion and ledger insertion.

Levels also receive presentation-only rank names. Roast mode progresses from `Freshly Spawned Idiot` and `Certified Fucking Noob` through increasingly competent insults; clean mode uses ordinary ranks such as `Rookie`, `Specialist`, and `Architect`. Results choose one of four bundled, locally animated gremlin reactions by score. The original sprite sheet and deterministic GIF builder live under `src/CertPrep/Assets/Reactions/` and `tools/`; no remote meme service or copyrighted reaction clip is required.

Normal and Boss sessions use bounded adaptive sampling: unseen and previously missed questions become more likely, but no eligible question is excluded. Boss Exams unlock when every exam objective is at least `Reliable`, balance their pool across objectives, and require 80% overall plus at least 60% in every represented objective for the clear reward.

## Verify it

```powershell
dotnet build CertPrep.sln
dotnet test CertPrep.sln --no-build
```

## Build the Windows installer

The release pipeline targets .NET 10 and produces an unsigned, self-contained `win-x64` VeloPack installer. It runs the Release test suite, publishes one application executable plus its license with no PDB files, and packages the installer and update artifacts under `artifacts/releases/win`.

```powershell
.\tools\build-release.ps1
```

Pass another SemVer value when preparing a later release:

```powershell
.\tools\build-release.ps1 -Version 0.2.0
```

The application bootstrap calls VeloPack before Avalonia starts so install and update hooks can exit cleanly without initializing the UI. No update feed or code signing is configured yet.

The test suite applies the real EF Core schema in SQLite, validates pre-migration backups, verifies idempotent question-bank merging and reward reconciliation, closes and resumes sessions through fresh service graphs, checks generic mastery/readiness and Boss unlock rules, tests bounded weak-question sampling, exercises missed-question review/retry, drives the real Avalonia window by keyboard, and renders the full workflow plus minimum-size dark/light views into `artifacts/ui-snapshots/`.

The generated PNGs are inspection artifacts rather than committed visual baselines because font rasterization varies by operating system. See [the snapshot notes](docs/ui-snapshots.md).

## Deliberately not built yet

Rotating contracts are intentionally deferred until the core XP/mastery loop has been used enough to tune its reward rate. The next useful content slice is authoring and portable package export, followed by fuller objective coverage and, if needed, scheduled review intervals. User accounts, an admin role system, and repository interfaces would add ceremony without solving a current need, so they are intentionally absent.

## License

The source code and original question-bank content are available under the [MIT License](LICENSE).

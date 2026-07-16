# UI snapshots

Run `dotnet test CertPrep.sln` to render the real Avalonia workflow into `artifacts/ui-snapshots/`.

The PNG files are generated inspection artifacts and are intentionally ignored. They are not committed visual-regression baselines because font rendering varies across operating systems. The headless tests verify that each actual view loads, participates in complete single-exam and mixed-exam runs, produces a substantial rendered frame, and contains the expected shell pixels rather than an incomplete compositor buffer.

The generated set covers dashboard campaign/readiness and Roast-mode state, resumable sessions, single-exam setup, Study question/feedback, animated score reactions, XP and mastery results, objective readiness, mixed setup, mixed Exam Simulation, missed-answer review, retry, and minimum-size dashboard renders in both dark and light themes. The review and retry-feedback captures intentionally scroll the real view so the explanation and source are visible below the fold.

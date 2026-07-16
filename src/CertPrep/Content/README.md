# Question bank authoring

Each production exam bank lives in `Banks/<exam-code>.json` as one `QuestionBankExam` object. The embedded starter package remains in `default-question-bank.json`.

## Content rules

- Use the current Microsoft study guide's top-level skills-measured areas as objectives.
- Cover every objective and roughly follow Microsoft's published percentage ranges.
- Write original questions. Do not copy exam dumps or Microsoft practice-assessment questions.
- Link every question to the specific supporting page on `learn.microsoft.com`, not a search page.
- Prefer operational scenarios over terminology recall, especially for associate and expert exams.
- Use four to six answer choices from the same technical domain, varying the count across a bank. True/False questions use exactly `True` then `False`, with a balanced answer key across each exam. A distractor or false statement should be plausible but wrong because of scope, capability, sequence, or a missing requirement.
- Avoid joke choices, unrelated products, `all of the above`, and `none of the above`.
- State the exact number of required answers in every multiple-choice prompt, such as `Choose two` or `Select three`. The practice screen also derives and displays this count from the answer key.
- For ordering questions, present complete sequences as the choices. Reuse the same or overlapping steps in plausible orders so the answer tests prerequisites and dependencies rather than recognition of unrelated steps.
- Explain why the correct choice satisfies the scenario. For advanced questions, also identify the constraint that rules out the closest distractor.
- Keep explanations short when one precise sentence is enough. Use a second or third sentence only when the distinction needs it.
- Use MSP and multi-tenant scenarios only when the documented exam objective covers the underlying feature.

Question content is versioned independently per exam so a study-guide update does not rewrite unrelated banks.

## Portable JSON import contract

External authors should use the authoring kit available from **Options → Save authoring kit**. The ZIP contains:

- `question-bank-template.json`: a complete, valid bank showing the required nesting and every field.
- `question-bank.schema.json`: the formal schema for editor validation and autocomplete.
- `README.md`: field definitions, allowed enum values, answer rules, and merge behavior.

CertPrep imports the completed `.json` file directly. The root object uses `schemaVersion: 1` and an `exams` array. Each exam owns its `objectives` and `questions`; each question owns its `choices` and refers to an objective through `objectiveKey`. The included kit is the public contract and should be kept in sync with `QuestionBankPackage` and `QuestionBankMerger.Validate`.

## SQLite import contract

SQLite import is retained for transferring a catalog from another CertPrep database. It is not the external authoring format. The Options page accepts `.db`, `.sqlite`, or `.sqlite3`; the source database is opened read-only. The destination merge is transactional and starts only after the complete source bank passes validation.

Required tables and columns:

- `Exams`: `Id`, `Provider`, `Code`, `Title`, `Summary`, `ContentVersion`, `IsArchived`
- `ExamObjectives`: `Id`, `ExamId`, `ContentKey`, `Name`, `SortOrder`
- `Questions`: `Id`, `ExamId`, `ExamObjectiveId`, `ContentKey`, `Prompt`, `Kind`, `Difficulty`, `Explanation`, `SourceName`, `SourceUrl`, `IsActive`
- `AnswerChoices`: `QuestionId`, `Text`, `IsCorrect`, `SortOrder`

`Kind` accepts `SingleChoice`, `MultipleChoice`, or `TrueFalse`. `Difficulty` accepts `Foundation`, `Intermediate`, or `Advanced`. Every question requires an explanation, a source name, an absolute source URL, at least one correct choice, and at least one incorrect choice. `SingleChoice` requires exactly one correct choice; `MultipleChoice` requires at least two; `TrueFalse` requires exactly two choices ordered as `True`, then `False`.

Exams are matched case-insensitively by `Provider + Code`. Objectives and questions are matched case-insensitively by their stable `ContentKey`, which may contain only letters, digits, `-`, and `_` and is limited to 100 characters. Matching rows are updated, new rows are added, and answer choices for updated questions are replaced. Entries absent from the source are not deleted.

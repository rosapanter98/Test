# CertPrep question-bank authoring kit

This kit defines the portable question-bank contract accepted by CertPrep.

## Use the kit

1. Copy `question-bank-template.json` and rename the copy for your bank.
2. Replace the example exam, objective, question, and choices with your content.
3. Keep `schemaVersion` set to `1`.
4. Validate the file against `question-bank.schema.json` in an editor with JSON Schema support.
5. In CertPrep, open **Options**, select **Import bank**, and choose the JSON file.

## Structure

```text
question bank
└── exams[]
    ├── objectives[]
    └── questions[]
        └── choices[]
```

An exam contains its own objectives and questions. A question connects to an objective through `objectiveKey`, which must equal that objective's `contentKey`.

## Root fields

- `schemaVersion`: must be `1`.
- `exams`: one or more exam objects.
- `$schema`: optional editor hint pointing to the included schema file.

## Exam fields

- `provider` + `code`: case-insensitive stable identity used when merging, for example `Microsoft` + `AZ-104`.
- `title`, `summary`, `contentVersion`: required display and content-version text.
- `objectives`: one or more objective objects.
- `questions`: one or more question objects.

## Objective fields

- `contentKey`: stable key containing only letters, digits, `-`, or `_`; maximum 100 characters.
- `name`: objective name shown in CertPrep.
- `sortOrder`: unique ordering number within the exam.

## Question fields

- `contentKey`: stable key unique within the exam.
- `objectiveKey`: the `contentKey` of an objective in the same exam.
- `prompt`: complete question shown to the user. Multiple-choice prompts should state how many answers to select.
- `kind`: `SingleChoice`, `MultipleChoice`, or `TrueFalse`.
- `difficulty`: `Foundation`, `Intermediate`, or `Advanced`.
- `explanation`: why the correct answer satisfies the scenario. Include the closest distractor distinction when useful.
- `sourceName`: readable source-page title.
- `sourceUrl`: absolute supporting source URL.
- `isActive`: `true` to include the question in new sessions.
- `choices`: two or more answer-choice objects.

## Choice fields and answer rules

- `text`: answer text.
- `isCorrect`: `true` for a correct choice.
- `sortOrder`: unique ordering number within the question.

`SingleChoice` requires exactly one correct choice. `MultipleChoice` requires at least two correct choices. `TrueFalse` requires exactly two choices ordered as `True`, then `False`, with exactly one correct choice. Every question also needs at least one incorrect choice.

## Merge behavior

The entire bank is validated before the destination database is changed. New content is added and matching content is updated in one transaction. Exams match by `provider + code`; objectives and questions match by `contentKey`. Matching questions receive the imported choice set. Content missing from the imported file is not deleted.

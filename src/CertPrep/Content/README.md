# Question bank authoring

Each production exam bank lives in `Banks/<exam-code>.json` as one `QuestionBankExam` object. The embedded starter package remains in `default-question-bank.json`.

## Content rules

- Use the current Microsoft study guide's top-level skills-measured areas as objectives.
- Cover every objective and roughly follow Microsoft's published percentage ranges.
- Write original questions. Do not copy exam dumps or Microsoft practice-assessment questions.
- Link every question to the specific supporting page on `learn.microsoft.com`, not a search page.
- Prefer operational scenarios over terminology recall, especially for associate and expert exams.
- Use four to six answer choices from the same technical domain, varying the count across a bank. A distractor should be plausible but wrong because of scope, capability, sequence, or a missing requirement.
- Avoid joke choices, unrelated products, `all of the above`, and `none of the above`.
- State the exact number of required answers in every multiple-choice prompt, such as `Choose two` or `Select three`. The practice screen also derives and displays this count from the answer key.
- For ordering questions, present complete sequences as the choices. Reuse the same or overlapping steps in plausible orders so the answer tests prerequisites and dependencies rather than recognition of unrelated steps.
- Explain why the correct choice satisfies the scenario. For advanced questions, also identify the constraint that rules out the closest distractor.
- Keep explanations short when one precise sentence is enough. Use a second or third sentence only when the distinction needs it.
- Use MSP and multi-tenant scenarios only when the documented exam objective covers the underlying feature.

Question content is versioned independently per exam so a study-guide update does not rewrite unrelated banks.

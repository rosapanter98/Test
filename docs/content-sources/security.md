# Security exam source notes

Research date: 2026-07-16. Only official Microsoft Learn and Microsoft product documentation was used. The questions are original; no Microsoft practice-assessment items were copied or paraphrased.

## SC-200 — Microsoft Security Operations Analyst

- Official study guide: <https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/sc-200>
- Official certification page: <https://learn.microsoft.com/en-us/credentials/certifications/security-operations-analyst/>
- Current objectives on the research date: skills measured as of April 16, 2026.
- The live study-guide page already displays the next English exam update, effective July 28, 2026. Its top-level objective names and weights remain the same. The bank uses the shared objective structure and avoids relying on the minor July-only changes.

Top-level objectives and weights:

1. Manage a security operations environment — 40–45%
2. Respond to security incidents — 35–40%
3. Perform threat hunting — 20–25%

Question distribution in `sc-200.json`: 77 questions, weighted 32 / 27 / 18 (41.6% / 35.1% / 23.4%).

Primary official documentation families used:

- Microsoft Defender XDR: <https://learn.microsoft.com/en-us/defender-xdr/>
- Microsoft Defender for Endpoint: <https://learn.microsoft.com/en-us/defender-endpoint/>
- Microsoft Defender for Identity: <https://learn.microsoft.com/en-us/defender-for-identity/>
- Microsoft Defender for Office 365: <https://learn.microsoft.com/en-us/defender-office-365/>
- Microsoft Defender for Cloud: <https://learn.microsoft.com/en-us/azure/defender-for-cloud/>
- Microsoft Defender for Cloud Apps: <https://learn.microsoft.com/en-us/defender-cloud-apps/>
- Microsoft Sentinel: <https://learn.microsoft.com/en-us/azure/sentinel/>
- Kusto Query Language: <https://learn.microsoft.com/en-us/kusto/query/?view=microsoft-fabric>
- Microsoft Purview Audit and eDiscovery training path: <https://learn.microsoft.com/en-us/training/paths/purview-audit-search/>

Each question carries the closest official `learn.microsoft.com` source page in its `sourceUrl` field.

## SC-401 — Administering Information Security in Microsoft 365

- Official study guide: <https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/sc-401>
- Official certification page: <https://learn.microsoft.com/en-us/credentials/certifications/information-security-administrator/>
- Current objectives on the research date: skills measured as of April 27, 2026.
- The live study-guide page already displays the next English exam update, effective July 28, 2026. Its top-level objective names and weights remain the same. The bank is based on the stable objective areas common to both versions.

Top-level objectives and weights:

1. Implement information protection — 30–35%
2. Implement data loss prevention and retention — 30–35%
3. Manage risks, alerts, and activities — 30–35%

Question distribution in `sc-401.json`: 75 questions, weighted 26 / 25 / 24 (34.7% / 33.3% / 32%).

Primary official documentation families used:

- Microsoft Purview: <https://learn.microsoft.com/en-us/purview/>
- Microsoft Purview Information Protection: <https://learn.microsoft.com/en-us/purview/information-protection>
- Microsoft Purview DLP: <https://learn.microsoft.com/en-us/purview/dlp-learn-about-dlp>
- Microsoft Purview retention: <https://learn.microsoft.com/en-us/purview/retention>
- Microsoft Purview Insider Risk Management: <https://learn.microsoft.com/en-us/purview/insider-risk-management>
- Microsoft Purview DSPM for AI: <https://learn.microsoft.com/en-us/purview/ai-microsoft-purview>
- Microsoft Defender for Cloud Apps: <https://learn.microsoft.com/en-us/defender-cloud-apps/>

Each question carries the closest official `learn.microsoft.com` source page in its `sourceUrl` field.

## Editorial audit — 2026-07-16

Reviewed the original 128 questions in the two security banks: 65 SC-200 questions and 63 SC-401 questions. The review checked the prompt, marked answer, distractors, explanation, objective mapping, selection cardinality, and direct Microsoft Learn source support for every item.

Substantive corrections:

- Corrected the SC-200 `contain-compromised-user` item. Defender XDR user containment is enforced at the Defender for Endpoint layer against attack-related paths such as SMB, RPC, and RDP on supported onboarded devices; it does not generically block the identity from all resources or disable the Entra account.
- Corrected SC-401 DLP precedence wording. The most restrictive applicable action is enforced; policy and rule priority resolve matches with otherwise identical actions.
- Narrowed the SC-401 Defender for Endpoint integration item to endpoint security-violation alerts and signals used by Insider Risk Management instead of implying unrestricted endpoint context transfer.
- Replaced several obviously unrelated distractors with nearby Microsoft security and compliance controls, while keeping each incorrect option unambiguously wrong for the stated requirement.
- Added explicit "which two" cardinality to every multi-select prompt that previously relied on the UI or explanation to convey the answer count.
- Replaced three retired SC-200 documentation URLs with current Microsoft Learn pages for Sentinel anomaly tuning, Defender XDR evidence and response, and Defender for Endpoint user containment.

Validation results:

- Before the True/False expansion, the banks contained 144 questions: 73 SC-200 and 71 SC-401.
- Current choice counts intentionally vary. SC-200 contains 4 two-choice, 67 four-choice, 4 five-choice, and 2 six-choice questions. SC-401 contains 4 two-choice, 65 four-choice, 4 five-choice, and 2 six-choice questions.
- Every single-choice question has exactly one marked answer. Every multi-select prompt states the exact number to choose and marks that number of answers.
- All question content keys are unique within each bank.
- All 109 unique question source URLs returned HTTP 200 on 2026-07-16.
- The 16 unique Microsoft Learn URLs used by the scenario expansion also returned HTTP 200 on 2026-07-16, bringing the two banks to 119 distinct source URLs after overlap with the original set.
- SC-500 remains excluded while the exam is in beta.

## Scenario expansion — 2026-07-16

Added eight operational scenarios to each associate bank. All new keys contain `-scenario-` so scenario coverage can be checked independently of normal question wording.

SC-200 additions cover:

- Azure Lighthouse workspace isolation and delegated MSSP operations.
- Sentinel connector-health monitoring and cross-workspace hunting.
- Sentinel playbook activation and verification order.
- A phased Defender for Endpoint attack surface reduction rollout.
- Ransomware device-response actions and cross-domain incident triage.
- Hunting suspicious OAuth application activity in `CloudAppEvents`.

SC-401 additions cover:

- Sensitivity-label creation and publishing order.
- DLP simulation, tuning, enforcement, and expansion order.
- Endpoint DLP deployment components and external-recipient encryption.
- Conflicting retention requirements.
- Insider Risk Management investigation order.
- Auto-label simulation review and eDiscovery preservation.

The ordering questions deliberately reuse the same operational steps in different sequences. SC-200 has two new ordering scenarios; SC-401 has three. The new questions use four, five, and six choices, include explicit `Choose two` or `Choose three` wording for multi-selects, and explain why the closest alternative is wrong.

## True/False expansion — 2026-07-16

Added four source-backed True/False questions to SC-200 and SC-401, bringing the two security banks to 152 questions. Each exam has two `True` and two `False` keyed answers. The additions cover active Sentinel rules, Defender response restrictions, hunting behavior, sensitivity-label persistence, Exact Data Match privacy, retention-label publishing, and eDiscovery holds. The complete security set now references 124 distinct Microsoft Learn URLs.

## SC-500 status and recommendation

- Official certification page: <https://learn.microsoft.com/en-us/credentials/certifications/cloud-and-ai-security-engineer-associate/>
- Official study guide: <https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/sc-500>
- Status on 2026-07-16: **Beta**. Microsoft states that beta exams are not scored immediately while exam-question quality data is collected. The certification page also reports that no training is currently available.
- Published beta objective groups:
  1. Manage identity, access, and governance — 20–25%
  2. Secure storage, databases, and networking — 25–30%
  3. Secure compute — 20–25%
  4. Manage and monitor security posture — 20–25%
- Recommendation: do not publish an SC-500 bank yet. Recheck the certification status and study-guide change log after general availability, then author against the GA objectives and Microsoft Learn training. This avoids building a large bank around beta weighting or terminology that may change.

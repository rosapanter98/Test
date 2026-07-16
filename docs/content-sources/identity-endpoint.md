# SC-300, MD-102, and MS-102 source notes

Research snapshot: 2026-07-16. Only Microsoft Learn credential pages, study guides, and Microsoft product documentation were used. The questions are original scenarios; Microsoft practice-assessment questions and exam dumps were not used.

## SC-300 — Microsoft Identity and Access Administrator

- Official study guide: <https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/sc-300>
- Certification page: <https://learn.microsoft.com/en-us/credentials/certifications/identity-and-access-administrator/>
- Skills measured since 2026-04-27:
  - Implement and manage user identities (20–25%)
  - Implement authentication and access management (25–30%)
  - Plan and implement workload identities (20–25%)
  - Plan and automate identity governance (20–25%)
- Current distribution: 77 questions — 18, 22, 18, and 19 respectively (23.4%, 28.6%, 23.4%, and 24.7%).
- Credential status: active associate certification, with SC-300 as the required exam. Microsoft Learn lists a 12-month renewal frequency and no separate prerequisite certification.
- Caveat: the current study guide's "Skills at a glance" gives authentication and access management a 25–30% weight, while the later section heading displays 20–25%. The bank follows the at-a-glance weight because it is the guide's explicit weighting summary.

## MD-102 — Endpoint Administrator

- Official study guide: <https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/md-102>
- Certification page: <https://learn.microsoft.com/en-us/credentials/certifications/modern-desktop/>
- Skills measured since 2026-04-28:
  - Prepare infrastructure for devices (25–30%)
  - Manage and maintain devices (30–35%)
  - Manage applications (15–20%)
  - Protect devices (15–20%)
- Current distribution for the objectives in force on 2026-07-16: 73 questions — 19, 25, 15, and 14 respectively (26%, 34.2%, 20.5%, and 19.2%).
- Credential status: active associate certification, with MD-102 as the required exam. Microsoft Learn lists a 12-month renewal frequency and no separate prerequisite certification.
- Transition risk: Microsoft Learn says the English certification/exam objectives will update on 2026-07-24, and the official study guide already publishes the upcoming objective set. The new set changes the weights to Prepare infrastructure (20–25%), Manage and maintain devices (25–30%), Protect devices (15–20%), Manage and secure applications (15–20%), and a new Optimize endpoint operations by using automation, monitoring, and reporting area (10–15%). It also adds or expands agentic tools, automation with PowerShell and Microsoft Graph, multi-admin approval, Windows Autopatch and Hotpatch, App Control for Business, operational reporting, Endpoint Analytics, and proactive remediations. This bank intentionally remains mapped to the four objectives in force on the 2026-07-16 research date. It must be restructured and re-audited before being represented as aligned to English exams delivered on or after 2026-07-24; renaming the existing four objective headings is not sufficient.

## MS-102 — Microsoft 365 Administrator

- Official study guide: <https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/ms-102>
- Expert certification page: <https://learn.microsoft.com/en-us/credentials/certifications/m365-administrator-expert/>
- Skills measured since 2026-04-28:
  - Deploy and manage a Microsoft 365 tenant (25–30%)
  - Implement and manage Microsoft Entra identity and access (25–30%)
  - Manage security and threats by using Microsoft Defender XDR (30–35%)
  - Manage compliance by using Microsoft Purview (10–15%)
- Current distribution: 68 questions — 18, 18, 23, and 9 respectively (26.5%, 26.5%, 33.8%, and 13.2%).
- Credential status: MS-102 is active and has no retirement date listed, but passing MS-102 alone does not award the Microsoft 365 Certified: Administrator Expert certification.
- Prerequisite caveat: the current expert certification page requires MS-102 plus at least one active prerequisite certification from: Endpoint Administrator Associate, Teams Administrator Associate, Identity and Access Administrator Associate, or Information Security Administrator Associate. This list can change as credentials retire, so the live certification page is authoritative at scheduling time.
- Weight-source caveat: Microsoft Learn rendered inconsistent MS-102 percentage summaries during the 2026-07-16 audit. The versioned official study guide's "Skills at a glance" and its detailed section headings consistently show 25–30%, 25–30%, 30–35%, and 10–15%; the bank follows that study guide because it is the detailed, dated objective contract used for question mapping. A nonversioned exam-page/search rendering showed 10–15%, 25–30%, 35–40%, and 15–20% while also describing the already-past 2026-04-28 update as future. Recheck both official pages when the study guide changes.
- Renewal: Microsoft Learn states that role-based and specialty certifications expire unless renewed and offers a no-cost Microsoft Learn renewal assessment. The expert credential page should be checked for the candidate's current renewal window.

## Content rules applied

- Every `objectiveKey` maps to an exact top-level objective from the applicable study guide.
- Objective counts approximate the published percentage ranges; they are not claims about the live exam's exact question mix.
- True/False questions have exactly two choices. Other questions have four to six same-domain choices, at least one correct choice, an explanation, and a direct `learn.microsoft.com` product-documentation source.
- `SingleChoice` and `TrueFalse` questions have exactly one correct answer. `MultipleChoice` questions have an exact stated answer count of at least two, using wording such as `Choose two` or `Choose three`.
- Question text is original and is not copied from Microsoft practice assessments.

## Editorial audit — 2026-07-16

- Reviewed all 182 questions in the three banks: 65 SC-300, 61 MD-102, and 56 MS-102.
- Checked each prompt for a single defensible interpretation, each answer key against the cited Microsoft Learn product documentation, and each distractor for same-domain plausibility while remaining unambiguously wrong under the stated scenario.
- Corrected four existing multi-select prompts so they explicitly say that two answers are required: three in SC-300 and one in MD-102. No existing correct-answer key required a semantic change.
- Tightened a new corporate-identifier scenario from ambiguous Android ownership behavior to a supported corporate iPhone and Company Portal enrollment case.
- Replaced a general SSPR citation with the administrator-policy page that documents the two-gate reset requirement used by the question.
- Repaired moved documentation links for Threat Explorer, Lifecycle Workflows custom extensions, PIM for Azure resources, and Application Proxy Kerberos constrained delegation before publishing the new items.
- Added 75 original questions: 25 per bank. Similar topics are intentionally allowed, but exact prompts and content keys are unique.
- Revalidated all 162 unique question source URLs after editing; every URL resolved successfully on the audit date.
- The MD-102 result is valid only for the objective set in force through 2026-07-23. The already-published 2026-07-24 objective set introduces a fifth skill area and materially changes weighting and coverage, so this is an explicit transition blocker rather than a cosmetic documentation warning.

## Scenario expansion audit — 2026-07-16

- Added and semantically reviewed 24 operational scenario questions: eight each for SC-300, MD-102, and MS-102. That checkpoint brought the combined identity and endpoint set to 206 questions before the True/False expansion.
- Each exam received the same scenario choice-count mix: two questions with four choices, three with five choices, and three with six choices. Current whole-bank distributions for two/four/five/six choices are MD-102: 4/63/3/3, MS-102: 4/58/3/3, and SC-300: 4/67/3/3.
- Added two ordered-deployment questions to MD-102, four to MS-102, and two to SC-300. Each ordering item reuses the same operational steps in plausible alternative orders so the answer depends on prerequisites and rollout sequence rather than vocabulary recognition.
- Added explicitly counted multi-select scenarios: two in MD-102, three in MS-102, and three in SC-300. Every `Choose two` item has exactly two keyed answers, and every `Choose three` item has exactly three.
- Scenario coverage emphasizes MSP operations: Autopilot pre-provisioning, Win32 packaging, Defender onboarding, app-based access, GDAP onboarding and role mapping, custom-domain cutover, Conditional Access rollout, DLP simulation, cross-tenant trust and synchronization, emergency access, PIM activation, and entitlement management.
- Parsed all three JSON banks and checked every new key, objective reference, choice sort order, answer cardinality, and source scheme. All 24 new content keys and prompts are unique.
- Revalidated the 24 distinct source URLs used by the new scenarios; all returned HTTP 200. Across the three complete banks, 180 distinct Microsoft Learn source URLs are now referenced.
- The scenario expansion does not claim alignment with the upcoming fifth MD-102 skill area. Its eight additions remain mapped to the four-objective contract in force on the research date; the existing 2026-07-24 transition warning remains mandatory.

## True/False expansion — 2026-07-16

Added four source-backed True/False questions to SC-300, MD-102, and MS-102, bringing the combined set to 218 questions. Every exam has a balanced two-`True`, two-`False` answer key. The statements cover cross-tenant and privileged identity behavior, co-management and Intune deployment rules, administrative units, retention, Conditional Access, and Defender security settings management. The complete three-bank set now references 187 distinct Microsoft Learn URLs.

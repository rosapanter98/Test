# Fundamentals exam source notes

Research date: 2026-07-16. Only official Microsoft Learn training, study guides, and product documentation were used. All questions are original; Microsoft practice-assessment questions and exam dumps were not copied or paraphrased.

## AZ-900 — Microsoft Azure Fundamentals

- Certification: <https://learn.microsoft.com/en-us/credentials/certifications/azure-fundamentals/>
- Official study guide: <https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/az-900>
- Skills version in effect on the research date: January 14, 2026
- Scheduled English exam update: July 20, 2026
- Bank coverage: 36 questions

| Official top-level objective | Published weight | Questions | Bank share |
|---|---:|---:|---:|
| Describe cloud concepts | 25–30% | 11 | 30.6% |
| Describe Azure architecture and services | 35–40% | 14 | 38.9% |
| Describe Azure management and governance | 30–35% | 11 | 30.6% |

Status: active fundamentals certification. The live study guide already displays the July 20, 2026 English objective wording. Microsoft identifies minor changes under compute and networking services, deployment and management tools, and monitoring tools; the three top-level domains and their weights remain unchanged. The bank uses GA concepts shared by the January and July versions. Recheck localized exam guides because Microsoft states that localized updates can follow the English update by approximately eight weeks.

Primary official documentation used includes the Microsoft Learn AZ-900 training modules, shared responsibility, Azure regions and availability zones, the Azure resource hierarchy, Virtual Machine Scale Sets, Azure Functions, virtual network peering, Private Link, Azure Storage redundancy and File Sync, Microsoft Entra Domain Services, Azure RBAC, Azure Policy, tags, Cost Management budgets, resource locks, Microsoft Purview, Cloud Shell, ARM templates, Azure Arc, Azure Advisor, and Azure Service Health. Every question carries its closest supporting `learn.microsoft.com` source URL.

## SC-900 — Microsoft Security, Compliance, and Identity Fundamentals

- Certification: <https://learn.microsoft.com/en-us/credentials/certifications/security-compliance-and-identity-fundamentals/>
- Official study guide: <https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/sc-900>
- Skills version in effect on the research date: November 7, 2025
- Scheduled English exam update: July 28, 2026
- Bank coverage: 36 questions

| Official top-level objective | Published weight | Questions | Bank share |
|---|---:|---:|---:|
| Describe the concepts of security, compliance, and identity | 10–15% | 5 | 13.9% |
| Describe the capabilities of Microsoft Entra | 25–30% | 10 | 27.8% |
| Describe the capabilities of Microsoft security solutions | 35–40% | 13 | 36.1% |
| Describe the capabilities of Microsoft compliance solutions | 20–25% | 8 | 22.2% |

Status: active fundamentals certification. The live study guide already displays the July 28, 2026 English objectives. The incoming guide adds agent ID to identity types, updates wording in several security topics, removes Microsoft Priva from the privacy subsection, and adds insider risk, eDiscovery, and audit coverage. This bank is based on the in-effect November 2025 objectives and favors topics shared with the incoming guide. It therefore covers Microsoft privacy principles rather than relying on the departing Priva objective or the not-yet-effective eDiscovery additions. Recheck the guide after July 28 before treating new topics as required coverage.

Primary official documentation used includes shared responsibility, Zero Trust, identity concepts, Microsoft Entra ID, External ID, hybrid identity, passwordless authentication, Conditional Access, access reviews, Privileged Identity Management, ID Protection, Azure network security services, Defender for Cloud and CSPM, Microsoft Sentinel, Defender for Endpoint, Defender for Office 365, Defender for Identity, the Service Trust Portal, Compliance Manager, sensitivity labels, DLP, retention, Content Explorer, and Microsoft's privacy principles. Every question carries its closest supporting `learn.microsoft.com` source URL.

## Editorial audit — 2026-07-16

The original 62 fundamentals questions were reviewed against their specific Microsoft Learn source and the applicable in-effect official study-guide objective: 31 AZ-900 questions and 31 SC-900 questions. The first expansion added exactly 25 original questions to each exam.

Substantive corrections and checks:

- Restored the missing fourth SC-900 top-level domain. Compliance is now separate from Microsoft security solutions and carries the published 20–25% weighting.
- Corrected the AZ-900 role-assignment question from management and governance to Azure architecture and services, where the official guide places Azure RBAC.
- Rewrote weak existing distractors so alternatives are credible neighboring cloud, identity, security, or compliance concepts rather than unrelated products.
- Made selection cardinality explicit in every multi-select prompt and verified that each marked-answer count matches the prompt.
- Tightened existing prompts and explanations to distinguish consumption pricing from capital expenditure, public Azure DNS from private DNS and traffic routing, Azure Policy from Azure RBAC, Defender for Cloud from Defender XDR and Sentinel, and Conditional Access signals from unrelated tenant properties.
- Replaced broad or stale source links with the closest current Microsoft Learn training unit or product documentation page.
- Added weighted coverage for MSP-relevant fundamentals, including multi-subscription governance, customer cost allocation, hybrid server management, hybrid and external identities, privileged access governance, network protections, Microsoft Defender services, and Microsoft Purview controls.

## Editorial rules applied

- Four, five, or six plausible choices from the same technical domain, selected according to the scenario rather than padded to a fixed size.
- Single-choice questions have exactly one correct choice. Multi-select prompts state the required answer count and mark exactly that number of choices.
- Explanations state the deciding behavior and distinguish the nearest distractor where that improves clarity.
- Questions stay at fundamentals depth while using realistic MSP administration and security framing where it fits the published scope.
- Questions map to the official top-level objective granularity supported by the question-bank schema.

## Mini-scenario expansion — 2026-07-16

Added five application-focused mini-scenarios to each fundamentals exam, bringing the fundamentals total to 72 questions. All new keys contain `-scenario-` for measurable scenario coverage.

All 10 scenario source URLs returned HTTP 200 on 2026-07-16. After overlap with the original source set, the two fundamentals banks use 62 distinct Microsoft Learn URLs.

AZ-900 additions cover:

- Customer responsibilities for an application hosted on an Azure virtual machine.
- The Azure management-scope hierarchy in broad-to-specific order.
- Choosing a managed web-app platform and private ExpressRoute connectivity.
- Enforcing approved deployment regions with Azure Policy.

SC-900 additions cover:

- Applying explicit verification and least privilege in a Zero Trust request.
- Reviewing supplier guest access and responding to identity risk.
- Selecting Microsoft Sentinel for a cross-domain SIEM and SOAR requirement.
- Selecting Purview DLP for sensitive-data sharing across Microsoft 365.

Each exam's additions include one multi-select that explicitly says `Choose two`. Each full fundamentals bank now contains 32 four-choice, 2 five-choice, and 2 six-choice questions.

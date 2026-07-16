# AZ-104 and AZ-700 official source map

Research date: 2026-07-16. Only Microsoft Learn and Microsoft product documentation were used. The questions are original scenarios; Microsoft practice-assessment questions and exam dumps were not used.

## AZ-104 — Microsoft Azure Administrator

- Certification: https://learn.microsoft.com/en-us/credentials/certifications/azure-administrator/
- Official study guide: https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/az-104
- Current skills version on the study guide: April 17, 2026
- Bank coverage: 79 questions

| Official top-level objective | Published weight | Questions | Bank share |
|---|---:|---:|---:|
| Manage Azure identities and governance | 20–25% | 19 | 24.1% |
| Implement and manage storage | 15–20% | 15 | 19% |
| Deploy and manage Azure compute resources | 20–25% | 19 | 24.1% |
| Implement and manage virtual networking | 15–20% | 15 | 19% |
| Monitor and maintain Azure resources | 10–15% | 11 | 13.9% |

Status: active associate-level certification. Microsoft Learn states that associate certifications renew annually. No retirement notice was present on the certification or study-guide page on the research date.

Key official documentation used by the bank includes Azure RBAC, Azure Policy, management groups, resource locks, Azure Lighthouse, Microsoft Entra B2B and SSPR, Storage networking and authorization, Azure Files, Blob lifecycle and replication, ARM/Bicep what-if, Azure VM availability and encryption, VM Scale Sets, Container Apps and ACR, App Service, VNet peering, NSGs, Bastion, Private Link, Azure DNS, Load Balancer, Azure Monitor, Azure Backup, and Site Recovery. Every question carries its specific `learn.microsoft.com` source URL.

## AZ-700 — Designing and Implementing Microsoft Azure Networking Solutions

- Certification: https://learn.microsoft.com/en-us/credentials/certifications/azure-network-engineer-associate/
- Official study guide: https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/az-700
- Skills version in effect on the research date: April 24, 2026
- Scheduled English exam update: July 27, 2026
- Bank coverage: 74 questions

| Official top-level objective | Published weight | Questions | Bank share |
|---|---:|---:|---:|
| Design and implement core networking infrastructure | 25–30% | 20 | 27% |
| Design, implement, and manage connectivity services | 20–25% | 16 | 21.6% |
| Design and implement application delivery services | 15–20% | 13 | 17.6% |
| Design and implement private access to Azure services | 10–15% | 9 | 12.2% |
| Design and implement Azure network security services | 15–20% | 16 | 21.6% |

Status: active associate-level certification with annual renewal. The certification page announced an English exam update for July 27, 2026. Microsoft had already published the incoming study-guide wording when this research was performed. Its change log identifies only minor changes under IP addressing, network monitoring, and NSGs; the five top-level objectives and their weights remain unchanged. The draft therefore keeps the current top-level mapping and uses current GA product documentation, including virtual network flow logs rather than proposing new NSG flow-log deployments. Recheck the guide after July 27 before promoting this draft to a released bank.

Key official documentation used by the bank includes VNet IP addressing, subnet delegation, custom IP prefixes, peering and routing, Azure Route Server, Azure Virtual Network Manager, DDoS Network Protection, Azure DNS Private Resolver, NAT Gateway, Network Watcher, VPN Gateway, ExpressRoute, Virtual WAN, Load Balancer, Traffic Manager, Application Gateway, Front Door, Private Link, service endpoints and endpoint policies, NSGs and ASGs, VNet flow logs, Azure Firewall, Firewall Manager, and WAF. Every question carries its specific `learn.microsoft.com` source URL.

## Editorial audit — 2026-07-16

Before the True/False expansion, all 145 Azure questions were reviewed against their specific current Microsoft Learn source and the applicable official study-guide objective: 75 AZ-104 questions and 70 AZ-700 questions. The original audit validated 115 unique source URLs. The 20 scenario additions were separately parsed, semantically reviewed, and their 20 direct sources returned HTTP 200; after source reuse is accounted for, that checkpoint used 128 unique `learn.microsoft.com` URLs.

Substantive corrections and checks included:

- Correctness and prerequisites: the storage-firewall scenario now requires both subnet service endpoints and default-deny virtual network rules; the resource-lock explanation no longer suggests that a ReadOnly lock permits control-plane POST operations.
- Scope-dependent answers: the ACR managed-identity question now states the `RBAC Registry Permissions` mode, distinguishing `AcrPull` from the repository-scoped roles used by ABAC-enabled registries.
- Product-boundary precision: service endpoints are described as extending VNet and subnet identity to a service firewall while retaining the public service endpoint, rather than implying that the service endpoint itself enforces access.
- Assessment clarity: every multiple-choice prompt states its required answer count. Ambiguous plural wording was corrected in the DNS Private Resolver and availability-set questions.
- Distractor quality: weak cross-domain alternatives were replaced with credible near-neighbor configurations, including Bastion versus JIT and point-to-site access, NAT Gateway versus Load Balancer outbound rules and default outbound access, Route Server versus BGP gateways and centrally deployed UDRs, and Firewall Manager versus Firewall Policy and Virtual WAN routing intent.
- Source maintenance: two retired documentation paths for Azure Policy initiatives and nested group licensing were replaced with current Microsoft Learn pages.
- Coverage expansion: 25 original AZ-700 scenarios were added across the published objective weights, including subnet delegation, BYOIP, forced tunneling, Route Server, Virtual Network Manager, DDoS Network Protection, VPN and ExpressRoute resiliency, Virtual WAN NVAs, Gateway and global Load Balancer, Private Link service, private-endpoint network policies, security admin rules, WAF evaluation, and Firewall Policy inheritance.

## Scenario and variable-choice audit — 2026-07-16

Ten application-focused scenarios were added to each Azure exam. Their content keys contain `-scenario-`; sequence-dependent items also contain `-order-` so automated checks can measure this coverage.

- AZ-104 additions: 10 scenarios, including two deployment-order questions and four explicit-count multiple-choice questions. Objective additions are balanced at two questions per published top-level objective.
- AZ-700 additions: 10 scenarios, including two dependency-order questions and three explicit-count multiple-choice questions. Additions cover all five published top-level objectives.
- Choice counts for each exam's additions: three questions with four choices, three with five choices, and four with six choices.
- Every ordering choice contains the same material steps, or the same steps plus a plausible but invalid configuration, in a different sequence. The correct sequence is determined by a documented prerequisite, approval boundary, or verification point.
- Every new multiple-choice prompt says exactly how many answers to choose. Correct-answer counts were checked against that number.
- Distractors stay within the same service or architecture decision and fail because of sequencing, scope, permissions, routing specificity, prerequisites, or a least-privilege constraint.
- Explanations state the deciding product behavior and distinguish the closest tempting alternative. The additions don't reproduce Microsoft practice-assessment items or third-party exam material.
- Final ambiguity corrections: the ACR pull scenario explicitly fixes the registry to `RBAC Registry Permissions` mode and notes the repository-scoped role difference for ABAC-enabled registries. A proposed gateway-transit ordering item was removed because portal and API workflows don't establish one universal order for the reciprocal peering flags; it was replaced with a DNS Private Resolver ARM dependency-order scenario. The Private Link workflow tests the documented load-balancer, service, alias, request, and approval dependencies rather than a subnet-policy step that the portal can perform automatically.

## Editorial rules applied

- True/False items use exactly two choices; other questions use four to six plausible choices from the same technical domain.
- Single-choice questions have exactly one correct choice; multiple-choice questions state the expected number and have that number of correct choices.
- Explanations identify the deciding behavior and distinguish the closest distractor where useful.
- Scenarios favor MSP operations, delegated administration, governance, security, hybrid connectivity, and repeatable multi-customer network operations.
- Questions are mapped only to the exact official top-level objectives, because that is the granularity supported by the current question-bank schema.

## True/False expansion — 2026-07-16

Added four source-backed True/False questions to each Azure associate exam. AZ-104 now has 79 questions and AZ-700 has 74, for 153 combined. Each bank has two `True` and two `False` keyed answers. The additions test stateful NSGs, managed-identity lifecycle, VM deallocation billing, private endpoints, route-table association, ExpressRoute encryption, Application Gateway v2, and Standard Load Balancer security. The complete Azure set now references 133 distinct Microsoft Learn URLs.

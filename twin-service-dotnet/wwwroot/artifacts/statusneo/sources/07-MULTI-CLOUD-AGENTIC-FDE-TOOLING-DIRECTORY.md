# Multi-Cloud Agentic FDE Ecosystem & Tooling Directory
## Generic, AWS, GCP & Azure agentic toolchains mapped to Embedded & Remote FDE PODs
*Capability directory · June 2026 · prepared for the StatusNeo FDE practice*

**Purpose.** A current reference of the agentic-AI tooling landscape for forward-deployed engineering, organized so a practice can pick a stack per cloud and per topology. For each provider it separates **Embedded FDE options** (human-in-the-loop, IDE-native) from **Remote FDE options** (autonomous, managed-runtime).

> **READ THIS FIRST — a dating caveat.** This directory names specific 2026 products, versions, and dates (AgentCore, Kiro, Gemini Enterprise Agent Platform, Agent Framework 1.0, and others). The agentic tooling market moves monthly — product names, GA dates, and billing details change fast. **Verify every named product and date against the vendor's own docs before quoting this to a client or sponsor.** The durable layer is the structure (Embedded vs. Remote, the four substrates); the perishable layer is the specific tool names.

---

## Generic & Cloud-Agnostic Orchestration

Generic frameworks provide the foundational logic for multi-agent coordination without cloud lock-in — ideal for hybrid deployments where agents span multiple environments or legacy local systems. They are the portable core beneath any cloud-native binding.

| Framework | Best for | Embedded fit | Remote fit |
|---|---|---|---|
| **LangGraph** | Complex, stateful graphs and cyclic workflows. | High — embeds as local state machines in apps. | High — deployable as persistent micro-services. |
| **CrewAI** | Role-based collaborative agent squads. | High — ideal for local "coding squads" in IDEs. | Medium — typically needs a managed backend (CrewAI Enterprise). |
| **AutoGen 2.0** | Conversation-based multi-agent teams. | Medium — best for interactive chat-based dev. | High — powerful for autonomous, headless loops. |
| **LlamaIndex Workflows** | RAG-heavy agents needing massive data reasoning. | High — embedded search & retrieval agents. | High — remote data-syncing agents. |
| **Dify** | Visual orchestration and managed platform. | Low — primarily a centralized platform. | High — ready-to-use remote agent infrastructure. |

**The critical enterprise choice** is *managed platform vs. developer framework*: platforms (Dify, Nexus-class) trade control for speed-to-deploy; frameworks (LangGraph, CrewAI) trade setup effort for portability and control. Regulated/air-gapped buyers usually need the framework path; fast-moving product teams often prefer the platform path.

---

## ☁ AWS Agentic Ecosystem (June 2026)

*Per the source: following the June 2026 AWS NY Summit, AWS shifted from "Copilots" to "AgentCore" infrastructure — positioning agents as a combination of models plus managed harnesses. [Verify product names/dates before use.]*

**Embedded FDE options**
- **Kiro** — a developer toolchain with a mobile app and IDE integration for validating infrastructure and testing.
- **AWS DevOps Agent** — automated release-readiness and remediation, working directly inside CI/CD pipelines.
- **Strands Agents SDK (v1.0)** — open-source toolkit for production agents with integrated chaos testing and context management.

**Remote FDE options**
- **AgentCore Harness** — a managed execution environment that decouples agent logic from models; runs agents on isolated micro-VMs, handling state persistence and error recovery autonomously.
- **AWS Context Service** — unified knowledge-graph construction so remote agents maintain consistent codebase topology.
- **Amazon Q Enterprise** — an enterprise-wide AI assistant deployable as an autonomous service for codebase modernization.

**Performance & infrastructure**
- Compute: EC2 G7 instances with NVIDIA Blackwell GPUs for high-speed agent inference.
- Retrieval: Web Search on Amazon Bedrock AgentCore.

---

## ☁ Google Cloud (GCP) Agentic Ecosystem

*Per the source: GCP consolidated its offerings into the Gemini Enterprise Agent Platform (formerly Vertex AI), focusing on long-running autonomous agents and multimodal reasoning. [Verify product names/dates before use.]*

**Embedded FDE options**
- **Google ADK** — the native framework for building agents on Gemini; stable v1.0 across four languages in 2026.
- **Workspace Studio** — a no-code agent builder embedded in Google Workspace for process-specific business agents.
- **Agentic Vision** — a Gemini 3 Flash capability letting embedded agents process visual development assets in real time.

**Remote FDE options**
- **Gemini Enterprise Agent Platform** — supports long-running agents that independently solve complex problems over several days.
- **Agent Engine (Agent Runtime)** — a managed service providing secure code execution, sessions, and "Memory Banks" for remote agents (billing live Feb 11, 2026 per source).
- **A2A Protocol v1.0** — an agent-to-agent communication standard, reported in production at 150+ organizations for multi-cloud interoperability.

**Performance & infrastructure**
- Compute: TPU 8t (training) and TPU 8i (inference), engineered for the agentic era.

---

## ☁ Microsoft Azure Agentic Ecosystem

*Per the source: Microsoft launched Agent Framework 1.0 (April 2026), the production-ready convergence of Semantic Kernel and AutoGen into a single unified SDK. [Verify product names/dates before use.]*

**Embedded FDE options**
- **Agent Framework 1.0** — native .NET and Python support with stable APIs for embedding agentic logic into enterprise apps.
- **Microsoft Foundry VS Code Extension** — GA as of June 3, 2026; deploy hosted agents and browse 1,900+ models in the editor.
- **Azure Cosmos DB Agent Memory Toolkit** — standardizes persistent agent memory using local emulators for dev/test without cloud dependency.

**Remote FDE options**
- **Azure AI Agent Service** — a managed service for multi-agent systems, integrating with Foundry for evaluations and memory stores.
- **Foundry Agent Service (BYOM)** — "Bring Your Own Model" — remote agents can use fine-tuned or third-party models behind Azure API Management.
- **Azure Container Apps Dynamic Sessions** — specialized compute for fast, secure isolation of remote agent code execution.

---

## PART V — Parallel Rollout Crosswalk: Embedded vs. Remote

The adoption pattern across every cloud is the same: deploy **Embedded** for trust-building and developer speed, and **Remote** for high-scale autonomous service delivery.

| Component | Embedded PODs (Human-in-the-Loop) | Remote PODs (Autonomous Services) |
|---|---|---|
| **User persona** | Individual developers, team leads. | DevOps engineers, SREs, product owners. |
| **Primary interface** | IDE (VS Code, Cursor), Slack, GitHub PRs. | API gateway, event bus (Kafka/SQS). |
| **Core tools** | Kiro, Foundry VS Code, Strands SDK. | Bedrock AgentCore, GCP Agent Runtime, AutoGen. |
| **Orchestration** | LangGraph, Semantic Kernel. | Azure AI Agent Service, Gemini Platform. |
| **Acceptance criteria** | Human code review / approval gate. | Automated evaluations / canary deployments. |

**Mapping the directory to the 80:20 model.** This crosswalk is the tooling expression of the 80:20 split. **Embedded = ~70:30** (the human holds the approval gate in the IDE); **Remote = ~90:10** (automated evaluations and canary deploys hold the gate, under human-set policy). Same accountability, different interaction surface — exactly the topology split defined in the FDE 80:20 lifecycle.

**Industry trends (June 2026, per source)**
- **Agentic SDLC:** over 70% of teams report high GenAI usage across the full development lifecycle.
- **Talent shift:** entry-level coding roles are shifting toward senior-agent collaboration — agents perform tasks formerly assigned to junior engineers.
- **Managed vs. framework:** the distinction between managed platforms and developer frameworks is the most critical enterprise adoption decision.

> **How to use this directory:** pick per cloud and per topology, not per hype. For each engagement, choose the portable core (LangGraph/CrewAI) first, then bind to the client's existing cloud-native runtime (AgentCore / Agent Engine / Azure AI Agent Service). Start Embedded to earn trust, graduate to Remote to scale. And re-verify the product names — this market re-prices itself every quarter.

---

*Source attribution: product, version, and date claims in the AWS/GCP/Azure sections are drawn from the user-supplied June-2026 ecosystem brief and are pending independent verification against vendor documentation.*

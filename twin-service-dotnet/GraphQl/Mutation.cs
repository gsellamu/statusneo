using System.Text.Json;
using HotChocolate.Subscriptions;
using Spine.Twin.Data;
using Spine.Twin.Domain;

namespace Spine.Twin.GraphQl;

public class Mutation
{
    // propose -> validate -> GATE -> commit -> propagate, all here.
    public async Task<CommandResultDto> PlaceModucule(
        string room, PlaceInput cmd,
        [Service] EventStore store, [Service] ITopicEventSender sender,
        int? expectedVersion = null, string? commandId = null)
    {
        if (commandId is not null)                              // idempotent retry
        {
            var prior = await store.ByCommandIdAsync(room, commandId);
            if (prior is not null)
                return new CommandResultDto("ACCEPTED", new(),
                    new EventDto(prior.Seq, prior.Type, prior.Payload, prior.Actor, prior.Ts))
                    { IdempotentReplay = true, Version = await store.VersionAsync(room) };
        }

        var current = await store.VersionAsync(room);
        bool rebased = expectedVersion is not null && expectedVersion != current;  // stale client view

        var state = await store.ReadModelAsync(room);           // validate against CURRENT truth
        var others = state.Instances.Select(i => new Instance(i.InstanceId, i.TypeId, i.X, i.Y, i.Rotation)).ToList();

        var candidate = new Instance($"i-{Guid.NewGuid():N}"[..10], cmd.TypeId, cmd.X, cmd.Y, cmd.Rotation);
        var verdict = Validator.Validate(candidate, others, EventStore.Room);
        var vlist = verdict.Violations
            .Select(v => new ViolationDto(v.Rule, v.Severity.ToString().ToUpper(), v.Message, v.Refs)).ToList();

        if (!verdict.Ok)                                        // GATE: any ERROR blocks the commit
            return new CommandResultDto("REJECTED", vlist, null) { Rebased = rebased, Version = current };

        var defn = Registry.Def(candidate.TypeId);
        var payload = new
        {
            instance_id = candidate.InstanceId, type_id = candidate.TypeId,
            type_version = defn.Version,                        // pin the exact validated version
            x = candidate.X, y = candidate.Y, rotation = candidate.Rotation,
            bindings = verdict.Bindings,
            warnings = vlist.Where(v => v.Severity == "WARNING"),
        };
        var row = await store.AppendAsync(room, "MODUCULE_PLACED", payload, "planner", commandId);  // COMMIT
        await sender.SendAsync(room, await store.ReadModelAsync(room));                              // PROPAGATE
        return new CommandResultDto("ACCEPTED", vlist,
            new EventDto(row.Seq, row.Type, row.Payload, row.Actor, row.Ts))
            { Rebased = rebased, Version = await store.VersionAsync(room) };
    }

    // Stamp a Room Moducule (reusable template) into a target room. Each component goes through
    // the SAME gate, in dependency order, so the bed earns its med-gas edge to the just-placed
    // headwall. "Design once, reuse everywhere" — the literal lego point. Publishes the twin once.
    public async Task<StampResultDto> StampRoom(
        string room, string templateId,
        [Service] EventStore store, [Service] ITopicEventSender sender)
    {
        var tpl = RoomTemplates.Get(templateId);
        if (tpl is null)
            return new StampResultDto("REJECTED", templateId, 0, "EMPTY",
                new() { new ViolationDto("template", "ERROR", $"No such Room Moducule '{templateId}'.", Array.Empty<string>()) });

        // working set = current room contents; we validate each component against it as it grows
        var state = await store.ReadModelAsync(room);
        var working = state.Instances.Select(i => new Instance(i.InstanceId, i.TypeId, i.X, i.Y, i.Rotation)).ToList();
        var localToInstance = new Dictionary<string, string>();
        var allViolations = new List<ViolationDto>();
        int placed = 0;

        foreach (var c in tpl.Components)                       // dependency order (headwall -> bed -> sink)
        {
            var instId = $"{c.LocalId}-{Guid.NewGuid():N}"[..12];
            var cand = new Instance(instId, c.TypeId, c.X, c.Y, c.Rotation);
            var verdict = Validator.Validate(cand, working, EventStore.Room);   // GATE
            allViolations.AddRange(verdict.Violations
                .Select(v => new ViolationDto(v.Rule, v.Severity.ToString().ToUpper(), v.Message, v.Refs)));

            if (!verdict.Ok) continue;                          // skip a component that would violate; keep going

            var defn = Registry.Def(c.TypeId);
            var payload = new
            {
                instance_id = instId, type_id = c.TypeId, type_version = defn.Version,
                x = c.X, y = c.Y, rotation = c.Rotation,
                bindings = verdict.Bindings,
                warnings = verdict.Violations.Where(v => v.Severity == Severity.Warning).Select(v => v.Message),
            };
            await store.AppendAsync(room, "MODUCULE_PLACED", payload, "template:" + templateId);  // COMMIT
            working.Add(cand);
            localToInstance[c.LocalId] = instId;
            placed++;
        }

        await sender.SendAsync(room, await store.ReadModelAsync(room));         // PROPAGATE (once)
        var finalState = await store.ReadModelAsync(room);
        var status = HierarchyProgram.RoomStatus(finalState);
        return new StampResultDto(placed > 0 ? "ACCEPTED" : "REJECTED", templateId, placed, status, allViolations)
            { Version = finalState.Version };
    }

    public async Task<CommandResultDto> RemoveModucule(
        string room, string instanceId,
        [Service] EventStore store, [Service] ITopicEventSender sender)
    {
        var state = await store.ReadModelAsync(room);
        if (!state.Instances.Any(i => i.InstanceId == instanceId))
            return new CommandResultDto("REJECTED",
                new() { new ViolationDto("exists", "ERROR", "No such instance.", Array.Empty<string>()) }, null);

        var row = await store.AppendAsync(room, "MODUCULE_REMOVED", new { instance_id = instanceId }, "planner");
        await sender.SendAsync(room, await store.ReadModelAsync(room));
        return new CommandResultDto("ACCEPTED", new(),
            new EventDto(row.Seq, row.Type, row.Payload, row.Actor, row.Ts));
    }

    // The agent PROPOSES a command; it never commits. A human approves in the UI.
    // Deterministic stub — a LangGraph agent plugs in behind this identical contract.
    // The agent PROPOSES via grounding (Neo4j) + brain (Ollama); the gate validates its
    // output; a human commits. Real where the infra is up, deterministic fallback otherwise.
    public async Task<AgentProposalDto> AgentSuggest(
        string room, [Service] EventStore store,
        [Service] Integration.IGroundingStore grounding, [Service] Integration.IAgentBrain brain,
        string goal = "observation room")
    {
        var facts = await grounding.RetrieveAsync(goal);            // real Cypher (or fallback)
        var state = await store.ReadModelAsync(room);
        var current = state.Instances
            .Select(i => new Instance(i.InstanceId, i.TypeId, i.X, i.Y, i.Rotation)).ToList();

        var proposal = await brain.ProposeAsync(facts, current, goal);   // LLM, grounded-only
        var citations = facts.Rules.Select(r => r.Id).ToArray();
        if (citations.Length == 0) citations = new[] { "R3-medgas", "R2-clearance" };

        if (proposal is null)
            return new AgentProposalDto(null,
                current.Any(i => i.TypeId == facts.HeadwallType)
                    ? "No valid proposal produced."
                    : "No med-gas source placed yet; cannot satisfy med-gas reach.",
                citations);

        // GATE the LLM output before surfacing it — never free-form trust
        var cand = new Instance("preview", proposal.TypeId, proposal.X, proposal.Y, proposal.Rotation);
        var verdict = Validator.Validate(cand, current, EventStore.Room);
        if (!verdict.Ok)
            return new AgentProposalDto(null,
                "Model proposed an invalid layout, rejected by the gate (" +
                string.Join(", ", verdict.Violations.Where(v => v.Severity == Severity.Error).Select(v => v.Rule)) +
                "). Try again or place manually.",
                citations);

        return new AgentProposalDto(
            new ProposedPlacement(proposal.TypeId, proposal.X, proposal.Y, proposal.Rotation),
            proposal.Rationale, citations);
    }
}

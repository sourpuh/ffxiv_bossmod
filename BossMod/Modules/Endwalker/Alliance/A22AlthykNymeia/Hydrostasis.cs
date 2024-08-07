﻿namespace BossMod.Endwalker.Alliance.A22AlthykNymeia;

class Hydrostasis(BossModule module) : Components.Knockback(module)
{
    private readonly List<Source> _sources = [];

    public bool Active => _sources.Count == 3 || NumCasts > 0;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Active ? _sources : Enumerable.Empty<Source>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HydrostasisAOE1 or AID.HydrostasisAOE2 or AID.HydrostasisAOE3 or AID.HydrostasisAOEDelayed)
            AddSource(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HydrostasisAOE1 or AID.HydrostasisAOE2 or AID.HydrostasisAOE3 or AID.HydrostasisAOE0 or AID.HydrostasisAOEDelayed)
        {
            ++NumCasts;
            if (_sources.Count > 0)
                _sources.RemoveAt(0);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.HydrostasisQuick)
            AddSource(source.Position, WorldState.FutureTime(12));
    }

    private void AddSource(WPos pos, DateTime activation)
    {
        _sources.Add(new(pos, 28, activation));
        _sources.SortBy(s => s.Activation);
    }
}

﻿using System.Collections.Generic;
using System.Linq;

namespace BossMod.Stormblood.Ultimate.UWU
{
    class P1Plumes : BossComponent
    {
        private List<Actor> _razor = new();
        private List<Actor> _spiny = new();
        private List<Actor> _satin = new();

        public bool Active => _razor.Any(p => p.IsTargetable) || _spiny.Any(p => p.IsTargetable) || _satin.Any(p => p.IsTargetable);

        public override void Init(BossModule module)
        {
            _razor = module.Enemies(OID.RazorPlume);
            _spiny = module.Enemies(OID.SpinyPlume);
            _satin = module.Enemies(OID.SatinPlume);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, IArena arena)
        {
            arena.Actors(_razor, ArenaColor.Enemy);
            arena.Actors(_spiny, ArenaColor.Enemy);
            arena.Actors(_satin, ArenaColor.Enemy);
        }
    }
}

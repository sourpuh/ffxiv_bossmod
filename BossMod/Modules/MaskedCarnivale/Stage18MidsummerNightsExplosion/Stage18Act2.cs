using System.Linq;
using BossMod.Components;

// CONTRIB: made by malediktus, not checked
namespace BossMod.MaskedCarnivale.Stage18.Act2
{
    public enum OID : uint
    {
        Boss = 0x2725, //R=3.0
        Keg = 0x2726, //R=0.65
    };

    public enum AID : uint
    {
        WildCharge = 15055, // 2725->players, 3,5s cast, width 8 rect charge
        Explosion = 15054, // 2726->self, 2,0s cast, range 10 circle
        Fireball = 15051, // 2725->location, 4,0s cast, range 6 circle
        RipperClaw = 15050, // 2725->self, 4,0s cast, range 5+R 90-degree cone
        TailSmash = 15052, // 2725->self, 4,0s cast, range 12+R 90-degree cone
        BoneShaker = 15053, // 2725->self, no cast, range 50 circle, harmless raidwide
    };

    class Explosion : SelfTargetedAOEs
    {
        public Explosion() : base(ActionID.MakeSpell(AID.Explosion), new AOEShapeCircle(10)) { }
    }

    class Fireball : LocationTargetedAOEs
    {
        public Fireball() : base(ActionID.MakeSpell(AID.Fireball), 6) { }
    }

    class RipperClaw : SelfTargetedAOEs
    {
        public RipperClaw() : base(ActionID.MakeSpell(AID.RipperClaw), new AOEShapeCone(8, 45.Degrees())) { }
    }

    class TailSmash : SelfTargetedAOEs
    {
        public TailSmash() : base(ActionID.MakeSpell(AID.TailSmash), new AOEShapeCone(15, 45.Degrees())) { }
    }

    class WildCharge : GenericWildCharge
    {
        private bool casting;

        public WildCharge() : base(4, ActionID.MakeSpell(AID.WildCharge)) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                casting = true;
                Source = caster;
                foreach (var (slot, player) in module.Raid.WithSlot())
                {
                    PlayerRoles[slot] = player.InstanceID == spell.TargetID ? PlayerRole.Target : PlayerRole.Target;
                }
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                Source = null;
                casting = false;
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (casting && !module.Enemies(OID.Keg).All(e => e.IsDead))
                hints.Add("Aim charge at a keg!");
        }
    }

    class WildChargeKB : KnockbackFromCastTarget
    {
        public WildChargeKB() : base(ActionID.MakeSpell(AID.WildCharge), 10, kind: Kind.DirForward) { } //knockback actually delayed by 0.5s to 1s, maybe it depends on the rectangle length of the charge
        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints) { }
    }

    class KegExplosion : GenericStackSpread
    {
        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var p in module.Enemies(OID.Keg).Where(x => x.HP.Cur > 0))
                arena.AddCircle(p.Position, 10, ComponentType.Danger);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var player = module.Raid.Player();
            if (player != null)
                foreach (var p in module.Enemies(OID.Keg).Where(x => x.HP.Cur > 0))
                    if (player.Position.InCircle(p.Position, 10))
                    {
                        hints.Add("In keg explosion radius!");
                    }
        }
    }

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Same as last stage. Make the manticores run to the kegs and their attacks\nwill make them blow up. Their attacks will also do friendly fire damage\nto each other.\nThe Ram's Voice and Ultravibration combo can be used to kill manticores.");
        }
    }

    class Stage18Act2States : StateMachineBuilder
    {
        public Stage18Act2States(BossModule module) : base(module)
        {
            TrivialPhase()
                .DeactivateOnEnter<Hints>()
                .ActivateOnEnter<Explosion>()
                .ActivateOnEnter<Fireball>()
                .ActivateOnEnter<RipperClaw>()
                .ActivateOnEnter<TailSmash>()
                .ActivateOnEnter<WildCharge>()
                .ActivateOnEnter<WildChargeKB>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Keg).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 628, NameID = 8116)]
    public class Stage18Act2 : BossModule
    {
        public Stage18Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
            ActivateComponent<KegExplosion>();
        }

        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Keg).Any(e => e.InCombat); }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ComponentType.ActorEnemy, false);
            foreach (var s in Enemies(OID.Keg))
                Arena.Actor(s, ComponentType.ActorObject, false);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.Boss => 1,
                    OID.Keg => 0,
                    _ => 0
                };
            }
        }
    }
}

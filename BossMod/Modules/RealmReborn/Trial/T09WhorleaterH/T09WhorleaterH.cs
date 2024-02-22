// CONTRIB: made by taurenkey, changed by malediktus, not checked
using BossMod.Components;

namespace BossMod.Modules.RealmReborn.Trial.T09WhorleaterH
{
    class GrandFall : LocationTargetedAOEs
    {
        public GrandFall() : base(ActionID.MakeSpell(AID.GrandFall), 8) { }
    }

    class Hydroshot : PersistentVoidzoneAtCastTarget
    {
        public Hydroshot() : base(5, ActionID.MakeSpell(AID.Hydroshot), m => m.Enemies(OID.HydroshotZone), 0) { }
    }
    class Dreadstorm : PersistentVoidzoneAtCastTarget
    {
        public Dreadstorm() : base(5, ActionID.MakeSpell(AID.Dreadstorm), m => m.Enemies(OID.DreadstormZone), 0) { }
    }

    class T09WhorleaterHStates : StateMachineBuilder
    {
        public T09WhorleaterHStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<GrandFall>()
                .ActivateOnEnter<Hydroshot>()
                .ActivateOnEnter<Dreadstorm>()
                .ActivateOnEnter<BodySlamKB>()
                .ActivateOnEnter<BodySlamAOE>()
                .ActivateOnEnter<SpinningDive>()
                .ActivateOnEnter<SpinningDiveKB>()
                .ActivateOnEnter<Hints>();
        }
    }

    [ModuleInfo(CFCID = 72, NameID = 2505)]
    public class T09WhorleaterH(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsRect(new(-0, 0), 14.5f, 20))
    {
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ComponentType.ActorEnemy, true);
            foreach (var s in Enemies(OID.Spume))
                Arena.Actor(s, ComponentType.PlayerInteresting, false);
            foreach (var e in Enemies(OID.Tail))
                Arena.Actor(e, ComponentType.ActorEnemy, false);
            foreach (var e in Enemies(OID.Sahagin))
                Arena.Actor(e, ComponentType.ActorEnemy, false);
            foreach (var e in Enemies(OID.DangerousSahagins))
                Arena.Actor(e, ComponentType.ActorEnemy, false);
            foreach (var c in Enemies(OID.Converter))
                Arena.Actor(c, ComponentType.ActorObject, false);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.DangerousSahagins => 4,
                    OID.Spume => 3,
                    OID.Sahagin => 2,
                    OID.Boss or OID.Tail => 1,
                    _ => 0
                };
            }
        }
    }
}

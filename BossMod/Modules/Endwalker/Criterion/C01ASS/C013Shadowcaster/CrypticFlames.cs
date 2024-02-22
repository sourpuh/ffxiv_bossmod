﻿using System.Collections.Generic;

namespace BossMod.Endwalker.Criterion.C01ASS.C013Shadowcaster
{
    class CrypticFlames : BossComponent
    {
        public bool ReadyToBreak { get; private set; }
        private int[] _playerOrder = new int[4];
        private List<(Actor laser, int order)> _lasers = new();
        private int _numBrokenLasers;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var order = _playerOrder[slot];
            if (order != 0)
                hints.Add($"Break order: {order}", order == CurrentBreakOrder);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var order = _playerOrder[pcSlot];
            foreach (var l in _lasers)
            {
                var dir = l.laser.Rotation.ToDirection();
                var extent = 2 * dir * dir.Dot(module.Bounds.Center - l.laser.Position);
                var color = l.order != _playerOrder[pcSlot] ? ComponentType.ActorEnemy : order == CurrentBreakOrder ? ComponentType.Safe : ComponentType.Danger;
                arena.AddLine(l.laser.Position, l.laser.Position + extent, color, 2);
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.FirstBrand:
                    SetPlayerOrder(module, actor, 1);
                    break;
                case SID.SecondBrand:
                    SetPlayerOrder(module, actor, 2);
                    break;
                case SID.ThirdBrand:
                    SetPlayerOrder(module, actor, 3);
                    break;
                case SID.FourthBrand:
                    SetPlayerOrder(module, actor, 4);
                    break;
                case SID.FirstFlame:
                case SID.SecondFlame:
                case SID.ThirdFlame:
                case SID.FourthFlame:
                    ReadyToBreak = true;
                    break;
                case SID.Counter:
                    var order = status.Extra switch
                    {
                        0x1C1 => -1, // unbreakable
                        0x1C2 or 0x1C6 => 1,
                        0x1C3 or 0x1C7 => 2,
                        0x1C4 or 0x1C8 => 3,
                        0x1C5 or 0x1C9 => 4,
                        _ => 0
                    };
                    if (order != 0)
                        _lasers.Add((actor, order));
                    break;
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Counter)
            {
                _numBrokenLasers += _lasers.RemoveAll(l => l.laser == actor);
            }
        }

        private void SetPlayerOrder(BossModule module, Actor player, int order)
        {
            int slot = module.Raid.FindSlot(player.InstanceID);
            if (slot >= 0 && slot < _playerOrder.Length)
                _playerOrder[slot] = order;
        }

        private int CurrentBreakOrder => _numBrokenLasers switch
        {
            < 4 => _numBrokenLasers + 1,
            < 8 => _numBrokenLasers - 4 + 1,
            _ => 0
        };
    }
}

﻿using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using SAIN.SAINComponent;
using SAIN.Helpers;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers.Combat.Solo
{
    internal class DogFightAction : SAINAction
    {
        public DogFightAction(BotOwner bot) : base(bot, nameof(DogFightAction))
        {
        }

        public override void Update()
        {
            SAINEnemyClass enemy = SAIN.Enemy;
            if (enemy == null)
            {
                return;
            }

            SAIN.Mover.SetTargetPose(1f);
            SAIN.Mover.SetTargetMoveSpeed(1f);
            SAIN.Steering.SteerByPriority(false);
            bool EnemyVisible = enemy.IsVisible;

            if (UpdateMovementTimer < Time.time)
            {
                UpdateMovementTimer = Time.time + 0.15f;
                if (EnemyVisible 
                    && BackUp(out var pos))
                {
                    BotOwner.GoToPoint(pos, false, -1, false, false, false);
                }
                else if (!EnemyVisible 
                    && CheckMoveToEnemyTimer < Time.time
                    && enemy.Path.PathToEnemyStatus == NavMeshPathStatus.PathComplete 
                    && (enemy.EnemyPosition - BotOwner.Position).sqrMagnitude > 2f)
                {
                    CheckMoveToEnemyTimer = Time.time + 1f;
                    BotOwner.MoveToEnemyData.TryMoveToEnemy(enemy.EnemyPosition);
                }
            }

            Shoot.Update();
        }

        private float UpdateMovementTimer = 0f;
        private float CheckMoveToEnemyTimer = 0;

        private bool BackUp(out Vector3 trgPos)
        {
            Vector3 a = -Vector.NormalizeFastSelf(SAIN.Enemy.EnemyDirection);
            trgPos = Vector3.zero;
            float num = 0f;
            Vector3 random = Random.onUnitSphere * 1f;
            random.y = 0f;
            if (NavMesh.SamplePosition(BotOwner.Position + a * 2f / 2f + random, out NavMeshHit navMeshHit, 1f, -1))
            {
                trgPos = navMeshHit.position;
                Vector3 a2 = trgPos - BotOwner.Position;
                float magnitude = a2.magnitude;
                if (magnitude != 0f)
                {
                    Vector3 a3 = a2 / magnitude;
                    num = magnitude;
                    if (NavMesh.SamplePosition(BotOwner.Position + a3 * 2f, out navMeshHit, 1f, -1))
                    {
                        trgPos = navMeshHit.position;
                        num = (trgPos - BotOwner.Position).magnitude;
                    }
                }
            }
            if (num != 0f && num > BotOwner.Settings.FileSettings.Move.REACH_DIST)
            {
                navMeshPath_0.ClearCorners();
                if (NavMesh.CalculatePath(BotOwner.Position, trgPos, -1, navMeshPath_0) && navMeshPath_0.status == NavMeshPathStatus.PathComplete)
                {
                    trgPos = navMeshPath_0.corners[navMeshPath_0.corners.Length - 1];
                    return CheckLength(navMeshPath_0, num);
                }
            }
            return false;
        }

        private bool CheckLength(NavMeshPath path, float straighDist)
        {
            return path.CalculatePathLength() < straighDist * 1.2f;
        }

        private readonly NavMeshPath navMeshPath_0 = new NavMeshPath();

        public override void Start()
        {
            SAIN.Mover.Sprint(false);
        }

        public override void Stop()
        {
        }
    }
}
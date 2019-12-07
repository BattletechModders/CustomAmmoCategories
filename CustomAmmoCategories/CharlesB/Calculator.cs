using BattleTech;
using UnityEngine;

namespace CharlesB
{
    public static class Calculator
    {
        // pilot skill can mitigate up to skill level * 10%
        public static float PilotingMitigation(AbstractActor attacker)
        {
            var pilotSkill = attacker.SkillPiloting;
            var mitigationMax = Mathf.Min(pilotSkill, 10) / 10.0f;
            return Random.Range(0, mitigationMax);
        }
    }
}